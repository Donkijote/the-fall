# iOS Development Builds

Status: Confirmed local development path

## Purpose and boundary

Issue #30 establishes one repository-safe path from the matching Unity editor to a locally signed Xcode development build on Manuel's physical iPhone. It is early device evidence, not a production iOS support floor, App Store configuration, TestFlight, distribution signing, or first-playable iOS acceptance.

The physical phone is the acceptance target. The simulator path is useful for compilation and launch diagnosis, but it cannot prove physical touch, hardware safe areas, rotation sensors, signing, installation, thermal behavior, or device performance.

## Recorded toolchain and device

Recorded on 2026-07-26:

| Item | Recorded value |
| --- | --- |
| Unity editor | `6000.5.4f1` (`d550df8bd089`) |
| Unity module | matching iOS Build Support under `PlaybackEngines/iOSSupport` |
| Xcode | `26.6` (`17F113`) |
| iPhoneOS SDK | `26.5` |
| Simulator runtime | iOS `26.5` (`23F77`), arm64 |
| Physical device | iPhone 17 Pro (`iPhone18,1`) |
| Device OS | iOS `26.5.2` (`23F84`) |
| Chip | A19 Pro |
| Memory | not published by Apple and not exposed by the local device inventory; do not infer a value |
| Display | 6.3-inch Super Retina XDR OLED, `2622 x 1206` at 460 ppi |
| Refresh rate | adaptive ProMotion up to 120 Hz |

The hardware facts come from [Apple's iPhone 17 Pro technical specifications](https://support.apple.com/en-us/125090). The project retains the provisional iOS `15.0` deployment target because this issue does not select a production minimum OS or device tier.

## Repository and secret boundary

The repository owns:

- `com.donkijote.thefall`
- automatic rotation with Landscape Left and Landscape Right enabled and both portrait directions disabled
- arm64 simulator exports for Apple silicon hosts
- the provisional iOS `15.0` deployment target
- unsigned Unity Xcode exports under ignored `Build/Smoke/`
- ignored Xcode Derived Data, logs, test results, captures, and profiler evidence

The local developer environment owns:

- Apple Account and Personal Team selection
- development certificates and private keys
- provisioning profiles
- physical device identifiers and pairing records
- account credentials and two-factor authentication

Never add a team ID, certificate, private key, provisioning profile, device identifier, Apple Account address, or other account-specific value to `ProjectSettings`, scripts, documentation, the generated Xcode project, commits, issues, or pull requests. The generated Xcode project and all build evidence remain ignored.

## One-time setup

1. Install the Unity editor version from `ProjectSettings/ProjectVersion.txt` and add its **iOS Build Support** module in Unity Hub.
2. Install Xcode, select it under **Xcode > Settings > Locations > Command Line Tools**, then complete first-launch setup:

   ```sh
   xcodebuild -runFirstLaunch
   ```

3. In **Xcode > Settings > Components**, install a simulator/runtime matching the selected Xcode SDK when Xcode reports that the iOS platform is unavailable.
4. In **Xcode > Settings > Apple Accounts**, sign in locally and confirm a Personal Team or other authorized development team is visible.
5. Connect the iPhone over USB for the first deployment, unlock it, accept **Trust This Computer** on the phone if offered, and confirm **Settings > Privacy & Security > Developer Mode** is enabled. Restart the phone when iOS requests it.
6. Keep the phone unlocked while Xcode prepares device support. After one successful cable deployment, wireless deployment may be enabled from Xcode's Devices and Simulators window while both devices share a trusted network.

The connected phone must be paired, Developer Mode enabled, and available to Xcode. A simulator, a paired-but-locked device, or a successful Unity export does not satisfy that condition.

## Export, sign, install, and launch

From the repository root:

```sh
scripts/validate-unity.sh smoke ios
open Build/Smoke/iOS/Unity-iPhone.xcodeproj
```

The export writes an unsigned development Xcode project to `Build/Smoke/iOS/`. In Xcode:

1. Select the `Unity-iPhone` project and `Unity-iPhone` target.
2. Open **Signing & Capabilities**.
3. Enable **Automatically manage signing** and select the local authorized team. Confirm the bundle identifier remains `com.donkijote.thefall`.
4. Select the unlocked physical iPhone as the run destination.
5. Press **Run**. Xcode must build, sign, install, and launch the app.

Team selection belongs only to the ignored generated project. A fresh Unity export may recreate that project, so repeat the local team selection when Xcode requests it. Never copy the selected team back into Unity `ProjectSettings`.

## Retained-scene launch checks

Every device check begins through the `Bootstrap` scene. A normal development launch must transition
from Bootstrap to `Login`.

Development builds accept an optional launch argument for the retained scene paths:

```text
--the-fall-scene Login
--the-fall-scene Hub
--the-fall-scene Match
--the-fall-scene MatchPrototype
--the-fall-scene AnimationLab
```

In Xcode, use **Product > Scheme > Edit Scheme > Run > Arguments** and add the flag and scene as two consecutive arguments, for example `--the-fall-scene` followed by `MatchPrototype`. Enable exactly one pair before pressing Run. The override is ignored in non-development builds and rejects scenes outside this checklist.

For each launch, record the commit, scene, orientation, visible safe-area behavior, and whether touch responds. Check:

1. normal Bootstrap-to-Login launch and Login-to-Hub transition
2. `Login` and `Hub` in both landscape directions, including one active 180-degree rotation
3. `Match` loading, table interaction, result, return-to-Hub state retention, and safe areas
4. `MatchPrototype` basic card touch/selection, landscape safe areas, and rotation while selected
5. `AnimationLab` basic controls in both landscape directions with active rotation

Landscape-direction rotation must recompose immediately without restarting, changing authoritative
match state, cancelling or duplicating an intent, moving the fixed gameplay camera, or placing
controls under unsafe screen regions. Portrait is unsupported under
[ADR 0003](../decisions/0003-landscape-only-mobile.md).

## Simulator support

Export a separate simulator project without changing the committed device-SDK setting:

```sh
scripts/validate-unity.sh smoke ios-simulator
```

Then build and launch it against an installed runtime:

```sh
xcrun simctl create \
  'The Fall iPhone 17 Pro' \
  'iPhone 17 Pro' \
  'iOS 26.5'

xcodebuild \
  -project Build/Smoke/iOSSimulator/Unity-iPhone.xcodeproj \
  -scheme Unity-iPhone \
  -configuration Debug \
  -sdk iphonesimulator \
  -destination 'platform=iOS Simulator,name=The Fall iPhone 17 Pro,OS=26.5' \
  -derivedDataPath Build/Smoke/iOSSimulator-DerivedData \
  build

xcrun simctl boot 'The Fall iPhone 17 Pro'
xcrun simctl bootstatus 'The Fall iPhone 17 Pro' -b
open -a Simulator
xcrun simctl install 'The Fall iPhone 17 Pro' \
  Build/Smoke/iOSSimulator-DerivedData/Build/Products/Debug-iphonesimulator/TheFall.app
xcrun simctl launch \
  'The Fall iPhone 17 Pro' \
  com.donkijote.thefall \
  --the-fall-scene \
  MatchPrototype
```

Create the named simulator only when it does not already exist. If it is already booted, `simctl boot` reports that state and the remaining commands can continue. Simulator results are recorded separately from physical-device evidence.

## Failure diagnosis

| Failure | Check |
| --- | --- |
| Unity reports iOS unsupported | verify the iOS module exists beside the exact editor from `ProjectVersion.txt`; add the module in Unity Hub |
| Xcode plug-in or first-launch failure | run `xcodebuild -runFirstLaunch`; install the matching runtime under Xcode Components |
| no signing identity or team | sign in under Xcode Apple Accounts; select the team only in the ignored generated project; let automatic signing create/update local development assets |
| device paired but unavailable | unlock it, reconnect USB, accept Trust, verify Developer Mode, and wait for Xcode device preparation |
| provisioning failure | confirm automatic signing, team access, bundle identifier, network access, and device availability; do not commit the resulting profile or team |
| install succeeds but launch fails | launch again from Xcode, inspect the device console, and record the scene plus exception; keep rule and presentation diagnosis separate |
| touch or rotation changes state | record scene, acting seat, selected card, viewport, safe rectangle, orientation, ordered intents/events, and final authoritative state |
| unsafe or clipped layout | record portrait/landscape screenshots, safe rectangle, scene, device model/OS, and the obscured control or card |
| Device Simulator and phone choose different layouts | verify responsive profile selection uses `UnityEngine.Device.Screen` and `UnityEngine.Device.Application`, and that the active screen has one `Bitbebop.SafeArea`; focus the Simulator view before entering Play Mode and compare the exact model, orientation, safe area, and generated-build timestamp |
| icons show overlapping labels on-device | icon-only controls must bind localization to `tooltip`, leave visible `text` empty, and retain the icon as the sole rendered button child |
| simulator destination missing | install the runtime matching Xcode, rerun `xcrun simctl list runtimes`, then rebuild the simulator export |

Generated logs and device evidence remain under ignored `Logs/`, `Build/`, `MemoryCaptures/`, or `Recordings/`. Do not paste device identifiers, signing details, or account data into diagnosis notes.

## Validation checkpoint

Validated on 2026-07-26 with the recorded toolchain and physical iPhone. Portrait results below are
historical evidence from before the landscape-only decision; they are not a current requirement:

- foundation validation passed
- Edit Mode tests: 100 passed
- Play Mode tests: 30 passed
- `scripts/validate-unity.sh smoke ios`: passed
- unsigned native iPhoneOS Xcode build: passed
- automatic local development signing, install, and launch: passed
- normal Bootstrap-to-Home launch: passed
- `Home`, `MatchPrototype`, and `AnimationLab` development launch paths: passed
- physical touch, safe-area layout, portrait, landscape, and active rotation: passed
- `scripts/validate-unity.sh smoke ios-simulator`: passed
- arm64 iOS 26.5 simulator native build, install, launch, and touch check: passed

The physical checks were completed with the project owner present. Signing assets, account details, device identifiers, generated Xcode projects, screenshots, logs, and Derived Data remained local and ignored.

## Issue #31 first-playable checkpoint

Validated on 2026-07-26 from candidate code commit. Portrait results below are retained as historical
evidence from before ADR 0003:
`aae863a74c7392af80b1a41207b809d611e6b791` on the recorded physical iPhone:

- the complete touch-only flow from Home through configuration, match completion, replay, and return
  to Home passed without editor intervention
- inspection, selection, confirmation, cancellation, rejection, and temporary input blocking passed
- portrait and landscape safe areas, card and character readability, hidden-hand privacy, and active
  rotation during card selection and animation passed
- normal, fast-forward, reduced-motion, skip, interruption, cancellation, and final-state
  synchronization passed
- three cold launches reached usable Home in `3.438 s`, `3.308 s`, and `3.352 s`
- three Home-to-match samples reached the first accepted interaction in `0.069 s`, `0.067 s`,
  and `0.069 s`
- a five-minute warm-up and 15-minute measured representative loop completed 48 matches with
  52,359 measured frames, zero frames over `100 ms`, zero authoritative/rendered mismatches,
  nominal thermal state, and peak app memory of `378,946,816` bytes (about `361.4 MiB`)
- wall-clock frame time measured `16.7 ms` median and `17.25 ms` p95; CPU frame time measured
  `16.7 ms` median and `17.3 ms` p95; GPU frame time measured `3.15 ms` median and `4.4 ms` p95
- both configurable rule values, both dealer seats, all required completion paths, 368 canto events,
  and 1,792 submitted human intents were exercised by the automated loop

This flagship-class 120 Hz phone is recorded as a **High** tier device. It provides useful physical
iOS evidence but does not substitute for the unresolved Reference or Constrained mobile rows. Its
memory, loading, thermal, state-agreement, and hitch checks passed. Its wall-clock and CPU p95 values
missed the stricter Reference-mobile `16.7 ms` frame-time gate by `0.55 ms` and `0.6 ms`
respectively; issue [#42](https://github.com/Donkijote/the-fall/issues/42) owns that focused
frame-pacing follow-up. No minimum iOS version, broad device support, distribution signing, or App
Store readiness is claimed.
