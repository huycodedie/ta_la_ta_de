# P07.7 — CLEANSE / DISPEL / STATUS REMOVAL — LOCKED

## Core architecture
- Generic status removal is authoritative in `EntityStatusController`.
- Cleanse removes negative statuses according to target/category/selection rules.
- Dispel removes positive Buffs according to target/category/selection rules.
- Uses the existing `SkillExecutor` and `EffectResolver` pipeline.
- Selection modes include Oldest, Newest, Random, HighestPriority, LowestPriority and All where configured.
- Random tests use injected deterministic `RandomRangeProvider`.
- Partial stack removal is supported; zero-stack instances are cleaned up.
- Preserve P07.5 stack/refresh/replace/max-stack semantics.
- Cleansing CC immediately restores permissions.
- Stun/Freeze interruption behavior remains unchanged; Root does not gain an interrupt side effect.
- Cleansing Freeze must not trigger Freeze Shatter.
- Ordinary Cleanse/Dispel must not remove Anti-CC immunity unless an explicitly defined effect targets it.
- CC Resistance is not changed by Cleanse/Dispel.
- Shield isolation remains intact: standard Cleanse/Dispel does not remove Shield unless explicitly targeting the Shield category.

## Acceptance status
- Automated: 55/55 PASS.
- Play Mode: 35/35 PASS.
- Master Regression: PASS.
- P07.7: LOCKED according to supplied acceptance evidence.
- Visual status is not to be inferred from test counts unless actual visual evidence exists.

## Evidence note
Results are based on user-provided implementation/acceptance reports; the assistant did not independently execute Unity.
