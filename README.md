# Kuwantima

A glass-glow design system for [Avalonia UI](https://avaloniaui.net/).
One `StyleInclude`, 15 styled controls, automatic light/dark theming.

<!-- TODO: Add screenshot of sandbox (light + dark side-by-side) -->

## Features

- **Glass morphism** aesthetic with glow borders, frosted panels, and themed shadows
- **Light & Dark** themes that swap automatically — MidnightBlue ink on light, AliceBlue frost on dark
- **Fluent-integrated** — extends Avalonia's FluentTheme palette so styled and unstyled controls stay harmonious
- **Class-based** — opt in per control with `Classes="Kuwantima"`, no global override

## Quick Start

Install via NuGet:

```
dotnet add package Kuwantima
```

Then in your `App.axaml`:

```xml
<Application.Styles>
    <StyleInclude Source="avares://Kuwantima/Theme/KuwantimaPrimaryTheme.axaml"/>
</Application.Styles>
```

That's it. Every Fluent control picks up the Kuwantima color palette. To apply full Kuwantima styling to individual controls, add the class:

```xml
<Button Classes="Kuwantima" Content="Click me"/>
<CheckBox Classes="Kuwantima" Content="Accept terms"/>
<TextBox Classes="Kuwantima" PlaceholderText="Search..."/>
```

## Controls

| Control | Class | Variants |
|---------|-------|----------|
| Button | `Kuwantima` | `Accent` |
| CheckBox | `Kuwantima` | `Classic` |
| ComboBox | `Kuwantima` | |
| Expander | `Kuwantima` | |
| Border (Glass) | `KuwantimaGlass` | |
| GridSplitter | `Kuwantima` | `Pill`, `Arrow` (+ `Horizontal`/`Vertical`) |
| ListBox | `Kuwantima` | |
| MenuToggleButton | `KuwantimaMenu` | |
| ProgressBar | `Kuwantima` | |
| RadioButton | `Kuwantima` | `Classic` |
| Slider | `Kuwantima` | |
| TabControl | `Kuwantima` | |
| TextBox | `Kuwantima` | `ReadOnly` |
| ToggleButton | `Kuwantima` | |
| ToolTip | `Kuwantima` | |

### Variant Examples

```xml
<!-- Classic checkbox: traditional square-on-left layout -->
<CheckBox Classes="Kuwantima Classic" Content="Remember me"/>

<!-- Accent button: filled accent background -->
<Button Classes="Kuwantima Accent" Content="Save"/>

<!-- Pill splitter: floating handle between glass panels -->
<GridSplitter Classes="Kuwantima Pill"/>

<!-- Arrow splitter: rail + chevron inside a bordered container -->
<GridSplitter Classes="Kuwantima Arrow"/>
```

## Theming

Kuwantima's color story is built on three layers:

| Layer | Light | Dark | Role |
|-------|-------|------|------|
| Cool anchor | MidnightBlue `#191970` | AliceBlue `#F0F8FF` | Tints all Fluent tokens (text, chrome, backgrounds) |
| Warm accent | Orange `#FF8C00` | Orange `#FFA500` | Checked/selected borders — contrasts against cool blue |
| System accent | Fluent Blue `#0078D4` | Fluent Blue `#0078D4` | Filled accent backgrounds (buttons, selections) |

### Overriding Brushes

Custom brushes are defined in `KuwantimaThemeResources.axaml` inside `ThemeDictionaries`. To override, redefine the key in your own resource dictionary after the `StyleInclude`:

```xml
<Application.Styles>
    <StyleInclude Source="avares://Kuwantima/Theme/KuwantimaPrimaryTheme.axaml"/>
</Application.Styles>

<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <ResourceDictionary x:Key="Dark">
                <SolidColorBrush x:Key="KuwantimaGlassGlowBorder" Color="Purple" Opacity="0.5"/>
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Available Theme Resources

| Key | Purpose |
|-----|---------|
| `KuwantimaGlassBackground` | Glass panel fill |
| `KuwantimaGlassGlowBorder` | Glass control border at rest |
| `KuwantimaGlassGlowBorderHover` | Glass control border on hover |
| `KuwantimaGlassGlow` | Outer glow shadow at rest (BoxShadows) |
| `KuwantimaGlassGlowHover` | Outer glow shadow on hover (BoxShadows) |
| `KuwantimaControlHoverBrush` | Background tint on pointer-over |
| `KuwantimaAccentGlowPressed` | Warm outer glow on accent press (BoxShadows) |
| `KuwantimaAccentOrangeBrush` | Warm border for checked/selected state |
| `KuwantimaDarkBorderBrush` | Subtle separator (dark theme only) |
| `KuwantimaSuccessTextBrush` | Positive-outcome label text |
| `KuwantimaWarningTextBrush` | Negative-outcome label text |
| `KuwantimaTooltipBackground` | Tooltip backdrop |
| `KuwantimaTooltipForeground` | Tooltip text color |
| `KuwantimaSplitterBrush` | GridSplitter line at rest |
| `KuwantimaSplitterHoverBrush` | GridSplitter line on hover |
| `KuwantimaScrimBackground` | Wash behind a blocking overlay (a probe, save, or other long-running operation) |
| `KuwantimaScrimForeground` | Text/ink on the scrim |
| `SystemFillColorSuccessBrush` | Green status indicator |
| `SystemFillColorAttentionBrush` | Blue status indicator |
| `SystemFillColorCautionBrush` | Yellow status indicator |

### Fluent keys Kuwantima overrides

These are Avalonia Fluent's own keys, re-pointed so text stays readable on Kuwantima's surfaces.
Override them yourself only if you also re-check contrast against the backgrounds they land on.

| Key | Kuwantima value | Why |
|-----|-----------------|-----|
| `AccentButtonForeground` | White | Ink on accent fills, checkmarks and radio dots. The accent ramp darkens on interaction so one light ink clears WCAG AA on every state (4.53 / 7.32 / 10.50). |
| `SystemControlForegroundBaseMediumBrush` | `#55557F` / `#C8D4E8` | Fluent's value failed AA on the glass panel and on hovered controls. |
| `TextControlPlaceholderForeground` | `#55557F` / `#C8D4E8` | Same tone. A hovered empty TextBox puts placeholder text on the hover tint, which Fluent's value did not survive. |

## Icons

The same `StyleInclude` brings in 13 icon geometries. They are ordinary `StreamGeometry` resources,
so any control that takes a `Geometry` can use one:

```xml
<PathIcon Data="{StaticResource Icon.Home}" Width="18" Height="18"/>
```

| Key | Key | Key |
|---|---|---|
| `Icon.Home` | `Icon.Search` | `Icon.Refresh` |
| `Icon.Gear` | `Icon.Clear.Circle` | `Icon.Copy` |
| `Icon.Sliders` | `Icon.Layers` | `Icon.Sun` |
| `Icon.Expand` | `Icon.Map` | `Icon.Moon.ThirdEye.Smiling` |
| `Icon.Collapse` | | |

A key that does not exist renders **nothing** rather than failing loudly, so check a blank icon
against this table before looking anywhere else.

## Sidebar navigation

The sandbox's collapsible sidebar is built from shipped styles — there is no navigation control to
install. A `ToggleButton` with `Classes="KuwantimaMenu"` is the nav item:

```xml
<ToggleButton Classes="KuwantimaMenu"
              Classes.Expanded="{Binding IsPaneOpen}"
              Tag="{StaticResource Icon.Home}"
              Content="Home"
              IsChecked="{Binding IsHomeSelected}"/>
```

Two properties drive it, and it is worth being precise about which:

- **`Tag` is the icon.** The template binds it to the button's `PathIcon`, so any `StreamGeometry`
  works — one of the keys above, or your own.
- **`Content` is the label**, and it is hidden unless the `Expanded` class is on. That is what makes
  the button collapse to an icon-only square when the pane closes.

So do **not** put your own icon-plus-label `StackPanel` in `Content`. The template already places
both, and a panel there is invisible while collapsed and double-indented while expanded.

`Classes.Expanded` is what follows the pane's state, and checked state gets the accent fill and the
warm orange border automatically, so the selected page reads at a glance:

```xml
<SplitView DisplayMode="CompactInline"
           CompactPaneLength="56"
           OpenPaneLength="220"
           IsPaneOpen="{Binding IsPaneOpen}">
    <SplitView.Pane>
        <StackPanel Spacing="6" Margin="8">
            <!-- one ToggleButton per page -->
        </StackPanel>
    </SplitView.Pane>

    <!-- your page content -->
</SplitView>
```

### Wiring it to pages

The styles do not care how you choose pages, so this part is yours. But the shape below is worth
copying, because the obvious alternative fails silently — see the note at the end.

Keep **one** list of pages, and let it drive the sidebar and the content area both. Each entry
carries its label, its icon key, and a factory for the page itself:

```csharp
using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

public sealed partial class NavPage : ObservableObject
{
    private readonly Func<Control> _build;
    private Control? _view;

    public NavPage(string title, string iconKey, Func<Control> build)
        => (Title, IconKey, _build) = (title, iconKey, build);

    public string Title { get; }

    /// <summary>Key of a geometry from the table above, e.g. "Icon.Home".</summary>
    public string IconKey { get; }

    /// <summary>Built on first visit, then kept for the lifetime of the app.</summary>
    public Control View => _view ??= _build();

    /// <summary>Drives the nav button's checked state.</summary>
    [ObservableProperty] private bool _isSelected;
}
```

`IsSelected` **must** raise `PropertyChanged` — hence `ObservableObject` and `[ObservableProperty]`
above. A plain `public bool IsSelected { get; set; }` compiles, runs, and leaves the highlight stuck
on the first page while the content area changes underneath it, with no exception and no binding
error to go looking for.

The view model holds the list and the selection, and keeps `IsSelected` in step in one place:

```csharp
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<NavPage> Pages { get; } = new()
    {
        new NavPage("Home",     "Icon.Home", () => new HomePage()),
        new NavPage("Settings", "Icon.Gear", () => new SettingsPage()),
    };

    [ObservableProperty] private NavPage _selectedPage = null!;
    [ObservableProperty] private bool _isPaneOpen = true;

    public MainViewModel() => SelectedPage = Pages[0];

    partial void OnSelectedPageChanged(NavPage? oldValue, NavPage newValue)
    {
        if (oldValue is not null) oldValue.IsSelected = false;
        newValue.IsSelected = true;
    }

    [RelayCommand]
    private void NavigateTo(NavPage page) => SelectedPage = page;
}
```

Because the icon is a *key* rather than a geometry, one small converter turns it into the real
resource at bind time. A `DataTemplate` cannot write `{StaticResource {Binding IconKey}}` — a
resource key has to be known when the markup is parsed — so the lookup happens here, which also
keeps `Geometry` out of your view model:

```csharp
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

public class ResourceKeyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key || Application.Current is not { } app)
            return null;

        return app.TryGetResource(key, app.ActualThemeVariant, out var resource) ? resource : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException($"{nameof(ResourceKeyConverter)} is one-way.");
}
```

Pass the theme variant explicitly, as above. `TryGetResource` without one misses theme-scoped
resources, and returning `null` on an unknown key leaves a nav button iconless rather than taking
the window down.

The sidebar then becomes one `ItemsControl` over the list, and the content one `ContentControl`:

```xml
<Window.Resources>
    <conv:ResourceKeyConverter x:Key="ResourceKey"/>
</Window.Resources>

<SplitView.Pane>
    <ItemsControl ItemsSource="{Binding Pages}">
        <ItemsControl.ItemsPanel>
            <ItemsPanelTemplate>
                <StackPanel Spacing="6" Margin="8"/>
            </ItemsPanelTemplate>
        </ItemsControl.ItemsPanel>
        <ItemsControl.ItemTemplate>
            <DataTemplate x:DataType="vm:NavPage">
                <ToggleButton Classes="KuwantimaMenu"
                              Classes.Expanded="{Binding $parent[Window].((vm:MainViewModel)DataContext).IsPaneOpen}"
                              Tag="{Binding IconKey, Converter={StaticResource ResourceKey}}"
                              Content="{Binding Title}"
                              IsChecked="{Binding IsSelected, Mode=OneWay}"
                              Command="{Binding $parent[Window].((vm:MainViewModel)DataContext).NavigateToCommand}"
                              CommandParameter="{Binding}"/>
            </DataTemplate>
        </ItemsControl.ItemTemplate>
    </ItemsControl>
</SplitView.Pane>

<ContentControl Content="{Binding SelectedPage.View}"/>
```

Adding a page is now one line in `Pages`. The trade-off: navigating away detaches a page from the
visual tree, so transient control state such as scroll position resets when you come back. The page
object itself is kept, so anything held in your view model persists.

### If your app uses a Dependency Injection Container

`NavPage` takes a factory — `Func<Control>` — rather than a finished page, which is what makes pages
build lazily. That factory is also the seam for a **Dependency Injection Container**: a library, such
as `Microsoft.Extensions.DependencyInjection`, that constructs your objects for you and supplies
whatever those objects need. If you use one, resolve the page there instead of calling `new`:

```csharp
new NavPage("Settings", "Icon.Gear", () => provider.GetRequiredService<SettingsPage>()),
```

Nothing else changes. If you are not using a container, the `() => new SettingsPage()` above is
complete and correct — this is an extension point, not a requirement.

### Why one list

The tempting alternative is a nav button per page in the markup, plus an `int SelectedPageIndex` and
one `bool IsThisPageVisible` property per page. Kuwantima's own sandbox was written that way, and it
had two failure modes that produce **no exception and no binding error**:

- A per-page notification list that must be kept in step by hand. Miss an entry and the nav button
  highlights correctly while the page never appears.
- `CommandParameter="3"` as a magic number that has to agree with an index in the view model. Off by
  one and you silently get the wrong page.

With a single list there is nothing left to keep in sync. The one notification that still matters —
`NavPage.IsSelected` — is raised for you by `[ObservableProperty]` and set in exactly one place,
`OnSelectedPageChanged`. The `Kuwantima.Sandbox` project is a complete worked example, and the code
above is the code it runs.

## Sandbox

The `Kuwantima.Sandbox` project is a live gallery of every control and variant. Run it to preview the full design system:

```
dotnet run --project Kuwantima.Sandbox
```

Seven demo pages: **Buttons**, **Inputs**, **Toggles**, **Feedback**, **Containers**, **Theme Preview** (side-by-side light/dark), and **Documents** (styled README + license dialog).

## Requirements

- .NET 10.0
- Avalonia 12.0+
- Avalonia.Themes.Fluent 12.0+

## License

MIT License. See [LICENSE](LICENSE) for details.
