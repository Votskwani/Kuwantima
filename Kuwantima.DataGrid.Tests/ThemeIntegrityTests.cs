using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Kuwantima.DataGrid.Tests;

/// <summary>
/// Tier 1 — theme integrity, scoped to this companion package. Same shape and reasoning as
/// Kuwantima.Tests/ThemeIntegrityTests.cs: zero C# of our own to unit test, so this instead
/// catches the theme failing to load, a key existing in one variant but not the other, or
/// DataGrid failing to template under Classes="Kuwantima".
/// </summary>
public class ThemeIntegrityTests
{
    private static readonly XNamespace Av = "https://github.com/avaloniaui";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

    /// <summary>
    /// Only the 5 keys that genuinely vary by variant (hover wash, grid lines, invalid brushes —
    /// see KuwantimaDataGridThemeResources.axaml's header). The theme-invariant accent keys and
    /// opacity doubles live flat, outside ThemeDictionaries, by design — there is nothing to vary.
    /// </summary>
    private static Dictionary<string, string[]> DeclaredKeysByVariant()
    {
        using var stream = typeof(ThemeIntegrityTests).Assembly
            .GetManifestResourceStream("KuwantimaDataGridThemeResources.axaml")
            ?? throw new InvalidOperationException(
                "KuwantimaDataGridThemeResources.axaml is not embedded in the test assembly.");

        return XDocument.Load(stream)
            .Descendants(Av + "ResourceDictionary")
            .Where(d => (string?)d.Attribute(X + "Key") is "Light" or "Dark")
            .ToDictionary(
                d => (string)d.Attribute(X + "Key")!,
                d => d.Elements()
                      .Select(e => (string?)e.Attribute(X + "Key"))
                      .Where(k => k is not null)
                      .Select(k => k!)
                      .ToArray());
    }

    /// <summary>The theme-invariant keys declared flat, outside ThemeDictionaries.</summary>
    private static string[] FlatKeys()
    {
        using var stream = typeof(ThemeIntegrityTests).Assembly
            .GetManifestResourceStream("KuwantimaDataGridThemeResources.axaml")!;

        var doc = XDocument.Load(stream);
        var themed = doc.Descendants(Av + "ResourceDictionary.ThemeDictionaries").Single();

        return doc.Root!.Elements()
            .Where(e => e != themed)
            .Select(e => (string?)e.Attribute(X + "Key"))
            .Where(k => k is not null)
            .Select(k => k!)
            .ToArray();
    }

    [AvaloniaFact]
    public void Theme_loads_from_the_documented_two_StyleIncludes()
    {
        // TestApp adds Kuwantima's own entry point, then this package's — the same order
        // CLAUDE.md's DataGrid section documents for a real consumer. If either theme or
        // anything it pulls in (including the vendor's own DataGrid Fluent theme) is malformed,
        // the headless session dies here and every test in the suite fails.
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
        var keys = DeclaredKeysByVariant().Values.SelectMany(k => k)
            .Concat(FlatKeys())
            .Distinct()
            .ToArray();

        Assert.True(keys.Length >= 5, $"Only parsed {keys.Length} keys — the parser is probably not reading the dictionary.");

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

    public static IEnumerable<object[]> VariantNames() => new[]
    {
        new object[] { "Light" },
        new object[] { "Dark" },
    };

    [AvaloniaTheory]
    [MemberData(nameof(VariantNames))]
    public void DataGrid_templates_and_lays_out(string variantName)
    {
        var control = new Avalonia.Controls.DataGrid { Classes = { "Kuwantima" } };

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
            $"DataGrid produced no visual tree under {variantName} — its template did not apply.");
        Assert.True(
            control.Bounds.Width > 0 && control.Bounds.Height > 0,
            $"DataGrid laid out to a zero size under {variantName} ({control.Bounds}).");
    }
}
