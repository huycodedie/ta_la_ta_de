# P09-B Manual Launcher R6 Verification Suite
# Verifies all failure and classification paths of launch_manual_p09b_session.ps1:
# 1. Non-zero OS exit (exit 42) -> Session=UNITY_OS_FAILURE, Wrapper Exit=1 (Save Guard Diff=0 maintained)
# 2. Persistence failure (Restore failure) -> Session=PERSISTENCE_FAILURE, Wrapper Exit=3 (Persistence failure takes precedence)
# 3. Happy path (exit 0) -> Session=SUCCESS, Wrapper Exit=0

param(
    [string]$ProjectRoot = "E:\code\TLTD",
    [switch]$InjectFailure
)

$ErrorActionPreference = "Stop"
$runMode = if ($InjectFailure) { "injected" } else { "normal" }
$logFile = "$ProjectRoot\scratch\launcher_r6_verification_$runMode.log"
$sandboxDir = "$ProjectRoot\scratch\.launcher_r6_sandbox_$runMode"
$rawArtifactsDir = "$ProjectRoot\scratch\launcher_r6_artifacts\$runMode"
$disposableRegKey = "Software\Unity\UnityEditor\DefaultCompany\TLTD_LauncherR6_Sandbox_$runMode"
$launcherScript = "$ProjectRoot\Tools\Verification\P09B\launch_manual_p09b_session.ps1"

if (-not (Test-Path "$ProjectRoot\scratch")) {
    New-Item -ItemType Directory -Path "$ProjectRoot\scratch" -Force | Out-Null
}
if (-not (Test-Path $rawArtifactsDir)) {
    New-Item -ItemType Directory -Path $rawArtifactsDir -Force | Out-Null
}
Remove-Item -Force -ErrorAction SilentlyContinue $logFile
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $sandboxDir
New-Item -ItemType Directory -Path $sandboxDir -Force | Out-Null

function Log([string]$msg, [ConsoleColor]$color = [ConsoleColor]::White) {
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss.fff")
    $line = "[$timestamp] $msg"
    Write-Host $line -ForegroundColor $color
    Add-Content -Path $logFile -Value $line -Encoding UTF8
}

# ---------------------------------------------------------------------------
# Get-T2WrapperMetadata
# Reads Restore/Compare/Session/WrapperExit from a wrapper log file path.
# RULES:
#   - Only reads values on the SAME physical line as the label (regex [^\r\n]+
#     ensures no cross-line bleed even when content contains CRLF).
#   - Returns a Hashtable with keys: Source, Restore, Compare, Session,
#     WrapperExit.  Missing/blank values are NOT_RECORDED.
#   - Does NOT fall back to any file other than the one explicitly passed.
#   - Does NOT infer values from pass-counts or exit codes.
# This function is defined at top-level so unit tests can dot-source and
# call it directly without running the orchestration block.
# ---------------------------------------------------------------------------
function Get-T2WrapperMetadata([string]$wrapperLogPath) {
    $NOT_RECORDED = 'NOT_RECORDED'

    # Determine evidence source and load raw text
    if ([string]::IsNullOrWhiteSpace($wrapperLogPath) -or -not (Test-Path $wrapperLogPath)) {
        return [ordered]@{
            Source      = $NOT_RECORDED
            Restore     = $NOT_RECORDED
            Compare     = $NOT_RECORDED
            Session     = $NOT_RECORDED
            WrapperExit = $NOT_RECORDED
            ParseNote   = 'Log file absent or path not provided'
        }
    }

    $rawText = Get-Content $wrapperLogPath -Raw -ErrorAction SilentlyContinue
    if ([string]::IsNullOrEmpty($rawText)) {
        return [ordered]@{
            Source      = $wrapperLogPath
            Restore     = $NOT_RECORDED
            Compare     = $NOT_RECORDED
            Session     = $NOT_RECORDED
            WrapperExit = $NOT_RECORDED
            ParseNote   = 'Log file empty'
        }
    }

    # Parse a labelled field: match only on the SAME physical line as the label.
    # Pattern: label + optional spaces + captured value up to end-of-line.
    # [^\r\n]+ prevents bleeding onto subsequent lines.
    function ParseField([string]$text, [string]$label) {
        $escaped = [regex]::Escape($label)
        $pattern = $escaped + '[ \t]*([^\r\n]+)'
        $m = [regex]::Match($text, $pattern)
        if (-not $m.Success) { return $null }
        $val = $m.Groups[1].Value.Trim()
        if ([string]::IsNullOrWhiteSpace($val)) { return $null }
        return $val
    }

    $restore     = ParseField $rawText 'Restore Result:'
    $compare     = ParseField $rawText 'Compare Result:'
    $session     = ParseField $rawText 'Session Result:'
    $wrapperExit = ParseField $rawText 'Wrapper Exit:'

    return [ordered]@{
        Source      = $wrapperLogPath
        Restore     = if ($null -ne $restore)     { $restore }     else { $NOT_RECORDED }
        Compare     = if ($null -ne $compare)     { $compare }     else { $NOT_RECORDED }
        Session     = if ($null -ne $session)     { $session }     else { $NOT_RECORDED }
        WrapperExit = if ($null -ne $wrapperExit) { $wrapperExit } else { $NOT_RECORDED }
        ParseNote   = 'Parsed from live run log'
    }
}

Log "============================================================" -color Cyan
Log " STARTING P09-B MANUAL LAUNCHER R6 VERIFICATION" -color Cyan
Log " Launcher Script:   $launcherScript" -color Cyan
Log " Sandbox Dir:       $sandboxDir" -color Cyan
Log " Raw Artifacts Dir: $rawArtifactsDir" -color Cyan
Log " Disposable Reg:    $disposableRegKey" -color Cyan
if ($InjectFailure) {
    Log " INJECT FAILURE:   ENABLED (Deliberate Orchestration Failure)" -color Yellow
}
Log "============================================================" -color Cyan

$allPassed = $true
$completedCases = 0
$expectedCases = 3
$orchestrationError = $null
# Track t2 execution independently of pass-count
$t2Ran         = $false
$t2WrapperPath = $null   # Sandbox path of t2 wrapper log (set when t2 starts)

try {
    # Initialize sandbox registry token
    & reg.exe add "HKCU\$disposableRegKey" /v "SandboxToken" /t REG_SZ /d "R6_Init_Token_123" /f | Out-Null

    # ------------------------------------------------------------------
    # Test 1: Non-zero OS Exit (exit 42) -> UNITY_OS_FAILURE (Exit 1)
    # ------------------------------------------------------------------
    Log ""
    Log "[TEST 1] Testing Non-zero OS Exit classification..." -color Cyan
    $t1BackupDir = "$sandboxDir\t1_backup"
    $t1Log = "$sandboxDir\t1_unity.log"
    $t1WrapperLog = "$sandboxDir\t1_wrapper.log"

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$launcherScript" `
        -ProjectRoot "$ProjectRoot" `
        -UnityPath "cmd.exe" `
        -UnityExtraArgs "/c exit 42" `
        -SessionTimeoutSeconds 10 `
        -BackupDir "$t1BackupDir" `
        -LogFile "$t1Log" `
        -WrapperLog "$t1WrapperLog" `
        -CustomRegKey "$disposableRegKey" `
        -AllowRunningUnity
    $t1Exit = $LASTEXITCODE

    $t1Content = if (Test-Path $t1WrapperLog) { Get-Content $t1WrapperLog -Raw } else { "" }
    $t1StatusOk = $t1Content -like "*Session Result:*UNITY_OS_FAILURE*"
    $t1ExitOk = ($t1Exit -eq 1)

    if ($t1StatusOk -and $t1ExitOk) {
        Log "[TEST 1 PASS] Non-zero OS exit classified as UNITY_OS_FAILURE with Wrapper Exit 1." -color Green
        $completedCases++
    } else {
        Log "[TEST 1 FAIL] Expected UNITY_OS_FAILURE (Exit 1), got Exit=$t1Exit, StatusOk=$t1StatusOk" -color Red
        $allPassed = $false
    }

    # Runner-level Failure Injection: prove aborting before all cases completes fails cleanly
    if ($InjectFailure) {
        throw "Deliberate runner-level orchestration failure injected before case completion (-InjectFailure active)!"
    }

    # ------------------------------------------------------------------
    # Test 2: Persistence Failure takes precedence over OS exit -> Exit 3
    # ------------------------------------------------------------------
    Log ""
    Log "[TEST 2] Testing Persistence Failure precedence..." -color Cyan
    $t2BackupDir  = "$sandboxDir\t2_backup"
    $t2Log        = "$sandboxDir\t2_unity.log"
    $t2WrapperLog = "$sandboxDir\t2_wrapper.log"
    # Record sandbox path BEFORE launching so metadata can find it even if
    # the process fails to create the log (in which case the file won't exist
    # and Get-T2WrapperMetadata will correctly return NOT_RECORDED).
    $t2WrapperPath = $t2WrapperLog
    $t2Ran         = $true   # Tracks that t2 was ATTEMPTED this run

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$launcherScript" `
        -ProjectRoot "$ProjectRoot" `
        -UnityPath "cmd.exe" `
        -UnityExtraArgs "/c exit 0" `
        -SessionTimeoutSeconds 10 `
        -BackupDir "$t2BackupDir" `
        -LogFile "$t2Log" `
        -WrapperLog "$t2WrapperLog" `
        -CustomRegKey "$disposableRegKey" `
        -AllowRunningUnity `
        -FailureInjection "FailRestore"
    $t2Exit = $LASTEXITCODE

    $t2Content = if (Test-Path $t2WrapperLog) { Get-Content $t2WrapperLog -Raw } else { "" }
    $t2StatusOk = $t2Content -like "*Session Result:*PERSISTENCE_FAILURE*"
    $t2ExitOk = ($t2Exit -eq 3)

    if ($t2StatusOk -and $t2ExitOk) {
        Log "[TEST 2 PASS] Persistence failure correctly prioritized as PERSISTENCE_FAILURE with Wrapper Exit 3." -color Green
        $completedCases++
    } else {
        Log "[TEST 2 FAIL] Expected PERSISTENCE_FAILURE (Exit 3), got Exit=$t2Exit, StatusOk=$t2StatusOk" -color Red
        $allPassed = $false
    }

    # ------------------------------------------------------------------
    # Test 3: Happy Path (Exit 0) -> SUCCESS (Exit 0)
    # ------------------------------------------------------------------
    Log ""
    Log "[TEST 3] Testing Happy Path execution..." -color Cyan
    $t3BackupDir = "$sandboxDir\t3_backup"
    $t3Log = "$sandboxDir\t3_unity.log"
    $t3WrapperLog = "$sandboxDir\t3_wrapper.log"

    & powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$launcherScript" `
        -ProjectRoot "$ProjectRoot" `
        -UnityPath "cmd.exe" `
        -UnityExtraArgs "/c exit 0" `
        -SessionTimeoutSeconds 10 `
        -BackupDir "$t3BackupDir" `
        -LogFile "$t3Log" `
        -WrapperLog "$t3WrapperLog" `
        -CustomRegKey "$disposableRegKey" `
        -AllowRunningUnity
    $t3Exit = $LASTEXITCODE

    $t3Content = if (Test-Path $t3WrapperLog) { Get-Content $t3WrapperLog -Raw } else { "" }
    $t3StatusOk = $t3Content -like "*Session Result:*SUCCESS*"
    $t3ExitOk = ($t3Exit -eq 0)

    if ($t3StatusOk -and $t3ExitOk) {
        Log "[TEST 3 PASS] Happy path cleanly succeeded with SUCCESS and Wrapper Exit 0." -color Green
        $completedCases++
    } else {
        Log "[TEST 3 FAIL] Expected SUCCESS (Exit 0), got Exit=$t3Exit, StatusOk=$t3StatusOk" -color Red
        $allPassed = $false
    }
}
catch {
    $orchestrationError = $_
    Log "[ORCHESTRATION ERROR] Terminating exception caught at runner level: $_" -color Red
    $allPassed = $false
}
finally {
    # 1. Read t2 metadata from sandbox BEFORE deleting sandbox.
    #    Uses Get-T2WrapperMetadata defined at top-level (not from rawArtifactsDir
    #    which may contain a stale log from a previous run).
    #    $t2Ran tracks whether t2 was ATTEMPTED in THIS run, independently of
    #    $completedCases (which only increments on PASS).
    $failRestoreDetails = $null
    if ($t2Ran -and $null -ne $t2WrapperPath) {
        $t2Meta = Get-T2WrapperMetadata $t2WrapperPath
        Log "[METADATA] t2 FailRestore parsed: Restore=$($t2Meta.Restore), Compare=$($t2Meta.Compare), Session=$($t2Meta.Session), WrapperExit=$($t2Meta.WrapperExit)" -color Cyan
        $failRestoreDetails = $t2Meta
    } elseif ($InjectFailure) {
        $failRestoreDetails = [ordered]@{
            Source      = 'NOT_RECORDED (InjectFailure aborted before t2)'
            Restore     = 'NOT_RECORDED'
            Compare     = 'NOT_RECORDED'
            Session     = 'NOT_RECORDED'
            WrapperExit = 'NOT_RECORDED'
            ParseNote   = 'InjectFailure active; t2 never reached'
        }
    } else {
        $failRestoreDetails = [ordered]@{
            Source      = 'NOT_RECORDED'
            Restore     = 'NOT_RECORDED'
            Compare     = 'NOT_RECORDED'
            Session     = 'NOT_RECORDED'
            WrapperExit = 'NOT_RECORDED'
            ParseNote   = 't2 not attempted in this run'
        }
    }

    # 2. Preserve raw child-wrapper logs outside sandbox before sandbox deletion
    try {
        if (Test-Path $sandboxDir) {
            Get-ChildItem -Path $sandboxDir -Filter "*_wrapper.log" -File -ErrorAction SilentlyContinue | ForEach-Object {
                Copy-Item -Path $_.FullName -Destination "$rawArtifactsDir\$($_.Name)" -Force -ErrorAction SilentlyContinue
            }
            Get-ChildItem -Path $sandboxDir -Filter "*_unity.log" -File -ErrorAction SilentlyContinue | ForEach-Object {
                Copy-Item -Path $_.FullName -Destination "$rawArtifactsDir\$($_.Name)" -Force -ErrorAction SilentlyContinue
            }

            $meta = [ordered]@{
                RunMode            = $runMode
                CommandLine        = "powershell -ExecutionPolicy Bypass -File Tools/Verification/P09B/test_manual_launcher_r6.ps1 $(if ($InjectFailure) { '-InjectFailure' })"
                OuterExitCode      = if ($allPassed -and ($completedCases -eq $expectedCases) -and ($null -eq $orchestrationError)) { 0 } else { 1 }
                CompletedCases     = $completedCases
                ExpectedCases      = $expectedCases
                OrchestrationError = if ($orchestrationError) { "$orchestrationError" } else { $null }
                AllPassed          = $allPassed
                T2Ran              = $t2Ran
                FailRestoreDetails = $failRestoreDetails
                Timestamp          = (Get-Date).ToString('o')
            }
            $meta | ConvertTo-Json -Depth 6 | Set-Content -Path "$rawArtifactsDir\run_metadata.json" -Encoding UTF8
        }
    } catch {
        Log "[CLEANUP WARNING] Failed to preserve raw child logs: $_" -color Yellow
    }

    # 2. Teardown sandbox registry token
    & reg.exe delete "HKCU\$disposableRegKey" /f 2>$null | Out-Null
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $sandboxDir

    Log ""
    Log "============================================================" -color Cyan
    $isCompleteSuccess = ($allPassed -and ($completedCases -eq $expectedCases) -and ($null -eq $orchestrationError))
    if ($isCompleteSuccess) {
        Log " P09-B MANUAL LAUNCHER R6 VERIFICATION RESULT: ALL $completedCases/$expectedCases TESTS PASSED" -color Green
    } else {
        Log " P09-B MANUAL LAUNCHER R6 VERIFICATION RESULT: FAILURES DETECTED (Completed $completedCases/$expectedCases Cases, Error=$orchestrationError)" -color Red
    }
    Log "============================================================" -color Cyan
}

# 3. Controlled exit decision made AFTER finally block, preserving original error status
if ($allPassed -and ($completedCases -eq $expectedCases) -and ($null -eq $orchestrationError)) {
    exit 0
} else {
    exit 1
}
