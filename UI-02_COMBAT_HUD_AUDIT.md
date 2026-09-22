# TLTD — UI-02 COMBAT ZONE & HUD AUDIT

**Milestone:** UI-02 Combat Zone & Combat HUD Polish  
**Engine:** Unity 6000.6.0f1  
**Project:** E:\code\TLTD  
**Status:** Audit Complete — Pre-Implementation  
**Rule:** Zero gameplay changes. UI is strictly a presentation layer over authoritative runtime state.

---

## 1. Executive Summary

Milestone UI-01 successfully established the portrait canvas architecture (`1080x1920`), `SafeAreaRoot`, `MainGameShell`, `GlobalBottomNavigation` (5 positions with prominent center Main Hub), and collapsible debug drawer.

However, the combat zone and HUD currently exhibit "developer/prototype appearance":
1. **Top HUD (`TopHeader`):** Basic dark translucent rectangle with plain text for level, rank, and title; player resources (Gold, Material) are hidden inside the debug panel rather than integrated into the header.
2. **Monster/Boss HUD (`MonsterUI`):** Rigid rectangular panel with flat color bars; lacks wuxia framing, boss ornamentation, and has no status icon indicators.
3. **Hero Combat HUD (`HeroUI`):** Flat rectangular box stacking HP, Shield, Rage, and Cast bars without ornamental borders, Qi energy visual cues, or status indicators.
4. **Skills & Action Controls:** No dedicated on-screen skill action bar exists during combat. Skills can only be previewed via modal windows (`MindMethodUI`), and no compact action buttons (Basic, Tuyệt Kỹ, Ngoại Công, Thần Công/Ultimate) or Auto/Speed toggles are present on the combat HUD.
5. **Cast / Channel Presentation (`CastBarUI`):** Fully functional and authoritative (P07.9 Phase 5.3 compliant), but visually basic.
6. **Shield Presentation:** Authority correctly sits in `EntityStatusController` (P07.8 compliant), but visual treatment lacks a distinct Qi shield overlay/sheen and ornamental containment.
7. **Battle Area Composition:** Prototype equipment and inventory panels (`EquipmentStatsPanel`, `EquipmentDropPanel`, `ComparisonPanel`, `UpgradePanel`) occupy the middle screen, obscuring the battle area when on the combat screen.
8. **Companions (`EntityType.Companion`):** The engine supports `EntityType.Companion`, but no companion presentation or HUD exists to reflect companion state during battle.
9. **Combat Feedback (`DamagePopup`):** Minimalist floating text lacking wuxia calligraphic flair and shield absorption visual feedback.

---

## 2. Authoritative Data Bindings & Invariants

To guarantee 100% compliance with locked gameplay rules (P01–P07.9, D1–D23), all UI elements are bound strictly to authoritative components without duplicate calculations:

| Presentation Element | Authoritative Component / Source | Authoritative Fields / Events Read | Invariant / Restriction |
| :--- | :--- | :--- | :--- |
| **Hero HP** | `HealthComponent` | `CurrentHealth`, `MaxHealth`, `OnHealthChanged` | Read-only. No UI calculation. |
| **Monster HP** | `HealthComponent` | `CurrentHealth`, `MaxHealth`, `OnHealthChanged` | Read-only. No UI calculation. |
| **Shield (Hero/Monster)** | `EntityStatusController` | `TotalShieldAmount`, `HasActiveShield`, `ActiveShields`, `OnShieldApplied`, `OnShieldAbsorbed`, `OnShieldDepleted`, `OnShieldExpired`, `OnShieldRemoved` | Aggregate authoritative state. No UI shield timer or values. Disappears at zero. |
| **Hero Rage** | `RageComponent` | `CurrentRage`, `MaxRage`, `OnRageChanged` | Read-only. Visual pulse at MaxRage / Ultimate ready. |
| **Cast / Channel** | `Entity.CastState` (`SkillCastState`) | `Progress`, `ElapsedTime`, `CastDuration`, `ElapsedChannelTime`, `ChannelDuration`, `CurrentPhase`, `IsActive` | Existing `CastBarUI` reused. No second timer. |
| **Cast Interrupt** | `EventBus.OnSkillCastInterrupted` | `SkillCastInterruptSource` (Stun, Freeze, etc.) | Existing interrupt feedback reused. |
| **Status Effects** | `EntityStatusController` | `IsStunned`, `IsRooted`, `IsFrozen`, `HasAntiCCImmunity`, `ActiveBuffs`, `ActiveDebuffs`, `ActiveCrowdControls` | Read-only data-driven status icons. |
| **Skill Execution** | `Hero`, `MindMethodManager`, `CooldownManager` | `MindMethodManager.Instance.GetSelectedSkillForSlot(slot)`, `CooldownManager.IsOnCooldown(skillId, out remaining)`, `Hero.CanUseSkill`, `Hero.CanUseUltimate`, `Hero.ExecuteSelectedSkill(slot)` | Icon-first buttons. Cooldown sweep read from `CooldownManager`. |
| **Battle Speed / Auto** | Presentation Layer | `Time.timeScale` (clamped to 1x/2x for gameplay; restored to 1.0f for tests) | Visual toggle. Does not alter combat logic. |
| **Combat Feedback** | `EventBus.OnEntityDamaged`, `EventBus.OnShieldAbsorbed` | `DamageResult.FinalDamage`, `IsCrit`, `IsDodged`, `DamageType` | Visual polish only. |

---

## 3. Structural & Visual Deficiencies Identified

| Component | Current State | Deficiencies | Required Enhancement |
| :--- | :--- | :--- | :--- |
| **Top Header** | 1000x90 plain dark rectangle | Stiff text layout, Gold & Material resources buried in debug drawer. | Framed wuxia header with antique gold borders, level badge, compact EXP progress bar, and player resource badges (Gold, Jade/Material). |
| **Monster HUD** | 980x140 plain box | Giant red bar, plain label "YÊU THÚ", no boss framing, no status icon row. | Wuxia crimson/gold boss-style frame, dynamic monster/boss title, segmented HP bar with text readout, Qi shield bar overlay, and compact status row. |
| **Hero HUD** | 980x210 plain box | Flat jade bar, plain orange rage bar, stacked cast bar, no status row. | Jade dragon/lotus framed status panel, glowing Qi shield overlay, energetic Rage bar with ultimate-ready visual pulse, and compact status row. |
| **Action / Skill Area** | None on combat screen | No way to trigger or monitor skills without opening modals; no action bar layout. | Mobile RPG radial/cluster action bar: Slot 1 (Basic Attack), Slot 2 (Tuyệt Kỹ), Slot 3 (Ngoại Công 1), Slot 4 (Ngoại Công 2), Slot 5 (Prominent Thần Công / Ultimate), plus Auto and Speed toggles. |
| **Battle Viewport** | Obscured by middle equipment panels | Character models at `X = -4` and `X = +4` partially hidden behind equipment panels. | Organize middle equipment panels into the "TRANG BỊ" (Equipment) navigation tab; leave the Combat screen's central battle area open and visually dominant. |
| **Companions** | Not rendered in UI | If companions spawn, no compact HUD exists to show their status. | Compact companion portrait/status badges that dynamically bind when an `EntityType.Companion` entity exists. |
| **Debug Drawer** | Drawer exists (UI-01) | Drawer toggle button is visible; must remain unobtrusive during combat. | Keep drawer toggle compact and collapsed by default; retain all 12 debug buttons without interfering with combat presentation. |

---

## 4. Proposed Component Architecture

```
Assets/_Game/UI/
├── BattleHUD.cs                (Main Combat HUD controller - preserve all existing serialized fields & API)
├── CastBarUI.cs                (Cast/Channel presentation - preserved & visually enhanced)
├── HealthBarUI.cs              (Authoritative HP presentation - preserved & visually enhanced)
├── RageBarUI.cs                (Authoritative Rage presentation - preserved & visually enhanced)
├── DamagePopup.cs              (Polished floating combat text)
├── DamagePopupManager.cs       (Listens to damage & shield absorb events)
├── HUD/
│   ├── StatusIconRowUI.cs      [NEW] Compact data-driven status icon bar (CC, buffs, debuffs, anti-CC)
│   ├── SkillBarUI.cs           [NEW] Action cluster for 5 skill slots + Auto + Speed toggles
│   └── CompanionHUDUI.cs       [NEW] Compact companion status cards (reads authoritative companion state)
└── Core/
    ├── UIStyleConfig.cs        (Color tokens & layout constants)
    ├── MainGameShell.cs        (Shell coordinator)
    ├── GlobalBottomNavigation.cs (5-slot navigation bar)
    └── SafeArea.cs             (Device safe area)
```

---

## 5. Risk Assessment & Mitigations

1. **Test Suite Compatibility (P07.8, P07.9 Phase 5.3, Risk04):**
   - *Risk:* Automated tests instantiate or inspect specific fields on `BattleHUD`, `CastBarUI`, and `HealthBarUI`.
   - *Mitigation:* All existing serialized fields, public properties, methods, and GameObject names (`HeroHPBar`, `MonsterHPBar`, `HeroShieldBar`, `MonsterShieldBar`, `HeroRageBar`, `HeroCastBar`, etc.) will be strictly preserved.
2. **TimeScale Contamination:**
   - *Risk:* A speed toggle button could leave `Time.timeScale != 1.0f`, breaking regression tests.
   - *Mitigation:* Speed toggle button only operates in interactive Play Mode and resets to `1.0f` on disable/destroy. Tests always run with standard `1.0f` timescale.
3. **No Gameplay Logic in UI:**
   - *Risk:* Accidental cooldown calculation, rage consumption, or shield calculation in UI scripts.
   - *Mitigation:* UI scripts only read from `CooldownManager`, `RageComponent`, and `EntityStatusController`. Skill activation delegates directly to `Hero.ExecuteSelectedSkill(slot)`.

---

## 6. Audit Conclusion

The architecture is sound and fully prepared for UI-02 implementation without any disruption to locked gameplay logic. We proceed to create the implementation plan and seek user approval.
