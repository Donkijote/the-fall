# Landscape-only mobile presentation

Status: Accepted
Date: 2026-07-29

## Context

The project previously supported separate portrait and landscape compositions on phones and tablets.
Maintaining both doubled layout, safe-area, capture, and physical-device validation work while the
gameplay table and current product direction are designed around a horizontal field of view.

## Options

- keep separate portrait and landscape mobile compositions
- support portrait only
- support landscape only while allowing both landscape directions

## Decision

All Android and iOS phone and tablet builds are landscape-only. Unity uses automatic rotation with
Landscape Left and Landscape Right enabled; Portrait and Portrait Upside Down are disabled. This lets
a player rotate the device 180 degrees without ever entering a portrait composition.

Desktop windows remain resizable. All future mobile UI, gameplay, planning, acceptance criteria,
captures, and device validation must use representative phone and tablet landscape viewports. Mobile
runtime presentation resolves to Phone Landscape or Tablet Landscape from the safe viewport's
orientation-independent aspect ratio. Both inherit the shared `profile-mobile-landscape` rules and
may add form-factor-specific overrides; neither can select portrait.

## Consequences

- Mobile UI can spend its layout and validation budget on one authored horizontal composition.
- Safe-area checks must cover both landscape directions and representative phone/tablet cutouts.
- New work must not add portrait breakpoints, portrait acceptance criteria, or portrait-only assets.
- Historical portrait captures and recorded validation remain evidence of earlier work, not current
  product requirements.
- Removing unreachable legacy portrait USS rules is permitted as focused cleanup, but they cannot be
  selected at runtime.
