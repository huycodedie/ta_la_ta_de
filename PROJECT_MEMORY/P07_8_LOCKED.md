# P07.8 — SHIELD / BARRIER — LOCKED

## Architecture
- `EntityStatusController` is the sole runtime shield authority.
- `DamageCalculator` remains authoritative for damage calculation.
- `HealthComponent` remains authoritative for HP deduction.
- Shield interception occurs after damage calculation/modifiers and before HP decrement.
- Multiple shields consume deterministically: Priority descending -> StartTime ascending/FIFO -> ShieldId ordinal ascending.
- No per-shield Update/coroutine/GameObject architecture.
- Stacking policies: Additive, RefreshDuration, Replace, Independent, Ignore.
- Shield state uses `RuntimeShieldInstance` and `ShieldEffectDefinitionSO` through the existing skill/effect pipeline.
- Standard Cleanse/Dispel do not remove Shields unless explicitly targeting `StatusRemovalCategory.Shield = 9`.
- Anti-CC/CC Resistance and Freeze Shatter remain isolated.
- Shield events are integrated into EventBus.

## Acceptance
- Automated: 55/55 PASS.
- Play Mode: 35/35 PASS.
- UI tests: 5/5 PASS.
- Visual V01-V08: PASS.
- Required regressions: PASS.
- Master Regression P01-P07.8: 100% PASS according to the latest supplied report.
- P07.8: LOCKED.

## Important evidence qualification
The acceptance status above is based on the user's supplied Unity execution/remediation report. It is not an independent execution by the assistant.

## Regression remediation recorded
- Test environment cleanup was added to prevent leaked singleton/GameObject/EventBus/PlayerPrefs state between suites.
- P05.7.1.14 was hardened against random Dodge dependence by allowing repeated ticks according to the reported fix.
