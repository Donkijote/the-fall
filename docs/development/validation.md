# Testing and Platform Validation Baseline

Status: Confirmed V0 baseline

## Purpose

This baseline makes deterministic rules, Unity integration, available player builds, and manual platform evidence repeatable. It is a V0 development gate, not production certification or an exhaustive device lab.

Unity `6000.5.4f1`, as recorded in `ProjectSettings/ProjectVersion.txt`, is the authoritative editor version. Generated logs, test results, captures, profiler recordings, and smoke builds remain outside version control under `Logs/`, `Build/`, `MemoryCaptures/`, or `Recordings/`.

## Validation responsibilities

| Layer | Owns | Does not prove |
| --- | --- | --- |
| Pure domain Edit Mode | deterministic state transitions, seeded replay, legal and invalid intents, ordered resolved events, immutable failure behavior | scenes, input bindings, rendering, or timing |
| Unity Edit Mode | assembly boundaries, authored configuration translation, asset metadata, adapters, generators, layout classification, and event-to-presentation mapping | live scene lifecycle or player-visible flow |
| Unity Play Mode | bootstrap and scene integration, touch/desktop intent equivalence, recomposition, visible feedback, animation interruption, and final-state synchronization | physical-device input, GPU cost, thermal behavior, or final readability |
| Build smoke | all enabled scenes and runtime assemblies compile into a development player for one target | signing, store compliance, device launch, sustained performance, or gameplay correctness |
| Manual exploratory | physical input, safe areas, readability, frame pacing, memory, loading, orientation, and thermal behavior | deterministic rule completeness or automated regression coverage |

Domain behavior belongs in Edit Mode. Play Mode is reserved for Unity integration and player-visible flow. A defect should be covered at the lowest layer that can reproduce it without hiding a cross-layer failure.

## Repeatable local commands

From the repository root, point `UNITY_THE_FALL` at the executable matching `ProjectVersion.txt` when it is not installed at the default macOS path:

```sh
export UNITY_THE_FALL="/Applications/Unity/Hub/Editor/6000.5.4f1/Unity.app/Contents/MacOS/Unity"

scripts/validate-unity.sh tests
scripts/validate-unity.sh smoke macos
scripts/validate-unity.sh all macos
```

`tests` runs foundation validation, the complete Edit Mode suite, and the complete Play Mode suite. `smoke` builds one platform. `all` runs the suites and then the selected smoke build. Results are written to:

- `Logs/FoundationValidation.log`
- `Logs/EditModeResults.xml` and `Logs/EditModeTests.log`
- `Logs/PlayModeResults.xml` and `Logs/PlayModeTests.log`
- `Logs/BuildSmoke-<platform>.log`
- `Build/Smoke/<platform>/`

Supported smoke arguments are `macos`, `windows`, `android`, and `ios`. Each requires the matching Unity Hub module and native toolchain. The editor alternatives are **Window > General > Test Runner** for test suites and **The Fall > Validation > Build Smoke** for development builds.

### Focused failure replay

Use a fully qualified fixture or test name with Unity's `-testFilter` option instead of repeatedly running unrelated suites. For example:

```sh
"$UNITY_THE_FALL" -batchmode -nographics \
  -projectPath "$(pwd)" \
  -runTests -testPlatform EditMode \
  -testFilter TheFall.Tests.EditMode.DomainFoundationEditModeTests \
  -testResults Logs/DomainReplayResults.xml \
  -logFile Logs/DomainReplay.log

"$UNITY_THE_FALL" -batchmode -nographics \
  -projectPath "$(pwd)" \
  -runTests -testPlatform PlayMode \
  -testFilter TheFall.Tests.PlayMode.AnimationLabPlayModeTests \
  -testResults Logs/AnimationReplayResults.xml \
  -logFile Logs/AnimationReplay.log
```

Run the same focused replay twice before classifying a seeded failure as nondeterministic.

## Build-smoke paths

`PlatformBuildSmoke` builds every enabled scene in documented order as an unsigned development player. The command runner selects these outputs:

| Argument | Unity target | Output | Current checkpoint |
| --- | --- | --- | --- |
| `macos` | `StandaloneOSX` | `Build/Smoke/macOS/TheFall.app` | Available and validated on the project-owned editor |
| `windows` | `StandaloneWindows64` | `Build/Smoke/Windows/TheFall.exe` | Path implemented; Windows module and Windows-host launch remain unavailable |
| `android` | `Android` | `Build/Smoke/Android/TheFall.apk` | Path implemented; Android module, SDK/NDK/JDK, and physical launch remain unavailable |
| `ios` | `iOS` | `Build/Smoke/iOS/` Xcode project | Path implemented; iOS module, Xcode build/signing, and physical launch remain unavailable |

A successful Android APK or iOS Xcode export is not a signed-device pass. A successful cross-built Windows player is not a launch pass. Record build and launch evidence separately.

## Initial platform matrix

The OS lanes below are deliberately relative. They define what to compare without silently selecting minimum supported OS versions, which remain an open product decision.

| Platform lane | OS coverage | Viewports or resolutions | Required evidence |
| --- | --- | --- | --- |
| Android constrained phone | oldest candidate Android version, once selected | `360 x 800` portrait and `800 x 360` landscape, including cutout insets | physical touch, safe area, rotate during selection and animation, constrained-tier profile |
| Android reference phone | current stable Android version | `390 x 844` portrait and `844 x 390` landscape, including cutout insets | physical touch, safe area, loading, 15-minute thermal/frame-pacing sample |
| iOS constrained phone | oldest candidate iOS version and device, once selected | compact notched portrait and landscape viewport near the `390 x 844` reference | physical touch, safe area, rotate during selection and animation, constrained-tier profile |
| iOS reference phone | current stable iOS version | `390 x 844` portrait and `844 x 390` landscape with safe areas | physical touch, safe area, loading, 15-minute thermal/frame-pacing sample |
| Windows desktop | current supported Windows 11 x64 servicing baseline | `1280 x 720`, `1920 x 1080`, `2560 x 1440`; resizable window | mouse and keyboard, resize, fullscreen decision evidence, build and launch |
| macOS desktop | current and previous major macOS release | `1280 x 720`, `1440 x 900`, `1920 x 1080`, `2560 x 1440`; resizable window | mouse and keyboard, resize, universal player build, Apple silicon launch |

Editor simulation continues to cover `390 x 844`, `844 x 390`, `1440 x 1080`, and `1920 x 1080` on every change to composition or interaction. Simulation is supporting evidence only and never replaces the physical mobile rows.

### Representative mobile tiers

| Tier | Working definition | Use |
| --- | --- | --- |
| Constrained | 64-bit phone, 4 GB system memory where platform inventory exposes it, 60 Hz display, lower GPU class, and the oldest candidate OS lane | establish the V0 floor and expose memory, fill-rate, thermal, and loading risk |
| Reference | current mainstream non-flagship phone, 6–8 GB system memory where exposed, 60 Hz or higher display, current stable OS | primary tuning and acceptance evidence |
| High | current flagship-class phone or tablet-class GPU | detect scaling defects and high-refresh pacing; never substitute for the other two tiers |

Exact device models and minimum OS commitments require an owned platform-support decision. Until that decision exists, every manual report records model, chipset, system memory when available, OS/build, display resolution and refresh rate, orientation, quality settings, build commit, and test duration.

## Initial V0 measurement gates

These are owner-approved development gates for the representative V0 scenes, not launch promises.

| Measure | Constrained mobile | Reference mobile | Desktop reference |
| --- | ---: | ---: | ---: |
| frame pacing during a 15-minute representative loop | at least 30 fps; p95 frame time at most `33.3 ms` | 60 fps target; p95 frame time at most `16.7 ms` | 60 fps target at `1920 x 1080`; p95 frame time at most `16.7 ms` |
| peak app memory | at most `2.0 GiB` | at most `2.0 GiB` | at most `2.0 GiB` |
| cold launch to usable Home | record three cold runs; no pass/fail budget yet | record three cold runs; no pass/fail budget yet | first playable: every one of three runs at most `10 s` |
| Home to usable match scene | record three runs; no pass/fail budget yet | record three runs; no pass/fail budget yet | first playable: every one of three runs at most `5 s` |
| orientation recomposition | at most `250 ms`, with no changed or duplicated intent | at most `250 ms`, with no changed or duplicated intent | not applicable; resize uses the same state-preservation contract |

All tiers must preserve the existing readability comparisons: card identity at 48-pixel card width, character expression at 64-pixel head height, names and scores without clipping, hover-independent interaction symbols, and distinguishable state cues in grayscale. A five-minute warm-up precedes the 15-minute sample. Record median and p95 CPU/GPU frame times, peak app memory, every loading sample, thermal state when available, and any frame over `100 ms`.

Loading is measured from launching a fully closed app until Home accepts input, and from requesting the match until its first intended interaction is accepted. The current prototype scenes remain measurement evidence. The pass/fail budgets apply to the integrated macOS candidate defined by the [first playable milestone](../planning/first-playable-milestone.md), not to the isolated V0 scenes.

## Manual exploratory procedure

For each required matrix row:

1. Install or launch a development build made from a recorded commit.
2. Record hardware, OS, display, quality, build, and profiler attachment state.
3. Cold-launch three times and record Home and match readiness.
4. Exercise touch or mouse/keyboard inspection, selection, confirmation, cancellation, rejection, and temporary blocking.
5. Rotate mobile during a selected card and during animation; on desktop resize across every required resolution.
6. Exercise both 1v1 acting seats, portrait and landscape, normal completion, skip, interruption, cancellation, and fast-forward.
7. Confirm cards, names, scores, hands, capture piles, and state symbols remain readable and unobscured.
8. Run the representative loop for 20 minutes total: five-minute warm-up plus a 15-minute measured sample.
9. Save profiler evidence and note hitches, memory trend, thermal state, clipping, unsafe areas, input duplication, or state mismatch.

The test passes only when both the authoritative final `MatchState` and player-visible result agree. Visual quality without state agreement is a failure; state agreement with unreadable or inconsistent presentation is also a failure.

## Failure diagnosis contract

### Seeded rule failures

The report must include editor version, commit, test name, explicit seed, initial state or builder inputs, ordered recorded intents, returned `RuleError`, ordered resolved events, and final-state snapshot. Replay the focused test twice with the same seed.

- Different state or events from identical input indicates a domain determinism or hidden-state defect.
- An explicit rejected result with the unchanged input state is a correctly contained rule failure; verify the expected `RuleError`.
- Identical domain output with a different rendered result is not a rule failure. Hand the same state and events to presentation diagnosis.

Do not replace a failing seed with a passing seed. Keep the smallest failing replay as regression coverage.

### Presentation failures

The report must include scene, acting seat, input path, viewport, safe rectangle, orientation/profile, ordered domain events, authoritative final state, completion reason, and screenshot or capture when useful. First compare the final domain state and event list with the rendered snapshot.

- Mismatched authoritative state or events: application/domain boundary.
- Matching state and events but wrong visuals: presentation mapping or synchronization.
- Correct final snapshot after skip but not after normal playback: sequencing or interruption lifecycle.
- State or intent history changed by rotation: interaction/recomposition boundary.
- Correct editor result but incorrect physical device result: platform input, safe-area, timing, shader, or resource issue.

Existing table-composition and animation capture menus write comparison images to `Logs/`. Captures support diagnosis but are not automated visual-regression approval.

## GitHub Actions decision

**Confirmed for V0:** defer GitHub Actions and Unity CI. The project owner does not want CI machinery until the game becomes serious enough to justify it.

No workflow, Unity activation secret, third-party CI action, or self-hosted runner is introduced. Pull requests instead record the local editor version, suite counts, exercised smoke target, and skipped platform rows. Revisit automation only when repeated manual validation cost or project risk supports it; at that point decide licensing, runner ownership, LFS/cache storage, platform modules, secret handling, and artifact retention together.

## Coverage gaps and ownership

| Gap | Current disposition | Owner or recommended follow-up |
| --- | --- | --- |
| Unity suites and player builds in CI | deliberately deferred for V0 | project owner decides when project seriousness justifies a CI issue |
| physical Android and iOS input, safe area, thermal, memory, and frame pacing | unvalidated | create a mobile device-validation issue after exact devices and minimum OS candidates are selected |
| Windows build and launch | unvalidated | create a Windows smoke issue when a Windows host or runner is available |
| signed iOS launch and store toolchain | out of V0 build smoke | future distribution/signing issue after store direction is selected |
| minimum OS versions, desktop fullscreen, and minimum window size | first playable accepts the current project-owned macOS environment and four resizable layouts without setting production minimums or fullscreen behavior | revisit before a release or wider desktop-support milestone |
| first-playable loading budgets | `10 s` cold launch and `5 s` Home-to-match for every one of three macOS samples | verify and record in acceptance issue #28 |
| full-match allocation, GPU, VFX, audio, character, and repeated-round cost | prototype scenes are not representative | profile the first playable slice before production asset approval |
| screenshot or visual-regression automation | deferred | presentation QA issue if manual capture comparison becomes unreliable or expensive |
| accessibility and localized readability thresholds | prototype comparisons only | dedicated accessibility/localization validation before a release milestone |

## Issue #10 validation checkpoint

Validated locally on 2026-07-20 with Unity `6000.5.4f1`:

- foundation structural validation: passed
- complete Edit Mode suite: 40/40 passed
- complete Play Mode suite: 11/11 passed
- macOS development player build smoke: succeeded
- Android, iOS, and Windows build modules/toolchains: unavailable and not claimed
- physical mobile and Windows exploratory rows: not run

Related: [testing strategy](../technical/testing.md), [platform requirements](../technical/platforms.md), [bootstrap validation](bootstrap-validation.md), [table composition](../design/table-composition-prototype.md), [card interaction](../design/card-interaction-prototype.md), and [animation laboratory](../technical/animation.md).
