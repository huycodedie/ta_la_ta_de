# ==============================================================================
# P09B_PreflightGuard.ps1
# P09-B Save Guard Preflight Inspection Helper
#
# PURPOSE:
# Inspects existing journal state before any destructive action, recovery,
# or Unity process launch.
# - If no journal exists: allows standard session flow.
# - If journal status is VERIFIED: allows standard session flow.
# - If journal status is BACKED_UP, RESTORE_IN_PROGRESS, or RESTORE_FAILED:
#   blocks execution with PRECHECK_BLOCKED_UNRESOLVED_JOURNAL.
# - If journal is unreadable, malformed, missing status, or unrecognized status:
#   safely blocks execution with PRECHECK_BLOCKED_UNRESOLVED_JOURNAL.
# ==============================================================================

function Test-P09BJournalPreflight {
    param(
        [Parameter(Mandatory = $true)]
        [string]$BackupDir,
        [scriptblock]$Logger = $null
    )

    $journalPath = Join-Path $BackupDir "journal.json"

    $emitLog = {
        param([string]$msg, [string]$color = "White")
        if ($null -ne $Logger) {
            try { & $Logger $msg $color } catch { & $Logger $msg }
        }
    }

    # 1. No journal file exists
    if (-not (Test-Path -LiteralPath $journalPath)) {
        & $emitLog "[PREFLIGHT] No existing journal found at '$journalPath'. Proceeding with standard flow." "Cyan"
        return [ordered]@{
            Allowed = $true
            Status  = $null
            Reason  = "NO_JOURNAL"
            Message = "No existing journal found. Proceeding with standard flow."
        }
    }

    # 2. Journal file exists: safely read content
    $rawContent = $null
    try {
        $rawContent = [System.IO.File]::ReadAllText($journalPath, [System.Text.Encoding]::UTF8)
    } catch {
        $reason = "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL: Journal file exists but cannot be read: $($_.Exception.Message)"
        & $emitLog "[PREFLIGHT FATAL] $reason" "Red"
        return [ordered]@{
            Allowed = $false
            Status  = "UNREADABLE"
            Reason  = $reason
            Message = $reason
        }
    }

    if ([string]::IsNullOrWhiteSpace($rawContent)) {
        $reason = "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL: Journal file is empty."
        & $emitLog "[PREFLIGHT FATAL] $reason" "Red"
        return [ordered]@{
            Allowed = $false
            Status  = "EMPTY"
            Reason  = $reason
            Message = $reason
        }
    }

    # 3. Parse JSON safely
    $parsed = $null
    try {
        $parsed = $rawContent | ConvertFrom-Json -ErrorAction Stop
    } catch {
        $reason = "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL: Journal file contains malformed or invalid JSON."
        & $emitLog "[PREFLIGHT FATAL] $reason" "Red"
        return [ordered]@{
            Allowed = $false
            Status  = "MALFORMED"
            Reason  = $reason
            Message = $reason
        }
    }

    if ($null -eq $parsed) {
        $reason = "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL: Journal JSON parsed as null object."
        & $emitLog "[PREFLIGHT FATAL] $reason" "Red"
        return [ordered]@{
            Allowed = $false
            Status  = "NULL_OBJECT"
            Reason  = $reason
            Message = $reason
        }
    }

    # 4. Check 'Status' property
    $statusProp = $parsed.PSObject.Properties["Status"]
    if ($null -eq $statusProp -or [string]::IsNullOrWhiteSpace($statusProp.Value)) {
        $reason = "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL: Journal is missing or has empty 'Status' field."
        & $emitLog "[PREFLIGHT FATAL] $reason" "Red"
        return [ordered]@{
            Allowed = $false
            Status  = "MISSING_STATUS"
            Reason  = $reason
            Message = $reason
        }
    }

    $statusVal = [string]$statusProp.Value

    # 5. Block unresolved statuses
    $unresolvedStatuses = @("BACKED_UP", "RESTORE_IN_PROGRESS", "RESTORE_FAILED")
    if ($unresolvedStatuses -contains $statusVal) {
        $reason = "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL: Unresolved journal detected with Status '$statusVal'. Automatic restore/recovery is blocked without explicit decision."
        & $emitLog "[PREFLIGHT FATAL] $reason" "Red"
        return [ordered]@{
            Allowed = $false
            Status  = $statusVal
            Reason  = $reason
            Message = $reason
        }
    }

    # 6. Allow recognized completed status supported by Save Guard source (VERIFIED)
    if ($statusVal -eq "VERIFIED") {
        & $emitLog "[PREFLIGHT] Valid completed journal with status 'VERIFIED' detected at '$journalPath'. Safe to proceed." "Green"
        return [ordered]@{
            Allowed = $true
            Status  = "VERIFIED"
            Reason  = "VERIFIED_JOURNAL"
            Message = "Valid completed journal with status 'VERIFIED'. Safe to proceed."
        }
    }

    # 7. Safe rejection for unknown or unsupported status
    $reason = "PRECHECK_BLOCKED_UNRESOLVED_JOURNAL: Journal status '$statusVal' is not a recognized completed status ('VERIFIED'). Safe rejection."
    & $emitLog "[PREFLIGHT FATAL] $reason" "Red"
    return [ordered]@{
        Allowed = $false
        Status  = $statusVal
        Reason  = $reason
        Message = $reason
    }
}
