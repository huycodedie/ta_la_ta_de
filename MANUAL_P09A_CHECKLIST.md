# MANUAL_P09A_CHECKLIST.md
# Hướng Dẫn Khởi Chạy Phiên Quan Sát Manual & Checklist Nghiệm Thu P09-A (Dành Cho Chủ Dự Án)

> **TRẠNG THÁI HIỆN TẠI**: `READY_FOR_USER_TEST` (Khung thao tác thủ công đã cô lập an toàn, sẵn sàng để Chủ Dự Án trực tiếp kiểm tra).  
> **NGHIỆM THU NGƯỜI DÙNG**: `USER_VERIFICATION = NOT_EXECUTED` (Antigravity **tuyệt đối không tự ý đánh dấu USER_VERIFIED** hoặc **P09-A LOCKED** khi Chủ Dự Án chưa xác nhận).

---

## 1. THÔNG BÁO MINH BẠCH VỀ DỮ LIỆU THỬ NGHIỆM (FIXTURE RAM vs PRODUCTION ASSET)

> [!IMPORTANT]
> **Vị trí & Bản chất của Kỹ năng Projectile trong Giai đoạn P09-A**:
> - Cơ chế Projectile Delivery Foundation đã hoàn thiện ở tầng runtime (`ProjectileController`, `SkillExecutor`, `EffectResolver`, `BattleManager`).
> - **Toàn bộ 61 file ScriptableObject kỹ năng sản xuất trong `Assets/_Game/Data/Skills/` vẫn đang giữ nguyên `IsProjectile = false` (kỹ năng cận chiến / tức thời sản xuất)** theo đúng ranh giới khóa production và phạm vi thiết kế của P08/P09.
> - Các kỹ năng đạn bay trong đợt kiểm thử P09-A được khởi tạo động trong bộ nhớ RAM qua **Transient Fixtures**:
>   - `p09_manual_instant` (Instant Projectile):
>     - Tên: Thái Cực Kiếm Khí (Instant Đạn Bay)
>     - Vận tốc: $6.0\text{ u/s}$, Tầm bay tối đa: $8.0\text{s}$, Cooldown: $4.0\text{s}$, Tiêu hao: $15$ Nộ.
>     - Sát thương: Multiplier $2.0\times$. Với Hero Base Attack $100$ và Target Defense $0$, sát thương thực tế là $200$ (khi không bạo kích, trừ máu Target từ $500 \to 300$).
>   - `p09_manual_cast` (Cast-Time Projectile):
>     - Tên: Huyền Vũ Thần Tiễn (1.5s Vận Khí + Đạn Bay)
>     - Thời gian vận khí: $1.5\text{s}$, Vận tốc: $6.0\text{ u/s}$, Tầm bay tối đa: $8.0\text{s}$, Cooldown: $6.0\text{s}$, Tiêu hao: $25$ Nộ.
>     - Sát thương: Multiplier $3.5\times$. Với Hero Base Attack $100$ và Target Defense $0$, sát thương thực tế là $350$ (khi không bạo kích, trừ máu Target từ $500 \to 150$).
>   - Thực thể quan sát:
>     - **Hero** (`Hero_Observation`, Cyan Capsule tại $X = -4.5, Y = 0$): HP $1000/1000$, Nộ $100/100$.
>     - **Target A** (`Monster_Target_A`, Red Sphere tại $X = 4.5, Y = +1.0$): HP $500/500$, Giáp $0$.
>     - **Target B** (`Monster_Target_B`, Orange Sphere tại $X = 4.5, Y = -1.0$): HP $500/500$, Giáp $0$.
> - Toàn bộ thực thể nằm dưới container riêng `[P09_Manual_Observation_Fixture_Root]` trong scene RAM trống, không đè lên scene người dùng và không can thiệp object của game thường.

---

## 2. LỆNH KHỞI CHẠY DUY NHẤT & QUY TRÌNH SAVE GUARD BẢO VỆ

Toàn bộ phiên làm việc GUI được bảo vệ bởi **Save Guard Wrapper**, đảm bảo an toàn tuyệt đối cho PlayerPrefs và registry của workspace.

### Bước 2.1: Chuẩn bị
1. Nếu bạn đang mở sẵn Unity Editor cho dự án `E:\code\TLTD`, vui lòng **LƯU CÔNG VIỆC** và **ĐÓNG HOÀN TOÀN** Unity Editor.
2. Wrapper sẽ từ chối khởi chạy nếu phát hiện còn tiến trình Unity Editor khác đang hoạt động để tránh xung đột dữ liệu.

### Bước 2.2: Khởi chạy phiên quan sát an toàn
Mở PowerShell tại thư mục gốc dự án `E:\code\TLTD` và thực thi **MỘT LỆNH DUY NHẤT**:
```powershell
powershell -ExecutionPolicy Bypass -File Tools\Verification\P09\launch_manual_p09a_session.ps1
```

**Các bước tự động diễn ra:**
1. **Save Guard Backup**: Tự động kiểm tra journal và sao lưu toàn bộ PlayerPrefs/Registry vào `scratch\.save_backup_manual_session`.
2. **Khởi động Unity GUI với cờ cấp phép**: Unity Editor mở lên với tham số `-p09ManualSession`, tạo scene RAM trống không lưu đè.
3. **Mở Bảng Điều Khiển**: Cửa sổ `P09 Manual Observation` tự động xuất hiện và khởi tạo fixture.
4. **Giữ phiên**: Phiên chạy giữ nguyên trạng thái Play Mode để Chủ Dự Án tự do tương tác, bấm nút và quan sát chu kỳ khung hình tự nhiên (`Time.timeScale = 1.0`).

### Bước 2.3: Kết thúc phiên và Phục hồi Trạng thái
1. Khi hoàn thành kiểm tra, đóng cửa sổ Unity Editor bình thường (`Alt + F4` hoặc `File -> Exit`).
2. Wrapper trên PowerShell sẽ tự động:
   - Chờ tiến trình Unity đóng hẳn (không kill Editor ngoài phiên).
   - Gọi độc lập lệnh `Restore` phục hồi PlayerPrefs về trạng thái nguyên bản trước khi mở.
   - Gọi độc lập lệnh `Compare` để xác minh đối chiếu dữ liệu (`Diff = 0`).
3. Màn hình console PowerShell thông báo:
   `[SAVE GUARD PASS] State 100% Restored. Diff = 0 Verified (Exact match).`
   *(Lưu ý: `SAVE_GUARD_PASS` xác nhận việc sao lưu và phục hồi registry thành công, không thay thế cho `USER_VERIFIED` của Chủ Dự Án).*

---

## 3. GIAO DIỆN & CÁC NÚT ĐIỀU KHIỂN TRONG GAME VIEW

Bảng điều khiển **P09 Manual Observation** cung cấp các nút tương tác trực tiếp qua runtime API thực của game:

### A. Nhóm Nút Tương Tác Thao Tác (Actions)
1. **`[Tạo / Reset Fixture (Khôi phục ban đầu)]`**:
   - Dọn dẹp triệt để fixture cũ và tái tạo toàn bộ thực thể về trạng thái ban đầu:
   - Hero ở $X = -4.5, Y = 0$ với $1000/1000$ HP, $100/100$ Nộ.
   - Target A ở $X = 4.5, Y = +1.0$ với $500/500$ HP.
   - Target B ở $X = 4.5, Y = -1.0$ với $500/500$ HP.
   - Tự động gán tấn công thường thụ động (`SetAttackEnabled(false)`) để cô lập quan sát: một lần bấm chỉ sinh đúng một request.
2. **`[Thi triển Instant Đạn Bay (Speed 6, CD 4.0s, Nộ 15, Mult 2.0x)]`**:
   - Chọn skill vào slot và gọi `Hero.ExecuteSelectedSkill(SkillSlotType.Skill, currentTarget)`.
   - Trừ $15$ Nộ ngay lập tức; Bắt đầu cooldown $4.0\text{s}$; Phóng ra đạn bay tự nhiên về phía mục tiêu đang chọn.
3. **`[Thi triển Cast-Time Đạn Bay (Vận khí 1.5s, Speed 6, CD 6.0s, Nộ 25, Mult 3.5x)]`**:
   - Chọn skill vào slot và gọi `Hero.ExecuteSelectedSkill(SkillSlotType.Skill, currentTarget)`.
   - Trừ $25$ Nộ ngay khi bắt đầu; Hero vào trạng thái vận khí $1.5\text{s}$ (hiển thị tiến độ % vận khí); Sau $1.5\text{s}$, phóng đạn và bắt đầu tính cooldown $6.0\text{s}$.
4. **`[Chọn Target A (Đỏ, Y = +1.0)]` / `[Chọn Target B (Cam, Y = -1.0)]`**:
   - Chuyển đổi mục tiêu hiện tại của Hero (`Hero.SetCurrentTarget`).
5. **`[Tạm dừng Combat (Pause)]` / `[Tiếp tục Combat (Resume)]`**:
   - Gọi cơ chế tạm dừng thực tế của combat qua `BattleManager.PauseCombat()` và `BattleManager.ResumeCombat()`.
   - Đạn đang bay lập tức đứng yên giữa không trung; khi Resume, đạn tiếp tục bay bình thường.

### B. Bảng Giám Sát Thời Gian Thực (Live Runtime Inspection)
- **Hero Status**: HP ($1000/1000$), Nộ ($100 \to 85 \to 60$).
- **Casting Status**: `Nhàn rỗi (Ready)` hoặc `ĐANG VẬN KHÍ (xx%)`.
- **Cooldown Status**: Hồi chiêu Instant ($4.0\text{s} \to 0\text{s}$), Hồi chiêu Cast-Time ($6.0\text{s} \to 0\text{s}$).
- **Target Status**: Mục tiêu đang chọn, HP Target A ($500$), HP Target B ($500$).
- **Sát thương thực đo (EventBus)**: Đo trực tiếp qua `EventBus.OnEntityDamaged` (ghi rõ lượng sát thương trừ máu thực tế và bạo kích).
- **Active Projectiles**: Số lượng đạn thực tế đang bay (`Count`) kèm danh tính mục tiêu, khoảng cách, tọa độ và thời gian bay.

---

## 4. CHECKLIST NGHIỆM THU CHI TIẾT TỪNG KỊCH BẢN (SCENARIOS)

| STT | Kịch Bản Kiểm Tra | Thao Tác Bấm Nút | Kết Quả Mong Đợi (Quan Sát Game View & Panel) | Trạng Thái Tự Động | Trạng Thái Thủ Công |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **S01** | **Kỹ năng đạn bay tức thời (Instant Projectile)** | 1. Bấm `[Tạo / Reset Fixture]`<br>2. Bấm `[Thi triển Instant Đạn Bay]` | - Nộ giảm ngay từ 100 xuống 85 (-15).<br>- Cooldown bắt đầu đếm lùi từ 4.0s.<br>- Đạn vàng xuất hiện từ Hero và bay tự nhiên về phía Target A (speed 6.0 u/s).<br>- Trước khi đạn chạm: HP Target A giữ nguyên 500/500.<br>- Khi đạn chạm: Target A nhận 200 sát thương (HP còn 300/500), đạn biến mất.<br>- Cooldown tiếp tục giảm bình thường, KHÔNG bị reset lại lúc chạm. | **PASS** (T02, Gate 2) | `READY_FOR_USER_TEST` |
| **S02** | **Kỹ năng đạn bay có vận khí (Cast-Time Projectile)** | 1. Bấm `[Tạo / Reset Fixture]`<br>2. Bấm `[Thi triển Cast-Time Đạn Bay]` | - Nộ giảm ngay từ 100 xuống 75 (-25 lúc bắt đầu).<br>- Hero vào trạng thái vận khí 1.5s (Panel hiện `ĐANG VẬN KHÍ`).<br>- Trong lúc vận khí: Chưa có đạn sinh ra, cooldown chưa chạy.<br>- Sau đúng 1.5s: Đạn xuất hiện, bắt đầu bay về phía Target A; Cooldown bắt đầu đếm lùi từ 6.0s.<br>- Khi đạn chạm: Target A nhận 350 sát thương (HP còn 150/500). | **PASS** (T09, Gate 2) | `READY_FOR_USER_TEST` |
| **S03** | **Đổi mục tiêu khi đạn đang bay (Retargeting)** | 1. Bấm `[Thi triển Instant Đạn Bay]`<br>2. Trong lúc đạn đang bay (~1.5s), bấm ngay `[Chọn Target B]` | - Hero đổi mục tiêu khóa sang Target B.<br>- Quả đạn đang bay vẫn giữ nguyên khóa mục tiêu ban đầu (Target A) và bay thẳng vào Target A.<br>- Target A nhận sát thương; Target B hoàn toàn không bị ảnh hưởng. | **PASS** (T03) | `READY_FOR_USER_TEST` |
| **S04** | **Tạm dừng và tiếp tục (Pause / Resume)** | 1. Bấm `[Thi triển Instant Đạn Bay]`<br>2. Khi đạn đang ở giữa không trung, bấm `[Tạm dừng Combat]`<br>3. Quan sát đạn đứng yên, bấm `[Tiếp tục Combat]` | - Lúc Pause: Đạn đứng yên trên không gian Game View, Active Projectiles = 1, sát thương chưa áp dụng.<br>- Sau Resume: Đạn tiếp tục hành trình bay với vận tốc ban đầu và trúng Target A gây sát thương chính xác. | **PASS** (T10) | `READY_FOR_USER_TEST` |
| **S05** | **Reset Fixture giữa các ca thử nghiệm** | Bấm `[Tạo / Reset Fixture]` bất kỳ lúc nào (bấm 3 lần liên tiếp) | - Toàn bộ đạn đang bay bị dọn dẹp sạch sẽ.<br>- HP, Nộ, vị trí Hero, Target A, Target B được khôi phục 100% về ban đầu.<br>- Fixture tái tạo đúng một camera và entity, không bị nhân đôi, không lặp lại do repaint. | **PASS** | `READY_FOR_USER_TEST` |
| **S06** | **Target chết tự nhiên trước khi đạn chạm** | *Kịch bản tự động* (Target bị tiêu diệt giữa đường bay) | Đạn tự động hủy (`Cancel`), không gây sát thương trúng xác chết; Dọn dẹp sạch khỏi runtime. | **PASS** (T04, Gate 2) | `NOT AVAILABLE IN UI`<br>*(Đã kiểm chứng tự động qua Gate 1 T04 & Gate 2)* |
| **S07** | **Caster tử trận trước khi đạn chạm** | *Kịch bản tự động* (Hero chết trong lúc đạn đang bay) | Toàn bộ đạn của caster bị hủy an toàn, không gây sát thương mồ côi. | **PASS** (T05) | `NOT AVAILABLE IN UI`<br>*(Đã kiểm chứng tự động qua Gate 1 T05)* |
| **S08** | **Chuyển Scene / Scene Unload dọn dẹp đạn** | *Kịch bản tự động* (Unload scene chứa đạn) | Unload scene thực qua Unity API; toàn bộ đạn đang bay bị dọn dẹp sạch (`Count = 0`), không rò rỉ sát thương. | **PASS** (T21) | `NOT AVAILABLE IN UI`<br>*(Đã kiểm chứng tự động qua Gate 1 T21 với điều kiện nghiêm ngặt)* |

---

## 5. NƠI LẤY NHẬT KÝ VÀ BẰNG CHỨNG THỬ NGHIỆM

Sau khi kết thúc phiên thử nghiệm, toàn bộ log và bằng chứng nằm tại:
1. **Nhật ký phiên thủ công (Manual Session Logs)**:
   - `manual_session.log` (tại thư mục gốc `E:\code\TLTD`): Ghi nhận toàn bộ log của engine Unity, các sự kiện bay của đạn và DamageResult.
   - `scratch\manual_wrapper.log`: Ghi nhận nhật ký giám sát của PowerShell wrapper và chu trình Backup/Restore/Compare của Save Guard.
2. **Nhật ký kiểm thử tự động toàn diện**:
   - `gate1_p09_tests.log`: Bằng chứng 21/21 Unit/Integration Tests PASS (bao gồm sửa dứt điểm T21 false positive).
   - `gate2_p09_playmode.log`: Bằng chứng kịch bản Play Mode Natural Frames (Segment 1 Instant, Segment 2 Cast-time dứt điểm quái).
   - `Tools\Verification\P09\wrapper_failure_path_test.log`: Bằng chứng 8/8 kịch bản lỗi Save Guard được xử lý chuẩn mực.
