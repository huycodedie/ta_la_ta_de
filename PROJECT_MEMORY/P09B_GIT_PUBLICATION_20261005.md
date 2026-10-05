# P09-B — GIT PUBLICATION AUTHORIZATION AND PROVENANCE

Publication date: 2026-10-05, UTC+07:00. Acceptance date remains 2026-10-04.

## Explicit publication authority

The Project Owner instructed Codex: “nếu đã chốt thì hãy gửi nó lên git”. This authorizes publishing the accepted P09-B decision and its continuity documents to `huycodedie/Ai_MEMORY_TLTD/main`, with the corresponding documentation mirror in `huycodedie/ta_la_ta_de/main`.

This task-specific authorization supersedes the no-commit/no-push restriction for the earlier documentation-only local handoff task. It does not authorize gameplay/source/asset changes, save recovery, test reruns, force-push, or unrelated local changes. Historical decision/review documents are preserved as issued; their older task restrictions are not rewritten as if publication had been authorized then.

## Published authority

- [P09-B original Codex decision](DECISION_P09B_ACCEPTED_LOCKED_20261004.md): ACCEPTED / LOCKED within the reviewed runtime/fixture scope; F-SAVE-P09B-01 CLOSED / RECOVERY ACCEPTED.
- [Handoff review addendum](TECH_LEAD_HANDOFF_REVIEW_20261004.md): corrected provenance and evidence limits; raw Restore/execution PARTIAL; baseline Compare/preflight METADATA_ONLY.
- [Previously accepted P09-A decision](DECISION_P09A_ACCEPTED_LOCKED_20260927.md): included to restore the missing prerequisite pointer on remote; no new gameplay acceptance or implementation is implied.
- ACTIVE_WORK_HANDOFF, CURRENT_DESIGN_AUTHORITY, DESIGN_CHANGELOG and entrypoint summaries are updated to prevent reopening P09-A/P09-B as pending implementation.

## Source of the publication

| Artifact | SHA256 |
|---|---|
| Corrected handoff ZIP, 27,849 bytes, 17 payload / 18 files | `DFFF66AEA952AA6B1E3641DBBBC829A956EE29ECAA8D20EA2DE36A96979DD219` |
| P09-B decision, original 9,149 bytes | `D47608B21B77F4DA8EB13E8D8C070BCBF9EE25091D11DCB7AD84E8EF85798F52` |
| Recovery result ZIP, 6,493 bytes | `2DCF00E47A80B955345E42A7B4F8D2F6D4CCEC70824C57C5FE254D5C0EC9E07C` |
| P09-A tooling maintenance ZIP | `1D3ABBD30001E104AD861B033967F9FA92E88713B494BABD804522EEA2A5FF99` |

Antigravity reported local corrected-handoff synchronization complete at 2026-10-04 23:40:00 +07:00. That is executor-reported local synchronization, not an independent Codex inspection of E:\code\TLTD. Codex reviewed the supplied packages and prepared this publication; no Unity/PowerShell/Registry execution is performed as part of publication.

Raw logs, Registry backups and source ZIPs remain in the existing evidence archives; this commit publishes documentation, not a new test run or a full evidence dump. Accepted original documents remain byte-identical. The P09-A source hashes describe its historical accepted revision; the P09-B decision identifies the later reviewed runtime revision.

Canonical memory publication: [3240a49](https://github.com/huycodedie/Ai_MEMORY_TLTD/commit/3240a49788bc097acff6b530b95db39876646550). This game-repository commit mirrors the accepted documentation.

## Remote bases and verification

- Canonical memory base observed: `8ff7f79fcdcbb966a88e9a74fd41bd97c5c4a9c2`.
- Game documentation mirror base observed: `5eafa685b830282665aca72323ef143ff063f9f5`.
- Build each commit on its repository's observed base; verify refs before publication; update main by fast-forward only; verify remote changed-file scope and blob contents afterward.
- A file cannot contain its own commit SHA. Resolve this file's publishing commit/main and use the commit links returned by Codex for publication identity.

## Next action

Read the current roadmap and locked design documents to propose the next bounded gameplay scope. No production skill-asset rollout or new milestone implementation is authorized by this publication. P09-B does not require another gameplay test, recovery attempt or ZIP acceptance loop.
