using System.Text.RegularExpressions;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;

namespace Kuwantima.Tests;

/// <summary>
/// The icon set — KuwantimaStreamIcons.axaml's Icon.* keys against everything that restates them.
///
/// The icon set is the control count's smaller sibling: one ResourceDictionary is ground truth, and
/// the README restates it three times over (a count, a key table, an attribution list) while the
/// file's own Design.PreviewWith restates it a fourth. Until v1.11.0 none of those were under test,
/// and the preview had already drifted — it showed 12 of the 15 keys that shipped. A key missing
/// from the README table is worse than it sounds, because a key that does not exist renders
/// NOTHING rather than throwing (see README, "Icons"); the table is the consumer's only way to tell
/// a typo from a bug.
///
/// Same rules as the rest of this class: keys are read from the parsed XML tree, never grepped,
/// and the README is sliced to its Icons section so the nav-recipe snippets elsewhere in it
/// (which legitimately mention Icon.Home and Icon.Gear) are not mistaken for the catalogue.
/// </summary>
public partial class InvariantTests
{
    private const string IconsFile = "KuwantimaStreamIcons.axaml";

    private static readonly Regex BacktickedIconKey =
        new(@"`(Icon\.[A-Za-z0-9.]+)`", RegexOptions.Compiled);

    private static readonly Regex ClaimedIconCount =
        new(@"(\d+)\s+icon\s+geometries\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex StaticResourceIconKey =
        new(@"\{StaticResource\s+(Icon\.[A-Za-z0-9.]+)\}", RegexOptions.Compiled);

    /// <summary>GROUND TRUTH: every Icon.* StreamGeometry the icon dictionary defines, with its path data.</summary>
    private static (string Key, string Data)[] IconGeometries() =>
        StyleDoc(IconsFile).Root!
            .Elements(Av + "StreamGeometry")
            .Select(e => (Key: (string?)e.Attribute(Xaml + "Key"), Data: e.Value.Trim()))
            .Where(g => g.Key is not null && g.Key.StartsWith("Icon.", StringComparison.Ordinal))
            .Select(g => (g.Key!, g.Data))
            .OrderBy(g => g.Item1, StringComparer.Ordinal)
            .ToArray();

    [AvaloniaFact]
    public void Published_icon_set_matches_the_icons_that_exist()
    {
        var actual = IconGeometries().Select(g => g.Key).ToArray();
        Assert.True(actual.Length > 0, $"{IconsFile} defines no Icon.* StreamGeometry — the ground truth is empty.");

        // Slice the README to its Icons section. The nav-recipe sections later in the file quote
        // Icon.Home / Icon.Gear as example keys; those are usage, not the catalogue.
        var readme = LoadText("README.md");
        var sectionStart = readme.IndexOf("\n## Icons", StringComparison.Ordinal);
        Assert.True(sectionStart >= 0, "README.md no longer has an '## Icons' section — this test reads the icon catalogue from it.");
        var attributionStart = readme.IndexOf("### Icon Attribution", sectionStart, StringComparison.Ordinal);
        Assert.True(attributionStart > sectionStart, "README.md's Icons section no longer has an '### Icon Attribution' subsection.");
        var sectionEnd = readme.IndexOf("\n## ", attributionStart, StringComparison.Ordinal);
        if (sectionEnd < 0) sectionEnd = readme.Length;

        var catalogue = readme[sectionStart..attributionStart];
        var attribution = readme[attributionStart..sectionEnd];

        // 1. The count claim.
        var claimed = ClaimedIconCount.Matches(catalogue).Select(m => int.Parse(m.Groups[1].Value)).ToArray();
        Assert.True(
            claimed.Length > 0,
            "README.md's Icons section no longer says 'N icon geometries', so the count can no longer be verified. "
            + "Either restore the claim or delete this assertion deliberately.");
        Assert.True(
            claimed.All(n => n == actual.Length),
            $"README.md claims {string.Join(" and ", claimed.Distinct())} icon geometries; {IconsFile} defines {actual.Length} "
            + $"({string.Join(", ", actual)}).");

        // 2. The key table — exactly the keys that exist, no more, no fewer.
        var listed = BacktickedIconKey.Matches(catalogue)
            .Select(m => m.Groups[1].Value)
            .Distinct()
            .OrderBy(k => k, StringComparer.Ordinal)
            .ToArray();
        var missingFromTable = actual.Except(listed).ToArray();
        var phantomInTable = listed.Except(actual).ToArray();
        Assert.True(
            missingFromTable.Length == 0 && phantomInTable.Length == 0,
            "README.md's Icons table disagrees with the dictionary." + Environment.NewLine
            + $"  shipped but not listed: [{string.Join(", ", missingFromTable)}]" + Environment.NewLine
            + $"  listed but not shipped: [{string.Join(", ", phantomInTable)}]" + Environment.NewLine
            + "A consumer reading the table is told that a listed key exists and an unlisted one does not — and an "
            + "unknown key renders blank rather than throwing, so the table is their only diagnostic.");

        // 3. Attribution — retaining credit is a licence condition for every borrowed glyph, so every
        //    key must appear in the attribution list (original geometry is listed there too, as original).
        var credited = BacktickedIconKey.Matches(attribution).Select(m => m.Groups[1].Value).Distinct().ToArray();
        var uncredited = actual.Except(credited).ToArray();
        Assert.True(
            uncredited.Length == 0,
            $"README.md's Icon Attribution list omits [{string.Join(", ", uncredited)}]. Every shipped key must be "
            + "credited to its source set (or declared original) — Material Design Icons (Apache 2.0) and Fluent "
            + "System Icons (MIT) both make retaining credit a licence condition.");

        // 4. The file's own preview must demonstrate every key, on BOTH sides. This is the one that had
        //    drifted: 12 of 15 at v1.10.0. Read from the Design.PreviewWith subtree, not the whole file,
        //    so the geometry definitions themselves do not count as a demonstration.
        var preview = StyleDoc(IconsFile).Root!.Element(Av + "Design.PreviewWith");
        Assert.NotNull(preview);
        var previewed = preview!.Descendants()
            .SelectMany(e => e.Attributes())
            .SelectMany(a => StaticResourceIconKey.Matches(a.Value).Select(m => m.Groups[1].Value))
            .GroupBy(k => k)
            .ToDictionary(g => g.Key, g => g.Count());
        var notPreviewed = actual.Where(k => !previewed.ContainsKey(k)).ToArray();
        var onlyOneSide = actual.Where(k => previewed.TryGetValue(k, out var n) && n < 2).ToArray();
        Assert.True(
            notPreviewed.Length == 0 && onlyOneSide.Length == 0,
            $"{IconsFile}'s Design.PreviewWith does not show every icon under both themes." + Environment.NewLine
            + $"  absent from the preview: [{string.Join(", ", notPreviewed)}]" + Environment.NewLine
            + $"  shown on one side only:  [{string.Join(", ", onlyOneSide)}]" + Environment.NewLine
            + "Add a row to both the Light and Dark ThemeVariantScope columns.");
    }

    [AvaloniaFact]
    public void Every_icon_geometry_parses_and_resolves_under_both_variants()
    {
        var geometries = IconGeometries();
        var app = Application.Current!;

        foreach (var (key, data) in geometries)
        {
            // Parsing is the check a copy-paste from an SVG actually needs: a truncated or mangled path
            // string fails here, loudly, instead of at the consumer's first render.
            var parsed = Record.Exception(() => StreamGeometry.Parse(data));
            Assert.True(parsed is null, $"{key}'s path data does not parse as a StreamGeometry: {parsed?.Message}");

            foreach (var variant in new[] { ThemeVariant.Light, ThemeVariant.Dark })
            {
                Assert.True(
                    app.TryGetResource(key, variant, out var value) && value is StreamGeometry,
                    $"{key} does not resolve to a StreamGeometry through the theme under {variant}. "
                    + $"{IconsFile} is merged into KuwantimaPrimaryTheme.axaml by ResourceInclude — check it still is.");
            }
        }
    }
}
