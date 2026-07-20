# The Fall Documentation

This directory is the source of truth for The Fall. It is written for both human contributors and AI agents.

## How to read these documents

Read documents in this order before planning or implementing project work:

1. [Product vision](product/vision.md)
2. [V0 scope](product/v0-scope.md)
3. [Game rules](game/rules.md) and [game modes](game/modes.md)
4. [Experience design](design/experience.md), [art direction](design/art-direction.md), and the [visual reference board](design/visual-reference-board.md)
5. [Technical architecture](technical/architecture.md)
6. [V0 foundation plan](planning/v0-foundation-plan.md)
7. [Development workflow](development/workflow.md) and [project guidelines](development/guidelines.md)
8. [Decision log](decisions/README.md)

## Decision language

Every requirement or recommendation uses one of these states:

- **Confirmed**: approved project direction. Future work must respect it unless a newer decision supersedes it.
- **Proposed**: a recommended direction awaiting validation or an explicit decision.
- **Open**: unresolved. Do not silently choose an answer during implementation.
- **Deferred**: intentionally excluded from the current phase.

When documents disagree, prefer the newest accepted entry in the [decision log](decisions/README.md), then update the affected documents.

## Documentation map

### Product

- [Vision](product/vision.md): identity, audience, experience, and long-term direction.
- [V0 scope](product/v0-scope.md): goals, non-goals, workstreams, and exit criteria.

### Game

- [Rules](game/rules.md): authoritative V0 rules baseline.
- [Modes](game/modes.md): mode-specific behavior and delivery order.
- [Glossary](game/glossary.md): canonical project terminology.

### Design

- [Experience](design/experience.md): camera, player representation, interaction, and UI principles.
- [Fixed table composition prototype](design/table-composition-prototype.md): V0 camera, seat-anchor, orientation, safe-area, readability, validation, and unresolved-constraint evidence.
- [Art direction](design/art-direction.md): visual principles, palette, cultural neutrality, readability, anti-goals, and V0 technical envelope.
- [Visual reference board](design/visual-reference-board.md): annotated references, provenance, usage boundaries, and category-specific study notes.

### Assets

- [Asset strategy](assets/strategy.md): prototype sourcing, generation workflow, licensing, and budgets.
- [Prototype asset briefs](assets/prototype-briefs.md): generation-ready character, table, and card briefs with review gates.

### Technical

- [Architecture](technical/architecture.md): deterministic C# domain and Unity presentation boundaries.
- [Animation](technical/animation.md): animation responsibilities, experiments, and orchestration.
- [Testing](technical/testing.md): domain, Unity, integration, and platform validation.
- [Deterministic domain foundation](technical/domain-foundation.md): implemented 1v1 state, intent, result, and event vocabulary.
- [Platforms](technical/platforms.md): mobile and desktop targets, orientations, and input.

### Planning and development

- [V0 foundation plan](planning/v0-foundation-plan.md): ordered discovery and implementation preparation.
- [Development workflow](development/workflow.md): issues, branches, commits, pull requests, and project status.
- [Project guidelines](development/guidelines.md): Unity, C#, asset, scene, prefab, and naming guidance.
- [Bootstrap and validation](development/bootstrap-validation.md): implemented Unity foundation, scene roles, commands, and platform checkpoint.
- [Decision log](decisions/README.md): durable record of important choices and their rationale.

## Maintenance rule

Documentation changes are part of implementation. When code changes an established rule, architecture boundary, workflow, or player-facing behavior, update the relevant document in the same issue and pull request.
