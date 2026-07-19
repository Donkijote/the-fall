# Testing Strategy

Status: Proposed

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

### Unity EditMode tests

Cover configuration translation, asset metadata validation, and adapter behavior that does not require a running scene.

### Unity PlayMode tests

Cover bootstrap, scene transitions, input binding, UI state, card-anchor placement, orientation changes, and representative event-to-animation flows.

### Manual device validation

Validate readability, touch targets, safe areas, thermal behavior, frame pacing, memory, loading, and visual quality on the agreed device matrix.

## Test data

Use explicit seeds and compact game-state builders. A failing deterministic test should report the seed, intent, prior state, and resolved events.

The implemented issue #4 coverage and replay vocabulary are described in the [deterministic domain foundation](domain-foundation.md). Its Edit Mode tests use an explicit seeded random source, recorded play-card intents, immutable input states, and ordered event logs.

## Open decisions

- CI environment and Unity licensing
- minimum coverage expectations
- screenshot or visual-regression tooling
- performance budgets and representative hardware
- automated mobile build frequency

The implemented local foundation commands and current platform checkpoint are recorded in [bootstrap and validation](../development/bootstrap-validation.md).
