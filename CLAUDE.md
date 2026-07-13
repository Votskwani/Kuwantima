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
Items 2, 3, 4, and 8 below are **enforced by `Kuwantima.Tests`** — you cannot forget them, the
suite goes red. The rest are still on you. Run `dotnet test` before you commit.

### New Control Checklist
1. **Style file** — `Kuwantima/Styles/Kuwantima{Control}.axaml` with `Design.PreviewWith` for both themes
2. **StyleInclude** — register in `KuwantimaPrimaryTheme.axaml` (section 2: Control Styles) — *tested*
3. **Disabled state** — pin Foreground, Cursor="Arrow", Opacity to prevent Fluent double-dimming — *tested*
4. **Interactive controls** — set `Cursor="Hand"` on base style — *tested*
5. **Test subject** — add an entry to the `Subjects` table in `Kuwantima.Tests/InvariantTests.cs`.
   The suite embeds `Styles/*.axaml` by wildcard, so a new file is swept in automatically and the
   suite **fails until you classify it** — is it interactive, does it need a disabled pin? If it is
   exempt from an invariant, write the *reason* in the exemption string. An exemption invented to
   turn a red test green is a weakening; an exemption the invariant genuinely allows is not.
6. **Sandbox page** — demo the control in the appropriate sandbox page
7. **Documents page** — add to the controls table in `DocumentsPage.axaml`
8. **README.md** — add to the Controls table
9. **Control count** — update in README intro, `.csproj` Description, and KuwantimaPrimaryTheme
   header (2 places) — *tested*. **The count is CONTROLS, not style files.** There are 16 style
   files but only **15 controls**: `KuwantimaStreamIcons.axaml` is a `ResourceDictionary` of icon
   geometry, registered via `ResourceInclude`, and is not a control. Miscounting it is exactly the
   off-by-one that shipped in v1.0.0 and survived until the suite caught it.
10. **Handouts** — if the control is worth teaching, add it to `docs/` (see Handouts below)

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
4. **Handout stamps** — update `doc-version` in the footer of all three `docs/*.html`
5. **Git tag** — `git tag v{version}` after commit

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
- **Version-agnostic prose, one version stamp** — the body text names no version or control
  count, so a release doesn't stale it. Each file carries exactly one stamp, in the footer
  (`<span class="doc-version">Written for Kuwantima vX.Y.Z</span>`), so a handout sitting on
  a USB stick still says which release it belongs to. Keep the stamp the only version string
  in the file.
- **Palette** — same story as the library: MidnightBlue on AliceBlue, Inter, 750px column.

## Testing (`Kuwantima.Tests`)
`dotnet test Kuwantima.Tests/Kuwantima.Tests.csproj` — ~1s. Runs in CI on every push, and gates the
NuGet publish: a red suite cannot ship.

There are no *unit* tests, because there are no units — the library is pure XAML, zero C#. What the
suite covers instead:
- **Tier 1 `ThemeIntegrityTests`** — the theme loads from its single public StyleInclude; Light and
  Dark declare exactly the same resource keys; every key resolves under both variants; controls
  template and lay out under both.
- **Tier 2 `InvariantTests`** — the completeness invariants above, executed. Registration, the
  Cursor="Hand" rule, the disabled pins, the retired-key ban, the control count.

Rules that keep it honest:
- **Parse, never grep.** Facts come from `XDocument` over the AXAML tree. Text matching produces
  false positives on comments — this repo documents its own retired keys in comments, and
  `KuwantimaGlassBorder` is simultaneously a retired *key* and a live *filename*.
- **Keys and files are read from source, not hardcoded.** Add a brush or a style file and it is
  under test immediately, with no edit to the suite.
- **Exemptions carry their reason in code.** See the `Subjects` table.
- **Never weaken a test to make it green.** A red test on a real violation is the suite working.

Deliberately NOT built: pixel/golden-image snapshots. Blur and antialiasing vary across rasterizers
and CI machines, so they would false-fail constantly, and every intentional design tweak would
invalidate every baseline. The sandbox remains the harness for *visual* correctness — glass, glow,
spacing, the orange checked border. Tests cover the mechanical layer; eyes cover the aesthetic one.

## File Conventions
- Theme files: `Kuwantima/Theme/`
- Style files: `Kuwantima/Styles/Kuwantima{Control}.axaml`
- Sandbox pages: `Kuwantima.Sandbox/Views/Pages/{Name}Page.axaml`
- Handouts: `docs/*.html`
- Tests: `Kuwantima.Tests/` — see Testing above
