# First-Playable 1v1 Table Presentation

Status: Implemented first-playable presentation contract

## Purpose

Issue #25 integrates the complete authoritative 1v1 match into the existing first-playable `Home` scene. The table is visible during Match and Result while the application flow continues to own setup, loading, replay, and return-to-Home lifecycle.

The integration promotes the fixed-table, card-interaction, generated-table, and modular-card prototypes into one full-match presentation without promoting their prototype art to production fidelity.

## Authority boundary

`FirstPlayableTablePresentation` reads the `FirstPlayableFlow.Match` orchestrator already composed by Bootstrap. It never creates a second match, invokes `OneVersusOneRules`, evaluates captures, validates cantos, awards points, rotates the dealer, or decides victory.

`FirstPlayableTableSnapshot` projects each immutable `MatchState` into rendering data:

- local hand identities, public table cards, and both public captured piles
- opponent-hand count without opponent card identities
- face-down dealer-spread count without hidden card identities
- deck count, scores, dealer, active seat, round, deal, final-deal, tie-extension, canto, phase, and winner state

The HUD also maps the latest ordered domain event into localized semantic text. This communicates the resolved outcome while leaving all calculations in the domain.

## Composition

- the gameplay camera remains fixed at `(0, 7.2, -5.4)`, rotation `(52, 0, 0)`, and `44°` vertical field of view
- the local player remains at the bottom and the bot remains at the top
- `RoundCardTable` is the table asset
- every visible face uses the forty-card `CardVisualCatalog` and shared atlas material
- opponent hands and dealer-selection cards use the direction-neutral back
- captured cards remain in separate owner piles
- inexpensive generated primitive upper bodies preserve identity and active/dealer cues
- the review-only high-resolution `WarmChallenger` asset is not referenced
- active turn combines a brass ring and `>` marker; dealer combines a diamond token and `D` marker

The UI leaves a transparent central table area and keeps scores, dealer, active turn, canto, round/deal state, latest resolved event, available non-card actions, and interaction feedback in screen-space panels.

## Interaction

The integrated local hand reuses `CardInteractionSession` and `CardInteractionInputAdapter`:

1. inspect
2. select
3. confirm
4. play
5. cancel

Gameplay actions use the object or player context that owns the decision instead of a persistent
available-actions panel:

- dealer selection spreads every remaining face-down dealer card across the table; activating one
  submits the corresponding authoritative `SelectDealerCardIntent` without exposing its identity
- a local hand card is selected in place; activating the selected card again confirms and plays it,
  while keyboard confirm and cancel retain the same semantics
- an optional canto control appears beside the local player only while canto intents are legal; it
  opens the authoritative list of available announcements, and playing a card without using it
  declines the opportunity naturally
- a mandatory dealer-options control appears beside the local player when the human is the dealer;
  its menu opens immediately and offers the four authoritative hands/table ordering and opening-pattern
  combinations

The dealer-options and canto menus are transient contextual popovers. They do not reserve a permanent
screen column, and they contain only intents supplied by the orchestrator.

The interaction session now accepts application delegates as well as a direct `MatchSession`. The integrated delegate submits the confirmed `PlayCardIntent` through `FirstPlayableFlow.TrySubmitHumanIntent`, so automatic bot turns, trace recording, result transition, and authoritative rejection stay on the existing application path.

Legal, selected, confirmed, rejected, and temporarily blocked states retain the documented shape/text symbols and localized feedback. Dealer selection, dealer setup, and canto announcements remain localized UI actions because they are not card-hand interactions.

## Recomposition contract

Viewport or safe-area changes rebuild only transient view objects. The `FirstPlayableFlow`, orchestrator, `MatchState`, match trace, `CardInteractionSession`, selected card, interaction revision, and pending intent history remain unchanged.

Automated Play Mode coverage exercises `1280 x 720`, `1440 x 900`, `1920 x 1080`, and `2560 x 1440` while a card is selected. It also verifies that temporary blocking retains the selection and that a later confirmation records exactly one human play.

## Generation and validation

Use:

- `The Fall > First Playable Table > Generate`
- `The Fall > First Playable Table > Validate`

The generator updates the existing Home flow assets, binds the fixed camera, `RoundCardTable`, and `CardVisualCatalog`, then saves the scene through Unity serialization.

Focused Edit Mode coverage verifies privacy-safe state projection and complete-match snapshot agreement. Focused Play Mode coverage verifies full-match visual/state agreement, private hands, exact public card collections, interaction semantics, fixed camera, and resizing safety.

Validated on 2026-07-20 with Unity `6000.5.4f1`:

- first-playable table generation and structural validation: passed
- complete Edit Mode suite: 63/63 passed
- complete Play Mode suite: 16/16 passed
- macOS universal development-player smoke build: passed
- offscreen `1440 x 900` dealer-selection, dealer-options, and canto/selection captures: reviewed
- built-player manual visual inspection: skipped because the desktop session was locked; issue #28 retains the full manual acceptance and performance matrix

Production animation, VFX, timing, interruption sequencing, and audio remain owned by issues #26 and #27. This issue renders resolved state and semantic events immediately; it does not promote the AnimationLab sequencer into the complete match.

Related: [first-playable application flow](first-playable-flow.md), [match orchestration](match-orchestration.md), [fixed table composition](../design/table-composition-prototype.md), [card interaction](../design/card-interaction-prototype.md), and [modular card visuals](../assets/card-visual-pipeline.md).
