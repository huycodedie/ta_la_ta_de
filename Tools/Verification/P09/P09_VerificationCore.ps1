# P09 Shared Verification Dispatcher & Coordination Module
# Implements unified lifecycle management for P09 verification suites:
# 1. Concurrency isolation (protects external interactive Unity sessions)
# 2. Save Guard journal check & durable pre-run backup
# 3. Process execution with valid PID tracking and finite watchdog tree kill
# 4. Mandatory finally block executing Save Guard Restore and Compare
# 5. Strict status evaluation (zero false PASS, persistence failure enforcement)

function Stop-ProcessTree([int]$parentPid) {
    try {
        $children = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object { $_.ParentProcessId -eq $parentPid }
        foreach ($child in $children) {
            Stop-ProcessTree $child.ProcessId
        }
        Stop-Process -Id $parentPid -Force -ErrorAction SilentlyContinue
    } catch {}
}

function Invoke-P09SaveGuardRecovery {
    param(
        [Parameter(Mandatory=$true)][string]$SaveGuardScript,
        [Parameter(Mandatory=$true)][string]$BackupDir,
        [string[]]$SaveGuardExtraArgs = @(),
        [scriptblock]$Logger = $null,
        [hashtable]$FailureInjection = @{}
    )

    $restoreSuccess = $false
    $compareSuccess = $false
    $restoreExit = 1
    $compareExit = 1

    # 1. Independent Restore invocation with exception isolation
    try {
        if ($Logger) { & $Logger "[SAVE GUARD] Restoring persistence to exact pre-run state..." }
        if ($FailureInjection.ContainsKey("FailRestore") -and $FailureInjection["FailRestore"]) {
            if ($Logger) { & $Logger "[FAILURE INJECTION] Injected Restore failure!" }
            $restoreExit = 98
        } elseif ($FailureInjection.ContainsKey("ThrowRestore") -and $FailureInjection["ThrowRestore"]) {
            if ($Logger) { & $Logger "[FAILURE INJECTION] Injected Restore exception!" }
            throw "Simulated exception during Restore execution (FailureInjection)"
        } else {
            $restoreProc = Start-Process -FilePath "powershell.exe" -ArgumentList (@("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $SaveGuardScript, "-Action", "Restore", "-BackupDir", $BackupDir) + $SaveGuardExtraArgs) -Wait -PassThru
            $restoreExit = $restoreProc.ExitCode
        }
    } catch {
        $restoreExit = 98
        if ($Logger) { & $Logger "[SAVE GUARD EXCEPTION] Exception during Restore: $($_.Exception.Message)" }
    }

    if ($restoreExit -eq 0) {
        $restoreSuccess = $true
        if ($Logger) { & $Logger "[SAVE GUARD] Restore succeeded." }
    } else {
        $restoreSuccess = $false
        if ($Logger) { & $Logger "[SAVE GUARD ERROR] Restore failed with exit code $restoreExit!" }
    }

    # 2. Independent Compare invocation with exception isolation
    # GUARANTEED TO EXECUTE EVEN IF RESTORE FAILED OR THREW AN EXCEPTION
    try {
        if ($Logger) { & $Logger "[SAVE GUARD] Comparing state against baseline backup to verify zero diff..." }
        if ($FailureInjection.ContainsKey("FailCompare") -and $FailureInjection["FailCompare"]) {
            if ($Logger) { & $Logger "[FAILURE INJECTION] Injected Compare failure!" }
            $compareExit = 97
        } elseif ($FailureInjection.ContainsKey("ThrowCompare") -and $FailureInjection["ThrowCompare"]) {
            if ($Logger) { & $Logger "[FAILURE INJECTION] Injected Compare exception!" }
            throw "Simulated exception during Compare execution (FailureInjection)"
        } else {
            $compareProc = Start-Process -FilePath "powershell.exe" -ArgumentList (@("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $SaveGuardScript, "-Action", "Compare", "-BackupDir", $BackupDir) + $SaveGuardExtraArgs) -Wait -PassThru
            $compareExit = $compareProc.ExitCode
        }
    } catch {
        $compareExit = 97
        if ($Logger) { & $Logger "[SAVE GUARD EXCEPTION] Exception during Compare: $($_.Exception.Message)" }
    }

    if ($compareExit -eq 0) {
        $compareSuccess = $true
        if ($Logger) { & $Logger "[SAVE GUARD] Comparison: Diff = 0 Verified (Exact match)." }
    } else {
        $compareSuccess = $false
        if ($Logger) { & $Logger "[SAVE GUARD ERROR] Comparison FAILED: Diff detected or comparison failed (Exit code $compareExit)!" }
    }

    return @{
        RestoreSuccess = $restoreSuccess
        CompareSuccess = $compareSuccess
        RestoreExit = $restoreExit
        CompareExit = $compareExit
        Passed = ($restoreSuccess -and $compareSuccess)
    }
}

function Invoke-P09VerificationSession {
    param(
        [Parameter(Mandatory=$true)][string]$SuiteName,
        [string]$ProjectRoot = "E:\code\TLTD",
        [string]$ExecutablePath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe",
        [string[]]$ArgumentList = @(),
        [string]$LogFile = "",
        [string]$WrapperLog = "",
        [string]$BackupDir = "",
        [int]$TimeoutSeconds = 90,
        [string]$SaveGuardScript = "$ProjectRoot\Tools\Verification\P09\tltd_save_guard.ps1",
        [string]$PassPattern = "ALL_PASS=True",
        [string[]]$SummaryPatterns = @("P09", "RESULT", "FAIL", "EXCEPTION"),
        [string]$CustomRegKey = $null,
        [bool]$AllowRunningUnity = $false,
        [hashtable]$FailureInjection = @{}
    )

    if ([string]::IsNullOrEmpty($BackupDir)) {
        $BackupDir = "$ProjectRoot\scratch\.save_backup_default"
    }

    if (-not [string]::IsNullOrEmpty($LogFile)) {
        Remove-Item -Force -ErrorAction SilentlyContinue $LogFile
    }
    if (-not [string]::IsNullOrEmpty($WrapperLog)) {
        Remove-Item -Force -ErrorAction SilentlyContinue $WrapperLog
    }

    function LogSession([string]$msg) {
        $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
        $line = "[$timestamp] $msg"
        Write-Host $line
        if (-not [string]::IsNullOrEmpty($WrapperLog)) {
            Add-Content -Path $WrapperLog -Value $line -Encoding UTF8
        }
    }

    LogSession "============================================================"
    LogSession " Running $SuiteName"
    LogSession " Executable:       $ExecutablePath"
    LogSession " Log File:         $LogFile"
    LogSession " Wrapper Log:      $WrapperLog"
    LogSession " Timeout Budget:   $TimeoutSeconds seconds"
    LogSession " Save Guard:       $SaveGuardScript"
    LogSession " Backup Dir:       $BackupDir"
    if ($CustomRegKey) { LogSession " Custom Reg Key:   $CustomRegKey" }
    LogSession "============================================================"

    # Phase 1: Assert no external Unity running (never kill developer's Unity)
    if (-not $AllowRunningUnity) {
        $unityProcs = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
        if ($null -ne $unityProcs -and $unityProcs.Count -gt 0) {
            $pids = ($unityProcs | ForEach-Object { "$($_.Id)" }) -join ", "
            LogSession "[FATAL] Unity process currently running (PID: $pids). Wrapper will not kill external Unity. Terminate first."
            return @{
                Status = "EXTERNAL_UNITY_RUNNING"
                ExitCode = 1
                Launched = $false
                LaunchFailed = $false
                TimedOut = $false
                ScenarioPassed = $false
                SaveRestored = $false
                SaveDiffZero = $false
                UnityOsExit = "NOT_LAUNCHED"
            }
        }
    }

    # Save Guard extra args helper
    $sgExtraArgs = @()
    if ($CustomRegKey) { $sgExtraArgs += @("-CustomRegKey", $CustomRegKey) }
    if ($AllowRunningUnity) { $sgExtraArgs += @("-AllowRunningUnity") }

    # Phase 2: Save Guard - Check interrupted journal
    LogSession "[SAVE GUARD] Checking interrupted journal..."
    $journalProc = Start-Process -FilePath "powershell.exe" -ArgumentList (@("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $SaveGuardScript, "-Action", "CheckInterruptedJournal", "-BackupDir", $BackupDir) + $sgExtraArgs) -Wait -PassThru
    $journalExit = $journalProc.ExitCode
    if ($journalExit -ne 0) {
        LogSession "[SAVE GUARD FATAL] CheckInterruptedJournal failed with exit code $journalExit. Refusing to launch."
        return @{
            Status = "JOURNAL_CHECK_FAILED"
            ExitCode = 1
            Launched = $false
            LaunchFailed = $false
            TimedOut = $false
            ScenarioPassed = $false
            SaveRestored = $false
            SaveDiffZero = $false
            UnityOsExit = "NOT_LAUNCHED"
        }
    }

    # Phase 3: Save Guard - Durable Pre-Run Backup
    LogSession "[SAVE GUARD] Taking durable pre-run backup to $BackupDir..."
    $backupExit = 1
    if ($FailureInjection.ContainsKey("FailBackup") -and $FailureInjection["FailBackup"]) {
        LogSession "[FAILURE INJECTION] Injected Backup failure!"
        $backupExit = 99
    } else {
        $backupProc = Start-Process -FilePath "powershell.exe" -ArgumentList (@("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $SaveGuardScript, "-Action", "Backup", "-BackupDir", $BackupDir) + $sgExtraArgs) -Wait -PassThru
        $backupExit = $backupProc.ExitCode
    }

    if ($backupExit -ne 0) {
        LogSession "[SAVE GUARD FATAL] Backup failed with exit code $backupExit. Refusing to launch."
        return @{
            Status = "BACKUP_FAILED"
            ExitCode = 1
            Launched = $false
            LaunchFailed = $false
            TimedOut = $false
            ScenarioPassed = $false
            SaveRestored = $false
            SaveDiffZero = $false
            UnityOsExit = "NOT_LAUNCHED"
        }
    }

    # Phase 4: Launch and watchdog execution
    $launched = $false
    $launchFailed = $false
    $timedOut = $false
    $unityOsExitCode = "UNKNOWN"
    $scenarioPassed = $false
    $saveRestored = $false
    $saveDiffZero = $false
    $wrapperExitCode = 1
    $proc = $null

    try {
        if ($FailureInjection.ContainsKey("FailLaunch") -and $FailureInjection["FailLaunch"]) {
            $launchFailed = $true
            throw "Simulated Process Launch Failure (FailureInjection)"
        }

        LogSession "[SESSION] Launching process with timeout budget of $TimeoutSeconds seconds..."
        try {
            $proc = Start-Process -FilePath $ExecutablePath -ArgumentList $ArgumentList -PassThru
            if ($null -eq $proc -or $proc.Id -le 0) {
                throw "Start-Process returned null or invalid process object."
            }
            $launched = $true
            LogSession "[SESSION] Process started with PID $($proc.Id)."
        } catch {
            $launchFailed = $true
            LogSession "[SESSION ERROR] Process launch failed: $($_.Exception.Message)"
            throw
        }

        # Watchdog loop
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $forceTimeout = ($FailureInjection.ContainsKey("FailTimeout") -and $FailureInjection["FailTimeout"])

        while (-not $proc.HasExited) {
            Start-Sleep -Milliseconds 500
            if ($sw.Elapsed.TotalSeconds -ge $TimeoutSeconds -or $forceTimeout) {
                $timedOut = $true
                LogSession "[WATCHDOG] Process exceeded timeout budget of $TimeoutSeconds seconds. Terminating wrapper-owned process tree PID $($proc.Id)..."
                Stop-ProcessTree $proc.Id
                Start-Sleep -Seconds 2
                break
            }
        }
        $sw.Stop()

        if ($FailureInjection.ContainsKey("FailCrash") -and $FailureInjection["FailCrash"]) {
            throw "Simulated Process Crash (FailureInjection)"
        }

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

        LogSession "Process finished: OS Exit Code = $unityOsExitCode, Elapsed = $([math]::Round($sw.Elapsed.TotalSeconds, 2))s, TimedOut = $timedOut"
    }
    catch {
        LogSession "[SESSION EXCEPTION] Caught exception during execution: $($_.Exception.Message)"
    }
    finally {
        # Phase 5: Save Guard - Restore and Compare inside finally using shared recovery helper
        $recovery = Invoke-P09SaveGuardRecovery `
            -SaveGuardScript $SaveGuardScript `
            -BackupDir $BackupDir `
            -SaveGuardExtraArgs $sgExtraArgs `
            -Logger ${function:LogSession} `
            -FailureInjection $FailureInjection

        $saveRestored = $recovery.RestoreSuccess
        $saveDiffZero = $recovery.CompareSuccess
    }

    # Phase 6: Parse log results
    if (-not [string]::IsNullOrEmpty($LogFile) -and (Test-Path $LogFile)) {
        LogSession "=== LOG SUMMARY ==="
        Get-Content $LogFile | Select-String $SummaryPatterns | ForEach-Object { LogSession $_.Line }
        $passLine = Get-Content $LogFile | Select-String $PassPattern
        if ($null -ne $passLine) {
            $scenarioPassed = $true
        }
    }

    if ($FailureInjection.ContainsKey("ForceScenarioPass") -and $FailureInjection["ForceScenarioPass"]) {
        $scenarioPassed = $true
    }
    if ($FailureInjection.ContainsKey("ForceScenarioFail") -and $FailureInjection["ForceScenarioFail"]) {
        $scenarioPassed = $false
    }

    # Phase 7: Final Status Determination
    LogSession "============================================================"
    LogSession "   $SuiteName EXECUTION SUMMARY"
    LogSession "============================================================"
    LogSession " Process Launched:      $launched"
    LogSession " Launch Failed:         $launchFailed"
    LogSession " Process OS Exit:       $unityOsExitCode"
    LogSession " Timeout Status:        $(if ($timedOut) { 'TIMED OUT' } else { 'COMPLETED WITHIN BUDGET' })"
    LogSession " Scenario/Suite Result: $(if ($scenarioPassed) { 'PASSED' } else { 'FAILED OR INCOMPLETE' })"
    LogSession " Save State Restore:    $(if ($saveRestored) { 'SUCCESS' } else { 'FAILED' })"
    LogSession " Save Diff Match:       $(if ($saveDiffZero) { 'DIFF = 0 VERIFIED' } else { 'DIFF DETECTED / FAILED' })"

    $finalStatus = "FAIL"

    # Strict status evaluation
    if (-not $saveRestored -or -not $saveDiffZero) {
        $wrapperExitCode = 1
        $finalStatus = "PERSISTENCE_FAILURE"
        LogSession " OVERALL RESULT: PERSISTENCE FAILURE (Restore=$saveRestored, DiffZero=$saveDiffZero). ALWAYS FAILS."
    } elseif ($scenarioPassed -and ($unityOsExitCode -eq 0) -and (-not $timedOut) -and $saveRestored -and $saveDiffZero) {
        $wrapperExitCode = 0
        $finalStatus = "PASS"
        LogSession " OVERALL RESULT: ALL CHECKS PASSED + ZERO DIFF (PASS)"
    } elseif ($scenarioPassed -and $timedOut -and $saveRestored -and $saveDiffZero) {
        $wrapperExitCode = 2
        $finalStatus = "SCENARIO_PASS_EXIT_TIMEOUT"
        LogSession " OVERALL RESULT: SCENARIO PASSED BUT EXIT TIMED OUT (Separately reported: exit code 2, Diff = 0)"
    } else {
        $wrapperExitCode = 1
        $finalStatus = "FAIL"
        LogSession " OVERALL RESULT: FAILED (ExitCode=$unityOsExitCode, DiffZero=$saveDiffZero, ScenarioPassed=$scenarioPassed, TimedOut=$timedOut)"
    }
    LogSession "============================================================"

    return @{
        Status = $finalStatus
        ExitCode = $wrapperExitCode
        Launched = $launched
        LaunchFailed = $launchFailed
        TimedOut = $timedOut
        ScenarioPassed = $scenarioPassed
        SaveRestored = $saveRestored
        SaveDiffZero = $saveDiffZero
        UnityOsExit = $unityOsExitCode
    }
}
