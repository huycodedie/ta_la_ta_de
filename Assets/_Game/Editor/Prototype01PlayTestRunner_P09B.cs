#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Entities.Components;
using WuxiaGame.Equipment;
using WuxiaGame.Inventory;
using WuxiaGame.Progression;
using WuxiaGame.Stats;

namespace WuxiaGame.Editor
{
    public static class Prototype01PlayTestRunner_P09B
    {
        private static string _savedActiveMmId;

        [MenuItem("Tools/Wuxia RPG/Run P09-B Dash Foundation Tests (T01 - T16)")]
        public static bool RunAllP09BTests()
        {
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "P09-B Safe Execution Notice",
                    "Running P09-B foundation tests directly in the interactive Editor mutates PlayerPrefs.\n\n" +
                    "To safely run these tests with full Save Guard isolation (pre-run backup, journal check, and exact restore), please execute the verification wrapper:\n\n" +
                    "  Tools\\Verification\\P09B\\run_gate1_p09b.ps1\n\n" +
                    "Direct Editor execution is blocked to protect your workspace persistence.",
                    "OK"
                );
                return false;
            }

            return RunAllP09BTestsInternal();
        }

        public static bool RunAllP09BTestsInternal()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING P09-B DASH FOUNDATION AUTOMATED SUITE (T01 -> T16)");
            Debug.Log("================================================================================");

            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
            }

            int passed = 0;
            int total = 16;

            bool t01 = T01_DataValidation_RejectsInvalidConfig_NaN_Infinity_MixedEffects();
            if (t01) passed++;

            bool t02 = T02_CC_Stun_Root_Freeze_And_Casting_BlockStart_BeforeResource();
            if (t02) passed++;

            bool t03 = T03_ZeroUsefulDistance_Or_AlreadyInAttackRange_RejectedBeforeResource();
            if (t03) passed++;

            bool t04 = T04_StartupFalse_Or_Exception_RollbacksRage_And_NoCooldown();
            if (t04) passed++;

            bool t05 = T05_ValidDashStart_CommitsOnce_SingleRage_SingleCooldown_SingleSuccess_ZeroDamage();
            if (t05) passed++;

            bool t06 = T06_ContinuousMultiFrameMovement_ClampsAtEndpoint_NoTeleport_YPreserved();
            if (t06) passed++;

            bool t07 = T07_TargetMovesCloser_StopsEarlyWithoutOvershoot();
            if (t07) passed++;

            bool t08 = T08_TargetDeath_Or_Unregistered_AbortsDashImmediately();
            if (t08) passed++;

            bool t09 = T09_MidDashCC_Stun_Root_Freeze_AbortsDashImmediately_NoRageRefund();
            if (t09) passed++;

            bool t10 = T10_PauseFreezesDash_ResumeContinuesSmoothly_NoCompensatoryTime();
            if (t10) passed++;

            bool t11 = T11_EncounterTransition_Or_TerminalBattle_CancelsActiveDash();
            if (t11) passed++;

            bool t12 = T12_BasicAttackBlockedDuringDash_AndNotStuckDisabledAfterDash();
            if (t12) passed++;

            bool t13 = T13_ActionBusy_BlocksNewSkill_AndUltimate_DuringDash();
            if (t13) passed++;

            bool t14 = T14_AI_SkipsManualOnlySkill_InBothNormalAndUltimate();
            if (t14) passed++;

            bool t15 = T15_DisableAndReenable_SceneUnload_CleansUpDashState();
            if (t15) passed++;

            bool t16 = T16_DeterministicSimulation_HeroExecuteSelectedSkill_FullCycle();
            if (t16) passed++;

            bool allPass = (passed == total);
            Debug.Log("================================================================================");
            Debug.Log($"   [P09-B AUTOMATED TESTS RESULT]: {passed}/{total} PASSED | ALL_PASS={allPass}");
            Debug.Log("================================================================================");

            return allPass;
        }

        public static void RunGate1_P09B_CLI()
        {
            try
            {
                bool pass = RunAllP09BTestsInternal();
                EditorApplication.Exit(pass ? 0 : 1);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[P09-B CLI FATAL] Exception: {ex}");
                EditorApplication.Exit(1);
            }
        }

        #region Natural Play Mode Runner

        [MenuItem("Tools/Wuxia RPG/Run P09-B Natural Play Mode Scenario")]
        public static void RunPlayModeNatural_P09B()
        {
            if (!Application.isBatchMode && !SessionState.GetBool("P09B_Authorized_PlayMode_Session", false))
            {
                EditorUtility.DisplayDialog(
                    "P09-B Safe Execution Notice",
                    "Running P09-B Natural Play Mode Scenario directly in the interactive Editor mutates PlayerPrefs and switches scenes.\n\n" +
                    "To safely run this scenario with full Save Guard isolation (pre-run backup, journal check, and exact restore), please execute the verification wrapper:\n\n" +
                    "  Tools\\Verification\\P09B\\run_natural_playmode_p09b.ps1\n\n" +
                    "Direct Editor execution is blocked to protect your workspace persistence.",
                    "OK"
                );
                return;
            }

            if (EditorApplication.isPlaying)
            {
                CreateNaturalPlayModeHarnessIfMissing();
            }
            else
            {
                SessionState.SetBool("RunP09BPlayModeScenario", true);
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
                EditorApplication.isPlaying = true;
            }
        }

        public static void RunPlayModeNatural_P09B_CLI()
        {
            Debug.Log("[P09-B PLAY MODE CLI] Initializing natural Play Mode runner under Save Guard protection...");
            RunPlayModeNatural_P09B();
        }

        [InitializeOnLoadMethod]
        private static void RegisterP09BPlayModeWatcher()
        {
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("RunP09BPlayModeScenario", false))
                {
                    SessionState.SetBool("RunP09BPlayModeScenario", false);
                    CreateNaturalPlayModeHarnessIfMissing();
                }
            };
        }

        private static void CreateNaturalPlayModeHarnessIfMissing()
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<P09BPlayModeHarness>();
            if (existing != null) return;

            var go = new GameObject("P09B_Natural_PlayMode_Harness");
            var harness = go.AddComponent<P09BPlayModeHarness>();
            harness.StartCoroutine(harness.RunNaturalScenarioCoroutine());
        }

        #endregion

        #region Helpers

        private static void SetupEncounter(
            out GameObject heroGO, out Hero hero,
            out GameObject bmGO, out BattleManager bm,
            out GameObject mmMgrGO, out MindMethodManager mmMgr)
        {
            MindMethodManager.ResetInstance();
            BattleManager.ResetInstance();
            EquipmentManager.ResetInstance();
            Inventory.Inventory.ResetInstance();
            ResourceManager.ResetInstance();
            ProgressionManager.ResetInstance();
            EventBus.ClearAllListeners();

            _savedActiveMmId = PlayerPrefs.HasKey("TLTD_MM_ActiveId") ? PlayerPrefs.GetString("TLTD_MM_ActiveId") : null;

            GameObject servicesGO = new GameObject("TestServices_P09B");
            servicesGO.AddComponent<EquipmentManager>();
            servicesGO.AddComponent<Inventory.Inventory>();
            servicesGO.AddComponent<ResourceManager>();
            servicesGO.AddComponent<ProgressionManager>();

            mmMgrGO = new GameObject("TestMindMethodManager_P09B");
            mmMgr = mmMgrGO.AddComponent<MindMethodManager>();

            var tempDb = ScriptableObject.CreateInstance<MindMethodDatabaseSO>();
            var tempMmDef = ScriptableObject.CreateInstance<MindMethodDefinitionSO>();
            tempMmDef.InitializeMindMethod("mm_taiji", "Taiji", "Test Taiji", 10, true, new MindMethodPassiveData());
            tempDb.SetMindMethods(new List<MindMethodDefinitionSO> { tempMmDef });
            mmMgr.SetDatabase(tempDb);
            mmMgr.SetActiveMindMethod("mm_taiji");
            typeof(MindMethodManager).GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.SetValue(null, mmMgr);

            heroGO = new GameObject("TestHero_P09B");
            hero = heroGO.AddComponent<Hero>();
            hero.InitializeHero();
            heroGO.transform.position = new Vector3(-3f, -0.3f, 0f);

            bmGO = new GameObject("TestBM_P09B");
            bm = bmGO.AddComponent<BattleManager>();
            bm.RegisterHero(hero);
            bm.StartBattle();
        }

        private static void TeardownEncounter(GameObject heroGO, GameObject bmGO, GameObject mmMgrGO)
        {
            if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
            if (bmGO != null)
            {
                var bm = bmGO.GetComponent<BattleManager>();
                if (bm != null && bm.ActiveMonsters != null)
                {
                    var copy = new List<Monster>(bm.ActiveMonsters);
                    foreach (var m in copy)
                    {
                        if (m != null) UnityEngine.Object.DestroyImmediate(m.gameObject);
                    }
                }
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            if (mmMgrGO != null) UnityEngine.Object.DestroyImmediate(mmMgrGO);

            GameObject servicesGO = GameObject.Find("TestServices_P09B");
            if (servicesGO != null) UnityEngine.Object.DestroyImmediate(servicesGO);

            MindMethodManager.ResetInstance();
            BattleManager.ResetInstance();
            EquipmentManager.ResetInstance();
            Inventory.Inventory.ResetInstance();
            ResourceManager.ResetInstance();
            ProgressionManager.ResetInstance();
            EventBus.ClearAllListeners();

            if (_savedActiveMmId != null)
            {
                PlayerPrefs.SetString("TLTD_MM_ActiveId", _savedActiveMmId);
            }
            else
            {
                PlayerPrefs.DeleteKey("TLTD_MM_ActiveId");
            }
            PlayerPrefs.DeleteKey("TLTD_MM_Unlocked_mm_taiji");
            PlayerPrefs.DeleteKey("TLTD_MM_Level_mm_taiji");
            for (int i = 1; i <= 5; i++)
            {
                PlayerPrefs.DeleteKey($"TLTD_MM_Slot_mm_taiji_{i}");
            }
            PlayerPrefs.Save();
        }

        private static SkillDefinitionSO CreateTestDashSkill(
            string skillId,
            string name,
            float distance = 4.0f,
            float speed = 12.0f,
            float rageCost = 20.0f,
            float cd = 5.0f,
            bool isManualOnly = false,
            SkillSlotType slotType = SkillSlotType.Skill)
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            skill.InitializeSkill(
                id: skillId,
                mmId: "mm_taiji",
                slot: slotType,
                name: name,
                desc: "P09-B Test Dash Skill",
                conditions: null,
                dmgMultiplier: 0f,
                costRage: rageCost,
                cd: cd,
                passive: false,
                skillEffects: null,
                shatterFreeze: false,
                skillCastTime: 0f,
                channel: false,
                chDuration: 0f,
                chTickInterval: 0f,
                skillPriority: 50,
                projectile: false,
                projSpeed: 0f,
                projLifetime: 0f
            );
            skill.ConfigureDash(true, distance, speed, isManualOnly);
            return skill;
        }

        private static void RegisterAndSelectSkill(MindMethodManager mmMgr, SkillDefinitionSO skill)
        {
            var activeDef = mmMgr.ActiveMindMethodDefinition;
            if (activeDef != null)
            {
                var skillsField = typeof(MindMethodDefinitionSO).GetField("skills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var list = skillsField != null ? skillsField.GetValue(activeDef) as List<SkillDefinitionSO> : null;
                if (list != null && !list.Contains(skill))
                {
                    list.Add(skill);
                }
            }

            var activeState = mmMgr.ActiveMindMethodState;
            if (activeState != null)
            {
                activeState.SkillStates[skill.SkillId] = new SkillRuntimeState(skill.SkillId, true, 1);
                activeState.SelectedSkillPerSlot[skill.SlotType] = skill.SkillId;
            }
        }

        #endregion

        #region Tests

        /// <summary>
        /// T01: Data Validation - Rejects non-positive, NaN, Infinity, non-Hero, mismatched slot, mixed effects.
        /// </summary>
        private static bool T01_DataValidation_RejectsInvalidConfig_NaN_Infinity_MixedEffects()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(4f, -0.3f, 0f);

                // 1. Distance <= 0
                var sk1 = CreateTestDashSkill("p09b_t01_d0", "Zero Dist", distance: 0f);
                RegisterAndSelectSkill(mmMgr, sk1);
                bool v1 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk1, SkillSlotType.Skill, target), out var r1, out _) &&
                          r1 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 2. Distance NaN
                var sk2 = CreateTestDashSkill("p09b_t01_dnan", "NaN Dist", distance: float.NaN);
                RegisterAndSelectSkill(mmMgr, sk2);
                bool v2 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk2, SkillSlotType.Skill, target), out var r2, out _) &&
                          r2 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 3. Distance Infinity
                var sk3 = CreateTestDashSkill("p09b_t01_dinf", "Inf Dist", distance: float.PositiveInfinity);
                RegisterAndSelectSkill(mmMgr, sk3);
                bool v3 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk3, SkillSlotType.Skill, target), out var r3, out _) &&
                          r3 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 4. Speed <= 0
                var sk4 = CreateTestDashSkill("p09b_t01_s0", "Zero Speed", speed: -5f);
                RegisterAndSelectSkill(mmMgr, sk4);
                bool v4 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk4, SkillSlotType.Skill, target), out var r4, out _) &&
                          r4 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 5. Speed Infinity
                var sk5 = CreateTestDashSkill("p09b_t01_sinf", "Inf Speed", speed: float.PositiveInfinity);
                RegisterAndSelectSkill(mmMgr, sk5);
                bool v5 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk5, SkillSlotType.Skill, target), out var r5, out _) &&
                          r5 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 6. Non-Hero source (Monster trying to dash)
                var validDash = CreateTestDashSkill("p09b_t01_valid", "Valid Dash");
                bool v6 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(target, validDash, SkillSlotType.Skill, hero), out var r6, out _) &&
                          r6 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 7. Dash + Projectile
                var sk7 = CreateTestDashSkill("p09b_t01_proj", "Dash Proj");
                sk7.SetProjectile(true);
                RegisterAndSelectSkill(mmMgr, sk7);
                bool v7 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk7, SkillSlotType.Skill, target), out var r7, out _) &&
                          r7 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 8. Dash + Channel
                var sk8 = CreateTestDashSkill("p09b_t01_chan", "Dash Chan");
                sk8.SetChannel(true, 2f, 0.5f);
                RegisterAndSelectSkill(mmMgr, sk8);
                bool v8 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk8, SkillSlotType.Skill, target), out var r8, out _) &&
                          r8 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 9. Dash + CastTime > 0
                var sk9 = CreateTestDashSkill("p09b_t01_cast", "Dash Cast");
                sk9.SetCastTime(1.5f);
                RegisterAndSelectSkill(mmMgr, sk9);
                bool v9 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk9, SkillSlotType.Skill, target), out var r9, out _) &&
                          r9 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 10. Dash + DamageMultiplier > 0
                var sk10 = CreateTestDashSkill("p09b_t01_dmg", "Dash Dmg");
                typeof(SkillDefinitionSO).GetField("damageMultiplier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(sk10, 1.5f);
                RegisterAndSelectSkill(mmMgr, sk10);
                bool v10 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk10, SkillSlotType.Skill, target), out var r10, out _) &&
                           r10 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 11. Dash + Explicit Effects
                var sk11 = CreateTestDashSkill("p09b_t01_eff", "Dash Eff");
                var dummyEffect = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
                dummyEffect.Initialize(1.0f, SkillTargetPolicy.SingleTarget, DamageType.Skill);
                sk11.AddEffect(dummyEffect);
                RegisterAndSelectSkill(mmMgr, sk11);
                bool v11 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk11, SkillSlotType.Skill, target), out var r11, out _) &&
                           r11 == SkillExecutionFailureReason.InvalidDeliveryConfiguration;

                // 12. Slot NormalAttack or Ultimate rejected
                var sk12 = CreateTestDashSkill("p09b_t01_ult", "Dash Ult", slotType: SkillSlotType.Ultimate);
                RegisterAndSelectSkill(mmMgr, sk12);
                bool v12 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk12, SkillSlotType.Ultimate, target), out var r12, out _) &&
                           (r12 == SkillExecutionFailureReason.InvalidDeliveryConfiguration || r12 == SkillExecutionFailureReason.SkillNotSelected);

                // 13. Mismatched slot (request slot ExternalSkill1 vs skill slot Skill)
                var sk13 = CreateTestDashSkill("p09b_t01_mismatch", "Dash Mismatch", slotType: SkillSlotType.Skill);
                RegisterAndSelectSkill(mmMgr, sk13);
                bool v13 = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, sk13, SkillSlotType.ExternalSkill1, target), out var r13, out _) &&
                           (r13 == SkillExecutionFailureReason.InvalidDeliveryConfiguration || r13 == SkillExecutionFailureReason.SkillNotSelected);

                bool pass = v1 && v2 && v3 && v4 && v5 && v6 && v7 && v8 && v9 && v10 && v11 && v12 && v13;
                Debug.Log($"[T01 Details] v1={v1}({r1}), v2={v2}({r2}), v3={v3}({r3}), v4={v4}({r4}), v5={v5}({r5}), v6={v6}({r6}), v7={v7}({r7}), v8={v8}({r8}), v9={v9}({r9}), v10={v10}({r10}), v11={v11}({r11}), v12={v12}({r12}), v13={v13}({r13})");
                Debug.Log($"[T01] Data Validation (NaN, Inf, NonHero, SlotMismatch, Mixed Effects): pass={pass}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T02: CC (Stun, Root, Freeze) and Active Casting block dash start before resource consumption.
        /// </summary>
        private static bool T02_CC_Stun_Root_Freeze_And_Casting_BlockStart_BeforeResource()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(4f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t02_cc", "Dash CC", rageCost: 25f, cd: 4f);
                RegisterAndSelectSkill(mmMgr, dashSkill);

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                // 1. Root check
                hero.StatusController.ApplyCrowdControl("t02_root", CrowdControlType.Root, 2.0f);
                bool canDashRoot = hero.CanDash;
                bool vRoot = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target), out var rRoot, out _);
                bool rootReasonOk = (rRoot == SkillExecutionFailureReason.SourceCrowdControlled);
                bool rootRageUntouched = Mathf.Approximately(hero.Rage.CurrentRage, 100f);
                bool rootNoCd = !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _);
                hero.StatusController.ClearAllCrowdControl();

                // 2. Stun check
                hero.StatusController.ApplyCrowdControl("t02_stun", CrowdControlType.Stun, 2.0f);
                bool vStun = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target), out var rStun, out _);
                bool stunReasonOk = (rStun == SkillExecutionFailureReason.SourceCrowdControlled);
                hero.StatusController.ClearAllCrowdControl();

                // 3. Freeze check
                hero.StatusController.ApplyCrowdControl("t02_freeze", CrowdControlType.Freeze, 2.0f);
                bool vFreeze = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target), out var rFreeze, out _);
                bool freezeReasonOk = (rFreeze == SkillExecutionFailureReason.SourceCrowdControlled);
                hero.StatusController.ClearAllCrowdControl();

                // 4. Casting check
                var dummyReq = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                hero.CastState.StartCast(dummyReq, 1.5f, 0f);
                bool vCasting = !SkillExecutionValidator.Validate(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target), out var rCasting, out _);
                bool castingReasonOk = (rCasting == SkillExecutionFailureReason.SourceAlreadyCasting);
                hero.CastState.Reset();

                bool pass = !canDashRoot && vRoot && rootReasonOk && rootRageUntouched && rootNoCd &&
                            vStun && stunReasonOk && vFreeze && freezeReasonOk && vCasting && castingReasonOk;
                Debug.Log($"[T02] CC & Cast Gating Before Resource: Root={vRoot}, Stun={vStun}, Freeze={vFreeze}, Cast={vCasting}, NoRageLoss={rootRageUntouched} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T03: Zero useful distance or already within attack range is rejected before resource.
        /// Covers right-side, left-side, and pure Z-offset (dx <= AttackRange).
        /// </summary>
        private static bool T03_ZeroUsefulDistance_Or_AlreadyInAttackRange_RejectedBeforeResource()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;

                var dashSkill = CreateTestDashSkill("p09b_t03_close", "Dash Close", distance: 4.0f, rageCost: 20f);
                RegisterAndSelectSkill(mmMgr, dashSkill);

                // Subcase 1: Target to the Right within attack range (x=1.0m, dx=1.0m <= 1.8m)
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(1.0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var req1 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var res1 = SkillExecutor.Execute(req1);
                bool c1 = !res1.Success && res1.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                          Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _) &&
                          !hero.Movement.IsDashing && Mathf.Approximately(hero.transform.position.x, 0f);

                // Subcase 2: Target to the Left within attack range (x=-1.0m, abs(dx)=1.0m <= 1.8m)
                target.transform.position = new Vector3(-1.0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var req2 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var res2 = SkillExecutor.Execute(req2);
                bool c2 = !res2.Success && res2.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                          Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _) &&
                          !hero.Movement.IsDashing && Mathf.Approximately(hero.transform.position.x, 0f);

                // Subcase 3: Target with Pure Z-Offset without useful X distance (x=0m, z=3.0m, dx=0 <= 1.8m)
                target.transform.position = new Vector3(0f, -0.3f, 3.0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var req3 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var res3 = SkillExecutor.Execute(req3);
                bool c3 = !res3.Success && res3.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                          Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _) &&
                          !hero.Movement.IsDashing && Mathf.Approximately(hero.transform.position.x, 0f);

                // Subcase 4: Target with Z-Offset and dx <= 1.8m (x=0.8m, z=4.0m)
                target.transform.position = new Vector3(0.8f, -0.3f, 4.0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var req4 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var res4 = SkillExecutor.Execute(req4);
                bool c4 = !res4.Success && res4.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                          Mathf.Approximately(hero.Rage.CurrentRage, 100f) && !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _) &&
                          !hero.Movement.IsDashing && Mathf.Approximately(hero.transform.position.x, 0f);

                bool pass = c1 && c2 && c3 && c4;
                Debug.Log($"[T03] Zero Useful Distance / In Attack Range Rejection: Right={c1}, Left={c2}, PureZ={c3}, ZWithSmallX={c4} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T04: Startup reservation, safe debit, rollback on exception, and re-entry prevention.
        /// Meets all Section B requirements across 5 distinct production-path test cases.
        /// </summary>
        private static bool T04_StartupFalse_Or_Exception_RollbacksRage_And_NoCooldown()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(5f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t04_rb", "Dash Rollback", rageCost: 30f, cd: 6.0f);
                RegisterAndSelectSkill(mmMgr, dashSkill);

                // -------------------------------------------------------------
                // Case 1: Listener on OnSkillExecutionRequested invalidates target -> rejected before debit
                // -------------------------------------------------------------
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                Action<SkillExecutionRequest> invalidatingListener = (r) =>
                {
                    target.gameObject.SetActive(false);
                };
                EventBus.OnSkillExecutionRequested += invalidatingListener;

                var req1 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var res1 = SkillExecutor.Execute(req1);

                EventBus.OnSkillExecutionRequested -= invalidatingListener;
                target.gameObject.SetActive(true);

                bool case1Pass = !res1.Success && res1.FailureReason == SkillExecutionFailureReason.TargetInvalidOrDead &&
                                 Mathf.Approximately(hero.Rage.CurrentRage, 100f) &&
                                 !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _) &&
                                 !hero.Movement.IsDashing && !hero.Movement.IsStartingDash;
                Debug.Log($"[T04 Case 1] Pre-debit requested mutation: Rejected={!res1.Success} ({res1.FailureReason}), Rage100={Mathf.Approximately(hero.Rage.CurrentRage, 100f)} | Case1Pass={case1Pass}");

                // -------------------------------------------------------------
                // Case 2: Post-debit startup rejection (TryStartDash=false) -> Rage refunded, 0 CD, no leftover state
                // -------------------------------------------------------------
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                // Listener on RageComponent.OnRageChanged disables Movement right when Rage is debited
                Action<float, float> disableMovementOnRageChanged = null;
                disableMovementOnRageChanged = (curRage, maxRage) =>
                {
                    if (curRage < 100f && hero.Movement != null)
                    {
                        hero.Movement.enabled = false;
                    }
                };
                hero.Rage.OnRageChanged += disableMovementOnRageChanged;

                var req2 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var res2 = SkillExecutor.Execute(req2);

                hero.Rage.OnRageChanged -= disableMovementOnRageChanged;
                hero.Movement.enabled = true; // Restore movement for subsequent tests

                bool case2Pass = !res2.Success &&
                                 (res2.FailureReason == SkillExecutionFailureReason.SourceActionBusy || res2.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration || res2.FailureReason == SkillExecutionFailureReason.SourceInvalidOrDead) &&
                                 Mathf.Approximately(hero.Rage.CurrentRage, 100f) && // Full refund verified!
                                 !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _) &&
                                 !hero.Movement.IsDashing && !hero.Movement.IsStartingDash;
                Debug.Log($"[T04 Case 2] Post-debit TryStartDash=false rejection: Rejected={!res2.Success}, RefundedRage100={Mathf.Approximately(hero.Rage.CurrentRage, 100f)}, NoCD={!CooldownManager.IsOnCooldown(dashSkill.SkillId, out _)} | Case2Pass={case2Pass}");

                // -------------------------------------------------------------
                // Case 3: Listener on OnRageChanged throws exception after debit -> caught, refunded, clean state
                // -------------------------------------------------------------
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                Action<float, float> throwingListener = null;
                throwingListener = (curRage, maxRage) =>
                {
                    if (curRage < 100f)
                    {
                        throw new InvalidOperationException("Test deliberate listener throw after debit");
                    }
                };
                hero.Rage.OnRageChanged += throwingListener;

                var req3 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var res3 = SkillExecutor.Execute(req3);

                hero.Rage.OnRageChanged -= throwingListener;

                bool case3Pass = !res3.Success &&
                                 res3.FailureReason == SkillExecutionFailureReason.InvalidDeliveryConfiguration &&
                                 Mathf.Approximately(hero.Rage.CurrentRage, 100f) && // Safe refund verified despite listener throw!
                                 !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _) &&
                                 !hero.Movement.IsDashing && !hero.Movement.IsStartingDash;
                Debug.Log($"[T04 Case 3] OnRageChanged listener throw after debit: Rejected={!res3.Success} ({res3.FailureReason}), RefundedRage100={Mathf.Approximately(hero.Rage.CurrentRage, 100f)} | Case3Pass={case3Pass}");

                // -------------------------------------------------------------
                // Case 4: Re-entrant nested execution during OnSkillExecutionRequested
                // -------------------------------------------------------------
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                SkillExecutionResult nestedResult = null;
                bool otherEntityNotBlocked = false;

                Action<SkillExecutionRequest> nestedCallingListener = null;
                nestedCallingListener = (r) =>
                {
                    // Attempt nested call on same hero
                    nestedResult = hero.ExecuteSelectedSkill(SkillSlotType.Skill, target);

                    // Test that another entity is NOT blocked (no global lock)
                    var otherPlan = new DashExecutionPlan(target, hero, Vector3.left, 2f, 10f, 1.8f, bm.EncounterIndex, bm);
                    otherEntityNotBlocked = target.Movement.TryReserveDashStartup(otherPlan);
                    if (otherEntityNotBlocked)
                    {
                        target.Movement.ReleaseDashReservation(otherPlan.TransactionId);
                    }
                };
                EventBus.OnSkillExecutionRequested += nestedCallingListener;

                int successEventCount = 0;
                Action<SkillExecutionRequest, SkillExecutionResult> successCounter = (r, res) =>
                {
                    if (r.Skill.SkillId == dashSkill.SkillId) successEventCount++;
                };
                EventBus.OnSkillExecutionSucceeded += successCounter;

                var req4 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var res4 = SkillExecutor.Execute(req4);

                EventBus.OnSkillExecutionRequested -= nestedCallingListener;
                EventBus.OnSkillExecutionSucceeded -= successCounter;

                bool nestedBlocked = (nestedResult != null && !nestedResult.Success && nestedResult.FailureReason == SkillExecutionFailureReason.SourceActionBusy);
                bool outerSucceeded = res4.Success && hero.Movement.IsDashing;
                bool singleRageDebit = Mathf.Approximately(hero.Rage.CurrentRage, 70f); // 100 - 30 = 70 (single debit)
                bool singleSuccess = (successEventCount == 1);
                hero.Movement.AbortDash(); // Cleanup active dash

                bool case4Pass = nestedBlocked && outerSucceeded && singleRageDebit && singleSuccess && otherEntityNotBlocked;
                Debug.Log($"[T04 Case 4] Nested Re-entry: NestedBlocked={nestedBlocked}, OuterSuccess={outerSucceeded}, SingleRage70={singleRageDebit}, SingleSuccess={singleSuccess}, OtherEntityUnblocked={otherEntityNotBlocked} | Case4Pass={case4Pass}");

                // -------------------------------------------------------------
                // Case 5: Post-commit observer exception on OnSkillExecutionSucceeded
                // -------------------------------------------------------------
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                Action<SkillExecutionRequest, SkillExecutionResult> throwingObserver = null;
                throwingObserver = (r, res) =>
                {
                    throw new InvalidOperationException("Test deliberate observer exception on success");
                };
                EventBus.OnSkillExecutionSucceeded += throwingObserver;

                var req5 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var res5 = SkillExecutor.Execute(req5);

                EventBus.OnSkillExecutionSucceeded -= throwingObserver;

                bool case5Pass = res5.Success && hero.Movement.IsDashing &&
                                 Mathf.Approximately(hero.Rage.CurrentRage, 70f) && // Rage NOT refunded for committed dash!
                                 CooldownManager.IsOnCooldown(dashSkill.SkillId, out _); // Cooldown NOT cancelled!
                hero.Movement.AbortDash(); // Cleanup

                Debug.Log($"[T04 Case 5] Post-commit observer throw: Success={res5.Success}, Rage70={Mathf.Approximately(hero.Rage.CurrentRage, 70f)}, CDActive={CooldownManager.IsOnCooldown(dashSkill.SkillId, out _)} | Case5Pass={case5Pass}");

                // -------------------------------------------------------------
                // Case 6: Exact Section 2.1 Counter-Example
                // Initial Rage=50, Max=100, cost=30.
                // OnSkillExecutionRequested independently adds 30 -> 80.
                // Listener armed once to throw ONLY on debit notification.
                // Debit subtracts 30 -> 50, then notification throws.
                // Expect controlled failure: Final Rage is 80 (preserving +30), Dash=false, CD=false, no reservation.
                // -------------------------------------------------------------
                hero.Rage.ResetRage(50f);
                CooldownManager.ResetAllCooldowns();

                Action<SkillExecutionRequest> independentRageAdder = (r) =>
                {
                    hero.Rage.AddRage(30f); // 50 -> 80
                };
                EventBus.OnSkillExecutionRequested += independentRageAdder;

                Action<float, float> singleThrowOnDebit = null;
                singleThrowOnDebit = (cur, max) =>
                {
                    // Throw when debited from 80 -> 50
                    if (Mathf.Approximately(cur, 50f))
                    {
                        hero.Rage.OnRageChanged -= singleThrowOnDebit;
                        throw new InvalidOperationException("Counter-example deliberate throw on debit notification");
                    }
                };
                hero.Rage.OnRageChanged += singleThrowOnDebit;

                var req6 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var res6 = SkillExecutor.Execute(req6);

                EventBus.OnSkillExecutionRequested -= independentRageAdder;
                hero.Rage.OnRageChanged -= singleThrowOnDebit;

                bool case6FailureControlled = res6 != null && !res6.Success && res6.FailureReason != SkillExecutionFailureReason.InsufficientRage;
                bool case6FinalRage80 = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                bool case6NoDash = !hero.Movement.IsDashing;
                bool case6NoReservation = !hero.Movement.IsStartingDash;
                bool case6NoCooldown = !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _);

                bool case6Pass = case6FailureControlled && case6FinalRage80 && case6NoDash && case6NoReservation && case6NoCooldown;
                Debug.Log($"[T04 Case 6 Counter-Example] ControlledFail={case6FailureControlled}, FinalRage80={case6FinalRage80} (cur={hero.Rage.CurrentRage}), NoDash={case6NoDash}, NoRes={case6NoReservation}, NoCD={case6NoCooldown} | Case6Pass={case6Pass}");

                // -------------------------------------------------------------
                // Case 7: EventBus.OnRageChanged notification throws after debit
                // Initial Rage=60, cost=30.
                // EventBus notification throws when debited to 30.
                // Rollback returns 30 -> final Rage=60. No Dash, no CD, no reservation.
                // -------------------------------------------------------------
                hero.Rage.ResetRage(60f);
                CooldownManager.ResetAllCooldowns();

                Action<Entity, float, float> eventBusThrower = null;
                eventBusThrower = (ent, cur, max) =>
                {
                    if (ent == hero && Mathf.Approximately(cur, 30f))
                    {
                        EventBus.OnRageChanged -= eventBusThrower;
                        throw new InvalidOperationException("Deliberate throw in EventBus.OnRageChanged after debit");
                    }
                };
                EventBus.OnRageChanged += eventBusThrower;

                SkillExecutionResult res7 = null;
                try
                {
                    var req7 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                    res7 = SkillExecutor.Execute(req7);
                }
                finally
                {
                    EventBus.OnRageChanged -= eventBusThrower;
                }

                bool case7FailureControlled = res7 != null && !res7.Success && res7.FailureReason != SkillExecutionFailureReason.InsufficientRage;
                bool case7FinalRage60 = Mathf.Approximately(hero.Rage.CurrentRage, 60f);
                bool case7NoDash = !hero.Movement.IsDashing;
                bool case7NoReservation = !hero.Movement.IsStartingDash;
                bool case7NoCooldown = !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _);
                bool case7Pass = case7FailureControlled && case7FinalRage60 && case7NoDash && case7NoReservation && case7NoCooldown;
                Debug.Log($"[T04 Case 7 EventBus Throw] ControlledFail={case7FailureControlled}, FinalRage60={case7FinalRage60}, NoDash={case7NoDash}, NoRes={case7NoReservation}, NoCD={case7NoCooldown} | Case7Pass={case7Pass}");

                // -------------------------------------------------------------
                // Case 8: Listener creates independent Rage change inside debit notification then throws error
                // Initial Rage=40, cost=30. (below max 100).
                // Debit: 40 -> 10.
                // Inside notification: listener adds +15 independent gain -> 25, then throws.
                // Rollback refunds 30: 25 + 30 = 55.
                // Proves exact cost refund and preserves independent change.
                // -------------------------------------------------------------
                hero.Rage.ResetRage(40f);
                CooldownManager.ResetAllCooldowns();

                Action<float, float> independentChangerThrower = null;
                independentChangerThrower = (cur, max) =>
                {
                    if (Mathf.Approximately(cur, 10f))
                    {
                        hero.Rage.OnRageChanged -= independentChangerThrower;
                        hero.Rage.AddRage(15f); // 10 -> 25 (independent gain, well below max 100)
                        throw new InvalidOperationException("Independent change + throw inside notification");
                    }
                };
                hero.Rage.OnRageChanged += independentChangerThrower;

                SkillExecutionResult res8 = null;
                try
                {
                    var req8 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                    res8 = SkillExecutor.Execute(req8);
                }
                finally
                {
                    hero.Rage.OnRageChanged -= independentChangerThrower;
                }

                bool case8FailureControlled = res8 != null && !res8.Success && res8.FailureReason != SkillExecutionFailureReason.InsufficientRage;
                bool case8FinalRage55 = Mathf.Approximately(hero.Rage.CurrentRage, 55f); // 40 - 30 + 15 + 30 = 55
                bool case8NoDash = !hero.Movement.IsDashing;
                bool case8NoReservation = !hero.Movement.IsStartingDash;
                bool case8NoCooldown = !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _);
                bool case8Pass = case8FailureControlled && case8FinalRage55 && case8NoDash && case8NoReservation && case8NoCooldown;
                Debug.Log($"[T04 Case 8 Independent Change in Debit] ControlledFail={case8FailureControlled}, FinalRage55={case8FinalRage55} (cur={hero.Rage.CurrentRage}), NoDash={case8NoDash}, NoRes={case8NoReservation}, NoCD={case8NoCooldown} | Case8Pass={case8Pass}");

                // -------------------------------------------------------------
                // Case 9: Refund AddRage updates balance, then refund notification throws error
                // Initial Rage=50, cost=30.
                // Debit notification throws, initiating refund.
                // Refund AddRage(30) restores 20 -> 50, and its notification throws.
                // SkillExecutor catches refund notification exception.
                // Reservation cleaned up in finally, no Dash, no CD, controlled failure.
                // -------------------------------------------------------------
                hero.Rage.ResetRage(50f);
                CooldownManager.ResetAllCooldowns();

                Action<float, float> debitThrower9 = null;
                Action<float, float> refundThrower9 = null;
                debitThrower9 = (cur, max) =>
                {
                    if (Mathf.Approximately(cur, 20f))
                    {
                        hero.Rage.OnRageChanged -= debitThrower9;
                        throw new InvalidOperationException("Deliberate debit throw to trigger refund");
                    }
                };
                refundThrower9 = (cur, max) =>
                {
                    if (Mathf.Approximately(cur, 50f))
                    {
                        hero.Rage.OnRageChanged -= refundThrower9;
                        throw new InvalidOperationException("Deliberate refund throw during AddRage notification");
                    }
                };
                hero.Rage.OnRageChanged += debitThrower9;
                hero.Rage.OnRageChanged += refundThrower9;

                SkillExecutionResult res9 = null;
                try
                {
                    var req9 = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                    res9 = SkillExecutor.Execute(req9);
                }
                finally
                {
                    hero.Rage.OnRageChanged -= debitThrower9;
                    hero.Rage.OnRageChanged -= refundThrower9;
                }

                bool case9FailureControlled = res9 != null && !res9.Success && res9.FailureReason != SkillExecutionFailureReason.InsufficientRage;
                bool case9FinalRage50 = Mathf.Approximately(hero.Rage.CurrentRage, 50f); // 50 - 30 + 30 = 50
                bool case9NoDash = !hero.Movement.IsDashing;
                bool case9NoReservation = !hero.Movement.IsStartingDash;
                bool case9NoCooldown = !CooldownManager.IsOnCooldown(dashSkill.SkillId, out _);
                bool case9Pass = case9FailureControlled && case9FinalRage50 && case9NoDash && case9NoReservation && case9NoCooldown;
                Debug.Log($"[T04 Case 9 Refund Notification Throw] ControlledFail={case9FailureControlled}, FinalRage50={case9FinalRage50}, NoDash={case9NoDash}, NoRes={case9NoReservation}, NoCD={case9NoCooldown} | Case9Pass={case9Pass}");

                bool pass = case1Pass && case2Pass && case3Pass && case4Pass && case5Pass && case6Pass && case7Pass && case8Pass && case9Pass;
                Debug.Log($"[T04] Startup Rollback & Re-entry Integrity (9 Cases): AllPassed={pass}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T05: Valid dash start commits once: single Rage consumption, single cooldown, single success event, zero damage.
        /// Cooldown deadline is strictly preserved and does not reset on dash completion.
        /// Also verifies OnDashStarted observer callbacks (throw, abort, abort then re-call).
        /// </summary>
        private static bool T05_ValidDashStart_CommitsOnce_SingleRage_SingleCooldown_SingleSuccess_ZeroDamage()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            ITimeProvider origTime = CooldownManager.TimeProvider;
            Action<SkillExecutionRequest, SkillExecutionResult> successListener = null;
            try
            {
                var testTime = new TestTimeProvider(100f);
                CooldownManager.TimeProvider = testTime;

                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);
                float targetHpBefore = target.Health.CurrentHealth;

                var dashSkill = CreateTestDashSkill("p09b_t05_valid", "Valid Dash", distance: 4.0f, speed: 12.0f, rageCost: 20f, cd: 5.0f);
                RegisterAndSelectSkill(mmMgr, dashSkill);

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                int successEventCount = 0;
                successListener = (r, res) =>
                {
                    if (r.Skill.SkillId == dashSkill.SkillId) successEventCount++;
                };
                // Keep listener subscribed until the very end of the lifecycle (R4 compliance)
                EventBus.OnSkillExecutionSucceeded += successListener;

                var req = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var result = SkillExecutor.Execute(req);

                bool success = result.Success;
                bool rageConsumedOnce = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                bool cdTriggered = CooldownManager.IsOnCooldown(dashSkill.SkillId, out float cdRemAtStart) && Mathf.Approximately(cdRemAtStart, 5.0f);
                bool eventRaisedOnceAtStart = (successEventCount == 1);

                // Advance clock during dash by 1.5s
                testTime.Advance(1.5f);
                CooldownManager.IsOnCooldown(dashSkill.SkillId, out float cdRemMid);
                bool cdDecreasedBy1_5s = Mathf.Approximately(cdRemMid, 3.5f);

                // Complete dash
                hero.Movement.CompleteDash();
                bool notDashingAfterComplete = !hero.Movement.IsDashing;

                // Verify cooldown deadline did NOT reset on completion
                CooldownManager.IsOnCooldown(dashSkill.SkillId, out float cdRemAfterComplete);
                bool cdNotResetOnComplete = Mathf.Approximately(cdRemAfterComplete, 3.5f);

                // Event must still be exactly 1
                bool eventCountStillOne = (successEventCount == 1);
                bool targetHpUntouched = Mathf.Approximately(target.Health.CurrentHealth, targetHpBefore);

                // -------------------------------------------------------------
                // Subcase B: OnDashStarted observer throws exception -> transaction committed, CD active, rage NOT refunded
                // -------------------------------------------------------------
                CooldownManager.ResetAllCooldowns();
                hero.Rage.ResetRage(100f);
                hero.transform.position = new Vector3(0f, -0.3f, 0f);

                Action<long> throwingDashObserver = (txId) =>
                {
                    throw new InvalidOperationException("Test deliberate throw inside OnDashStarted observer");
                };
                hero.Movement.OnDashStarted += throwingDashObserver;

                var reqB = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var resB = SkillExecutor.Execute(reqB);

                hero.Movement.OnDashStarted -= throwingDashObserver;

                bool subcaseBSuccess = resB != null && resB.Success && hero.Movement.IsDashing;
                bool subcaseBRagePaid = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                bool subcaseBCdActive = CooldownManager.IsOnCooldown(dashSkill.SkillId, out _);
                hero.Movement.AbortDash(); // cleanup

                bool subcaseBPass = subcaseBSuccess && subcaseBRagePaid && subcaseBCdActive;
                Debug.Log($"[T05 Subcase B] OnDashStarted Throw: Success={subcaseBSuccess}, RagePaid={subcaseBRagePaid}, CDActive={subcaseBCdActive} | SubcaseBPass={subcaseBPass}");

                // -------------------------------------------------------------
                // Subcase C & D: Re-entry inside OnDashStarted callback
                // 1. Observer verifies CD already active at callback time
                // 2. Observer calls AbortDash(txId)
                // 3. Observer calls Execute with same skill & owner RIGHT INSIDE callback
                // 4. Records nested result before outer returns
                // 5. Assert nested blocked (CooldownNotReady), single debit (Rage=80), CD deadline not reset
                // 6. Outer reflects committed state (resC.Success=true), abort post-commit does not refund
                // -------------------------------------------------------------
                CooldownManager.ResetAllCooldowns();
                hero.Rage.ResetRage(100f);
                hero.transform.position = new Vector3(0f, -0.3f, 0f);

                SkillExecutionResult nestedResult = null;
                bool nestedCdActiveAtCallback = false;
                float nestedCdRemainAtCallback = 0f;
                float nestedRageAtCallback = 0f;

                Action<long> reentrantDashObserver = (txId) =>
                {
                    // 1. Assert CD is already active inside OnDashStarted
                    nestedCdActiveAtCallback = CooldownManager.IsOnCooldown(dashSkill.SkillId, out nestedCdRemainAtCallback);
                    nestedRageAtCallback = hero.Rage.CurrentRage;

                    // 2. Abort active dash inside callback
                    hero.Movement.AbortDash(txId);

                    // 3. Call Execute for same skill & owner directly inside callback
                    var nestedReq = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                    nestedResult = SkillExecutor.Execute(nestedReq);
                };

                hero.Movement.OnDashStarted += reentrantDashObserver;

                SkillExecutionResult resC = null;
                try
                {
                    var reqC = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                    resC = SkillExecutor.Execute(reqC);
                }
                finally
                {
                    hero.Movement.OnDashStarted -= reentrantDashObserver;
                }

                bool subcaseCAborted = !hero.Movement.IsDashing;
                bool subcaseCNoRefund = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                bool subcaseCCdActive = CooldownManager.IsOnCooldown(dashSkill.SkillId, out float cdRemOuter);
                bool cdDeadlineNotReset = Mathf.Approximately(cdRemOuter, 5.0f);

                // Nested verification
                bool nestedBlocked = (nestedResult != null && !nestedResult.Success && nestedResult.FailureReason == SkillExecutionFailureReason.CooldownNotReady);
                bool singleDebitMaintained = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                bool outerReflectsCommitted = (resC != null && resC.Success);

                bool subcaseCPass = subcaseCAborted && subcaseCNoRefund && subcaseCCdActive &&
                                    nestedCdActiveAtCallback && nestedBlocked && singleDebitMaintained &&
                                    outerReflectsCommitted && cdDeadlineNotReset;

                Debug.Log($"[T05 Subcase C/D Callback Reentry] NestedCallbackCD={nestedCdActiveAtCallback} ({nestedCdRemainAtCallback:F2}s), NestedBlocked={nestedBlocked}, OuterSuccess={outerReflectsCommitted}, SingleRage80={singleDebitMaintained}, Aborted={subcaseCAborted}, NoRefund={subcaseCNoRefund} | SubcaseCPass={subcaseCPass}");

                bool pass = success && rageConsumedOnce && cdTriggered && eventRaisedOnceAtStart &&
                            cdDecreasedBy1_5s && notDashingAfterComplete && cdNotResetOnComplete &&
                            eventCountStillOne && targetHpUntouched && subcaseBPass && subcaseCPass;

                Debug.Log($"[T05] Valid Dash Start & Commit: Success={success}, Rage80={rageConsumedOnce}, CD5={cdTriggered}, CDMid={cdDecreasedBy1_5s}, CDPost={cdNotResetOnComplete}, Event=1={eventCountStillOne}, ZeroDmg={targetHpUntouched}, ObserversChecked={subcaseBPass && subcaseCPass} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                if (successListener != null) EventBus.OnSkillExecutionSucceeded -= successListener;
                CooldownManager.TimeProvider = origTime;
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T06: Continuous multi-frame movement, clamps at endpoint, no teleport, Y and Z preserved.
        /// Covers right-direction, left-direction, and invariant trajectory when target moves further away.
        /// </summary>
        private static bool T06_ContinuousMultiFrameMovement_ClampsAtEndpoint_NoTeleport_YPreserved()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;

                var dashSkill = CreateTestDashSkill("p09b_t06_mv", "Dash Move", distance: 4.0f, speed: 12.0f, rageCost: 20f, cd: 5.0f);
                RegisterAndSelectSkill(mmMgr, dashSkill);

                // -------------------------------------------------------------
                // Part 1: Moving Right with Z-Offset (Target at (5, -0.3, 4), Hero at (0, -0.3, 0))
                // -------------------------------------------------------------
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 4f); // Z is 4m on target, hero must preserve Z=0!
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));

                float x0 = hero.transform.position.x;
                float y0 = hero.transform.position.y;
                float z0 = hero.transform.position.z;

                hero.Movement.ManualTick(0.1f);
                float x1 = hero.transform.position.x;
                float y1 = hero.transform.position.y;
                float z1 = hero.transform.position.z;

                hero.Movement.ManualTick(0.1f);
                float x2 = hero.transform.position.x;
                float y2 = hero.transform.position.y;
                float z2 = hero.transform.position.z;

                hero.Movement.ManualTick(0.1f);
                float x3 = hero.transform.position.x;
                float y3 = hero.transform.position.y;
                float z3 = hero.transform.position.z;

                bool rightContinuous = (x0 < x1) && (x1 < x2) && (x2 < x3);
                bool rightNotTeleported = Mathf.Approximately(x1, 1.2f) && Mathf.Approximately(x2, 2.4f);
                bool rightClamped = Mathf.Approximately(x3, 3.2f); // 5.0 - 1.8 = 3.2m
                bool rightYPreserved = Mathf.Approximately(y0, -0.3f) && Mathf.Approximately(y1, -0.3f) && Mathf.Approximately(y2, -0.3f) && Mathf.Approximately(y3, -0.3f);
                bool rightZPreserved = Mathf.Approximately(z0, 0f) && Mathf.Approximately(z1, 0f) && Mathf.Approximately(z2, 0f) && Mathf.Approximately(z3, 0f);
                bool rightCompleted = !hero.Movement.IsDashing;

                // -------------------------------------------------------------
                // Part 2: Moving Left (Target at (-5, -0.3, -3), Hero at (0, -0.3, 0))
                // -------------------------------------------------------------
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(-5f, -0.3f, -3f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));

                float lx0 = hero.transform.position.x;
                hero.Movement.ManualTick(0.1f);
                float lx1 = hero.transform.position.x;
                hero.Movement.ManualTick(0.1f);
                float lx2 = hero.transform.position.x;
                hero.Movement.ManualTick(0.1f);
                float lx3 = hero.transform.position.x;

                bool leftContinuous = (lx0 > lx1) && (lx1 > lx2) && (lx2 > lx3);
                bool leftClamped = Mathf.Approximately(lx3, -3.2f); // -5.0 + 1.8 = -3.2m
                bool leftYPreserved = Mathf.Approximately(hero.transform.position.y, -0.3f);
                bool leftZPreserved = Mathf.Approximately(hero.transform.position.z, 0f);
                bool leftCompleted = !hero.Movement.IsDashing;

                // -------------------------------------------------------------
                // Part 3: Target moves further away during active dash (Does not extend bound dash past max travel)
                // -------------------------------------------------------------
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(10f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                hero.Movement.ManualTick(0.1f); // Hero at 1.2m

                // Target teleports far away to x=25m
                target.transform.position = new Vector3(25f, -0.3f, 0f);

                // Hero continues dash: max distance is 4.0m, so total travel must not exceed 4.0m
                hero.Movement.ManualTick(0.1f); // 2.4m
                hero.Movement.ManualTick(0.1f); // 3.6m
                hero.Movement.ManualTick(0.1f); // 4.0m clamp

                bool clampedAtMaxTravel = Mathf.Approximately(hero.transform.position.x, 4.0f);
                bool maxTravelCompleted = !hero.Movement.IsDashing;

                bool pass = rightContinuous && rightNotTeleported && rightClamped && rightYPreserved && rightZPreserved && rightCompleted &&
                            leftContinuous && leftClamped && leftYPreserved && leftZPreserved && leftCompleted &&
                            clampedAtMaxTravel && maxTravelCompleted;

                Debug.Log($"[T06] Continuous Multi-Frame Movement: Right={rightContinuous && rightClamped}, Left={leftContinuous && leftClamped}, MaxClamp={clampedAtMaxTravel} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T07: Target moves closer or crosses behind -> stops early without overshoot.
        /// </summary>
        private static bool T07_TargetMovesCloser_StopsEarlyWithoutOvershoot()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(6f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t07_cross", "Dash Close Stop", distance: 4.0f, speed: 10.0f);
                RegisterAndSelectSkill(mmMgr, dashSkill);
                hero.Rage.ResetRage(100f);

                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));

                // Frame 1: move 1.0m (hero at x=1.0)
                hero.Movement.ManualTick(0.1f);

                // Now target moves to x=2.0m (distance is now 1.0m <= AttackRange 1.8m!)
                target.transform.position = new Vector3(2.0f, -0.3f, 0f);

                // Frame 2: should stop immediately because target entered stopping distance
                hero.Movement.ManualTick(0.1f);

                bool stopped = !hero.Movement.IsDashing;
                bool didNotPenetrate = hero.transform.position.x <= 2.0f;

                bool pass = stopped && didNotPenetrate;
                Debug.Log($"[T07] Target Approaching Clamping: Stopped={stopped}, NoPenetrate={didNotPenetrate} (x={hero.transform.position.x:F2}) | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T08: Target death OR unregistration aborts dash immediately.
        /// Separately tests living target unregistration and target death.
        /// </summary>
        private static bool T08_TargetDeath_Or_Unregistered_AbortsDashImmediately()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(6f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t08_die", "Dash Die", distance: 4.0f, speed: 10.0f);
                RegisterAndSelectSkill(mmMgr, dashSkill);
                hero.Rage.ResetRage(100f);

                // Case A: Target Death
                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                hero.Movement.ManualTick(0.1f);
                float posX = hero.transform.position.x;

                target.Health.TakeDamage(9999f);
                hero.Movement.ManualTick(0.1f);

                bool caseADeathAborted = !hero.Movement.IsDashing;
                bool caseAPosFrozen = Mathf.Approximately(hero.transform.position.x, posX);

                // Case B: Living target unregistration from BattleManager
                var monster2GO = new GameObject("Monster_LiveUnreg");
                var monster2 = monster2GO.AddComponent<Monster>();
                var mCfg = ScriptableObject.CreateInstance<MonsterConfigSO>();
                mCfg.InitializeMonsterConfig("Monster 2", 1000f, 10f, 0f, 0f, 1f, 1f, 0);
                monster2.InitializeMonster(mCfg, ScriptableObject.CreateInstance<CombatConfigSO>());
                monster2GO.transform.position = new Vector3(6f, -0.3f, 0f);
                bm.RegisterMonster(monster2);

                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, monster2));
                hero.Movement.ManualTick(0.1f);
                float posUnreg = hero.transform.position.x;

                // Unregister monster2 while it is fully alive!
                var activeListField = typeof(BattleManager).GetField("activeMonsters", BindingFlags.NonPublic | BindingFlags.Instance);
                var activeList = activeListField?.GetValue(bm) as List<Monster>;
                activeList?.Remove(monster2);
                hero.Movement.ManualTick(0.1f);

                bool caseBUnregAborted = !hero.Movement.IsDashing;
                bool caseBPosFrozen = Mathf.Approximately(hero.transform.position.x, posUnreg);
                bool monster2StillAlive = monster2.IsAlive && monster2.Health.CurrentHealth > 0f;

                UnityEngine.Object.DestroyImmediate(monster2GO);
                UnityEngine.Object.DestroyImmediate(mCfg);

                bool pass = caseADeathAborted && caseAPosFrozen && caseBUnregAborted && caseBPosFrozen && monster2StillAlive;
                Debug.Log($"[T08] Target Death & Unregistration Abort: CaseA(Death)={caseADeathAborted}, CaseB(UnregLiving)={caseBUnregAborted} (Living={monster2StillAlive}) | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T09: Mid-dash CC (Stun, Root, Freeze) aborts dash immediately without Rage refund.
        /// Tests Stun, Root, Freeze individually, and verifies cooldown deadline is maintained.
        /// </summary>
        private static bool T09_MidDashCC_Stun_Root_Freeze_AbortsDashImmediately_NoRageRefund()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            ITimeProvider origTime = CooldownManager.TimeProvider;
            try
            {
                var testTime = new TestTimeProvider(100f);
                CooldownManager.TimeProvider = testTime;

                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(6f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t09_cc", "Dash Mid CC", distance: 4.0f, speed: 10.0f, rageCost: 20f, cd: 5.0f);
                RegisterAndSelectSkill(mmMgr, dashSkill);

                // 1. Stun
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                hero.Movement.ManualTick(0.1f);
                float xStun = hero.transform.position.x;
                hero.StatusController.ApplyCrowdControl("t09_stun", CrowdControlType.Stun, 2.0f);
                hero.Movement.ManualTick(0.1f);
                bool stunAborted = !hero.Movement.IsDashing;
                bool stunNoRefund = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                testTime.Advance(1.0f);
                CooldownManager.IsOnCooldown(dashSkill.SkillId, out float cdStun);
                bool stunCdMaintained = Mathf.Approximately(cdStun, 4.0f);
                hero.StatusController.ClearAllCrowdControl();

                // 2. Root
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                hero.Movement.ManualTick(0.1f);
                hero.StatusController.ApplyCrowdControl("t09_root", CrowdControlType.Root, 2.0f);
                hero.Movement.ManualTick(0.1f);
                bool rootAborted = !hero.Movement.IsDashing;
                bool rootNoRefund = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                testTime.Advance(1.0f);
                CooldownManager.IsOnCooldown(dashSkill.SkillId, out float cdRoot);
                bool rootCdMaintained = Mathf.Approximately(cdRoot, 4.0f);
                hero.StatusController.ClearAllCrowdControl();

                // 3. Freeze
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                hero.Movement.ManualTick(0.1f);
                hero.StatusController.ApplyCrowdControl("t09_freeze", CrowdControlType.Freeze, 2.0f);
                hero.Movement.ManualTick(0.1f);
                bool freezeAborted = !hero.Movement.IsDashing;
                bool freezeNoRefund = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                testTime.Advance(1.0f);
                CooldownManager.IsOnCooldown(dashSkill.SkillId, out float cdFreeze);
                bool freezeCdMaintained = Mathf.Approximately(cdFreeze, 4.0f);
                hero.StatusController.ClearAllCrowdControl();

                bool pass = stunAborted && stunNoRefund && stunCdMaintained &&
                            rootAborted && rootNoRefund && rootCdMaintained &&
                            freezeAborted && freezeNoRefund && freezeCdMaintained;

                Debug.Log($"[T09] Mid-Dash CC Interruption: Stun={stunAborted} (CD={stunCdMaintained}), Root={rootAborted} (CD={rootCdMaintained}), Freeze={freezeAborted} (CD={freezeCdMaintained}) | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                CooldownManager.TimeProvider = origTime;
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T10: Pause freezes dash, Resume continues smoothly, no compensatory time or attack during pause.
        /// </summary>
        private static bool T10_PauseFreezesDash_ResumeContinuesSmoothly_NoCompensatoryTime()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(6f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t10_p", "Dash Pause", distance: 4.0f, speed: 10.0f);
                RegisterAndSelectSkill(mmMgr, dashSkill);
                hero.Rage.ResetRage(100f);

                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                hero.Movement.ManualTick(0.1f);
                float posBeforePause = hero.transform.position.x;

                // UI Pause
                bm.PauseCombat();

                // Ticking while paused
                hero.Movement.ManualTick(0.5f);
                hero.Movement.ManualTick(0.5f);

                bool posFrozen = Mathf.Approximately(hero.transform.position.x, posBeforePause);
                bool basicAttackBlockedWhilePaused = !hero.CanBasicAttack;

                // Resume
                bm.ResumeCombat();
                bool basicAttackStillBlockedInDash = !hero.CanBasicAttack;

                // Continue dash
                hero.Movement.ManualTick(0.1f);
                bool resumedMoving = hero.transform.position.x > posBeforePause;

                hero.Movement.AbortDash();

                bool pass = posFrozen && basicAttackBlockedWhilePaused && basicAttackStillBlockedInDash && resumedMoving;
                Debug.Log($"[T10] Pause Freeze & Clean Resume: Frozen={posFrozen}, AttackBlockedInPause={basicAttackBlockedWhilePaused}, AttackBlockedAfterResume={basicAttackStillBlockedInDash}, Resumed={resumedMoving} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T11: Encounter transition, terminal battle, or BattleManager lifecycle changes cancel active dash (Fail-Closed).
        /// Meets all Section C requirements: destroyed/missing manager, replaced manager, pause invalidation, and encounter transition.
        /// </summary>
        private static bool T11_EncounterTransition_Or_TerminalBattle_CancelsActiveDash()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(6f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t11_enc", "Dash Encounter", distance: 4.0f, speed: 10.0f);
                RegisterAndSelectSkill(mmMgr, dashSkill);

                // Case 1: BattleState.EncounterTransition cancels active dash
                hero.Rage.ResetRage(100f);
                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                var stateProp = typeof(BattleManager).GetProperty("CurrentBattleState", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                stateProp?.GetSetMethod(true)?.Invoke(bm, new object[] { BattleState.EncounterTransition });
                hero.Movement.ManualTick(0.1f);
                bool abortedTransition = !hero.Movement.IsDashing;

                // Case 2: BattleState.MonsterDead cancels active dash
                stateProp?.GetSetMethod(true)?.Invoke(bm, new object[] { BattleState.InProgress });
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                stateProp?.GetSetMethod(true)?.Invoke(bm, new object[] { BattleState.MonsterDead });
                hero.Movement.ManualTick(0.1f);
                bool abortedMonsterDead = !hero.Movement.IsDashing;

                // Case 3: EncounterIndex mismatch cancels active dash
                stateProp?.GetSetMethod(true)?.Invoke(bm, new object[] { BattleState.InProgress });
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                var encProp = typeof(BattleManager).GetProperty("EncounterIndex", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                encProp?.GetSetMethod(true)?.Invoke(bm, new object[] { 99 });
                hero.Movement.ManualTick(0.1f);
                bool abortedEncIndex = !hero.Movement.IsDashing;

                // Case 4: Missing / Null manager at start rejected before resource/commit; direct Movement API rejects null manager
                var nullManagerPlan = new DashExecutionPlan(hero, target, Vector3.right, 3f, 10f, 1.8f, 0, null);
                bool directNullManagerRejected = !hero.Movement.TryStartDash(nullManagerPlan);

                // Case 5: Destroy manager while Hero & target remain active and alive -> next tick aborts dash immediately, no further movement
                stateProp?.GetSetMethod(true)?.Invoke(bm, new object[] { BattleState.InProgress });
                encProp?.GetSetMethod(true)?.Invoke(bm, new object[] { 0 });
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                hero.Movement.ManualTick(0.1f); // Move 1.0m
                float xBeforeDestroy = hero.transform.position.x;
                bool wasDashingBeforeDestroy = hero.Movement.IsDashing;

                // Destroy BattleManager independently of Hero and Target
                UnityEngine.Object.DestroyImmediate(bmGO);
                bmGO = null;

                bool heroStillAlive = hero != null && hero.IsAlive && hero.gameObject.activeInHierarchy;
                bool targetStillAlive = target != null && target.IsAlive && target.gameObject.activeInHierarchy;

                hero.Movement.ManualTick(0.1f); // Next tick after BM destroyed
                bool abortedOnDestroyedBM = !hero.Movement.IsDashing;
                bool posFrozenOnDestroyedBM = Mathf.Approximately(hero.transform.position.x, xBeforeDestroy);

                // Recreate BM for remaining tests
                bmGO = new GameObject("BattleManager_Recreated", typeof(BattleManager));
                bm = bmGO.GetComponent<BattleManager>();
                bm.RegisterHero(hero);
                bm.RegisterMonster(target);
                bm.StartBattle();
                bm.SetAutoBattle(false);

                // Case 6A: ResetInstance when old manager alive -> aborts active dash fail-closed
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                var req6A = SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                bool started6A = req6A != null && req6A.Success && hero.Movement.IsDashing;
                bool managerBound6A = hero.Movement.ActiveDashPlan?.BoundBattleManager == bm;
                bool targetBound6A = hero.Movement.ActiveDashPlan?.BoundTarget == target;
                hero.Movement.ManualTick(0.1f);
                float xBeforeReset = hero.transform.position.x;

                // Explicit ResetInstance API call
                BattleManager.ResetInstance();
                bool instanceIsNull6A = BattleManager.Instance == null;
                bool oldManagerAlive6A = bm != null && bm.gameObject.activeInHierarchy;

                hero.Movement.ManualTick(0.1f);
                bool abortedOnResetBM = !hero.Movement.IsDashing;
                bool posFrozenOnResetBM = Mathf.Approximately(hero.transform.position.x, xBeforeReset);

                // Restore singleton via real API
                bm.SetAsInstance();
                bm.RegisterHero(hero);
                bm.RegisterMonster(target);
                bool restoredInstance6A = BattleManager.Instance == bm;

                // Case 6B: Singleton replacement with new manager while old manager alive
                // Create second BM without self-destruction: reset instance temporarily during instantiation, then restore bm
                BattleManager.ResetInstance();
                var bmGO2 = new GameObject("BattleManager_Replacement");
                var bm2 = bmGO2.AddComponent<BattleManager>();
                bm2.RegisterHero(hero);
                bm2.RegisterMonster(target);
                bm.SetAsInstance(); // Hero starts dash bound to bm while bm is active instance
                bm.RegisterHero(hero);
                bm.RegisterMonster(target);

                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                var req6B = SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                bool started6B = req6B != null && req6B.Success && hero.Movement.IsDashing;
                bool managerBound6B = hero.Movement.ActiveDashPlan?.BoundBattleManager == bm;
                bool targetBound6B = hero.Movement.ActiveDashPlan?.BoundTarget == target;
                hero.Movement.ManualTick(0.1f);
                float xBeforeReplace = hero.transform.position.x;

                // Replace singleton with bm2 using real SetAsInstance API (no reflection)
                bm2.SetAsInstance();
                bm2.RegisterHero(hero);
                bm2.RegisterMonster(target);
                bool instanceIsBm2 = BattleManager.Instance == bm2;
                bool oldManagerStillAlive6B = bm != null && bm.gameObject.activeInHierarchy;

                hero.Movement.ManualTick(0.1f);
                bool abortedOnReplacedBM = !hero.Movement.IsDashing;
                bool posFrozenOnReplacedBM = Mathf.Approximately(hero.transform.position.x, xBeforeReplace);

                UnityEngine.Object.DestroyImmediate(bmGO2);
                bm.SetAsInstance();
                bm.RegisterHero(hero);
                bm.RegisterMonster(target);
                bool restoredInstance6B = BattleManager.Instance == bm;

                // Case 7: Invalidation during UI pause (pause active, dt = 0) -> target invalidation guard runs before pause check
                target.Health.Revive();
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                var req7 = SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                bool started7 = req7 != null && req7.Success && hero.Movement.IsDashing;
                bool managerBound7 = hero.Movement.ActiveDashPlan?.BoundBattleManager == bm;
                bool targetBound7 = hero.Movement.ActiveDashPlan?.BoundTarget == target;
                hero.Movement.ManualTick(0.1f);
                float xBeforePause = hero.transform.position.x;

                bm.PauseCombat();
                bool combatIsPaused = bm.IsCombatPausedByUI;

                // Tick while paused with dt = 0 when target is healthy: dash is preserved
                hero.Movement.ManualTick(0f);
                bool preservedInPauseWhenHealthy = hero.Movement.IsDashing;

                // Invalidate target while paused
                target.Health.TakeDamage(9999f);
                bool targetDeadInPause = !target.IsAlive;

                // Tick while paused with dt = 0: guard runs before pause bypass and aborts immediately
                hero.Movement.ManualTick(0f);
                bool abortedInPauseOnDeath = !hero.Movement.IsDashing;
                bool posFrozenInPause = Mathf.Approximately(hero.transform.position.x, xBeforePause);
                bm.ResumeCombat();

                bool pass = abortedTransition && abortedMonsterDead && abortedEncIndex &&
                            directNullManagerRejected && wasDashingBeforeDestroy && heroStillAlive && targetStillAlive &&
                            abortedOnDestroyedBM && posFrozenOnDestroyedBM &&
                            started6A && managerBound6A && targetBound6A && instanceIsNull6A && oldManagerAlive6A && abortedOnResetBM && posFrozenOnResetBM && restoredInstance6A &&
                            started6B && managerBound6B && targetBound6B && instanceIsBm2 && oldManagerStillAlive6B && abortedOnReplacedBM && posFrozenOnReplacedBM && restoredInstance6B &&
                            started7 && managerBound7 && targetBound7 && combatIsPaused && preservedInPauseWhenHealthy && targetDeadInPause && abortedInPauseOnDeath && posFrozenInPause;

                Debug.Log($"[T11] Fail-Closed Manager & Encounter Tests: Transition={abortedTransition}, MonsterDead={abortedMonsterDead}, EncMismatch={abortedEncIndex}, NullBMRejected={directNullManagerRejected}, DestroyBMAbort={abortedOnDestroyedBM}, ResetBMAbort={abortedOnResetBM}, ReplacedBMAbort={abortedOnReplacedBM}, PauseDeathAbort={abortedInPauseOnDeath} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T12: Basic Attack blocked during dash, not stuck disabled after dash or CC expiry.
        /// </summary>
        private static bool T12_BasicAttackBlockedDuringDash_AndNotStuckDisabledAfterDash()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(4f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t12_atk", "Dash Atk", distance: 2.0f, speed: 20.0f);
                RegisterAndSelectSkill(mmMgr, dashSkill);
                hero.Rage.ResetRage(100f);

                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));

                bool blockedDuringDash = !hero.CanBasicAttack;

                // Interrupt with Stun mid-dash
                hero.StatusController.ApplyCrowdControl("t12_stun", CrowdControlType.Stun, 2.0f);
                hero.Movement.ManualTick(0.1f);
                bool blockedDuringCC = !hero.CanBasicAttack && !hero.CanMove && !hero.CanUseSkill;

                // Expire CC
                hero.StatusController.ClearAllCrowdControl();

                bool unblockedAfterCC = hero.CanBasicAttack && hero.CanMove && hero.CanUseSkill;
                bool isAttackEnabled = hero.Attack.IsAttackEnabled;

                bool pass = blockedDuringDash && blockedDuringCC && unblockedAfterCC && isAttackEnabled;
                Debug.Log($"[T12] Action Restoration After CC Expiry: BlockedDuringDash={blockedDuringDash}, BlockedDuringCC={blockedDuringCC}, UnblockedAfterCC={unblockedAfterCC} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T13: ActionBusy blocks both new normal skill and Ultimate during active dash without resetting timer.
        /// </summary>
        private static bool T13_ActionBusy_BlocksNewSkill_AndUltimate_DuringDash()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(6f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t13_d1", "Dash Main", distance: 4.0f, speed: 10.0f);
                var normalSkill2 = CreateTestDashSkill("p09b_t13_d2", "Normal Skill 2", distance: 4.0f, speed: 10.0f, slotType: SkillSlotType.ExternalSkill1);
                var ultSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                ultSkill.InitializeSkill("p09b_t13_ult", "mm_taiji", SkillSlotType.Ultimate, "Taiji Ult", "", null, 2f, 50f, 10f);

                RegisterAndSelectSkill(mmMgr, dashSkill);
                RegisterAndSelectSkill(mmMgr, normalSkill2);
                RegisterAndSelectSkill(mmMgr, ultSkill);

                hero.Rage.ResetRage(100f);
                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));

                // 1. Attempt to execute normal skill
                var resNormal = SkillExecutor.Execute(new SkillExecutionRequest(hero, normalSkill2, SkillSlotType.ExternalSkill1, target));
                bool normalRejected = !resNormal.Success && resNormal.FailureReason == SkillExecutionFailureReason.SourceActionBusy;

                // 2. Attempt to execute Ultimate
                var resUlt = SkillExecutor.Execute(new SkillExecutionRequest(hero, ultSkill, SkillSlotType.Ultimate, target));
                bool ultRejected = !resUlt.Success && resUlt.FailureReason == SkillExecutionFailureReason.SourceActionBusy;

                hero.Movement.AbortDash();

                bool pass = normalRejected && ultRejected;
                Debug.Log($"[T13] ActionBusy Rejection During Dash: NormalSkillBusy={normalRejected}, UltimateBusy={ultRejected} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T14: AI skips Manual-Only skill in both normal and ultimate decision loops.
        /// Tests normal dash manual-only, ultimate manual-only, legacy flag=false, and manual request when Auto ON.
        /// </summary>
        private static bool T14_AI_SkipsManualOnlySkill_InBothNormalAndUltimate()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(5f, -0.3f, 0f);

                // 1. Normal Dash with isManualOnly = true
                var manualDash = CreateTestDashSkill("p09b_t14_manual", "Manual Dash", isManualOnly: true);
                RegisterAndSelectSkill(mmMgr, manualDash);

                // 2. Ultimate with isManualOnly = true
                var manualUlt = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                manualUlt.InitializeSkill("p09b_t14_ult", "mm_taiji", SkillSlotType.Ultimate, "Manual Ult", "", null, 2f, 30f, 10f);
                manualUlt.SetManualOnly(true);
                RegisterAndSelectSkill(mmMgr, manualUlt);

                hero.Rage.ResetRage(100f);

                // AI tick with Auto Battle ON
                bool aiDecided = hero.SkillDecisionController.TickDecision(0.1f);
                bool aiSkippedBoth = !aiDecided && !hero.Movement.IsDashing;

                // Manual request succeeds even though Auto Battle is ON
                var manualRes = hero.ExecuteSelectedSkill(SkillSlotType.Skill, target);
                bool manualSuccess = manualRes.Success && hero.Movement.IsDashing;
                hero.Movement.AbortDash();

                // 3. Legacy skill with isManualOnly = false is picked by AI
                var legacySkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                legacySkill.InitializeSkill("p09b_t14_legacy", "mm_taiji", SkillSlotType.ExternalSkill1, "Legacy Skill", "", null, 1.5f, 20f, 5f);
                legacySkill.SetManualOnly(false);
                RegisterAndSelectSkill(mmMgr, legacySkill);

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                bool aiPickedLegacy = hero.SkillDecisionController.TickDecision(0.1f);

                bool pass = aiSkippedBoth && manualSuccess && aiPickedLegacy;
                Debug.Log($"[T14] AI Manual-Only Skip (Normal & Ult) & Legacy Pick: SkippedBoth={aiSkippedBoth}, ManualSuccess={manualSuccess}, LegacyPicked={aiPickedLegacy} | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T15: Component disable cleans up dash state without relying on getter, and real scene unload integration test.
        /// </summary>
        private static bool T15_DisableAndReenable_SceneUnload_CleansUpDashState()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                target.transform.position = new Vector3(5f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t15_dis", "Dash Disable");
                RegisterAndSelectSkill(mmMgr, dashSkill);
                hero.Rage.ResetRage(100f);

                // Case A: Component Disable (Edit Mode simulation of OnDisable, Play Mode natural in Segment 4)
                SkillExecutor.Execute(new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target));
                bool started = hero.Movement.IsDashing;

                hero.Movement.enabled = false;
                hero.Movement.SendMessage("OnDisable", SendMessageOptions.DontRequireReceiver);
                // Pure getter check: IsDashing is false
                bool abortedOnDisable = !hero.Movement.IsDashing;

                hero.Movement.enabled = true;
                bool staysAborted = !hero.Movement.IsDashing;

                // Case B: Real Scene Unload integration test with valid encounter and registered BattleManager
                var tempScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

                // Move active hero to tempScene
                SceneManager.MoveGameObjectToScene(heroGO, tempScene);

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                var reqScene = new SkillExecutionRequest(hero, dashSkill, SkillSlotType.Skill, target);
                var resScene = SkillExecutor.Execute(reqScene);
                bool planStarted = resScene.Success && hero.Movement.IsDashing;

                int abortCallbackCount = 0;
                hero.Movement.OnDashAborted += (txId) => { abortCallbackCount++; };

                // Unload the temporary additive scene using real Unity API
                EditorSceneManager.CloseScene(tempScene, true);
                heroGO = null; // Scene close already destroys heroGO

                bool unloadFiredCallbackOnce = (abortCallbackCount == 1);

                bool pass = started && abortedOnDisable && staysAborted && planStarted && unloadFiredCallbackOnce;
                Debug.Log($"[T15] Component Disable & Real Scene Unload Cleanup: Started={started}, AbortedOnDisable={abortedOnDisable}, StaysAborted={staysAborted}, TempPlanStarted={planStarted}, UnloadFiredCallbackOnce={unloadFiredCallbackOnce} (count={abortCallbackCount}) | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        /// <summary>
        /// T16: Deterministic unit simulation via Hero.ExecuteSelectedSkill full cycle.
        /// (Relabeled from Natural Frames per Tech Lead R4 finding).
        /// </summary>
        private static bool T16_DeterministicSimulation_HeroExecuteSelectedSkill_FullCycle()
        {
            GameObject heroGO = null, bmGO = null, mmMgrGO = null;
            try
            {
                SetupEncounter(out heroGO, out var hero, out bmGO, out var bm, out mmMgrGO, out var mmMgr);
                Monster target = bm.CurrentMonster;
                hero.transform.position = new Vector3(0f, -0.3f, 0f);
                target.transform.position = new Vector3(5f, -0.3f, 0f);

                var dashSkill = CreateTestDashSkill("p09b_t16_sim", "Sim Dash", distance: 4.0f, speed: 12.0f, rageCost: 20f, cd: 5.0f);
                RegisterAndSelectSkill(mmMgr, dashSkill);
                hero.Rage.ResetRage(100f);

                var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, target);
                bool initiated = res.Success && hero.Movement.IsDashing;

                float totalTime = 0f;
                float dt = 0.02f;
                while (hero.Movement.IsDashing && totalTime < 2.0f)
                {
                    hero.Movement.ManualTick(dt);
                    totalTime += dt;
                }

                bool finished = !hero.Movement.IsDashing;
                bool arrivedAtRange = Mathf.Abs(hero.transform.position.x - 3.2f) <= 0.05f;

                bool pass = initiated && finished && arrivedAtRange;
                Debug.Log($"[T16] Deterministic Simulation Full Cycle Scenario: Initiated={initiated}, Finished={finished}, ArrivedAtRange={arrivedAtRange} (x={hero.transform.position.x:F2}) | {(pass ? "PASS" : "FAIL")}");
                return pass;
            }
            finally
            {
                TeardownEncounter(heroGO, bmGO, mmMgrGO);
            }
        }

        #endregion
    }

    /// <summary>
    /// Real Play Mode Runner for P09-B Dash Foundation.
    /// Runs naturally over actual game frames (Time.timeScale = 1.0, yield return null).
    /// Covers: Start -> Intermediate frame -> Completion, Pause/Resume freeze, CC abort + expiry, and component disable.
    /// </summary>
    public class P09BPlayModeHarness : MonoBehaviour
    {
        public bool Passed { get; private set; } = false;
        public string ErrorMessage { get; private set; } = "";

        public IEnumerator RunNaturalScenarioCoroutine()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING P09-B NATURAL PLAY MODE SCENARIO (Real Frames & Coroutine)");
            Debug.Log("================================================================================");

            GameObject heroGO = null, monsterGO = null, bmGO = null, servicesGO = null;
            SkillDefinitionSO dashSkill = null;
            MindMethodDefinitionSO tempMmDef = null;
            MindMethodDatabaseSO tempDb = null;

            int damageCount = 0;
            Action<Entity, DamageResult> damageListener = (entity, dmgResult) =>
            {
                if (dmgResult.FinalDamage > 0f) damageCount++;
            };

            try
            {
                EventBus.ClearAllListeners();
                EventBus.OnEntityDamaged += damageListener;

                // Setup services
                servicesGO = new GameObject("P09B_PlayMode_Services");
                servicesGO.AddComponent<EquipmentManager>();
                servicesGO.AddComponent<Inventory.Inventory>();
                servicesGO.AddComponent<ResourceManager>();
                var prog = servicesGO.AddComponent<ProgressionManager>();
                prog.enabled = false;

                // MindMethodManager
                var mmGo = new GameObject("MindMethodManager", typeof(MindMethodManager));
                var mmMgr = mmGo.GetComponent<MindMethodManager>();
                typeof(MindMethodManager).GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.SetValue(null, mmMgr);

                tempDb = ScriptableObject.CreateInstance<MindMethodDatabaseSO>();
                tempMmDef = ScriptableObject.CreateInstance<MindMethodDefinitionSO>();
                tempMmDef.InitializeMindMethod("mm_dash_nat", "Natural Taiji", "Natural Dash", 10, true, new MindMethodPassiveData());
                tempDb.SetMindMethods(new List<MindMethodDefinitionSO> { tempMmDef });
                mmMgr.SetDatabase(tempDb);
                mmMgr.SetActiveMindMethod("mm_dash_nat");

                // Hero at x=0, y=-0.30, z=0
                heroGO = new GameObject("Hero_Natural_P09B");
                heroGO.transform.position = new Vector3(0f, -0.30f, 0f);
                var hero = heroGO.AddComponent<Hero>();
                hero.InitializeHero();
                hero.Health.InitializeHealth(1000f, hero);
                hero.Rage.InitializeRage(100f, 100f, hero);
                typeof(Hero).GetField("attackRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(hero, 1.5f);
                hero.Stats.SetBaseValue(StatType.MoveSpeed, 0f); // Suppress auto-move
                hero.Movement.InitializeSpeed(0f, hero);
                if (hero.Attack != null) hero.Attack.SetAttackEnabled(false);
                var heroAi = heroGO.GetComponent<HeroSkillDecisionController>();
                if (heroAi != null) heroAi.enabled = false;

                // BattleManager
                bmGO = new GameObject("BM_Natural_P09B", typeof(BattleManager));
                var bm = bmGO.GetComponent<BattleManager>();
                bm.RegisterHero(hero);
                bm.SetAutoBattle(false);
                bm.StartBattle();
                bm.SetAutoBattle(false);
                bm.enabled = false;

                // Active Monster at x=5.0, y=-0.30, z=0 (Useful distance = 5.0 - 1.5 = 3.5m)
                var monster = bm.CurrentMonster;
                monster.transform.position = new Vector3(5.0f, -0.30f, 0f);
                if (monster.Attack != null) monster.Attack.SetAttackEnabled(false);
                if (monster.Stats != null) monster.Stats.SetBaseValue(StatType.MoveSpeed, 0f);
                if (monster.Movement != null) monster.Movement.InitializeSpeed(0f, monster);
                monsterGO = monster.gameObject;

                hero.transform.position = new Vector3(0f, -0.30f, 0f);
                if (hero.Stats != null) hero.Stats.SetBaseValue(StatType.MoveSpeed, 0f);
                if (hero.Movement != null) hero.Movement.InitializeSpeed(0f, hero);
                hero.SetCurrentTarget(monster);

                // Create Dash skill: Distance=4.0, Speed=12.0, Rage=20, CD=5, isManualOnly=true
                dashSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                dashSkill.InitializeSkill("skill_dash_natural", "mm_dash_nat", SkillSlotType.Skill, "Natural Dash", "", null, 0f, 20f, 5f);
                dashSkill.ConfigureDash(true, 4.0f, 12.0f, true);

                var defSkills = typeof(MindMethodDefinitionSO).GetField("skills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (defSkills != null) defSkills.SetValue(tempMmDef, new List<SkillDefinitionSO> { dashSkill });

                var activeState = mmMgr.ActiveMindMethodState;
                if (activeState != null)
                {
                    activeState.SkillStates[dashSkill.SkillId] = new SkillRuntimeState(dashSkill.SkillId, true, 1);
                    activeState.SelectedSkillPerSlot[SkillSlotType.Skill] = dashSkill.SkillId;
                }

                // Stabilize positions and warm-up frames to ensure stable frame rate before testing
                hero.transform.position = new Vector3(0f, -0.30f, 0f);
                monster.transform.position = new Vector3(5.0f, -0.30f, 0f);
                for (int w = 0; w < 5; w++)
                {
                    yield return null;
                }

                // -------------------------------------------------------------
                // SEGMENT 1: Natural Start -> Intermediate Frame -> Completion
                // -------------------------------------------------------------
                hero.transform.position = new Vector3(0f, -0.30f, 0f);
                monster.transform.position = new Vector3(5.0f, -0.30f, 0f);
                Debug.Log($"[NATURAL SEGMENT 1] Initiating Dash via Hero.ExecuteSelectedSkill at Frame={Time.frameCount}, Time={Time.time:F3}...");
                int startFrame = Time.frameCount;
                float startTime = Time.time;
                float startX = hero.transform.position.x;
                float expectedEndpoint = monster.transform.position.x - hero.AttackRange;

                var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);
                if (res == null || !res.Success || !hero.Movement.IsDashing)
                {
                    ErrorMessage = $"Segment 1 failed to start dash! Reason={res?.FailureReason}";
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }

                bool intermediateObserved = false;
                float intermediateX = startX;
                float timeout = 2.5f;
                float elapsed = 0f;

                while (hero.Movement.IsDashing && elapsed < timeout)
                {
                    yield return null; // NATURAL FRAME
                    elapsed += Time.deltaTime;

                    if (!intermediateObserved && hero.Movement.IsDashing && hero.transform.position.x > startX + 0.2f && hero.transform.position.x < expectedEndpoint - 0.2f)
                    {
                        intermediateObserved = true;
                        intermediateX = hero.transform.position.x;
                        Debug.Log($"[NATURAL SEGMENT 1 INTERMEDIATE] Frame={Time.frameCount}, Time={Time.time:F3}, HeroX={intermediateX:F3}, RemainingDist={hero.Movement.DashRemainingDistance:F2}m, CanBasicAttack={hero.CanBasicAttack}");
                    }
                }

                int endFrame = Time.frameCount;
                float endTime = Time.time;
                float endX = hero.transform.position.x;
                bool completedNaturally = !hero.Movement.IsDashing;
                bool intermediateValid = intermediateObserved && (startX < intermediateX) && (intermediateX < endX);
                bool arrivedAtTargetEndpoint = Mathf.Abs(endX - expectedEndpoint) <= 0.05f;
                bool rageDeductedOnce = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                bool zeroDamageDealt = (damageCount == 0);

                Debug.Log($"[NATURAL SEGMENT 1 RESULT] Frames={startFrame}->{endFrame} (dt={endTime - startTime:F3}s), StartX={startX:F3}, IntermediateX={intermediateX:F3}, EndX={endX:F3}, TargetX={monster.transform.position.x:F3}, ExpectedEnd={expectedEndpoint:F3}, IntermediateValid={intermediateValid}, Arrived={arrivedAtTargetEndpoint}, Rage80={rageDeductedOnce}, ZeroDmg={zeroDamageDealt}");

                if (!completedNaturally || !intermediateValid || !arrivedAtTargetEndpoint || !rageDeductedOnce || !zeroDamageDealt)
                {
                    ErrorMessage = $"Segment 1 Natural Dash verification failed! Completed={completedNaturally}, IntermediateValid={intermediateValid}, Arrived={arrivedAtTargetEndpoint}, Rage80={rageDeductedOnce}, ZeroDmg={zeroDamageDealt}";
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }

                // -------------------------------------------------------------
                // SEGMENT 2: Natural UI Pause & Resume Smooth Continuation
                // -------------------------------------------------------------
                Debug.Log("[NATURAL SEGMENT 2] Testing UI Pause Freeze and Resume on Natural Frames...");
                hero.transform.position = new Vector3(0f, -0.30f, 0f);
                monster.transform.position = new Vector3(5.0f, -0.30f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var res2 = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);
                if (res2 == null || !res2.Success || !hero.Movement.IsDashing)
                {
                    ErrorMessage = $"Segment 2 failed to start dash! Reason={res2?.FailureReason}";
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }
                yield return null; // Move 1 natural frame

                float pauseX = hero.transform.position.x;
                bm.PauseCombat();
                int pauseStartFrame = Time.frameCount;

                // Wait 5 natural frames while paused
                for (int i = 0; i < 5; i++)
                {
                    yield return null;
                    if (Mathf.Abs(hero.transform.position.x - pauseX) > 0.001f)
                    {
                        ErrorMessage = "Segment 2: Position drifted while paused!";
                        Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                        yield break;
                    }
                }

                bm.ResumeCombat();
                Debug.Log($"[NATURAL SEGMENT 2] Resumed Combat at Frame={Time.frameCount}. Completing natural dash...");

                elapsed = 0f;
                while (hero.Movement.IsDashing && elapsed < timeout)
                {
                    yield return null;
                    elapsed += Time.deltaTime;
                }

                bool seg2Completed = !hero.Movement.IsDashing && Mathf.Abs(hero.transform.position.x - (monster.transform.position.x - hero.AttackRange)) <= 0.05f;
                Debug.Log($"[NATURAL SEGMENT 2 RESULT] PausedAtX={pauseX:F3}, FinalX={hero.transform.position.x:F3}, Completed={seg2Completed}");

                if (!seg2Completed)
                {
                    ErrorMessage = "Segment 2 Pause/Resume verification failed!";
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }

                // -------------------------------------------------------------
                // SEGMENT 3: Natural CC Abort + Expiry & Action Restoration
                // -------------------------------------------------------------
                Debug.Log("[NATURAL SEGMENT 3] Testing Natural CC Interruption and Expiry...");
                hero.transform.position = new Vector3(0f, -0.30f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var res3 = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);
                if (res3 == null || !res3.Success || !hero.Movement.IsDashing)
                {
                    ErrorMessage = $"Segment 3 failed to start dash! Reason={res3?.FailureReason}";
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }
                yield return null; // 1 natural frame

                hero.StatusController.ApplyCrowdControl("nat_stun", CrowdControlType.Stun, 0.5f);
                yield return null; // Next frame must abort

                bool ccAborted = !hero.Movement.IsDashing;
                bool actionsBlockedByCC = !hero.CanBasicAttack && !hero.CanMove;
                float ccFrozenX = hero.transform.position.x;

                // Wait for CC duration to expire naturally
                float ccWait = 0f;
                while (hero.StatusController.IsStunned && ccWait < 1.0f)
                {
                    yield return null;
                    ccWait += Time.deltaTime;
                }

                bool actionsRestoredAfterCC = hero.CanBasicAttack && hero.CanMove;
                Debug.Log($"[NATURAL SEGMENT 3 RESULT] CCAborted={ccAborted}, ActionsBlocked={actionsBlockedByCC}, ActionsRestoredAfterExpiry={actionsRestoredAfterCC}, FrozenX={ccFrozenX:F3}");

                if (!ccAborted || !actionsBlockedByCC || !actionsRestoredAfterCC)
                {
                    ErrorMessage = "Segment 3 CC abort & restoration failed!";
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }

                // -------------------------------------------------------------
                // SEGMENT 4: Lifecycle Component Disable Cleanup
                // -------------------------------------------------------------
                Debug.Log("[NATURAL SEGMENT 4] Testing Component Disable Cleanup...");
                hero.transform.position = new Vector3(0f, -0.30f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var res4 = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);
                if (res4 == null || !res4.Success || !hero.Movement.IsDashing)
                {
                    ErrorMessage = $"Segment 4 failed to start dash! Reason={res4?.FailureReason}";
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }
                yield return null;

                hero.Movement.enabled = false;
                yield return null;

                bool disableAborted = !hero.Movement.IsDashing;
                hero.Movement.enabled = true;
                yield return null;
                bool noResurrect = !hero.Movement.IsDashing;

                Debug.Log($"[NATURAL SEGMENT 4 RESULT] DisableAborted={disableAborted}, NoResurrection={noResurrect}");

                if (!disableAborted || !noResurrect)
                {
                    ErrorMessage = "Segment 4 Component Disable failed!";
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }

                // -------------------------------------------------------------
                // SEGMENT 5: Basic Attack Blocked by Dash/CC Permissions and Restored Post-CC via Natural Movement
                // -------------------------------------------------------------
                Debug.Log("[NATURAL SEGMENT 5] Testing Basic Attack Blocked by Permissions (AttackEnabled=true) and Restores Post-CC via Natural Movement...");
                // Reset Segment 5 Fixture geometry before measurement interval
                hero.transform.position = new Vector3(0f, -0.30f, 0f);
                monster.transform.position = new Vector3(3.0f, -0.30f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                if (hero.Attack != null)
                {
                    hero.Attack.SetAttackEnabled(true);
                    hero.Attack.ResetAttackTimer();
                }
                if (hero.Movement != null)
                {
                    hero.Movement.SetMovementEnabled(true);
                    hero.Movement.InitializeSpeed(3.5f, hero);
                }
                hero.SetCurrentTarget(monster);

                int invalidDmgDuringDashOrCCCount = 0;
                int postCcValidHitCount = 0;
                Action<Entity, DamageResult> seg5DamageListener = (tgt, dmg) =>
                {
                    if (dmg.Attacker == hero && tgt == monster && dmg.DamageType == DamageType.BasicAttack)
                    {
                        // Event-time classification: any basic damage occurring while Dash is active or CC is active is an illegal violation!
                        bool isDuringDashOrCC = (hero != null && hero.Movement != null && hero.Movement.IsDashing) ||
                                                (hero != null && hero.StatusController != null && hero.StatusController.IsStunned) ||
                                                (hero != null && !hero.CanBasicAttack) ||
                                                (hero != null && !hero.CanMove);
                        if (isDuringDashOrCC)
                        {
                            invalidDmgDuringDashOrCCCount++;
                            Debug.LogError($"[SEGMENT 5 VIOLATION] Basic Attack dealt while Dash/CC active! IsDashing={hero.Movement.IsDashing}, Stunned={hero.StatusController.IsStunned}, CanAtk={hero.CanBasicAttack}, CanMove={hero.CanMove}");
                        }
                        else
                        {
                            postCcValidHitCount++;
                            Debug.Log($"[SEGMENT 5 EVENT] Valid Post-CC Basic Attack landed: Damage={dmg.FinalDamage}, Attacker={dmg.Attacker.name}, Target={tgt.name}");
                        }
                    }
                };
                EventBus.OnEntityDamaged += seg5DamageListener;

                var res5 = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);
                bool seg5Started = res5 != null && res5.Success && hero.Movement.IsDashing;
                yield return null; // 1 natural frame

                // 1. During active dash: CanBasicAttack is false solely because of Movement.IsDashing
                bool atkBlockedDuringDash = !hero.CanBasicAttack && hero.Movement.IsDashing;

                // 2. Interrupt with Stun mid-dash
                hero.StatusController.ApplyCrowdControl("nat_atk_stun", CrowdControlType.Stun, 0.3f);
                yield return null; // Next natural frame: dash aborts immediately

                bool dashAbortedByCC = !hero.Movement.IsDashing;
                bool atkBlockedByCC = !hero.CanBasicAttack && !hero.CanMove;

                // 3. Wait for Stun duration to expire naturally (during this entire window, seg5DamageListener traps any premature attack)
                float ccTimer5 = 0f;
                while (hero.StatusController.IsStunned && ccTimer5 < 1.0f)
                {
                    yield return null;
                    ccTimer5 += Time.deltaTime;
                }

                bool ccExpired5 = !hero.StatusController.IsStunned;
                bool atkPermissionRestored5 = hero.CanBasicAttack && hero.Attack.IsAttackEnabled && hero.CanMove;

                // 4. Natural recovery & movement: Hero naturally walks into AttackRange (1.5m) and attacks (NO TELEPORT, Time.timeScale=1)
                float naturalAtkWait = 0f;
                while (postCcValidHitCount == 0 && naturalAtkWait < 3.0f)
                {
                    yield return null;
                    naturalAtkWait += Time.deltaTime;
                }

                EventBus.OnEntityDamaged -= seg5DamageListener;

                bool zeroDmgDuringDashAndCC = (invalidDmgDuringDashOrCCCount == 0);
                bool attackHitPostCC = (postCcValidHitCount >= 1);

                Debug.Log($"[NATURAL SEGMENT 5 RESULT] DashStarted={seg5Started}, AtkBlockedInDash={atkBlockedDuringDash}, DashAborted={dashAbortedByCC}, AtkBlockedInCC={atkBlockedByCC}, CCExpired={ccExpired5}, AtkRestored={atkPermissionRestored5}, ZeroDmgDuringDashOrCC={zeroDmgDuringDashAndCC} (violations={invalidDmgDuringDashOrCCCount}), AttackHitPostCC={attackHitPostCC} (hits={postCcValidHitCount})");

                if (!seg5Started || !atkBlockedDuringDash || !dashAbortedByCC || !atkBlockedByCC || !ccExpired5 || !atkPermissionRestored5 || !zeroDmgDuringDashAndCC || !attackHitPostCC)
                {
                    ErrorMessage = $"Segment 5 Basic Attack permission gating & restoration failed! Started={seg5Started}, BlockedDash={atkBlockedDuringDash}, AbortCC={dashAbortedByCC}, BlockedCC={atkBlockedByCC}, Expired={ccExpired5}, Restored={atkPermissionRestored5}, ZeroDmgDuringDashOrCC={zeroDmgDuringDashAndCC}, HitPostCC={attackHitPostCC}";
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }

                // -------------------------------------------------------------
                // SEGMENT 6: Runtime Additive Scene Unload Cleans Up Active Dash
                // -------------------------------------------------------------
                Debug.Log("[NATURAL SEGMENT 6] Testing SceneManager.UnloadSceneAsync on Additive Scene with Active Dash...");

                // 1. Setup error logging listener to verify log purity during Segment 6
                int seg6ErrorCount = 0;
                var seg6Errors = new System.Collections.Generic.List<string>();
                Application.LogCallback seg6LogListener = (condition, stackTrace, type) =>
                {
                    if (type == LogType.Error || type == LogType.Exception)
                    {
                        seg6ErrorCount++;
                        seg6Errors.Add($"[{type}] {condition}");
                    }
                };
                Application.logMessageReceived += seg6LogListener;

                // 2. Create an additive runtime scene
                UnityEngine.SceneManagement.Scene additiveScene = UnityEngine.SceneManagement.SceneManager.CreateScene("P09B_Additive_Dash_Scene");

                // 3. Move active encounter entities into additive scene
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(heroGO, additiveScene);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(monsterGO, additiveScene);

                hero.transform.position = new Vector3(0f, -0.30f, 0f);
                monster.transform.position = new Vector3(5.0f, -0.30f, 0f);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();
                hero.SetCurrentTarget(monster);

                // 4. Setup external observer living outside scene to track transactionId and callback count
                long startedTxId = 0;
                long observedAbortTxId = 0;
                int abortCallbackCount = 0;
                Action<long> externalAbortObserver = (txId) =>
                {
                    observedAbortTxId = txId;
                    abortCallbackCount++;
                };
                hero.Movement.OnDashAborted += externalAbortObserver;

                // 5. Initiate dash on natural frames
                var res6 = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);
                bool sceneDashStarted = res6 != null && res6.Success && hero.Movement.IsDashing;
                if (sceneDashStarted && hero.Movement.ActiveDashPlan != null)
                {
                    startedTxId = hero.Movement.ActiveDashPlan.TransactionId;
                }
                yield return null; // Natural frame to advance dash

                bool wasDashingInScene = hero != null && hero.Movement != null && hero.Movement.IsDashing;
                float sceneIntermediateX = hero != null ? hero.transform.position.x : 0f;

                // 6. Unload additive scene asynchronously via natural Unity engine lifecycle
                var unloadOp = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(additiveScene);
                if (unloadOp == null)
                {
                    Application.logMessageReceived -= seg6LogListener;
                    hero.Movement.OnDashAborted -= externalAbortObserver;
                    ErrorMessage = "Segment 6: SceneManager.UnloadSceneAsync returned null!";
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }

                float unloadElapsed = 0f;
                while (!unloadOp.isDone && unloadElapsed < 5.0f)
                {
                    yield return null; // Natural frame
                    unloadElapsed += Time.deltaTime;
                }

                bool unloadCompleted = unloadOp.isDone;

                // 7. Verify cleanup happened once, entities destroyed, no resurrection
                yield return null; // Extra natural frame for garbage collection / engine update
                bool heroDestroyed = (heroGO == null || hero == null);
                bool targetDestroyed = (monsterGO == null || monster == null);

                // Mark references null for finally block
                if (heroDestroyed) heroGO = null;
                if (targetDestroyed) monsterGO = null;

                // 8. Create and unload an additional dummy scene to verify subscription was cleanly detached and not invoked again
                UnityEngine.SceneManagement.Scene dummyScene = UnityEngine.SceneManagement.SceneManager.CreateScene("P09B_Dummy_Cleanup_Scene");
                var dummyOp = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(dummyScene);
                while (dummyOp != null && !dummyOp.isDone)
                {
                    yield return null;
                }

                // Wait 2 more natural frames to verify no active state resurrects
                for (int f = 0; f < 2; f++)
                {
                    yield return null;
                }

                Application.logMessageReceived -= seg6LogListener;

                bool abortedOnceWithCorrectTx = (abortCallbackCount == 1) && (observedAbortTxId == startedTxId);
                bool zeroConsoleErrors = (seg6ErrorCount == 0);

                Debug.Log($"[NATURAL SEGMENT 6 RESULT] SceneDashStarted={sceneDashStarted}, WasDashing={wasDashingInScene}, IntermediateX={sceneIntermediateX:F3}, UnloadCompleted={unloadCompleted}, HeroDestroyed={heroDestroyed}, TargetDestroyed={targetDestroyed}, AbortedOnce={abortedOnceWithCorrectTx} (count={abortCallbackCount}, tx={observedAbortTxId}), ZeroErrors={zeroConsoleErrors} (errors={seg6ErrorCount})");

                if (!sceneDashStarted || !wasDashingInScene || !unloadCompleted || !heroDestroyed || !targetDestroyed || !abortedOnceWithCorrectTx || !zeroConsoleErrors)
                {
                    ErrorMessage = $"Segment 6 Runtime Scene Unload failed! Started={sceneDashStarted}, WasDashing={wasDashingInScene}, UnloadDone={unloadCompleted}, HeroDestroyed={heroDestroyed}, TargetDestroyed={targetDestroyed}, AbortOnce={abortedOnceWithCorrectTx}, ZeroErrors={zeroConsoleErrors}";
                    if (seg6Errors.Count > 0)
                    {
                        ErrorMessage += $" | Errors: {string.Join("; ", seg6Errors)}";
                    }
                    Debug.LogError($"[P09-B NATURAL ERROR] {ErrorMessage}");
                    yield break;
                }

                Passed = true;
                Debug.Log("================================================================================");
                Debug.Log("   [P09-B NATURAL PLAY MODE SUITE]: ALL 6 SEGMENTS PASSED NATURALLY!");
                Debug.Log("================================================================================");
            }
            finally
            {
                EventBus.OnEntityDamaged -= damageListener;

                if (dashSkill != null) UnityEngine.Object.DestroyImmediate(dashSkill);
                if (tempMmDef != null) UnityEngine.Object.DestroyImmediate(tempMmDef);
                if (tempDb != null) UnityEngine.Object.DestroyImmediate(tempDb);
                if (heroGO != null) UnityEngine.Object.Destroy(heroGO);
                if (monsterGO != null) UnityEngine.Object.Destroy(monsterGO);
                if (bmGO != null) UnityEngine.Object.Destroy(bmGO);
                if (servicesGO != null) UnityEngine.Object.Destroy(servicesGO);

                MindMethodManager.ResetInstance();
                BattleManager.ResetInstance();
                EventBus.ClearAllListeners();

                if (Application.isBatchMode)
                {
                    EditorApplication.isPlaying = false;
                    EditorApplication.update += () =>
                    {
                        EditorApplication.Exit(Passed ? 0 : 1);
                    };
                    EditorApplication.Exit(Passed ? 0 : 1);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
#endif
