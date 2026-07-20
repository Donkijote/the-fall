# Select a complete macOS 1v1 match as the first playable

Status: Accepted
Date: 2026-07-20

## Context

V0 proved the deterministic architecture, fixed table composition, shared interaction model, generated-asset intake, modular cards, event-driven animation, and repeatable validation path. The project now needs one player-visible integration milestone without silently committing to Story Mode, all modes, production content, or platforms that have not been built and launched.

## Options

1. Continue with another representative-turn prototype. This is inexpensive but does not prove setup, repeated deals, rounds, cantos, bot orchestration, victory, or a usable application flow.
2. Build a complete 1v1 match against one bot and accept it first on the validated macOS path. This proves the game loop while bounding participants, platform, content, and opponent quality.
3. Make Story Mode or all target platforms part of the first playable. This combines unproven content, progression, support, device, and distribution work with the rules-integration risk.

## Decision

Select option 2.

The first playable is one complete offline 1v1 match against a deterministic baseline bot, from Home through a visible winner, replay, and return. It includes every confirmed rule required for a complete 1v1 match, fixes the target at 24 points, and exposes only the shared Casas option and the Trivilín effect.

Acceptance targets a macOS universal development player with mouse and keyboard, a resizable desktop layout, prototype visual and audio fidelity, and the measurable gates recorded in the [first playable milestone](../planning/first-playable-milestone.md).

## Consequences

- Completing the full 1v1 rules model is the first implementation dependency.
- Human and bot choices must share the same intent surface, and the bot must not use hidden opponent information.
- The prototype table and modular cards may ship in the milestone; the review-only high-resolution character may not.
- Story Mode, three-player, 2v2, advanced bot behavior, production fidelity, Android, iOS, Windows, signing, and distribution remain deferred.
- Existing mobile and orientation tests remain regression coverage without becoming physical-device acceptance claims.
- V0 may exit because its remaining work is bounded implementation rather than unresolved foundation discovery.
