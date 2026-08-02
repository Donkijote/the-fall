# Scroll-free responsive player UI

Status: Accepted
Date: 2026-07-29

## Context

Several player-facing screens used `ScrollView` as a fallback when their fixed desktop or enlarged
mobile measurements exceeded the viewport. The match table also applied fixed mobile card
multipliers, making cards occupy too much of a phone's short landscape axis.

Scrolling makes important actions dependent on undiscovered off-screen content and prevents each
screen from presenting one deliberate composition. Fixed multipliers cannot account for the
difference between a wide phone safe area and a more square tablet safe area.

## Decision

Login, Hub, Settings, setup, loading, match, and result must fit within one hardware-safe viewport.
Player-facing runtime UXML must not contain `ScrollView`, and no layout may require horizontal or
vertical panning. Layouts reflow, compact spacing and typography, use horizontal grouping, and remove
decorative emphasis before allowing required information or actions to leave the viewport.

This contract applies to supported desktop and web window sizes and to phone and tablet landscape
viewports in both directions.

Every screen UXML owns one `Bitbebop.SafeArea` containing all interactive controls. The screen
element, rather than the flow controller, converts and applies physical safe-area margins. Full-bleed
atmospheric elements may remain outside that container, but interactive elements may not.

Gameplay cards retain the exact `63:88` aspect ratio. Mobile card and zone spacing multipliers derive
from the normalized safe-area aspect ratio. Local actionable cards remain larger than public cards,
but steady-state local, public, and dealer cards are also bounded to `19%`, `15%`, and `18%` of safe
viewport height respectively. A selected card may briefly exceed its steady-state bound by the
documented interaction emphasis. Local and public visual identity remains at least `34 pt` and
`26 pt` respectively, while required touch targets remain at least `44 pt`; interactive cards use a
larger invisible collider when necessary.

## Consequences

- Every runtime screen is understandable without a scroll gesture.
- Safe-area ownership is visible and editable in each screen asset, with no duplicate controller
  inset path.
- Localization expansion must be handled by wrapping and responsive composition, not a hidden
  scrollbar.
- Settings uses simultaneous responsive groups instead of a scrolling modal body.
- Phone cards no longer inherit the prior fixed `4.10x`, `3.4x`, and `2.8x` scale multipliers.
- Automated coverage rejects player-facing `ScrollView` assets and enforces both minimum identity
  widths and maximum safe-height proportions for mobile cards.
- Physical phone and tablet review remains required for final touch comfort and viewing distance.
