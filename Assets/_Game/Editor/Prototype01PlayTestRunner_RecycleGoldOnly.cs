#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
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
    public class DummyModalView_Recycle : IModalView
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
        [MenuItem("Tools/Wuxia RPG/Recycle/Run Recycle Gold Only Tests (R1 - R5)")]
        public static bool RunAllRecycleGoldOnlyTests()
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

        public static void RunRecycleGoldOnlyCLI()
        {
            try
            {
                bool passed = RunAllRecycleGoldOnlyTests();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(passed ? 0 : 1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        // =========================================================================
        // R1: Resource Result: Valid item -> Gold delta matches formula once,
        // tuple matGain = 0, Material and EXP unchanged, inventory handles identity
        // =========================================================================
        private static bool R1_ResourceResult_ValidItem_AwardsGoldOnly_MaterialAndExpUnchanged()
        {
            SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog);
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
            SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog);
            bool pass = false;

            try
            {
                int gold0 = 1500;
                int mat0 = 20;
                res.SetResources(gold0, mat0);

                // 1. Null item check directly on DismantleEquipment
                var (nullGold, nullMat) = res.DismantleEquipment(null);
                bool nullHandled = (nullGold == 0) && (nullMat == 0) && (res.Gold == gold0) && (res.Material == mat0);

                // 2. BattleManager guard: CompleteLootDecisionAndResume when no loot is pending
                GameObject bmGO = new GameObject("Test_BattleManager_R2");
                BattleManager bm = bmGO.AddComponent<BattleManager>();

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

                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            finally
            {
                TeardownTestEnvironment(rootGO);
            }

            return pass;
        }

        // =========================================================================
        // R3: Sequential Loot & Modal: At least two items, Tách first item awards
        // Gold only, duplicate click rejected, next item remains intact in queue
        // =========================================================================
        private static bool R3_SequentialLootModal_DismantleFirstItem_DuplicateRejected_NextItemIntact()
        {
            SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog);
            bool pass = false;
            GameObject coordGO = null;
            DummyModalView_Recycle dummyView = null;
            ModalCoordinator coord = null;

            try
            {
                int gold0 = 3000;
                int mat0 = 10;
                res.SetResources(gold0, mat0);

                GameObject bmGO = new GameObject("Test_BattleManager_R3");
                BattleManager bm = bmGO.AddComponent<BattleManager>();

                // Set up ModalCoordinator to simulate active modal blocking next loot presentation
                coord = ModalCoordinator.Instance;
                if (coord == null)
                {
                    coordGO = new GameObject("ModalCoordinator_R3");
                    coord = coordGO.AddComponent<ModalCoordinator>();
                }
                dummyView = new DummyModalView_Recycle { ModalId = "TestModal_R3" };
                coord.RegisterModalView(dummyView);

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

                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            finally
            {
                if (coord != null && dummyView != null)
                {
                    coord.UnregisterModalView(dummyView);
                }
                if (coordGO != null)
                {
                    UnityEngine.Object.DestroyImmediate(coordGO);
                }
                ModalCoordinator.ResetInstance();
                TeardownTestEnvironment(rootGO);
            }

            return pass;
        }

        // =========================================================================
        // R4: Unaffected Resources: AddMaterial API still works from valid sources,
        // ConsumeResources handles costs accurately, Material system preserved
        // =========================================================================
        private static bool R4_UnaffectedResources_AddMaterialAndConsumeStillFunctional()
        {
            SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog);
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
        // PlayerPrefs reflects Gold change while Material remains exact
        // =========================================================================
        private static bool R5_PersistenceAndCleanup_GoldPersistedViaSaveState_ListenersClean()
        {
            SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog);
            bool pass = false;

            // Preserve existing PlayerPrefs values before test
            int origPrefGold = PlayerPrefs.GetInt("TLTD_Gold", 0);
            int origPrefMat = PlayerPrefs.GetInt("TLTD_Material", 0);

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
                Action<int, int> testListener = (g, m) => { eventFireCount++; };
                res.OnResourcesChanged += testListener;
                res.AddGold(100);
                bool listenerFired = (eventFireCount == 1);
                res.OnResourcesChanged -= testListener;
                res.AddGold(100);
                bool listenerCleaned = (eventFireCount == 1);

                pass = goldPersisted && matPersistedUnchanged && listenerFired && listenerCleaned;
                Debug.Log($"[R5 PERSISTENCE & CLEANUP] GoldPersisted={goldPersisted} ({savedGold}=={expectedGold}), MatPersistedUnchanged={matPersistedUnchanged} ({savedMat}=={expectedMat}), ListenerFired={listenerFired}, ListenerCleaned={listenerCleaned} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                // Restore original PlayerPrefs values
                PlayerPrefs.SetInt("TLTD_Gold", origPrefGold);
                PlayerPrefs.SetInt("TLTD_Material", origPrefMat);
                PlayerPrefs.Save();
                TeardownTestEnvironment(rootGO);
            }

            return pass;
        }

        // =========================================================================
        // Test Environment Helpers
        // =========================================================================
        private static void SetupTestEnvironment(out GameObject rootGO, out ResourceManager res, out Inventory.Inventory inv, out ProgressionManager prog)
        {
            rootGO = new GameObject("TestRoot_RecycleGoldOnly");
            res = rootGO.AddComponent<ResourceManager>();
            inv = rootGO.AddComponent<Inventory.Inventory>();
            prog = rootGO.AddComponent<ProgressionManager>();

            // Reset singletons to point to our test instances
            ResourceManager.ResetInstance();
            Inventory.Inventory.ResetInstance();
            ProgressionManager.ResetInstance();
        }

        private static void TeardownTestEnvironment(GameObject rootGO)
        {
            if (rootGO != null)
            {
                UnityEngine.Object.DestroyImmediate(rootGO);
            }
            ResourceManager.ResetInstance();
            Inventory.Inventory.ResetInstance();
            ProgressionManager.ResetInstance();
        }
    }
}
#endif
