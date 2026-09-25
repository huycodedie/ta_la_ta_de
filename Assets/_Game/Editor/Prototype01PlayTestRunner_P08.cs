#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Drop;
using WuxiaGame.Entities;
using WuxiaGame.Entities.Components;
using WuxiaGame.Equipment;
using WuxiaGame.Items;
using WuxiaGame.Progression;
using WuxiaGame.Stats;
using WuxiaGame.UI;
using WuxiaGame.UI.Modal;

namespace WuxiaGame.Editor
{
    public class DummyModalView_P08 : IModalView
    {
        public string ModalId { get; set; }
        public ModalPriority DefaultPriority => ModalPriority.Informational;
        public bool IsDismissable => true;
        public bool IsVisible { get; private set; }
        public void ShowModal(ModalRequest request = null) => IsVisible = true;
        public void HideModal(DismissalReason reason = DismissalReason.UserClosed) => IsVisible = false;
    }

    public static class Prototype01PlayTestRunner_P08
    {
        private const string ScenePath = "Assets/_Game/Scenes/Prototype01.unity";

        [MenuItem("Tools/Wuxia RPG/P08/Run Gate 1 (P08 Tests CLI)")]
        public static void RunGate1_P08Tests_CLI()
        {
            try
            {
                bool passed = RunP08AutomatedTests();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(passed ? 0 : 1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/Wuxia RPG/P08/Run Gate 2 (137 Locked Regression CLI)")]
        public static void RunGate2_LockedRegression_CLI()
        {
            try
            {
                bool passed = Prototype01PlayTestRunner.RunAll137LockedRegressionTests();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(passed ? 0 : 1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/Wuxia RPG/P08/Run Gate 3 (Phase B1 Modal Suite CLI)")]
        public static void RunGate3_PhaseB1Modal_CLI()
        {
            try
            {
                bool passed = Prototype01PlayTestRunner.RunAllUI01PhaseB1Tests();
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(passed ? 0 : 1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/Wuxia RPG/P08/Run P08 Automated Tests (T01 - T46)")]
        public static bool RunP08AutomatedTests()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING P08 AOE / MULTI-TARGET & NORMAL WAVE AUTOMATED TEST SUITE           ");
            Debug.Log("================================================================================");

            SkillExecutor.EnableRageCost = true;
            SkillExecutor.EnableCooldown = true;
            CooldownManager.ResetAllCooldowns();

            var tests = new List<(string Name, Func<bool> Action)>
            {
                ("T01_SingleTargetResultUnchanged", T01_SingleTargetResultUnchanged),
                ("T02_AreaHitsTwoInRangeEnemies", T02_AreaHitsTwoInRangeEnemies),
                ("T03_PrimaryIsHitExactlyOnce", T03_PrimaryIsHitExactlyOnce),
                ("T04_OutOfRadiusEnemyIsUnaffected", T04_OutOfRadiusEnemyIsUnaffected),
                ("T05_DeadInactiveUnregisteredEnemyIsExcluded", T05_DeadInactiveUnregisteredEnemyIsExcluded),
                ("T06_MaxTargetCountIsEnforced", T06_MaxTargetCountIsEnforced),
                ("T07_Ordering_PrimaryDistanceRegistration", T07_Ordering_PrimaryDistanceRegistration),
                ("T08_NoDuplicateTarget", T08_NoDuplicateTarget),
                ("T09_EffectExecutesExactlyOncePerResolvedTarget", T09_EffectExecutesExactlyOncePerResolvedTarget),
                ("T10_RageConsumedOncePerCast", T10_RageConsumedOncePerCast),
                ("T11_CooldownTriggeredOncePerCast", T11_CooldownTriggeredOncePerCast),
                ("T12_StatusAppliedIndependentlyToTargets", T12_StatusAppliedIndependentlyToTargets),
                ("T13_OneImmuneFailedTargetDoesNotCancelAnotherValidTarget", T13_OneImmuneFailedTargetDoesNotCancelAnotherValidTarget),
                ("T14_HeroCompanionNotHitByHeroArea", T14_HeroCompanionNotHitByHeroArea),
                ("T15_HeroRetargetsAndAutoCombatContinuesAfterPrimaryDeath", T15_HeroRetargetsAndAutoCombatContinuesAfterPrimaryDeath),
                ("T16_LootAndExpAwardPrecision", T16_LootAndExpAwardPrecision),
                ("T17_EncounterRemainsActiveAfterFirstDeath", T17_EncounterRemainsActiveAfterFirstDeath),
                ("T18_EncounterCompletesExactlyOnceAfterFinalDeath", T18_EncounterCompletesExactlyOnceAfterFinalDeath),
                ("T19_StopResumeAffectsAllActiveMonsters", T19_StopResumeAffectsAllActiveMonsters),
                ("T20_AllEnemiesPolicyResolvesAllLivingRegisteredEnemies", T20_AllEnemiesPolicyResolvesAllLivingRegisteredEnemies),
                ("T21_InvalidAreaAllEnemiesFailsBeforeRageCooldown", T21_InvalidAreaAllEnemiesFailsBeforeRageCooldown),
                ("T21_B_ChannelEagerSnapshotExcludesLateRegisteredMonster", T21_B_ChannelEagerSnapshotExcludesLateRegisteredMonster),
                ("T22_CooldownRejectionCreatesNoChannelAndConsumesNoResources", T22_CooldownRejectionCreatesNoChannelAndConsumesNoResources),
                ("T23_RequiredTargetSnapshotFailureConsumesNoResources", T23_RequiredTargetSnapshotFailureConsumesNoResources),
                ("T24_TwoExecutionsDoNotShareSnapshots", T24_TwoExecutionsDoNotShareSnapshots),
                ("T25_TwoSupportedCastersHaveSeparateSnapshots", T25_TwoSupportedCastersHaveSeparateSnapshots),
                ("T26_LaterRegisteredMonsterExcludedFromStartedChannel", T26_LaterRegisteredMonsterExcludedFromStartedChannel),
                ("T27_LaterPulsesSkipDeadMembersWithoutAddingReplacements", T27_LaterPulsesSkipDeadMembersWithoutAddingReplacements),
                ("T28_RageAndCooldownFollowOncePerExecutionContract", T28_RageAndCooldownFollowOncePerExecutionContract),
                ("T29_ValidLegacyHeroToMonsterSingleTarget", T29_ValidLegacyHeroToMonsterSingleTarget),
                ("T30_ValidLegacyMonsterToHeroSingleTarget", T30_ValidLegacyMonsterToHeroSingleTarget),
                ("T31_InvalidFriendlyOrDeadSingleTargetRejected", T31_InvalidFriendlyOrDeadSingleTargetRejected),
                ("T32_UnregisteredAoeTargetRejected", T32_UnregisteredAoeTargetRejected),
                ("T33_OldOrExternalMonsterDeathDoesNotAffectEncounter", T33_OldOrExternalMonsterDeathDoesNotAffectEncounter),
                ("T34_MultiEffectSkillFinishingEncounterDoesNotHitNewEncounter", T34_MultiEffectSkillFinishingEncounterDoesNotHitNewEncounter),
                ("T35_LootLifecycleContinuesAfterLongModalHold", T35_LootLifecycleContinuesAfterLongModalHold),
                ("T36_NormalWaveSpawnsExpectedCount_4or5", T36_NormalWaveSpawnsExpectedCount_4or5),
                ("T37_ConsecutiveThreeWaves_SpawnsAndProgressesCleanly", T37_ConsecutiveThreeWaves_SpawnsAndProgressesCleanly),
                ("T38_WaveStatGrowth_ExactMultiplierProgression", T38_WaveStatGrowth_ExactMultiplierProgression),
                ("T39_WaveProgressionIdempotency_NoDoubleScaling", T39_WaveProgressionIdempotency_NoDoubleScaling),
                ("T40_RecalculateStatsPreservesScaledBaseStats", T40_RecalculateStatsPreservesScaledBaseStats),
                ("T41_ChannelMidWaveDeathLifecycleAndCleanNextWaveSpawn", T41_ChannelMidWaveDeathLifecycleAndCleanNextWaveSpawn),
                ("T42_ModalRaceAndHeroDeathDuringTransition", T42_ModalRaceAndHeroDeathDuringTransition),
                ("T43_CompanionCasterTrackedDuringFinishingExecution", T43_CompanionCasterTrackedDuringFinishingExecution),
                ("T44_UnregisteredOrDeactivatedMonsterDoesNotCountAsWaveDefeat", T44_UnregisteredOrDeactivatedMonsterDoesNotCountAsWaveDefeat),
                ("T45_EntryPointsProtectedAgainstInvalidStartAndLootState", T45_EntryPointsProtectedAgainstInvalidStartAndLootState),
                ("T46_SeparateSupportedCastersExecuteIndependently", T46_SeparateSupportedCastersExecuteIndependently)
            };

            int passed = 0;
            int total = tests.Count;

            for (int i = 0; i < tests.Count; i++)
            {
                var (name, action) = tests[i];
                bool res = false;
                try
                {
                    res = action();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[TEST EXCEPTION] {name} threw: {ex.Message}\n{ex.StackTrace}");
                    res = false;
                }

                if (res)
                {
                    passed++;
                    Debug.Log($"[{i + 1:D2}/{total}] PASS: {name}");
                }
                else
                {
                    Debug.LogError($"[{i + 1:D2}/{total}] FAIL: {name}");
                }
            }

            bool allPass = (passed == total);
            Debug.Log("================================================================================");
            Debug.Log($"   [P08 AUTOMATED TESTS RESULT]: {passed}/{total} PASSED | ALL_PASS={allPass}");
            Debug.Log("================================================================================");
            return allPass;
        }

        #region Helper Methods for Test Setup & Safe Configuration
        public static (GameObject heroGO, Hero hero) CreateMockHero(string name = "MockHero", Vector3? pos = null)
        {
            var go = new GameObject(name);
            go.transform.position = pos ?? new Vector3(0f, 0f, 0f);
            var hero = go.AddComponent<Hero>();
            var so = new SerializedObject(hero);
            so.Update();
            so.FindProperty("entityType").enumValueIndex = (int)EntityType.Hero;
            so.FindProperty("entityName").stringValue = name;
            so.ApplyModifiedPropertiesWithoutUndo();

            var health = go.GetComponent<HealthComponent>();
            if (health != null) health.InitializeHealth(1000f, hero);
            var rage = go.GetComponent<RageComponent>();
            if (rage != null) rage.InitializeRage(100f, 100f, hero);
            if (hero.Stats != null)
            {
                hero.Stats.SetBaseValue(StatType.Dodge, 0f);
                hero.Stats.SetBaseValue(StatType.CritRate, 0f);
            }
            return (go, hero);
        }

        public static (GameObject monsterGO, Monster monster) CreateMockMonster(string name = "MockMonster", Vector3? pos = null)
        {
            var go = new GameObject(name);
            go.transform.position = pos ?? new Vector3(2f, 0f, 0f);
            var monster = go.AddComponent<Monster>();
            var so = new SerializedObject(monster);
            so.Update();
            so.FindProperty("entityType").enumValueIndex = (int)EntityType.Monster;
            so.FindProperty("entityName").stringValue = name;
            so.ApplyModifiedPropertiesWithoutUndo();

            var initField = typeof(Monster).GetField("isInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (initField != null) initField.SetValue(monster, true);

            var health = go.GetComponent<HealthComponent>();
            if (health != null) health.InitializeHealth(500f, monster);
            if (monster.Stats != null) monster.Stats.SetBaseValue(StatType.Dodge, 0f);
            return (go, monster);
        }

        private static (GameObject bmGO, BattleManager bm) CreateMockBattleManager()
        {
            EventBus.ClearAllListeners();
            BattleManager.ResetInstance();
            var go = new GameObject("MockBattleManager");
            var bm = go.AddComponent<BattleManager>();
            bm.SetAsInstance();
            return (go, bm);
        }

        private static (GameObject mmGO, MindMethodManager mm) CreateMockMindMethodManager()
        {
            MindMethodManager.ResetInstance();
            var go = new GameObject("MockMindMethodManager");
            var mm = go.AddComponent<MindMethodManager>();
            mm.LoadDatabaseIfMissing();
            mm.ResetPersistence();
            mm.InitializeFromDatabase();
            if (string.IsNullOrEmpty(mm.ActiveMindMethodId))
            {
                mm.SetActiveMindMethod("mm_taiji");
            }
            return (go, mm);
        }

        private static void RegisterTestSkillToMindMethod(MindMethodManager mm, SkillDefinitionSO skill, SkillSlotType slot = SkillSlotType.Skill)
        {
            if (mm == null || skill == null) return;
            string activeId = mm.ActiveMindMethodId;
            if (string.IsNullOrEmpty(activeId))
            {
                mm.SetActiveMindMethod("mm_taiji");
                activeId = mm.ActiveMindMethodId;
            }

            var so = new SerializedObject(skill);
            so.Update();
            so.FindProperty("mindMethodId").stringValue = activeId;
            so.FindProperty("slotType").intValue = (int)slot;
            so.ApplyModifiedPropertiesWithoutUndo();

            var state = mm.ActiveMindMethodState;
            if (state != null)
            {
                state.SkillStates[skill.SkillId] = new SkillRuntimeState(skill.SkillId, true, 1);
                state.SelectedSkillPerSlot[slot] = skill.SkillId;
            }
        }

        public static DamageEffectDefinitionSO CreateConfiguredEffect(SkillTargetPolicy policy, float radius, int maxTargets)
        {
            var effect = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
            int enumIndex = policy switch
            {
                SkillTargetPolicy.SingleTarget => 0,
                SkillTargetPolicy.Self => 1,
                SkillTargetPolicy.AllEnemies => 2,
                SkillTargetPolicy.AllAllies => 3,
                SkillTargetPolicy.Area => 4,
                SkillTargetPolicy.MultipleTargets => 5,
                SkillTargetPolicy.RandomTarget => 6,
                _ => 0
            };
            var so = new SerializedObject(effect);
            so.Update();
            so.FindProperty("targetPolicy").enumValueIndex = enumIndex;
            so.FindProperty("targetRadius").floatValue = radius;
            so.FindProperty("maxTargetCount").intValue = maxTargets;
            so.FindProperty("damageMultiplier").floatValue = 1.0f;
            so.ApplyModifiedPropertiesWithoutUndo();
            effect.Initialize(1.0f, policy);
            return effect;
        }

        private static SkillDefinitionSO CreateConfiguredSkill(
            string id,
            SkillEffectDefinitionSO effect,
            float rageCost = 20f,
            float cooldown = 5f,
            bool isChannel = false,
            SkillSlotType slot = SkillSlotType.Skill,
            float channelDuration = 2.0f,
            float channelTickInterval = 0.5f,
            float tickInterval = -1f)
        {
            float actualTickInterval = tickInterval > 0f ? tickInterval : channelTickInterval;
            var skill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            var so = new SerializedObject(skill);
            so.Update();
            so.FindProperty("skillId").stringValue = id;
            so.FindProperty("skillName").stringValue = id;
            so.FindProperty("mindMethodId").stringValue = "mm_taiji";
            so.FindProperty("slotType").intValue = (int)slot;
            so.FindProperty("rageCost").floatValue = rageCost;
            so.FindProperty("cooldown").floatValue = cooldown;
            so.FindProperty("castTime").floatValue = 0f;
            so.FindProperty("isChannel").boolValue = isChannel;
            so.FindProperty("channelDuration").floatValue = channelDuration;
            so.FindProperty("channelTickInterval").floatValue = actualTickInterval;

            var effectsProp = so.FindProperty("effects");
            effectsProp.arraySize = 1;
            effectsProp.GetArrayElementAtIndex(0).objectReferenceValue = effect;
            so.ApplyModifiedPropertiesWithoutUndo();
            return skill;
        }
        #endregion

        #region T01 - T21 Test Implementations
        public static bool T01_SingleTargetResultUnchanged()
        {
            var (heroGO, hero) = CreateMockHero();
            var (mGO, monster) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.SingleTarget, 0f, 1);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(monster);

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.NormalAttack, monster);
                var resolver = new DefaultSkillTargetResolver();
                var targets = resolver.ResolveTargets(req, effect);

                pass = targets.Count == 1 && targets[0] == monster;
                Debug.Log($"[P08 T01] Single-Target Result Unchanged: TargetCount={targets.Count} | {(pass ? "PASS" : "FAIL")}");
                string t0Name = targets.Count > 0 ? targets[0]?.name : "null";
                Debug.Log($"[DIAG T01] targets[0]={t0Name}, monster={monster.name}, BM.Instance==(bm): {BattleManager.Instance == bm}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(mGO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T02_AreaHitsTwoInRangeEnemies()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(4f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 5f, 0);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, m1);
                var resolver = new DefaultSkillTargetResolver();
                var targets = resolver.ResolveTargets(req, effect);

                pass = targets.Count == 2 && targets.Contains(m1) && targets.Contains(m2);
                Debug.Log($"[P08 T02] Area Hits Two In-Range Enemies: TargetCount={targets.Count} | {(pass ? "PASS" : "FAIL")}");
                Debug.Log($"[DIAG T02] targets.Count={targets.Count}, BM.Instance==(bm): {BattleManager.Instance == bm}, activeCount={BattleManager.Instance?.ActiveMonsters?.Count}");
                if (BattleManager.Instance != null && BattleManager.Instance.ActiveMonsters != null)
                {
                    for (int i = 0; i < BattleManager.Instance.ActiveMonsters.Count; i++)
                    {
                        var m = BattleManager.Instance.ActiveMonsters[i];
                        bool valid = CombatTargetQuery.IsValidEncounterMonster(hero, m);
                        Debug.Log($"[DIAG T02] Active[{i}]={m?.name}, pos={(m != null ? m.transform.position.ToString() : "null")}, isValid={valid}");
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T03_PrimaryIsHitExactlyOnce()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 5f, 0);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, m1);
                var resolver = new DefaultSkillTargetResolver();
                var targets = resolver.ResolveTargets(req, effect);

                int primaryCount = 0;
                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == m1) primaryCount++;
                }

                pass = targets.Count >= 2 && targets[0] == m1 && primaryCount == 1;
                bool firstIsM1 = targets.Count > 0 && targets[0] == m1;
                Debug.Log($"[P08 T03] Primary Is Hit Exactly Once and First: Count={primaryCount}, FirstIsM1={firstIsM1} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T04_OutOfRadiusEnemyIsUnaffected()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(10f, 0f, 0f)); // 8 units away
            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 3f, 0); // Radius 3

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, m1);
                var resolver = new DefaultSkillTargetResolver();
                var targets = resolver.ResolveTargets(req, effect);

                pass = targets.Count == 1 && targets[0] == m1 && !targets.Contains(m2);
                Debug.Log($"[P08 T04] Out Of Radius Enemy Excluded: Count={targets.Count}, ContainsM2={targets.Contains(m2)} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T05_DeadInactiveUnregisteredEnemyIsExcluded()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (mDeadGO, mDead) = CreateMockMonster("DeadM", new Vector3(2.5f, 0f, 0f));
            mDead.Health.InitializeHealth(0f, mDead); // Dead
            var (mInactiveGO, mInactive) = CreateMockMonster("InactiveM", new Vector3(3f, 0f, 0f));
            mInactiveGO.SetActive(false); // Inactive
            var (mUnregGO, mUnreg) = CreateMockMonster("UnregM", new Vector3(3.5f, 0f, 0f)); // Not registered

            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(mDead);
                bm.RegisterMonster(mInactive);
                // mUnreg not registered

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, m1);
                var resolver = new DefaultSkillTargetResolver();
                var targets = resolver.ResolveTargets(req, effect);

                pass = targets.Count == 1 && targets[0] == m1 &&
                       !targets.Contains(mDead) &&
                       !targets.Contains(mInactive) &&
                       !targets.Contains(mUnreg);
                Debug.Log($"[P08 T05] Dead/Inactive/Unregistered Excluded: TargetsCount={targets.Count} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(mDeadGO);
                UnityEngine.Object.DestroyImmediate(mInactiveGO);
                UnityEngine.Object.DestroyImmediate(mUnregGO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T06_MaxTargetCountIsEnforced()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (m3GO, m3) = CreateMockMonster("M3", new Vector3(4f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var effectLimit2 = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 2);
            var effectNoLimit = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);
                bm.RegisterMonster(m3);

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, m1);
                var resolver = new DefaultSkillTargetResolver();

                var targets2 = resolver.ResolveTargets(req, effectLimit2);
                var targetsAll = resolver.ResolveTargets(req, effectNoLimit);

                pass = targets2.Count == 2 && targetsAll.Count == 3;
                Debug.Log($"[P08 T06] MaxTargetCount Enforced: Limit2Count={targets2.Count}, NoLimitCount={targetsAll.Count} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effectLimit2);
                UnityEngine.Object.DestroyImmediate(effectNoLimit);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(m3GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T07_Ordering_PrimaryDistanceRegistration()
        {
            var (heroGO, hero) = CreateMockHero();
            // Anchor at (0,0,0)
            var (anchorGO, anchor) = CreateMockMonster("Anchor", new Vector3(0f, 0f, 0f));
            // Candidate at distance 5
            var (mFarGO, mFar) = CreateMockMonster("Far", new Vector3(5f, 0f, 0f));
            // Two candidates at equal distance 2
            var (mEq1GO, mEq1) = CreateMockMonster("Eq1", new Vector3(2f, 0f, 0f));
            var (mEq2GO, mEq2) = CreateMockMonster("Eq2", new Vector3(2f, 0f, 0f));

            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                // Register Far 1st, Eq1 2nd, Eq2 3rd
                bm.RegisterMonster(anchor);
                bm.RegisterMonster(mFar);
                bm.RegisterMonster(mEq1);
                bm.RegisterMonster(mEq2);

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, anchor);
                var resolver = new DefaultSkillTargetResolver();
                var targets = resolver.ResolveTargets(req, effect);

                // Expected order:
                // 0: Anchor
                // 1: Eq1 (distance 2, registered before Eq2)
                // 2: Eq2 (distance 2, registered after Eq1)
                // 3: Far (distance 5)
                pass = targets.Count == 4 &&
                       targets[0] == anchor &&
                       targets[1] == mEq1 &&
                       targets[2] == mEq2 &&
                       targets[3] == mFar;

                string t0 = targets.Count > 0 ? targets[0]?.EntityName : "none";
                string t1 = targets.Count > 1 ? targets[1]?.EntityName : "none";
                string t2 = targets.Count > 2 ? targets[2]?.EntityName : "none";
                string t3 = targets.Count > 3 ? targets[3]?.EntityName : "none";
                Debug.Log($"[P08 T07] Deterministic Ordering Verified: [0]={t0}, [1]={t1}, [2]={t2}, [3]={t3} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(anchorGO);
                UnityEngine.Object.DestroyImmediate(mFarGO);
                UnityEngine.Object.DestroyImmediate(mEq1GO);
                UnityEngine.Object.DestroyImmediate(mEq2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T08_NoDuplicateTarget()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                hero.SetCurrentTarget(m1);

                // Both request.Target and hero.CurrentTarget point to m1
                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, m1);
                var resolver = new DefaultSkillTargetResolver();
                var targets = resolver.ResolveTargets(req, effect);

                pass = targets.Count == 1 && targets[0] == m1;
                Debug.Log($"[P08 T08] No Duplicate Target: Count={targets.Count} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T09_EffectExecutesExactlyOncePerResolvedTarget()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, m1);
                var results = EffectResolver.Instance.ProcessEffects(req, new List<SkillEffectDefinitionSO> { effect });

                // Two resolved targets -> exactly two execution results
                pass = results.Count == 2 &&
                       results[0].Target == m1 &&
                       results[1].Target == m2 &&
                       results[0].Success &&
                       results[1].Success;

                Debug.Log($"[P08 T09] Effect Executes Once Per Target: ResultsCount={results.Count} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T10_RageConsumedOncePerCast()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);
            var skill = CreateConfiguredSkill("p08_rage_test", effect, rageCost: 25f, cooldown: 0f);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                hero.Rage.ResetRage(100f);
                float initialRage = hero.Rage.CurrentRage;

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);
                SkillExecutor.EnableRageCost = true;
                SkillExecutor.EnableCooldown = false;
                var result = SkillExecutor.Execute(req);

                float finalRage = hero.Rage.CurrentRage;
                float expectedRage = initialRage - 25f;

                pass = result.Success && Mathf.Approximately(finalRage, expectedRage);
                Debug.Log($"[P08 T10] Rage Consumed Once: Initial={initialRage}, Final={finalRage}, Expected={expectedRage} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T11_CooldownTriggeredOncePerCast()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);
            var skill = CreateConfiguredSkill("p08_cd_test", effect, rageCost: 0f, cooldown: 4f);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);
                SkillExecutor.EnableRageCost = false;
                SkillExecutor.EnableCooldown = true;
                CooldownManager.ResetAllCooldowns();

                var result = SkillExecutor.Execute(req);
                bool onCd = CooldownManager.IsOnCooldown("p08_cd_test", out float remain);

                pass = result.Success && onCd && remain <= 4f && remain > 3f;
                Debug.Log($"[P08 T11] Cooldown Triggered Once: OnCd={onCd}, Remaining={remain:F2}s | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                SkillExecutor.EnableRageCost = true;
                SkillExecutor.EnableCooldown = true;
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T12_StatusAppliedIndependentlyToTargets()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                float hp1Before = m1.Health.CurrentHealth;
                float hp2Before = m2.Health.CurrentHealth;

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, m1);
                var results = EffectResolver.Instance.ProcessEffects(req, new List<SkillEffectDefinitionSO> { effect });

                float hp1After = m1.Health.CurrentHealth;
                float hp2After = m2.Health.CurrentHealth;

                pass = hp1After < hp1Before && hp2After < hp2Before && results.Count == 2;
                Debug.Log($"[P08 T12] Status/Damage Applied Independently: M1 Hp={hp1Before}->{hp1After}, M2 Hp={hp2Before}->{hp2After} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T13_OneImmuneFailedTargetDoesNotCancelAnotherValidTarget()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                // m1 is killed right before effect resolution
                m1.Health.SetCurrentHealth(0f);

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, m1);
                var results = effect.Execute(req, m1, null);
                var results2 = effect.Execute(req, m2, null);

                pass = !results.Success && results2.Success;
                Debug.Log($"[P08 T13] One Failed Target Does Not Cancel Others: M1Success={results.Success}, M2Success={results2.Success} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T14_HeroCompanionNotHitByHeroArea()
        {
            var (heroGO, hero) = CreateMockHero();
            var (compGO, companion) = CreateMockHero("HeroCompanion", new Vector3(1f, 0f, 0f));
            var so = new SerializedObject(companion);
            so.Update();
            so.FindProperty("entityType").enumValueIndex = (int)EntityType.Companion;
            so.ApplyModifiedPropertiesWithoutUndo();

            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Skill, m1);
                var resolver = new DefaultSkillTargetResolver();
                var targets = resolver.ResolveTargets(req, effect);

                pass = targets.Count == 1 && targets[0] == m1 && !targets.Contains(companion) && !targets.Contains(hero);
                Debug.Log($"[P08 T14] Hero/Companion Friendly Fire Excluded: ContainsHero={targets.Contains(hero)}, ContainsComp={targets.Contains(companion)} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(compGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T15_HeroRetargetsAndAutoCombatContinuesAfterPrimaryDeath()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3.8f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                bm.SetAutoBattle(true);
                bm.StartBattle();

                // Kill M1
                m1.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));

                pass = bm.CurrentMonster == m2 && hero.CurrentTarget == m2 && bm.IsBattleActive;
                Debug.Log($"[P08 T15] Primary Death Retargets: NextMonster={bm.CurrentMonster?.EntityName}, HeroTarget={hero.CurrentTarget?.EntityName}, BattleActive={bm.IsBattleActive} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T16_LootAndExpAwardPrecision()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3.8f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            ProgressionManager.ResetInstance();
            var pmGO = new GameObject("MockProgressionManager");
            var pm = pmGO.AddComponent<ProgressionManager>();
            pm.LoadConfigsIfMissing();
            EventBus.OnEntityDied += pm.HandleEntityDied;

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                bm.StartBattle();

                // 1. EXP awarded exactly once per monster death
                m1.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                bool m1ExpOnce = m1.HasAwardedExp;

                // Repeated death event on same monster must be idempotent
                EventBus.RaiseEntityDied(m1);
                EventBus.RaiseEntityDied(m1);
                bool m1ExpStillOnce = m1.HasAwardedExp;

                // 2. Null drop is not queued
                int queueBeforeNull = bm.PendingLootQueueCount;
                bm.EnqueuePendingLoot(null);
                bool nullDropNotQueued = (bm.PendingLootQueueCount == queueBeforeNull);

                // Enqueue 2 non-null drops to verify multi-item sequential queue resolution
                var drop1 = new EquipmentInstance("test_drop_1", "Test Sword 1", EquipmentSlotType.Weapon, 5, null, new List<AffixInstance>());
                var drop2 = new EquipmentInstance("test_drop_2", "Test Armor 2", EquipmentSlotType.Armor, 5, null, new List<AffixInstance>());
                bm.EnqueuePendingLoot(drop1);
                bm.EnqueuePendingLoot(drop2);
                bool twoDropsQueued = (bm.PendingLootQueueCount == 2);

                // 3. Final death occurs: M2 dies
                int encBefore = bm.EncounterIndex;
                m2.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                bool m2ExpOnce = m2.HasAwardedExp;

                // 4. Every non-null generated drop is presented and resolved exactly once
                // First drop is presented
                bool drop1Presented = (bm.CurrentBattleState == BattleState.LootPending && bm.PendingLootItem == drop1);
                bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);

                // Second drop is presented
                bool drop2Presented = (bm.CurrentBattleState == BattleState.LootPending && bm.PendingLootItem == drop2);
                bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);

                // 5. Next encounter starts only after the pending loot queue is empty
                bool queueEmpty = (bm.PendingLootQueueCount == 0 && bm.PendingLootItem == null);
                bool nextEncounterStarted = (bm.EncounterIndex > encBefore);

                pass = m1ExpOnce && m1ExpStillOnce && m2ExpOnce &&
                       nullDropNotQueued && twoDropsQueued &&
                       drop1Presented && drop2Presented &&
                       queueEmpty && nextEncounterStarted;

                Debug.Log($"[P08 T16] Loot and EXP Precision: M1ExpOnce={m1ExpOnce}, NullDropNotQueued={nullDropNotQueued}, DropsPresented=({drop1Presented},{drop2Presented}), QueueEmpty={queueEmpty}, NextEncStarted={nextEncounterStarted} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                EventBus.OnEntityDied -= pm.HandleEntityDied;
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(pmGO);
                ProgressionManager.ResetInstance();
            }
            return pass;
        }

        public static bool T17_EncounterRemainsActiveAfterFirstDeath()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3.8f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                bm.StartBattle();

                // Kill first monster
                m1.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));

                // Assert encounter remains active
                pass = bm.IsBattleActive && bm.CurrentBattleState == BattleState.InProgress && bm.EncounterIndex == 1;
                Debug.Log($"[P08 T17] Encounter Active After First Death: BattleActive={bm.IsBattleActive}, State={bm.CurrentBattleState}, Enc={bm.EncounterIndex} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T18_EncounterCompletesExactlyOnceAfterFinalDeath()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3.8f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                bm.StartBattle();

                int battleStateChangeCount = 0;
                Action<BattleState> onStateChange = (s) =>
                {
                    if (s == BattleState.MonsterDead || s == BattleState.LootPending || s == BattleState.EncounterTransition)
                        battleStateChangeCount++;
                };
                EventBus.OnBattleStateChanged += onStateChange;

                // Kill M1
                m1.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                int stateChangesAfterM1 = battleStateChangeCount;

                // Kill M2 (Final death)
                m2.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                int stateChangesAfterM2 = battleStateChangeCount;

                EventBus.OnBattleStateChanged -= onStateChange;

                pass = stateChangesAfterM1 == 0 && stateChangesAfterM2 >= 1;
                Debug.Log($"[P08 T18] Encounter Completes Once After Final Death: AfterM1={stateChangesAfterM1}, AfterM2={stateChangesAfterM2} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T19_StopResumeAffectsAllActiveMonsters()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3.8f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                bm.StartBattle();

                // Pause
                bm.PauseCombat();
                bool bothStopped = (!m1.Attack.IsAttackEnabled && !m2.Attack.IsAttackEnabled);

                // Resume
                bm.ResumeCombat();
                bool bothResumed = (m1.Attack.IsAttackEnabled && m2.Attack.IsAttackEnabled);

                pass = bothStopped && bothResumed;
                Debug.Log($"[P08 T19] Stop/Resume Affects All Active Monsters: BothStopped={bothStopped}, BothResumed={bothResumed} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T20_AllEnemiesPolicyResolvesAllLivingRegisteredEnemies()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(10f, 0f, 0f));
            var (m3GO, m3) = CreateMockMonster("M3", new Vector3(20f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var effectAll = CreateConfiguredEffect(SkillTargetPolicy.AllEnemies, 0f, 0);
            var effectLimit2 = CreateConfiguredEffect(SkillTargetPolicy.AllEnemies, 0f, 2);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);
                bm.RegisterMonster(m3);

                var req = new SkillExecutionRequest(hero, null, SkillSlotType.Ultimate, null);
                var resolver = new DefaultSkillTargetResolver();

                var allTargets = resolver.ResolveTargets(req, effectAll);
                var limit2Targets = resolver.ResolveTargets(req, effectLimit2);

                pass = allTargets.Count == 3 && limit2Targets.Count == 2;
                Debug.Log($"[P08 T20] AllEnemies Resolves All Registered Enemies: AllCount={allTargets.Count}, Limit2Count={limit2Targets.Count} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(effectAll);
                UnityEngine.Object.DestroyImmediate(effectLimit2);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(m3GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T21_InvalidAreaAllEnemiesFailsBeforeRageCooldown()
        {
            var (heroGO, hero) = CreateMockHero();
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var badRadiusEffect = CreateConfiguredEffect(SkillTargetPolicy.Area, -1f, 0);
            var skillBadRadius = CreateConfiguredSkill("skill_bad_radius", badRadiusEffect, rageCost: 30f, cooldown: 5f);
            RegisterTestSkillToMindMethod(mm, skillBadRadius, SkillSlotType.Skill);

            var noEnemiesEffect = CreateConfiguredEffect(SkillTargetPolicy.AllEnemies, 0f, 0);
            var skillNoEnemies = CreateConfiguredSkill("skill_no_enemies", noEnemiesEffect, rageCost: 30f, cooldown: 5f);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                // 1. Invalid Area: negative radius
                var req1 = new SkillExecutionRequest(hero, skillBadRadius, SkillSlotType.Skill, null);
                var res1 = SkillExecutor.Execute(req1);

                // 2. Invalid AllEnemies: no living registered enemies
                RegisterTestSkillToMindMethod(mm, skillNoEnemies, SkillSlotType.Skill);
                var req2 = new SkillExecutionRequest(hero, skillNoEnemies, SkillSlotType.Skill, null);
                var res2 = SkillExecutor.Execute(req2);

                float finalRage = hero.Rage.CurrentRage;
                bool onCd1 = CooldownManager.IsOnCooldown("skill_bad_radius", out _);
                bool onCd2 = CooldownManager.IsOnCooldown("skill_no_enemies", out _);

                pass = !res1.Success && !res2.Success &&
                       Mathf.Approximately(finalRage, 100f) &&
                       !onCd1 && !onCd2;

                Debug.Log($"[P08 T21] Invalid AOE Fails Before Rage/Cooldown: Res1Fail={!res1.Success}, Res2Fail={!res2.Success}, RagePreserved={Mathf.Approximately(finalRage, 100f)}, NoCooldown={!onCd1 && !onCd2} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(skillBadRadius);
                UnityEngine.Object.DestroyImmediate(badRadiusEffect);
                UnityEngine.Object.DestroyImmediate(skillNoEnemies);
                UnityEngine.Object.DestroyImmediate(noEnemiesEffect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T21_B_ChannelEagerSnapshotExcludesLateRegisteredMonster()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (m3LateGO, m3Late) = CreateMockMonster("M3_Late", new Vector3(2.5f, 0f, 0f));

            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();
            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);
            var skill = CreateConfiguredSkill("p08_channel_eager", effect, rageCost: 10f, cooldown: 5f, isChannel: true);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);

                // 1. Validate request and establish eager snapshot
                bool valid = SkillExecutionValidator.Validate(req, out _, out _);

                // 2. Start Channel with PreservedTargetResolver
                var resolver = new PreservedTargetResolver(req);

                // 3. Register late monster AFTER channel starts, BEFORE pulse 1
                bm.RegisterMonster(m3Late);

                // 4. Resolve targets on pulse 1
                var pulse1Targets = resolver.ResolveTargets(req, effect);

                // 5. Kill M1, pulse 2
                m1.Health.SetCurrentHealth(0f);
                var pulse2Targets = resolver.ResolveTargets(req, effect);

                // Verification:
                // Pulse 1 has M1 and M2 only (M3_Late is excluded because it wasn't in eager snapshot)
                // Pulse 2 skips M1 (now dead), preserves M2 only, and still excludes M3_Late
                pass = valid &&
                       pulse1Targets.Count == 2 &&
                       pulse1Targets.Contains(m1) &&
                       pulse1Targets.Contains(m2) &&
                       !pulse1Targets.Contains(m3Late) &&
                       pulse2Targets.Count == 1 &&
                       pulse2Targets.Contains(m2) &&
                       !pulse2Targets.Contains(m1) &&
                       !pulse2Targets.Contains(m3Late);

                Debug.Log($"[P08 T21-B] Eager Channel Snapshot Excludes Late Spawns: Pulse1Count={pulse1Targets.Count}, Pulse2Count={pulse2Targets.Count} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(m3LateGO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }
        public static bool T22_CooldownRejectionCreatesNoChannelAndConsumesNoResources()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);
            var skill = CreateConfiguredSkill("p08_channel_cd_test", effect, rageCost: 20f, cooldown: 10f, isChannel: true);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);

                hero.Rage.ResetRage(100f);
                CooldownManager.TriggerCooldown(skill.SkillId, 10f);

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);

                bool valid = SkillExecutionValidator.Validate(req, out var reason, out _);
                bool validFailedOnCd = (!valid && reason == SkillExecutionFailureReason.CooldownNotReady);

                var result = SkillExecutor.Execute(req);
                bool execFailed = (!result.Success && result.FailureReason == SkillExecutionFailureReason.CooldownNotReady);
                bool ragePreserved = Mathf.Approximately(hero.Rage.CurrentRage, 100f);
                bool noChannel = !hero.CastState.IsActive && hero.CastState.CurrentPhase == SkillCastPhase.Ready;

                pass = validFailedOnCd && execFailed && ragePreserved && noChannel;
                Debug.Log($"[P08 T22] Cooldown Rejection Creates No Channel And Consumes No Resources: ValidFailedOnCd={validFailedOnCd}, ExecFailed={execFailed}, RagePreserved={ragePreserved}, NoChannel={noChannel} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T23_RequiredTargetSnapshotFailureConsumesNoResources()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);
            var skill = CreateConfiguredSkill("p08_channel_snapfail_test", effect, rageCost: 20f, cooldown: 10f, isChannel: true);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                // Kill monster M1 so required target snapshot fails
                m1.Health.SetCurrentHealth(0f);

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);

                var result = SkillExecutor.Execute(req);
                bool execFailed = !result.Success;
                bool ragePreserved = Mathf.Approximately(hero.Rage.CurrentRage, 100f);
                bool noCooldown = !CooldownManager.IsOnCooldown(skill.SkillId, out _);
                bool noChannel = !hero.CastState.IsActive;

                pass = execFailed && ragePreserved && noCooldown && noChannel;
                Debug.Log($"[P08 T23] Required Target Snapshot Failure Consumes No Resources: ExecFailed={execFailed}, RagePreserved={ragePreserved}, NoCooldown={noCooldown}, NoChannel={noChannel} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T24_TwoExecutionsDoNotShareSnapshots()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (m3GO, m3) = CreateMockMonster("M3", new Vector3(20f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 5f, 10);
            var skill = CreateConfiguredSkill("p08_channel_two_exec", effect, rageCost: 10f, cooldown: 0f, isChannel: true, channelDuration: 2f, tickInterval: 1f);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);
                bm.RegisterMonster(m3);

                hero.Rage.ResetRage(100f);

                // Execution 1 targeting M1: Area radius 5 -> hits M1 and M2
                var req1 = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);
                var res1 = SkillExecutor.Execute(req1);

                // Tick 1
                hero.CastState.Tick(1.0f);
                float m1HpAfterPulse1 = m1.Health.CurrentHealth;
                float m2HpAfterPulse1 = m2.Health.CurrentHealth;
                float m3HpAfterPulse1 = m3.Health.CurrentHealth;

                // Reset channel state for execution 2
                hero.CastState.Reset();
                CooldownManager.ResetAllCooldowns();

                // Kill M1
                m1.Health.SetCurrentHealth(0f);

                // Execution 2 targeting M3: Area radius 5 -> hits M3 only
                var req2 = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m3);
                var res2 = SkillExecutor.Execute(req2);

                // Tick 1 of Execution 2
                hero.CastState.Tick(1.0f);
                float m2HpAfterExec2 = m2.Health.CurrentHealth;
                float m3HpAfterExec2 = m3.Health.CurrentHealth;

                bool exec1Ok = res1.Success && Mathf.Approximately(m1HpAfterPulse1, 490f) && Mathf.Approximately(m2HpAfterPulse1, 490f) && Mathf.Approximately(m3HpAfterPulse1, 500f);
                bool exec2Ok = res2.Success && Mathf.Approximately(m2HpAfterExec2, 490f) && Mathf.Approximately(m3HpAfterExec2, 490f);

                pass = exec1Ok && exec2Ok;
                Debug.Log($"[P08 T24] Two Executions Do Not Share Snapshots: Exec1Ok={exec1Ok}, Exec2Ok={exec2Ok} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(m3GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T25_TwoSupportedCastersHaveSeparateSnapshots()
        {
            var (hero1GO, hero1) = CreateMockHero("Hero1", new Vector3(0f, 0f, 0f));
            var (hero2GO, hero2) = CreateMockHero("Hero2", new Vector3(50f, 0f, 0f));
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (m3GO, m3) = CreateMockMonster("M3", new Vector3(52f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 5f, 10);
            var skill = CreateConfiguredSkill("p08_channel_two_casters", effect, rageCost: 10f, cooldown: 0f, isChannel: true, channelDuration: 2f, tickInterval: 1f);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero1);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);
                bm.RegisterMonster(m3);

                hero1.Rage.ResetRage(100f);
                hero2.Rage.ResetRage(100f);

                var reqA = new SkillExecutionRequest(hero1, skill, SkillSlotType.Skill, m1);
                var resA = SkillExecutor.Execute(reqA);

                var reqB = new SkillExecutionRequest(hero2, skill, SkillSlotType.Skill, m3);
                var resB = SkillExecutor.Execute(reqB);

                // Tick Caster A
                hero1.CastState.Tick(1.0f);
                float m1HpAfterA = m1.Health.CurrentHealth;
                float m2HpAfterA = m2.Health.CurrentHealth;
                float m3HpAfterA = m3.Health.CurrentHealth;

                // Tick Caster B
                hero2.CastState.Tick(1.0f);
                float m1HpAfterB = m1.Health.CurrentHealth;
                float m2HpAfterB = m2.Health.CurrentHealth;
                float m3HpAfterB = m3.Health.CurrentHealth;

                bool aOk = resA.Success && Mathf.Approximately(m1HpAfterA, 490f) && Mathf.Approximately(m2HpAfterA, 490f) && Mathf.Approximately(m3HpAfterA, 500f);
                bool bOk = resB.Success && Mathf.Approximately(m1HpAfterB, 490f) && Mathf.Approximately(m2HpAfterB, 490f) && Mathf.Approximately(m3HpAfterB, 490f);

                pass = aOk && bOk;
                Debug.Log($"[P08 T25] Two Supported Casters Have Separate Snapshots: AOk={aOk}, BOk={bOk} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(hero1GO);
                UnityEngine.Object.DestroyImmediate(hero2GO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(m3GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T26_LaterRegisteredMonsterExcludedFromStartedChannel()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (m3LateGO, m3Late) = CreateMockMonster("M3_Late", new Vector3(2.5f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 10);
            var skill = CreateConfiguredSkill("p08_channel_late_reg", effect, rageCost: 10f, cooldown: 5f, isChannel: true, channelDuration: 2f, tickInterval: 1f);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                hero.Rage.ResetRage(100f);
                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);

                // Execute starts channel with eager snapshot [M1, M2]
                var res = SkillExecutor.Execute(req);

                // Register late monster AFTER channel starts
                bm.RegisterMonster(m3Late);

                // Pulse 1
                hero.CastState.Tick(1.0f);
                float m1HpP1 = m1.Health.CurrentHealth;
                float m2HpP1 = m2.Health.CurrentHealth;
                float m3LateHpP1 = m3Late.Health.CurrentHealth;

                // Kill M1
                m1.Health.SetCurrentHealth(0f);

                // Pulse 2
                hero.CastState.Tick(1.0f);
                float m2HpP2 = m2.Health.CurrentHealth;
                float m3LateHpP2 = m3Late.Health.CurrentHealth;

                bool p1Ok = res.Success && Mathf.Approximately(m1HpP1, 490f) && Mathf.Approximately(m2HpP1, 490f) && Mathf.Approximately(m3LateHpP1, 500f);
                bool p2Ok = Mathf.Approximately(m2HpP2, 480f) && Mathf.Approximately(m3LateHpP2, 500f);

                pass = p1Ok && p2Ok;
                Debug.Log($"[P08 T26] Later Registered Monster Excluded From Started Channel: P1Ok={p1Ok}, P2Ok={p2Ok} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(m3LateGO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T27_LaterPulsesSkipDeadMembersWithoutAddingReplacements()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (m2GO, m2) = CreateMockMonster("M2", new Vector3(3f, 0f, 0f));
            var (mNewGO, mNew) = CreateMockMonster("M_New", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 15);
            var skill = CreateConfiguredSkill("p08_channel_skip_dead", effect, rageCost: 10f, cooldown: 5f, isChannel: true, channelDuration: 2f, tickInterval: 1f);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.RegisterMonster(m2);

                hero.Rage.ResetRage(100f);
                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);
                var res = SkillExecutor.Execute(req);

                // Pulse 1
                hero.CastState.Tick(1.0f);
                float m1P1 = m1.Health.CurrentHealth;
                float m2P1 = m2.Health.CurrentHealth;

                // Kill M1 before pulse 2, and register a new monster in the exact same spot
                m1.Health.SetCurrentHealth(0f);
                bm.RegisterMonster(mNew);

                // Pulse 2
                hero.CastState.Tick(1.0f);
                float m2P2 = m2.Health.CurrentHealth;
                float mNewP2 = mNew.Health.CurrentHealth;

                bool p1Ok = res.Success && Mathf.Approximately(m1P1, 490f) && Mathf.Approximately(m2P1, 490f);
                bool p2Ok = Mathf.Approximately(m2P2, 480f) && Mathf.Approximately(mNewP2, 500f);

                pass = p1Ok && p2Ok;
                Debug.Log($"[P08 T27] Later Pulses Skip Dead Members Without Adding Replacements: P1Ok={p1Ok}, P2Ok={p2Ok} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(m2GO);
                UnityEngine.Object.DestroyImmediate(mNewGO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T28_RageAndCooldownFollowOncePerExecutionContract()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 10);
            var skill = CreateConfiguredSkill("p08_channel_contract", effect, rageCost: 25f, cooldown: 8f, isChannel: true, channelDuration: 3f, tickInterval: 1f);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);

                SkillExecutor.EnableRageCost = true;
                SkillExecutor.EnableCooldown = true;
                hero.Rage.ResetRage(60f);
                CooldownManager.ResetAllCooldowns();

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);
                var res = SkillExecutor.Execute(req);

                // Immediately after start: Rage consumed once (60 - 25 = 35), channel active, not on cooldown yet (cooldown follows baseline completion contract)
                float rageAtStart = hero.Rage.CurrentRage;
                bool cdAtStart = CooldownManager.IsOnCooldown(skill.SkillId, out _);

                // Pulse 1
                hero.CastState.Tick(1.0f);
                float rageAtP1 = hero.Rage.CurrentRage;

                // Pulse 2
                hero.CastState.Tick(1.0f);
                float rageAtP2 = hero.Rage.CurrentRage;

                // Pulse 3 (completes channel -> triggers cooldown once)
                hero.CastState.Tick(1.0f);
                float rageAtP3 = hero.Rage.CurrentRage;
                bool channelFinished = !hero.CastState.IsActive;
                bool cdAtEnd = CooldownManager.IsOnCooldown(skill.SkillId, out float cdValEnd);

                pass = res.Success &&
                       Mathf.Approximately(rageAtStart, 35f) &&
                       !cdAtStart &&
                       cdAtEnd && cdValEnd > 0f &&
                       Mathf.Approximately(rageAtP1, 35f) &&
                       Mathf.Approximately(rageAtP2, 35f) &&
                       Mathf.Approximately(rageAtP3, 35f) &&
                       channelFinished;

                Debug.Log($"[P08 T28] Rage And Cooldown Follow Once Per Execution Contract: ResOk={res.Success}, RagePreserved={Mathf.Approximately(rageAtP3, 35f)}, CdTriggeredOnce={cdAtEnd}, Finished={channelFinished} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                SkillExecutor.EnableRageCost = true;
                SkillExecutor.EnableCooldown = true;
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T29_ValidLegacyHeroToMonsterSingleTarget()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.SingleTarget, 0f, 20);
            var skill = CreateConfiguredSkill("p08_single_hero_to_monster", effect, rageCost: 10f, cooldown: 2f);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);

                hero.Rage.ResetRage(100f);
                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);

                bool valid = SkillExecutionValidator.Validate(req, out _, out _);
                var res = SkillExecutor.Execute(req);

                bool hitMonster = Mathf.Approximately(m1.Health.CurrentHealth, 490f);
                pass = valid && res.Success && hitMonster;

                Debug.Log($"[P08 T29] Valid Legacy Hero To Monster SingleTarget: Valid={valid}, ResSuccess={res.Success}, HitMonster={hitMonster} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T30_ValidLegacyMonsterToHeroSingleTarget()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.SingleTarget, 0f, 20);
            var skill = CreateConfiguredSkill("p08_single_monster_to_hero", effect, rageCost: 0f, cooldown: 2f);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);

                // Monster targets Hero with SingleTarget offensive skill
                var req = new SkillExecutionRequest(m1, skill, SkillSlotType.NormalAttack, hero);

                bool valid = SkillExecutionValidator.Validate(req, out var reason, out var msg);
                var res = SkillExecutor.Execute(req);

                bool hitHero = Mathf.Approximately(hero.Health.CurrentHealth, 990f);
                pass = valid && res.Success && hitHero;

                Debug.Log($"[P08 T30] Valid Legacy Monster To Hero SingleTarget: Valid={valid}, Reason={reason}, ResSuccess={res.Success}, HitHero={hitHero} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T31_InvalidFriendlyOrDeadSingleTargetRejected()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.SingleTarget, 0f, 20);
            var skill = CreateConfiguredSkill("p08_single_invalid_test", effect, rageCost: 15f, cooldown: 5f);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);

                hero.Rage.ResetRage(100f);

                // 1. Friendly target (Hero targeting self with offensive SingleTarget skill)
                var reqFriendly = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, hero);
                bool validFriendly = SkillExecutionValidator.Validate(reqFriendly, out var reasonFriendly, out _);
                var resFriendly = SkillExecutor.Execute(reqFriendly);

                // 2. Dead target
                m1.Health.SetCurrentHealth(0f);
                var reqDead = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, m1);
                bool validDead = SkillExecutionValidator.Validate(reqDead, out var reasonDead, out _);
                var resDead = SkillExecutor.Execute(reqDead);

                bool ragePreserved = Mathf.Approximately(hero.Rage.CurrentRage, 100f);
                bool noCooldown = !CooldownManager.IsOnCooldown(skill.SkillId, out _);

                pass = !validFriendly && reasonFriendly == SkillExecutionFailureReason.TargetInvalidOrDead && !resFriendly.Success &&
                       !validDead && reasonDead == SkillExecutionFailureReason.TargetInvalidOrDead && !resDead.Success &&
                       ragePreserved && noCooldown;

                Debug.Log($"[P08 T31] Invalid Friendly Or Dead SingleTarget Rejected: FriendlyBlocked={!validFriendly}, DeadBlocked={!validDead}, RagePreserved={ragePreserved}, NoCooldown={noCooldown} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T32_UnregisteredAoeTargetRejected()
        {
            var (heroGO, hero) = CreateMockHero();
            var (mUnregGO, mUnreg) = CreateMockMonster("M_Unregistered", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 5f, 20);
            var skill = CreateConfiguredSkill("p08_aoe_unregistered_test", effect, rageCost: 15f, cooldown: 5f);
            RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                // Deliberately DO NOT register mUnreg in BattleManager!

                hero.Rage.ResetRage(100f);
                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, mUnreg);

                bool valid = SkillExecutionValidator.Validate(req, out var reason, out _);
                var res = SkillExecutor.Execute(req);

                bool ragePreserved = Mathf.Approximately(hero.Rage.CurrentRage, 100f);
                bool noCooldown = !CooldownManager.IsOnCooldown(skill.SkillId, out _);

                pass = !valid && reason == SkillExecutionFailureReason.TargetInvalidOrDead && !res.Success && ragePreserved && noCooldown;

                Debug.Log($"[P08 T32] Unregistered AOE Target Rejected: ValidBlocked={!valid}, Reason={reason}, ResBlocked={!res.Success}, RagePreserved={ragePreserved} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                CooldownManager.ResetAllCooldowns();
                UnityEngine.Object.DestroyImmediate(skill);
                UnityEngine.Object.DestroyImmediate(effect);
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(mUnregGO);
                UnityEngine.Object.DestroyImmediate(bmGO);
                UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T33_OldOrExternalMonsterDeathDoesNotAffectEncounter()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1_Active", new Vector3(2f, 0f, 0f));
            var (mExternalGO, mExternal) = CreateMockMonster("M_External", new Vector3(10f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                // mExternal is deliberately NOT registered into bm.ActiveMonsters

                bm.StartBattle();
                int initialEncounter = bm.EncounterIndex;
                BattleState initialBattleState = bm.CurrentBattleState;
                Entity initialTarget = hero.CurrentTarget;
                int initialQueueCount = bm.PendingLootQueueCount;

                // 1. External monster dies
                mExternal.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                EventBus.RaiseEntityDied(mExternal);

                bool stateUnchanged = (bm.CurrentBattleState == initialBattleState);
                bool encounterUnchanged = (bm.EncounterIndex == initialEncounter);
                bool heroTargetUnchanged = (hero.CurrentTarget == initialTarget && hero.CurrentTarget == m1);
                bool lootUnchanged = (bm.PendingLootQueueCount == initialQueueCount);
                bool m1StillActive = (bm.CurrentMonster == m1 && m1.IsAlive);

                // 2. Active monster dies once
                m1.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));

                // 3. Repeated death event on now-dead monster must be completely idempotent
                int encAfterDeath = bm.EncounterIndex;
                BattleState stateAfterDeath = bm.CurrentBattleState;
                EventBus.RaiseEntityDied(m1);
                EventBus.RaiseEntityDied(m1);
                bool duplicateDeathIdempotent = (bm.EncounterIndex == encAfterDeath && bm.CurrentBattleState == stateAfterDeath);

                pass = stateUnchanged && encounterUnchanged && heroTargetUnchanged && lootUnchanged && m1StillActive && duplicateDeathIdempotent;
                Debug.Log($"[P08 T33] External & Old Monster Death Guard: StateUnchanged={stateUnchanged}, EncUnchanged={encounterUnchanged}, TargetUnchanged={heroTargetUnchanged}, LootUnchanged={lootUnchanged}, Idempotent={duplicateDeathIdempotent} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(mExternalGO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T34_MultiEffectSkillFinishingEncounterDoesNotHitNewEncounter()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            // Isolate unit test fixture from pre-existing scene monsters in open editor scene
            var ambientMonsters = UnityEngine.Object.FindObjectsByType<Monster>(FindObjectsInactive.Exclude);
            var disabledAmbient = new List<GameObject>();
            foreach (var amb in ambientMonsters)
            {
                if (amb != m1 && amb.gameObject.activeInHierarchy)
                {
                    amb.gameObject.SetActive(false);
                    disabledAmbient.Add(amb.gameObject);
                }
            }

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.StartBattle();

                // Setup 2-effect skill: Effect 1 deals 100 damage (kills 50-HP M1), Effect 2 deals 100 damage
                hero.Stats.SetBaseValue(StatType.Attack, 100f);
                m1.Health.InitializeHealth(50f, m1);
                m1.Stats.SetBaseValue(StatType.Health, 50f);
                m1.Stats.SetBaseValue(StatType.MaxHealth, 50f);
                m1.Stats.SetBaseValue(StatType.Dodge, 0f);
                m1.Stats.SetBaseValue(StatType.Defense, 0f);

                var eff1 = CreateConfiguredEffect(SkillTargetPolicy.SingleTarget, 0f, 1);
                var eff2 = CreateConfiguredEffect(SkillTargetPolicy.SingleTarget, 0f, 1);

                var multiSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                multiSkill.InitializeSkill(
                    id: "skill_multi_eff_test",
                    mmId: mm.ActiveMindMethodId,
                    slot: SkillSlotType.Skill,
                    name: "Multi Effect Test Skill",
                    desc: "Two effect skill",
                    conditions: null,
                    dmgMultiplier: 1.0f,
                    costRage: 20f,
                    cd: 5f,
                    passive: false,
                    skillEffects: new List<SkillEffectDefinitionSO> { eff1, eff2 }
                );

                RegisterTestSkillToMindMethod(mm, multiSkill, SkillSlotType.Skill);
                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                // Track exact damage events with target identity, damage amount, and encounter index
                int damageEventsOnM1 = 0;
                int foreignEntityDamageEvents = 0;
                int damageEventsInEncounter2OrHigher = 0;

                Action<Entity, DamageResult> onDamage = (victim, res) =>
                {
                    if (victim == m1)
                    {
                        damageEventsOnM1++;
                    }
                    else
                    {
                        foreignEntityDamageEvents++;
                        Debug.LogError($"[T34 DAMAGE BLEED DETECTED] Victim={victim?.EntityName ?? "null"}, Attacker={res.Attacker?.EntityName ?? "null"}, FinalDamage={res.FinalDamage:F1}, Encounter={bm.EncounterIndex}");
                    }

                    if (bm.EncounterIndex > 1)
                    {
                        damageEventsInEncounter2OrHigher++;
                    }
                };
                EventBus.OnEntityDamaged += onDamage;

                // Execute the 2-effect skill through real pipeline
                var req = new SkillExecutionRequest(hero, multiSkill, SkillSlotType.Skill, m1);
                var execRes = SkillExecutor.Execute(req);

                EventBus.OnEntityDamaged -= onDamage;

                // Effect 1 defeats M1
                bool m1Dead = (m1 == null || !m1.IsAlive || m1.Health.CurrentHealth <= 0f);
                bool execSuccess = execRes.Success;
                bool rageConsumedOnce = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                bool cdTriggeredOnce = CooldownManager.IsOnCooldown(multiSkill.SkillId, out _);

                // Effect 2 results: must NOT damage any monster from a new encounter
                bool noForeignHits = (foreignEntityDamageEvents == 0);
                bool noBleedToEncounter2 = (damageEventsInEncounter2OrHigher == 0);
                bool exactlyOneHitOnM1 = (damageEventsOnM1 == 1);
                bool encounterEndedCleanly = (bm.CurrentBattleState == BattleState.EncounterTransition || bm.CurrentBattleState == BattleState.MonsterDead);

                // After execution completes, verify encounter advances cleanly to Encounter 2 when transition is processed
                int encBeforeAdvance = bm.EncounterIndex;
                bm.EndEncounterAndStartNext();
                bool encounterAdvancedToNext = (bm.EncounterIndex == encBeforeAdvance + 1 && bm.CurrentMonster != null && bm.CurrentMonster.IsAlive);

                pass = execSuccess && m1Dead && rageConsumedOnce && cdTriggeredOnce && exactlyOneHitOnM1 && noForeignHits && noBleedToEncounter2 && encounterEndedCleanly && encounterAdvancedToNext;
                Debug.Log($"[P08 T34] Multi-Effect Encounter Isolation: ExecSuccess={execSuccess}, M1Dead={m1Dead}, RageOnce={rageConsumedOnce}, CdOnce={cdTriggeredOnce}, HitsM1={damageEventsOnM1}, ForeignHits={foreignEntityDamageEvents}, BleedEnc2={damageEventsInEncounter2OrHigher}, State={bm.CurrentBattleState}, Advanced={encounterAdvancedToNext} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                foreach (var ambGo in disabledAmbient)
                {
                    if (ambGo != null) ambGo.SetActive(true);
                }
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (m1GO != null) UnityEngine.Object.DestroyImmediate(m1GO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
                if (mmGO != null) UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
                CooldownManager.ResetAllCooldowns();
            }
            return pass;
        }

        public static bool T35_LootLifecycleContinuesAfterLongModalHold()
        {
            var (heroGO, hero) = CreateMockHero();
            var (m1GO, m1) = CreateMockMonster("M1", new Vector3(2f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterMonster(m1);
                bm.StartBattle();

                var drop1 = new EquipmentInstance("drop_hold_1", "Sword of Patience", EquipmentSlotType.Weapon, 1, null, new List<AffixInstance>());
                var drop2 = new EquipmentInstance("drop_hold_2", "Shield of Endurance", EquipmentSlotType.Armor, 1, null, new List<AffixInstance>());
                bm.EnqueuePendingLoot(drop1);
                bm.EnqueuePendingLoot(drop2);

                // Kill monster to enter LootPending with drop1
                m1.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));

                bool enteredLootPending1 = (bm.CurrentBattleState == BattleState.LootPending && bm.PendingLootItem == drop1);

                // Complete decision for drop1 (Equip)
                bool completeRes1 = bm.CompleteLootDecisionAndResume(equip: true, dismantle: false);

                // Sequential queue: drop2 popped into pendingLootItem
                bool drop2Presented = (bm.PendingLootItem == drop2 && bm.CurrentBattleState == BattleState.LootPending);

                // Complete decision for drop2 (Dismantle)
                bool completeRes2 = bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);

                bool allLootResolved = (bm.PendingLootItem == null && bm.PendingLootQueueCount == 0);
                bool resumed = (bm.CurrentBattleState == BattleState.InProgress || bm.CurrentBattleState == BattleState.EncounterTransition || bm.EncounterIndex > 1);

                pass = enteredLootPending1 && completeRes1 && drop2Presented && completeRes2 && allLootResolved && resumed;
                Debug.Log($"[P08 T35] Sequential Loot Resolution: Drop1Pending={enteredLootPending1}, Complete1={completeRes1}, Drop2Presented={drop2Presented}, Complete2={completeRes2}, AllResolved={allLootResolved}, Resumed={resumed} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T36_NormalWaveSpawnsExpectedCount_4or5()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T36", new Vector3(-2.2f, -0.3f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            var origRandomState = UnityEngine.Random.state;
            try
            {
                bm.RegisterHero(hero);

                // Branch 1: Explicit 4-monster wave
                bm.PrepareAndStartNormalWave(4);
                bool count4Ok = (bm.ActiveMonsters.Count == 4);
                bool all4Alive = true;
                bool all4DistinctY = true;
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    var m = bm.ActiveMonsters[i];
                    if (m == null || !m.IsAlive || m.Health == null || m.Health.CurrentHealth <= 0f) all4Alive = false;
                    if (m != null && Mathf.Abs(m.transform.position.y - (-0.3f)) > 0.001f) all4DistinctY = false;
                }
                bool target4Ok = (bm.CurrentMonster == bm.ActiveMonsters[0] && hero.CurrentTarget == bm.ActiveMonsters[0]);
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    if (bm.ActiveMonsters[i].CurrentTarget != hero) target4Ok = false;
                }

                // Branch 2: Explicit 5-monster wave
                bm.PrepareAndStartNormalWave(5);
                bool count5Ok = (bm.ActiveMonsters.Count == 5);
                bool all5Alive = true;
                bool all5DistinctY = true;
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    var m = bm.ActiveMonsters[i];
                    if (m == null || !m.IsAlive || m.Health == null || m.Health.CurrentHealth <= 0f) all5Alive = false;
                    if (m != null && Mathf.Abs(m.transform.position.y - (-0.3f)) > 0.001f) all5DistinctY = false;
                }
                bool target5Ok = (bm.CurrentMonster == bm.ActiveMonsters[0] && hero.CurrentTarget == bm.ActiveMonsters[0]);
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    if (bm.ActiveMonsters[i].CurrentTarget != hero) target5Ok = false;
                }

                // Branch 3: Default random pick (must strictly be in {4, 5})
                bm.PrepareAndStartNormalWave();
                bool randomBranchOk = (bm.ActiveMonsters.Count == 4 || bm.ActiveMonsters.Count == 5);

                pass = count4Ok && all4Alive && all4DistinctY && target4Ok &&
                       count5Ok && all5Alive && all5DistinctY && target5Ok &&
                       randomBranchOk;

                Debug.Log($"[P08 T36] Wave Count Contract: Branch4={count4Ok} (Alive={all4Alive}, Y={all4DistinctY}, Target={target4Ok}), Branch5={count5Ok} (Alive={all5Alive}, Y={all5DistinctY}, Target={target5Ok}), RandomBranch={randomBranchOk} ({bm.ActiveMonsters.Count}) | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Random.state = origRandomState;
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T37_ConsecutiveThreeWaves_SpawnsAndProgressesCleanly()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T37", new Vector3(-2.2f, -0.3f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);

                // Wave 1
                bm.PrepareAndStartNormalWave(4);
                bool w1Ok = (bm.ActiveMonsters.Count == 4 && bm.EncounterIndex == 1 && bm.CompletedNormalWaveCount == 0);

                // Wave 1 defeat via loot path
                var drop1 = new EquipmentInstance("drop_t37_1", "Helm of Victory", EquipmentSlotType.Helmet, 1, null, new List<AffixInstance>());
                bm.EnqueuePendingLoot(drop1);
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    bm.ActiveMonsters[i].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                }
                bool w1LootPending = (bm.CurrentBattleState == BattleState.LootPending && bm.PendingLootItem == drop1);
                bm.CompleteLootDecisionAndResume(equip: true);

                // Wave 2 starts via loot advance
                if (bm.CurrentBattleState == BattleState.EncounterTransition)
                {
                    typeof(BattleManager).GetMethod("AdvanceEncounterAfterLoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(bm, null);
                }
                bool w2Ok = (bm.ActiveMonsters.Count >= 4 && bm.ActiveMonsters.Count <= 5 && bm.EncounterIndex == 2 && bm.CompletedNormalWaveCount == 1);

                // Wave 2 defeat via non-loot path
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    bm.ActiveMonsters[i].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                }
                bm.EndEncounterAndStartNext();

                // Wave 3 starts
                bool w3Ok = (bm.ActiveMonsters.Count >= 4 && bm.ActiveMonsters.Count <= 5 && bm.EncounterIndex == 3 && bm.CompletedNormalWaveCount == 2);

                pass = w1Ok && w1LootPending && w2Ok && w3Ok;
                Debug.Log($"[P08 T37] 3 Consecutive Waves: Wave1={w1Ok}, W1LootPending={w1LootPending}, Wave2={w2Ok}, Wave3={w3Ok} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T38_WaveStatGrowth_ExactMultiplierProgression()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T38", new Vector3(-2.2f, -0.3f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);

                // Wave 1: Completed=0 -> multiplier = 1.0 (500 / 50 / 10)
                bm.PrepareAndStartNormalWave(4);
                bool w1StatsOk = true;
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    var m = bm.ActiveMonsters[i];
                    if (Mathf.Abs(m.Health.MaxHealth - 500f) > 0.001f) w1StatsOk = false;
                    if (Mathf.Abs(m.Health.CurrentHealth - 500f) > 0.001f) w1StatsOk = false;
                    if (Mathf.Abs(m.Stats.GetValue(StatType.Attack) - 50f) > 0.001f) w1StatsOk = false;
                    if (Mathf.Abs(m.Stats.GetValue(StatType.Defense) - 10f) > 0.001f) w1StatsOk = false;
                    if (Mathf.Abs(m.Stats.GetValue(StatType.MoveSpeed) - 3f) > 0.001f) w1StatsOk = false;
                    if (Mathf.Abs(m.Stats.GetValue(StatType.AttackInterval) - 2f) > 0.001f) w1StatsOk = false;
                }

                // Defeat Wave 1
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    bm.ActiveMonsters[i].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                }
                bool w1DefeatedOk = (bm.CompletedNormalWaveCount == 1 && Mathf.Abs(bm.NextWaveStatMultiplier - 1.01f) < 0.0001f);

                // Wave 2: Completed=1 -> multiplier = 1.01 (505 / 50.5 / 10.1)
                bm.PrepareAndStartNormalWave(4);
                bool w2StatsOk = (bm.CurrentWaveTier == 1 && Mathf.Abs(bm.CurrentWaveStatMultiplier - 1.01f) < 0.0001f);
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    var m = bm.ActiveMonsters[i];
                    if (Mathf.Abs(m.Health.MaxHealth - 505.0f) > 0.001f) w2StatsOk = false;
                    if (Mathf.Abs(m.Health.CurrentHealth - 505.0f) > 0.001f) w2StatsOk = false;
                    if (Mathf.Abs(m.Stats.GetValue(StatType.Attack) - 50.5f) > 0.001f) w2StatsOk = false;
                    if (Mathf.Abs(m.Stats.GetValue(StatType.Defense) - 10.1f) > 0.001f) w2StatsOk = false;
                }

                // Defeat Wave 2
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    bm.ActiveMonsters[i].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                }
                bool w2DefeatedOk = (bm.CompletedNormalWaveCount == 2 && Mathf.Abs(bm.NextWaveStatMultiplier - 1.0201f) < 0.0001f);

                // Wave 3: Completed=2 -> multiplier = 1.0201 (510.05 / 51.005 / 10.201)
                bm.PrepareAndStartNormalWave(4);
                bool w3StatsOk = (bm.CurrentWaveTier == 2 && Mathf.Abs(bm.CurrentWaveStatMultiplier - 1.0201f) < 0.0001f);
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    var m = bm.ActiveMonsters[i];
                    if (Mathf.Abs(m.Health.MaxHealth - 510.05f) > 0.001f) w3StatsOk = false;
                    if (Mathf.Abs(m.Health.CurrentHealth - 510.05f) > 0.001f) w3StatsOk = false;
                    if (Mathf.Abs(m.Stats.GetValue(StatType.Attack) - 51.005f) > 0.001f) w3StatsOk = false;
                    if (Mathf.Abs(m.Stats.GetValue(StatType.Defense) - 10.201f) > 0.001f) w3StatsOk = false;
                }

                pass = w1StatsOk && w1DefeatedOk && w2StatsOk && w2DefeatedOk && w3StatsOk;
                Debug.Log($"[P08 T38] Stat Growth Formula Verification: W1(500/50/10)={w1StatsOk}, W1Defeat={w1DefeatedOk}, W2(505/50.5/10.1)={w2StatsOk}, W2Defeat={w2DefeatedOk}, W3(510.05/51.005/10.201)={w3StatsOk} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T39_WaveProgressionIdempotency_NoDoubleScaling()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T39", new Vector3(-2.2f, -0.3f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.PrepareAndStartNormalWave(4);

                // 1. Partial kill (2 of 4) does NOT advance tier
                bm.ActiveMonsters[0].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                bm.ActiveMonsters[1].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                bool partialKillOk = (bm.CompletedNormalWaveCount == 0);

                // 2. RegisterMonster does NOT advance tier
                var (extraM_GO, extraM) = CreateMockMonster("ExtraMonster", new Vector3(6f, -0.3f, 0f));
                bm.RegisterMonster(extraM);
                bool registerOk = (bm.CompletedNormalWaveCount == 0);
                bm.UnregisterMonster(extraM);
                UnityEngine.Object.DestroyImmediate(extraM_GO);

                // 3. RecalculateStats does NOT advance tier
                bm.ActiveMonsters[2].RecalculateStats();
                bool recalcOk = (bm.CompletedNormalWaveCount == 0);

                // 4. Pause / Resume does NOT advance tier
                bm.PauseCombat();
                bm.ResumeCombat();
                bool pauseResumeOk = (bm.CompletedNormalWaveCount == 0);

                // 5. Defeat remaining monsters: completed count increments to 1
                var lastMonster = bm.ActiveMonsters[3];
                bm.ActiveMonsters[2].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                lastMonster.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                bool fullKillOk = (bm.CompletedNormalWaveCount == 1);

                // 6. Duplicate death event does NOT increment tier again
                EventBus.RaiseEntityDied(lastMonster);
                bool dupDeathOk = (bm.CompletedNormalWaveCount == 1);

                // 7. Multiple sequential loot decisions do NOT increment tier
                var dropA = new EquipmentInstance("drop_idemp_a", "Item A", EquipmentSlotType.Weapon, 1, null, new List<AffixInstance>());
                var dropB = new EquipmentInstance("drop_idemp_b", "Item B", EquipmentSlotType.Armor, 1, null, new List<AffixInstance>());
                bm.EnqueuePendingLoot(dropA);
                bm.EnqueuePendingLoot(dropB);
                bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                bool multiLootOk = (bm.CompletedNormalWaveCount == 1);

                // 8. Hero death & retry preserves tier without adding a false win
                hero.Health.TakeDamage(new DamageResult(null, null, 99999f, 99999f, false, false, DamageType.Skill));
                bool heroDeadState = (bm.CurrentBattleState == BattleState.AwaitingPlayerStart);
                bm.StartCombatAfterHeroDeath();
                bool retryPreservesTier = (bm.CompletedNormalWaveCount == 1 && bm.CurrentWaveTier == 1);

                pass = partialKillOk && registerOk && recalcOk && pauseResumeOk && fullKillOk && dupDeathOk && multiLootOk && heroDeadState && retryPreservesTier;
                Debug.Log($"[P08 T39] Progression Idempotency: PartialKill={partialKillOk}, Register={registerOk}, Recalc={recalcOk}, PauseResume={pauseResumeOk}, FullKill={fullKillOk}, DupDeath={dupDeathOk}, MultiLoot={multiLootOk}, HeroDead={heroDeadState}, RetryPreservedTier={retryPreservesTier} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T40_RecalculateStatsPreservesScaledBaseStats()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T40", new Vector3(-2.2f, -0.3f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);

                // Advance to Wave 2 (tier 1: HP=505, Atk=50.5, Def=10.1)
                bm.PrepareAndStartNormalWave(4);
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    bm.ActiveMonsters[i].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                }
                bm.PrepareAndStartNormalWave(4);

                var monster = bm.ActiveMonsters[0];
                float hpBefore = monster.Health.MaxHealth;
                float atkBefore = monster.Stats.GetValue(StatType.Attack);
                float defBefore = monster.Stats.GetValue(StatType.Defense);

                bool initialScaledOk = (Mathf.Abs(hpBefore - 505.0f) < 0.001f && Mathf.Abs(atkBefore - 50.5f) < 0.001f && Mathf.Abs(defBefore - 10.1f) < 0.001f);

                // Recalculate stats: must remain at scaled base!
                monster.RecalculateStats();
                float hpAfterRecalc = monster.Health.MaxHealth;
                float atkAfterRecalc = monster.Stats.GetValue(StatType.Attack);
                float defAfterRecalc = monster.Stats.GetValue(StatType.Defense);

                bool recalcPreservedOk = (Mathf.Abs(hpAfterRecalc - 505.0f) < 0.001f && Mathf.Abs(atkAfterRecalc - 50.5f) < 0.001f && Mathf.Abs(defAfterRecalc - 10.1f) < 0.001f);

                // Apply temporary Attack buff (+10)
                monster.Stats.ModifyValue(StatType.Attack, 10f);
                float atkWithBuff = monster.Stats.GetValue(StatType.Attack);
                bool buffAppliedOk = (Mathf.Abs(atkWithBuff - 60.5f) < 0.001f);

                // RecalculateStats (buff expiration): must revert to 50.5, NOT wave 1's 50.0!
                monster.RecalculateStats();
                float atkAfterBuffExpire = monster.Stats.GetValue(StatType.Attack);
                bool buffExpiredRevertOk = (Mathf.Abs(atkAfterBuffExpire - 50.5f) < 0.001f);

                // Check that original disk asset was untouched
                var diskConfig = Resources.Load<MonsterConfigSO>("Data/MonsterConfig");
#if UNITY_EDITOR
                if (diskConfig == null) diskConfig = AssetDatabase.LoadAssetAtPath<MonsterConfigSO>("Assets/_Game/Data/MonsterConfig.asset");
#endif
                bool diskUntouchedOk = (diskConfig != null && Mathf.Abs(diskConfig.MaxHealth - 500f) < 0.001f && Mathf.Abs(diskConfig.Attack - 50f) < 0.001f && Mathf.Abs(diskConfig.Defense - 10f) < 0.001f);

                pass = initialScaledOk && recalcPreservedOk && buffAppliedOk && buffExpiredRevertOk && diskUntouchedOk;
                Debug.Log($"[P08 T40] RecalculateStats Preservation: InitialScaled={initialScaledOk}, Preserved={recalcPreservedOk}, BuffApplied={buffAppliedOk}, BuffExpireReverted={buffExpiredRevertOk}, DiskUntouched={diskUntouchedOk} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T41_ChannelMidWaveDeathLifecycleAndCleanNextWaveSpawn()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T41", new Vector3(-2.2f, -0.3f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.PrepareAndStartNormalWave(4);

                // Defeat first 3 monsters so only 1 monster remains (the FINAL monster of the wave)
                for (int i = 0; i < 3; i++)
                {
                    bm.ActiveMonsters[i].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                }
                var finalMonster = bm.ActiveMonsters[3];
                hero.SetCurrentTarget(finalMonster);

                // Create channel skill: duration=1.5s, 3 ticks
                var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);
                var skill = CreateConfiguredSkill("skill_t41", effect, rageCost: 20f, cooldown: 5f, isChannel: true, channelDuration: 1.5f, channelTickInterval: 0.5f);
                RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

                hero.Rage.ResetRage(100f);
                CooldownManager.ResetAllCooldowns();

                var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, finalMonster);
                var startResult = SkillExecutor.Execute(req);
                bool channelStarted = startResult.Success && hero.CastState.IsActive && hero.CastState.CurrentPhase == SkillCastPhase.Channeling;

                // Pulse 1 defeats the final monster
                finalMonster.Health.TakeDamage(new DamageResult(hero, finalMonster, 1000f, 1000f, false, false, DamageType.Skill));

                // Assertions at final monster death mid-channel:
                bool canTickDuringTransition = bm.CanEntityTickDuringTransition(hero);
                bool channelNotAborted = (hero.CastState.IsActive && hero.CastState.CurrentPhase == SkillCastPhase.Channeling);
                bool nextWaveNotPremature = (bm.EncounterIndex == 1 && bm.ActiveMonsters.Count == 4);

                // Tick channel through completion
                hero.CastState.Tick(0.6f);
                hero.CastState.Tick(0.6f);
                hero.CastState.Tick(0.4f);

                bool channelFinishedCleanly = (hero.CastState.IsFinished || !hero.CastState.IsActive);
                bool rageConsumedOnce = (hero.Rage.CurrentRage == 80f);
                bool cooldownActive = CooldownManager.IsOnCooldown(skill.SkillId, out _);

                pass = channelStarted && canTickDuringTransition && channelNotAborted && nextWaveNotPremature && channelFinishedCleanly && rageConsumedOnce && cooldownActive;
                Debug.Log($"[P08 T41] Channel Final Death Lifecycle: Started={channelStarted}, CanTickDuringTransition={canTickDuringTransition}, ChannelNotAborted={channelNotAborted}, NextWaveNotPremature={nextWaveNotPremature}, Finished={channelFinishedCleanly}, RageOnce={rageConsumedOnce}, Cooldown={cooldownActive} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
                if (mmGO != null) UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }

        public static bool T42_ModalRaceAndHeroDeathDuringTransition()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T42", new Vector3(-2.2f, -0.3f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.PrepareAndStartNormalWave(4);

                // Ensure ModalCoordinator exists to test real modal race
                GameObject coordGO = null;
                var coord = ModalCoordinator.Instance;
                if (coord == null)
                {
                    coordGO = new GameObject("ModalCoordinator_T42");
                    coord = coordGO.AddComponent<ModalCoordinator>();
                }

                var dummyView = new DummyModalView_P08 { ModalId = "TestModal_T42" };
                coord.RegisterModalView(dummyView);

                // Kill all monsters to trigger wave completion transition
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    bm.ActiveMonsters[i].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                }

                // Request a blocking modal to test modal race during transition
                var req = new ModalRequest("TestModal_T42", ModalPriority.SystemProgression, true, null);
                coord.RequestModal(req);

                bool modalActive = (coord.ActiveBlockingModalCount > 0);
                bool transitionBlocked = (bm.EncounterIndex == 1);

                // In transition state while modal is active, Hero dies
                hero.Health.TakeDamage(new DamageResult(null, null, 99999f, 99999f, false, false, DamageType.Skill));

                // Verify lifecycle cancelled immediately and battle state is AwaitingPlayerStart
                bool heroDeadHandled = (bm.CurrentBattleState == BattleState.AwaitingPlayerStart && !bm.IsBattleActive);

                if (coord.ActiveBlockingModalCount > 0)
                {
                    coord.DismissActiveModal(DismissalReason.SystemDismissed);
                }
                coord.UnregisterModalView(dummyView);
                if (coordGO != null) UnityEngine.Object.DestroyImmediate(coordGO);
                ModalCoordinator.ResetInstance();

                pass = modalActive && transitionBlocked && heroDeadHandled;
                Debug.Log($"[P08 T42] Modal Race & Hero Death During Transition: ModalActive={modalActive}, TransitionBlocked={transitionBlocked}, AwaitingPlayerStart={heroDeadHandled} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T44_UnregisteredOrDeactivatedMonsterDoesNotCountAsWaveDefeat()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T44", new Vector3(-2.2f, -0.3f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.PrepareAndStartNormalWave(5);

                bool initial5Spawned = (bm.ActiveMonsters.Count == 5);
                var livingMonsterToUnregister = bm.ActiveMonsters[4];

                // 1. Unregister living monster 5 outside legitimate death
                bm.UnregisterMonster(livingMonsterToUnregister);
                bool monsterUnregisteredFromActive = true;
                for (int mi = 0; mi < bm.ActiveMonsters.Count; mi++)
                {
                    if (bm.ActiveMonsters[mi] == livingMonsterToUnregister)
                    {
                        monsterUnregisteredFromActive = false;
                        break;
                    }
                }

                // 2. Kill the remaining 4 monsters legitimately
                for (int i = 0; i < 4; i++)
                {
                    bm.ActiveMonsters[i].Health.TakeDamage(new DamageResult(hero, bm.ActiveMonsters[i], 1000f, 1000f, false, false, DamageType.Skill));
                }

                // 3. Verify wave is NOT fully defeated because monster 5 was NOT legitimately killed!
                bool waveNotDefeatedAfterUnregister = !bm.IsWaveFullyDefeated();
                bool waveCountNotIncremented = (bm.CompletedNormalWaveCount == 0);
                bool encounterNotAdvanced = (bm.EncounterIndex == 1);

                // 4. Test deactivated monster outside combat death
                livingMonsterToUnregister.gameObject.SetActive(false);
                bool waveStillNotDefeatedAfterDeactivate = !bm.IsWaveFullyDefeated();

                // 5. Test repeated death event on already dead monster: does not cause double counting or victory
                var deadMonster = bm.ActiveMonsters[0];
                EventBus.RaiseEntityDied(deadMonster);
                bool waveCountStillZeroAfterDuplicateDeath = (bm.CompletedNormalWaveCount == 0);

                pass = initial5Spawned && monsterUnregisteredFromActive && waveNotDefeatedAfterUnregister &&
                       waveCountNotIncremented && encounterNotAdvanced && waveStillNotDefeatedAfterDeactivate &&
                       waveCountStillZeroAfterDuplicateDeath;

                Debug.Log($"[P08 T44] Wave Defeat Decoupling: Initial5={initial5Spawned}, UnregLivingNotDefeated={waveNotDefeatedAfterUnregister}, WaveCountZero={waveCountNotIncremented}, Enc1={encounterNotAdvanced}, DeactivateNotDefeated={waveStillNotDefeatedAfterDeactivate}, DupDeathSafe={waveCountStillZeroAfterDuplicateDeath} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T45_EntryPointsProtectedAgainstInvalidStartAndLootState()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T45", new Vector3(-2.2f, -0.3f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.PrepareAndStartNormalWave(4);

                // 1. Enter LootPending state manually
                var stateProp = typeof(BattleManager).GetProperty("CurrentBattleState");
                stateProp.SetValue(bm, BattleState.LootPending);
                var lootQueueField = typeof(BattleManager).GetField("_pendingLootQueue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var pendingLootField = typeof(BattleManager).GetField("pendingLootItem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                var dummyItem = new EquipmentInstance("t45_drop", "T45 Drop", EquipmentSlotType.Weapon, 1, null, new List<AffixInstance>());
                var queue = lootQueueField.GetValue(bm) as Queue<EquipmentInstance>;
                queue.Enqueue(dummyItem);

                // StartBattle() called during LootPending must be rejected without cancelling loot lifecycle
                bm.StartBattle();
                bool startBattleRejectedInLoot = (bm.CurrentBattleState == BattleState.LootPending && queue.Count == 1);

                // ExecutePlayerStartCommand() called during LootPending must be rejected
                bm.ExecutePlayerStartCommand();
                bool playerStartRejectedInLoot = (bm.CurrentBattleState == BattleState.LootPending);

                // PrepareAndStartNormalWave() called during LootPending must be rejected
                int waveMembersBefore = bm.CurrentWaveMembers.Count;
                bm.PrepareAndStartNormalWave(4);
                bool prepareWaveRejectedInLoot = (bm.CurrentWaveMembers.Count == waveMembersBefore);

                // CompleteLootDecisionAndResume() with pendingLootItem == null must NOT dequeue from queue
                pendingLootField.SetValue(bm, null);
                bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                bool rogueDequeueBlocked = (queue.Count == 1 && bm.PendingLootItem == null);

                // Now present item legitimately
                pendingLootField.SetValue(bm, dummyItem);
                queue.Dequeue();
                bm.CompleteLootDecisionAndResume(equip: false, dismantle: true);
                bool legitimateDecisionWorked = (bm.PendingLootItem == null);

                // Stale duplicate call to CompleteLootDecisionAndResume
                bm.CompleteLootDecisionAndResume(equip: true, dismantle: false);
                bool duplicateCallSafe = (bm.PendingLootItem == null && queue.Count == 0);

                pass = startBattleRejectedInLoot && playerStartRejectedInLoot && prepareWaveRejectedInLoot &&
                       rogueDequeueBlocked && legitimateDecisionWorked && duplicateCallSafe;

                Debug.Log($"[P08 T45] Entry Points Protection: StartRejectedInLoot={startBattleRejectedInLoot}, PlayerStartRejectedInLoot={playerStartRejectedInLoot}, PrepareWaveRejected={prepareWaveRejectedInLoot}, RogueDequeueBlocked={rogueDequeueBlocked}, LegitimateWorked={legitimateDecisionWorked}, DupCallSafe={duplicateCallSafe} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T46_SeparateSupportedCastersExecuteIndependently()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T46_Main", new Vector3(0f, 0f, 0f));
            var (allyGO, ally) = CreateMockHero("Hero_T46_Ally", new Vector3(-2f, 0f, 0f));
            var (foreignGO, foreignHero) = CreateMockHero("Hero_T46_Foreign", new Vector3(100f, 0f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.RegisterAlly(ally);
                bm.PrepareAndStartNormalWave(4);

                // 1. Encounter ownership verification
                bool heroBelongs = bm.BelongsToEncounter(hero);
                bool allyBelongs = bm.BelongsToEncounter(ally);
                bool foreignDoesNotBelong = !bm.BelongsToEncounter(foreignHero);

                // 2. Transition tick permission: foreign hero outside encounter can never tick during transition
                bool foreignCannotTick = !bm.CanEntityTickDuringTransition(foreignHero);

                // 3. Foreign hero death in scene does NOT trigger player defeat (AwaitingPlayerStart)
                foreignHero.Health.TakeDamage(new DamageResult(null, foreignHero, 99999f, 99999f, false, false, DamageType.Skill));
                bool battleStillInProgressAfterForeignDeath = (bm.CurrentBattleState == BattleState.InProgress && bm.IsBattleActive);

                // 4. Ally death does NOT trigger player defeat
                ally.Health.TakeDamage(new DamageResult(null, ally, 99999f, 99999f, false, false, DamageType.Skill));
                bool battleStillInProgressAfterAllyDeath = (bm.CurrentBattleState == BattleState.InProgress && bm.IsBattleActive);

                // 5. Main hero death DOES trigger player defeat
                hero.Health.TakeDamage(new DamageResult(null, hero, 99999f, 99999f, false, false, DamageType.Skill));
                bool defeatTriggeredByMainHero = (bm.CurrentBattleState == BattleState.AwaitingPlayerStart && !bm.IsBattleActive);

                pass = heroBelongs && allyBelongs && foreignDoesNotBelong &&
                       foreignCannotTick &&
                       battleStillInProgressAfterForeignDeath && battleStillInProgressAfterAllyDeath &&
                       defeatTriggeredByMainHero;

                Debug.Log($"[P08 T46] Encounter Ownership & Scoping: HeroBelongs={heroBelongs}, AllyBelongs={allyBelongs}, ForeignExcluded={foreignDoesNotBelong}, ForeignCannotTick={foreignCannotTick}, ForeignDeathSafe={battleStillInProgressAfterForeignDeath}, AllyDeathSafe={battleStillInProgressAfterAllyDeath}, MainHeroDefeat={defeatTriggeredByMainHero} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (allyGO != null) UnityEngine.Object.DestroyImmediate(allyGO);
                if (foreignGO != null) UnityEngine.Object.DestroyImmediate(foreignGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
            }
            return pass;
        }

        public static bool T43_CompanionCasterTrackedDuringFinishingExecution()
        {
            var (heroGO, hero) = CreateMockHero("Hero_T43", new Vector3(-2.2f, -0.3f, 0f));
            var (compGO, comp) = CreateMockHero("Companion_T43", new Vector3(-3.0f, -0.3f, 0f));
            var (bmGO, bm) = CreateMockBattleManager();
            var (mmGO, mm) = CreateMockMindMethodManager();

            bool pass = false;
            try
            {
                bm.RegisterHero(hero);
                bm.PrepareAndStartNormalWave(4);

                // Set companion entity type to Companion
                var typeF = typeof(Entity).GetField("entityType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (typeF != null) typeF.SetValue(comp, EntityType.Companion);

                var effect = CreateConfiguredEffect(SkillTargetPolicy.Area, 10f, 0);
                var skill = CreateConfiguredSkill("skill_t43_comp", effect, rageCost: 20f, cooldown: 5f, isChannel: true, channelDuration: 1.5f, channelTickInterval: 0.5f);
                RegisterTestSkillToMindMethod(mm, skill, SkillSlotType.Skill);

                comp.Rage.ResetRage(100f);
                var req = new SkillExecutionRequest(comp, skill, SkillSlotType.Skill, bm.ActiveMonsters[0]);
                var startResult = SkillExecutor.Execute(req);

                // Kill all monsters
                for (int i = 0; i < bm.ActiveMonsters.Count; i++)
                {
                    bm.ActiveMonsters[i].Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));
                }

                // Companion should be tracked as finishing caster
                bool compTracked = bm.CanEntityTickDuringTransition(comp);

                pass = startResult.Success && compTracked;
                Debug.Log($"[P08 T43] Companion Finishing Execution Tracking: Started={startResult.Success}, Tracked={compTracked} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                if (heroGO != null) UnityEngine.Object.DestroyImmediate(heroGO);
                if (compGO != null) UnityEngine.Object.DestroyImmediate(compGO);
                if (bmGO != null) UnityEngine.Object.DestroyImmediate(bmGO);
                if (mmGO != null) UnityEngine.Object.DestroyImmediate(mmGO);
                MindMethodManager.ResetInstance();
            }
            return pass;
        }
        #endregion

        #region Real Play Mode Runner
        private static void CreateHarnessIfMissing()
        {
            if (UnityEngine.Object.FindAnyObjectByType<P08PlayModeHarness>() != null) return;
            var go = new GameObject("P08PlayModeHarness");
            go.hideFlags = HideFlags.DontSave;
            go.AddComponent<P08PlayModeHarness>();
        }

        [MenuItem("Tools/Wuxia RPG/P08/Run P08 Real Play Mode Scenario")]
        public static void RunP08PlayModeScenarioFromMenu()
        {
            Debug.Log("[RUNNER P08] Initializing Real Play Mode Runtime Scenario...");
            if (EditorApplication.isPlaying)
            {
                CreateHarnessIfMissing();
            }
            else
            {
                SessionState.SetBool("RunP08PlayModeScenario", true);
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("Tools/Wuxia RPG/P08/Setup Manual Direct Verification Observer")]
        public static void SetupManualDirectVerificationObserver()
        {
            Debug.Log("[MANUAL OBSERVER P08] Setting up Manual Direct Verification Observer...");
            var existing = UnityEngine.Object.FindAnyObjectByType<P08ManualDirectVerificationObserver>();
            if (existing != null)
            {
                Debug.Log("[MANUAL OBSERVER P08] Observer already active in scene.");
                return;
            }
            var go = new GameObject("P08ManualDirectVerificationObserver");
            go.AddComponent<P08ManualDirectVerificationObserver>();
            Debug.Log("[MANUAL OBSERVER P08] Observer setup complete. Enter Play Mode to observe real-time channel & loot lifecycle verification. Full events logged to manual_direct_test.log.");
        }

        [InitializeOnLoadMethod]
        private static void RegisterPlayModeWatcher()
        {
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("RunP08PlayModeScenario", false))
                {
                    SessionState.SetBool("RunP08PlayModeScenario", false);
                    CreateHarnessIfMissing();
                }
            };

            EditorApplication.update += () =>
            {
                if (EditorApplication.isPlaying && SessionState.GetBool("RunP08PlayModeScenario", false))
                {
                    SessionState.SetBool("RunP08PlayModeScenario", false);
                    CreateHarnessIfMissing();
                }
            };
        }
        #endregion
    }

    public class P08PlayModeHarness : MonoBehaviour
    {
        private int unexpectedErrors = 0;
        private readonly List<string> errorLogs = new List<string>();
        private float scenarioStartTime;
        private bool hasFinished = false;
        private string currentPhase = "00_INITIALIZING";

        // Snapshot state for fixture isolation and restoration
        private bool _dropSysPresent = false;
        private bool _mmPresent = false;
        private bool _rmPresent = false;
        private bool _pmPresent = false;
        private bool _eqPresent = false;
        private bool _invPresent = false;

        private float _origNormalDropRate = 100f;
        private float _origEquipDropRate = 100f;
        private string _origActiveMmId = null;
        private readonly Dictionary<SkillSlotType, string> _origSelectedSkills = new Dictionary<SkillSlotType, string>();
        private List<SkillDefinitionSO> _mindMethodSkillsList = null;
        private SkillDefinitionSO _tempAreaSkill = null;
        private DamageEffectDefinitionSO _tempAoeEffect = null;
        private SkillDefinitionSO _tempChannelSkill = null;
        private DamageEffectDefinitionSO _tempChannelEffect = null;
        private SkillDefinitionSO _tempChannelSkillB = null;
        private DamageEffectDefinitionSO _tempChannelEffectB = null;
        private SkillDefinitionSO _tempSkillCasterA = null;
        private DamageEffectDefinitionSO _tempEffectCasterA = null;
        private SkillDefinitionSO _tempSkillCasterB = null;
        private DamageEffectDefinitionSO _tempEffectCasterB = null;
        private SkillDefinitionSO _tempSingleSkill = null;
        private DamageEffectDefinitionSO _tempSingleEffect = null;

        private float _origRage = 100f;
        private long _origGold = 0;
        private long _origMaterial = 0;
        private int _origLevel = 1;
        private float _origExp = 0f;
        private int _origTitleIndex = 0;
        private readonly Dictionary<EquipmentSlotType, EquipmentInstance> _origEquipped = new Dictionary<EquipmentSlotType, EquipmentInstance>();
        private readonly List<EquipmentInstance> _origInventory = new List<EquipmentInstance>();
        private int _origInventoryCount = 0;
        private int _origEncounterIndex = 1;
        private bool _hasSnapshotted = false;
        private bool _restorationExecuted = false;
        private bool _cleanupResult = false;

        private void Awake()
        {
            Application.logMessageReceived += HandleLogMessage;
            scenarioStartTime = Time.realtimeSinceStartup;
            Debug.Log("[PLAY MODE P08] CHECKPOINT: SCENARIO_STARTED");
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLogMessage;
            if (!_restorationExecuted)
            {
                PerformRestoration();
            }
        }

        private void HandleLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                unexpectedErrors++;
                errorLogs.Add($"[{type}] {condition}\n{stackTrace}");
                Debug.LogWarning($"[PLAY MODE P08] Tracked unexpected log ({type}): {condition}");
            }
        }

        private void Update()
        {
            if (hasFinished) return;
            float elapsed = Time.realtimeSinceStartup - scenarioStartTime;
            if (elapsed >= 180f)
            {
                hasFinished = true;
                Debug.LogError($"[PLAY MODE P08 WATCHDOG TIMEOUT] Scenario exceeded 180s realtime limit! Elapsed={elapsed:F1}s, Phase={currentPhase}");
                DumpDiagnostics(true);
                PerformRestoration();
                if (Application.isBatchMode)
                {
                    EditorApplication.isPlaying = false;
                    EditorApplication.Exit(1);
                }
            }
        }

        private void DumpDiagnostics(bool timedOut)
        {
            var bm = BattleManager.Instance;
            var hero = bm != null ? bm.CurrentHero : null;
            var coordinator = ModalCoordinator.Instance;
            Debug.LogError("================================================================================");
            Debug.LogError("   [PLAY MODE P08 DIAGNOSTIC DUMP]                                              ");
            Debug.LogError($"   RunPhase: {currentPhase}");
            Debug.LogError($"   ElapsedRealtime: {Time.realtimeSinceStartup - scenarioStartTime:F2}s");
            Debug.LogError($"   BattleActive: {(bm != null ? bm.IsBattleActive.ToString() : "N/A")}, BattleState: {(bm != null ? bm.CurrentBattleState.ToString() : "N/A")}, EncounterIndex: {(bm != null ? bm.EncounterIndex.ToString() : "N/A")}");
            Debug.LogError($"   ActiveMonsters: {(bm != null ? bm.ActiveMonsters.Count.ToString() : "N/A")}, LivingMonsters: {(bm != null ? bm.HasLivingMonster().ToString() : "N/A")}");
            Debug.LogError($"   HeroTarget: {(hero != null && hero.CurrentTarget != null ? hero.CurrentTarget.EntityName : "None")}, HeroCastPhase: {(hero != null && hero.CastState != null ? hero.CastState.CurrentPhase.ToString() : "N/A")}");
            Debug.LogError($"   PendingLootItem: {(bm != null && bm.PendingLootItem != null ? bm.PendingLootItem.ItemName : "None")}, LootQueueCount: {(bm != null ? bm.PendingLootQueueCount.ToString() : "N/A")}");
            Debug.LogError($"   ActiveBlockingModals: {(coordinator != null ? coordinator.ActiveBlockingModalCount.ToString() : "N/A")}, TotalQueuedModals: {(coordinator != null ? coordinator.TotalQueuedCount.ToString() : "N/A")}");
            Debug.LogError($"   UnexpectedErrors: {unexpectedErrors}, TimedOut: {timedOut}");
            Debug.LogError("================================================================================");
        }

        private void SnapshotFixtureState()
        {
            if (_hasSnapshotted) return;

            // 1. DropSystem
            var dropSys = DropSystem.Instance != null ? DropSystem.Instance : UnityEngine.Object.FindAnyObjectByType<DropSystem>();
            _dropSysPresent = (dropSys != null);
            if (dropSys != null)
            {
                _origNormalDropRate = dropSys.NormalMonsterDropRate;
                dropSys.NormalMonsterDropRate = 100f;
                if (dropSys.DropConfig != null)
                {
                    _origEquipDropRate = dropSys.DropConfig.EquipmentDropRate;
                    dropSys.DropConfig.EquipmentDropRate = 100f;
                }
            }

            // 2. MindMethod
            var mm = MindMethodManager.Instance;
            _mmPresent = (mm != null);
            if (mm != null)
            {
                _origActiveMmId = mm.ActiveMindMethodId;
                // Preserve exact active ID without overwriting snapshot if originally empty
                if (string.IsNullOrEmpty(mm.ActiveMindMethodId))
                {
                    mm.SetActiveMindMethod("mm_taiji");
                }
                foreach (SkillSlotType slot in Enum.GetValues(typeof(SkillSlotType)))
                {
                    _origSelectedSkills[slot] = mm.GetSelectedSkillIdForSlot(slot);
                }
            }

            // 3. Resources
            var rm = ResourceManager.Instance;
            _rmPresent = (rm != null);
            if (rm != null)
            {
                _origGold = rm.Gold;
                _origMaterial = rm.Material;
            }

            // 4. Progression
            var pm = ProgressionManager.Instance;
            _pmPresent = (pm != null);
            if (pm != null)
            {
                _origLevel = pm.CurrentLevel;
                _origExp = pm.CurrentExp;
                var titleField = typeof(ProgressionManager).GetField("currentTitleIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (titleField != null) _origTitleIndex = (int)titleField.GetValue(pm);
            }

            // 5. Equipment
            var eqMgr = EquipmentManager.Instance;
            _eqPresent = (eqMgr != null);
            if (eqMgr != null)
            {
                foreach (EquipmentSlotType slot in Enum.GetValues(typeof(EquipmentSlotType)))
                {
                    _origEquipped[slot] = eqMgr.GetEquippedItem(slot);
                }
            }

            // 6. Inventory
            var inv = Inventory.Inventory.Instance != null ? Inventory.Inventory.Instance : UnityEngine.Object.FindAnyObjectByType<Inventory.Inventory>();
            _invPresent = (inv != null);
            if (inv != null && inv.Items != null)
            {
                _origInventory.Clear();
                foreach (var itm in inv.Items)
                {
                    if (itm != null) _origInventory.Add(itm);
                }
                _origInventoryCount = _origInventory.Count;
            }

            // 7. Hero Rage
            var hero = BattleManager.Instance != null ? BattleManager.Instance.CurrentHero : UnityEngine.Object.FindAnyObjectByType<Hero>();
            if (hero != null && hero.Rage != null)
            {
                _origRage = hero.Rage.CurrentRage;
            }

            // 8. BattleManager
            var bm = BattleManager.Instance;
            if (bm != null)
            {
                _origEncounterIndex = bm.EncounterIndex;
            }

            _hasSnapshotted = true;
            Debug.Log($"[P08 FIXTURE ISOLATION] Initial state snapshotted successfully: Gold={_origGold}, Mat={_origMaterial}, Lvl={_origLevel}, Rage={_origRage}, ActiveMM='{_origActiveMmId}', InvCount={_origInventoryCount}");
        }

        private bool PerformRestoration()
        {
            if (_restorationExecuted) return _cleanupResult;
            _restorationExecuted = true;
            _cleanupResult = ExecuteRestorationInternal();
            return _cleanupResult;
        }

        private bool ExecuteRestorationInternal()
        {
            try
            {
                // 1. DropSystem
                var dropSys = DropSystem.Instance != null ? DropSystem.Instance : UnityEngine.Object.FindAnyObjectByType<DropSystem>();
                if (dropSys != null)
                {
                    dropSys.NormalMonsterDropRate = _origNormalDropRate;
                    if (dropSys.DropConfig != null)
                    {
                        dropSys.DropConfig.EquipmentDropRate = _origEquipDropRate;
                    }
                }

                // 2. MindMethod
                var mm = MindMethodManager.Instance;
                if (mm != null)
                {
                    var activeDef = mm.ActiveMindMethodDefinition;
                    if (activeDef != null && _mindMethodSkillsList != null)
                    {
                        if (_tempAreaSkill != null) _mindMethodSkillsList.Remove(_tempAreaSkill);
                        if (_tempChannelSkill != null) _mindMethodSkillsList.Remove(_tempChannelSkill);
                        if (_tempChannelSkillB != null) _mindMethodSkillsList.Remove(_tempChannelSkillB);
                        if (_tempSkillCasterA != null) _mindMethodSkillsList.Remove(_tempSkillCasterA);
                        if (_tempSkillCasterB != null) _mindMethodSkillsList.Remove(_tempSkillCasterB);
                        if (_tempSingleSkill != null) _mindMethodSkillsList.Remove(_tempSingleSkill);
                    }
                    var activeState = mm.ActiveMindMethodState;
                    if (activeState != null)
                    {
                        if (_tempAreaSkill != null) activeState.SkillStates.Remove(_tempAreaSkill.SkillId);
                        if (_tempChannelSkill != null) activeState.SkillStates.Remove(_tempChannelSkill.SkillId);
                        if (_tempChannelSkillB != null) activeState.SkillStates.Remove(_tempChannelSkillB.SkillId);
                        if (_tempSkillCasterA != null) activeState.SkillStates.Remove(_tempSkillCasterA.SkillId);
                        if (_tempSkillCasterB != null) activeState.SkillStates.Remove(_tempSkillCasterB.SkillId);
                        if (_tempSingleSkill != null) activeState.SkillStates.Remove(_tempSingleSkill.SkillId);

                        foreach (var kvp in _origSelectedSkills)
                        {
                            if (string.IsNullOrEmpty(kvp.Value))
                            {
                                activeState.SelectedSkillPerSlot.Remove(kvp.Key);
                            }
                            else
                            {
                                activeState.SelectedSkillPerSlot[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(_origActiveMmId))
                    {
                        var activeIdField = typeof(MindMethodManager).GetField("activeMindMethodId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (activeIdField != null) activeIdField.SetValue(mm, null);
                    }
                    else
                    {
                        mm.SetActiveMindMethod(_origActiveMmId);
                    }
                    mm.SaveState();
                }

                // 3. Destroy temporary ScriptableObjects
                if (_tempAreaSkill != null) UnityEngine.Object.DestroyImmediate(_tempAreaSkill);
                if (_tempAoeEffect != null) UnityEngine.Object.DestroyImmediate(_tempAoeEffect);
                if (_tempChannelSkill != null) UnityEngine.Object.DestroyImmediate(_tempChannelSkill);
                if (_tempChannelEffect != null) UnityEngine.Object.DestroyImmediate(_tempChannelEffect);
                if (_tempChannelSkillB != null) UnityEngine.Object.DestroyImmediate(_tempChannelSkillB);
                if (_tempChannelEffectB != null) UnityEngine.Object.DestroyImmediate(_tempChannelEffectB);
                if (_tempSkillCasterA != null) UnityEngine.Object.DestroyImmediate(_tempSkillCasterA);
                if (_tempEffectCasterA != null) UnityEngine.Object.DestroyImmediate(_tempEffectCasterA);
                if (_tempSkillCasterB != null) UnityEngine.Object.DestroyImmediate(_tempSkillCasterB);
                if (_tempEffectCasterB != null) UnityEngine.Object.DestroyImmediate(_tempEffectCasterB);
                if (_tempSingleSkill != null) UnityEngine.Object.DestroyImmediate(_tempSingleSkill);
                if (_tempSingleEffect != null) UnityEngine.Object.DestroyImmediate(_tempSingleEffect);

                // 4. Resources (Authoritative SetResources API, zero negative AddGold)
                var rm = ResourceManager.Instance;
                if (rm != null)
                {
                    rm.SetResources((int)_origGold, (int)_origMaterial);
                }

                // 5. Progression (Authoritative restoration and persistence)
                var pm = ProgressionManager.Instance;
                if (pm != null)
                {
                    var lvlField = typeof(ProgressionManager).GetField("currentLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (lvlField != null) lvlField.SetValue(pm, _origLevel);
                    var expField = typeof(ProgressionManager).GetField("currentExp", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (expField != null) expField.SetValue(pm, _origExp);
                    var titleField = typeof(ProgressionManager).GetField("currentTitleIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (titleField != null) titleField.SetValue(pm, _origTitleIndex);
                    pm.SaveState();
                }

                // 6. Equipment restoration
                var eqMgr = EquipmentManager.Instance;
                if (eqMgr != null)
                {
                    foreach (EquipmentSlotType slot in Enum.GetValues(typeof(EquipmentSlotType)))
                    {
                        var cur = eqMgr.GetEquippedItem(slot);
                        var orig = _origEquipped.ContainsKey(slot) ? _origEquipped[slot] : null;
                        if (cur != orig)
                        {
                            if (cur != null) eqMgr.Unequip(slot);
                            if (orig != null) eqMgr.EquipToSlot(slot, orig);
                        }
                    }
                }

                // 7. Inventory restoration
                var inv = Inventory.Inventory.Instance != null ? Inventory.Inventory.Instance : UnityEngine.Object.FindAnyObjectByType<Inventory.Inventory>();
                if (inv != null)
                {
                    var curItems = new List<EquipmentInstance>(inv.Items);
                    foreach (var itm in curItems)
                    {
                        if (!_origInventory.Contains(itm))
                        {
                            inv.RemoveItem(itm);
                        }
                    }
                    foreach (var itm in _origInventory)
                    {
                        if (!inv.HasItem(itm))
                        {
                            inv.AddItem(itm);
                        }
                    }
                }

                // 8. BattleManager and Combat Entities restoration
                var bm = BattleManager.Instance;
                if (bm != null)
                {
                    var cancelMethod = typeof(BattleManager).GetMethod("CancelLootLifecycle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    if (cancelMethod != null) cancelMethod.Invoke(bm, null);
                    bm.ClearActiveMonsters();
                }
                var hero = bm != null ? bm.CurrentHero : UnityEngine.Object.FindAnyObjectByType<Hero>();
                if (hero != null)
                {
                    hero.SetCurrentTarget(null);
                    if (hero.CastState != null) hero.CastState.Reset();
                    if (hero.Rage != null) hero.Rage.ResetRage(_origRage);
                    if (hero.Attack != null) hero.Attack.SetAttackEnabled(true);
                    var decisionCtrl = hero.GetComponent<HeroSkillDecisionController>();
                    if (decisionCtrl != null) decisionCtrl.enabled = true;
                }
                CooldownManager.ResetAllCooldowns();

                // 9. Post-Restoration Verification (Strict: manager present at snapshot MUST NOT be null)
                bool verifyDrop = (!_dropSysPresent || (dropSys != null && Mathf.Approximately(dropSys.NormalMonsterDropRate, _origNormalDropRate) && (dropSys.DropConfig == null || Mathf.Approximately(dropSys.DropConfig.EquipmentDropRate, _origEquipDropRate))));
                bool verifyRes = (!_rmPresent || (rm != null && rm.Gold == (int)_origGold && rm.Material == (int)_origMaterial));
                bool verifyProg = (!_pmPresent || (pm != null && pm.CurrentLevel == _origLevel && Mathf.Approximately(pm.CurrentExp, _origExp)));

                bool verifyEq = true;
                if (_eqPresent)
                {
                    if (eqMgr == null)
                    {
                        verifyEq = false;
                    }
                    else
                    {
                        foreach (EquipmentSlotType slot in Enum.GetValues(typeof(EquipmentSlotType)))
                        {
                            var cur = eqMgr.GetEquippedItem(slot);
                            var exp = _origEquipped.ContainsKey(slot) ? _origEquipped[slot] : null;
                            if (cur != exp) { verifyEq = false; break; }
                        }
                    }
                }

                bool verifyInv = true;
                if (_invPresent)
                {
                    if (inv == null || inv.Items == null || inv.Items.Count != _origInventoryCount)
                    {
                        verifyInv = false;
                    }
                    else
                    {
                        foreach (var itm in _origInventory)
                        {
                            if (!inv.HasItem(itm)) { verifyInv = false; break; }
                        }
                    }
                }

                bool verifyMm = true;
                if (_mmPresent)
                {
                    if (mm == null)
                    {
                        verifyMm = false;
                    }
                    else
                    {
                        string curActive = mm.ActiveMindMethodId;
                        bool activeMatch = string.IsNullOrEmpty(_origActiveMmId) ? string.IsNullOrEmpty(curActive) : (curActive == _origActiveMmId);
                        bool noTempInDef = (_mindMethodSkillsList == null || (!_mindMethodSkillsList.Contains(_tempAreaSkill) && !_mindMethodSkillsList.Contains(_tempChannelSkill) && !_mindMethodSkillsList.Contains(_tempChannelSkillB) && !_mindMethodSkillsList.Contains(_tempSkillCasterA) && !_mindMethodSkillsList.Contains(_tempSkillCasterB) && !_mindMethodSkillsList.Contains(_tempSingleSkill)));
                        bool noTempInState = true;
                        if (mm.ActiveMindMethodState != null)
                        {
                            var ss = mm.ActiveMindMethodState.SkillStates;
                            if (_tempAreaSkill != null && ss.ContainsKey(_tempAreaSkill.SkillId)) noTempInState = false;
                            if (_tempChannelSkill != null && ss.ContainsKey(_tempChannelSkill.SkillId)) noTempInState = false;
                            if (_tempChannelSkillB != null && ss.ContainsKey(_tempChannelSkillB.SkillId)) noTempInState = false;
                            if (_tempSkillCasterA != null && ss.ContainsKey(_tempSkillCasterA.SkillId)) noTempInState = false;
                            if (_tempSkillCasterB != null && ss.ContainsKey(_tempSkillCasterB.SkillId)) noTempInState = false;
                            if (_tempSingleSkill != null && ss.ContainsKey(_tempSingleSkill.SkillId)) noTempInState = false;
                        }
                        verifyMm = activeMatch && noTempInDef && noTempInState;
                    }
                }

                bool verifyRage = (hero == null || hero.Rage == null || Mathf.Approximately(hero.Rage.CurrentRage, _origRage));

                bool persistentDataRestored = verifyRes && verifyProg && verifyMm;
                bool runtimeCleanupSucceeded = verifyDrop && verifyEq && verifyInv && verifyRage && persistentDataRestored;

                if (runtimeCleanupSucceeded)
                {
                    Debug.Log($"[P08 FIXTURE RESTORATION] Cleanup succeeded: All state restored to initial snapshot. RuntimeCleanupSucceeded={runtimeCleanupSucceeded}, PersistentDataRestored={persistentDataRestored}");
                    return true;
                }
                else
                {
                    Debug.LogError($"[P08 FIXTURE RESTORATION] Cleanup verification failed! RuntimeCleanupSucceeded={runtimeCleanupSucceeded}, PersistentDataRestored={persistentDataRestored}, verifyDrop={verifyDrop}, verifyRes={verifyRes}, verifyProg={verifyProg}, verifyEq={verifyEq}, verifyInv={verifyInv}, verifyMm={verifyMm}, verifyRage={verifyRage}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[P08 FIXTURE RESTORATION] Exception during cleanup: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private void Start()
        {
            StartCoroutine(RunScenarioCoroutine());
        }

        private IEnumerator RunScenarioCoroutine()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING REAL PLAY MODE SCENARIO SUITE: P08 FINAL EVIDENCE                   ");
            Debug.Log($"   Application.isPlaying={Application.isPlaying}, Time.timeScale={Time.timeScale}");
            Debug.Log("================================================================================");

            currentPhase = "01_FIXTURE_SETUP";
            yield return new WaitForSeconds(0.2f);

            SnapshotFixtureState();

            var bm = BattleManager.Instance;
            if (bm == null)
            {
                Debug.LogError("[PLAY MODE P08] FAIL: BattleManager.Instance is null!");
                DumpDiagnostics(false);
                PerformRestoration();
                hasFinished = true;
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            // ====================================================================
            // SCENARIO 1: REAL AOE COMBAT & SEQUENTIAL TWO-LOOT RESOLUTION
            // ====================================================================
            currentPhase = "SCENARIO_1_AOE_LOOT_START";
            Debug.Log("[PLAY MODE P08] >>> Starting Scenario 1: Real AOE Combat & Sequential Loot Resolution <<<");

            // 1. Wait for battle active and 4-5 monsters registered
            if (!bm.IsBattleActive && bm.ActiveMonsters.Count == 0)
            {
                bm.StartBattle();
            }
            while ((bm.ActiveMonsters.Count < 4 || bm.CurrentHero == null || !bm.IsBattleActive) && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            if (bm.ActiveMonsters.Count < 4 || bm.ActiveMonsters.Count > 5 || bm.CurrentHero == null || !bm.IsBattleActive)
            {
                Debug.LogError($"[PLAY MODE P08] FAIL: Encounter failed to start with 4-5 registered monsters! Count={bm.ActiveMonsters.Count}");
                DumpDiagnostics(false);
                PerformRestoration();
                hasFinished = true;
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            var hero = bm.CurrentHero;
            bool orderOk = (bm.ActiveMonsters.Count == 4 || bm.ActiveMonsters.Count == 5);
            bool allAlive = true;
            bool distinctYMinus03 = true;
            for (int i = 0; i < bm.ActiveMonsters.Count; i++)
            {
                var mon = bm.ActiveMonsters[i];
                if (mon == null || !mon.IsAlive) allAlive = false;
                if (mon != null && !Mathf.Approximately(mon.transform.position.y, -0.3f)) distinctYMinus03 = false;
            }
            var m1 = bm.ActiveMonsters[0];
            var m2 = bm.ActiveMonsters[1];
            Debug.Log($"[PLAY MODE P08] Canonical wave monsters registered: Count={bm.ActiveMonsters.Count} (orderOk={orderOk}), AllAlive={allAlive}, YMinus03={distinctYMinus03}");

            // Take Screenshot 1: 01_TWO_MONSTERS_ALIVE.png
            CaptureScreenshot("01_TWO_MONSTERS_ALIVE.png");

            // 2. Configure Area skill into MindMethod
            currentPhase = "SCENARIO_1_CONFIGURE_AREA_SKILL";
            var mm = MindMethodManager.Instance;
            if (mm != null)
            {
                _tempAoeEffect = Prototype01PlayTestRunner_P08.CreateConfiguredEffect(SkillTargetPolicy.Area, 20.0f, 0);
                var soAoe = new SerializedObject(_tempAoeEffect);
                soAoe.Update();
                soAoe.FindProperty("damageMultiplier").floatValue = 2.5f;
                soAoe.ApplyModifiedPropertiesWithoutUndo();

                _tempAreaSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                _tempAreaSkill.InitializeSkill(
                    id: "skill_p08_playmode_aoe",
                    mmId: mm.ActiveMindMethodId,
                    slot: SkillSlotType.Skill,
                    name: "Thiên Cương Quần Long",
                    desc: "AOE Skill",
                    conditions: null,
                    dmgMultiplier: 2.5f,
                    costRage: 20f,
                    cd: 2f,
                    passive: false,
                    skillEffects: new List<SkillEffectDefinitionSO> { _tempAoeEffect }
                );

                var activeDef = mm.ActiveMindMethodDefinition;
                if (activeDef != null)
                {
                    var skillsField = typeof(MindMethodDefinitionSO).GetField("skills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    _mindMethodSkillsList = skillsField != null ? skillsField.GetValue(activeDef) as List<SkillDefinitionSO> : null;
                    if (_mindMethodSkillsList != null && !_mindMethodSkillsList.Contains(_tempAreaSkill))
                    {
                        _mindMethodSkillsList.Add(_tempAreaSkill);
                    }
                }

                var aoeActiveState = mm.ActiveMindMethodState;
                if (aoeActiveState != null)
                {
                    aoeActiveState.SkillStates[_tempAreaSkill.SkillId] = new SkillRuntimeState(_tempAreaSkill.SkillId, true, 1);
                    aoeActiveState.SelectedSkillPerSlot[SkillSlotType.Skill] = _tempAreaSkill.SkillId;
                }
            }

            if (hero.Rage != null) hero.Rage.ResetRage(100f);
            CooldownManager.ResetAllCooldowns();

            // 3. Execute Area Skill via Hero.ExecuteSelectedSkill
            currentPhase = "SCENARIO_1_EXECUTE_AOE_SKILL";
            float m1HpBefore = m1.Health.CurrentHealth;
            float m2HpBefore = m2.Health.CurrentHealth;
            int initialEncounterIndex = bm.EncounterIndex;

            var skillResult = hero.ExecuteSelectedSkill(SkillSlotType.Skill, m1);
            bool aoeHitBoth = (skillResult.Success && m1.Health.CurrentHealth < m1HpBefore && m2.Health.CurrentHealth < m2HpBefore);
            Debug.Log($"[PLAY MODE P08] Area skill executed via Hero.ExecuteSelectedSkill: Success={skillResult.Success}, M1 HP: {m1HpBefore}->{m1.Health.CurrentHealth}, M2 HP: {m2HpBefore}->{m2.Health.CurrentHealth}, BothHit={aoeHitBoth}");

            // Take Screenshot 2: 02_AOE_HITS_BOTH.png
            CaptureScreenshot("02_AOE_HITS_BOTH.png");

            // 4. Natural Auto-Combat to First Death (ZERO forced calls)
            currentPhase = "SCENARIO_1_COMBAT_FIRST_DEATH";
            Debug.Log("[PLAY MODE P08] Natural auto-combat progressing to primary monster defeat...");
            while (m1 != null && m1.IsAlive && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.3f);

            bool m1Dead = (m1 == null || !m1.IsAlive);
            var nextLiving = bm.GetNextLivingMonster();
            bool firstDeathRetargetOk = (m1Dead && nextLiving != null && bm.CurrentMonster == nextLiving && hero.CurrentTarget == nextLiving &&
                                         bm.IsBattleActive && bm.CurrentBattleState == BattleState.InProgress);
            Debug.Log($"[PLAY MODE P08] First death natural retargeting: M1Dead={m1Dead}, CurrentMonster={bm.CurrentMonster?.EntityName}, HeroTarget={hero.CurrentTarget?.EntityName}, BattleActive={bm.IsBattleActive} | {(firstDeathRetargetOk ? "PASS" : "FAIL")}");

            // Take Screenshot 3: 03_RETARGET_AFTER_FIRST_DEATH.png
            CaptureScreenshot("03_RETARGET_AFTER_FIRST_DEATH.png");

            // 5. Natural Auto-Combat until FULL wave defeat
            currentPhase = "SCENARIO_1_COMBAT_FULL_WAVE_DEFEAT";
            Debug.Log("[PLAY MODE P08] Natural auto-combat progressing to full wave defeat...");
            while (bm.HasLivingMonster() && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            // 6. Sequential Loot UI Interaction - First Modal
            currentPhase = "SCENARIO_1_LOOT_MODAL_1";
            Debug.Log("[PLAY MODE P08] Awaiting first loot modal...");
            while ((ModalCoordinator.Instance == null || ModalCoordinator.Instance.ActiveBlockingModalCount == 0 || LootDecisionUI.Instance == null || !LootDecisionUI.Instance.IsVisible) && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            var lootUI = LootDecisionUI.Instance;
            var firstItem = bm.PendingLootItem;
            string firstItemName = firstItem != null ? firstItem.ItemName : string.Empty;
            string firstItemId = firstItem != null ? firstItem.InstanceId : string.Empty;

            // Section 4 strict pre-invocation assertion:
            bool modal1CorrectActive = (ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveBlockingModalCount > 0 && lootUI != null && lootUI.IsVisible);
            bool modal1ItemDisplayed = (bm.PendingLootItem != null && !string.IsNullOrEmpty(firstItemName));
            bool modal1BtnValid = (lootUI != null && lootUI.EquipButton != null && lootUI.EquipButton.gameObject.activeInHierarchy && lootUI.EquipButton.interactable);
            bool modal1PreAssertOk = modal1CorrectActive && modal1ItemDisplayed && modal1BtnValid;
            Debug.Log($"[PLAY MODE P08] Modal 1 Pre-invocation Assert: ModalActive={modal1CorrectActive}, ItemDisplayed={modal1ItemDisplayed} ({firstItemName}), ButtonInteractable={modal1BtnValid} | {(modal1PreAssertOk ? "PASS" : "FAIL")}");

            // Take Screenshot 4: 04_LOOT_FIRST.png
            CaptureScreenshot("04_LOOT_FIRST.png");

            // Hold modal 1 open for 16 seconds (exceeding legacy 15s timeout) to prove no timeout abort occurs
            Debug.Log("[PLAY MODE P08] Holding modal 1 open for 16 seconds (exceeding legacy 15s timeout)...");
            float modal1HoldStart = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - modal1HoldStart < 16.0f)
            {
                yield return null;
            }
            bool modal1HeldPast15s = (Time.realtimeSinceStartup - modal1HoldStart >= 15.5f);
            bool modal1StillActiveAfter16s = (ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveBlockingModalCount > 0 &&
                                             lootUI != null && lootUI.IsVisible && bm.CurrentBattleState == BattleState.LootPending &&
                                             bm.PendingLootItem == firstItem);
            Debug.Log($"[PLAY MODE P08] Modal 1 held past 15s: Duration={(Time.realtimeSinceStartup - modal1HoldStart):F1}s, HeldPast15s={modal1HeldPast15s}, StillActiveAndValid={modal1StillActiveAfter16s}");

            // Click Equip on modal 1
            Debug.Log("[PLAY MODE P08] Invoking Equip button click on modal 1...");
            lootUI.EquipButton.onClick.Invoke();

            // Wait for modal 1 to close
            while (ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveBlockingModalCount > 0 && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            // Immediately request real blocking modal (TitleBreakthrough) during loot deferral waiting window
            Debug.Log("[PLAY MODE P08] Requesting real blocking modal (TitleBreakthrough) during loot deferral waiting window...");
            var blockingReq = new ModalRequest("TitleBreakthrough", ModalPriority.SystemProgression, isDismissable: true, payload: null);
            ModalCoordinator.Instance.RequestModal(blockingReq);

            // Hold blocking modal for 16 seconds (exceeding legacy 15s timeout) while DeferNextLootDecisionRequest yields
            float deferralHoldStart = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - deferralHoldStart < 16.0f)
            {
                yield return null;
            }
            bool deferralHeldPast15s = (Time.realtimeSinceStartup - deferralHoldStart >= 15.5f);
            bool blockingModalActive = (ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveBlockingModalCount > 0);
            bool secondLootNotPrematurelyShown = (lootUI == null || !lootUI.IsVisible || bm.PendingLootItem == null);
            bool encounterNotPrematurelyAdvanced = (bm.EncounterIndex == initialEncounterIndex);
            Debug.Log($"[PLAY MODE P08] Blocking modal held during deferral: Duration={(Time.realtimeSinceStartup - deferralHoldStart):F1}s, BlockingActive={blockingModalActive}, SecondLootHeld={secondLootNotPrematurelyShown}, EncounterNotAdvanced={encounterNotPrematurelyAdvanced}");

            // Dismiss the blocking modal to allow DeferNextLootDecisionRequest to proceed to modal 2
            Debug.Log("[PLAY MODE P08] Dismissing blocking modal to allow deferred second loot presentation...");
            ModalCoordinator.Instance.DismissActiveModal(DismissalReason.UserClosed);

            // 7. Sequential Loot UI Interaction - Second Modal
            currentPhase = "SCENARIO_1_LOOT_MODAL_2";
            Debug.Log("[PLAY MODE P08] Awaiting deferred second loot modal...");
            while ((ModalCoordinator.Instance == null || ModalCoordinator.Instance.ActiveBlockingModalCount == 0 || LootDecisionUI.Instance == null || !LootDecisionUI.Instance.IsVisible) && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            var secondItem = bm.PendingLootItem;
            string secondItemName = secondItem != null ? secondItem.ItemName : string.Empty;
            string secondItemId = secondItem != null ? secondItem.InstanceId : string.Empty;

            // Section 4 strict pre-invocation assertion:
            bool modal2CorrectActive = (ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveBlockingModalCount > 0 && lootUI != null && lootUI.IsVisible);
            bool modal2ItemDisplayed = (bm.PendingLootItem != null && !string.IsNullOrEmpty(secondItemName));
            bool modal2BtnValid = (lootUI != null && lootUI.DismantleButton != null && lootUI.DismantleButton.gameObject.activeInHierarchy && lootUI.DismantleButton.interactable);
            bool modal2PreAssertOk = modal2CorrectActive && modal2ItemDisplayed && modal2BtnValid;
            Debug.Log($"[PLAY MODE P08] Modal 2 Pre-invocation Assert: ModalActive={modal2CorrectActive}, ItemDisplayed={modal2ItemDisplayed} ({secondItemName}), ButtonInteractable={modal2BtnValid} | {(modal2PreAssertOk ? "PASS" : "FAIL")}");

            // Take Screenshot 5: 05_LOOT_SECOND.png
            CaptureScreenshot("05_LOOT_SECOND.png");

            // Click Dismantle on modal 2
            Debug.Log("[PLAY MODE P08] Invoking Dismantle button click on modal 2...");
            lootUI.DismantleButton.onClick.Invoke();

            // Resolve any remaining queued drops to let encounter advance naturally
            while (bm.PendingLootItem != null || bm.PendingLootQueueCount > 0)
            {
                yield return new WaitForSeconds(0.2f);
                if (LootDecisionUI.Instance != null && LootDecisionUI.Instance.IsVisible && LootDecisionUI.Instance.DismantleButton != null && LootDecisionUI.Instance.DismantleButton.interactable)
                {
                    LootDecisionUI.Instance.DismantleButton.onClick.Invoke();
                }
            }

            // 8. Wait for encounter advance to Encounter 2
            currentPhase = "SCENARIO_1_WAIT_ENCOUNTER_ADVANCE";
            while ((ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveBlockingModalCount > 0 || bm.EncounterIndex <= initialEncounterIndex) && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            // Wait until new wave in Encounter 2 is ready in InProgress
            while ((bm.ActiveMonsters.Count < 4 || bm.CurrentBattleState != BattleState.InProgress) && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            bool enc2CountOk = (bm.ActiveMonsters.Count == 4 || bm.ActiveMonsters.Count == 5);
            bool enc2IndexOk = (bm.EncounterIndex == 2);
            bool enc2CompletedCountOk = (bm.CompletedNormalWaveCount == 1);
            bool enc2StatMultiplierOk = Mathf.Approximately(bm.CurrentWaveStatMultiplier, 1.01f);
            bool enc2BaseHpScaled = (bm.ActiveWaveMonsterConfig != null && Mathf.Approximately(bm.ActiveWaveMonsterConfig.MaxHealth, 505f));

            bool secondDeathAdvanceOk = bm.EncounterIndex > initialEncounterIndex && enc2CountOk && enc2IndexOk && enc2CompletedCountOk && enc2StatMultiplierOk && enc2BaseHpScaled;
            bool distinctItemsResolvedOnce = (firstItem != null && secondItem != null && firstItem != secondItem && (firstItemId != secondItemId || firstItemName != secondItemName) && !string.IsNullOrEmpty(firstItemName) && !string.IsNullOrEmpty(secondItemName));
            bool finalModalClosedBeforeAdvance = (ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveBlockingModalCount == 0 && ModalCoordinator.Instance.TotalQueuedCount == 0);
            bool aoeLootScenarioPass = orderOk && allAlive && distinctYMinus03 && aoeHitBoth && firstDeathRetargetOk && secondDeathAdvanceOk &&
                                       modal1PreAssertOk && modal2PreAssertOk && distinctItemsResolvedOnce &&
                                       finalModalClosedBeforeAdvance && modal1HeldPast15s && modal1StillActiveAfter16s &&
                                       deferralHeldPast15s && blockingModalActive && secondLootNotPrematurelyShown &&
                                       encounterNotPrematurelyAdvanced;

            Debug.Log($"[PLAY MODE P08] Scenario 1 Summary: AdvanceOk={secondDeathAdvanceOk}, Enc2Count={bm.ActiveMonsters.Count} (CountOk={enc2CountOk}), Enc2Idx={bm.EncounterIndex} (IdxOk={enc2IndexOk}), WaveCount={bm.CompletedNormalWaveCount} (WaveOk={enc2CompletedCountOk}), Mult={bm.CurrentWaveStatMultiplier} (MultOk={enc2StatMultiplierOk}), BaseHp={bm.ActiveWaveMonsterConfig?.MaxHealth} (HpOk={enc2BaseHpScaled}), DistinctItems={distinctItemsResolvedOnce} | {(aoeLootScenarioPass ? "PASS" : "FAIL")}");

            // ====================================================================
            // WAVE 2 & 3 PRODUCTION VERIFICATION (NATURAL COMBAT & FINISHING CHANNEL)
            // ====================================================================
            currentPhase = "WAVE_2_NATURAL_COMBAT";
            Debug.Log("[PLAY MODE P08] >>> Starting Wave 2 Production Combat & Finishing Channel <<<");

            // 1. For Wave 2 -> Wave 3, disable drop rates to prove the NO-LOOT transition path!
            var dropSys = DropSystem.Instance != null ? DropSystem.Instance : UnityEngine.Object.FindAnyObjectByType<DropSystem>();
            if (dropSys != null)
            {
                dropSys.NormalMonsterDropRate = 0f;
                if (dropSys.DropConfig != null) dropSys.DropConfig.EquipmentDropRate = 0f;
            }
            Debug.Log("[PLAY MODE P08] Wave 2 drops set to 0% to verify no-loot transition path.");

            // 2. Configure Single-Target Combat Skill & Finishing Channel Skill for Wave 2
            _tempSingleEffect = Prototype01PlayTestRunner_P08.CreateConfiguredEffect(SkillTargetPolicy.SingleTarget, 0f, 1);
            var soSingleEff = new SerializedObject(_tempSingleEffect);
            soSingleEff.Update();
            soSingleEff.FindProperty("damageMultiplier").floatValue = 3.0f;
            soSingleEff.ApplyModifiedPropertiesWithoutUndo();

            _tempSingleSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            _tempSingleSkill.InitializeSkill(
                id: "skill_p08_playmode_single",
                mmId: mm != null ? mm.ActiveMindMethodId : "mm_taiji",
                slot: SkillSlotType.Skill,
                name: "Thái Cực Đơn Đả",
                desc: "Single Target Combat Skill",
                conditions: null,
                dmgMultiplier: 3.0f,
                costRage: 15f,
                cd: 1.0f,
                passive: false,
                skillEffects: new List<SkillEffectDefinitionSO> { _tempSingleEffect }
            );

            _tempChannelEffect = Prototype01PlayTestRunner_P08.CreateConfiguredEffect(SkillTargetPolicy.Area, 10.0f, 0);
            var soChanEff = new SerializedObject(_tempChannelEffect);
            soChanEff.Update();
            soChanEff.FindProperty("damageMultiplier").floatValue = 10.0f;
            soChanEff.ApplyModifiedPropertiesWithoutUndo();

            _tempChannelSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            _tempChannelSkill.InitializeSkill(
                id: "skill_p08_playmode_chan",
                mmId: mm != null ? mm.ActiveMindMethodId : "mm_taiji",
                slot: SkillSlotType.Skill,
                name: "Thiên Lôi Tụ Khí (Channel)",
                desc: "Channel Skill",
                conditions: null,
                dmgMultiplier: 10.0f,
                costRage: 20f,
                cd: 4f,
                passive: false,
                skillEffects: new List<SkillEffectDefinitionSO> { _tempChannelEffect }
            );

            var soChan = new SerializedObject(_tempChannelSkill);
            soChan.Update();
            soChan.FindProperty("isChannel").boolValue = true;
            soChan.FindProperty("channelDuration").floatValue = 1.5f;
            soChan.FindProperty("channelTickInterval").floatValue = 0.5f;
            soChan.FindProperty("castTime").floatValue = 0f;
            soChan.ApplyModifiedPropertiesWithoutUndo();

            if (mm != null)
            {
                var activeDef = mm.ActiveMindMethodDefinition;
                if (activeDef != null && _mindMethodSkillsList != null)
                {
                    if (!_mindMethodSkillsList.Contains(_tempSingleSkill)) _mindMethodSkillsList.Add(_tempSingleSkill);
                    if (!_mindMethodSkillsList.Contains(_tempChannelSkill)) _mindMethodSkillsList.Add(_tempChannelSkill);
                }
                var chanActiveState = mm.ActiveMindMethodState;
                if (chanActiveState != null)
                {
                    chanActiveState.SkillStates[_tempSingleSkill.SkillId] = new SkillRuntimeState(_tempSingleSkill.SkillId, true, 1);
                    chanActiveState.SkillStates[_tempChannelSkill.SkillId] = new SkillRuntimeState(_tempChannelSkill.SkillId, true, 1);
                    chanActiveState.SelectedSkillPerSlot[SkillSlotType.Skill] = _tempSingleSkill.SkillId;
                }
            }

            // 3. Natural auto-combat progresses through Wave 2 until exactly 1 living monster remains
            Debug.Log("[PLAY MODE P08] Wave 2 single-target combat running until 1 final monster remains...");
            while (bm.GetLivingMonsterCount() > 1 && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            var decisionCtrl = hero.GetComponent<HeroSkillDecisionController>();
            if (decisionCtrl != null) decisionCtrl.enabled = false;
            if (hero.Attack != null) hero.Attack.SetAttackEnabled(false);

            while (hero.IsCasting && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            var finalMonster = bm.GetNextLivingMonster();
            bool hasFinalMonster = (finalMonster != null && finalMonster.IsAlive);
            Debug.Log($"[PLAY MODE P08] Wave 2 reached final monster: Name={finalMonster?.EntityName}, LivingCount={bm.GetLivingMonsterCount()}");

            // 4. Execute Channel Skill on the final monster
            currentPhase = "WAVE_2_FINISHING_CHANNEL";
            var finishActiveState = mm != null ? mm.ActiveMindMethodState : null;
            if (finishActiveState != null)
            {
                finishActiveState.SelectedSkillPerSlot[SkillSlotType.Skill] = _tempChannelSkill.SkillId;
            }
            if (hero.Rage != null)
            {
                hero.Rage.ResetRage(100f);
            }
            CooldownManager.ResetAllCooldowns();

            int wave2EncounterIndex = bm.EncounterIndex;
            var chanReq = new SkillExecutionRequest(hero, _tempChannelSkill, SkillSlotType.Skill, finalMonster);
            var chanExecRes = SkillExecutor.Execute(chanReq);

            bool chanStarted = chanExecRes.Success && hero.IsCasting && hero.IsChanneling;
            bool rageConsumedOnceAtStart = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
            bool noCooldownAtStart = !CooldownManager.IsOnCooldown(_tempChannelSkill.SkillId, out _);
            Debug.Log($"[PLAY MODE P08 CHANNEL] Wave 2 Final Monster Channel: Started={chanStarted}, RageOnce={rageConsumedOnceAtStart}, NoCdAtStart={noCooldownAtStart}");

            // 5. Let natural frames advance the channel. When the final monster dies, wave transition begins!
            while (finalMonster != null && finalMonster.IsAlive && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            yield return null; // allow death event to process in BM
            bool finalMonsterDead = (finalMonster == null || !finalMonster.IsAlive);
            bool waveFullyDefeated = bm.IsWaveFullyDefeated();
            bool channelStillActiveDuringTransition = hero.IsCasting && hero.IsChanneling && bm.CanEntityTickDuringTransition(hero);
            bool transitionHoldsNextEncounter = (bm.EncounterIndex == wave2EncounterIndex);
            bool noEarlyCooldownDuringTransition = !CooldownManager.IsOnCooldown(_tempChannelSkill.SkillId, out _);

            Debug.Log($"[PLAY MODE P08 CHANNEL] Mid-Transition: FinalDead={finalMonsterDead}, WaveDefeated={waveFullyDefeated}, ChannelContinuing={channelStillActiveDuringTransition}, NextEncHeld={transitionHoldsNextEncounter}, NoEarlyCd={noEarlyCooldownDuringTransition}");

            // Request modal during finishing channel to ensure transition does NOT abandon transaction
            currentPhase = "WAVE_2_MODAL_RACE_TEST";
            var mockYieldView = new DummyModalView_P08 { ModalId = "P08TransitionYieldModal" };
            ModalCoordinator.Instance.RegisterModalView(mockYieldView);

            var transYieldModalReq = new ModalRequest("P08TransitionYieldModal", ModalPriority.SystemProgression, isDismissable: true, payload: null);
            ModalCoordinator.Instance.RequestModal(transYieldModalReq);

            // 6. Natural channel completion: hero finishes channeling through transition
            while (hero.CastState.IsActive && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            bool channelFinishedNaturally = !hero.IsCasting && !hero.IsChanneling;
            bool cooldownTriggeredOnCompletion = CooldownManager.IsOnCooldown(_tempChannelSkill.SkillId, out float cdRemain) && cdRemain > 0f;
            Debug.Log($"[PLAY MODE P08 CHANNEL] Completion: FinishedNaturally={channelFinishedNaturally}, CooldownTriggered={cooldownTriggeredOnCompletion} ({cdRemain:F1}s)");

            // Modal is still active: verify transition is held and does NOT spawn Wave 3 while modal is active
            yield return new WaitForSeconds(0.4f);
            bool modalActiveInYield = (ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveBlockingModalCount > 0);
            bool wave3HeldWhileModalActive = (bm.EncounterIndex == wave2EncounterIndex);
            Debug.Log($"[PLAY MODE P08] Transition Yield Modal Race: ModalActive={modalActiveInYield}, Wave3Held={wave3HeldWhileModalActive}");

            // Dismiss the blocking modal to allow transition to complete and spawn Wave 3
            ModalCoordinator.Instance.DismissActiveModal(DismissalReason.UserClosed);
            ModalCoordinator.Instance.UnregisterModalView(mockYieldView);

            // 8. Wait for transition to complete and Wave 3 (Encounter 3) to spawn
            currentPhase = "WAVE_3_SPAWN_AND_PROGRESS";
            while ((bm.EncounterIndex < 3 || bm.CurrentBattleState != BattleState.InProgress || bm.ActiveMonsters.Count < 4) && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            if (decisionCtrl != null) decisionCtrl.enabled = true;
            if (hero.Attack != null) hero.Attack.SetAttackEnabled(true);

            // Verify Wave 3 invariants
            bool enc3CountOk = (bm.ActiveMonsters.Count == 4 || bm.ActiveMonsters.Count == 5);
            bool enc3IndexOk = (bm.EncounterIndex == 3);
            bool enc3CompletedCountOk = (bm.CompletedNormalWaveCount == 2);
            bool enc3StatMultiplierOk = Mathf.Abs(bm.CurrentWaveStatMultiplier - 1.0201f) < 0.0001f;
            bool enc3BaseHpScaled = (bm.ActiveWaveMonsterConfig != null && Mathf.Abs(bm.ActiveWaveMonsterConfig.MaxHealth - 510.05f) < 0.01f);
            bool enc3BaseAtkScaled = (bm.ActiveWaveMonsterConfig != null && Mathf.Abs(bm.ActiveWaveMonsterConfig.Attack - 51.005f) < 0.01f);
            bool enc3BaseDefScaled = (bm.ActiveWaveMonsterConfig != null && Mathf.Abs(bm.ActiveWaveMonsterConfig.Defense - 10.201f) < 0.01f);
            bool enc3AllYMinus03 = true;
            for (int i = 0; i < bm.ActiveMonsters.Count; i++)
            {
                var mon = bm.ActiveMonsters[i];
                if (mon != null && !Mathf.Approximately(mon.transform.position.y, -0.3f)) enc3AllYMinus03 = false;
            }

            // Let Wave 3 combat engage naturally for 1 second
            yield return new WaitForSeconds(1.0f);
            bool enc3CombatActive = (bm.IsBattleActive && bm.CurrentBattleState == BattleState.InProgress && bm.HasLivingMonster());

            bool wave2And3Pass = hasFinalMonster && chanStarted && rageConsumedOnceAtStart && noCooldownAtStart &&
                                 finalMonsterDead && waveFullyDefeated && channelStillActiveDuringTransition &&
                                 transitionHoldsNextEncounter && noEarlyCooldownDuringTransition &&
                                 channelFinishedNaturally && cooldownTriggeredOnCompletion &&
                                 modalActiveInYield && wave3HeldWhileModalActive &&
                                 enc3CountOk && enc3IndexOk && enc3CompletedCountOk && enc3StatMultiplierOk &&
                                 enc3BaseHpScaled && enc3BaseAtkScaled && enc3BaseDefScaled && enc3AllYMinus03 &&
                                 enc3CombatActive;

            Debug.Log($"[PLAY MODE P08] Wave 2 & 3 Summary: Wave2ChannelPass={channelFinishedNaturally}, Wave3Count={bm.ActiveMonsters.Count} (CountOk={enc3CountOk}), Wave3Idx={bm.EncounterIndex} (IdxOk={enc3IndexOk}), CompletedWaves={bm.CompletedNormalWaveCount} (CompletedOk={enc3CompletedCountOk}), Mult={bm.CurrentWaveStatMultiplier} (MultOk={enc3StatMultiplierOk}), BaseHp={bm.ActiveWaveMonsterConfig?.MaxHealth} (HpOk={enc3BaseHpScaled}), CombatActive={enc3CombatActive} | {(wave2And3Pass ? "PASS" : "FAIL")}");

            // ====================================================================
            // IDEMPOTENT RESTORATION & POST-CLEANUP VERIFICATION
            // ====================================================================
            currentPhase = "FINAL_CLEANUP_AND_RESTORE";
            bool cleanupSucceeded = PerformRestoration();

            bool isTimedOut = (Time.realtimeSinceStartup - scenarioStartTime >= 180f);
            bool scenarioPass = aoeLootScenarioPass && wave2And3Pass && cleanupSucceeded && (unexpectedErrors == 0) && !isTimedOut;

            hasFinished = true;
            Debug.Log("================================================================================");
            Debug.Log($"   [REAL PLAY MODE P08 SCENARIO]: ALL_PASS={scenarioPass}, CleanupSucceeded={cleanupSucceeded}, UnexpectedErrors={unexpectedErrors}, TimedOut={isTimedOut}");
            if (unexpectedErrors > 0)
            {
                Debug.LogWarning($"[PLAY MODE P08] Encountered {unexpectedErrors} unexpected errors/exceptions:");
                var errSnapshot = new List<string>(errorLogs);
                for (int i = 0; i < errSnapshot.Count; i++)
                {
                    Debug.LogWarning($"[UNEXPECTED_LOG #{i + 1}] {errSnapshot[i]}");
                }
            }
            Debug.Log("================================================================================");

            if (Application.isBatchMode)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.Exit(scenarioPass ? 0 : 1);
            }
        }

        private static void CaptureScreenshot(string filename)
        {
            Camera cam = Camera.main ?? UnityEngine.Object.FindAnyObjectByType<Camera>();
            Canvas canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();

            RenderMode origMode = RenderMode.ScreenSpaceOverlay;
            Camera origCam = null;
            float origDist = 100f;
            int origSortingOrder = 0;
            bool modifiedCanvas = false;

            if (canvas != null && cam != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                origMode = canvas.renderMode;
                origCam = canvas.worldCamera;
                origDist = canvas.planeDistance;
                origSortingOrder = canvas.sortingOrder;

                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = cam;
                canvas.planeDistance = 5f;
                canvas.sortingOrder = 1000;
                modifiedCanvas = true;
            }

            try
            {
                const int width = 1080;
                const int height = 1920;
                RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
                RenderTexture prevRt = cam != null ? cam.targetTexture : null;
                if (cam != null) cam.targetTexture = rt;

                if (cam != null) cam.Render();

                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

                if (cam != null) cam.targetTexture = prevRt;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);

                byte[] bytes = tex.EncodeToPNG();
                UnityEngine.Object.DestroyImmediate(tex);

                string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                string dir1 = Path.Combine(projectRoot, "screenshots");
                string dir2 = Path.Combine(projectRoot, "review_package_p08_final_acceptance", "screenshots");
                string dir3 = Path.Combine(projectRoot, "review_package_p08_final_closure", "screenshots");
                string dir4 = Path.Combine(projectRoot, "review_package_p08_corrective_v2", "screenshots");
                string dir5 = Path.Combine(projectRoot, "review_package_p08_normal_wave_stat_growth", "screenshots");
                Directory.CreateDirectory(dir1);
                Directory.CreateDirectory(dir2);
                Directory.CreateDirectory(dir3);
                Directory.CreateDirectory(dir4);
                Directory.CreateDirectory(dir5);

                string path1 = Path.Combine(dir1, filename);
                string path2 = Path.Combine(dir2, filename);
                string path3 = Path.Combine(dir3, filename);
                string path4 = Path.Combine(dir4, filename);
                string path5 = Path.Combine(dir5, filename);
                File.WriteAllBytes(path1, bytes);
                File.WriteAllBytes(path2, bytes);
                File.WriteAllBytes(path3, bytes);
                File.WriteAllBytes(path4, bytes);
                File.WriteAllBytes(path5, bytes);

                Debug.Log($"[PLAY MODE P08] Screenshot '{filename}' captured successfully ({bytes.Length} bytes).");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PLAY MODE P08] Screenshot capture failed for '{filename}': {ex.Message}");
            }
            finally
            {
                if (modifiedCanvas && canvas != null)
                {
                    canvas.renderMode = origMode;
                    canvas.worldCamera = origCam;
                    canvas.planeDistance = origDist;
                    canvas.sortingOrder = origSortingOrder;
                }
            }
        }
    }

    /// <summary>
    /// Manual Direct Verification Observer for the project owner to verify channel & loot lifecycle
    /// via real Unity Editor GUI / Console without requiring automated batchmode execution.
    /// </summary>
    public class P08ManualDirectVerificationObserver : MonoBehaviour
    {
        private readonly List<string> _eventsLog = new List<string>();
        private string _logFilePath;

        private void Awake()
        {
            _logFilePath = Path.Combine(Application.dataPath, "..", "manual_direct_test.log");
            LogEvent("P08 Manual Direct Verification Observer Initialized.");
            EventBus.OnEntityDamaged += HandleDamage;
            EventBus.OnBattleStateChanged += HandleStateChanged;
            EventBus.OnSkillExecutionSucceeded += HandleSkillSucceeded;
            EventBus.OnLootDecisionRequested += HandleLootRequested;
        }

        private void OnDestroy()
        {
            EventBus.OnEntityDamaged -= HandleDamage;
            EventBus.OnBattleStateChanged -= HandleStateChanged;
            EventBus.OnSkillExecutionSucceeded -= HandleSkillSucceeded;
            EventBus.OnLootDecisionRequested -= HandleLootRequested;
            LogEvent("P08 Manual Direct Verification Observer Destroyed.");
        }

        private void HandleDamage(Entity victim, DamageResult result)
        {
            LogEvent($"[DAMAGE] Attacker={result.Attacker?.EntityName ?? "null"}, Victim={victim?.EntityName ?? "null"}, FinalDamage={result.FinalDamage:F1}");
        }

        private void HandleStateChanged(BattleState state)
        {
            LogEvent($"[BATTLE_STATE] State changed to: {state}");
        }

        private void HandleSkillSucceeded(SkillExecutionRequest request, SkillExecutionResult result)
        {
            LogEvent($"[SKILL_SUCCESS] Skill={request.Skill?.SkillId ?? "null"}, Source={request.Source?.EntityName ?? "null"}");
        }

        private void HandleLootRequested(EquipmentInstance item)
        {
            LogEvent($"[LOOT_REQUESTED] Item={item?.ItemName ?? "null"} ({item?.InstanceId ?? "null"})");
        }

        private void LogEvent(string msg)
        {
            string line = $"[{DateTime.Now:HH:mm:ss.fff}] {msg}";
            _eventsLog.Add(line);
            try
            {
                File.AppendAllText(_logFilePath, line + Environment.NewLine);
            }
            catch {}
            Debug.Log($"[MANUAL OBSERVER] {msg}");
        }

        private void OnGUI()
        {
            var bm = BattleManager.Instance;
            var hero = bm != null ? bm.CurrentHero : null;

            GUILayout.BeginArea(new Rect(10, 10, 520, 520), "P08 Manual Direct Verification Observer", GUI.skin.window);
            GUILayout.Label($"<b>Wave ID:</b> {(bm != null ? bm.CurrentWaveId.ToString() : "N/A")} | <b>Encounter:</b> {(bm != null ? bm.EncounterIndex.ToString() : "N/A")} | <b>State:</b> {(bm != null ? bm.CurrentBattleState.ToString() : "N/A")}");
            GUILayout.Label($"<b>Monster Counts:</b> Expected={(bm != null ? bm.LastWaveMonsterCount.ToString() : "N/A")} | Registered={(bm != null ? bm.ActiveMonsters.Count.ToString() : "N/A")} | Living={(bm != null ? bm.GetLivingMonsterCount().ToString() : "N/A")}");
            GUILayout.Label($"<b>Wave Tier:</b> Current={(bm != null ? bm.CurrentWaveTier.ToString() : "N/A")} (x{(bm != null ? bm.CurrentWaveStatMultiplier.ToString("F4") : "1.0000")}) | Next={(bm != null ? bm.CompletedNormalWaveCount.ToString() : "N/A")} (x{(bm != null ? bm.NextWaveStatMultiplier.ToString("F4") : "1.0000")})");
            
            var cfg = bm != null ? bm.ActiveWaveMonsterConfig : null;
            GUILayout.Label($"<b>Spawn Base Stats:</b> HP={(cfg != null ? cfg.MaxHealth.ToString("F1") : "500.0")} | ATK={(cfg != null ? cfg.Attack.ToString("F2") : "50.00")} | DEF={(cfg != null ? cfg.Defense.ToString("F2") : "10.00")}");
            
            float rage = hero != null && hero.Rage != null ? hero.Rage.CurrentRage : 0f;
            GUILayout.Label($"<b>Hero State:</b> Rage={rage:F1} | Casting={(hero != null && hero.IsCasting)} | Channeling={(hero != null && hero.IsChanneling)} | TransitionTick={(bm != null && hero != null && bm.CanEntityTickDuringTransition(hero))}");
            if (hero != null && hero.CastState != null && hero.IsCasting)
            {
                GUILayout.Label($"<b>Cast Details:</b> Phase={hero.CastState.CurrentPhase} | Elapsed={hero.CastState.ElapsedChannelTime:F2}s / {hero.CastState.ChannelDuration:F2}s | Ticks={hero.CastState.ChannelTicksExecuted}");
            }
            GUILayout.Label($"<b>Loot State:</b> Pending={(bm != null && bm.PendingLootItem != null ? bm.PendingLootItem.ItemName : "None")} | Queue={(bm != null ? bm.PendingLootQueueCount.ToString() : "0")} | Modals={(ModalCoordinator.Instance != null ? ModalCoordinator.Instance.ActiveBlockingModalCount.ToString() : "0")}");

            GUILayout.Space(8);
            GUILayout.Label("<b>Recent Events</b> (logged to <i>manual_direct_test.log</i>):");
            int start = Mathf.Max(0, _eventsLog.Count - 5);
            for (int i = start; i < _eventsLog.Count; i++)
            {
                GUILayout.Label("<size=10>" + _eventsLog[i] + "</size>");
            }
            GUILayout.EndArea();
        }
    }
}
#endif
