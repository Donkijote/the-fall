# Deterministic 1v1 Domain

Status: Confirmed complete first-playable rules implementation

## Purpose

Issue #22 expands the issue #4 representative-turn spike into the complete pure-C# 1v1 match required by the first playable. A match now advances deterministically from the face-down dealer spread through a unique winner without a Unity scene, frame time, `UnityEngine.Random`, presentation policy, or bot policy.

The authoritative rule behavior remains defined by [game rules](../game/rules.md). This document records the implemented state, intent, result, event, and validation contract.

## Boundary

- `TheFall.Domain` owns cards, players, immutable match state, legal intents, rule resolution, scoring, round progression, canto classification, and ordered outcome events. Its assembly retains `noEngineReferences` and has no `UnityEngine` dependency.
- `TheFall.Application.MatchSession` is the shared intent entry point for human and bot callers. It can start a full match from two players and an injected `IRandomSource`, or continue from an explicit state used by focused tests and presentation experiments.
- `TheFall.Infrastructure.SeededRandomSource` supplies replayable shuffle and reinsertion choices through the domain-owned random boundary.
- Presentation consumers continue to receive the accepted `RuleResult.State` and `RuleResult.Events`; they do not recalculate captures, scores, dealer flow, cantos, or victory.

## Match lifecycle and state

`MatchState` is an immutable snapshot. Every accepted intent returns a new state; every rejected intent returns the exact input state instance and no events.

The implemented phases are:

| Phase | Meaning |
| --- | --- |
| `DealerSelection` | Players select from the remaining face-down spread. Rank-only ties continue from that spread. |
| `AwaitingDealerChoice` | The selected or rotated dealer chooses hand/table order and the opening pattern. |
| `Active` | Players announce cantos when eligible and play the current three-card deal. |
| `Completed` | A unique winner has been resolved and no further intent is legal. |

The state records players, private hands, captured piles, table, remaining deck, dealer, active seat, scores, previous play, last capturer, round and deal numbers, final-deal status, dealer selections, preserved canto announcements, dealer choices, tie-extension status, and winner. Collections are copied and exposed read-only. The table constructor and resolver enforce at most one card per rank.

Scores persist between rounds. Hands, table, captures, previous play, and other round-scoped values reset when the dealer rotates. Tie-extension rounds retain the tied scores, continue dealer rotation without another dealer-selection phase, and use the complete opening setup.

## Intents and explicit rejection

The common `PlayerIntent` surface now includes:

- `SelectDealerCardIntent`
- `ChooseDealOptionsIntent`
- `AnnounceCantoIntent`
- `PlayCardIntent`

`OneVersusOneRules.GetLegalIntents` exposes only phase-, turn-, and ownership-appropriate intents. Every eligible canto opportunity includes all seven canto claims so false announcements remain playable.

`OneVersusOneRules.Resolve` returns an explicit `RuleError` for unsupported intents, completed matches, wrong phases, unknown players, wrong turns, cards outside the hand or dealer spread, non-dealer choices, closed or repeated canto opportunities, and missing randomness for transitions that require it. Rejection does not consume state or emit presentation facts.

## Implemented rules

The complete 1v1 flow implements:

- full-deck dealer selection, rank-only tie rounds, selection-card return, and complete reshuffle
- dealer-selected hands-first or table-first ordering and ascending or descending opening patterns
- one-card-at-a-time dealing from the dealer's right
- unique opening ranks, deterministic random duplicate reinsertion, replacement at the rejected position, and positional opening scoring
- repeated three-card deals until the forty-card deck is exhausted
- mandatory equal-rank capture and ordered cascades through `1-2-3-4-5-6-7-10-11-12`
- immediate Falls, rank-based Fall points, and four-point clean tables outside the final deal
- end-of-round leftovers, the 20-card quota, excess-card scoring, dealer rotation, and score persistence
- immediate and round-completion victory timing at the fixed standard target of 24
- complete equal-leader tie-extension rounds until the next scoring outcome creates a unique leader

The original representative state factory remains available for focused domain and animation tests. It uses the same capture, score, result, and event implementation as a full match.

## Cantos

`CantoRules.Classify` classifies a three-card hand as at most one of Casa Grande, Casa Chica, Registro, Vigía, Patrulla, Trivilín, or Ronda under the immutable match configuration.

The implementation preserves the announced three-card hand privately, allows false claims, applies cumulative clamped penalties before valid canto scoring, compares value or effect, then underlying rank strength, then dealer-right order. The shared Casas option falls Casa hands back to Ronda when disabled. The Trivilín option selects five points or immediate victory.

A valid sole canto that reaches 24 resolves at announcement. Other non-winning single cantos and multi-player comparisons resolve at deal completion. Immediate Fall or clean-table victory pre-empts unresolved announcements.

## Ordered events

Accepted results emit immutable events in presentation order for:

- match start, dealer selections, ties, selected dealer, and shuffles
- dealer choice, deal start, each dealt card, rejected opening duplicates, and accepted opening cards
- canto announcement, validation, comparison result, and score effect
- card play, table placement, ordered capture and cascade cards, score changes, and turn changes
- deal completion, leftovers, captured-card scoring, round completion, dealer rotation, and tie extension
- terminal match completion

`CardsCapturedEvent` keeps the played card, matching table card, and every cascade card in capture order. `ScoreChangedEvent` records the scoring reason, signed adjustment, and resulting total.

## Determinism and validation

The full-flow Edit Mode fixture uses explicit seeds and covers dealer ties, both deal orders, opening duplicate reinsertion, positional scoring, canto classification and options, false penalties, canto comparison, captured-card boundaries, tie extension, invalid intents, and complete-match replay.

The complete replay test starts at dealer selection, submits only legal deterministic intents, reaches one winner under the 24-point standard rules, and compares the final snapshot and ordered event log across identical runs. The original issue #4 seeded capture and presentation-buffer tests remain in place as regression coverage.

Use the repository validation command:

```sh
bash scripts/validate-unity.sh tests
```

## Deferred

- three-player and 2v2 rules
- additional bot policies, difficulty levels, and opponent personalities beyond the implemented first-playable baseline
- save or online serialization formats
- networking authority
- Unity presentation, animation, audio, and UI for the expanded event vocabulary

These belong to later first-playable issues and must consume this authoritative state, intent, result, and event boundary rather than reproduce its decisions.
