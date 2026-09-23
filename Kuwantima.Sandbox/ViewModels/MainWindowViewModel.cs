using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kuwantima.Sandbox.Views.Pages;

namespace Kuwantima.Sandbox.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            SelectedPage = Pages[0];
        }

        public static string KuwantimaVersion =>
            typeof(Kuwantima.Sandbox.App).Assembly
                .GetReferencedAssemblies()
                .FirstOrDefault(a => a.Name == "Kuwantima")
                ?.Version?.ToString(3) ?? "0.0.0";

        /// <summary>
        /// The sandbox's pages, in sidebar order. This is the ONLY place a page is registered:
        /// the nav buttons and the content area are both bound to this list, so adding a page is
        /// one line here and nothing else. Keep README's Sandbox section in step with the count.
        /// </summary>
        public ObservableCollection<NavPage> Pages { get; } = new()
        {
            new NavPage("Buttons",       "Icon.Home",    () => new ButtonsPage()),
            new NavPage("Inputs",        "Icon.Search",  () => new InputsPage()),
            new NavPage("Toggles",       "Icon.Sliders", () => new TogglesPage()),
            new NavPage("Feedback",      "Icon.Layers",  () => new FeedbackPage()),
            new NavPage("Containers",    "Icon.Map",     () => new ContainersPage()),
            new NavPage("Theme Preview", "Icon.Sun",     () => new ThemePreviewPage()),
            new NavPage("Documents",     "Icon.Copy",    () => new DocumentsPage()),
        };

        [ObservableProperty]
        private NavPage _selectedPage = null!;

        partial void OnSelectedPageChanged(NavPage? oldValue, NavPage newValue)
        {
            if (oldValue is not null)
                oldValue.IsSelected = false;

            newValue.IsSelected = true;
        }

        [RelayCommand]
        private void NavigateTo(NavPage page) => SelectedPage = page;

        [ObservableProperty]
        private bool _isDarkTheme;

        partial void OnIsDarkThemeChanged(bool value)
        {
            var variant = value ? ThemeVariant.Dark : ThemeVariant.Light;

            if (Application.Current is { } app)
                app.RequestedThemeVariant = variant;
        }

        [ObservableProperty]
        private bool _isPaneOpen = true;

        [RelayCommand]
        private void TogglePane()
        {
            IsPaneOpen = !IsPaneOpen;
        }

        [ObservableProperty]
        private double _sliderValue = 60;

        [ObservableProperty]
        private double _progressValue = 45;

        [ObservableProperty]
        private bool _isToggled = true;

        [ObservableProperty]
        private bool _isChecked = true;
    }
}
