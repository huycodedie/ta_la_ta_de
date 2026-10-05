# QUYẾT ĐỊNH TECH LEAD — P09-B DASH FOUNDATION

Ngày: 04/10/2026. Vai trò review: Codex / Tech Lead.

**P09-B DASH FOUNDATION = ACCEPTED / LOCKED trong phạm vi runtime và fixture đã review. F-SAVE-P09B-01 = CLOSED / RECOVERY ACCEPTED. Không yêu cầu Chủ Dự Án kiểm tra game lại.**

## 1. Phạm vi quyết định

Quyết định này tổng hợp các vòng review source, kiểm thử tự động, xác nhận trực tiếp của Chủ Dự Án và gói recovery cuối. N-META tiếp tục CLOSED; journal preflight tiếp tục FIX VERIFIED. M1–M7, Reset ba lần không có lỗi đỏ mới, Pause → Resume và Dash hoàn tất tiếp tục USER_VERIFIED.

LOCK áp dụng cho nền tảng Dash và công cụ/fixture theo các phiên bản đã review. Đây không phải phê duyệt triển khai Dash cho toàn bộ skill asset sản xuất, thay đổi scene chính hay bắt đầu một hạng mục mới. P08, P09-A và các quyết định runtime trước giữ nguyên phạm vi nghiệm thu của chúng.

Review thực hiện trên các file được bàn giao. Codex không chạy Unity, PowerShell hoặc Registry Windows; không trực tiếp kiểm tra trạng thái hiện tại trên máy Chủ Dự Án.

## 2. Gói recovery được đối chiếu

| Thuộc tính | Kết quả |
|---|---|
| ZIP | `review_package_p09b_recovery_result.zip` |
| Kích thước | 6,493 bytes |
| SHA256 | `2DCF00E47A80B955345E42A7B4F8D2F6D4CCEC70824C57C5FE254D5C0EC9E07C` |
| Tính toàn vẹn | CRC hợp lệ; 6/6 payload khớp manifest; 7 file gồm manifest; không có file ngoài danh mục |
| Recovery Run ID | `REC-P09B-20261004-151227` |
| Snapshot sau recovery | `2026-10-04T15:14:04.823+07:00` |
| Scope | `HKCU\Software\Unity\UnityEditor\DefaultCompany\TLTD` |

### Đối chiếu độc lập từ dữ liệu trong gói

- Snapshot sau recovery có **24 tên value duy nhất**, gồm 17 DWord và 7 Binary.
- **24/24 tên, Registry kind và digest dữ liệu khớp baseline trước phiên test**. Digest ở snapshot đã che dữ liệu là 16 ký tự hex đầu của SHA256; đây không phải so sánh trực tiếp toàn bộ byte Registry đang chạy.
- Cả **56 value Added** trong audit trước đã vắng mặt; cả **4 value DataChanged** đã trở về digest baseline. Không thiếu hoặc thừa tên value so với baseline.
- Metadata journal ghi `VERIFIED`, `RestoredTimestamp = 2026-10-04 15:13:18`; baseline gốc vẫn mang thời gian `2026-10-03 23:39:13`.
- BackupHash baseline `557B4739A7AEE4B6728C0CA0E8A653E755C8D17362B4699E2037D3B2D889DE71` khớp mốc đã review.

### Kết quả được Antigravity báo cáo qua metadata

| Hạng mục | Dữ liệu bàn giao |
|---|---|
| Chấp thuận của Chủ Dự Án | Báo cáo Antigravity ghi lúc 15:11:37 ngày 04/10; Codex không trực tiếp chứng kiến bước chấp thuận này |
| Checkpoint rollback trước recovery | 80 values; tạo lúc 15:12:28; mục đích `PRE_RECOVERY_ROLLBACK` |
| Checkpoint BackupHash | `7684F5878854E0724A407CC455A6F9C951597D190A6B4BDECF6B27AAA0526197` |
| Restore / Compare | Metadata ghi exit 0 / exit 0; `DIFF_ZERO_VERIFIED` |
| Subkeys sau recovery | Audit summary ghi `PostRecoverySubKeyCount = 0` |
| Preflight sau recovery | Metadata ghi `Allowed = true`, `Status = VERIFIED` |

Checkpoint rollback mang trạng thái BACKED_UP là hợp lệ theo mục đích lưu bản trước recovery. Không nhầm checkpoint này với journal đang được launcher sử dụng, được báo cáo là VERIFIED.

## 3. Giới hạn bằng chứng và quyết định đóng finding

Gói không chứa raw log/stdout/stderr của các lệnh Backup, Restore, Compare và preflight. Vì vậy, exit code nêu trên là kết quả được báo cáo trong metadata, chưa có raw command log để đối chứng độc lập. Bản backup rollback đầy đủ cũng không nằm trong gói này.

Hai file metadata journal/checkpoint có `SubKeyCount = null` do lấy trường ở cấp ngoài thay vì trong Snapshot. Không diễn giải null thành zero; số zero sau recovery được lấy từ audit summary. `tree_integrity_audit.json` trùng bản của gói trước, nên chỉ là inventory tham chiếu, không phải chứng cứ một lần kiểm tra working tree mới sau recovery.

`RECOVERY_REPORT.md` có lỗi mã hóa tiếng Việt. Nội dung đọc được sau chuyển mã trong bộ nhớ; file gốc được giữ nguyên để bảo toàn checksum.

**Quyết định Tech Lead:** sự nhất quán của snapshot 24/24 với baseline, việc loại bỏ đủ 56 value phát sinh, phục hồi 4 digest và metadata journal VERIFIED đủ để chấp nhận recovery trong hồ sơ này. F-SAVE-P09B-01 được đóng. Thiếu raw log và lỗi mã hóa là phần lưu trữ cần bổ sung nếu dữ liệu còn tồn tại; không chặn nghiệm thu chức năng và không yêu cầu chạy lại recovery để tạo bằng chứng thay thế.

Quyết định không đồng nghĩa “mọi raw log đều đầy đủ”, “toàn bộ byte Registry đã được Codex xác minh” hoặc “working tree hiện tại được đo lại”. Snapshot chỉ mô tả mốc 15:14:04 ngày 04/10/2026, không bảo đảm trạng thái ở mọi thời điểm sau đó.

## 4. Giữ nguyên lịch sử và nghiệm thu trước

| Hồ sơ | Trạng thái giữ nguyên |
|---|---|
| P09-B Gate 1 | 16/16 đã được công nhận |
| Natural Play Mode | 6/6 đã được công nhận |
| Queue lifecycle / harness smoke | 5/5 và 5/5 đã được công nhận |
| N-META / AST assertions | CLOSED; 7/7 cases, 18/18 assertions theo log đã review |
| Journal preflight | FIX VERIFIED; 16/16, gồm 8 unit và 8 integration cases |
| Gameplay GUI | USER_VERIFIED theo xác nhận trực tiếp của Chủ Dự Án |
| GUI cũ PID 25360 | `USER_REPORTED_PASS / RESTORE_UNRESOLVED` tại phiên cũ; không sửa thành historical Restore PASS |
| Recovery hiện tại | Sự kiện riêng lúc 15:13:18 ngày 04/10/2026, được công nhận trong quyết định này |

Không dùng kết quả recovery mới để lấp đoạn log bị cắt của PID 25360. Giữ nguyên các phiên timeout và exit code lịch sử, bao gồm E2 P09-A `SCENARIO_PASS_EXIT_TIMEOUT` / exit 2.

## 5. Mốc source được nghiệm thu

Các SHA256 dưới đây được đối chiếu trên source đã bàn giao ở `review_package_p09b_metadata_evidence_closeout.zip`. Chúng xác định phiên bản được LOCK, không phải khẳng định máy người dùng hiện đang chứa đúng những byte này.

| Runtime file | SHA256 |
|---|---|
| `Assets/_Game/Combat/HeroSkillDecisionController.cs` | `90FDFFA889B756D31930B848E156FF77E94CCC3EF34869485501A8C03A792565` |
| `Assets/_Game/Combat/SkillExecutionTypes.cs` | `002AC95694EE43B3DBF8AD8D0529096E0B074477300AA75BFC338BC136A2FC52` |
| `Assets/_Game/Combat/SkillExecutionValidator.cs` | `040E84DF4C89C550200E549ED00B63191BFE89FD865F543B39EADE3E37DF0424` |
| `Assets/_Game/Combat/SkillExecutor.cs` | `283E2081A6B815C418945D65D552D74559E4B337A9B3191C0AF897D67664407C` |
| `Assets/_Game/Data/SkillDefinitionSO.cs` | `28366290C3D4765CA94BA3CBF722FE28030A8E94457D7C49931B8B6305AED5B4` |
| `Assets/_Game/Entities/Components/MovementComponent.cs` | `65853F6065050C51C0528C8F09E2C607FEC7B83131B90568BBD206114312DFDF` |
| `Assets/_Game/Entities/Entity.cs` | `17D7B30BEF373BFA26009AD75D0E7A4D51519C8F9E567FBA1D60FC7EDB2E6156` |

Mốc tooling được giữ:

- Manual harness: `DDF010083BA1F7A105E8EC3E46FE69010B29CB558154DA51CE079EB25D9D85C8`.
- R6: `F7D142D8F36F009AE2920440BBC144C6052D2BE5E911EEFE0481833202C69A46`.
- Launcher có preflight: `3F19534A0F7B6216EB2CBAD2EFF43F2371CDEC6F535191044147ECC14D6F16FA`.
- Preflight helper: `0FB6A8894AF8217C59D6A2E809990B2156369BB167B8AF73D83FDA8726B089E4`.
- Shared Save Guard: `AFD8EE2E23A17995879DCE4578629D98643561090F8F03F061C1306BA6D41997`.

Gói nguồn và bằng chứng liên quan:

| ZIP | SHA256 |
|---|---|
| `review_package_p09b_metadata_evidence_closeout.zip` | `1A4E9427A76685D1DB2532C767F6078A83C209249A7D0C74B6EE19C2C87D8A44` |
| `review_package_p09b_last_evidence.zip` | `C6DC81F4F2ED33A4240A60AA2BA4D6689FBBD3530F9CAF5FCE880F80E370ECEC` |
| `review_package_p09b_save_guard_preflight.zip` | `D0E62C66CADC7CFE0BA1364481979F54F1AE501E8FC208265DACC6E215751089` |

## 6. Bàn giao tiếp theo

Antigravity ghi quyết định này vào tài liệu dự án và handoff hiện hành, bảo toàn source, save và working tree. Chỉ lưu bổ sung raw log có sẵn, tạo bản báo cáo UTF-8 đọc được và ghi rõ dữ liệu nào không còn. Không mở Unity, chạy suite hay phục hồi Registry thêm lần nữa để hoàn thiện hồ sơ.

Không cần một vòng phê duyệt lại P09-B cho công việc lưu trữ đó. Phạm vi công việc tiếp theo cần lấy từ roadmap đã thống nhất; quyết định này không tự cấp phép triển khai hạng mục mới.
