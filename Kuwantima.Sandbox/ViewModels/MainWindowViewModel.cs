using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
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

        [ObservableProperty]
        private bool _isScrimVisible;

        [RelayCommand]
        private void ToggleScrim() => IsScrimVisible = !IsScrimVisible;

        [ObservableProperty]
        private bool _isValidationDemoActive;

        /// <summary>
        /// Drives the real Avalonia.Controls.DataValidationErrors.Errors attached property on the
        /// Inputs page's demo TextBox/ComboBox — the actual mechanism BBService's ObservableValidator
        /// forms will use, not a pseudo-class forced directly. Non-null/non-empty is what fires the
        /// native :error pseudo-class.
        /// </summary>
        public IEnumerable<string>? ValidationDemoErrors =>
            IsValidationDemoActive ? new[] { "This field is required." } : null;

        partial void OnIsValidationDemoActiveChanged(bool value) =>
            OnPropertyChanged(nameof(ValidationDemoErrors));

        [RelayCommand]
        private void ToggleValidationDemo() => IsValidationDemoActive = !IsValidationDemoActive;

        [ObservableProperty]
        private AccentColorOption _selectedAccentColor = AccentColorOption.Blue;

        [RelayCommand]
        private void SetAccentColor(AccentColorOption option) => SelectedAccentColor = option;

        /// <summary>
        /// Re-tints the live Fluent accent ramp. Looks the color up from Kuwantima's own
        /// KuwantimaPalette{X} theme resource rather than hardcoding hex here, so this always
        /// matches whatever the library ships — no second copy of the palette to drift out of sync.
        /// </summary>
        partial void OnSelectedAccentColorChanged(AccentColorOption value)
        {
            if (Application.Current is not { } app)
                return;

            var key = $"KuwantimaPalette{value}";
            if (!app.TryFindResource(key, app.ActualThemeVariant, out var resource))
                return;
            if (resource is not ISolidColorBrush brush)
                return;

            if (FindFluentTheme(app.Styles) is not { } fluentTheme)
                return;

            fluentTheme.Palettes[ThemeVariant.Light].Accent = brush.Color;
            fluentTheme.Palettes[ThemeVariant.Dark].Accent = brush.Color;
        }

        /// <summary>
        /// KuwantimaPrimaryTheme.axaml embeds its own &lt;fluent:FluentTheme&gt; (with Kuwantima's
        /// Light/Dark ColorPaletteResources already attached) as one item inside the Styles object
        /// that App.axaml's single StyleInclude resolves to — so it has to be found by walking the
        /// style tree rather than indexed directly.
        /// </summary>
        private static FluentTheme? FindFluentTheme(IEnumerable<IStyle> styles)
        {
            foreach (var style in styles)
            {
                if (style is FluentTheme fluent)
                    return fluent;

                if (style is Styles nested && FindFluentTheme(nested) is { } found)
                    return found;
            }

            return null;
        }
    }
}
