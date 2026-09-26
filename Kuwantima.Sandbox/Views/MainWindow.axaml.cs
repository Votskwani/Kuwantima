using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Styling;
using Kuwantima.Sandbox.ViewModels;

namespace Kuwantima.Sandbox.Views
{
    public partial class MainWindow : Window
    {
        private readonly Rectangle _maximizeRestoreGlyph;

        public MainWindow()
        {
            InitializeComponent();

            if (DataContext is MainWindowViewModel vm)
                InitTheme(vm);

            DataContextChanged += (_, _) =>
            {
                if (DataContext is MainWindowViewModel viewModel)
                    InitTheme(viewModel);
            };

            // Custom chrome: WindowDecorations="None" removes the native minimize/maximize/close
            // buttons, so the maximize glyph and tooltip have to be kept in sync with WindowState
            // by hand — there is no "maximized" pseudo-class to style against, unlike :checked.
            _maximizeRestoreGlyph = (Rectangle)MaximizeRestoreButton.Content!;
            PropertyChanged += (_, e) =>
            {
                if (e.Property == WindowStateProperty)
                    UpdateMaximizeRestoreGlyph(WindowState);
            };
        }

        private void InitTheme(MainWindowViewModel vm)
        {
            var app = Application.Current;
            if (app is null) return;

            vm.IsDarkTheme = app.ActualThemeVariant == ThemeVariant.Dark;
        }

        private void OnMinimizeClick(object? sender, RoutedEventArgs e) =>
            WindowState = WindowState.Minimized;

        private void OnMaximizeRestoreClick(object? sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;

        private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

        private void UpdateMaximizeRestoreGlyph(WindowState state)
        {
            var isMaximized = state == WindowState.Maximized;
            ToolTip.SetTip(MaximizeRestoreButton, isMaximized ? "Restore" : "Maximize");

            // Same square glyph either way, on purpose: a two-state restore icon (overlapping
            // squares) needs a fill that matches whatever is behind the button at the time, which
            // drifts across hover/pressed states. The tooltip carries the state instead.
            _maximizeRestoreGlyph.StrokeThickness = isMaximized ? 1.6 : 1.3;
        }
    }
}
