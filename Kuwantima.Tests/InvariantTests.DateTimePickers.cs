using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Kuwantima.Tests;

/// <summary>
/// DatePicker.Kuwantima and TimePicker.Kuwantima — the two controls in this library whose
/// styled surface is mostly NOT the control you place in the form. Each is a trigger button plus
/// a presenter (DatePickerPresenter / TimePickerPresenter) that only ever exists inside a Popup,
/// and the whole flyout is restyled from the picker's own style file by a descendant selector
/// that reaches through that Popup.
///
/// Nothing else in the suite exercises that reach. Invariant_5 proves the keys resolve;
/// Invariant_7 proves the preview can find them; neither opens the popup. If the presenter
/// style stopped applying — a selector typo, a renamed template part, or Avalonia changing how
/// Popup content is parented — the trigger would still look right in every static check and
/// the consumer would get stock Fluent the moment they clicked. These tests click.
/// </summary>
public partial class InvariantTests
{
    private static TemplatedControl NewPicker(string controlName) =>
        controlName switch
        {
            "DatePicker" => new DatePicker(),
            "TimePicker" => new TimePicker(),
            _ => throw new ArgumentOutOfRangeException(nameof(controlName), controlName, null),
        };

    public static IEnumerable<object[]> PickerMatrix() =>
        from control in new[] { "DatePicker", "TimePicker" }
        from variant in new[] { "Light", "Dark" }
        select new object[] { control, variant };

    /// <summary>
    /// The :error contract, as for TextBox and ComboBox (InvariantTests.Validation.cs). The
    /// styled border is the inner flyout Button's — that is what the ^:error rule sets — so it is
    /// read from there. The Button's own BorderBrush carries no transition (the transition sits on
    /// its presenter), so it can be read directly.
    /// </summary>
    [AvaloniaTheory]
    [MemberData(nameof(PickerMatrix))]
    public void Invariant_8_error_pseudo_class_repaints_picker_border(string controlName, string variantName)
    {
        var variant = variantName == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        var control = NewPicker(controlName);
        control.Classes.Add("Kuwantima");
        ShowUnderVariant(control, variant);

        var button = control.GetVisualDescendants()
            .OfType<Button>()
            .First(b => b.Name == "PART_FlyoutButton");

        var restBorderBrush = button.BorderBrush;

        ((IPseudoClasses)control.Classes).Add(":error");
        Dispatcher.UIThread.RunJobs();
        var errorBorderBrush = button.BorderBrush;

        var expected = ResolveColor("KuwantimaValidationErrorBrush", variant);

        Assert.True(
            !BrushEquals(restBorderBrush, errorBorderBrush),
            $"{variantName}: {controlName}.Kuwantima's flyout button BorderBrush did not change when :error "
            + "was forced. Fluent's own :error treatment lives in the template this style replaces, so there "
            + $"is no fallback — check the ^:error selector in Kuwantima{controlName}.axaml still sits after "
            + "^:focus-visible and before ^:disabled.");

        Assert.True(
            BrushColorEquals(errorBorderBrush, expected.Color),
            $"{variantName}: {controlName}.Kuwantima's :error BorderBrush resolved to {Describe(errorBorderBrush)}, "
            + $"not KuwantimaValidationErrorBrush ({expected.Color}).");
    }

    /// <summary>
    /// Opens the flyout the way a user does — by clicking PART_FlyoutButton, which is the one event
    /// the picker listens to — and checks that what appears in the Popup is the Kuwantima presenter,
    /// not Fluent's: the replaced template is present, its surface is the opaque region brush, the
    /// highlight band is the accent chip, and the item under the band carries the accent ink.
    /// </summary>
    [AvaloniaTheory]
    [MemberData(nameof(PickerMatrix))]
    public void Invariant_9_picker_flyout_is_restyled_through_its_popup(string controlName, string variantName)
    {
        var variant = variantName == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        var control = NewPicker(controlName);
        control.Classes.Add("Kuwantima");
        ShowUnderVariant(control, variant);

        var button = control.GetVisualDescendants()
            .OfType<Button>()
            .First(b => b.Name == "PART_FlyoutButton");
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();

        var popup = control.GetVisualDescendants().OfType<Popup>().First(p => p.Name == "PART_Popup");
        Assert.True(popup.IsOpen, $"{controlName}: clicking PART_FlyoutButton did not open PART_Popup.");

        var presenter = Assert.IsAssignableFrom<TemplatedControl>(popup.Child);
        Dispatcher.UIThread.RunJobs();

        var popupBorder = presenter.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "PopupBorder");
        Assert.True(
            popupBorder is not null,
            $"{variantName}: {controlName}.Kuwantima opened a flyout whose presenter is NOT templated by "
            + $"Kuwantima{controlName}.axaml (no Border#PopupBorder). The '{controlName}.Kuwantima "
            + $"{presenter.GetType().Name}' descendant selector no longer reaches the presenter through the "
            + "Popup, so consumers get stock Fluent the moment they click.");

        var region = ResolveColor("SystemRegionBrush", variant);
        Assert.True(
            BrushColorEquals(popupBorder!.Background, region.Color),
            $"{variantName}: the flyout surface is {Describe(popupBorder.Background)}, not the opaque "
            + $"SystemRegionBrush ({region.Color}) every Kuwantima popup uses.");

        var band = presenter.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "HighlightBand");
        var accent = ResolveColor("SystemControlBackgroundAccentBrush", variant);
        Assert.True(
            band is not null && BrushColorEquals(band.Background, accent.Color),
            $"{variantName}: the highlight band is {(band is null ? "missing" : Describe(band.Background))}, "
            + $"expected the accent chip ({accent.Color}).");

        var selected = presenter.GetVisualDescendants().OfType<ListBoxItem>().Where(i => i.IsSelected).ToArray();
        Assert.True(
            selected.Length > 0,
            $"{controlName}: no ListBoxItem in the open flyout is :selected — the selectors did not populate, "
            + "so the item style cannot be checked.");

        var ink = ResolveColor("AccentButtonForeground", variant);
        foreach (var item in selected)
        {
            var presenterPart = item.GetVisualDescendants()
                .OfType<ContentPresenter>()
                .First(c => c.Name == "PART_ContentPresenter");

            Assert.True(
                BrushColorEquals(presenterPart.Foreground, ink.Color),
                $"{variantName}: the selected '{item.Content}' item under the band is inked "
                + $"{Describe(presenterPart.Foreground)}, not AccentButtonForeground ({ink.Color}). Dark text on "
                + "the accent band is the 2.33:1 pairing the v1.3.0 re-ramp removed everywhere else.");
        }
    }
}
