# P07.6 — DEBUFF / CC / RESISTANCE / ANTI-CC — LOCKED

## Scope
Extension of P07.5 using the shared status/effect architecture.

## Core locked behavior
- 9 numerical Debuff categories: Attack, Defense, AttackSpeed, MoveSpeed, CritRate, DodgeRate, RageGain, HealingReceived, DamageDealt.
- Modifier modes: Flat / Percentage.
- CC types: Stun, Root, Freeze.
- Effect Power Tier: A/B/C.
- `StatType.CcResistance` is clamped 0..1.
- Effective CC duration follows the locked resistance rule: base duration × (1 - resistance).
- 100% resistance blocks CC.
- Anti-CC immunity is checked before CC application/resistance.
- Freeze has Stun-like action-control behavior; Freeze Shatter is an explicit hook and only triggers when the applicable effect has `CanShatterFreeze=true`.
- Action permissions include CanMove, CanBasicAttack, CanUseSkill, CanUseUltimate, CanDash.
- Movement/attack/skill systems enforce the shared permissions.
- Root permits skill use according to the established permission rules.
- Debuff modifiers integrate with Rage Gain, Healing Received, Damage Dealt and Attack Interval.

## Acceptance status
- Automated: 55/55 PASS.
- Play Mode: 35/35 PASS.
- P06 Play Mode regression: 14/14 PASS.
- P07.5 Automated regression: 46/46 PASS.
- P07.5 Play Mode regression: 30/30 PASS.
- Master Regression: PASS.
- P07.6: LOCKED according to the supplied acceptance report.

## Evidence note
Results are user-supplied execution evidence; the assistant did not independently execute Unity.
