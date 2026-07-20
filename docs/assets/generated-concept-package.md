# V0 Generated Concept Package

Status: Concept-generation checkpoint; table conversion completed in the generated 3D intake

## Scope boundary

This package records the image-generation portion of issue #8 for the three representative V0 assets and their supporting visual family: an optional full-body character reference, a matching chair, and Four cards for all four Spanish suits. The concept folders deliberately stop before 3D output. Manuel performs Meshy actions manually; the approved table download has now completed the repository and Unity stages recorded in the [generated 3D intake](generated-3d-intake.md).

These images are approved as **visual anchors for consistent follow-up generation**. They are not approved 3D prototype assets and do not satisfy issue #8's end-to-end Meshy, Unity import, or performance gates.

The retained complete-card renders are also references rather than production face textures. The accepted [modular card visual pipeline](card-visual-pipeline.md) recreates the shared design language as replaceable rank, suit, base, back, and court components, then lets Unity generate the complete forty-card atlas. Card production does not require Meshy.

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
| [`CHR-P-WARM-CHALLENGER_Concept.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Characters/CHR-P-WARM-CHALLENGER_Concept.png) | clean upper-body character conversion anchor | `e0e309edbefd5625726e8f2b42a844d03646b2ac073ad415a98a00e879c43a47` |
| [`CHR-P-WARM-CHALLENGER_FullBody.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Characters/CHR-P-WARM-CHALLENGER_FullBody.png) | optional head-to-toe character design reference | `35f25b052bae47704470831579cc07fc56f233019a4cf2950529576fc57cd307` |
| [`CHR-P-WARM-CHALLENGER_ReviewSheet.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Characters/CHR-P-WARM-CHALLENGER_ReviewSheet.png) | turnaround, expressions, and elevated clay review | `ade41c3bb96e559b1f198c00858f5424c9c1dc106cd9eb58ecd646dfc1543e19` |
| [`CHR-P-WARM-CHALLENGER_GestureSheet.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Characters/CHR-P-WARM-CHALLENGER_GestureSheet.png) | silhouettes, hand studies, and material swatches | `12e14148cd0610f38ff3c018f956d121c5231eda57d77017519c67dc236a24e8` |
| [`ENV-P-ROUND-TABLE_Concept.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Furniture/ENV-P-ROUND-TABLE_Concept.png) | clean table conversion anchor | `fa960584c2d104d0b9c1873a9c2dba8b33b768461a59e033455a9e55cbcac5dd` |
| [`ENV-P-ROUND-TABLE_ReviewSheet.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Furniture/ENV-P-ROUND-TABLE_ReviewSheet.png) | orthographic, underside, card-readability, and exploded review | `30c92767624ca2c3154c4e9ed8364b02d430f8a1a212ab2f84f1fa90d954995a` |
| [`ENV-P-SIMPLE-CHAIR_Concept.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Furniture/ENV-P-SIMPLE-CHAIR_Concept.png) | matching armless chair design reference | `c0b29345c537777a04d99883429903624948f0d71fb62b0b5696453a4ebc2359` |
| [`CRD-P-FOUR-COINS_Concept.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Cards/CRD-P-FOUR-COINS_Concept.png) | clean Coins card conversion anchor | `222ba7304ff81ba983a54211a28a06ead9715a28cc51061c829112ed43909c86` |
| [`CRD-P-FOUR-COINS_ReviewSheet.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Cards/CRD-P-FOUR-COINS_ReviewSheet.png) | face, back, scale, overlap, edge, and lighting review | `df3fb7867eb18cf4a87b7043a4128164ba2d82d31d15308d059e87880ee630da` |
| [`CRD-P-FOUR-CLUBS_Concept.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Cards/CRD-P-FOUR-CLUBS_Concept.png) | matching Clubs suit reference using wooden bastones | `9da6d8c9df0745de996e9a2f6e9dbed5e94610182b71aec2ae66c2de35f690a4` |
| [`CRD-P-FOUR-SWORDS_Concept.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Cards/CRD-P-FOUR-SWORDS_Concept.png) | matching Swords suit reference using the Clubs corner template | `e4d9c2ff85cd4e25e478b576279f33297c8226243ead0b71868a65afdc6420d7` |
| [`CRD-P-FOUR-CUPS_Concept.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Cards/CRD-P-FOUR-CUPS_Concept.png) | matching Cups suit reference | `0ca351d1dd2489944fac73b231e1b5c7093c7a69d6df76b4f7c885968aaa15f6` |
| [`V0-PrototypeAssets_Cohesion.png`](../../Assets/TheFall/Content/PrototypeAssets/Concepts/Cohesion/V0-PrototypeAssets_Cohesion.png) | cross-asset artistic-cohesion check | `cc14f7558fc747b16380b577526a4a1e1226496598a102f549d604634a9496a9` |

## Generation provenance

- generation date: 2026-07-20
- operator: Codex for Manuel / Donkijote
- tool: OpenAI built-in image-generation tool
- exposed model/version: not exposed by the built-in tool
- seeds and sampler settings: not exposed by the built-in tool
- input references for the three original anchor images: none
- input references for review sheets: only the matching retained project-owned anchor
- input references for the cohesion image: only the three retained project-owned anchors
- input reference for the full-body character: only the retained project-owned character anchor
- input reference for the chair: only the retained project-owned table anchor
- input reference for the original Swords and Cups extensions: only the retained project-owned Four of Coins anchor
- input references for the retained Swords rebuild: the retained Clubs card as the authoritative card-layout, corner-frame, and spacing edit target; the previous project-owned Swords card supplied only the sword artwork, materials, colors, and central arrangement
- input references for the retained Clubs rebuild: only the approved project-owned Swords and Cups sibling cards as locked layout, material, camera, rendering, typography, and border references; no Coins image or rejected Clubs image was supplied
- observation-only images from the reference board: not supplied to the generator
- direct per-image cost: not exposed; no separate purchase was made

As between the user and OpenAI, the current Europe Terms state that the user owns output to the extent permitted by applicable law. The terms also warn that output may not be unique and still requires suitability review. License basis checked 2026-07-20: [OpenAI Europe Terms of Use](https://openai.com/policies/eu-terms-of-use/).

No Meshy output is stored in the concept folders, so this concept package claims no Meshy license. The selected table FBX/PBR files, owner-reported Private License, costs, hashes, and Unity results are recorded separately in the [generated 3D intake](generated-3d-intake.md). For later manual character work, record the actual account tier and license shown at generation time. Meshy's current guidance says paid-plan customers own generated assets, while free-plan output uses CC BY 4.0 with attribution; free-plan Meshy 6 downloads are currently restricted. Checked 2026-07-20: [commercial-use guidance](https://help.meshy.ai/en/articles/9992001-can-i-use-my-generated-assets-for-commercial-projects) and [free-plan guidance](https://help.meshy.ai/en/articles/15696428-what-is-included-on-the-free-plan).

## Review results

| Gate | Character family | Furniture family | Card family |
| --- | --- | --- | --- |
| Project-owned prompt and provenance | Pass | Pass | Pass |
| Brief-required review coverage | Pass for required upper-body concept stage; full body is supporting reference | Pass for required table concept stage; chair is supporting reference | Pass for required Coins concept stage; other suits are supporting references |
| Shared art direction | Pass | Pass | Pass |
| Cultural-neutrality screen | Pass; no identifying motif | Pass; functional construction only | Pass; original geometric suit language |
| Small-format/readability evidence | silhouette and elevated views retained | thumbnail and overhead card test retained | Coins grayscale and 48/64/96-size comparisons retained; four-suit differentiation visually reviewed |
| Meshy conversion | Deferred for owner work | Pass for selected Smart Topology table | N/A; modular 2D path accepted |
| Geometry/material/rig budget | Pending 3D output | V0 exception: 13,253 triangles versus provisional 12K; one material; static/no rig | Pass through modular card pipeline |
| Unity import and scene performance | Pending 3D output | Prototype pass; details in generated 3D intake | Pass through modular card pipeline |

The cohesion render and extensions confirm a stable family: the full-body character preserves the upper-body identity while adding simple trousers, wraps, and rounded boots; the chair reuses the table's walnut, charred-walnut, brass, faceting, and joinery language; all Four cards preserve the same vellum face, border, corner-rank system, proportions, and two-way four-pip layout while remaining distinct by silhouette.

## Iteration record

The first character anchor placed cards between both hands. It was rejected because the occlusion would confuse future image-to-3D reconstruction. That file remains outside the repository at `<external-working-archive>/issue-8/rejected/CHR-P-WARM-CHALLENGER_v1-rejected.png`, SHA-256 `6405c7c893fca9402b537ff04a9b2f68cab55521f27ff295c1a22361f68d24a9`. A targeted edit removed the cards, opened the pose, and preserved the successful identity and clothing design.

Two attempted combined-character layouts were rejected by the image service before producing output. They created no file and incurred no retained repository artifact. The successful simpler cohesion prompt preserved the three anchors without the rejected request structure.

Two Clubs extensions resembled the Coins family too closely and were rejected. They remain only in the external working archive as `<external-working-archive>/issue-8/rejected/CRD-P-FOUR-CLUBS_v1-rejected.png`, SHA-256 `07ab5417792e86f90631b35fa62e525bb24c873c3911b7d13f333321ebba7d2c`, and `<external-working-archive>/issue-8/rejected/CRD-P-FOUR-CLUBS_v2-rejected.png`, SHA-256 `3f9745c8d7b1cb737fc8d1fa9f1dcb5210f69ffad3d044144db40ba93fec00b9`. The retained design was rebuilt from scratch around elongated wooden Spanish-suit bastones, using only the approved Swords and Cups cards as presentation references and explicitly excluding lobes, disks, medallions, and other coin-derived construction. One initial full-body request was rejected by the image service before producing output; the simplified project-owned character-extension request succeeded without changing the character identity.

The first Swords card clipped each small corner sword against the curved inner border. Although the central design was approved, the clipped icons were inconsistent with Coins, Cups, and Clubs and weakened overlap readability. It remains only in the external working archive as `<external-working-archive>/issue-8/rejected/CRD-P-FOUR-SWORDS_v1-corner-clipped.png`, SHA-256 `fdf6fc969d31fb19306e356dfc69247674f3a8bf7fc2e1cf9b2e1acad7495126`. The next four rejected corrections are also external: `CRD-P-FOUR-SWORDS_v2-complete-oversized.png`, SHA-256 `62febe494cb53bd12e0268f0ba0a2a42325f0c57939ae94ce9b97a74bf1f92bf`, made the complete icons too large; `CRD-P-FOUR-SWORDS_v3-small-icons-oversized-frames.png`, SHA-256 `28e8b6f669c0baf5cdf18401f9d2cdcb92a510c94d596952cd927200b673606a`, fixed icon scale but left overlong corner frames; `CRD-P-FOUR-SWORDS_v4-shorter-frames-loose-spacing.png`, SHA-256 `5757e1872baf4f8a4107ce08dc1cdec3547164c76f335c7940a7345bdf6498a8`, shortened frames but left excessive vellum around the icons; and `CRD-P-FOUR-SWORDS_v5-tight-spacing-template-drift.png`, SHA-256 `99e24541379670059aa67b32c2d75d5cab0b0b49cb7210288b6fffbd82523df4`, tightened spacing but changed the approved card perspective and border weight. The retained Swords card instead uses Clubs as the authoritative card/layout target and replaces only its suit artwork, which locks the same corner-frame geometry and tight `4 → icon → border` rhythm by construction.

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

### Full-body character extension

```text
Using only the retained project-owned Warm Challenger anchor as the identity and style reference, create one complete head-to-toe version of the same fictional character. Preserve the exact wavy dark hair, approachable face, broad shoulders, moss tunic, walnut sleeveless layer, aged-vellum neck cloth, woad-blue binding and sash, dark wrist wraps, broad seams, thick hems, and plain oval antique-brass clasp. Extend the design with simple dark-walnut trousers, readable knee articulation, restrained ankle wraps, and low rounded handmade boots. Show one centered three-quarter neutral A-pose with both empty hands, both legs, and both feet separated and fully visible on a plain light-gray studio background. Match the original broad planar forms, matte stylized 3D finish, low-frequency detail, warm medieval-cartoon proportions, and culturally neutral fictional design. No costume redesign, armor, weapons, cape, skirt, dangling accessories, cultural or religious motifs, text, watermark, cropped feet, extreme pose, or extra props.
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

### Matching chair extension

```text
Using only the retained project-owned round table as the material, construction, and rendering reference, create one original simple armless chair for the same fictional card-game room. Seat height approximately 0.46 metres for the 0.76-metre table; low-to-medium shallow-arched back below the seated character's shoulders; broad seat; chunky tapered legs and simple stretchers. Reuse warm walnut, quieter charred-walnut seat and back panels, restrained matte antique-brass braces, softly faceted edges, sturdy handmade joinery, broad planar forms, low-frequency grain, and subtle asymmetry. Present one centered three-quarter view with the entire chair visible on a plain light-gray studio background in the same matte stylized 3D medieval-cartoon finish. No throne silhouette, arms, upholstery, carving, heraldry, cultural or religious motif, spikes, fragile legs, glossy varnish, room, character, text, logo, or watermark.
```

### Card anchor and review

```text
Create one original physical Four of Coins card for The Fall at exact 63 by 88 millimetre proportions with subtly visible 0.7 millimetre thickness. Use an aged-vellum face, lampblack keyline, generous border, numeral 4 plus the same original Coins icon in all four corners for two-way reading, and exactly four large symmetric coin pips. The icon uses an antique-brass outer circle, offset inner disk, and four broad notches with restrained woad accents and no currency or real coin design. Present one centered near-front three-quarter card on a plain light-gray background with neutral glare-free studio light. Use crisp vector-like graphics, matte stylized 3D presentation, and low-frequency handmade print texture. No branded-deck resemblance, heraldry, religious or national symbols, poker suits, directional mark, filigree, glossy foil, dirt, extra cards, logo, or watermark.
```

```text
Produce a professional review sheet for the retained Four of Coins anchor. Show a large exact flat front; a large rotationally symmetric back using shallow arches and interlocking circles in lampblack, woad, and vellum; a grayscale front; table-readability views at 48, 64, and 96 pixel-equivalent widths; one three-quarter edge view showing rounded corners and 0.7 mm thickness; one corner-overlap/fan test; and neutral-light versus warm-room-light views. Preserve exactly four central pips, the notched Coins icon, lampblack keyline, vellum face, woad accents, 63:88 proportion, and four two-way corner ranks. Keep one card design across panels. No branded-deck elements, extra rank or suit, directional back, glare, logo, or watermark.
```

### Four-suit card extensions

Cups uses the Four of Coins as its locked card-template reference. After two coin-derived Clubs iterations were rejected, Clubs was rebuilt from scratch with approved sibling cards serving only as presentation references. The final Swords card uses Clubs as its authoritative card-layout target so their corner frames and spacing match exactly.

```text
Create a brand-new Four of Clubs card using unmistakable wooden clubs or cudgels in the Spanish bastos tradition, not a modification of the Coins icon. Use the approved project-owned Swords and Cups cards only as locked card-template, material, camera, rendering, typography, border, and layout references. Show exactly four large elongated wooden cudgels in the central field, arranged symmetrically with two upright in the upper half and two inverted in the lower half. Each club is one solid irregular length of hand-carved dark walnut with a narrow wrapped grip, thick tapered shaft, and heavier blunt knotted striking end; use restrained moss-green grip wraps and one simple antique-brass collar as accents. Add a matching small wooden-club icon beside each numeral 4 for two-way reading. Preserve the exact 63:88 proportions, rounded corners, visible thickness, generous vellum field, lampblack-brown keylines, matte vector-like graphics, low-frequency handmade texture, neutral studio light, and near-front three-quarter presentation of the approved sibling cards. Exactly four central pips, all complete and separated. No coin-derived design, French trefoil, clover, flower, three-lobed or circular head, disk, ring, medallion, inner coin inset, mace, axe, sword, cup, heraldry, religious or national motif, dense carving, glossy wood, realistic violence, extra pips, mismatched rank, text beyond numeral 4, logo, or watermark.
```

```text
Four of Swords: preserve the retained Four of Coins card's exact 63:88 proportions, vellum face, lampblack keyline, generous border, corner-number placement, two-way reading, matte vector-like 3D presentation, and exactly four central pips arranged two upright and two inverted. Replace every Coins symbol with one original culturally neutral Sword symbol: a straight broad blade with softly rounded tip, compact plain guard, restrained woad-blue grip, and antique-brass pommel, using muted iron and lampblack outlines. Keep four matching corner suit icons and exactly four large central sword pips. No cross or religious reading, historical insignia, heraldry, named weapon design, extra pips, mismatched rank, text beyond numeral 4, logo, or watermark.
```

Targeted corner-symbol correction applied to the approved central Swords design:

```text
Change only the four small sword suit icons beneath or beside the corner numeral 4s. Scale and reposition each small corner sword so the complete blade tip, blade, guard, woad-blue wrapped grip, and round antique-brass pommel are visible inside its corner compartment with clear vellum padding on every side. Keep the two upper swords upright beneath their 4s and the two lower swords inverted above their rotated 4s. Do not let any part touch, cross, hide behind, or clip against the curved inner border. Preserve the four large central swords, their positions and sizes, all four numeral 4s, every border line and corner-compartment shape, aged-vellum texture, card proportions and thickness, perspective, crop, studio background, colors, lighting, shadows, and matte stylized 3D finish. Exactly four central swords and four complete corner sword icons; no redesign, extra symbol, text, logo, or watermark.
```

Intermediate scale correction using the retained Clubs card only as a corner-envelope reference:

```text
Change only the four small sword icons in the Swords card's corner compartments. Uniformly reduce each complete corner sword to approximately 80 percent of its current height so its visual bounding box, surrounding vellum space, center alignment, and optical weight match the corresponding small club icon. Preserve the sword's natural tall, narrow proportions without stretching it or changing the blade-to-handle ratio. Keep the two upper swords upright and centered beneath their numeral 4s, and the two lower swords inverted and centered above their rotated numeral 4s. Preserve the four large central swords, all four numeral 4s, every border and compartment shape, vellum texture, card proportions, thickness, perspective, crop, studio background, materials, colors, lighting, shadows, and matte stylized 3D finish. Exactly four central swords and four small complete corner swords; no clipping, oversized icons, changed central field, new ornament, extra symbol, text, logo, or watermark.
```

Final retained Swords rebuild:

```text
Starting from the retained Four of Clubs as the authoritative edit target and locked card-layout template, replace only its eight wooden club symbols with sword symbols: four large central pips and four small corner suit icons. Preserve the Clubs card's aspect ratio, camera, crop, silhouette, thickness, rounded corners, outer border, all four curved inner corner compartments and their exact dimensions and arcs, all numeral 4 glyphs and positions, vellum texture, empty-space distribution, corner-stack spacing, lighting, shadow, background, and matte stylized 3D presentation. Use the previous project-owned Swords card only for the sword artwork: iron blade, lampblack outline, plain antique-brass guard and pommel, and woad-blue wrapped grip. Arrange exactly two complete large swords upright above and two inverted below. Replace each small club with one complete small sword occupying the exact same visual bounding box, center point, and vertical envelope as the club it replaces; preserve the tight gap from numeral 4 to icon and from icon to curved border. Upper swords upright and lower swords inverted. Exactly four large central swords and four complete small corner swords. No retained club, changed border, extended compartment, excessive empty space, oversized or clipped sword, changed number, changed perspective, new ornament, extra symbol, text, logo, or watermark.
```

```text
Four of Cups: preserve the retained Four of Coins card's exact 63:88 proportions, vellum face, lampblack keyline, generous border, corner-number placement, two-way reading, matte vector-like 3D presentation, and exactly four central pips arranged two upright and two inverted. Replace every Coins symbol with one original culturally neutral Cup symbol: a wide shallow bowl, narrow stem, broad stable foot, and tiny functional side grips, using antique brass, restrained madder accents, and lampblack outlines. Keep four matching corner suit icons and exactly four large central cup pips. No religious chalice, ceremonial vessel, trophy, heraldry, cultural ornament, extra pips, mismatched rank, text beyond numeral 4, logo, or watermark.
```

### Cross-asset cohesion

```text
Combine the three retained project-owned fictional designs into one clean elevated-camera gameplay concept render. The friendly character sits behind the round wooden table; the Four of Coins and a few matching cards rest on the dark central play field. Keep the character readable around the perimeter and cards unobstructed in the center. Use consistent warm medieval-cartoon stylized 3D art, broad handcrafted forms, matte materials, low-frequency detail, warm overhead key, subtle cool fill, and a walnut/charred-walnut/moss/vellum/woad/lampblack/brass palette. Retain the recognizable visual language, colors, materials, and key design features of all three anchors, including exactly four central pips on the visible Four of Coins. No extra characters, logos, watermark, poker/casino language, glossy surfaces, dense ornament, cultural symbols, or cinematic background.
```

## Manual Meshy handoff for remaining assets

When Manuel resumes the remaining external conversion, process one asset at a time and append actual evidence to the generated 3D intake rather than replacing the concept record. The table has completed these steps; the character remains pending:

1. Confirm the Meshy account tier, selected license, current model name/version, and credit balance before generation.
2. Upload only the clean anchor image for the matching ID; do not upload observation-only references or review sheets as hidden style inputs.
3. Keep intermediate and rejected Meshy outputs in the external working archive.
4. Record task URL/ID, settings, credits charged/refunded, generation date, selected result, and rejection rationale.
5. Export the selected source and optimized Unity candidate only after topology, UV, material, scale, pivot, cultural-neutrality, and license review.
6. Promote approved binaries through Git LFS; leave bulk variants, caches, and temporary conversions outside the repository.
7. Complete the geometry/material/rig, Unity import, and representative-scene performance rows before calling any asset an approved V0 prototype.

The permanent external-archive location and backup policy remain an open project decision. `<external-working-archive>` is intentionally a placeholder, not a repository-relative folder.
