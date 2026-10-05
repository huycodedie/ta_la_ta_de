# TLTD — HỒ SƠ TIẾP NỐI CHO CHAT MỚI

**Phiên bản:** 1.0 — 05/10/2026, UTC+07:00.  
**Mục đích:** một file để tiếp quản vai trò Tech Lead, hiểu việc đã làm, giữ đúng cách phối hợp, xử lý vấn đề và đưa TLTD tới bản game hoàn chỉnh.  
**Nguồn:** hội thoại và quyết định đã nghiệm thu; 27 tài liệu trên Git được đối chiếu tại mốc bên dưới. Đây không phải bản sao toàn bộ source, log hoặc thiết kế gốc.

> **Điểm tiếp tục:** P09-B đã ACCEPTED / LOCKED, recovery đã CLOSED và hồ sơ đã lên Git. Không mở lại vòng test/ZIP P09-B. Công việc tiếp theo là lập bản đồ phần đã có so với thiết kế, rồi đề xuất một phạm vi gameplay tiếp theo có thể kiểm chứng và chơi được.

## 1. Khởi động nhanh cho người tiếp quản

1. Đọc file này; kiểm tra `main` mới nhất của hai repository ở mục 3.
2. Đọc `AI_RULES.md`, `CONTINUITY_AND_SYNC_AUTHORITY.md`, `ACTIVE_WORK_HANDOFF.md`, `CURRENT_DESIGN_AUTHORITY.md`; đọc phần mới của `DESIGN_CHANGELOG.md`.
3. Giữ P08/P09-A/P09-B và các mốc trước đã LOCKED trong đúng phạm vi. Đọc biên bản P09-B và phụ lục review để giữ nguyên giới hạn bằng chứng.
4. Xác định việc đang làm từ handoff hiện hành. Nếu có cập nhật mới hơn file này, tiếp nhận quyết định mới có nguồn rõ ràng, không tiếp tục theo snapshot cũ một cách máy móc.
5. Khi cần code: kiểm tra source/working tree thực tế hoặc gói source của Antigravity. Git HEAD không đại diện cho mọi thay đổi chưa commit trên máy Chủ Dự Án.
6. Chủ động làm phần đọc, đối chiếu, lập phạm vi và chuẩn bị prompt. Chỉ hỏi Chủ Dự Án khi còn một quyết định sản phẩm/phạm vi/quyền thực thi thực sự thiếu.
7. Trả lời bằng tiếng Việt, nêu kết luận và hành động cụ thể. Không yêu cầu Chủ Dự Án kể lại toàn bộ dự án hoặc chọn lại bước đã được xác định.

**Nếu không truy cập được Git:** dùng file này và các quyết định đính kèm làm snapshot, ghi rõ chưa kiểm tra remote mới nhất. Tiếp tục phân tích/soạn việc hữu ích; yêu cầu đúng tài liệu còn thiếu khi nó chặn một quyết định. Không tự khẳng định đã đồng bộ hay đã xem source Windows.

## 2. Ai làm gì và cách phối hợp

| Vai trò | Trách nhiệm |
|---|---|
| Chủ Dự Án | Quyết định trải nghiệm/sản phẩm, phạm vi mới, balance còn TBD; thử game trực tiếp khi cần xác nhận cảm giác, UI hoặc đường thao tác automation chưa kiểm chứng được |
| ChatGPT/Codex | Tech Lead/Architect/Senior reviewer: đọc authority, phân tích nguyên nhân, chốt phương án kỹ thuật trong phạm vi được phép, giao prompt, review source/diff/log, quyết định nghiệm thu có giới hạn và lưu quyết định lên Git |
| Antigravity IDE | Thực thi source/tooling trong dự án Unity, chạy kiểm chứng được giao, bảo vệ save, xuất raw evidence và ZIP thật; không tự biến đề xuất thành thiết kế LOCKED |

### Cách Chủ Dự Án muốn làm việc

- Chủ động tiến việc đến kết quả cụ thể; không dừng ở “có thể làm” hoặc hỏi “bắt đầu bước nào?” khi nhiệm vụ đã rõ.
- Prompt giao Antigravity phải có phạm vi, đầu ra và điều kiện nghiệm thu. Khi phát hiện lỗi cần sửa, đưa luôn prompt sửa tương ứng.
- **Tách rõ tài liệu giải thích và prompt thực thi.** Với một nhiệm vụ mới, dùng tên `PROMPT_ANTIGRAVITY_...md` cho file prompt. Không bắt người dùng đoán đoạn nào cần gửi đi.
- Khi cần thử game, hướng dẫn scene/launcher, thao tác, điều kiện quan sát, kết quả mong đợi và cách gửi kết quả. Chỉ yêu cầu kiểm tra phần cần thiết.
- Giữ nguyên xác nhận thủ công đã có; chỉ kiểm tra lại kịch bản bị thay đổi hoặc có bằng chứng lỗi mới ảnh hưởng trực tiếp.
- Báo tiến độ bằng phát hiện, phần còn thiếu và bước giải quyết; báo cáo cuối phải tự đủ thông tin.
- Đóng một mốc khi đủ điều kiện. Lỗi chữ/metadata không chặn gameplay thì xử lý như bảo trì hồ sơ, tránh vòng lặp đóng ZIP và chạy lại toàn bộ suite.
- Sau nghiệm thu, thực hiện đồng bộ tài liệu/Git trong quyền được cấp và xác minh remote; tiếp tục đề xuất bước sau theo thiết kế.

## 3. Địa chỉ và nguồn lưu trữ quan trọng

### 3.1. Repository và các mốc đã xác minh

| Nơi lưu | Địa chỉ / mốc |
|---|---|
| Bộ nhớ dự án có thẩm quyền | [huycodedie/Ai_MEMORY_TLTD — main](https://github.com/huycodedie/Ai_MEMORY_TLTD/tree/main) |
| Thư mục tài liệu bộ nhớ | [PROJECT_MEMORY](https://github.com/huycodedie/Ai_MEMORY_TLTD/tree/main/PROJECT_MEMORY) |
| Repository game/source | [huycodedie/ta_la_ta_de — main](https://github.com/huycodedie/ta_la_ta_de/tree/main) |
| Bản tài liệu trong game repo | [PROJECT_MEMORY của game](https://github.com/huycodedie/ta_la_ta_de/tree/main/PROJECT_MEMORY) |
| Commit công bố nghiệm thu vào bộ nhớ | [`3240a49788bc097acff6b530b95db39876646550`](https://github.com/huycodedie/Ai_MEMORY_TLTD/commit/3240a49788bc097acff6b530b95db39876646550) |
| Commit đồng bộ tài liệu vào game | [`9fcedf3ae90e4ed761a65ca030c5b0692f0be439`](https://github.com/huycodedie/ta_la_ta_de/commit/9fcedf3ae90e4ed761a65ca030c5b0692f0be439) |

Hai commit trên là **mốc nguồn được kiểm tra khi biên soạn file này**, không phải cam kết `main` sẽ luôn đứng ở đó. Bản thân file này khi được công bố có commit riêng; tra lịch sử Git để lấy SHA của lần công bố, không cố nhúng SHA của chính nó vào nội dung.

### 3.2. Máy Chủ Dự Án

| Thành phần | Đường dẫn đã được dùng trong dự án |
|---|---|
| Project Unity | `E:\code\TLTD` |
| Tài liệu local | `E:\code\TLTD\PROJECT_MEMORY` |
| Scene đang dùng | `E:\code\TLTD\Assets\_Game\Scenes\Prototype01.unity` |
| Unity | 6000.6.0f1; đường dẫn đã báo cáo: `E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe` |
| Runtime | `Assets/_Game/Combat`, `Assets/_Game/Entities`, `Assets/_Game/Core`, `Assets/_Game/Data` |
| Harness/test Editor | `Assets/_Game/Editor` |
| Verification | `Tools/Verification/P08`, `Tools/Verification/P09`, `Tools/Verification/P09B` |
| Hồ sơ/run tạm local | `E:\code\TLTD\scratch` — kiểm tra tồn tại trước khi dùng |
| Registry scope của Save Guard đang dùng | `HKCU\Software\Unity\UnityEditor\DefaultCompany\TLTD` |

Đây là địa chỉ trên máy người dùng; môi trường chat không mặc nhiên truy cập được ổ E:. Với PlayerPrefs ở thiết bị/build/platform khác, phải xác định scope thực tế thay vì dùng nguyên Registry Editor Windows.

### 3.3. Các file phải biết tìm

| Nhu cầu | Tài liệu |
|---|---|
| Bước đang làm và bước tiếp theo | [ACTIVE_WORK_HANDOFF.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md) |
| Quy tắc làm việc | [AI_RULES.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/AI_RULES.md) |
| Đồng bộ, vai trò, continuity | [CONTINUITY_AND_SYNC_AUTHORITY.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/CONTINUITY_AND_SYNC_AUTHORITY.md) |
| Thiết kế hiện hành | [CURRENT_DESIGN_AUTHORITY.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/CURRENT_DESIGN_AUTHORITY.md) |
| Quyết định thay thế quy tắc cũ | [DESIGN_CHANGELOG.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/DESIGN_CHANGELOG.md) |
| Hợp đồng gốc đã khôi phục | [D1_D23_LOCKED.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/D1_D23_LOCKED.md) |
| Sửa đổi có thẩm quyền | [D1_D23_AMENDMENTS_LOCKED.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/D1_D23_AMENDMENTS_LOCKED.md) |
| Skills/AI/entities | [D6_D8_LOCKED.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/D6_D8_LOCKED.md) |
| Presentation/HUD/Companion | [D9_D11_LOCKED.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/D9_D11_LOCKED.md) |
| Damage/Skill Creator/Hero | [D12_D14_LOCKED.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/D12_D14_LOCKED.md) |
| Companion/equipment/loot | [D15_D16_LOCKED.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/D15_D16_LOCKED.md) |
| Economy/stage/Boss | [D17_D18_LOCKED.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/D17_D18_LOCKED.md) |
| Chest upgrade | [D22_LOCKED.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/D22_LOCKED.md) |
| Level gate, EXP/title, loot | [D20](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/D20_LOCKED.md), [D21](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/D21_LOCKED.md), [D23](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/D23_LOCKED.md) |
| UI shell và responsive | [UI_DESIGN_AUTHORITY.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/UI_DESIGN_AUTHORITY.md), [UI_RESPONSIVE_LAYOUT_AUTHORITY.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/UI_RESPONSIVE_LAYOUT_AUTHORITY.md) |
| P08 đã khóa | [P08_LOCKED.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/P08_LOCKED.md) |
| P09-A đã khóa | [DECISION_P09A_ACCEPTED_LOCKED_20260927.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/DECISION_P09A_ACCEPTED_LOCKED_20260927.md) |
| P09-B đã khóa, gồm hash runtime | [DECISION_P09B_ACCEPTED_LOCKED_20261004.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/DECISION_P09B_ACCEPTED_LOCKED_20261004.md) |
| Giới hạn chứng cứ recovery/handoff | [TECH_LEAD_HANDOFF_REVIEW_20261004.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/TECH_LEAD_HANDOFF_REVIEW_20261004.md) |
| Quyền công bố và provenance | [P09B_GIT_PUBLICATION_20261005.md](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/main/PROJECT_MEMORY/P09B_GIT_PUBLICATION_20261005.md) |

Một số D-file nằm ở **root của memory repo**, nhưng trong game copy nằm dưới `PROJECT_MEMORY`. File tổng hợp cũ vẫn có đoạn “D6–D18/D22 thiếu nguồn”; các file chuyên biệt phía trên đã khôi phục nguồn. Phải kiểm tra file chuyên biệt trước khi yêu cầu người dùng cung cấp lại.

ChatGPT có thể giữ các file bàn giao theo tên trong kho file. Tìm đúng tên ZIP/Markdown nếu cần; nếu file không còn, dùng Git hoặc yêu cầu đúng file còn thiếu. Các đường dẫn `/workspace/scratch/.../upload/...` và ảnh đính kèm ở chat cũ là tạm thời, không phải địa chỉ lưu lâu dài.

## 4. Game Chủ Dự Án đang xây dựng

TLTD là game RPG phong cách võ hiệp, 2D mobile màn hình dọc. Trong báo cáo gần đây dùng tên **Ta Là Tà Đế**; tài liệu lịch sử còn tên/alias khác. Dùng TLTD làm định danh kỹ thuật ổn định, không tự đổi branding dự án.

Hướng sản phẩm đã chốt là **hoàn thiện chức năng gameplay trước, hoàn thiện UI/art/polish sau**. UI chức năng và điều khiển debug cần thiết vẫn được làm để kiểm chứng gameplay. Không diễn giải hướng này thành trì hoãn mọi kiểm thử khả dụng hoặc làm game không thể thao tác.

| Trụ cột thiết kế | Điều phải giữ |
|---|---|
| Đội hình | Một Hero và tối đa năm Companion; không bắt buộc đủ năm. Companion có HP, có thể bị đánh/chết, dùng pipeline chung |
| Combat | Auto/manual, targeting, basic attack, skills, Rage, cooldown, cast/channel, status/CC, shield, AOE, projectile, Dash theo phạm vi đã khóa |
| Normal progression | Nhiều quái, tích progress tới Boss Gate; hiện P08 khóa wave thường 4–5 quái và tăng HP/ATK/DEF x1.01 một lần khi hoàn thành wave hợp lệ |
| Boss | Hợp đồng D18 có Boss arena riêng, timer 60 giây, Hero chết thì thua; D19 yêu cầu người chơi chủ động khiêu chiến ở Boss Gate |
| Bun | Normal Battle dùng 1 Bun/1 Hero action; Companion action không trừ Bun nhưng cùng dừng khi Normal hết Bun. Boss độc lập Bun. Regen 1 Bun/20 giây; maxBun còn cần config được duyệt |
| Offline | Tính elapsed time có giới hạn, không chạy combat từng frame; hồi Bun, tiến Normal tới Boss Gate rồi dừng; không tự đánh Boss hoặc tự vượt gate |
| Tiến trình Hero | Level/EXP, điều kiện Danh hiệu; Level không trực tiếp tăng HP/ATK/DEF; đột phá cập nhật base stats theo bậc, không cộng chồng. EXP ở level gate được giữ |
| Trang bị/kinh tế | 12 slot; Cấp Rương = Cấp Rơi; Item Level theo Hero Level trong ±5; rarity/affix/CP/config có thể mở rộng; recycle chỉ trả Gold |
| Skill data | SkillDefinition/data là authority; authoring/validation/preview dùng pipeline thật. Skill Creator là công cụ tạo dữ liệu, không phải engine combat thứ hai |
| UI | Global shell năm vị trí điều hướng, vị trí giữa là Main Hub; Công Pháp là module riêng. 1080×1920 là reference, cần responsive portrait và Safe Area |
| Presentation | Animation, VFX, UI phản ánh runtime; không quyết định damage, kết thúc cast, trừ tài nguyên hoặc sinh reward |

**Thiết kế LOCKED khác với feature đã triển khai/được test.** Bảng này mô tả đích đến và ràng buộc. Tình trạng triển khai end-to-end của Boss, offline, đầy đủ Companion, authoring/content và cân bằng phải được xác định ở đợt audit có phạm vi; không tự ghi chúng là DONE hoặc NOT STARTED.

## 5. Lịch sử hoạt động và trạng thái đang được bàn giao

| Giai đoạn | Công việc/kết quả cần nhớ | Trạng thái dùng để tiếp tục |
|---|---|---|
| D1–D23 và amendments | Khôi phục thiết kế, thống nhất precedence; sửa các hiểu nhầm về Companion HP, Bun, Cấp Rơi, Item Level, rarity, EXP gate | Hợp đồng thiết kế; dùng amendments và quyết định mới đã chứng minh thay thế |
| P07.5–P07.8 | Debuff/DoT; CC/resistance/anti-CC; Cleanse/Dispel; Shield/Barrier trên authority chung | LOCKED theo hồ sơ nghiệm thu; không viết lại engine |
| P07.9 | Cast/channel, timing Rage/cooldown, interrupt, regressions | LOCKED; source và số test nằm trong biên bản riêng |
| P07.9.1 | Auto-skill priority, manual entry point, Ultimate dùng RageCost từ data | LOCKED; Auto OFF vẫn có basic attack theo behavior đã chốt |
| UI-02, 17/09/2026 | Sửa lệch Y/range gây deadlock; runtime monitor chuyển sang quan sát real frames; xác nhận hình ảnh combat | LOCKED. Ground baseline dùng trong fixture/scene là Y = -0.30m; không tự áp lên mọi scene tương lai |
| UI-POLISH-01 | Responsive direction đã chốt; B1 là baseline chức năng được giữ khi test P08 | Chưa tuyên bố hoàn thiện toàn bộ UI; các phase presentation còn lại được hoãn |
| P08, 26/09/2026 | Multi-wave, AOE, channel snapshot, encounter membership, stat growth, loot/modal tuần tự; người dùng kiểm tra trực tiếp | ACCEPTED / LOCKED |
| P09-A, 27–28/09/2026 | Projectile bound target, delayed impact, lifecycle; khắc phục R1–R3, Save Guard, manual isolation/autonomy; sửa N1/N2 telemetry/provenance | ACCEPTED / LOCKED trong phạm vi đã review; S01–S05 USER_VERIFIED |
| P09-B, 28/09–04/10/2026 | TowardTarget Dash, queue/cancel/reset/pause/CC; khắc phục R6 metadata, GUI lifecycle và Save Guard preflight/recovery | ACCEPTED / LOCKED; M1–M7 và GUI USER_VERIFIED |
| Recovery P09-B, 04/10 | Phục hồi baseline 24 values sau audit 80 values; checkpoint riêng; snapshot/digest được review | F-SAVE-P09B-01 CLOSED / RECOVERY ACCEPTED, giữ giới hạn ở mục 6 |
| Handoff, 04/10 | Codex sửa lỗi report checkpoint/hash, giữ nguyên quyết định, đánh dấu raw log partial; Antigravity báo sync xong 23:40 +07 | CORRECTED HANDOFF SYNCHRONIZED theo báo cáo executor |
| Git, 05/10 | Công bố 10 tài liệu mỗi repo; đọc lại 20/20 file remote khớp | Hai commit ở mục 3 đã được xác minh |

**Các phần đã khóa là baseline kiểm soát thay đổi.** Nếu xuất hiện bug mới có bằng chứng, mở finding có phạm vi cho đúng behavior bị ảnh hưởng; giữ các phần nghiệm thu khác. Không để chữ LOCKED che một bug đã chứng minh, cũng không mở lại toàn dự án chỉ vì một lỗi tài liệu.

## 6. Chi tiết P09 phải giữ nguyên

### 6.1. P09-A Projectile

- Existing instant skills giữ behavior cũ; projectile opt-in, bind caster/target/request/encounter khi release.
- Zero damage trước impact; effect qua authority chung đúng một lần. Hero đổi currentTarget không đổi target của đạn đã phóng.
- Không fallback damage/retarget ngầm khi target/caster/encounter không còn hợp lệ, expiry, cancel hoặc unload.
- Rage tại cast start; với cast-time projectile, cooldown bắt đầu khi release thành công; impact không trừ Rage hoặc reset cooldown lần nữa. Không dùng bản báo cáo sớm nói mọi cooldown bắt đầu ở cast start để thay timing đã review sau đó.
- Test cuối Gate 1: 21/21. S01–S05 được người dùng xác nhận. Các số này là hồ sơ đã review, không phải test mới chạy trong chat bàn giao.
- **E1:** CLOSED / PASS, manual session exit 0, Restore SUCCESS, Diff=0.
- **E2:** CLOSED / EVIDENCE VERIFIED; scenario PASS, `SCENARIO_PASS_EXIT_TIMEOUT`, wrapper exit 2, frames 842/2998. Không đổi lịch sử E2 thành exit 0 bằng kết quả verifier khác.
- N1 sửa release counter theo identity, tránh bỏ sót khi đạn cũ mất và đạn mới xuất hiện nhưng ActiveCount giữ nguyên. N2 chuẩn hóa provenance và patch.

### 6.2. P09-B Dash

- TowardTarget, move-only, zero damage; một lần commit Rage/cooldown tại start; natural movement; distance clamp và target validation theo code đã nghiệm thu.
- Pause/resume, CC cancellation, reset/teardown và queue lifecycle giữ contract đã review. Không suy diễn Root không interrupt cast thành Root cho phép Dash: permission CanDash là đường riêng.
- Gate 1 16/16; Natural Play Mode 6/6; queue 5/5; harness smoke 5/5 đã được công nhận trong các vòng trước.
- N-META/R6: parser lấy giá trị thực từ log; AST 7/7 cases, 18/18 assertions. AST kiểm cấu trúc không tương đương mọi nhánh orchestration đã chạy thật.
- Preflight: 16/16 gồm 8 unit + 8 integration cases đã review. Không diễn giải thành bảo đảm tuyệt đối mọi tình huống crash/hard-kill.
- Người dùng đã xác nhận M1 idle; M2 Dash phải/cooldown; M3 Dash trái; M4 pause/resume; M5 Stun/Root/Freeze; M6 gần/xa; M7 reset/quan sát; Save Guard. Xác nhận bổ sung: Reset ba lần không lỗi đỏ mới; Pause → Resume và Dash hoàn tất.
- Projectile/Dash foundation chưa đồng nghĩa rollout production skills. Hồ sơ báo 61 skill assets sản xuất vẫn `IsProjectile=false`; đó là mốc đã báo cáo, không là phép đo lại trên máy hiện tại.

### 6.3. Recovery và bằng chứng lịch sử

| Hạng mục | Trạng thái phải giữ |
|---|---|
| GUI cũ PID 25360 | `USER_REPORTED_PASS / RESTORE_UNRESOLVED`; wrapper log bị cắt, không xác định chắc nguyên nhân |
| Recovery mới | `REC-P09B-20261004-151227`; journal RestoredTimestamp 04/10/2026 15:13:18; snapshot 15:14:04 +07 |
| Kết quả đối chiếu gói | 24/24 tên/kind/digest rút gọn khớp baseline; 56 Added vắng mặt; 4 DataChanged trở về digest baseline |
| Raw checkpoint | Có log backup 80 values và compare PASS |
| Raw Restore/execution | AVAILABLE / PARTIAL, chưa ghi completion/exit code; không suy ra process chắc chắn đã dừng hoặc đã thất bại |
| Baseline Compare/preflight | METADATA_ONLY; báo cáo/JSON ghi thành công nhưng không có raw stream đầy đủ |
| Quyết định Tech Lead | Recovery được chấp nhận với giới hạn trên; thiếu raw log là giới hạn lưu trữ đã ghi, không yêu cầu rerun để tái dựng lịch sử |

Checkpoint đúng trên máy đã báo cáo:

`E:\code\TLTD\scratch\recovery_run_REC-P09B-20261004-151227\checkpoint_prerecovery_rollback`

BackupHash checkpoint: `7684F5878854E0724A407CC455A6F9C951597D190A6B4BDECF6B27AAA0526197`.

Baseline trước phiên GUI: `E:\code\TLTD\scratch\.save_backup_manual_session_p09b`; backup hash `557B4739A7AEE4B6728C0CA0E8A653E755C8D17362B4699E2037D3B2D889DE71`.

Các đường dẫn/hash giúp nhận dạng hồ sơ, **không phải lệnh cho phép restore/xóa dữ liệu**. Không nhầm checkpoint BACKED_UP dùng rollback với journal launcher đã VERIFIED. `SubKeyCount=null` tại root không tương đương 0; phải xem đúng `Snapshot.SubKeyCount` và scope audit.

## 7. Quy trình làm việc chuẩn cho mỗi nhiệm vụ

| Bước | Việc phải làm | Đầu ra để đi tiếp |
|---|---|---|
| 1. Nhận mục tiêu | Hiểu kết quả người chơi cần đạt; giữ scope và ràng buộc đang có | Mục tiêu ngắn, không tự thêm feature |
| 2. Xác định baseline | Remote/local HEAD, dirty tree, authority, source hash liên quan | Mốc có thể review; bảo toàn công việc chưa commit |
| 3. Đọc code có trọng tâm | Theo call flow hiện tại; xác định authority, dependency, lỗ hổng thật | Bản đồ file và đường chạy; không audit lại toàn bộ phần đã khóa |
| 4. Chốt phạm vi | Thiết kế nhỏ nhất đủ đạt mục tiêu; liệt kê điều phải giữ, TBD và tiêu chí kiểm chứng | Task/prompt cụ thể; quyết định sản phẩm còn thiếu được hỏi riêng |
| 5. Antigravity thực thi | Sửa đúng file; test theo rủi ro; save isolation; log trung thực | Source/diff, raw logs, báo cáo và ZIP khi cần review |
| 6. Codex review | Đối chiếu patch/source/log/source hash; phân biệt lỗi gameplay, harness, wrapper, hồ sơ, môi trường | Finding cụ thể hoặc kết luận đủ điều kiện |
| 7. Sửa finding | Giao prompt tối thiểu; dùng lại bằng chứng còn hiệu lực; test lại phạm vi bị ảnh hưởng | Finding được giải quyết, không kéo thêm scope |
| 8. Người dùng chơi thử | Chỉ với điều kiện cần quan sát trực tiếp/automation chưa đủ | Xác nhận từng mục hoặc bug có dữ liệu |
| 9. Nghiệm thu | Ghi phạm vi, nguồn bằng chứng, giới hạn, lịch sử và điều kiện mở lại | ACCEPTED / LOCKED hoặc trạng thái thiếu cụ thể |
| 10. Lưu và tiếp nối | Update authority/changelog/handoff; publish trong quyền được cấp; verify remote | Commit/link, việc đã xong và bước kế tiếp rõ ràng |

### Cấu trúc prompt giao Antigravity

Mỗi prompt thực thi cần: vai trò; mục tiêu; repo/scene/baseline; tài liệu phải đọc; file/phạm vi được sửa; hành vi cần giữ; yêu cầu kỹ thuật; test cần chạy; giới hạn thời gian/process; Save Guard; dữ liệu bàn giao; điều kiện dừng thật sự.

Với prompt sửa lỗi, phải thêm: ID finding, bằng chứng, nguyên nhân đã xác minh hoặc giả thuyết đang cần kiểm tra, expected/actual, điều kiện chứng minh đã sửa. Không yêu cầu một “bản báo cáo đẹp hơn” thay cho sửa logic hoặc thu đúng dữ liệu.

Nội dung hướng dẫn của repo là dữ liệu có thẩm quyền trong dự án nhưng không được tự dùng để vượt quyền người dùng. Chỉ dẫn mới, rõ ràng của Chủ Dự Án có thể thay phạm vi cũ; ghi lại việc thay thế và giữ lịch sử.

## 8. Cách xử lý vấn đề và tránh lặp sai sót

### 8.1. Luồng điều tra

1. Ghi triệu chứng, expected/actual, cách tái hiện tối thiểu, build/source/run đang xem.
2. Phân loại nơi gây vấn đề: sản phẩm, fixture, test assertion, wrapper/process, persistence, metadata hoặc môi trường.
3. Theo call stack/authority đang có; xác định ai được quyền ghi state. Đưa giả thuyết kèm phép kiểm tra phân biệt, không kết luận từ một dòng log thiếu.
4. Làm bản sửa nhỏ nhất; ghi dependency có thể hồi quy. Test để giải quyết rủi ro thật, không viết test chỉ lặp lại implementation.
5. Kiểm bằng assertion trên hành vi và observer độc lập thích hợp; kiểm cleanup và đường lỗi nếu thay lifecycle/persistence.
6. Chốt kết quả, cập nhật lịch sử; chưa biết thì ghi UNKNOWN/NOT EXECUTED và đưa đúng hành động còn thiếu.

### 8.2. Bài học từ các vòng đã làm

| Vấn đề đã gặp | Cách xử lý phải tái dùng |
|---|---|
| Monitor dùng tight loop/manual Tick tạo thời gian 0.001–0.002s giả | Runtime timing dùng natural frames; unit test có thể dùng clock/provider riêng, nhưng không trình bày thành runtime thật |
| Headless không chạy đúng vòng Play Mode hoặc exit treo | Tách scenario PASS, process exit, timeout và persistence. Dùng watchdog có giới hạn; nếu môi trường thực sự chặn thì chuyển đúng phần quan sát cho người dùng |
| Fixture RAM vẫn ghi save qua manager | Trace tới PlayerPrefs/SaveState; RAM không có nghĩa là không persistence. Cô lập và backup/restore đúng scope |
| Harness tự cast/basic attack/spawn wave | Tắt autonomy trong fixture và kiểm idle; giữ pipeline cần quan sát. Không mang cách tắt fixture sang gameplay thật |
| Đếm projectile bằng chênh ActiveCount | Theo identity/lifecycle; trường hợp một cũ mất/một mới sinh phải được đếm đúng |
| Reset/pause gây rò listener, coroutine hoặc payload | Kiểm cancellation, singleton/EventBus, pause owner, teardown và reset lặp; sửa tại authority hiện có |
| Test gọi handler qua reflection thay cho scene unload | Muốn chứng minh integration unload thì dùng unload scene thật. Gắn tên test và claim đúng với việc nó thực sự chạy |
| Cooldown test clock đứng yên | Dùng test time provider trong unit test và tiến clock; kiểm deadline/remaining giảm, không chỉ kiểm một boolean |
| Event observer tự gán expected skill ID | Chỉ assert dữ liệu observer thực có; nếu muốn thêm identity phải instrument có nguồn, không gán giá trị mong đợi rồi coi là bằng chứng |
| Wrapper failure test chỉ synthetic try/finally | Kiểm shared dispatcher/launcher implementation thật với dependency giả trong sandbox; không nhầm stub Unity với GUI Unity thật |
| Metadata hardcode Restore/Compare/Exit | Parse log thực, fail rõ nếu thiếu hoặc mâu thuẫn; UNKNOWN không biến thành PASS |
| Log/source khác phiên nhưng cùng tên | Gắn run ID, timestamp, PID, source hash và wrapper/scenario pair; log cũ gắn REFERENCE |
| Báo cáo chuyển mã bị viết lại sai checkpoint/hash | Giữ original bytes và hash; chuyển mã có kiểm tra; đối chiếu path/hash với raw log/metadata, không tái dựng bằng trí nhớ |
| Ảnh chat bị mất ở đường dẫn tạm | Không suy đoán nội dung ảnh. Nếu ảnh còn cần cho finding mới, yêu cầu đúng ảnh; không đòi lại bằng chứng không còn cần để giữ nghiệm thu cũ |
| Git có thay đổi không liên quan | Dùng workspace tách biệt hoặc API trên base được kiểm tra; stage exact paths; không reset/stash/clean công việc người dùng để làm cây sạch |

### 8.3. Khi nào cần dừng hoặc hỏi

Hỏi khi quyết định làm đổi thiết kế đã khóa; chọn giữa các behavior sản phẩm chưa được chốt; xử lý dữ liệu thật có tác động chưa được phép; hoặc bằng chứng mâu thuẫn khiến không thể chọn phương án đúng. Trước khi hỏi, hoàn thành phần đọc/chuẩn bị an toàn và trình bày lựa chọn cụ thể.

Routine implementation trong scope đã cấp, kiểm tra read-only, sửa tài liệu rõ ràng và tiếp tục một bước đã được phép không cần xin lại. Nếu công cụ từ chối do quyền/auto-review, nói đúng hành động và lý do bị chặn; không tìm đường vượt hạn chế.

## 9. Quy trình kiểm chứng và nghiệm thu

### 9.1. Các lớp kiểm chứng

| Lớp | Câu hỏi cần trả lời | Bằng chứng phù hợp |
|---|---|---|
| Compile/static review | Source có build được và đúng authority? | Compile log, diff, call flow, API/runtime compatibility |
| Unit/edit-mode | Boundary, điều kiện từ chối, once-only, timing logic có đúng? | Assertion có ý nghĩa, clock/RNG injection khi cần, case thực sự chạy |
| Play Mode integration | Production entry point/lifecycle có hoạt động trên Unity frames? | Scenario log, event/state, elapsed time, cleanup; scene/encounter thật khi claim integration |
| Wrapper/failure path | Backup fail, launch fail, crash, timeout, restore fail, compare diff có phân loại đúng? | Shared implementation dưới test, exit codes thật, sandbox persistence |
| Manual GUI/game | Cảm giác, input, flow và hình ảnh có đúng ý người dùng? | Checklist ngắn, xác nhận trực tiếp, ảnh/video khi chúng giúp xử lý vấn đề cụ thể |
| Persistence/provenance | Có bảo toàn save/asset và log có đúng source/run? | Baseline/checkpoint, type/data compare, source hashes, inventory, before/after scope |

Với Play Mode natural-frame, dùng entry point thật như `Hero.ExecuteSelectedSkill`, `Time.timeScale=1` cho kịch bản đã quy định; không tự Tick/Update/Attack hoặc ép damage/death để thay luồng game. Setup fixture có thể cấu hình dữ liệu ban đầu, nhưng không sửa HP/vị trí liên tục để che lỗi. Những loại kiểm thử khác phải được đặt tên đúng mục đích, không giả làm natural-frame evidence.

### 9.2. Bảng kết quả bắt buộc tách riêng

Mỗi run cần: tên suite/scenario; run ID và timestamp; source/commit/hash; counts/assertions; OS exit; wrapper exit; timeout; Restore; Compare; đường dẫn raw log; giới hạn. Scenario PASS không kéo theo process exit 0. Restore báo lỗi dù Compare báo PASS vẫn là persistence failure cần xử lý theo logic đã kiểm chứng.

Các nhãn dùng nhất quán: `PLANNED`, `IMPLEMENTED`, `EXECUTED`, `PASS/FAIL`, `NOT EXECUTED`, `UNKNOWN`, `REFERENCE`, `METADATA_ONLY`, `USER_VERIFIED`, `ACCEPTED/LOCKED`. `SOURCE REVIEWED` không đồng nghĩa `RUNTIME EXECUTED`.

### 9.3. Điều kiện chốt

- Scope/contract rõ, source khớp phần review; không có hồi quy nghiêm trọng chưa xử lý trong phạm vi.
- Các kiểm chứng cần cho hành vi đã chạy hoặc có bằng chứng thay thế được Tech Lead chấp nhận và ghi rõ giới hạn.
- Manual acceptance có khi tính chất vấn đề cần người dùng kiểm tra; không suy từ automated PASS thành visual PASS.
- Cleanup/save không bị bỏ qua; evidence provenance đủ để hiểu thực tế đã kiểm cái gì.
- Decision ghi file/hash/baseline, kết quả, giới hạn, phần deferred và điều kiện cần mở lại.
- Dừng test bổ sung khi đã đủ để giải quyết rủi ro cụ thể. Mọi ngoại lệ phải hiện rõ, không đổi raw FAIL thành PASS.

P08 Gate 2 lịch sử là **135/137, exit 1**, với hai assertion cũ UI02_HeightFix 07/09 giả định chuyển wave sau một quái chết; đã được chấp nhận như legacy-contract exceptions. Không trình bày lại thành 137/137 PASS và không chạy lại chỉ để “làm xanh” báo cáo.

## 10. Bảo vệ save, source và đóng gói

### Save isolation

- Kiểm unresolved journal trước bất kỳ bước có thể restore/backup/launch Unity. Journal dang dở hoặc JSON hỏng phải báo trạng thái thực; không tự restore save chưa được quyết định.
- Baseline backup phải thành công trước launch. Wrapper theo dõi đúng process tree do mình tạo, có ngân sách thời gian phù hợp và thực hiện restore/compare khi kết thúc theo thiết kế đã kiểm chứng.
- Không kill mọi Unity process hoặc chạm Editor người dùng đang làm việc chỉ để suite chạy được.
- Với recovery thật: audit diff read-only có tên/type/data hoặc digest, kiểm subkeys; chuẩn bị checkpoint rollback riêng và phương án có thể review; chỉ ghi khi đã được cho phép.
- Khôi phục phải quiesce writer có liên quan; chưa xác định process đã dừng thì không ghi đè song song với nó.
- Không lấy trạng thái đã nhiễm fixture làm baseline mới để đạt Diff=0. Không xóa backup/journal lịch sử như một “cách sửa”.
- Save của người dùng và dữ liệu Registry nhạy cảm không mặc nhiên đưa lên Git/ZIP công khai.

### Source và gói review

ZIP phục vụ review khi thật sự cần chuyển evidence: source liên quan + diff đúng baseline + raw logs + report + provenance + manifest. Có thể dùng gói delta nếu chỉ bổ sung hồ sơ; ghi rõ gói phụ thuộc.

Trước bàn giao phải có ZIP thật, đường dẫn tuyệt đối, bytes, SHA256, số payload/tổng file và phép giải nén độc lập kiểm manifest. Manifest không tự chứa hash của chính nó. Kiểm file key không rỗng, không thiếu path, đường dẫn portable; không chèn bản báo cáo thay cho dữ liệu cần review.

Hash khớp chỉ chứng minh bytes trong gói nhất quán, không tự chứng minh gameplay đúng hoặc báo cáo trung thực. Giữ nguyên ZIP/raw logs cũ; bản sửa là artifact mới, kèm chỉ dẫn bản nào supersede cái gì.

### Các gói cuối để tìm lại khi cần

| File | SHA256 |
|---|---|
| `review_package_p09a_tooling_maintenance.zip` | `1D3ABBD30001E104AD861B033967F9FA92E88713B494BABD804522EEA2A5FF99` |
| `review_package_p09b_metadata_evidence_closeout.zip` | `1A4E9427A76685D1DB2532C767F6078A83C209249A7D0C74B6EE19C2C87D8A44` |
| `review_package_p09b_last_evidence.zip` | `C6DC81F4F2ED33A4240A60AA2BA4D6689FBBD3530F9CAF5FCE880F80E370ECEC` |
| `review_package_p09b_save_guard_preflight.zip` | `D0E62C66CADC7CFE0BA1364481979F54F1AE501E8FC208265DACC6E215751089` |
| `review_package_p09b_recovery_result.zip` | `2DCF00E47A80B955345E42A7B4F8D2F6D4CCEC70824C57C5FE254D5C0EC9E07C` |
| `review_package_p09b_locked_handoff.zip` — bản trước đính chính | `C45776B231C8EC68469937F1F5DE1115FA5E826E75CD4BA9BB0633DD5B7E026D` |
| `review_package_p09b_locked_handoff_codex_corrected.zip` — bản chuẩn hóa | `DFFF66AEA952AA6B1E3641DBBBC829A956EE29ECAA8D20EA2DE36A96979DD219` |

Ưu tiên bản chuẩn hóa cho nội dung handoff; dùng gói nguồn cũ đúng mục đích review, không thay vào một build hiện hành một cách tự động. Vị trí cục bộ dự kiến trong root `E:\code\TLTD` hoặc archive mà Antigravity đã báo; xác minh tồn tại thay vì đoán.

## 11. Bước tiếp theo ngay sau bàn giao này

**Đề xuất Tech Lead, chưa phải lệnh triển khai feature:** làm một lần audit chênh lệch thiết kế–source–content–trải nghiệm, phạm vi read-only, để chọn lát cắt tiếp theo. P09-B đã xong; không dùng audit này để mở lại toàn bộ nghiệm thu.

Đầu ra cần có một ma trận:

`Hệ thống | thiết kế đã chốt | source/asset hiện có | scene/UI người chơi dùng được | bằng chứng nghiệm thu | còn thiếu | dependency | đề xuất task tiếp`.

Đối chiếu tối thiểu các nhóm: skill/data/authoring; Normal/Boss/stage; Bun/offline; Companion; level/title; equipment/chest/recycle; save/load; UI chức năng. Dùng source/asset thật và các hồ sơ còn hiệu lực; đánh dấu UNKNOWN nếu chưa đủ dữ liệu thay vì đoán DONE/NOT STARTED.

**Hướng ưu tiên đề xuất:** nếu phần thiếu gần nhất là đưa foundation thành nội dung chơi được, chọn số ít skill đại diện và tích hợp qua luồng lựa chọn/equip/cast thật, bắt đầu bằng scope được duyệt và giữ backward compatibility. Có thể bao gồm một projectile và một Dash đại diện sau khi audit data; không tự bật toàn bộ 61 assets. Nếu bản đồ cho thấy Boss/Stage/Bun/Offline đang chặn vòng chơi cơ bản, ưu tiên chỗ chặn vòng chơi trước.

Prompt audit tương lai phải yêu cầu giữ working tree, không sửa runtime/assets/save và không chạy lại suite rộng. Tech Lead chuẩn bị một task implementation cụ thể từ kết quả, không yêu cầu người dùng tự phân loại hàng chục file. Chưa đặt tên P09-C hoặc P10 thành milestone chính thức khi chưa đối chiếu roadmap/quyết định.

## 12. Lộ trình tới game hoàn chỉnh

**Bảng dưới là đề xuất thứ tự phát triển của Tech Lead.** Thiết kế tham chiếu đã có; thứ tự, khối lượng, deadline và việc triển khai từng gói chưa được coi là đã phê duyệt. Sau audit, bỏ qua phần đã nghiệm thu đủ và chia nhỏ phần còn thiếu theo dependency. Không lập lịch thời gian giả khi chưa biết nguồn lực/nội dung mục tiêu.

| Chặng đề xuất | Kết quả người chơi phải có | Điều kiện kiểm chứng/chốt |
|---|---|---|
| Chặng 0 — Chốt bản đồ hiện trạng | Biết chính xác cái gì đang chơi được và cái gì còn thiếu | Ma trận thiết kế/source/asset/evidence; một task tiếp theo rõ ràng, không audit vô hạn |
| Chặng 1 — Một vòng chơi xuyên suốt | Vào game → Normal combat → loot/so sánh/trang bị → progress → Boss Gate → chủ động Boss → thắng/thua → tiếp tục → đóng/mở giữ state | Dùng scene/entry point thật, không cần console cheat để đi qua luồng; kiểm Bun và save theo contract |
| Chặng 2 — Hệ thống nhân vật và nội dung combat | Skill/loadout/Tâm pháp theo thiết kế, chọn/equip dùng được; loại skill được scope; Companion tham chiến/chết/hồi sinh | Data authoring/validation, authority chung, timing/resource, progression/unlock, UI chức năng; không lấy fixture RAM làm production content |
| Chặng 3 — Kinh tế và tiến trình | EXP/title gate, equipment/chest/drop-level, recycle/protection, reward/progression có giá trị chơi thực | Không mất EXP gate, không double reward/double spend, không recycle item đang trang bị/khóa, config rõ và persistence đúng |
| Chặng 4 — Save và offline end-to-end | Mở/đóng/background/crash/reopen có kết quả nhất quán; offline reward, Bun regen, chest timer và Boss Gate đúng luật | Save schema/migration/recovery có scope; không nhận reward lặp; test interruption và corrupted data cần thiết; phân biệt Editor save với build |
| Chặng 5 — Mở rộng content và balance | Nhiều skill/monster/Boss/item/title/stage có tiến triển hợp lý, không lặp một dummy fixture | Bảng data có validation, simulation cân bằng có giới hạn, playtest theo build; các con số mới được duyệt, chưa duyệt để TBD |
| Chặng 6 — UI/UX và presentation hoàn chỉnh | Onboarding, HUD, screen flows, touch targets, responsive/Safe Area, art/animation/VFX/audio thể hiện rõ gameplay | Kiểm các màn hình/thiết bị mục tiêu; modal/pause có chủ sở hữu; animation/VFX không thay combat authority; asset có nguồn/quyền sử dụng rõ |
| Chặng 7 — Độ ổn định và build trên thiết bị | Build mục tiêu chơi liên tục được, tải scene và memory ổn, không leak/reward/save lỗi khi gián đoạn | Chốt nền tảng/device/budget; đo FPS/frame time/memory/load ở build thật; bounded soak/regression, debug tooling tách khỏi bản phát hành |
| Chặng 8 — Chuẩn bị phát hành và bảo trì | Bản ứng viên có version, release notes, hướng dẫn hỗ trợ và kế hoạch update/rollback | Chốt phạm vi v1, blockers, backup/migration; Chủ Dự Án chấp nhận trải nghiệm; kiểm yêu cầu nền tảng hiện hành tại thời điểm phát hành |

Các chặng có thể đan xen có kiểm soát: persistence phải được bảo vệ ngay khi làm chặng 1/2, không chờ chặng 4 mới nghĩ đến save; UI chức năng cần đủ dùng từ đầu; phần art/polish hoàn chỉnh theo ưu tiên sau gameplay. Không mở đồng thời nhiều feature chạm cùng authority khi chưa ổn định lát cắt đang làm.

### Thế nào là “game hoàn chỉnh như Chủ Dự Án muốn”

Trước release candidate, phải có scope v1 được người dùng duyệt và ma trận yêu cầu truy ngược về D-contract/quyết định mới. Mỗi hệ thống trong scope có đường thao tác người chơi, nội dung thật, lưu trạng thái và tiêu chí trải nghiệm; không còn phụ thuộc harness hoặc lệnh debug cho đường chơi thông thường. Chốt các lỗi blocker, ghi rõ debt được chấp nhận, chạy được trên build/thiết bị mục tiêu và có phương án hỗ trợ cập nhật.

Số lượng stage/Boss/skill cần cho v1, thời lượng chơi, nền tảng phát hành, monetization, tài khoản/cloud, multiplayer/PvP, sự kiện hoặc tính năng xã hội phải được quyết định riêng khi chúng trở thành phạm vi thực. D18 có khung PvP và D17 có hướng server-authoritative nếu làm multiplayer; điều đó không tự phê duyệt xây server hay thêm mọi tính năng online ngay lúc này.

## 13. Các quyết định còn mở — không tự điền

| Chủ đề | Điều còn cần kiểm tra/chốt | Khi cần hỏi |
|---|---|---|
| Skill production | Skill đại diện nào, data/visual/balance nào; migration scope | Trước task đưa foundation vào content thật |
| Bun/offline | Max Bun, max offline time, mô hình reward hiệu quả và tham số ngoài phần đã khóa | Khi audit thấy cần triển khai/cân bằng nhánh đó |
| Boss lifecycle | Đóng game giữa Boss: reset/thua hay resume; post-loss UX và reward cụ thể | Trước viết persistence/result behavior tương ứng |
| Gear/economy | Rarity unlock/probability, Item Level distribution trong ±5, affix range, CP weights, recycle Gold, chest cost/time/open cost | Trước tạo dữ liệu balance có ảnh hưởng người chơi |
| Content progression | Title stat tables, level/stage thresholds, nội dung/v1 coverage | Khi chọn mục tiêu content và pacing |
| Skill/movement nâng cao | Projectile AOE/piercing/bounce, jump, knockback, channel+projectile hoặc hỗn hợp delivery ngoài slice | Trước mở rộng runtime validator/executor |
| Thiết bị/phát hành | Android/iOS ưu tiên, cấu hình tối thiểu, store/distribution, backend nếu có | Trước lập build/performance/release contract |

Không hỏi lại những điều đã chốt: Companion có HP; recycle Gold-only; Chest=Drop Level; Item Level theo Hero ±5; rarity không có trần cứng theo 9 hàng ảnh; chest upgrade một cấp/lần, không queue/cancel/claim; EXP ở gate không mất. Các ví dụ test như ATK/HP/damage/giá trong screenshot không tự trở thành balance vĩnh viễn.

## 14. Kiểm soát thay đổi, kỹ thuật và rủi ro tương lai

| Rủi ro | Nguyên tắc xử lý |
|---|---|
| Mỗi feature sinh manager/pipeline riêng | Mở rộng `SkillExecutor`, `SkillExecutionValidator`, `EffectResolver`, `DamageCalculator`, `HealthComponent`, `EntityStatusController`, `CooldownManager`, `RageComponent` và encounter authority hiện có; mọi authority mới phải có nhu cầu chứng minh |
| Schema/asset mới phá save cũ | Default tương thích; version/migration có scope; mẫu save cũ và rollback khi cần; không mass migration trước audit |
| Race/cancel/pause qua nhiều hệ thống | State transition và owner rõ; test đúng thời điểm release/impact/teardown, pending callbacks và encounter change |
| Pooling/performance làm mất payload isolation | Chỉ tối ưu khi đo được; dùng identity/generation/reset lifecycle rõ, bảo toàn once-only semantics |
| Random làm test flaky | Fixture/RNG provider có thể điều khiển cho test; không thay production dodge/crit để đạt PASS |
| Offline/clock/reward abuse | Chốt chính sách thời gian khi làm sản phẩm; reward nhận một lần, dữ liệu thời gian có kiểm chứng, giới hạn offline theo config; không tự thêm server nếu chưa có scope |
| Content bùng nổ trước khi vòng chơi ổn | Vertical slice có data thật nhỏ; công cụ validation/authoring; tăng content sau khi hợp đồng và workflow ổn |
| Pipeline chạy lại vô tận | Ngân sách thời gian, log giữ lại, stop condition cụ thể; chạy lại để giải quyết rủi ro mới, không để đổi màu báo cáo |
| Chat mới hiểu lịch sử là task hiện tại | Luôn đọc active handoff và decision mới trước task cũ; cập nhật mục “việc tiếp theo” khi kết thúc mỗi mốc |
| Git local/remote lệch | Ghi hai mốc riêng, diff exact paths, xác minh remote; không tự đè source hoặc force-push để đồng bộ |

Khi có design change, ghi: quy tắc cũ → bằng chứng/vấn đề → phương án mới → ai phê duyệt → source/asset/save chịu ảnh hưởng → test cần chạy → quyết định supersede. Code khác tài liệu chưa đủ chứng minh thiết kế đã được thay đổi.

## 15. Quy tắc Git và cập nhật hồ sơ về sau

- Canonical memory là `Ai_MEMORY_TLTD/main`; game repo chứa bản mirror và source. Không copy Unity production code vào memory repo.
- Quyền công bố ngày 05/10 áp dụng tài liệu nghiệm thu/continuity và mirror tương ứng. Không biến quyền đó thành quyền push mọi source chưa review hoặc triển khai mọi đề xuất trong file này.
- Khi nhiệm vụ được phép, chuẩn bị diff cụ thể, stage exact paths, kiểm `diff --check` với chính sách Markdown thích hợp, commit có scope và push fast-forward. Không sửa published history hoặc force-push.
- Nếu working tree có công việc khác, bảo toàn nó. Dùng workspace tách biệt/API trên base đã kiểm tra khi phù hợp; không stash/reset/clean chỉ để thuận tiện.
- Nếu remote có thay đổi đồng thời, đối chiếu lại; không ghi đè tiến bộ của người khác. Nếu quyền/auth/network chặn publication, giữ patch/commit và báo `PENDING — NOT ON REMOTE` với lỗi thật.
- Chỉ báo “đã lên Git” sau khi xác minh ref/commit và nội dung file remote; trả SHA và link. Chưa sửa được local Windows thì không tuyên bố local đã sync.
- Sau mỗi milestone, cập nhật decision + active handoff + authority/changelog khi cần. File này cập nhật phần snapshot/lịch sử/next action nếu mục tiêu tiếp quản thay đổi; không ghi đè raw evidence.
- Một report mới hoặc task cũ không tự có quyền thay quyết định đã khóa. Proposal phải được gắn nhãn; việc đưa proposal vào Git không biến nó thành LOCKED.

## 16. Mẫu hồ sơ ngắn để duy trì sau mỗi lượt

```text
Ngày / task / mục tiêu người chơi:
Authority và baseline source/asset/save:
Việc đã làm (đường dẫn cụ thể):
Kiểm chứng đã chạy / chưa chạy:
Run ID, raw log, exit/timeout, Restore/Compare:
Xác nhận trực tiếp của người dùng (nguyên văn nếu có):
Finding còn mở / giới hạn được chấp nhận:
Quyết định và phạm vi LOCK:
Commit remote / trạng thái đồng bộ:
Bước tiếp theo cụ thể / quyền thực thi / điều chưa được phép:
```

Không cần người dùng tự điền hết mẫu; Tech Lead/executor trích từ công việc thực. Người dùng chỉ trả kết quả trải nghiệm hoặc quyết định cần họ chọn. Mẫu giúp không mất trạng thái khi chuyển chat.

## 17. PROMPT KHỞI ĐỘNG CHAT MỚI — có thể sao chép nguyên khối

```text
Bạn tiếp quản vai trò Tech Lead/Architect cho dự án TLTD. Hãy đọc file TLTD_NEW_CHAT_CONTINUATION_MASTER.md tôi đính kèm, rồi kiểm tra main hiện hành của huycodedie/Ai_MEMORY_TLTD và bản PROJECT_MEMORY trong huycodedie/ta_la_ta_de.

Giữ P08/P09-A/P09-B đã ACCEPTED / LOCKED theo đúng phạm vi; không yêu cầu tôi thử lại hoặc đóng ZIP nghiệm thu các phần đã xong nếu không có thay đổi/rủi ro mới cụ thể. Giữ nguyên giới hạn raw log và lịch sử recovery.

Bạn phụ trách phân tích, chốt scope kỹ thuật, giao prompt, review source/evidence, nghiệm thu và lưu continuity. Antigravity thực thi Unity/code/test; tôi quyết định sản phẩm và kiểm tra game trực tiếp khi cần. Khi cần sửa, đưa prompt cụ thể; khi cần tôi chơi thử, nói rõ thao tác và kết quả cần quan sát. Tách rõ file prompt khỏi tài liệu giải thích.

Hãy xác định trạng thái mới nhất, phần việc còn thiếu và bước tiếp theo cụ thể. Nếu chưa có roadmap implementation đầy đủ, lập task read-only đối chiếu thiết kế–source–content–trải nghiệm để chọn lát cắt gameplay tiếp theo. Không tự biến lộ trình đề xuất thành thiết kế đã phê duyệt, không tự bật toàn bộ skill assets, không mở feature mới ngoài scope.

Tiếp tục chủ động trong quyền đã có. Nếu thiếu dữ liệu, làm hết phần hữu ích trước và chỉ yêu cầu đúng dữ liệu còn chặn công việc. Bảo toàn working tree/save; không tuyên bố đã xem file, chạy Unity hay push Git nếu chưa thực hiện và xác minh.
```

**Điểm dừng của hồ sơ v1.0:** P09-B đã chốt và công bố; chưa triển khai một milestone mới sau P09-B. File này lưu cách tiếp quản và lộ trình đề xuất, không phải kết quả thực thi những chặng tương lai.
