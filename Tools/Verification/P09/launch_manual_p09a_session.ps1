# P09-A Manual Safe Observation Session Launcher
# Launches interactive Unity session with full Save Guard protection
# Automatically executes Backup before launch and Restore & Compare upon closure.

param(
    [string]$ProjectRoot = "E:\code\TLTD",
    [string]$UnityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe",
    [int]$SessionTimeoutSeconds = 3600,
    [string]$BackupDir = "$ProjectRoot\scratch\.save_backup_manual_session",
    [string]$LogFile = "$ProjectRoot\manual_session.log"
)

$ErrorActionPreference = "Stop"
$saveGuardScript = "$ProjectRoot\Tools\Verification\P09\tltd_save_guard.ps1"

Write-Host "============================================================"
Write-Host "   P09-A SAFE MANUAL OBSERVATION SESSION LAUNCHER"
Write-Host "   Unity Path:      $UnityPath"
Write-Host "   Project Root:    $ProjectRoot"
Write-Host "   Backup Dir:      $BackupDir"
Write-Host "   Session Timeout: $SessionTimeoutSeconds seconds"
Write-Host "============================================================"

# 1. Concurrency Check: Never kill developer's Unity
$unityProcs = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($null -ne $unityProcs -and $unityProcs.Count -gt 0) {
    $pids = ($unityProcs | ForEach-Object { "$($_.Id)" }) -join ", "
    Write-Host "[FATAL] An external Unity Editor is currently running (PID: $pids)." -ForegroundColor Red
    Write-Host ""
    Write-Host "To prevent concurrent PlayerPrefs / registry conflicts and protect your save state:" -ForegroundColor Yellow
    Write-Host "  1. Please save any open work in your running Unity Editor." -ForegroundColor Yellow
    Write-Host "  2. Close the running Unity Editor." -ForegroundColor Yellow
    Write-Host "  3. Re-run this launcher script." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "This launcher refuses to terminate external developer processes." -ForegroundColor Red
    exit 1
}

# 2. Save Guard: Journal check
Write-Host "[SAVE GUARD] Checking interrupted journal..." -ForegroundColor Cyan
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action CheckInterruptedJournal -BackupDir $BackupDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "[SAVE GUARD FATAL] CheckInterruptedJournal failed (Exit code $LASTEXITCODE). Refusing to launch." -ForegroundColor Red
    exit 1
}

# 3. Save Guard: Pre-session Backup
Write-Host "[SAVE GUARD] Taking durable pre-session backup..." -ForegroundColor Cyan
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Backup -BackupDir $BackupDir
if ($LASTEXITCODE -ne 0) {
    Write-Host "[SAVE GUARD FATAL] Backup failed (Exit code $LASTEXITCODE). Refusing to launch." -ForegroundColor Red
    exit 1
}

Write-Host "[SAVE GUARD] Pre-session backup secured successfully." -ForegroundColor Green
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " LAUNCHING INTERACTIVE UNITY FOR MANUAL INSPECTION" -ForegroundColor Cyan
Write-Host " - You may inspect scenes, skills, and mechanics in Game View." -ForegroundColor Cyan
Write-Host " - When finished, simply CLOSE Unity normally (Alt+F4 or File -> Exit)." -ForegroundColor Cyan
Write-Host " - Save Guard will immediately execute Restore & Compare in finally." -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$timedOut = $false

try {
    # Launch interactive Unity (no -batchmode, no -quit)
    $proc = Start-Process -FilePath $UnityPath -ArgumentList "-projectPath", "`"$ProjectRoot`"", "-logFile", "`"$LogFile`"" -PassThru
    Write-Host "[SESSION] Unity launched with PID $($proc.Id). Waiting for manual session completion..."

    while (-not $proc.HasExited) {
        Start-Sleep -Seconds 1
        if ($sw.Elapsed.TotalSeconds -ge $SessionTimeoutSeconds) {
            $timedOut = $true
            Write-Host "[TIMEOUT] Manual session reached timeout budget of $SessionTimeoutSeconds seconds. Terminating session process..." -ForegroundColor Yellow
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            break
        }
    }
    $sw.Stop()

    if (-not $timedOut) {
        Write-Host "[SESSION] Unity Editor closed normally. Elapsed: $([math]::Round($sw.Elapsed.TotalMinutes, 1)) minutes." -ForegroundColor Green
    }
}
finally {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host " EXECUTING POST-SESSION SAVE GUARD RESTORE & COMPARE" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Cyan

    Write-Host "[SAVE GUARD] Restoring persistence to exact pre-session state..." -ForegroundColor Cyan
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Restore -BackupDir $BackupDir
    $restoreExit = $LASTEXITCODE

    Write-Host "[SAVE GUARD] Comparing state against baseline backup..." -ForegroundColor Cyan
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Compare -BackupDir $BackupDir
    $compareExit = $LASTEXITCODE

    if ($restoreExit -eq 0 -and $compareExit -eq 0) {
        Write-Host "[SAVE GUARD PASS] State 100% Restored. Diff = 0 Verified (Exact match)." -ForegroundColor Green
    } else {
        Write-Host "[SAVE GUARD WARNING] Restore/Compare reported issues: RestoreExit=$restoreExit, CompareExit=$compareExit" -ForegroundColor Red
    }
    Write-Host "============================================================" -ForegroundColor Cyan
}
