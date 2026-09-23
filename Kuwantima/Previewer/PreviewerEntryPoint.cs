using Avalonia;

namespace Kuwantima
{
    /// <summary>
    /// DEBUG-ONLY. This file is the reason the IDE previewer works on the style files in this
    /// project, and it is excluded from Release builds and from the NuGet package entirely — see
    /// the Compile Remove in Kuwantima.csproj. The shipped library still contains zero C#.
    ///
    /// Avalonia's previewer does not read a .axaml file. It runs the assembly that CONTAINS the
    /// compiled XAML — literally `dotnet exec … Avalonia.Designer.HostApp.dll <assembly>.dll` —
    /// and asks it for an AppBuilder. A class library has no entry point, so that exec fails with
    /// "Assembly … doesn't have an entry point" and the IDE reports *no previewer available* for
    /// every file in the project. That is not a misconfiguration; it is what a library is.
    ///
    /// So this gives the library the two things the designer host requires — an entry point and a
    /// BuildAvaloniaApp — without making it an application anyone runs. Main is deliberately empty:
    /// nothing should happen if it is ever launched.
    /// </summary>
    internal static class PreviewerEntryPoint
    {
        public static void Main(string[] args)
        {
            // Intentionally empty. Kuwantima is a style library; this entry point exists only so
            // the designer host has something to exec.
        }

        /// <summary>
        /// Found by convention by Avalonia.Designer.HostApp.
        /// </summary>
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<PreviewerApp>()
                .UsePlatformDetect();
    }

    /// <summary>
    /// Deliberately EMPTY — it must not load KuwantimaPrimaryTheme.
    ///
    /// The theme carries the shipped copy of every control style. If this app loaded it, previewing
    /// a style file would show you the released style layered over the one you are editing, so your
    /// change would appear to do nothing. Each style file instead supplies its own FluentTheme and
    /// theme resources inside Design.PreviewWith (enforced by Invariant_7), which is what makes a
    /// bare application the correct host here rather than a deficient one.
    /// </summary>
    internal sealed class PreviewerApp : Application
    {
    }
}
