# Validation error styling — plan

**Status: Kuwantima-side DONE as of v1.7.0 (2026-10-03). BBService-side not started.** Opened
2026-10-02, driven by a BBService need — see "Downstream: BBService" in CLAUDE.md for the shipped
writeup.

## The problem (in BBService, not Kuwantima)

BBService's onboarding forms (six workflows, each 10-20 bound fields across several
`Border Classes="KuwantimaGlass"` sections in a `ScrollViewer`) gate their Save button with a
`CanSave()` boolean built from a long `&&` chain of per-field conditions, mirrored by a
`[NotifyCanExecuteChangedFor(nameof(SaveCommand))]` attribute on nearly every `[ObservableProperty]`
just to keep that chain live as the user types. The result: Save silently stays disabled with zero
indication of *which* of the ~15 conditions is unmet, which is actively confusing once the required
fields are spread across several scrolled sections — exactly the complaint that started this plan.

## The direction

Flip it: **Save stays enabled**; clicking it with missing data **highlights the specific missing
fields** (red border + message) rather than refusing to do anything. Standard "submit reveals
validation" pattern, and it scales to many-required-field forms far better than a disabled button —
a click now tells you exactly what's wrong and where, instead of nothing.

## Split of responsibility

Kuwantima ships **zero C#** today (see CLAUDE.md's "Who this is actually for" / TargetFramework
notes) — that's deliberate, not an oversight, and this plan doesn't change it. BBService's own
CLAUDE.md already draws this exact line for its custom window chrome ("the library itself ships zero
C#, so this behavior lives here"). Same split applies here:

### Kuwantima's part — styling only, no new C#

Avalonia renders validation errors natively: a binding that fails `INotifyDataErrorInfo`/
`ObservableValidator` validation sets `DataValidationErrors.HasErrors="True"` on the control, which
drives Avalonia's built-in `:error` pseudo-class. Kuwantima needs a visual treatment for that state on
its own input controls — this is a **New Theme Resource** + **New Variant** addition, following the
checklists already in CLAUDE.md:

1. **DONE (v1.7.0).** `KuwantimaValidationErrorBrush`, Light + Dark, in `KuwantimaThemeResources.axaml`
   — reuses `KuwantimaWarningTextBrush`'s exact hex under the new border-role key (a deliberate,
   bounded exception in CLAUDE.md's Color Philosophy, not a new hue). Border-only, so it's excluded
   from `Invariant_6`'s mandatory contrast sweep by design (that test only fires on `Background`/
   `Fill` setters) — no separate text-color key was needed.
2. **DONE (v1.7.0).** One plain `^:error` selector added to `KuwantimaTextBox.axaml` and
   `KuwantimaComboBox.axaml` each (border color only, no adornment) — placed after `:focus`/
   `:focus-visible` and before `:disabled`, so document order alone makes it win over focus and lose
   to disabled. Guarded by a new permanent test, `InvariantTests.Validation.cs`'s `Invariant_8`.
3. **Open question to resolve before writing any style**: does Fluent's default theme (which
   `KuwantimaPrimaryTheme.axaml` builds on) already supply baseline `:error` visuals that Kuwantima
   would be *overriding* rather than introducing fresh? Check this first — it changes whether step 1
   is a new key or an override of an existing Fluent one (see README's "Fluent keys Kuwantima
   overrides" table for that distinction).

   **Partial pass, 2026-10-02 — inconclusive, do not treat as the answer.** A static string-scan of
   the installed `Avalonia.Themes.Fluent` 12.1.3 DLL (no source `.axaml` ships in the NuGet package,
   no decompiler available, so this was a DLL string scan, not a read of real selectors) found:
   - `SystemErrorTextColor` — a real Fluent key (backs `ColorPaletteResources.ErrorText`). If it
     resolves and reads right, step 1 may be an *override* of this rather than a fresh
     `KuwantimaValidationErrorBrush` — check the README override-table precedent either way.
   - `TooltipDataValidationErrors` — a boolean flag `Avalonia.Controls.DataValidationErrors` reads to
     pick tooltip-style vs. inline error display. Matters for the sandbox demo: need to know whether
     Fluent pops its own tooltip alongside whatever border treatment gets added, before deciding what
     the demo should show.
   - No `:error` pseudo-class selector, `ErrorBrush`, or `ErrorTemplate` turned up anywhere in the
     assembly, and **no `AutoCompleteBox` theming exists in Fluent at all** — suggestive that
     TextBox/ComboBox get no automatic red border from Fluent, but a string scan proving an absence is
     exactly the kind of probe this repo's own Avalonia Gotchas section warns about trusting
     ("a probe that cannot fail proves nothing").
   - **Resolved, 2026-10-03** — ran the throwaway `[AvaloniaFact]` queued above: built
     `TextBox.Kuwantima` in a headless `Window` under both variants, read
     `Background`/`BorderBrush`/`Foreground` at rest, forced `:error` via
     `((IPseudoClasses)control.Classes).Add(":error")`, read them again. All three were
     **byte-identical rest vs. error, in both Light and Dark** — Fluent supplies no baseline
     `:error` treatment on this control at all. The DLL scan's suggestion ("TextBox/ComboBox get
     no automatic red border from Fluent") is now confirmed by reading the actual resolved value,
     not inferred from a string that didn't turn up. Decides the open question: step 1 is a
     **new** resource key, not an override of an existing Fluent one — there is nothing to
     override. Probe deleted after use, per the throwaway convention.
4. **DONE (v1.7.0).** `InputsPage`'s new "Validation" section binds a real
   `DataValidationErrors.Errors` attached property (via `MainWindowViewModel.ValidationDemoErrors`,
   toggled by a "Show Validation Error" button) — exercises the actual mechanism BBService's forms
   will drive, not a forced pseudo-class. Confirmed visually in both Light and Dark via the running
   sandbox. **Bonus finding**: Fluent renders an inline "This field is required." message
   automatically under an errored control with no styling from this repo — suggests
   `TooltipDataValidationErrors`'s default is inline text, not a popup, at least on this path. Still
   open and non-blocking: whether a tooltip path exists elsewhere, and whether
   `KuwantimaToolTip.axaml`'s bare `ToolTip` selector would catch it if so (see CLAUDE.md's shipped
   writeup for the exact wording).
5. **DONE (v1.7.0).** Documents page and README Controls table both updated. Confirmed a variant,
   not a new control — control count stays 16.

### BBService's part — validation logic + wiring, consumes the new style

1. **Prototype on one workflow first** — `MigrateLegacyPropertyViewModel` (the most complex of the
   six, most recently touched, and the one most likely to expose problems with the approach before
   it's propagated everywhere else the same way the FeatureType/CustomMakeName pattern was this
   session).
2. Migrate the ViewModel from `ObservableObject` to CommunityToolkit.Mvvm's `ObservableValidator` — a
   drop-in base class in the same package (already a dependency), supporting `[Required]`/custom
   `ValidationAttribute`s plus `INotifyDataErrorInfo`.
3. Replace the `CanSave()` chain + `[NotifyCanExecuteChangedFor(nameof(SaveCommand))]` sprawl: `Save`
   stays executable whenever `!IsSaving && !IsSaved`; `SaveAsync()` calls `ValidateAllProperties()` at
   the top and bails out (no save attempt) if `HasErrors` — the now-styled fields self-highlight via
   their own bindings, so no separate per-field message list is needed in the view model.
4. **Known wrinkle, needs a spike**: most required fields here are `SelectedXxx` properties bound to
   an EF entity reference (a `ComboBox`'s `SelectedItem`), not a string/primitive — `[Required]`
   DataAnnotations work fine for strings but entity references need confirming they validate the same
   way, or a small custom attribute / manual `ValidateProperty` call in the property's partial
   `OnXxxChanged` if not.
5. **Known wrinkle, needs a spike**: per-season validation. `Seasons` is an
   `ObservableCollection<SeasonEntry>`, and each `SeasonEntry` needs its own service-interval/rate-type
   pick — `NewFeatureOnExistingPropertyViewModel.cs` already has a comment flagging this stays a
   click-time check specifically because wiring `PropertyChanged` on every row didn't seem worth it
   under the old disabled-button approach. Under the new approach, each `SeasonEntry` likely needs to
   become its own small `ObservableValidator`, with the parent aggregating "does any row have errors."
6. Once the prototype's approach is confirmed workable, propagate to the other five workflows the same
   incremental way this session propagated the FeatureType/CustomMakeName split from workflow 3 to
   workflow 6 — one workflow at a time, not a single mechanical find-replace across all six.

## Why this is split across two repos instead of done entirely in BBService

The naive alternative — hand-roll per-field `Classes="error"` bindings and ad-hoc red borders directly
in BBService's `.axaml` files — works, but produces an inconsistent, one-off visual treatment per
form instead of a single reusable "this is what an invalid Kuwantima field looks like" answer, and
BBService isn't the only possible consumer of that answer (Tunatya already shares Kuwantima's base
styles). Doing it once in Kuwantima, styling-only, keeps the zero-C# invariant intact and gives every
current and future Kuwantima consumer the same answer for free.
