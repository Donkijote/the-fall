# UI Screen Composition

Status: Implemented

## Authoritative assets

Each of the `Login`, `Hub`, and `Match` scenes keeps one `UIDocument`. Its source asset points directly
to that scene's authoritative screen, so selecting `Screen UI` in Unity shows the same UXML that owns
the visible layout.

| Screen | Structure | Screen-specific style |
| --- | --- | --- |
| Login | `Screen/Login/UI/LoginScreen.uxml` | `Screen/Login/Styles/LoginScreen.uss` |
| Hub and Settings | `Screen/Hub/UI/HubScreen.uxml` | ordered `Screen/Hub/Styles/HubScreen.*.uss` cascade described below |
| Legacy explicit setup route | `Screen/Setup/UI/SetupScreen.uxml` | `Screen/Setup/Styles/SetupScreen.uss` |
| Loading | `Screen/Loading/UI/LoadingScreen.uxml` | `Screen/Loading/Styles/LoadingScreen.uss` |
| Match HUD and contextual actions | `Screen/Match/UI/MatchScreen.uxml` | `Screen/Match/Styles/MatchScreen.uss` |
| Result | `Screen/Result/UI/ResultScreen.uxml` | `Screen/Result/Styles/ResultScreen.uss` |

Scene ownership is intentionally coarser than UXML ownership:

| Unity scene | Direct `UIDocument` source | Allowed runtime screens |
| --- | --- | --- |
| Login | `LoginScreen.uxml` | Login |
| Hub | `HubScreen.uxml` | Hub, legacy Setup |
| Match | `MatchScreen.uxml` | Loading, Match, Result |

The on-disk structure is consistent for every screen:

```text
Screen/
  Shared/
    Styles/FlowShared.uss
  <ScreenName>/
    UI/<ScreenName>Screen.uxml
    Styles/<ScreenName>Screen*.uss
```

`Screen/Shared/Styles/FlowShared.uss` owns reusable screen-root, safe-area, typography, panel, button,
semantic-state, adaptive-profile, and icon rules. Put a rule in a screen's `Styles` folder when it
describes only that screen; put it in `FlowShared.uss` only when two or more screens intentionally
share the component contract.

### Hub stylesheet cascade

USS does not support CSS-style nested selectors. This is invalid and must not be used:

```css
.screen-root.profile-desktop {
    .hub-layout {
        padding: 24px;
    }
}
```

Repeat the complete selector instead:

```css
.screen-root.profile-desktop .hub-layout {
    padding: 24px;
}
```

`HubScreen.uxml` loads its styles in this explicit order:

1. `FlowShared.uss` — profile-neutral cross-screen foundations and reusable components;
2. `HubScreen.Base.uss` — Hub rules shared by every supported profile;
3. `HubScreen.Desktop.uss` — `.screen-root.profile-desktop` overrides;
4. `HubScreen.PhoneLandscape.uss` — `.profile-phone-landscape` overrides;
5. `HubScreen.TabletLandscape.uss` — `.profile-tablet-landscape` overrides.

Use the narrowest file that owns the intended behavior. A value common to every Hub profile belongs
in Base. Form-factor-specific exceptions belong in their named file. Keep the full profile prefix on
every override so loading all four assets never activates the wrong composition. `FlowShared.uss`
contains no profile-qualified or scene-specific selectors, so an empty Desktop, Phone, or Tablet
file means Base is the only Hub composition. Unsupported portrait Hub rules are not retained.

## Runtime lifecycle

`Bootstrap` owns `FirstPlayableFlow` and `FirstPlayablePresentationState` across scene loads.
`FirstPlayableFlowController` maps the current gateway/flow state to both a presentation scene and a
`FirstPlayableScreenKind`. When the required scene changes, it replaces the current presentation
scene with `LoadSceneMode.Single`; the Bootstrap root survives. Inside the active scene, the
controller binds the direct source tree when it already represents the current screen. Changing to
another screen kind owned by that scene:

1. clears references to the previous screen;
2. removes the previous visual tree from the document root;
3. clones the selected serialized `VisualTreeAsset`;
4. queries and binds only that screen's controls;
5. reapplies the current adaptive profile;
6. renders localized values from the existing application and match state.

Exactly one screen tree is present. Audio, motion, rule, gateway, and local-chat preferences live in
Bootstrap-owned presentation-session state rather than in a scene controller or detached toggle, so
Hub-to-Match-to-Hub navigation restores them without retaining the Hub hierarchy. The deterministic
flow and match never live in a `VisualElement` or presentation scene.

## Responsive viewport contract

Every player-facing screen fits one safe viewport without horizontal or vertical scrolling on the
supported desktop, web, phone-landscape, and tablet-landscape layouts. Runtime screen UXML must not
use `ScrollView`. Screens use flex reflow, bounded panels, compact profile-specific spacing, and
horizontal grouping; decorative content yields before required information or actions.

Safe-area insets constrain interactive content, not atmospheric presentation. In particular, the
Login background artwork and scrim remain full-bleed behind notches and rounded display corners,
while the hero copy and gateway form are inset into the safe area.

Every screen UXML owns exactly one `Bitbebop.SafeArea`, and every interactive control is nested under
it. That element is the sole UI safe-area owner: it converts the physical safe rectangle to panel
margins and responds to geometry/orientation changes. `FirstPlayableFlowController` must not write
safe-area offsets into stages or content, avoiding double insets and keeping authoring ownership in
the screen asset.

Settings is a three-group responsive composition for rules, audio, and motion rather than a scrolling
modal body. This contract is enforced by Edit Mode source coverage and
[ADR 0005](../decisions/0005-scroll-free-responsive-player-ui.md).

Hub uses the same Base composition for Desktop, Phone Landscape, and Tablet Landscape while its
profile files remain empty. Identity, resources, and global actions occupy the top row; objective,
persistent navigation, and chat share the lower row. The chat panel owns overflow containment: its
rounded, inset tab rail and composer remain inside the panel boundary rather than using the outer
border as their content box.

The controller reads `UnityEngine.Device.Screen` and
`UnityEngine.Device.Application.isMobilePlatform` only to select and refresh the authored responsive
profile. Mobile safe-viewport aspect selects the Phone Landscape or Tablet Landscape variant;
desktop and web use Desktop. `Bitbebop.SafeArea` encapsulates the UI Toolkit panel-space safe-area
conversion. Do not add a second controller-side inset path; profile selection and physical edge
containment intentionally have separate owners.

Icon-only buttons localize their `tooltip`, never their visible `text`. A graphic and a localized
label must not occupy the same button content box; this is enforced by Edit Mode source coverage.

## Editing workflow

Open the individual screen UXML in UI Builder. For Login, Hub, or Match, the scene's `Screen UI`
document must reference that same named screen asset. Do not introduce a generic document shell or
duplicate a screen hierarchy directly into a Unity scene.

Every authoritative screen has one `AdaptiveUiPreviewRoot`, selected in UI Builder's Hierarchy.
Select it and change **Preview Profile** in the Inspector to switch among:

- `Phone Landscape`: `1920 x 887` logical preview with representative side cutouts;
- `Tablet Landscape`: `1920 x 1440` logical preview with representative tablet edge insets;
- `Desktop`: `1920 x 1080` logical preview without hardware insets.

Use UI Builder's Fit Canvas or zoom controls after switching profiles. The logical sizes mirror the
project's reference-panel behavior; they are intentionally not physical pixel claims. The preview
safe area is representative, so final cutout behavior remains a Device Simulator/runtime concern.

Keep each screen's responsive rules in that screen's own USS file. Put shared phone/tablet rules
under `.screen-root.profile-mobile-landscape`, and deliberate variants under
`.screen-root.profile-phone-landscape`, `.screen-root.profile-tablet-landscape`, or
`.screen-root.profile-desktop`. The preview root applies the same classes in UI Builder, making USS
changes immediately visible without entering Play Mode. Do not put profile-qualified selectors or
screen-owned component selectors in `FlowShared.uss`.

`profile-mobile-landscape` is a shared selector class, not a fourth Preview Profile. Phone receives
both `profile-mobile-landscape` and `profile-phone-landscape`; tablet receives both
`profile-mobile-landscape` and `profile-tablet-landscape`; desktop receives only
`profile-desktop`. This avoids duplicating identical phone/tablet rules while leaving a precise
override class for each selectable form factor.

With empty Hub profile files, Desktop, Phone Landscape, and Tablet Landscape therefore resolve the
same Hub declarations from Base. Their rendered geometry can still differ because each preview uses
its own aspect ratio and representative safe-area insets; that is viewport reflow, not a hidden USS
override.

Preview Profile switches active classes; it does not create per-profile copies of UXML Inline Styles.
An inline Inspector value is shared by every profile and wins over USS selectors. Keep common values
in the base screen selector, remove a property from Inline Styles before making it responsive, and
store each different value in its profile-qualified USS selector. Switching profiles then reveals
the independently saved USS values instead of replacing them.

This remains a visual UI Builder workflow: select or add the qualified selector in the
**StyleSheets** pane, then edit its Padding, Size, Flex, or other USS properties in the Inspector.
Repeat for the other profile selectors and use **Preview Profile** to compare them. Do not edit the
same property under the element's **Inline Styles** section afterward, because that shared inline
value will override all three selectors. UI Builder does not provide profile tabs inside Inline
Styles; a generic tabbed responsive-style inspector would require a separate custom editor tool.

When a UXML is cloned into a scene, `FirstPlayableFlowController` removes the preview-only dimensions,
sample safe-area styling, and profile classes from `AdaptiveUiPreviewRoot`. It then applies
`screen-root` and the actual authoring/runtime profile to the `UIDocument` root. In Edit Mode, the
scene preview follows the profile saved in that UXML's preview root. In Play Mode, the real
simulated/device platform and safe viewport are authoritative. Application flow, control binding,
navigation, and localization refresh remain Play-Mode-only.

Static labels, buttons, and toggles bind directly to the `UI` localization table in UXML. UI Builder
resolves those bindings with the project's authoring locale, so the preview shows the same localized
copy that the game uses. Edit authoritative copy in the Localization Tables window, not by replacing
the binding with a hard-coded `text` value.

Labels whose values come from live match, session, or validation state include a representative
`text` value for layout preview only. `FirstPlayableFlowController` replaces those samples from the
authoritative runtime state before the active screen is painted. Keep preview values realistic enough
to exercise wrapping and spacing, but do not treat them as player-facing copy.

For a new screen:

1. create `Screen/<Name>/UI` and `Screen/<Name>/Styles`, place its UXML and USS assets in their
   corresponding folders, and wrap its hierarchy in one `AdaptiveUiPreviewRoot`;
2. add localization bindings and preview-only values for dynamic labels;
3. prove that every required control fits without adding a `ScrollView`;
4. add a `FirstPlayableScreenKind` and assign it to Login, Hub, or Match scene ownership;
5. serialize and configure its `VisualTreeAsset` in `FirstPlayableFlowController` and
   `FirstPlayableFlowSetup`;
6. add a screen-scoped binding method;
7. update Edit Mode scene-mapping coverage and PlayMode navigation coverage to prove that the correct
   source asset and presentation scene are active, the old controller is gone, and only the current
   screen tree is present.

Create another Unity scene only when the screen introduces a materially different world composition,
camera, lifecycle, loading boundary, or transition staging requirement. A modal or short-lived UI
substate belongs in its existing presentation scene.

Run `The Fall > First Playable Flow > Generate` after adding or replacing screen assets so the scene
references remain reproducible. Run `scripts/validate-unity.sh tests` before handing off the change.
