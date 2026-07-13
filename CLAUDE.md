# Kuwantima — Claude Code Guidelines

## Project Identity
Kuwantima is an open-source Avalonia UI glass-glow design system library.
Single entry point: `<StyleInclude Source="avares://Kuwantima/Theme/KuwantimaPrimaryTheme.axaml"/>`

## Version Management
- **Source of truth**: `<Version>` in `Kuwantima/Kuwantima.csproj`
- Sandbox UI reads version from the Kuwantima assembly at runtime (`MainWindowViewModel.KuwantimaVersion`)
- Header bar and Documents page both bind to this property — no hardcoded version strings
- `KuwantimaPrimaryTheme.axaml` header has a manual VERSION HISTORY changelog — update it when bumping version
- Git tags should match: `git tag v{version}`

## Completeness Invariants
When adding or changing controls/features, keep ALL of these in sync:

### New Control Checklist
1. **Style file** — `Kuwantima/Styles/Kuwantima{Control}.axaml` with `Design.PreviewWith` for both themes
2. **StyleInclude** — register in `KuwantimaPrimaryTheme.axaml` (section 2: Control Styles)
3. **Disabled state** — pin Foreground, Cursor="Arrow", Opacity to prevent Fluent double-dimming
4. **Interactive controls** — set `Cursor="Hand"` on base style
5. **Sandbox page** — demo the control in the appropriate sandbox page
6. **Documents page** — add to the controls table in `DocumentsPage.axaml`
7. **README.md** — add to the Controls table
8. **Control count** — update count in README intro, `.csproj` Description, and KuwantimaPrimaryTheme header
9. **Handouts** — if the control is worth teaching, add it to `docs/` (see Handouts below)

### New Variant Checklist
1. **Style selectors** — add within the control's existing style file
2. **Design.PreviewWith** — add variant section to the previewer
3. **Sandbox page** — demo the variant
4. **Documents page** — add to Variants column in controls table
5. **README.md** — add to Controls table Variants column and Variant Examples if notable
6. **Handouts** — if the variant changes what a lesson teaches, revise `docs/` (see Handouts below)

### New Theme Resource Checklist
1. **Define in both themes** — Light AND Dark dictionaries in `KuwantimaThemeResources.axaml`
2. **Resource catalog** — update the table in the `KuwantimaThemeResources.axaml` header comment
3. **Documents page** — add to Available Theme Resources table if user-facing
4. **README.md** — add to Available Theme Resources table

### New Sandbox Page Checklist
1. **AXAML + code-behind** — `Kuwantima.Sandbox/Views/Pages/{Name}Page.axaml(.cs)`
2. **ViewModel** — add `Is{Name}PageVisible` property + `OnPropertyChanged` call in `OnSelectedPageIndexChanged`
3. **MainWindow nav** — add `ToggleButton` in SplitView.Pane with next sequential CommandParameter
4. **MainWindow content** — add `<pages:{Name}Page IsVisible="{Binding Is{Name}PageVisible}"/>` in Panel
5. **README.md** — update page count and page list in Sandbox section

### Version Bump Checklist
1. **Kuwantima.csproj** — update `<Version>` (this is the source of truth)
2. **KuwantimaPrimaryTheme.axaml** — add new entry to VERSION HISTORY in header comment
3. **Documents page** — add new version entry to Version History section
4. **Git tag** — `git tag v{version}` after commit

## Retired Resources — Do Not Reintroduce
These were replaced by Fluent equivalents. Use the Fluent key instead:
- ~~KuwantimaAccentForeground~~ → `TextOnAccentFillColorPrimaryBrush`
- ~~KuwantimaSecondaryTextBrush~~ → `TextFillColorSecondaryBrush`
- ~~KuwantimaSubtitleTextBrush~~ → `TextFillColorTertiaryBrush`
- ~~KuwantimaGlassBorder / KuwantimaGlassBorderHover~~ — removed (unused)
- ~~KuwantimaShadowNormal / Hover / Accent~~ — removed (orphaned)

## Color Philosophy
- **Cool anchor**: MidnightBlue (#191970) / AliceBlue (#F0F8FF)
- **Warm accent**: Orange (#FF8C00 light / #FFA500 dark) — checked/selected borders
- **System accent**: #0078D4 (Fluent blue) — filled accent backgrounds
- Do not introduce colors outside this story without intention

## Build Notes
- Running sandbox locks DLLs — close app before full rebuild
- `dotnet build --no-dependencies` + grep `error CS` to verify code when DLL locked
- `dotnet pack Kuwantima/Kuwantima.csproj -c Release` to build NuGet package

## Handouts (`docs/`)
Teaching material, distributed as standalone files — copied to USB, printed, emailed
individually. Not repo-internal docs, and not linked from the README.
- `01-your-toolkit.html` — lesson 1
- `02-your-first-project.html` — lesson 2
- `teacher-guide.html` — instructor companion

Conventions:
- **Self-contained** — CSS stays inlined in each file even though ~75% is shared between
  them. A linked stylesheet breaks the moment one handout travels alone. Accept the
  duplication; if you restyle, edit all three.
- **Version-agnostic** — no version numbers or control counts in the prose, so they don't
  go stale on every release. Keep it that way.
- **Palette** — same story as the library: MidnightBlue on AliceBlue, Inter, 750px column.

## File Conventions
- Theme files: `Kuwantima/Theme/`
- Style files: `Kuwantima/Styles/Kuwantima{Control}.axaml`
- Sandbox pages: `Kuwantima.Sandbox/Views/Pages/{Name}Page.axaml`
- Handouts: `docs/*.html`
- No unit tests — sandbox IS the test harness (visual correctness, not assertion-based)
