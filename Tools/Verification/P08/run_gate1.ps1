$projectRoot = "E:\code\TLTD"
$unityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe"
$logFile = "$projectRoot\gate1_p08_tests.log"

Remove-Item -Force -ErrorAction SilentlyContinue $logFile

Write-Host "============================================================"
Write-Host " Running Gate 1: P08 Automated Tests (T01 - T49)"
Write-Host " Unity Path: $unityPath"
Write-Host " Log File:   $logFile"
Write-Host "============================================================"

$proc = Start-Process -FilePath $unityPath -ArgumentList "-batchmode", "-quit", "-projectPath", "`"$projectRoot`"", "-executeMethod", "WuxiaGame.Editor.Prototype01PlayTestRunner_P08.RunGate1_P08Tests_CLI", "-logFile", "`"$logFile`"" -PassThru -Wait

$exitCode = $proc.ExitCode
Write-Host "Unity Process Exited with Code: $exitCode"

if (Test-Path $logFile) {
    Write-Host "=== LOG SUMMARY ==="
    Get-Content $logFile | Select-String "P08 AUTOMATED TESTS RESULT", "PASS:", "FAIL:", "TEST EXCEPTION" | ForEach-Object { Write-Host $_ }
} else {
    Write-Host "WARNING: Log file $logFile was not found!"
}

exit $exitCode
