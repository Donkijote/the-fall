# First Playable Milestone

Status: Completed with tracked device follow-up

## Player-visible outcome

From the Home screen, a player can configure and complete one offline 1v1 match against a single bot in a macOS development build, see a clear winner, then replay or return Home without editor intervention.

This is the first integrated playable proof of The Fall. It is not Story Mode, a content-complete demo, a production-art milestone, or a public release.

## Why this slice

The V0 evidence supports integration rather than another isolated prototype:

| Foundation evidence | What it supports | Boundary retained for the milestone |
| --- | --- | --- |
| Pure C# 1v1 resolver and seeded replay | complete deterministic 1v1 rules can grow without scene ownership | three-player, 2v2, save serialization, and networking remain deferred |
| Fixed table composition | local-bottom 1v1 seating, private hands, captured piles, stationary camera, and desktop resizing are readable | prototype numeric layout values may still change during integration |
| Shared card interaction | mouse, keyboard, and touch can express the same application intents | macOS mouse and keyboard are the acceptance input; touch remains regression evidence only |
| Generated asset intake and modular card pipeline | the prototype table and complete card atlas can support a readable integrated match | the unrigged 366,508-triangle `WarmChallenger` remains review-only; gameplay uses inexpensive upper-body placeholders |
| Animation laboratory | resolved events can drive ordered, skippable presentation and converge on authoritative state | full-match beats, audio, VFX, and repeated-round cost still require implementation and measurement |
| Validation baseline | suites, macOS smoke builds, failure diagnosis, resolutions, and measurable budgets are repeatable | Android, iOS, Windows, physical mobile, signing, and store evidence are not first-playable gates |

A representative turn would be too small to prove the game loop. Story Mode or multi-platform acceptance would combine unresolved content, support, and device commitments with the rules-integration risk. A complete 1v1 match on the currently validated macOS path is the smallest player-visible outcome that proves the real game.

## Mode and opponent

- **Mode:** offline 1v1 only.
- **Participants:** one local human and one local bot.
- **Bot scope:** one deterministic baseline policy with no selectable difficulty or personality.
- **Information boundary:** the bot may use public match state and its own private hand only. It must not inspect the local hand or hidden deck order when selecting an intent.
- **Intent boundary:** human and bot choices use the same legal application/domain intent surface.
- **Quality target:** the bot must choose legal actions and finish matches reliably. Strategic challenge, teaching, bluffing, and character behavior are not milestone gates.

## Rules boundary

The first playable implements the complete confirmed 1v1 rules required to reach an authoritative winner:

- dealer selection, rank-only ties, selection-card return, and full reshuffle
- dealer choice of player-first/table-first dealing and ascending/descending opening pattern
- unique opening-table ranks, deterministic duplicate reinsertion, and positional opening scoring
- repeated three-card deals through deck exhaustion
- mandatory same-rank capture, ordered cascades, Falls, and non-final-deal clean tables
- canto opportunity, hidden-hand validation, false announcements and penalties, comparison, timing, and scoring
- leftover assignment, 20-card captured-card quota, round completion, score persistence, and dealer rotation
- the fixed 24-point victory target and complete 1v1 tie-extension rounds
- explicit rejected intents with unchanged authoritative state

### Player-configurable rules

Only these two pre-match options are exposed:

| Option | Default | Alternatives |
| --- | --- | --- |
| Casa Grande and Casa Chica | Enabled | Disabled; their hands fall back to the corresponding Ronda |
| Trivilín effect | Five points | Immediate victory |

Mode, participant count, victory target, deck, turn direction, opening rules, canto set, scoring, and bot policy are not configurable. The deterministic seed is a diagnostic input, not a normal player setting.

When a canto opportunity is open, the human flow must permit any canto announcement so the confirmed false-announcement rule remains playable rather than being silently removed by UI validation.

## Fidelity boundary

### Art and composition

- retain the warm, stylized, culturally neutral medieval-cartoon direction
- use the approved prototype `RoundCardTable`, modular forty-card atlas, shared card material, fixed camera, and 1v1 seat anchors
- use inexpensive upper-body placeholders that preserve identity, active-seat, privacy, and capture-pile readability
- use a minimal, quiet room or background that does not compete with the cards
- do not require production characters, final environment art, final lighting, final court illustrations, or production VFX

### UI

The usable flow is `Home -> Match setup -> Loading -> Match -> Result`, with replay and return-to-Home actions. It must show or explain:

- the two rule options and their defaults
- local and bot identity, dealer, active turn, scores, and round/deal state
- legal, selected, confirmed, rejected, and temporarily blocked card states
- canto opportunity, announcement, resolution, and false-announcement penalty
- Fall, clean-table, captured-card, tie-extension, and winner results

All player-facing text uses stable Localization keys. English and pseudo-localization are required validation locales. Production UI skin, profile, rewards, settings, onboarding, and accessibility certification are deferred.

### Animation and VFX

Resolved state and ordered events remain authoritative before presentation begins. Functional beats are required for dealer selection, dealing, card play, placement, capture, each cascade step, Fall, clean table, canto, score change, round transition, tie extension, and victory.

Normal completion, fast-forward, reduced motion, skip, interruption, cancellation, and scene teardown must converge on the same rendered authoritative state where applicable. Timing, easing, and visual intensity remain presentation configuration. Camera movement is prohibited.

Prototype VFX may use simple arcs, rings, trails, stamps, text, or value changes. Production particles, final easing, haptics, and character acting are not required.

### Audio

Functional prototype cues distinguish deal, play, capture, cascade, Fall, clean table, canto, score, turn/round transition, and victory. Sources must be project-owned, generated, or clearly licensed and must retain provenance. Master and effects controls can silence gameplay audio; a music control exists even if milestone music is omitted.

Final music, ambience, voice acting, dialogue, character vocals, spatial mix, haptics, and production mastering are deferred.

## Platform and validation target

Milestone acceptance is intentionally limited to a macOS universal development player on the project-owned Apple silicon reference Mac using the current project editor and installed macOS environment.

- input: mouse and keyboard
- window: resizable window; fullscreen behavior is not an acceptance gate
- required layouts: `1280 x 720`, `1440 x 900`, `1920 x 1080`, and `2560 x 1440`
- reference measurement layout: `1920 x 1080`
- frame pacing: 60 fps target with p95 frame time at most `16.7 ms` during the measured loop
- memory: peak app memory at most `2.0 GiB`
- cold launch: each of three fully closed launches reaches usable Home within `10 s`
- match load: each of three Home-to-match runs reaches the first accepted interaction within `5 s`
- endurance: five-minute warm-up followed by a 15-minute measured representative loop

Existing historical portrait evidence predates the landscape-only mobile decision. Current touch,
safe-area, and orientation regression coverage uses landscape phone/tablet viewports and both
landscape directions. It is not a claim of broad physical mobile acceptance. Android, iOS, and
Windows remain confirmed product targets but require later build, launch, hardware, and support
decisions.

## Objective acceptance criteria

The first playable is accepted only when all of the following are true:

1. A macOS development player launches into Home and allows a player to configure, complete, replay, and leave a full 1v1 bot match without editor intervention.
2. The full included rule set and both values of each configurable option have automated evidence at the lowest suitable layer.
3. Seeded complete-match simulations finish without invalid bot intents, deadlock, hidden-information access, or inconsistent terminal state.
4. Dealer, turn, cards, table, captures, scores, canto state, round state, and winner remain readable and agree with authoritative state at every required resolution.
5. Normal, fast-forward, reduced-motion, skipped, interrupted, and cancelled presentation paths converge on the same final authoritative state where applicable.
6. `scripts/validate-unity.sh all macos` passes from the candidate commit, and the built universal player launches successfully.
7. The 15-minute sample satisfies the `16.7 ms` p95 frame-time and `2.0 GiB` peak-memory gates.
8. All three cold-launch samples are at most `10 s`, and all three Home-to-match samples are at most `5 s`.
9. Retained audio and asset sources have traceable provenance, licenses, intended use, and prototype replacement status.
10. Skipped platform rows, production-fidelity gaps, defects, and follow-up owners are recorded in the acceptance issue.

## Non-goals

- Story Mode world, route hub, narrative, tournament, progression, objectives, rewards, unlocks, or character selection
- three-player, 2v2, local pass-and-play, or online PvP
- multiple bot difficulties, strategic AI, tutorials, or adaptive behavior
- Android, iOS, Windows, controller, signing, store, distribution, or production support commitments
- save/resume, profiles, analytics, achievements, cloud services, or monetization
- production characters, rigging, final environment, final card art, final VFX, soundtrack, voice, or mastering
- launch-ready accessibility or localization beyond English and pseudo-localization
- GitHub Actions, automated visual regression, or production certification

## Risks and dependencies

| Risk or dependency | Milestone response |
| --- | --- |
| Most confirmed rules exist only as documentation | implement and test them as a pure-domain issue before application or scene integration |
| Full-match orchestration is larger than the representative turn | keep bot/application flow separate from rules and presentation, with seeded complete-match simulations |
| Prototype systems live in separate experiment scenes | integrate through authoritative state and event contracts rather than copying rule logic into a scene |
| `WarmChallenger` is unrigged and 14.7 times the provisional triangle target | keep it out of gameplay and use placeholders; production character work follows measured need |
| Full-match VFX, audio, repeated rounds, and allocations are unmeasured | profile the integrated candidate before accepting the milestone or promoting pooling/frameworks |
| Mobile and Windows paths are unvalidated | retain automated regressions and keep macOS as the milestone gate; use #30 and #31 to establish one physical-iPhone lane without claiming broad iOS support |
| Accessibility and localized readability remain prototype evidence | preserve semantic, non-color-only cues and localization keys; schedule dedicated certification before release |

## Ordered implementation plan

Every implementation issue is assigned to `Donkijote`, has exactly one normal-work label, belongs to the The Fall project, and starts in `Ready`.

1. [#22 Implement the complete deterministic 1v1 rules flow](https://github.com/Donkijote/the-fall/issues/22)
2. [#23 Build first-playable match orchestration and the baseline bot](https://github.com/Donkijote/the-fall/issues/23), after #22
3. [#24 Create the first-playable Home, setup, and result flow](https://github.com/Donkijote/the-fall/issues/24), after #23 exposes the application contract
4. [#25 Integrate the complete 1v1 table presentation](https://github.com/Donkijote/the-fall/issues/25), after #23 and #24
5. [#29 Turn AnimationLab into a real-time sequence workbench](https://github.com/Donkijote/the-fall/issues/29), after #9 and before full-match animation promotion
6. [#26 Complete first-playable event-driven animation](https://github.com/Donkijote/the-fall/issues/26), after #25 and #29
7. [#27 Add functional prototype audio for the first playable](https://github.com/Donkijote/the-fall/issues/27), after #25 and parallel with #26 where practical
8. [#28 Validate and accept the macOS first playable](https://github.com/Donkijote/the-fall/issues/28), after the core issues above

### Parallel iOS enablement and follow-up

- [#30 Configure iOS development builds for a physical iPhone](https://github.com/Donkijote/the-fall/issues/30) can proceed from #10 independently of the core implementation. It establishes local Unity export, Xcode development signing, deployment, and basic prototype checks without committing account secrets.
- [#31 Validate the first playable on a physical iPhone](https://github.com/Donkijote/the-fall/issues/31) follows #28 and #30. It exercises the accepted match with touch, safe areas, rotation, loading, frame pacing, memory, and thermal evidence on Manuel's recorded phone.

These iOS issues add early physical-device evidence. They do not replace #28, change the macOS milestone gate, select a production iOS support floor, or claim App Store readiness.

## Completion checkpoint

The V0 foundation exited with #11. The ordered first-playable implementation issues #22–#28 and the
AnimationLab workbench issue #29 are complete, and issue #30 established the local physical-iPhone
development path. Issue #31 completes the final planned first-playable follow-up with a passing manual
device checklist plus loading, memory, thermal, and state-agreement evidence.

The recorded iPhone is a High-tier device, not the unresolved Reference or Constrained tier. Its
wall-clock and CPU p95 frame times narrowly missed the stricter Reference-mobile `16.7 ms` gate, so
focused issue [#42](https://github.com/Donkijote/the-fall/issues/42) remains in the project backlog.
That follow-up does not reopen the completed V0 foundation or the accepted macOS first-playable
milestone. Android, Windows, representative mobile tiers, production support floors, Story Mode,
additional match modes, online play, and release readiness remain future scoped work rather than
unchecked items in this plan.

The next bounded phase is the [V0.1 1v1 playtest milestone](v0.1-1v1-playtest-milestone.md). It
improves adaptive UI, phone readability, onboarding, presentation comfort, distribution, and external
feedback without committing to Story Mode or another game mode.

## V0 foundation exit decision

**Confirmed:** V0 exits when issue #11 is merged.

The foundation has removed the blocking uncertainties it was created to answer: rules are authoritative, the Unity bootstrap is project-owned, deterministic resolution is proven, composition and interaction are viable, representative assets complete a traceable intake, animation consumes resolved events, validation is repeatable, and the next milestone is an ordered project plan.

The planned work above was first-playable implementation work, not missing V0 discovery. The remaining
mobile-tier evidence, production content, Story Mode, additional modes, and online systems are visible
future decisions and do not reopen the completed V0 foundation or block the bounded macOS first
playable.

Related: [V0 foundation plan](v0-foundation-plan.md), [V0 scope](../product/v0-scope.md), [V0.1 1v1 playtest milestone](v0.1-1v1-playtest-milestone.md), [game modes](../game/modes.md), [validation baseline](../development/validation.md), and [ADR 0002](../decisions/0002-first-playable-milestone.md).
