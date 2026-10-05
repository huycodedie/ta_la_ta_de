# PROMPT ANTIGRAVITY — TLTD GAMEPLAY GAP AUDIT
Task ID: AUDIT_GAMEPLAY_GAP_20261005.
Trạng thái: **READ-ONLY TASK AUTHORIZED; IMPLEMENTATION NOT AUTHORIZED**.
Ngày: 05/10/2026, UTC+07:00.
Đây là task audit, không phải milestone P09-C/P10 và không phê duyệt roadmap feature.

## 1. Vai trò và mục tiêu
Bạn là executor Unity/source của TLTD. Làm một đợt audit có điểm kết thúc để nối thiết kế LOCKED với source/content/scene/UI/evidence hiện tại; chọn một lát cắt gameplay tiếp theo có thể review và chơi được. Chưa sửa code và chưa chạy Unity.

Codex là Tech Lead/Architect/reviewer và publisher continuity. Chủ Dự Án chốt sản phẩm/balance/phạm vi mới và chơi thử khi có yêu cầu cụ thể. Bạn không tự thay authority hoặc tự implement đề xuất.

## 2. Baseline phải ghi riêng
- Project local: `E:\code\TLTD`.
- Scene để đọc wiring: `Assets/_Game/Scenes/Prototype01.unity`.
- Canonical: `huycodedie/Ai_MEMORY_TLTD/main`; game/mirror: `huycodedie/ta_la_ta_de/main`.
- Mốc trước publication task: memory `c5c61357b8d4604fc087faf116272d1eaec09460`, game `06e5e473bd468a2842d15d9b5299cbee0f881283`.
- Local đã được đọc khi soạn: HEAD `5eafa685b830282665aca72323ef143ff063f9f5`, branch main, 3 tracked modifications + 533 untracked. Đây là snapshot tham chiếu, phải đo lại read-only khi audit.
- Hai commit game từ local HEAD tới remote baseline chỉ thay 11 PROJECT_MEMORY Markdown paths. Không giả định working tree hiện tại bằng remote.
- Ba tracked modifications tại snapshot: MANUAL_P09A_CHECKLIST.md, PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md, REVIEW_RESPONSE.md. Bảo toàn chúng và mọi artifact/untracked khác.

Resolve main một lần tại đầu task và ghi exact SHAs. Nếu main mới hơn chỉ cập nhật docs task này thì dùng ACTIVE_WORK_HANDOFF mới; nếu xuất hiện quyết định/runtime mới, đọc diff và báo tác động, không áp snapshot cũ máy móc. Không fetch/pull/merge trực tiếp vào dirty project.

## 3. Authority phải đọc
Dùng canonical snapshot tách biệt dưới thư mục audit, không ghi đè local PROJECT_MEMORY. Game mirror đang được bảo trì docs; local E: chưa được tự động sync. Nếu thiếu quyền mạng, dùng bản canonical được bàn giao và ghi rõ mốc, hoàn thành mọi nhóm độc lập trước khi báo đúng file thiếu.

Đọc:
1. AI_RULES.md; CONTINUITY_AND_SYNC_AUTHORITY.md; ACTIVE_WORK_HANDOFF.md; CURRENT_DESIGN_AUTHORITY.md; phần mới DESIGN_CHANGELOG.md.
2. TLTD_NEW_CHAT_CONTINUATION_MASTER.md, nhất là evidence boundaries và mục 11–15.
3. P08_LOCKED.md; DECISION_P09A_ACCEPTED_LOCKED_20260927.md; DECISION_P09B_ACCEPTED_LOCKED_20261004.md; TECH_LEAD_HANDOFF_REVIEW_20261004.md.
4. D1_D23_LOCKED.md và D1_D23_AMENDMENTS_LOCKED.md; các file D6_D8/D9_D11/D12_D14/D15_D16/D17_D18/D20/D21/D22/D23 liên quan; P07_9/P07_9_1.
5. UI_DESIGN_AUTHORITY/UI_RESPONSIVE_LAYOUT_AUTHORITY/spec và SKILL_DATA_AUTHORITY_MAP như tài liệu để kiểm, không coi map/report cũ là bằng chứng mọi asset hiện tại.

Một số D-file ở root memory repo, còn game copy ở PROJECT_MEMORY. Đọc đúng nguồn trước khi nói tài liệu thiếu. Task/review B1 ngày 17/09 là lịch sử; B1 functional baseline đã được giữ trong accepted P08. Không mở lại task UI cũ.

## 4. Phạm vi được làm và cấm
Được đọc git HEAD/status/diff/log, source/config/assets/meta/scene/prefab text, các report/log hiện có phục vụ matrix. Dùng `git --no-optional-locks` cho status nếu phù hợp. Được viết **audit artifacts** vào một thư mục mới `scratch/gameplay_gap_audit_<timestamp>`; không sửa artifact cũ. Ghi rõ những output audit vừa tạo trong before/after inventory.

Không được:
- sửa production runtime, Editor code, asset, .meta, scene, prefab, Packages, ProjectSettings hoặc local PROJECT_MEMORY;
- launch/import Unity, enter Play Mode, chạy test/harness/verification wrapper/Save Guard;
- đọc dump nhạy cảm hoặc ghi Registry/PlayerPrefs/save; backup/restore/recovery để audit;
- tự kill process; stash/reset/clean/checkout đè, stage/commit/push/merge/rebase;
- enable/migrate/mass-rewrite skill assets, hard-code balance/TBD, tạo engine/authority mới;
- tái đóng ZIP nghiệm thu P08/P09, yêu cầu Chủ Dự Án lặp S01–S05/M1–M7 hoặc làm xanh lịch sử FAIL/timeout.

Nếu không chứng minh được đường chơi bằng đọc source/scene/existing evidence, ghi **UNKNOWN / RUNTIME NOT EXECUTED**. Không dùng “read-only Play Mode”: Unity vẫn có thể import/ghi persistence.

## 5. Bảo toàn baseline đã nghiệm thu
- P08/P09-A/P09-B giữ ACCEPTED/LOCKED theo phạm vi biên bản. Bảy runtime file P09-B local đã khớp bảng SHA256 khi soạn; nếu đo lại, chỉ hashing, không rerun test.
- P08 Gate 2 = 135/137, exit 1 với legacy assertions 07/09; giữ raw failures.
- P09-A E2 = SCENARIO_PASS_EXIT_TIMEOUT / wrapper exit 2; không sửa bằng verifier exit 0. E1 giữ CLOSED/PASS.
- P09-B M1–M7 và GUI USER_VERIFIED; recovery F-SAVE-P09B-01 CLOSED/RECOVERY ACCEPTED.
- PID 25360 giữ USER_REPORTED_PASS/RESTORE_UNRESOLVED.
- Run recovery REC-P09B-20261004-151227; snapshot 04/10 15:14:04 +07; 24/24 name/kind/truncated digest match, 56 Added gone, 4 changed digest restored.
- Raw baseline Restore/execution PARTIAL, baseline Compare/preflight METADATA_ONLY. Không biến snapshot 16-hex digest thành live full-byte Registry check; không tái dựng raw log thiếu bằng rerun.
- Foundation không đồng nghĩa production skill rollout. Gameplay trước; finished UI/art/polish hoãn; giữ authority pipeline chung.

## 6. Audit matrix — tám nhóm bắt buộc
Mỗi dòng phải có:
`Hệ thống / yêu cầu cụ thể | authority + đoạn | source + lines / asset path-ID-GUID-load path | production entry point / scene-UI wiring | evidence còn hiệu lực + provenance | status và giới hạn | gap/conflict | dependency | candidate task`.

Không chỉ làm bảng “có class/không class”. Truy từ input/UI → manager/validator → execution/state → data/reward → persistence khi có. Phân biệt production với fixture/test/debug; existing automated report với runtime/player acceptance; khóa contract với rollout content.

1. **Skill/data/authoring:** D6/D13/D14/P07/P09; definition/validator/executor, database/load path, unlock/selected slots/manual/auto; authoring/validation/preview dùng pipeline thật. Kiểm select/equip→cast→save/reopen theo source. Inventory asset xác định unique SkillId/GUID và duplicate Data/Skills và Resources/Data/Skills.
2. **Normal/Boss/stage:** Normal waves/loot P08; progress và gate D17/D18/D19; player chủ động challenge, timer 60s, Hero death loss, win/loss/next stage/result authority. Chỉ ghi source được thấy; không tự triển khai Boss.
3. **Bun/offline:** 1 Bun/Hero action Normal, Companion không tiêu nhưng cùng stop ở 0, Boss độc lập; regen/offline cap/progress tới gate rồi stop; time/reward/save ownership. Chưa có MaxBun/maxOffline config được duyệt thì TBD.
4. **Companion:** tối đa 5, HP/damage/death/respawn, shared pipeline, content/spawn/loadout/UI/persistence; phân biệt enum/HUD/test support với playable roster.
5. **Level/title:** EXP gate không mất, level không tăng trực tiếp HP/ATK/DEF, breakthrough cập nhật base theo bậc; config/unlock/UI/persistence.
6. **Equipment/chest/recycle:** 12 slot, Chest=Drop Level, ItemLevel Hero ±5, extensible rarity; upgrade một cấp/timestamp/no queue-cancel-claim; recycle Gold-only/protection. Không tự điền distribution, CP weights, cost/time/Gold tables.
7. **Save/load:** inventory/equipment/loadout/progress/stage/time/schema/default/migration hiện có; centralized save có thật hay chỉ scalar PlayerPrefs. Không đọc nội dung save người dùng; chỉ source và existing redacted evidence. Editor không đại diện build/device.
8. **UI chức năng:** global shell 5 vị trí, Main Hub giữa, Công Pháp riêng, modal ownership; chứng minh navigation và player input cho từng flow; không bắt đầu final presentation.

Có thể gộp các yêu cầu chung đã đủ bằng chứng, nhưng không bỏ nhóm. Nhãn thống nhất: LOCKED DECISION, SOURCE REVIEWED, EXISTING USER/EXECUTOR EVIDENCE, USER_VERIFIED, PROPOSAL, TBD, UNKNOWN, NOT EXECUTED. Không tự gọi DONE vì tìm thấy class, hoặc NOT STARTED vì grep không thấy tên.

## 7. Các điểm cần giải quyết trước tiên
Đây là quan sát static để audit, không phải lệnh sửa:
- **A-RECYCLE:** LootDecisionUI L542/556 → BattleManager L1048/1102 → Assets/_Game/Progression/ResourceManager.cs L132–151: DismantleEquipment đang thêm Gold và Material; CURRENT_DESIGN_AUTHORITY ghi GOLD ONLY. Trace actual call và thuật ngữ Dismantle/Recycle; tìm later explicit decision có supersede không. Xuất [CONFLICT]: rule / current source / later LOCK nếu có / evidence / recommended correction / need owner decision YES-NO. Không tự giảm/thêm reward.
- **A-LOOP:** BattleManager L961–968 tăng CompletedNormalWaveCount; L1158–1166 đi tiếp Normal sau loot. Xác minh source/scene/data thật cho stage progress/Boss Gate/Bun; title breakthrough currentStageIndex không phải chứng cứ combat Stage.
- **A-SKILL-FLOW:** MindMethodManager.SelectSkillForSlot L487 và SaveState L520 có API; MindMethodUI BindButtons L169 trở đi và alternatives summaries chưa thể hiện production selection caller trong scan. Tìm UnityEvent wiring hoặc caller khác trước kết luận gap.
- **A-ASSET-INVENTORY:** Snapshot thấy 30 Data/Skills + 30 Resources/Data/Skills .asset paths; tất cả paths đã kiểm có isProjectile/isDash=0. Đây không phải số unique skill; đối chiếu báo cáo lịch sử “61 production assets” và actual loaded authority; không sửa lịch sử hoặc enable assets.
- **A-PERSISTENCE/COMPANION:** Inventory/EquipmentManager chưa thấy Save/Load ở class chọn lọc; enum/HUD Companion không chứng minh roster/respawn. Kiểm centralized ownership trước kết luận.

Tiếp tục các nhóm độc lập khi một nhóm còn UNKNOWN. Chỉ dừng nhánh phụ thuộc quyết định thực sự thiếu; không yêu cầu người dùng tìm hàng chục file.

## 8. Chọn đúng một lát cắt tiếp theo — vẫn PROPOSAL
Đầu ra chọn một candidate chính, tối đa một dự phòng. Lý do dựa trên player outcome và dependency:
- Nếu Normal→loot/equip→progress→Boss Gate→challenge/result→continue/save bị chặn, đề xuất blocker nhỏ gần nhất. Không gom Stage+Boss+Bun+offline+save vào một implementation lớn.
- Nếu audit chứng minh core loop đủ và content/loadout là khoảng thiếu gần nhất, cân nhắc số ít skill đại diện, có thể projectile/Dash opt-in qua select/equip/cast thật; không bật toàn bộ assets.
- Không đặt số/balance mới. TBD nào chặn candidate thì liệt kê exact parameter/behavior cần Chủ Dự Án chốt; những TBD không chặn thì để deferred.

Candidate brief: player outcome; included/excluded behaviors; existing authority/files dự kiến; data/content/save ảnh hưởng; dependency; acceptance/check plan phù hợp và manual steps dự kiến nếu cần; rủi ro; tối đa vài quyết định sản phẩm thật. Đây là brief để Codex soạn implementation prompt sau review, không phải quyền code.

## 9. Đầu ra và điểm kết thúc
Xuất UTF-8:
1. `GAMEPLAY_GAP_AUDIT_REPORT.md`: kết luận, baseline, cách trace, 8 nhóm, conflicts, evidence limits.
2. `GAMEPLAY_GAP_MATRIX.csv` hoặc Markdown table trong report nếu đủ rõ.
3. `NEXT_GAMEPLAY_SLICE_PROPOSAL.md`: candidate chính/dự phòng, scope và kiểm chứng dự kiến, TBD.
4. `PROVENANCE.md`: local/remote HEAD, before/after tracked status, exact files inspected và SHA256 của file key; mọi existing log là REFERENCE, không có run mới. Tránh xuất save/data nhạy cảm.

Task đạt khi tám nhóm có trace/evidence hoặc UNKNOWN có lý do; A-RECYCLE/A-LOOP/A-SKILL-FLOW/A-ASSET-INVENTORY đã được phân loại; có một candidate nhỏ và dependency rõ; diff không có runtime/assets/scene/save modifications do task; không fake PASS; không thay LOCK.

Không cần ZIP mới cho audit thuần tài liệu. Trả đường dẫn tuyệt đối, bytes/SHA256 của outputs và executive summary: cái đã có, gap gần nhất, candidate, blocker thật. Nếu công cụ không chuyển file được, trả đủ nội dung report/proposal; không yêu cầu đóng lại ZIP P09.

Dừng tại bàn giao audit cho Codex review. Không tự chạy implementation, gửi bài test mới cho Chủ Dự Án hoặc commit/push.
