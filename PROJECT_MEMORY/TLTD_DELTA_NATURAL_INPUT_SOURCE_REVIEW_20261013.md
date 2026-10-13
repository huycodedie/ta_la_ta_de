# TLTD — Review source kiểm chứng D01/D05

Ngày review: 13/10/2026, UTC+07. Task được review: `F-TLTD-DELTA-NATURAL-INPUT-PREP-01`.

**Kết luận: STATIC PREPARATION PARTIAL / REVISION REQUIRED / NO RUN AUTHORIZED.** Source được bàn giao và biên dịch được; chưa đáp ứng boundary natural PlayMode/TMP input của prompt. `runAuthorized=false` tiếp tục được giữ nguyên. Đây là review harness, không phải kết quả thực thi D01/D05 hoặc bằng chứng lỗi mới của game.

## 1. Nguồn đã đọc và công cụ thực sự chạy

Đã đọc adapter, launcher, context, compiler script, report, API/manual docs và receipts trong `work/complete_game_next_20261013/natural_input_preparation/`; đối chiếu với source thực tại `E:/code/TLTD`, prompt chuẩn bị và authority canonical. Ba reviewer độc lập đối chiếu D01, D05 và entry/guard/cleanup; Codex chính đọc lại các nhánh và xác minh công cụ riêng.

Main kiểm tra trước cập nhật review:

- `huycodedie/Ai_MEMORY_TLTD`: `f8c88b9156b426741a09f783bb4012102596ef09`.
- `huycodedie/ta_la_ta_de`: `37dc30208d600c61a8639b37682994586b5cf110`.
- Hai checkout publication trên `main`, sạch, `ls-remote` khớp. Local `PROJECT_MEMORY` ở E là bản có thể chậm hơn; hash bản local không thay thế việc xác minh main canonical.

Đã thực hiện độc lập trong `work/complete_game_next_20261013/natural_input_review_20261013/`:

| Kiểm tra | Kết quả | Giới hạn |
|---|---|---|
| SHA256 manifest | 12/12 payload khớp | Packet thực có 21 file: 12 payload, manifest và 8 compiler outputs; manifest không bao phủ 8 outputs này |
| PowerShell AST | Launcher 0 errors; compiler script 0 errors | Chỉ parse, không invoke launcher/compiler script bàn giao |
| Direct Roslyn csc | Runtime126: exit0, 0 errors, 1 warning CS0414 `UIRaycastDebugger.logAllHits`; adapter + builder: exit0, 0 errors, 0 warnings | 128 source inputs; references/defines từ response files của compile hiện hành, DLL Unity đã có. Không Unity compilation hoặc runtime |
| Compile provenance | Source/reference hashes giữ nguyên trong quá trình compile | Receipt riêng lưu từng input, rsp hash, DLL hash; DLL/PDB mới nằm trong thư mục review |
| Packet preservation | 21/21 file không đổi | Giữ nguyên packet đã bàn giao, không sửa report hay receipts gốc |
| E preservation | 786/786 selected fingerprints không đổi trước/sau và so với snapshot delta Unity trước đó | Bao gồm Assets/_Game, Assets/Scenes, ProjectSettings, Packages, index/HEAD và 3 dirty Markdown; không chứng nhận toàn Library/untracked/save thật |
| Git E | HEAD `7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425`; 12 tracked modified + 530 untracked file rows không đổi | `status --porcelain=v1 -uall`; không stage/commit/push/pull/stash/reset/clean E |
| Recovery history | Bốn journal file hashes khớp snapshot trước | Chỉ file metadata; không Restore, Compare live Registry hoặc nâng nhãn recovery/raw cũ |

Trong review này: **không launch Unity, không invoke wrapper dù preflight-only, không execute adapter, không PlayMode, không SaveGuard/live Registry hoặc prefs/save game**. Giới hạn “NOT RUN” của Antigravity là báo cáo của executor; Codex xác nhận các thao tác của chính phiên review này, không suy diễn toàn lịch sử tiến trình máy.

## 2. Findings bắt buộc sửa

`Runner` bên dưới là `adapters/DeltaNaturalInputTestRunner.cs`; `Launcher` là `launchers/run_delta_natural_input_spike.ps1`. Dòng là bản hash đã bàn giao: Runner `7C7C6FB7D91B648F71B7A7170A57917D1A290662511DE3F14A1ABA7F3BA950F8`; Launcher `3E2BC6115F9028675A1650F0D38F4DA5DB18BC3DA532FF09245007D037786026`.

### N01 — Natural PlayMode chưa được triển khai — P1

Runner L175–254 và L256–272 chạy `Action` đồng bộ; không có owned entry/exit PlayMode, driver, coroutine hoặc engine-frame yield. L383–392 gọi `heroCastState.Tick(0.016f)` trong vòng lặp cùng stack rồi L401 gọi thẳng `bm.TryPresentNextQueuedLoot()`. Đó là direct-method simulation, chưa kiểm chứng `Entity.Update` và coroutine BM chờ finishing execution. Boundary receipt ghi `UNITY_PLAYMODE_AND_INPUT_DELTA_VERIFICATION` vượt quá source.

Sửa: driver/phase machine có ownership qua domain reload; channel dùng `hero.CastState` thật, đợi frame engine có deadline/frame bound. Không manual Tick, không forcing presentation/drain để làm predicate PASS.

### N02 — Fixture D01 chưa tạo combat state hợp lệ — P1

- Runner L341–342 tạo cast state local rồi helper L639–643 dùng `Hero.GetType().GetField`. `castState` là private của `Entity.cs:61`; reflection kiểu derived không trả private field base này. Helper âm thầm bỏ qua missing field. BM đọc `hero.CastState`, không phải state local đã channel.
- L338–348 thêm Health mới dù Hero/Monster đều có `RequireComponent(HealthComponent)`. `Entity.Health` lấy/cache component đầu (`Entity.cs:29,108`); damage đi vào component thứ hai. Không chứng minh death đúng entity mà BM quản lý.
- L351–352 chỉ RegisterHero/Monster; chưa khởi động battle. `CurrentBattleState` mặc định None (`BattleManager.cs:740`), registration không đặt InProgress; death handler có guard ở L911. Initial assertion InProgress chưa có dataflow làm đúng.
- Comment/report nói có Inventory/EquipmentManager/Progression nhưng D01 không tạo chúng. Entity/config/lifecycle cũng chưa được initialize đầy đủ.

Sửa: đọc public setup/start APIs thật, init roster/config hợp lệ, dùng `hero.Health`/`monster.Health` duy nhất, xác minh component identity, Hero.CastState, state và drop route trước action. Reflection nếu cần cho fixture metadata phải fail khi field/type không tồn tại; ưu tiên initializer public. Không sửa private state để giả lập battle hoặc completion.

### N03 — D01 thiếu oracle và ép modal drain — P1

L377–381 chỉ kiểm ba predicate phủ định, không chứng minh queue có exact drop: không có drop vẫn có thể thỏa check. L403–407 chỉ non-null, thiếu retained drop/item identity và inventory ownership. L419 đòi Material tuyệt đối bằng0 dù ResourceManager mặc định10 (`ResourceManager.cs:20`); phải đo delta trước quyết định. Gold snapshot L354 nằm trước death, chưa tách thưởng chết hợp lệ. Không có EXP snapshot/check.

Duplicate L424 đúng ý same-stack nhưng chỉ giữ Gold; thiếu closed-window precondition và Material/EXP/Inventory/Encounter/event invariants. L434 gọi `modalCoord.ClearAll()` rồi kiểm counts0; không chờ drain tự nhiên và không có assertion EncounterIndex initial+1 hoặc ổn định sau advance, dù report khẳng định đã có.

Sửa: observer retained drop; trong channel queue>0 với đúng item/request; sau finish đúng pending/displayed/inventory item; snapshot economy sau death hợp lệ, ngay trước Tách; primary qua UI; duplicate ngay cùng stack trước yield; chờ drain/BM defer thật; assert đúng một advance và không advance thêm trong khoảng quan sát bounded.

### N04 — D05 chưa đi qua TMP hit geometry/ancestor dispatch — P1

L544 gọi thẳng `SelectSlot`, L552/L561/L569 gọi thẳng `TrySelectAlternative`. `ForceMeshUpdate` L538–541 không được dùng lấy link geometry/tọa độ. Không có positive `ExecuteEvents.ExecuteHierarchy`, raycast hoặc cùng-link press/release. UI root L501–503 là sibling của Canvas/panel/TMP L504–517; dispatch từ TMP qua ancestor không đến UI. Production builder bind MindMethodUI ở canvasGO (`Prototype01SceneBuilder.cs:1734`).

L578–615 chỉ có right/hidden/decorative mismatch, thiếu slot→alternative, A→B cùng TMP, nonlink→link; events chưa có valid geometry/hit baseline nên no-op không phân biệt đúng guard. `StandaloneInputModule` không khớp project `activeInputHandler:1`; chưa có execution để kết luận module lỗi, nhưng phải chọn module/dispatch và mô tả boundary đúng source.

Sửa: hierarchy giống đường ancestor production, font/rect/layout thật, extract geometry từ `textInfo.linkInfo`, chứng minh link hit và graphic raycast tại điểm press/release. Synthetic dispatch qua EventSystem/ExecuteHierarchy được ghi rõ là synthetic, không nâng thành physical touch/manual visual PASS. API calls chỉ được dùng setup hoặc supplementary manager checks, không thay đường positive input.

### N05 — Data fixture D05 không chứng minh unlocked/locked đúng lý do — P1

L474–487 không gán `mindMethodId` cho A/B/C. Source `SkillDefinitionSO` mặc định `mm_taiji`, trong khi MM fixture là `MM_Test_01`; manager từ chối khác method (`MindMethodManager.cs:499–502`). C không có unlock conditions; initializer mở skill không có điều kiện (manager L193–195), nên “C locked” không đúng. Return values của setup L494–498 bị bỏ qua. Negative check có thể từ chối vì sai method, không phải khóa skill.

Sửa: initialize metadata bằng API thật, skills cùng method/slot và conditions rõ; assert A/B unlocked, C locked trước input; kiểm return và selected/default identity. Không mở assets production, không thêm production unlock flow trong task này.

### N06 — D05 thiếu combat/event/persistence/badge oracles — P1

D05 không tạo Hero hay snapshot HP/Rage/revive đã dùng/cooldown đang chạy. Không observer `EventBus.OnSkillSelected`, không badge assertion, không persistence delta; reselect chỉ check bool/ID. Chưa có wrong slot/foreign method/unknown ID/tab/disabled checks của prompt. Báo cáo về “event đúng1, combat không reset, 0 redundant save” chưa có nguồn kiểm chứng tương ứng.

Sửa: seed trạng thái combat không-default rồi chụp sát trước/sau action, lấy tick elapsed thực làm allowance cho cooldown; event counter đăng ký owned, badge/TMP và known prefs kiểm thật. Exact count native `PlayerPrefs.Save` chỉ được công bố khi có observer hợp lệ; nếu không, ghi NOT_OBSERVED, kiểm persistence và đối chiếu SaveState dataflow, không bịa counter hoặc patch production để instrumentation.

### N07 — Context/launcher chưa bảo vệ đúng clone và profile — P1

Runner L664–693 chỉ kiểm hash CLI khi có, equality RunId/token, batch và company. Missing ID/token cả JSON/CLI có thể so bằng nhau; thiếu task/version/exact dataPath/product/allowed roots/adapter hash/input fingerprints. `AllowedRoots` khai báo L42–45 nhưng không dùng; `sourceFingerprintSha256` chỉ được chép vào receipt. Scene refusal L645–662 chỉ sáu type, bỏ qua ambient managers/Canvas/EventSystem hoặc scene roots khác.

Launcher L81–107 không kiểm allowed root/exact Assets/settings thật/adapter và source hashes/version/ambient Unity trước import. Reparse helper L38–46 dừng ngay nếu leaf output chưa tồn tại, bỏ qua ancestor; result path chưa containment/reparse check, raw log cũ chưa bị chặn. Hash tính từ context rồi truyền CLI đảm bảo agreement, không tự chứng minh context đã được Tech Lead phê duyệt.

Sửa: template false; phiên sau phải có context/hash được chỉ đạo riêng, thiếu/malformed/duplicate CLI thì refuse. Pre-import launcher và pre-fixture adapter kiểm exact owned paths/profile/version/source receipt, hiện trạng settings/Editor scripts, reparse ancestors kể cả leaf chưa tồn tại và results/log absent. Refuse ambient project/process/scene; không tự sửa settings/source/prefs để vượt gate. Unowned entry chỉ detach/refuse, không exit Editor đang mở.

### N08 — Prefs ledger và cleanup chưa đầy đủ — P1

Runner L46–49/L220/L242 chỉ quản lý7 Resource/LootTier keys. D05 SaveState L494–498 ghi `TLTD_MM_ActiveId`, method unlocked/level/slot và `TLTD_Skill_Unlocked_*` (manager L617–643), chưa baseline absent/owned delete/recheck. Khi D01 bổ sung Progression/Hero, còn phải trace các key liên quan trước lifecycle.

L142–171 hủy roots ngược allocation (entities trước BM), clear ledger ngay, dùng Destroy deferred trong PlayMode nhưng cleanup PASS ngay khi không exception; không retained survivor/frame/driver proof. Outcome L262 tính trước cleanup; có thể dispatch D05 sau D01 cleanup fail. Observer log bị gỡ L239 trước prefs cleanup; exception prefs còn có thể bỏ receipt.

Sửa: exact keys ledger trước mọi lifecycle ghi, chỉ xóa keys đã sở hữu trong profile fresh, revalidate context và HasKey=false cuối phiên; BM trước entities/runtime config; dispose outstanding nested iterators khi abort; retain refs đến sau frame và driver sau destruction. Aggregate case sau assertions+cleanup; giữ evidence observer đến cleanup cuối. Không ResetInstance/global EventBus reset/DeleteAll hay recovery rerun.

### N09 — Watchdog còn kill theo PID và thiếu finally — P1

Parent PID/StartTime capture L133–134 là điểm giữ lại. Nhưng L154–158 kill immediate children theo PID, không xác minh child CreationDate/StartTime/recursive ownership; query/kill exceptions bị suppress. Không finally phủ spawn/poll/evidence, không bounded wait xác minh exit sau stop. Lỗi đọc StartTime/poll có thể bỏ process sống.

Sửa: captured Process identity, descendants chỉ khi proven lineage/creation-time, query error fail-closed; try/finally, bounded stop/HasExited proof và receipt conflict/timeout. Không kill ambient Unity hoặc taskkill theo name/PID-only. Scope chỉ launcher nguồn mới; chưa chạy watchdog trong review.

### N10 — Evidence acceptance có đường false PASS hoặc timeout — P1

Launcher L167–184 chỉ cần result.outcome PASS; bỏ OS exit0, task/run/token/context/source match, exact cases/checks, từng cleanup, driver/suite terminals và surplus/failure rows. Adapter không emit terminals, không controlled owned exit. Launcher không dùng -quit và adapter không exit sau hoàn tất; source hiện tại chưa có dataflow kết thúc phiên, có nguy cơ watchdog timeout.

WriteReceipt L706–721 có thể overwrite/ghi ngoài root, nuốt write failure; thiếu resultPath vẫn không throw. Sửa: CreateNew UTF8 no BOM trong output đã validate, write failure chặn PASS/exit0; đúng1 case row mỗi D01/D05 +1 driver +1 suite kèm nonce/schema; cases/check IDs và cleanup thật, FAIL/NOT_REACHED/surplus không được bỏ. OS exit và scenario outcome lưu riêng; controlled exit chỉ sau owned PlayMode exit/driver destruction/receipt flush.

### N11 — Report/provenance không khớp dữ liệu đo — P1 cho truthfulness, không chứng minh source bị đổi

Bảng9 files trong report/pasted handoff có **7/9 rows không khớp source thật**; chỉ builder và MindMethodUI khớp. Ví dụ EquipmentManager report13.003bytes nhưng E và preservation receipt thật là5.981bytes; Inventory report10.230 nhưng thực3.317bytes. Bảng actual ở mục3; 786 fingerprints vẫn khớp snapshot root trước đó nên không có bằng chứng E bị rollback/đổi trong task này.

`preservation_receipt_E.json` ghi28 untracked entries (collapsed folders), trong khi handoff ghi530; cần ghi rõ mode `-uall` nếu muốn so file rows. Report L179 ghi compiler receipt1.281bytes/hash A047… nhưng actual receipt1.550bytes/hash21F182…; manifest đã đúng với actual. Báo cáo mô tả oracles chưa tồn tại, thêm “font thật/full matrix/natural frames/EXP0/one advance” không có source tương ứng. README/report và tên fixture vẫn mojibake khi decode đúng UTF8. Không suy đoán ý định; các khẳng định này phải được sửa theo dữ liệu thật.

## 3. Baseline E actual để giao bản sửa

| File dưới Assets/_Game | Bytes | SHA256 |
|---|---:|---|
| Core/BattleManager.cs | 58.725 | 5EC2200E5B7D3F48846B0518A48C67978A2F5CB8E00D48794CCCA343DCD11A38 |
| Editor/Prototype01SceneBuilder.cs | 183.357 | 19F04C3E98F8DFAA4AFC1EFF565D299A57146826C0EFAE93EAFAC7858945C8AC |
| Equipment/EquipmentManager.cs | 5.981 | 3177448604AC33E1EE6C8E5628A62DD7D68A15A38B09027816147B4C41D6A919 |
| Equipment/EquipmentUpgradeService.cs | 7.125 | 4BECD334CAB685042F2381C0A3725957E50AB5640342FBA0C556AA688969454F |
| Inventory/Inventory.cs | 3.317 | D32EE1DCE8A2A84F5B67230B0DD61658032E1C5CBCE2E4709EFAE433901B93E4 |
| Progression/LootTierProgressionManager.cs | 13.078 | 04E0027600F7977EC7804E0A28034894BADDD0175B2C9822848FB3642B07C6CC |
| UI/EquipmentDropDebugUI.cs | 30.589 | AC4B9EE0C6C16F7DA86CC72FBB083B32C7A01B1D5C80DC8C532EA8CDC2ACAC8A |
| UI/LootTierProgressionUI.cs | 23.727 | 5C02B4E2431B48859FB099E255761C604B5760C8F2A4D847449CDB9D52FB524C |
| UI/MindMethodUI.cs | 22.210 | D1702857311ED4089B481E67F0CF319B544C09898771A779D31622AC6D0E3FE6 |

Owner dirty `PROJECT_MEMORY/ACTIVE_WORK_HANDOFF.md`: 6.358bytes / SHA842B4FED974040C4F69F947C82585700502ACE0594E822DE70A66A4FAB728B48, không overwrite. Hai dirty Markdown còn lại/index/HEAD nằm trong snapshot đã giữ nguyên. Trước executor sửa phải đo lại actual; lệch thì báo cụ thể, không reset để khớp bảng.

## 4. Những phần giữ lại và bước tiếp theo

Giữ standalone compile/API đối chiếu, manifest12/12, template unauthorized, entry Run tường minh, không MenuItem/auto-start, primary Tách qua UI, immediate duplicate cùng stack, parent PID/StartTime và intent fresh profile. Sửa đúng các findings; không dùng rewrite để làm mất các phần đúng.

**Bước tiếp theo: `F-TLTD-DELTA-NATURAL-INPUT-PREP-01-R1`, SOURCE PREPARATION ONLY**, theo file riêng [PROMPT_ANTIGRAVITY_TLTD_DELTA_NATURAL_INPUT_PREP_R1_20261013.md](PROMPT_ANTIGRAVITY_TLTD_DELTA_NATURAL_INPUT_PREP_R1_20261013.md). Bàn giao packet mới, giữ nguyên packet cũ, source/AST/compiler/evidence truthfulness để Codex review. Không cấp phiên Unity trong báo cáo này; khi source đủ sẽ có chỉ đạo phiên riêng với paths/context/nonce/fingerprints cụ thể.

P08/P09-A/P09-B tiếp tục ACCEPTED/LOCKED. Ba case delta EditMode33/33/cleanup trước đó giữ nguyên boundary đã xác minh; D01/D05 mới còn NOT RUN. Old lab/R4 và Phase B không bị đóng hoặc triển khai bởi task này. Boss-exit LOSE/gate100% đã duyệt nhưng implementation ngoài scope. Roadmap vẫn là proposal; không auto-unlock skill assets, không feature rollout hoặc yêu cầu Owner replay/ZIP phần đã xong.

Receipts review local: `static_verification_receipt.json`, `before_snapshot.json`, `after_snapshot.json`, `preservation_receipt.json` và `standalone_compile/` dưới `work/complete_game_next_20261013/natural_input_review_20261013/`. Publication continuity riêng sẽ ghi commit/main thật sau push và verify; report này không tuyên bố source E đã được publish.
