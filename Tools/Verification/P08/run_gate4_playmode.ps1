# P08 Real Play Mode Scenario Verification Runner (Gate 4 Corrective)
# Strictly controls lifecycle, watchdog budgets, persistence guard, and OS process exit codes.

$ErrorActionPreference = "Continue"

$projectRoot = "E:\code\TLTD"
$runId = Get-Date -Format "yyyyMMdd_HHmmss"
$reviewPkgDir = "$projectRoot\review_package_p08_corrective_patch"
$runOutputDir = "$reviewPkgDir\runs\run_$runId"
$archiveDir = "$projectRoot\.logs_archive"
$logFile = "$projectRoot\gate4_playmode_scenario.log"
$wrapperLogFile = "$projectRoot\gate4_wrapper_run.log"
$unityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe"
$saveGuardScript = "$projectRoot\Tools\Verification\P08\tltd_save_guard.ps1"

if (-not (Test-Path $runOutputDir)) { New-Item -ItemType Directory -Path $runOutputDir -Force | Out-Null }
if (-not (Test-Path $archiveDir)) { New-Item -ItemType Directory -Path $archiveDir -Force | Out-Null }

function Log-Output([string]$msg) {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "[$timestamp] $msg"
    Write-Host $line
    try {
        Add-Content -Path $wrapperLogFile -Value $line -Encoding UTF8 -ErrorAction SilentlyContinue
        Add-Content -Path "$runOutputDir\wrapper_run.log" -Value $line -Encoding UTF8 -ErrorAction SilentlyContinue
    } catch {}
}

function Stop-ProcessTree([int]$parentPid) {
    Log-Output "[INFO] Terminating process tree for PID: $parentPid"
    try {
        $children = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object { $_.ParentProcessId -eq $parentPid }
        foreach ($child in $children) {
            Stop-ProcessTree $child.ProcessId
        }
        Stop-Process -Id $parentPid -Force -ErrorAction SilentlyContinue
    } catch {
        Log-Output "[WARN] Exception while terminating process ${parentPid}: $($_.Exception.Message)"
    }
}

Remove-Item -Force -ErrorAction SilentlyContinue $wrapperLogFile
Log-Output "============================================================"
Log-Output " Running Gate 4: P08 Real Play Mode Runner (Corrective Patch)"
Log-Output " Run ID: $runId"
Log-Output " Output Directory: $runOutputDir"
Log-Output " Save Guard Script: $saveGuardScript"
Log-Output "============================================================"

# 1. Concurrency Check: ensure no other Unity instances are running before taking backup
$existingUnity = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($null -ne $existingUnity -and $existingUnity.Count -gt 0) {
    $pids = ($existingUnity | ForEach-Object { "$($_.Id)" }) -join ", "
    Log-Output "[ERROR] Existing Unity Editor process(es) currently running (PID: $pids). Aborting to avoid database lock and protect save."
    exit 1
}

# 2. Save Guard: Check for interrupted journal and create durable pre-run backup
Log-Output "[SAVE GUARD] Checking for interrupted journal..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action CheckInterruptedJournal
if ($LASTEXITCODE -ne 0) {
    Log-Output "[ERROR] Failed during CheckInterruptedJournal! Aborting."
    exit 1
}

Log-Output "[SAVE GUARD] Creating pre-run save backup..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Backup
if ($LASTEXITCODE -ne 0) {
    Log-Output "[ERROR] Save backup failed! Aborting test execution."
    exit 1
}
Log-Output "[SAVE GUARD] Pre-run save backup successfully verified."

# 3. Archive previous log evidence
if (Test-Path $logFile) {
    Copy-Item -Path $logFile -Destination "$archiveDir\gate4_playmode_$runId.log" -Force
    Remove-Item -Force -ErrorAction SilentlyContinue $logFile
    Log-Output "[INFO] Archived prior gate4 log to .logs_archive\gate4_playmode_$runId.log"
}

# 4. Clean up screenshots from previous runs
$screenshots = @(
    "$projectRoot\screenshots\01_TWO_MONSTERS_ALIVE.png",
    "$projectRoot\screenshots\02_AOE_HITS_BOTH.png",
    "$projectRoot\screenshots\03_RETARGET_AFTER_FIRST_DEATH.png",
    "$projectRoot\screenshots\04_LOOT_FIRST.png",
    "$projectRoot\screenshots\05_LOOT_SECOND.png"
)
foreach ($s in $screenshots) {
    Remove-Item -Force -ErrorAction SilentlyContinue $s
}

$patchPkgScreenshotsDir = "$reviewPkgDir\screenshots"
if (-not (Test-Path $patchPkgScreenshotsDir)) {
    New-Item -ItemType Directory -Path $patchPkgScreenshotsDir -Force | Out-Null
} else {
    foreach ($s in (Get-ChildItem -Path $patchPkgScreenshotsDir -Filter "*.png")) {
        Remove-Item -Force -ErrorAction SilentlyContinue $s.FullName
    }
}

# 5. Launch Unity Batch Process using System.Diagnostics.ProcessStartInfo
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $unityPath
$psi.Arguments = "-batchmode -projectPath `"$projectRoot`" -executeMethod WuxiaGame.Editor.Prototype01PlayTestRunner_P08.RunP08PlayModeScenarioFromMenu -logFile `"$logFile`""
$psi.UseShellExecute = $false
$psi.CreateNoWindow = $true

Log-Output "[INFO] Launching Unity: $unityPath $($psi.Arguments)"
$launchStart = Get-Date
try {
    $proc = [System.Diagnostics.Process]::Start($psi)
} catch {
    Log-Output "[ERROR] Failed to start Unity process: $($_.Exception.Message)"
    & powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Restore
    exit 1
}

if ($null -eq $proc) {
    Log-Output "[ERROR] Process::Start returned null process handle."
    & powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Restore
    exit 1
}
$unityPid = $proc.Id
Log-Output "[INFO] Unity process launched successfully with PID: $unityPid"

# 6. Startup & Import Budget: up to 240 seconds to reach checkpoint
$startupTimeoutSeconds = 240
$checkpointFound = $false
$checkpointPattern = "\[PLAY MODE P08\] CHECKPOINT: SCENARIO_STARTED"
$startupStart = Get-Date

Log-Output "[INFO] Waiting for scenario startup checkpoint (budget: ${startupTimeoutSeconds}s)..."
while (-not $checkpointFound) {
    if ($proc.HasExited) {
        Log-Output "[ERROR] Unity process exited prematurely before reaching startup checkpoint!"
        break
    }
    if (((Get-Date) - $startupStart).TotalSeconds -gt $startupTimeoutSeconds) {
        Log-Output "[ERROR] Unity startup budget exceeded (${startupTimeoutSeconds}s) without reaching checkpoint!"
        break
    }
    if (Test-Path $logFile) {
        $content = Get-Content $logFile -Raw -ErrorAction SilentlyContinue
        if ($null -ne $content -and $content -match $checkpointPattern) {
            $checkpointFound = $true
            $startupElapsed = ((Get-Date) - $startupStart).TotalSeconds.ToString("F1")
            Log-Output "[OK] Scenario started checkpoint detected after ${startupElapsed}s."
            break
        }
    }
    Start-Sleep -Milliseconds 1000
}

if (-not $checkpointFound) {
    Log-Output "[ERROR] Terminating Unity process tree PID $unityPid due to startup failure..."
    Stop-ProcessTree $unityPid
    & powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Restore
    exit 1
}

# 7. Scenario Budget: 180 seconds realtime from checkpoint
$scenarioTimeoutSeconds = 180
$scenarioStart = Get-Date
$scenarioTimedOut = $false

Log-Output "[INFO] Monitoring scenario execution (budget: ${scenarioTimeoutSeconds}s realtime)..."
while (-not $proc.HasExited) {
    $scenarioElapsed = ((Get-Date) - $scenarioStart).TotalSeconds
    if ($scenarioElapsed -gt $scenarioTimeoutSeconds) {
        $scenarioTimedOut = $true
        Log-Output "[ERROR] Scenario realtime budget exceeded (${scenarioTimeoutSeconds}s)! Terminating process tree PID $unityPid..."
        Stop-ProcessTree $unityPid
        break
    }
    Start-Sleep -Milliseconds 1000
}

# 8. Shutdown Deadline: wait up to 30 seconds for clean process termination
$shutdownWaitMs = 30000
$cleanlyShutdown = $false
try {
    $cleanlyShutdown = $proc.WaitForExit($shutdownWaitMs)
} catch {}

if (-not $cleanlyShutdown -and -not $proc.HasExited) {
    Log-Output "[ERROR] Unity process PID $unityPid did not exit within ${shutdownWaitMs}ms shutdown deadline! Terminating..."
    Stop-ProcessTree $unityPid
}

$totalElapsed = ((Get-Date) - $launchStart).TotalSeconds.ToString("F1")
Log-Output "[INFO] Unity process finished. Total runtime: ${totalElapsed}s."

# 9. Read Actual Process Exit Code (ZERO log-based fallbacks)
$unityExitCode = $null
if ($proc.HasExited) {
    try {
        $unityExitCode = $proc.ExitCode
    } catch {
        Log-Output "[WARN] Could not retrieve ExitCode from process handle: $($_.Exception.Message)"
    }
}

if ($null -eq $unityExitCode) {
    Log-Output "[ERROR] Actual Unity ExitCode is unavailable (null). Marked as FAILURE. No log fallback allowed."
    & powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Restore
    exit 1
}
Log-Output "[INFO] Actual Unity Process Exit Code: $unityExitCode"

# 10. External Save Restoration & Baseline Comparison
Log-Output "[SAVE GUARD] Restoring persistent save state and verifying baseline match..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Restore
$restoreExitCode = $LASTEXITCODE

& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Compare
$compareExitCode = $LASTEXITCODE

$userSaveUnchanged = ($restoreExitCode -eq 0 -and $compareExitCode -eq 0)
Log-Output "[SAVE GUARD] UserSaveUnchanged=$userSaveUnchanged"

# 11. Verify Current-Run Log Output
$summaryFound = $false
$runtimeCleanupFound = $false
$persistentDataFound = $false
$noErrorsFound = $false
$noTimeoutFound = $false

if (Test-Path $logFile) {
    Copy-Item -Path $logFile -Destination "$runOutputDir\gate4_playmode_scenario.log" -Force

    $logContent = Get-Content $logFile -Raw
    $summaryFound = ($logContent -match "\[REAL PLAY MODE P08 SCENARIO\]:\s*ALL_PASS=True")
    $noErrorsFound = ($logContent -match "UnexpectedErrors=0")
    $noTimeoutFound = ($logContent -match "TimedOut=False") -and (-not $scenarioTimedOut)
    $runtimeCleanupFound = ($logContent -match "RuntimeCleanupSucceeded=True")
    $persistentDataFound = ($logContent -match "PersistentDataRestored=True")
    
    Log-Output "Log Analysis:"
    Log-Output "  - Scenario Summary Pass:      $summaryFound"
    Log-Output "  - Zero Unexpected Errors:      $noErrorsFound"
    Log-Output "  - No Timeout:                  $noTimeoutFound"
    Log-Output "  - RuntimeCleanupSucceeded:     $runtimeCleanupFound"
    Log-Output "  - PersistentDataRestored:      $persistentDataFound"
} else {
    Log-Output "[ERROR] Log file missing: $logFile"
    exit 1
}

# 12. Verify Screenshots
$allScreenshotsOk = $true
$runScreenshotsDir = "$runOutputDir\screenshots"
if (-not (Test-Path $runScreenshotsDir)) { New-Item -ItemType Directory -Path $runScreenshotsDir -Force | Out-Null }

Log-Output "Screenshot Verification:"
foreach ($s in $screenshots) {
    $leaf = Split-Path $s -Leaf
    if (Test-Path $s) {
        $len = (Get-Item $s).Length
        if ($len -gt 0) {
            Log-Output "  [OK] $leaf ($len bytes)"
            Copy-Item -Path $s -Destination "$runScreenshotsDir\$leaf" -Force
            Copy-Item -Path $s -Destination "$patchPkgScreenshotsDir\$leaf" -Force
        } else {
            Log-Output "  [FAIL] $leaf is 0 bytes"
            $allScreenshotsOk = $false
        }
    } else {
        Log-Output "  [FAIL] $leaf is MISSING"
        $allScreenshotsOk = $false
    }
}

# 13. Final Acceptance Evaluation
$allPassed = ($unityExitCode -eq 0) -and $checkpointFound -and $summaryFound -and $noErrorsFound -and $noTimeoutFound -and $runtimeCleanupFound -and $persistentDataFound -and $userSaveUnchanged -and $allScreenshotsOk

Log-Output "============================================================"
Log-Output "GATE 4 EVALUATION METRICS:"
Log-Output "  UnityExitCode=0:              $($unityExitCode -eq 0) ($unityExitCode)"
Log-Output "  CheckpointReached:            $checkpointFound"
Log-Output "  ScenarioAllPass:              $summaryFound"
Log-Output "  ZeroUnexpectedErrors:         $noErrorsFound"
Log-Output "  NoTimeout:                    $noTimeoutFound"
Log-Output "  RuntimeCleanupSucceeded:      $runtimeCleanupFound"
Log-Output "  PersistentDataRestored:       $persistentDataFound"
Log-Output "  UserSaveUnchanged:            $userSaveUnchanged"
Log-Output "  AllScreenshotsOk:             $allScreenshotsOk"
Log-Output "============================================================"

if ($allPassed) {
    Log-Output "   GATE 4 PLAY MODE: ALL PASS (VERIFIED, ISOLATED & ZERO FALLBACKS)"
    Log-Output "   RuntimeCleanupSucceeded=True"
    Log-Output "   PersistentDataRestored=True"
    Log-Output "   UserSaveUnchanged=True"
    Log-Output "   UnityExitCode=0, WrapperExitCode=0"
    Log-Output "============================================================"
    exit 0
} else {
    Log-Output "   GATE 4 PLAY MODE: FAILED"
    Log-Output "   UnityExitCode=$unityExitCode, Summary=$summaryFound, Errors=$noErrorsFound, NoTimeout=$noTimeoutFound, RuntimeCleanup=$runtimeCleanupFound, PersistentData=$persistentDataFound, UserSaveUnchanged=$userSaveUnchanged, Screenshots=$allScreenshotsOk"
    Log-Output "============================================================"
    exit 1
}
