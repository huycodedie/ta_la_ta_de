$ErrorActionPreference = "Stop"

$projectRoot = "E:\code\TLTD"
$unityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe"
$logFile = "$projectRoot\gate1_p09_tests.log"
$saveGuardScript = "$projectRoot\Tools\Verification\P09\tltd_save_guard.ps1"
$backupDir = "$projectRoot\scratch\.save_backup_gate1_p09"

Remove-Item -Force -ErrorAction SilentlyContinue $logFile

Write-Host "============================================================"
Write-Host " Running Gate 1: P09-A Projectile Foundation Tests (T01 - T15)"
Write-Host " Unity Path:       $unityPath"
Write-Host " Log File:         $logFile"
Write-Host " Save Guard:       $saveGuardScript"
Write-Host "============================================================"

# 1. Assert no other Unity instance running
$unityProcs = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($null -ne $unityProcs -and $unityProcs.Count -gt 0) {
    $pids = ($unityProcs | ForEach-Object { "$($_.Id)" }) -join ", "
    Write-Error "[FATAL] Unity process currently running (PID: $pids). Terminate first."
    exit 1
}

# 2. Save Guard: Check journal and Backup
Write-Host "[SAVE GUARD] Checking interrupted journal..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action CheckInterruptedJournal
Write-Host "[SAVE GUARD] Taking durable pre-run backup to $backupDir..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Backup -BackupDir $backupDir

# 3. Run Unity Gate 1
Write-Host "[GATE 1] Launching Unity batchmode suite..."
$proc = Start-Process -FilePath $unityPath -ArgumentList "-batchmode", "-quit", "-projectPath", "`"$projectRoot`"", "-executeMethod", "WuxiaGame.Editor.Prototype01PlayTestRunner_P09.RunGate1_P09_CLI", "-logFile", "`"$logFile`"" -PassThru -Wait
$exitCode = $proc.ExitCode
Write-Host "Unity Process Exited with Code: $exitCode"

# 4. Save Guard: Restore and Compare
Write-Host "[SAVE GUARD] Restoring persistence to exact pre-run state..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Restore -BackupDir $backupDir

Write-Host "[SAVE GUARD] Comparing state to verify zero diff..."
& powershell -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Compare -BackupDir $backupDir
$guardDiffZero = ($LASTEXITCODE -eq 0)

if (Test-Path $logFile) {
    Write-Host "=== LOG SUMMARY ==="
    Get-Content $logFile | Select-String "P09 AUTOMATED TESTS RESULT", "\[T", "FAIL", "EXCEPTION" | ForEach-Object { Write-Host $_ }
} else {
    Write-Host "WARNING: Log file $logFile was not found!"
}

if ($exitCode -eq 0 -and $guardDiffZero) {
    Write-Host "============================================================"
    Write-Host "   GATE 1 RESULT: ALL TESTS PASSED + SAVE GUARD ZERO DIFF"
    Write-Host "============================================================"
    exit 0
} else {
    Write-Host "============================================================"
    Write-Host "   GATE 1 RESULT: FAILED (UnityCode=$exitCode, SaveGuardDiffZero=$guardDiffZero)"
    Write-Host "============================================================"
    exit 1
}
