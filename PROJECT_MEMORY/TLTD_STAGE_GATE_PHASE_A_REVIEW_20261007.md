# TLTD — Review SG-LAB-00 và scope corrective trong lab

Ngày 07/10/2026, UTC+07. Reviewer: Codex Tech Lead/Architect. Đây là tài liệu review/quyết định kỹ thuật; prompt thực thi nằm riêng tại [PROMPT_ANTIGRAVITY_LOOT_CHANNEL_HANDOFF_LAB_20261007.md](PROMPT_ANTIGRAVITY_LOOT_CHANNEL_HANDOFF_LAB_20261007.md).

## 1. Kết luận và continuation

**SPIKE-STAGE-GATE-LAB-01 / SG-LAB-00 Phase A = REVIEW COMPLETE / NARROW FAILURE EVIDENCE VERIFIED.** Probe đã tới nhánh finishing-channel → first loot, nhưng giao dịch Tách bị từ chối vì `_isLootDecisionOpen=false`. Raw scenario **FAIL**, Unity OS exit **1**, không watchdog timeout. Kết quả này đủ căn cứ sửa rủi ro tích hợp vừa quan sát trong lab; không thay thế hoặc hủy nghiệm thu P08/P09-A/P09-B.

**Phase B = NOT EXECUTED / STOPPED BY PHASE A RULE.** Không tiếp tục gate patch hoặc coi compile thành công là Phase A PASS. Wrapper/harness hiện tại **NOT ACCEPTED FOR REUSE** vì vi phạm guard conflict và thiếu ownership/cleanup. Guard summaries/final journal có giới hạn riêng; không chứng nhận save an toàn tuyệt đối.

**Next technical task: F-LOOT-CHANNEL-HANDOFF-LAB-01 — AUTHORIZED LAB ONLY / PROMPT PREPARED.** Antigravity sửa safety của runner/wrapper trước launch, rồi sửa đúng first-loot handoff trong BattleManager của bản sao lab và chạy một session kiểm chứng giới hạn. Codex chưa gửi prompt qua một external tool, chưa thực thi corrective, chưa nhận diff/PASS của corrective. Phase B cần Codex review/accept corrective packet và giao continuation rõ ràng; không tự mở lại sau PASS.

Không sửa/copy source vào `E:\code\TLTD`, không push source, không Stage/Boss production/persistence/schema/asset activation. Không Owner playtest, broad P08/P09 retest, recovery rerun hoặc ZIP nghiệm thu lúc này. Các correction metadata dưới đây không cần chạy lại.

## 2. Đã đọc và đối chiếu

Attachment `C:\Users\conca\.codex\attachments\f777c199-b3fc-412a-9648-71f4a8bcf429\Văn bản đã dán.txt`; bốn reports và hai cặp Unity/wrapper logs trong `work/stage_gate_lab_output_20261007_204500`; actual lab runner/wrapper/ProjectSettings; relevant BattleManager, LootDecisionUI, ModalCoordinator và lifecycle sources; 12 runtime fingerprints, reused guards, metadata final lab journal; canonical/game PROJECT_MEMORY và main refs.

Lab absolute root: `C:\Users\conca\Documents\Codex\2026-10-05\b-n-ti-p-qu-n\work\stage_gate_lab_20261007_204500`. Không có `.git` tại lab root. Không đủ delivered clone/copy provenance để xác nhận lab được tạo từ exact commit như prompt ban đầu yêu cầu. Current file equality được đo riêng bên dưới.

Pre-publication refs được đọc lại trong turn này:

- Canonical memory main: `6e8d31cb4a6e1fbbb13f88fe2f5bc1ec4c1f5310`.
- Game main: `03a1744620236c996f22eb6d15615e51fd40f595`.
- Local E: HEAD: `7441b9ec9508e6a39b1ec5a1c3d9abf89c73b425`. Runtime source baseline `393a0487083fd1e311db28cdf07393427769a4de`, tiếp theo là các docs-only commits; remote HEAD không chứng nhận toàn bộ local tested build.

Hai isolated publication checkouts đã `pull --ff-only`, clean trước edits. Snapshot E: lúc **21:20:52+07** ghi **3 tracked modifications + 530 untracked entries**; 17 runtime/harness/meta/dirty-doc fingerprints. Không có before/after inventory do executor cung cấp để chứng minh toàn bộ lịch sử spike. Codex chỉ bảo toàn/đối chiếu phần đã đo trong review/publication; không suy thành mọi untracked byte hoặc save đều được chứng nhận.

Codex không chạy Unity, test, wrapper, SaveGuard, không đọc live Registry/save và không sửa E: hoặc lab source. Final publication refs/payload verification nằm trong receipt riêng.

## 3. Integrity của packet

Sáu artifacts có bytes/SHA khớp report. Hai compile-attempt logs bổ sung được giữ nguyên. Không có external manifest, changed-path patches, E: before/after inventories hoặc raw guard/preflight transcripts đầy đủ trong output packet.

| Relative path trong output | Bytes | SHA256 |
|---|---:|---|
| EXECUTIVE_SUMMARY.md | 4720 | 82EB4EFF3AA6CE1DABD3B2F6F046EBB8AB3F26107D2E23F8FD365CAA712FE90D |
| SG_LAB_00_EMPIRICAL_FINDING.md | 6449 | 0AD08AD935C65993F3BE02B56DB404D3A4B62E634A4DD069F8E70EA23A546B2B |
| SOURCE_BASELINE_AND_DIFF.md | 2578 | 53A80E497DD5DFF62DD1E803CDD20CAEE5B622939224B56B04F098D873CA3584 |
| PROVENANCE_AND_REPRODUCIBILITY.md | 4748 | DF41BE67CB40F480D83D01100E1305E44BF07962F4063843887A7B3D689A24D5 |
| runs/run_20261007_210744/sg_lab_00_unity.log | 4680880 | 35FB69A28AF3671755C065E5151F33AEA056F8C93C7155EED327BBCB6228561B |
| runs/run_20261007_210744/wrapper_run.log | 7258 | 5F08056863D5E23E3F5BE144DADD9771F48E8074898A7F3399A30D13D620099E |
| runs/run_20261007_210155/sg_lab_00_unity.log | 435729 | F643231DEEFD03C1949642EF8546A98A2770D1FA94F1C193679B91DEC30E013A |
| runs/run_20261007_210155/wrapper_run.log | 4130 | F7A79E8501E25ED0D1CB8826E6D76F9BCF275D35CD1984C4E47242F5757778EC |

## 4. Outcome và empirical limit

| Attempt | Raw process | Scenario | Guard limit |
|---|---|---|---|
| 20261007_210155 | PID **12100**, 21:01:56 → 21:05:25, OS exit1, khoảng209s từ timestamps | Harness compilation error / NOT_REACHED; UNKNOWN marker không phải runtime PASS | Wrapper RestoreTrue/CompareTrue; riêng journal của attempt không được giữ |
| 20261007_210744 | PID **24764**, 21:07:45 → 21:11:49, OS exit1, khoảng244s từ timestamps | Reached **FAIL / HazardConfirmedTrue**, không watchdog timeout | Wrapper RestoreTrue/CompareTrue, current final journalVERIFIED; raw guard streams thiếu |

Wrapper elapsed format `${elapsed:F2}` thực tế in `Elapsed: s`. Các duration trên là suy ra từ timestamps, không phải số F2 đã ghi. Compile attempt có11 diagnostic locations của runner; đây là một failed execution trước final probe, vượt kế hoạch một Phase A launch. Giữ lịch sử, không gom thành một run sạch. Không cần launch thứ ba để lặp lại baseline lỗi đã đủ evidence.

Final raw Unity log cho thấy:

1. Lines18800–18833: actual company **TLTDLab**, product **StageGate_20261007_204500**, đúng lab Assets path, batchTrue.
2. Hero channel tạo bằng **SkillCastState.StartChannel** với request thật, duration0.5s/tick0.1; request được tick qua Entity.Update/natural frames. Final monster bị lethal damage bằng **Health.TakeDamage**, EventBus đưa vào BM death handler. Reflection chỉ đọc caster/queue; không sửa flag/pending hoặc cưỡng bức completion.
3. Lines19299/19323: finishing caster1, loot queue1; first item đã dequeue, remaining queue0. Lines19330–19356 stack tới `WaitForFinishingExecutionsThenProceed` rồi real LootDecisionUI request. Logged wait0.483s không phải tổng channel duration hoặc counter29frames.
4. Lines19385–19445: LootPending, pending Iron Bracers với InstanceId, openflagFalse/HasPendingFalse, active modal1, panel.activeSelfTrue.
5. Một lần gọi **functional OnDismantleClicked handler**, không phải Owner pointer click. Lines19482–19507: BM invalid-transaction warning, UI giữ modal. Lines19545–19605: item/panel/modal còn sau action trong khoảng quan sát. Line19665: PhaseAFAIL.

Đây là **one controlled channel-state/lifecycle fixture**, không SkillExecutor end-to-end, không Dash/Projectile/Rage/CD validation hay authored-skill gameplay. Fixture có một registered monster, không production4–5 roster; Monster tạo trước BM.Awake có thể bị deactivated, và không đủ owned Inventory/Resource/Progression/Equipment managers để đo reward/equip effect. Các giới hạn này không phủ nhận transaction guard rejection trước side effects, nhưng corrective PASS phải dùng setup/effect assertions tốt hơn.

Không chứng minh Equip, multiple/double click, sequential items, blocked/queued modal, no-loot hoặc mọi caster path. “Queue undrained” phải hiểu là **displayed pending decision unresolved**, không queue còn item. “100%”, “mọi vận công/dash/projectile”, “treo vĩnh viễn”, “crash”, “29frames đo thực”, “isolation tuyệt đối/không chạm một byte” không được nghiệm thu. Engine exit1 là explicit failure exit; final log còn package IO/import và UnityEditor.Search indexing errors, không claim clean zero-error session.

## 5. Source invariant và correction được chọn

Actual BattleManager: **58100bytes / 3A021DE3B78D985A7B51715CD4743F44202A229F9EEBDFEBECF02B0D6DDF6448**, E: = lab = R1 fingerprint.

- `CompleteLootDecisionAndResume(bool equip, bool dismantle)`1052–1055 trong PlayMode yêu cầu decision-open, LootPending và pending item có InstanceId không rỗng. API không nhận item/InstanceId của caller và không kiểm tra old-ID request sau khi next item presented. Không nới guard hoặc thêm identity API trong task này.
- `TryPresentNextQueuedLoot`874–890 là presentation authority: state/pending/queue/modal guards, dequeue886, **openflagTRUE887**, request889.
- Finishing coroutine1289–1297 tự dequeue/request, thiếu openflag và bỏ qua modal guards. `EnqueuePendingLoot` chỉ enqueue; report gọi đó là method set flag là sai.
- `DeferNextLootDecisionRequest`1308–1361 đã đợi actual modal clear, natural frame và revalidate **lootTransactionCounter**, expected encounter, Hero/state trước canonical TryPresent. Existing normal first-loot và subsequent decisions có pattern này.

**Technical correction chosen for lab:** bỏ first-loot dequeue/request riêng trong finishing branch; enter/notify LootPending và dùng canonical TryPresent khi active/queued modals clear, hoặc existing deferred wake path với đúng captured encounter/loot token. Giữ item trong queue đến khi thật sự present. Chỉ flip flag sửa được refusal đã thấy nhưng giữ bypass modal gating; chỉ TryPresent rồi return khi modal bận có thể strand, nên phải có wake owner. Không yêu cầu broad coroutine/state-machine refactor.

Preserve natural finishing caster logic, typed transition-vs-loot counters, close-before-side-effects, pending-item validity guards, sequential loot, Hero cancellation, completed-wave growth và reward authorities. Duplicate completion chỉ kiểm chứng trong decision-closed window trước next request; không suy thành chống mọi stale pointer hoặc direct Resource API duplicate. Không cancel caster/reset counter để ép PASS, không Stage hook, fake readiness hoặc new persistence.

## 6. Wrapper/harness chưa đạt safety của assigned scope

Wrapper dùng `AllowRunningUnity` vô điều kiện ở Backup/Restore và có branch warning đề xuất bypass; trái prompt đã giao. Không có warning chứng minh conflict thật trong hai retained runs, nhưng source không được chạy lại nguyên trạng. Wrapper thiếu exact resolved lab path/profile/key validation trước launch, chỉ log3hash thay vì compare12baseline, ErrorActionContinue, fresh per-attempt backup, retained guard child streams, exit gate theo Restore/Compare/source/identity/cleanup, WaitForExit trước restore khi watchdog. Restore failure có thể không làm wrapper fail nếu Unityexit0. Không kill/close Owner Unity hoặc sửa shared guard để né conflict.

Runner thiếu reject already-playing trước NewScene, product/path checks chỉ prefix/substring, anonymous watcher không detach ownership và có public coroutine bypass; recheck cần cả post-domain-reload/PlayMode boundary. Có global **ModalCoordinator.ResetInstance**, setup ngoài try/finally và không explicit owned partial teardown/config disposal/coroutine/listener cleanup. Engine shutdown không thay cleanup evidence. Fix trước new launch; chỉ destroy owned fixture, không global reset/shutdown/unowned manager reuse.

Current final lab journal: **846bytes / AF0E9D1AEEA62E07714BFA8F5B0DBD16CFA9FE4DB99A116E69ED35A6F00C593E**, exact lab RegSubKey; OriginallyExistedFalse, Snapshot.ExistsFalse, ValueCount0, StatusVERIFIED, Timestamp21:07:45, RestoredTimestamp21:11:49; BackupFile/Hashnull. Đây là metadata của final absent lab baseline cùng wrapper summaries. Hai attempts dùng cùng backupDir nên first journal không còn. Không có full raw child SaveGuard transcript hoặc live Owner-save comparison. Không promote thành all-exception/full recovery proof; không sửa lại old recovery history.

## 7. Correction metadata / provenance, originals giữ nguyên

- ResourceManager actual **4729bytes / E5BB03C03B5EA1ADCE10251F1CCE6A6472E8D8795C40AE0779B3288AC34EA12C**, không7163/7845… trong report.
- Hero actual **16275bytes / DFFDEBEEF37832278DD0D801D046681A2B97897AE6CA2B84428D5AF551A708E1**, không8FDD….
- E: HEAD7441b9e hiện hành, report5eafa stale; compile PID12100, không23780. Các correction chỉ ở addendum này, không chỉnh raw/original docs.
- 12runtime hiện E:/lab/R1 khớp. Reused P09 guard35548bytes/hashAFD8EE2E23A17995879DCE4578629D98643561090F8F03F061C1306BA6D41997; core15668bytes/hash0E52435071274A0D437B2267231AE23DAA02C628E51173D4EFC8DF647080744A đều khớp E:.
- Original runner17102/hashD55D93590CD05DF960F1038B95E7DB6471BF6581ACD455E984CB42A363EB211B; wrapper8742/hash01F5FF0155305CA66776A2716BFC582F0B81A22C396EDBFDD6B2FB04F2794837; settings24402/hash0880EB33A406B4A2DDEA8B418219855C763AFDFA17DD99ED9E930843BA7B8FD3. Corrective phải preserve byte copies trước edit, không nhầm failed baseline với corrected build.
- Ba dirty E: docs vẫn đúng historical fingerprints: MANUAL_P09A_CHECKLIST16177/09C34360D9E3C93FFEC5A2043A5E2762FB4D06E7C574D6A5B40457BC22DF4004; ACTIVE6358/842B4FED974040C4F69F947C82585700502ACE0594E822DE70A66A4FAB728B48; REVIEW_RESPONSE16925/E9143AE8E923EFEAE091E2D0BA49D2F7845D3950EE01D271E117C69D2B4E4332. Không overwrite dirty memory ở E: để sync.

## 8. Acceptance boundary cho bước tiếp

Một guarded corrective session sau safety/source patch, cap300s cả import/compile, trong exact lab với profile mới riêng và backup/output fresh. Cases: first finishing loot với real effect; hai actual drops/unique IDs/exactly-one effect và advance sau modal drain; real active/queued modal first-handoff wait/wake; natural no-loot boundary; targeted stale/Hero cancellation cho deferred path sửa. Telemetry request/terminal reason/frame/context/transaction/result/effect/advance và explicit teardown. No reflection repair, manual completion hoặc forced loot presentation.

PASS chỉ có nghĩa những cases thật sự reached/executed cùng guard/cleanup proof trong lab đã đạt; không Stage integration/production/Owner visual acceptance. FAIL/NOT_REACHED/SETUP_BLOCKED/TIMEOUT/compile hoặc cleanup failure trả riêng, stop; không retest loop. Nếu bị Unity conflict, giữ preparation/diff, trả exact blocker và không đụng Owner editor/save.

Sau packet, Codex review diff/evidence rồi mới chốt continuation. Hiện chưa có complete approved implementation roadmap; Stage Gate còn technical lab/dependency candidate. Production scope/exposure/persistence và product decisions chỉ chốt sau real bounded proof, không suy từ proposal hoặc lab approval.

## 9. Locked history giữ nguyên

P08/P09-A/P09-B **ACCEPTED / LOCKED** trong scope; S01–S05 và M1–M7/GUI **USER_VERIFIED**. P08Gate2 **135/137exit1**/legacy07,09; P09-AE2 **SCENARIO_PASS_EXIT_TIMEOUT/wrapperexit2**; oldPID25360 **USER_REPORTED_PASS / RESTORE_UNRESOLVED**. F-SAVE-P09B-01 **CLOSED / RECOVERY ACCEPTED** theo snapshot04/10 15:14:04+07/24metadata values; raw Restore/execution **PARTIAL**, Compare/preflight **METADATA_ONLY**, không new live Registryfullbytes.

F-RECYCLE-GOLD-ONLY-01 và F-RECYCLE-HARNESS-SAFE-ENTRY-01 closeouts giữ scope/evidence limits; A10Gold-onlyLOCKED, current Goldformula preserved implementation/TBDbalance. Historical61skillassets và current30Data+30Resources counts giữ riêng, không rollout. Không mở Boss/Material/Bun/offline/Companion/balance/UIpolish hoặc reopening old accepted scopes.
