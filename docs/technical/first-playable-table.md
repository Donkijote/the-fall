# First-Playable 1v1 Table Presentation

Status: Implemented first-playable presentation and animation contract

## Purpose

Issue #25 integrates the complete authoritative 1v1 match into the existing first-playable `Home` scene.
Issue #26 promotes the reusable AnimationLab beats into that table, and issue #27 maps those resolved beats
to functional prototype audio. The table is visible during Match and Result while the application flow
continues to own setup, loading, replay, and return-to-Home lifecycle.

The integration promotes the fixed-table, card-interaction, generated-table, and modular-card prototypes into one full-match presentation without promoting their prototype art to production fidelity.

## Authority boundary

`FirstPlayableTablePresentation` reads the `FirstPlayableFlow.Match` orchestrator already composed by Bootstrap. It never creates a second match, invokes `OneVersusOneRules`, evaluates captures, validates cantos, awards points, rotates the dealer, or decides victory. Its animation player consumes only immutable startup events and accepted resolution records.

`FirstPlayableTableSnapshot` projects each immutable `MatchState` into rendering data:

- local hand identities, public table cards, and both public captured piles
- opponent-hand count without opponent card identities
- face-down dealer-spread count without hidden card identities
- deck count, scores, dealer, active seat, round, deal, final-deal, tie-extension, canto, phase, and winner state

During presentation, the same projection can be built from an `AnimationPresentationState` prefix plus the authoritative reference state. The prefix exposes only facts explained by completed/current beats and preserves the snapshot's hidden-information boundary. On normal completion or any early exit, the table is rebuilt from the exact accepted `MatchState`.

The HUD maps the active ordered domain event into localized semantic text. This communicates each resolved outcome while leaving all calculations in the domain.

## Edit Mode authoring

`Home.unity` contains an active `First Playable Table Authoring` hierarchy that is visible without
entering Play Mode. Open it with `The Fall > First Playable Table > Open Authoring Layout`, or select it
directly in the Home hierarchy. The saved scene is the runtime composition source of truth:

- edit the Main Camera transform and field of view directly;
- move, rotate, or scale `RoundCardTable — Edit And Save`;
- edit the environment and either player object directly;
- move or rotate the named card-zone anchors to relocate hands, table cards, deck, captures, or dealer spread;
- scale the X axis of `Card Size Reference — Scale X Only` to resize every card while the layout component
  preserves the `63:88` ratio and synchronizes the other representative cards.

Save `Home.unity` normally after editing. On entering Play Mode, the authoring hierarchy is hidden and the
presentation clones its saved environment, table, and player objects, then creates authoritative match cards
under the saved anchors. Rerunning the generator preserves an existing authored layout and camera instead of
resetting manual changes.

## Composition

- the authored gameplay camera initially uses `(0, 8.6, -2.35)`, rotation `(74, 0, 0)`, and `36°` vertical field of view; runtime never overwrites the saved camera pose or field of view
- the local player remains at the bottom and the bot remains at the top
- `RoundCardTable` is the table asset, widened uniformly across its surface to a `2.10 m` gameplay diameter while retaining its authored height
- every visible face uses the forty-card `CardVisualCatalog` and shared atlas material
- opponent hands and dealer-selection cards use the direction-neutral back
- captured cards remain face down in separate owner piles
- public table cards retain their presentation slots when another card is captured; capture and
  cascade removal never compact or rearrange the surviving table
- compact generated head-and-shoulder placeholders preserve identity and active/dealer cues without competing with the table
- the review-only high-resolution `WarmChallenger` asset is not referenced
- active turn combines a brass ring and `>` marker; dealer combines a diamond token and `D` marker

The UI leaves a transparent central table area and keeps scores, dealer, active turn, canto, round/deal
state, latest resolved event, contextual non-card decisions, and interaction feedback in screen-space
elements.

Every gameplay card uses one consistent footprint with the source art's exact `63:88` aspect ratio.
Hands, table cards, deck cards, captured cards, and the dealer-selection spread distinguish their zones
through position and overlap instead of changing card size or stretching the artwork. This keeps rank and
suit scale predictable at the required desktop resolutions while preserving clear ownership zones.
For perspective testing, face-up cards use a deliberately minimal atlas treatment with a dominant rank and
one suit marker. Every face is oriented toward the local seat so the single rank reads upright; detailed
pip and court art remains deferred to the dedicated card-design pass.

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

After one intent is accepted, both the flow controller and card-interaction session remain blocked until all human and automatic-bot resolution records in that batch have been presented. Card selection is retained, but another card confirmation, dealer selection, dealer option, or canto cannot be accepted out of order. Fast-forward and reduced-motion toggles may change the active transport without changing the match; Skip synchronizes immediately. Interruption, cancellation, leaving Home, disable, and teardown also synchronize before transient views are released.

Legal, selected, confirmed, rejected, and temporarily blocked states retain the documented shape/text symbols and localized feedback. Dealer selection, dealer setup, and canto announcements remain localized UI actions because they are not card-hand interactions.

## Recomposition contract

Viewport or safe-area changes rebuild only transient view objects. The `FirstPlayableFlow`, orchestrator, `MatchState`, match trace, `CardInteractionSession`, selected card, interaction revision, and pending intent history remain unchanged.

Automated Play Mode coverage exercises `1280 x 720`, `1440 x 900`, `1920 x 1080`, and `2560 x 1440` while a card is selected. It also verifies that temporary blocking retains the selection and that a later confirmation records exactly one human play.

## Generation and validation

Use:

- `The Fall > First Playable Table > Generate`
- `The Fall > First Playable Table > Validate`

The generator updates the existing Home flow assets, creates the persistent authoring hierarchy when it is
missing, binds the authored camera, `RoundCardTable`, layout, `CardVisualCatalog`, and versioned animation preset, then saves the scene
through Unity serialization. Existing layout transforms and camera framing are preserved on subsequent runs.

Focused Edit Mode coverage verifies privacy-safe state projection, complete-match snapshot agreement, complete event-vocabulary mapping, and batch-by-batch animation convergence. Focused Play Mode coverage verifies full-match visual/state agreement, private hands, exact public card collections, interaction semantics, fixed camera, resizing safety, duplicate-input blocking, presentation controls, and every completion/early-exit synchronization path.

Validated on 2026-07-20 with Unity `6000.5.4f1`:

- first-playable table generation and structural validation: passed
- complete Edit Mode suite: 64/64 passed
- complete Play Mode suite: 16/16 passed
- macOS universal development-player smoke build: passed
- offscreen `1440 x 900` dealer-selection, dealer-options, canto/selection, and overhead-composition captures: reviewed
- built-player manual visual inspection: skipped because the desktop session was locked; issue #28 retains the full manual acceptance and performance matrix

Production VFX and production audio remain outside this contract. Functional project-owned procedural
cues now distinguish the required first-playable actions and outcomes, use independent master/effects/music
controls, and stop on every early-exit or session boundary. The implemented animation and audio remain
presentation-only and use the same resolved beat stream without changing transport or rule state.

Related: [first-playable application flow](first-playable-flow.md),
[first-playable functional audio](audio.md), [match orchestration](match-orchestration.md),
[fixed table composition](../design/table-composition-prototype.md),
[card interaction](../design/card-interaction-prototype.md), and
[modular card visuals](../assets/card-visual-pipeline.md).
