# V0 Scope

Status: Completed confirmed foundation scope

## Purpose

V0 was the foundation and exploration phase. It did not implement a playable milestone or production release; it reduced the uncertainties required to select one.

## Goals

- establish a reviewed source of truth for the complete game rules
- replace the Unity tutorial baseline with a minimal project-owned bootstrap through a dedicated issue
- define the Unity architecture and deterministic game-domain boundaries
- validate the visual direction with inexpensive prototypes
- establish an asset-generation, review, licensing, and import workflow
- prototype camera, card interaction, and representative animation sequences
- define testing layers and an initial device matrix
- turn confirmed work into small, issue-sized units on the project board

## Non-goals

- completing the full game
- migrating TypeScript/PixiJS code or assets
- committing to final production assets before the art direction is proven
- implementing online multiplayer before the offline rules foundation is reliable
- declaring a first playable milestone before the necessary experiments are understood

## Confirmed V0 workstreams

1. Documentation and rule validation
2. Tutorial-content cleanup and project bootstrap
3. Deterministic domain architecture spike
4. Camera and table-composition prototype
5. Card interaction prototype for touch and desktop
6. Prototype asset pipeline
7. Animation orchestration experiment
8. Testing and platform baseline
9. First playable-slice definition

The detailed ordering and exit criteria live in the [V0 foundation plan](../planning/v0-foundation-plan.md).

## Confirmed exit criteria

V0 is complete when:

- the rules required for the first gameplay slice contain no blocking ambiguities
- the project has a clean bootstrap owned by The Fall
- a deterministic rules model can execute independently of Unity presentation
- the overhead table camera is readable in desktop layouts and representative mobile landscape layouts
- representative assets can move from concept to approved Unity prototype with traceable licensing
- touch and mouse/keyboard can express core card intents
- one representative gameplay sequence can be animated from domain events
- EditMode and PlayMode tests run through an agreed workflow
- the next milestone is defined as a prioritized set of GitHub issues

## Exit decision

**Confirmed:** V0 exits with issue #11. Every foundation exit criterion has sufficient evidence, and the remaining work is bounded first-playable implementation rather than missing discovery.

The [first playable milestone](../planning/first-playable-milestone.md) is one complete offline 1v1 match against a deterministic baseline bot, accepted first as a macOS development build with prototype fidelity. Its core implementation and physical-iPhone follow-up are tracked by issues #22–#31.

Android, iOS, Windows, minimum supported OS versions, production performance tiers, Story Mode content, additional game modes, and online play remain later product or platform decisions. They do not block the bounded macOS first playable.

The earlier decision to use 1v1 against one bot as the first technical rules prototype is retained and promoted into a complete match for this next milestone.
