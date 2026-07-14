using System.Text.RegularExpressions;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Kuwantima.Tests;

/// <summary>
/// Tier 2 — executable invariants.
///
/// CLAUDE.md declares a set of completeness rules every Kuwantima control must satisfy: a
/// two-theme previewer, registration in the one public entry point, a fully pinned disabled
/// state, a hand cursor on anything clickable, no resurrected resource keys. Those rules are
/// currently enforced by a human remembering to re-read a checklist. These tests enforce them.
///
/// EVERYTHING HERE PARSES THE XML TREE. Nothing greps the text, and that is not fastidiousness —
/// a text scan gets all five of the following wrong:
///
///   • The literal string "Button.Kuwantima:disabled" appears in no file. Avalonia nests state
///     styles with a caret: &lt;Style Selector="^:disabled"&gt; inside &lt;Style Selector="Button.Kuwantima"&gt;.
///     Grep the expanded selector and every disabled pin in the library looks missing.
///   • KuwantimaPrimaryTheme.axaml and KuwantimaThemeResources.axaml *name* three retired keys —
///     in comments, documenting the migration away from them. XComment nodes are not XElements, so
///     a tree walk never sees them. A grep reports six violations that are actually documentation.
///   • &lt;StyleInclude Source="avares://Kuwantima/Styles/KuwantimaGlassBorder.axaml"/&gt; is live markup
///     containing the retired token "KuwantimaGlassBorder" — as a FILENAME. Stripping comments does
///     not save you here; only reading resource *references* does.
///   • &lt;BrushTransition Property="Foreground"/&gt; is not a &lt;Setter&gt;. KuwantimaExpander has four of
///     these. Grep Property="Foreground" and its missing disabled pin looks present.
///   • The live keys KuwantimaGlassGlowBorder / KuwantimaGlassGlowBorderHover contain a retired name
///     under any fuzzy regex (KuwantimaGlass.*Border). Keys are compared with exact equality instead.
///
/// The exemptions below (ProgressBar needs no hand cursor; ToolTip has no disabled state) are
/// argued, not asserted. An exemption the invariant genuinely allows is not a weakening. An
/// exemption invented to turn a red test green would be — so each one carries its reason in code.
/// </summary>
public class InvariantTests
{
    private static readonly XNamespace Av = "https://github.com/avaloniaui";
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml";

    // ───────────────────────────────────────────────────────────────────────────────
    // The subject table
    // ───────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// What each style file is, and which invariants apply to it. A null exemption means the
    /// invariant is REQUIRED; a non-null one is the argument for why it does not apply.
    ///
    /// <para><see cref="ForegroundPinExemption"/> exists because the disabled invariant is really
    /// three claims — pin Foreground, pin Cursor="Arrow", pin Opacity — and they do not stand or
    /// fall together. A control can have no text to double-dim while still very much needing the
    /// cursor and opacity pins. Collapsing them into one all-or-nothing exemption is how the
    /// GridSplitter originally escaped the invariant on the strength of an argument that was only
    /// true about Foreground.</para>
    /// </summary>
    private sealed record Subject(
        string Control,
        string? CursorExemption,
        string? DisabledExemption,
        string? ForegroundPinExemption = null);

    private static readonly Dictionary<string, Subject> Subjects = new(StringComparer.Ordinal)
    {
        ["KuwantimaButton.axaml"] = new("Button", null, null),
        ["KuwantimaCheckBox.axaml"] = new("CheckBox", null, null),
        ["KuwantimaComboBox.axaml"] = new("ComboBox", null, null),
        ["KuwantimaExpander.axaml"] = new("Expander", null, null),
        ["KuwantimaListBox.axaml"] = new("ListBox", null, null),
        ["KuwantimaMenuToggleButton.axaml"] = new("ToggleButton (.KuwantimaMenu)", null, null),
        ["KuwantimaRadioButton.axaml"] = new("RadioButton", null, null),
        ["KuwantimaSlider.axaml"] = new("Slider", null, null),
        ["KuwantimaTabControl.axaml"] = new("TabControl", null, null),
        ["KuwantimaToggleButton.axaml"] = new("ToggleButton", null, null),

        ["KuwantimaGlassBorder.axaml"] = new(
            "Border",
            CursorExemption:
                "Border is a Decorator — a decorative surface, not a pointer-activated control. Its "
                + ":pointerover only intensifies the glow, which is ambience, not an affordance.",
            DisabledExemption:
                "Border exposes no Foreground property at all and has no Fluent template to double-dim. "
                + "The pin is impossible to author here, not merely absent."),

        ["KuwantimaGridSplitter.axaml"] = new(
            "GridSplitter",
            CursorExemption:
                "A drag-to-resize handle must telegraph resizability, not clickability: it correctly sets "
                + "SizeWestEast / SizeNorthSouth. That exemption is only honest if the resize cursor is "
                + "really there, so it is checked instead by GridSplitter_sets_a_resize_cursor_on_every_variant.",
            // NOT exempt from the disabled invariant. It was, briefly, on the argument that a splitter
            // renders no text so there is nothing to double-dim. True — and true only of the Foreground
            // clause, which was then used to excuse the cursor and opacity clauses too. Those matter MORE
            // here than anywhere else in the library: a splitter has no label, no glyph, no content, so the
            // cursor and the dimming ARE its entire affordance. Kuwantima is a published package, so
            // "nothing in this repo disables a splitter" is a fact about us, not about our consumers.
            DisabledExemption: null,
            ForegroundPinExemption:
                "A GridSplitter renders no text — its rails, pill and chevrons are all Background-driven — so "
                + "nothing reaches the screen through Foreground and there is nothing for Fluent to dim twice. "
                + "The Cursor and Opacity pins still apply and are enforced. Note that the hover/press glow "
                + "needs no neutralising: Avalonia does not set :pointerover on a disabled control (unlike "
                + "WPF's IsMouseOver), verified headlessly, so those selectors are already dead. That leaves "
                + "Opacity as the ONLY signal that a splitter is inert."),

        ["KuwantimaProgressBar.axaml"] = new(
            "ProgressBar",
            CursorExemption:
                "A ProgressBar is a read-only status indicator: it cannot be clicked, dragged or activated. "
                + "The file corroborates this by pinning Cursor=\"Arrow\" — not Hand — when disabled.",
            DisabledExemption: null),

        ["KuwantimaTextBox.axaml"] = new(
            "TextBox",
            CursorExemption:
                "A text-entry field must keep the IBeam cursor; forcing Hand would be a UX regression. That "
                + "is why the base style deliberately sets no Cursor at all, and pins Arrow only where editing "
                + "is off (:disabled and the ReadOnly variant).",
            DisabledExemption: null),

        ["KuwantimaToolTip.axaml"] = new(
            "ToolTip",
            CursorExemption:
                "Transient popup chrome: a tooltip is dismissed by pointer movement and is never clicked.",
            DisabledExemption:
                "A ToolTip has no user-facing disabled state, so Fluent never applies disabled brushes to it "
                + "and there is nothing to double-dim."),

        ["KuwantimaStreamIcons.axaml"] = new(
            "(none — icon geometry)",
            CursorExemption:
                "A <ResourceDictionary> of StreamGeometry path data. It declares zero <Style> elements; there "
                + "is no control here to give a cursor to.",
            DisabledExemption:
                "Same: no control, no template, nothing for Fluent to double-dim."),
    };

    /// <summary>The eight keys CLAUDE.md retired in favour of Fluent equivalents.</summary>
    private static readonly string[] Retired =
    {
        "KuwantimaAccentForeground",
        "KuwantimaSecondaryTextBrush",
        "KuwantimaSubtitleTextBrush",
        "KuwantimaGlassBorder",
        "KuwantimaGlassBorderHover",
        "KuwantimaShadowNormal",
        "KuwantimaShadowHover",
        "KuwantimaShadowAccent",
    };

    // ───────────────────────────────────────────────────────────────────────────────
    // Loading — the library's markup does not survive compilation, so it is linked in
    // as an EmbeddedResource. See Kuwantima.Tests.csproj.
    // ───────────────────────────────────────────────────────────────────────────────

    private const string StylesPrefix = "Styles/";
    private const string ThemePath = "Theme/KuwantimaPrimaryTheme.axaml";

    private static string[] StyleFileNames() =>
        typeof(InvariantTests).Assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(StylesPrefix, StringComparison.Ordinal))
            .Select(n => n[StylesPrefix.Length..])
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

    private static Stream Open(string logicalName) =>
        typeof(InvariantTests).Assembly.GetManifestResourceStream(logicalName)
        ?? throw new InvalidOperationException(
            $"'{logicalName}' is not embedded in the test assembly, so this test cannot read it. Avalonia "
            + "compiles .axaml to IL and strips the markup, so the source file has to be linked in as an "
            + "<EmbeddedResource> with a LogicalName in Kuwantima.Tests.csproj. Currently embedded: ["
            + string.Join(", ", typeof(InvariantTests).Assembly.GetManifestResourceNames()) + "]");

    private static XDocument Load(string logicalName)
    {
        using var stream = Open(logicalName);
        return XDocument.Load(stream);
    }

    private static string LoadText(string logicalName)
    {
        using var reader = new StreamReader(Open(logicalName));
        return reader.ReadToEnd();
    }

    /// <summary>A style file, freshly parsed. Never cached — callers mutate it (see <see cref="StripPreview"/>).</summary>
    private static XDocument StyleDoc(string file) => Load(StylesPrefix + file);

    /// <summary>
    /// Design.PreviewWith is design-time scaffolding, not shipped style code, and it is full of decoys:
    /// it hands out Classes="Kuwantima" / Classes="KuwantimaGlass" to Buttons and Borders that belong to
    /// other files, hardcodes #808080 backdrops, and references keys the live style never touches. It is
    /// removed before any question is asked about what a style actually does.
    /// </summary>
    private static void StripPreview(XDocument doc)
    {
        foreach (var preview in doc.Descendants(Av + "Design.PreviewWith").ToArray())
            preview.Remove();
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // The style model
    // ───────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// One &lt;Style&gt; with its selector RESOLVED. Avalonia lets a nested style stand in for its parent
    /// with a caret, so &lt;Style Selector="^:disabled"&gt; inside &lt;Style Selector="Button.Kuwantima"&gt; really
    /// means "Button.Kuwantima:disabled" — a string that appears nowhere in the file. Resolving it here is
    /// what lets the invariants below be stated in terms of what the markup MEANS rather than how it is typed.
    /// The library uses both shapes (KuwantimaCheckBox nests; KuwantimaProgressBar is flat), so both must work.
    /// </summary>
    private sealed record Rule(string Selector, XElement Element)
    {
        /// <summary>A control-level selector: no pseudo-class, no template part.</summary>
        public bool IsBase => !Selector.Contains(':') && !Selector.Contains("/template/", StringComparison.Ordinal);

        public bool IsDisabled => Selector.Contains(":disabled", StringComparison.Ordinal);

        /// <summary>
        /// DIRECT &lt;Setter&gt; children only. This is load-bearing: a &lt;Setter Property="Transitions"&gt; wraps
        /// &lt;BrushTransition Property="Foreground"/&gt; elements, which carry a Property attribute but set nothing.
        /// Taking direct Setter children excludes them by construction.
        /// </summary>
        public IEnumerable<(string Property, string Value)> Setters =>
            Element.Elements(Av + "Setter")
                   .Select(e => (P: (string?)e.Attribute("Property"), V: (string?)e.Attribute("Value")))
                   .Where(t => t.P is not null)
                   .Select(t => (t.P!, t.V ?? string.Empty));
    }

    private static List<Rule> RulesOf(string file)
    {
        var doc = StyleDoc(file);
        StripPreview(doc);

        var rules = new List<Rule>();
        Walk(doc.Root!, parentSelector: null, rules);
        return rules;
    }

    private static void Walk(XElement parent, string? parentSelector, List<Rule> into)
    {
        // Elements(), not Descendants(): nested styles are DIRECT children of their parent style. Walking
        // descendants would climb into a <Setter Property="Template">'s ControlTemplate and treat any style
        // declared in there as a top-level rule.
        foreach (var style in parent.Elements(Av + "Style"))
        {
            var raw = ((string?)style.Attribute("Selector") ?? string.Empty).Trim();
            var resolved = parentSelector is null ? raw : raw.Replace("^", parentSelector);

            into.Add(new Rule(resolved, style));
            Walk(style, resolved, into);
        }
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // Theory data
    // ───────────────────────────────────────────────────────────────────────────────

    public static IEnumerable<object[]> AllStyleFiles() =>
        StyleFileNames().Select(f => new object[] { f });

    public static IEnumerable<object[]> FilesNeedingDisabledPin() =>
        StyleFileNames().Where(f => Subjects.TryGetValue(f, out var s) && s.DisabledExemption is null)
                        .Select(f => new object[] { f });

    public static IEnumerable<object[]> InteractiveFiles() =>
        StyleFileNames().Where(f => Subjects.TryGetValue(f, out var s) && s.CursorExemption is null)
                        .Select(f => new object[] { f });

    /// <summary>Every .axaml the library ships, style files and theme alike.</summary>
    public static IEnumerable<object[]> AllShippedMarkup() =>
        StyleFileNames().Select(f => new object[] { StylesPrefix + f })
            .Append(new object[] { ThemePath })
            .Append(new object[] { "KuwantimaThemeResources.axaml" });

    // ───────────────────────────────────────────────────────────────────────────────
    // Meta: the suite must not be able to pass vacuously
    // ───────────────────────────────────────────────────────────────────────────────

    [AvaloniaFact]
    public void Every_style_file_is_classified()
    {
        var embedded = StyleFileNames();

        Assert.True(
            embedded.Length >= 16,
            $"Only {embedded.Length} style files were embedded, so most of this suite would silently test "
            + "nothing. The <EmbeddedResource Include=\"..\\Kuwantima\\Styles\\*.axaml\"> glob in "
            + "Kuwantima.Tests.csproj is not matching.");

        var unclassified = embedded.Where(f => !Subjects.ContainsKey(f)).OrderBy(f => f).ToArray();
        var stale = Subjects.Keys.Where(f => !embedded.Contains(f)).OrderBy(f => f).ToArray();

        Assert.True(
            unclassified.Length == 0,
            $"New style file(s) with no entry in InvariantTests.Subjects: {string.Join(", ", unclassified)}."
            + Environment.NewLine
            + "Add each one and state two things: is the control pointer-activated (does it need Cursor=\"Hand\"), "
            + "and does it have a disabled state to pin? Passing null means 'the invariant applies'. Passing a "
            + "reason string exempts it — and that reason has to be defensible, because it is the only thing "
            + "standing between the control and the checklist.");

        Assert.True(
            stale.Length == 0,
            $"InvariantTests.Subjects classifies files that no longer exist: {string.Join(", ", stale)}.");
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // Invariant 1 — Design.PreviewWith covering BOTH themes
    // ───────────────────────────────────────────────────────────────────────────────

    [AvaloniaTheory]
    [MemberData(nameof(AllStyleFiles))]
    public void Invariant_1_previews_both_light_and_dark(string file)
    {
        var previews = StyleDoc(file).Descendants(Av + "Design.PreviewWith").ToArray();

        Assert.True(
            previews.Length > 0,
            $"{file} declares no <Design.PreviewWith>. CLAUDE.md's New Control Checklist requires one so the "
            + "style can be eyeballed in the XAML previewer without launching the sandbox.");

        var variants = previews
            .SelectMany(p => p.Descendants(Av + "ThemeVariantScope"))
            .Select(s => (string?)s.Attribute("RequestedThemeVariant"))
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = new[] { "Light", "Dark" }.Where(v => !variants.Contains(v)).ToArray();

        Assert.True(
            missing.Length == 0,
            $"{file}: its <Design.PreviewWith> never previews {string.Join(" or ", missing)}. Wrap the samples "
            + "in a <ThemeVariantScope RequestedThemeVariant=\"Light\"> AND a \"Dark\" one. A glass-glow style "
            + "that reads correctly in one variant and washes out in the other is exactly the failure this "
            + $"previewer exists to make visible. Variants found: [{string.Join(", ", variants.OrderBy(v => v))}]");
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // Invariant 2 — registered in the one public entry point
    // ───────────────────────────────────────────────────────────────────────────────

    /// <summary>Leaf filenames of every &lt;StyleInclude&gt;/&lt;ResourceInclude&gt; in the theme pointing into Styles/.</summary>
    private static string[] IncludedFiles(XDocument theme, string element) =>
        theme.Descendants(Av + element)
             .Select(e => (string?)e.Attribute("Source") ?? string.Empty)
             .Where(s => s.Contains("/Styles/", StringComparison.Ordinal))
             .Select(s => s[(s.LastIndexOf('/') + 1)..])
             .ToArray();

    [AvaloniaTheory]
    [MemberData(nameof(AllStyleFiles))]
    public void Invariant_2_registered_in_the_primary_theme(string file)
    {
        var theme = Load(ThemePath);
        var isResourceDictionary = StyleDoc(file).Root!.Name == Av + "ResourceDictionary";

        if (isResourceDictionary)
        {
            // KuwantimaStreamIcons.axaml is a ResourceDictionary of icon geometry, not a <Styles> tree.
            // A ResourceDictionary root CANNOT be loaded by <StyleInclude> — so <ResourceInclude> is not an
            // exception to this invariant, it is the only way to satisfy it. (A test that demands a
            // StyleInclude for every file in Styles/ fails here on correct code.)
            var merged = IncludedFiles(theme, "ResourceInclude");
            Assert.True(
                merged.Contains(file),
                $"{file} has a <ResourceDictionary> root but is not merged into KuwantimaPrimaryTheme.axaml, so "
                + "nothing it declares reaches a consumer. Add <ResourceInclude Source=\"avares://Kuwantima/"
                + $"Styles/{file}\"/> under Styles.Resources. Currently merged: [{string.Join(", ", merged)}]");
        }
        else
        {
            var included = IncludedFiles(theme, "StyleInclude");
            Assert.True(
                included.Contains(file),
                $"{file} declares styles but is not registered in KuwantimaPrimaryTheme.axaml. The theme is the "
                + "library's single entry point, so an unregistered style file ships to NuGet and does nothing — "
                + "the control silently falls back to plain Fluent. Add <StyleInclude Source=\"avares://Kuwantima/"
                + $"Styles/{file}\"/> to section 2 (Control Styles).");
        }
    }

    [AvaloniaFact]
    public void Invariant_2_theme_registers_each_style_file_exactly_once()
    {
        var theme = Load(ThemePath);
        var all = IncludedFiles(theme, "StyleInclude").Concat(IncludedFiles(theme, "ResourceInclude")).ToArray();

        var duplicated = all.GroupBy(f => f, StringComparer.Ordinal)
                            .Where(g => g.Count() > 1)
                            .Select(g => $"{g.Key} (×{g.Count()})")
                            .ToArray();

        Assert.True(
            duplicated.Length == 0,
            "KuwantimaPrimaryTheme.axaml registers the same style file more than once — later entries override "
            + $"earlier ones, so this is a silent way to make load order meaningless: {string.Join(", ", duplicated)}");

        var known = StyleFileNames();
        var dangling = all.Where(f => !known.Contains(f, StringComparer.Ordinal)).Distinct().ToArray();

        Assert.True(
            dangling.Length == 0,
            "KuwantimaPrimaryTheme.axaml registers files that do not exist in Kuwantima/Styles/: "
            + $"{string.Join(", ", dangling)}");
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // Invariant 3 — the disabled state pins Foreground, Cursor="Arrow" and Opacity
    // ───────────────────────────────────────────────────────────────────────────────

    [AvaloniaTheory]
    [MemberData(nameof(FilesNeedingDisabledPin))]
    public void Invariant_3_disabled_state_is_fully_pinned(string file)
    {
        var disabled = RulesOf(file).Where(r => r.IsDisabled).ToArray();

        // The pin is legitimately SPLIT across several :disabled selectors, and that is not sloppiness:
        //   • KuwantimaTextBox pins Opacity/Foreground/Cursor on the control and Background/BorderBrush on
        //     Border#PART_BorderElement, because the template's Border — not the TextBox — paints the chrome.
        //   • KuwantimaButton, CheckBox and RadioButton repeat it per variant (.Accent, .Classic).
        //   • KuwantimaComboBox and ListBox add a second, deliberately thinner :disabled block for their item
        //     container, which inherits the rest.
        // So the invariant is checked against the UNION of every :disabled selector in the file. Demanding all
        // three pins inside EVERY :disabled block would fail five compliant files.
        var pinned = disabled.SelectMany(r => r.Setters).ToArray();

        // The three clauses are checked independently. A control may be exempt from the Foreground pin
        // (nothing renders through it) while still owing the Cursor and Opacity pins — see Subject.
        var foregroundExempt = Subjects[file].ForegroundPinExemption is not null;

        var missing = new List<string>();
        if (!foregroundExempt && !pinned.Any(s => s.Property == "Foreground")) missing.Add("Foreground");
        if (!pinned.Any(s => s.Property == "Cursor" && s.Value == "Arrow")) missing.Add("Cursor=\"Arrow\"");
        if (!pinned.Any(s => s.Property == "Opacity")) missing.Add("Opacity");

        var found = disabled.Length == 0
            ? "It declares no :disabled selector at all."
            : $"Its {disabled.Length} :disabled selector(s) — {string.Join(" | ", disabled.Select(d => d.Selector))} — "
              + $"set only [{string.Join(", ", pinned.Select(s => s.Property).Distinct().OrderBy(p => p))}] between them.";

        Assert.True(
            missing.Count == 0,
            $"{file} ({Subjects[file].Control}) does not fully pin its disabled state. "
            + $"Missing: {string.Join(", ", missing)}." + Environment.NewLine
            + found + Environment.NewLine
            + "CLAUDE.md requires a disabled control to pin Foreground, Cursor=\"Arrow\" and Opacity. Each one "
            + "buys something concrete: without the Foreground pin, Fluent's own disabled brush lands on top of "
            + "Kuwantima's Opacity=0.5 and the text is dimmed twice — the 'double-dimming' the pin exists to "
            + "prevent, and the reason every other control in the library pins Foreground to SystemBaseHighColor. "
            + "Without Cursor=\"Arrow\", a dead control keeps advertising itself as usable under the pointer.");
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // Invariant 4 — interactive controls set Cursor="Hand"
    // ───────────────────────────────────────────────────────────────────────────────

    private static string[] CursorValuesIn(XDocument styleDoc) =>
        styleDoc.Descendants(Av + "Setter")
            .Where(s => (string?)s.Attribute("Property") == "Cursor")
            .Select(s => (string?)s.Attribute("Value") ?? "(none)")
            .Concat(styleDoc.Descendants()
                            .Select(e => (string?)e.Attribute("Cursor"))
                            .Where(v => v is not null)
                            .Select(v => v!))
            .Distinct()
            .OrderBy(v => v, StringComparer.Ordinal)
            .ToArray();

    [AvaloniaTheory]
    [MemberData(nameof(InteractiveFiles))]
    public void Invariant_4_interactive_controls_set_a_hand_cursor(string file)
    {
        var doc = StyleDoc(file);
        StripPreview(doc);

        // Cursor="Hand" is legal in TWO shapes, and both are live markup:
        //   (a) <Setter Property="Cursor" Value="Hand"/>  — how nine of the ten interactive files do it.
        //   (b) an inline Cursor="Hand" attribute inside a ControlTemplate — KuwantimaExpander puts it on
        //       Border#HeaderBorder and ToggleButton#PART_toggle, which together ARE the clickable surface.
        // A check that only accepts (a) reports a bug in Expander that does not exist. Accept both.
        var asSetter = doc.Descendants(Av + "Setter").Any(s =>
            (string?)s.Attribute("Property") == "Cursor" && (string?)s.Attribute("Value") == "Hand");

        var asInlineAttribute = doc.Descendants().Any(e => (string?)e.Attribute("Cursor") == "Hand");

        // Nor is it required on the *base* selector, despite CLAUDE.md's wording. KuwantimaTabControl puts it
        // on `TabControl.Kuwantima TabItem` and KuwantimaListBox on `ListBox.Kuwantima ListBoxItem` — correctly,
        // because the tab header and the row are the click targets while the container is not. Presence in the
        // file's live style markup is the honest form of the invariant.
        var suggestions = RulesOf(file).Where(r => r.IsBase).Select(r => r.Selector).ToArray();

        Assert.True(
            asSetter || asInlineAttribute,
            $"{file} ({Subjects[file].Control}) is pointer-activated but never sets Cursor=\"Hand\" anywhere in "
            + "its live style markup, so it looks inert under the pointer while every other clickable Kuwantima "
            + "control changes the cursor." + Environment.NewLine
            + $"Cursor values this file DOES set: [{string.Join(", ", CursorValuesIn(doc))}] — note that pinning "
            + "Arrow on :disabled is the opposite invariant, not this one." + Environment.NewLine
            + "Add <Setter Property=\"Cursor\" Value=\"Hand\"/> to the base style"
            + (suggestions.Length > 0 ? $" (i.e. {string.Join(" / ", suggestions)})" : string.Empty)
            + ", or — as KuwantimaExpander does — as an inline Cursor=\"Hand\" attribute on the clickable "
            + "element inside its ControlTemplate.");
    }

    [AvaloniaFact]
    public void Invariant_4_gridsplitter_sets_a_resize_cursor_on_every_variant()
    {
        // GridSplitter is the one interactive control exempt from Cursor="Hand": you drag it, you do not click
        // it, so it must show a resize cursor instead. That exemption is only honest if the resize cursor is
        // actually there — otherwise "it's exempt" quietly becomes "it has no cursor at all". This asserts the
        // thing the exemption stands in for.
        const string File = "KuwantimaGridSplitter.axaml";
        var resizeCursors = new[] { "SizeWestEast", "SizeNorthSouth" };

        var variants = RulesOf(File).Where(r => r.IsBase).ToArray();

        Assert.True(
            variants.Length >= 6,
            $"Expected at least the 6 GridSplitter variants (base, Horizontal, Pill, Pill Vertical, Arrow, "
            + $"Arrow Vertical) but parsed {variants.Length} control-level selectors — the parser has lost track "
            + "of the file.");

        var cursorless = variants
            .Where(v => !v.Setters.Any(s => s.Property == "Cursor" && resizeCursors.Contains(s.Value)))
            .Select(v => v.Selector)
            .ToArray();

        Assert.True(
            cursorless.Length == 0,
            $"{File}: these GridSplitter variants set no resize cursor, so they give the user no signal that they "
            + $"can be dragged: {string.Join(", ", cursorless)}. Each variant's base selector must set Cursor to "
            + "SizeWestEast (vertical splitter) or SizeNorthSouth (horizontal splitter).");
    }

    /// <summary>
    /// Every other test in this file reasons about the MARKUP. This one drives a live control, because the
    /// splitter's disabled block rests on a claim about the framework, not about our XAML: that Avalonia
    /// resolves competing setters by DOCUMENT ORDER rather than selector specificity, so one late
    /// "GridSplitter.Kuwantima:disabled" overrides the resize cursor that all six variant base styles set
    /// above it. If that ever stops being true — an Avalonia upgrade, a reordering of the file — the markup
    /// still parses, the static tests still pass, and a disabled splitter silently goes back to advertising
    /// itself as draggable. Only a real control can catch that.
    /// </summary>
    [AvaloniaTheory]
    [InlineData("Kuwantima", "SizeWestEast")]
    [InlineData("Kuwantima Horizontal", "SizeNorthSouth")]
    [InlineData("Kuwantima Pill", "SizeNorthSouth")]
    [InlineData("Kuwantima Pill Vertical", "SizeWestEast")]
    [InlineData("Kuwantima Arrow", "SizeNorthSouth")]
    [InlineData("Kuwantima Arrow Vertical", "SizeWestEast")]
    public void Disabled_splitter_drops_its_resize_cursor_and_dims(string classes, string enabledCursor)
    {
        GridSplitter Splitter(bool enabled)
        {
            var s = new GridSplitter { IsEnabled = enabled };
            foreach (var c in classes.Split(' ')) s.Classes.Add(c);
            return s;
        }

        var enabledSplitter = Splitter(true);
        var disabledSplitter = Splitter(false);

        var window = new Window
        {
            Content = new StackPanel { Children = { enabledSplitter, disabledSplitter } },
            Width = 300,
            Height = 300,
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // Cursor overrides Equals with reference semantics, so compare the resolved name.
        Assert.Equal(enabledCursor, enabledSplitter.Cursor?.ToString());
        Assert.Equal(1d, enabledSplitter.Opacity, 3);

        Assert.Equal(
            new Cursor(StandardCursorType.Arrow).ToString(),
            disabledSplitter.Cursor?.ToString());
        Assert.Equal(0.5d, disabledSplitter.Opacity, 3);
    }

    /// <summary>
    /// The splitter's disabled styling is only worth anything if it fires the way consumers actually
    /// disable things — by disabling a whole PANE, not by reaching in and disabling the splitter itself.
    /// That works because :disabled tracks IsEffectivelyEnabled, which inherits down the tree: note that
    /// the splitter's own IsEnabled stays true here. If Avalonia ever scoped the pseudo-class to the
    /// element's own IsEnabled instead, our disabled block would silently stop firing in the common case
    /// while every other test in this file still passed.
    /// </summary>
    [AvaloniaFact]
    public void Disabled_styling_reaches_a_splitter_inside_a_disabled_ancestor()
    {
        var splitter = new GridSplitter();
        splitter.Classes.Add("Kuwantima");

        var pane = new Grid { IsEnabled = false };
        pane.Children.Add(splitter);

        var window = new Window { Content = pane, Width = 300, Height = 300 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        Assert.True(splitter.IsEnabled, "Precondition: the splitter itself is never disabled — the PANE is.");
        Assert.False(splitter.IsEffectivelyEnabled);

        Assert.Equal(new Cursor(StandardCursorType.Arrow).ToString(), splitter.Cursor?.ToString());
        Assert.Equal(0.5d, splitter.Opacity, 3);
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // Retired resources
    // ───────────────────────────────────────────────────────────────────────────────

    private static readonly Regex ResourceReference =
        new(@"\{\s*(?:Dynamic|Static)Resource\s+([^\s},]+)", RegexOptions.Compiled);

    /// <summary>
    /// Every resource KEY the document references or declares — and nothing else.
    ///
    /// This is the whole defence against the retired-name false positives. Keys are read from
    /// {DynamicResource X} / {StaticResource X} markup extensions and from x:Key declarations, both of which
    /// live in ATTRIBUTES of ELEMENTS. Consequently:
    ///   • Comments are never visited (XComment is not XElement), so the migration notes that name three
    ///     retired keys in KuwantimaPrimaryTheme.axaml and KuwantimaThemeResources.axaml are invisible.
    ///   • Source="avares://Kuwantima/Styles/KuwantimaGlassBorder.axaml" is not a resource reference, so the
    ///     live StyleInclude whose FILENAME happens to contain a retired token is never extracted.
    ///   • Keys are returned whole and compared with exact equality, so KuwantimaGlassGlowBorder does not
    ///     collide with the retired KuwantimaGlassBorder.
    /// </summary>
    private static string[] ReferencedKeys(XDocument doc)
    {
        var fromDeclarations = doc.Descendants()
            .Select(e => (string?)e.Attribute(Xaml + "Key"))
            .Where(k => k is not null)
            .Select(k => k!);

        return ConsumedKeys(doc).Concat(fromDeclarations).Distinct(StringComparer.Ordinal).ToArray();
    }

    /// <summary>
    /// The keys a document CONSUMES — {DynamicResource X} / {StaticResource X} only, with x:Key declarations
    /// excluded. <see cref="ReferencedKeys"/> deliberately lumps the two together, because DECLARING a retired
    /// key is as much a violation as using one. The resolution invariant below needs the opposite: a declaration
    /// is trivially resolvable (it is the definition), so folding declarations in would make the test tautological
    /// for every key ThemeResources defines.
    /// </summary>
    private static string[] ConsumedKeys(XDocument doc) =>
        doc.Descendants()
           .SelectMany(e => e.Attributes())
           .SelectMany(a => ResourceReference.Matches(a.Value))
           .Select(m => m.Groups[1].Value.Trim())
           .Distinct(StringComparer.Ordinal)
           .ToArray();

    private static string[] RetiredKeysIn(XDocument doc) =>
        ReferencedKeys(doc).Where(k => Retired.Contains(k, StringComparer.Ordinal))
                           .OrderBy(k => k, StringComparer.Ordinal)
                           .ToArray();

    [AvaloniaTheory]
    [MemberData(nameof(AllShippedMarkup))]
    public void Retired_resources_are_never_referenced(string logicalName)
    {
        var doc = Load(logicalName);
        var keys = ReferencedKeys(doc);

        Assert.True(
            keys.Length > 0,
            $"{logicalName}: the key extractor found no resource references at all. Every shipped .axaml uses "
            + "at least one, so the parser has stopped matching the markup and this test has gone vacuous.");

        var violations = RetiredKeysIn(doc);

        Assert.True(
            violations.Length == 0,
            $"{logicalName} references resource key(s) that CLAUDE.md retired: {string.Join(", ", violations)}."
            + Environment.NewLine
            + "These were replaced by Fluent equivalents so the theme tracks system accent and high-contrast "
            + "changes automatically: KuwantimaAccentForeground → AccentButtonForeground, "
            + "KuwantimaSecondaryTextBrush → SystemControlForegroundBaseMediumHighBrush, KuwantimaSubtitleTextBrush "
            + "→ SystemControlForegroundBaseMediumBrush. The GlassBorder and Shadow keys were removed outright. "
            + "A retired key resolves to nothing, so the property silently falls back to its default."
            + Environment.NewLine
            + "Those three replacements are not the ones this message named until v1.2.0: it sent authors to the "
            + "WinUI TextFillColor*/TextOnAccentFillColor* keys, which Avalonia 12's Fluent does not ship. This "
            + "test enforced a migration onto keys that resolved to nothing — the exact failure it warns about, "
            + "in its own last sentence. Invariant_5_every_consumed_resource_key_resolves now catches that.");
    }

    [AvaloniaFact]
    public void Retired_resource_scanner_detects_a_real_violation_and_ignores_the_two_decoys()
    {
        // A scanner that quietly matches nothing is worse than no scanner, and this one is deliberately narrow
        // enough that "it found nothing" is a plausible bug. So: prove it fires on a genuine retired reference,
        // and prove it stays silent on the two shapes in this repo that legitimately contain a retired token —
        // the comment and the StyleInclude filename. If someone loosens ReferencedKeys into a text scan, this
        // test goes red before it can start flagging documentation as a violation.
        var probe = XDocument.Parse(
            """
            <Styles xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
              <!-- KuwantimaShadowNormal and KuwantimaGlassBorderHover were retired: a comment, not a usage. -->
              <StyleInclude Source="avares://Kuwantima/Styles/KuwantimaGlassBorder.axaml"/>
              <Style Selector="Border.Probe">
                <Setter Property="BorderBrush" Value="{DynamicResource KuwantimaGlassGlowBorder}"/>
                <Setter Property="Background" Value="{DynamicResource KuwantimaAccentForeground}"/>
              </Style>
            </Styles>
            """);

        Assert.Equal(new[] { "KuwantimaAccentForeground" }, RetiredKeysIn(probe));
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // Published control count
    // ───────────────────────────────────────────────────────────────────────────────

    private static readonly Regex ClaimedControlCount =
        new(@"(\d+)\s+(?:styled\s+)?controls\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static int[] CountsClaimedIn(string text) =>
        ClaimedControlCount.Matches(text).Select(m => int.Parse(m.Groups[1].Value)).ToArray();

    [AvaloniaFact]
    public void Published_control_count_matches_the_controls_that_exist()
    {
        // GROUND TRUTH: one style file per styled control. KuwantimaStreamIcons.axaml is excluded because its
        // root is a <ResourceDictionary> of icon geometry — it declares zero <Style> elements and is merged with
        // <ResourceInclude>, not <StyleInclude>. It is not a control, and the theme header says so itself when it
        // writes "... ToolTip) + StreamIcons".
        var controlFiles = StyleFileNames()
            .Where(f => StyleDoc(f).Root!.Name == Av + "Styles")
            .OrderBy(f => f, StringComparer.Ordinal)
            .ToArray();
        var actual = controlFiles.Length;

        // Corroborate ground truth against the theme's own registration list before trusting it.
        var styleIncludes = IncludedFiles(Load(ThemePath), "StyleInclude");
        Assert.True(
            styleIncludes.Length == actual,
            $"Ground truth is ambiguous: {actual} style file(s) declare styles but the theme has "
            + $"{styleIncludes.Length} <StyleInclude>(s). Fix Invariant_2 first — the control count cannot be "
            + "checked until the theme and the folder agree.");

        var readme = LoadText("README.md");
        var description = XDocument.Parse(LoadText("Kuwantima.csproj"))
            .Descendants("Description").Single().Value;

        // The theme's count lives in its file-header comment. XComment is a first-class node, so this reads a
        // known node — it is not a text grep hoping to land in the right place.
        var themeHeader = Load(ThemePath).Nodes().OfType<XComment>().First().Value;

        var claims = new (string Source, int[] Counts)[]
        {
            ("README.md", CountsClaimedIn(readme)),
            ("Kuwantima/Kuwantima.csproj  <Description>", CountsClaimedIn(description)),
            ("Kuwantima/Theme/KuwantimaPrimaryTheme.axaml  header comment", CountsClaimedIn(themeHeader)),
        };

        foreach (var (source, counts) in claims)
            Assert.True(
                counts.Length > 0,
                $"{source} no longer states a control count in the form 'N styled controls', so this test can no "
                + "longer verify it. Either restore the claim or delete this assertion deliberately.");

        var wrong = claims.Where(c => c.Counts.Any(n => n != actual)).ToArray();

        // Two independent enumerations inside the very documents that get the summary number wrong — quoted in
        // the failure so the fix is obvious without opening anything.
        var readmeTableRows = ReadmeControlsTableRowCount(readme);

        Assert.True(
            wrong.Length == 0,
            $"The published control count is wrong. Kuwantima ships {actual} styled controls "
            + $"({string.Join(", ", controlFiles.Select(f => f["Kuwantima".Length..^".axaml".Length]))})."
            + Environment.NewLine + Environment.NewLine
            + string.Join(Environment.NewLine,
                claims.Select(c => $"  {(c.Counts.All(n => n == actual) ? "ok  " : "WRONG")}  {c.Source} "
                                   + $"claims {string.Join(" and ", c.Counts.Distinct())}"))
            + Environment.NewLine + Environment.NewLine
            + $"Both documents contradict their own summary: the README's Controls table lists {readmeTableRows} "
            + $"rows, and the theme header's parenthetical names {actual} controls before adding \"+ StreamIcons\" "
            + "— which is the likely origin of the error, StreamIcons being counted as a control when it is a "
            + "ResourceDictionary of icon geometry." + Environment.NewLine
            + $"Fix: say {actual} in all three places, or ship a {actual + 1}th control.");
    }

    /// <summary>Rows in the README's Controls table — corroborating evidence, quoted in the failure above.</summary>
    private static int ReadmeControlsTableRowCount(string readme)
    {
        var lines = readme.Split('\n').Select(l => l.TrimEnd('\r').TrimStart()).ToArray();
        var header = Array.FindIndex(lines, l => l.StartsWith("| Control ", StringComparison.Ordinal));

        return header < 0
            ? -1
            : lines.Skip(header + 2)                      // skip the header row and the |---|---| separator
                   .TakeWhile(l => l.StartsWith('|'))
                   .Count();
    }

    // ───────────────────────────────────────────────────────────────────────────────
    // Invariant 5 — every key the library CONSUMES actually resolves
    // ───────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// ThemeIntegrityTests proves every key the theme DEFINES resolves under both variants. Nothing proved the
    /// same of the keys it CONSUMES — and the gap shipped a real bug for two releases.
    ///
    /// Avalonia 12's Fluent theme does not carry the WinUI "FillColor" resource family that Avalonia 11 did:
    /// TextFillColor*, TextOnAccentFillColor*, ControlFillColor* and AccentFillColor* are all absent. Kuwantima
    /// referenced two of them — TextOnAccentFillColorPrimaryBrush in seven style files (the Foreground of every
    /// accent control, and the Stroke/Fill of every checkmark and radio dot) and TextFillColorSecondaryBrush in
    /// two more. An unresolvable DynamicResource does not throw and does not fail the build: the setter is simply
    /// never applied and the property keeps its default. So accent buttons rendered black-on-blue, and no test —
    /// and no build — said a word.
    ///
    /// This closes that hole for the whole class of failure, not just the two keys that caused it. It is the
    /// mechanical half of CLAUDE.md's rule that agreement between sources sharing an origin is not verification:
    /// the style files, the theme header and the retired-keys migration note all AGREED on
    /// TextOnAccentFillColorPrimaryBrush. They were unanimous, and they were wrong, because none of them had
    /// asked the framework. This test asks the framework.
    /// </summary>
    [AvaloniaTheory]
    [MemberData(nameof(AllShippedMarkup))]
    public void Invariant_5_every_consumed_resource_key_resolves(string logicalName)
    {
        var app = Application.Current
                  ?? throw new InvalidOperationException("No Application — this must run as an [AvaloniaTheory].");

        var doc = Load(logicalName);
        var consumed = ConsumedKeys(doc);

        // Non-vacuity guard. A file may legitimately consume NOTHING — KuwantimaThemeResources.axaml is pure
        // definition, every brush built from a literal colour — so "zero references" is only suspicious in a file
        // that declares no resources either. Such a file must be markup the parser has stopped understanding.
        var declaresResources = doc.Descendants().Any(e => e.Attribute(Xaml + "Key") is not null);

        Assert.True(
            consumed.Length > 0 || declaresResources,
            $"{logicalName}: the extractor found neither a {{DynamicResource}}/{{StaticResource}} reference nor an "
            + "x:Key declaration. A shipped .axaml that does neither is not markup this parser still understands — "
            + "the test has gone vacuous, which is worse than not having it.");

        if (consumed.Length == 0)
            return;   // definitions-only dictionary: nothing consumed, nothing to resolve.

        var dead = new List<string>();
        foreach (var key in consumed.OrderBy(k => k, StringComparer.Ordinal))
        {
            var inLight = app.TryGetResource(key, ThemeVariant.Light, out var light) && light is not null;
            var inDark = app.TryGetResource(key, ThemeVariant.Dark, out var dark) && dark is not null;

            if (!inLight || !inDark)
                dead.Add($"{key} (missing in {(!inLight && !inDark ? "BOTH variants" : !inLight ? "Light" : "Dark")})");
        }

        Assert.True(
            dead.Count == 0,
            $"{logicalName} consumes {dead.Count} resource key(s) that do not resolve:" + Environment.NewLine
            + string.Join(Environment.NewLine, dead.Select(d => "  • " + d)) + Environment.NewLine + Environment.NewLine
            + "An unresolvable DynamicResource is SILENT: it does not throw, it does not fail the build, and the "
            + "property quietly keeps its default value. The control looks subtly wrong and nothing reports it."
            + Environment.NewLine + Environment.NewLine
            + "If a key vanished in an Avalonia upgrade, do not guess its replacement — probe for it. A throwaway "
            + "[AvaloniaFact] calling Application.Current.TryGetResource over your candidates answers it in under a "
            + "minute. That is how AccentButtonForeground and TextControlPlaceholderForeground were found when "
            + "Avalonia 12 dropped the WinUI FillColor family.");
    }
}
