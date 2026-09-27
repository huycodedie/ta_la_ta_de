$ErrorActionPreference = "Stop"

$projectRoot = "E:\code\TLTD"
$unityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe"
$logFile = "$projectRoot\gate1_p09_tests.log"
$wrapperLog = "$projectRoot\gate1_p09_wrapper.log"
$saveGuardScript = "$projectRoot\Tools\Verification\P09\tltd_save_guard.ps1"
$coreScript = "$projectRoot\Tools\Verification\P09\P09_VerificationCore.ps1"
$backupDir = "$projectRoot\scratch\.save_backup_gate1_p09"
$timeoutSeconds = 150

. $coreScript

$argsList = @(
    "-batchmode",
    "-quit",
    "-projectPath", "$projectRoot",
    "-executeMethod", "WuxiaGame.Editor.Prototype01PlayTestRunner_P09.RunGate1_P09_CLI",
    "-logFile", "$logFile"
)

$sessionResult = Invoke-P09VerificationSession `
    -SuiteName "Gate 1: P09-A Projectile Foundation Tests (T01 - T21)" `
    -ProjectRoot $projectRoot `
    -ExecutablePath $unityPath `
    -ArgumentList $argsList `
    -LogFile $logFile `
    -WrapperLog $wrapperLog `
    -BackupDir $backupDir `
    -TimeoutSeconds $timeoutSeconds `
    -SaveGuardScript $saveGuardScript `
    -PassPattern "ALL_PASS=True" `
    -SummaryPatterns @("P09 AUTOMATED TESTS RESULT", "\[T", "FAIL", "EXCEPTION")

exit $sessionResult.ExitCode
