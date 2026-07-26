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

Supported smoke arguments are `macos`, `windows`, `android`, `ios`, and the supplemental `ios-simulator`. Each requires the matching Unity Hub module and native toolchain. The editor alternatives are **Window > General > Test Runner** for test suites and **The Fall > Validation > Build Smoke** for development builds.

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
| `ios` | `iOS` device SDK | `Build/Smoke/iOS/` Xcode project | Export and unsigned native Xcode build validated; local signed physical-device evidence belongs to issue #30 |
| `ios-simulator` | `iOS` simulator SDK | `Build/Smoke/iOSSimulator/` Xcode project | Supplemental compile/launch path; never substitutes for physical-device evidence |

A successful Android APK or iOS Xcode export is not a signed-device pass. A successful cross-built Windows player is not a launch pass. Record build and launch evidence separately.

The repeatable local signing, deployment, retained-scene launch, simulator, and iPhone diagnosis procedure is documented in [iOS development builds](ios-development-builds.md).

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

### First-playable development-player probe

Issue #28 adds an opt-in probe to the macOS development player so the acceptance measurements are
repeatable without adding a production analytics or telemetry system. The probe is inert during normal
launches and is attached only when both conditions are true:

- the player is a development build
- the command line includes `--first-playable-acceptance`

The readiness mode records process-start-to-usable-Home and Home-to-first-accepted-interaction timings,
then exits:

```sh
"Build/Smoke/macOS/TheFall.app/Contents/MacOS/The Fall" \
  --first-playable-acceptance \
  --acceptance-readiness-only \
  --acceptance-commit "$(git rev-parse HEAD)" \
  --acceptance-output "$(pwd)/Logs/Acceptance-readiness-1.json"
```

Run readiness mode three times from a fully closed player and use a different ignored output path for
each run. The endurance mode drives the same integrated Home flow, application orchestrator, table,
animation, and audio presentation for a five-minute warm-up plus a 15-minute measured sample at
`1920 x 1080`:

```sh
"Build/Smoke/macOS/TheFall.app/Contents/MacOS/The Fall" \
  --first-playable-acceptance \
  --acceptance-warmup-seconds 300 \
  --acceptance-measure-seconds 900 \
  --acceptance-commit "$(git rev-parse HEAD)" \
  --acceptance-output "$(pwd)/Logs/Acceptance-endurance.json"
```

The fixed-memory histogram reports wall-clock, CPU, and GPU frame-time median/p95 values, frames over
`100 ms`, peak process working set, peak Unity allocation, completed-match/event coverage, and any
rendered-versus-authoritative mismatch. Reports remain outside version control under `Logs/`. The probe
is supporting repeatable evidence; it does not replace mouse/keyboard exploratory play, resolution
readability review, or the automated rule and synchronization suites.

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
| physical mobile input, safe area, thermal, memory, and frame pacing | one High-tier physical iPhone row recorded by #31; Android and the Reference/Constrained iOS rows remain unvalidated | #42 owns the recorded iPhone frame-pacing miss; create bounded device issues when representative Android and iOS hardware/support floors are selected |
| Windows build and launch | unvalidated | create a Windows smoke issue when a Windows host or runner is available |
| signed iOS launch and store toolchain | local development signing belongs to #30; distribution remains out of scope | future App Store Connect, TestFlight, and release-signing issue after store direction is selected |
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

## Issue #22 validation checkpoint

Validated locally on 2026-07-20 with Unity `6000.5.4f1`:

- foundation structural validation: passed
- complete Edit Mode suite: 52/52 passed, including 12 complete-1v1 rule tests
- complete Play Mode suite: 11/11 passed
- macOS universal development-player smoke build: succeeded
- deterministic complete-match replay: reached one winner from dealer selection with identical final state and ordered events across identical seeded runs
- Android, iOS, Windows, physical-device, and performance rows: not run and not claimed by this pure-domain issue

## Issue #23 validation checkpoint

Validated locally on 2026-07-20 with Unity `6000.5.4f1`:

- foundation structural validation: passed
- complete Edit Mode suite: 58/58 passed, including six application-orchestration tests and 24 seeded complete-match simulations
- complete Play Mode suite: 11/11 passed
- macOS universal development-player smoke build: succeeded
- deterministic orchestration replay: identical bot intents, ordered events, and final state for the same seed and human intent sequence
- bot information boundary: the public bot view exposes public match context plus its own hand, without opponent hands or hidden deck state
- Android, iOS, Windows, physical-device, and performance rows: not run and not claimed by this application-layer issue

## Issue #24 validation checkpoint

Validated locally on 2026-07-20 with Unity `6000.5.4f1`:

- first-playable flow generation and structural validation: passed
- complete Edit Mode suite: 61/61 passed, including guarded navigation, documented defaults, fresh replay sessions, and stale-state cleanup
- complete Play Mode suite: 13/13 passed, including full UI-adapter completion/replay/return and pseudo-localized keyboard-focusable layout
- macOS universal development-player smoke build: succeeded
- complete table presentation, animation, audio, manual resolution review, and acceptance performance measurements remain owned by issues #25–#28
- Android, iOS, Windows, physical-device, and production-localization rows were not run and are not claimed by this UI-flow issue

## Issue #25 validation checkpoint

Validated locally on 2026-07-20 with Unity `6000.5.4f1`:

- first-playable table generation and structural validation: passed
- complete Edit Mode suite: 63/63 passed, including privacy-safe projection and complete-match snapshot agreement
- complete Play Mode suite: 15/15 passed, including full-match rendered-state agreement, private opponent hands, exact public card collections, all five semantic interaction states, and exactly-once confirmed play
- `1280 x 720`, `1440 x 900`, `1920 x 1080`, and `2560 x 1440` recomposition preserved the selected card, interaction revision/history, authoritative state, match trace, and fixed camera
- macOS universal development-player smoke build: succeeded
- built-player manual visual inspection was skipped because the desktop session was locked; the complete manual resolution, loading, performance, and endurance matrix remains owned by issue #28
- Android, iOS, Windows, physical-device, production animation, and audio rows were not run or claimed by this presentation-integration issue

## Issue #29 validation checkpoint

Validated locally on 2026-07-21 with Unity `6000.5.4f1`:

- animation-workbench generation and structural validation: passed
- complete Edit Mode suite: 70/70 passed, including named/versioned preset loading, reusable beat composition, shared wireframe/runtime path evaluation, scene-backed preview without Play Mode, deterministic transport, reset, and state convergence
- complete Play Mode suite: 18/18 passed, including pause/resume, step, seek, reset, live preset/scenario changes, both 1v1 seats, portrait/landscape/desktop comparison, skip/interruption/cancellation, and authoritative convergence
- macOS universal development-player smoke build: succeeded
- source recordings and accepted final states remained unchanged by Edit Mode authoring, Scene-view trajectory changes, composition, timing, speed, loop, reduced-motion, and transport controls
- Android, iOS, Windows, physical-device, production VFX/audio/character acting, and first-playable promotion were not run or claimed by this tooling issue

## Issue #26 validation checkpoint

Validated locally on 2026-07-22 with Unity `6000.5.4f1`:

- Home generation and structural foundation validation: passed with the versioned Workbench Default animation preset bound to the integrated table
- complete Edit Mode suite: 77/77 passed, including the one-recording/one-beat contract for all 22 isolated workbench animations, complete event-vocabulary mapping, batch-by-batch complete-match convergence, early-exit convergence, and timing-variant rule-state isolation
- complete Play Mode suite: 21/21 passed, including every isolated animation for both seats, normal, fast-forward, reduced-motion, skip, interruption, cancellation, teardown, duplicate-input blocking, four required desktop resolutions, and full-match authoritative agreement
- macOS universal development-player smoke build: succeeded
- seed-2400 pure replay: 129 accepted intent records, 585 source events, 732 beats, 6,879 deterministic transport ticks, `9.98 ms` aggregate presentation CPU, and `0.209 ms` peak tick
- seed-2400 integrated headless replay: 955,791 uncapped editor updates, `2,229.42 ms` aggregate presentation CPU, `4.260 ms` peak sampled update, and about `31.5 s` wall time; these are framework/pooling evidence rather than built-player frame-pacing acceptance
- no pooling, Animator, Timeline, tweening framework, or third-party sequencer was promoted; built-player median/p95 frame pacing, physical devices, production VFX/audio/character acting, and endurance remain outside this issue

## Issue #27 validation checkpoint

Validated locally on 2026-07-24 with Unity `6000.5.4f1`:

- first-playable table/audio generation and structural foundation validation: passed with one non-looping, non-spatial effects source and localized master/effects/music controls
- complete Edit Mode suite: 81/81 passed, including the required semantic cue vocabulary, distinct procedural waveform fingerprints, and scene-source safety
- complete Play Mode suite: 28/28 passed, including one-for-one complete-match cue/beat agreement, master and effects muting, the source-free music control, fast-forward, skip, interruption, cancellation, replay, return-to-Home, and teardown cleanup
- macOS universal development-player smoke build: succeeded
- all retained cue sources are project-owned runtime-generated waveforms with provenance, ownership/license status, intended use, parameters, and prototype replacement status recorded in `docs/assets/prototype-audio.md`
- no recording, external sample, music, ambience, voice, spatial mix, haptics, production sound design, or mastering was retained or claimed

## Issue #28 validation checkpoint

Validated locally on 2026-07-24 from candidate commit
`c2bd7e0d076f687dade824684574b3c22090bd34` with Unity `6000.5.4f1`:

- foundation validation, 94/94 Edit Mode tests, 30/30 Play Mode tests, and the macOS universal
  development-player smoke build passed
- the universal player launched and the complete mouse/keyboard flow passed at `1280 x 720`,
  `1440 x 900`, `1920 x 1080`, and `2560 x 1440`
- three cold launches reached usable Home in `8.042 s`, `3.605 s`, and `3.613 s`; three
  Home-to-match samples reached the first accepted interaction in `0.072 s`, `0.062 s`, and
  `0.056 s`
- the five-minute warm-up and 15-minute measured loop recorded 2,188,635 frames with
  wall-clock/CPU/GPU p95 values of `0.60 ms`, `0.60 ms`, and `0.65 ms`, peak app memory of about
  `420.7 MiB`, 52 completed matches, and zero authoritative/rendered mismatches
- all first-playable acceptance gates passed; skipped platforms and production-fidelity gaps remained
  outside the bounded macOS milestone

## Issue #31 validation checkpoint

Validated on 2026-07-26 from candidate code commit
`aae863a74c7392af80b1a41207b809d611e6b791` with Unity `6000.5.4f1` and the physical iPhone
recorded in [iOS development builds](ios-development-builds.md):

- foundation validation, 106/106 Edit Mode tests, 30/30 Play Mode tests, the Unity iOS export, signed
  native Xcode build, install, and launch passed
- the project owner passed the complete touch-only match, replay, return, interaction-state,
  safe-area, readability, privacy, portrait/landscape, active-rotation, animation-control, and
  authoritative-state checklist
- cold-launch samples were `3.438 s`, `3.308 s`, and `3.352 s`; Home-to-match samples were
  `0.069 s`, `0.067 s`, and `0.069 s`
- after a five-minute warm-up, the 15-minute sample completed 48 matches and measured 52,359 frames,
  zero frames over `100 ms`, zero authoritative/rendered mismatches, nominal thermal state, and
  `378,946,816` bytes (about `361.4 MiB`) peak app memory
- wall-clock/CPU/GPU median values were `16.7 ms`, `16.7 ms`, and `3.15 ms`; p95 values were
  `17.25 ms`, `17.3 ms`, and `4.4 ms`
- the recorded device is High tier, so it does not close the Reference or Constrained iOS rows; its
  wall-clock and CPU p95 values also miss the stricter Reference-mobile `16.7 ms` gate
- focused issue [#42](https://github.com/Donkijote/the-fall/issues/42) owns the frame-pacing follow-up;
  Android, representative iOS tiers, production support floors, and distribution remain unclaimed

Related: [testing strategy](../technical/testing.md), [platform requirements](../technical/platforms.md), [bootstrap validation](bootstrap-validation.md), [table composition](../design/table-composition-prototype.md), [card interaction](../design/card-interaction-prototype.md), and [animation laboratory](../technical/animation.md).
