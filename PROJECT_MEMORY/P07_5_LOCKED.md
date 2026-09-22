# P07.5 — DEBUFF / DoT / STATUS TICK — LOCKED

## Scope
Direct extension of the existing status/effect architecture. No duplicate status authority or separate damage pipeline.

## Locked acceptance status
- Automated tests: 46/46 PASS.
- Unity Play Mode acceptance: 30/30 PASS.
- Master Regression through P07.5: PASS.
- P07.5: LOCKED.

## Current architecture
- Shared `EntityStatusController` owns runtime statuses.
- Shared `EffectResolver` / skill effect pipeline resolves effects.
- Debuff, DoT and status ticking must remain integrated with the existing combat/stat pipeline.
- Preserve stacking/refresh/replace/max-stack behavior established by P07.5 when extending later status features.

## Evidence note
These results are based on the user's supplied implementation/acceptance reports and are not an independent execution by the assistant.
