using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Xunit;

namespace Kuwantima.Tests;

/// <summary>
/// Invariant 7 — a style file's Design.PreviewWith must be able to resolve what it references.
///
/// A style file is self-contained by design, which is exactly why its preview was broken from
/// v1.0.0 until 2026-09-23: the setters reference brushes that live in KuwantimaThemeResources
/// and Fluent, and the file included neither. At runtime the consumer's single StyleInclude
/// supplies them; in the previewer only the one file is in scope, so every brush setter was
/// silently skipped and the preview rendered bare.
///
/// Nothing caught it, and nothing could have:
///   • It is not a build error. An unresolvable DynamicResource is SILENT — no throw, no warning.
///   • Invariant_5 is green, correctly. It asks whether a key resolves against the FULL theme,
///     which is the right question for the shipped library and the wrong one for the previewer.
///
/// That is the same shape as the Invariant_5 / Invariant_6 split: resolution is not readability,
/// and resolution-with-the-whole-theme is not resolution-in-isolation. This asks the third
/// question — can this file, ALONE, find what it points at?
///
/// It matters more than a test of a demo harness sounds. CLAUDE.md's "Who this is actually for"
/// puts the IDE previewer first: a beginner who opens a style file and sees something wrong has no
/// way to know it is not their fault. That is where a first-time contributor quits.
///
/// Deliberately checks BOTH variants. The theme resources are declared in ThemeDictionaries, so a
/// key can resolve under Light and be missing under Dark, and a preview shows both side by side.
/// </summary>
public class PreviewInvariantTests
{
    private static readonly XNamespace Av = "https://github.com/avaloniaui";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

    public static IEnumerable<object[]> StyleFiles() =>
        typeof(InvariantTests).Assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith("Styles/", StringComparison.Ordinal))
            .OrderBy(n => n, StringComparer.Ordinal)
            .Select(n => new object[] { n["Styles/".Length..] });

    [AvaloniaTheory]
    [MemberData(nameof(StyleFiles))]
    public void Invariant_7_preview_resolves_every_key_the_file_consumes(string file)
    {
        var text = ReadStyleSource(file);
        var doc = XDocument.Parse(text);

        var preview = doc.Descendants(Av + "Design.PreviewWith").SingleOrDefault();
        Assert.True(preview is not null,
            $"{file} has no <Design.PreviewWith>. Every style file carries one — see the New Control "
            + "Checklist in CLAUDE.md. Invariant_1 covers the light/dark shape of it.");

        // Re-declare the namespaces the root inherited from <Styles>, or constructs like {x:Null}
        // fail to parse once the subtree is lifted out on its own.
        var root = new XElement(preview!.Elements().Single());
        root.SetAttributeValue("xmlns", Av.NamespaceName);
        root.SetAttributeValue(XNamespace.Xmlns + "x", X.NamespaceName);

        var control = AvaloniaRuntimeXamlLoader.Parse<Control>(root.ToString());

        // Keys the file defines itself (StreamIcons' geometry) need not come from the theme.
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
                + $"keys the file references — {string.Join(", ", missing)}.\n\n"
                + "The preview renders these setters as if they were never written, with no error. "
                + "Fix it by adding the theme to the Design.PreviewWith ROOT (not by deleting the "
                + "reference):\n\n"
                + "    <{Root}.Styles><FluentTheme/></{Root}.Styles>\n"
                + "    <{Root}.Resources>\n"
                + "      <ResourceDictionary>\n"
                + "        <ResourceDictionary.MergedDictionaries>\n"
                + "          <ResourceInclude Source=\"avares://Kuwantima/Theme/KuwantimaThemeResources.axaml\"/>\n"
                + "        </ResourceDictionary.MergedDictionaries>\n"
                + "      </ResourceDictionary>\n"
                + "    </{Root}.Resources>\n\n"
                + "Add avares://Kuwantima/Styles/KuwantimaStreamIcons.axaml too if the preview uses "
                + "Icon.* geometry. Do NOT reach for KuwantimaPrimaryTheme — it carries the SHIPPED "
                + "control styles, so the preview would show you those instead of your edit.");
        }
    }

    private static string ReadStyleSource(string file)
    {
        using var stream = typeof(InvariantTests).Assembly.GetManifestResourceStream("Styles/" + file)
            ?? throw new InvalidOperationException(
                $"'Styles/{file}' is not embedded in the test assembly. Avalonia compiles .axaml to IL "
                + "and strips the markup, so the sources are linked in as <EmbeddedResource>. See "
                + "Kuwantima.Tests.csproj.");

        return new StreamReader(stream).ReadToEnd();
    }
}
