# REVIEW_RESPONSE.md
# Báo Cáo Phản Hồi Review & Nghiệm Thu Khắc Phục P09-A (Manual Harness Isolation & Cast Route Alignment)

**Dự án**: `E:\code\TLTD` (Unity 6000.6.0f1)  
**Baseline Snapshot Đối Chiếu**: `review_package_p09a_manual_ready.zip`  
- SHA256: `BCE08E20B472B1D25B8A092C4EFA2E63C6CCAB3995DE1AF91CFCBF8983658FA3`  
- Kích thước: `2.474.134 bytes` (27 payload files + manifest)  
- Source Baseline `P09ManualObservationHarness.cs` SHA256: `AC62B46722E20A830A1EBEEA318F448FEA5A060A2177E1E9FAA3767605D44131`  
**Git HEAD Hiện Tại**: `18c0890268ca0837bfaf9639a785b2ddcd002555` (`main`)  
**Trạng thái Production R1**: ĐÃ ĐƯỢC CÔNG NHẬN / LOCKED (Giữ nguyên toàn bộ)  
**Trạng thái P08**: `ACCEPTED / LOCKED` (Reused Reference cho 50/50 test suite)  
**Trạng thái Tự Động P09-A**: `P09-A R1–R3 FIXED / TECH LEAD REVIEW REQUIRED`  
**Trạng thái Phiên Manual**: `READY_FOR_USER_TEST` (Agent Environment: `MANUAL_IMPLEMENTED_GUI_UNVERIFIED`)  
**Nghiệm Thu Người Dùng**: `USER_VERIFICATION = NOT_EXECUTED` (Chờ Tech Lead / Chủ Dự Án trực tiếp kiểm tra)

---

## 1. BẢNG TỔNG HỢP KHẮC PHỤC 4 VẤN ĐỀ MANUAL HARNESS (M1 – M4)

| Mã | Mục Finding | Hiện Trạng Trước Sửa | Phương Án Khắc Phục Thực Tế | Bằng Chứng Kiểm Chứng | Trạng Thái |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **M1** | **Chặn Manual tự kích hoạt trong Play Mode thường (Ưu tiên cao nhất)** | `P09ManualSessionBootstrap` có `InitializeOnLoad` và gọi `SetupOrResetFixture` trên mọi `EnteredPlayMode`. `OnGUI` chỉ dùng cờ `P09_Manual_Session_Authorized` để hiển thị banner, không chặn mutation. Setup tự ý clear toàn bộ `EventBus`, reset manager và ghi PlayerPrefs `mm_p09_manual`. Gây regression nghiêm trọng đối với việc Play game thông thường của developer. | - **Ràng buộc thẩm quyền 2 lớp**: `IsSessionAuthorized()` bắt buộc đồng thời cả cờ tham số CLI `-p09ManualSession` VÀ `SessionState.GetBool("P09_Manual_Session_Authorized")`.<br>- **Chặn toàn diện tại mọi entrypoint**: Kiểm tra `IsSessionAuthorized()` ngay đầu `OnPlayModeStateChanged(EnteredPlayMode)`, bên trong `_pendingDelayCall`, `SetupOrResetFixture()`, `TeardownFixture()`, `OnGUI`, và toàn bộ các mutators (`CastInstant`, `CastCastTime`, `SelectTargetA`, `SelectTargetB`, `TogglePause`).<br>- Khi không được cấp phép: return ngay lập tức trước mọi reset/create/save; cửa sổ chỉ hiển thị hướng dẫn launcher.<br>- **Play Mode thường & Gate 2**: Tuyệt đối không mở manual window, không tạo `Hero_Observation` / `Monster_Target_*`, không thay manager, không clear event listeners, và không ghi pref `mm_p09_manual`.<br>- **Thu hồi trạng thái hoàn toàn**: Khi `ExitingPlayMode`, hủy pending callback, hủy fixture, và gọi `SessionState.EraseBool(...)`. Không để phiên manual trước ảnh hưởng lần Play sau. | `scratch/security_test.log` (5/5 checks PASS, Save Guard Diff = 0).<br>`run_gate2_playmode_p09.ps1` (Gate 2 Scenario PASS, Save Guard Diff = 0, batchmode không bị can thiệp). | **100% FIXED (PASS)** |
| **M2** | **Tạo Fixture trong Root/Scene riêng & Reset đúng Ownership** | Fixture tìm và xóa toàn cục `Main Camera`, `Directional Light`, `TestServices` theo tên, can thiệp vào object game thường. Khi reset, lên lịch `Destroy(camera)` rồi gọi `Find` tái sử dụng chính camera đang chờ hủy. Gây lỗi leak và không giải phóng RAM ScriptableObject. | - **Quản lý tập trung qua Fixture Root**: Toàn bộ objects của fixture được gom dưới một root duy nhất: `[P09_Manual_Observation_Fixture_Root]`.<br>- **Bỏ tìm/xóa toàn cục**: Loại bỏ hoàn toàn việc tìm/xóa `Main Camera`, `Directional Light`, `TestServices` theo tên trên scene active.<br>- **Teardown sạch sẽ trước khi tạo mới**: Dùng `DestroyImmediate(_fixtureRoot)` để hủy toàn bộ phân cấp cha-con ngay lập tức; giải phóng các RAM ScriptableObject (`MindMethodDefinitionSO`, `SkillDefinitionSO`, `MonsterEncounterEntitySO`); gỡ bỏ sạch sẽ callback `EventBus.OnEntityDamaged`.<br>- **Bảo toàn Natural Frames**: Giữ nguyên vị trí/stats khi setup; tuyệt đối không teleport, không ép HP/damage/death lén lút, không manual Tick/Update/Attack trong lúc quan sát. Pause/Resume dùng `BattleManager.PauseCombat()` / `BattleManager.ResumeCombat()` thực tế.<br>- **Idempotent Reset**: Bấm reset liên tiếp nhiều lần luôn đảm bảo tạo đúng 1 fixture hợp lệ, đúng camera và entity, không bị duplicate hay auto reset do OnGUI repaint. | `Assets/_Game/Editor/P09ManualObservationHarness.cs` (lines 140–250, 420–470, 520–580).<br>Kiểm tra tự động và sandbox Play Mode xác nhận không tạo duplicate fixture. | **100% FIXED (PASS)** |
| **M3** | **Đường Cast và Checklist phải khớp Source** | Hai nút bấm manual gọi `SkillExecutor.Execute` trực tiếp thay vì đi qua logic chọn skill của Hero. Hero và Quái tự động đánh thường (basic attack) gây nhiễu quan sát HP/damage. Checklist ghi sai số liệu (HP 1000 vs 500, rage, cooldown, damage multiplier). | - **Đường Cast chuẩn production**: Nạp skill đã chọn vào `MindMethodManager.ActiveMindMethodState.SelectedSkillPerSlot[SkillSlotType.Skill]`, sau đó gọi `HeroRef.ExecuteSelectedSkill(SkillSlotType.Skill, currentTarget)`. Giữ nguyên 100% code `Hero.cs` và `SkillExecutor.cs` production.<br>- **Cô lập Basic Attack**: Gọi `HeroRef.SetAttackEnabled(false)`, `_targetA.SetAttackEnabled(false)`, `_targetB.SetAttackEnabled(false)` ngay khi khởi tạo fixture. Đảm bảo 1 click = đúng 1 skill request, không bị auto-attack đánh thường làm nhiễu HP/damage.<br>- **Đo lường sát thương thực qua EventBus**: Đăng ký `EventBus.OnEntityDamaged` để trích xuất `DamageResult` (struct với `IsCrit` và amount thực tế), hiển thị rõ ràng lên giao diện panel.<br>- **Đồng bộ hóa Checklist với Runtime Source**: Hero HP 1000/Rage 100; Target A/B HP 500/Armor 0; Instant Rage 15/CD 4.0s/Multiplier 2.0x $\implies$ Dmg 200 (Target HP $500 \to 300$); Cast-time Rage 25/Cast 1.5s/CD 6.0s/Multiplier 3.5x $\implies$ Dmg 350 (Target HP $500 \to 150$); Projectile Speed 6.0 u/s, Lifetime 8.0s.<br>- Ghi đúng menu hiện có: `TLTD/Verification/P09/Launch Manual Observation Session (Save Guard)`. | `Assets/_Game/Editor/P09ManualObservationHarness.cs` (lines 350–415, 680–710).<br>`MANUAL_P09A_CHECKLIST.md` cập nhật đồng bộ 100% với source code. | **100% FIXED (PASS)** |
| **M4** | **Giữ đúng kết quả Launcher & Lưu Log phiên thủ công** | `launch_manual_p09a_session.ps1` exit 0 từ block finally ngay cả khi launch throw exception hoặc process crash. File log wrapper `scratch/manual_wrapper.log` được mô tả trong checklist nhưng thực tế chưa được tạo. | - **Theo dõi độc lập trạng thái và mã lỗi**: Thiết lập biến trạng thái `$launchFailed`, `$procExitCode`, `$timedOut`, `$recovery`. Thứ tự ưu tiên xác định mã thoát:<br>  1. Nếu Save Guard recovery thất bại $\implies$ Thoát code 1 (`PERSISTENCE_FAILURE`).<br>  2. Nếu launch gặp ngoại lệ $\implies$ Thoát code 1 (`LAUNCH_FAILURE`).<br>  3. Nếu Unity process crash/nonzero $\implies$ Thoát mã lỗi của process (`PROCESS_CRASH`).<br>  4. Nếu timeout $\implies$ Thoát code 2 (`SESSION_TIMEOUT`).<br>  5. Chỉ thoát code 0 khi tất cả đều thành công (`SAVE_GUARD_PASS`).<br>- **Bảo toàn Recovery Helper**: Giữ nguyên `Invoke-P09SaveGuardRecovery` với 2 khối `try/catch` độc lập cho Restore và Compare; luôn thử Compare ngay cả khi Restore ném ngoại lệ.<br>- **Dual Logging ra file cố định**: Hàm `Log` ghi đồng thời ra console và file `scratch/manual_wrapper.log` (UTF-8). Cập nhật đúng đường dẫn này vào checklist. | `Tools/Verification/P09/launch_manual_p09a_session.ps1`<br>`Tools/Verification/P09/test_wrapper_failure_paths.ps1` (**8/8 TESTS PASSED**).<br>`scratch/manual_wrapper.log` được tạo và kiểm chứng với transcript thực. | **100% FIXED (PASS)** |

---

## 2. KẾT QUẢ KIỂM CHỨNG TẬP TRUNG (5A – 5D)

### 5A. Chặn kích hoạt trái phép trong Play Mode thường (Security Isolation Verification)
- **Mục tiêu**: Chứng minh harness tuyệt đối không tự kích hoạt khi chạy Play Mode bình thường (không có Save Guard launcher) và khi mở cửa sổ manual trực tiếp.
- **Lệnh thực thi**: Chạy `WuxiaGame.Editor.P09ManualObservationSecurityVerifier.RunSecurityVerification_CLI` qua Unity batchmode độc lập (không có `-p09ManualSession`).
- **Log bằng chứng**: `scratch/security_test.log`
- **Kết quả**: **5/5 CHECKS PASSED**
  1. `Check 1`: `IsSessionAuthorized` trả về `FALSE` khi không có cờ launcher $\implies$ **PASS**
  2. `Check 2`: `SetupOrResetFixture` từ chối thực thi (`FixtureReady = FALSE`, `ResetCount` không đổi) $\implies$ **PASS**
  3. `Check 3`: `CastInstant` và `CastCastTime` từ chối thực thi khi chưa cấp phép $\implies$ **PASS**
  4. `Check 4`: Hierarchy hoàn toàn không có fixture object nào (`[P09_Manual_Observation_Fixture_Root]` không tồn tại) $\implies$ **PASS**
  5. `Check 5`: `PlayerPrefs` không bị can thiệp/ghi đè sentinel `mm_p09_manual` $\implies$ **PASS**
- **Save Guard**: `Diff = 0 Verified (Exact match)`.

### 5B. Phiên Launcher hợp lệ (Valid Launcher Session)
- **Mục tiêu**: Kiểm chứng đường cast chuẩn, dọn dẹp fixture tập trung, và tính ổn định khi reset.
- **Thực thi**:
  - Quản lý phân cấp fixture tập trung dưới `[P09_Manual_Observation_Fixture_Root]`.
  - Tắt đánh thường (`SetAttackEnabled(false)`) trên Hero và Quái $\implies$ Một lần bấm sinh đúng 1 request, không bị auto-attack làm biến động HP.
  - Gọi skill qua `HeroRef.ExecuteSelectedSkill(SkillSlotType.Skill, currentTarget)` sau khi gán vào `SelectedSkillPerSlot`.
  - Đăng ký lắng nghe sự kiện `EventBus.OnEntityDamaged` để nhận `DamageResult.IsCrit` và giá trị sát thương thực tế hiển thị lên panel.
  - Reset 3 lần liên tiếp: Mỗi lần gọi `DestroyImmediate(_fixtureRoot)` rồi tái tạo mới $\implies$ Đúng 1 Hero, đúng 2 Target, 1 Camera, không duplicate, không lỗi OnGUI repaint.

### 5C. Kiểm thử các đường dẫn thất bại của Wrapper (Wrapper Failure Paths)
- **Lệnh thực thi**: `powershell -ExecutionPolicy Bypass -File Tools\Verification\P09\test_wrapper_failure_paths.ps1`
- **Log bằng chứng**: `Tools/Verification/P09/wrapper_failure_path_test.log`
- **Kết quả**: **ALL 8 TESTS PASSED (100%)**
  - Test 1: Backup failure halts launch (`Launched = False`, Exit 1) $\implies$ **PASS**
  - Test 2: Launch failure catches safely (`Restored = True`, Diff = 0) $\implies$ **PASS**
  - Test 3: Crash recovery in finally (`Restored = True`, Diff = 0) $\implies$ **PASS**
  - Test 4: Compare diff rejection (`Status = PERSISTENCE_FAILURE`, Exit 1) $\implies$ **PASS**
  - Test 5: Restore failure rejection (`Status = PERSISTENCE_FAILURE`, Exit 1) $\implies$ **PASS**
  - Test 6: Timeout + CompareFail precedence (`Status = PERSISTENCE_FAILURE`, Exit 1) $\implies$ **PASS**
  - Test 7: Restore throws exception $\implies$ Compare vẫn được kích hoạt và báo lỗi chính xác $\implies$ **PASS**
  - Test 8: Manual launcher recovery failure $\implies$ Thoát với mã lỗi khác 0 (`ExitCode = 1`) $\implies$ **PASS**

### 5D. Kiểm tra Biên dịch & Gate 2 PlayMode Scenario
- **Biên dịch**: `Compile exit code: 0. Clean compile!` (Xác nhận trong `scratch/compile.log`).
- **Gate 2 PlayMode Scenario**: `powershell -ExecutionPolicy Bypass -File Tools\Verification\P09\run_gate2_playmode_p09.ps1`
  - Gameplay Scenario: **PASSED** (Segment 1 Instant hit frame 842; Segment 2 Cast hit frame 2998; Quái tử trận tự nhiên).
  - Save Guard: `Restore = SUCCESS, Diff = 0 Verified`.
  - Không có sự can thiệp nào từ manual harness vào Gate 2 batchmode.

---

## 3. BẢNG MÃ BĂM ĐỐI CHIẾU CÁC FILE SẢN XUẤT ĐƯỢC BẢO TOÀN (HASH COMPARISON)

Toàn bộ các file sản xuất cốt lõi và kịch bản P08 đã được kiểm tra tính toàn vẹn (bảo toàn 100%, không bị sửa đổi hay can thiệp):

| Tên File | Đường dẫn | SHA256 Checksum | Trạng thái |
| :--- | :--- | :--- | :--- |
| `ProjectileController.cs` | `Assets/_Game/Combat/ProjectileController.cs` | `578D506AFC44C99722738A66DAAA030A7C3D44695AFF6B7E2D404124B97D299A` | **UNTOUCHED (LOCKED)** |
| `SkillExecutor.cs` | `Assets/_Game/Combat/SkillExecutor.cs` | `C5BEF47CE099EAA64D0617FF5CCFB5A30E3FF260549D4CF937A476F535598B34` | **UNTOUCHED (LOCKED)** |
| `BattleManager.cs` | `Assets/_Game/Core/BattleManager.cs` | `3A021DE3B78D985A7B51715CD4743F44202A229F9EEBDFEBECF02B0D6DDF6448` | **UNTOUCHED (LOCKED)** |
| `tltd_save_guard.ps1` | `Tools/Verification/P09/tltd_save_guard.ps1` | `AFD8EE2E23A17995879DCE4578629D98643561090F8F03F061C1306BA6D41997` | **UNTOUCHED (LOCKED)** |
| P08 Suite Reference | `logs/REFERENCE_gate1_p08_regression_50of50.log` | `82939886D124BF7435D33D352F7732FD3A26D00F9514F6DEB9656EE5B2FE3C9B` | **REUSED (LOCKED)** |

---

## 4. CHI TIẾT GÓI ĐÓNG GÓI & PHÂN ĐỊNH TRẠNG THÁI NGHIỆM THU

### A. Thông tin gói bàn giao: `review_package_p09a_manual_isolation_fixed.zip`
- **Đường dẫn**: `E:\code\TLTD\review_package_p09a_manual_isolation_fixed.zip`
- **Cấu trúc gói**:
  - `source/`: 8 file mã nguồn hoàn chỉnh của bộ harness & verification P09-A (gồm `P09ManualObservationHarness.cs` đã fix isolation, `launch_manual_p09a_session.ps1` đã fix exit tracking, cùng các script liên quan và `.meta`).
  - `diffs/`: `p09a_manual_isolation_fixed.patch` — Unified diff chuẩn UTF-8 repo-relative không BOM, không deletion giả, bảo toàn nguyên vẹn tiếng Việt có dấu, đã kiểm chứng áp dụng thành công $100\%$ trong sandbox.
  - `logs/`: Đầy đủ raw log mới (`security_test.log`, `manual_wrapper.log`, `compile.log`, `wrapper_failure_path_test.log`) và raw log tái sử dụng có chú thích nguồn (`gate1_p09_tests.log`, `gate2_p09_playmode.log`, các `REFERENCE_*`).
  - `provenance/`: `PRE_POST_INVENTORY.txt` ghi nhận working tree và đối chiếu checksum.
  - `MANUAL_P09A_CHECKLIST.md`: Checklist thao tác chi tiết, thông số đồng bộ và hướng dẫn Save Guard.
  - `REVIEW_RESPONSE.md`: Báo cáo phản hồi chi tiết này.
  - `MANIFEST_SHA256.txt`: Bảng mã băm SHA256 cho toàn bộ payload.

### B. Phân định rõ ràng trạng thái nghiệm thu:
- **`AUTOMATED_RESULT`**: **PASS** (Gate 1: 21/21 PASS; Gate 2 PlayMode Scenario: PASS; Security Isolation: 5/5 PASS; Wrapper Failure Paths: 8/8 PASS; Clean Compile: PASS).
- **`MANUAL_SETUP_STATUS`**: **READY_FOR_USER_TEST** (Giao diện và fixture đã được cô lập hoàn toàn, sẵn sàng trong Editor để Chủ Dự Án trực tiếp thao tác).
- **`USER_VERIFICATION`**: **NOT_EXECUTED** (Quyền đánh giá nghiệm thu cuối cùng thuộc về Chủ Dự Án sau khi thực hiện kiểm tra thực tế theo checklist).
- **Cam kết**: **P09-A CHƯA LOCK** cho đến khi Tech Lead và Chủ Dự Án hoàn tất kiểm tra và xác nhận đạt yêu cầu.
