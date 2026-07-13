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
