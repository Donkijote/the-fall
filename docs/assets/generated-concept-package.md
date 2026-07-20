# V0 Generated Concept Package

Status: Concept-generation checkpoint; Meshy conversion deferred for manual owner work

## Scope boundary

This package records the image-generation portion of issue #8 for the three representative V0 assets. It deliberately stops before upload, conversion, credit use, download, optimization, or Unity model import in Meshy AI. Manuel chose on 2026-07-20 to perform that external step manually later.

These images are approved as **visual anchors for consistent follow-up generation**. They are not approved 3D prototype assets and do not satisfy issue #8's end-to-end Meshy, Unity import, or performance gates.

## Shared artistic lock

Every follow-up image should preserve this family before introducing new creative choices:

- warm, culturally neutral medieval-cartoon design
- broad planar silhouettes and handcrafted construction
- matte stylized 3D rendering with low-frequency surface detail
- lampblack, walnut, charred walnut, moss, aged vellum, restrained woad blue, and antique brass
- neutral studio presentation for review; warm broad key and restrained cool fill in gameplay context
- cards first, character and hands second, table construction third, background last
- no named artist or commercial-game imitation, culture-specific motif, heraldry, religious mark, casino language, glossy surface, or dense procedural wear

The [art direction](../design/art-direction.md), [reference board](../design/visual-reference-board.md), and [prototype briefs](prototype-briefs.md) remain authoritative. The prompt source revision is commit `5187e2885d03c0a32a88a37c441e60f3961496ab`.

## Retained files

All retained PNG files are covered by the repository's Git LFS rules.

| Asset | Purpose | SHA-256 |
| --- | --- | --- |
| [`CHR-P-WARM-CHALLENGER_Concept.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/CHR-P-WARM-CHALLENGER_Concept.png) | clean character conversion anchor | `e0e309edbefd5625726e8f2b42a844d03646b2ac073ad415a98a00e879c43a47` |
| [`CHR-P-WARM-CHALLENGER_ReviewSheet.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Review/CHR-P-WARM-CHALLENGER_ReviewSheet.png) | turnaround, expressions, and elevated clay review | `ade41c3bb96e559b1f198c00858f5424c9c1dc106cd9eb58ecd646dfc1543e19` |
| [`CHR-P-WARM-CHALLENGER_GestureSheet.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Review/CHR-P-WARM-CHALLENGER_GestureSheet.png) | silhouettes, hand studies, and material swatches | `12e14148cd0610f38ff3c018f956d121c5231eda57d77017519c67dc236a24e8` |
| [`ENV-P-ROUND-TABLE_Concept.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/ENV-P-ROUND-TABLE_Concept.png) | clean table conversion anchor | `fa960584c2d104d0b9c1873a9c2dba8b33b768461a59e033455a9e55cbcac5dd` |
| [`ENV-P-ROUND-TABLE_ReviewSheet.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Review/ENV-P-ROUND-TABLE_ReviewSheet.png) | orthographic, underside, card-readability, and exploded review | `30c92767624ca2c3154c4e9ed8364b02d430f8a1a212ab2f84f1fa90d954995a` |
| [`CRD-P-FOUR-COINS_Concept.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/CRD-P-FOUR-COINS_Concept.png) | clean card conversion anchor | `222ba7304ff81ba983a54211a28a06ead9715a28cc51061c829112ed43909c86` |
| [`CRD-P-FOUR-COINS_ReviewSheet.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Review/CRD-P-FOUR-COINS_ReviewSheet.png) | face, back, scale, overlap, edge, and lighting review | `df3fb7867eb18cf4a87b7043a4128164ba2d82d31d15308d059e87880ee630da` |
| [`V0-PrototypeAssets_Cohesion.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Review/V0-PrototypeAssets_Cohesion.png) | cross-asset artistic-cohesion check | `cc14f7558fc747b16380b577526a4a1e1226496598a102f549d604634a9496a9` |

## Generation provenance

- generation date: 2026-07-20
- operator: Codex for Manuel / Donkijote
- tool: OpenAI built-in image-generation tool
- exposed model/version: not exposed by the built-in tool
- seeds and sampler settings: not exposed by the built-in tool
- input references for the three anchor images: none
- input references for review sheets: only the matching retained project-owned anchor
- input references for the cohesion image: only the three retained project-owned anchors
- observation-only images from the reference board: not supplied to the generator
- direct per-image cost: not exposed; no separate purchase was made

As between the user and OpenAI, the current Europe Terms state that the user owns output to the extent permitted by applicable law. The terms also warn that output may not be unique and still requires suitability review. License basis checked 2026-07-20: [OpenAI Europe Terms of Use](https://openai.com/policies/eu-terms-of-use/).

No Meshy output exists in this package, so no Meshy license is claimed. For the later manual step, record the actual account tier and license shown at generation time. Meshy's current guidance says paid-plan customers own generated assets, while free-plan output uses CC BY 4.0 with attribution; free-plan Meshy 6 downloads are currently restricted. Checked 2026-07-20: [commercial-use guidance](https://help.meshy.ai/en/articles/9992001-can-i-use-my-generated-assets-for-commercial-projects) and [free-plan guidance](https://help.meshy.ai/en/articles/15696428-what-is-included-on-the-free-plan).

## Review results

| Gate | Character | Table | Card |
| --- | --- | --- | --- |
| Project-owned prompt and provenance | Pass | Pass | Pass |
| Brief-required review coverage | Pass for concept stage | Pass for concept stage | Pass for concept stage |
| Shared art direction | Pass | Pass | Pass |
| Cultural-neutrality screen | Pass; no identifying motif | Pass; functional construction only | Pass; original geometric coin language |
| Small-format/readability evidence | silhouette and elevated views retained | thumbnail and overhead card test retained | grayscale and 48/64/96-size comparisons retained |
| Meshy conversion | Deferred | Deferred | Deferred |
| Geometry/material/rig budget | Pending 3D output | Pending 3D output | Pending 3D output |
| Unity import and scene performance | Pending 3D output | Pending 3D output | Pending 3D output |

The cohesion render confirms a stable family: the character keeps the same hair, clothing, clasp, proportions, and palette; the table keeps the segmented rim, quiet center, pedestal, and braces; the visible card keeps four pips, the notched Coins icon, vellum face, and two-way corners.

## Iteration record

The first character anchor placed cards between both hands. It was rejected because the occlusion would confuse future image-to-3D reconstruction. That file remains outside the repository at `<external-working-archive>/issue-8/rejected/CHR-P-WARM-CHALLENGER_v1-rejected.png`, SHA-256 `6405c7c893fca9402b537ff04a9b2f68cab55521f27ff295c1a22361f68d24a9`. A targeted edit removed the cards, opened the pose, and preserved the successful identity and clothing design.

Two attempted combined-character layouts were rejected by the image service before producing output. They created no file and incurred no retained repository artifact. The successful simpler cohesion prompt preserved the three anchors without the rejected request structure.

## Recorded execution prompts

The full positive and negative design prompts remain in the [prototype briefs](prototype-briefs.md). The blocks below record the complete semantic instructions used for the retained anchors and review images, normalized by removing tool-only labels such as `Use case` and `Asset type`.

### Character anchor

```text
Create one original stylized 3D character concept for The Fall, a warm medieval-cartoon card game in a culturally neutral fictional world. Adult human card player called Warm Challenger; approachable and quietly competitive; medium-broad soft-triangular shoulders; slightly enlarged expressive head and hands with natural non-chibi proportions; short-to-medium asymmetrical wavy dark hair; both eyes visible from an elevated camera; complete upper body down to below the ribs; complete shoulders, arms, wrists, and hands. Clothing: moss-wool tunic, walnut sleeveless over-layer, aged-vellum neck cloth, restrained woad-blue binding, dark wrist wraps, one plain antique-brass oval offset collar clasp; broad seams and thick hems. Clean stylized 3D concept render with broad planar forms, matte materials, low-frequency detail. One neutral A-pose character, centered, single three-quarter front view, entire upper body and both hands visible, isolated on a plain light-gray background with generous padding, suitable for image-to-3D conversion. Original project-owned design; culturally neutral; no text or watermark. Avoid named game or artist style, photorealism, chibi, armor, weapons, cape, tall hat, dangling jewelry, text, logo, religious or national symbols, culture collage, stereotype, grimdark, glossy cloth, dense embroidery, cinematic background, hidden hands or eyes, cropped head, extra or fused fingers, and floating bust.
```

Targeted correction applied to the rejected first result:

```text
Remove the playing cards completely and change only the arm and hand pose into a neutral A-pose suitable for image-to-3D character reconstruction. Preserve the exact same fictional character design, face, hair, expression, clothing, clasp, colors, proportions, matte stylized 3D finish, neutral background, lighting, and framing. Show the complete upper body below the ribs. Both arms angle slightly away from the torso and both empty relaxed hands remain fully visible and separated. No tabletop, props, text, or watermark.
```

### Character review sheets

```text
Produce a professional model sheet for the fictional Warm Challenger shown in the retained anchor. On a plain light-gray background, show four evenly spaced upper-body turnaround renders (front, three-quarter, side, back), five small facial-expression studies (neutral, focused, pleased, surprised, disappointed), and one small elevated-camera clay render. Match the anchor's stylized matte 3D rendering, broad planar forms, warm medieval-cartoon design, neutral studio lighting, and restrained palette. Retain the wavy dark hair, approachable adult face, broad shoulder silhouette, moss tunic, walnut sleeveless layer, vellum neck cloth, woad binding, dark wrist wraps, and plain oval brass clasp. Empty relaxed hands remain visible. No labels, props, cards, costume redesign, extra characters, or watermark.
```

```text
Create a supplementary pose, hand, silhouette, and palette sheet for the retained Warm Challenger anchor. Show exactly four lampblack upper-body silhouettes: neutral seated pose, play-card reach, pleased reaction, and disappointed reaction. Show four matching stylized 3D hand studies: open relaxed hand, hand holding one plain blank card, pointing toward a table, and relaxed on a simple table edge. Show plain material blocks for skin, dark hair, moss wool, walnut, aged vellum, woad blue, dark wrist wrap, and antique brass. Match the anchor's proportions, silhouette, sleeve and wrist-wrap shapes, hand scale, matte materials, and culturally neutral art language. No labels, logos, costume changes, extra characters, or cultural symbols.
```

### Table anchor and review

```text
Create one original stylized 3D gameplay table concept for The Fall: a round wooden card table, 1.45 metres diameter and 0.76 metres high, with an uninterrupted 0.95 metre central play field, broad 0.20 metre softly faceted forearm-friendly rim, compact central pedestal, four chunky feet, and restrained matte aged-metal braces. Use warm walnut for rim and base, a quieter charred-walnut center, low-frequency grain, subtle asymmetry, and wear only at the rim. Show one centered three-quarter view from a slightly elevated angle, entire table visible on a plain light-gray background. Preserve a sturdy culturally neutral handmade silhouette and no authored seat positions. Avoid poker felt, casino markings, fixed seats, cultural carving, glossy varnish, dense knots, grunge, fragile construction, text, logos, and cinematic background.
```

```text
Produce a professional model sheet for the retained round table anchor. On a light-gray studio sheet show top orthographic, side orthographic, underside construction, three-quarter neutral-light, elevated fixed-camera clay with plain white cards, an exploded material-ID view, and a small grayscale/mobile-thumbnail check. Keep the same circular silhouette, segmented rim, dark uninterrupted play field, compact central pedestal, four chunky feet, aged-metal braces, broad handmade construction, matte stylized rendering, and low-frequency grain in every panel. No redesign, labels, dimensions, room, characters, chairs, logos, or watermark.
```

### Card anchor and review

```text
Create one original physical Four of Coins card for The Fall at exact 63 by 88 millimetre proportions with subtly visible 0.7 millimetre thickness. Use an aged-vellum face, lampblack keyline, generous border, numeral 4 plus the same original Coins icon in all four corners for two-way reading, and exactly four large symmetric coin pips. The icon uses an antique-brass outer circle, offset inner disk, and four broad notches with restrained woad accents and no currency or real coin design. Present one centered near-front three-quarter card on a plain light-gray background with neutral glare-free studio light. Use crisp vector-like graphics, matte stylized 3D presentation, and low-frequency handmade print texture. No branded-deck resemblance, heraldry, religious or national symbols, poker suits, directional mark, filigree, glossy foil, dirt, extra cards, logo, or watermark.
```

```text
Produce a professional review sheet for the retained Four of Coins anchor. Show a large exact flat front; a large rotationally symmetric back using shallow arches and interlocking circles in lampblack, woad, and vellum; a grayscale front; table-readability views at 48, 64, and 96 pixel-equivalent widths; one three-quarter edge view showing rounded corners and 0.7 mm thickness; one corner-overlap/fan test; and neutral-light versus warm-room-light views. Preserve exactly four central pips, the notched Coins icon, lampblack keyline, vellum face, woad accents, 63:88 proportion, and four two-way corner ranks. Keep one card design across panels. No branded-deck elements, extra rank or suit, directional back, glare, logo, or watermark.
```

### Cross-asset cohesion

```text
Combine the three retained project-owned fictional designs into one clean elevated-camera gameplay concept render. The friendly character sits behind the round wooden table; the Four of Coins and a few matching cards rest on the dark central play field. Keep the character readable around the perimeter and cards unobstructed in the center. Use consistent warm medieval-cartoon stylized 3D art, broad handcrafted forms, matte materials, low-frequency detail, warm overhead key, subtle cool fill, and a walnut/charred-walnut/moss/vellum/woad/lampblack/brass palette. Retain the recognizable visual language, colors, materials, and key design features of all three anchors, including exactly four central pips on the visible Four of Coins. No extra characters, logos, watermark, poker/casino language, glossy surfaces, dense ornament, cultural symbols, or cinematic background.
```

## Manual Meshy handoff for later

When Manuel resumes the external conversion, process one asset at a time and append actual evidence here rather than replacing the concept record:

1. Confirm the Meshy account tier, selected license, current model name/version, and credit balance before generation.
2. Upload only the clean anchor image for the matching ID; do not upload observation-only references or review sheets as hidden style inputs.
3. Keep intermediate and rejected Meshy outputs in the external working archive.
4. Record task URL/ID, settings, credits charged/refunded, generation date, selected result, and rejection rationale.
5. Export the selected source and optimized Unity candidate only after topology, UV, material, scale, pivot, cultural-neutrality, and license review.
6. Promote approved binaries through Git LFS; leave bulk variants, caches, and temporary conversions outside the repository.
7. Complete the geometry/material/rig, Unity import, and representative-scene performance rows before calling any asset an approved V0 prototype.

The permanent external-archive location and backup policy remain an open project decision. `<external-working-archive>` is intentionally a placeholder, not a repository-relative folder.
