# Game Modes

Status: Draft

## Shared rules

All modes use the same deck, capture, cascade, Fall, clean-table, canto, round, and victory concepts unless this document explicitly defines a mode variation.

## 1v1

**Confirmed:** Two individual players compete. Each player has a 20-card captured-card quota before extra-card points begin.

**Confirmed:** Use 1v1 against one bot as the first architecture-proving mode because it has the smallest state and presentation surface. That V0 technical decision supplied the basis for the later first-playable selection.

**Confirmed:** The first playable extends that foundation into one complete offline 1v1 match against a deterministic baseline bot. See the [first playable milestone](../planning/first-playable-milestone.md) for the exact rules, fidelity, and macOS acceptance boundary.

## Three-player free-for-all

**Confirmed:** Three individual players compete.

Captured-card counting gives the dealer a quota of 13 and each other player a quota of 12.

Distribute all three seats evenly around the table. Anchor the local player at the bottom of the screen. The two opponent identities may be assigned randomly to the remaining seats around the upper, left, and right portions of the composition while preserving logical counter-clockwise order.

When the round-completion victory check leaves two players tied for the highest score at or above 24, eliminate the lower-scoring third player. Only the two tied leaders play the tie-extension round and continue extensions until one gains a strictly higher score.

If all three players tie at the highest score, all three continue under normal three-player rules. If two survive, switch to standard 1v1 rules and quotas. Dealer rotation continues among survivors without a new dealer-selection phase, and every extension uses the complete normal opening setup.

## 2v2

**Confirmed:** Four players compete as two teams. Team members share the match outcome and team score.

Captured-card counting uses a 20-card quota per team.

Team members sit opposite each other, causing counter-clockwise turns to alternate between teams. Dealer rotation passes through every individual seat; the player immediately to the dealer's right becomes the next dealer, including when that seat belongs to the opposing team.

Every hand remains private, including between teammates.

Keep captured cards visibly associated with the individual who captured them. Combine both teammates' captured-card counts only when calculating the team's round-completion quota.

All scoring is shared by the team:

- opening table patterns
- cantos
- Falls
- clean tables
- captured-card excess
- false-canto penalties

Both teammates may announce cantos independently, but all four announcements enter one global comparison. Only the strongest valid canto scores for its team.

If both teams remain tied at 24 or more, both teams continue complete normal tie-extension rounds until one becomes the unique leader.

## Story Mode

**Confirmed:** Story Mode is an offline campaign played against bots. It uses a predefined cast and a tournament-like structure with progressively harder matches. Matches may introduce custom objectives and rule variations.

**Deferred:** Character creation and customization may be considered for a future version but are not part of the initial Story Mode direction.

**Open:** Define its narrative structure, match sequence, difficulty curve, custom objectives, rule variations, rewards, unlocks, and whether it teaches rules incrementally.

## Online PvP

**Confirmed:** Online player-versus-player multiplayer is part of the long-term product.

**Deferred:** Networking technology, matchmaking, authoritative hosting, reconnection, anti-cheat, and social features are outside the initial V0 implementation.

## Delivery decision

**Confirmed:** Deliver the complete offline 1v1 bot match before Story Mode, three-player, 2v2, or online PvP. The first playable proves the integrated rules and presentation loop; it does not establish the long-term public release order for the deferred modes.
