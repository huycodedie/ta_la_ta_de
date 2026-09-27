# MANUAL_P09A_CHECKLIST.md
# Hướng Dẫn Khởi Chạy Phiên Quan Sát Manual & Checklist Nghiệm Thu P09-A (Dành Cho Chủ Dự Án)

> **TRẠNG THÁI HIỆN TẠI**: `READY_FOR_USER_TEST` (Khung thao tác thủ công đã sẵn sàng để Chủ Dự Án trực tiếp kiểm tra).  
> **NGHIỆM THU NGƯỜI DÙNG**: `USER_VERIFICATION = NOT_EXECUTED` (Antigravity **tuyệt đối không tự ý đánh dấu USER_VERIFIED** hoặc **P09-A LOCKED**).

---

## 1. THÔNG BÁO MINH BẠCH VỀ DỮ LIỆU THỬ NGHIỆM (FIXTURE RAM vs PRODUCTION ASSET)

> [!IMPORTANT]
> **Vị trí & Bản chất của Kỹ năng Projectile trong Giai đoạn P09-A**:
> - Cơ chế Projectile Delivery Foundation đã hoàn thiện ở tầng runtime (`ProjectileController`, `SkillExecutor`, `EffectResolver`, `BattleManager`).
> - **Toàn bộ 61 file ScriptableObject kỹ năng sản xuất trong `Assets/_Game/Data/Skills/` vẫn đang giữ nguyên `IsProjectile = false` (kỹ năng cận chiến / tức thời sản xuất)** theo đúng ranh giới khóa production và phạm vi thiết kế của P08/P09.
> - Các kỹ năng đạn bay trong đợt kiểm thử P09-A được khởi tạo động trong bộ nhớ RAM qua **Transient Fixtures**:
>   - `p09_manual_instant` (Instant Projectile: Vận tốc $8.0\text{ m/s}$, Tầm bay tối đa $5.0\text{s}$, Cooldown $4.0\text{s}$, Tiêu hao $30$ Nộ, Sát thương $100$, Khoảng cách bắn $12\text{m} \implies$ Thời gian bay $\approx 1.5\text{s}$, đủ để quan sát chuyển động và thử nghiệm).
>   - `p09_manual_cast` (Cast-Time Projectile: Thời gian vận khí $1.0\text{s}$, Vận tốc $8.0\text{ m/s}$, Tầm bay tối đa $5.0\text{s}$, Cooldown $4.0\text{s}$, Tiêu hao $25$ Nộ, Sát thương $120$, Khoảng cách bắn $12\text{m}$).
>   - `p09_manual_retarget` (Homing Projectile: Dùng để kiểm chứng đạn không bị đổi hướng khi chuyển mục tiêu giữa đường bay).
> - Phiên kiểm thử thủ công này sử dụng **Bảng Điều Khiển Riêng (`P09 Manual Observation Harness`)**, không yêu cầu Chủ Dự Án phải tìm kiếm kỹ năng RAM trong production HUD hay tự gõ mã C#.

---

## 2. LỆNH KHỞI CHẠY DUY NHẤT & QUY TRÌNH SAVE GUARD BẢO VỆ

Toàn bộ phiên làm việc GUI được bảo vệ bởi **Save Guard Wrapper**, đảm bảo an toàn tuyệt đối cho PlayerPrefs và file cấu hình registry.

### Bước 2.1: Chuẩn bị
1. Nếu bạn đang mở sẵn Unity Editor cho dự án `E:\code\TLTD`, vui lòng **LƯU CÔNG VIỆC** và **ĐÓNG HOÀN TOÀN** Unity Editor.
2. Wrapper sẽ từ chối khởi chạy nếu phát hiện còn tiến trình Unity Editor khác đang hoạt động để tránh xung đột dữ liệu.

### Bước 2.2: Khởi chạy phiên quan sát an toàn
Mở PowerShell tại thư mục gốc dự án `E:\code\TLTD` và thực thi **MỘT LỆNH DUY NHẤT**:
```powershell
powershell -ExecutionPolicy Bypass -File Tools\Verification\P09\launch_manual_p09a_session.ps1
```

**Các bước tự động diễn ra:**
1. **Save Guard Backup**: Tự động sao lưu toàn bộ PlayerPrefs/Registry vào `scratch\.save_backup_manual_session`.
2. **Khởi động Unity GUI**: Unity Editor mở lên với scene cô lập an toàn, tự động vào **Play Mode** với **Game View** trực quan.
3. **Mở Bảng Điều Khiển**: Cửa sổ `P09 Manual Observation Harness` tự động xuất hiện (hoặc mở từ menu `Window -> TLTD -> P09 Manual Observation Harness`).
4. **Giữ phiên**: Phiên chạy giữ nguyên trạng thái Play Mode để Chủ Dự Án tự do tương tác, bấm nút và quan sát chu kỳ khung hình tự nhiên (`Time.timeScale = 1.0`).

### Bước 2.3: Kết thúc phiên và Phục hồi Trạng thái
1. Khi hoàn thành kiểm tra, đóng cửa sổ Unity Editor bình thường (`Alt + F4` hoặc `File -> Exit`).
2. Wrapper trên PowerShell sẽ tự động:
   - Chờ tiến trình Unity đóng hẳn.
   - Gọi độc lập lệnh `Restore` phục hồi PlayerPrefs về trạng thái nguyên bản trước khi mở.
   - Gọi độc lập lệnh `Compare` để xác minh đối chiếu dữ liệu (`Diff = 0`).
3. Màn hình console PowerShell thông báo:
   `[SAVE GUARD PASS] State 100% Restored. Diff = 0 Verified (Exact match).`
   *(Lưu ý: `SAVE_GUARD_PASS` chỉ xác nhận việc sao lưu và phục hồi registry thành công, không thay thế cho `USER_VERIFIED` của Chủ Dự Án).*

---

## 3. GIAO DIỆN & CÁC NÚT ĐIỀU KHIỂN TRONG GAME VIEW

Bảng điều khiển **P09 Manual Observation Harness** cung cấp các nút tương tác trực tiếp qua runtime API thực của game:

### A. Nhóm Nút Tương Tác Thao Tác (Actions)
1. **`[Setup / Reset Fixture]`**:
   - Khởi tạo mới hoặc reset toàn bộ thực thể về trạng thái ban đầu:
   - Hero ở tọa độ $(0, 0, 0)$ với $500/500$ HP, $100$ Nộ (đầy Nộ).
   - Target A (`Dummy_Target_A`) ở tọa độ $(12, 0, 0)$ với $300/300$ HP.
   - Target B (`Dummy_Target_B`) ở tọa độ $(12, 0, 4)$ với $300/300$ HP.
   - Camera được định vị nhìn rõ Hero, hai Dummy Targets và quỹ đạo bay đạn.
2. **`[Cast Instant Projectile]`**:
   - Gọi trực tiếp `Hero.ExecuteSelectedSkill(p09_manual_instant)`.
   - Trừ $30$ Nộ ngay lập tức; Bắt đầu cooldown $4.0\text{s}$; Phóng ra đạn bay về phía mục tiêu đang chọn.
3. **`[Cast Cast-Time Projectile]`**:
   - Gọi trực tiếp `Hero.ExecuteSelectedSkill(p09_manual_cast)`.
   - Trừ $25$ Nộ ngay; Hero vào trạng thái vận khí $1.0\text{s}$ (thanh trạng thái hiển thị `Casting...`); Sau $1.0\text{s}$, phóng đạn và bắt đầu tính cooldown $4.0\text{s}$.
4. **`[Target A (Default)]` / `[Target B]`**:
   - Gọi API chuyển mục tiêu runtime thực (`BattleManager.Instance.SelectTarget()`).
5. **`[Pause / Resume Game]`**:
   - Gọi cơ chế tạm dừng của game qua `BattleManager.Instance.TogglePause()` hoặc `Time.timeScale = 0`.
   - Đạn đang bay lập tức dừng chuyển động trên không; khi Resume, đạn tiếp tục bay bình thường.

### B. Bảng Giám Sát Thời Gian Thực (Live Runtime Inspection)
- **Hero Status**: HP ($500/500$), Rage ($100 \to 70 \to 45$), Cooldown còn lại ($4.0\text{s} \to 0\text{s}$).
- **Casting Status**: `Idle` hoặc `Casting: p09_manual_cast (x.xx / 1.00s)`.
- **Target Status**: Mục tiêu đang khóa (`Target A` hoặc `Target B`), HP hiện tại của Target A và Target B.
- **Active Projectiles**: Số lượng đạn thực tế đang bay trong hệ thống (`ActiveProjectiles.Count`).

---

## 4. CHECKLIST NGHIỆM THU CHI TIẾT TỪNG KỊCH BẢN (SCENARIOS)

| STT | Kịch Bản Kiểm Tra | Thao Tác Bấm Nút | Kết Quả Mong Đợi (Quan Sát Game View & Panel) | Trạng Thái Tự Động | Trạng Thái Thủ Công |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **S01** | **Kỹ năng đạn bay tức thời (Instant Projectile)** | 1. Bấm `[Setup / Reset Fixture]`<br>2. Bấm `[Cast Instant Projectile]` | - Nộ giảm ngay từ 100 xuống 70.<br>- Cooldown bắt đầu đếm lùi từ 4.0s.<br>- Đạn vàng xuất hiện từ Hero và bay tự nhiên về phía Target A.<br>- Trước khi đạn chạm: HP Target A giữ nguyên 300/300.<br>- Khi đạn chạm: Target A nhận 100 sát thương (HP còn 200/300), đạn biến mất.<br>- Cooldown tiếp tục giảm bình thường, KHÔNG bị reset lại lúc chạm. | **PASS** (T02, Gate 2) | `READY_FOR_USER_TEST` |
| **S02** | **Kỹ năng đạn bay có vận khí (Cast-Time Projectile)** | 1. Bấm `[Setup / Reset Fixture]`<br>2. Bấm `[Cast Cast-Time Projectile]` | - Nộ giảm ngay từ 100 xuống 75.<br>- Hero vào trạng thái vận khí 1.0s (Panel hiện `Casting...`).<br>- Trong lúc vận khí: Chưa có đạn sinh ra, cooldown chưa chạy.<br>- Sau đúng 1.0s: Đạn xuất hiện, bắt đầu bay về phía Target A; Cooldown bắt đầu đếm lùi từ 4.0s.<br>- Khi đạn chạm: Target A nhận 120 sát thương (HP còn 180/300). | **PASS** (T09, Gate 2) | `READY_FOR_USER_TEST` |
| **S03** | **Đổi mục tiêu khi đạn đang bay (Retargeting)** | 1. Bấm `[Cast Instant Projectile]`<br>2. Trong lúc đạn đang bay (~1.5s), bấm ngay `[Target B]` | - Runtime Target chuyển sang Target B.<br>- Quả đạn đang bay vẫn giữ nguyên khóa mục tiêu ban đầu (Target A) và bay thẳng vào Target A.<br>- Target A nhận sát thương; Target B hoàn toàn không bị ảnh hưởng. | **PASS** (T03) | `READY_FOR_USER_TEST` |
| **S04** | **Tạm dừng và tiếp tục (Pause / Resume)** | 1. Bấm `[Cast Instant Projectile]`<br>2. Khi đạn đang ở giữa không trung, bấm `[Pause / Resume Game]`<br>3. Quan sát đạn đứng yên, bấm lại `[Pause / Resume Game]` | - Lúc Pause: Đạn đứng yên trên không gian Game View, Active Projectiles = 1, sát thương chưa áp dụng.<br>- Sau Resume: Đạn tiếp tục hành trình bay với vận tốc ban đầu và trúng Target A gây sát thương chính xác. | **PASS** (T10) | `READY_FOR_USER_TEST` |
| **S05** | **Reset Fixture giữa các ca thử nghiệm** | Bấm `[Setup / Reset Fixture]` bất kỳ lúc nào | - Toàn bộ đạn đang bay bị hủy an toàn.<br>- HP, Nộ, vị trí Hero, Target A, Target B được khôi phục 100%. | **PASS** | `READY_FOR_USER_TEST` |
| **S06** | **Target chết tự nhiên trước khi đạn chạm** | *Kịch bản tự động* (Target bị tiêu diệt giữa đường bay) | Đạn tự động hủy (`Cancel`), không gây sát thương trúng xác chết; Dọn dẹp sạch khỏi runtime. | **PASS** (T04, Gate 2) | `NOT AVAILABLE IN UI`<br>*(Đã kiểm chứng tự động qua Gate 1 T04 & Gate 2)* |
| **S07** | **Caster tử trận trước khi đạn chạm** | *Kịch bản tự động* (Hero chết trong lúc đạn đang bay) | Toàn bộ đạn của caster bị hủy an toàn, không gây sát thương mồ côi. | **PASS** (T05) | `NOT AVAILABLE IN UI`<br>*(Đã kiểm chứng tự động qua Gate 1 T05)* |
| **S08** | **Chuyển Scene / Scene Unload dọn dẹp đạn** | *Kịch bản tự động* (Unload scene chứa đạn) | Unload scene thực qua Unity API; toàn bộ đạn đang bay bị dọn dẹp sạch (`Count = 0`), không rò rỉ sát thương. | **PASS** (T21) | `NOT AVAILABLE IN UI`<br>*(Đã kiểm chứng tự động qua Gate 1 T21 với điều kiện nghiêm ngặt)* |

---

## 5. NƠI LẤY NHẬT KÝ VÀ BẰNG CHỨNG THỬ NGHIỆM

Sau khi kết thúc phiên thử nghiệm, toàn bộ log và bằng chứng nằm tại:
1. **Nhật ký phiên thủ công (Manual Session Log)**:
   - `scratch\manual_session.log`: Ghi nhận chi tiết toàn bộ chu kỳ khởi động, frame update và các lần cast đạn của session.
   - `scratch\manual_wrapper.log`: Ghi nhận nhật ký giám sát tiến trình và kết quả Save Guard.
2. **Nhật ký kiểm thử tự động toàn diện**:
   - `gate1_p09_tests.log`: Bằng chứng 21/21 Unit/Integration Tests PASS (bao gồm sửa dứt điểm T21 false positive).
   - `gate2_p09_playmode.log`: Bằng chứng kịch bản Play Mode Natural Frames (Segment 1 Instant, Segment 2 Cast-time dứt điểm quái).
   - `Tools\Verification\P09\wrapper_failure_path_test.log`: Bằng chứng 8/8 kịch bản lỗi Save Guard được xử lý chuẩn mực.
