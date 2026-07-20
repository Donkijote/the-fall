# Experience Design

Status: Draft

## Camera

**Confirmed:** Use a completely stationary fixed cinematic camera during gameplay, looking down toward the table. It provides an elevated, readable view and does not zoom, pan, orbit, or move for deals, captures, cantos, scoring, or victory.

Issue #6's working camera, seat-anchor, orientation, safe-area, and readability values are recorded in the [fixed table composition prototype](table-composition-prototype.md). Those numeric values are V0 evidence rather than final production approval.

Lobby and home-screen camera behavior will be designed separately and may use a different presentation model.

## Player representation

**Confirmed:** Show character upper bodies around the table. The framing should communicate who occupies each seat without requiring full-body characters.

Display the player's name near or above the character. A user avatar may be added later if it improves identification without cluttering the 3D scene.

Story Mode uses a predefined character cast. Character creation and customization are deferred as possible future-version features.

Anchor the local player at the bottom of the gameplay composition. Three-player opponents occupy the remaining evenly distributed seats with randomized identity placement. In 2v2, the local player's teammate sits opposite and opponents occupy the other two seats.

Captured piles remain visually owned by the character who earned them even when team rules aggregate their values. Never expose a teammate's private hand.

## Interaction

**Confirmed:**

- mobile uses touch input
- desktop uses mouse and keyboard
- mobile supports landscape and portrait orientation

**Confirmed interaction principle:** Inputs express game intent—select card, inspect card, play card, confirm choice, or cancel—while deterministic rules decide whether and how the intent resolves. Captures are mandatory and automatic when a played rank is already on the table; the player does not choose whether or what to capture.

Issue #7's working shared-intent sequence, touch/mouse/keyboard mappings, semantic feedback states, and orientation-preservation behavior are recorded in the [cross-platform card interaction prototype](card-interaction-prototype.md). Those controls and visual treatments are V0 evidence rather than final production approval.

## Readability

- the active player must be unmistakable
- the local hand must remain readable in both orientations
- scores, teams, turn order, and canto state must not depend only on color
- important captures and scoring events should be visually traceable
- animation should not block essential decisions longer than necessary

## Open experience decisions

- production camera angle, field of view, and safe framing thresholds beyond the V0 prototype
- production tap-versus-drag behavior and input customization beyond the V0 tap/select/confirm prototype
- card inspection and zoom behavior
- portrait-mode seating and UI composition
- how remote player hands are represented
- avatar source and placement
- accessibility options, animation speed, and reduced-motion behavior
