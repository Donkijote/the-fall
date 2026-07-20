# Generated 3D Asset Intake

Status: Table path executed; character conversion remains owner-operated and pending

## Purpose

This record turns the [asset strategy](strategy.md) and [prototype briefs](prototype-briefs.md) into a repeatable concept-to-Unity intake. It records the first retained Meshy asset, its rejected alternatives, Unity settings, validation, and known exceptions. It does not promote the table to final production art.

The retained 2D anchors and prompts remain in the [generated concept package](generated-concept-package.md). Cards use the separate [modular card visual pipeline](card-visual-pipeline.md), so no full-card Meshy conversion is required.

## Repeatable intake procedure

Use this sequence for every generated 3D asset:

1. Start from an approved project-owned concept and its prototype brief. Record the tool, model/topology mode, date, task identifier if exposed, settings, credits, account tier, and license shown by the service.
2. Keep retries, rejected generations, remesh experiments, original download archives, and temporary inspection projects outside the repository.
3. Inspect an archive before extraction. Reject absolute paths, parent traversal, unexpected executables, or unrelated files. Extract into a temporary directory, hash every selected source, and copy only the accepted model and maps into its `Source` folder.
4. Check the source in neutral and textured views. Record mesh/render count, triangles, vertices, UV0, normals, tangents, animation/rig state, texture dimensions, and material slots. A successful conversion is not approval.
5. Run the asset-specific Unity generator. It must make imports reproducible, derive packed textures where needed, create the URP material and prefab, apply metre scale and `+Y`, establish the required pivot/colliders, and validate the result.
6. Run the representative scene capture at the normal gameplay camera and compare any requested texture or optimization variants. Leave the lowest-cost setting that shows no meaningful gameplay-view loss.
7. Run EditMode and PlayMode tests. Confirm every approved binary is a Git LFS object before commit and push.
8. Append license evidence, cost, review decision, exceptions, and replacement status here. Production approval still requires representative-device profiling and explicit art approval.

For the table, use these Unity menu actions:

- `The Fall > Prototype Assets > Table > Generate`
- `The Fall > Prototype Assets > Table > Validate`
- `The Fall > Prototype Assets > Table > Capture 1K-2K Comparison`
- `The Fall > Table Composition > Capture Validation Set`

The generator is idempotent: it configures importers, rebuilds the metallic/smoothness mask, updates the URP material and prefab, and leaves gameplay textures at 1024 pixels.

## ENV-P-ROUND-TABLE intake record

### Provenance and selection

| Field | Recorded value |
| --- | --- |
| source concept | `ENV-P-ROUND-TABLE_Concept.png`, prompt and hash recorded in the generated concept package |
| generator | Meshy AI, Image to 3D, Smart Topology model type |
| generation date | 2026-07-20 |
| selected download | `Meshy_AI_Medieval_Round_Table_0720125243_texture_fbx.zip` |
| archive SHA-256 | `c15ec499ed5768af0eedb247ab9293a5e8987b53f9133358045b676386fcddf0` |
| geometry cost | 5 Meshy credits, owner reported |
| texture cost | not recorded by the service operator; pending rather than inferred |
| texture settings | 2048 PBR maps; base-color highlight/shadow removal enabled |
| account/license | Pro account, Private License selected, owner reported |
| license evidence | service UI evidence is not embedded in FBX; screenshot or task receipt remains pending external evidence |
| intended use | V0 fixed-camera table composition and asset-pipeline validation |
| approval | accepted as a V0 prototype with a documented triangle-budget exception; not production-approved |

The license statement is traceable to the owner review but cannot be independently recovered from FBX metadata. Do not reinterpret the absence of embedded license fields as CC BY 4.0. Retain the Meshy task receipt or screenshot outside the repository when available.

### Retained source

| Repository source | SHA-256 | Source dimensions |
| --- | --- | --- |
| `ENV-P-ROUND-TABLE_SmartTopology.fbx` | `1e410dc5c1707d5d2fe95597cd21cd937ee338a19116f0bf76e4debc413e46ca` | 13,253 triangles; 14,656 imported vertices |
| `ENV-P-ROUND-TABLE_Albedo_2K.png` | `af047bee2f7b9eb53f3888504733b2a12755261ea74e6ea932dfb9bd4e0e0a23` | 2048 × 2048 |
| `ENV-P-ROUND-TABLE_Normal_2K.png` | `6352195b7405ee547b54f42565fe79120e27929ecb20a125b6e8cf64c057e51d` | 2048 × 2048 |
| `ENV-P-ROUND-TABLE_Metallic_2K.png` | `9f9882a49fbda7485c31c3cde87d539a6c40b0bbec58bc289a44ffcd504aa331` | 2048 × 2048 |
| `ENV-P-ROUND-TABLE_Roughness_2K.png` | `e28675b42c2c7d9c7c57e8f957e65c3ba8d592cf85220196b37b3609273ec88e` | 2048 × 2048 |
| `ENV-P-ROUND-TABLE_Emission_2K.png` | `8de2e4940e7f1e1d14cd6955d3f36858751ccfd80f1aea9ae42859ce46aa15a4` | 2048 × 2048; almost black and intentionally unused |

The source has one static mesh, one renderer, one material slot, complete normals/tangents/UV0, and no animation. The selected source archive is not committed because it duplicates the retained files. All retained FBX and PNG binaries are covered by Git LFS.

### Unity output and import settings

The generator creates:

- `ENV-P-ROUND-TABLE_MetallicSmoothness_1K.png`: metallic in red and inverted roughness in alpha, downsampled by a deterministic 2 × 2 average; SHA-256 `bf4f7d153d60212b5efc547f17bfce1711338ef5927cba75f12852cbd3e9e28a`
- `ENV-P-ROUND-TABLE_V0.mat`: one URP Lit material using albedo, normal, and packed metallic/smoothness; normal strength `0.75`, smoothness scale `0.52`, emission disabled
- `ENV-P-ROUND-TABLE_V0.prefab`: exact 1.45 m diameter and 0.76 m height, floor-centre pivot, one tabletop box collider, and one pedestal capsule collider

Model import disables animation, cameras, lights, blend shapes, material extraction, and CPU read/write; enables polygon/vertex optimization; and uses medium mesh compression. The visual child corrects the source axis/scale without modifying the retained FBX. Albedo, normal, metallic, roughness, and packed mask use mipmaps where relevant; runtime albedo, normal, and mask import at 1024 pixels. The original 2K files remain the reproducible source.

### Review and performance results

| Gate | Result | Evidence and action |
| --- | --- | --- |
| provenance and license | Conditional pass | source/archive hashes and owner-reported Private License recorded; external service receipt still pending |
| brief silhouette and gameplay function | Pass for V0 | round uninterrupted dark play field, broad rim, compact pedestal, and no authored seat positions remain readable from the fixed camera |
| topology and UV | Pass for V0 | one mesh/renderer, complete UV0/normals/tangents, no holes observed in the selected Smart Topology output |
| triangle target | Exception | 13,253 triangles is 1,253 triangles (10.4%) above the provisional 12K LOD0 target |
| material/texture target | Pass | one material; 1024 runtime texture path; 2K retained only as source/comparison |
| scale, pivot, collision | Pass | automated validation confirms 1.45 × 0.76 × 1.45 m bounds, floor pivot, and two primitive colliders |
| 1K versus 2K | 1K selected | exact 1920 × 1080 fixed-camera captures showed no meaningful visual gain at 2K; both rendered the same geometry |
| representative scene | Prototype pass | 2v2 desktop capture rendered 28,395 triangles, 26,504 vertices, and 98 renderer/material submissions across the whole programmatic prototype composition |
| mobile/device performance | Pending | representative-device frame timing and GPU profiling remain part of later platform validation |
| replacement status | Prototype | suitable for composition and interaction experiments; final retopology, LODs, lightmap UV review, and art approval remain open |

The 12K exception is intentional and narrow. Meshy's 10K, 30K, fixed 100K, adaptive-high, and adaptive-ultra remesh attempts visibly destroyed rim, centre, or support detail. Increasing the Smart Topology source by 1,253 triangles is cheaper and safer for V0 than committing a damaged remesh. Revisit this only with controlled manual retopology or a production LOD pass.

Validation on Unity 6000.5.4f1 completed on 2026-07-20:

- table generation and asset validation: passed twice with the same generated mask hash
- complete EditMode suite: 29/29 passed
- complete PlayMode suite: 6/6 passed, including MatchPrototype prefab presence and recomposition
- FoundationSetup validation: passed
- representative 1K/2K and portrait/landscape captures: rendered successfully; temporary captures remain outside version control

### Rejected and superseded iterations

These files remain outside the repository. Hashes identify the reviewed outputs without retaining bulk experiments:

| Variant | SHA-256 | Result |
| --- | --- | --- |
| accurate-shape private master, 568,534 triangles | `1fe7e260bd29f75d3adec71085d3952f71ff0950ba816e156b28bc5293bb150e` | silhouette preferred, but unsuitable for runtime and without usable UVs |
| fixed 100K remesh, actual 102,040 triangles | `f51b6a4aa9dd6ac544bc804a2b6c613d92a8971e6b6dfbe0d76519040fa083c5` | holes, inverted patches, and melted geometry; rejected |
| untextured Smart Topology candidate | `7eed38c70e9c130a93edcc29b356ee24a34fc9302a66e046ac32257186789765` | selected geometry before texture generation; superseded by the textured package |

The owner also visually rejected 10K, 30K, adaptive-high, and adaptive-ultra remeshes; those variants were not promoted or copied into the repository. One accurate-shape retry was free, as reported by the owner.

## Remaining issue work

- Run the owner-operated Meshy conversion for `CHR-P-WARM-CHALLENGER`, then append the same license, cost, topology, rig/readiness, Unity import, and representative-scene evidence.
- Retain the modular card path as the representative card category; no embedded-symbol full-card 3D generation is required.
- Perform representative-device profiling before any generated asset is called production-ready.
