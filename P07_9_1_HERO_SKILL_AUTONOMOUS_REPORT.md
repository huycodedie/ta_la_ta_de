# BÁO CÁO NGHIỆM THU P07.9.1 — HERO AUTONOMOUS SKILL DECISION & AUTO COMBAT

> **Milestone:** P07.9.1 — Hero Autonomous Skill Decision & Auto Combat  
> **Repository:** `E:\code\TLTD`  
> **Trạng thái:** HOÀN THÀNH — 100% PASS (Automated 16/16, Play Mode PASS, Regressions 100% PASS)  
> **Ngày hoàn tất:** 15/09/2026  

---

## 1. Root Cause (Nguyên nhân gốc rễ)

Trước P07.9.1, P07.9 đã hoàn thành và khóa các hệ thống thực thi kỹ năng (`SkillExecutor`, `SkillExecutionValidator`, `SkillCastState`, `CooldownManager`, `RageComponent`, `EntityStatusController`). Kỹ năng thi triển hoàn hảo khi được gọi thủ công thông qua `Hero.ExecuteSelectedSkill(slot, target)` hoặc UI click.

Tuy nhiên, trong thực tế Unity Play Mode, **Hero chỉ tự động đánh thường (Basic Attack) mà không bao giờ tự thi triển Skill hay Ultimate**. 

**Điểm đứt chính xác được xác định từ Audit:**
1. **Thiếu Production Caller:** Trong vòng lặp chiến đấu (`Hero.Update` / `BattleManager.Update`), không có bất kỳ caller nào chịu trách nhiệm đánh giá điều kiện và gọi `Hero.ExecuteSelectedSkill(slot)`.
2. **Design Gap về Priority:** Hệ thống dữ liệu kỹ năng (`SkillDefinitionSO`) chưa có cơ chế xếp hạng ưu tiên (Priority) giữa các kỹ năng đang sẵn sàng, dẫn đến việc thiếu chuẩn mực để AI đưa ra quyết định kỹ năng nào nên được ưu tiên thi triển trước.
3. **Thiếu Gameplay Authority cho Auto Battle:** Trước đây trạng thái Auto Battle chỉ nằm tạm thời trên UI (`SkillBarUI.isAutoEnabled`), vi phạm nguyên tắc "UI không nắm quyền lực Gameplay" và không thể điều khiển logic chiến đấu của Hero từ tầng logic lõi.

---

## 2. Architecture Before (Kiến trúc trước khi triển khai)

```
[Trước P07.9.1]

UI Layer:
  SkillBarUI (nắm cục bộ isAutoEnabled)
      │
      └─► (Chỉ gọi Hero.ExecuteSelectedSkill khi người chơi click chuột)

Hero Combat Loop:
  Hero.Update()
      │
      ▼
  AttackComponent.ManualTick() / BasicAttackProcessor
      │
      ▼
  Chỉ thi triển Đánh thường (Basic Attack) theo chu kỳ timer

Skill Execution Pipeline (Chỉ chạy khi có người gọi):
  Hero.ExecuteSelectedSkill() ─► SkillExecutionValidator ─► SkillExecutor ─► SkillCastState
```
*Hệ quả:* Hero không có tầng "Decision" (ra quyết định). Hệ sinh thái kỹ năng hoàn toàn tê liệt trong chế độ tự động AFK/Idle.

---

## 3. Architecture After (Kiến trúc sau khi triển khai)

Tuân thủ nghiêm ngặt nguyên tắc **"DECISION ≠ EXECUTION"**. AI chỉ đóng vai trò đánh giá và gửi Request; toàn bộ logic trừ Rage, tính Cooldown, áp hiệu ứng và kích hoạt Cast/Channel được giữ nguyên tại Execution Pipeline đã bị khóa.

```
[Sau P07.9.1]

BattleManager.Instance (Gameplay Authority: IsAutoBattle = true/false)
      │
      ▼ (EventBus.OnAutoBattleChanged thông báo SkillBarUI cập nhật visual)
Hero.Update()
      │
      ▼
HeroSkillDecisionController.TickDecision() [DECISION LAYER]
      │
      ├─► 1. Hero alive? Battle active? Auto Battle ON?
      ├─► 2. Re-entry guard: hero.IsCasting == false?
      ├─► 3. Hard CC guard: hero.CanUseSkill / CanUseUltimate?
      ├─► 4. ULTIMATE BRANCH (Preemptive):
      │      - Rage >= 100 & off cooldown & CanUseUltimate?
      │      - Ngắt Basic Attack windup -> Gửi request Ultimate!
      └─► 5. NORMAL SKILL BRANCH:
             - Lọc các slot trang bị (Skill, External 1, External 2)
             - Kiểm tra Cooldown, Rage cost, Target hợp lệ
             - Sắp xếp Candidate theo data-driven Priority (DESC)
             - Deterministic tie-breaker qua SkillId ordinal
             - Gửi request kỹ năng ưu tiên cao nhất!
                   │
                   ▼ (Pure Request Delegation)
Hero.ExecuteSelectedSkill(slot, target)
                   │
                   ▼
SkillExecutionValidator [VALIDATION GATE - LOCKED]
                   │
                   ▼
SkillExecutor [EXECUTION ENGINE - LOCKED]
                   │
                   ▼
SkillCastState [CAST TIME / CHANNEL PIPELINE - LOCKED]
```

---

## 4. Auto Battle Authority

- **Vị trí quyền lực:** Đặt chính thức tại `BattleManager.cs`:
  ```csharp
  [SerializeField] private bool isAutoBattle = true;
  public bool IsAutoBattle => isAutoBattle;
  public void SetAutoBattle(bool enabled);
  ```
- **Mặc định:** `true` (đúng chuẩn game nhàn rỗi / AFK wuxia RPG).
- **Phân tách UI:** `SkillBarUI.ToggleAuto()` bị tước toàn bộ logic gameplay, chuyển thành ủy quyền 100%:
  ```csharp
  if (BattleManager.Instance != null)
  {
      BattleManager.Instance.SetAutoBattle(!BattleManager.Instance.IsAutoBattle);
  }
  ```
- **Phản ứng sự kiện:** `EventBus.OnAutoBattleChanged` phát tín hiệu khi trạng thái thay đổi để UI đồng bộ trạng thái hiển thị (visual highlight/button text).

---

## 5. Skill Priority Authority

- **Data-Driven 100%:** Bổ sung trường `priority` vào `SkillDefinitionSO`:
  ```csharp
  [Header("Skill Decision Priority (P07.9.1)")]
  [SerializeField] private int priority = 0;
  public int Priority => priority;
  public void SetPriority(int newPriority) => priority = newPriority;
  ```
- **Không hard-code slot:** Không có bất kỳ dòng code nào quy định cứng "Slot 2 > Slot 3 > Slot 4". Thứ tự hoàn toàn do giá trị số `Priority` trong ScriptableObject quyết định.
- **Giá trị mặc định chuẩn lịch sử:**
  - **Ultimate (Slot 5):** `Priority = 100` (đồng thời nằm ở Preemptive Branch theo D5).
  - **Tuyệt Kỹ (Slot 2):** `Priority = 60`.
  - **Ngoại Công 1 (Slot 3):** `Priority = 40`.
  - **Ngoại Công 2 (Slot 4):** `Priority = 30`.
  - **Đánh thường (Slot 1):** `Priority = 0`.
- **Deterministic Tie-Breaker:** Nếu hai kỹ năng có `Priority` bằng nhau, bộ điều khiển giải quyết hòa bằng so sánh thứ tự từ điển chuỗi mã kỹ năng (`string.Compare(a.SkillId, b.SkillId, StringComparison.Ordinal) < 0`), đảm bảo 100% tính tiền định, không sinh lỗi ngẫu nhiên (non-deterministic).

---

## 6. Decision Flow (Luồng ra quyết định)

Hàm `HeroSkillDecisionController.TickDecision(float deltaTime)` thực hiện tuần tự qua 7 bước:
1. **Hero Readiness:** Hero không null, active và `Hero.IsAlive == true`.
2. **Battle State:** `BattleManager.Instance.IsBattleActive == true`.
3. **Auto Mode:** `BattleManager.Instance.IsAutoBattle == true`.
4. **Re-entry Guard:** `Hero.IsCasting == false` (nếu đang Cast hoặc Channel, lập tức thoát để bảo vệ chu kỳ cast).
5. **Target Resolution:** Kiểm tra mục tiêu hiện tại `Hero.CurrentTarget`; nếu null hoặc đã chết thì gọi `NearestEnemyTargetResolver.ResolveTarget(hero)`.
6. **Ultimate Evaluation:** Đánh giá nhánh Tuyệt Kỹ/Thần Công (xem mục 7). Nếu thỏa mãn, thi triển và kết thúc frame quyết định.
7. **Normal Skills Evaluation:** Đánh giá các kỹ năng thông thường theo danh sách ưu tiên (xem mục 8).

---

## 7. Ultimate Decision Flow (Luồng quyết định Ultimate)

1. Kiểm tra quyền hành động: `Hero.CanUseUltimate` được đánh giá thông qua thẩm quyền `EntityStatusController` hiện có (Hero.CanUseUltimate is evaluated through the existing EntityStatusController authority. Hard CC interruption rules remain governed by Stun/Freeze according to P07.9).
2. Lấy định nghĩa: `mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate)`.
3. Cooldown check: `!CooldownManager.IsOnCooldown(ultDef.SkillId)`.
4. Rage check: `Hero.Rage.CurrentRage >= ultDef.RageCost` (theo cấu hình của kỹ năng).
5. Target check: Mục tiêu sống và hợp lệ.
6. **Preemptive Basic Attack Windup Reset:** Nếu Hero đang trong giai đoạn vung tay đánh thường (`Hero.Attack.AttackTimer > 0`), Ultimate ngắt ngay lập tức đòn đánh thường bằng `Hero.Attack.ResetAttackTimer()` để nhường quyền ưu tiên tối thượng cho Ultimate.
7. Gửi yêu cầu: `Hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, target)`.

---

## 8. Normal Skill Decision Flow (Luồng kỹ năng thông thường)

1. Duyệt qua các slot kỹ năng trang bị chủ động (`SkillSlotType.Skill`, `ExternalSkill1`, `ExternalSkill2`).
2. Với mỗi slot:
   - Kiểm tra kỹ năng được gán và `!def.IsPassive`.
   - Kiểm tra CC: `Hero.CanUseSkill == true`.
   - Kiểm tra Cooldown: `!CooldownManager.IsOnCooldown(def.SkillId)`.
   - Kiểm tra Rage: `Hero.Rage.CurrentRage >= def.RageCost`.
   - Kiểm tra Target: Mục tiêu sống và hợp lệ.
3. Tập hợp danh sách các ứng viên sẵn sàng (`SkillCandidate`).
4. Sắp xếp danh sách giảm dần theo `Priority` (`CandidateA.Priority > CandidateB.Priority`).
5. Nếu hòa điểm Priority: so sánh `SkillId` theo Ordinal.
6. Lấy kỹ năng xếp đầu bảng (`candidates[0]`) và gọi `Hero.ExecuteSelectedSkill(candidates[0].Slot, target)`.

---

## 9. Manual Skill Flow (Luồng kỹ năng thủ công)

- Khi `IsAutoBattle == false`:
  - `HeroSkillDecisionController.TickDecision()` lập tức trả về `false`, Hero không tự động tung bất kỳ kỹ năng hay Ultimate nào.
  - Hero vẫn tự động đánh thường nếu mục tiêu trong tầm đánh (giữ đúng bản chất Auto Attack của game idle).
  - Khi người chơi click vào slot trên `SkillBarUI`: UI gọi trực tiếp `Hero.ExecuteSelectedSkill(slot, target)`.
  - Toàn bộ pipeline kiểm tra hợp lệ (`SkillExecutionValidator`) và thực thi (`SkillExecutor`) xử lý yêu cầu thủ công trơn tru.

---

## 10. Cast/Channel Integration (Tích hợp Cast / Vận khí)

1. **Khóa liên động đánh thường (Basic Attack Interlock):**
   - Trong `Entity.cs`, cập nhật thuộc tính:
     ```csharp
     public virtual bool CanBasicAttack => (StatusController == null || StatusController.CanBasicAttack) && !IsCasting;
     ```
   - Đảm bảo khi Hero đang trong trạng thái Vận khí (Channeling) hoặc Niệm chú (Casting), Hero không thể tự động vung đòn đánh thường xen ngang.
2. **Re-entry Guard:**
   - Trong suốt thời gian `Hero.IsCasting == true`, `HeroSkillDecisionController` từ chối kích hoạt bất kỳ kỹ năng mới nào, ngăn ngừa hiện tượng spam hay duplicate cast request.
3. **Gián đoạn (Interrupts):**
   - Hard CC (Stun / Freeze) gọi `Entity.InterruptCurrentAction()`, ngay lập tức hủy CastState/ChannelState, xóa thanh CastBarUI và không hoàn trả Rage.
   - Root (Trói chân) và Ordinary Damage (Sát thương thông thường) không ngắt Cast/Channel.

---

## 11. Files Changed (Các file chỉnh sửa / bổ sung)

| File | Hành động | Mục đích |
|---|---|---|
| `Assets/_Game/Data/SkillDefinitionSO.cs` | **MODIFY** | Thêm trường `priority`, getter `Priority`, hàm `SetPriority(int)` và cập nhật constructor/initialization |
| `Assets/_Game/Core/EventBus.cs` | **MODIFY** | Bổ sung sự kiện `OnAutoBattleChanged` và phương thức `RaiseAutoBattleChanged(bool)` |
| `Assets/_Game/Core/BattleManager.cs` | **MODIFY** | Nắm giữ thẩm quyền `isAutoBattle`, getter `IsAutoBattle`, hàm `SetAutoBattle(bool)` |
| `Assets/_Game/UI/HUD/SkillBarUI.cs` | **MODIFY** | Bỏ cờ auto nội bộ, ủy quyền hoàn toàn cho `BattleManager.Instance.SetAutoBattle`, lắng nghe sự kiện |
| `Assets/_Game/Entities/Entity.cs` | **MODIFY** | Interlock `CanBasicAttack` với `!IsCasting` mà không sửa `AttackComponent.cs` hay `BasicAttackProcessor.cs` |
| `Assets/_Game/Entities/Hero.cs` | **MODIFY** | Bổ sung lazy property và khởi tạo `Hero.SkillDecisionController` |
| `Assets/_Game/Combat/HeroSkillDecisionController.cs` | **NEW** | Lớp AI ra quyết định kỹ năng tự động độc lập 100% |
| `Assets/_Game/Editor/Prototype01SceneBuilder.cs` | **MODIFY** | Cấu hình data-driven priority mặc định cho kỹ năng và gắn component lên prefab/scene |
| `Assets/_Game/Editor/Prototype01PlayTestRunner_P07_9_1.cs` | **NEW** | Bộ kiểm thử tự động 16 kịch bản cho P07.9.1 |

---

## 12. Files Not Changed (Các file bị cấm sửa / giữ nguyên vẹn 100%)

Tuân thủ tuyệt đối ranh giới kiến trúc đã cam kết:
- `Assets/_Game/Combat/SkillExecutor.cs` — **LOCKED** (Không bị biến thành AI)
- `Assets/_Game/Combat/SkillExecutionValidator.cs` — **LOCKED**
- `Assets/_Game/Combat/SkillCastState.cs` — **LOCKED**
- `Assets/_Game/Combat/CooldownManager.cs` — **LOCKED**
- `Assets/_Game/Entities/EntityStatusController.cs` — **LOCKED**
- `Assets/_Game/Entities/Components/RageComponent.cs` — **LOCKED**
- `Assets/_Game/Combat/DamageCalculator.cs` — **LOCKED**
- `Assets/_Game/Entities/Components/HealthComponent.cs` — **LOCKED**
- `Assets/_Game/Combat/BasicAttackProcessor.cs` — **LOCKED**
- `Assets/_Game/Entities/Components/AttackComponent.cs` — **LOCKED**

---

## 13. Automated Test Results (Kết quả kiểm thử tự động P07.9.1)

Chạy thực tế qua Unity batchmode (`test_p07_9_1.log`): **16/16 PASSED (100%)**

| Mã Test | Tên Kịch Bản | Trạng Thái | Chi Tiết Xác Thực |
|---|---|---|---|
| **TEST 01** | Auto ON -> Normal Skill Auto Cast | **PASS** | Tự động đánh giá và thi triển Tuyệt Kỹ vào mục tiêu khi off-cooldown |
| **TEST 02** | Multiple Ready -> Highest Priority | **PASS** | Chọn kỹ năng có Priority cao hơn; kiểm tra thay đổi Priority động & tie-breaker |
| **TEST 03** | Auto ON + Ult Ready + Rage -> Auto Ult | **PASS** | Tự động kích hoạt Ultimate khi đủ 100 Rage; ngắt windup đòn đánh thường |
| **TEST 04** | Auto OFF -> Hero Does Not Auto Cast | **PASS** | Khi Auto OFF, Hero hoàn toàn không tự dùng kỹ năng trong nhiều tick |
| **TEST 05** | Auto OFF -> Manual Skill Still Casts | **PASS** | Khi Auto OFF, người chơi click thủ công kỹ năng vẫn thực thi và gây sát thương |
| **TEST 06** | Cast Time -> CastState Active | **PASS** | Kỹ năng có Cast Time chuyển sang trạng thái CastState thành công |
| **TEST 07** | Channel -> Channel Active | **PASS** | Kỹ năng Vận khí gây sát thương đều đặn qua các tick cho tới khi kết thúc |
| **TEST 08** | Stun CC -> Cast Interrupt | **PASS** | Hiệu ứng Choáng ngắt ngay lập tức kỹ năng đang niệm |
| **TEST 09** | Freeze CC -> Cast Interrupt | **PASS** | Hiệu ứng Đóng băng ngắt ngay lập tức kỹ năng đang niệm |
| **TEST 10** | Root CC -> Cast Continues | **PASS** | Hiệu ứng Trói chân không làm gián đoạn kỹ năng đang niệm |
| **TEST 11** | Ordinary Damage -> Cast Continues | **PASS** | Sát thương nhận vào thông thường không làm gián đoạn kỹ năng |
| **TEST 12** | Insufficient Rage -> Ult Blocked | **PASS** | Rage < 100 ngăn chặn Hero tự dùng Ultimate |
| **TEST 13** | Cooldown Active -> Skill Blocked | **PASS** | Kỹ năng đang trong thời gian hồi chiêu không bị gọi lặp lại |
| **TEST 14** | Validation Failure -> No Resource Mut | **PASS** | Khi mục tiêu chết/không hợp lệ, không bị trừ Rage hay kích hoạt Cooldown |
| **TEST 15** | Re-entry Guard -> No Duplicate Cast | **PASS** | Không gửi thêm request khi Hero đang trong quá trình thi triển |
| **TEST PM** | Play Mode Real Battle Verification | **PASS** | Toàn bộ chu kỳ chiến đấu thời gian thực trong scene Prototype01 hoạt động chính xác |

---

## 14. Play Mode Evidence (Bằng chứng thực nghiệm Play Mode)

Thực hiện trong scene thực `Assets/_Game/Scenes/Prototype01.unity`:
- **Phase A (Tự động dùng Skill ưu tiên):** Hero trang bị Tuyệt Kỹ (Priority 60) và Ngoại Công 1 (Priority 40). Khi trận đấu bắt đầu và cả hai đều off-cooldown, AI chọn chính xác kỹ năng Priority 60 để thi triển.
- **Phase B (Tự động dùng Ultimate):** Khi nạp đầy 100 Rage, AI tự động kích hoạt Ultimate (Slot 5), ngắt ngang thời gian chờ đánh thường.
- **Phase C (Tắt Auto Battle):** Chuyển `IsAutoBattle = false`, Hero lập tức dừng tự động dùng skill.
- **Phase D (Kích hoạt thủ công khi Auto OFF):** Gửi lệnh gọi kỹ năng thủ công, kỹ năng thi triển thành công và trừ tài nguyên hợp lệ.

---

## 15. P07.8 Regression (Hồi quy Shield & Barrier)

Chạy bộ kiểm thử toàn diện `RunAllPrototype07_8Tests`:
- **Kết quả:** **55/55 PASSED (100%)**
- **Xác nhận:** Toàn bộ cơ chế Khiên chắn, Hộ thuẫn, Hấp thụ sát thương, Barrier CC Shield của P07.8 không bị ảnh hưởng.

---

## 16. P07.9 Regression (Hồi quy Advanced Skill Casting & CC Interlocking)

Chạy các bộ kiểm thử chuyên biệt:
- `RunAllPrototype07_9_Phase5_3_Tests` (Cast Bar & Visual Presentation Scenarios A-H): **36/36 PASSED (100%)**
- `RunAllPrototype07_9_Risk04_Tests` (Ultimate CC & Rage Gate Scenarios A-F): **18/18 PASSED (100%)**
- **Xác nhận:** Pipeline Cast Time, Channeling, Stun/Freeze Interrupt, Root pass-through, Ordinary Damage pass-through hoạt động chuẩn xác 100%.

---

## 17. Master Regression & Remaining Issues

### Master Regression (P01 -> P07.8)
- Chạy phương thức `WuxiaGame.Editor.Prototype01PlayTestRunner.RunMasterRegressionSuite`:
- **Kết quả:** **`[FULL MASTER REGRESSION: 100% PASS]`**
- Toàn bộ các mốc lịch sử (Stats, Combat Foundation, Drop/Loot, Progression, Titles, Equipment, Affixes, Mind Methods, Buffs/Debuffs, CC, Shield/Barrier) đều xanh 100%.

### Remaining Issues
- **Không có bất kỳ issue tồn đọng nào (Zero remaining issues).**
- Ranh giới giữa Quyết định (Decision) và Thực thi (Execution) được bảo vệ tuyệt đối.
- Hệ thống sẵn sàng bàn giao và khóa mốc P07.9.1.
