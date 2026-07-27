# Login Gateway Background

Status: Accepted UI background candidate

## Asset record

| Field | Value |
| --- | --- |
| identifier | `LoginGatewayBackground` |
| owner | The Fall project |
| source and creation date | OpenAI built-in image generation, 2026-07-27 |
| original generated file | Codex generated-image archive, call `DBlgJPyHco6MmQAJVg53Y1Oh` |
| Unity-ready file | `Assets/TheFall/Content/UI/LoginGatewayBackground.png` |
| dimensions | `1672 × 941`, RGB PNG |
| intended use | full-screen gateway backdrop behind localized UI Toolkit controls |
| reference inputs | none; the supplied UI reference informed the written composition brief only |
| processing | copied without raster edits; Unity generates platform import metadata |
| replacement status | accepted UI candidate; production approval still requires target-device review |

## Generation prompt

```text
Use case: stylized-concept
Asset type: full-screen 16:9 login background for a fantasy card game
Primary request: create a polished dark medieval gateway landscape for The Fall login screen,
inspired by the composition of a distant castle framed by forested cliffs, but entirely original artwork
Scene/backdrop: an ancient hilltop citadel with narrow towers in the middle distance, dense shadowed
woodland and steep rocky slopes framing both sides, low ground fog, a subtle open path leading toward
the stronghold
Style/medium: cinematic realistic digital matte painting, premium game key art, restrained detail so
overlaid UI remains readable
Composition/framing: wide landscape, castle centered in the middle distance; darker low-detail negative
space across the left third for hero copy and the right third for a login panel; no foreground people
or props
Lighting/mood: overcast dusk, ominous but inviting, soft mist and subdued atmospheric depth
Color palette: The Fall palette—lampblack and charred walnut shadows, aged vellum mist, restrained
antique brass highlights, muted woad blue distance, faint madder warmth
Constraints: background artwork only; no interface, no panels, no buttons, no characters, no circles,
no abstract geometric overlays, no icons, no readable signs
Avoid: text, logos, watermark, neon colors, bright daylight, modern buildings, copyrighted characters,
obvious resemblance to a specific real castle
```

## Usage boundary

The texture contains no embedded UI or player copy. UI Toolkit owns all text, controls, focus states,
responsive composition, and dimming. The image must remain decorative and cannot encode navigation,
game state, card identity, or other information needed to use the gateway.

The generated output is retained under the project asset policy. Before production release, review
commercial-use terms associated with the generation service at the retained creation date and repeat
contrast, crop, memory, and compression checks on every supported device tier.
