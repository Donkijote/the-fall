# Asset Strategy

Status: Draft

## Confirmed strategy

Prioritize free and generated prototype assets until the art direction is proven. Do not migrate assets from the previous TypeScript project by default.

The planned concept-to-3D workflow is:

1. Define an asset brief and gameplay constraints.
2. Generate or iterate 2D visual concepts with ChatGPT.
3. Review silhouette, function, camera readability, and consistency.
4. Convert approved concepts into prototype 3D assets with Meshy AI.
5. Review topology, UVs, materials, scale, pivots, rigging needs, and license evidence.
6. Optimize and import approved prototypes into Unity.
7. Record source, generation settings, license, version, and intended usage.

The [V0 visual reference board](../design/visual-reference-board.md) defines what may be observed or reused, and the [prototype asset briefs](prototype-briefs.md) provide the generation-ready inputs for the representative character, table, and card. Issue #8 owns executing and measuring this pipeline; a brief alone is not an approved asset record.

The [generated concept package](generated-concept-package.md) records the retained visual anchors and consistency sheets created on 2026-07-20. The [generated 3D intake](generated-3d-intake.md) executes and measures the Meshy-to-Unity path for the representative table. Character conversion remains pending manual owner work; the table is a V0 prototype with a documented 12K-target exception, not production art.

Cards follow a separate confirmed 2D production path. Unity composes project-owned rank, suit, base, back, and court-art components into the complete forty-card face atlas at editor time. The full concept-card renders remain art-direction references rather than per-card production textures. See the [modular card visual pipeline](card-visual-pipeline.md) and [ADR 0001](../decisions/0001-modular-card-visual-pipeline.md).

## Working budget

**Confirmed:** The current monthly tool budget is approximately EUR 40:

- approximately EUR 20 for Meshy AI
- approximately EUR 20 for ChatGPT/Codex tooling

Avoid additional paid asset commitments until a need and budget are explicitly approved.

## Asset categories

- Spanish deck cards and card backs
- table and seating
- upper-body characters and clothing variations
- environment architecture and props
- UI icons, frames, typography, and localization-safe layouts
- gameplay VFX
- character and card animations
- music, ambience, and sound effects

## Prototype versus production

- Prototype assets prove composition, interaction, performance, and style.
- Prototype approval requires the brief's silhouette/readability tests, provenance, cultural-neutrality review, and provisional technical envelope; it permits use in V0 experiments only.
- Production approval requires visual consistency, technical quality, clear licensing, and target-device validation.
- Generated does not mean automatically usable; every generated asset requires review.

The provisional V0 geometry, material, texture, rig, and Unity import targets live in [art direction](../design/art-direction.md#v0-prototype-technical-envelope). They are conservative comparison budgets, not measured production limits. Replace them only with evidence from the intake and platform-validation issues.

## Asset record requirements

Each accepted external or generated asset should record:

- asset identifier and owner
- source and creation date
- source prompt or brief when applicable
- license and commercial-use evidence
- original and processed file locations
- intended use
- Unity import settings
- optimization or rigging work performed
- replacement status: prototype or production candidate

## Repository storage policy

**Confirmed:** Use a hybrid source-storage policy.

Commit these to the repository:

- text prompts, briefs, provenance, license records, and generation notes as normal Git text files
- approved Unity-ready models, textures, audio, fonts, and other binary project dependencies through Git LFS
- selected source files required to reproduce or materially revise an accepted project asset through Git LFS

Keep these outside the repository in a separate working archive:

- rejected or superseded image generations
- bulk Meshy experiments and downloads
- temporary exports, caches, and conversion intermediates
- source variations that are not needed to reproduce an accepted project asset

Promote an item from the working archive into Git only after review gives it a project purpose and a complete asset record.

The repository already contains a Unity-oriented `.gitattributes` file and Git LFS configuration for common model, image, audio, video, font, archive, and binary formats. Keep that configuration under version control and review it during the bootstrap cleanup.

### LFS budget guardrail

GitHub Free and Pro currently include 10 GiB of Git LFS storage and 10 GiB of monthly download bandwidth. A changed binary version consumes storage again at its full size. Keep the GitHub LFS overage budget at zero unless Manuel explicitly approves additional spending, and review LFS usage before promoting large asset batches.

## Open decisions

- whether Blender is part of the cleanup pipeline
- measured production polygon, texture, material, and bone budgets by device tier
- preferred interchange formats
- location and backup policy for the external working archive
