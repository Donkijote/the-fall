# Split the first playable into presentation scenes

Status: Accepted
Date: 2026-07-29
Supersedes: [ADR 0004](0004-on-demand-ui-screen-composition.md) for Unity scene ownership

## Context

ADR 0004 separated each UI screen into an authoritative UXML/USS pair but mounted every pair inside
one persistent `Home` Unity scene. That solved overlapping UI authoring, but Login, Hub, and Match
still shared one scene lifecycle. The project now needs room for scene-specific cameras, world
objects, transitions, loading treatments, animation staging, and future effects without accumulating
all presentation objects in one hierarchy.

The deterministic application flow, match orchestration, user preferences, and localization state
must survive presentation-scene replacement. Pure UI substates do not each justify another Unity
scene.

## Decision

`Bootstrap` remains the sole application entry point and persistent composition root. It owns the
authoritative `FirstPlayableFlow` and presentation-session preferences, then loads one mutually
exclusive presentation scene:

- `Login` owns the gateway UI and its screen-local `UI`/`Styles` assets.
- `Hub` owns the hub and settings UI. The legacy setup screen remains a Hub substate.
- `Match` owns loading, the fixed-camera table, match HUD, result presentation, animation, and audio.

Each scene has one `UIDocument` and one `FirstPlayableFlowController`. The document source asset maps
directly to the scene's authoritative screen: `LoginScreen.uxml`, `HubScreen.uxml`, or
`MatchScreen.uxml`. A controller may replace that document tree only with a secondary screen asset
owned by the same scene. Scene changes use `LoadSceneMode.Single`; the Bootstrap composition root
survives with `DontDestroyOnLoad`, so replacing a scene never recreates authoritative application or
domain state.

Each screen asset also owns its `Bitbebop.SafeArea`. Scene controllers select responsive profiles but
do not apply physical UI insets.

Screen assets use `Screen/<Name>/UI` and `Screen/<Name>/Styles` folders. Cross-screen USS rules live
under `Screen/Shared/Styles`; screen-specific profile cascades stay inside that screen's Styles
folder. Moving these assets must preserve their Unity metas so serialized scene references remain
stable.

Each authoritative UXML owns an `AdaptiveUiPreviewRoot` whose Inspector exposes Phone Landscape,
Tablet Landscape, and Desktop authoring profiles. UI Builder uses that root for a runtime-equivalent
logical viewport, representative safe area, and responsive USS classes. The controller strips those
preview-only constraints when it mounts the tree. Its responsive-profile path executes in Edit Mode
so an open scene preview follows the profile saved in the UXML; Play Mode uses the simulated/device
platform and safe viewport. Flow state, navigation, binding, and rendering remain Play-Mode-only.

Loading and Result remain Match-scene UI substates because they use the same table/session lifecycle.
Settings remains a Hub substate because it does not introduce a different world composition.

## Consequences

- Login, Hub, and Match can independently gain scene objects, transitions, cameras, lighting, and
  animation staging.
- Opening a presentation scene in Unity immediately exposes its authoritative screen asset on the
  `Screen UI` document; there is no generic shell asset to edit or accidentally assign.
- Selecting `AdaptiveUiPreviewRoot` in UI Builder provides a single screen-local profile switch
  without duplicating the hierarchy or depending on a scene MonoBehaviour.
- The Match scene is the only production scene that contains the 3D table, gameplay camera, resolved
  event animation presenter, and prototype audio presenter.
- Rules, audio, motion, and local chat presentation preferences live in Bootstrap-owned session
  state so they survive Hub-to-Match-to-Hub navigation.
- Tests must wait for scene transitions and reacquire scene-owned controllers and UI documents.
- Adding a UXML screen does not automatically require adding a Unity scene. A new scene is justified
  only when its world objects, lifecycle, loading boundary, or transition staging differ materially.
