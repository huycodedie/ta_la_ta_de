# BIÊN BẢN QUYẾT ĐỊNH TECH LEAD: NGHIỆM THU P09-A & BẢO TRÌ TOOLING N1 / N2
# Mã văn bản: DECISION_P09A_ACCEPTED_LOCKED_20260927.md

> **NGÀY BAN HÀNH**: 2026-09-27 / 2026-09-28 (Asia/Ho_Chi_Minh)  
> **ĐỊA BÀN THỰC THI**: `E:\code\TLTD` (Unity 6000.6.0f1)  
> **TRẠNG THÁI CHÍNH THỨC**: `P09-A FUNCTIONAL ACCEPTANCE PRESERVED / N1–N2 TOOLING MAINTENANCE VERIFIED`

---

## 1. CÁC QUYẾT ĐỊNH ĐÃ CHỐT CHÍNH THỨC (TECH LEAD DECISIONS)

1. **F-MANUAL-AUTONOMY = CLOSED / FIX VERIFIED**:
   - Lỗi tự quyết định trong manual observation harness (AI tự cast chiêu, wave advance sinh Wild Monster, basic attack chạy ngầm) đã được sửa dứt điểm trong tooling và kiểm chứng đạt 100%.
2. **P09-A PROJECTILE FOUNDATION = ACCEPTED / LOCKED**:
   - Nền tảng phân phối đạn bay (`ProjectileController`, `SkillExecutor`, `EffectResolver`, `BattleManager`) được chính thức nghiệm thu và khóa (LOCKED) trong phạm vi runtime đã review.
3. **BẢO TOÀN NGHIỆM THU NGƯỜI DÙNG (S01–S05: USER_VERIFIED)**:
   - Các kịch bản kiểm tra thủ công S01–S05 do Chủ Dự Án trực tiếp thực hiện và xác nhận tiếp tục được bảo toàn tuyệt đối; không yêu cầu thử lại checklist hoặc quay video lại.
4. **BẢO TOÀN LỊCH SỬ BẰNG CHỨNG E1 & E2**:
   - **E1**: Giữ nguyên `CLOSED / PASS` (phiên Unity thực tế của Chủ Dự Án, exit 0, Restore SUCCESS, Diff = 0).
   - **E2**: Giữ nguyên `CLOSED / EVIDENCE VERIFIED`, `SCENARIO_PASS_EXIT_TIMEOUT`, wrapper exit 2 (Frame 842 / Frame 2998, kịch bản PASS).
   - Tuyệt đối không dùng kết quả exit 0 của bộ kiểm chứng autonomy verifier để viết đè hoặc sửa đổi lịch sử E2.
5. **BẢO TOÀN SẢN XUẤT R1 & P08 (ACCEPTED / LOCKED)**:
   - Toàn bộ gameplay, asset, scene của R1 và P08 giữ nguyên trạng khóa.
   - Toàn bộ 61 file ScriptableObject kỹ năng sản xuất trong `Assets/_Game/Data/Skills/` tiếp tục giữ `IsProjectile = false`. Việc cấu hình đạn bay cho asset sản xuất chưa thuộc phạm vi quyết định này.
   - Tuyệt đối không commit, push, stage, stash, reset, checkout đè hay clean working tree.

---

## 2. GHI CHÚ BẢO TRÌ TOOLING N1 / N2 (TOOLING MAINTENANCE NOTES)

*(Lưu ý: Không tuyên bố "không còn lỗi tooling"; ghi nhận trung thực phạm vi bảo trì đã thực hiện).*

### N1 — Sửa đúng bộ đếm release trong Editor harness
- **Bối cảnh lỗi**: Cơ chế đếm cũ dựa vào biến thiên số lượng `TotalProjectileReleases += currentCount - previousCount` khi `count` tăng. Tại frame tương đương 10689, khi đạn Cast-time va chạm mục tiêu (bị hủy) và đạn Instant mới xuất hiện trong cùng khoảng quan sát, tổng số đạn active vẫn bằng 1, khiến guardian không bắt được lượt phóng thứ tư của cycle.
- **Biện pháp khắc phục**:
  - Nhận diện từng thực thể đạn mới bằng tham chiếu đối tượng ổn định (`HashSet<ProjectileController>` với `ProjectileReferenceComparer` sử dụng `System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode`), đảm bảo không phụ thuộc vào phương thức lỗi thời `GetInstanceID()`.
  - Mỗi khi một quả đạn mới xuất hiện trong `ActiveProjectiles`, guardian thêm tham chiếu vào tập quan sát và tăng `TotalProjectileReleases` độc lập với việc số lượng count có tăng hay không.
  - Reset tập tham chiếu đúng lúc tại `ResetTelemetry()` và `TeardownFixture()`.
  - Không gán số release bằng số command thành công; giữ nguyên mô hình ghi nhận theo frame thật.
  - Bổ sung assertion chặt chẽ trong Check 4: chu kỳ bắt đầu tại Check 3 đạt đúng 4 command thành công, 4 projectile release, 4 damage event và 2 target death trước khi reset.

### N2 — Chuẩn hóa hồ sơ bằng dữ liệu thực tế
1. **Phân biệt Git HEAD rõ ràng**:
   - `18c0890268ca0837bfaf9639a785b2ddcd002555`: Commit trước đó (`HEAD~1` / milestone harness ban đầu).
   - `1df6ce821121ce991e0f54a427deb96bf18c6425`: Git HEAD hiện tại của nhánh `main` tại thời điểm chạy và đóng gói bảo trì tooling.
2. **Khớp mã băm thực tế của mã nguồn sản xuất và tooling**:
   - `Assets/_Game/Entities/Hero.cs`: `DFFDEBEEF37832278DD0D801D046681A2B97897AE6CA2B84428D5AF551A708E1`
   - `Assets/_Game/Combat/HeroSkillDecisionController.cs`: `D3D137EEB00806C116579E4310D3A5F1CFBAECB57CDE1AC43B61FD3A38BE81F1`
   - `Assets/_Game/Combat/ProjectileController.cs`: `578D506AFC44C99722738A66DAAA030A7C3D44695AFF6B7E2D404124B97D299A`
   - `Assets/_Game/Combat/SkillExecutor.cs`: `C5BEF47CE099EAA64D0617FF5CCFB5A30E3FF260549D4CF937A476F535598B34`
   - `Assets/_Game/Core/BattleManager.cs`: `3A021DE3B78D985A7B51715CD4743F44202A229F9EEBDFEBECF02B0D6DDF6448`
   - `Tools/Verification/P09/tltd_save_guard.ps1`: `AFD8EE2E23A17995879DCE4578629D98643561090F8F03F061C1306BA6D41997`
   - Harness sau sửa: tính toán trực tiếp từ file thực tế và ghi vào Manifest SHA256.
3. **Độ chính xác thông số quan sát**:
   - Thời gian chờ xác minh đóng băng khi pause trong Check 2 là $0.5\text{s}$ (khớp chính xác với `pauseWait < 0.5f` trong source code).
4. **Định dạng Patch chuẩn hóa**:
   - Patch được tạo theo chuẩn UTF-8 không BOM với đường dẫn tương đối `a/...` và `b/...`.
