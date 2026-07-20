# Gameplay Animation Laboratory

Status: V0 prototype evidence

## Purpose and boundary

Issue #9 proves that Unity presentation can explain a resolved turn without owning, delaying, or repeating game rules. `RepresentativeAnimationTurn` creates a deterministic 1v1 application fixture and submits one real `PlayCardIntent` through `MatchSession`. The resulting `RuleResult.State` and ordered `RuleResult.Events` are recorded once. `AnimationLabController` consumes that recording through `ResolvedMatchBuffer`; it never invokes the resolver while sequencing animation.

The representative turn plays rank 2 into table ranks 2, 3, and 4 immediately after the opponent's non-capturing rank-2 play. The domain resolves:

1. card played
2. equal-rank capture
3. cascades through ranks 3 and 4
4. one-point Fall
5. four-point clean table
6. turn change

The fixture runs with either 1v1 seat as the actor. Its final score of 12, empty table, two-card hand, four-card captured pile, and next turn all come from the authoritative final `MatchState`.

## Event-to-presentation sequence

`ResolvedAnimationSequence` maps event meaning to presentation beats in event-list order:

| Resolved domain event | Presentation beat |
| --- | --- |
| `CardPlayedEvent` | move the played card from the acting hand into the table action area |
| `CardPlacedOnTableEvent` | settle a non-capturing play on the table |
| `CardsCapturedEvent` | move the played and equal-rank cards as one normal-capture beat, then emit one countable cascade beat per remaining event card |
| `ScoreChangedEvent` with `Fall` | show one Fall score beat and assign the event's resulting total |
| `ScoreChangedEvent` with `CleanTable` | show the clean-table confirmation and assign the event's resulting total |
| `TurnChangedEvent` | update the active-seat cue from the event's current seat |
| `MatchCompletedEvent` | show match resolution from the event's winning team |
| end of recording | copy the authoritative final `MatchState` into the rendered snapshot |

Presentation uses the card list, score total, reason, seat, and winner already carried by events. It does not compare ranks, search for legal captures, calculate Fall points, detect an empty table, or decide a winner.

## Runtime presentation

`AnimationLab.unity` retains the fixed gameplay camera and uses:

- the approved V0 `RoundCardTable` prefab
- the generated forty-card `CardVisualCatalog` and shared atlas material
- inexpensive upper-body placeholders because `WarmChallenger` is approved only for static review and remains far above the gameplay triangle target
- the existing portrait and wide-landscape table-composition profiles

The scene creates only transient `DontSave` experiment geometry. It stays isolated from `MatchPrototype` and the production scene flow.

## Presentation configuration

`AnimationSequenceConfiguration.asset` owns card-play, normal-capture, cascade-step, score-beat, and turn-change durations, plus fast-forward and reduced-motion scales. These values are presentation configuration only. No timing value is translated into `RuleConfiguration`, and changing timing cannot change accepted intents, captured cards, scores, turns, or match completion.

The V0 defaults are:

| Parameter | Value |
| --- | ---: |
| card play / table placement | `0.22 s` |
| normal capture | `0.28 s` |
| cascade step | `0.14 s` |
| Fall / clean-table score beat | `0.28 s` |
| turn or match transition | `0.08 s` |
| fast-forward multiplier | `4x` |
| reduced-motion duration scale | `0.25x` |

These values are V0 tuning evidence, not rule values or final production timing.

## Interruption and synchronization contract

The domain result is authoritative before the first animation frame. Presentation controls affect only how that result is explained:

- **complete**: play every mapped beat, then copy the final `MatchState`
- **fast-forward**: preserve beat order and end state while dividing durations by the configured multiplier
- **reduced motion**: shorten travel and beat durations while retaining result cues
- **skip**: stop the current coroutine and immediately copy the final `MatchState`
- **interrupt**: stop because another presentation lifecycle event took priority, then immediately copy the final `MatchState`
- **cancel**: cancel presentation playback, not the accepted player intent; immediately copy the final `MatchState`
- **disable or scene teardown during playback**: use the interruption path before releasing generated views

Every early exit sets the rendered table, hands, captured piles, scores, active seat, phase, and winner from the resolved state. A cancelled animation never rolls back or re-resolves accepted gameplay.

## Orientation and seat behavior

The same recorded outcome is exercised with `Seat.First` at the bottom and `Seat.Second` at the opposite anchor. Portrait (`390 x 844`) and wide landscape (`844 x 390`) rebuild the generated view from the unchanged presentation snapshot. Recomposition selects only a presentation profile; it does not submit an intent, reorder events, or modify domain state.

`The Fall > Animation Laboratory > Capture Validation Set` writes final-state captures for both actors and both orientations to the ignored `Logs` directory. The captures retain the stationary camera, show the captured cards at the acting seat, keep the table empty, and display the resolved 12-point score.

## Validation and initial performance findings

Validated on 2026-07-20 with Unity `6000.5.4f1` on a 12-core Apple M4 Pro MacBook Pro with 24 GB memory and macOS 26.5.2:

- animation-laboratory generation and structural validation: passed
- complete Edit Mode suite: 40/40 passed, including six animation-sequence cases
- complete Play Mode suite: 11/11 passed, including normal completion, skip, interruption, cancellation, fast-forward, both 1v1 seats, and portrait/landscape outcomes
- four GPU-rendered macOS Editor validation captures: visually reviewed
- macOS universal player smoke build: succeeded
- fast-forward Play Mode sample at `1920 x 1080`: `300.8 ms` configured wall time, `5.00 ms` measured presentation CPU total, `0.046 ms` peak measured presentation update, and six animated card views

The batch test runner issued 12,879 scheduling updates during that fast-forward sample because an uncapped headless runner can advance far more often than a displayed player. That count is not a frame-rate result. The useful initial signal is that event mapping, snapshot mutation, six-card transform updates, and final synchronization consumed little CPU on the available desktop target. The sample does not measure GPU frame time, thermal behavior, memory pressure over repeated matches, or mobile performance.

## Remaining risks and deferred decisions

- Issue #29 owns the real-time sequence workbench for composing, tuning, transporting, and saving reusable presentation beats. Issue #26 then promotes those lab-tested beats into the complete first playable.
- Physical Android and iOS profiling, safe areas, thermal behavior, and device frame pacing remain unvalidated.
- Desktop player GPU profiling and repeated-sequence allocation sampling should be added with issue #10's platform baseline.
- The V0 view animates six representative cards; full deals, larger table populations, VFX, audio, characters, cantos, round transitions, and match victory can change cost and interruption pressure.
- Production input blocking while a sequence is active must integrate this synchronization contract with the application interaction session.
- Pooling is not selected. The laboratory uses transient objects; add pooling only after representative-match measurement supports it.
- Animator, Timeline, a custom promoted orchestrator, or a third-party tweening framework remains an open architecture decision. This issue uses a small custom coroutine experiment and does not establish a project-wide framework choice.
- Final easing, particles, sound, haptics, accessibility controls, and animation polish remain outside V0 acceptance.

Related: [architecture](architecture.md), [deterministic domain foundation](domain-foundation.md), [testing](testing.md), [fixed table composition](../design/table-composition-prototype.md), and [card interaction](../design/card-interaction-prototype.md).
