# UI Icon Library

Status: Accepted UI asset set

## Decision

The Fall uses curated PNG icons from [Game-icons.net](https://game-icons.net/) for both interface
actions and thematic game symbols. This keeps the gateway, Home hub, Settings, match controls, and
result screen inside one high-contrast silhouette language instead of mixing Unicode glyphs or
unrelated icon families.

Only icons referenced by the current UI are retained. The complete collection is not vendored.
Additional icons must be added individually with their source, author, license, and intended role.

## License and processing

The retained icons are licensed under
[Creative Commons Attribution 3.0](https://creativecommons.org/licenses/by/3.0/).
Each file is the `512 × 512` white-on-transparent PNG supplied by Game-icons.net. The binary artwork
is not redrawn or converted; UI Toolkit applies aged-vellum, antique-brass, woad, madder, moss, or
lampblack tint according to context. File names are normalized to the role they serve in The Fall.

The Settings panel exposes the author, source, and license attribution in the playable UI.
`Assets/TheFall/Content/UI/Icons/ATTRIBUTION.md` retains the same credit beside the imported assets.

## Retained icons

| Local file | Icon and source | Author | Current role |
| --- | --- | --- | --- |
| `audio.png` | [Speaker](https://game-icons.net/1x1/delapouite/speaker.html) | Delapouite | audio Settings |
| `bag.png` | [Backpack](https://game-icons.net/1x1/delapouite/backpack.html) | Delapouite | Bag destination |
| `canto.png` | [Scroll unfurled](https://game-icons.net/1x1/lorc/scroll-unfurled.html) | Lorc | canto action |
| `clubs.png` | [Wood club](https://game-icons.net/1x1/delapouite/wood-club.html) | Delapouite | Clubs suit motif |
| `coins.png` | [Two coins](https://game-icons.net/1x1/delapouite/two-coins.html) | Delapouite | Coins resource and suit motif |
| `cups.png` | [Jeweled chalice](https://game-icons.net/1x1/lorc/jeweled-chalice.html) | Lorc | Cups suit motif |
| `decks.png` | [Card draw](https://game-icons.net/1x1/faithtoken/card-draw.html) | Faithtoken | Decks and dealer action |
| `energy.png` | [Bolt drop](https://game-icons.net/1x1/delapouite/bolt-drop.html) | Delapouite | energy resource |
| `envelope.png` | [Envelope](https://game-icons.net/1x1/lorc/envelope.html) | Lorc | email and mailbox |
| `gems.png` | [Cut diamond](https://game-icons.net/1x1/lorc/cut-diamond.html) | Lorc | gems resource |
| `home.png` | [House](https://game-icons.net/1x1/delapouite/house.html) | Delapouite | return Home |
| `padlock.png` | [Padlock](https://game-icons.net/1x1/lorc/padlock.html) | Lorc | password field |
| `quest.png` | [Crossed swords](https://game-icons.net/1x1/lorc/crossed-swords.html) | Lorc | quest, Swords, and match rules |
| `rank.png` | [Trophy](https://game-icons.net/1x1/lorc/trophy.html) | Lorc | Rank and match result |
| `replay.png` | [Cycle](https://game-icons.net/1x1/lorc/cycle.html) | Lorc | replay |
| `send.png` | [Paper plane](https://game-icons.net/1x1/delapouite/paper-plane.html) | Delapouite | local chat send |
| `settings.png` | [Cog](https://game-icons.net/1x1/lorc/cog.html) | Lorc | Settings |
| `shield.png` | [Crenulated shield](https://game-icons.net/1x1/lorc/crenulated-shield.html) | Lorc | gateway and profile identity |
| `shop.png` | [Shop](https://game-icons.net/1x1/delapouite/shop.html) | Delapouite | Shop destination |
| `skip.png` | [Fast forward button](https://game-icons.net/1x1/delapouite/fast-forward-button.html) | Delapouite | motion Settings and animation skip |

## Import and usage

- retain transparent alpha and disable wrap
- retain a `256 px` uncompressed import with mipmaps and trilinear filtering so the same source stays
  clean in small desktop treatments and larger phone profiles
- reference icons directly from UXML/USS; do not load them dynamically or add a runtime package
- keep localized text or a localized tooltip for every action so meaning never depends on the icon alone
- preserve the same icon for a semantic action across desktop, mobile portrait, and mobile landscape
- add a new attribution row whenever another Game-icons.net PNG enters the repository

Related: [asset strategy](strategy.md), [art direction](../design/art-direction.md), and
[first-playable application flow](../technical/first-playable-flow.md).
