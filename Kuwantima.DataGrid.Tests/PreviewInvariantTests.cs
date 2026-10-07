using System.Text.RegularExpressions;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Xunit;

namespace Kuwantima.DataGrid.Tests;

/// <summary>
/// Invariant 7's equivalent for this package — see Kuwantima.Tests/InvariantTests.Preview.cs for
/// the full rationale (an unresolvable DynamicResource in Design.PreviewWith is silent, so a
/// style file's own preview can look fine while resolving nothing). Unlike the core version, the
/// lifted root here carries an extra xmlns (col: for AvaloniaList, needed by the preview's
/// ItemsSource literals — see KuwantimaDataGrid.axaml), so every namespace the original root
/// declared is copied across, not just av/x.
/// </summary>
public class PreviewInvariantTests
{
    private static readonly XNamespace Av = "https://github.com/avaloniaui";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

    [AvaloniaFact]
    public void Invariant_7_preview_resolves_every_key_the_file_consumes()
    {
        const string file = "KuwantimaDataGrid.axaml";

        using var stream = typeof(PreviewInvariantTests).Assembly.GetManifestResourceStream("DataGridStyles/" + file)
            ?? throw new InvalidOperationException($"'DataGridStyles/{file}' is not embedded in the test assembly.");
        var text = new StreamReader(stream).ReadToEnd();
        var doc = XDocument.Parse(text);

        var preview = doc.Descendants(Av + "Design.PreviewWith").Single();
        var root = new XElement(preview.Elements().Single());

        // Copy every namespace declaration the real <Styles> root carried, not just av/x —
        // this file's preview also uses xmlns:col (Avalonia.Collections.AvaloniaList), which a
        // hardcoded av/x-only re-declaration would silently drop, breaking the parse rather than
        // the resource resolution this test is actually checking.
        foreach (var attr in doc.Root!.Attributes().Where(a => a.IsNamespaceDeclaration))
            root.SetAttributeValue(attr.Name, attr.Value);
        root.SetAttributeValue("xmlns", Av.NamespaceName);
        root.SetAttributeValue(XNamespace.Xmlns + "x", X.NamespaceName);

        var control = AvaloniaRuntimeXamlLoader.Parse<Control>(root.ToString());

        var selfDefined = doc.Descendants()
            .Select(e => (string?)e.Attribute(X + "Key"))
            .Where(k => k is not null)
            .ToHashSet()!;

        var consumed = Regex.Matches(text, @"\{(?:Dynamic|Static)Resource ([A-Za-z0-9.]+)\}")
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .Where(k => !selfDefined.Contains(k))
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToArray();

        foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
        {
            var missing = consumed.Where(k => !control.TryFindResource(k, variant, out _)).ToArray();

            Assert.True(missing.Length == 0,
                $"{file} [{variant}]: the preview cannot resolve {missing.Length} of {consumed.Length} "
                + $"keys the file references — {string.Join(", ", missing)}.");
        }
    }
}
