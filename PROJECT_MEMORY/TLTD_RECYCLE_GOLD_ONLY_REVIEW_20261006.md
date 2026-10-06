# TLTD — Review F-RECYCLE-GOLD-ONLY-01

Ngày: 06/10/2026, UTC+07:00. Reviewer: Codex, Tech Lead/Architect.

## Kết luận và bước tiếp theo

**Sửa runtime Gold-only: ACCEPTED trong phạm vi source và batch EditMode fixture đã review. Closeout tổng F-RECYCLE-GOLD-ONLY-01: PENDING HARNESS SAFETY FIX.** Không tạo một thiết kế LOCKED mới: Gold-only đã được khóa ở amendment A10. Công thức Gold hiện tại vẫn là preserved implementation / TBD balance.

ResourceManager chỉ cấp Gold; tuple trả `(goldGain, 0)`. Gold formula, null semantics, inventory removal và các API Material khác giữ nguyên. Bốn nhóm assertion cũ được cập nhật đúng contract. Không sửa BattleManager, LootDecisionUI, scene hoặc production skill assets.

Rủi ro mới cụ thể nằm trong runner Editor vừa thêm: menu chạy trực tiếp có thể ghi PlayerPrefs và thay singleton/scene state ngoài wrapper Save Guard. Vì vậy chưa đóng toàn bộ task. Giao [PROMPT_ANTIGRAVITY_RECYCLE_HARNESS_SAFE_ENTRY_20261006.md](PROMPT_ANTIGRAVITY_RECYCLE_HARNESS_SAFE_ENTRY_20261006.md), chỉ sửa tooling mới; không làm lại runtime Gold-only hoặc P08/P09.

Chưa yêu cầu Owner chơi thử hoặc đóng ZIP. Stage progress/Boss Gate vẫn là proposal sau corrective closeout, chưa được giao implementation. Audit gameplay đã đóng với review addendum; không audit lại toàn dự án.

## Baseline Git và phạm vi kiểm tra

- Canonical main được đọc qua checkout tách biệt: `8509e7fc1d37027786d765b2940b886fc89d588d`.
- Game remote main và local `E:\code\TLTD` HEAD khi review: `01c5b23fa47dde9a61306f8d894a497b6d882673`, parent `6d9e1923b07deb28ca66d2ba3e1bb13e2c22c7be`. [Commit source đã tồn tại](https://github.com/huycodedie/ta_la_ta_de/commit/01c5b23fa47dde9a61306f8d894a497b6d882673) gồm đúng bốn path dưới đây.
- Báo cáo executor lúc 00:06 ghi baseline `5eafa685...` và không commit/push. Git hiện hành đã tiến lên sau snapshot đó. Codex chỉ ghi nhận commit đã tồn tại, không suy đoán actor thực hiện hoặc quy kết sai báo cáo lịch sử. Codex không tạo/push commit source này.
- Local có ba tracked modifications và 530 untracked entries (533 dòng porcelain `--untracked-files=all`) khi review. Ba tài liệu dirty giữ đúng bytes/hash của inventory executor. Không pull/reset/stash/clean/checkout overwrite/stage tại E:.
- Đọc actual source, patch, FIX_REPORT/PROVENANCE, script wrapper, ba Unity logs, ba wrapper logs và final journal metadata. Không đọc/export nội dung save backup, không live-compare Registry, không chạy Unity/test/Save Guard.

### Local source bytes đã đo

| Path dưới E:\code\TLTD | Bytes | SHA256 |
| --- | ---: | --- |
| Assets/_Game/Progression/ResourceManager.cs | 4729 | E5BB03C03B5EA1ADCE10251F1CCE6A6472E8D8795C40AE0779B3288AC34EA12C |
| Assets/_Game/Editor/Prototype01PlayTestRunner.cs | 1512894 | C66406CB4FE660CBFC0A80CD4E607F5DB746091FF3312277DCA9D92092D51A09 |
| Assets/_Game/Editor/Prototype01PlayTestRunner_RecycleGoldOnly.cs | 20141 | 0C2087496C94E8CAACAB2AB2286AF7BE6744F978020EB66A3F5AED83E9AB9731 |
| Assets/_Game/Editor/Prototype01PlayTestRunner_RecycleGoldOnly.cs.meta | 59 | 98703CD9C3E4CC62414519287F9A4F74547F34C6A8C9E9AEB0474F91BBE45173 |

Checkout remote trên Windows có khác biệt newline; cả bốn file tương đương text sau normalize CRLF/LF. Chỉ base runner khớp raw bytes trong checkout đo lần này; không tuyên bố 4/4 remote/local raw byte identity. GUID meta hiện tại `b1e3240415cf9f6418044f7443e55379`; không đổi GUID để khớp bảng cũ.

Dirty docs preserved: MANUAL_P09A_CHECKLIST.md 16177 bytes / `09C34360D9E3C93FFEC5A2043A5E2762FB4D06E7C574D6A5B40457BC22DF4004`; local ACTIVE_WORK_HANDOFF.md 6358 / `842B4FED974040C4F69F947C82585700502ACE0594E822DE70A66A4FAB728B48`; REVIEW_RESPONSE.md 16925 / `E9143AE8E923EFEAE091E2D0BA49D2F7845D3950EE01D271E117C69D2B4E4332`. Local ACTIVE chưa đồng bộ canonical; dùng snapshot tách biệt, không ghi đè file đó.

## Evidence thực có và giới hạn

Artifact root: `E:\code\TLTD\scratch\recycle_gold_only_20261005_234850`.

| Run | Session start +07 | PID | Seconds | OS exit | Test result | Wrapper restore/compare |
| --- | --- | ---: | ---: | ---: | --- | --- |
| 1 | 05/10 23:53:28 | 25604 | 123.71 | 1 | R2 reflection exception | SUCCESS / DiffCount=0 |
| 2 | 05/10 23:57:24 | 8504 | 83.45 | 1 | 4/5; R3 fixture FAIL | SUCCESS / DiffCount=0 |
| 3 | 06/10 00:00:04 | 26520 | 115.97 | 0 | 5/5; ALL_PASS=True | SUCCESS / DiffCount=0 |

Ba raw wrappers ghi không timeout; budget 300s. Run1/run2 thất bại được giữ nguyên. Một backup directory dùng lại; journal hiện tại chỉ corroborate final run (Timestamp 00:00:06, restore 00:02:05, VERIFIED và BackupHash khớp file). Hai run đầu dựa vào raw wrapper lịch sử. Đây không phải Codex xác minh live Registry ở thời điểm review.

Command thực trong script/log:

```text
E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe
-batchmode -quit -projectPath E:\code\TLTD
-executeMethod WuxiaGame.Editor.Prototype01PlayTestRunner_RecycleGoldOnly.RunRecycleGoldOnlyCLI
```

Runner synchronous trong Editor, không vào Play Mode hoặc chờ natural frames. Final log không có C# compile errors/managed exceptions; vẫn có licensing/D3D/network diagnostics. Không gọi kết quả đó là zero warnings hoặc một phiên người chơi thao tác thật.

| Check | Phạm vi chứng minh |
| --- | --- |
| R1 | Gold delta/tuple0, Material/EXP unchanged, item removal trong fixture |
| R2 | null và invalid/premature/duplicate qua production BattleManager API; state được seed reflection |
| R3 | hai item, queue identity, duplicate guard và dummy modal blocking/release qua API EditMode |
| R4 | AddMaterial/ConsumeResources fixture vẫn hoạt động; không chứng minh production có nguồn Material khác |
| R5 | immediate PlayerPrefs readback và unsubscribe listener riêng của test |

R3 không chứng minh LootDecisionUI click, modal/pause cleanup đầy đủ hoặc next normal wave theo frame. R5 không chứng minh reload/disk durability toàn diện hoặc toàn bộ listener/pause owners. Bốn assertion P05.7-14 / P05.7.2-28 / P05.7.3-13 / P05.7.4-16 trong base runner được **source updated, NOT EXECUTED** trong ba run này.

Tech Lead chấp nhận các giới hạn R3/R5 cho delta reward nhỏ: production transaction/UI/modal/wave không thay đổi, P08/B1 acceptance vẫn có hiệu lực. Không cần chạy lại acceptance cũ để tăng tuyên bố evidence. Nhưng entry/teardown của runner mới là rủi ro mới và phải sửa riêng.

## Findings phải xử lý

**F-RECYCLE-HARNESS-SAFE-ENTRY-01 — OPEN.** Runner L32 exposes MenuItem; public suite/CLI không từ chối interactive Editor trước fixture setup. R1–R4 SetResources ghi save; R5 chỉ chụp hai giá trị sau các case trước, SetInt lại làm mất key-absence semantics. Setup resets singletons, R3 có thể dùng ModalCoordinator đang tồn tại. GUI invocation không có bảo vệ tương đương wrapper.

Teardown bổ sung trong cùng finding: R2/R3 bmGO chỉ DestroyImmediate trên normal path, exception bỏ sót; fixture-owned normal monsters do production API tạo là root objects, BattleManager.OnDestroy không hủy chúng. Chỉ dọn objects/listeners/modal owners task tạo, không dọn scene toàn cục hoặc sửa runtime để chiều test.

## Đính chính provenance và patch — không sửa artifacts gốc

1. Actual PROVENANCE.md: 9109 bytes / `D358CC5184A17B891629A6A4AAC1118236AFDE5AC6E838CD09315D5583D635CE`; hash tự ghi DD95… không khớp. Không đặt hash của chính tài liệu bên trong nó làm final manifest.
2. Actual local meta: 59 bytes / 98703C… theo bảng trên, không phải 215/A6D2… .
3. Command trong provenance ghi Unity2022.3.21f1, -nographics và method không namespace là sai. Command thực là Unity6000.6.0f1/CLI ở trên.
4. Các block `[RECYCLE_GOLD_ONLY]` là diễn giải, không verbatim raw lines. Raw markers thật là `[R1 RESOURCE RESULT]`, `[R2 REJECTED/NULL]`, `[R3 SEQUENTIAL LOOT]`, `[R4 UNAFFECTED RESOURCES]`, `[R5 PERSISTENCE & CLEANUP]`. Giữ raw logs gốc.
5. Patch gốc 5137 bytes / D093… chỉ chứa hai tracked paths, thiếu runner/meta mới và có chữ TÁCH bị biến đổi encoding. Không dùng nó như standalone complete patch.

Codex đã xuất patch bổ sung từ commit source đã tồn tại: `outputs/0001-Award-only-gold-when-recycling-equipment-and-update-.patch`, 26983 bytes / SHA256 `6BAF0F005FC372DF503F32A718AFD1C00CE18D95E5F56E8DBD6920E44B60B0FB`. Đủ bốn paths; chữ TÁCH đúng; reverse apply --check thành công trên checkout tách biệt và git diff --check thành công. Đây là Git commit delta từ parent6d9e192, không một bảo đảm áp dụng raw-byte-identical vào dirty E:; không apply tại E:. Patch vẫn chứa runner chưa sửa safe-entry, chỉ dùng review/archive. Không đưa production patch vào memory repository.

## Continuity được bảo toàn

P08/P09-A/P09-B = ACCEPTED / LOCKED theo phạm vi cũ. P08 Gate2=135/137 exit1 với legacy test IDs07/09; P09-A E2=SCENARIO_PASS_EXIT_TIMEOUT/wrapper exit2. S01–S05/M1–M7/GUI giữ USER_VERIFIED.

F-SAVE-P09B-01 CLOSED / RECOVERY ACCEPTED giữ snapshot 04/10 15:14:04+07, run REC-P09B-20261004-151227, 24/24 names/kinds/truncated16hex digests; PID25360 vẫn USER_REPORTED_PASS / RESTORE_UNRESOLVED. Raw Restore/execution PARTIAL và Compare/preflight METADATA_ONLY không được “nâng cấp” nhờ recycle runs này.

Material acquisition ngoài recycle/debug còn là economy/content gap; không thêm reward, refund, đổi costs hoặc balance. Stage count config50 normal kills, giữ100% tới Boss WIN, loss về Normal Screen; Stage/Boss arena/config riêng dùng chung combat engine. Đó là contract tham chiếu, chưa phải authorization code feature tiếp theo.
