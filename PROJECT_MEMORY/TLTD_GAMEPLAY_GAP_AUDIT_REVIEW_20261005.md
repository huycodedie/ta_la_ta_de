# TLTD — Review kết quả audit và giao việc tiếp theo
Ngày: 05/10/2026, UTC+07:00.
Task nguồn: AUDIT_GAMEPLAY_GAP_20261005.
**Kết luận: AUDIT REVIEW COMPLETE / CLOSED WITH REVIEW ADDENDUM.**
Đã đủ dữ liệu dùng làm bản đồ hiện trạng. Đề xuất Stage/Boss nguyên bản **NOT APPROVED FOR IMPLEMENTATION**; không phải nghiệm thu gameplay mới. Việc kế tiếp là bản sửa có phạm vi F-RECYCLE-GOLD-ONLY-01 theo contract đã LOCKED.

## 1. Đã kiểm thực tế
Đã đọc văn bản Chủ Dự Án dán và bốn file tại `E:\code\TLTD\scratch\gameplay_gap_audit_20261005_225642`; giữ bản gốc, sao chép nguyên byte vào work của chat để review. Không sửa các artifact gốc.

| Artifact | Bytes đo lại | SHA256 đo lại |
|---|---:|---|
| GAMEPLAY_GAP_AUDIT_REPORT.md | 22,755 | CA6D3D3A4874D03C0436B8C2213ABFD4C5C0D199CA5C51248A807389A2F9AC21 |
| GAMEPLAY_GAP_MATRIX.csv | 13,247 | 3DE5473CD0747D94118C46CF3851AE70CA71077578C1CA7B417A18868DF524D6 |
| NEXT_GAMEPLAY_SLICE_PROPOSAL.md | 13,215 | 3A822E0DD4591CDAA8069A7A402A5CDF3D83D08C5F2803F23B7EC2959674D040 |
| PROVENANCE.md | 7,383 | FE5229EA9F840D44C72F6DC504C9922E17BEECDAD89A32D67DBC343DC15D84EA |

Ba hash đầu khớp văn bản dán. Hash PROVENANCE trong văn bản dán sai phần cuối; bảng trên là phép đo mới trên file thật. Đây là correction metadata, không chứng minh artifact bị sửa và không cần đóng ZIP mới.

Matrix parse được: **9 cột, 22 dòng, đủ 8 nhóm**. Đã đo lại **19/19 file source/scene** trong bảng PROVENANCE: bytes và SHA256 đều khớp file đang đọc.

Remote được resolve lại: memory `caa089d6c32e531c9d64f16ff8bf071f4fe7e402`; game `0ad2af344134401c98f30567e581e2691ae8171f`. Local HEAD vẫn `5eafa685b830282665aca72323ef143ff063f9f5`, main. Git status khớp snapshot tiếp quản: 3 tracked modifications và 533 untracked. So sánh remote game với local HEAD hiện là **3 commits / 21 changed paths, tất cả PROJECT_MEMORY Markdown**, không phải 2 commits/11 files ghi trong provenance.

Git status không chứng minh nguyên byte 100% ba tài liệu vốn đang sửa; không có before-hash cho chúng trong audit. Kết luận bảo toàn được giới hạn ở trạng thái đo và source hashes cụ thể, không certify toàn working tree/save. Codex không chạy Unity, test hoặc Save Guard, không thao tác save/Registry.

## 2. Những gap đủ căn cứ để dùng
| Nhóm | Kết luận đã review | Giới hạn |
|---|---|---|
| Stage/Boss | Chưa có production Stage progress → Boss Gate → challenge → result trong source đã trace; normal tiếp tục wave theo P08 | Đây là phần thiết kế chưa triển khai, không phải phát hiện P08 thất bại hoặc một runtime deadlock mới |
| Bun/offline | Chưa thấy runtime action economy/offline settlement trong scope scan | Regen đã LOCKED 1 Bun/20s; MaxBun và MaxOfflineTime mới là TBD |
| Companion | Có enum/shared pipeline/HUD/fixture; chưa có production roster/spawn/death/respawn đã trace | Không thu hồi P08 common-pipeline/Companion AOE acceptance |
| Skill selection | Có SelectSkillForSlot API/persistence; UI production chưa nối lựa chọn/equip skill | UI vẫn có toggle/close buttons; chỉ đường slot/equip đang thiếu, không gọi toàn UI “không có Button” |
| Skill content | 30 Data/Skills + 30 Resources/Data/Skills; 30 cặp tên/SkillId tương ứng, GUID khác nhau; flags projectile/dash tắt | Đây là duplicate logical IDs/copies, không phải GUID collision. Rollout foundation đã được hoãn có chủ ý |
| Inventory/equipment save | Chưa tìm thấy serialize/load item identity/loadout trong source đã trace | “Mất khi reload” là suy luận static cần test tương lai trong scope persistence, chưa có run mới. Không bắt buộc tạo SaveManager chỉ vì chưa có class đó |
| Level/title | Active title evaluator/data phải được phân biệt với legacy stage requirement | Không chấp nhận kết luận hiện tại Title bị khóa bởi thiếu PvE Stage |
| Recycle | Đường Tách thật đi tới AddGold + AddMaterial, mâu thuẫn A10 Gold-only | Source finding có phạm vi, chưa sửa; không thay hoặc mở lại toàn P08 |

Scene Prototype01 tham chiếu Data MindMethod database (GUID `bf3a01f160f208143b5296e9d22fb619`); Resources database `bb7a04edcb3879f4da3261ca2af288ae` là fallback khi không có serialized database. Không tự chọn Resources làm authority cho hai asset rollout. `MindMethodManager.FindSkillDefinition` tra database references.

Các câu “chuyển mượt”, “hoạt động tốt”, “chuẩn xác/hoàn chỉnh” trong audit chỉ được giữ ở phạm vi acceptance cũ hoặc SOURCE REVIEWED; không nâng thành gameplay/visual PASS mới. USER_VERIFIED chỉ dành cho xác nhận Chủ Dự Án; báo cáo executor dùng USER/EXECUTOR-PROVIDED EVIDENCE. Số “42 PlayerPrefs writers” chưa có counting scope; scan runtime SetInt/SetFloat/SetString đã review cho 16 call lines, không dùng 42 để quyết định kiến trúc.

## 3. Các sai lệch phải sửa trong đề xuất trước khi code
1. **50 quái thường chết/config `monstersRequired`, không phải 5 wave.** D18.3/.4/.29 chốt 50 hiện hành; P08 chỉ chốt 4–5 quái/wave. Không hỏi lại 50 như TBD, không triển khai 5 wave thành luật.
2. **Progress 100% giữ tới khi thắng Boss.** Thua trả về Normal Screen, không có quyền reset gate để farm lại từ 0. Post-loss UX chi tiết vẫn có phần TBD.
3. **Boss có config và arena riêng.** D18 yêu cầu distinct Boss/config và NormalArena/BossArena; phóng to Monster x1.4 là placeholder đề xuất, không đủ contract hay balance authority. Reuse shared combat engine, không engine Boss thứ hai.
4. **Persistence Stage không chỉ hai Prefs lúc thắng.** D18.23 cần PlayerProgress theo scope, gồm chapter/current/highest/unlocked/bossProgress; partial/gate state phải tồn tại qua reopen. Không dùng title breakthrough index làm PvE stage.
5. **Title unblock claim chưa đúng.** Active `TitleBreakthroughConfigSO` enum L9–20 không có ClearStage; `GetRequirements` L123–136 không đưa legacy requiredStageCleared vào fallback. `TitleBreakthroughManager` L347–442 evaluate list đó. Resources TitleBreakthrough_00..07 đều requiredStageCleared=0; ProgressionManager L289–294 delegate active manager. Chỉ legacy path L311–312 có ClearStage. Thắng Stage không tự làm một requirement hiện tại “ĐÃ ĐẠT”.
6. **Citation sai:** A9 là chest upgrade, A7 Companion combo/counter, A8 status/CC; D17 chest/economy, D18 Stage/Boss, D19 Bun/offline. Không dùng chúng làm chứng cứ 5-wave/Stage-title.
7. **Bun regen không TBD.** 1/20s đã LOCKED. Bỏ Bun/Companion/offline chỉ là phased implementation gaps, không chứng minh full Stage loop đã đầy đủ/độc lập.
8. **Fallback không chỉ bật hai flags.** Dash asset được đề xuất còn distance/speed=0 và DamageEffect; phải validate movement-only contract/data/load path trước một rollout riêng. Không enable dù chỉ hai asset trong task sửa Gold-only.

Rủi ro content riêng đã quan sát ở bốn bản asset mẫu skill_nyin_3_b/skill_nyin_4_a (Data và Resources): mỗi file khoảng 8.1 MB, có 14,555–14,556 DamageEffectDefinitionSO YAML blocks; main skill chỉ tham chiếu một effect. Ghi backlog content hygiene/authoring; chưa đo runtime/import cost, không suy thành multi-hit và không tự cleanup hoặc mở lại P09.

Đính chính ở review này đủ để tiếp tục; không yêu cầu Antigravity audit lại tám nhóm hoặc sửa báo cáo cho “đẹp”.

## 4. Việc thực thi kế tiếp — F-RECYCLE-GOLD-ONLY-01
**Contract đã LOCKED:** A10 và CURRENT_DESIGN_AUTHORITY quy định recycle Gold-only. Không tìm thấy explicit later decision cho phép recycle Material; P08 loot/modal acceptance không tự supersede reward rule. Vì vậy không cần Chủ Dự Án chọn lại YES/NO về Material.

Đường source đã đọc: `LootDecisionUI.OnDismantleClicked` → `BattleManager.CompleteLootDecisionAndResume(dismantle:true)` → `ResourceManager.DismantleEquipment` L132–151. Hàm đang AddGold và AddMaterial. Bản sửa tối thiểu: giữ Gold formula hiện tại như baseline chưa LOCK balance; bỏ Material award; giữ API tương thích nếu cần bằng matGain=0; log phản ánh đúng; bảo toàn item/loot/modal/once-only authority.

Prompt executable tách riêng: [PROMPT_ANTIGRAVITY_FIX_RECYCLE_GOLD_ONLY_20261005.md](PROMPT_ANTIGRAVITY_FIX_RECYCLE_GOLD_ONLY_20261005.md). Antigravity sửa source và chạy chỉ kiểm chứng bị ảnh hưởng với Save Guard đã có; Codex review patch/log trước nghiệm thu. **Chưa có code sửa hoặc test mới trong lượt review này.**

Dependency cụ thể: scan hiện có tìm thấy nguồn AddMaterial ở dismantle và nút debug EquipmentDropDebugUI; chưa xác lập nguồn cấp Material production khác. Title/chest hiện có cost Material. Bản sửa không được bù bằng reward mới, giảm cost, tặng Material hoặc trừ ngược save cũ. Ghi đây là gap economy/content cần scope sau; không tuyên bố vòng progression hoàn chỉnh.

## 5. Hướng gameplay sau bản sửa — PROPOSAL
Ưu tiên kỹ thuật đề xuất vẫn là **Stage progress + Boss Gate foundation**, tách khỏi full Boss combat:
- progress từ legitimate normal monster deaths, dedupe identity; config50; separate completed-wave growth;
- khi kill50 nằm giữa wave, latch gate pending, cho roster 4–5 quái hiện tại/channel/loot/modal hoàn tất hợp lệ rồi enter gate; không trim wave hoặc destroy sống để giả completion; không spawn normal wave tiếp theo sau gate;
- HUD tiến độ chức năng; distinct PvE stage identity; scope StageProgress persistence cho partial/gate state, backward-compatible với save hiện có;
- không Boss fight/win/reward/title-requirement mutation; không skill rollout; Bun/offline/Companion ghi rõ deferred;
- đây là partial foundation có thể kiểm chứng, không claim player-complete Stage loop.

Đây là brief kỹ thuật để chốt task mới sau review bản sửa, **chưa là implementation authorization**. Full Boss tiếp theo còn cần quyết định exit/background/reopen reset-loss hay resume, post-loss UX trong retained gate, production boss/reward/config và stage unlock rules; không hỏi các điều đã LOCKED như50/60s/regen. Có thể ưu tiên interactive existing-skill selection riêng nếu sản phẩm chọn hướng đó, nhưng bỏ rollout hai flags khỏi task UI.

## 6. Baseline được bảo toàn
P08/P09-A/P09-B giữ ACCEPTED/LOCKED. P08 Gate2=135/137 exit1; E2 P09-A SCENARIO_PASS_EXIT_TIMEOUT / wrapper exit2; S01–S05 và M1–M7/GUI USER_VERIFIED. F-SAVE-P09B-01 CLOSED/RECOVERY ACCEPTED; PID25360 USER_REPORTED_PASS/RESTORE_UNRESOLVED; raw Restore/execution PARTIAL; Compare/preflight METADATA_ONLY; snapshot recovery không phải live full-byte Registry observation.

Hiện **không cần Chủ Dự Án chơi thử hoặc gửi ZIP cũ**. Manual check mới chỉ cân nhắc sau delta source/test nếu có behavior còn chưa kiểm được; chỉ đường Tách bị đổi, không lặp toàn P08/P09.

## Nguồn
- [A10 và precedence](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/caa089d6c32e531c9d64f16ff8bf071f4fe7e402/D1_D23_AMENDMENTS_LOCKED.md).
- [D18 Stage/Boss/config/progress](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/caa089d6c32e531c9d64f16ff8bf071f4fe7e402/D17_D18_LOCKED.md#L211).
- [D19 Bun/gate](https://github.com/huycodedie/Ai_MEMORY_TLTD/blob/caa089d6c32e531c9d64f16ff8bf071f4fe7e402/D1_D23_LOCKED.md#L73).
- [ResourceManager](https://github.com/huycodedie/ta_la_ta_de/blob/0ad2af344134401c98f30567e581e2691ae8171f/Assets/_Game/Progression/ResourceManager.cs#L132), [active requirements](https://github.com/huycodedie/ta_la_ta_de/blob/0ad2af344134401c98f30567e581e2691ae8171f/Assets/_Game/Data/TitleBreakthroughConfigSO.cs#L9).
