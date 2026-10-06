#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Drop;
using WuxiaGame.Entities;
using WuxiaGame.Equipment;
using WuxiaGame.Inventory;
using WuxiaGame.Items;
using WuxiaGame.Progression;
using WuxiaGame.UI.Modal;

namespace WuxiaGame.Editor
{
    internal class DummyModalView_Recycle : IModalView
    {
        public string ModalId { get; set; }
        public ModalPriority DefaultPriority => ModalPriority.Informational;
        public bool IsDismissable => true;
        public bool IsVisible { get; private set; }
        public void ShowModal(ModalRequest request = null) => IsVisible = true;
        public void HideModal(DismissalReason reason = DismissalReason.UserClosed) => IsVisible = false;
    }

    public static class Prototype01PlayTestRunner_RecycleGoldOnly
    {
        /// <summary>
        /// Public entry point for CLI / BatchMode verification only.
        /// Strictly rejects interactive Editor invocation before ANY fixture creation,
        /// singleton reset, modal action, or PlayerPrefs access.
        /// </summary>
        public static void RunRecycleGoldOnlyCLI()
        {
            // 1. Interactive rejection: must NOT execute interactively in Editor
            if (!Application.isBatchMode)
            {
                Debug.LogError("[F-RECYCLE-HARNESS-SAFE-ENTRY-01] Interactive Editor execution is strictly rejected. This suite mutates persistence/fixtures and must only be executed via the Save Guard batch wrapper.");
                return;
            }

            // 2. Reject PlayMode or pending PlayMode transition
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("[F-RECYCLE-HARNESS-SAFE-ENTRY-01] PlayMode or pending PlayMode transition detected in batch run. Rejecting execution.");
                EditorApplication.Exit(1);
                return;
            }

            // 3. Transient empty scene setup to isolate fixtures from any scene assets (do not save scene asset)
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
            catch (Exception sceneEx)
            {
                Debug.LogError($"[F-RECYCLE-HARNESS-SAFE-ENTRY-01] Failed to create transient empty scene: {sceneEx.Message}");
                EditorApplication.Exit(1);
                return;
            }

            // 4. Verify fresh batch context: reject if unowned singletons exist
            if (ResourceManager.Instance != null || Inventory.Inventory.Instance != null || ProgressionManager.Instance != null || ModalCoordinator.Instance != null)
            {
                Debug.LogError("[F-RECYCLE-HARNESS-SAFE-ENTRY-01] Pre-existing scene singletons detected even in fresh scene. Aborting batch run.");
                EditorApplication.Exit(1);
                return;
            }

            // 5. Execute suite
            try
            {
                bool passed = RunAllRecycleGoldOnlyTests();
                EditorApplication.Exit(passed ? 0 : 1);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Private test suite orchestrator. Only callable by RunRecycleGoldOnlyCLI in batch mode.
        /// </summary>
        private static bool RunAllRecycleGoldOnlyTests()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING F-RECYCLE-GOLD-ONLY-01 TARGETED VERIFICATION SUITE (R1 -> R5)      ");
            Debug.Log("================================================================================");

            int passed = 0;
            int total = 5;

            if (R1_ResourceResult_ValidItem_AwardsGoldOnly_MaterialAndExpUnchanged()) passed++;
            if (R2_RejectedAndNullItem_NoRewardNoMutation_DuplicateCallRejected()) passed++;
            if (R3_SequentialLootModal_DismantleFirstItem_DuplicateRejected_NextItemIntact()) passed++;
            if (R4_UnaffectedResources_AddMaterialAndConsumeStillFunctional()) passed++;
            if (R5_PersistenceAndCleanup_GoldPersistedViaSaveState_ListenersClean()) passed++;

            Debug.Log("--------------------------------------------------------------------------------");
            Debug.Log($"   [F-RECYCLE-GOLD-ONLY-01 RESULTS]: {passed}/{total} PASSED");
            Debug.Log("--------------------------------------------------------------------------------");

            bool allPass = (passed == total);
            Debug.Log($"ALL_PASS={allPass}");
            return allPass;
        }

        // =========================================================================
        // R1: Resource Result: Valid item -> Gold delta matches formula once,
        // tuple matGain = 0, Material and EXP unchanged, inventory handles identity
        // =========================================================================
        private static bool R1_ResourceResult_ValidItem_AwardsGoldOnly_MaterialAndExpUnchanged()
        {
            if (!SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog))
            {
                Debug.LogError("[R1 RESOURCE RESULT] SetupTestEnvironment failed.");
                return false;
            }

            bool pass = false;

            try
            {
                int gold0 = 2000;
                int mat0 = 15;
                res.SetResources(gold0, mat0);
                float exp0 = prog != null ? prog.CurrentExp : 0f;

                // Create a test item with known level = 3
                int itemLevel = 3;
                EquipmentInstance item = new EquipmentInstance("test_item_r1", "Bảo Đao R1", EquipmentSlotType.Weapon, itemLevel, null, new List<AffixInstance>());
                inv.AddItem(item);
                bool itemAdded = inv.HasItem(item);

                // Call DismantleEquipment
                var (goldGain, matGain) = res.DismantleEquipment(item);

                // Expected formula: Mathf.Max(50, itemLevel * 100) = 300
                int expectedGoldGain = Mathf.Max(50, itemLevel * 100);
                bool goldGainCorrect = (goldGain == expectedGoldGain) && (res.Gold == gold0 + expectedGoldGain);
                bool matGainZero = (matGain == 0);
                bool matUnchanged = (res.Material == mat0);
                bool expUnchanged = (prog == null) || (prog.CurrentExp == exp0);
                bool itemRemoved = !inv.HasItem(item);

                pass = itemAdded && goldGainCorrect && matGainZero && matUnchanged && expUnchanged && itemRemoved;
                Debug.Log($"[R1 RESOURCE RESULT] ItemAdded={itemAdded}, GoldGainCorrect={goldGainCorrect} ({gold0}->{res.Gold}), MatGainZero={matGainZero}, MatUnchanged={matUnchanged} ({mat0}->{res.Material}), ExpUnchanged={expUnchanged}, ItemRemoved={itemRemoved} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                TeardownTestEnvironment(rootGO);
            }

            return pass;
        }

        // =========================================================================
        // R2: Rejected & Null Item: Null item returns (0,0) with no mutation;
        // invalid/premature/duplicate loot decision rejected by BattleManager guard
        // =========================================================================
        private static bool R2_RejectedAndNullItem_NoRewardNoMutation_DuplicateCallRejected()
        {
            if (!SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog))
            {
                Debug.LogError("[R2 REJECTED/NULL] SetupTestEnvironment failed.");
                return false;
            }

            bool pass = false;
            GameObject bmGO = null;
            BattleManager bm = null;

            try
            {
                int gold0 = 1500;
                int mat0 = 20;
                res.SetResources(gold0, mat0);

                // 1. Null item check directly on DismantleEquipment
                var (nullGold, nullMat) = res.DismantleEquipment(null);
                bool nullHandled = (nullGold == 0) && (nullMat == 0) && (res.Gold == gold0) && (res.Material == mat0);

                // 2. BattleManager guard: CompleteLootDecisionAndResume when no loot is pending
                bmGO = new GameObject("Test_BattleManager_R2");
                bm = bmGO.AddComponent<BattleManager>();

                // Premature / no-item call must return false
                bool prematureRejected = !bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                bool resourcesUntouchedAfterPremature = (res.Gold == gold0) && (res.Material == mat0);

                // 3. Present legitimate item, decide it, then attempt duplicate call
                EquipmentInstance testItem = new EquipmentInstance("test_item_r2", "Hộ Oản R2", EquipmentSlotType.Gloves, 2, null, new List<AffixInstance>());
                typeof(BattleManager).GetField("pendingLootItem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bm, testItem);
                typeof(BattleManager).GetField("_isLootDecisionOpen", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bm, true);
                typeof(BattleManager).GetProperty("CurrentBattleState").SetValue(bm, BattleState.LootPending);

                bool firstDecisionSuccess = bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                int expectedFirstGold = gold0 + Mathf.Max(50, 2 * 100);
                bool firstDecisionAwardedCorrectly = firstDecisionSuccess && (res.Gold == expectedFirstGold) && (res.Material == mat0);

                // Stale duplicate call to CompleteLootDecisionAndResume must be rejected by guard
                bool duplicateRejected = !bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                bool noExtraRewardOnDuplicate = (res.Gold == expectedFirstGold) && (res.Material == mat0);

                pass = nullHandled && prematureRejected && resourcesUntouchedAfterPremature &&
                       firstDecisionAwardedCorrectly && duplicateRejected && noExtraRewardOnDuplicate;

                Debug.Log($"[R2 REJECTED/NULL] NullHandled={nullHandled}, PrematureRejected={prematureRejected}, ResourcesUntouched={resourcesUntouchedAfterPremature}, FirstDecided={firstDecisionAwardedCorrectly}, DupRejected={duplicateRejected}, NoExtraReward={noExtraRewardOnDuplicate} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (bm != null)
                {
                    CleanupBattleManagerMonsters(bm);
                }
                if (bmGO != null)
                {
                    UnityEngine.Object.DestroyImmediate(bmGO);
                    bmGO = null;
                }
                TeardownTestEnvironment(rootGO);
            }

            // Verify owned fixture cleanup count is 0
            bool fixtureClean = (bmGO == null);
            if (!fixtureClean)
            {
                Debug.LogError("[R2 REJECTED/NULL] Fixture cleanup failed: bmGO reference not cleanly destroyed.");
                pass = false;
            }

            return pass;
        }

        // =========================================================================
        // R3: Sequential Loot & Modal: At least two items, Tách first item awards
        // Gold only, duplicate click rejected, next item remains intact in queue
        // =========================================================================
        private static bool R3_SequentialLootModal_DismantleFirstItem_DuplicateRejected_NextItemIntact()
        {
            if (!SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog))
            {
                Debug.LogError("[R3 SEQUENTIAL LOOT] SetupTestEnvironment failed.");
                return false;
            }

            // Fresh modal coordinator check: reject if unowned coordinator exists
            if (ModalCoordinator.Instance != null)
            {
                Debug.LogError("[R3 SEQUENTIAL LOOT] Pre-existing ModalCoordinator detected. Aborting to protect unowned modal state.");
                TeardownTestEnvironment(rootGO);
                return false;
            }

            bool pass = false;
            GameObject bmGO = null;
            BattleManager bm = null;
            GameObject coordGO = null;
            ModalCoordinator coord = null;
            DummyModalView_Recycle dummyView = null;
            bool modalRegistered = false;

            try
            {
                int gold0 = 3000;
                int mat0 = 10;
                res.SetResources(gold0, mat0);

                bmGO = new GameObject("Test_BattleManager_R3");
                bm = bmGO.AddComponent<BattleManager>();

                coordGO = new GameObject("ModalCoordinator_R3");
                coord = coordGO.AddComponent<ModalCoordinator>();

                dummyView = new DummyModalView_Recycle { ModalId = "TestModal_R3" };
                coord.RegisterModalView(dummyView);
                modalRegistered = true;

                // Open blocking modal before completing A so that B is held in queue awaiting presentation
                var req = new ModalRequest("TestModal_R3", ModalPriority.SystemProgression, true, null);
                coord.RequestModal(req);

                EquipmentInstance itemA = new EquipmentInstance("item_A", "Kiếm A", EquipmentSlotType.Weapon, 1, null, new List<AffixInstance>());
                EquipmentInstance itemB = new EquipmentInstance("item_B", "Giáp B", EquipmentSlotType.Armor, 2, null, new List<AffixInstance>());

                // Queue item B into internal loot queue via EnqueuePendingLoot
                bm.EnqueuePendingLoot(itemB);

                // Present item A
                typeof(BattleManager).GetField("pendingLootItem", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bm, itemA);
                typeof(BattleManager).GetField("_isLootDecisionOpen", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bm, true);
                typeof(BattleManager).GetProperty("CurrentBattleState").SetValue(bm, BattleState.LootPending);

                // 1. Dismantle item A
                bool aHandled = bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                int expectedGoldAfterA = gold0 + Mathf.Max(50, 1 * 100);
                bool aAwardedGoldOnly = aHandled && (res.Gold == expectedGoldAfterA) && (res.Material == mat0);

                // 2. Duplicate click during modal interval must be rejected
                bool duplicateRejected = !bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                bool goldUnchangedOnDup = (res.Gold == expectedGoldAfterA) && (res.Material == mat0);

                // 3. Verify item B remains intact in queue
                var queueField = typeof(BattleManager).GetField("_pendingLootQueue", BindingFlags.NonPublic | BindingFlags.Instance);
                Queue<EquipmentInstance> queue = (Queue<EquipmentInstance>)queueField.GetValue(bm);
                bool itemBIntactInQueue = (queue.Count == 1) && (queue.Peek() == itemB);

                // 4. Dismiss blocking modal
                coord.DismissActiveModal(DismissalReason.SystemDismissed);

                // 5. Present item B legitimately
                bool bPresented = bm.TryPresentNextQueuedLoot();
                bool bNowPending = bPresented && (bm.PendingLootItem == itemB) && (queue.Count == 0);

                // 6. Dismantle item B
                bool bHandled = bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                int expectedGoldAfterB = expectedGoldAfterA + Mathf.Max(50, 2 * 100);
                bool bAwardedGoldOnly = bHandled && (res.Gold == expectedGoldAfterB) && (res.Material == mat0);

                pass = aAwardedGoldOnly && duplicateRejected && goldUnchangedOnDup &&
                       itemBIntactInQueue && bNowPending && bAwardedGoldOnly;

                Debug.Log($"[R3 SEQUENTIAL LOOT] AAwardedGoldOnly={aAwardedGoldOnly}, DupRejected={duplicateRejected}, GoldSafeOnDup={goldUnchangedOnDup}, BIntactInQueue={itemBIntactInQueue}, BPresented={bNowPending}, BAwardedGoldOnly={bAwardedGoldOnly} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                // Clean up only owned dummy view; do NOT call ModalCoordinator.ResetInstance()
                if (coord != null)
                {
                    if (coord.ActiveRequest != null && coord.ActiveRequest.ModalId == "TestModal_R3")
                    {
                        coord.DismissActiveModal(DismissalReason.SystemDismissed);
                    }
                    if (dummyView != null && modalRegistered)
                    {
                        coord.UnregisterModalView(dummyView);
                    }
                }

                if (bm != null)
                {
                    CleanupBattleManagerMonsters(bm);
                }
                if (bmGO != null)
                {
                    UnityEngine.Object.DestroyImmediate(bmGO);
                    bmGO = null;
                }
                if (coordGO != null)
                {
                    UnityEngine.Object.DestroyImmediate(coordGO);
                    coordGO = null;
                }

                TeardownTestEnvironment(rootGO);
            }

            // Verify owned fixture cleanup: bmGO and coordGO destroyed
            bool fixtureClean = (bmGO == null) && (coordGO == null) && (ModalCoordinator.Instance == null);
            if (!fixtureClean)
            {
                Debug.LogError("[R3 SEQUENTIAL LOOT] Fixture cleanup failed: owned references not destroyed.");
                pass = false;
            }

            return pass;
        }

        // =========================================================================
        // R4: Unaffected Resources: AddMaterial API still works from valid sources,
        // ConsumeResources handles costs accurately, Material system preserved
        // =========================================================================
        private static bool R4_UnaffectedResources_AddMaterialAndConsumeStillFunctional()
        {
            if (!SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog))
            {
                Debug.LogError("[R4 UNAFFECTED RESOURCES] SetupTestEnvironment failed.");
                return false;
            }

            bool pass = false;

            try
            {
                res.SetResources(1000, 50);

                // 1. AddMaterial still functions correctly for external callers
                res.AddMaterial(25);
                bool addMaterialWorks = (res.Material == 75);

                // 2. ConsumeResources consumes both Gold and Material accurately
                bool consumeSuccess = res.ConsumeResources(400, 20);
                bool consumeAccurate = consumeSuccess && (res.Gold == 600) && (res.Material == 55);

                // 3. Insufficient resource consumption rejected safely
                bool insufficientRejected = !res.ConsumeResources(1000, 10);
                bool resourcesUnmodified = (res.Gold == 600) && (res.Material == 55);

                pass = addMaterialWorks && consumeAccurate && insufficientRejected && resourcesUnmodified;
                Debug.Log($"[R4 UNAFFECTED RESOURCES] AddMaterialWorks={addMaterialWorks} (50+25=75), ConsumeAccurate={consumeAccurate} (Gold:600, Mat:55), InsufficientRejected={insufficientRejected}, ResourcesUnmodified={resourcesUnmodified} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                TeardownTestEnvironment(rootGO);
            }

            return pass;
        }

        // =========================================================================
        // R5: Persistence & Cleanup: Gold-only record goes through SaveState authority,
        // PlayerPrefs reflects Gold change while Material remains exact.
        // Preserves key-absence semantics and cleans listeners in finally.
        // =========================================================================
        private static bool R5_PersistenceAndCleanup_GoldPersistedViaSaveState_ListenersClean()
        {
            if (!SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog))
            {
                Debug.LogError("[R5 PERSISTENCE & CLEANUP] SetupTestEnvironment failed.");
                return false;
            }

            bool pass = false;

            // Capture exact presence and value of local test keys before mutation to preserve key-absence semantics.
            // NOTE: This local key restoration provides in-test state isolation only and is NOT a full baseline recovery.
            // Authoritative baseline recovery and diff=0 verification are strictly performed by the outer Save Guard wrapper.
            bool hadPrefGold = PlayerPrefs.HasKey("TLTD_Gold");
            int origPrefGold = hadPrefGold ? PlayerPrefs.GetInt("TLTD_Gold") : 0;
            bool hadPrefMat = PlayerPrefs.HasKey("TLTD_Material");
            int origPrefMat = hadPrefMat ? PlayerPrefs.GetInt("TLTD_Material") : 0;

            Action<int, int> testListener = null;
            bool listenerSubscribed = false;

            try
            {
                int testGold = 7777;
                int testMat = 88;
                res.SetResources(testGold, testMat);

                // Create and dismantle an item
                EquipmentInstance item = new EquipmentInstance("persist_item", "Ngọc Bội", EquipmentSlotType.Accessory, 4, null, new List<AffixInstance>());
                res.DismantleEquipment(item);

                int expectedGold = testGold + Mathf.Max(50, 4 * 100); // 7777 + 400 = 8177
                int expectedMat = testMat; // 88 (unchanged)

                // Read persisted values from PlayerPrefs
                int savedGold = PlayerPrefs.GetInt("TLTD_Gold", -1);
                int savedMat = PlayerPrefs.GetInt("TLTD_Material", -1);

                bool goldPersisted = (savedGold == expectedGold);
                bool matPersistedUnchanged = (savedMat == expectedMat);

                // Listener cleanup check
                int eventFireCount = 0;
                testListener = (g, m) => { eventFireCount++; };
                res.OnResourcesChanged += testListener;
                listenerSubscribed = true;

                res.AddGold(100);
                bool listenerFired = (eventFireCount == 1);

                res.OnResourcesChanged -= testListener;
                listenerSubscribed = false;

                res.AddGold(100);
                bool listenerCleaned = (eventFireCount == 1);

                pass = goldPersisted && matPersistedUnchanged && listenerFired && listenerCleaned;
                Debug.Log($"[R5 PERSISTENCE & CLEANUP] GoldPersisted={goldPersisted} ({savedGold}=={expectedGold}), MatPersistedUnchanged={matPersistedUnchanged} ({savedMat}=={expectedMat}), ListenerFired={listenerFired}, ListenerCleaned={listenerCleaned} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                // Exception-safe listener unsubscription
                if (listenerSubscribed && res != null && testListener != null)
                {
                    res.OnResourcesChanged -= testListener;
                    listenerSubscribed = false;
                }

                // Preserve local key existence / absence semantics
                if (hadPrefGold)
                {
                    PlayerPrefs.SetInt("TLTD_Gold", origPrefGold);
                }
                else
                {
                    PlayerPrefs.DeleteKey("TLTD_Gold");
                }

                if (hadPrefMat)
                {
                    PlayerPrefs.SetInt("TLTD_Material", origPrefMat);
                }
                else
                {
                    PlayerPrefs.DeleteKey("TLTD_Material");
                }
                PlayerPrefs.Save();

                TeardownTestEnvironment(rootGO);
            }

            // Verify listener teardown
            if (listenerSubscribed)
            {
                Debug.LogError("[R5 PERSISTENCE & CLEANUP] Listener cleanup failed: listener remained subscribed.");
                pass = false;
            }

            return pass;
        }

        // =========================================================================
        // Test Environment Helpers
        // =========================================================================
        private static bool SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog)
        {
            rootGO = null;
            res = null;
            inv = null;
            prog = null;

            // Reject if unowned singletons already exist in the environment
            if (ResourceManager.Instance != null || Inventory.Inventory.Instance != null || ProgressionManager.Instance != null)
            {
                Debug.LogError("[SETUP] Pre-existing unowned singleton detected in environment prior to test setup. Refusing to reuse or reset external state.");
                return false;
            }

            GameObject createdRoot = null;
            try
            {
                createdRoot = new GameObject("TestRoot_RecycleGoldOnly");
                res = createdRoot.AddComponent<ResourceManager>();
                inv = createdRoot.AddComponent<Inventory.Inventory>();
                prog = createdRoot.AddComponent<ProgressionManager>();

                rootGO = createdRoot;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SETUP] Exception during SetupTestEnvironment: {ex.Message}");
                if (createdRoot != null)
                {
                    UnityEngine.Object.DestroyImmediate(createdRoot);
                }
                rootGO = null;
                res = null;
                inv = null;
                prog = null;
                return false;
            }
        }

        private static void TeardownTestEnvironment(GameObject rootGO)
        {
            if (rootGO != null)
            {
                UnityEngine.Object.DestroyImmediate(rootGO);
            }
        }

        private static void CleanupBattleManagerMonsters(BattleManager bm)
        {
            if (bm == null) return;
            try
            {
                if (bm.ActiveMonsters != null)
                {
                    var monsters = new List<Monster>(bm.ActiveMonsters);
                    foreach (var m in monsters)
                    {
                        if (m != null && m.gameObject != null)
                        {
                            UnityEngine.Object.DestroyImmediate(m.gameObject);
                        }
                    }
                }
                if (bm.CurrentMonster != null && bm.CurrentMonster.gameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(bm.CurrentMonster.gameObject);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[CLEANUP] Exception cleaning up BattleManager monsters: {ex.Message}");
            }
        }
    }
}
#endif
