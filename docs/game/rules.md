# Game Rules

Status: Confirmed V0 baseline

This document is the authoritative V0 rules specification for The Fall. It incorporates the completed issue #1 rules interview. Any future ambiguity or proposed change must be recorded explicitly rather than inferred from the previous project.

## Deck

**Confirmed:** Use a 40-card Spanish deck with four suits and ranks 1–7 and 10–12. Ranks 8 and 9 are excluded.

**Confirmed terminology:** The English suit names are `Coins`, `Cups`, `Swords`, and `Clubs`.

## Objective

**Confirmed:** The first player or team to reach 24 points wins.

## Supported rulesets

**Confirmed:** The rules include 1v1, three-player free-for-all, and 2v2. Mode-specific differences are described in [modes](modes.md).

## Dealer selection

**Confirmed:**

1. Shuffle the complete 40-card deck.
2. Spread every card face-down in a straight line.
3. Each player selects any card from the full spread.
4. Reveal the selected cards.
5. The player with the highest rank becomes the dealer.
6. If multiple players share the highest rank, only those tied players select again from the remaining face-down cards until the tie is broken.
7. Suits have no tie-breaking power.
8. Return all selection cards to the deck and shuffle the complete deck again before dealing.

## Turn direction

**Confirmed:** Play proceeds counter-clockwise. The player to the dealer's right acts first.

## Dealing and initial table

Each deal gives every player three cards. Begin with the player to the dealer's right and deal one card at a time counter-clockwise until every player has three.

### Dealer's first deal

Only the first deal performed by the current dealer includes four initial table cards. Before that deal, the dealer chooses:

- whether to deal the player hands before the table or the table before the player hands
- the `Ascending` or `Descending` opening table pattern

The opening table cards are revealed in order and remain available for normal play.

Every opening table rank must be unique. For each table position:

1. Draw the top card of the deck.
2. If its rank is not on the table, place it in the current position and evaluate that position's opening-pattern score.
3. If its rank duplicates a table rank, do not place it and do not award opening-pattern points for that duplicate, even when its rank matches the selected pattern position.
4. Reinsert the duplicate card at a random position in the remaining deck.
5. Draw again from the top until a unique rank can occupy the table position.
6. Evaluate the accepted unique replacement against the same opening-pattern position. Award its positional points normally when it matches.

Use the match's injected deterministic random source for reinsertion. Duplicate ranks are never allowed to remain on the table.

### Opening table pattern

The patterns are exclusive and positional:

- `Ascending`: positions expect `1, 2, 3, 4`
- `Descending`: positions expect `4, 3, 2, 1`

Each position scores independently when the revealed card's rank matches that position's expected rank. The awarded points equal the expected rank:

| Pattern position | Ascending expects | Descending expects | Points when matched |
| ---: | ---: | ---: | ---: |
| 1 | 1 | 4 | Expected rank value |
| 2 | 2 | 3 | Expected rank value |
| 3 | 3 | 2 | Expected rank value |
| 4 | 4 | 1 | Expected rank value |

Example: with `Ascending` selected, revealing `7, 2, 11, 4` scores `2 + 4 = 6` points. A perfect match scores `1 + 2 + 3 + 4 = 10` points.

If the first accepted card is rank 2 and the next draw is also rank 2, the second card is a duplicate. It scores nothing for the second `Ascending` position, returns to a random position in the deck, and is replaced by another top-deck draw.

Award opening-pattern points to the dealer in individual modes or the dealer's team in 2v2.

No other sequence qualifies. In particular, patterns cannot cross the missing ranks or use a sequence such as `6, 7, 10, 11`.

### Later deals by the same dealer

After players use all three cards, deal the next hand only to the players. Do not add new table cards and do not repeat opening-pattern scoring. Continue until the deck is exhausted and the round ends.

## Player turn

A turn contains these intents:

1. Announce an eligible canto when the timing rules allow it.
2. Play one card.
3. Capture a valid same-rank card and any valid cascade, or leave the played card on the table.
4. Resolve scoring, animation events, end-of-hand state, and the next player.

## Capture

The table maintains a strict invariant: at most one card of each rank may be present.

When a player plays a card:

- if its rank is absent from the table, place the card on the table
- if its rank is present, capture is mandatory and automatic

A capture always begins from the same rank. Table-card combinations cannot be captured by adding their values to equal the played card.

Move the played card, the same-rank table card, and every card captured by the resulting cascade to the player's captured pile. In 2v2, those cards contribute to the shared team result.

## Cascade

After the same-rank capture, continue automatically through this ordered rank sequence:

```text
1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 10 -> 11 -> 12
```

Starting immediately after the captured rank, capture each next rank while it is present on the table. The first missing rank stops the cascade. Do not skip a gap even if a later rank is available.

Example: playing a 2 when the table contains ranks 2, 3, 4, and 6 captures 2, 3, and 4, then stops because rank 5 is missing.

Because the table contains at most one card per rank, neither the same-rank capture nor a cascade requires duplicate selection.

## Fall

**Confirmed:** A Fall occurs only when:

1. the immediately previous player plays a non-capturing card onto the table
2. the very next player plays the same rank, triggering its mandatory capture

The Fall capture continues into a normal cascade when the next ranks are available.

Fall scoring is:

| Captured rank | Points |
| --- | ---: |
| 1–7 | 1 |
| 10 | 2 |
| 11 | 3 |
| 12 | 4 |

Award the points to the capturing player in individual modes or the capturing player's team in 2v2.

Fall and clean-table scoring stack. If a Fall capture empties the table outside the final deal, award both the rank-based Fall points and the four clean-table points.

## Clean table

**Confirmed:** A capture that empties the table awards four points except during the final deal.

The final deal begins as soon as the deck becomes empty after distributing the final three-card hands. No turn played from those final hands can receive a clean-table bonus, even if its capture empties the table.

## Cantos

**Confirmed:** Casa Grande, Casa Chica, Registro, Vigía, Patrulla, Trivilín, and Ronda are core game logic and must be represented intentionally.

### Announcement and timing

A canto announcement opportunity occurs during the player's own turn while all three cards from the current deal remain in that player's hand. The player must announce before playing the first card. Once that first card is played, the opportunity is permanently lost for that deal, even when the unannounced pattern would have been the strongest.

The announcement publicly reveals the claimed canto name only. Do not reveal the card ranks, suits, or the three-card hand at announcement time. Preserve the qualifying hand privately for later validation because its cards may have been played before canto resolution.

False canto announcements are allowed. At the end of the current deal, exclude each false claim and subtract one point from its announcing player or team, clamped to a minimum score of zero. If the strongest claim was false, the strongest remaining valid canto scores normally.

Fall and clean-table points resolve immediately. If either causes a player or team to win before pending cantos resolve, the match ends and those pending cantos do not change the result.

When exactly one player announces, validate the claim privately. If the valid canto's points would bring that player or team to at least 24, award it immediately and end the match. Otherwise, award it at the end of the current deal.

When multiple players announce cantos, resolve them after all players finish playing the current three-card deal. Only the strongest valid announced canto scores; every other valid announced canto scores zero.

**Confirmed terminology:** A `Deal` distributes three cards to each player and ends when those hands are completely played. A `Round` repeats deals until the complete deck is exhausted, then assigns leftovers and counts captured cards.

Validate cantos internally against the preserved three-card hand. Do not display a separate proof or reconstruct the hand for other players; they can infer it from the cards played during the deal.

The canto table is:

| Canto | Pattern | Score or effect |
| --- | --- | ---: |
| Casa Grande | 12, 12, 1 | 12 |
| Casa Chica | 11, 11, 1 | 10 |
| Registro | 12, 11, 1 | 8 |
| Vigía | Pair plus adjacent rank | 7 |
| Patrulla | Three consecutive ranks | 6 |
| Trivilín | Three of a kind | 5 points or immediate victory, by rule option |
| Ronda | Pair plus any other card | Pair ranks 1–7: 1; 10: 2; 11: 3; 12: 4 |

Casa Grande and Casa Chica are controlled together by one shared pre-match option.

When that option is disabled, their patterns fall back to ordinary Rondas:

- `12, 12, 1` is a Ronda of 12 worth four points
- `11, 11, 1` is a Ronda of 11 worth three points

Trivilín always exists. Its pre-match option selects its effect:

- disabled: award five points
- enabled: win the match immediately

### Pattern boundaries

Use the game rank order `1-2-3-4-5-6-7-10-11-12` for canto adjacency.

- Vigía is a pair plus the rank immediately below or above the pair. Adjacency crosses between 7 and 10, so `7, 7, 10` and `10, 10, 7` are valid.
- Patrulla is any three consecutive ranks in that order. `6, 7, 10` and `7, 10, 11` are valid. The sequence does not wrap from 12 to 1.
- Patrulla strength is determined by its highest rank.
- Canto classification returns exactly one valid pattern for a hand under the active rule configuration. A hand never resolves as two valid cantos simultaneously.

### Canto comparison

When multiple valid cantos are announced:

1. The canto with the highest value or effect wins.
2. If players announced the same canto, compare its underlying strength: Ronda and Vigía use their paired rank, Trivilín uses its triple, and Patrulla uses its highest rank.
3. If strength is identical, compare players in order beginning with the player to the dealer's right and continuing counter-clockwise. The closest player in that order wins.
4. Award points only to the winning player or team. Every other valid announcement scores zero.

For Vigía, the higher paired rank wins. For example, `10, 10, 7` is stronger than `7, 7, 10`.

In 2v2, both teammates may announce independently. Compare all four players' announcements globally; only the strongest valid announcement scores for its team.

At deal resolution, apply all false-announcement penalties before awarding the winning canto. Each false claim creates a separate one-point penalty, including two penalties when both teammates falsely announce, with the team score still clamped to zero.

## End of deal and round

When the deck and every player hand are empty:

1. Give all remaining table cards to the last player who captured during the round.
2. If nobody captured during the entire round, give the remaining cards to the player on the dealer's right.
3. Count captured cards and award the resulting points.
4. Determine whether a player or team has reached at least 24 points and won the match.
5. If nobody won, pass the dealer position one seat to the right and begin the next round.

## Captured-card counting

Confirmed captured-card quotas are:

- 1v1: each player counts to 20; every extra card scores one point.
- Three-player: non-dealers count to 12 and the dealer to 13; every extra card scores one point.
- 2v2: each team counts to 20; every extra card scores one point.

Exactly meeting a quota awards zero points. Only cards exceeding the applicable quota score.

Captured-card counting happens before the round-completion victory check because its points may cause a player or team to reach or exceed 24.

## Victory timing and ties

- Fall and clean-table points update the score immediately and end the match immediately when they produce an uncontested lead at or above 24, subject to the canto timing rules above.
- Opening-table-pattern points update the score after each revealed table-card position and may end the match immediately when the dealer or team reaches 24.
- Canto points follow their announcement and reveal timing.
- Captured-card points resolve at round completion after leftover cards are assigned.
- If multiple players reach at least 24 during three-player captured-card counting, the highest total score wins.
- If the highest totals are tied, eliminate every lower-scoring player from the match. Only players tied at the highest qualifying score continue into tie-extension rounds.
- If all three players share the same highest score, all three continue under normal three-player rules.
- If exactly two players remain tied, switch completely to standard 1v1 rules, including the 20-card captured-card quota.
- Continue dealer rotation among surviving players only. Do not repeat the face-down dealer-selection process. With two survivors, the dealer alternates between them.
- Every tie-extension round uses the complete normal setup and rules, including the dealer's opening table deal, pattern choice, and positional scoring.
- Continue tie-extension rounds until one remaining player gains a higher score than the other tied participants. Scores persist at or above 24 during this extension.
- The next scoring outcome that produces a unique leader decides the match according to its normal timing. Opening-pattern points may therefore end a tie before normal turns begin.

Scores persist unchanged between rounds. Reset the deck, hands, table, captured-card piles, and other round-scoped state before the next round.

## Rule-review process

Each rule must ultimately include:

- preconditions
- player intent
- deterministic resolution
- score effects
- next state
- invalid-action behavior
- mode-specific differences
- presentation events emitted after resolution

Related: [glossary](glossary.md), [modes](modes.md), and [architecture](../technical/architecture.md).
