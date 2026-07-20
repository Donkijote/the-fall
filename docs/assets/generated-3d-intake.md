# Generated 3D Asset Intake

Status: Representative table, character, and modular-card paths executed and approved for V0 prototype use

## Purpose

This record turns the [asset strategy](strategy.md) and [prototype briefs](prototype-briefs.md) into a repeatable concept-to-Unity intake. It records the retained Meshy table, chair, and character assets, their rejected alternatives, Unity settings, validation, and known exceptions. It does not promote them to final production art.

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
- `The Fall > Prototype Assets > Generate Chair and Character`
- `The Fall > Prototype Assets > Validate Chair and Character`
- `The Fall > Prototype Assets > Open Asset Review Scene`
- `The Fall > Table Composition > Capture Validation Set`

The generator is idempotent: it configures importers, rebuilds the metallic/smoothness mask, updates the URP material and prefab, and leaves gameplay textures at 1024 pixels.

`AssetReview.unity` presents the three generated 3D prototypes on a neutral floor with warm key and cool fill lighting. In Play mode, press `1` for Round Card Table, `2` for Simple Chair, or `3` for Warm Challenger; drag the left mouse button or use the arrow keys to orbit, use the mouse wheel to zoom, and press `R` to reset. The scene stays separate from gameplay and is enabled in the build scene list for direct inspection.

## Project-owned Unity naming

Vendor download titles remain only in provenance records. Unity assets use short project-owned identities:

| Brief identity | Unity identity | Source model |
| --- | --- | --- |
| `ENV-P-ROUND-TABLE` | `RoundCardTable` | `RoundCardTable.fbx` |
| `ENV-P-SIMPLE-CHAIR` | `SimpleChair` | `SimpleChair.fbx` |
| `CHR-P-WARM-CHALLENGER` | `WarmChallenger` | `WarmChallenger.fbx` |

The former technical table folder and filenames were migrated through Unity's `AssetDatabase`, preserving their `.meta` GUIDs and existing scene references. No retained Unity object uses a Meshy-generated title.

## RoundCardTable intake record

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
| `RoundCardTable.fbx` | `1e410dc5c1707d5d2fe95597cd21cd937ee338a19116f0bf76e4debc413e46ca` | 13,253 triangles; 14,656 imported vertices |
| `RoundCardTable_Albedo_2K.png` | `af047bee2f7b9eb53f3888504733b2a12755261ea74e6ea932dfb9bd4e0e0a23` | 2048 × 2048 |
| `RoundCardTable_Normal_2K.png` | `6352195b7405ee547b54f42565fe79120e27929ecb20a125b6e8cf64c057e51d` | 2048 × 2048 |
| `RoundCardTable_Metallic_2K.png` | `9f9882a49fbda7485c31c3cde87d539a6c40b0bbec58bc289a44ffcd504aa331` | 2048 × 2048 |
| `RoundCardTable_Roughness_2K.png` | `e28675b42c2c7d9c7c57e8f957e65c3ba8d592cf85220196b37b3609273ec88e` | 2048 × 2048 |
| `RoundCardTable_Emission_2K.png` | `8de2e4940e7f1e1d14cd6955d3f36858751ccfd80f1aea9ae42859ce46aa15a4` | 2048 × 2048; almost black and intentionally unused |

The source has one static mesh, one renderer, one material slot, complete normals/tangents/UV0, and no animation. The selected source archive is not committed because it duplicates the retained files. All retained FBX and PNG binaries are covered by Git LFS.

### Unity output and import settings

The generator creates:

- `RoundCardTable_MetallicSmoothness_1K.png`: metallic in red and inverted roughness in alpha, downsampled by a deterministic 2 × 2 average; SHA-256 `bf4f7d153d60212b5efc547f17bfce1711338ef5927cba75f12852cbd3e9e28a`
- `RoundCardTable.mat`: one URP Lit material using albedo, normal, and packed metallic/smoothness; normal strength `0.75`, smoothness scale `0.52`, emission disabled
- `RoundCardTable.prefab`: exact 1.45 m diameter and 0.76 m height, floor-centre pivot, one tabletop box collider, and one pedestal capsule collider

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
- complete EditMode suite: 34/34 passed, including readable-name, source-geometry, material, prefab, and review-scene coverage for all three retained 3D assets
- complete PlayMode suite: 7/7 passed, including MatchPrototype prefab presence, recomposition, and AssetReview framing
- RoundCardTable, SimpleChair, WarmChallenger, and AssetReview validators: passed
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

## SimpleChair intake record

### Provenance and selection

| Field | Recorded value |
| --- | --- |
| source concept | `ENV-P-SIMPLE-CHAIR_Concept.png`, recorded in the generated concept package |
| generation date | 2026-07-20 |
| original download | `Meshy_AI_Brass_Banded_Rustic_C_0720134610_texture_fbx.zip` |
| archive SHA-256 | `ad30f41977c7d7377ef6e97423f9797b8940b2fdea019f9ac98a8581255515cc` |
| generator/topology setting | Meshy AI; exact model/topology label not embedded in FBX |
| account/license | Pro account, Private License selected, owner confirmed on 2026-07-20 |
| credits | exact generation/texture breakdown not retained by the operator; recorded as unknown rather than inferred |
| approval | accepted as a static V0 supporting furniture prototype |

The safe archive contained one FBX and five 2048² PBR maps. The archive itself remains outside the repository.

| Retained source | SHA-256 |
| --- | --- |
| `SimpleChair.fbx` | `1e15ec10d14d9667c4f7731bfc159d038296ac7f9846e899e4bba2e6d59fea05` |
| `SimpleChair_Albedo_2K.png` | `2325faf7be9cccd104397286aaf63996729073c8a3d1dfb9e2d53214a8109dd1` |
| `SimpleChair_Normal_2K.png` | `66adddb1016126f1f903f9a8fe94340b2de4bedaa090db5ecf411ac6fd781f78` |
| `SimpleChair_Metallic_2K.png` | `6d2563d9c5db7a0bd68f7a6da500316a85ae8bf39b8d7e2242c3bc3d48abbed7` |
| `SimpleChair_Roughness_2K.png` | `72e906a09c9f8720c25a4e0518390444ccf4c1e1f1eaff9ca4c7ca85ee696bb8` |
| `SimpleChair_Emission_2K.png` | `b1379322d3b4ad060a5344249eddb8307b7f4910bd35b0b4cb95ef3305a4a6dd` |

Unity inspection records one static mesh/renderer/material slot, 12,336 triangles, 9,320 vertices, complete UV0/normals/tangents, and no animation. `SimpleChair.prefab` preserves proportions at 1.00 m total height with a floor-centre pivot and one review collider. Its single URP material uses 1024 runtime maps and generated mask `fa1468d179ea0ac735acfd1a1ce788555c43301abb9b0125e430d353c62049fa`.

## WarmChallenger intake record

### Provenance and selection

| Field | Recorded value |
| --- | --- |
| source concept | `CHR-P-WARM-CHALLENGER_FullBody.png`, recorded in the generated concept package |
| generation date | 2026-07-20 |
| original download | `Meshy_AI_Aric_Stormwood_0720135109_texture_fbx.zip`; vendor title retained only for traceability |
| archive SHA-256 | `7e4679f3fdd75fcba3573518d27e9a168198d9330ad77f449039149b29a999de` |
| project identity | `WarmChallenger`; the vendor-generated character name is not used in Unity |
| generator/topology setting | Meshy AI; selected high-resolution result after the owner rejected a visually damaged 100K remesh |
| account/license | Pro account, Private License selected, owner confirmed on 2026-07-20 |
| credits | exact generation/texture breakdown not retained by the operator; recorded as unknown rather than inferred |
| approval | owner-selected high-resolution V0 visual reference; static review only, not gameplay- or production-approved |

| Retained source | SHA-256 |
| --- | --- |
| `WarmChallenger.fbx` | `84b889a5c8ebd8fcbc837df89c64fc0f393332868bc7c4ce6c433fe81f879399` |
| `WarmChallenger_Albedo_2K.png` | `a9db16e9bd5719a608b369402e0ea9950daf545436553d8f2ee542dee1d2cde3` |
| `WarmChallenger_Normal_2K.png` | `417e4bc01eab2f1a39e938c96c01015951c2308a5003d589857e19574e1e3b20` |
| `WarmChallenger_Metallic_2K.png` | `15b773cc2b23a3d395ef01cd1b85b50eb4d6d722b0b689e2577fc5df766fef84` |
| `WarmChallenger_Roughness_2K.png` | `49ed514e909ce61cb5214505e3479bd2c8f6a3021cb619f05634c120bb851103` |
| `WarmChallenger_Emission_2K.png` | `f54a06c69891a97ef185dfa0a0e36942303e54ab74e6cb66412489de4f5da61e` |

Unity inspection records one static mesh/renderer/material slot, 366,508 triangles, 192,160 vertices, complete UV0/normals/tangents, no bones, and no animation clips. `WarmChallenger.prefab` preserves proportions at 1.78 m standing height with a floor-centre pivot and one capsule review collider. Its single URP material uses 1024 runtime maps and generated mask `fe210ef01b2dfa8081f15216c85a123c2b65e25c20914de7dd2c19dd566f409c`.

The character is 341,508 triangles above, or 14.7 times, the provisional 25K LOD0 target. Mesh compression is disabled to avoid adding damage to the explicitly selected high-resolution source. This exception does not revise the target: the asset stays isolated in `AssetReview`, must not replace gameplay seat characters, and requires controlled retopology, LODs, rigging, deformation tests, and device profiling before gameplay use. The rejected 100K remesh remains outside the repository; no file/hash was supplied for it.

## Follow-up production work

- Retain external Meshy UI receipts when available; the owner-confirmed Private License is not embedded in FBX metadata.
- Retain the modular card path as the completed representative card category; no embedded-symbol full-card 3D generation is required.
- Perform representative-device profiling before any generated asset is called production-ready.
