$ErrorActionPreference = "Stop"

$projectRoot = "E:\code\TLTD"
$unityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe"
$logFile = "$projectRoot\natural_playmode_p09b.log"
$wrapperLog = "$projectRoot\natural_playmode_p09b_wrapper.log"
$saveGuardScript = "$projectRoot\Tools\Verification\P09\tltd_save_guard.ps1"
$coreScript = "$projectRoot\Tools\Verification\P09\P09_VerificationCore.ps1"
$backupDir = "$projectRoot\scratch\.save_backup_natural_playmode_p09b"
$timeoutSeconds = 300

. $coreScript

$argsList = @(
    "-batchmode",
    "-projectPath", "$projectRoot",
    "-executeMethod", "WuxiaGame.Editor.Prototype01PlayTestRunner_P09B.RunPlayModeNatural_P09B_CLI",
    "-logFile", "$logFile"
)

$sessionResult = Invoke-P09VerificationSession `
    -SuiteName "P09-B Real Natural Play Mode Dash Scenario" `
    -ProjectRoot $projectRoot `
    -ExecutablePath $unityPath `
    -ArgumentList $argsList `
    -LogFile $logFile `
    -WrapperLog $wrapperLog `
    -BackupDir $backupDir `
    -TimeoutSeconds $timeoutSeconds `
    -SaveGuardScript $saveGuardScript `
    -PassPattern "ALL 6 SEGMENTS PASSED NATURALLY!" `
    -SummaryPatterns @("P09-B NATURAL", "NATURAL SEGMENT", "Frame", "RESULT", "ERROR")

exit $sessionResult.ExitCode
