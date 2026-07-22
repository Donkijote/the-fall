# Gameplay Animation Workbench

Status: Implemented first-playable presentation tooling

## Purpose and authority boundary

`AnimationLab` is an Edit Mode-first sequence workbench for wireframing, composing, tuning, comparing, and diagnosing reusable presentation beats without entering Play Mode or running a complete match. It extends the issue #9 experiment while retaining its central contract: resolved domain events and the authoritative final `MatchState` exist before presentation begins.

`AnimationScenarioRecording` resolves a selected deterministic scenario through `MatchSession` exactly once. The workbench may reorder presentation beats, change timing and visual treatment, pause, seek, skip, or replay that recording, but it never invokes the resolver during transport and never submits or changes an accepted intent.

The included recordings are:

- Fall, cascade, and clean table
- non-capturing play and table placement

Each recording supports either 1v1 acting seat. Portrait, landscape, and wide desktop profiles rebuild only transient views from the same recording.

## Reusable beat vocabulary

`ResolvedAnimationSequence` maps the complete current domain-event vocabulary into reusable presentation categories:

- match start, dealer selection, and dealer choice
- deal, opening rejection, and opening placement
- play, table placement, normal capture, and one beat per cascade card
- Fall, clean table, canto, and other score changes
- deal completion, leftovers, round completion, dealer rotation, and tie extension
- turn change and match victory
- authoritative final-state synchronization

The ordered source event list is retained unchanged for diagnosis. A preset selects and orders beat categories; repeated source events of one category retain their authoritative order. Final synchronization is mandatory and cannot be removed from a preset.

## Edit Mode authoring workflow

Open `The Fall > Animation Laboratory > Open Workbench`. The command opens the isolated `AnimationLab` scene and starts a transient preview while Unity remains in Edit Mode.

The Editor window can:

- select a deterministic recording, either 1v1 seat, and portrait, landscape, or desktop presentation profile
- select and preview one resolved beat at a time or play the complete composition
- play, pause, step, reset, loop one selected beat, and scrub the selected beat or complete sequence
- reorder or disable reusable beat categories
- edit duration, delay, easing, trajectory offset, and emphasis with Undo/Redo support
- save the working preset or create a new named project-owned preset
- display the active beat, source event, elapsed time, and state-agreement diagnosis

For transform beats, the Scene view draws a wireframe path using the same evaluator as runtime:

- green point: authoritative start supplied by the recorded initial/prefix state
- blue point: authoritative target supplied by the resolved next state
- yellow handle: editable presentation trajectory offset

The endpoints are intentionally not authorable because moving them would allow presentation to contradict authoritative state. The path between them is presentation-owned and may be adjusted freely.

Generated tables, cards, labels, materials, and preview roots use `DontSave` and are destroyed when the window closes, the scene changes, or Play Mode begins. Authoring never dirties the scene; only an explicit preset save changes a project asset.

## Versioned presentation presets

`AnimationSequenceConfiguration` is a project-owned, versioned `ScriptableObject` presentation preset. The committed presets are:

- `AnimationSequenceConfiguration.asset` — **Workbench Default**
- `AnimationFastIterationPreset.asset` — **Fast Iteration**

Each reusable beat exposes:

- enabled state and composition order
- duration and pre-beat delay
- linear, ease-in-out, or anticipate easing
- trajectory offset
- emphasis

Each preset also owns playback speed, loop preference, fast-forward multiplier, reduced-motion duration scale, and reduced-motion trajectory scale. These assets live only under `Assets/TheFall/Content/Animation`; no field is translated into `RuleConfiguration` or domain state.

The Edit Mode workbench uses a transient copy of the selected asset. Changes affect the next preview immediately without scene regeneration or entering Play Mode. **Save Preset** writes the working copy back to the selected asset; **Save Preset As…** creates a new version-controlled asset. Runtime playback also uses a transient copy, so neither authoring nor gameplay mutates a committed preset implicitly.

## Transport and deterministic replay

`AnimationSequenceTransport` owns presentation time independently of Unity frame time and domain state. In Edit Mode it advances through `EditorApplication.update`; in game it advances through the runtime frame source. Both surfaces provide:

- play, pause, and resume
- restart from the deterministic initial snapshot
- one-beat step
- normalized scrub/seek through deterministic replay-to-position
- loop and adjustable playback speed
- fast-forward and reduced-motion variants
- skip to the authoritative end state
- reset to the authoritative initial snapshot

Seeking and stepping reconstruct rendered state from the initial snapshot by applying the same composed beat prefix. Normal completion, skip, interruption, cancellation, disable, and teardown converge on the accepted final `MatchState`. Reset reconstructs the accepted initial snapshot. Transport controls never mutate the recording, its source events, or its final state.

## Runtime reuse

`AnimationBeatEvaluator`, `AnimationSequenceTransport`, `ResolvedAnimationSequence`, and `AnimationPresentationState` are shared by Edit Mode preview and runtime playback. The Editor window is an authoring adapter around that code, not a separate approximation. Issue #26 can bind the same saved beat definitions to equivalent resolved events in the integrated match.

The in-game AnimationLab overlay remains available for final runtime integration comparison, but it is no longer required to create, tune, or test an individual beat.

## Workbench diagnosis

Diagnosis displays:

- ordered authoritative source events
- the source event associated with the active beat
- active beat index and elapsed/total presentation time
- rendered-versus-authoritative agreement

Agreement is expected at the end state. During an in-flight preview the rendered snapshot intentionally represents only the applied beat prefix.

## Presentation rendering

`AnimationLab.unity` retains the stationary gameplay camera and uses the approved V0 `RoundCardTable`, generated forty-card catalog/shared material, inexpensive upper-body placeholders, and existing table-composition profiles. Camera movement remains prohibited.

Trajectory and easing affect transient card movement only. Emphasis affects the diagnostic event cue. Reduced motion shortens duration and suppresses trajectory while keeping semantic cues and final-state synchronization.

## Generation and validation

Use:

- `The Fall > Animation Laboratory > Generate`
- `The Fall > Animation Laboratory > Validate`
- `The Fall > Animation Laboratory > Capture Validation Set`

The generator creates missing preset assets, binds both presets to the scene, preserves the stationary camera, and validates preset versions and beat content. The Editor command opens the dedicated authoring window without entering Play Mode.

Focused Edit Mode coverage verifies source-event mapping, preset serialization, composition order, the shared path evaluator, window availability, scene-backed preview while `Application.isPlaying` is false, per-beat seeking, editor-time transport, both seats, timing variants, and state convergence. Focused Play Mode coverage verifies that the same assets and evaluator retain normal completion, pause/resume, step, seek, reset, skip, interruption, cancellation, fast-forward, live preset changes, scenario selection, both seats, profile comparison, stationary camera, and authoritative convergence.

## Remaining boundaries

- Issue #26 promotes these lab-tested categories and presets into full-match presentation.
- Production VFX, audio, character acting, haptics, and final easing remain outside the workbench milestone.
- Animator, Timeline, pooling, and third-party tweening or sequencing frameworks remain unselected. Introducing one still requires measured need and an accepted architecture decision.
- Physical mobile performance, safe areas, thermal behavior, and device frame pacing remain separate validation work.

Related: [architecture](architecture.md), [deterministic domain foundation](domain-foundation.md), [testing](testing.md), [first-playable milestone](../planning/first-playable-milestone.md), and [fixed table composition](../design/table-composition-prototype.md).
