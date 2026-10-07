# TLTD — Stage Gate R1 review và scope kiểm chứng lab

Ngày07/10/2026, UTC+07. Codex — Tech Lead/Architect.

**R1 = REVIEW COMPLETE / CLOSED AS ANALYSIS WITH TECH LEAD ADDENDUM.** Các sửa citation/source/matrix hữu ích được giữ. Không công nhận tuyên bố “toàn bộ resolved”, full Stage Gate spec đã sẵn sàng hoặc gameplay mới đã được nghiệm thu.

**Quyết định kỹ thuật tiếp theo: SPIKE-STAGE-GATE-LAB-01 — AUTHORIZED LAB ONLY / PROMPT PREPARED.** Antigravity được kiểm chứng integration bằng source thật trong bản sao Unity dùng riêng cho lab, với policy Stage RAM-only. Prompt thực thi riêng: [PROMPT_ANTIGRAVITY_STAGE_GATE_LAB_SPIKE_20261007.md](PROMPT_ANTIGRAVITY_STAGE_GATE_LAB_SPIKE_20261007.md).

**Stage/Boss implementation trong project chính, activation/exposure và source publication vẫn NOT APPROVED.** Lab không hoàn thành PlayerProgress/D18.23, progression xuyên phiên hoặc vòng gameplay Stage→Boss. Đây là quyết định Tech Lead về kiểm chứng kỹ thuật, không phải thiết kế sản phẩm mới LOCKED. Không hỏi Owner một lựa chọn kiến trúc đã có thể chốt; không yêu cầu chơi thử/ZIP cho bước này.

## 1. Inputs thực và main hiện hành

Đã đọc attachment R1 và sáu files thực tại `E:\code\TLTD\scratch\stage_gate_scope_spec_r1_20261007_195000`. Originals giữ nguyên. Bảng SHA256:

| Artifact | Actual bytes | SHA256 |
|---|---:|---|
| STAGE_GATE_SCOPE_SPEC_R1.md | 30791 | 4340304D647DE7B9E19E7A911A9E21DEE1C4F820EEE91FF64CD3505EC08BF841 |
| STAGE_GATE_ACCEPTANCE_MATRIX_R1.md | 11674 | 97ADBC7E15CC5DBD1135B34217D7F7CF7540B8035EDA6A9FEC545EFC85A48F8F |
| STAGE_GATE_DECISION_PACKET_R1.md | 9581 | 748A292580F0917E697B18F046C0622AA8D8616BA312EB82000B1B1EF8E79ABF |
| REVISION_MAP_R1.md | 10336 | 8F01AA501DFCBDEF4A3ABBE0F1E79B8EF74B4ED92FE5DCC29F13E66C388AF9AE |
| PROVENANCE_R1.md | 7851 | B528C1B8428EF271DE2F99666CD239386257E45AE8B9E678CB02DC181EA8D9B8 |
| MANIFEST_SHA256.txt | **503** | ED855E89875CE9347DC2AAD904630C7D77B669A6D8E1323B9431A7C66F34EA3A |

Sáu hashes khớp báo cáo; manifest5/5 payloads khớp. Manifest actual503 bytes có UTF-8 BOM3bytes; báo cáo500 không tính BOM. Metadata correction bằng addendum, không sửa bản gốc hay chạy test.

Main trước publication review này: canonical memory **b6d5d0ee01aca8c624a08c083253d41cc60ccb56**; game và E:HEAD **7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425**. Hai docs commits1410af4/7441b9e kế tiếp source393a0487083fd1e311db28cdf07393427769a4de; không suy diễn actor cập nhật E:. R1 ghi local1410 là snapshot executor, không current ref của Codex.

Provenance có16 rows:12 runtime +2 D authorities **14 exact matches**; hai authority rows sai/thiếu nguồn đo. Review cũ canonical blob15657bytes/hashEEFD0A…; E:15768bytes/hash28C20593… không phải12183 như R1 và row này không có SHA. CURRENT_DESIGN_AUTHORITY E:13614bytes/hash4B6491F657E4C409C53A3032E6F017B7EDA1BA1023ED097C18E66AE235698F46, không còn12661/07D3…; canonical blob tại b6d5d0e là13457bytes, cần phân biệt Git LF payload với local encoding/newlines. Không coi origin/main label là exact read provenance.

Bốn originals đợt đầu và script helper vẫn khớp hashes; ba dirty docs MANUAL_P09A_CHECKLIST.md/ACTIVE_WORK_HANDOFF.md/REVIEW_RESPONSE.md vẫn đúng16177/6358/16925bytes và hashes09C343…/842B4F…/E9143A…. Snapshot Codex07/10 20:00:02+07: E:HEAD7441b9e,3tracked modifications+530untracked entries. Historical before/after của executor vẫn reported evidence; không phải chứng minh full working tree lịch sử.

## 2. Kết quả R1 được giữ và các claim bị supersede

Giữ: gate100% đến BossWIN; BossLOSE không reset0/reward rollback; same-session Start resume survivors; hook count ngoài optional drop sau943–944; EXP listener riêng; DropSystem.AddItem trước enqueue; RAM inventory/queue và independent resource writes; natural-frame evidence khác EditMode; zero living khác registry.Count0; deterministic4+4+5 và next/current multiplier phân biệt; bỏ production Replay/reset; proposals chờ review.

Addendum này thay các claim còn thiếu căn cứ sau:

| Claim trong R1 | Disposition của Codex |
|---|---|
| Isolated fixture “100% SaveGuard/no Registry pollution/proves all logic” | Chưa có implementation/run/proof. Stage key prefix không cô lập existing EXP/Gold keys. Chỉ có kết quả measured trong profile lab sau execution; không bảo đảm mọi failure. |
| Giữ nguyên3 production files, chỉ thêm Editor harness nhưng chứng minh real gate guards/HUD | Không executable theo source hiện tại: BattleManager chưa có Stage hook/policy seam. Chọn source integration thật trong disposable lab; Editor mirror chỉ chứng minh model. |
| Toàn bộ14 routes đã đủ | Table thực tế **17 rows, A3/B8/C5/D1**; hai rows thiếu một cell. Còn cần phân loại ResumeCombat, RegisterHero, DeferNextLootDecisionRequest/initialization side effects. Coverage theo actual inventory/callers, không theo con số14. |
|12 test cases |12families SG01–12, **13 rows** do SG04A/B. Tất cả PLANNED/NOT EXECUTED; chưa có Stage PASS. |
| Schema đầy đủD18.23 | HighestStageUnlocked chưa thay được unlockedStages identities/mapping; thiếu growth/checkpoint identity và durable semantics. Schema/current invalid fallback chưa bảo vệ savedReady. **Full schema NOT READY, deferred khỏi lab**. |
| Corrupt/future data fallbackNormalActive hoặc disable flag→endless farm là safe | Giữ bytes không đủ nếu bypass100% saved gate. Không chọn production fallback này. Lab không đọc/ghi Stage persistence, không dùng SG10/11 để claim product reopen. |
| Durable transaction chắc chắn cần global SaveFramework/deferEXP và sẽ phá P08/P09 | R1 chỉ mô tả một phương án rộng; chưa chứng minh mọi bounded receipt alternative bất khả thi hoặc tự động vi phạm lock. Không chọn phương án này; không mở framework hoặc đổi reward authorities. |
| Gold formula trong LockedInputs | A10 Gold-only LOCKED; formula hiện hành là preserved implementation/TBD balance, không một quyết định balance mới. |
| Normal-death order: BattleManager rồi EventBus rồiEXP | HealthComponent raise EventBus trước handlers; BM/Progression subscription order có thể khác. Không dùng thứ tự giả đó như transaction guarantee. |
| Reopen chắc chắn nạp EXP cũ rồi cấp EXP lần hai | Source chứng minh EXP writes độc lập, chưa chứng minh correct cold-start load. Progression.Start khởi tạo defaults; không tìm thấy production autoLoadState caller trong trace. Ghi divergence risk, không runtime replay proof hoặc implicit progression-load fix. |
| InstanceId/removal item là dedupe reward bền vững | ResourceManager.DismantleEquipment trả Gold cho non-null item, không tự có durable receipt ledger. BM UI transaction window bảo vệ trong phạm vi riêng; không biến removal RAM thành exactly-once API guarantee. |

Không gửi R1 lại để sửa toàn bộ các chi tiết metadata. Addendum đóng analysis và chốt một phép kiểm chứng cụ thể; remaining production design giữ NOT READY.

## 3. Rủi ro source mới: finishing channel → first loot

Source tại baseline393a048/runtime current:

- `BattleManager.WaitForFinishingExecutionsThenProceed`1289–1297 tự Dequeue vào pendingLootItem, raise LootPending/Request nhưng không set `_isLootDecisionOpen`.
- `TryPresentNextQueuedLoot`874–890 set cờ true trước request; nhánh trên không dùng method này.
- `CompleteLootDecisionAndResume`1050–1055 trong PlayMode từ chối nếu cờ false. `LootDecisionUI.HandleLootDecisionRequested`268–290 trình bày item/request modal, không mở cờ BM.

Đây là **static integration hazard**, chưa phải runtime FAIL. Owner từng nghiệm thu channel+loot vẫn USER_VERIFIED; không xóa acceptance hoặc suy diễn build/hành vi đã thử. Spike có một **baseline probe giới hạn** để xác định nhánh này có thực sự reachable và có chặn giao dịch trong fixture tự nhiên hay không. Probe dùng source runtime chưa thêm gate, channel request thật, death qua Health/EventBus và first loot/modal thật. Không force cờ bằng reflection, không gọi TryPresentNextQueuedLoot để che nhánh, không dummy modal thay natural flow.

Nếu fail hoặc không thể tạo scenario đúng: capture exact state/frames/request/token/PID/exit/cleanup và dừng trước gate patch; trả mới found blocker cho Codex, không tự sửa accepted source. Nếu pass: chỉ tiếp tục lab integration; không coi probe là retest toànP08 hoặc proof mọi channel path.

## 4. Binding technical scope của SPIKE-STAGE-GATE-LAB-01

### Mục tiêu

Chứng minh một policy RAM opt-in có thể móc vào accepted death/whole-wave/channel/loot engine và chặn normal start/advance bằng source thật, trong lab clone. Phạm vi proof: current-session legitimate count; midwave latch/survivors; safe drain; no next-wave; selected bypasses và natural timing; policy absent preserves bounded observed baseline.

Không claim durable player progress, D18.23, reward exactly-once, crash/reopen, Stage/Boss playable loop hoặc full route coverage nếu evidence thiếu. Không phát hành lab code. Việc copy/apply vào E:/main phải có task review/authorization riêng sau kết quả lab và scope sản phẩm.

### Cách ly và changed paths

Antigravity tạo disposable checkout/source copy ngoài `E:\code\TLTD`, resolve exact main/source và hash relevant runtime đối với E:. Không copy Library/Temp/Logs/UserSettings/save/Registry dumps từ E:. Không dùng production scene hoặc dirty memory làm writable staging. Tạo transient owned scene/canvas/loot views; không lưu scene/prefab/Stage asset.

Trước **bất kỳ Unity launch/import** nào, đổi **chỉ ProjectSettings của lab** sang company/product duy nhất theoRunID. DefaultCompany/TLTD vẫn là profile thật dù project folder khác. All SaveGuard Backup/Restore/Compare/CheckInterruptedJournal calls phải truyền exact matching CustomRegKey + fresh absolute lab BackupDir. Existing guard/VerificationCore có CustomRegKey; không sửa module recovery hoặc để default fallback về E:/real key. Preflight journals có liên quan trước launch; không recovery old journals hoặc bypass global Unity-process conflict. Log Application.companyName/productName/dataPath và identity gate trước fixture mutation; namespace lab comparison chỉ proof lab scope, không live Owner-save comparison.

Future lab-only allowlist:

1. `Assets/_Game/Core/BattleManager.cs`
2. `Assets/_Game/Core/GameBootstrap.cs`
3. `Assets/_Game/UI/BattleHUD.cs`
4. `Assets/_Game/Development/StageGateLabPolicy.cs` NEW + `.meta`; folder `Development.meta` chỉ nếu chưa có.
5. `Assets/_Game/Editor/Prototype01PlayTestRunner_StageGateLab.cs` NEW + `.meta`.
6. `Tools/Verification/StageGateLab/run_stage_gate_lab_spike.ps1` NEW.
7. `ProjectSettings/ProjectSettings.asset` — lab company/product only.

Generated Library/Temp/logs/caches là lab artifacts, không patch paths. Không StageDefinitionSO/config asset, production scene/builder wiring, Progression/Resource/Inventory/Drop/Skill/Modal changes, existing runners/guards hoặc Packages upgrades. Exact path mở rộng phải báo blocker trước edit; không broad refactor hoặc clean/revert dirty E:.

### Policy và integration constraints

- Một injected owned policy per lab BattleManager, default absent/inactive. Stage count/state/threshold chỉ RAM, threshold test-configurable valid >0 (current contract50). Không singleton tự gắn vào production scene hoặc Stage PlayerPrefs/SaveState/LoadState/new migrations.
- Accepted BM membership/dedupe first, hook ngoài optional drop. Count không thay EXP/drop/Gold; ngoài-wave duplicate oracle chỉ Stage/drop của BM, không giả global Progression membership guard.
- NormalActive→GatePending ởthreshold; survivors cùngwave tiếp tục. Full-wave growth vẫnonce/wave. Explicit Restart khi còn survivors có thể giữ existing replace-wave behavior **trong lab**, count capped/unchanged, không tăng completed tier giả. Start giữsame-session resume. Pending đãzero living nhưng drain chưa xong **không auto create replacement wave**. Interrupted final-death/Hero-death/loot-cancel policy phải ghi observed blocked/TBD nếu chưa giải được trong bounds; không fabricate terminalReady hoặc thưởng.
- GateReady finalization một owner, predicate null-safe, legitimate completed wave/no survivors/natural finishing phase/pending displayed item+queue/decision+modal/UI pause/Hero state đúng context. False predicate phải có wait/re-evaluation owner, không early-return strand. Dùng captured encounter/wave identity và **đúng** counter: `_encounterTransitionCounter` khác `_lootTransactionCounter`; threshold-time token có thể đổi ở final deathCancelTransition. Không so current token với chính nó để “prove” validity.
- Guard before side effects: private spawner trước cancel/destroy/create, Start/Restart/revive/enable/retarget/EncounterIndex advance; registration không đưa foreign/late monster vào sealed pending/ready roster. Direct Instantiate object có thể tồn tại trước registration — proof là no accepted participation/auto combat, không tuyên bố constructor của Unity bị chặn. Cleanup chỉ owned object.
- Coroutine cancellation/Hero-death cũng clear `_finishingCasters`; Count0 một mình không chứng minh natural completion. Không bắt coroutine handle==null: channel→loot branch hiện có thể giữ handle hoàn tất. Preserve natural cast requests; stale lifecycle guards và typed token không dùng để cắt caster để đạtReady.
- HUD lab-owned binding, denominator threshold, label endpoint chưa có Boss và disabled action; không production reset/replay/challenge redirect, fakeBossWIN, Stageadvance/unlock hoặc new rewards.

## 5. Planned proof và kết quả cần trả

Baseline probeSG-LAB-00 trước gate patch như§3. Sau probePASS, exact integration checks trong prompt: no-drop49→50; remaining waveEXP/drop, no automaticGold; duplicate/out-of-rosterStage; Hero survivorresume/explicitRestart; natural channel+sequentialloot/modal/pause/no-loot; Ready guards/invalid early advance/stale callbacks; deterministic4+4+5 growth; data-driven HUD và policyabsent. Case IDs/call coverage theo measured rows, không“All14” bằng prose.

EditMode API states tách PlayMode real frames. Lab runner batch-only, owns transient root/managers/subscriptions, refuses unowned singletons, setup/teardown finally-safe; no global reset/shutdown/unowned destroys. One bounded guarded baseline probe, one bounded integration session only if baselinePASS; no repeated broad suites. Known failure/timeout logs giữ nguyên; passscenario+wrapperTimeout phải phân loại riêng như lịch sử, không fake exit0.

Trả source diff hai pha tách biệt (baseline runner/profile/wrapper; gate integration), exact files/bytes/hash, real profile/paths/PID/command/OSexit/timing/assertions/frame telemetry, raw guarded logs/journal compare trong lab scope, before/after E: inventory+measured hashes, limits và missing coverage. Artifacts outside E:, no ZIP/source push. Codex review trước mọi source transfer/product activation.

## 6. Continuity giữ nguyên

P08/P09-A/P09-B ACCEPTED/LOCKED; recycle Gold-only và safe-entry corrective CLOSED theo existing limits. P08Gate2 135/137exit1 legacy07/09, P09-AE2SCENARIO_PASS_EXIT_TIMEOUT/wrapperexit2, S01–S05/M1–M7/GUI USER_VERIFIED không đổi. RecoveryREC-P09B-20261004-151227 snapshot04/10 15:14:04+07/24values/truncateddigests; rawRestore/executionPARTIAL, Compare/preflightMETADATA_ONLY; oldPID25360USER_REPORTED_PASS/RESTORE_UNRESOLVED không đổi. Lab test không bổ sung/rewrite chứng từ recovery cũ.

Codex review này chỉ đọc E: và tạo/publish docs ở checkout continuity riêng; không chạy Unity/tests/SaveGuard/liveRegistry/save. Publication receipt mới ghi exact refs/blobs/preservation. Nextlab đã authorized technicalscope và prompt prepared, **chưa tuyên bố Antigravity đã nhận/chạy**.
