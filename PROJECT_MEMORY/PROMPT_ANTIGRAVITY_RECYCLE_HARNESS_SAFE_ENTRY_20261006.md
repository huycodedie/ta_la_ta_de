# PROMPT ANTIGRAVITY — RECYCLE HARNESS SAFE ENTRY

Task: **F-RECYCLE-HARNESS-SAFE-ENTRY-01**, follow-up hẹp của F-RECYCLE-GOLD-ONLY-01. Ngày 06/10/2026, UTC+07.

## Mục tiêu và authority

Codex đã review source/runtime Gold-only và raw batch fixtures. Runtime reward được ACCEPTED trong phạm vi đó; closeout tổng còn chờ safe-entry/teardown của runner mới. Đọc riêng [TLTD_RECYCLE_GOLD_ONLY_REVIEW_20261006.md](TLTD_RECYCLE_GOLD_ONLY_REVIEW_20261006.md). Đây là prompt thực thi tooling, không yêu cầu Owner chọn sản phẩm hoặc chơi lại.

Đọc canonical main hiện hành qua snapshot tách biệt: AI_RULES, CONTINUITY_AND_SYNC_AUTHORITY, ACTIVE_WORK_HANDOFF, CURRENT_DESIGN_AUTHORITY và review này. Local PROJECT_MEMORY/ACTIVE_WORK_HANDOFF đang dirty: không ghi đè để sync. Resolve và ghi exact authority SHA trước sửa.

Game source/main Codex vừa quan sát `01c5b23fa47dde9a61306f8d894a497b6d882673` (parent6d9e192). Commit source đã tồn tại; không tạo lại, revert hoặc suy đoán actor. Tại E:\code\TLTD có ba tracked docs modified +530 untracked entries khi review (533 porcelain rows). Chụp before status/hash; không coi working tree clean.

Runner local baseline: `Assets/_Game/Editor/Prototype01PlayTestRunner_RecycleGoldOnly.cs`, 20141 bytes, SHA256 `0C2087496C94E8CAACAB2AB2286AF7BE6744F978020EB66A3F5AED83E9AB9731`. Meta59 bytes/SHA256 `98703CD9C3E4CC62414519287F9A4F74547F34C6A8C9E9AEB0474F91BBE45173`, GUID b1e3240415cf9f6418044f7443e55379. Nếu baseline thay đổi, đối chiếu delta trước sửa; bảo toàn work khác.

## Exact allowed paths

1. Chỉ sửa runner mới `Assets/_Game/Editor/Prototype01PlayTestRunner_RecycleGoldOnly.cs`.
2. Artifacts mới dưới `scratch/recycle_harness_safe_entry_<runid>`.
3. Meta chỉ đọc/kiểm integrity; giữ GUID, không đổi để khớp bảng215bytes sai.

Không sửa ResourceManager, base runner bốn assertions, BattleManager, LootDecisionUI, ModalCoordinator, Save Guard/VerificationCore/preflight, scenes/assets, production skills hoặc tài liệu dirty. Không thêm feature/gameplay scope.

## Sửa entry trước mọi fixture/persistence mutation

- Xóa MenuItem của suite này; đây là batch tooling qua existing Save Guard wrapper, không công cụ click trong Editor của Owner.
- `RunAllRecycleGoldOnlyTests` chuyển thành helper private sau khi rg callers xác nhận chỉ CLI gọi. Các case/helpers không tạo public đường tắt mới.
- Public `RunRecycleGoldOnlyCLI` phải từ chối khi không `Application.isBatchMode` **trước** suite call, GameObject creation, singleton reset, modal action hoặc PlayerPrefs access/write. Log rõ rejection rồi return; không Exit Editor tương tác.
- Chỉ batch session tách biệt qua wrapper Save Guard đã review được phép chạy case ghi persistence; không gọi bare executeMethod để né backup. Không thêm guard/recovery framework khác. Không coi batch flag tự nó là bằng chứng backup đã chạy.
- Không reuse scene singletons/modal state của Editor đang mở. Kiểm tra fixture ownership/fresh batch context trước mutate; nếu có state không thuộc task, fail trước sửa state thay vì reset/dismiss singleton của bên khác.
- Trong child batch session đã backup, có thể tạo một transient empty Editor scene để cô lập fixture; không SaveScene/ghi scene asset. Từ chối PlayMode/transition trước setup. Không thực hiện việc này trong Editor tương tác.

## Teardown phải an toàn cả exception

- R2/R3: khai báo bmGO/owned references đủ để finally luôn dọn, kể cả reflection/assertion throw. Không để DestroyImmediate chỉ ở cuối try.
- Đưa SetupTestEnvironment vào vùng try/finally với root reference được quản lý ngay lúc tạo; partial setup failure cũng phải dọn object task đã tạo. Không chỉ bảo vệ phần sau khi setup trả về.
- Track và dọn đúng normal monsters/runtime fixture objects do BattleManager của test tạo. Các monster là root objects; hủy bmGO chưa đủ. Dùng owned references/activeMonsters của fixture, không “destroy all Monster/GameObject” trong scene. Dọn trước khi mất reference tới bm.
- Modal chỉ register/request/dismiss/unregister của dummy view task sở hữu; không ResetInstance hoặc dismiss modal của state có trước.
- Listener R5 unsubscribe trong finally khi đã subscribe; exception không để handler sống.
- R5 immediate readback không phải full-save recovery. Nếu giữ local key restoration, capture HasKey+value trước mutation và preserve absence; không tuyên bố việc SetInt hai key sau case trước khôi phục suite baseline. Full baseline vẫn do outer Save Guard restore/compare. Không tự export/delete Registry keys toàn cục.
- Giữ các reward/duplicate/item-identity predicates hiện có. Thêm kiểm chứng owned fixture cleanup còn0 sau R2/R3 và listener teardown; không sửa production để đạt test hoặc nâng EditMode thành natural-frame evidence.

## Kiểm chứng bounded

Trước bất kỳ Unity launch/import/compile/test: unresolved-journal preflight đã review → backup thành công → process/project writer isolation → launch. Unresolved/corrupt journal hoặc Editor writer đang dùng project mà không đảm bảo an toàn: block dependent run, báo exact blocker; không kill Unity ngoài task, recover tự ý hoặc lấy contaminated baseline.

1. Source review/call graph: không còn MenuItem/public suite bypass; interactive rejection đứng trước mọi mutation. Không cần mở Editor người dùng để “thử” nhánh nguy hiểm; code inspection chứng minh nhánh return.
2. **Một** guarded batch run suite R1–R5 sau sửa harness, budget300s, ghi Unity actual version/full method/source hash/OS exit/timeout và raw logs. Đây là run mới do harness thay đổi, không rerun nghiệm thu P08/P09. Không vào PlayMode hoặc master suite chỉ để nâng nhãn evidence.
3. Log/assert owned objects/modal/listener task còn sạch sau teardown. Report case IDs/counts thật. Outer wrapper restore+compare phải thành công; nếu diff/failure, giữ journal/log và báo, không loop/recovery tự động.
4. Không retry tự động trong task này. Nếu compile/test fail, giữ logs và source delta, chuyển Codex review lỗi cụ thể trước run tiếp.

Có thể gọi existing dispatcher/core từ script artifact mới với exact method hiện hành; không sửa core. Dùng backup directory/journal mới của run này, kiểm unresolved journals cả phạm vi preflight hiện có trước launch. Không viết đè raw run1/run2/run3 hoặc final journal của task Gold-only cũ.

`Test-P09BJournalPreflight` chỉ kiểm journal tại BackupDir được truyền, không scan mọi nơi. Phải kiểm old recycle final journal và new run path; NO_JOURNAL của path mới không chứng minh journal cũ an toàn. Nếu phát hiện một real-save journal khác đang active cho cùng Registry scope, thêm exact directory đó vào danh sách. Không scan fixture journals hoặc mở lại historical P09 recovery để điền raw evidence.

Tạo directory artifact mới `scratch/recycle_harness_safe_entry_<runid>`, lưu `run_suite_guarded.ps1` bên trong theo recipe dưới đây. Script dùng PSScriptRoot và từ chối mọi log/backup đã có, nên không đụng wrapper cũ. Không sửa tooling core. Nếu Unity path/version thực thay đổi, báo và đối chiếu trước run; không silently đổi engine. Standard Restore/Compare cho chính run này trong finally của core vẫn bắt buộc; cấm retry/recovery bổ sung sau failure.

```powershell
$ErrorActionPreference = 'Stop'
$taskProjectRoot = 'E:\code\TLTD'
$taskUnityPath = 'E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe'
$taskCorePath = Join-Path $taskProjectRoot 'Tools\Verification\P09\P09_VerificationCore.ps1'
$taskSaveGuardPath = Join-Path $taskProjectRoot 'Tools\Verification\P09\tltd_save_guard.ps1'
$taskPreflightPath = Join-Path $taskProjectRoot 'Tools\Verification\P09B\P09B_PreflightGuard.ps1'
$taskPriorBackupDir = Join-Path $taskProjectRoot 'scratch\recycle_gold_only_20261005_234850\.save_backup'
$taskRunRoot = $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($taskRunRoot)) { throw 'Run from the saved script in a fresh artifact directory' }
$taskBackupDir = Join-Path $taskRunRoot '.save_backup'
$taskUnityLog = Join-Path $taskRunRoot 'recycle_harness_safe_entry_unity.log'
$taskWrapperLog = Join-Path $taskRunRoot 'recycle_harness_safe_entry_wrapper.log'
$taskPreflightLog = Join-Path $taskRunRoot 'preflight.log'
foreach ($taskRequiredPath in @($taskUnityPath, $taskCorePath, $taskSaveGuardPath, $taskPreflightPath)) {
    if (-not (Test-Path -LiteralPath $taskRequiredPath -PathType Leaf)) { throw "Required path missing: $taskRequiredPath" }
}
foreach ($taskOutputPath in @($taskBackupDir, $taskUnityLog, $taskWrapperLog, $taskPreflightLog)) {
    if (Test-Path -LiteralPath $taskOutputPath) { throw "Refusing to reuse output path: $taskOutputPath" }
}
$taskPriorJournal = Join-Path $taskPriorBackupDir 'journal.json'
if (-not (Test-Path -LiteralPath $taskPriorJournal -PathType Leaf)) {
    throw "Prior recycle final journal unavailable; report blocker: $taskPriorJournal"
}
. $taskPreflightPath
$taskPreflightLogger = {
    param($taskMessage, $taskColor)
    Write-Host $taskMessage
    Add-Content -LiteralPath $taskPreflightLog -Encoding UTF8 -Value $taskMessage
}
foreach ($taskJournalDir in @($taskPriorBackupDir, $taskBackupDir)) {
    $taskPreflight = Test-P09BJournalPreflight -BackupDir $taskJournalDir -Logger $taskPreflightLogger
    if (-not $taskPreflight.Allowed) { throw "Preflight blocked before launch/backup: $($taskPreflight.Reason)" }
}
. $taskCorePath
$taskUnityArgs = @(
    '-batchmode', '-quit',
    '-projectPath', $taskProjectRoot,
    '-executeMethod', 'WuxiaGame.Editor.Prototype01PlayTestRunner_RecycleGoldOnly.RunRecycleGoldOnlyCLI',
    '-logFile', $taskUnityLog
)
$taskResult = Invoke-P09VerificationSession `
    -SuiteName 'F-RECYCLE-HARNESS-SAFE-ENTRY-01: R1-R5' `
    -ProjectRoot $taskProjectRoot `
    -ExecutablePath $taskUnityPath `
    -ArgumentList $taskUnityArgs `
    -LogFile $taskUnityLog `
    -WrapperLog $taskWrapperLog `
    -BackupDir $taskBackupDir `
    -TimeoutSeconds 300 `
    -SaveGuardScript $taskSaveGuardPath `
    -AllowRunningUnity:$false `
    -PassPattern 'ALL_PASS=True' `
    -SummaryPatterns @('F-RECYCLE', '\[R', 'ALL_PASS', 'FAIL', 'EXCEPTION', 'CLEANUP')
Write-Host "Session exit=$($taskResult.ExitCode)"
exit ([int]$taskResult.ExitCode)
```

Invoke script artifact bằng PowerShell -NoProfile -File với literal path thực vừa tạo; ghi full invocation vào provenance. Core từ chối Unity đang chạy trước backup và quản lý đúng process tree task tạo. Không mở GUI để thử guard hoặc yêu cầu Owner chạy bare runner.

## Bàn giao và điểm dừng

- `HARNESS_FIX_REPORT.md`: allowed delta, exact before/after hashes/status, ownership/exception cleanup, mode/limitations.
- Patch UTF-8 theo before bytes thật, chỉ runner thay đổi; giữ meta/hash như quan sát. Codex đã bổ sung patch bốn file của commit cũ, không cần đóng ZIP hoặc làm lại package P08/P09.
- Raw Unity/wrapper logs và `PROVENANCE_ADDENDUM.md` riêng: ghi command Unity6000.6.0f1 / namespace CLI thực tế (hoặc version thực nếu đã thay đổi), run IDs/times/PID/exit/restore/compare, count thật; hash manifest tách khỏi tài liệu tự hash.
- Giữ báo cáo/raw evidence cũ. Trích PASS summary dưới nhãn derived summary nếu không verbatim; không sửa lịch sử thất bại. Metadata corrections của task trước đã có trong Codex review, không yêu cầu chạy lại vì lỗi hồ sơ.
- ResourceManager/base runner và ba dirty docs phải có after hash unchanged so với before task này. Ghi untracked inventory delta chỉ thuộc task; không xóa work khác.

Không git pull vào dirty E:, stash/reset/clean/checkout overwrite/stage/commit/push/rebase. Đưa delta cho Codex review rồi dừng. Không tự ghi LOCKED/CLOSED; không triển khai Stage/Boss, Bun/offline, Material source, skill rollout hoặc UI polish. Không yêu cầu Owner chơi lại S01–S05/M1–M7/P08 hoặc tạo ZIP nghiệm thu.
