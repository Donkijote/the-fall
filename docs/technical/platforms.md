# Platform Requirements

Status: Draft

## Confirmed targets

| Platform | Input | Orientation/window |
| --- | --- | --- |
| Android | Touch | Landscape and portrait |
| iOS | Touch | Landscape and portrait |
| Windows | Mouse and keyboard | Resizable desktop window and fullscreen to be defined |
| macOS | Mouse and keyboard | Resizable desktop window and fullscreen to be defined |

Controller support is not currently required.

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
