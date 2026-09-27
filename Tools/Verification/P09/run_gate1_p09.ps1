$ErrorActionPreference = "Continue"

$projectRoot = "E:\code\TLTD"
$unityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe"
$logFile = "$projectRoot\gate1_p09_tests.log"
$wrapperLog = "$projectRoot\gate1_p09_wrapper.log"
$saveGuardScript = "$projectRoot\Tools\Verification\P09\tltd_save_guard.ps1"
$backupDir = "$projectRoot\scratch\.save_backup_gate1_p09"
$timeoutSeconds = 90

Remove-Item -Force -ErrorAction SilentlyContinue $logFile
Remove-Item -Force -ErrorAction SilentlyContinue $wrapperLog

function Log([string]$msg) {
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    $line = "[$timestamp] $msg"
    Write-Host $line
    Add-Content -Path $wrapperLog -Value $line -Encoding UTF8
}

function Stop-ProcessTree([int]$parentPid) {
    try {
        $children = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object { $_.ParentProcessId -eq $parentPid }
        foreach ($child in $children) {
            Stop-ProcessTree $child.ProcessId
        }
        Stop-Process -Id $parentPid -Force -ErrorAction SilentlyContinue
    } catch {}
}

Log "============================================================"
Log " Running Gate 1: P09-A Projectile Foundation Tests (T01 - T21)"
Log " Unity Path:       $unityPath"
Log " Log File:         $logFile"
Log " Wrapper Log:      $wrapperLog"
Log " Timeout Budget:   $timeoutSeconds seconds"
Log " Save Guard:       $saveGuardScript"
Log " Backup Dir:       $backupDir"
Log "============================================================"

# 1. Assert no other Unity instance running (never kill developer's Unity)
$unityProcs = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($null -ne $unityProcs -and $unityProcs.Count -gt 0) {
    $pids = ($unityProcs | ForEach-Object { "$($_.Id)" }) -join ", "
    Log "[FATAL] Unity process currently running (PID: $pids). Wrapper will not kill external Unity. Terminate first."
    exit 1
}

# 2. Save Guard: Check journal and Backup
Log "[SAVE GUARD] Checking interrupted journal..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action CheckInterruptedJournal -BackupDir $backupDir
$journalExit = $LASTEXITCODE
if ($journalExit -ne 0) {
    Log "[SAVE GUARD FATAL] CheckInterruptedJournal failed with exit code $journalExit. Refusing to launch Unity."
    exit 1
}

Log "[SAVE GUARD] Taking durable pre-run backup to $backupDir..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Backup -BackupDir $backupDir
$backupExit = $LASTEXITCODE
if ($backupExit -ne 0) {
    Log "[SAVE GUARD FATAL] Backup failed with exit code $backupExit. Refusing to launch Unity."
    exit 1
}

$timedOut = $false
$unityOsExitCode = "UNKNOWN"
$runnerPassed = $false
$saveRestored = $false
$saveDiffZero = $false
$wrapperExitCode = 1

try {
    # 3. Launch Unity Gate 1 with finite watchdog timeout
    Log "[GATE 1] Launching Unity batchmode suite with timeout budget of $timeoutSeconds seconds..."
    $proc = Start-Process -FilePath $unityPath -ArgumentList "-batchmode", "-quit", "-projectPath", "`"$projectRoot`"", "-executeMethod", "WuxiaGame.Editor.Prototype01PlayTestRunner_P09.RunGate1_P09_CLI", "-logFile", "`"$logFile`"" -PassThru

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    while (-not $proc.HasExited) {
        Start-Sleep -Milliseconds 500
        if ($sw.Elapsed.TotalSeconds -ge $timeoutSeconds) {
            $timedOut = $true
            Log "[WATCHDOG] Unity process exceeded timeout budget of $timeoutSeconds seconds. Terminating wrapper-owned process tree PID $($proc.Id)..."
            Stop-ProcessTree $proc.Id
            Start-Sleep -Seconds 2
            break
        }
    }
    $sw.Stop()

    if (-not $timedOut) {
        try {
            $proc.WaitForExit()
            $unityOsExitCode = $proc.ExitCode
        } catch {
            $unityOsExitCode = "UNKNOWN"
        }
    } else {
        $unityOsExitCode = "UNKNOWN (TIMED_OUT)"
    }

    Log "Unity process finished: OS Exit Code = $unityOsExitCode, Elapsed = $([math]::Round($sw.Elapsed.TotalSeconds, 2))s, TimedOut = $timedOut"
}
finally {
    # 4. Save Guard: Restore and Compare inside finally
    Log "[SAVE GUARD] Restoring persistence to exact pre-run state..."
    & powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Restore -BackupDir $backupDir
    $restoreExit = $LASTEXITCODE
    if ($restoreExit -eq 0) {
        $saveRestored = $true
        Log "[SAVE GUARD] Restore succeeded."
    } else {
        $saveRestored = $false
        Log "[SAVE GUARD ERROR] Restore failed with exit code $restoreExit!"
    }

    Log "[SAVE GUARD] Comparing state to verify zero diff..."
    & powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Compare -BackupDir $backupDir
    $compareExit = $LASTEXITCODE
    if ($compareExit -eq 0) {
        $saveDiffZero = $true
        Log "[SAVE GUARD] Comparison: Diff = 0 Verified (Exact match)."
    } else {
        $saveDiffZero = $false
        Log "[SAVE GUARD ERROR] Comparison FAILED: Diff detected or comparison failed (Exit code $compareExit)!"
    }
}

# 5. Parse test log results
if (Test-Path $logFile) {
    Log "=== LOG SUMMARY ==="
    Get-Content $logFile | Select-String "P09 AUTOMATED TESTS RESULT", "\[T", "FAIL", "EXCEPTION" | ForEach-Object { Log $_.Line }
    $passLine = Get-Content $logFile | Select-String "ALL_PASS=True"
    if ($null -ne $passLine) {
        $runnerPassed = $true
    }
} else {
    Log "WARNING: Log file $logFile was not found!"
}

# 6. Final Status Determination
Log "============================================================"
Log "   GATE 1 EXECUTION SUMMARY"
Log "============================================================"
Log " Unity Process OS Exit: $unityOsExitCode"
Log " Timeout Status:        $(if ($timedOut) { 'TIMED OUT' } else { 'COMPLETED WITHIN BUDGET' })"
Log " Runner Suite Result:   $(if ($runnerPassed) { 'ALL TESTS PASSED' } else { 'FAILED OR INCOMPLETE' })"
Log " Save State Restore:    $(if ($saveRestored) { 'SUCCESS' } else { 'FAILED' })"
Log " Save Diff Match:       $(if ($saveDiffZero) { 'DIFF = 0 VERIFIED' } else { 'DIFF DETECTED / FAILED' })"

if ($runnerPassed -and ($unityOsExitCode -eq 0) -and $saveDiffZero) {
    $wrapperExitCode = 0
    Log " OVERALL GATE 1 RESULT: ALL TESTS PASSED + ZERO DIFF (PASS)"
} elseif ($runnerPassed -and $timedOut) {
    $wrapperExitCode = 2
    Log " OVERALL GATE 1 RESULT: RUNNER PASSED BUT EXIT TIMED OUT (Separately reported)"
} else {
    $wrapperExitCode = 1
    Log " OVERALL GATE 1 RESULT: FAILED (ExitCode=$unityOsExitCode, SaveDiffZero=$saveDiffZero, RunnerPassed=$runnerPassed)"
}
Log "============================================================"

exit $wrapperExitCode
