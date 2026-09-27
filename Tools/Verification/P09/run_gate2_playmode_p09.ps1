$ErrorActionPreference = "Stop"

$projectRoot = "E:\code\TLTD"
$unityPath = "E:\Unity Hub\editor\6000.6.0f1\Editor\Unity.exe"
$logFile = "$projectRoot\gate2_p09_playmode.log"
$wrapperLog = "$projectRoot\gate2_p09_wrapper.log"
$saveGuardScript = "$projectRoot\Tools\Verification\P09\tltd_save_guard.ps1"
$coreScript = "$projectRoot\Tools\Verification\P09\P09_VerificationCore.ps1"
$backupDir = "$projectRoot\scratch\.save_backup_gate2_p09"
$timeoutSeconds = 90

. $coreScript

$argsList = @(
    "-batchmode",
    "-projectPath", "$projectRoot",
    "-executeMethod", "WuxiaGame.Editor.Prototype01PlayTestRunner_P09.RunP09PlayModeScenarioMenu",
    "-logFile", "$logFile"
)

$sessionResult = Invoke-P09VerificationSession `
    -SuiteName "Gate 2: P09-A Play Mode Natural Frames Scenario" `
    -ProjectRoot $projectRoot `
    -ExecutablePath $unityPath `
    -ArgumentList $argsList `
    -LogFile $logFile `
    -WrapperLog $wrapperLog `
    -BackupDir $backupDir `
    -TimeoutSeconds $timeoutSeconds `
    -SaveGuardScript $saveGuardScript `
    -PassPattern "\[P09 PLAY MODE SCENARIO RESULT\]: PASSED" `
    -SummaryPatterns @("P09 PLAY MODE", "RESULT", "Release Frame", "Arrival Frame", "ERROR")

exit $sessionResult.ExitCode
