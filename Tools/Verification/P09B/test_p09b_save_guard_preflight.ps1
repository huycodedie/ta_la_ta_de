# ==============================================================================
# test_p09b_save_guard_preflight.ps1
# P09-B Save Guard Preflight Verification Test Suite
#
# Tests both:
# 1. Direct unit tests for Test-P09BJournalPreflight
# 2. End-to-end sandbox tests against ACTUAL launch_manual_p09b_session.ps1
#
# Requirements verified:
# - All 3 unresolved statuses (BACKED_UP, RESTORE_IN_PROGRESS, RESTORE_FAILED) are blocked.
# - Malformed JSON, missing status, or unrecognized status are safely blocked.
# - Missing journal and VERIFIED journal proceed to expected steps without skipping guards.
# - Blocked branches prove: zero CheckInterruptedJournal/Backup/Restore calls,
#   zero Unity launch, journal byte-unchanged, nonzero exit, correct error message.
# - Isolated sandbox: zero mutation to real Registry, zero real Unity launch.
# ==============================================================================

$ErrorActionPreference = "Stop"

$repoRoot = "E:\code\TLTD"
$actualLauncher = "$repoRoot\Tools\Verification\P09B\launch_manual_p09b_session.ps1"
$actualPreflight = "$repoRoot\Tools\Verification\P09B\P09B_PreflightGuard.ps1"
$actualCore = "$repoRoot\Tools\Verification\P09\P09_VerificationCore.ps1"

Write-Host "=================================================================="
Write-Host " P09-B PREFLIGHT GUARD VERIFICATION TEST SUITE"
Write-Host " Launcher:   $actualLauncher"
Write-Host " Preflight:  $actualPreflight"
Write-Host " Time:       $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host "=================================================================="

# Hash source files for audit record
$launcherHash = (Get-FileHash $actualLauncher -Algorithm SHA256).Hash
$preflightHash = (Get-FileHash $actualPreflight -Algorithm SHA256).Hash
Write-Host " Launcher SHA256:  $launcherHash"
Write-Host " Preflight SHA256: $preflightHash"
Write-Host ""

. $actualPreflight

# ------------------------------------------------------------------------------
# PART 1: Direct Unit Tests
# ------------------------------------------------------------------------------
Write-Host "--- PART 1: DIRECT UNIT TESTS OF Test-P09BJournalPreflight ---"

$unitSandbox = "$repoRoot\scratch\test_preflight_unit_sandbox"
if (Test-Path $unitSandbox) { Remove-Item -Recurse -Force $unitSandbox }
New-Item -ItemType Directory -Path $unitSandbox -Force | Out-Null

$unitResults = [System.Collections.Generic.List[object]]::new()

function Run-UnitTest(
    [string]$testName,
    [string]$content,
    [bool]$expectedAllowed,
    [string]$expectedStatus,
    [string]$expectedReasonSnippet,
    [switch]$NoJournal
) {
    $dir = Join-Path $unitSandbox $testName
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
    $jPath = Join-Path $dir "journal.json"
    if (-not $NoJournal) {
        [System.IO.File]::WriteAllText($jPath, $content, [System.Text.Encoding]::UTF8)
    }

    $res = Test-P09BJournalPreflight -BackupDir $dir
    $statusMatch = ($res.Status -eq $expectedStatus) -or ($null -eq $res.Status -and [string]::IsNullOrEmpty($expectedStatus))
    $pass = ($res.Allowed -eq $expectedAllowed) -and $statusMatch -and ($res.Reason -like "*$expectedReasonSnippet*")
    
    $statusStr = if ($pass) { "PASS" } else { "FAIL" }
    Write-Host "  [$statusStr] $testName : Allowed=$($res.Allowed), Status=$($res.Status)"
    if (-not $pass) {
        Write-Warning "    Expected Allowed=$expectedAllowed, Status=$expectedStatus, Snippet=$expectedReasonSnippet"
        Write-Warning "    Actual: Allowed=$($res.Allowed), Status=$($res.Status), Reason=$($res.Reason)"
    }

    $unitResults.Add([ordered]@{
        TestName = $testName
        Passed = $pass
        Allowed = $res.Allowed
        Status = $res.Status
        Reason = $res.Reason
    })
}

Run-UnitTest "U1_Status_BACKED_UP" '{"Status":"BACKED_UP","Timestamp":"2026-10-03"}' $false "BACKED_UP" "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL"
Run-UnitTest "U2_Status_RESTORE_IN_PROGRESS" '{"Status":"RESTORE_IN_PROGRESS"}' $false "RESTORE_IN_PROGRESS" "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL"
Run-UnitTest "U3_Status_RESTORE_FAILED" '{"Status":"RESTORE_FAILED"}' $false "RESTORE_FAILED" "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL"
Run-UnitTest "U4_Malformed_JSON" '{Status:"BACKED_UP", broken}' $false "MALFORMED" "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL"
Run-UnitTest "U5_Missing_Status" '{"Timestamp":"2026-10-03"}' $false "MISSING_STATUS" "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL"
Run-UnitTest "U6_Unrecognized_Status" '{"Status":"SOME_RANDOM_STATUS"}' $false "SOME_RANDOM_STATUS" "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL"
Run-UnitTest "U7_No_Journal" "" $true $null "NO_JOURNAL" -NoJournal
Run-UnitTest "U8_Status_VERIFIED" '{"Status":"VERIFIED","RestoredTimestamp":"2026-10-03"}' $true "VERIFIED" "VERIFIED_JOURNAL"

$unitPassedCount = ($unitResults | Where-Object { $_.Passed }).Count
Write-Host "Part 1 Result: $unitPassedCount / $($unitResults.Count) unit tests passed."
Write-Host ""

# ------------------------------------------------------------------------------
# PART 2: Integration Tests on ACTUAL launch_manual_p09b_session.ps1
# ------------------------------------------------------------------------------
Write-Host "--- PART 2: SANDBOX INTEGRATION TESTS ON ACTUAL LAUNCHER ---"

$integSandbox = "$repoRoot\scratch\test_preflight_integ_sandbox"
if (Test-Path $integSandbox) { Remove-Item -Recurse -Force $integSandbox }
New-Item -ItemType Directory -Path $integSandbox -Force | Out-Null

# Setup mock project structure in sandbox
$sbToolsP09 = Join-Path $integSandbox "Tools\Verification\P09"
$sbToolsP09B = Join-Path $integSandbox "Tools\Verification\P09B"
New-Item -ItemType Directory -Path $sbToolsP09 -Force | Out-Null
New-Item -ItemType Directory -Path $sbToolsP09B -Force | Out-Null

# Copy actual VerificationCore and actual PreflightGuard
Copy-Item $actualCore -Destination (Join-Path $sbToolsP09 "P09_VerificationCore.ps1") -Force
Copy-Item $actualPreflight -Destination (Join-Path $sbToolsP09B "P09B_PreflightGuard.ps1") -Force

# Create stub Save Guard with sentinel tracking
$stubSaveGuard = Join-Path $sbToolsP09 "tltd_save_guard.ps1"
$stubSaveGuardCode = @'
param(
    [string]$Action,
    [string]$BackupDir,
    [string]$CustomRegKey,
    [switch]$AllowRunningUnity
)
$sentinelFile = Join-Path $BackupDir "sentinel_saveguard.log"
$line = "[$([DateTime]::Now.ToString('HH:mm:ss.fff'))] ACTION_CALLED: $Action"
Add-Content -LiteralPath $sentinelFile -Value $line -Encoding UTF8
exit 0
'@
Set-Content -Path $stubSaveGuard -Value $stubSaveGuardCode -Encoding UTF8

# Create stub fake_unity.bat
$fakeUnityBat = Join-Path $integSandbox "fake_unity.bat"
$fakeUnityBatCode = "@echo off`r`necho UNITY_LAUNCHED >> `"%~dp0sentinel_unity.log`"`r`nexit /b 0`r`n"
[System.IO.File]::WriteAllText($fakeUnityBat, $fakeUnityBatCode, [System.Text.Encoding]::ASCII)

$integResults = [System.Collections.Generic.List[object]]::new()

function Run-LauncherTest(
    [string]$caseId,
    [string]$caseDesc,
    [string]$journalContent,
    [bool]$shouldBlock,
    [switch]$NoJournal
) {
    $caseDir = Join-Path $integSandbox $caseId
    New-Item -ItemType Directory -Path $caseDir -Force | Out-Null

    $backupDir = Join-Path $caseDir "backup"
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null

    $jPath = Join-Path $backupDir "journal.json"
    $hashBefore = $null
    if (-not $NoJournal) {
        [System.IO.File]::WriteAllText($jPath, $journalContent, [System.Text.Encoding]::UTF8)
        $hashBefore = (Get-FileHash $jPath -Algorithm SHA256).Hash
    }

    $caseLog = Join-Path $caseDir "wrapper.log"
    $sentinelSG = Join-Path $backupDir "sentinel_saveguard.log"
    $sentinelUnity = Join-Path $integSandbox "sentinel_unity.log"
    if (Test-Path $sentinelUnity) { Remove-Item -Force $sentinelUnity }

    # Command to run actual launcher as child process via -Command to avoid parameter token splitting
    $cmd = "& '$actualLauncher' -ProjectRoot '$integSandbox' -BackupDir '$backupDir' -WrapperLog '$caseLog' -UnityPath '$fakeUnityBat' -AllowRunningUnity"

    $proc = Start-Process -FilePath "powershell.exe" -ArgumentList @("-NoProfile", "-ExecutionPolicy", "Bypass", "-Command", $cmd) -PassThru -NoNewWindow -Wait
    $exitCode = $proc.ExitCode

    $logOutput = if (Test-Path $caseLog) { Get-Content $caseLog -Raw } else { "" }

    # Hash check after run
    $hashAfter = if (Test-Path $jPath) { (Get-FileHash $jPath -Algorithm SHA256).Hash } else { $null }
    $hashUnchanged = ($null -eq $hashBefore -and $null -eq $hashAfter) -or ($hashBefore -eq $hashAfter)

    # Sentinel counts
    $sgCalls = if (Test-Path $sentinelSG) { @(Get-Content $sentinelSG) } else { @() }
    $unityCalls = if (Test-Path $sentinelUnity) { @(Get-Content $sentinelUnity) } else { @() }

    $testPassed = $false
    $failureReasons = [System.Collections.Generic.List[string]]::new()

    if ($shouldBlock) {
        # Requirements for BLOCKED branches:
        # 1. Nonzero exit code
        if ($exitCode -eq 0) { $failureReasons.Add("Expected nonzero exit code, got 0") }
        # 2. Output contains PRECHECK_BLOCKED_UNRESOLVED_JOURNAL
        if ($logOutput -notlike "*PRECHECK_BLOCKED_UNRESOLVED_JOURNAL*") { $failureReasons.Add("Output missing PRECHECK_BLOCKED_UNRESOLVED_JOURNAL") }
        # 3. Zero Save Guard calls
        if ($sgCalls.Count -ne 0) { $failureReasons.Add("Expected 0 Save Guard calls, found $($sgCalls.Count): $($sgCalls -join '; ')") }
        # 4. Zero Unity launches
        if ($unityCalls.Count -ne 0) { $failureReasons.Add("Expected 0 Unity launches, found $($unityCalls.Count)") }
        # 5. Journal byte-unchanged
        if (-not $hashUnchanged) { $failureReasons.Add("Journal was mutated! Before=$hashBefore, After=$hashAfter") }

        $testPassed = ($failureReasons.Count -eq 0)
    } else {
        # Requirements for ALLOWED branches:
        # 1. Exit code 0
        if ($exitCode -ne 0) { $failureReasons.Add("Expected exit code 0, got $exitCode") }
        # 2. Save Guard executed CheckInterruptedJournal, Backup, Restore, Compare
        $hasCheck = ($sgCalls | Where-Object { $_ -like "*CheckInterruptedJournal*" }).Count -gt 0
        $hasBackup = ($sgCalls | Where-Object { $_ -like "*Backup*" }).Count -gt 0
        $hasRestore = ($sgCalls | Where-Object { $_ -like "*Restore*" }).Count -gt 0
        $hasCompare = ($sgCalls | Where-Object { $_ -like "*Compare*" }).Count -gt 0
        if (-not $hasCheck) { $failureReasons.Add("Missing CheckInterruptedJournal call") }
        if (-not $hasBackup) { $failureReasons.Add("Missing Backup call") }
        if (-not $hasRestore) { $failureReasons.Add("Missing Restore call") }
        if (-not $hasCompare) { $failureReasons.Add("Missing Compare call") }
        # 3. Unity stub launched
        if ($unityCalls.Count -eq 0) { $failureReasons.Add("Expected Unity stub launch, none recorded") }

        $testPassed = ($failureReasons.Count -eq 0)
    }

    $passLabel = if ($testPassed) { "PASS" } else { "FAIL" }
    Write-Host "  [$passLabel] ${caseId}: $caseDesc (ExitCode: $exitCode, SG Calls: $($sgCalls.Count), Unity: $($unityCalls.Count))"
    if (-not $testPassed) {
        foreach ($r in $failureReasons) {
            Write-Warning "    Failure: $r"
        }
    }

    $integResults.Add([ordered]@{
        CaseId = $caseId
        Description = $caseDesc
        ShouldBlock = $shouldBlock
        ExitCode = $exitCode
        SGCalls = $sgCalls.Count
        UnityCalls = $unityCalls.Count
        JournalUnchanged = $hashUnchanged
        Passed = $testPassed
        FailureReasons = @($failureReasons)
    })
}

# Run the 8 test scenarios
Run-LauncherTest "I1_BACKED_UP" "Status BACKED_UP blocked" '{"Status":"BACKED_UP","Timestamp":"2026-10-03"}' $true
Run-LauncherTest "I2_RESTORE_IN_PROGRESS" "Status RESTORE_IN_PROGRESS blocked" '{"Status":"RESTORE_IN_PROGRESS","Timestamp":"2026-10-03"}' $true
Run-LauncherTest "I3_RESTORE_FAILED" "Status RESTORE_FAILED blocked" '{"Status":"RESTORE_FAILED","Timestamp":"2026-10-03"}' $true
Run-LauncherTest "I4_MALFORMED_JSON" "Malformed JSON blocked" '{bad_json: missing_quotes}' $true
Run-LauncherTest "I5_MISSING_STATUS" "Missing Status field blocked" '{"Timestamp":"2026-10-03"}' $true
Run-LauncherTest "I6_UNKNOWN_STATUS" "Unrecognized Status blocked" '{"Status":"CUSTOM_UNKNOWN_STATUS"}' $true
Run-LauncherTest "I7_NO_JOURNAL" "No journal proceeds to stub" "" $false -NoJournal
Run-LauncherTest "I8_VERIFIED_JOURNAL" "Status VERIFIED proceeds to stub" '{"Status":"VERIFIED","RestoredTimestamp":"2026-10-03"}' $false

$integPassedCount = ($integResults | Where-Object { $_.Passed }).Count
Write-Host ""
Write-Host "Part 2 Result: $integPassedCount / $($integResults.Count) integration tests passed."
Write-Host "=================================================================="

# Export raw test summary
$testReport = [ordered]@{
    Timestamp = (Get-Date -Format 'yyyy-MM-ddTHH:mm:ss.fffzzz')
    LauncherPath = $actualLauncher
    LauncherSHA256 = $launcherHash
    PreflightPath = $actualPreflight
    PreflightSHA256 = $preflightHash
    UnitTests = [ordered]@{
        Total = $unitResults.Count
        Passed = $unitPassedCount
        Results = $unitResults
    }
    IntegrationTests = [ordered]@{
        Total = $integResults.Count
        Passed = $integPassedCount
        Results = $integResults
    }
    AllPassed = ($unitPassedCount -eq $unitResults.Count -and $integPassedCount -eq $integResults.Count)
}

$reportPath = "$repoRoot\scratch\PREFLIGHT_TEST_RESULTS.json"
$testReport | ConvertTo-Json -Depth 6 | Set-Content -Path $reportPath -Encoding UTF8
Write-Host "Results saved to: $reportPath"
Write-Host "All tests passed: $($testReport.AllPassed)"

if (-not $testReport.AllPassed) {
    exit 1
}
