# Platform Requirements

Status: Confirmed targets with open support commitments

## Confirmed targets

| Platform | Input | Orientation/window |
| --- | --- | --- |
| Android | Touch | Landscape and portrait |
| iOS | Touch | Landscape and portrait |
| Windows | Mouse and keyboard | Resizable desktop window and fullscreen to be defined |
| macOS | Mouse and keyboard | Resizable desktop window and fullscreen to be defined |

Controller support is not currently required.

## First playable commitment

**Confirmed:** Accept the first playable as a macOS universal development player on the project-owned Apple silicon reference Mac with mouse and keyboard. Required resizable layouts are `1280 x 720`, `1440 x 900`, `1920 x 1080`, and `2560 x 1440`; fullscreen is not an acceptance gate.

Android, iOS, and Windows remain confirmed product targets. Existing simulated touch, portrait, landscape, safe-area, and orientation coverage must keep passing, but the first playable makes no broad physical-device, signing, store, or production-support claim for those platforms. Issue #30 establishes local development deployment to Manuel's iPhone, and #31 records a passing manual first-playable checkpoint plus loading, memory, thermal, and state-agreement evidence on that High-tier phone without changing the macOS gate. The measured wall-clock and CPU p95 values narrowly miss the stricter Reference-mobile frame-time gate; focused follow-up #42 owns that work. Reference and Constrained iOS rows remain open. See the [first playable milestone](../planning/first-playable-milestone.md).

## Shared requirements

- one game-intent model across input methods
- responsive table and UI composition
- safe-area handling on mobile
- readable cards, names, scores, and actions at target resolutions
- input prompts appropriate to the active platform
- no rule differences caused by platform or orientation

## Orientation strategy

**Confirmed:** Mobile supports both landscape and portrait.

**Confirmed:** Rotating the device during an active match immediately recomposes the camera-safe presentation and UI without restarting or changing the match state.

**Proposed:** Treat each orientation as an authored composition sharing the same game state, rather than scaling one layout mechanically.

Issue #6 implements and tests that proposal with three authored layout profiles and normalized safe-area recomposition. See [fixed table composition prototype](../design/table-composition-prototype.md) for its working parameters and remaining device-validation gaps.

Issue #7 keeps selection and inspection in application-owned interaction state while those profiles rebuild their generated card views. Its touch, mouse, and keyboard mappings produce the same application intent sequence without platform-specific rules. See [cross-platform card interaction prototype](../design/card-interaction-prototype.md).

## Open platform decisions

- minimum Android API and device tier
- minimum iOS version and device tier
- Windows and macOS minimum versions
- target frame rate by device class
- render-scale and quality tiers
- animation and interaction behavior while an orientation transition is in progress
- desktop window minimum size and aspect-ratio constraints
- distribution stores and signing workflow

The initial player settings and currently validated build target are recorded in [bootstrap and validation](../development/bootstrap-validation.md).

The initial device/OS lanes, mobile tiers, desktop resolutions, manual procedure, build-smoke paths, and V0 performance gates are recorded in [testing and platform validation baseline](../development/validation.md). Those lanes intentionally use relative current/oldest-candidate OS coverage and do not settle the minimum-version decisions above.

The repository-safe device export, local Xcode signing boundary, physical-iPhone checklist, and supplemental simulator path are recorded in [iOS development builds](../development/ios-development-builds.md). That checkpoint retains iOS `15.0` as a provisional project value and does not turn it into a production support commitment.
