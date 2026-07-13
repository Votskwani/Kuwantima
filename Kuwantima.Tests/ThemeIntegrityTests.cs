using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Kuwantima.Tests;

/// <summary>
/// Tier 1 — theme integrity.
///
/// Kuwantima is a pure XAML package: zero C# in the library, so there is nothing
/// to unit test in the classic sense. What CAN break is the theme failing to load,
/// a resource key existing in one variant but not the other, or a control failing
/// to template. All three are invisible until a human looks at the app. These tests
/// make them fail loudly instead.
///
/// The keys are read from the shipped .axaml asset rather than hardcoded, so adding
/// a brush automatically brings it under test.
/// </summary>
public class ThemeIntegrityTests
{
    private static readonly XNamespace Avalonia_ = "https://github.com/avaloniaui";
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    /// <summary>
    /// Resource keys declared under each ThemeDictionaries variant, read from the library's
    /// own source dictionary (linked in as an embedded resource — see the .csproj for why it
    /// cannot be loaded from the built assembly). Reading the file rather than hardcoding a
    /// list means a newly added brush is under test the moment it is declared.
    /// </summary>
    private static Dictionary<string, string[]> DeclaredKeysByVariant()
    {
        using var stream = typeof(ThemeIntegrityTests).Assembly
            .GetManifestResourceStream("KuwantimaThemeResources.axaml")
            ?? throw new InvalidOperationException(
                "KuwantimaThemeResources.axaml is not embedded in the test assembly.");

        return XDocument.Load(stream)
            .Descendants(Avalonia_ + "ResourceDictionary")
            .Where(d => (string?)d.Attribute(Xaml + "Key") is "Light" or "Dark")
            .ToDictionary(
                d => (string)d.Attribute(Xaml + "Key")!,
                // Every child carrying an x:Key is a resource, whatever its type
                // (SolidColorBrush, BoxShadows, ...). Deliberately type-agnostic.
                d => d.Elements()
                      .Select(e => (string?)e.Attribute(Xaml + "Key"))
                      .Where(k => k is not null)
                      .Select(k => k!)
                      .ToArray());
    }

    [AvaloniaFact]
    public void Theme_loads_from_a_single_style_include()
    {
        // TestApp adds exactly one StyleInclude — the public entry point. If the theme
        // or anything it pulls in is malformed, the headless session dies here and every
        // test in the suite fails, which is the signal we want.
        Assert.NotNull(Application.Current);
        Assert.NotEmpty(Application.Current!.Styles);
    }

    [AvaloniaFact]
    public void Light_and_Dark_declare_the_same_resource_keys()
    {
        var byVariant = DeclaredKeysByVariant();
        Assert.Equal(2, byVariant.Count);

        var light = byVariant["Light"].ToHashSet();
        var dark = byVariant["Dark"].ToHashSet();
        Assert.NotEmpty(light);

        var missingFromDark = light.Except(dark).OrderBy(k => k).ToArray();
        var missingFromLight = dark.Except(light).OrderBy(k => k).ToArray();

        Assert.True(
            missingFromDark.Length == 0,
            $"Declared in Light but not Dark: {string.Join(", ", missingFromDark)}");
        Assert.True(
            missingFromLight.Length == 0,
            $"Declared in Dark but not Light: {string.Join(", ", missingFromLight)}");
    }

    [AvaloniaFact]
    public void Every_declared_resource_resolves_in_both_variants()
    {
        var keys = DeclaredKeysByVariant().Values.SelectMany(k => k).Distinct().ToArray();

        // Guard against a vacuous pass: if the parser stops matching the dictionary's
        // shape it would find nothing and the loop below would assert on an empty set.
        Assert.True(keys.Length >= 15, $"Only parsed {keys.Length} keys — the parser is probably not reading the dictionary.");

        var app = Application.Current!;
        var unresolved = new List<string>();

        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        foreach (var key in keys)
        {
            if (!app.TryGetResource(key, variant, out var value) || value is null)
                unresolved.Add($"{key} ({variant})");
        }

        Assert.True(
            unresolved.Count == 0,
            "Theme resources that failed to resolve:" + Environment.NewLine + "  " +
            string.Join(Environment.NewLine + "  ", unresolved));
    }

    /// <summary>Self-sizing styled controls × both theme variants.</summary>
    public static IEnumerable<object[]> ControlMatrix() =>
        from control in new[]
        {
            typeof(Button), typeof(CheckBox), typeof(RadioButton), typeof(ToggleButton),
            typeof(TextBox), typeof(ComboBox), typeof(ListBox), typeof(Slider),
            typeof(ProgressBar), typeof(TabControl), typeof(Expander),
        }
        from variant in new[] { "Light", "Dark" }
        select new object[] { control, variant };

    [AvaloniaTheory]
    [MemberData(nameof(ControlMatrix))]
    public void Styled_control_templates_and_lays_out(Type controlType, string variantName)
    {
        var control = (Control)Activator.CreateInstance(controlType)!;
        control.Classes.Add("Kuwantima");

        var window = new Window
        {
            RequestedThemeVariant = variantName == "Light" ? ThemeVariant.Light : ThemeVariant.Dark,
            Content = control,
            Width = 400,
            Height = 300,
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.True(
            control.GetVisualChildren().Any(),
            $"{controlType.Name} produced no visual tree under {variantName} — its template did not apply.");
        Assert.True(
            control.Bounds.Width > 0 && control.Bounds.Height > 0,
            $"{controlType.Name} laid out to a zero size under {variantName} ({control.Bounds}).");
    }
}
