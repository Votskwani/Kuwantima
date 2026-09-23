# Kuwantima — Claude Code Guidelines

## Project Identity
Kuwantima is an open-source Avalonia UI glass-glow design system library.
Single entry point: `<StyleInclude Source="avares://Kuwantima/Theme/KuwantimaPrimaryTheme.axaml"/>`

## Who this is actually for
Checked 2026-07-13: NuGet shows ~245 downloads (mirrors and scanners — noise), GitHub shows 0 stars,
0 watchers, 0 views, and **1 fork**. The `docs/` handouts are step-by-step instructions for forking
this repo. That one fork is almost certainly the person the teacher guide was written for.

So Kuwantima's real user is **a student learning to fork, clone, edit and push** — not a NuGet
consumer integrating a design system. Prioritise accordingly:
- **The IDE previewer working is a first-class feature**, not a nicety. A beginner who opens a style
  file and sees an error has no way to know it isn't their fault. That is where a first-time
  contributor quits.
- Readability beats cleverness. Someone is reading this code *to learn from it*.
- The handouts are as much the product as the package is.
- API/versioning polish matters least. It is still worth doing right (see below) — just don't let it
  outrank the two above.

## Version Management
- **Source of truth**: `<Version>` in `Kuwantima/Kuwantima.csproj`
- Sandbox UI reads version from the Kuwantima assembly at runtime (`MainWindowViewModel.KuwantimaVersion`)
- Header bar and Documents page both bind to this property — no hardcoded version strings
- `KuwantimaPrimaryTheme.axaml` header has a manual VERSION HISTORY changelog — update it when bumping version
- Git tags should match: `git tag v{version}`

### What earns a major version
Version numbers describe the **consumer's migration burden, not the maintainer's effort**. A big
session is not a major release.

MAJOR (2.0.0) — the consumer must edit something to keep working:
- removing or renaming a class selector (`Kuwantima`, `KuwantimaGlass`, `Pill`, `Accent`, …)
- removing or renaming a theme resource key
- changing the `StyleInclude` entry point
- dropping a control, or a deliberate redesign of the colour story
- **raising `<TargetFramework>`** — this makes the package *uninstallable* for consumers on the old
  TFM, which is worse than any visual change

MINOR (1.x.0) — new controls/variants, bug fixes, visible behaviour corrections, dependency minors.
Fixing a bug is not a breaking change even when it is visible.

NOT A RELEASE AT ALL — changes confined to `Kuwantima.Sandbox`, `Kuwantima.Tests` or `docs/`. None
of them are in the package, so the consumer's migration burden is zero and there is nothing new in
the DLL. Resist bumping out of habit. **The one exception is `README.md`**, which ships as the
package's front page (`<PackageReadmeFile>`), so a meaningful documentation improvement *can* justify
a release on its own — just decide that deliberately rather than by reflex.

### TargetFramework: stay on `net10.0`. Deliberately.
Do not "upgrade" to .NET 11 when it ships. Reasons, in order:
1. **Zero benefit.** The library has no C#. A framework bump buys language features, runtime perf and
   BCL APIs — all of which need code to use. The TFM here is nearly vestigial.
2. **`net10.0` packages already run inside `net11.0` apps.** Forward compatibility means you lose no
   reach by staying. Raising the TFM only ever *subtracts* consumers.
3. .NET 10 is **LTS**; .NET 11 is **STS** (18 months). A library should floor on LTS.
4. Multi-targeting `net10.0;net11.0` is NOT the clever workaround — it produces two identical
   outputs for a package with no code. Don't.

Revisit when .NET 10 leaves support (~2028), not before. If you want to play with a new .NET, move
**`Kuwantima.Sandbox`** — it has its own TFM and is never published.

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
3. **README.md** — add to the Available Theme Resources table. If the key is **Fluent's**, being
   overridden rather than introduced, it goes in the *Fluent keys Kuwantima overrides* table instead,
   with the contrast reason — a consumer who re-overrides it needs to know what it was protecting.
4. **Documents page** — it has no resource table, only the three-layer colour-story card. Update that
   card only if the *story* changed (a new layer, or a layer's role changed), not for every key.
5. **Contrast** — if anything readable sits on it, add the (ink, surface) pair to `Invariant_6`
   (`InvariantTests.Contrast.cs`) and let it measure both variants. Translucent? Give it the backdrop
   chain so it is composited rather than read as a literal.

### New Sandbox Page Checklist
1. **AXAML + code-behind** — `Kuwantima.Sandbox/Views/Pages/{Name}Page.axaml(.cs)`
2. **ViewModel** — add `Is{Name}PageVisible` property + `OnPropertyChanged` call in `OnSelectedPageIndexChanged`
3. **MainWindow nav** — add `ToggleButton` in SplitView.Pane with next sequential CommandParameter
4. **MainWindow content** — add `<pages:{Name}Page IsVisible="{Binding Is{Name}PageVisible}"/>` in Panel
5. **README.md** — update page count and page list in Sandbox section

**Two of those steps fail SILENTLY, which is why this navigation is slated for rework (v1.4.0).**
Nothing here is enforced by the suite — the sandbox is not under test — so the failure modes matter:
- Forget the `OnPropertyChanged` line in step 2 and the nav button highlights correctly while the
  page never appears. No exception, no binding error. A hand-maintained notification list is the
  bug; `OnPropertyChanged(string.Empty)` notifies everything and can never go stale.
- The `CommandParameter` in step 3 is a **magic number** that must match the property's
  `SelectedPageIndex == N`. Off by one and you silently get the wrong page.

**Also: `Kuwantima.Sandbox/ViewLocator.cs` is dead code, and it would not work if it were live.**
It is never invoked — `MainWindow` is constructed directly in `App.axaml.cs`, there are no page
ViewModels, and no view binds a `ContentControl`. Its name mangling also maps
`…ViewModels.MainWindowViewModel` → `…Views.MainWindowView`, but the class is `…Views.MainWindow`,
so it would render `Not Found:` if anything did route through it. It is leftover
`dotnet new avalonia.mvvm` scaffolding. Do not treat it as the page-routing mechanism; it routes
nothing. v1.4.0 should either make it real (ViewModel-first navigation, which removes both footguns
above structurally) or delete it.

### Version Bump Checklist
1. **Kuwantima.csproj** — update `<Version>` (this is the source of truth) **and rewrite
   `<PackageReleaseNotes>`**. Those notes render on the nuget.org package page and in Visual
   Studio's package-details pane — they are the only "what changed" a consumer sees when deciding
   whether to upgrade, and shipping the previous release's notes is worse than shipping none.
   (There is no install-time note to use instead: `readme.txt` auto-display and `tools/install.ps1`
   were `packages.config` features and do **not** run under PackageReference.)
2. **KuwantimaPrimaryTheme.axaml** — add new entry to VERSION HISTORY in header comment
3. **Documents page** — add new version entry to Version History section
4. **Handout stamps** — update `doc-version` in the footer of all three `docs/*.html`
5. **Git tag** — `git tag v{version}` after commit

## Retired Resources — Do Not Reintroduce
These were replaced by Fluent equivalents. Use the Fluent key instead:
- ~~KuwantimaAccentForeground~~ → `AccentButtonForeground`
- ~~KuwantimaSecondaryTextBrush~~ → `SystemControlForegroundBaseMediumHighBrush`
- ~~KuwantimaSubtitleTextBrush~~ → `SystemControlForegroundBaseMediumBrush`
- ~~KuwantimaGlassBorder / KuwantimaGlassBorderHover~~ — removed (unused)
- ~~KuwantimaShadowNormal / Hover / Accent~~ — removed (orphaned)

**This table was wrong from v1.1.0 to v1.2.0, and that is the third time this repo's
verification trap has fired.** It sent the first three to `TextOnAccentFillColorPrimaryBrush` /
`TextFillColorSecondaryBrush` / `TextFillColorTertiaryBrush` — WinUI "FillColor" keys that Avalonia
**11**'s Fluent shipped and Avalonia **12**'s does not. Avalonia 12 dropped the whole family
(`TextFillColor*`, `TextOnAccentFillColor*`, `ControlFillColor*`, `AccentFillColor*`).

An unresolvable `DynamicResource` is **silent** — no throw, no build error; the setter just never
applies and the property keeps its default. So from the v1.1.0 Avalonia-12 upgrade onward, 20
references across 8 style files did nothing: every accent control rendered black-on-blue, and every
checkmark and radio dot (`Stroke`/`Fill`, not just `Foreground`) rendered dark on its accent chip.
Contrast happened to stay above AA, which is why eyes never caught it.

Now enforced by `InvariantTests.Invariant_5_every_consumed_resource_key_resolves` — every key the
library *consumes* must resolve under both variants. The suite previously only checked keys the theme
*defines*, which is why this ran for two releases. **Before substituting a framework key, probe it**
(`Application.Current.TryGetResource` in a throwaway `[AvaloniaFact]`). Do not trust this table, the
theme header, or the style files agreeing with each other — they are one origin, restated.

## Downstream: Tunatya / Navoti
Kuwantima **replaces** Navoti in `../Tunatya`. It does not compose with it — they are the same
design system at two points in time (same architecture, same class convention, 15 controls vs
Navoti's 12). The Retired Resources list above is, literally, a changelog of what Navoti still has.

- **Never run both.** Each ships its own `<fluent:FluentTheme>` and overrides the same three
  `SystemFillColor*` keys. Avalonia resolves by document order, so one silently loses — no error.
- **The retirement list is a migration burden, not just history.** 27 live references in Tunatya
  point at `KuwantimaGlassBorder` / `KuwantimaGlassBorderHover` — keys retired here as *"unused."*
  They were unused **in Kuwantima**. Before retiring anything else, grep Tunatya first.
- **Kuwantima is NOT a strict superset.** Navoti has `NavotiOverlayBackground` /
  `NavotiOverlayTextBrush`; there is no Kuwantima equivalent. Decide deliberately whether to add them.
- Migration plan: `Tunatya/NAVOTI-TO-KUWANTIMA.md`

## A verification trap this repo has set three times
Each of these shipped (or nearly), and each survived a check that *felt* rigorous:

- **The control count.** README, `.csproj`, the theme header and the Documents page all said 16.
  Four sources, unanimous — and all wrong, because all four restate a single origin. Checking them
  against each other **confirmed** the error. The truth was `ls Kuwantima/Styles/` minus StreamIcons.
- **The retired resources.** They were "unused" — measured within Kuwantima only. 27 references were
  live in Tunatya the whole time.
- **The retired keys' replacements.** The Retired Resources table, the theme header and the migration
  note all agreed the old brushes mapped to `TextOnAccentFillColorPrimaryBrush` /
  `TextFillColor*Brush`. Unanimous — and all dead, because Avalonia 12's Fluent dropped that whole
  family and none of the three sources had asked the framework. Silent for two releases (see
  Retired Resources above and the accent-text note below).

**Agreement between sources that share an origin is not verification.** Go to ground truth: run
`dotnet test`, write a throwaway `[AvaloniaFact]`, list the directory, grep the *consumer*, resolve
the key against the live theme. Not to a restatement, however many of them agree.

## Color Philosophy
- **Cool anchor**: MidnightBlue (#191970) / AliceBlue (#F0F8FF)
- **Warm accent**: Orange (#FF8C00 light / #FFA500 dark) — checked/selected borders
- **System accent**: #0078D4 (Fluent blue) — filled accent backgrounds
- Do not introduce colors outside this story without intention

### Accent-on text is WHITE, and the ramp is what makes that possible (v1.3.0)
`AccentButtonForeground` is overridden to **White** in both theme dictionaries
(`KuwantimaThemeResources.axaml`). It was **Black** in v1.2.0. The flip is not a reversal of
judgement — the *shape of the problem* changed, and that is the part worth remembering.

**v1.2.0's ramp ran in two directions at once.** `Button.Kuwantima.Accent` painted `#0078D4` at
rest, went **lighter** (`SystemAccentColorLight3` `#a3d7ff`) on hover, and went **orange** on press.
A foreground is set once on the base style, so one colour had to survive all three. No light colour
can: white measures 1.53 on the pale hover and 2.33 on orange. Black was therefore the *only*
accessible answer — correct, but an answer to a badly-shaped question. It left the resting state
muted at 4.64.

**v1.3.0 reshapes the ramp to run one way — darker on interaction — and a single light foreground
then clears AA everywhere.** Measured, and identical under both variants because the
`SystemAccentColor*` ramp is itself theme-invariant (only `KuwantimaAccentOrangeBrush` differs):

| State | Background | White |
|---|---|---|
| rest / checked / selected | `SystemControlBackgroundAccentBrush` `#0078D4` | 4.53 ✓ |
| `:pointerover` | `SystemAccentColorDark1` `#00589b` | 7.32 ✓ |
| `:pressed` (Button) | `SystemAccentColorDark2` `#004172` | 10.50 ✓ |

**Orange moved off the fill and onto the edge** — border plus a new `KuwantimaAccentGlowPressed`
BoxShadow on press. That is where the colour philosophy always had it ("warm accent: checked/selected
**borders**"); using it as a text background was the anomaly, and it is the reason light text was
impossible before.

**The 4.53 at rest passes by 0.03.** Accepted deliberately: it is stock Fluent accent with white
text, the most standard pairing in Windows design, so rejecting it means rejecting Fluent's default.
But it is the weakest link in the ramp, and any future nudge to the accent colour breaks it first.

**13 sites, one key.** The re-ramp had to land on every accent-chip surface *before* the shared key
could flip, because the same key colours the checkmark `Stroke`, the radio-dot `Fill` and
selected-item `Foreground`: CheckBox ×4, RadioButton ×2, ComboBox ×2, ListBox, MenuToggleButton,
ToggleButton, Button, and the Slider thumb (that last one carries no text — re-ramped for ramp
consistency, not contrast).

**TabControl and Expander are NOT part of the accent family. Do not re-ramp them.** Their
selected/expanded state keeps a *pale* background with **dark** text and signals state with the
orange border only — they never consumed `AccentButtonForeground`. Darkening them to `Dark1` would
put dark text on navy at ~2:1. This is the one place where a mechanical find-and-replace across the
15 `SystemAccentColorLight3` sites would have broken the two controls that were not broken.

**A pre-existing failure those two also had, now fixed:** their `SystemBaseHighColor` foreground
**inverts per variant** while `Light3` does not, so selected+hover rendered near-white on pale blue
at **1.43:1**. v1.3.0 drops the override so they fall through to `KuwantimaControlHoverBrush`.

**And that brush was itself broken in Dark.** It was `#80D4DFFF` — a 50% *light* wash on a dark
ground, compositing to mid-grey (`#787e8e` over the region, `#848999` over glass) and taking even
primary text to **3.25:1**, on every hovered control in the library. v1.3.0 sets alpha to `0x33`,
matching the Light variant: primary **6.94**, secondary **5.11**.

**And the muted INK was broken too — a separate problem wearing the same costume.**
`SystemControlForegroundBaseMediumBrush` and `TextControlPlaceholderForeground` (one value, shared)
failed on hover surfaces in **both** variants at **every** alpha — Fluent's `#6a6a9e` / `#a0b4d0`
measured Light 3.51–4.13, Dark 3.52–4.68. No hover-brush change could fix it; lowering alpha only
trades one variant against the other. Real, not hypothetical: `TextBox`'s `PART_Placeholder` uses
that grey and sits *inside* `PART_BorderElement`, whose background becomes the hover brush, so
hovering an empty TextBox puts placeholder text on exactly the failing pair.

v1.3.0 overrides both — **`#55557F` Light, `#C8D4E8` Dark** — worst case now 5.25 / 4.98. Chosen by
sweeping candidates against every surface the ink lands on and taking the smallest step that clears
AA *with margin*, not the first that passes: `#5E5E8C` cleared Light by 0.05, and this repo has been
bitten twice by margins that thin. Deliberately **not** darkened further — the ink has to stay
distinguishable from primary or it stops reading as tertiary at all (Light keeps 2.12:1 from
MidnightBlue, Dark 1.39:1 from AliceBlue). Dark has less room because its worst surface is mid-tone
while primary is near-white, so AA margin and tonal separation trade directly against each other.

Measure before touching any of this: a translucent brush's contrast depends on its backdrop, so
composite it over the real surface rather than reading the hex.

**The trap this section exists to record.** In v1.2.0 the dead key's Black fallback *passed* AA on
all states (4.64 / 13.72 / 9). Migrating it to Fluent's white `AccentButtonForeground` made the
default crisp and silently dropped hover to 1.53 and press to 2.33 — a regression `Invariant_5` does
**not** catch, because the key resolves fine; it is just unreadable. **Resolution is not
readability.** Both that and the TabControl inversion were found by throwaway `[AvaloniaFact]`s that
measured every foreground against every state background *in both variants* — not by looking at the
default state, which looked fine in each case.

**Now enforced by `InvariantTests.Invariant_6`** (`InvariantTests.Contrast.cs`): every accent ink /
surface pair measured at ≥ 4.5:1 under *both* variants, resolved per variant rather than assumed to
carry across. A companion test parses every `Background`/`Fill` setter in the style files and fails if
the styles paint an accent surface the table does not measure — so a new surface cannot arrive
unmeasured, and reintroducing `SystemAccentColorLight3` as a fill turns the suite red. Verified to
bite by injecting that exact regression, not by assuming a green suite means a working test.

A second theory covers **composited hover surfaces**: each entry names its backdrop chain (page, then
glass panel) and the test composites source-over exactly as the renderer does, because a translucent
brush has no contrast of its own. It was deliberately held back until the muted ink was fixed —
adding it earlier would have meant either a red suite or hand-picking which inks it measured, and
selecting inks to keep a suite green is the weakening these rules exist to prevent. Once the ink
landed, every pair passed and no cherry-picking was needed.

## Avalonia Gotchas
Framework behaviours that are load-bearing for authoring styles here, and for *measuring* them.
Every one was **verified headlessly** rather than assumed, because reasoning got several backwards.
(Deliberately not numbered — a maintained count is the same liability as the control count, and it
had already gone stale once.)

- **Setters resolve by DOCUMENT ORDER, not selector specificity.** Unlike CSS. A later, *less*
  specific selector overrides an earlier, more specific one. This is why the GridSplitter's
  `:disabled` block sits at the bottom of its file — one plain `GridSplitter.Kuwantima:disabled`
  beats the resize cursor set by all six variant base styles above it. Move it up and it stops
  working, silently, while the markup still parses.
- **`:pointerover` does NOT fire on a disabled control** (unlike WPF's `IsMouseOver`). So hover and
  press styling is automatically dead when disabled — no need to neutralise it. It also means
  `Opacity` is often the *only* signal that a control is inert.
- **`:disabled` tracks `IsEffectivelyEnabled`, which inherits.** A control inside a disabled
  ancestor gets `:disabled` even though its own `IsEnabled` is still `true`. This is why disabled
  styling works when a consumer disables a whole pane — the realistic usage.
- **`.axaml` is compiled to IL and the raw markup is STRIPPED from the assembly.** The built
  Kuwantima package exposes exactly one avares asset: `!AvaloniaResourceXamlInfo`. You cannot
  `AssetLoader.Open()` a style file back at runtime. `Kuwantima.Tests` works around this by linking
  the sources in as `<EmbeddedResource>`.
- **Kuwantima is a resource-only assembly, and a `ProjectReference` does not LOAD one.** There is no
  C# in the library, so no consumer can reference a type from it, so nothing triggers the assembly
  load — the DLL is merely copied next to the binary. Avalonia 12.1.0 probed hard enough to resolve
  `avares://Kuwantima/...` anyway; **12.1.3 does not**, and the whole suite dies at session start with
  `XamlLoadException : No precompiled XAML found`. `TestApp.Initialize` fixes it with one
  `Assembly.Load("Kuwantima")` — a line that reads as dead code and is load-bearing, so it carries a
  DO-NOT-DELETE comment. Compiled XAML (`App.axaml`'s `<StyleInclude>`) references the assembly at
  load time and is unaffected, which is why this only ever hit the headless harness.

### Measuring a style is its own set of traps
The probes are as easy to get wrong as the styles, and a wrong probe is worse than none — it
produces a confident number. All three of these were hit in one session:

- **A translucent brush has NO contrast of its own. Only the blend does.** Reading the hex of
  `#80D4DFFF` tells you nothing; composite it source-over onto the surface it actually sits on
  (the glass panel is usually the worse backdrop, and the one most controls sit on). This is why a
  3.25:1 hover shipped unnoticed — as a colour literal it looks entirely reasonable.
- **Transitions poison a property read.** The base Button sets a 0.2s `BrushTransition` on
  `Background`/`BorderBrush`. Toggle a pseudo-class and read the property back and you get an
  *interpolated* value, not the style's target — a probe that silently measures a colour the design
  never specifies. Set `control.Transitions = null` before asserting.
- **Synthetic mouse events do not produce `:pointerover` on a non-foreground window.** `SetCursorPos`
  onto a control from a background process leaves it un-hovered, and `SetForegroundWindow` from a
  background process often fails silently. Screen-scraping a hover state this way produces frames
  that look like real renders and are not — mid-repaint captures that read as plausible bugs. Force
  the pseudo-class headlessly (`((IPseudoClasses)c.Classes).Add(":pointerover")`) and read the
  resolved values; use the sandbox for human judgement, not for automated measurement.

When you next have a question of this kind, **do not reason about it — probe it.** A throwaway
`[AvaloniaFact]` answers it in under a minute, and the harness already exists. Then check the probe
itself: if it disagrees with what you can see, suspect the probe before the framework.

### The 12.1.3 upgrade, and the shape of the mistake it produced
Worth keeping, because the evidence was consistent and pointed the wrong way — the same failure mode
as the control count, in a new costume.

All 135 tests failed on the bump, with an error naming the theme file. Bisect confirmed 12.1.0 green
/ 12.1.3 red. Conclusion drawn: *"12.1.3 is broken; the package would ship with nothing in it."*
Every step of that was sound except the last, which was **inference from an error message treated as
a verified fact**. The upgrade was nearly abandoned on it.

The one source never consulted was the one that could contradict it: **a real consumer.** Building
and running `Kuwantima.Sandbox` against 12.1.3 takes a minute and shows the library rendering
perfectly. The breakage was confined to the test harness the whole time.

**A red test suite tells you something is wrong, not what.** When the suspect is a dependency, run
the app before blaming the release — an all-red suite plus a plausible error message is exactly as
seductive, and exactly as unreliable, as four agreeing docs.

## Build Notes
- Running sandbox locks DLLs — close app before full rebuild
- `dotnet build --no-dependencies` + grep `error CS` to verify code when DLL locked
- `dotnet pack Kuwantima/Kuwantima.csproj -c Release` to build NuGet package
- `Avalonia.Headless.XUnit` is built against **xunit v3**. The v2 `xunit` package that
  `dotnet new xunit` scaffolds collides with it (`MemberDataAttribute` becomes ambiguous, CS0433).

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
- **Tier 2 `InvariantTests.Contrast.cs`** — `Invariant_6`, the accent contrast contract: every accent
  ink/surface pair ≥ 4.5:1, measured under *both* variants from the resolved colours. A companion
  test parses every `Background`/`Fill` setter and fails if the styles paint an accent surface the
  table does not measure, so nothing arrives unmeasured. **Resolution is not readability** —
  `Invariant_5` proves a key resolves and stays green while the result is invisible.

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
- Handouts: `docs/*.html` — standalone, NOT linked from the README (see Handouts above)
- Tests: `Kuwantima.Tests/` — see Testing above
- **`README.md` is shipped, not repo-internal.** `<PackageReadmeFile>` packs it, so it renders as
  the package's front page on nuget.org and in Visual Studio's package details. Write it for a
  consumer who has never seen the repo. It is the one file where a docs-only change reaches users.
