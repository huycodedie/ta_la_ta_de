# P09-A Manual Safe Observation Session Launcher
# Launches interactive Unity session with full Save Guard protection
# Automatically executes Backup before launch and Restore & Compare upon closure.

param(
    [string]$ProjectRoot = "E:\code\TLTD",
    [string]$UnityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe",
    [int]$SessionTimeoutSeconds = 3600,
    [string]$BackupDir = "$ProjectRoot\scratch\.save_backup_manual_session",
    [string]$LogFile = "$ProjectRoot\manual_session.log",
    [string]$CustomRegKey = $null,
    [switch]$AllowRunningUnity,
    [object]$FailureInjection = @{},
    [string]$UnityExtraArgs = ""
)

$ErrorActionPreference = "Stop"
if ($FailureInjection -is [string] -and -not [string]::IsNullOrWhiteSpace($FailureInjection)) {
    if ($FailureInjection -like "*FailRestore*") {
        $FailureInjection = @{ FailRestore = $true }
    } else {
        try { $FailureInjection = Invoke-Expression $FailureInjection } catch { $FailureInjection = @{} }
    }
}
$saveGuardScript = "$ProjectRoot\Tools\Verification\P09\tltd_save_guard.ps1"
$coreScript = "$ProjectRoot\Tools\Verification\P09\P09_VerificationCore.ps1"
. $coreScript

Write-Host "============================================================"
Write-Host "   P09-A SAFE MANUAL OBSERVATION SESSION LAUNCHER"
Write-Host "   Unity Path:      $UnityPath"
Write-Host "   Project Root:    $ProjectRoot"
Write-Host "   Backup Dir:      $BackupDir"
Write-Host "   Session Timeout: $SessionTimeoutSeconds seconds"
if ($CustomRegKey) { Write-Host "   Custom Reg Key:  $CustomRegKey" }
Write-Host "============================================================"

# 1. Concurrency Check: Never kill developer's Unity
if (-not $AllowRunningUnity) {
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
}

$sgExtraArgs = @()
if ($CustomRegKey) { $sgExtraArgs += @("-CustomRegKey", $CustomRegKey) }
if ($AllowRunningUnity) { $sgExtraArgs += @("-AllowRunningUnity") }

# 2. Save Guard: Journal check
Write-Host "[SAVE GUARD] Checking interrupted journal..." -ForegroundColor Cyan
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action CheckInterruptedJournal -BackupDir $BackupDir @sgExtraArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "[SAVE GUARD FATAL] CheckInterruptedJournal failed (Exit code $LASTEXITCODE). Refusing to launch." -ForegroundColor Red
    exit 1
}

# 3. Save Guard: Pre-session Backup
Write-Host "[SAVE GUARD] Taking durable pre-session backup..." -ForegroundColor Cyan
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Backup -BackupDir $BackupDir @sgExtraArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "[SAVE GUARD FATAL] Backup failed (Exit code $LASTEXITCODE). Refusing to launch." -ForegroundColor Red
    exit 1
}

Write-Host "[SAVE GUARD] Pre-session backup secured successfully." -ForegroundColor Green
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " LAUNCHING INTERACTIVE UNITY FOR MANUAL OBSERVATION" -ForegroundColor Cyan
Write-Host " - Entry point: WuxiaGame.Editor.P09ManualSessionBootstrap.LaunchFromSaveGuard" -ForegroundColor Cyan
Write-Host " - Automatic fixture setup: Hero (Cyan), Target A (Red), Target B (Orange)" -ForegroundColor Cyan
Write-Host " - Control Window: Click buttons to cast instant/cast-time, retarget, or pause" -ForegroundColor Cyan
Write-Host " - When finished, simply CLOSE Unity normally (Alt+F4 or File -> Exit)." -ForegroundColor Cyan
Write-Host " - Save Guard will immediately execute Restore & Compare in finally." -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$timedOut = $false
$proc = $null

try {
    # Launch interactive Unity with dedicated manual observation bootstrap (GUI mode, no -batchmode)
    if (-not [string]::IsNullOrWhiteSpace($UnityExtraArgs)) {
        $unityArgs = $UnityExtraArgs.Split(" ")
    } else {
        $unityArgs = @(
            "-projectPath", "`"$ProjectRoot`"",
            "-executeMethod", "WuxiaGame.Editor.P09ManualSessionBootstrap.LaunchFromSaveGuard",
            "-logFile", "`"$LogFile`""
        )
    }
    $proc = Start-Process -FilePath $UnityPath -ArgumentList $unityArgs -PassThru
    Write-Host "[SESSION] Unity launched with PID $($proc.Id). Waiting for manual session completion..."

    while (-not $proc.HasExited) {
        Start-Sleep -Seconds 1
        if ($sw.Elapsed.TotalSeconds -ge $SessionTimeoutSeconds) {
            $timedOut = $true
            Write-Host "[TIMEOUT] Manual session reached timeout budget of $SessionTimeoutSeconds seconds. Terminating session process..." -ForegroundColor Yellow
            Stop-ProcessTree $proc.Id
            break
        }
    }
    $sw.Stop()

    if (-not $timedOut) {
        Write-Host "[SESSION] Unity Editor closed normally. Elapsed: $([math]::Round($sw.Elapsed.TotalMinutes, 1)) minutes." -ForegroundColor Green
    }
}
finally {
    if ($null -ne $proc -and -not $proc.HasExited) {
        Write-Host "[SESSION] Ensuring wrapper session process tree PID $($proc.Id) is stopped before restore..."
        Stop-ProcessTree $proc.Id
        try { $proc.WaitForExit(5000) } catch {}
    }

    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host " EXECUTING POST-SESSION SAVE GUARD RESTORE & COMPARE" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Cyan

    $logFn = { param($m) Write-Host $m }
    $recovery = Invoke-P09SaveGuardRecovery `
        -SaveGuardScript $saveGuardScript `
        -BackupDir $BackupDir `
        -SaveGuardExtraArgs $sgExtraArgs `
        -Logger $logFn `
        -FailureInjection $FailureInjection

    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host "   MANUAL SESSION OUTCOME SUMMARY" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host " Session Timed Out:     $timedOut"
    Write-Host " Restore Status:        $(if ($recovery.RestoreSuccess) { 'SUCCESS' } else { 'FAILED' })"
    Write-Host " Diff Zero Match:       $(if ($recovery.CompareSuccess) { 'DIFF = 0 VERIFIED' } else { 'DIFF DETECTED / FAILED' })"

    if (-not $recovery.Passed) {
        Write-Host " OVERALL STATUS: PERSISTENCE_FAILURE (Exit code 1)" -ForegroundColor Red
        Write-Host " [CRITICAL] Save recovery did not succeed. Baseline backup preserved in $BackupDir." -ForegroundColor Red
        exit 1
    } elseif ($timedOut) {
        Write-Host " OVERALL STATUS: SESSION_TIMEOUT (Exit code 2)" -ForegroundColor Yellow
        Write-Host " [NOTICE] Session timed out. Persistence was restored (Diff = 0)." -ForegroundColor Yellow
        exit 2
    } else {
        Write-Host " OVERALL STATUS: SAVE_GUARD_PASS (Exit code 0)" -ForegroundColor Green
        Write-Host " [NOTICE] SAVE_GUARD_PASS confirms persistence recovery success only." -ForegroundColor Green
        Write-Host " [NOTICE] Developer verification status is tracked in MANUAL_P09A_CHECKLIST.md." -ForegroundColor Cyan
        exit 0
    }
}
