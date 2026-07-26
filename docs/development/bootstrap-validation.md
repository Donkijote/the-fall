# Bootstrap and Validation

Status: Confirmed implementation baseline

## Editor and packages

The foundation uses Unity `6000.5.4f1`, the latest supported Unity 6.5 patch available when issue #3 began on 2026-07-19. The project-owned editor installation and `ProjectVersion.txt` already matched that release, so no version migration was required.

The direct package baseline retains:

- Input System `1.19.0`
- Localization `1.5.12`
- Universal Render Pipeline `17.5.0`
- Unity Test Framework `1.7.0`
- uGUI and TextMeshPro `2.5.0`

The tutorial framework, Multiplayer Center, and Cinemachine packages were removed with the tutorial assets that used them. Localization brings Addressables as a required transitive package. This does not change the architecture decision to avoid project-level Addressables content loading until a demonstrated requirement exists; only Localization-managed locale and string-table assets use it.

## Project-owned root and assemblies

All retained assets live under `Assets/TheFall`. TextMeshPro essentials and package-generated UI, Localization, and Addressables configuration are kept under `Assets/TheFall/Content` rather than their default top-level locations.

Assembly dependencies point inward:

| Assembly | References | Unity engine access |
| --- | --- | --- |
| `TheFall.Domain` | none | disabled |
| `TheFall.Application` | `TheFall.Domain` | disabled |
| `TheFall.Infrastructure` | `TheFall.Application`, `TheFall.Domain` | enabled for adapters |
| `TheFall.Presentation` | `TheFall.Application`, `TheFall.Domain`, `TheFall.Infrastructure`, Input System | enabled |
| `TheFall.EditModeTests` | foundation runtime assemblies | editor tests |
| `TheFall.PlayModeTests` | presentation foundation | runtime tests |

`Bootstrap` owns the manual composition root. It validates the project-wide input boundary and is the only place intended to assemble future application and infrastructure services. No dependency-injection framework is installed.

Issue #4 adds the first infrastructure adapter, `SeededRandomSource`, which directly implements the domain-owned randomness boundary. This direct inward reference keeps the domain independent while allowing replayable seeded execution.

## Scenes

The enabled build scenes are ordered as follows:

| Order | Scene | Purpose |
| ---: | --- | --- |
| 0 | `Bootstrap` | application startup and persistent manual dependency composition |
| 1 | `Home` | localized first-playable flow plus the authoritative fixed-camera 1v1 table and resolved-event animation presentation |
| 2 | `MatchPrototype` | fixed-camera 1v1, three-player, and 2v2 table-composition prototype |
| 3 | `AnimationLab` | Edit Mode library and runtime preview for isolated reusable resolved-event animations, versioned presets, transport, profile comparison, and synchronization diagnosis |
| 4 | `AssetReview` | isolated generated-asset inspection with Play-mode orbit and zoom controls |

Bootstrap remains deliberately minimal and composes the first-playable flow before loading `Home`. Home owns the functional application flow documented in [first-playable application flow](../technical/first-playable-flow.md) and the integrated table documented in [first-playable 1v1 table presentation](../technical/first-playable-table.md). `AnimationLab` owns the isolated real-time sequence workbench documented in [gameplay animation](../technical/animation.md). `MatchPrototype` retains the presentation-only multi-mode composition evidence documented in the [fixed table composition prototype](../design/table-composition-prototype.md). `AssetReview` remains an isolated generated-asset inspection scene.

## Input, localization, and UI

`TheFallInput.inputactions` is the project-wide Input System asset. Its `Gameplay` map names shared `Point`, `Navigate`, `Inspect`, `Select`, `Confirm`, and `Cancel` intents and provides touch, mouse, and keyboard bindings. The presentation adapter resolves these action names but does not validate or execute game rules.

English (`en`) is the project source locale. Pseudo-localization (`qps-ploc`) is enabled with the package's expansion, accenting, and encapsulation transforms. The `UI` string table contains `app.title` plus the stable Home, setup, loading, match-action, result, canto, suit, score, and navigation keys required by the first playable. Dynamic entries use Smart Strings, and pseudo-localization transforms the same source entries.

UI Toolkit owns the adaptive screen-space first-playable flow in `Home`. A reusable world-space Canvas/TextMeshPro prefab establishes the uGUI path without hard-coded player-facing text.

## Player settings

- company: `Donkijote`
- product: `The Fall`
- pre-release bundle version: `0.0.0`
- application identifier: `com.donkijote.thefall` for Standalone, Android, and iOS
- mobile orientation: automatic rotation with portrait and both landscape directions enabled
- desktop window: resizable
- active input handling: Input System

Minimum OS/API versions, desktop fullscreen behavior, signing, and stores remain open as documented in [platform requirements](../technical/platforms.md).

## Local validation

Use the matching Unity editor executable for `ProjectSettings/ProjectVersion.txt`. The repository runner owns the repeatable foundation, suite, and build-smoke sequence:

```sh
export UNITY_THE_FALL="/Applications/Unity/Hub/Editor/6000.5.4f1/Unity.app/Contents/MacOS/Unity"

scripts/validate-unity.sh tests
scripts/validate-unity.sh smoke macos
scripts/validate-unity.sh smoke ios
scripts/validate-unity.sh smoke ios-simulator
scripts/validate-unity.sh all macos
```

See [testing and platform validation baseline](validation.md) for focused failure replay, all platform smoke arguments, result locations, the manual matrix, and evidence requirements.
See [iOS development builds](ios-development-builds.md) for the local Xcode signing boundary, physical-iPhone procedure, simulator path, retained-scene launch arguments, and device-specific diagnosis.

The editor menu `The Fall > Foundation > Generate` creates missing foundation assets and applies project settings without replacing scenes or UI prefabs that already exist. `The Fall > Foundation > Validate` performs the non-test structural checks.

## Validation checkpoint

Validated on 2026-07-19 with Unity `6000.5.4f1`:

- foundation generator and structural validation: passed
- project Edit Mode tests: 2 passed
- project Play Mode tests: 1 passed
- macOS universal player smoke build: succeeded

The issue #3 checkpoint included only macOS Standalone and WebGL support. Issue #30 later installed and verified the matching iOS module and Xcode toolchain without changing the provisional iOS `15.0` target or committing local signing data. See [iOS development builds](ios-development-builds.md) for the current iOS checkpoint. Android and Windows modules/toolchains remain unavailable and are not claimed.

## Git LFS audit

The repository `.gitattributes` covers the retained model, image, audio, video, font, archive, and binary formats. The retained Liberation Sans source font used by TextMeshPro is stored through Git LFS. New accepted binary assets must continue to follow the provenance and storage rules in [asset strategy](../assets/strategy.md).
