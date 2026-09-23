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

## Sidebar navigation

The sandbox's collapsible sidebar is built from shipped styles — there is no navigation control to
install. A `ToggleButton` with `Classes="KuwantimaMenu"` is the nav item, and the `Expanded` class
switches it between icon-only and icon-plus-label so it can follow a `SplitView`'s pane state:

```xml
<SplitView DisplayMode="CompactInline"
           CompactPaneLength="56"
           OpenPaneLength="220"
           IsPaneOpen="{Binding IsPaneOpen}">
    <SplitView.Pane>
        <StackPanel Spacing="6" Margin="8">
            <ToggleButton Classes="KuwantimaMenu"
                          Classes.Expanded="{Binding IsPaneOpen}"
                          IsChecked="{Binding IsHomeSelected}">
                <StackPanel Orientation="Horizontal" Spacing="12">
                    <PathIcon Data="{StaticResource HomeIcon}" Width="18" Height="18"/>
                    <TextBlock Text="Home" VerticalAlignment="Center"/>
                </StackPanel>
            </ToggleButton>
            <!-- one ToggleButton per page -->
        </StackPanel>
    </SplitView.Pane>

    <!-- your page content -->
</SplitView>
```

Checked state gets the accent fill and the warm orange border automatically, so the selected page
reads at a glance.

### Wiring it to pages

The styles do not care how you choose pages, so this part is yours. But the shape below is worth
copying, because the obvious alternative fails silently — see the note at the end.

Keep **one** list of pages, and let it drive the sidebar and the content area both:

```csharp
public sealed class NavPage
{
    private readonly Func<Control> _build;
    private Control? _view;

    public NavPage(string title, StreamGeometry icon, Func<Control> build)
        => (Title, Icon, _build) = (title, icon, build);

    public string Title { get; }
    public StreamGeometry Icon { get; }

    // Built on first visit, then kept for the lifetime of the app.
    public Control View => _view ??= _build();

    // Drives the nav button's checked state; raise PropertyChanged from your MVVM framework.
    public bool IsSelected { get; set; }
}
```

```csharp
public ObservableCollection<NavPage> Pages { get; } = new()
{
    new NavPage("Home",     HomeIcon,     () => new HomePage()),
    new NavPage("Settings", SettingsIcon, () => new SettingsPage()),
};

// When this changes, clear IsSelected on the old page and set it on the new one.
public NavPage SelectedPage { get; set; }
```

The sidebar becomes one `ItemsControl` over that list, and the content one `ContentControl`:

```xml
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
                              Tag="{Binding Icon}"
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

`Tag` is the icon: the `KuwantimaMenu` template binds it to the button's `PathIcon`, so any
`StreamGeometry` works. (Holding a `Geometry` on a view model bothers some people. If it bothers you,
store a resource key string instead and convert it with a small `IValueConverter` — a `DataTemplate`
cannot write `{StaticResource {Binding IconKey}}`, because a resource key must be known when the
markup is parsed.)

Adding a page is now one line in `Pages`. The trade-off: navigating away detaches a page from the
visual tree, so transient control state such as scroll position resets when you come back. The page
object itself is kept, so anything held in your view model persists.

### If your app uses a Dependency Injection Container

`NavPage` takes a factory — `Func<Control>` — rather than a finished page, which is what makes pages
build lazily. That factory is also the seam for a **Dependency Injection Container**: a library, such
as `Microsoft.Extensions.DependencyInjection`, that constructs your objects for you and supplies
whatever those objects need. If you use one, resolve the page there instead of calling `new`:

```csharp
new NavPage("Settings", SettingsIcon, () => provider.GetRequiredService<SettingsPage>()),
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

With a single list there is nothing to keep in sync, so neither mistake is available. The
`Kuwantima.Sandbox` project is a complete worked example.

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
