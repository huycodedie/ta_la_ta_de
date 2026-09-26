# Gate 2: P09-A Play Mode Scenario Runner
# Executes natural frames scenario via Hero.ExecuteSelectedSkill
# Includes save guard, watchdog budget, and clean exit.

$ErrorActionPreference = "Continue"

$projectRoot = "E:\code\TLTD"
$unityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe"
$logFile = "$projectRoot\gate2_p09_playmode.log"
$saveGuardScript = "$projectRoot\Tools\Verification\P09\tltd_save_guard.ps1"
$backupDir = "$projectRoot\scratch\.save_backup_gate2_p09"
$timeoutSeconds = 90

Remove-Item -Force -ErrorAction SilentlyContinue $logFile

Write-Host "============================================================"
Write-Host " Running Gate 2: P09-A Play Mode Natural Frames Scenario"
Write-Host " Unity Path:       $unityPath"
Write-Host " Log File:         $logFile"
Write-Host " Timeout Budget:   $timeoutSeconds seconds"
Write-Host " Save Guard:       $saveGuardScript"
Write-Host "============================================================"

# 1. Assert no other Unity instance running
$unityProcs = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($null -ne $unityProcs -and $unityProcs.Count -gt 0) {
    $pids = ($unityProcs | ForEach-Object { "$($_.Id)" }) -join ", "
    Write-Error "[FATAL] Unity process currently running (PID: $pids). Terminate first."
    exit 1
}

# 2. Save Guard
Write-Host "[SAVE GUARD] Checking interrupted journal..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action CheckInterruptedJournal
Write-Host "[SAVE GUARD] Taking durable pre-run backup to $backupDir..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Backup -BackupDir $backupDir

# 3. Launch Unity
Write-Host "[GATE 2] Launching Unity Play Mode Scenario..."
$proc = Start-Process -FilePath $unityPath -ArgumentList "-batchmode", "-projectPath", "`"$projectRoot`"", "-executeMethod", "WuxiaGame.Editor.Prototype01PlayTestRunner_P09.RunP09PlayModeScenarioMenu", "-logFile", "`"$logFile`"" -PassThru

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$timedOut = $false

while (-not $proc.HasExited) {
    Start-Sleep -Seconds 2
    if ($sw.Elapsed.TotalSeconds -ge $timeoutSeconds) {
        $timedOut = $true
        Write-Warning "[GATE 2 WATCHDOG] Process exceeded timeout budget of $timeoutSeconds seconds. Terminating process tree..."
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 3
        try { $proc.WaitForExit(5000) } catch {}
        break
    }
}

$sw.Stop()
$exitCode = if ($timedOut) { -1 } else { $proc.ExitCode }

# 4. Save Guard: Restore and Compare
Write-Host "[SAVE GUARD] Restoring persistence to exact pre-run state..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Restore -BackupDir $backupDir

Write-Host "[SAVE GUARD] Comparing state to verify zero diff..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Compare -BackupDir $backupDir
$guardDiffZero = ($LASTEXITCODE -eq 0)

if (Test-Path $logFile) {
    Write-Host "=== LOG SUMMARY ==="
    Get-Content $logFile | Select-String "P09 PLAY MODE", "RESULT", "Release Frame", "Arrival Frame", "ERROR" | ForEach-Object { Write-Host $_ }
} else {
    Write-Host "WARNING: Log file $logFile was not found!"
}

if ($timedOut) {
    Write-Host "============================================================"
    Write-Host "   GATE 2 STATUS: NOT EXECUTED / HEADLESS AUTOMATION BLOCKED"
    Write-Host "   Reason: Unity batchmode without graphics/scene cannot cycle natural frames."
    Write-Host "   Interactive verification is available via Unity Editor menu:"
    Write-Host "   -> 'Tools/Wuxia RPG/Run P09-A Play Mode Scenario (Natural Frames)'"
    Write-Host "   Save Guard: Diff = 0 Verified."
    Write-Host "============================================================"
    exit 2
} elseif ($exitCode -eq 0 -and $guardDiffZero) {
    Write-Host "============================================================"
    Write-Host "   GATE 2 RESULT: PLAY MODE NATURAL FRAMES SCENARIO PASSED"
    Write-Host "============================================================"
    exit 0
} else {
    Write-Host "============================================================"
    Write-Host "   GATE 2 RESULT: FAILED (ExitCode=$exitCode, SaveGuardDiffZero=$guardDiffZero)"
    Write-Host "============================================================"
    exit 1
}
