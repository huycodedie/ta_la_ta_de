# P09 Wrapper Failure Path Verification Suite
# Tests all 6 required failure modes directly through shared coordination module (Invoke-P09VerificationSession):
# 1. Backup failure -> launch refused (exit 1, Launched = False)
# 2. Launch failure -> handled safely, recovery guaranteed in finally
# 3. Crash / Run exception -> recovery executed in finally
# 4. Compare diff detected -> rejected, never reports PASS
# 5. RestoreFail + ComparePass -> rejected (must FAIL, persistence failure)
# 6. Timeout + CompareFail -> reported as persistence failure (exit 1), not masked as timeout (exit 2)

param(
    [string]$ProjectRoot = "E:\code\TLTD"
)

$ErrorActionPreference = "Stop"
$logFile = "$ProjectRoot\Tools\Verification\P09\wrapper_failure_path_test.log"
$sandboxDir = "$ProjectRoot\scratch\.wrapper_failure_sandbox"
$disposableRegKey = "Software\Unity\UnityEditor\DefaultCompany\TLTD_WrapperFailTest"
$saveGuardScript = "$ProjectRoot\Tools\Verification\P09\tltd_save_guard.ps1"
$coreScript = "$ProjectRoot\Tools\Verification\P09\P09_VerificationCore.ps1"

Remove-Item -Force -ErrorAction SilentlyContinue $logFile
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $sandboxDir
New-Item -ItemType Directory -Path $sandboxDir -Force | Out-Null

function Log([string]$msg) {
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
    $line = "[$timestamp] $msg"
    Write-Host $line
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

Log "============================================================"
Log " STARTING P09 WRAPPER FAILURE PATH VERIFICATION"
Log " Coordination Core: $coreScript"
Log " Save Guard:       $saveGuardScript"
Log " Sandbox Dir:      $sandboxDir"
Log " Disposable Reg:   $disposableRegKey"
Log "============================================================"

. $coreScript

$allPassed = $true

try {
    # ------------------------------------------------------------------
    # Setup Sandbox Registry Key
    # ------------------------------------------------------------------
    & reg.exe add "HKCU\$disposableRegKey" /v "SandboxToken" /t REG_SZ /d "InitialState_123" /f | Out-Null

    # ------------------------------------------------------------------
    # Test 1: Backup failure -> launch refused (exit 1, Launched = False)
    # ------------------------------------------------------------------
    Log "[TEST 1] Testing Backup Failure path..."
    $t1BackupDir = "$sandboxDir\t1_backup"
    $res1 = Invoke-P09VerificationSession `
        -SuiteName "Test1: Backup Failure" `
        -ProjectRoot $ProjectRoot `
        -ExecutablePath "powershell.exe" `
        -ArgumentList @("-NoProfile", "-Command", "Start-Sleep -Milliseconds 100") `
        -BackupDir $t1BackupDir `
        -CustomRegKey $disposableRegKey `
        -AllowRunningUnity $true `
        -FailureInjection @{ FailBackup = $true }

    $t1Pass = ($res1.ExitCode -eq 1) -and ($res1.Launched -eq $false) -and ($res1.Status -eq "BACKUP_FAILED")
    if ($t1Pass) {
        Log "[TEST 1 PASS] Backup failure correctly halted launch: Launched=$($res1.Launched), ExitCode=$($res1.ExitCode), Status=$($res1.Status)"
    } else {
        Log "[TEST 1 FAIL] Expected launch to be refused on backup failure: Launched=$($res1.Launched), ExitCode=$($res1.ExitCode)"
        $allPassed = $false
    }

    # ------------------------------------------------------------------
    # Test 2: Launch failure -> handled safely, recovery in finally
    # ------------------------------------------------------------------
    Log "[TEST 2] Testing Launch Failure path..."
    $t2BackupDir = "$sandboxDir\t2_backup"
    $res2 = Invoke-P09VerificationSession `
        -SuiteName "Test2: Launch Failure" `
        -ProjectRoot $ProjectRoot `
        -ExecutablePath "powershell.exe" `
        -ArgumentList @("-NoProfile", "-Command", "Start-Sleep -Milliseconds 100") `
        -BackupDir $t2BackupDir `
        -CustomRegKey $disposableRegKey `
        -AllowRunningUnity $true `
        -FailureInjection @{ FailLaunch = $true }

    $t2Pass = ($res2.LaunchFailed -eq $true) -and ($res2.SaveRestored -eq $true) -and ($res2.SaveDiffZero -eq $true) -and ($res2.ExitCode -ne 0)
    if ($t2Pass) {
        Log "[TEST 2 PASS] Launch failure safely handled with guaranteed recovery: LaunchFailed=$($res2.LaunchFailed), Restored=$($res2.SaveRestored), DiffZero=$($res2.SaveDiffZero), ExitCode=$($res2.ExitCode)"
    } else {
        Log "[TEST 2 FAIL] Launch failure did not properly recover: LaunchFailed=$($res2.LaunchFailed), Restored=$($res2.SaveRestored), DiffZero=$($res2.SaveDiffZero)"
        $allPassed = $false
    }

    # ------------------------------------------------------------------
    # Test 3: Crash / Run exception -> recovery executed in finally
    # ------------------------------------------------------------------
    Log "[TEST 3] Testing Crash / Run Exception path..."
    $t3BackupDir = "$sandboxDir\t3_backup"
    $res3 = Invoke-P09VerificationSession `
        -SuiteName "Test3: Crash Recovery" `
        -ProjectRoot $ProjectRoot `
        -ExecutablePath "powershell.exe" `
        -ArgumentList @("-NoProfile", "-Command", "Start-Sleep -Milliseconds 100") `
        -BackupDir $t3BackupDir `
        -CustomRegKey $disposableRegKey `
        -AllowRunningUnity $true `
        -FailureInjection @{ FailCrash = $true }

    $t3Pass = ($res3.SaveRestored -eq $true) -and ($res3.SaveDiffZero -eq $true) -and ($res3.ExitCode -ne 0)
    if ($t3Pass) {
        Log "[TEST 3 PASS] Crash during execution safely recovered: Restored=$($res3.SaveRestored), DiffZero=$($res3.SaveDiffZero), ExitCode=$($res3.ExitCode)"
    } else {
        Log "[TEST 3 FAIL] Crash recovery failed: Restored=$($res3.SaveRestored), DiffZero=$($res3.SaveDiffZero)"
        $allPassed = $false
    }

    # ------------------------------------------------------------------
    # Test 4: Compare diff detected -> rejected, never reports PASS
    # ------------------------------------------------------------------
    Log "[TEST 4] Testing Compare Diff Rejection path..."
    $t4BackupDir = "$sandboxDir\t4_backup"
    $res4 = Invoke-P09VerificationSession `
        -SuiteName "Test4: Compare Diff Rejection" `
        -ProjectRoot $ProjectRoot `
        -ExecutablePath "powershell.exe" `
        -ArgumentList @("-NoProfile", "-Command", "Start-Sleep -Milliseconds 100") `
        -BackupDir $t4BackupDir `
        -CustomRegKey $disposableRegKey `
        -AllowRunningUnity $true `
        -FailureInjection @{ FailCompare = $true; ForceScenarioPass = $true }

    $t4Pass = ($res4.SaveDiffZero -eq $false) -and ($res4.ExitCode -eq 1) -and ($res4.Status -eq "PERSISTENCE_FAILURE")
    if ($t4Pass) {
        Log "[TEST 4 PASS] Compare mismatch strictly rejected PASS: DiffZero=$($res4.SaveDiffZero), ExitCode=$($res4.ExitCode), Status=$($res4.Status)"
    } else {
        Log "[TEST 4 FAIL] Compare mismatch was not rejected: DiffZero=$($res4.SaveDiffZero), ExitCode=$($res4.ExitCode), Status=$($res4.Status)"
        $allPassed = $false
    }

    # ------------------------------------------------------------------
    # Test 5: RestoreFail + ComparePass -> rejected (must FAIL, exit 1)
    # ------------------------------------------------------------------
    Log "[TEST 5] Testing RestoreFail + ComparePass rejection path..."
    $t5BackupDir = "$sandboxDir\t5_backup"
    $res5 = Invoke-P09VerificationSession `
        -SuiteName "Test5: RestoreFail Rejection" `
        -ProjectRoot $ProjectRoot `
        -ExecutablePath "powershell.exe" `
        -ArgumentList @("-NoProfile", "-Command", "Start-Sleep -Milliseconds 100") `
        -BackupDir $t5BackupDir `
        -CustomRegKey $disposableRegKey `
        -AllowRunningUnity $true `
        -FailureInjection @{ FailRestore = $true; ForceScenarioPass = $true }

    $t5Pass = ($res5.SaveRestored -eq $false) -and ($res5.ExitCode -eq 1) -and ($res5.Status -eq "PERSISTENCE_FAILURE")
    if ($t5Pass) {
        Log "[TEST 5 PASS] RestoreFail + ComparePass strictly rejected PASS: Restored=$($res5.SaveRestored), ExitCode=$($res5.ExitCode), Status=$($res5.Status)"
    } else {
        Log "[TEST 5 FAIL] RestoreFail was not rejected: Restored=$($res5.SaveRestored), ExitCode=$($res5.ExitCode), Status=$($res5.Status)"
        $allPassed = $false
    }

    # ------------------------------------------------------------------
    # Test 6: Timeout + CompareFail -> reported as persistence failure (exit 1), NOT masked as timeout (exit 2)
    # ------------------------------------------------------------------
    Log "[TEST 6] Testing Timeout + CompareFail persistence failure path..."
    $t6BackupDir = "$sandboxDir\t6_backup"
    $res6 = Invoke-P09VerificationSession `
        -SuiteName "Test6: Timeout + CompareFail" `
        -ProjectRoot $ProjectRoot `
        -ExecutablePath "powershell.exe" `
        -ArgumentList @("-NoProfile", "-Command", "Start-Sleep -Milliseconds 100") `
        -BackupDir $t6BackupDir `
        -TimeoutSeconds 1 `
        -CustomRegKey $disposableRegKey `
        -AllowRunningUnity $true `
        -FailureInjection @{ FailTimeout = $true; FailCompare = $true; ForceScenarioPass = $true }

    $t6Pass = ($res6.TimedOut -eq $true) -and ($res6.SaveDiffZero -eq $false) -and ($res6.ExitCode -eq 1) -and ($res6.Status -eq "PERSISTENCE_FAILURE")
    if ($t6Pass) {
        Log "[TEST 6 PASS] Timeout + CompareFail correctly reported as PERSISTENCE_FAILURE (Exit 1, not exit 2): TimedOut=$($res6.TimedOut), DiffZero=$($res6.SaveDiffZero), ExitCode=$($res6.ExitCode), Status=$($res6.Status)"
    } else {
        Log "[TEST 6 FAIL] Timeout + CompareFail incorrectly evaluated: TimedOut=$($res6.TimedOut), DiffZero=$($res6.SaveDiffZero), ExitCode=$($res6.ExitCode), Status=$($res6.Status)"
        $allPassed = $false
    }
}
finally {
    # Teardown disposable fixtures
    & cmd.exe /c "reg delete `"HKCU\$disposableRegKey`" /f >nul 2>&1"
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $sandboxDir
}

Log "============================================================"
Log " WRAPPER FAILURE PATH VERIFICATION RESULT: $(if ($allPassed) { 'ALL 6 TESTS PASSED' } else { 'FAIL' })"
Log "============================================================"

if ($allPassed) { exit 0 } else { exit 1 }
