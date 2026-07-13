using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using Kuwantima.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Kuwantima.Tests;

/// <summary>
/// Boots Kuwantima the way a real consumer does: one StyleInclude, nothing else.
/// If the theme cannot load, every test in the suite fails at session start —
/// which is the intended signal.
/// </summary>
public sealed class TestApp : Application
{
    public const string ThemeUri = "avares://Kuwantima/Theme/KuwantimaPrimaryTheme.axaml";

    public override void Initialize()
    {
        Styles.Add(new StyleInclude(new Uri("avares://Kuwantima.Tests/"))
        {
            Source = new Uri(ThemeUri)
        });
    }
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
