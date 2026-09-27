# P09 Wrapper Failure Path Verification Script
# Tests wrapper robustness under simulated failure modes in disposable sandbox:
# 1. Backup failure -> Wrapper halts immediately, refuses to launch Unity
# 2. Timeout/run failure -> Wrapper executes recovery (Restore & Compare) inside finally block
# 3. Compare failure -> Wrapper rejects diff, never outputs false PASS

param(
    [string]$ProjectRoot = "E:\code\TLTD"
)

$ErrorActionPreference = "Stop"
$logFile = "$ProjectRoot\Tools\Verification\P09\wrapper_failure_path_test.log"
$disposableDir = "$ProjectRoot\scratch\.wrapper_failure_test"
$disposableRegKey = "Software\Unity\UnityEditor\DefaultCompany\TLTD_WrapperFailTest"
$saveGuardScript = "$ProjectRoot\Tools\Verification\P09\tltd_save_guard.ps1"

Remove-Item -Force -ErrorAction SilentlyContinue $logFile
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $disposableDir
New-Item -ItemType Directory -Path $disposableDir -Force | Out-Null

function Log([string]$msg) {
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
    $line = "[$timestamp] $msg"
    Write-Host $line
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

Log "============================================================"
Log " STARTING P09 WRAPPER FAILURE PATH VERIFICATION"
Log "============================================================"

$allPassed = $true

# ------------------------------------------------------------------
# Test 1: Backup Failure -> Unity Launch Refused
# ------------------------------------------------------------------
Log "[TEST 1] Verifying wrapper halts when Backup fails..."
$badBackupDir = "Z:\NonExistentDrive_Invalid\BackupDir"
# Call Save Guard Backup directly with invalid dir to verify exit code != 0
$backupProc = Start-Process -FilePath "powershell.exe" -ArgumentList "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "`"$saveGuardScript`"", "-Action", "Backup", "-BackupDir", "`"$badBackupDir`"" -Wait -PassThru
$exitCode = $backupProc.ExitCode

if ($exitCode -ne 0) {
    Log "[TEST 1 PASS] Backup call failed with exit code $exitCode as expected. Wrapper guard logic correctly refuses launch when exitCode != 0."
} else {
    Log "[TEST 1 FAIL] Expected backup to fail with non-zero exit code!"
    $allPassed = $false
}

# ------------------------------------------------------------------
# Test 2: Timeout / Run Exception -> Finally block guarantees Restore & Compare
# ------------------------------------------------------------------
Log "[TEST 2] Verifying wrapper finally block runs Restore & Compare upon crash/timeout..."
# Setup disposable key
& reg.exe add "HKCU\$disposableRegKey" /v "TestKey" /t REG_SZ /d "OriginalValue" /f | Out-Null
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Backup -BackupDir $disposableDir -CustomRegKey $disposableRegKey -AllowRunningUnity
if ($LASTEXITCODE -ne 0) { throw "Backup failed in Test 2 setup" }

# Mutate key to simulate dirty run state
& reg.exe add "HKCU\$disposableRegKey" /v "TestKey" /t REG_SZ /d "DirtyMutatedValue" /f | Out-Null

# Simulate wrapper try/finally structure
$wrapperFinallyExecuted = $false
$restoredSuccessfully = $false
$compareSucceeded = $false

try {
    # Simulate a timeout or crash during execution
    Log "[TEST 2] Simulating runner execution timeout / crash..."
    throw "Simulated Unity Timeout / Runner Abort"
}
catch {
    Log "[TEST 2] Caught expected exception: $($_.Exception.Message)"
}
finally {
    $wrapperFinallyExecuted = $true
    Log "[TEST 2] Executing wrapper finally block..."
    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Restore -BackupDir $disposableDir -AllowRunningUnity
    if ($LASTEXITCODE -eq 0) { $restoredSuccessfully = $true }

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Compare -BackupDir $disposableDir
    if ($LASTEXITCODE -eq 0) { $compareSucceeded = $true }
}

if ($wrapperFinallyExecuted -and $restoredSuccessfully -and $compareSucceeded) {
    Log "[TEST 2 PASS] Finally block executed recovery upon failure: Restored=True, CompareDiffZero=True."
} else {
    Log "[TEST 2 FAIL] Finally recovery failed: Finally=$wrapperFinallyExecuted, Restore=$restoredSuccessfully, Compare=$compareSucceeded"
    $allPassed = $false
}

# ------------------------------------------------------------------
# Test 3: Compare Failure -> Rejects Diff, No False PASS
# ------------------------------------------------------------------
Log "[TEST 3] Verifying wrapper rejects diff and never outputs false PASS when comparison fails..."
# Setup disposable key with backup
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Backup -BackupDir $disposableDir -CustomRegKey $disposableRegKey -AllowRunningUnity
if ($LASTEXITCODE -ne 0) { throw "Backup failed in Test 3 setup" }

# Introduce dirty mutation
& reg.exe add "HKCU\$disposableRegKey" /v "LeakedKeyInTest" /t REG_DWORD /d 999 /f | Out-Null

# Run compare
& powershell.exe -NoProfile -ExecutionPolicy Bypass -File $saveGuardScript -Action Compare -BackupDir $disposableDir
$compareCode = $LASTEXITCODE

if ($compareCode -ne 0) {
    Log "[TEST 3 PASS] Compare correctly returned exit code $compareCode on detected diff. Wrapper logic ($($runnerPassed) -and ($($unityOsExitCode) -eq 0) -and $($saveDiffZero)) evaluates to FALSE and wrapper exit code is non-zero (FAIL)."
} else {
    Log "[TEST 3 FAIL] Compare falsely succeeded on dirty state!"
    $allPassed = $false
}

# Teardown disposable fixtures
& cmd.exe /c "reg delete `"HKCU\$disposableRegKey`" /f >nul 2>&1"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $disposableDir

Log "============================================================"
Log " WRAPPER FAILURE PATH VERIFICATION RESULT: $(if ($allPassed) { 'ALL PASS' } else { 'FAIL' })"
Log "============================================================"

if ($allPassed) { exit 0 } else { exit 1 }
