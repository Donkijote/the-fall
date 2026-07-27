# V0.1 Adaptive UI Foundation

Status: Implemented V0.1 foundation

## Purpose and boundary

Issue #43 establishes the shared layout language, measurable size expectations, safe-area contract,
and reusable UI states used by the later Home/setup/result and match-HUD redesigns. It does not
finish those screen redesigns, change rules or bot behavior, select production platform floors, or
claim accessibility certification.

Desktop, mobile portrait, and mobile landscape are authored compositions selected from the runtime
platform and orientation. UI Toolkit panel width is not used as a proxy for a phone because the
panel's `1920 x 1080` reference scaling can make a physical phone report desktop-like logical width.
The selected profile and current safe rectangle can change without replacing the application flow,
match, selected card, intent history, or fixed gameplay camera.

## Evidence baseline

The audit baseline is `bdaf339`, immediately before issue #43. The player-facing UI and table code at
that commit is unchanged from physical-iPhone candidate `aae863a74c7392af80b1a41207b809d611e6b791`;
the only intervening runtime change tuned animation configuration.

Evidence used:

- the accepted macOS issue #28 review at `1280 x 720`, `1440 x 900`, `1920 x 1080`, and
  `2560 x 1440`
- the issue #31 touch-only Home, setup, match, result, safe-area, portrait, landscape, and active
  rotation review on the recorded iPhone 17 Pro
- the retained `1440 x 900` table and contextual-action captures in the ignored `Logs/` workspace
- matching iPhone-simulator captures created by issue #43 for reproducible composition comparison;
  simulator images support visual comparison and are not physical-device validation

Issue #31 proved containment, touch flow, privacy, and authoritative-state agreement. Owner review
also found the experience defect that starts V0.1: content inherited desktop scale and became too
small even though it remained inside the physical phone's safe area.

## Baseline audit

Every required screen and layout is covered below. `Pass` means the first-playable control remains
operable, not that its visual hierarchy is accepted for V0.1.

| Layout | Home | Setup | Match | Result |
| --- | --- | --- | --- | --- |
| Desktop `1280 x 720` | Pass; title and primary action read, but secondary copy and prompt are near the lower readable bound | Pass; two options fit, but descriptions and keyboard prompt are dense | Pass; centered score, compact status card, and floating exit preserve the minimum table area | Pass; winner and actions read, but prompt is visually weak |
| Desktop `1440 x 900` | Pass; hierarchy is clear | Pass; option grouping is clear | Pass; table dominates while the score and current decision remain immediately readable | Pass; clear outcome and actions |
| Desktop `1920 x 1080` | Pass; balanced two-column composition | Pass | Pass; presentation settings remain in Home and no match header competes with the table | Pass |
| Desktop `2560 x 1440` | Pass; fixed maximum widths leave deliberate but excessive unused space | Pass; panel does not use the additional space to strengthen rule comparison | Pass; the `1500`-unit shell cap leaves unused space instead of improving card or character identity | Pass; result panel remains readable but visually undersized for the viewport |
| Recorded iPhone portrait | Operable; desktop reference scaling makes title, copy, and action physically small and leaves excess vertical space | Operable; desktop option rows and prompts are too small for comfortable reading/touch | Operable; score, compact status, context controls, and table retain separate priority without persistent top or bottom bars | Operable; winner reads before supporting score/prompt, but controls and copy are undersized |
| Recorded iPhone landscape | Operable; desktop row fits but uses phone height inefficiently | Operable; copy, toggles, and buttons are small while horizontal space is underused | Operable; safe-area containment passes with floating score, exit, and status overlays preserving the short axis | Operable; primary result survives, supporting copy and controls remain small |

Cross-screen failures:

- UI safe-area use was implicit: the 3D table normalized `Screen.safeArea`, while UI Toolkit stages
  only applied percentage padding.
- The previous `compact` class depended on resolved panel width below `900`; reference-resolution
  scaling prevented that from reliably identifying a physical phone.
- Most buttons had a `50`-unit minimum and presentation toggles a `38`-unit minimum before panel
  scaling, so physical touch comfort was not protected.
- Focus used a color/value change but no stable minimum stroke. Semantic card feedback had text
  symbols, but reusable UI components had no shared legal/selected/confirmed/rejected/blocked class.
- Large desktop viewports capped the shell rather than using extra space to improve card,
  character, or decision hierarchy.

## Measurable V0.1 minimums

These are implementation and review thresholds for the recorded phone and accepted desktop layouts.
They are not universal accessibility thresholds or production support commitments.

| Element | Desktop minimum | Phone minimum | Measurement rule |
| --- | ---: | ---: | --- |
| Essential text: objective, next action, score, result | `16 px` | `16 pt` | rendered cap/x-height remains distinguishable at normal viewing distance |
| Secondary text: rule explanation, round/deal, prompts | `14 px` | `14 pt` | may wrap; never clip or become the only carrier of a required state |
| Mouse/keyboard control target | `44 x 44 px` | — | includes the focusable hit rectangle |
| Touch target | — | `44 x 44 pt` | targets may be visually smaller only when the full hit rectangle remains this size and targets keep `8 pt` separation |
| Local actionable card identity | `64 px` wide | `72 pt` wide | rank and suit identify the card without inspection |
| Public table-card identity | `48 px` wide | `48 pt` wide | rank remains identifiable; inspection can provide detail but cannot be required for the next action |
| Character identity | `64 px` head height | `64 pt` head height | head/shoulder silhouette and active/dealer cue remain distinct |
| Keyboard focus / selected stroke | `3 px` | `3 pt` | combined with value/shape or text; never color-only |
| Safe-area breathing room | `16 px` from window content edge | `12 pt` inside the hardware safe rectangle | additional profile padding is applied after hardware insets |

If a layout cannot satisfy every minimum, preserve the local hand and actionable controls first,
table state and score second, contextual explanation third, and decorative content last. Do not
reduce all four layers uniformly.

## Authored composition rules

### Desktop

- Keep Home as a two-region hero and primary-action composition.
- Keep setup and result in a centered, bounded reading panel; use surplus width for comparison and
  hierarchy rather than longer line length.
- Keep a compact match-status card at the table edge and the score centered above play. The table owns
  the largest region; local hand and contextual action remain visually attached to the table. Do not
  restore a persistent match header or bottom explanation strip.
- Mouse and keyboard share the same visible action. Focus remains visible after pointer movement and
  pseudo-localization wrapping.

### Mobile portrait

- Compose vertically: identity/status summary, table state, local hand/action, then secondary
  explanation. The local decision remains reachable above the bottom safe inset.
- Use the long axis for separation, not for enlarging decorative gaps.
- Present score and next action before round/deal explanation. Context menus expand into a
  phone-width surface with touch rows rather than a scaled desktop popover.
- Settings and non-urgent presentation controls remain on Home; they do not consume match space.

### Mobile landscape

- Compose for the short vertical axis: compact floating status, dominant table/local-hand region, and
  contextual actions only.
- Use horizontal space for status and contextual choices. Do not stack the portrait composition or
  shrink the desktop header.
- Hardware side insets are applied before profile gutters. Context popovers open away from the
  unsafe edge and retain full touch rows.

## Safe-area and state-preservation contract

`AdaptiveUiFoundation.Resolve` selects one of `profile-desktop`, `profile-mobile-portrait`, or
`profile-mobile-landscape`. `FirstPlayableFlowController` maps the normalized hardware safe
rectangle into current UI Toolkit panel units and applies it to every stage edge. USS then adds
profile-specific internal padding.

Rotation or resize may change the selected profile and rebuild transient presentation, but must not:

- restart or replace `FirstPlayableFlow` or its match orchestrator
- submit, confirm, cancel, or duplicate an intent
- clear a valid selected card or interaction revision
- reveal hidden cards
- move the fixed gameplay camera

## Reusable UI tokens and components

The runtime constants in `AdaptiveUiFoundation` own the measurable minimums. `HomeScreen.uss` owns the
visual token application for the current prototype palette.

Stable component classes:

- `.panel`: calm layered surface with visible boundary
- `.primary-button`, `.secondary-button`, `.quiet-button`: action hierarchy
- `.rule-toggle`, `.presentation-toggle`: keyboard-, mouse-, and touch-focusable toggles
- `.context-icon`, `.context-action-button`: table-attached decision entry and expanded choices
- `.match-status`: compact next-action and semantic-feedback surface

Stable state classes:

| State | Required channels |
| --- | --- |
| `.semantic-legal` | legal symbol/text plus moss boundary |
| `.semantic-selected` | selection symbol/text plus minimum brass stroke |
| `.semantic-confirmed` | confirmation text/icon plus woad boundary |
| `.semantic-rejected` | rejection text/icon plus madder boundary |
| `.semantic-blocked` | blocked text/icon plus neutral boundary and reduced value |

Buttons and toggles use a minimum three-unit focus stroke. Hover may supplement focus but never owns
the only visible feedback. Disabled state changes value in addition to retained text.

## Comparison captures

The committed captures are generated from the matching iPhone simulator so later issues can compare
the three authored compositions without depending on a connected phone:

- `adaptive-ui-captures/home-mobile-portrait.png`
- `adaptive-ui-captures/home-mobile-landscape.jpg`
- `adaptive-ui-captures/home-desktop-1440x900.jpg`

| Desktop `1440 x 900` | Mobile portrait | Mobile landscape |
| --- | --- | --- |
| ![Desktop Home composition](adaptive-ui-captures/home-desktop-1440x900.jpg) | ![Mobile portrait Home composition](adaptive-ui-captures/home-mobile-portrait.png) | ![Mobile landscape Home composition](adaptive-ui-captures/home-mobile-landscape.jpg) |

Physical-device acceptance still requires the recorded iPhone. A simulator capture cannot close
touch, safe-area hardware, viewing-distance, rotation-sensor, or physical readability checks.

## Validation ownership

- Edit Mode verifies all four desktop layouts plus both phone orientations resolve to the intended
  profile, safe rectangles map correctly, one profile/state class remains active, and minimum tokens
  cannot silently regress.
- Play Mode verifies portrait-to-landscape recomposition applies safe insets without changing the
  current flow stage. Existing table coverage continues to protect selected card, interaction
  revision/history, match instance, authoritative state, trace, and fixed camera.
- Later issues #44 and #45 may refine screen-specific structure, but they must consume these
  profiles, minimums, state classes, and priority rules rather than adding another breakpoint model.

Related: [V0.1 milestone](../planning/v0.1-1v1-playtest-milestone.md),
[experience design](experience.md), [first-playable flow](../technical/first-playable-flow.md),
[platform requirements](../technical/platforms.md), and
[validation baseline](../development/validation.md).
