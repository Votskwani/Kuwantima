# Kuwantima

A glass-glow design system for [Avalonia UI](https://avaloniaui.net/).
One `StyleInclude`, 16 styled controls, automatic light/dark theming.

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

### DataGrid (optional)

DataGrid ships as a separate package, **`Kuwantima.DataGrid`**, because the control itself lives
in Avalonia's own separate `Avalonia.Controls.DataGrid` package, not core Avalonia — installing
the core `Kuwantima` package never pulls that dependency in. If you want a styled grid, add the
second package (it depends on `Kuwantima`, so that comes along automatically) and one more
`StyleInclude`, after Kuwantima's own:

```
dotnet add package Kuwantima.DataGrid
```

```xml
<Application.Styles>
    <StyleInclude Source="avares://Kuwantima/Theme/KuwantimaPrimaryTheme.axaml"/>
    <StyleInclude Source="avares://Kuwantima.DataGrid/Theme/KuwantimaDataGridTheme.axaml"/>
</Application.Styles>
```

```xml
<DataGrid Classes="Kuwantima" AutoGenerateColumns="False">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Vehicle" Binding="{Binding Id}"/>
        <DataGridTextColumn Header="Status" Binding="{Binding Status}"/>
    </DataGrid.Columns>
</DataGrid>
```

Covers headers (hover/pressed, sort-direction glyph), rows (hover, selection joining the same
accent family as ListBox/ComboBox), alternating-row tint, grid lines, disabled state, and
`:invalid` reusing `KuwantimaValidationErrorBrush`. Not yet covered: row grouping, frozen columns,
inline cell-editing chrome, row-details expansion.

`Kuwantima.DataGrid` is versioned in lockstep with `Kuwantima` — same version number, same
release — even on a release where only one of the two actually changed.

## Controls

The table below is the **core `Kuwantima` package** — 16 controls, one dependency-light install.
DataGrid ships separately; see [DataGrid (optional)](#datagrid-optional) above.

| Control | Class | Variants |
|---------|-------|----------|
| Button | `Kuwantima` | `Accent` |
| CheckBox | `Kuwantima` | `Classic` |
| Color Palette Picker | `KuwantimaPalette` (trigger), `KuwantimaSwatch` (swatch) | |
| ComboBox | `Kuwantima` | `:error` |
| Expander | `Kuwantima` | |
| Border (Glass) | `KuwantimaGlass` | |
| GridSplitter | `Kuwantima` | `Pill`, `Arrow` (+ `Horizontal`/`Vertical`) |
| ListBox | `Kuwantima` | |
| MenuToggleButton | `KuwantimaMenu` | |
| ProgressBar | `Kuwantima` | |
| RadioButton | `Kuwantima` | `Classic` |
| Slider | `Kuwantima` | |
| TabControl | `Kuwantima` | |
| TextBox | `Kuwantima` | `ReadOnly`, `:error` |
| ToggleButton | `Kuwantima` | |
| ToolTip | `Kuwantima` | |

`:error` isn't a class you add — unlike every other variant above, it's Avalonia's native
`:error` pseudo-class, which fires automatically from `DataValidationErrors`/binding validation.

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
| `KuwantimaValidationErrorBrush` | Border edge on native `:error` (TextBox, ComboBox) |
| `KuwantimaSuccessTextBrush` | Positive-outcome label text |
| `KuwantimaWarningTextBrush` | Negative-outcome label text |
| `KuwantimaTooltipBackground` | Tooltip backdrop |
| `KuwantimaTooltipForeground` | Tooltip text color |
| `KuwantimaSplitterBrush` | GridSplitter line at rest |
| `KuwantimaSplitterHoverBrush` | GridSplitter line on hover |
| `KuwantimaScrimBackground` | Wash behind a blocking overlay (a probe, save, or other long-running operation) |
| `KuwantimaScrimForeground` | Text/ink on the scrim |
| `KuwantimaPaletteBlue` | ColorPalettePicker swatch: blue (theme-invariant) |
| `KuwantimaPaletteOrange` | ColorPalettePicker swatch: orange (theme-invariant) |
| `KuwantimaPalettePurple` | ColorPalettePicker swatch: purple (theme-invariant) |
| `KuwantimaPaletteGreen` | ColorPalettePicker swatch: green (theme-invariant) |
| `KuwantimaPaletteRose` | ColorPalettePicker swatch: rose (theme-invariant) |
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

The same `StyleInclude` brings in 14 icon geometries. They are ordinary `StreamGeometry` resources,
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
| `Icon.Collapse` | `Icon.Add` | |

A key that does not exist renders **nothing** rather than failing loudly, so check a blank icon
against this table before looking anywhere else.

### Icon Attribution

Most of these are drawn from existing icon sets rather than original geometry, and retaining
credit is a condition of both licenses:

- `Icon.Home`, `Icon.Gear`, `Icon.Sliders`, `Icon.Expand`, `Icon.Collapse`, `Icon.Search`,
  `Icon.Clear.Circle`, `Icon.Layers`, `Icon.Refresh`, `Icon.Map`, `Icon.Copy`, `Icon.Add` — from
  [Material Design Icons](https://pictogrammers.com/library/mdi/) by Pictogrammers, licensed
  [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0).
- `Icon.Sun` — from [Fluent System Icons](https://github.com/microsoft/fluentui-system-icons) by
  Microsoft, licensed [MIT](https://github.com/microsoft/fluentui-system-icons/blob/main/LICENSE).
- `Icon.Moon.ThirdEye.Smiling` is original.

No changes were made to the geometry beyond what's needed to use it as a `StreamGeometry` resource.

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

## Color Palette Picker

`Button.KuwantimaPalette` is a 48×48 square trigger — the same footprint as the `KuwantimaMenu`
nav button, so it sits naturally next to one — that opens a `Flyout` onto a grid of
`ToggleButton.KuwantimaSwatch` color swatches. It's the first Kuwantima control built on a
`Flyout` rather than a custom `Popup`.

```xml
<Button Classes="KuwantimaPalette">
    <Button.Flyout>
        <Flyout FlyoutPresenterClasses="KuwantimaPalette" Placement="Bottom">
            <Grid ColumnDefinitions="Auto,Auto,Auto" RowDefinitions="Auto,Auto" RowSpacing="8" ColumnSpacing="8">
                <ToggleButton Classes="KuwantimaSwatch" Grid.Row="0" Grid.Column="0" Background="{DynamicResource KuwantimaPaletteBlue}"/>
                <ToggleButton Classes="KuwantimaSwatch" Grid.Row="0" Grid.Column="1" Background="{DynamicResource KuwantimaPaletteOrange}"/>
                <ToggleButton Classes="KuwantimaSwatch" Grid.Row="0" Grid.Column="2" Background="{DynamicResource KuwantimaPalettePurple}"/>
                <ToggleButton Classes="KuwantimaSwatch" Grid.Row="1" Grid.Column="0" Background="{DynamicResource KuwantimaPaletteGreen}"/>
                <ToggleButton Classes="KuwantimaSwatch" Grid.Row="1" Grid.Column="1" Background="{DynamicResource KuwantimaPaletteRose}"/>
                <!-- Row 1, Column 2 is deliberately empty — room for a 6th, not a placeholder button. -->
            </Grid>
        </Flyout>
    </Button.Flyout>
</Button>
```

Two things worth being precise about, the same way the nav section above is about `Tag`/`Content`:

- **`FlyoutPresenterClasses="KuwantimaPalette"` is what styles the flyout itself** — an opaque
  panel matching `ComboBox`'s own dropdown, not the translucent glass background used elsewhere.
  A flyout floats over arbitrary content, so translucency there would let whatever's behind it
  show through the swatches. `FlyoutPresenterClasses` is the only way to reach the
  auto-generated presenter with a selector; it never appears in markup you write.
- **A swatch's color is set per instance, on `Background`** — it is not baked into the style. This
  is what makes the swatch grid open-ended rather than fixed at 5: adding a 6th color is one more
  `ToggleButton` line, not a style or layout change.

### Overriding the 5 default colors

`KuwantimaPaletteBlue/Orange/Purple/Green/Rose` are ordinary theme resources, overridable exactly
like [any other Kuwantima brush](#overriding-brushes) — redefine the key after the `StyleInclude`.
They're deliberately identical in both Light and Dark dictionaries (a swatch is a literal paint
choice, not UI ink, so it shouldn't shift with the app's theme), so override both if you want to
keep that property:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.ThemeDictionaries>
            <ResourceDictionary x:Key="Light">
                <SolidColorBrush x:Key="KuwantimaPaletteBlue" Color="#2563EB"/>
            </ResourceDictionary>
            <ResourceDictionary x:Key="Dark">
                <SolidColorBrush x:Key="KuwantimaPaletteBlue" Color="#2563EB"/>
            </ResourceDictionary>
        </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

If you replace one of these, re-check it the way `KuwantimaPaletteOrange` and `KuwantimaPaletteGreen`
had to be re-picked in this repo: as a **live Fluent accent**, not just a fill color — see below.

### Adding more colors

No resource involved at all — just add another swatch with whatever brush or literal color you
want:

```xml
<ToggleButton Classes="KuwantimaSwatch" Grid.Row="1" Grid.Column="2" Background="#009688"/>
```

### Wiring it to actually do something

The styles don't care what happens when a swatch is picked — same as the nav pattern above, that
part is yours. Mutual exclusion is the same shape as the nav `ToggleButton`s:
`IsChecked="{Binding SelectedColor, Mode=OneWay, Converter={x:Static ObjectConverters.Equal}, ConverterParameter=...}"`
plus a `Command` that updates `SelectedColor`.

`Kuwantima.Sandbox` goes one step further and uses the selection to re-tint the app's *live* Fluent
accent — every `SystemAccentColor*`-derived surface in the app, not just the swatch itself — by
reaching `Application.Current`'s `FluentTheme` and setting
`.Palettes[variant].Accent` (see `MainWindowViewModel.cs`). That's a real worked example, but it's
Sandbox application code, not something the package provides: Kuwantima ships zero C#, so "does
picking a color change anything" is entirely up to your app, the same way page navigation is.
**If you replace one of the 5 defaults, or add your own, re-check it against white text as a live
accent** the way the two colors above had to be — a color that reads fine as a small swatch can
still fail contrast once it's driving `SystemAccentColorDark1`/`Dark2` behind white ink.

## Sandbox

The `Kuwantima.Sandbox` project is a live gallery of every control and variant. Run it to preview the full design system:

```
dotnet run --project Kuwantima.Sandbox
```

Eight demo pages: **Buttons**, **Inputs**, **Toggles**, **Feedback**, **Containers**, **DataGrid**
(`Kuwantima.DataGrid`'s demo — requires the sandbox to reference that package too, which it does),
**Theme Preview** (side-by-side light/dark), and **Documents** (styled README + license dialog).

## Requirements

- .NET 10.0
- Avalonia 12.0+
- Avalonia.Themes.Fluent 12.0+

## License

MIT License. See [LICENSE](LICENSE) for details.
