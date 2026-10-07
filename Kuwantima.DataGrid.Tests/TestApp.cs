using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Kuwantima.DataGrid.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Kuwantima.DataGrid.Tests;

/// <summary>
/// Boots the way a real consumer does: Kuwantima's own StyleInclude FIRST, then this package's —
/// same document order CLAUDE.md's DataGrid section requires (the base DataGrid Fluent theme,
/// then Kuwantima's resource overrides, must both be in scope before KuwantimaDataGrid.axaml's
/// Classes="Kuwantima" selectors can mean anything).
/// </summary>
public sealed class TestApp : Application
{
    public const string KuwantimaThemeUri = "avares://Kuwantima/Theme/KuwantimaPrimaryTheme.axaml";
    public const string DataGridThemeUri = "avares://Kuwantima.DataGrid/Theme/KuwantimaDataGridTheme.axaml";

    public override void Initialize()
    {
        // DO NOT DELETE THESE LINES — see Kuwantima.Tests/TestApp.cs for the full story. Neither
        // Kuwantima nor Kuwantima.DataGrid has a type this project references directly (DataGrid
        // itself comes from Avalonia.Controls.DataGrid, a third assembly), so nothing else forces
        // either one to load, and Avalonia 12.1.3's avares:// resolution needs them loaded first.
        _ = System.Reflection.Assembly.Load("Kuwantima");
        _ = System.Reflection.Assembly.Load("Kuwantima.DataGrid");

        var baseUri = new Uri("avares://Kuwantima.DataGrid.Tests/");
        Styles.Add(new StyleInclude(baseUri) { Source = new Uri(KuwantimaThemeUri) });
        Styles.Add(new StyleInclude(baseUri) { Source = new Uri(DataGridThemeUri) });
    }
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
