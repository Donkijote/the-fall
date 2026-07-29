# V0 Foundation Plan

Status: Completed

## Objective

Reduce the largest product and technical uncertainties before committing to a playable milestone.

## Step 1: Establish documentation

- confirm product direction and vocabulary
- review the entire rules specification with Manuel
- record open decisions and acceptance criteria
- establish development and documentation workflows

Exit signal: the documentation can guide issue creation without relying on the old project.

## Step 2: Clean the Unity baseline

- create a dedicated cleanup issue
- remove tutorial-only scenes, scripts, packages, and assets
- establish a project-owned folder structure and bootstrap scene
- configure product identity and initial target settings
- install and configure Unity Localization with English as the source language

Exit signal: the editor opens into a minimal The Fall-owned project without tutorial dependencies.

## Step 3: Prove deterministic rules architecture

- define core value types and game state
- model commands/intents and resolved domain events
- implement a narrow rule slice in pure C#
- prove it with EditMode tests
- use 1v1 against one bot as the first technical prototype

Exit signal: a recorded intent deterministically produces state and presentation events without a scene.

## Step 4: Prove table composition

- block out the table and upper-body seating
- establish the fixed cinematic overhead camera
- test 1v1, three-player, and 2v2 composition
- test representative phone/tablet landscape and desktop aspect ratios

Exit signal: all seats, hands, table cards, names, and scores are legible with prototype geometry.

## Step 5: Prove interaction

- map touch and mouse/keyboard into shared game intents
- test card inspection, selection, play, and automatic capture feedback
- validate input feedback and invalid actions

Exit signal: a user can express a representative turn on mobile and desktop without platform-specific rules.

## Step 6: Prove the asset pipeline

- create initial style briefs
- generate concept images
- convert selected concepts through Meshy AI
- review, optimize, license-record, and import prototypes
- establish asset naming and import presets

Exit signal: at least one representative character, table/prop, and card asset completes the documented pipeline.

## Step 7: Prove animation orchestration

- build a focused animation experiment scene
- implement play, capture, and representative Fall sequences
- drive a sequence from recorded domain events
- test seats, both landscape directions, skip, and performance behavior

Exit signal: presentation can consume rule outcomes without controlling them.

## Step 8: Establish validation

- configure EditMode and PlayMode test assemblies
- define CI feasibility
- define a representative device matrix
- record initial performance targets

Exit signal: core validation has repeatable commands and ownership.

## Step 9: Select the first playable milestone

Issue #11 used evidence from the prior steps to decide:

- one complete offline 1v1 match against one deterministic baseline bot
- the complete confirmed 1v1 rules, with only the shared Casas and Trivilín-effect options exposed
- prototype art, UI, animation, VFX, and audio fidelity
- a macOS universal development-player acceptance target
- objective rule, interaction, state-synchronization, resolution, loading, frame-pacing, memory, and endurance gates

Exit signal: the [first playable milestone](first-playable-milestone.md) and its issues #22–#31 define the next implementation phase and parallel physical-iPhone evidence lane.

## V0 issue history

The V0 foundation work has been refined into these GitHub issues:

1. [#3 Clean the Unity baseline and create the project bootstrap](https://github.com/Donkijote/the-fall/issues/3)
2. [#4 Establish the deterministic card-game domain foundation](https://github.com/Donkijote/the-fall/issues/4)
3. [#5 Define the V0 art direction and prototype asset briefs](https://github.com/Donkijote/the-fall/issues/5)
4. [#6 Prototype the fixed overhead table composition](https://github.com/Donkijote/the-fall/issues/6)
5. [#7 Prototype cross-platform card interaction](https://github.com/Donkijote/the-fall/issues/7)
6. [#8 Establish the generated 3D asset intake pipeline](https://github.com/Donkijote/the-fall/issues/8)
7. [#9 Build the gameplay animation laboratory](https://github.com/Donkijote/the-fall/issues/9)
8. [#10 Establish the testing and platform validation baseline](https://github.com/Donkijote/the-fall/issues/10)
9. [#11 Define the first playable milestone](https://github.com/Donkijote/the-fall/issues/11)

All nine V0 issues were assigned, labeled, and attached to the The Fall project. Issues #3–#10 supplied the evidence consumed by #11.

**Confirmed:** Issue #11 closes the V0 foundation. The next phase is the ordered first-playable plan and physical-iPhone follow-up in issues #22–#31.
