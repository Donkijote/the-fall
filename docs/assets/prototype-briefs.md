# V0 Prototype Asset Briefs

Status: Generation-ready V0 briefs

## Purpose

These briefs define three inexpensive comparison assets: one representative upper-body character, one gameplay table, and one card. They are not final cast, environment, or deck approvals. Issue #8 owns generation, conversion, optimization, import, provenance records, and measured results.

All outputs must follow the [art direction](../design/art-direction.md), [reference board](../design/visual-reference-board.md), and [asset strategy](strategy.md). If a generator cannot reliably follow a requirement, record the miss rather than hiding it during cleanup.

## Shared generation package

For each brief, retain:

- asset ID and exact brief revision/commit
- generator/tool and model version where exposed
- complete positive and negative prompts
- seed and generation settings where exposed
- generation date and operator
- every reference input with its provenance and permitted use
- selected output and selection rationale
- rejected outputs outside the repository working tree
- commercial-use terms or license snapshot for the tool and output

The first review is a contact sheet or turntable, not an isolated beauty render. Neutral lighting and clay views are mandatory so presentation polish cannot hide shape problems.

## Brief A — CHR-P-WARM-CHALLENGER

### Role and test purpose

A representative adult player, provisionally called **Warm Challenger**, used to test upper-body silhouette, expression, seated card gestures, camera readability, rigging cost, and skin/hair/cloth separation. The identity is a prototype and does not reserve a Story Mode cast slot.

### Required visual result

- adult human with an approachable but competitive expression
- medium-broad, soft-triangular shoulder silhouette
- slightly enlarged head and hands, natural rather than chibi proportions
- asymmetrical short-to-medium wavy hair that leaves both eyes readable from above
- simple layered clothing: muted teal tunic, toasted-clay sleeveless over-layer, parchment neck cloth, dark wrist wraps
- one signature shape only: a broad offset collar clasp with an invented plain oval form
- no weapons, armor, cape, bag, long necklace, or tall hat
- no letters, runes, heraldry, flags, religious marks, or culturally identifiable textile motifs
- clean lower-torso cutoff below the ribs plus complete shoulders, arms, wrists, and hands

The design should feel handmade and fictional through broad seams, thick hems, and simplified material breaks. Skin tone, facial structure, age details, and hair texture must read as an individual without serving as shorthand for class, morality, faction, or ability.

### Required views and poses

1. neutral front, side, back, and three-quarter turnaround
2. clay render from the expected elevated table camera at neutral seated pose
3. silhouette-only sheet for neutral, play-card reach, pleased reaction, and disappointed reaction
4. expression sheet: neutral, focused, pleased, surprised, disappointed
5. hands open, holding one card, pointing toward the table, and relaxed on table edge
6. color/material swatch sheet under neutral lighting

Keep the body inside a provisional seated envelope 0.75 m wide, 0.55 m deep from chair back toward table, and 0.85 m from seat to head top. Issue #6 may revise this envelope.

### Positive prompt

```text
Create an original stylized 3D character concept sheet for The Fall, a warm medieval-cartoon card game in a culturally neutral fictional world. Adult human card player, approachable and quietly competitive, medium-broad soft-triangular shoulders, slightly enlarged expressive head and hands but natural non-chibi proportions. Short-to-medium asymmetrical wavy dark hair with both eyes visible from an elevated camera. Simple layered clothing with broad clean shapes: muted teal tunic, toasted-clay sleeveless over-layer, parchment neck cloth, dark wrist wraps, one plain oval offset collar clasp. Handmade construction shown through thick hems and a few broad seams, no culture-specific ornament. Strong readable head, shoulder, and hand silhouette from a fixed overhead seated-table camera. Low-frequency details, planar forms, matte materials, warm key and restrained cool fill. Include neutral orthographic turnaround, elevated-camera clay view, five facial expressions, four seated gesture silhouettes, hand studies, and color swatches on a plain background. Original project-owned design, production-neutral concept presentation.
```

### Negative prompt

```text
No named game or artist style, no photorealism, no chibi or bobble head, no child, no pin-up pose, no full armor, weapons, cape, tall hat, giant hair, long dangling jewelry, text, logo, rune, cross, religious symbol, flag, heraldry, national costume, identifiable historical uniform, culture collage, stereotype, grimdark, gore, dirt covering the face, plastic skin, glossy cloth, micro-patterns, dense embroidery, cinematic background, depth-of-field blur, hidden hands, hidden eyes, cropped head, extreme perspective, extra fingers, fused fingers, floating bust.
```

### 3D conversion and Unity targets

- build a complete upper-body mesh with mouth cavity only if the chosen expression path needs it
- LOD0 at or below 25k rendered triangles; LOD1 at or below 12k; LOD2 at or below 5k
- one skinned renderer preferred and two materials maximum
- one 1024 px prototype texture set; pack masks where practical
- at most 55 deform bones and four weights per vertex
- include head/neck, clavicles, shoulders, upper/lower arms, wrists, hands, and simple finger groups
- topology must support shoulder reach, wrist rotation, brow/mouth expressions, and a 30-degree head turn without obvious collapse
- pivot/rig root centered on the seated body midline at seat height; one Unity unit equals one metre

### Accept / revise / reject

**Accept for issue #8 intake** when the silhouette passes every required pose, eyes and hands read from the elevated camera, all required views exist, the design contains no identifying real-world motif, and the conversion appears feasible inside the V0 envelope.

**Revise** when the core silhouette works but one removable detail, palette value, hand shape, expression, or garment layer harms readability; request the smallest targeted change and preserve the successful seed/settings.

**Reject** when the design is derivative of a named property, culturally specific or stereotyped, photoreal/chibi, dependent on dense surface detail, missing usable hands/eyes, anatomically broken, or structurally unlikely to rig within budget. Do not repair a rejected direction through unbounded manual sculpting.

## Brief B — ENV-P-ROUND-TABLE

### Role and test purpose

A representative round gameplay table used to test fixed-camera composition for 1v1, three-player, and 2v2; card/material contrast; seat flexibility; collision anchors; and inexpensive static-asset import. Dimensions are provisional until issue #6 validates the layouts.

### Required visual result

- circular table, 1.45 m diameter and 0.76 m high
- central uninterrupted play field approximately 0.95 m diameter
- 0.18–0.22 m broad outer rim with softly faceted, hand-shaped profile
- warm smoked-oak base with low-frequency grain; central field one value step darker and less detailed
- four chunky supports or one central pedestal arranged so no mode looks assigned to fixed seats
- small matte aged-metal braces as secondary detail, no spikes or ornate iron scrollwork
- rounded edge safe for forearm poses; no cup holders, drawers, gambling markings, words, suits, or scoring tracks
- subtle asymmetry and wear at the rim only; clean enough for card contrast

### Required views

1. top orthographic view with 1.45 m outer and 0.95 m play-field dimensions
2. side and bottom construction views
3. three-quarter neutral-light beauty view
4. clay render from the fixed elevated camera with 40 plain white card rectangles distributed across the centre and hands
5. grayscale and high-contrast mobile thumbnail views
6. exploded material-ID view for wood and metal

### Positive prompt

```text
Create an original stylized 3D gameplay table concept and model sheet for The Fall, a warm medieval-cartoon card game in a culturally neutral fictional world. Round wooden card table, 1.45 metres diameter, 0.76 metres high, uninterrupted 0.95 metre central play field, broad softly faceted rim with rounded forearm-friendly edge. Smoked warm oak, simple low-frequency grain, central field slightly darker and quieter than the rim, four chunky supports or a compact central pedestal, a few matte aged-metal braces. Handmade but sturdy, broad exaggerated construction, restrained asymmetry, wear only at plausible hand-contact edges. No built-in seat positions so two, three, or four players fit equally. Strong clean silhouette from a fixed elevated camera, matte URP-friendly materials, mobile-conscious geometry. Include dimensioned top/side/bottom views, three-quarter view, clay overhead card-readability test with plain cards, grayscale thumbnail, and material-ID view on a plain background.
```

### Negative prompt

```text
No named game or artist style, no rectangular coffee table, no poker felt, casino markings, cup holders, drawers, text, logo, suit symbols, score track, throne-like chairs, Gothic spikes, heraldry, religious symbols, national motifs, culture-specific carving, weapons, skulls, candles attached to the play surface, food clutter, glossy varnish, mirror reflections, dense knots, noisy scratches, procedural grunge everywhere, photorealism, thin fragile legs, modern industrial furniture, impossible joinery, cinematic room background.
```

### 3D conversion and Unity targets

- LOD0 at or below 12k triangles; LOD1 at or below 6k
- no more than two materials; use one 1024 px texture set by default, with a 2048 px test allowed only if the overhead comparison proves a visible gain
- keep the central field UV density even; orient grain consistently with construction rather than radially stretching one texture
- separate optional metal braces only if material reuse or authoring needs it; avoid many tiny renderers
- simple primitive colliders for top and supports; no mesh collider required for the prototype
- pivot at floor level on the table centre; local up is `+Y`; one Unity unit equals one metre
- static mesh, no rig; lightmap UV candidate without overlaps if baked lighting is tested

### Accept / revise / reject

**Accept for issue #8 intake** when the full play field is readable, all seating modes can use the rim without authored seat bias, plain cards remain the brightest high-contrast objects, scale/views are complete, and construction can meet the static budget.

**Revise** when the overall round form works but the grain frequency, value, rim width, support placement, or metal prominence interferes; revise materials or one construction layer without replacing the concept.

**Reject** when the design encodes a fixed player count, obscures or clips the play area, resembles casino/poker furniture, directly reconstructs a culture-specific object, depends on dense carving, or exceeds the budget because of non-functional detail.

## Brief C — CRD-P-FOUR-COINS

### Role and test purpose

An original **Four of Coins** card used to test Spanish-suit recognition, rank readability, card proportions, table contrast, border behavior, orientation independence, and a shared-material implementation. It is not approval of the full deck's final illustration system.

### Required visual result

- physical dimensions 63 x 88 mm; prototype thickness 0.7 mm for visible 3D handling
- warm parchment face with a clean, darker outer keyline and generous safe border
- numeral `4` and a project-owned Coins icon in every corner, readable from either end
- four large original coin pips in a simple symmetric field arrangement
- coin icon built from a bold outer circle, offset inner disk, and four broad notches; no face, currency, lettering, crest, or real coin design
- charcoal linework, old-gold coins, and one muted teal micro-accent that does not carry suit identity by itself
- lightly imperfect printed registration and paper grain visible only in close inspection
- original geometric card back with rotational symmetry, using charcoal plum, muted teal, and parchment; no heraldry or hidden directional mark

The rules own the rank and suit identity; the art must not introduce an alternate value or a text label. The corners and central pips must agree.

### Required views

1. flat front and back at exact aspect ratio
2. grayscale and color-blind simulation contact sheet
3. card shown at 48, 64, and 96 pixels wide on the smoked-oak table
4. 3D neutral view showing edge, corner radius, thickness, and pivot
5. fan/overlap test with only the upper-left and lower-right corners exposed
6. neutral-light and warm-room-light comparison to detect color shift and glare

### Positive prompt

```text
Create an original vector-like card-face and card-back design for The Fall, a warm medieval-cartoon card game using a 40-card Spanish deck. Representative card: Four of Coins. Exact 63 by 88 millimetre proportion. Warm parchment face, generous clean border, dark charcoal keyline, number 4 plus a bold original Coins symbol in all four corners for two-way reading. Four large symmetric coin pips: simple old-gold outer circles, offset inner disks, four broad notches, no currency or portrait. Restrained hand-printed imperfection, broad crisp shapes, minimal low-frequency paper texture, excellent readability at 48 pixels wide. Card back is rotationally symmetric with original shallow arches and interlocking circles in charcoal plum, muted teal, and parchment; no culture-specific ornament. Provide flat front/back, grayscale, small-size table tests, overlap/fan test, and neutral-versus-warm lighting comparison on a plain presentation sheet. Original project-owned graphic design, not an imitation of a branded Spanish deck.
```

### Negative prompt

```text
No named game, artist, deck, or manufacturer style, no Fournier illustration, no copied Spanish-card border, no realistic currency, face, king, coat of arms, flag, cross, religious symbol, national emblem, letters, suit-name text, logo, watermark, casino design, poker suit, Roman numeral, asymmetric back, directional back mark, tiny filigree, dense engraving, distressed unreadable print, torn edge, dirty face, glossy foil, hologram, photoreal paper, fake mockup perspective only, mismatched rank and pip count.
```

### 3D conversion and Unity targets

- at or below 100 triangles including rounded corners and a minimal bevel/thickness
- pivot at geometric centre; local face normal and axis convention documented by issue #8
- one shared material path; no unique shader instance for this rank
- 512 x 1024 px working face during the single-card prototype; preserve vector/source shapes for later atlas decisions
- opaque face/back wherever possible; avoid alpha-cut card silhouettes and transparent edge effects
- mipmaps enabled for world rendering; test compression and anisotropic filtering against corner-symbol legibility
- collider no more complex than a thin box; physical thickness may be exaggerated in presentation without changing rule state

### Accept / revise / reject

**Accept for issue #8 intake** when rank and suit are correctly recognized at 48 px width, both exposed-corner tests work, grayscale still separates markings from face, the back has no orientation tell, and every mark is original.

**Revise** when the identity is correct but corner scale, border weight, coin spacing, paper noise, or warm-light color shift weakens a specific test; adjust the smallest graphic layer and rerun all sizes.

**Reject** when the card resembles a branded deck, contains a cultural/national symbol, uses the wrong pip count, relies on color alone, becomes unreadable under overlap, has a directional back, or needs unique high-cost materials to read.

## Review record

Issue #8 should copy this table into each retained asset record:

| Gate | Result | Evidence | Action |
| --- | --- | --- | --- |
| Provenance and license complete | Pending | links/snapshots | accept, revise record, or reject |
| Brief-required views complete | Pending | contact sheet/turntable | accept, regenerate missing views, or reject |
| Art-direction match | Pending | silhouette/value/palette review | accept, targeted revision, or reject |
| Cultural-neutrality check | Pending | motif and stereotype review | accept, remove motif, or reject |
| Mobile readability checks | Pending | specified pixel-size screenshots | accept, targeted revision, or reject |
| Geometry/material budget | Pending | Unity stats/import report | accept, optimize, or reject |
| Rig/animation readiness | Pending or N/A | deformation/pose tests | accept, revise topology, or reject |
| Representative scene performance | Pending | profiler/device evidence | prototype only, revise, or reject |

An **accepted prototype** is fit for experiments, not automatically a production candidate. Production review additionally requires cross-asset consistency, final topology/UV/material quality, complete licensing, accessibility and localization validation where relevant, representative-device profiling, and explicit art approval.
