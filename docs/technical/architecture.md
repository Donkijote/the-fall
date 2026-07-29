# Technical Architecture

Status: Confirmed foundation

## Architectural goal

Keep the card game deterministic, testable, and independent from Unity presentation. Unity owns rendering, input collection, animation, audio, platform services, and scene lifecycle; it does not own the truth of game rules.

## Confirmed boundaries

### Domain

Pure C# with no reference to the `UnityEngine` assemblies and no `MonoBehaviour`, `GameObject`, scene, input, animation, or frame-timing dependencies.

This boundary does not remove Unity from the game. It prevents the rules from depending on Unity-specific runtime objects. Unity still presents and operates the game through the outer layers. For example, the domain decides that playing a card captures ranks `2, 3, 4`; the Unity presentation decides where those card objects move, how long the animation lasts, and which sound plays.

Responsibilities:

- cards, deck, players, teams, table, scores, and rule configuration
- legal actions and deterministic state transitions
- dealer selection, dealing, turns, captures, cascades, cantos, scoring, rounds, and victory
- domain events describing resolved outcomes

### Application

Coordinates use cases and exposes player or bot intents to the domain.

Responsibilities:

- start match
- select dealer card
- choose deal options
- announce canto
- play card and receive its automatic capture resolution
- advance resolved game flow
- automatically supply baseline-bot choices until control returns to the human
- retain seeded intent, state, result, and event traces for replay and failure diagnosis
- save/load orchestration through interfaces

The implemented first-playable contract is recorded in [first-playable match orchestration](match-orchestration.md). Its bot receives a sanitized public turn view plus its own private hand; it does not receive the opponent hand, hidden deck order, or complete authoritative state.

### Unity adapters and presentation

Responsibilities:

- map touch, mouse, and keyboard actions into application intents
- render domain state
- turn resolved domain events into animation and audio sequences
- manage scenes, prefabs, cameras, UI, localization, and platform services
- load authored configuration without embedding rules in scene objects

## Dependency rule

Dependencies point inward:

```text
Unity presentation -> application -> domain
Unity infrastructure -> application ports
domain -> no Unity dependency
```

Presentation may also reference domain-owned immutable state and resolved event types for rendering. That is an inward read dependency only; presentation must submit intents through the application boundary and cannot invoke or reproduce rule decisions.

## Confirmed project organization

```text
Assets/TheFall/
├── Domain/
├── Application/
├── Infrastructure/
├── Presentation/
├── Content/
├── Tests/
└── Editor/
```

Assembly definitions enforce these boundaries from the initial project bootstrap.

Use `TheFall` as the root C# namespace and create these initial assemblies:

- `TheFall.Domain`: deterministic rules; no `UnityEngine` reference
- `TheFall.Application`: match use cases and orchestration
- `TheFall.Infrastructure`: persistence, injected randomness, and future external integrations
- `TheFall.Presentation`: Unity scenes, views, input, UI, animation, and audio
- dedicated Edit Mode and Play Mode test assemblies

Compose these dependencies manually from the bootstrap flow. Do not add a dependency-injection framework until a demonstrated need justifies it through an architecture decision record.

## Initial scenes

- `Bootstrap`: application startup and persistent services
- `Login`: gateway presentation
- `Hub`: player hub, settings, and pre-match presentation
- `Match`: loading, authoritative 1v1 table, HUD, result, animation, and audio
- `MatchPrototype`: foundation for the first 1v1 rules prototype
- `AnimationLab`: isolated card and character presentation experiments

## Unity-facing systems

- Use UI Toolkit for adaptive screen-space menus and HUD.
- Use uGUI and TextMeshPro where world-space or table-integrated UI requires them.
- Keep the Input System and translate touch, mouse, and keyboard input into shared application intents.
- Start with direct asset references and ScriptableObjects. Introduce Addressables only when the content-loading requirements justify the additional system.
- Require an architecture decision record before adding third-party frameworks for dependency injection, tweening, asynchronous flow, saving, or networking.

## Configuration

**Confirmed:** Use immutable domain configuration values for rule execution. ScriptableObjects may author and serialize configuration, but must be translated into domain-owned values before a match starts.

## Localization

**Confirmed:** Install and design around Unity Localization from the project bootstrap onward. English is the initial source language, but player-facing text must use stable localization keys instead of being embedded directly in scenes or gameplay code.

Localization package installation and initial configuration belong to the dedicated Unity bootstrap cleanup issue, not the documentation-only pull request.

## Determinism

- inject random sources or seeds
- do not use frame time inside game rules
- record player intents and resolved events where helpful for debugging
- make bots consume the same legal-action surface as human players
- design networking later around validated intents and authoritative state, not scene transforms

The initial immutable state, intent, rule-result, and resolved-event vocabulary is recorded in the [deterministic domain foundation](domain-foundation.md). It proves the boundary for a representative 1v1 play-card flow without selecting the eventual serialization or networking format.

The first-playable orchestrator now composes that vocabulary through a complete seeded human-versus-bot match while keeping bot policy and replay diagnostics outside the domain.

Issue #25 binds that same orchestrator to `FirstPlayableTablePresentation` in Home. A privacy-safe snapshot reduces hidden opponent and dealer-spread cards to counts before rendering, while public state and the latest ordered event drive the table and HUD. Confirmed card plays return through `FirstPlayableFlow`; resizing rebuilds view objects without replacing the application session or interaction state. See [first-playable 1v1 table presentation](first-playable-table.md).

## Open architecture decisions

- long-term state representation and serialization format
- command/event vocabulary beyond the initial 1v1 spike
- async orchestration model for presentation sequences
- save system boundaries
- networking authority model

Related: [animation](animation.md) and [testing](testing.md).
