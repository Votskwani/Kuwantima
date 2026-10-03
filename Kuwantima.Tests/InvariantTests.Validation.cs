using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Kuwantima.Tests;

/// <summary>
/// Validation error — the :error pseudo-class contract on TextBox.Kuwantima and ComboBox.Kuwantima.
///
/// A throwaway probe this session proved Fluent (which KuwantimaPrimaryTheme.axaml builds on)
/// supplies ZERO baseline treatment for :error on either control — Background, BorderBrush and
/// Foreground were byte-identical rest vs. forced :error, in both variants. So unlike every other
/// pseudo-class this library styles, nothing in Fluent will ever paper over a mistake here: if the
/// KuwantimaValidationErrorBrush selector is ever deleted, or placed on the wrong side of
/// :disabled/:focus in document order (CLAUDE.md's "Setters resolve by DOCUMENT ORDER, not
/// selector specificity"), the control will silently go back to looking perfectly fine while
/// invalid. This test exists so that regression fails loudly instead.
/// </summary>
public partial class InvariantTests
{
    private static void ShowUnderVariant(Control control, ThemeVariant variant)
    {
        var window = new Window
        {
            RequestedThemeVariant = variant,
            Content = control,
            Width = 400,
            Height = 300,
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void Invariant_8_error_pseudo_class_repaints_TextBox_border(string variantName)
    {
        var variant = variantName == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        var control = new TextBox();
        control.Classes.Add("Kuwantima");
        ShowUnderVariant(control, variant);

        var borderElement = control.GetVisualDescendants()
            .OfType<Border>()
            .First(b => b.Name == "PART_BorderElement");

        borderElement.Transitions = null;
        var restBorderBrush = borderElement.BorderBrush;

        ((IPseudoClasses)control.Classes).Add(":error");
        Dispatcher.UIThread.RunJobs();
        borderElement.Transitions = null;
        var errorBorderBrush = borderElement.BorderBrush;

        var expected = ResolveColor("KuwantimaValidationErrorBrush", variant);

        Assert.True(
            !BrushEquals(restBorderBrush, errorBorderBrush),
            $"{variantName}: TextBox.Kuwantima's BorderBrush did not change when :error was forced. "
            + "Fluent supplies no fallback here — if this regresses, an invalid field will render "
            + "indistinguishably from a valid one. Check the ^:error selector in KuwantimaTextBox.axaml "
            + "still sits after ^:focus and before ^:disabled.");

        Assert.True(
            BrushColorEquals(errorBorderBrush, expected.Color),
            $"{variantName}: TextBox.Kuwantima's :error BorderBrush resolved to {Describe(errorBorderBrush)}, "
            + $"not KuwantimaValidationErrorBrush ({expected.Color}).");
    }

    [AvaloniaTheory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void Invariant_8_error_pseudo_class_repaints_ComboBox_border(string variantName)
    {
        var variant = variantName == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        var control = new ComboBox();
        control.Classes.Add("Kuwantima");
        ShowUnderVariant(control, variant);

        var borderElement = control.GetVisualDescendants()
            .OfType<Border>()
            .First(b => b.Name == "Background");

        borderElement.Transitions = null;
        var restBorderBrush = borderElement.BorderBrush;

        ((IPseudoClasses)control.Classes).Add(":error");
        Dispatcher.UIThread.RunJobs();
        borderElement.Transitions = null;
        var errorBorderBrush = borderElement.BorderBrush;

        var expected = ResolveColor("KuwantimaValidationErrorBrush", variant);

        Assert.True(
            !BrushEquals(restBorderBrush, errorBorderBrush),
            $"{variantName}: ComboBox.Kuwantima's BorderBrush did not change when :error was forced. "
            + "Fluent supplies no fallback here — if this regresses, an invalid field will render "
            + "indistinguishably from a valid one. Check the ^:error selector in KuwantimaComboBox.axaml "
            + "still sits after ^:focus-visible and before ^:disabled.");

        Assert.True(
            BrushColorEquals(errorBorderBrush, expected.Color),
            $"{variantName}: ComboBox.Kuwantima's :error BorderBrush resolved to {Describe(errorBorderBrush)}, "
            + $"not KuwantimaValidationErrorBrush ({expected.Color}).");
    }

    private static string Describe(IBrush? brush) =>
        brush is ISolidColorBrush solid ? solid.Color.ToString() : brush?.GetType().Name ?? "null";

    private static bool BrushEquals(IBrush? a, IBrush? b) =>
        (a, b) switch
        {
            (null, null) => true,
            (ISolidColorBrush x, ISolidColorBrush y) => x.Color == y.Color && x.Opacity == y.Opacity,
            _ => ReferenceEquals(a, b),
        };

    private static bool BrushColorEquals(IBrush? brush, Color color) =>
        brush is ISolidColorBrush solid && solid.Color == color;
}
