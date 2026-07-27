# V0.1 1v1 Match HUD and Card Interaction

Status: Implemented V0.1 contract; physical-iPhone review required

## Purpose and boundary

Issue #45 applies the [adaptive UI foundation](adaptive-ui-foundation.md) to the complete authoritative
1v1 match. The redesign makes the current decision, score, progress, canto state, resolved outcome, and
interaction result readable without adding rules, changing bot policy, moving the gameplay camera, or
exposing private cards.

`FirstPlayableFlowController` projects localized screen-space HUD text from `MatchState` and resolved
domain events. `FirstPlayableTablePresentation` continues to render only
`FirstPlayableTableSnapshot`, preserve opponent-hand privacy, and submit the same shared interaction
intents. Neither presentation class calculates captures, cantos, scoring, dealer rotation, ties, or
victory.

## Information hierarchy

The match keeps the 3D table as the dominant layer and uses three compact screen-space surfaces:

1. the top score plaque identifies both players, both scores, and the 24-point objective;
2. the status card shows phase, round, deal, final-deal/tie-extension state, dealer, active player,
   public canto announcements, the current resolved outcome, and the local action result;
3. contextual dealer and canto controls appear only beside the local decision and contain only legal
   intents supplied by the orchestrator.

The status card does not restore the former persistent match header, action column, settings controls,
or bottom explanation strip. Presentation settings remain on Home; Skip animation remains contextual
while a resolved batch is busy.

## Authored match compositions

Desktop, mobile portrait, and mobile landscape use the same authoritative snapshot but different
presentation priorities.

### Desktop

- Preserve the accepted authored table, card footprint, camera, and four required desktop layouts.
- Keep the score centered above play, the status card at the table edge, and Return to Home floating
  at the upper-right.
- Keep mouse and keyboard on the same inspect, select, confirm, and cancel intent path.

### Mobile portrait

- Keep the long axis for the table, local hand, and local decision rather than decorative separation.
- Enlarge the local actionable hand above public table cards; keep secondary deck, captured pile, and
  hidden-hand representations subordinate.
- Place the full-width status/action surface after the table and local decision instead of scaling the
  desktop edge card over the play space. Open contextual menus back toward the safe centre so they do
  not extend through the unsafe side.

### Mobile landscape

- Reduce the overall table root for the short axis, then enlarge local, public, dealer-selection, and
  character identity independently instead of shrinking the desktop composition.
- Increase hand, public-table, and dealer-spread spacing with their card sizes.
- Keep floating score, status, Home, and contextual controls inside hardware side insets.

Every card retains the exact `63:88` aspect ratio. Automated projected-size checks enforce at least
`72 pt` local actionable card width, `48 pt` public table-card width, and `44 pt` dealer-selection
touch width in both phone orientations. Character presentation is enlarged from the desktop
placeholder scale in both phone profiles.

## HUD and outcome vocabulary

The HUD renders only public or local-authorized facts:

- score, target, round, deal, standard/final/tie-extension state
- dealer and active player
- public canto claimant and canto name, never the qualifying hand
- localized resolved-event text

Resolved outcomes combine text with a stable boundary/value treatment:

| Outcome | Required visible text |
| --- | --- |
| Capture | player and captured-card count |
| Cascade | explicit `Cascade` label plus player and count |
| Fall | `Fall`, awarded points, and resulting total |
| Clean table | `Clean table`, awarded points, and resulting total |
| Canto | claimant, canto name, and announced/scored/non-scoring/false result |
| Other score | score reason, signed change, and resulting total |
| Tie extension | explicit tie-extension label and tied score |
| Victory | explicit completed-match winner |

Outcome classes supplement this text; color never carries the result alone.

## Interaction feedback

All local card input paths retain the shared `Inspect -> Select -> Confirm -> Play` sequence and
explicit Cancel. Each reversible or terminal interaction result has its own symbol, localized text,
card treatment where applicable, and semantic HUD class:

| State | Symbol | Presentation |
| --- | --- | --- |
| Legal | `+` | available card and legal-action instruction |
| Inspected | `?` | raised inspected card and inspection instruction |
| Selected | `◆` | enlarged selected card and minimum selected boundary |
| Confirmed | `✓` / `OK` | accepted-play copy and confirmed card cue |
| Cancelled | `↶` | cancellation copy; selection clears without a play |
| Rejected | `×` / `X` | explicit rejection reason |
| Temporarily blocked | `Ⅱ` / `||` | busy copy; valid selection remains retained |

Touch inspection remains a hold and touch selection remains a tap. One touch never selects and
confirms a card in the same physical press. Mouse and keyboard retain right-click/`I` inspection,
click/`E` selection, Enter/Space confirmation, and Escape cancellation.

## Recomposition, privacy, and authority

Rotation or resize rebuilds transient geometry and profile-specific HUD layout only. It preserves:

- the `FirstPlayableFlow`, match orchestrator, `MatchState`, and trace
- selected and inspected cards, interaction revision, and intent history
- active animation prefix and eventual authoritative synchronization
- the saved fixed-camera transform and field of view
- opaque opponent hands and face-down dealer-spread identities

Public table cards, captured counts, score, canto claims, and rendered outcomes continue to agree
with the authoritative snapshot. Presentation-only size and spacing multipliers never enter domain or
application state.

## Validation ownership

- Edit Mode covers the distinct inspected/cancelled feedback vocabulary, semantic classes, shared
  input sequence, cancellation, rejection, blocking, and unchanged deterministic state.
- Play Mode covers both phone profiles, projected card/touch minimums, desktop regressions, fixed
  camera, selection and animation recomposition, intent/trace preservation, hidden-hand privacy, HUD
  content, and every critical outcome class.
- Complete Edit Mode and Play Mode suites remain the automated gate.
- The recorded physical iPhone remains the acceptance authority for viewing distance, physical touch
  comfort, hardware safe areas, and sensor-driven portrait/landscape rotation. Simulator or projected
  measurements support that review but do not replace it.

Related: [first-playable table](../technical/first-playable-table.md),
[card interaction prototype](card-interaction-prototype.md),
[V0.1 milestone](../planning/v0.1-1v1-playtest-milestone.md), and
[validation baseline](../development/validation.md).
