# Game Glossary

Status: Confirmed V0 baseline

Documentation is written in English. Original names may be retained as canonical proper names for culturally established game concepts.

| Term | Working definition |
| --- | --- |
| The Fall | Public game title. |
| Caída | Traditional game that The Fall adapts; also the origin of the title. |
| Capture | Mandatory resolution when a played card matches the rank already on the table. The table contains at most one card per rank. |
| Cascade | Automatic capture through `1-2-3-4-5-6-7-10-11-12` after a same-rank capture, stopping at the first missing rank. |
| Fall | Immediate same-rank capture by the next player of the non-capturing card just played. It may continue into a cascade. |
| Clean table | A capture that leaves no cards on the table and scores four points outside the final deal. |
| Canto | A scoring pattern formed by a player's three-card hand and announced at the allowed time. |
| Canto announcement | A public claim naming a canto during the player's turn while all three dealt cards remain in hand. The cards and ranks stay hidden. |
| Casa Grande | Optional 12-point canto formed by 12, 12, 1; falls back to Ronda when the shared Casa option is disabled. |
| Casa Chica | Optional 10-point canto formed by 11, 11, 1; falls back to Ronda when the shared Casa option is disabled. |
| Registro | Eight-point canto formed by 12, 11, 1. |
| Vigía | Seven-point canto formed by a pair plus the immediately lower or higher game rank; same-canto strength uses the paired rank. |
| Patrulla | Six-point canto formed by three consecutive game ranks without wrapping from 12 to 1. |
| Trivilín | Three-of-a-kind canto worth five points normally or immediate victory when its pre-match instant-win option is enabled. |
| Ronda | Pair-based canto worth 1 point for ranks 1–7, 2 for rank 10, 3 for rank 11, or 4 for rank 12. |
| Deal | Distribution of three cards to every player plus playing those hands completely. |
| Round | A complete cycle that ends with captured-card counting and dealer rotation. |
| Tie-extension round | A complete normal round played only by participants tied for the highest qualifying score. Lower scorers are eliminated, scores persist, dealer rotation continues among survivors, and the next unique leader wins. |
| Opening table pattern | Positional `1, 2, 3, 4` or `4, 3, 2, 1` pattern selected by the dealer for the first four table cards. Each matching position scores its expected rank value. |
| Opening duplicate | A drawn opening card whose rank is already on the table; it cannot score or remain on the table and is reinserted randomly into the deck before drawing a replacement. |
| Story Mode | Offline campaign with a predefined cast, progressively harder bot matches, tournament structure, custom objectives, and rule variations. |
| Player intent | A platform-neutral requested action submitted by either a human or bot to the same legal rules surface. |
| Rule result | The explicit accepted or rejected outcome of resolving an intent, containing the next state and ordered events or an error with unchanged state. |
| Domain event | An immutable fact emitted after rule resolution for application and presentation consumers; it describes an outcome but does not ask presentation to decide it. |

Future localization must map stable concept identifiers to translated display text rather than changing code identifiers per language.
