using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Kuwantima.DataGrid.Tests;

/// <summary>
/// Plain row selection (not editing) — DataGrid.Kuwantima DataGridRow:selected's own Foreground
/// Setter is what inverts cell text to white on the accent-blue fill; without it, selected rows
/// show dark base text at the same 2.33:1 failure v1.3.0 fixed everywhere else in the library.
/// This existed and was verified when the DataGrid package first shipped (v1.8.0) — then silently
/// deleted mid-release by an unrelated `sed` command elsewhere in this session that matched an
/// identical Setter line in a different style block and removed both. No existing test caught it;
/// it was found by eye, via mouse-click selection, after shipping. This closes that gap.
/// </summary>
public partial class InvariantTests
{
    [AvaloniaTheory]
    [InlineData("Light")]
    [InlineData("Dark")]
    public void Invariant_10_selected_row_text_inverts_to_accent_foreground(string variantName)
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

        // Realize the grid UNSELECTED first, then select afterward — the exact order a real user
        // hits (a fresh page has nothing selected; selection happens later from a click), as
        // opposed to selecting before the row's first realization.
        ShowUnderVariant(grid, variant);
        Dispatcher.UIThread.RunJobs();

        grid.SelectedIndex = 0;
        Dispatcher.UIThread.RunJobs();

        var textBlock = grid.GetVisualDescendants().OfType<TextBlock>()
            .First(t => t.Name == "CellTextBlock");

        var expected = ResolveColor("AccentButtonForeground", variant);

        Assert.True(
            BrushColorEquals(textBlock.Foreground, expected.Color),
            $"{variantName}: selected row's display TextBlock Foreground resolved to {Describe(textBlock.Foreground)}, "
            + $"not AccentButtonForeground ({expected.Color}). Check DataGrid.Kuwantima DataGridRow:selected "
            + "in KuwantimaDataGrid.axaml still sets Foreground.");
    }
}
