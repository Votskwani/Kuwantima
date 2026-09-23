using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kuwantima.Sandbox.ViewModels
{
    /// <summary>
    /// One entry in the sidebar, and the page it shows. A single list of these drives BOTH the
    /// navigation buttons and the content area, so adding a page is one line in
    /// <see cref="MainWindowViewModel.Pages"/> and nothing else.
    ///
    /// This replaces the old int SelectedPageIndex, which needed a CommandParameter magic number in
    /// the markup and a hand-maintained OnPropertyChanged list in the ViewModel to agree with it.
    /// Both could go wrong silently; neither exists any more.
    /// </summary>
    public partial class NavPage : ViewModelBase
    {
        private readonly Func<Control> _build;
        private Control? _view;

        public NavPage(string title, string iconKey, Func<Control> build)
        {
            Title = title;
            IconKey = iconKey;
            _build = build;
        }

        /// <summary>Label on the nav button.</summary>
        public string Title { get; }

        /// <summary>
        /// Key of a StreamGeometry in KuwantimaStreamIcons.axaml (e.g. "Icon.Home"). Resolved to
        /// the geometry by ResourceKeyConverter at bind time, so resource lookup stays in the view
        /// layer where it belongs.
        /// </summary>
        public string IconKey { get; }

        /// <summary>
        /// The page itself, built on first visit and then kept for the lifetime of the app.
        /// Lazy on purpose: the previewer instantiates this ViewModel through
        /// Design.DataContext, and building all seven pages just to render the shell would make
        /// the designer pay for pages it is not showing.
        /// </summary>
        public Control View => _view ??= _build();

        /// <summary>Drives the nav button's checked state. Set only by MainWindowViewModel.</summary>
        [ObservableProperty]
        private bool _isSelected;
    }
}
