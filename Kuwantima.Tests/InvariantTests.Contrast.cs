using System.Xml.Linq;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;

namespace Kuwantima.Tests;

/// <summary>
/// Invariant 6 — the accent contrast contract.
///
/// Invariant_5 proves a key RESOLVES. It says nothing about whether the result is READABLE, and
/// that gap has cost this repo twice:
///
///   • v1.2.0 — migrating the accent foreground to Fluent's white AccentButtonForeground made the
///     default state crisp and silently dropped :pointerover to 1.53:1 and :pressed to 2.33:1.
///     Every key resolved. Invariant_5 was green the whole time.
///   • v1.3.0 — TabControl's selected tab and Expander's expanded header pin their ink to
///     SystemBaseHighColor, which INVERTS per theme variant, over SystemAccentColorLight3, which
///     does not. 9.70:1 in Light, 1.43:1 in Dark. Checking one variant CONFIRMED the bug.
///
/// Both are one failure: a foreground is authored once and must survive every surface it lands on,
/// under BOTH variants. So this measures the resolved pair per variant and never assumes one
/// variant's number carries to the other.
///
/// Scope is the accent family — the surfaces that pair an authored light ink with an accent fill.
/// It is deliberately NOT a whole-library contrast sweep: KuwantimaControlHoverBrush is a 50%
/// light wash in Dark that composites to mid-grey, leaving most hovers at ~3.3-3.8:1 there. That
/// is a real, recorded gap (see CLAUDE.md), but closing it means changing hover across every
/// control — a deliberate design change, not something to smuggle in behind a test. Widening this
/// invariant to cover it before that change lands would only produce an exemption written to keep
/// the suite green, which is the weakening CLAUDE.md warns against.
/// </summary>
public partial class InvariantTests
{
    private const double AA = 4.5;

    /// <summary>One (ink, surface) pair the library actually paints.</summary>
    private sealed record Pair(string Ink, string Surface, string Where);

    /// <summary>
    /// A translucent surface composited onto the backdrop it actually sits on. A translucent brush
    /// has NO contrast of its own — only the blend does — which is exactly why KuwantimaControlHoverBrush
    /// sat at 3.25:1 in Dark unnoticed: as a colour literal it looks perfectly reasonable.
    /// </summary>
    private sealed record Layered(string Ink, string Surface, string[] Backdrop, string Where);

    /// <summary>
    /// Hover surfaces. KuwantimaControlHoverBrush is translucent, so each entry names the stack
    /// beneath it, outermost first. Both backdrops are checked because the glass panel sits between
    /// the page and the control on most pages, and it is the worse of the two.
    /// </summary>
    private static readonly Layered[] HoverPairs =
    [
        new("SystemBaseHighColor", "KuwantimaControlHoverBrush", ["SystemRegionBrush"],
            "primary text on a hovered control, directly on the page"),
        new("SystemBaseHighColor", "KuwantimaControlHoverBrush", ["SystemRegionBrush", "KuwantimaGlassBackground"],
            "primary text on a hovered control inside a glass panel"),
        new("SystemControlForegroundBaseMediumBrush", "KuwantimaControlHoverBrush", ["SystemRegionBrush", "KuwantimaGlassBackground"],
            "muted subtitle text on a hovered control inside a glass panel"),
        new("TextControlPlaceholderForeground", "KuwantimaControlHoverBrush", ["SystemRegionBrush", "KuwantimaGlassBackground"],
            "TextBox placeholder — PART_Placeholder sits inside PART_BorderElement, whose "
            + "background becomes the hover brush, so hovering an empty TextBox produces exactly this"),
    ];

    /// <summary>
    /// The scrim (KuwantimaScrimBackground/Foreground, added v1.5.0): a blocking overlay, so its
    /// ink/surface pair is measured the same way as a hover surface — both backdrops, both variants
    /// — even though it is not part of the accent family this file otherwise scopes to.
    /// </summary>
    private static readonly Layered[] ScrimPairs =
    [
        new("KuwantimaScrimForeground", "KuwantimaScrimBackground", ["SystemRegionBrush"],
            "scrim text directly on the page"),
        new("KuwantimaScrimForeground", "KuwantimaScrimBackground", ["SystemRegionBrush", "KuwantimaGlassBackground"],
            "scrim text over a glass panel — the realistic case, per Tunatya's OnboardingView.axaml"),
    ];

    private static readonly Pair[] AccentPairs =
    [
        new("AccentButtonForeground", "SystemControlBackgroundAccentBrush",
            "rest / :checked / :selected — the accent chip on Button, CheckBox, RadioButton, "
            + "ToggleButton, MenuToggleButton, ComboBox and ListBox"),
        new("AccentButtonForeground", "SystemAccentColorDark1",
            ":pointerover on every accent chip, plus the Slider thumb"),
        new("AccentButtonForeground", "SystemAccentColorDark2",
            ":pressed on Button.Kuwantima.Accent"),
    ];

    /// <summary>
    /// Every accent background a style file may paint, mapped to the ink that lands on it. The
    /// companion test below fails if the styles paint one that is missing here, so the table
    /// cannot quietly drift out of step with the markup it describes.
    /// </summary>
    private static readonly Dictionary<string, string?> AccentSurfaceInk = new(StringComparer.Ordinal)
    {
        ["SystemControlBackgroundAccentBrush"] = "AccentButtonForeground",
        ["SystemAccentColorDark1"] = "AccentButtonForeground",
        ["SystemAccentColorDark2"] = "AccentButtonForeground",
    };

    private static double Luminance(Color c)
    {
        static double Channel(byte v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
    }

    private static double Contrast(Color fg, Color bg)
    {
        var (a, b) = (Luminance(fg), Luminance(bg));
        var (hi, lo) = a > b ? (a, b) : (b, a);
        return (hi + 0.05) / (lo + 0.05);
    }

    private static (Color Color, double Opacity) ResolveColor(string key, ThemeVariant variant)
    {
        Assert.True(
            Application.Current!.TryGetResource(key, variant, out var value) && value is not null,
            $"{key} does not resolve under {variant}. Invariant_5 should have caught that first.");

        return value switch
        {
            ISolidColorBrush b => (b.Color, b.Opacity),
            Color c => (c, 1.0),
            _ => throw new InvalidOperationException(
                $"{key} resolved to {value!.GetType().Name}, which is not a colour."),
        };
    }

    public static IEnumerable<object[]> AccentPairMatrix() =>
        from pair in AccentPairs
        from variant in new[] { "Light", "Dark" }
        select new object[] { pair.Ink, pair.Surface, pair.Where, variant };

    [AvaloniaTheory]
    [MemberData(nameof(AccentPairMatrix))]
    public void Invariant_6_accent_ink_clears_AA_on_every_accent_surface(
        string inkKey, string surfaceKey, string where, string variantName)
    {
        var variant = variantName == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        var ink = ResolveColor(inkKey, variant);
        var surface = ResolveColor(surfaceKey, variant);

        // A translucent surface renders as a blend of whatever is behind it, so measuring its own
        // colour would be measuring something the eye never sees. None of the accent fills are
        // translucent today; if one becomes so, this must composite rather than silently lie.
        Assert.True(
            surface.Color.A == 255 && surface.Opacity >= 1.0,
            $"{surfaceKey} is translucent under {variantName} (alpha {surface.Color.A}, opacity "
            + $"{surface.Opacity:0.##}). Its rendered colour depends on the backdrop, so it cannot be "
            + "measured on its own — composite it over the surface it actually sits on first.");

        var ratio = Contrast(ink.Color, surface.Color);

        Assert.True(
            ratio >= AA,
            $"{variantName}: {inkKey} ({ink.Color}) on {surfaceKey} ({surface.Color}) measures "
            + $"{ratio:F2}:1 — below WCAG AA of {AA:F1}." + Environment.NewLine
            + $"  Surface: {where}" + Environment.NewLine + Environment.NewLine
            + "A foreground is authored ONCE on a base style and has to survive every state background "
            + "it inherits down to. If you changed a state background, the ink has to still clear AA on "
            + "all of them — and under BOTH variants, which are not interchangeable: SystemBaseHighColor "
            + "inverts per variant while the SystemAccentColor* ramp is theme-invariant." + Environment.NewLine
            + "Do not resolve this by exempting the pair. Re-ramp the surface, or change the ink.");
    }

    /// <summary>Source-over composite of a possibly-translucent brush onto an opaque backdrop.</summary>
    private static Color Over((Color Color, double Opacity) over, Color under)
    {
        var a = (over.Color.A / 255.0) * over.Opacity;
        byte Mix(byte o, byte u) => (byte)Math.Round(o * a + u * (1 - a));
        return Color.FromRgb(Mix(over.Color.R, under.R), Mix(over.Color.G, under.G), Mix(over.Color.B, under.B));
    }

    public static IEnumerable<object[]> HoverPairMatrix() =>
        from pair in HoverPairs
        from variant in new[] { "Light", "Dark" }
        select new object[] { pair.Ink, pair.Surface, string.Join(" > ", pair.Backdrop), pair.Where, variant };

    [AvaloniaTheory]
    [MemberData(nameof(HoverPairMatrix))]
    public void Invariant_6_ink_clears_AA_on_composited_hover_surfaces(
        string inkKey, string surfaceKey, string backdropChain, string where, string variantName)
    {
        var variant = variantName == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        // Build the stack from the page outwards, exactly as the renderer does.
        Color composited = default;
        var first = true;
        foreach (var key in backdropChain.Split(" > "))
        {
            var layer = ResolveColor(key, variant);
            composited = first ? Over((layer.Color, layer.Opacity), Colors.Black) : Over(layer, composited);
            first = false;
        }

        var surface = ResolveColor(surfaceKey, variant);
        composited = Over(surface, composited);

        var ink = ResolveColor(inkKey, variant);
        var ratio = Contrast(ink.Color, composited);

        Assert.True(
            ratio >= AA,
            $"{variantName}: {inkKey} ({ink.Color}) on {surfaceKey} composited over [{backdropChain}] "
            + $"= {composited}, measuring {ratio:F2}:1 — below WCAG AA of {AA:F1}." + Environment.NewLine
            + $"  Surface: {where}" + Environment.NewLine + Environment.NewLine
            + "A translucent brush has no contrast of its own — only the blend does. Reading its hex "
            + "tells you nothing, which is how the Dark hover brush sat at 3.25:1 across every hovered "
            + "control in the library without anyone noticing. If you changed the hover brush's alpha "
            + "or a muted ink, measure the composite over BOTH backdrops: the glass panel is the worse "
            + "of the two and the one most controls actually sit on.");
    }

    public static IEnumerable<object[]> ScrimPairMatrix() =>
        from pair in ScrimPairs
        from variant in new[] { "Light", "Dark" }
        select new object[] { pair.Ink, pair.Surface, string.Join(" > ", pair.Backdrop), pair.Where, variant };

    [AvaloniaTheory]
    [MemberData(nameof(ScrimPairMatrix))]
    public void Invariant_6_scrim_ink_clears_AA_on_composited_backdrops(
        string inkKey, string surfaceKey, string backdropChain, string where, string variantName)
    {
        var variant = variantName == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        Color composited = default;
        var first = true;
        foreach (var key in backdropChain.Split(" > "))
        {
            var layer = ResolveColor(key, variant);
            composited = first ? Over((layer.Color, layer.Opacity), Colors.Black) : Over(layer, composited);
            first = false;
        }

        var surface = ResolveColor(surfaceKey, variant);
        composited = Over(surface, composited);

        var ink = ResolveColor(inkKey, variant);
        var ratio = Contrast(ink.Color, composited);

        Assert.True(
            ratio >= AA,
            $"{variantName}: {inkKey} ({ink.Color}) on {surfaceKey} composited over [{backdropChain}] "
            + $"= {composited}, measuring {ratio:F2}:1 — below WCAG AA of {AA:F1}." + Environment.NewLine
            + $"  Surface: {where}" + Environment.NewLine + Environment.NewLine
            + "The scrim's opacity was chosen to read as a solid blocking surface, not to chase the AA "
            + "floor — if this fails, the wash lost too much opacity or the ink drifted, not the other "
            + "way round.");
    }

    /// <summary>
    /// Closes the loop. A new accent background in any style file has to be added to
    /// AccentSurfaceInk, and is therefore measured. Without this the table is one more restatement
    /// that can drift away from the markup it claims to describe — which is precisely how the
    /// control count stayed wrong across four agreeing sources for two releases.
    /// </summary>
    [AvaloniaFact]
    public void Invariant_6_every_accent_surface_the_styles_paint_is_measured()
    {
        var painted = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var file in StyleFileNames())
        foreach (var rule in RulesOf(file))
        foreach (var setter in rule.Element.Elements(Av + "Setter"))
        {
            // Background and Fill are the surface-painting properties. Foreground/Stroke are ink,
            // and BorderBrush is an edge — neither sits behind text.
            if ((string?)setter.Attribute("Property") is not ("Background" or "Fill")) continue;

            foreach (var key in ReferencedKeys(new XDocument(new XElement("probe", setter))))
                if (key.StartsWith("SystemAccentColor", StringComparison.Ordinal)
                    || key == "SystemControlBackgroundAccentBrush")
                    painted.Add(key);
        }

        Assert.True(
            painted.Count > 0,
            "No accent surfaces were found in any style file. The parser is not reading Setters — "
            + "this test would pass vacuously.");

        var unmeasured = painted.Where(k => !AccentSurfaceInk.ContainsKey(k)).ToArray();

        Assert.True(
            unmeasured.Length == 0,
            "Accent background(s) painted by the styles but absent from AccentSurfaceInk: "
            + string.Join(", ", unmeasured) + Environment.NewLine + Environment.NewLine
            + "Add each one together with the ink that lands on it, so Invariant_6 measures the pair. "
            + "If genuinely nothing readable sits on it — a Slider thumb carries no text — record that "
            + "deliberately rather than leaving the surface unmeasured." + Environment.NewLine
            + Environment.NewLine
            + "SystemAccentColorLight3 turning up here is the specific regression this guards against: "
            + "it is a PALE fill, and the library's accent ink is White. That pairing measures 1.53:1, "
            + "and it is what the whole v1.3.0 re-ramp existed to remove.");
    }
}
