# Cross-Platform Card Interaction Prototype

Status: V0 prototype evidence

## Purpose and boundary

Issue #7 proves that touch, mouse, and keyboard can express the same representative 1v1 card turn through application-owned interaction intents. The prototype does not add card-game rules. Inspection, selection, confirmation, cancellation, and temporary presentation blocking remain reversible application state; only the resulting `Play` intent submits the existing deterministic domain `PlayCardIntent`.

The prototype runs in `MatchPrototype` against an explicit state where the local player can play a rank 2 into a table containing ranks 2, 3, 4, and 6. The domain resolves the equal-rank capture and cascade through rank 4, then stops at the missing rank 5.

## Shared intent sequence

The application vocabulary is:

1. `Inspect`
2. `Select`
3. `Confirm`
4. `Play`
5. `Cancel`

`Confirm` emits `Play` only when the same player has a current legal selection and presentation is not temporarily blocked. `Play` is the sole interaction intent translated into a domain intent. Rejected or blocked interaction attempts never mutate deterministic match state.

The representative successful sequence is `Inspect -> Select -> Confirm -> Play` for every input path.

## V0 control mapping

| Intent | Touch | Mouse | Keyboard |
| --- | --- | --- | --- |
| Focus/navigation | point at a card | point at a card | arrows or WASD |
| Inspect | hold a card | right-click a card | `I` |
| Select | tap a card | left-click a card | `E` |
| Confirm | tap the selected card again | `Enter` or `Space` after mouse selection | `Enter` or `Space` |
| Cancel | second touch | `Escape` | `Escape` |

Touch selection uses a tap interaction while touch inspection uses a hold. A touch press is not also bound directly to `Confirm`, preventing one physical press from silently selecting and playing a card.

These are working V0 controls, not a production accessibility or customization decision.

## Hover-independent feedback

Every local card displays a persistent state cue that combines color, scale, and a symbol:

| State | Symbol | Meaning |
| --- | --- | --- |
| Legal | `◇` | the card can be selected for the active player |
| Selected | `◆` | the card is selected but has not been played |
| Confirmed | `✓` | confirmation produced an accepted domain play |
| Rejected | `×` | the requested card or action is unavailable |
| Temporarily blocked | `Ⅱ` | presentation is busy; the valid selection is retained |

No state depends on hover or color alone. Application feedback also exposes stable localization keys such as `interaction.feedback.card-unavailable`; final localized player-facing copy and production visuals remain outside this prototype.

## Orientation and recomposition

Interaction state is owned by `CardInteractionSession`, outside generated scene geometry. `TableCompositionPrototype` publishes a rebuild notification after portrait, landscape, desktop, or safe-area recomposition. The interaction prototype binds the newly generated card views to the unchanged application state.

Recomposition therefore does not submit, cancel, duplicate, or recreate an application intent. A selected card remains selected when valid, while deterministic `MatchState`, interaction revision, and intent history remain unchanged.

## Validation ownership

- Edit Mode verifies shared touch/desktop intent sequences, keyboard-only completion, immediate invalid-action feedback, temporary blocking, cancellation, and unchanged domain state for non-play interactions.
- Play Mode completes the representative turn with touch and desktop mappings, verifies the same application intent sequence, proves portrait recomposition preserves selection without play or duplication, and exercises all five visible feedback states.
- `The Fall > Card Interaction > Generate` adds the interaction prototype to `MatchPrototype`.
- `The Fall > Card Interaction > Validate` checks the scene binding.

## Validation checkpoint

Validated on 2026-07-20 with Unity `6000.5.4f1`:

- card-interaction generation and scene validation: passed
- Edit Mode suite: 22 tests passed (21 project tests and 1 package test)
- Play Mode suite: 6 project tests passed, including touch/desktop equivalence, every visible feedback state, and orientation preservation
- 1v1 landscape and portrait composition captures with persistent legal-state cues: visually reviewed
- macOS universal player smoke build: succeeded

This checkpoint proves the shared application and domain boundary, editor-simulated platform mappings, presentation state, orientation recomposition, and available desktop build path. Physical Android and iOS input, target size, haptics, and device orientation still require device validation.

## Deferred decisions

- production tap-versus-drag preferences and remapping
- final card inspection size and motion
- localized feedback copy and input prompts
- accessibility options, target-size thresholds, and haptic feedback
- behavior during longer event and animation sequences
- physical Android and iOS device validation

Related: [experience design](experience.md), [fixed table composition](table-composition-prototype.md), [application architecture](../technical/architecture.md), [platform requirements](../technical/platforms.md), and [testing strategy](../technical/testing.md).
