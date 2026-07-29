# Mount UI screens on demand

Status: Superseded by [ADR 0006](0006-first-playable-presentation-scenes.md) for Unity scene ownership
Date: 2026-07-29

## Context

The first-playable flow originally authored Login, Hub, Setup, Loading, Match HUD, and Result as
absolutely positioned siblings in one UXML document and hid inactive stages with `display: none`.
That kept navigation simple but made UI Builder show every screen overlapping, coupled one controller
binding pass to every control, and made isolated screen editing unnecessarily difficult.

## Options

- retain one monolithic UXML tree and hide inactive stages
- create one Unity scene and `UIDocument` for every UI screen
- retain one persistent Unity scene/document shell and mount one screen-specific `VisualTreeAsset`
  at a time

## Decision

The `Home` Unity scene owns one persistent `UIDocument` whose `HomeScreen.uxml` is only the application
shell and `screen-host`. Login, Hub, Setup, Loading, Match, and Result each own a separate UXML and USS.
`FirstPlayableFlowController` mounts exactly one screen asset into `screen-host` for the current
application-flow state, binds only that screen's controls, and removes the previous hierarchy during
navigation.

Application flow, match state, user preferences, localization, safe-area state, and the fixed 3D table
remain outside the mounted screen. Replacing a view must never replace or recreate authoritative
application/domain state.

## Consequences

- UI Builder opens each screen without unrelated overlapping stages.
- A screen's structure and screen-specific styling have one authoritative asset pair.
- Shared tokens, component styles, icon treatment, and adaptive profile rules remain in
  `FlowShared.uss`.
- Screen transitions allocate a small transient UI hierarchy and require one panel/layout update
  before resolved styles can be measured.
- Tests must assert that exactly one screen is mounted and that prior-screen controls are absent.
- Separate Unity scenes are unnecessary for these presentation states; scene changes remain reserved
  for genuinely different world/runtime composition boundaries.

ADR 0006 later accepted Login, Hub, and Match as distinct presentation/world lifecycle boundaries
and superseded the generic `HomeScreen` shell: every scene now references its own authoritative
screen UXML directly. The per-screen UXML/USS ownership and one-current-tree rules from this decision
remain in force inside those scenes.
