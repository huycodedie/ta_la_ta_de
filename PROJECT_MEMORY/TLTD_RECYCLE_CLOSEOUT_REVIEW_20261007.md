# TLTD — Recycle corrective closeout

Ngày review: 07/10/2026, UTC+07. Codex, Tech Lead/Architect.

## Quyết định nghiệm thu có phạm vi

**F-RECYCLE-HARNESS-SAFE-ENTRY-01 = CLOSED / FIX VERIFIED** trong phạm vi safe entry được review source và một guarded batch EditMode run thành công. **F-RECYCLE-GOLD-ONLY-01 = CLOSED / FIX VERIFIED WITH EVIDENCE LIMITS**, kết hợp runtime Gold-only đã nghiệm thu ngày06/10 với follow-up này.

Đây là khôi phục contract Gold-only A10 đã LOCKED, không một thiết kế/balance mới. Công thức `Mathf.Max(50, item.EquipmentLevel * 100)` vẫn là preserved implementation/TBD balance. Không cấp Material/EXP từ recycle, không tự bổ sung nguồn Material, đổi costs hoặc xử lý ngược save đã có.

Không còn blocker cụ thể trong delta được giao. Không yêu cầu run nữa, Owner chơi lại hoặc đóng ZIP. Không nâng nghiệm thu này thành PlayMode/UI/natural-frame/reload/crash coverage.

## Đã kiểm thực tế

Đọc attachment báo cáo mới, actual runner, source diff, HARNESS_FIX_REPORT/PROVENANCE_ADDENDUM, patch, script, preflight log, raw Unity/wrapper logs, journal metadata và các hash liên quan. Không chạy Unity/test/Save Guard hoặc live Registry compare; không đọc/export nội dung backup.reg, sửa source/E: hoặc thao tác save.

Main canonical lúc review: `d1da751bd5b0eeeab37954feff9698ec9d4288a3`. Game remote và local E:\code\TLTD HEAD: [393a0487083fd1e311db28cdf07393427769a4de](https://github.com/huycodedie/ta_la_ta_de/commit/393a0487083fd1e311db28cdf07393427769a4de), parent `5d38dbbb4e6e49bf94105ba1ed38adff8b938cdd`. Commit này đã tồn tại trước Codex review và chỉ thay đổi runner mới. Báo cáo executor23:45 ngày06/10 ghi không commit/push; commit quan sát có timestamp00:06 ngày07/10. Giữ hai snapshot theo thời điểm, không suy đoán actor. Codex không tạo/push source commit đó.

Local status: ba tracked modifications +530untracked entries, tổng533porcelain rows. Bảo toàn local ACTIVE đang dirty, đọc canonical bằng checkout tách biệt.

### Integrity local

**16/16 file/artifact bytes và SHA256** trong báo cáo dán khớp phép đo trên files thật. Sáu raw Unity/wrapper logs của Gold-only run1–run3 vẫn khớp hashes ở review trước; failures không bị ghi đè. Meta GUID giữ `b1e3240415cf9f6418044f7443e55379`.

| Path dưới E:\code\TLTD | Bytes | SHA256 |
| --- | ---: | --- |
| Assets/_Game/Editor/Prototype01PlayTestRunner_RecycleGoldOnly.cs | 28232 | F773DD66894908C839647820C06622A3A43AC340F94EA5D625DF082641CA17D5 |
| Assets/_Game/Editor/Prototype01PlayTestRunner_RecycleGoldOnly.cs.meta | 59 | 98703CD9C3E4CC62414519287F9A4F74547F34C6A8C9E9AEB0474F91BBE45173 |
| Assets/_Game/Progression/ResourceManager.cs | 4729 | E5BB03C03B5EA1ADCE10251F1CCE6A6472E8D8795C40AE0779B3288AC34EA12C |
| Assets/_Game/Editor/Prototype01PlayTestRunner.cs | 1512894 | C66406CB4FE660CBFC0A80CD4E607F5DB746091FF3312277DCA9D92092D51A09 |
| MANUAL_P09A_CHECKLIST.md | 16177 | 09C34360D9E3C93FFEC5A2043A5E2762FB4D06E7C574D6A5B40457BC22DF4004 |
| PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md | 6358 | 842B4FED974040C4F69F947C82585700502ACE0594E822DE70A66A4FAB728B48 |
| REVIEW_RESPONSE.md | 16925 | E9143AE8E923EFEAE091E2D0BA49D2F7845D3950EE01D271E117C69D2B4E4332 |

Runner before20141bytes/hash0C2087496C94E8CAACAB2AB2286AF7BE6744F978020EB66A3F5AED83E9AB9731 đã đo trong lượt trước. Sáu rows còn lại giữ nguyên so với baseline trước task. Checkout remote Windows có newline conversion; runner remote checkout28834bytes/hash9C96704F… tương đương text với local sau CRLF/LF normalization, không raw byte identity.

## Source delta đã review

- Không còn MenuItem; chỉ public CLI, orchestrator/cases/helpers private. CLI L41–45 trả về khi không batch trước scene/fixture/singleton/modal/PlayerPrefs và không Exit Editor tương tác.
- Từ chối PlayMode/transition L48–53. Tạo transient empty scene L58 trong child batch; không SaveScene/asset write; kiểm context không reuse singleton bên ngoài.
- Setup L526–563 kiểm ownership trước tạo root; catch dọn partial root. Không còn ResetInstance calls.
- R2/R3 finally dọn monsters do BattleManager fixture sở hữu rồi bmGO/root/coordinator. R3 chỉ đóng TestModal_R3 và unregister dummy view của task; không dismiss/reset coordinator có trước, không scene-wide object sweep.
- R5 giữ HasKey/value và DeleteKey khi key vắng; unsubscribe listener trong finally. Comments phân biệt per-case isolation với full baseline recovery của outer wrapper. Reward/duplicate/queue/API predicates giữ nguyên; production/general runner/meta không đổi.

## Run thực có evidence

Artifact root: `E:\code\TLTD\scratch\recycle_harness_safe_entry_20261006_233820`.

| Thông số | Evidence đã đọc |
| --- | --- |
| Engine/method | Unity6000.6.0f1; WuxiaGame.Editor.Prototype01PlayTestRunner_RecycleGoldOnly.RunRecycleGoldOnlyCLI |
| Mode | -batchmode -quit; synchronous EditMode; không EnterPlayMode/natural frames |
| Session / launch / finish +07 | 06/10 23:38:55 / 23:38:59 / 23:41:24 |
| PID / duration / budget | 10364 /145.41s /300s |
| OS exit / timeout | 0 /False |
| Suite | R1–R5 PASS;5/5;ALL_PASS=True |
| Preflight retained | old recycle journalVERIFIED; new artifactNO_JOURNAL trước backup/launch |
| Restore / compare | raw wrapper23:41:26SUCCESS;23:41:28Diff=0 |
| Final journal | VERIFIED; backup23:38:58; restore23:41:26;ValueCount24;BackupHash khớp file |

Raw R1–R5 lines ở Unity log560/802/1101/1114/1141; summary1166/ALL_PASS1190. Không có C# compile error/managed exception hoặc `[CLEANUP]` exception warning trong run này; licensing/D3D/service-network diagnostics vẫn được giữ nguyên. Không claim zero warnings.

Restore/Compare là executed historical evidence từ wrapper và source guard, được journal metadata corroborate. Codex không xác minh lại live Registry. R1 resource result, R2 invalid/null/duplicate API, R3 dummy modal/queue, R4 unaffected API và R5 immediatePrefs/own listener giữ phạm vi đã mô tả trong [review trước](TLTD_RECYCLE_GOLD_ONLY_REVIEW_20261006.md). Bốn assertion sửa trong base runner vẫn SOURCE UPDATED / NOT EXECUTED.

## Giới hạn và đính chính — giữ original artifacts

1. Interactive/PlayMode rejection, partial setup exception và key-absence branch được **SOURCE REVIEWED**, không chạy negative/fault-injection riêng. Successful batch5/5 không chứng minh mọi branch ngoại lệ.
2. R2 kiểm bmGO reference null; R3 kiểm bmGO/coordGO null và ModalCoordinator.Instance null; R5 kiểm listener flag sau teardown. Các postconditions đi vào result trả về, nhưng không independent census mọi Monster/root/config. Không dùng “toàn bộ object count0” hoặc “100% mọi ngoại lệ”. Helper monster cleanup chỉ warn nếu cleanup throw; run này không có warning đó. Nếu sau này sửa/exercise cleanup failure, cần kiểm observer/failure signaling theo đúng delta; không mở task/rerun mới chỉ để tăng tuyên bố hiện tại.
3. `-batchmode -quit` không có `-nographics`; gọi batch EditMode, không suy thành graphics-free headless. PID thoát sạch trong budget; core chỉ kill owned process tree khi timeout, không kill tree trong run này.
4. ValueCount24 là **24 Registry values trong scope snapshot**, không24subkeys. Guard so sánh existence/name/type/data, không byte-for-byte file.reg và không global Registry.
5. Patch executor45936bytes/hashF8101054… có BOM FF FE, UTF-16LE. Git reverse-apply check báo `No valid patches in input`; đây là lỗi packaging/encoding, không source/test failure. Codex bổ sung patch UTF-8 từ source commit đã tồn tại, không yêu cầu executor chạy lại hoặc sửa raw logs.

Patch bổ sung local: `outputs/0001-Isolate-gold-only-recycle-tests-and-protect-harness-.patch`,22830bytes/SHA256 `88B60FE8C951BC48901EDEE44651DD9CAAC0C03B330A2DD804A0D0858A6C95DB`. Đúng một runner path từ parent5d38dbb→393a048; reverse apply --check thành công trên checkout tách biệt và git diff --check thành công. Chỉ review/archive, không apply vào dirty E:, không copy production patch vào memory repo.

Các limits trên được Tech Lead chấp nhận cho corrective closeout này: rủi ro MenuItem đã bị loại bỏ, normal fixture cleanup đã sửa, thực thi chỉ qua guarded child batch, baseline restore/compare có evidence. Không có lỗi production/save mới quan sát được. Không biến giới hạn proof thành universal guarantee.

## Bước tiếp theo cụ thể

**SPEC-STAGE-GATE-READONLY-01 — PROMPT PREPARED / NOT EXECUTED.** [Prompt riêng](PROMPT_ANTIGRAVITY_STAGE_GATE_SCOPE_SPEC_READONLY_20261007.md) yêu cầu một implementation-scope/spec packet, không code hoặc lặp eight-group audit.

Tập trung progress death50 giữa wave; single accepted death authority; loot/channel/modal drain; mọi start/restart/advance route; checkpoint/load-before-bootstrap và risk RAM dedupe/loot queue. Gate-only candidate có thể chặn farm khi chưa có Boss fight: phải đưa lựa chọn exposure vào decision packet, không tự bật trong main player scene. Scope/data/acceptance plan là PROPOSAL để Codex review và Owner quyết định sản phẩm khi cần.

Stage/Boss implementation vẫn NOT APPROVED. Không Boss fight/win/reward/advance, nguồn Material, balance, skill rollout, Bun/offline/Companion hoặc UI polish. Material acquisition production còn là economy/content gap. Không tạo false challenge button hoặc tuyên bố full gameplay loop hoàn chỉnh.

## Baseline acceptance/recovery được bảo toàn

P08/P09-A/P09-B ACCEPTED / LOCKED theo scope cũ; S01–S05/M1–M7/GUI USER_VERIFIED. P08 Gate2=135/137 exit1 với legacyIDs07/09; P09-A E2=SCENARIO_PASS_EXIT_TIMEOUT/wrapperexit2. Không regrade raw outcomes.

F-SAVE-P09B-01 CLOSED / RECOVERY ACCEPTED; PID25360 USER_REPORTED_PASS / RESTORE_UNRESOLVED. Recovery run REC-P09B-20261004-151227/snapshot04/10 15:14:04+07,24/24names/kinds/truncated16hex digests, raw Restore/executionPARTIAL và Compare/preflightMETADATA_ONLY vẫn giữ nguyên. Recycle runs không nâng cấp lịch sử này thành live full-byte Registry proof.
