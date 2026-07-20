# Modular Card Visual Pipeline

Status: Implemented V0 prototype

## Outcome

The complete forty-card Spanish deck is generated from reusable, project-owned visual components. Full card concept renders remain visual references; they are not copied into production textures. This preserves one typography system, one set of suit symbols, one border system, and one runtime material while allowing each court card to receive distinct artwork.

For first-playable perspective testing, the generated face currently uses a deliberately simplified
readability treatment: one dominant central rank and one clear suit marker. Pip layouts and distinct court
slots remain retained in the source layout data for the later production card-design pass, but they are not
composed into the current test atlas.

This pipeline implements [ADR 0001](../decisions/0001-modular-card-visual-pipeline.md).

## Source components

The generator creates these prototype sources only when they do not already exist:

| Source | Size | Responsibility |
| --- | ---: | --- |
| `Assets/TheFall/Content/Cards/Source/CardBase.png` | 252 x 352 | 63:88 vellum face, keylines, and corner frames |
| `Assets/TheFall/Content/Cards/Source/CardBack.png` | 252 x 352 | rotationally symmetric card back |
| `Assets/TheFall/Content/Cards/Source/CardSuitAtlas.png` | 512 x 128 | Coins, Cups, Swords, and Clubs symbols |
| `Assets/TheFall/Content/Cards/Source/CardRankAtlas.png` | 480 x 192 | one consistent glyph set for 1–7 and 10–12, upright and inverted |
| `Assets/TheFall/Content/Cards/Source/CardCourtAtlas.png` | 1024 x 768 | twelve distinct rank/suit illustration slots for 10, 11, and 12 |

The 252 x 352 face size is exactly 63:88 at four pixels per millimetre. These PNGs are replaceable source assets. An artist can redraw a whole atlas or preserve its cell boundaries and replace individual cells. Running the generator afterwards recomposes the deck without changing rules or card layout code.

The current source graphics are deterministic code-native prototype art. In particular, the twelve court panels prove unique artwork slots and composition behavior; they are not final character illustrations.

## Layout and generated output

`SpanishDeckVisualLayout` retains forty production-oriented domain-card layouts:

- ranks 1–7 reuse one pip-placement template per rank and substitute the selected suit symbol;
- rank glyphs and corner symbols are shared components, so type size and spacing cannot drift per card;
- ranks 10–12 select one unique court-art cell for each rank and suit;
- upper and lower corner stacks are available for two-way production reading.

The current test-atlas composition substitutes the shared large rank and suit marker for those detailed
layouts. This isolates camera and physical-card readability from unfinished illustration fidelity.

`CardDeckAssetGenerator` produces:

| Output | Responsibility |
| --- | --- |
| `Assets/TheFall/Content/Cards/Generated/CardFaceAtlas.png` | all forty 252 x 352 faces in an 8 x 5 layout inside a 2048 x 2048 texture |
| `Assets/TheFall/Content/Cards/Generated/CardFaceShared.mat` | single URP material used by card-face renderers |
| `Assets/TheFall/Content/Cards/Generated/CardVisualCatalog.asset` | domain `Card` to atlas-rectangle mapping plus face, back, and material references |

Unused atlas padding is transparent. Faces remain opaque within their silhouettes and preserve the original aspect ratio.

## Generation and validation

In the Unity Editor, run:

- `The Fall > Cards > Generate Complete Deck`
- `The Fall > Cards > Validate Complete Deck`

For headless generation from the repository root:

```sh
/Applications/Unity/Hub/Editor/6000.5.4f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath /Users/manuel/Developer/personal/the-fall \
  -executeMethod TheFall.Editor.CardDeckAssetGenerator.GenerateAll
```

Validation fails when a component or generated asset is absent, the atlas/back dimensions change unexpectedly, the catalog does not contain forty unique domain cards, a rectangle is invalid, or the catalog loses its shared atlas material.

EditMode tests additionally verify the complete domain-deck mapping, pip counts for ranks 1–7, unique court layout selection, valid UV rectangles, and per-renderer atlas selection without cloning the shared material.

## Runtime binding

`CardVisualMaterialBinding.Apply` assigns `CardVisualCatalog.SharedFaceMaterial` to a renderer and selects the requested card through `_BaseMap_ST` and `_MainTex_ST` values in a `MaterialPropertyBlock`. The card-face mesh must expose normalized 0–1 UVs.

This keeps one shared material and atlas across all ranks and suits. Do not clone materials or build rank and suit graphics as separate runtime objects. The independent component layers are an authoring system; the baked atlas is the runtime representation.

## Replacement workflow

1. Keep the existing source image dimensions and atlas cell order.
2. Replace only the source component that requires art revision. The generation command will not overwrite it.
3. Regenerate the complete deck.
4. Inspect the full atlas for rank scale, suit recognition, spacing, and contrast. When the detailed
   production composition is restored, also inspect inversion, corner spacing, pip count, and court art.
5. Run complete-deck validation and the EditMode suite.
6. Commit both the reviewed source and regenerated output through Git LFS.

If source dimensions or cell packing must change, update the generator, catalog validation, this document, and representative-scene readability checks together.

## Reproducibility record

Prototype source and output SHA-256 values at implementation:

| File | SHA-256 |
| --- | --- |
| `CardBase.png` | `68c5c6143b9a8e6a1c0c96f9a4b00e42b1315092672520962c078c78d25faf0c` |
| `CardBack.png` | `db4f82185bb53f90197062394ff929e7a702b396737d41297967708bf0f55d66` |
| `CardSuitAtlas.png` | `590d738eba95ba01a714b6551773dfdaef10ba63c7b5a4fef56293c59cbaeca1` |
| `CardRankAtlas.png` | `9a9552386e1b70c6e65345df1dcdcdacd77fca014272e02825c66eba52233e13` |
| `CardCourtAtlas.png` | `39f78e664fe20a99b4371ca2df22e952349f99fd49d41524e3efb642d3420529` |
| `CardFaceAtlas.png` | `cfda4246abd089c2aa60aa4b9c73d83c8fa60990cb1804234f5ea524f1b420ae` |
