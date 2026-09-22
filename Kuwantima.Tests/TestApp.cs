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
        // DO NOT DELETE THIS LINE. It looks like dead code and it is not.
        //
        // Kuwantima contains zero C# — it is a resource-only assembly. Nothing in this project
        // references a type from it (there are none to reference), so the ProjectReference copies
        // Kuwantima.dll next to the test binary but never causes it to be LOADED. Avalonia 12.1.0
        // probed hard enough to find it anyway; Avalonia 12.1.3 does not, and every test in the
        // suite dies at session start with:
        //
        //     XamlLoadException : No precompiled XAML found for
        //     avares://Kuwantima/Theme/KuwantimaPrimaryTheme.axaml
        //
        // Forcing the load first makes the avares URI resolvable. The sandbox never needed this
        // because its App.axaml is compiled XAML that references the assembly at load time — which
        // is why this failure is confined to the headless harness and never reached a consumer.
        _ = System.Reflection.Assembly.Load("Kuwantima");

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
