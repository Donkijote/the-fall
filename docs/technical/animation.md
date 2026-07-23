# Gameplay Animation Workbench

Status: Implemented workbench and first-playable runtime contract

## Purpose and authority boundary

`AnimationLab` is an Edit Mode-first library for wireframing, tuning, comparing, and diagnosing one reusable presentation beat at a time without entering Play Mode or running a complete match. It extends the issue #9 experiment while retaining its central contract: resolved domain events and the authoritative final `MatchState` exist before presentation begins.

`AnimationScenarioRecording` supplies the minimal deterministic state and resolved event needed for one animation. The workbench may change that beat's timing and visual treatment, pause, seek, skip, or replay it, but it never invokes the resolver during transport and never submits or changes an accepted intent.

The isolated recording library is:

- match start, dealer-card selection, and dealer choice
- deal one card to each 1v1 seat, opening rejection, and opening placement
- play one card, reflow the remaining hand, and confirm table placement
- normal capture and one cascade-card capture
- Fall, clean-table, canto, and general score cues
- deal completion, leftovers collection, round completion, dealer rotation, and tie extension
- active-turn change and match victory

Every selector entry isolates one presentation treatment plus mandatory final synchronization. Most contain one tunable beat. **Deal one card** contains two consecutive instances of the same Deal beat so one preview shows a complete current-player/opponent pass without mixing in another treatment. Each recording supports either 1v1 acting seat. Portrait, landscape, and wide desktop profiles rebuild only transient views from the same recording.

## Reusable beat vocabulary

`ResolvedAnimationSequence` maps the complete current domain-event vocabulary into reusable presentation categories:

- match start, dealer selection, and dealer choice
- deal, opening rejection, and opening placement
- play, remaining-hand reflow, table placement, normal capture, and one beat per cascade card
- Fall, clean table, canto, and other score changes
- deal completion, leftovers, round completion, dealer rotation, and tie extension
- turn change and match victory
- authoritative final-state synchronization

The ordered source event list is retained unchanged for diagnosis. The workbench selects only the beat owned by the chosen isolated recording. The integrated game still maps all facts from a resolution in authoritative event order. Final synchronization is mandatory and cannot be removed from a preset.

## Edit Mode authoring workflow

Open `The Fall > Animation Laboratory > Open Workbench`. The command opens the isolated `AnimationLab` scene and starts a transient preview while Unity remains in Edit Mode.

The Editor window can:

- select one isolated animation, either 1v1 seat, and portrait, landscape, or desktop presentation profile
- play an animation once through its final resting pose, pause it, reset it explicitly, and scrub it
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

- duration and pre-beat delay
- linear, ease-in-out, or anticipate easing
- trajectory offset
- emphasis

Each preset also owns playback speed, loop preference, fast-forward multiplier, reduced-motion duration scale, and reduced-motion trajectory scale. These assets live only under `Assets/TheFall/Content/Animation`; no field is translated into `RuleConfiguration` or domain state.

The Edit Mode workbench uses a transient copy of the selected asset. Changes affect the next preview immediately without scene regeneration or entering Play Mode. **Save Preset** writes the working copy back to the selected asset; **Save Preset As…** creates a new version-controlled asset. Runtime playback reads the committed preset without writing it, so gameplay cannot persist an implicit tuning change.

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

## First-playable runtime integration

`AnimationBeatEvaluator`, `AnimationSequenceTransport`, `ResolvedAnimationSequence`, and `AnimationPresentationState` are shared by Edit Mode preview and runtime playback. The Editor window is an authoring adapter around that code, not a separate approximation. Issue #26 binds the same saved beat definitions to equivalent resolved events in the integrated match.

`FirstPlayableAnimationPlayer` now performs that binding for the complete 1v1 match. It consumes the immutable startup events and accepted human/bot resolution records already retained by `MatchTrace`. Each record is presented in authoritative source-event order, then ends with the mandatory final-state synchronization beat. Card play and remaining-hand reflow are separate beats produced from the same accepted `CardPlayedEvent`, so their durations and easing can be tuned independently before the runtime chains them.

The integrated `Home` table renders an `AnimationPresentationState` prefix while a batch is active and swaps back to the exact accepted `MatchState` when it completes. Timing, delay, easing, trajectory, fast-forward multiplier, and reduced-motion scaling come from the versioned `Workbench Default` preset. Presentation never submits an intent, calculates a capture or score, or changes an accepted result.

The player-visible controls are:

- fast-forward, which changes presentation speed only
- reduced motion, which shortens movement and suppresses trajectory while retaining semantic cues
- skip, which immediately synchronizes to the accepted end state

Interruption, cancellation, leaving the match, component disable, and teardown use the same synchronization path. While a batch is active, the flow and card-interaction session are both presentation-blocked. A repeated click, confirm, or contextual action therefore cannot create a second accepted intent, and the Result panel is not promoted until the final victory sequence has synchronized.

## Runtime event treatment

Every first-playable event has either spatial motion or an explicit semantic treatment. Both treatments update the rendered prefix from the already-resolved event; neither owns rules.

| Resolved outcome | Runtime treatment |
| --- | --- |
| match start; dealer card selection, tie, dealer result, and shuffle | dealer-spread state change plus localized active-event cue |
| dealer choice and deal start | semantic cue and round/deal metadata update |
| card dealt | configured deck-to-hand motion, including face-down opponent cards |
| opening rejection | configured semantic rejection cue; rejected card remains in the deck prefix |
| opening placement | configured deck-to-table motion |
| card played | configured motion for the selected card from hand to table; remaining cards retain their slots |
| remaining-hand reflow | separately configured motion that closes the empty hand slot |
| non-capturing placement | configured placement cue after play and reflow |
| normal capture | configured table-to-capture motion for the played and matching card |
| cascade capture | one configured table-to-capture beat per additional card, preserving event/card order |
| Fall, clean table, canto, and other score changes | distinct configured semantic beats with ordered score/canto prefix updates |
| deal completion | semantic completion cue |
| leftovers | configured table-to-capture motion for each collected card |
| round completion, dealer rotation, and tie extension | ordered semantic cues with round/dealer/tie metadata updates |
| turn change | active-seat cue update |
| match victory | ordered victory cue; Result remains deferred until synchronization |
| final synchronization | instantaneous mandatory copy of the accepted `MatchState` |

Captured-card identity is retained only inside the transient presentation matcher so a public card can move into the correct pile. The public rendered-card API and snapshot continue to expose those piles face down, and opponent hand identities remain unavailable.

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

The isolated dealer-card selection begins with the complete forty-card spread face down. The selected position uses separate back and face surfaces, lifts from the table, rotates 180 degrees around its long edge, and remains face up among the other anonymous cards. Previously selected dealer cards remain revealed through tie rounds while the unselected spread stays opaque.
The revealed card rests above every face-down row so its complete face remains readable. The workbench Animation button always performs a one-shot preview and leaves this resolved pose in place; pressing Animation again restarts the preview, while Reset explicitly returns to the initial spread.

The isolated deal preview starts with a complete face-down deck and two cards already held at each seat. The top card follows the configured deck-to-hand path to the current player while rotating from its back to its authoritative face. The next top card follows the same reusable Deal beat to the opponent and remains face down. Existing hand cards retain fixed three-card slots throughout both motions, so only the incoming card moves and the opponent's identities remain opaque.

The isolated opening-placement preview starts with the already accepted table cards and the complete remaining deck face down. Its top card is reused as the moving card instead of creating a duplicate beside the stack: it leaves the deck back-up, follows the configured deck-to-table path, rotates to reveal its authoritative face, and settles in the next table slot. Existing table cards remain stationary.

The isolated opening-rejection preview reverses that treatment. It begins with the rejected card face up beside the accepted table card and the remaining deck face down. The rejected card leaves its table slot, rotates back-down on the way to its authoritative reinsertion index, and enters a temporary middle-deck gap. The upper half of the deck lifts and shifts aside during insertion, then closes over the card; accepted table cards remain stationary and the final rendered state contains the reinserted card in the deck.

The isolated matching-pair capture is a two-beat preview. Its shared `CardPlay` beat moves the played card out of the acting player's hand and directly onto the authoritative same-rank table card, keeping the played card visibly above its match. The `NormalCapture` beat then lifts that two-card stack, carries both cards together to the acting player's collected pile, rotates both faces down in flight, and leaves only opaque card backs in the resolved pile.

## Generation and validation

Use:

- `The Fall > Animation Laboratory > Generate`
- `The Fall > Animation Laboratory > Validate`
- `The Fall > Animation Laboratory > Capture Validation Set`

The generator creates missing preset assets, binds both presets to the scene, preserves the stationary camera, and validates preset versions and beat content. The Editor command opens the dedicated authoring window without entering Play Mode.

Focused Edit Mode coverage verifies that all 22 selector entries produce their expected matching tunable beats (one each except the two-card Deal pass and the `CardPlay` plus `NormalCapture` matching-pair pass), plus source-event mapping, preset serialization, the shared path evaluator, window availability, scene-backed preview while `Application.isPlaying` is false, per-beat seeking, editor-time transport, both seats, timing variants, and state convergence. Play Mode previews every isolated animation for both seats and verifies final agreement. Complete-match coverage continues to exercise the integrated Home table across normal, fast-forward, reduced-motion, skipped, interrupted, cancelled, and teardown paths; duplicate-input blocking; both acting seats; all four required desktop resolutions; and final authoritative agreement.

The representative seed-2400 profile completed 129 accepted intent records, 585 source events, and 732 visible beats without a pooling, tweening, Timeline, Animator, or third-party sequencing layer. The pure transport/prefix replay used 6,879 deterministic `20 ms` ticks and about `9.98 ms` aggregate presentation CPU (`0.209 ms` peak tick). The headless integrated Play Mode replay deliberately ran at the editor's uncapped batch update rate: 955,791 updates, about `2,229.42 ms` aggregate presentation CPU, and a `4.260 ms` maximum sampled update over `31.5 s` wall time. Those batch-mode values establish allocation/framework evidence, not desktop frame-pacing acceptance; issue #28 owns built-player median and p95 frame-time evidence. The implementation retains direct transient view rebuilding because this profile does not justify a framework or pool before representative production assets exist.

## Remaining boundaries

- Production VFX, audio, character acting, haptics, and final easing remain outside the workbench milestone.
- Animator, Timeline, pooling, and third-party tweening or sequencing frameworks remain unselected. Introducing one still requires measured need and an accepted architecture decision.
- Physical mobile performance, safe areas, thermal behavior, and device frame pacing remain separate validation work.

Related: [architecture](architecture.md), [deterministic domain foundation](domain-foundation.md), [testing](testing.md), [first-playable milestone](../planning/first-playable-milestone.md), and [fixed table composition](../design/table-composition-prototype.md).
