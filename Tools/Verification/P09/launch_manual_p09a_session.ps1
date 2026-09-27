# P09-A Manual Safe Observation Session Launcher
# Launches interactive Unity session with full Save Guard protection
# Automatically executes Backup before launch and Restore & Compare upon closure.

param(
    [string]$ProjectRoot = "E:\code\TLTD",
    [string]$UnityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe",
    [int]$SessionTimeoutSeconds = 3600,
    [string]$BackupDir = "$ProjectRoot\scratch\.save_backup_manual_session",
    [string]$LogFile = "$ProjectRoot\manual_session.log",
    [string]$WrapperLog = "$ProjectRoot\scratch\manual_wrapper.log",
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

$scratchDir = Split-Path -Parent $WrapperLog
if ([string]::IsNullOrWhiteSpace($scratchDir)) {
    $scratchDir = "$ProjectRoot\scratch"
}
if (-not (Test-Path $scratchDir)) {
    New-Item -ItemType Directory -Path $scratchDir -Force | Out-Null
}
Remove-Item -Force -ErrorAction SilentlyContinue $WrapperLog

function Log([string]$msg, [ConsoleColor]$color = [ConsoleColor]::White) {
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
    $line = "[$timestamp] $msg"
    Write-Host $line -ForegroundColor $color
    Add-Content -Path $WrapperLog -Value $line -Encoding UTF8 -ErrorAction SilentlyContinue
}

Log "============================================================" -color Cyan
Log "   P09-A SAFE MANUAL OBSERVATION SESSION LAUNCHER" -color Cyan
Log "   Unity Path:      $UnityPath"
Log "   Project Root:    $ProjectRoot"
Log "   Backup Dir:      $BackupDir"
Log "   Unity Log:       $LogFile"
Log "   Wrapper Log:     $WrapperLog"
Log "   Session Timeout: $SessionTimeoutSeconds seconds"
if ($CustomRegKey) { Log "   Custom Reg Key:  $CustomRegKey" }
Log "============================================================" -color Cyan

# 1. Concurrency Check: Never kill developer's Unity
if (-not $AllowRunningUnity) {
    $unityProcs = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
    if ($null -ne $unityProcs -and $unityProcs.Count -gt 0) {
        $pids = ($unityProcs | ForEach-Object { "$($_.Id)" }) -join ", "
        Log "[FATAL] An external Unity Editor is currently running (PID: $pids)." -color Red
        Log ""
        Log "To prevent concurrent PlayerPrefs / registry conflicts and protect your save state:" -color Yellow
        Log "  1. Please save any open work in your running Unity Editor." -color Yellow
        Log "  2. Close the running Unity Editor." -color Yellow
        Log "  3. Re-run this launcher script." -color Yellow
        Log ""
        Log "This launcher refuses to terminate external developer processes." -color Red
        exit 1
    }
}

$sgExtraArgs = @()
if ($CustomRegKey) { $sgExtraArgs += @("-CustomRegKey", $CustomRegKey) }
if ($AllowRunningUnity) { $sgExtraArgs += @("-AllowRunningUnity") }

# 2. Save Guard: Journal check
Log "[SAVE GUARD] Checking interrupted journal..." -color Cyan
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action CheckInterruptedJournal -BackupDir $BackupDir @sgExtraArgs
if ($LASTEXITCODE -ne 0) {
    Log "[SAVE GUARD FATAL] CheckInterruptedJournal failed (Exit code $LASTEXITCODE). Refusing to launch." -color Red
    exit 1
}

# 3. Save Guard: Pre-session Backup
Log "[SAVE GUARD] Taking durable pre-session backup..." -color Cyan
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Backup -BackupDir $BackupDir @sgExtraArgs
if ($LASTEXITCODE -ne 0) {
    Log "[SAVE GUARD FATAL] Backup failed (Exit code $LASTEXITCODE). Refusing to launch." -color Red
    exit 1
}

Log "[SAVE GUARD] Pre-session backup secured successfully." -color Green
Log ""
Log "============================================================" -color Cyan
Log " LAUNCHING INTERACTIVE UNITY FOR MANUAL OBSERVATION" -color Cyan
Log " - Entry point: WuxiaGame.Editor.P09ManualSessionBootstrap.LaunchFromSaveGuard" -color Cyan
Log " - Isolated scene & root: [P09_Manual_Observation_Fixture_Root]" -color Cyan
Log " - Automatic fixture setup: Hero (Cyan), Target A (Red), Target B (Orange)" -color Cyan
Log " - Control Window: Click buttons to cast instant/cast-time, retarget, or pause" -color Cyan
Log " - When finished, simply CLOSE Unity normally (Alt+F4 or File -> Exit)." -color Cyan
Log " - Save Guard will immediately execute Restore & Compare in finally." -color Cyan
Log "============================================================" -color Cyan
Log ""

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$timedOut = $false
$proc = $null
$launchException = $null
$procExitCode = $null

try {
    # Launch interactive Unity with dedicated manual observation bootstrap (GUI mode, no -batchmode)
    if (-not [string]::IsNullOrWhiteSpace($UnityExtraArgs)) {
        $unityArgs = $UnityExtraArgs.Split(" ")
    } else {
        $unityArgs = @(
            "-projectPath", "`"$ProjectRoot`"",
            "-p09ManualSession",
            "-executeMethod", "WuxiaGame.Editor.P09ManualSessionBootstrap.LaunchFromSaveGuard",
            "-logFile", "`"$LogFile`""
        )
    }
    $proc = Start-Process -FilePath $UnityPath -ArgumentList $unityArgs -PassThru
    Log "[SESSION] Unity launched with PID $($proc.Id). Waiting for manual session completion..." -color Cyan

    while (-not $proc.HasExited) {
        Start-Sleep -Seconds 1
        if ($sw.Elapsed.TotalSeconds -ge $SessionTimeoutSeconds) {
            $timedOut = $true
            Log "[TIMEOUT] Manual session reached timeout budget of $SessionTimeoutSeconds seconds. Terminating session process..." -color Yellow
            Stop-ProcessTree $proc.Id
            break
        }
    }
    $sw.Stop()

    if ($null -ne $proc -and $proc.HasExited) {
        $procExitCode = $proc.ExitCode
    }

    if (-not $timedOut) {
        Log "[SESSION] Unity Editor closed. Exit code: $procExitCode. Elapsed: $([math]::Round($sw.Elapsed.TotalMinutes, 1)) minutes." -color Green
    }
}
catch {
    $launchException = $_
    Log "[LAUNCH ERROR] Exception during Unity launch/execution: $($_.Exception.Message)" -color Red
}
finally {
    if ($null -ne $proc -and -not $proc.HasExited) {
        Log "[SESSION] Ensuring wrapper session process tree PID $($proc.Id) is stopped before restore..." -color Yellow
        Stop-ProcessTree $proc.Id
        try { $proc.WaitForExit(5000) } catch {}
    }

    Log ""
    Log "============================================================" -color Cyan
    Log " EXECUTING POST-SESSION SAVE GUARD RESTORE & COMPARE" -color Cyan
    Log "============================================================" -color Cyan

    $logFn = { param($m) Log $m -color Cyan }
    $recovery = Invoke-P09SaveGuardRecovery `
        -SaveGuardScript $saveGuardScript `
        -BackupDir $BackupDir `
        -SaveGuardExtraArgs $sgExtraArgs `
        -Logger $logFn `
        -FailureInjection $FailureInjection

    Log "============================================================" -color Cyan
    Log "   MANUAL SESSION OUTCOME SUMMARY" -color Cyan
    Log "============================================================" -color Cyan
    Log " Launch Exception:      $(if ($null -ne $launchException) { $launchException.Exception.Message } else { 'NONE' })"
    Log " Process Exit Code:     $(if ($null -ne $procExitCode) { $procExitCode } else { 'N/A' })"
    Log " Session Timed Out:     $timedOut"
    Log " Restore Status:        $(if ($recovery.RestoreSuccess) { 'SUCCESS' } else { 'FAILED' })"
    Log " Diff Zero Match:       $(if ($recovery.CompareSuccess) { 'DIFF = 0 VERIFIED' } else { 'DIFF DETECTED / FAILED' })"

    # Evaluation precedence:
    # 1. If persistence recovery failed -> Exit 1 (PERSISTENCE_FAILURE)
    # 2. If launch threw exception -> Exit 1 (LAUNCH_FAILURE)
    # 3. If process exited with non-zero code -> Exit non-zero (PROCESS_CRASH / ERROR)
    # 4. If session timed out -> Exit 2 (SESSION_TIMEOUT)
    # 5. Success -> Exit 0 (SAVE_GUARD_PASS)
    if (-not $recovery.Passed) {
        Log " OVERALL STATUS: PERSISTENCE_FAILURE (Exit code 1)" -color Red
        Log " [CRITICAL] Save recovery did not succeed. Baseline backup preserved in $BackupDir." -color Red
        exit 1
    }
    if ($null -ne $launchException) {
        Log " OVERALL STATUS: LAUNCH_FAILURE (Exit code 1)" -color Red
        Log " [CRITICAL] Launch threw an exception even though Save Guard restored." -color Red
        exit 1
    }
    if ($null -ne $procExitCode -and $procExitCode -ne 0) {
        Log " OVERALL STATUS: PROCESS_CRASH (Exit code $procExitCode)" -color Red
        Log " [CRITICAL] Process exited with non-zero code $procExitCode." -color Red
        exit $procExitCode
    }
    if ($timedOut) {
        Log " OVERALL STATUS: SESSION_TIMEOUT (Exit code 2)" -color Yellow
        Log " [NOTICE] Session timed out. Persistence was restored (Diff = 0)." -color Yellow
        exit 2
    }

    Log " OVERALL STATUS: SAVE_GUARD_PASS (Exit code 0)" -color Green
    Log " [NOTICE] SAVE_GUARD_PASS confirms persistence recovery success only." -color Green
    Log " [NOTICE] Developer verification status is tracked in MANUAL_P09A_CHECKLIST.md." -color Cyan
    exit 0
}
