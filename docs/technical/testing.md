# Testing Strategy

Status: Confirmed V0 baseline

## Goals

- prove game rules independently from Unity scenes
- detect regressions in every supported mode
- verify presentation integration without relying only on manual play
- validate orientation, input, and performance on representative devices

## Test layers

### Domain unit tests

Run as fast EditMode tests against pure C#.

Cover:

- deck construction and deterministic shuffle
- dealer selection and ties
- dealing
- legal and illegal plays
- captures and cascades
- Fall and clean-table scoring
- every canto pattern and tie-break
- round-end collection and counting
- team and individual scoring
- victory conditions

### Application tests

Cover use-case orchestration, bot/human intent equivalence, event ordering, serialization boundaries, and invalid-intent handling.

The issue #7 interaction slice additionally compares touch and desktop mappings at the application-intent boundary, verifies immediate semantic rejection and temporary-blocking feedback, and proves that non-play interactions leave domain state unchanged.

### Unity EditMode tests

Cover configuration translation, asset metadata validation, and adapter behavior that does not require a running scene.

### Unity PlayMode tests

Cover bootstrap, scene transitions, input binding, UI state, card-anchor placement, orientation changes, and representative event-to-animation flows.

The interaction prototype's focused Play Mode coverage completes one representative turn through touch and desktop adapters, exercises visible legal/selected/confirmed/rejected/blocked states, and verifies that portrait recomposition preserves selection without submitting or duplicating a play.

The issue #9 animation laboratory replays a real resolved Fall/cascade/clean-table event list. Edit Mode verifies event-to-beat mapping, presentation-only timing, both acting seats, and authoritative final-state equality. Play Mode verifies normal completion, skip, interruption, cancellation, fast-forward, stationary-camera behavior, and equivalent portrait/landscape outcomes. Initial measurements and their limits are recorded in [gameplay animation laboratory](animation.md).

Issue #29 turns that laboratory into an Edit Mode-first workbench. The workbench now exposes a library of 22 isolated recordings rather than compound scenarios: one selector entry maps to one tunable beat. Edit Mode verifies this one-to-one mapping, named/versioned preset loading, shared Scene-view/runtime path evaluation, editor-window availability, scene-backed preview without Play Mode, deterministic transport reset, pause, step, seek, loop, skip, and convergence. Play Mode previews every isolated animation for both 1v1 seats and verifies runtime reuse through live preset changes, portrait/landscape/desktop profiles, and final-state agreement without changing the accepted recording.

### Manual device validation

Validate readability, touch targets, safe areas, thermal behavior, frame pacing, memory, loading, and visual quality on the agreed device matrix.

The repeatable commands, platform matrix, initial measurement gates, exploratory procedure, and evidence ownership are defined in [testing and platform validation baseline](../development/validation.md). Editor viewport simulation is supporting coverage only; it is not physical-device validation.

## Test data

Use explicit seeds and compact game-state builders. A failing deterministic test should report the seed, intent, prior state, and resolved events.

The implemented issue #4 coverage and replay vocabulary are described in the [deterministic domain foundation](domain-foundation.md). Its Edit Mode tests use an explicit seeded random source, recorded play-card intents, immutable input states, and ordered event logs.

## Failure diagnosis

A deterministic rule failure is replayed with its explicit seed, initial state, ordered intents, rule result, events, and final snapshot. A presentation failure starts from the already-resolved events and final state, then records scene, seat, input path, viewport, safe area, completion reason, and visual evidence. This prevents a rendering defect from being misclassified as a rule defect, or animation code from becoming a second rule engine.

The complete diagnosis contract and focused test commands are recorded in [testing and platform validation baseline](../development/validation.md).

Issue #22 extends the pure-domain layer through a complete seeded 1v1 match. Its Edit Mode coverage starts at dealer selection, exercises deals, opening duplicates and scoring, cantos, captures, round counting, victory, and tie extension, then compares the final state and ordered event log across identical replays. Focused boundary tests retain the input state instance and return explicit errors for rejected intents. See [deterministic 1v1 domain](domain-foundation.md).

Issue #23 adds application-layer complete-match simulations. They drive the human through the public legal-intent surface while the baseline bot supplies every opponent choice, verify every bot intent was legal and accepted, compare complete seeded replays, assert the bot-view information boundary, and retain rejected intent context. See [first-playable match orchestration](match-orchestration.md).

Issue #24 adds pure application-flow tests for defaults, guarded navigation, session replacement, stale-state cleanup, replay configuration, and complete match progression. Play Mode then launches through Bootstrap, observes the explicit loading stage, completes/replays/leaves through the UI adapter, and verifies pseudo-localized controls remain expanded, visible, and keyboard-focusable. See [first-playable application flow](first-playable-flow.md).

Issue #25 adds privacy-safe table-snapshot tests plus complete-match Play Mode agreement between every rendered public card collection and authoritative state. Recomposition coverage preserves a selected card, interaction revision, intent history, match instance, trace, and fixed camera across all four required desktop resolutions; the integrated adapter also covers inspect, select, confirm, cancel, reject, temporary blocking, and exactly-once play submission. See [first-playable 1v1 table presentation](first-playable-table.md).

Issue #26 adds complete-vocabulary source-order mapping and a seed-2400 complete-match runtime replay in Edit Mode. The runtime player must converge after every accepted human-plus-bot batch, and skipped, interrupted, and cancelled exits must copy the accepted state. Integrated Play Mode starts with normal playback, exercises fast-forward plus reduced motion, blocks a duplicate submission without changing trace history, covers skip/interruption/cancellation/teardown, recomposes at all four desktop resolutions, verifies events from both acting seats, and profiles an entire match without introducing pooling or a sequencing framework. See [gameplay animation workbench](animation.md).

## CI and remaining decisions

GitHub Actions and Unity CI are deliberately deferred for V0 by owner decision. Local validation evidence is required until project scale justifies choosing a licensed runner, platform modules, cache/artifact policy, and secret ownership.

Still open:

- minimum supported OS versions and exact representative hardware
- screenshot or visual-regression tooling
- automated mobile build frequency if CI is later adopted
- production coverage expectations

The first playable sets macOS loading, frame-pacing, memory, resolution, and endurance gates in the [first playable milestone](../planning/first-playable-milestone.md). The implemented foundation and current installed-module checkpoint remain recorded in [bootstrap and validation](../development/bootstrap-validation.md).
