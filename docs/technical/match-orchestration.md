# First-Playable Match Orchestration

Status: Confirmed first-playable application contract

## Purpose

Issue #23 composes the complete deterministic 1v1 domain from issue #22 into a human-facing application session with one deterministic baseline opponent. It proves the complete match can advance from dealer selection to one authoritative winner without Unity presentation or manual bot intervention.

The authoritative rules remain in [game rules](../game/rules.md), and the state, intent, result, and event vocabulary remains defined by the [deterministic 1v1 domain](domain-foundation.md).

## Application boundary

`FirstPlayableMatchOrchestrator` owns one `MatchSession`. A human adapter reads `GetHumanLegalIntents` and submits one of those domain-owned `PlayerIntent` values through `SubmitHumanIntent`. After every accepted human intent, the orchestrator automatically supplies consecutive bot choices until the match completes or control returns to the human.

Both callers therefore use the same legal-intent and rule-result surface:

- `SelectDealerCardIntent`
- `ChooseDealOptionsIntent`
- `AnnounceCantoIntent`
- `PlayCardIntent`

The orchestrator does not decide rules, deal cards, calculate captures, award points, or determine victory. It submits intents to `MatchSession`, records the returned state and ordered events, and stops when the domain returns control to the human or completes the match.

`FirstPlayableMatchFactory` is the infrastructure composition entry point. It creates independent `SeededRandomSource` streams for domain randomness and bot tie breaking from one recorded diagnostic seed. Supplying the same seed and human intent sequence produces the same bot choices, ordered events, and final state.

## Baseline bot policy

There is one fixed baseline policy with no difficulty or personality configuration:

- select an opaque face-down dealer option by seeded index, without reading its rank or suit
- select dealer setup options through seeded deterministic tie breaking
- announce the one valid canto classification when the opportunity exists; do not deliberately bluff
- prefer legal plays that capture the most table cards
- prefer immediate Fall and clean-table value when capture counts tie
- use the seeded source to break remaining equivalent choices

Strategic challenge, teaching behavior, bluffing, opponent modeling, and multiple personalities remain deferred.

## Information boundary

The policy receives a `BotTurnView`, not `MatchState`. Its public contract contains:

- public phase, dealer flag, scores, round/deal state, final-deal and tie-extension flags
- the public previous play and table cards
- the active rule configuration
- the bot's own private hand

It does not expose the opponent hand, hidden deck order, dealer-spread card identities, preserved opponent canto hands, or the complete player-state collection. Dealer-selection options are treated as opaque legal entries; the policy chooses only an index and never evaluates their hidden card values.

## Replay and diagnosis contract

Every session retains a `MatchTrace` containing:

- the explicit seed
- the initial authoritative state and startup events
- every human and bot intent in order
- the actor, prior state, accepted or rejected result, explicit error, resulting state, and ordered events for every submission
- the accumulated event log and latest final state

Rejected human intents remain in the trace with the same prior/resulting state instance and no events. An orchestration safety failure throws `MatchOrchestrationException`, reports the seed, intent count, and current phase, and carries the complete trace for diagnosis.

## Validation

The focused Edit Mode fixture proves:

- a human-facing session completes while the bot supplies every opponent choice
- every bot submission belongs to the same domain legal-intent surface and is accepted
- the public bot view omits opponent hands and hidden deck state
- identical seeds and human choices reproduce bot intents, ordered events, and final state
- rejected human intents retain the complete diagnosis contract
- 24 seeded complete-match simulations terminate with one winner and no invalid bot intents or deadlocks

Use the repository validation command:

```sh
bash scripts/validate-unity.sh tests
```

## Deferred

- Home, setup, loading, result, replay, and return navigation from issue #24
- complete table presentation from issue #25
- production animation, audio, save/resume, Story Mode, additional bots, and online authority

Later layers must consume this application contract rather than bypassing it or reproducing rule decisions.
