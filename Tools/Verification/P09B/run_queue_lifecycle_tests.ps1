$ErrorActionPreference = "Stop"

$projectRoot = "E:\code\TLTD"
$unityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe"
$logFile = "$projectRoot\scratch\queue_lifecycle_p09b.log"
$wrapperLog = "$projectRoot\scratch\queue_lifecycle_p09b_wrapper.log"
$saveGuardScript = "$projectRoot\Tools\Verification\P09\tltd_save_guard.ps1"
$coreScript = "$projectRoot\Tools\Verification\P09\P09_VerificationCore.ps1"
$backupDir = "$projectRoot\scratch\.save_backup_queue_lifecycle_p09b"
$timeoutSeconds = 300

. $coreScript

$argsList = @(
    "-batchmode",
    "-projectPath", "$projectRoot",
    "-p09bManualSession",
    "-executeMethod", "WuxiaGame.Editor.P09BQueueLifecycleRunner.RunQueueLifecycleTests_CLI",
    "-logFile", "$logFile"
)

$sessionResult = Invoke-P09VerificationSession `
    -SuiteName "P09-B Queue & Lifecycle Verification Suite (G1 & G2)" `
    -ProjectRoot $projectRoot `
    -ExecutablePath $unityPath `
    -ArgumentList $argsList `
    -LogFile $logFile `
    -WrapperLog $wrapperLog `
    -BackupDir $backupDir `
    -TimeoutSeconds $timeoutSeconds `
    -SaveGuardScript $saveGuardScript `
    -PassPattern "ALL 5 QUEUE TESTS PASSED NATURALLY!" `
    -SummaryPatterns @("QUEUE TEST Q", "QUEUE ERROR", "QUEUE LIFECYCLE TESTS RESULT")

exit $sessionResult.ExitCode
