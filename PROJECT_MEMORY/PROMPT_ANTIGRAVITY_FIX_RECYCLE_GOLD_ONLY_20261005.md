# PROMPT ANTIGRAVITY — FIX RECYCLE GOLD ONLY
Finding/task: **F-RECYCLE-GOLD-ONLY-01**.
Ngày: 05/10/2026, UTC+07:00.
**TECH LEAD SCOPED CORRECTIVE TASK — khôi phục contract đã LOCKED; không mở feature hoặc balance mới.**

## 1. Mục tiêu
Đường người chơi bấm [Tách] chỉ trả Gold, không Material/EXP. Giữ item identity, loot queue/modal/encounter/cast/channel và transaction guard đã nghiệm thu. Trả source patch và evidence cho Codex review; không tự nghiệm thu hoặc push.

Đọc trước [TLTD_GAMEPLAY_GAP_AUDIT_REVIEW_20261005.md](TLTD_GAMEPLAY_GAP_AUDIT_REVIEW_20261005.md). Audit tám nhóm đã review; không audit lại cả dự án. Stage/Boss proposal chưa được giao code.

## 2. Baseline/authority
Project `E:\code\TLTD`; main local tại review `5eafa685b830282665aca72323ef143ff063f9f5`.
Remote mốc review: canonical `caa089d6c32e531c9d64f16ff8bf071f4fe7e402`; game `0ad2af344134401c98f30567e581e2691ae8171f`. Task publication có commit riêng; resolve main rồi đọc ACTIVE mới, ghi exact source/local/authority SHAs riêng. Không pull vào dirty project.

Local status tham chiếu: 3 tracked modifications (MANUAL_P09A_CHECKLIST.md, PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md, REVIEW_RESPONSE.md), 533 untracked. Bảo toàn mọi thay đổi; ghi before hashes cho file sắp sửa và tracked docs vốn đang sửa.

Read canonical snapshot tách biệt, không ghi đè PROJECT_MEMORY local:
- AI_RULES, CONTINUITY_AND_SYNC_AUTHORITY, ACTIVE_WORK_HANDOFF, CURRENT_DESIGN_AUTHORITY.
- D1_D23_AMENDMENTS_LOCKED A10 (Gold-only), A12/A13 (precedence/no invent balance).
- P08_LOCKED và P09-A/P09-B decisions/review raw-evidence limits.
- Relevant actual source/tests/callers của DismantleEquipment.

## 3. Finding và bằng chứng
Actual:
`LootDecisionUI.cs` L542/556 → `BattleManager.cs` L1048/1102 → `Progression/ResourceManager.cs` L132–151.
Hàm tính matGain từ rarity, AddGold(goldGain) và AddMaterial(matGain).
ResourceManager mốc review: 4,609 bytes, SHA256 `E35B2F76300C029089ED3A24081E317A2DE3EFBA1D7B8D15E9E6605A6A8FEA3D`.

Expected:
Recycle = GOLD ONLY; Material/EXP unchanged. Không có explicit later approval supersede A10. Không hỏi Chủ Dự Án chọn lại Gold-only/Material.

## 4. Phạm vi sửa tối thiểu
Được sửa:
1. `Assets/_Game/Progression/ResourceManager.cs` đúng nhánh DismantleEquipment: resource award, return value, log/comment liên quan.
2. File test Editor mới/đã có chỉ cho assertions của finding; sửa assertion cũ liên quan chỉ khi chứng minh nó đang kỳ vọng Material recycle trái contract, ghi ID/diff rõ.
3. UI/debug caller liên quan trực tiếp nếu hiển thị/đọc reward tuple Material từ Tách; liệt kê lý do exact path. Nếu không có thì không sửa UI/scene.
4. Artifacts mới dưới `scratch/recycle_gold_only_<runid>`.

Yêu cầu kỹ thuật:
- Giữ Gold formula hiện tại `Mathf.Max(50, item.EquipmentLevel * 100)` để tránh thêm balance trong patch. Ghi đây là preserved implementation/TBD balance, không biến thành LOCKED formula.
- Không gọi AddMaterial từ recycle; Material/EXP không đổi.
- Giữ chữ ký tuple `(goldGain, matGain)` nếu cần tương thích, trả matGain=0; không tạo API/authority thứ hai cho reward.
- Log kết quả thật Gold-only; không ghi câu đã cấp Material khi không cấp.
- Giữ null item semantics (0,0/no reward) và inventory removal behavior đang có. Không đổi ownership item/transaction.
- Giữ AddMaterial/SetResources/ConsumeResources cho các nguồn/chi phí hợp lệ khác; không xóa hệ thống Material.
- Không thêm SaveManager, schema/migration, nguồn Material mới, auto-recycle feature, bảo vệ affix mới hoặc thay Gold cost/drop/rarity tables.
- Không thu hồi Material đã có trong save; không bù tặng Material, không giảm yêu cầu title/chest để che gap nguồn resource.
- Không sửa BattleManager/loot/modal/scene chỉ để đạt test. Nếu finding mới ở transaction authority thực sự chặn chứng minh, báo exact expected/actual/source; giữ phần sửa đã an toàn và dừng nhánh ngoài scope.

Cấm rollout skills, stage/Boss/Bun/offline/Companion implementation; không sửa 61-count historical claims để đổi inventory; không sửa biên bản nghiệm thu/raw logs cũ.

## 5. Kiểm chứng theo rủi ro
Chạy compile và kiểm chứng scoped, không master suite/P08/P09 toàn bộ.
Trước chạy phải xác định runner/fixture thực đang dùng và persistence side effects; RAM fixture không bảo đảm không PlayerPrefs.

Các behavioral checks cần có:
- **R1 Resource result:** item hợp lệ → Gold delta đúng formula baseline một lần, tuple matGain=0, Material và EXP unchanged, inventory xử lý đúng item identity.
- **R2 Rejected/null:** null item không reward/mutation; đường loot decision invalid/premature/duplicate bị BattleManager guard từ chối, không thêm Gold/Material lần nữa. Test duplicate qua production transaction, không gọi trực tiếp DismantleEquipment hai lần rồi tự thêm idempotency ngoài scope.
- **R3 Sequential loot/modal:** dùng scoped existing B1/P08 integration path: ít nhất hai item riêng, Tách đúng item đang hiển thị, duplicate click không cấp thêm; item tiếp theo còn nguyên; modal release và next normal wave đúng accepted boundary. Không ép damage/death để gọi đó là natural-frame evidence.
- **R4 Unaffected resources:** verify AddMaterial API từ source hợp lệ/test fixture vẫn hoạt động và existing cost transaction không bị đổi; không claim đã có production nguồn Material khác.
- **R5 Persistence/cleanup:** Gold-only record đi qua SaveState authority; fixture baseline restore/compare thành công, listeners/modal/pause owner sạch. Không chạy lại recovery cũ.

Có thể gộp checks vào một suite nhỏ; report exact test IDs/counts/case thực chạy. Reuse tests có ý nghĩa, không tạo tests chỉ kiểm một dòng source. Khi cần natural-frame integration dùng entry point game/scene thật, không manual Tick hoặc reflection thay lifecycle. Không thêm test rộng ngoài rủi ro này. Nếu compile/test environment chặn, ghi NOT EXECUTED và lỗi thật; không chuyển compile PASS thành gameplay PASS.

## 6. Save/process/working-tree protection
Trước bất kỳ Unity launch, compile import, test hoặc wrapper:
1. Chạy unresolved-journal preflight implementation đã review; journal unresolved/corrupt → block launch/backup/restore, báo path/status thật; không tự recover hoặc chấp nhận contaminated state làm baseline.
2. Backup baseline đúng registry/platform scope phải thành công trước launch. Dùng Save Guard/dispatcher đã có; không sửa tooling guard trong task này.
3. Chỉ quản lý đúng process tree task tạo; không kill mọi Unity hoặc Editor người dùng đang làm. Nếu project đang được Editor khác dùng thì compile/test chỉ tiếp tục khi an toàn, không sửa/restore song song writer chưa quiesce.
4. Bounded watchdog cho từng run (ghi budget trước launch, tối đa 10 phút/run nếu không có lý do khác); không loop chạy lại vô hạn. Tối đa một retry khi có lỗi môi trường đã xác định và đã sửa; giữ cả log thất bại.
5. Khi kết thúc restore/compare baseline theo guard; nếu restore fail/compare diff, báo persistence finding với raw evidence, không tự xóa journal hoặc dùng run mới viết đè.

Bảo toàn P09-B recovery history: F-SAVE-P09B-01 CLOSED, PID25360 RESTORE_UNRESOLVED historical, raw Restore/execution PARTIAL, Compare/preflight METADATA_ONLY. Không tuyên bố chúng được xác minh lại bằng run finding này.

Không git stash/reset/clean/checkout overwrite/stage/commit/push/rebase. Không ghi đè tài liệu đang sửa. Before/after status và hashes exact paths; thay đổi task phải tách khỏi unrelated paths.

## 7. Bàn giao và tiêu chí đóng finding
Trả:
- `FIX_REPORT.md`: expected/actual/fix, exact changed paths, baseline/source hashes, preserved behavior, Material-source gap deferred.
- Patch UTF-8 portable dựa trên **before bytes thực của task**, không giả HEAD clean; original before-hash và after-hash.
- Source file(s) thay đổi và tests liên quan có thể truy cập; raw compile/test/wrapper logs, provenance/run index.
- Bảng riêng: test IDs/counts, run ID/time/source hash, OS/wrapper exit, timeout, Restore, Compare, raw path và giới hạn.
- Before/after inventory/status; không export save dump nhạy cảm.

Nếu Codex đọc được artifact local, trả exact paths/bytes/SHA256; không cần ZIP. Chỉ tạo gói delta khi thực sự cần chuyển file, không đóng lại ZIP P08/P09 nghiệm thu.

Finding đóng khi source khớp Gold-only, targeted behavior checks đủ evidence, không reward trùng/đổi item/modal/wave, save cleanup hợp lệ hoặc có giới hạn Tech Lead chấp nhận, source/provenance rõ. Codex review rồi quyết định; bạn không tự ghi F-RECYCLE FIX VERIFIED/LOCKED.

Chưa yêu cầu Chủ Dự Án chơi thử. Nếu automation thực sự thiếu đúng một quan sát sau source fix, báo scene/steps/expected/result cần xem để Codex giao checklist ngắn. Không yêu cầu lặp S01–S05/M1–M7 hoặc toàn P08.

**Điểm dừng:** bàn giao delta cho Codex. Không triển khai Stage/Boss, không tự cấp nguồn Material và không push source.
