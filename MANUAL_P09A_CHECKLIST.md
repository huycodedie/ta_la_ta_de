# MANUAL_P09A_CHECKLIST.md
# Hướng Dẫn Kiểm Tra Trực Tiếp & Checklist Nghiệm Thu P09-A (Dành Cho Chủ Dự Án)

> **TRẠNG THÁI KIỂM THỬ THỦ CÔNG**: `READY_FOR_USER_TEST`  
> **LƯU Ý QUAN TRỌNG**: Antigravity **KHÔNG** tự ý đánh dấu `USER VERIFIED` hoặc `P09-A LOCKED`. Tài liệu này cung cấp đầy đủ thông tin, kịch bản, scene, skill, thao tác hiện có và công cụ bảo vệ Save Guard để Chủ Dự Án trực tiếp kiểm tra và nghiệm thu.

---

## 1. THÔNG BÁO MINH BẠCH VỀ PHẠM VI DỮ LIỆU (FIXTURE RAM vs ASSET SẢN XUẤT)

> [!WARNING]
> **Vị trí hiện tại của kỹ năng Projectile**:
> - Cơ chế bay đạn (Projectile Delivery Foundation) trong giai đoạn P09-A đã được tích hợp hoàn chỉnh vào engine runtime (`ProjectileController`, `SkillExecutor`, `EffectResolver`, `BattleManager`).
> - **Tuy nhiên, toàn bộ 61 file ScriptableObject kỹ năng sản xuất trong `Assets/_Game/Data/Skills/` hiện tại vẫn đang để `IsProjectile = false` (kỹ năng tức thời / cận chiến)**.
> - Các kỹ năng đạn bay trong đợt kiểm thử P09-A được cấu hình qua **Transient In-Memory Fixtures (RAM)**:
>   1. `p09_pm_instant` (Instant Projectile: Tốc độ 10, Tầm bay 5s, Cooldown 3.0s, Nộ 30).
>   2. `p09_pm_cast` (Cast-Time Projectile: Vận khí 0.3s, Tốc độ 10, Tầm bay 5s, Cooldown 3.0s, Nộ 25).
> - Antigravity **không tự ý sửa các asset sản xuất** để phục vụ việc demo UI khi chưa có chỉ đạo từ Tech Lead. Do đó, trong gameplay thông thường ở build hiện tại, các nút bấm trên UI đang thi triển các kỹ năng tức thời sản xuất có sẵn.
> - Để quan sát projectile tự nhiên trong Game View với đầy đủ chu kỳ khung hình (natural frames), Chủ Dự Án có thể sử dụng phiên quan sát an toàn được mô tả ở Mục 2 dưới đây.

---

## 2. QUY TRÌNH MỞ VÀ KẾT THÚC PHIÊN KIỂM TRA CÓ SAVE GUARD

Để đảm bảo tuyệt đối không làm bẩn hoặc sai lệch PlayerPrefs / file lưu trữ của dự án trong quá trình mở Editor thủ công:

### Bước 2.1: Chuẩn bị trước khi khởi chạy
1. Nếu bạn đang mở sẵn Unity Editor cho project `E:\code\TLTD`, vui lòng **LƯU CÔNG VIỆC** và **ĐÓNG HOÀN TOÀN** Unity Editor.
2. Việc này giúp tránh xung đột ghi đè registry giữa hai tiến trình Unity chạy song song.

### Bước 2.2: Khởi chạy phiên kiểm tra an toàn (Save Guard Launcher)
Mở PowerShell tại thư mục gốc dự án `E:\code\TLTD` và chạy lệnh duy nhất:
```powershell
powershell -ExecutionPolicy Bypass -File Tools\Verification\P09\launch_manual_p09a_session.ps1
```

**Cơ chế bảo vệ tự động của Launcher:**
- Tự động kiểm tra tiến trình Unity đang chạy (từ chối mở nếu có Editor bên ngoài chưa đóng).
- Tự động kiểm tra nhật ký lưu trữ (`CheckInterruptedJournal`).
- Thực hiện sao lưu bền vững toàn bộ PlayerPrefs/Registry sang `scratch\.save_backup_manual_session`.
- Khởi động Unity Editor tương tác đầy đủ với Game View.

### Bước 2.3: Kết thúc phiên kiểm tra và đối chiếu Save Guard
1. Khi hoàn thành kiểm tra, đóng cửa sổ Unity Editor bình thường (`Alt + F4` hoặc `File -> Exit`).
2. PowerShell Launcher trong khối `finally` sẽ tự động:
   - Phục hồi dữ liệu (`Restore`) về trạng thái nguyên bản trước khi mở.
   - So sánh đối chiếu (`Compare`) đảm bảo `Diff = 0` (Exact match).
3. Kết quả xác nhận hiển thị trên màn hình:
   `[SAVE GUARD PASS] State 100% Restored. Diff = 0 Verified (Exact match).`

---

## 3. CHECKLIST KIỂM TRA CÁC TRƯỜNG HỢP (SCENARIOS)

| STT | Trường hợp kiểm tra | Scene / Đối tượng | Thao tác thực hiện | Kết quả mong đợi | Trạng thái tự động | Trạng thái thủ công |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **P09-01** | **Kỹ năng thường (Tức thời)** | `Assets/_Game/Scenes/Prototype01.unity`<br>Hero: `Hero_Wuxia`<br>Skill: Kỹ năng tức thời có sẵn | Click nút Skill Slot 1 trên Combat HUD khi đủ Nộ | Trừ Nộ ngay lập tức; Gây sát thương ngay tại frame cast; Bắt đầu cooldown ngay. Không sinh đạn bay thừa. | **PASS** (T01) | `READY_FOR_USER_TEST` |
| **P09-02** | **Kỹ năng đạn bay tức thời (Instant Projectile)** | Fixture RAM: `p09_pm_instant`<br>Target: Quái đầu tiên trong Encounter | Thi triển qua `Hero.ExecuteSelectedSkill` hoặc chạy Gate 2 | Trừ 30 Nộ tại lúc phóng; Cooldown bắt đầu ngay lúc phóng (3.0s); Đạn bay về phía quái mục tiêu; Trước va chạm Target không nhận sát thương; Khi đạn chạm, Target nhận chính xác 1 lần sát thương; Cooldown giảm tự nhiên theo thời gian bay và KHÔNG bị reset lại lúc va chạm. | **PASS** (T02, Gate 2) | `READY_FOR_USER_TEST` (Fixture RAM) |
| **P09-03** | **Kỹ năng đạn bay có thời gian vận khí (Cast-time Projectile)** | Fixture RAM: `p09_pm_cast`<br>CastTime: 0.3s, Nộ: 25, CD: 3.0s | Thi triển qua `Hero.ExecuteSelectedSkill` hoặc chạy Gate 2 | Nộ bị trừ ngay tại thời điểm bắt đầu vận khí (Frame cast start); Trong lúc vận khí chưa xuất hiện đạn và chưa kích hoạt cooldown; Sau khi hoàn tất 0.3s vận khí, đạn được phóng ra và cooldown bắt đầu tính 1 lần duy nhất; Đạn bay trúng đích gây sát thương dứt điểm. | **PASS** (T09, Gate 2) | `READY_FOR_USER_TEST` (Fixture RAM) |
| **P09-04** | **Đổi mục tiêu trong lúc đạn đang bay (Target Retargeting)** | Fixture RAM: `p09_t03_homing`<br>Target A (Gốc), Target B | Phóng đạn vào Target A, trong lúc đạn đang bay, click chọn Target B | Đạn vẫn duy trì khóa mục tiêu gốc (Target A) và bay trúng Target A; Target B hoàn toàn không bị trúng đạn hay nhận sát thương rò rỉ. | **PASS** (T03) | `READY_FOR_USER_TEST` |
| **P09-05** | **Tạm dừng và Tiếp tục (Pause / Resume)** | Nút `PauseButton` trên Combat HUD | Khi đạn đang bay, click nút Pause; sau đó click Resume | Lúc Pause: đạn dừng chuyển động hoàn toàn trên không gian, không gây sát thương; Sau Resume: đạn tiếp tục bay với vận tốc cũ và gây sát thương đúng thời điểm va chạm. | **PASS** (T10) | `READY_FOR_USER_TEST` |
| **P09-06** | **Mục tiêu chết trước khi đạn chạm đích** | Quái mục tiêu bị hạ gục trước khi đạn tới | Đạn đang bay, quái chết do đòn đánh khác | Đạn tự động hủy (`Cancel`), không gây sát thương trúng xác chết hoặc mục tiêu kế tiếp; Dọn dẹp sạch sẽ khỏi RAM/Hierarchy. | **PASS** (T04) | `READY_FOR_USER_TEST` |
| **P09-07** | **Caster chết trước khi đạn chạm đích** | Hero chết trong lúc đạn đang bay | Hero ngã xuống trước khi đạn chạm quái | Đạn tự động bị hủy an toàn, không phát sinh sát thương mồ côi. | **PASS** (T05) | `READY_FOR_USER_TEST` |
| **P09-08** | **Chuyển Wave / Scene Unload** | Chuyển encounter hoặc dọn wave | Tiêu diệt quái cuối đợt hoặc chuyển màn | Toàn bộ đạn còn đang bay tự động hủy và dọn dẹp sạch sẽ (`ActiveProjectiles.Count == 0`), không gây sát thương sang wave mới. | **PASS** (T07, T21) | `READY_FOR_USER_TEST` |

---

## 4. BẰNG CHỨNG TỰ ĐỘNG THAY THẾ (NƠI XEM LOGS)

Nếu Chủ Dự Án muốn xem chi tiết các thông số đo đạc khung hình tự nhiên:
1. **Gate 1 Foundation Tests (21/21 PASS)**:
   - File log: `gate1_p09_tests.log`
   - Wrapper log: `gate1_p09_wrapper.log`
   - Kết quả: Đầy đủ 21/21 test cases PASS (bao gồm T02 cooldown clock advance, T09 cast separation, T21 real scene unload).
2. **Gate 2 Natural Frames Play Mode Scenario (PASS)**:
   - File log: `gate2_p09_playmode.log`
   - Wrapper log: `gate2_p09_wrapper.log`
   - Đo lường khung hình tự nhiên:
     - Segment 1 (Instant): Phóng tại Frame 1 ($t=0.000s$), Va chạm tại Frame 709 ($t=0.651s$), Target HP: $300 \to 120$.
     - Segment 2 (Cast-time): Bắt đầu vận khí tại Frame 709, Hoàn tất vận khí và Phóng đạn tại Frame 1770 ($t=0.951s$), Va chạm dứt điểm tại Frame 2772 ($t=1.264s$), Quái chết tự nhiên ($HP=0$), kích hoạt sự kiện tử trận.
3. **Wrapper Failure Path Robustness (6/6 PASS)**:
   - File log: `Tools\Verification\P09\wrapper_failure_path_test.log`
   - Chứng minh toàn bộ 6 nhánh lỗi giả lập (Backup fail, Launch fail, Crash, Compare diff, Restore fail, Timeout diff) đều được xử lý chuẩn mực.
