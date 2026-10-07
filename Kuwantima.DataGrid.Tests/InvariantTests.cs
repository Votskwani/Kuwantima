using System.Xml.Linq;
using Xunit;

namespace Kuwantima.DataGrid.Tests;

/// <summary>
/// Tier 2 — the completeness invariants from CLAUDE.md's New Control Checklist, executed against
/// this package's one control. Kuwantima.Tests runs a generalized, data-driven version of these
/// across 16 style files via a Subjects classification table; a one-control companion package
/// doesn't need that machinery, so these are direct, explicit assertions against the real shipped
/// markup instead — parsed as XML, never grepped as text, same discipline as the core suite.
/// </summary>
public class InvariantTests
{
    private static readonly XNamespace Av = "https://github.com/avaloniaui";

    private static XDocument LoadEmbedded(string logicalName)
    {
        using var stream = typeof(InvariantTests).Assembly.GetManifestResourceStream(logicalName)
            ?? throw new InvalidOperationException($"'{logicalName}' is not embedded in the test assembly.");

        return XDocument.Load(stream);
    }

    [Fact]
    public void KuwantimaDataGrid_style_is_registered_in_the_entry_point_exactly_once()
    {
        var theme = LoadEmbedded("DataGridTheme/KuwantimaDataGridTheme.axaml");

        var registrations = theme.Descendants(Av + "StyleInclude")
            .Select(e => (string?)e.Attribute("Source") ?? string.Empty)
            .Where(s => s.EndsWith("/Styles/KuwantimaDataGrid.axaml", StringComparison.Ordinal))
            .ToArray();

        Assert.True(registrations.Length == 1,
            $"Expected exactly one StyleInclude for KuwantimaDataGrid.axaml in KuwantimaDataGridTheme.axaml, found {registrations.Length}.");
    }

    [Fact]
    public void Entry_point_includes_the_vendor_DataGrid_base_theme_before_Kuwantima_overrides()
    {
        // Document order matters: the vendor's own ControlTemplate must load before Kuwantima's
        // resource-key overrides, or there is no template for the overrides to apply on top of.
        var theme = LoadEmbedded("DataGridTheme/KuwantimaDataGridTheme.axaml");

        var sources = theme.Descendants()
            .Select(e => (string?)e.Attribute("Source"))
            .Where(s => s is not null)
            .Select(s => s!)
            .ToArray();

        var vendorIndex = Array.FindIndex(sources, s => s.Contains("Avalonia.Controls.DataGrid/Themes/Fluent.xaml", StringComparison.Ordinal));
        var resourcesIndex = Array.FindIndex(sources, s => s.EndsWith("KuwantimaDataGridThemeResources.axaml", StringComparison.Ordinal));
        var styleIndex = Array.FindIndex(sources, s => s.EndsWith("/Styles/KuwantimaDataGrid.axaml", StringComparison.Ordinal));

        Assert.True(vendorIndex >= 0, "KuwantimaDataGridTheme.axaml does not include the vendor's DataGrid Fluent theme.");
        Assert.True(resourcesIndex >= 0, "KuwantimaDataGridTheme.axaml does not include KuwantimaDataGridThemeResources.axaml.");
        Assert.True(styleIndex >= 0, "KuwantimaDataGridTheme.axaml does not include KuwantimaDataGrid.axaml.");
        Assert.True(vendorIndex < resourcesIndex && resourcesIndex < styleIndex,
            "Document order must be: vendor base theme, then Kuwantima resource overrides, then the Kuwantima control style.");
    }

    [Fact]
    public void Interactive_parts_declare_Cursor_Hand()
    {
        // DataGridColumnHeader (sortable) and DataGridRow (selectable) are the two clickable
        // units — same reasoning as ListBoxItem (not ListBox) getting Hand in the core package.
        var style = LoadEmbedded("DataGridStyles/KuwantimaDataGrid.axaml");

        bool HasHandCursor(string selectorSuffix) =>
            style.Descendants(Av + "Style")
                .Where(s => ((string?)s.Attribute("Selector"))?.EndsWith(selectorSuffix, StringComparison.Ordinal) == true)
                .SelectMany(s => s.Elements(Av + "Setter"))
                .Any(setter => (string?)setter.Attribute("Property") == "Cursor"
                               && (string?)setter.Attribute("Value") == "Hand");

        Assert.True(HasHandCursor("DataGridColumnHeader"), "DataGridColumnHeader's base style must set Cursor=\"Hand\" — it's the sortable, clickable part.");
        Assert.True(HasHandCursor("DataGridRow"), "DataGridRow's base style must set Cursor=\"Hand\" — it's the selectable part.");
    }

    [Fact]
    public void Disabled_state_pins_foreground_opacity_and_cursor()
    {
        // The standard three, same as every control in the core package: without these, Fluent's
        // own disabled brushes double-dim on top of whatever Kuwantima already painted.
        var style = LoadEmbedded("DataGridStyles/KuwantimaDataGrid.axaml");

        var disabledStyle = style.Descendants(Av + "Style")
            .SingleOrDefault(s => (string?)s.Attribute("Selector") == "^:disabled");

        Assert.True(disabledStyle is not null, "DataGrid.Kuwantima must have a nested ^:disabled style.");

        var setters = disabledStyle!.Elements(Av + "Setter")
            .ToDictionary(s => (string)s.Attribute("Property")!, s => (string?)s.Attribute("Value"));

        Assert.True(setters.ContainsKey("Foreground"), "^:disabled must pin Foreground.");
        Assert.Equal("0.5", setters.GetValueOrDefault("Opacity"));
        Assert.Equal("Arrow", setters.GetValueOrDefault("Cursor"));
    }

    [Fact]
    public void Style_file_carries_a_Design_PreviewWith_with_its_own_theme_and_resources()
    {
        var style = LoadEmbedded("DataGridStyles/KuwantimaDataGrid.axaml");

        var preview = style.Descendants(Av + "Design.PreviewWith").SingleOrDefault();
        Assert.True(preview is not null, "KuwantimaDataGrid.axaml must carry a Design.PreviewWith.");

        var text = preview!.ToString();
        Assert.Contains("<FluentTheme", text);
        Assert.Contains("Avalonia.Controls.DataGrid/Themes/Fluent.xaml", text);
        Assert.Contains("avares://Kuwantima/Theme/KuwantimaThemeResources.axaml", text);
        Assert.Contains("avares://Kuwantima.DataGrid/Theme/KuwantimaDataGridThemeResources.axaml", text);
    }
}
