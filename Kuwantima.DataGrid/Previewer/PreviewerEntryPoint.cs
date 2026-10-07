using Avalonia;

namespace Kuwantima.DataGrid
{
    /// <summary>
    /// DEBUG-ONLY. Exact mirror of Kuwantima/Previewer/PreviewerEntryPoint.cs — see that file for
    /// the full rationale. Excluded from Release builds and from the NuGet package entirely.
    /// </summary>
    internal static class PreviewerEntryPoint
    {
        public static void Main(string[] args)
        {
            // Intentionally empty. Kuwantima.DataGrid is a style library; this entry point exists
            // only so the designer host has something to exec.
        }

        /// <summary>
        /// Found by convention by Avalonia.Designer.HostApp.
        /// </summary>
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<PreviewerApp>()
                .UsePlatformDetect();
    }

    /// <summary>
    /// Deliberately EMPTY — it must not load KuwantimaDataGridTheme. Each style file instead
    /// supplies its own FluentTheme, the DataGrid base theme, and the theme resources it needs
    /// inside Design.PreviewWith, so a bare application is the correct host here.
    /// </summary>
    internal sealed class PreviewerApp : Application
    {
    }
}
