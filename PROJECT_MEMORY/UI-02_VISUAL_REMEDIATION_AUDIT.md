# UI-02 — COMBAT ZONE & COMBAT HUD VISUAL REMEDIATION AUDIT

**PROJECT:** TLTD (Thao Thiet Long Than Dao - 饕餮龙神道)  
**WORKSPACE:** `E:\code\TLTD`  
**ENGINE:** Unity 6000.6.0f1 (64-bit)  
**MILESTONE:** UI-02 Visual Remediation  
**AUDIT DATE:** 2026-09-16  
**STATUS:** PENDING VISUAL REMEDIATION (Technical: PASS, Visual Acceptance: PENDING)  

---

## 1. Executive Summary & Audit Context

Milestone UI-02 previously achieved a **Technical PASS** by satisfying all 125 automated regression tests (P07.8, P07.9, P07.9.1) without violating locked gameplay authorities. However, an inspection of the actual Unity Play Mode runtime reveals that the visual presentation remains fundamentally a **developer/prototype interface** composed of plain text labels, flat monochrome rectangle bars, raw stretched sprites, and debug-style instrumentation.

This audit documents the exact deficiencies observed in the runtime scene, contrasts them against the architectural mandates of `UI_DESIGN_AUTHORITY.md` and `UI_REDESIGN_SPECIFICATION.md`, and specifies the concrete presentation remediation required to deliver a genuine 2D mobile portrait wuxia/anime RPG combat HUD (`1080x1920`, Safe Area compliant).

---

## 2. Screenshot-Observed & Runtime Deficiencies

| Component | Observed Runtime State (Deficiencies) | Required Target Wuxia RPG State |
|---|---|---|
| **Combat Zone World** | Camera background is a solid flat dark color (`#0F141F`). Ground is a plain flat brown stretched box. Hero is a cyan rectangle with 3D text "HERO". Monster is a red rectangle with 3D text "MONSTER". Large empty void between Y: 587 and Y: 1617. | Atmospheric 2D misty mountain/celestial background, layered stone combat platform with ground shadow and mist gradients, stylized wuxia combatant silhouettes/standees with character aura, clear depth and portrait framing. |
| **TopHeader** | Hardcoded title text "PROTOTYPE 05". Raw text labels ("HERO LV. 1", "Novice Disciple", "5,000 G", "10 MAT") floating on a basic box. | Compact, framed wuxia header with player avatar/realm plate, decorative gold/bronze trim, formatted EXP bar, and styled resource badges. |
| **Sub-Header Buttons** | Three large, brightly colored rectangular buttons ("CẤP RƠI", "DANH HIỆU", "TÂM PHÁP") occupying combat screen vertical space. | Integrated into modal navigation or minimized so they do not obstruct or clutter the primary combat viewport. |
| **Monster/Boss HUD** | Plain dark rectangle with flat red bar showing decimal text `500.00 / 500.00`. Generic text "YÊU THÚ - HOANG DÃ". Lacks enemy tier distinction. | Framed crimson-bordered card with Boss crest/skull, ornamental corner brackets, clean HP fill with beveled highlight, active shield overlay, and status badge row. |
| **Hero Combat HUD** | Oversized dark rectangle with multiple stacked flat bars resembling developer instrumentation. Text reads `1000.00 / 1000.00` and `Rage: 0 / 100`. | Coherent, ornamental combat card. Jade green health bar with inner gradient and glass sheen, Qi-cyan shield overlay, amber rage bar with excitation glow, and compact cast/channel progress bar. |
| **Skill Action Bar** | 5 flat square buttons with basic centered text ("Đánh Thường", "Tuyệt Kỹ", "Ngoại Công 1", "Ngoại Công 2", "THẦN CÔNG"). No icons, no wuxia framing. | 5-slot mobile action cluster with circular/beveled metallic frames, martial art icon silhouettes, radial dark cooldown sweep with numeric countdown, and prominent golden Ultimate slot. |
| **Auto / Speed Controls** | Plain rectangular buttons with basic text ("AUTO: TẮT", "1X") placed next to skill slots. | Polished toggle pills with distinct active/inactive wuxia styling (Gold active, Slate inactive). Speed toggle clearly identified as visual display without modifying `Time.timeScale`. |
| **Debug UI** | Prominent blue button labeled "DEBUG PANEL" rendered at bottom-left of gameplay area, directly above navigation. | Developer controls cleanly relegated to a discreet, collapsible pull-drawer tab (`[DEV]`) at the screen edge, keeping normal combat screen 100% player-facing. |
| **Global Bottom Navigation** | Text buttons labeled "Túi Đồ", "Tâm Pháp", "Đại Điện", "Bang Hội", "Thiết Lập" using plain stretched white squares for icons. | Framed navigation bar with stylized wuxia icons (Chest, Scroll, Temple/Sword, Banner, Seal), visual prominence for center Main Hub, and responsive Safe Area clearance. |

---

## 3. Source-Code Findings

1. **`CreateWhiteSprite()` Uniformity:**
   In [Prototype01SceneBuilder.cs:1793](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs#L1793), a single 32x32 solid white texture is generated and used for every sprite renderer, panel, bar fill, and button in the game. Without texture gradients, borders, or 9-slice sprites, all elements render as flat, featureless rectangles.
2. **Hardcoded Prototype Text:**
   In [Prototype01SceneBuilder.cs:533](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs#L533), the header title is hardcoded to `"PROTOTYPE 05"`.
3. **World Text Floating Combatants:**
   In [Prototype01SceneBuilder.cs:433](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs#L433) and [L453](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs#L453), `CreateWorldLabel` instantiates 3D TextMeshPro objects with literal strings `"HERO"` and `"MONSTER"` parented to the entity boxes.
4. **Navigation Label Discrepancy:**
   In [Prototype01SceneBuilder.cs:2008](file:///E:/code/TLTD/Assets/_Game/Editor/Prototype01SceneBuilder.cs#L2008), the labels are instantiated as `new string[] { "Túi Đồ", "Tâm Pháp", "Đại Điện", "Bang Hội", "Thiết Lập" }`. However, previous reports claimed `"Nhân Vật", "Ba Lô", "Chiến Đấu", "Công Pháp", "Tông Môn"`.
5. **Asset Gap:**
   The repository contains **zero** PNG/JPG character sprites or UI textures in `Assets/`. All visual polish must be achieved through procedurally generated wuxia textures (gradients, borders, rounded frames, icon silhouettes) and refined Unity UI styling tokens.

---

## 4. Hierarchy Findings & Layout Vertical Rhythm

The current scene hierarchy has all UI elements parented to `MainContentArea` without proper vertical grouping, resulting in awkward overlaps and a massive empty void in the middle of the screen.

### Target Vertical Rhythm (1080 x 1920 Reference Canvas):
```
┌─────────────────────────────────────────────────────────────┐
│ SAFE AREA TOP PADDING (Notch / Status Bar Margin)           │
├─────────────────────────────────────────────────────────────┤
│ 1. TOP HEADER (Player Avatar, Realm, EXP, Resources)        │  Height: ~100px
├─────────────────────────────────────────────────────────────┤
│ 2. BOSS / MONSTER HUD (Crimson Card, Boss Crest, HP/Shield) │  Height: ~130px
├─────────────────────────────────────────────────────────────┤
│ 3. COMBAT VIEWPORT (Misty Backdrop, Platform, Characters)   │  Height: ~820px
│    - Hero Standee & Aura (Left, X: -3.8)                    │
│    - Monster Standee & Aura (Right, X: +3.8)                │
│    - Damage Popups (Center / Headroom)                      │
├─────────────────────────────────────────────────────────────┤
│ 4. HERO COMBAT HUD (Jade HP, Qi Shield, Rage, Cast Bar)     │  Height: ~160px
├─────────────────────────────────────────────────────────────┤
│ 5. SKILL ACTION CLUSTER (5 Slots + Auto/Speed Toggles)      │  Height: ~180px
├─────────────────────────────────────────────────────────────┤
│ 6. GLOBAL BOTTOM NAVIGATION (5 Positions, Center Main Hub)  │  Height: ~150px
├─────────────────────────────────────────────────────────────┤
│ SAFE AREA BOTTOM PADDING (Home Indicator Margin)            │
└─────────────────────────────────────────────────────────────┘
* Collapsible Developer Debug Drawer accessible via minimal discreet tab at corner.
```

---

## 5. Specific Component Remediation Plan

### 5.1 Procedural Wuxia Texture Suite (`UIProceduralTextureFactory.cs`)
Since no raw image files exist in `Assets/`, create a dedicated procedural texture generator that creates runtime sprites:
- **Panel Frame:** 9-slice dark charcoal background with beveled edges, gold/bronze outer borders, and inner gradient shading.
- **Bar Frame & Fills:** Rounded pill bars with subtle horizontal gradients and upper-edge glass highlight lines (Jade Green, Crimson Red, Qi Blue, Amber Orange, Purple Channel, Cyan Cast).
- **Skill Button Frames:** Circular and beveled diamond frames with double-ring metallic borders, inner dark medallion, and distinct martial arts icon silhouettes (Sword for Basic, Palm/Gale for Skills, Dragon Crest for Ultimate).
- **Combat Scene Background:** Atmospheric 2D mountain silhouette backdrop with soft radial mist and sky gradient.
- **Combat Platform:** Textured stone battle ring with edge highlight and ambient ground shadow.
- **Character Standees:** Stylized wuxia martial artist standee silhouette with dao/jian aura (Hero) and ferocious beast/demon silhouette (Monster), replacing the plain colored rectangles and floating "HERO"/"MONSTER" labels.

### 5.2 Monster/Boss HUD
- Replace generic text with formatted wuxia monster title: `"YÊU THÚ • THIÊN LANG HOANG DÃ"`.
- Add Boss skull/crest badge on the left side of the health bar.
- Format HP bar with clean beveling and shield overlay.
- Keep `CurrentDisplayedText` backward-compatible (`${current:F2} / {max:F2}`).

### 5.3 Hero Combat HUD
- Replace raw rectangle with ornamental framed card featuring Hero crest and title (`"ĐẠO HỮU • LUYỆN KHÍ TẦNG 3"`).
- Jade HP Bar with Qi Shield overlay.
- Amber Rage Bar with full-rage excitation glow when `currentRage >= maxRage`.
- Cast Bar with cyan Qi animation and purple channel mode.

### 5.4 Skill Action Cluster
- Center the 5 slots with ergonomic thumb reach.
- Slot 1: Basic Attack (Swords icon).
- Slots 2–4: Equipped normal skills (Palm / Strike / Martial Art icons).
- Slot 5: Ultimate Skill (Larger 104px circle, Golden Dragon crest, pulsating ready aura).
- Auto & Speed toggle pills neatly integrated on the right flank.

### 5.5 Global Bottom Navigation
- Preserve exact 5-slot structural architecture per `UI_DESIGN_AUTHORITY.md`.
- Emphasize center Main Hub (`Chiến Đấu` / `Đại Điện`).
- Use procedural wuxia icons (Chest, Scroll, Crossed Swords, Sect Banner, Seal).
- Maintain navigation naming decision as OPEN per Section 16 of specification, while aligning with the current scene's structural indices.

### 5.6 Developer Debug Drawer
- Relegate all 12 debug buttons (`+10 EXP`, `+1K G`, `SIM 1000`, `RESET PROG`, etc.) to a slide-up drawer at the bottom right.
- Accessible via a minimal, discreet pull-tab (`[DEV DEBUG]`), completely removing developer clutter from the player-facing combat HUD.

---

## 6. Files Proposed for Modification

| File Path | Purpose |
|---|---|
| `Assets/_Game/UI/Core/UIStyleConfig.cs` | Add procedural texture references and refined wuxia visual tokens. |
| `Assets/_Game/UI/Core/UIProceduralTextureFactory.cs` [NEW] | Pure presentation generator for wuxia frames, bars, icons, and silhouettes. |
| `Assets/_Game/Editor/Prototype01SceneBuilder.cs` | Rebuild scene hierarchy with new procedural sprites, proper vertical spacing, and character framing. |
| `Assets/_Game/Scenes/Prototype01.unity` | Updated serialized scene asset. |
| `Assets/_Game/Editor/Prototype01VisualAcceptanceCapture.cs` [NEW] | Editor script to execute and capture Play Mode screenshots SHOT A through SHOT M. |

---

## 7. Files Explicitly Protected from Modification (Zero Gameplay Change)

The following gameplay authority files MUST NOT be modified under any circumstances:
- `Assets/_Game/Combat/SkillExecutor.cs`
- `Assets/_Game/Combat/SkillExecutionValidator.cs`
- `Assets/_Game/Combat/SkillCastState.cs`
- `Assets/_Game/Combat/CooldownManager.cs`
- `Assets/_Game/Combat/DamageCalculator.cs`
- `Assets/_Game/Combat/EntityStatusController.cs`
- `Assets/_Game/Combat/HeroSkillDecisionController.cs`
- `Assets/_Game/Combat/BattleManager.cs`
- `Assets/_Game/Entities/Components/HealthComponent.cs`
- `Assets/_Game/Entities/Components/RageComponent.cs`
- `Assets/_Game/Entities/Components/AttackComponent.cs`
- `Assets/_Game/Progression/MindMethodManager.cs`

---

## 8. Audit Conclusion

The technical plumbing and authority bindings of UI-02 are sound (125/125 tests PASS). The required remediation is strictly visual and hierarchical: replacing flat prototype geometry with cohesive procedural wuxia presentation assets, restructuring the vertical composition, eliminating developer clutter, and verifying the result via actual Unity Play Mode screenshot captures (SHOT A–M).
