# Fixed Table Composition Prototype

Status: V0 prototype evidence

## Purpose and boundary

Issue #6 uses inexpensive runtime geometry in `MatchPrototype` to prove the fixed gameplay-camera composition before final assets, interaction, or game rules are added. The prototype is presentation-only: it does not create or mutate domain match state.

The fixed camera, local-bottom seat, logical counter-clockwise order, opposite teammates, private hands, individual capture-pile ownership, and immediate orientation recomposition follow accepted project decisions. The numeric camera and layout values below are working V0 evidence rather than production approval.

## Stationary camera

The camera never pans, orbits, zooms, shakes, or changes projection during gameplay or recomposition.

| Parameter | Prototype value |
| --- | ---: |
| Position | `(0, 7.2, -5.4)` metres |
| Rotation | `(52, 0, 0)` degrees |
| Vertical field of view | `44` degrees |
| Near / far clip | `0.1 / 50` metres |
| Projection | Perspective |

Portrait and safe-area changes move and uniformly scale the presentation root; they never change the camera transform or field of view.

## Table and readability blockout

- table diameter: `1.45 m`
- table height: `0.79 m` at the play surface
- quiet central field diameter: `0.98 m`
- seat radius: `1.50–1.65 m` by profile
- cards: vellum faces with large shape marks; woad backs with a direction-neutral mark
- names and scores: world-space text above each upper-body silhouette
- active seat: brass shape cue plus a `▶` text cue, so activity does not depend on color alone
- teams: alternating `T1` / `T2` text plus moss / woad body blocks
- captured piles: separate named anchors beside every character, including teammates
- hands: only the local player's three cards show faces; every remote hand, including the teammate's, shows backs

The blockout uses the accepted lampblack, walnut, vellum, moss, woad, madder, and brass relationships. It intentionally does not claim final character, card, table, environment, typography, or material art.

## Seat anchors

Angles advance counter-clockwise from the local player at `0°`, visually anchored at the bottom.

| Mode | Logical seat angles | Team order |
| --- | --- | --- |
| 1v1 | `0°, 180°` | `T1, T2` |
| Three-player | `0°, 120°, 240°` | individual `T1, T2, T3` |
| 2v2 | `0°, 90°, 180°, 270°` | `T1, T2, T1, T2` |

The `180°` 2v2 seat is the local player's teammate. Identity data may be assigned to non-local logical seats without changing their counter-clockwise order.

## Orientation and safe areas

This section records the original V0 prototype, including its portrait profile. Portrait is now
unsupported on phones and tablets under [ADR 0003](../decisions/0003-landscape-only-mobile.md);
future capture and validation sets use representative phone and tablet landscape viewports.

The layout chooses one authored profile from the active viewport:

| Profile | Selection | Root scale | Seat radius |
| --- | --- | ---: | ---: |
| Portrait | width below height | `0.72` | `1.50 m` |
| Standard landscape | aspect below `1.7` | `1.15` | `1.60 m` |
| Wide landscape / desktop | aspect at least `1.7` | `1.35` | `1.65 m` |

The safe rectangle is normalized and clamped to the viewport. The presentation root receives a uniform safe-area scale clamped to `0.78–1.00` and an offset derived from the safe rectangle's center. Uniform scaling keeps the round table round. Recomposition rebuilds view geometry while preserving seating mode, active seat, representative scores, privacy, and the presentation-state version.

Automated checks exercise `390×844` portrait, `844×390` landscape with side insets, `1440×1080` standard desktop, and `1920×1080` wide desktop classifications. The authored profiles should also be manually reviewed at `1280×720`, `1440×900`, `1920×1080`, and `2560×1440`, plus representative notched devices, before production approval.

## Validation ownership

- Edit Mode verifies seat count, bottom anchoring, counter-clockwise order, 2v2 team opposition, remote-hand privacy, profile selection, and safe-area normalization.
- Play Mode loads `MatchPrototype`, exercises every seating mode, verifies individual capture anchors, and proves portrait recomposition preserves active presentation state and camera pose.
- `The Fall > Table Composition > Generate` updates the scene with the prototype component and fixed camera values.
- `The Fall > Table Composition > Validate` checks that the component and stationary camera parameters are present.
- `The Fall > Table Composition > Capture Validation Set` writes phone- and tablet-landscape captures for all three modes to the ignored `Logs` directory for visual review.

## Validation checkpoint

Validated on 2026-07-20 with Unity `6000.5.4f1`:

- table-composition generation and structural validation: passed
- Edit Mode run: 17 tests passed (16 project tests and 1 package test)
- project Play Mode suite: 3 tests passed, including all six mode/orientation combinations
- portrait and landscape camera captures for all three modes: visually reviewed
- macOS universal player smoke build: succeeded

This checkpoint proves editor simulation and the available desktop build path. It does not claim physical Android or iOS device readability or performance.

## Unresolved constraints

- Real phone and tablet captures are still required; editor aspect simulation is not device validation.
- Minimum desktop window size and final safe-area padding remain open platform decisions.
- Final font, localization expansion, text contrast, and accessibility thresholds require representative UI assets and device testing.
- Character silhouettes, gestures, and occlusion must be repeated with issue #8 prototype assets.
- Card overlap, inspection size, and touch targets belong to issue #7.
- Remote-hand abstraction and animation envelopes may revise the non-production anchor offsets without changing logical seat order or privacy.
- Measured performance and screenshot-regression tooling belong to issue #10.

Related: [experience design](experience.md), [art direction](art-direction.md), [platform requirements](../technical/platforms.md), and [testing strategy](../technical/testing.md).
