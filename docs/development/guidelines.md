# Project Guidelines

Status: Confirmed foundation

## General

- Prefer explicit, testable behavior over scene-driven hidden coupling.
- Keep game rules out of `MonoBehaviour` classes.
- Keep authored Unity data separate from runtime domain state.
- Record important decisions rather than encoding them only in code or prefabs.
- Avoid importing packages or assets before documenting the need and license.

## Unity version policy

Track the latest suitable production Unity release as closely as practical instead of pinning V0 indefinitely. Treat an editor upgrade as an intentional repository change: perform it on an issue branch, review package and serialization changes, run the relevant tests, and verify that the project opens and builds before merging.

## C# naming

- Namespaces and types: `PascalCase`
- Public members: `PascalCase`
- Private fields: `_camelCase`
- Local variables and parameters: `camelCase`
- Interfaces: `IName`
- Tests: describe behavior and expected result clearly

Use `TheFall` as the root namespace.

**Open:** Decide whether the project standardizes expression-bodied members, `var`, nullable reference types, and analyzers.

## Unity assets

**Confirmed root:** `Assets/TheFall/`

- use descriptive names; avoid unexplained numeric suffixes
- keep Unity `.meta` files with their assets
- do not move or rename assets outside Unity when reference preservation matters
- distinguish source/generated files from optimized Unity imports
- prefer prefabs for reusable authored compositions
- document import presets for repeated asset types
- use the repository's committed `.gitattributes` and Git LFS rules for tracked binary formats
- keep bulk rejected generations and temporary conversion intermediates outside the repository

## Scenes

- scenes compose presentation and bootstrap dependencies; they do not define game rules
- keep experiments separate from production flow
- give every committed scene a clear purpose and owner document
- avoid a single scene accumulating unrelated systems

## Prefabs

- prefer focused prefabs with explicit responsibilities
- avoid deep inheritance unless it materially reduces duplication
- do not use scene lookups as an implicit dependency-injection system
- expose only parameters that are safe and meaningful for authors

## Parameters and configuration

- separate rule configuration, presentation tuning, and platform tuning
- use units in parameter names or tooltips where ambiguity is possible
- define safe ranges for animation and VFX tuning
- translate ScriptableObject data into domain values before match execution
- do not let inspector defaults become undocumented rules

## Localization

- English is the source language, not a reason to hard-code player-facing strings
- use stable localization keys from the initial project bootstrap
- keep rule identifiers and localization display text separate
- test layouts with expansion-prone translations before declaring UI composition final

## Tests

- add domain tests with each rule behavior
- add regression tests for fixed rule bugs
- use explicit random seeds
- keep PlayMode coverage focused on Unity integration and player-visible flow

## Performance

- profile on representative mobile hardware before production asset approval
- pool frequently created presentation objects when measurement supports it
- define texture, mesh, material, animation, and VFX budgets before scaling content production

## Open guideline decisions

- formatting and analyzer tooling
- folder and asset-name suffix standards
- scene-loading strategy
- logging and diagnostics conventions
- source-asset promotion thresholds and external-archive backup policy
