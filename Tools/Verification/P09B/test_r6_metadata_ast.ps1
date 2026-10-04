# ==============================================================================
# TEST SCRIPT: test_r6_metadata_ast.ps1
# PURPOSE: Independent unit verification of Get-T2WrapperMetadata via AST extraction
# RULES:
#   - Does NOT dot-source test_manual_launcher_r6.ps1 (avoids top-level side effects).
#   - Uses PowerShell AST (FunctionDefinitionAst) to extract Get-T2WrapperMetadata
#     directly from production source.
#   - Verifies ParseFile errors == 0 and records source SHA256.
#   - No Unity, no Registry mutation, no child process launch.
#   - Explicitly records scope limitation for TC6/TC7 (helper test vs orchestration).
# ==============================================================================

param(
    [string]$ProductionScript = 'E:\code\TLTD\Tools\Verification\P09B\test_manual_launcher_r6.ps1',
    [string]$RealT2Log = 'E:\code\TLTD\scratch\launcher_r6_artifacts\normal\t2_wrapper.log'
)

$ErrorActionPreference = 'Stop'

$scriptBytes = [System.IO.File]::ReadAllBytes($ProductionScript)
$scriptHash  = (Get-FileHash -Path $ProductionScript -Algorithm SHA256).Hash

Write-Host '=================================================================='
Write-Host ' METADATA_ONLY_VERIFICATION — AST Unit Test for Get-T2WrapperMetadata'
Write-Host " Timestamp:         $((Get-Date).ToString('o'))"
Write-Host " Script Under Test: $ProductionScript"
Write-Host " Script Size:       $($scriptBytes.Length) bytes"
Write-Host " Script SHA256:     $scriptHash"
Write-Host ' Extraction:        PowerShell AST (FunctionDefinitionAst) - Zero Dot-Source'
Write-Host '=================================================================='

# --- 1. Parse File via AST ---
$tokens = $null
$parseErrors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile($ProductionScript, [ref]$tokens, [ref]$parseErrors)

if ($parseErrors.Count -gt 0) {
    Write-Error "AST ParseFile encountered $($parseErrors.Count) errors!"
    exit 1
}

$funcDef = $ast.FindAll({
    param($node)
    $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq 'Get-T2WrapperMetadata'
}, $true) | Select-Object -First 1

if ($null -eq $funcDef) {
    Write-Error "Function 'Get-T2WrapperMetadata' not found in AST!"
    exit 1
}

$funcLen = $funcDef.Extent.Text.Length
Write-Host "[AST] FunctionDefinitionAst extracted successfully: length = $funcLen characters."
Invoke-Expression $funcDef.Extent.Text

if (-not (Get-Command 'Get-T2WrapperMetadata' -ErrorAction SilentlyContinue)) {
    Write-Error 'Failed to define Get-T2WrapperMetadata via Invoke-Expression!'
    exit 1
}
Write-Host '[AST] Function Get-T2WrapperMetadata loaded into session.'
Write-Host ''

# --- Test Tracking ---
$totalCases = 7
$passedTests = 0
$totalAssertions = 0
$passedAssertions = 0

function Assert-Equal {
    param(
        [string]$testName,
        [string]$assertionDesc,
        [object]$actual,
        [object]$expected
    )
    $script:totalAssertions++
    if ($actual -eq $expected) {
        $script:passedAssertions++
        Write-Host "    [ASSERT PASS] $assertionDesc : '$actual'" -ForegroundColor Green
        return $true
    } else {
        Write-Host "    [ASSERT FAIL] $assertionDesc : Expected '$expected', got '$actual'" -ForegroundColor Red
        return $false
    }
}

# Setup isolated temp dir for fixtures
$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) ('TLTD_R6_MetaTest_' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

try {
    # --------------------------------------------------------------------------
    # TC1: Real t2 wrapper log (FailRestore case)
    # --------------------------------------------------------------------------
    Write-Host '[TC1] Real t2 log parsing from live artifact...'
    $tc1Pass = $true
    if (Test-Path $RealT2Log) {
        $meta1 = Get-T2WrapperMetadata $RealT2Log
        $a1 = Assert-Equal -testName 'TC1' -assertionDesc 'Restore Result' -actual $meta1.Restore -expected 'FAILED'
        $a2 = Assert-Equal -testName 'TC1' -assertionDesc 'Compare Result' -actual $meta1.Compare -expected 'SUCCESS (0 Diffs verified)'
        $a3 = Assert-Equal -testName 'TC1' -assertionDesc 'Session Result' -actual $meta1.Session -expected 'PERSISTENCE_FAILURE'
        $a4 = Assert-Equal -testName 'TC1' -assertionDesc 'Wrapper Exit'  -actual $meta1.WrapperExit -expected '3'
        $tc1Pass = $a1 -and $a2 -and $a3 -and $a4
    } else {
        Write-Host "    [SKIP] Real t2 log not found at $RealT2Log" -ForegroundColor Yellow
        $tc1Pass = $false
    }
    if ($tc1Pass) { $passedTests++; Write-Host '  -> TC1 PASS' -ForegroundColor Green } else { Write-Host '  -> TC1 FAIL' -ForegroundColor Red }

    # --------------------------------------------------------------------------
    # TC2: Non-existent file path
    # --------------------------------------------------------------------------
    Write-Host '[TC2] Non-existent log file path...'
    $nonExistentPath = Join-Path $tempDir 'missing_log_file.log'
    $meta2 = Get-T2WrapperMetadata $nonExistentPath
    $a1 = Assert-Equal -testName 'TC2' -assertionDesc 'Source'      -actual $meta2.Source      -expected 'NOT_RECORDED'
    $a2 = Assert-Equal -testName 'TC2' -assertionDesc 'Restore'     -actual $meta2.Restore     -expected 'NOT_RECORDED'
    $a3 = Assert-Equal -testName 'TC2' -assertionDesc 'Compare'     -actual $meta2.Compare     -expected 'NOT_RECORDED'
    $a4 = Assert-Equal -testName 'TC2' -assertionDesc 'Session'     -actual $meta2.Session     -expected 'NOT_RECORDED'
    $a5 = Assert-Equal -testName 'TC2' -assertionDesc 'WrapperExit' -actual $meta2.WrapperExit -expected 'NOT_RECORDED'
    $tc2Pass = $a1 -and $a2 -and $a3 -and $a4 -and $a5
    if ($tc2Pass) { $passedTests++; Write-Host '  -> TC2 PASS' -ForegroundColor Green } else { Write-Host '  -> TC2 FAIL' -ForegroundColor Red }

    # --------------------------------------------------------------------------
    # TC3: Empty log file (0 bytes)
    # --------------------------------------------------------------------------
    Write-Host '[TC3] Empty log file (zero bytes)...'
    $emptyFile = Join-Path $tempDir 'empty_wrapper.log'
    New-Item -ItemType File -Path $emptyFile -Force | Out-Null
    $meta3 = Get-T2WrapperMetadata $emptyFile
    $a1 = Assert-Equal -testName 'TC3' -assertionDesc 'Restore'   -actual $meta3.Restore   -expected 'NOT_RECORDED'
    $a2 = Assert-Equal -testName 'TC3' -assertionDesc 'Compare'   -actual $meta3.Compare   -expected 'NOT_RECORDED'
    $a3 = Assert-Equal -testName 'TC3' -assertionDesc 'ParseNote' -actual $meta3.ParseNote -expected 'Log file empty'
    $tc3Pass = $a1 -and $a2 -and $a3
    if ($tc3Pass) { $passedTests++; Write-Host '  -> TC3 PASS' -ForegroundColor Green } else { Write-Host '  -> TC3 FAIL' -ForegroundColor Red }

    # --------------------------------------------------------------------------
    # TC4: Blank field value with immediate newline (No cross-line bleed)
    # --------------------------------------------------------------------------
    Write-Host '[TC4] Blank field value, testing no cross-line bleed...'
    $blankFieldFile = Join-Path $tempDir 'blank_field.log'
    $content4 = "Restore Result:   `r`nCompare Result:   SUCCESS`r`nSession Result:   PASS`r`n"
    [System.IO.File]::WriteAllText($blankFieldFile, $content4, [System.Text.Encoding]::UTF8)
    $meta4 = Get-T2WrapperMetadata $blankFieldFile
    $a1 = Assert-Equal -testName 'TC4' -assertionDesc 'Blank Restore becomes NOT_RECORDED' -actual $meta4.Restore -expected 'NOT_RECORDED'
    $a2 = Assert-Equal -testName 'TC4' -assertionDesc 'Subsequent Compare parsed correctly' -actual $meta4.Compare -expected 'SUCCESS'
    $tc4Pass = $a1 -and $a2
    if ($tc4Pass) { $passedTests++; Write-Host '  -> TC4 PASS' -ForegroundColor Green } else { Write-Host '  -> TC4 FAIL' -ForegroundColor Red }

    # --------------------------------------------------------------------------
    # TC5: Null/empty path - verify NO fallback to stale artifact
    # --------------------------------------------------------------------------
    Write-Host '[TC5] Empty string path, verify no fallback to stale artifacts...'
    $meta5 = Get-T2WrapperMetadata ''
    $a1 = Assert-Equal -testName 'TC5' -assertionDesc 'Empty path returns Source=NOT_RECORDED'  -actual $meta5.Source  -expected 'NOT_RECORDED'
    $a2 = Assert-Equal -testName 'TC5' -assertionDesc 'Empty path returns Restore=NOT_RECORDED' -actual $meta5.Restore -expected 'NOT_RECORDED'
    $tc5Pass = $a1 -and $a2
    if ($tc5Pass) { $passedTests++; Write-Host '  -> TC5 PASS' -ForegroundColor Green } else { Write-Host '  -> TC5 FAIL' -ForegroundColor Red }

    # --------------------------------------------------------------------------
    # TC6: t2 attempted in run (Helper parsing evaluation)
    # Scope limitation note: TC6 tests helper parsing when t2 path is supplied.
    # The runner $t2Ran orchestration branch dispatch is evaluated in script logic.
    # --------------------------------------------------------------------------
    Write-Host '[TC6] t2 attempted: Verify helper parsing of simulated run artifact...'
    $t2SimLog = Join-Path $tempDir 't2_sim.log'
    $content6 = "Restore Result:   FAILED`r`nCompare Result:   SUCCESS`r`nSession Result:   PERSISTENCE_FAILURE`r`nWrapper Exit:     3`r`n"
    [System.IO.File]::WriteAllText($t2SimLog, $content6, [System.Text.Encoding]::UTF8)
    $meta6 = Get-T2WrapperMetadata $t2SimLog
    $a1 = Assert-Equal -testName 'TC6' -assertionDesc 'Helper parses attempted t2 log (Restore=FAILED)' -actual $meta6.Restore -expected 'FAILED'
    $tc6Pass = $a1
    if ($tc6Pass) { $passedTests++; Write-Host '  -> TC6 PASS' -ForegroundColor Green } else { Write-Host '  -> TC6 FAIL' -ForegroundColor Red }

    # --------------------------------------------------------------------------
    # TC7: t2 not attempted (Helper parsing evaluation with absent log)
    # Scope limitation note: Runner else branch yields NOT_RECORDED when $t2Ran is false.
    # Helper called with null/absent path returns NOT_RECORDED.
    # --------------------------------------------------------------------------
    Write-Host '[TC7] t2 not attempted: Verify helper returns NOT_RECORDED...'
    $meta7 = Get-T2WrapperMetadata $null
    $a1 = Assert-Equal -testName 'TC7' -assertionDesc 'Null path returns Restore=NOT_RECORDED' -actual $meta7.Restore -expected 'NOT_RECORDED'
    $tc7Pass = $a1
    if ($tc7Pass) { $passedTests++; Write-Host '  -> TC7 PASS' -ForegroundColor Green } else { Write-Host '  -> TC7 FAIL' -ForegroundColor Red }

} finally {
    Remove-Item -Recurse -Force $tempDir -ErrorAction SilentlyContinue
}

Write-Host ''
Write-Host '=================================================================='
Write-Host ' TEST EXECUTION SUMMARY'
Write-Host " Test Cases:  $passedTests / $totalCases PASS"
Write-Host " Assertions:  $passedAssertions / $totalAssertions PASS"
Write-Host ' Extraction:  PowerShell AST (FunctionDefinitionAst)'
Write-Host " SUT SHA256:  $scriptHash"
Write-Host " SUT Size:    $($scriptBytes.Length) bytes"
$resultSummary = if ($passedAssertions -eq $totalAssertions -and $passedTests -eq $totalCases) { 'ALL PASS' } else { 'FAIL' }
Write-Host " RESULT:      $resultSummary"
Write-Host '=================================================================='
Write-Host ' SCOPE LIMITATION NOTE FOR TC6 / TC7:'
Write-Host ' - TC1..TC5 rigorously verify parser extraction, regex safety against'
Write-Host '   cross-line bleed, whitespace normalization, and no stale artifact fallback.'
Write-Host ' - TC6 and TC7 verify Get-T2WrapperMetadata behavior for attempted vs unattempted log inputs.'
Write-Host ' - The orchestrator dispatch branch ($t2Ran -> $t2WrapperPath) in test_manual_launcher_r6.ps1'
Write-Host '   was reviewed statically (A3 FIX VERIFIED) and is NOT executed here because this test suite'
Write-Host '   strictly isolates helper evaluation without launching child Unity/Registry processes.'
Write-Host '=================================================================='

if ($passedAssertions -ne $totalAssertions -or $passedTests -ne $totalCases) {
    exit 1
} else {
    exit 0
}
