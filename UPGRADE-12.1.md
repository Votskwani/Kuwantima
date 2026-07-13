# Upgrade plan — Avalonia 12.0.1 → 12.1.0

Written 2026-07-13. Delete this file once the upgrade ships.

Research: 7-lens multi-agent sweep, every claim verified by fetching the cited source.
95 findings → **86 confirmed, 6 refuted, 3 unverifiable**.

## Verdict: do it. It fixes more than it risks.

Two things are **broken today** and the upgrade fixes both. Nothing confirmed will break the library.

---

## What moves, and what doesn't

| Component | Now | Action |
|---|---|---|
| Avalonia (+ Desktop, Themes.Fluent, Fonts.Inter) | 12.0.1 | → **12.1.0** |
| `Avalonia.Diagnostics` | 11.3.14 | **delete the reference** (see below) |
| TargetFramework | `net10.0` | **stay.** .NET 11 is preview-only; this is a published library, so a preview TFM would force every consumer onto a preview runtime |
| CommunityToolkit.Mvvm | 8.4.2 | **stay** — already latest |

Bump Avalonia in **three** csproj files: `Kuwantima`, `Kuwantima.Sandbox`, **and `Kuwantima.Tests`**.
The test project pins its own Avalonia version — leave it behind and you are testing a different
framework than you ship.

---

## The three findings that matter

### 1. Your XAML previewer is broken right now — 12.0.2 fixes it

Avalonia 12.0.0 removed the `Design.SetPreviewWith(AvaloniaObject, Control?)` overload, leaving only
`ITemplate<Control>`. **All 16 style files** open with `<Design.PreviewWith><Border …>` — a raw
`Control` child — so every one of them hits the missing overload. Symptom is a design-time error
(*"…is not a valid value for ITemplate<Control>?"*), **not** a build failure, which is why it went
unnoticed since v1.1.0.

PR [#21184](https://github.com/AvaloniaUI/Avalonia/pull/21184) restores the overload in **12.0.2**.

CLAUDE.md mandates a `Design.PreviewWith` in every style file and treats the previewer as the
authoring harness — so this restores the primary workflow. **This alone justifies the bump.**

### 2. `Avalonia.Diagnostics` is dead — and you never used it

Formally deprecated on NuGet, no 12.x exists (stops at 11.3.18), replaced by
`AvaloniaUI.DiagnosticsSupport`, and `AttachDevTools()` is renamed `AttachDeveloperTools()`.

**A grep of all 13 sandbox C# files found zero `AttachDevTools` calls.** The Debug-only
`PackageReference` in `Kuwantima.Sandbox.csproj` is dead weight. Delete it. No code change.

### 3. Fluent's `PathIcon` theme dropped its `Foreground` setter (12.0.2)

PR [#21251](https://github.com/AvaloniaUI/Avalonia/pull/21251) — verified against the theme file at
both tags. A bare `PathIcon` now **inherits** `Foreground` instead of being pinned to
`TextControlForeground`.

- **Library: safe.** All 4 in-template icons pin `Foreground` explicitly
  (`KuwantimaComboBox.axaml:130`, `KuwantimaExpander.axaml:120`,
  `KuwantimaMenuToggleButton.axaml:106`, and all of `KuwantimaStreamIcons.axaml`).
- **Sandbox: 11 unpinned PathIcons** — `MainWindow.axaml:31,62` and
  `ButtonsPage.axaml:21,42,48,54,78,82,86,90,94`.
- **The direction is a FIX, not a break.** `ButtonsPage.axaml:42` is a bare icon inside
  `<Button Classes="Kuwantima Accent">`. Today that icon renders **dark next to a white label** on
  an accent-blue button. After the bump both go white — matching the `TextElement.Foreground` the
  Button template already sets on its content presenter.
- **Consumer note:** Kuwantima's global `<Style Selector="PathIcon">`
  (`KuwantimaPrimaryTheme.axaml:275`) sets only Width/Height, so this change flows straight through
  to downstream consumers. That is a deliberate design decision to make — see step 6.

---

## Steps

0. **Baseline.** `dotnet test` → **110 green at 12.0.1**. This is the control. Anything red after
   the bump is a framework regression, precisely localized.
1. **Bump** Avalonia to `12.1.0` in `Kuwantima.csproj`, `Kuwantima.Sandbox.csproj`,
   `Kuwantima.Tests.csproj`.
2. **Delete** the `Avalonia.Diagnostics` `PackageReference` from `Kuwantima.Sandbox.csproj`.
3. *(Optional)* Add a `global.json` pinning SDK `10.0.x` — there is none today, so the build (and CI)
   floats to whatever SDK is installed.
4. **`dotnet test`** → expect 110 green. Investigate anything red before proceeding.
5. **`dotnet build Kuwantima.slnx`** → sandbox included.
6. **Visual pass — the tests cannot see.** This is the part no suite covers:
   - Open any `Kuwantima/Styles/*.axaml` in the IDE previewer. The Light|Dark panes should now
     **render instead of erroring**. Check one before assuming all 16.
   - Run the sandbox → **ButtonsPage**, both themes. The Accent "Search" button icon should now be
     **white, matching its label** (it is dark today). Confirm that is what you want.
   - Check the 5 icon-only Minimal buttons and the MainWindow nav icons.
   - **Decide:** leave `PathIcon` inheriting (*recommended* — it honours the `TextElement.Foreground`
     the Button template already sets), or re-pin `Foreground` in the global `PathIcon` style at
     `KuwantimaPrimaryTheme.axaml:275`.
7. **Release** per the Version Bump Checklist in CLAUDE.md → v1.2.0: `.csproj`, theme VERSION
   HISTORY, Documents page, **the 3 handout stamps in `docs/`**, then `git tag v1.2.0`.
8. **Push the tag.** Actions runs the suite as a gate, then publishes to NuGet.

---

## Notes

- **6 findings were refuted** during verification (hallucinated or out-of-range breaking changes).
  They are not listed here on purpose — do not go looking for them.
- **3 were unverifiable**, all peripheral. The main one: the exact licensing/architecture of
  `AvaloniaUI.DiagnosticsSupport` (there are hints of a paid tier). It does not affect us, since
  nothing in the repo calls DevTools at all.
- The suite catches: theme load failure, Light/Dark key asymmetry, unresolvable resources, controls
  that stop templating, and every CLAUDE.md invariant. It does **not** catch: colour, blur, glow,
  spacing, or the PathIcon change. That is what step 6 is for.
