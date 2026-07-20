# Compose card faces from reusable components at editor time

Status: Accepted
Date: 2026-07-20

## Context

The Fall uses a forty-card Spanish deck: four suits and ranks 1–7, 10, 11, and 12. Creating one complete image or material for every card would duplicate borders, typography, and suit artwork, make small consistency corrections expensive, and invite visual drift. Runtime composition from layered Unity objects would keep the pieces reusable but add draw calls, hierarchy complexity, and presentation-state work to every card.

The approved Four of Coins, Cups, Swords, and Clubs concepts establish the visual family, but they are perspective concept renders rather than production-ready flat card faces. The owner approved extracting or recreating individual components and letting Unity generate the complete deck.

## Options

1. Author forty independent complete card textures and materials.
2. Build every visible card from layered meshes or UI elements at runtime.
3. Maintain reusable source components and deterministic layout data, then bake a complete atlas in the Unity Editor.

## Decision

Use option 3.

- Maintain one replaceable source for the card base, rotationally symmetric back, shared rank glyphs, four suit symbols, and twelve court-illustration slots.
- Define ranks 1–7 as deterministic pip placements shared across suits.
- Give ranks 10–12 a distinct illustration slot for every suit and rank combination.
- Bake all forty faces into one atlas in the Unity Editor and map domain cards to atlas rectangles through a `ScriptableObject` catalog.
- Bind atlas regions with a `MaterialPropertyBlock` while retaining one shared material. Do not instantiate a material per card.
- Treat generated component art as project-owned V0 prototype artwork. The generator creates source components only when they are missing, so approved artist replacements are never overwritten.
- Keep Meshy out of the card-face pipeline. The existing card mesh may remain simple shared geometry; Meshy conversion is reserved for the manually owned character and furniture work.

## Consequences

Typography, borders, corner spacing, and suit symbols stay consistent across the deck. Layout or source-art changes can regenerate every affected face in one operation, while game rules remain independent of Unity and visual assets. Runtime rendering uses a compact shared-material path instead of layered card hierarchies.

Generated atlas output is committed through Git LFS for reproducible Unity imports. Source component dimensions and atlas packing are currently fixed by the V0 generator, so changing them requires updating validation and possibly mesh/material assumptions. The court illustrations are explicit prototype placeholders and require later art review; replacing them does not require changing deck logic.
