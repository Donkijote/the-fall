# Functional Prototype Audio Sources

Status: Implemented first-playable asset record

## Retained source

The first playable retains no imported recording, sample library, music file, ambience, voice,
or other third-party audio. Its functional cues are deterministic waveforms generated in memory by
`PrototypeAudioCueLibrary` when the Match scene starts.

| Record field | Value |
| --- | --- |
| asset identifier | `first-playable-procedural-cues-v1` |
| owner | Donkijote / The Fall |
| source and creation date | project-authored C# generator, 2026-07-24 |
| source location | `Assets/TheFall/Presentation/Audio/PrototypeAudioCueLibrary.cs` |
| inputs | numeric oscillator, pulse-envelope, deterministic-noise, gain, and duration parameters only |
| provenance | authored for issue #27; no recording, generated media input, model output, or external sample was used |
| license | project-owned proprietary source; no third-party audio license or attribution applies |
| intended use | functional first-playable feedback for already-resolved presentation beats |
| processed file location | none; clips are generated in memory and are never written to the repository |
| Unity import settings | not applicable; mono `44.1 kHz` runtime clips use one non-spatial, non-looping `AudioSource` |
| optimization | ten short mono definitions, each at most `0.38 s`; only the current effects cue may play |
| replacement status | prototype; replace before production mastering with separately reviewed and recorded sources |

## Semantic cue inventory

| Cue | Primary / secondary frequency | Duration | Pulses | Intended use |
| --- | ---: | ---: | ---: | --- |
| Deal | `620 / 930 Hz` | `0.055 s` | 1 | cards entering a hand |
| Play | `260 / 170 Hz` | `0.080 s` | 1 | an accepted card play |
| Capture | `440 / 660 Hz` | `0.120 s` | 2 | same-rank capture |
| Cascade | `540 / 810 Hz` | `0.090 s` | 2 | each visible cascade step |
| Fall | `220 / 880 Hz` | `0.220 s` | 3 | resolved Fall scoring |
| Clean table | `740 / 1110 Hz` | `0.240 s` | 3 | resolved clean-table scoring |
| Canto | `392 / 784 Hz` | `0.260 s` | 2 | canto announcement or resolution |
| Score | `660 / 990 Hz` | `0.140 s` | 2 | other resolved score changes |
| Transition | `330 / 495 Hz` | `0.100 s` | 1 | match, turn, deal, or round transition |
| Victory | `523 / 1046 Hz` | `0.380 s` | 4 | authoritative match completion |

The frequencies are functional identifiers, not musical or mastering approval. Gains remain between
`0.18` and `0.30`, and a new cue replaces the previous effects cue instead of layering over it. This keeps
prototype audio subordinate to card readability and accepted interaction timing.

## Exclusions

The first playable retains no music. The music control is present and stateful so the channel boundary is
explicit, but it intentionally has no source or playback path. Soundtrack, ambience, voice, character
vocalization, spatial mix, haptics, production sound design, and mastering remain deferred.

Related: [asset strategy](strategy.md), [technical audio](../technical/audio.md), and
[first playable milestone](../planning/first-playable-milestone.md).
