# First-Playable Functional Audio

Status: Implemented first-playable presentation contract

## Authority boundary

`FirstPlayableAudioPresenter` consumes the same `ResolvedAnimationStep` objects already composed from
accepted domain events for the table animation. It does not inspect legal intents, calculate captures,
classify cantos, award points, advance presentation time, or mutate `MatchState`.

`PrototypeAudioCueLibrary.TryResolve` maps resolved beat kinds into this semantic vocabulary:

| Resolved beat | Cue |
| --- | --- |
| `Deal` | Deal |
| `CardPlay` | Play |
| `NormalCapture` | Capture |
| `CascadeCapture` | Cascade |
| `CaptureCollection` | Capture |
| `FallScore` | Fall |
| `CleanTableScore` | Clean table |
| `Canto` | Canto |
| other `Score` | Score |
| match start, deal completion, turn, round, dealer rotation, or tie extension | Transition |
| `MatchCompleted` | Victory |

Unmapped visual beats remain silent. Audio therefore describes facts that presentation has already
received and never becomes an outcome or accepted-intent timing dependency.

## Playback and lifecycle

The Home composition owns one two-dimensional `AudioSource`. It is non-looping, does not play on awake,
and plays only one short effects clip at a time. Every cue starts immediately when its mapped beat becomes
active; there is no coroutine, delayed callback, scheduled DSP playback, queue, or audio-owned duration.
A new cue replaces an unfinished cue rather than stacking it.

`FirstPlayableTablePresentation` begins a fresh audio session with each match session and emits one cue for
each mapped active beat reference. Its existing presentation lifecycle also controls audio:

- changing fast-forward stops the active cue; later cues use a small pitch lift without affecting transport
- skip, interruption, and cancellation stop active playback before authoritative synchronization
- replay and return to Home stop playback and clear the previous session boundary
- component disable and teardown stop playback and release every generated in-memory clip

The cue history is diagnostic only. Complete-match Play Mode coverage compares it one-for-one with mapped
animation steps to catch missing or duplicated emission. It is not persisted or read by game logic.

## Controls

Home Settings exposes independent Master audio, Effects, and Music toggles. Those preferences remain
active at the table without duplicating configuration controls in the match HUD.

- Master off prevents and stops gameplay effects.
- Effects off prevents and stops gameplay effects while preserving the master preference.
- Music retains an independent preference but controls no playback because milestone music is omitted.

The preferences survive replay and Match-to-Hub scene replacement because Bootstrap owns their
presentation-session state. They are not translated into rule configuration.

## Source and fidelity

The prototype uses short project-authored procedural waveforms, not imported recordings. Complete
provenance, ownership/license, parameters, intended use, and replacement status are recorded in
[functional prototype audio sources](../assets/prototype-audio.md).

Production music, ambience, voice, spatial mixing, haptics, sound libraries, and mastering remain outside
this contract.

Related: [architecture](architecture.md), [animation](animation.md),
[first-playable table](first-playable-table.md), and
[first playable milestone](../planning/first-playable-milestone.md).
