# Decision Log

This directory records durable product and technical decisions. Create one Markdown file per substantial decision when alternatives, consequences, or later reversal matter.

## Decision states

- Proposed
- Accepted
- Superseded
- Rejected

## Accepted baseline decisions

| Decision | State | Date | Source |
| --- | --- | --- | --- |
| Public title is The Fall | Accepted | 2026-07-19 | Issue #1 discussion |
| Documentation is written in English | Accepted | 2026-07-19 | Issue #1 discussion |
| UI is designed for future localization | Accepted | 2026-07-19 | Issue #1 discussion |
| Visual direction is stylized medieval cartoon | Accepted | 2026-07-19 | Issue #1 discussion |
| The fictional setting is culturally neutral | Accepted | 2026-07-19 | Issue #1 discussion |
| Players use upper-body character representations | Accepted | 2026-07-19 | Issue #1 discussion |
| Camera is a fixed cinematic overhead table view | Accepted | 2026-07-19 | Issue #1 discussion |
| Target platforms are Android, iOS, Windows, and macOS | Accepted | 2026-07-19 | Issue #1 discussion |
| Mobile supports landscape and portrait | Accepted | 2026-07-19 | Issue #1 discussion |
| Mobile uses touch; desktop uses mouse and keyboard | Accepted | 2026-07-19 | Issue #1 discussion |
| Story Mode is an offline tournament-like campaign against bots; online PvP comes later | Accepted | 2026-07-19 | Issue #1 discussion |
| Story Mode uses a predefined cast; character customization is deferred | Accepted | 2026-07-19 | Issue #1 discussion |
| Use 1v1 against one bot for the first technical rules prototype | Accepted | 2026-07-19 | Issue #1 discussion |
| Gameplay camera remains completely stationary | Accepted | 2026-07-19 | Issue #1 discussion |
| Active mobile matches immediately recompose after device rotation | Accepted | 2026-07-19 | Issue #1 discussion |
| Adopt Unity Localization from the project bootstrap | Accepted | 2026-07-19 | Issue #1 discussion |
| English suit names are Coins, Cups, Swords, and Clubs | Accepted | 2026-07-19 | Issue #1 rules interview |
| Dealer selection uses a face-down spread of the full deck with rank-only tie breaking | Accepted | 2026-07-19 | Issue #1 rules interview |
| Selection cards return to the deck followed by a complete reshuffle | Accepted | 2026-07-19 | Issue #1 rules interview |
| Deal player hands one card at a time | Accepted | 2026-07-19 | Issue #1 rules interview |
| Only a dealer's first deal adds four table cards and opening-pattern scoring | Accepted | 2026-07-19 | Issue #1 rules interview |
| Opening pattern scoring is positional and awards the expected rank value, up to 10 points | Accepted | 2026-07-19 | Issue #1 rules interview |
| Duplicate opening ranks cannot score or enter the table and are randomly reinserted before replacement | Accepted | 2026-07-19 | Issue #1 rules interview |
| A unique replacement retains the rejected card's opening position and may score normally | Accepted | 2026-07-19 | Issue #1 rules interview |
| Dealer-selection ties continue from the remaining face-down spread | Accepted | 2026-07-19 | Issue #1 rules interview |
| Deal begins to the dealer's right and continues counter-clockwise | Accepted | 2026-07-19 | Issue #1 rules interview |
| The table contains at most one card of each rank | Accepted | 2026-07-19 | Issue #1 rules interview |
| Playing a rank already on the table always triggers a capture | Accepted | 2026-07-19 | Issue #1 rules interview |
| Cascades follow `1-2-3-4-5-6-7-10-11-12` and stop at the first missing rank | Accepted | 2026-07-19 | Issue #1 rules interview |
| Captures begin only from equal ranks; table-card values are not combined | Accepted | 2026-07-19 | Issue #1 rules interview |
| Fall scores 1 for ranks 1–7, 2 for rank 10, 3 for rank 11, and 4 for rank 12 | Accepted | 2026-07-19 | Issue #1 rules interview |
| A Fall requires an immediate capture of the previous player's non-capturing play | Accepted | 2026-07-19 | Issue #1 rules interview |
| Fall captures may cascade and their score stacks with clean-table scoring | Accepted | 2026-07-19 | Issue #1 rules interview |
| Clean-table scoring is disabled throughout the final three-card hands | Accepted | 2026-07-19 | Issue #1 rules interview |
| Round leftovers go to the last capturer or, if none exists, the player to the dealer's right | Accepted | 2026-07-19 | Issue #1 rules interview |
| Captured-card points resolve before the round-completion victory check | Accepted | 2026-07-19 | Issue #1 rules interview |
| If nobody wins, the dealer position passes one seat to the right | Accepted | 2026-07-19 | Issue #1 rules interview |
| Captured-card quotas are 20 in 1v1, dealer 13 and others 12 in three-player, and team 20 in 2v2 | Accepted | 2026-07-19 | Issue #1 rules interview |
| Only captured cards exceeding a quota award points | Accepted | 2026-07-19 | Issue #1 rules interview |
| Canto scoring requires an announcement before playing the qualifying hand | Accepted | 2026-07-19 | Issue #1 rules interview |
| Canto announcements happen on the player's turn while all three dealt cards remain in hand | Accepted | 2026-07-19 | Issue #1 rules interview |
| Announcements reveal the canto name but keep the qualifying cards and ranks hidden | Accepted | 2026-07-19 | Issue #1 rules interview |
| False canto announcements are allowed and cost the player or team one point | Accepted | 2026-07-19 | Issue #1 rules interview |
| With multiple announcements, only the strongest valid canto scores | Accepted | 2026-07-19 | Issue #1 rules interview |
| A deal covers one complete set of three-card hands; a round consumes the complete deck | Accepted | 2026-07-19 | Issue #1 rules interview |
| Casa Grande and Casa Chica share a pre-match enable/disable option | Accepted | 2026-07-19 | Issue #1 rules interview |
| A non-winning single canto scores at the end of the current deal | Accepted | 2026-07-19 | Issue #1 rules interview |
| Canto validation is internal and does not display separate proof to opponents | Accepted | 2026-07-19 | Issue #1 rules interview |
| False-canto penalties resolve at deal end and cannot reduce a score below zero | Accepted | 2026-07-19 | Issue #1 rules interview |
| Trivilín always exists; its option switches between five points and immediate victory | Accepted | 2026-07-19 | Issue #1 rules interview |
| Ronda scores from its pair rank: 1 for ranks 1–7, 2 for 10, 3 for 11, and 4 for 12 | Accepted | 2026-07-19 | Issue #1 rules interview |
| Same-canto ties compare underlying rank strength, then dealer-right order | Accepted | 2026-07-19 | Issue #1 rules interview |
| A false strongest announcement is penalized and the strongest remaining valid canto scores | Accepted | 2026-07-19 | Issue #1 rules interview |
| Opening-table-pattern points resolve positionally and may produce an immediate victory | Accepted | 2026-07-19 | Issue #1 rules interview |
| Vigía uses immediate lower/upper adjacency and crosses between ranks 7 and 10 | Accepted | 2026-07-19 | Issue #1 rules interview |
| Vigía strength is determined by its paired rank | Accepted | 2026-07-19 | Issue #1 rules interview |
| Patrulla uses three consecutive game ranks, crosses 7–10, does not wrap, and compares highest rank | Accepted | 2026-07-19 | Issue #1 rules interview |
| Disabled Casa patterns fall back to their corresponding Ronda scores | Accepted | 2026-07-19 | Issue #1 rules interview |
| Active configuration classifies each hand as at most one valid canto | Accepted | 2026-07-19 | Issue #1 rules interview |
| Teammates announce independently but canto comparison is global across all players | Accepted | 2026-07-19 | Issue #1 rules interview |
| False-canto penalties are cumulative and resolve before the winning canto scores | Accepted | 2026-07-19 | Issue #1 rules interview |
| Immediate Fall or clean-table victory may pre-empt unresolved cantos | Accepted | 2026-07-19 | Issue #1 rules interview |
| Highest total wins when multiple players cross 24 after captured-card counting | Accepted | 2026-07-19 | Issue #1 rules interview |
| Equal leaders at or above 24 continue playing rounds until one gains a higher score | Accepted | 2026-07-19 | Issue #1 rules interview |
| Tie extensions eliminate lower scorers and include only players tied for the highest score | Accepted | 2026-07-19 | Issue #1 rules interview |
| A three-way tie continues under three-player rules; two survivors switch to standard 1v1 | Accepted | 2026-07-19 | Issue #1 rules interview |
| Tie-extension dealer rotation continues among survivors without repeating dealer selection | Accepted | 2026-07-19 | Issue #1 rules interview |
| Tie extensions use the complete opening setup and the next unique leader wins | Accepted | 2026-07-19 | Issue #1 rules interview |
| Three-player seating is even, with the local player anchored at the bottom | Accepted | 2026-07-19 | Issue #1 rules interview |
| 2v2 teammates sit opposite and dealer rotation passes through every seat | Accepted | 2026-07-19 | Issue #1 rules interview |
| Team hands remain private and captured piles remain visually individual | Accepted | 2026-07-19 | Issue #1 rules interview |
| Every point and false-canto penalty is shared by the team | Accepted | 2026-07-19 | Issue #1 rules interview |
| Tied teams continue complete rounds until one becomes the unique leader | Accepted | 2026-07-19 | Issue #1 rules interview |
| Scores persist between rounds while round-scoped card state resets | Accepted | 2026-07-19 | Issue #1 rules interview |
| Asset storage uses normal Git, Git LFS, and an external working archive by asset state | Accepted | 2026-07-19 | Issue #1 discussion |
| Prototype assets are free or generated until direction is proven | Accepted | 2026-07-19 | Issue #1 discussion |
| Rules live in deterministic pure C# outside Unity presentation | Accepted | 2026-07-19 | Issue #1 discussion |
| Unity tutorial content will be replaced by a project-owned bootstrap | Accepted | 2026-07-19 | Issue #1 discussion |
| Track the latest suitable production Unity release as closely as practical | Accepted | 2026-07-19 | Issue #1 architecture interview |
| Project-owned Unity content uses `Assets/TheFall` and the `TheFall` root namespace | Accepted | 2026-07-19 | Issue #1 architecture interview |
| Assembly definitions separate Domain, Application, Infrastructure, Presentation, and test code | Accepted | 2026-07-19 | Issue #1 architecture interview |
| Domain code has no `UnityEngine` dependency | Accepted | 2026-07-19 | Issue #1 architecture interview |
| Bootstrap, Home, MatchPrototype, and AnimationLab are the initial project scenes | Accepted | 2026-07-19 | Issue #1 architecture interview |
| Dependencies are composed manually until a demonstrated need justifies a framework | Accepted | 2026-07-19 | Issue #1 architecture interview |
| UI Toolkit handles adaptive screen-space UI while uGUI and TextMeshPro support world-space UI | Accepted | 2026-07-19 | Issue #1 architecture interview |
| The Input System maps touch, mouse, and keyboard into shared application intents | Accepted | 2026-07-19 | Issue #1 architecture interview |
| Initial assets use direct references and ScriptableObjects rather than Addressables | Accepted | 2026-07-19 | Issue #1 architecture interview |
| Third-party architecture frameworks require an accepted decision record | Accepted | 2026-07-19 | Issue #1 architecture interview |
| Issue work branches from main using `<category>/ghi#<issue-number>` | Accepted | 2026-07-19 | Project workflow |

## ADR template

Use a zero-padded filename such as `0001-example-decision.md`.

```markdown
# Decision title

Status: Proposed
Date: YYYY-MM-DD

## Context

What problem or constraint requires a decision?

## Options

What credible alternatives were considered?

## Decision

What was selected and why?

## Consequences

What becomes easier, harder, required, or intentionally excluded?
```

When a decision is superseded, keep the original file and link it to the replacement.
