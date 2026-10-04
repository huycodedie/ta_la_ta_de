$ErrorActionPreference = "Stop"

$projectRoot = "E:\code\TLTD"
$unityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe"
$logFile = "$projectRoot\scratch\autonomy_test.log"
$wrapperLog = "$projectRoot\scratch\autonomy_wrapper.log"
$saveGuardScript = "$projectRoot\Tools\Verification\P09\tltd_save_guard.ps1"
$coreScript = "$projectRoot\Tools\Verification\P09\P09_VerificationCore.ps1"
$backupDir = "$projectRoot\scratch\.save_backup_autonomy"
$timeoutSeconds = 300

. $coreScript

$argsList = @(
    "-batchmode",
    "-projectPath", "$projectRoot",
    "-p09ManualSession",
    "-executeMethod", "WuxiaGame.Editor.P09ManualAutonomyVerifier.RunAutonomyVerification_CLI",
    "-logFile", "$logFile"
)

$sessionResult = Invoke-P09VerificationSession `
    -SuiteName "F-MANUAL-AUTONOMY Natural Frame Play Mode Verification" `
    -ProjectRoot $projectRoot `
    -ExecutablePath $unityPath `
    -ArgumentList $argsList `
    -LogFile $logFile `
    -WrapperLog $wrapperLog `
    -BackupDir $backupDir `
    -TimeoutSeconds $timeoutSeconds `
    -SaveGuardScript $saveGuardScript `
    -PassPattern "\[AUTONOMY VERIFIER RESULT\]: ALL 4 CHECKS PASSED" `
    -SummaryPatterns @("AUTONOMY VERIFIER", "CHECK 1", "CHECK 2", "CHECK 3", "CHECK 4", "FIXTURE_READY", "VIOLATION", "RESULT")

exit $sessionResult.ExitCode
