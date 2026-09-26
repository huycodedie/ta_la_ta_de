# TLTD Save Guard & Recovery Module (Corrected V2)
# Durable persistence protection with full data comparison, pre-restore hash verification,
# atomic journaling, active process checking, and real process failure-path verification.

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Backup", "Restore", "Compare", "VerifyRecovery", "CheckInterruptedJournal")]
    [string]$Action = "CheckInterruptedJournal",

    [Parameter(Mandatory=$false)]
    [string]$BackupDir = "E:\code\TLTD\scratch\.save_backup",

    [Parameter(Mandatory=$false)]
    [string]$CustomRegKey = "Software\Unity\UnityEditor\DefaultCompany\TLTD",

    [Parameter(Mandatory=$false)]
    [switch]$AllowRunningUnity = $false
)

$ErrorActionPreference = "Stop"

function Assert-NoConflictingUnityProcess([switch]$allow) {
    if ($allow) { return }
    $unityProcs = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
    if ($null -ne $unityProcs -and $unityProcs.Count -gt 0) {
        $pids = ($unityProcs | ForEach-Object { "$($_.Id)" }) -join ", "
        throw "[SAVE GUARD FATAL] Unity process(es) currently running (PID: $pids). Operations cannot safely proceed while Unity holds or mutates PlayerPrefs!"
    }
}

function Safe-DeleteRegistryKey([string]$fullKeyPath, [string]$subKeyPath) {
    $k = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($subKeyPath, $false)
    if ($null -ne $k) {
        $k.Close()
        & cmd.exe /c "reg delete `"$fullKeyPath`" /f >nul 2>&1"
    }
}

function Get-RegistryScopeSnapshot([string]$subKeyPath) {
    $rootKey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($subKeyPath, $false)
    if ($null -eq $rootKey) {
        return @{
            Exists = $false
            SubKeyPath = $subKeyPath
            ValueCount = 0
            Values = @{}
            SubKeyCount = 0
            SubKeys = @{}
        }
    }

    try {
        function Get-NodeSnapshot([Microsoft.Win32.RegistryKey]$k) {
            $valueNames = $k.GetValueNames()
            $valuesMap = @{}
            foreach ($vName in $valueNames) {
                $kind = $k.GetValueKind($vName)
                $rawVal = $k.GetValue($vName, $null, [Microsoft.Win32.RegistryValueOptions]::DoNotExpandEnvironmentNames)
                $dataRep = $null

                switch ($kind) {
                    ([Microsoft.Win32.RegistryValueKind]::Binary) {
                        $bytes = [byte[]]$rawVal
                        $dataRep = if ($bytes) { [BitConverter]::ToString($bytes) } else { "" }
                    }
                    ([Microsoft.Win32.RegistryValueKind]::MultiString) {
                        $strArr = [string[]]$rawVal
                        $dataRep = if ($strArr) { [string[]]$strArr } else { @() }
                    }
                    ([Microsoft.Win32.RegistryValueKind]::DWord) {
                        $dataRep = [long]$rawVal
                    }
                    ([Microsoft.Win32.RegistryValueKind]::QWord) {
                        $dataRep = [long]$rawVal
                    }
                    Default {
                        $dataRep = if ($rawVal -ne $null) { [string]$rawVal } else { "" }
                    }
                }

                $valuesMap[$vName] = @{
                    Name = $vName
                    Kind = $kind.ToString()
                    Data = $dataRep
                }
            }

            $subKeyNames = $k.GetSubKeyNames()
            $subKeysMap = @{}
            foreach ($skName in $subKeyNames) {
                $childKey = $k.OpenSubKey($skName, $false)
                if ($null -ne $childKey) {
                    try {
                        $subKeysMap[$skName] = Get-NodeSnapshot $childKey
                    } finally {
                        $childKey.Close()
                    }
                }
            }

            return @{
                ValueCount = $valueNames.Count
                Values = $valuesMap
                SubKeyCount = $subKeyNames.Count
                SubKeys = $subKeysMap
            }
        }

        $rootNode = Get-NodeSnapshot $rootKey
        return @{
            Exists = $true
            SubKeyPath = $subKeyPath
            ValueCount = $rootNode.ValueCount
            Values = $rootNode.Values
            SubKeyCount = $rootNode.SubKeyCount
            SubKeys = $rootNode.SubKeys
        }
    }
    finally {
        $rootKey.Close()
    }
}

function Get-SnapshotValueNames($valuesObj) {
    if ($null -eq $valuesObj) { return @() }
    if ($valuesObj -is [System.Collections.IDictionary]) {
        return @($valuesObj.Keys)
    }
    return @($valuesObj.PSObject.Properties | Select-Object -ExpandProperty Name)
}

function Get-SnapshotValueEntry($valuesObj, [string]$propName) {
    if ($null -eq $valuesObj) { return $null }
    if ($valuesObj -is [System.Collections.IDictionary]) {
        if ($valuesObj.Contains($propName)) { return $valuesObj[$propName] }
        return $null
    }
    $p = $valuesObj.PSObject.Properties[$propName]
    if ($null -ne $p) { return $p.Value }
    return $null
}

function Compare-RegistryNodeEntries($baseNode, $currNode, [string]$path, [ref]$diffMessage) {
    if ($null -eq $baseNode -or $null -eq $currNode) {
        $diffMessage.Value = "Node null mismatch at '$path'"
        return $false
    }

    if ($baseNode.ValueCount -ne $currNode.ValueCount) {
        $diffMessage.Value = "Value count mismatch at '$path': Baseline=$($baseNode.ValueCount), Current=$($currNode.ValueCount)"
        return $false
    }

    $baseNames = Get-SnapshotValueNames $baseNode.Values
    $currNames = Get-SnapshotValueNames $currNode.Values

    $currNamesSet = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::Ordinal)
    foreach ($cn in $currNames) { $currNamesSet.Add($cn) | Out-Null }

    foreach ($name in $baseNames) {
        if (-not $currNamesSet.Contains($name)) {
            $diffMessage.Value = "Value missing in current snapshot at '$path': '$name'"
            return $false
        }

        $baseEntry = Get-SnapshotValueEntry $baseNode.Values $name
        $currEntry = Get-SnapshotValueEntry $currNode.Values $name

        if (-not [string]::Equals($baseEntry.Kind, $currEntry.Kind, [System.StringComparison]::Ordinal)) {
            $diffMessage.Value = "Value '$name' at '$path' kind mismatch: Baseline=$($baseEntry.Kind), Current=$($currEntry.Kind)"
            return $false
        }

        if ($baseEntry.Kind -eq "MultiString") {
            $arr1 = [string[]]$baseEntry.Data
            $arr2 = [string[]]$currEntry.Data
            if ($arr1.Length -ne $arr2.Length) {
                $diffMessage.Value = "Value '$name' at '$path' MultiString length mismatch: Baseline=$($arr1.Length), Current=$($arr2.Length)"
                return $false
            }
            for ($i = 0; $i -lt $arr1.Length; $i++) {
                if (-not [string]::Equals($arr1[$i], $arr2[$i], [System.StringComparison]::Ordinal)) {
                    $diffMessage.Value = "Value '$name' at '$path' MultiString element at index $i mismatch: '$($arr1[$i])' vs '$($arr2[$i])'"
                    return $false
                }
            }
        }
        elseif ($baseEntry.Kind -eq "Binary") {
            if (-not [string]::Equals([string]$baseEntry.Data, [string]$currEntry.Data, [System.StringComparison]::Ordinal)) {
                $diffMessage.Value = "Value '$name' at '$path' Binary byte mismatch: Baseline=$($baseEntry.Data), Current=$($currEntry.Data)"
                return $false
            }
        }
        elseif ($baseEntry.Kind -eq "DWord" -or $baseEntry.Kind -eq "QWord") {
            if ([long]$baseEntry.Data -ne [long]$currEntry.Data) {
                $diffMessage.Value = "Value '$name' at '$path' integer data mismatch: Baseline=$($baseEntry.Data), Current=$($currEntry.Data)"
                return $false
            }
        }
        else {
            if (-not [string]::Equals([string]$baseEntry.Data, [string]$currEntry.Data, [System.StringComparison]::Ordinal)) {
                $diffMessage.Value = "Value '$name' at '$path' String data mismatch (case/content): Baseline='$($baseEntry.Data)', Current='$($currEntry.Data)'"
                return $false
            }
        }
    }

    $baseNamesSet = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::Ordinal)
    foreach ($bn in $baseNames) { $baseNamesSet.Add($bn) | Out-Null }

    foreach ($name in $currNames) {
        if (-not $baseNamesSet.Contains($name)) {
            $diffMessage.Value = "Unexpected extra value found in current snapshot at '$path': '$name'"
            return $false
        }
    }

    # Recursive subkey comparison
    $baseSubKeys = Get-SnapshotValueNames $baseNode.SubKeys
    $currSubKeys = Get-SnapshotValueNames $currNode.SubKeys

    $currSubKeysSet = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::Ordinal)
    foreach ($csk in $currSubKeys) { $currSubKeysSet.Add($csk) | Out-Null }

    foreach ($sk in $baseSubKeys) {
        if (-not $currSubKeysSet.Contains($sk)) {
            $diffMessage.Value = "Subkey missing in current snapshot at '$path': '$sk'"
            return $false
        }
        $childBase = Get-SnapshotValueEntry $baseNode.SubKeys $sk
        $childCurr = Get-SnapshotValueEntry $currNode.SubKeys $sk
        $childPath = if ($path) { "$path\$sk" } else { $sk }
        if (-not (Compare-RegistryNodeEntries $childBase $childCurr $childPath $diffMessage)) {
            return $false
        }
    }

    $baseSubKeysSet = New-Object System.Collections.Generic.HashSet[string]([System.StringComparer]::Ordinal)
    foreach ($bsk in $baseSubKeys) { $baseSubKeysSet.Add($bsk) | Out-Null }

    foreach ($sk in $currSubKeys) {
        if (-not $baseSubKeysSet.Contains($sk)) {
            $diffMessage.Value = "Unexpected extra subkey found in current snapshot at '$path': '$sk'"
            return $false
        }
    }

    return $true
}

function Compare-RegistryScopeSnapshots($baseline, $current, [ref]$diffMessage) {
    if ($baseline.Exists -ne $current.Exists) {
        $diffMessage.Value = "Existence mismatch: Baseline.Exists=$($baseline.Exists), Current.Exists=$($current.Exists)"
        return $false
    }

    if (-not $baseline.Exists) {
        return $true
    }

    return Compare-RegistryNodeEntries $baseline $current "" $diffMessage
}

function Backup-SaveState([string]$dir, [string]$regSubKey, [switch]$allowUnity) {
    Assert-NoConflictingUnityProcess -allow:$allowUnity

    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }

    $journalPath = Join-Path $dir "journal.json"
    $journalTmpPath = Join-Path $dir "journal.json.tmp"
    $regFile = Join-Path $dir "tltd_playerprefs.reg"

    # Check for unhandled prior backup
    if (Test-Path $journalPath) {
        try {
            $existing = Get-Content $journalPath -Raw | ConvertFrom-Json
            if ($existing.Status -eq "BACKED_UP" -or $existing.Status -eq "RESTORE_IN_PROGRESS") {
                throw "[SAVE GUARD FATAL] Prior backup journal at $journalPath is still in uncompleted state '$($existing.Status)'! Must restore or clear safely before creating new backup."
            }
        } catch {
            if ($_.Exception.Message -like "*Prior backup journal*") { throw }
            Write-Warning "[SAVE GUARD] Existing journal unreadable; proceeding with caution."
        }
    }

    Write-Host "[SAVE GUARD] Capturing persistence baseline for: HKCU:\$regSubKey"
    $snapshot = Get-RegistryScopeSnapshot $regSubKey

    $fullRegPath = "HKEY_CURRENT_USER\$regSubKey"
    $backupHash = $null

    if ($snapshot.Exists) {
        Write-Host "[SAVE GUARD] Scope exists with $($snapshot.ValueCount) values. Exporting backup to $regFile..."
        & reg.exe export $fullRegPath $regFile /y
        if ($LASTEXITCODE -ne 0 -or -not (Test-Path $regFile) -or (Get-Item $regFile).Length -eq 0) {
            throw "[SAVE GUARD FATAL] Registry export failed! ExitCode: $LASTEXITCODE"
        }
        $backupHash = (Get-FileHash -Path $regFile -Algorithm SHA256).Hash
        Write-Host "[SAVE GUARD] Backup export verified: $regFile ($((Get-Item $regFile).Length) bytes, SHA256: $backupHash)"
    } else {
        Write-Host "[SAVE GUARD] Scope did not previously exist."
    }

    $journal = @{
        Timestamp = (Get-Date -Format "yyyy-MM-dd HH:mm:ss")
        Status = "BACKED_UP"
        RegSubKey = $regSubKey
        FullRegPath = $fullRegPath
        OriginallyExisted = $snapshot.Exists
        ValueCount = $snapshot.ValueCount
        BackupFile = $(if ($snapshot.Exists) { $regFile } else { $null })
        BackupHash = $backupHash
        Snapshot = $snapshot
    }

    # Atomic journal write
    $journalJson = $journal | ConvertTo-Json -Depth 10
    Set-Content -Path $journalTmpPath -Value $journalJson -Encoding UTF8
    Move-Item -Path $journalTmpPath -Destination $journalPath -Force
    Write-Host "[SAVE GUARD] Durable atomic journal created at: $journalPath (Status: BACKED_UP)"
    return $true
}

function Restore-SaveState([string]$dir, [switch]$allowUnity) {
    Assert-NoConflictingUnityProcess -allow:$allowUnity

    $journalPath = Join-Path $dir "journal.json"
    $journalTmpPath = Join-Path $dir "journal.json.tmp"
    if (-not (Test-Path $journalPath)) {
        Write-Warning "[SAVE GUARD] No journal found at $journalPath. Nothing to restore."
        return $true
    }

    $journal = Get-Content $journalPath -Raw | ConvertFrom-Json
    $fullRegPath = $journal.FullRegPath
    $regSubKey = $journal.RegSubKey

    Write-Host "[SAVE GUARD] Initiating recovery for: $fullRegPath"

    # Pre-restore verification
    if ($journal.OriginallyExisted) {
        $regFile = $journal.BackupFile
        if (-not (Test-Path $regFile)) {
            throw "[SAVE GUARD FATAL] Backup file missing during restore: $regFile"
        }

        # Validate file readability and hash match BEFORE touching current data
        $currentHash = (Get-FileHash -Path $regFile -Algorithm SHA256).Hash
        if ($currentHash -ne $journal.BackupHash) {
            throw "[SAVE GUARD FATAL] Backup hash validation FAILED! Expected: $($journal.BackupHash), Found: $currentHash. Registry restoration aborted to prevent data corruption."
        }
        Write-Host "[SAVE GUARD] Backup SHA256 verified ($currentHash). Safe to proceed."
    }

    # Set status to RESTORE_IN_PROGRESS atomically
    $journal.Status = "RESTORE_IN_PROGRESS"
    $journal | ConvertTo-Json -Depth 10 | Set-Content -Path $journalTmpPath -Encoding UTF8
    Move-Item -Path $journalTmpPath -Destination $journalPath -Force

    if ($journal.OriginallyExisted) {
        # Clean current mutated key before import to eliminate extra values created during tests
        Safe-DeleteRegistryKey $fullRegPath $regSubKey
        Start-Sleep -Milliseconds 50

        # Import original state
        & reg.exe import $journal.BackupFile
        if ($LASTEXITCODE -ne 0) {
            throw "[SAVE GUARD FATAL] Registry import failed! ExitCode: $LASTEXITCODE"
        }
        Write-Host "[SAVE GUARD] Registry restored from $($journal.BackupFile)"
    } else {
        # Originally non-existent: delete any key created
        Safe-DeleteRegistryKey $fullRegPath $regSubKey
        Write-Host "[SAVE GUARD] Scope deleted to match originally non-existent baseline"
    }

    # Verify comparison against baseline snapshot BEFORE marking complete
    $currentSnapshot = Get-RegistryScopeSnapshot $regSubKey
    $diff = ""
    $match = Compare-RegistryScopeSnapshots $journal.Snapshot $currentSnapshot ([ref]$diff)

    if (-not $match) {
        $journal.Status = "RESTORE_FAILED"
        $journal | ConvertTo-Json -Depth 10 | Set-Content -Path $journalTmpPath -Encoding UTF8
        Move-Item -Path $journalTmpPath -Destination $journalPath -Force
        throw "[SAVE GUARD FATAL] Post-restore comparison failed! Diff: $diff"
    }

    # Status marked VERIFIED/COMPLETE only after verified comparison
    $journal.Status = "VERIFIED"
    $journal | Add-Member -MemberType NoteProperty -Name "RestoredTimestamp" -Value (Get-Date -Format "yyyy-MM-dd HH:mm:ss") -Force
    $journal | ConvertTo-Json -Depth 10 | Set-Content -Path $journalTmpPath -Encoding UTF8
    Move-Item -Path $journalTmpPath -Destination $journalPath -Force
    Write-Host "[SAVE GUARD] Recovery verified and journal marked VERIFIED."
    return $true
}

function Compare-SaveState([string]$dir) {
    $journalPath = Join-Path $dir "journal.json"
    if (-not (Test-Path $journalPath)) {
        throw "[SAVE GUARD FATAL] Journal missing for comparison: $journalPath"
    }

    $journal = Get-Content $journalPath -Raw | ConvertFrom-Json
    $regSubKey = $journal.RegSubKey
    $currentSnapshot = Get-RegistryScopeSnapshot $regSubKey

    $diff = ""
    $match = Compare-RegistryScopeSnapshots $journal.Snapshot $currentSnapshot ([ref]$diff)

    if (-not $match) {
        Write-Warning "[SAVE GUARD COMPARISON FAILED] $diff"
        return $false
    }

    Write-Host "[SAVE GUARD COMPARISON] Exact type and data match against baseline ($($journal.ValueCount) values verified): PASS"
    return $true
}

function Check-InterruptedJournal([string]$dir) {
    $journalPath = Join-Path $dir "journal.json"
    if (Test-Path $journalPath) {
        $journal = Get-Content $journalPath -Raw | ConvertFrom-Json
        if ($journal.Status -eq "BACKED_UP" -or $journal.Status -eq "RESTORE_IN_PROGRESS" -or $journal.Status -eq "RESTORE_FAILED") {
            Write-Warning "[SAVE GUARD] Interrupted session detected! Status is '$($journal.Status)'. Executing recovery..."
            Restore-SaveState $dir
            Write-Host "[SAVE GUARD] Interrupted session cleanly recovered and verified."
        }
    }
}

function Run-DisposableFailureTest() {
    $disposableDir = "E:\code\TLTD\scratch\.disposable_recovery_test"
    $disposableKey = "Software\Unity\UnityEditor\DefaultCompany\TLTD_DisposableRecoveryTest"
    $fullKeyPath = "HKEY_CURRENT_USER\$disposableKey"
    $testLog = "E:\code\TLTD\Tools\Verification\P08\failure_path_recovery_test.log"

    $logLines = [System.Collections.Generic.List[string]]::new()
    function LogMsg($msg) {
        $line = "[$([DateTime]::Now.ToString('yyyy-MM-dd HH:mm:ss.fff'))] $msg"
        Write-Host $line
        $logLines.Add($line)
    }

    LogMsg "=== STARTING COMPREHENSIVE DISPOSABLE RECOVERY FAILURE PATH TEST ==="

    try {
        # Clean any prior residue
        Safe-DeleteRegistryKey $fullKeyPath $disposableKey
        Remove-Item -Recurse -Force $disposableDir -ErrorAction SilentlyContinue

        # -------------------------------------------------------------
        # SUITE A: Real Process Execution, Mutation & Forcible Termination
        # -------------------------------------------------------------
        LogMsg "[SUITE A] Setting up baseline with multi-type values (DWORD, String, Binary)..."
        & reg.exe add $fullKeyPath /v "Baseline_DWord" /t REG_DWORD /d 1337 /f | Out-Null
        & reg.exe add $fullKeyPath /v "Baseline_String" /t REG_SZ /d "StandardText" /f | Out-Null
        & reg.exe add $fullKeyPath /v "Baseline_Binary" /t REG_BINARY /d "0102030405AABBCCDDEEFF" /f | Out-Null

        Backup-SaveState $disposableDir $disposableKey -allowUnity | Out-Null
        LogMsg "[SUITE A] Baseline backed up. Journal status: BACKED_UP."

        # Spawn a REAL mutating background worker process
        LogMsg "[SUITE A] Spawning a real background PowerShell process mutating the disposable registry..."
        $workerPs1 = Join-Path $disposableDir "worker_mutate.ps1"
        $workerContent = @"
& reg.exe add "$fullKeyPath" /v "Baseline_DWord" /t REG_DWORD /d 999999 /f
& reg.exe add "$fullKeyPath" /v "Baseline_Binary" /t REG_BINARY /d "FFFFFFFF" /f
& reg.exe add "$fullKeyPath" /v "Dirty_CreatedByProcess" /t REG_SZ /d "CorruptData" /f
Start-Sleep -Seconds 60
"@
        Set-Content -Path $workerPs1 -Value $workerContent -Encoding UTF8
        $workerOut = Join-Path $disposableDir "worker_out.log"
        $workerErr = Join-Path $disposableDir "worker_err.log"
        $proc = Start-Process -FilePath "powershell.exe" -ArgumentList @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $workerPs1) -RedirectStandardOutput $workerOut -RedirectStandardError $workerErr -PassThru

        $sessionOwnerPid = $proc.Id
        $sessionStartTime = $proc.StartTime
        LogMsg "[SUITE A] Worker process started with PID: $sessionOwnerPid, StartTime: $sessionStartTime."

        # Wait for worker to perform mutations (polling up to 5s)
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $mutatedSnap = $null
        while ($sw.ElapsedMilliseconds -lt 5000) {
            $mutatedSnap = Get-RegistryScopeSnapshot $disposableKey
            if ($mutatedSnap.Values.ContainsKey("Dirty_CreatedByProcess") -and $mutatedSnap.Values["Baseline_DWord"].Data -eq 999999) {
                break
            }
            Start-Sleep -Milliseconds 200
        }

        if (-not $mutatedSnap.Values.ContainsKey("Dirty_CreatedByProcess") -or $mutatedSnap.Values["Baseline_DWord"].Data -ne 999999) {
            throw "[SUITE A FATAL] Worker process failed to mutate registry within timeout!"
        }
        LogMsg "[SUITE A] Confirmed worker mutated DWORD, Binary, and added dirty keys."

        # Forcibly terminate the process tree owned by the session
        LogMsg "[SUITE A] Forcibly killing real worker process PID: $sessionOwnerPid..."
        Stop-Process -Id $sessionOwnerPid -Force -ErrorAction SilentlyContinue
        Start-Sleep -Milliseconds 200

        if (-not $proc.HasExited) {
            throw "[SUITE A FATAL] Worker process failed to terminate!"
        }
        LogMsg "[SUITE A] Worker process terminated cleanly (HasExited=True)."

        # Execute recovery from journal
        LogMsg "[SUITE A] Executing durable journal recovery..."
        Restore-SaveState $disposableDir -allowUnity | Out-Null

        # Compare post-recovery state
        $matchA = Compare-SaveState $disposableDir
        if (-not $matchA) {
            throw "[SUITE A FATAL] Post-recovery comparison failed!"
        }
        LogMsg "[SUITE A] Post-recovery exact data comparison: PASS"

        # -------------------------------------------------------------
        # SUITE B: Negative Check - Same Name and Count But Mutated Data
        # -------------------------------------------------------------
        LogMsg "[SUITE B] Negative Check: Mutating data while keeping count and names identical..."
        & reg.exe add $fullKeyPath /v "Baseline_DWord" /t REG_DWORD /d 8888 /f | Out-Null
        $diffB = ""
        $currSnapB = Get-RegistryScopeSnapshot $disposableKey
        $journalB = Get-Content (Join-Path $disposableDir "journal.json") -Raw | ConvertFrom-Json
        $matchB = Compare-RegistryScopeSnapshots $journalB.Snapshot $currSnapB ([ref]$diffB)
        if ($matchB) {
            throw "[SUITE B FATAL] Comparison falsely PASSED on mutated data!"
        }
        LogMsg "[SUITE B] Comparison correctly REJECTED mutated data ($diffB): PASS"

        # Restore back to clean baseline
        Restore-SaveState $disposableDir -allowUnity | Out-Null

        # -------------------------------------------------------------
        # SUITE C: Negative Check - Different Value Type
        # -------------------------------------------------------------
        LogMsg "[SUITE C] Negative Check: Altering value type (DWORD to String)..."
        & reg.exe add $fullKeyPath /v "Baseline_DWord" /t REG_SZ /d "1337" /f | Out-Null
        $diffC = ""
        $currSnapC = Get-RegistryScopeSnapshot $disposableKey
        $matchC = Compare-RegistryScopeSnapshots $journalB.Snapshot $currSnapC ([ref]$diffC)
        if ($matchC) {
            throw "[SUITE C FATAL] Comparison falsely PASSED on type mismatch!"
        }
        LogMsg "[SUITE C] Comparison correctly REJECTED value kind mismatch ($diffC): PASS"

        # Restore back to clean baseline
        Restore-SaveState $disposableDir -allowUnity | Out-Null

        # -------------------------------------------------------------
        # SUITE D: Negative Check - Tampered / Corrupted Backup File
        # -------------------------------------------------------------
        LogMsg "[SUITE D] Negative Check: Tampering with backup file to test hash guard..."
        $regFile = (Get-Content (Join-Path $disposableDir "journal.json") -Raw | ConvertFrom-Json).BackupFile
        # Modify journal to BACKED_UP to allow restore attempt
        $jData = Get-Content (Join-Path $disposableDir "journal.json") -Raw | ConvertFrom-Json
        $jData.Status = "BACKED_UP"
        $jData | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $disposableDir "journal.json") -Encoding UTF8

        # Corrupt reg file
        Add-Content -Path $regFile -Value ";CORRUPTED_TAMPERED_CONTENT" -Encoding UTF8
        $hashFailed = $false
        try {
            Restore-SaveState $disposableDir -allowUnity | Out-Null
        } catch {
            if ($_.Exception.Message -like "*Backup hash validation FAILED*") {
                $hashFailed = $true
                LogMsg "[SUITE D] Restore correctly REFUSED corrupted backup before touching current state: PASS"
            } else {
                throw
            }
        }
        if (-not $hashFailed) {
            throw "[SUITE D FATAL] Restore succeeded on corrupted backup file!"
        }

        # Re-establish clean backup after tampering test
        Remove-Item -Recurse -Force $disposableDir -ErrorAction SilentlyContinue
        Backup-SaveState $disposableDir $disposableKey -allowUnity | Out-Null
        $journalClean = Get-Content (Join-Path $disposableDir "journal.json") -Raw | ConvertFrom-Json

        # -------------------------------------------------------------
        # SUITE F: Negative Check - String Case-Sensitive Ordinal Difference
        # -------------------------------------------------------------
        LogMsg "[SUITE F] Negative Check: Altering string case only ('StandardText' -> 'standardtext')..."
        & reg.exe add $fullKeyPath /v "Baseline_String" /t REG_SZ /d "standardtext" /f | Out-Null
        $diffF = ""
        $currSnapF = Get-RegistryScopeSnapshot $disposableKey
        $matchF = Compare-RegistryScopeSnapshots $journalClean.Snapshot $currSnapF ([ref]$diffF)
        if ($matchF) {
            throw "[SUITE F FATAL] Comparison falsely PASSED when string case differed!"
        }
        LogMsg "[SUITE F] Comparison correctly REJECTED string case difference ($diffF): PASS"

        # Restore back to clean baseline
        Restore-SaveState $disposableDir -allowUnity | Out-Null

        # -------------------------------------------------------------
        # SUITE G: Negative Check - MultiString Case and Element Order
        # -------------------------------------------------------------
        LogMsg "[SUITE G] Negative Check: Testing MultiString case and order sensitivity..."
        & reg.exe add $fullKeyPath /v "Baseline_Multi" /t REG_MULTI_SZ /d "Alpha\0Beta\0Gamma" /f | Out-Null
        Remove-Item -Recurse -Force $disposableDir -ErrorAction SilentlyContinue
        Backup-SaveState $disposableDir $disposableKey -allowUnity | Out-Null
        $journalG = Get-Content (Join-Path $disposableDir "journal.json") -Raw | ConvertFrom-Json

        # Test G1: Case difference in one element
        & reg.exe add $fullKeyPath /v "Baseline_Multi" /t REG_MULTI_SZ /d "alpha\0Beta\0Gamma" /f | Out-Null
        $diffG1 = ""
        $currSnapG1 = Get-RegistryScopeSnapshot $disposableKey
        $matchG1 = Compare-RegistryScopeSnapshots $journalG.Snapshot $currSnapG1 ([ref]$diffG1)
        if ($matchG1) {
            throw "[SUITE G1 FATAL] Comparison falsely PASSED on MultiString element case difference!"
        }
        LogMsg "[SUITE G1] Comparison correctly REJECTED MultiString case difference ($diffG1): PASS"

        # Test G2: Element order swap
        & reg.exe add $fullKeyPath /v "Baseline_Multi" /t REG_MULTI_SZ /d "Beta\0Alpha\0Gamma" /f | Out-Null
        $diffG2 = ""
        $currSnapG2 = Get-RegistryScopeSnapshot $disposableKey
        $matchG2 = Compare-RegistryScopeSnapshots $journalG.Snapshot $currSnapG2 ([ref]$diffG2)
        if ($matchG2) {
            throw "[SUITE G2 FATAL] Comparison falsely PASSED on MultiString element order swap!"
        }
        LogMsg "[SUITE G2] Comparison correctly REJECTED MultiString order swap ($diffG2): PASS"

        # Restore back to clean baseline
        Restore-SaveState $disposableDir -allowUnity | Out-Null

        # -------------------------------------------------------------
        # SUITE H: Negative Check - Binary Byte-Exact Mutation
        # -------------------------------------------------------------
        LogMsg "[SUITE H] Negative Check: Mutating 1 byte in Binary value..."
        Safe-DeleteRegistryKey $fullKeyPath $disposableKey
        Remove-Item -Recurse -Force $disposableDir -ErrorAction SilentlyContinue
        & reg.exe add $fullKeyPath /v "Baseline_DWord" /t REG_DWORD /d 1337 /f | Out-Null
        & reg.exe add $fullKeyPath /v "Baseline_String" /t REG_SZ /d "StandardText" /f | Out-Null
        & reg.exe add $fullKeyPath /v "Baseline_Binary" /t REG_BINARY /d "0102030405AABBCCDDEEFF" /f | Out-Null
        Backup-SaveState $disposableDir $disposableKey -allowUnity | Out-Null
        $journalH = Get-Content (Join-Path $disposableDir "journal.json") -Raw | ConvertFrom-Json

        & reg.exe add $fullKeyPath /v "Baseline_Binary" /t REG_BINARY /d "0102030405AABBCCDDEEFE" /f | Out-Null
        $diffH = ""
        $currSnapH = Get-RegistryScopeSnapshot $disposableKey
        $matchH = Compare-RegistryScopeSnapshots $journalH.Snapshot $currSnapH ([ref]$diffH)
        if ($matchH) {
            throw "[SUITE H FATAL] Comparison falsely PASSED on Binary single-byte mutation!"
        }
        if ($diffH -notlike "*Binary byte mismatch*") {
            throw "[SUITE H FATAL] Comparison rejected for wrong reason (expected Binary byte mismatch): $diffH"
        }
        LogMsg "[SUITE H] Comparison correctly REJECTED Binary byte mutation ($diffH): PASS"

        # Restore back to clean baseline
        Restore-SaveState $disposableDir -allowUnity | Out-Null

        # -------------------------------------------------------------
        # SUITE I: Negative Check - Recursive Subkey Mutation, Addition, and Deletion
        # -------------------------------------------------------------
        LogMsg "[SUITE I] Negative Check: Testing recursive subkey snapshot and comparison..."
        $subKeyPath1 = "$fullKeyPath\SubFolder1"
        $subKeyPath2 = "$fullKeyPath\SubFolder1\SubFolder2"
        & reg.exe add $subKeyPath1 /v "SubVal1" /t REG_SZ /d "OriginalSubText" /f | Out-Null
        & reg.exe add $subKeyPath2 /v "DeepVal" /t REG_DWORD /d 42 /f | Out-Null

        Backup-SaveState $disposableDir $disposableKey -allowUnity | Out-Null
        $journalI = Get-Content (Join-Path $disposableDir "journal.json") -Raw | ConvertFrom-Json

        # Test I1: Mutate value inside recursive subkey
        & reg.exe add $subKeyPath1 /v "SubVal1" /t REG_SZ /d "originalsubtext" /f | Out-Null
        $diffI1 = ""
        $currSnapI1 = Get-RegistryScopeSnapshot $disposableKey
        $matchI1 = Compare-RegistryScopeSnapshots $journalI.Snapshot $currSnapI1 ([ref]$diffI1)
        if ($matchI1) {
            throw "[SUITE I1 FATAL] Comparison falsely PASSED on subkey value case change!"
        }
        LogMsg "[SUITE I1] Comparison correctly REJECTED subkey value modification ($diffI1): PASS"

        # Test I2: Add unexpected child subkey
        Restore-SaveState $disposableDir -allowUnity | Out-Null
        $extraSubKey = "$fullKeyPath\SubFolder1\UnexpectedExtraChild"
        & reg.exe add $extraSubKey /v "Dummy" /t REG_SZ /d "Extra" /f | Out-Null
        $diffI2 = ""
        $currSnapI2 = Get-RegistryScopeSnapshot $disposableKey
        $matchI2 = Compare-RegistryScopeSnapshots $journalI.Snapshot $currSnapI2 ([ref]$diffI2)
        if ($matchI2) {
            throw "[SUITE I2 FATAL] Comparison falsely PASSED on unexpected extra subkey!"
        }
        LogMsg "[SUITE I2] Comparison correctly REJECTED extra subkey ($diffI2): PASS"

        # Test I3: Delete nested subkey
        Restore-SaveState $disposableDir -allowUnity | Out-Null
        Safe-DeleteRegistryKey $subKeyPath2 "$disposableKey\SubFolder1\SubFolder2"
        $diffI3 = ""
        $currSnapI3 = Get-RegistryScopeSnapshot $disposableKey
        $matchI3 = Compare-RegistryScopeSnapshots $journalI.Snapshot $currSnapI3 ([ref]$diffI3)
        if ($matchI3) {
            throw "[SUITE I3 FATAL] Comparison falsely PASSED on missing deleted subkey!"
        }
        LogMsg "[SUITE I3] Comparison correctly REJECTED missing deleted subkey ($diffI3): PASS"

        # Restore back to clean baseline
        Restore-SaveState $disposableDir -allowUnity | Out-Null

        # -------------------------------------------------------------
        # SUITE E: Negative Check - Non-Existent Initial Scope
        # -------------------------------------------------------------
        LogMsg "[SUITE E] Testing scope that originally did NOT exist..."
        Safe-DeleteRegistryKey $fullKeyPath $disposableKey
        Remove-Item -Recurse -Force $disposableDir -ErrorAction SilentlyContinue

        Backup-SaveState $disposableDir $disposableKey -allowUnity | Out-Null
        LogMsg "[SUITE E] Backed up non-existent scope. Creating dirty key during test..."
        & reg.exe add $fullKeyPath /v "Dirty_CreatedInTest" /t REG_DWORD /d 123 /f | Out-Null

        Restore-SaveState $disposableDir -allowUnity | Out-Null
        $keyStillExists = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($disposableKey, $false)
        if ($null -ne $keyStillExists) {
            $keyStillExists.Close()
            throw "[SUITE E FATAL] Non-existent scope was not deleted during restore!"
        }
        LogMsg "[SUITE E] Non-existent scope correctly returned to non-existent state: PASS"

        # Final teardown
        Safe-DeleteRegistryKey $fullKeyPath $disposableKey
        Remove-Item -Recurse -Force $disposableDir -ErrorAction SilentlyContinue
        LogMsg "[TEARDOWN] All disposable test fixtures cleanly removed."
        LogMsg "=== ALL DISPOSABLE FAILURE PATH TESTS COMPLETED: ALL PASS ==="

        $logLines -join "`r`n" | Set-Content -Path $testLog -Encoding UTF8
        return $true
    }
    catch {
        LogMsg "[ERROR] $($_.Exception.Message)"
        $logLines -join "`r`n" | Set-Content -Path $testLog -Encoding UTF8
        Safe-DeleteRegistryKey $fullKeyPath $disposableKey
        Write-Error $_
        return $false
    }
}

# Main Dispatcher
switch ($Action) {
    "Backup" {
        Check-InterruptedJournal $BackupDir
        Backup-SaveState $BackupDir $CustomRegKey -allowUnity:$AllowRunningUnity
    }
    "Restore" {
        Restore-SaveState $BackupDir -allowUnity:$AllowRunningUnity
    }
    "Compare" {
        $res = Compare-SaveState $BackupDir
        if (-not $res) { exit 1 } else { exit 0 }
    }
    "CheckInterruptedJournal" {
        Check-InterruptedJournal $BackupDir
    }
    "VerifyRecovery" {
        $res = Run-DisposableFailureTest
        if (-not $res) { exit 1 } else { exit 0 }
    }
}
