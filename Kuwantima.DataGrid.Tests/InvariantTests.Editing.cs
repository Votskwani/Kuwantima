using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Kuwantima.DataGrid.Tests;

/// <summary>
/// Cell-editing chrome — DataGridTextColumn's edit-mode TextBox (Name="CellTextBox") is a bare
/// control themed by the vendor's own DataGridCellTextBoxTheme, never Classes="Kuwantima" (see
/// KuwantimaDataGrid.axaml's own comment on the new DataGridCell:edited style). Mirrors
/// Kuwantima.Tests/InvariantTests.Validation.cs's :error pattern — force the state, null out
/// Transitions, read the resolved template-part brush, compare to the expected Kuwantima key —
/// adapted for a real edit session rather than a forced pseudo-class, since DataGridCell's
/// ":edited" is read-only, computed off OwningGrid.EditingRow/EditingColumnIndex. Binds to a
/// mutable row type with a real settable property rather than a bare string self-binding:
/// BeginEdit() returns false with no settable property to commit the edit to.
/// </summary>
public partial class InvariantTests
{
    private sealed class EditableRow
    {
        public string Name { get; set; } = "";
    }

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

    private static (Color Color, double Opacity) ResolveColor(string key, ThemeVariant variant)
    {
        Assert.True(
            Application.Current!.TryGetResource(key, variant, out var value) && value is not null,
            $"{key} does not resolve under {variant}.");

        return value switch
        {
            ISolidColorBrush b => (b.Color, b.Opacity),
            Color c => (c, 1.0),
            _ => throw new InvalidOperationException(
                $"{key} resolved to {value!.GetType().Name}, which is not a colour."),
        };
    }

    private static string Describe(IBrush? brush) =>
        brush is ISolidColorBrush solid ? solid.Color.ToString() : brush?.GetType().Name ?? "null";

    private static bool BrushColorEquals(IBrush? brush, Color color) =>
        brush is ISolidColorBrush solid && solid.Color == color;

    [AvaloniaTheory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void Invariant_9_edited_cell_textbox_picks_up_Kuwantima_accent_styling(string variantName)
    {
        var variant = variantName == "Light" ? ThemeVariant.Light : ThemeVariant.Dark;

        var grid = new Avalonia.Controls.DataGrid { AutoGenerateColumns = false };
        grid.Classes.Add("Kuwantima");
        grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new Binding("Name") });
        grid.ItemsSource = new AvaloniaList<EditableRow>
        {
            new() { Name = "Row One" },
            new() { Name = "Row Two" },
        };

        ShowUnderVariant(grid, variant);

        grid.SelectedIndex = 0;
        grid.CurrentColumn = grid.Columns[0];
        Dispatcher.UIThread.RunJobs();

        Assert.True(grid.BeginEdit(),
            "BeginEdit returned false — check the column's binding path is a real settable property, "
            + "and that SelectedIndex/CurrentColumn are both set before calling it.");
        Dispatcher.UIThread.RunJobs();

        var editBox = grid.GetVisualDescendants().OfType<TextBox>()
            .First(t => t.Name == "CellTextBox");
        var borderElement = editBox.GetVisualDescendants().OfType<Border>()
            .First(b => b.Name == "PART_BorderElement");
        borderElement.Transitions = null;

        var expected = ResolveColor("KuwantimaAccentOrangeBrush", variant);

        Assert.True(
            BrushColorEquals(borderElement.BorderBrush, expected.Color),
            $"{variantName}: editing TextBox's PART_BorderElement resolved to {Describe(borderElement.BorderBrush)}, "
            + $"not KuwantimaAccentOrangeBrush ({expected.Color}). Check the DataGrid.Kuwantima "
            + "DataGridCell:edited TextBox#CellTextBox style in KuwantimaDataGrid.axaml targets "
            + "/template/ Border#PART_BorderElement rather than the TextBox control's own properties — "
            + "Fluent's own :focus state (which the editing box is always in) sets BorderBrush directly "
            + "on that nested part, which otherwise silently wins.");
    }
}
