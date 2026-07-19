# Deterministic Domain Foundation

Status: Confirmed implementation spike

## Purpose

Issue #4 proves that a representative 1v1 turn can resolve in pure C# without Unity presentation, scene state, frame time, or `UnityEngine.Random`. This is a deliberately narrow foundation rather than the complete rules implementation.

## Boundary

- `TheFall.Domain` owns cards, participants, match state, legal intents, rule results, and resolved events. Its assembly has `noEngineReferences` enabled.
- `TheFall.Application.MatchSession` is the shared intent entry point. Human input and bots submit the same domain intents and receive the same result shape.
- `TheFall.Infrastructure.SeededRandomSource` implements the domain-owned `IRandomSource` boundary with an explicit seed.
- `TheFall.Presentation.ResolvedMatchBuffer` consumes `RuleResult.State` and `RuleResult.Events` in order without evaluating rules. Later rendering and animation code can consume that buffer; it must not repeat capture, cascade, Fall, clean-table, score, or turn decisions.

## State vocabulary

| Type | Meaning in the spike |
| --- | --- |
| `Card`, `CardRank`, `CardSuit` | Immutable Spanish-deck card values using ranks 1–7 and 10–12 and the canonical English suit names. |
| `Deck` | Immutable ordered card collection with deterministic injected shuffling. |
| `Seat` | Logical counter-clockwise position. The representative 1v1 flow uses `First` and `Second`. |
| `Player`, `PlayerId`, `PlayerControl` | Stable player identity and metadata. `PlayerControl` distinguishes human and bot origins without changing their intent surface. |
| `Team`, `TeamId` | Scoring ownership. Each player owns a separate team in 1v1; the type leaves room for shared teams later. |
| `Score` | Non-negative immutable point total with explicit add and clamped-subtract operations. |
| `RuleConfiguration` | Immutable pre-match values. The foundation records the 24-point target plus confirmed Casa and Trivilín options even though canto resolution is deferred. |
| `PlayerState` | One player's hand and captured-card pile. |
| `PreviousPlay` | The immediately preceding played card and whether it captured, sufficient to recognize a Fall. |
| `MatchState` | Immutable 1v1 snapshot containing players, table, remaining deck, scores, turn, dealer, final-deal flag, rules, and completion state. |

`MatchState` enforces the confirmed table invariant that at most one card of each rank may be present.

## Intent and result vocabulary

`PlayerIntent` is the common base for recorded decisions. The spike implements `PlayCardIntent(playerId, card)` only.

`OneVersusOneRules.Resolve` always returns a `RuleResult`:

- accepted results contain a new immutable `MatchState` and ordered domain events
- rejected results contain an explicit `RuleError`, the unchanged state instance, and no events
- resolving never mutates the input state

`OneVersusOneRules.GetLegalIntents` exposes the same legal play-card intents for the current player whether `PlayerControl` is `Human` or `Bot`.

## Event vocabulary

Events describe facts already decided by the domain. Their list order is the presentation sequence order.

| Event | Meaning |
| --- | --- |
| `CardPlayedEvent` | A legal card left the acting player's hand. |
| `CardPlacedOnTableEvent` | No same-rank card existed, so the played card remained on the table. |
| `CardsCapturedEvent` | The played card, same-rank table card, and ordered cascade cards moved to the player's captured pile. |
| `ScoreChangedEvent` | Fall or clean-table points changed a team's score, including reason and resulting total. |
| `TurnChangedEvent` | Resolution completed and the next logical seat became active. |
| `MatchCompletedEvent` | An uncontested score at or above the configured target ended the representative match. |

## Implemented rule slice

The representative resolver validates turn ownership and card ownership, then:

1. removes the played card from the hand
2. places it when no equal rank exists, or performs the mandatory equal-rank capture
3. captures the ordered cascade through `1-2-3-4-5-6-7-10-11-12`, stopping at the first gap
4. awards rank-based Fall points for an immediate capture of the previous non-capturing play
5. awards four clean-table points outside the final deal
6. completes the match on an uncontested score at or above the configured target, or advances the turn

Seeded Edit Mode tests replay recorded intents and compare the resulting state and ordered events. They also cover the 40-card deck, deterministic shuffle, cascade stopping, stacked Fall and clean-table scoring, invalid-intent immutability, and the shared human/bot intent surface.

## Intentionally deferred

- dealer selection and tied selections
- initial table dealing, duplicate reinsertion, and opening-pattern scoring
- deal and round orchestration
- canto classification, announcement, comparison, and penalties
- captured-card quotas and leftover assignment
- three-player, 2v2, and tie-extension behavior
- serialization format, networking authority, and save/load boundaries

These remain governed by the authoritative [game rules](../game/rules.md) and should be added as independently tested slices rather than inferred from this prototype.
