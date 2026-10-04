# P09-B Manual Safe Observation Session Launcher
# Launches interactive Unity session with full Save Guard protection for Dash Observation
# Automatically executes Backup before launch and Restore & Compare upon closure.

param(
    [string]$ProjectRoot = "E:\code\TLTD",
    [string]$UnityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe",
    [int]$SessionTimeoutSeconds = 3600,
    [string]$BackupDir = "$ProjectRoot\scratch\.save_backup_manual_session_p09b",
    [string]$LogFile = "$ProjectRoot\manual_session_p09b.log",
    [string]$WrapperLog = "$ProjectRoot\scratch\manual_wrapper_p09b.log",
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
Log "   P09-B SAFE MANUAL OBSERVATION SESSION LAUNCHER" -color Cyan
Log "   Unity Path:      $UnityPath"
Log "   Project Root:    $ProjectRoot"
Log "   Backup Dir:      $BackupDir"
Log "   Unity Log:       $LogFile"
Log "   Wrapper Log:     $WrapperLog"
Log "   Session Timeout: $SessionTimeoutSeconds seconds"
if ($CustomRegKey) { Log "   Custom Reg Key:  $CustomRegKey" }
Log "============================================================" -color Cyan

# 1. Concurrency Check
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
Log " LAUNCHING INTERACTIVE UNITY FOR P09-B DASH MANUAL OBSERVATION" -color Cyan
Log " - Entry point: WuxiaGame.Editor.P09BManualSessionBootstrap.LaunchFromSaveGuard" -color Cyan
Log " - Isolated scene & root: [P09B_Manual_Observation_Fixture]" -color Cyan
Log " - Automatic fixture setup: Hero (0m), Target A (+4.5m), Target B (-4.5m)" -color Cyan
Log " - Control Window: Click buttons to dash, move target, pause, inject CC" -color Cyan
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
    if (-not [string]::IsNullOrWhiteSpace($UnityExtraArgs)) {
        $unityArgs = $UnityExtraArgs.Split(" ")
    } else {
        $unityArgs = @(
            "-projectPath", "`"$ProjectRoot`"",
            "-p09bManualSession",
            "-executeMethod", "WuxiaGame.Editor.P09BManualSessionBootstrap.LaunchFromSaveGuard",
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

    $restoreSuccess = $recovery.RestoreSuccess
    $compareSuccess = $recovery.CompareSuccess

    $wrapperExitCode = 0
    $sessionStatus = "SUCCESS"

    if (-not $restoreSuccess -or -not $compareSuccess) {
        $wrapperExitCode = 3
        $sessionStatus = "PERSISTENCE_FAILURE"
    } elseif ($timedOut) {
        $wrapperExitCode = 2
        $sessionStatus = "TIMEOUT"
    } elseif ($null -ne $launchException) {
        $wrapperExitCode = 1
        $sessionStatus = "LAUNCH_EXCEPTION"
    } elseif ($null -eq $procExitCode -or $procExitCode -ne 0) {
        $wrapperExitCode = 1
        $sessionStatus = "UNITY_OS_FAILURE"
    } else {
        $wrapperExitCode = 0
        $sessionStatus = "SUCCESS"
    }

    Log ""
    Log "============================================================" -color Cyan
    Log "   P09-B MANUAL OBSERVATION SESSION SUMMARY" -color Cyan
    Log "   Session Result:   $sessionStatus"
    Log "   Unity OS Exit:    $procExitCode"
    Log "   Wrapper Exit:     $wrapperExitCode"
    Log "   Duration:         $([math]::Round($sw.Elapsed.TotalMinutes, 2)) minutes"
    Log "   Restore Result:   $(if ($restoreSuccess) { 'SUCCESS' } else { 'FAILED' })"
    Log "   Compare Result:   $(if ($compareSuccess) { 'SUCCESS (0 Diffs verified)' } else { 'FAILED' })"
    Log "============================================================" -color Cyan

    exit $wrapperExitCode
}
