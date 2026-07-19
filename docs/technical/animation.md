# Animation Direction

Status: Draft

## Principle

Game rules resolve first. Animation explains and celebrates the resolved result.

The presentation layer should receive a sequence of meaningful events such as card dealt, card played, capture resolved, cascade resolved, Fall scored, clean table scored, canto awarded, round ended, and match won.

## Representative animation families

- dealer selection reveal
- cards dealt to each seat
- card played from a hand to the table
- simple capture
- cascade capture
- Fall, with intensity based on rank or score
- clean table
- canto reveal and scoring
- remaining-table-card collection
- round and match completion

## Proposed experiment workflow

1. Build animation experiments outside the full game loop.
2. Expose timing and easing parameters for rapid iteration.
3. Test the same sequence at different table seats and aspect ratios.
4. Validate cancellation, skip, fast-forward, and reduced-motion behavior.
5. Connect an approved sequence to recorded domain events.
6. Promote reusable animation orchestration into project infrastructure.

## Constraints

- animations must not alter authoritative game state
- sequence ordering must be deterministic for a resolved event list
- card origin and destination anchors must work for every seat
- portrait and landscape layouts must not require unrelated rule logic
- mobile performance must influence effect and character complexity

## Open decisions

- Animator, Timeline, tweening library, or custom orchestration responsibilities
- non-camera visual emphasis during scoring events
- animation skip and speed controls
- pooling strategy
- VFX quality tiers
