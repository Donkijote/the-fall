# Platform Requirements

Status: Confirmed targets with open support commitments

## Confirmed targets

| Platform | Input | Orientation/window |
| --- | --- | --- |
| Android | Touch | Landscape only; both landscape directions |
| iOS | Touch | Landscape only; both landscape directions |
| Windows | Mouse and keyboard | Resizable desktop window and fullscreen to be defined |
| macOS | Mouse and keyboard | Resizable desktop window and fullscreen to be defined |

Controller support is not currently required.

## First playable commitment

**Confirmed:** Accept the first playable as a macOS universal development player on the project-owned Apple silicon reference Mac with mouse and keyboard. Required resizable layouts are `1280 x 720`, `1440 x 900`, `1920 x 1080`, and `2560 x 1440`; fullscreen is not an acceptance gate.

Android, iOS, and Windows remain confirmed product targets. Simulated touch, landscape safe-area, and
landscape-direction rotation coverage must keep passing, but the first playable makes no broad
physical-device, signing, store, or production-support claim for those platforms. Issue #30
establishes local development deployment to Manuel's iPhone, and #31 records earlier physical-device
evidence without changing the macOS gate. See the [first playable milestone](../planning/first-playable-milestone.md).

## Shared requirements

- one game-intent model across input methods
- responsive table and UI composition
- safe-area handling on mobile
- readable cards, names, scores, and actions at target resolutions
- input prompts appropriate to the active platform
- no rule differences caused by platform or orientation

## Orientation strategy

**Confirmed:** Phones and tablets support landscape only. Both Landscape Left and Landscape Right
remain enabled so rotating the device 180 degrees recomposes safely without entering portrait.

**Confirmed:** Changing between the two landscape directions during an active match immediately
recomposes the camera-safe presentation and UI without restarting or changing match state.

**Confirmed for V0.1:** Treat desktop and mobile landscape as authored compositions sharing the same
game state, rather than scaling one layout mechanically. Issue #43 implements the screen-space
profile and safe-area foundation and records its measurable minimums in the
[adaptive UI foundation](../design/adaptive-ui-foundation.md).

Earlier portrait prototype evidence is historical and is not a current product or acceptance
requirement. [ADR 0003](../decisions/0003-landscape-only-mobile.md) owns the landscape-only decision.

Issue #7 keeps selection and inspection in application-owned interaction state while those profiles rebuild their generated card views. Its touch, mouse, and keyboard mappings produce the same application intent sequence without platform-specific rules. See [cross-platform card interaction prototype](../design/card-interaction-prototype.md).

## Open platform decisions

- minimum Android API and device tier
- minimum iOS version and device tier
- Windows and macOS minimum versions
- target frame rate by device class
- render-scale and quality tiers
- animation and interaction behavior while changing landscape direction
- desktop window minimum size and aspect-ratio constraints
- distribution stores and signing workflow

The initial player settings and currently validated build target are recorded in [bootstrap and validation](../development/bootstrap-validation.md).

The initial device/OS lanes, mobile tiers, desktop resolutions, manual procedure, build-smoke paths, and V0 performance gates are recorded in [testing and platform validation baseline](../development/validation.md). Those lanes intentionally use relative current/oldest-candidate OS coverage and do not settle the minimum-version decisions above.

The repository-safe device export, local Xcode signing boundary, physical-iPhone checklist, and supplemental simulator path are recorded in [iOS development builds](../development/ios-development-builds.md). That checkpoint retains iOS `15.0` as a provisional project value and does not turn it into a production support commitment.
