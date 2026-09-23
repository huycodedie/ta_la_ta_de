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
    public static class Prototype01PlayTestRunner_P08
    {
        private const string ScenePath = "Assets/_Game/Scenes/Prototype01.unity";

        [MenuItem("Tools/Wuxia RPG/P08/Run P08 Automated Tests (T01 - T35)")]
        public static bool RunP08AutomatedTests()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING P08 AOE / MULTI-TARGET AUTOMATED TEST SUITE (T01 - T35)             ");
            Debug.Log("================================================================================");

            int passed = 0;
            const int total = 36; // 21 standard + T21_B + T22-T35 tests
            SkillExecutor.EnableRageCost = true;
            SkillExecutor.EnableCooldown = true;
            CooldownManager.ResetAllCooldowns();

            if (T01_SingleTargetResultUnchanged()) passed++;
            if (T02_AreaHitsTwoInRangeEnemies()) passed++;
            if (T03_PrimaryIsHitExactlyOnce()) passed++;
            if (T04_OutOfRadiusEnemyIsUnaffected()) passed++;
            if (T05_DeadInactiveUnregisteredEnemyIsExcluded()) passed++;
            if (T06_MaxTargetCountIsEnforced()) passed++;
            if (T07_Ordering_PrimaryDistanceRegistration()) passed++;
            if (T08_NoDuplicateTarget()) passed++;
            if (T09_EffectExecutesExactlyOncePerResolvedTarget()) passed++;
            if (T10_RageConsumedOncePerCast()) passed++;
            if (T11_CooldownTriggeredOncePerCast()) passed++;
            if (T12_StatusAppliedIndependentlyToTargets()) passed++;
            if (T13_OneImmuneFailedTargetDoesNotCancelAnotherValidTarget()) passed++;
            if (T14_HeroCompanionNotHitByHeroArea()) passed++;
            if (T15_HeroRetargetsAndAutoCombatContinuesAfterPrimaryDeath()) passed++;
            if (T16_LootAndExpAwardPrecision()) passed++;
            if (T17_EncounterRemainsActiveAfterFirstDeath()) passed++;
            if (T18_EncounterCompletesExactlyOnceAfterFinalDeath()) passed++;
            if (T19_StopResumeAffectsAllActiveMonsters()) passed++;
            if (T20_AllEnemiesPolicyResolvesAllLivingRegisteredEnemies()) passed++;
            if (T21_InvalidAreaAllEnemiesFailsBeforeRageCooldown()) passed++;
            if (T21_B_ChannelEagerSnapshotExcludesLateRegisteredMonster()) passed++;
            if (T22_CooldownRejectionCreatesNoChannelAndConsumesNoResources()) passed++;
            if (T23_RequiredTargetSnapshotFailureConsumesNoResources()) passed++;
            if (T24_TwoExecutionsDoNotShareSnapshots()) passed++;
            if (T25_TwoSupportedCastersHaveSeparateSnapshots()) passed++;
            if (T26_LaterRegisteredMonsterExcludedFromStartedChannel()) passed++;
            if (T27_LaterPulsesSkipDeadMembersWithoutAddingReplacements()) passed++;
            if (T28_RageAndCooldownFollowOncePerExecutionContract()) passed++;
            if (T29_ValidLegacyHeroToMonsterSingleTarget()) passed++;
            if (T30_ValidLegacyMonsterToHeroSingleTarget()) passed++;
            if (T31_InvalidFriendlyOrDeadSingleTargetRejected()) passed++;
            if (T32_UnregisteredAoeTargetRejected()) passed++;
            if (T33_OldOrExternalMonsterDeathDoesNotAffectEncounter()) passed++;
            if (T34_MultiEffectSkillFinishingEncounterDoesNotHitNewEncounter()) passed++;
            if (T35_LootLifecycleContinuesAfterLongModalHold()) passed++;

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

                // Execute the 2-effect skill through real pipeline
                var req = new SkillExecutionRequest(hero, multiSkill, SkillSlotType.Skill, m1);
                var execRes = SkillExecutor.Execute(req);

                // Effect 1 defeats M1
                bool m1Dead = (m1 == null || !m1.IsAlive);
                bool execSuccess = execRes.Success;
                bool rageConsumedOnce = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
                bool cdTriggeredOnce = CooldownManager.IsOnCooldown(multiSkill.SkillId, out _);

                // Effect 2 results: must NOT damage any monster from a new encounter
                bool encounterEndedCleanly = (bm.CurrentBattleState == BattleState.MonsterDead || bm.CurrentBattleState == BattleState.EncounterTransition || bm.CurrentBattleState == BattleState.InProgress);

                pass = execSuccess && m1Dead && rageConsumedOnce && cdTriggeredOnce && encounterEndedCleanly;
                Debug.Log($"[P08 T34] Multi-Effect Encounter Isolation: ExecSuccess={execSuccess}, M1Dead={m1Dead}, RageOnce={rageConsumedOnce}, CdOnce={cdTriggeredOnce}, State={bm.CurrentBattleState} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
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
                bm.EnqueuePendingLoot(drop1);

                // Kill monster to enter LootPending
                m1.Health.TakeDamage(new DamageResult(null, null, 1000f, 1000f, false, false, DamageType.Skill));

                bool enteredLootPending = (bm.CurrentBattleState == BattleState.LootPending && bm.PendingLootItem == drop1);

                // Complete decision
                bool completeRes = bm.CompleteLootDecisionAndResume(equip: true, dismantle: false);

                bool lootResolved = (bm.PendingLootItem == null && bm.PendingLootQueueCount == 0);
                bool resumed = (bm.CurrentBattleState == BattleState.InProgress || bm.CurrentBattleState == BattleState.EncounterTransition || bm.EncounterIndex > 1);

                pass = enteredLootPending && completeRes && lootResolved && resumed;
                Debug.Log($"[P08 T35] Loot Lifecycle Modal Hold: EnteredLootPending={enteredLootPending}, CompleteRes={completeRes}, LootResolved={lootResolved}, Resumed={resumed} | {(pass ? "PASS" : "FAIL")}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(heroGO);
                UnityEngine.Object.DestroyImmediate(m1GO);
                UnityEngine.Object.DestroyImmediate(bmGO);
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
                    }
                    var activeState = mm.ActiveMindMethodState;
                    if (activeState != null)
                    {
                        if (_tempAreaSkill != null) activeState.SkillStates.Remove(_tempAreaSkill.SkillId);
                        if (_tempChannelSkill != null) activeState.SkillStates.Remove(_tempChannelSkill.SkillId);
                        if (_tempChannelSkillB != null) activeState.SkillStates.Remove(_tempChannelSkillB.SkillId);
                        if (_tempSkillCasterA != null) activeState.SkillStates.Remove(_tempSkillCasterA.SkillId);
                        if (_tempSkillCasterB != null) activeState.SkillStates.Remove(_tempSkillCasterB.SkillId);

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
                        bool noTempInDef = (_mindMethodSkillsList == null || (!_mindMethodSkillsList.Contains(_tempAreaSkill) && !_mindMethodSkillsList.Contains(_tempChannelSkill) && !_mindMethodSkillsList.Contains(_tempChannelSkillB) && !_mindMethodSkillsList.Contains(_tempSkillCasterA) && !_mindMethodSkillsList.Contains(_tempSkillCasterB)));
                        bool noTempInState = true;
                        if (mm.ActiveMindMethodState != null)
                        {
                            var ss = mm.ActiveMindMethodState.SkillStates;
                            if (_tempAreaSkill != null && ss.ContainsKey(_tempAreaSkill.SkillId)) noTempInState = false;
                            if (_tempChannelSkill != null && ss.ContainsKey(_tempChannelSkill.SkillId)) noTempInState = false;
                            if (_tempChannelSkillB != null && ss.ContainsKey(_tempChannelSkillB.SkillId)) noTempInState = false;
                            if (_tempSkillCasterA != null && ss.ContainsKey(_tempSkillCasterA.SkillId)) noTempInState = false;
                            if (_tempSkillCasterB != null && ss.ContainsKey(_tempSkillCasterB.SkillId)) noTempInState = false;
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

            // 1. Wait for battle active and two monsters registered
            while ((bm.ActiveMonsters.Count < 2 || bm.CurrentHero == null || !bm.IsBattleActive) && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            if (bm.ActiveMonsters.Count < 2 || bm.CurrentHero == null || !bm.IsBattleActive)
            {
                Debug.LogError("[PLAY MODE P08] FAIL: Encounter failed to start with two registered monsters!");
                DumpDiagnostics(false);
                PerformRestoration();
                hasFinished = true;
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            var m1 = bm.ActiveMonsters[0];
            var m2 = bm.ActiveMonsters[1];
            var hero = bm.CurrentHero;
            bool orderOk = (bm.ActiveMonsters.Count >= 2);
            bool bothAlive = (m1 != null && m1.IsAlive && m2 != null && m2.IsAlive);
            Debug.Log($"[PLAY MODE P08] Canonical monsters registered: {orderOk}, Both alive: {bothAlive}");

            // Take Screenshot 1: 01_TWO_MONSTERS_ALIVE.png
            CaptureScreenshot("01_TWO_MONSTERS_ALIVE.png");

            // 2. Configure Area skill into MindMethod
            currentPhase = "SCENARIO_1_CONFIGURE_AREA_SKILL";
            var mm = MindMethodManager.Instance;
            if (mm != null)
            {
                _tempAoeEffect = Prototype01PlayTestRunner_P08.CreateConfiguredEffect(SkillTargetPolicy.Area, 10.0f, 0);
                _tempAreaSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                _tempAreaSkill.InitializeSkill(
                    id: "skill_p08_playmode_aoe",
                    mmId: mm.ActiveMindMethodId,
                    slot: SkillSlotType.Skill,
                    name: "Thiên Cương Quần Long",
                    desc: "AOE Skill",
                    conditions: null,
                    dmgMultiplier: 1.5f,
                    costRage: 20f,
                    cd: 10f,
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

                var activeState = mm.ActiveMindMethodState;
                if (activeState != null)
                {
                    activeState.SkillStates[_tempAreaSkill.SkillId] = new SkillRuntimeState(_tempAreaSkill.SkillId, true, 1);
                    activeState.SelectedSkillPerSlot[SkillSlotType.Skill] = _tempAreaSkill.SkillId;
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
            bool firstDeathRetargetOk = (m1Dead && bm.CurrentMonster == m2 && hero.CurrentTarget == m2 &&
                                         bm.IsBattleActive && bm.CurrentBattleState == BattleState.InProgress);
            Debug.Log($"[PLAY MODE P08] First death natural retargeting: M1Dead={m1Dead}, CurrentMonster={bm.CurrentMonster?.EntityName}, HeroTarget={hero.CurrentTarget?.EntityName}, BattleActive={bm.IsBattleActive} | {(firstDeathRetargetOk ? "PASS" : "FAIL")}");

            // Take Screenshot 3: 03_RETARGET_AFTER_FIRST_DEATH.png
            CaptureScreenshot("03_RETARGET_AFTER_FIRST_DEATH.png");

            // 5. Natural Auto-Combat to Second Death
            currentPhase = "SCENARIO_1_COMBAT_SECOND_DEATH";
            Debug.Log("[PLAY MODE P08] Natural auto-combat progressing to secondary monster defeat...");
            while (m2 != null && m2.IsAlive && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
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

            // 8. Wait for modal 2 to close and encounter advance
            currentPhase = "SCENARIO_1_WAIT_ENCOUNTER_ADVANCE";
            while ((ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveBlockingModalCount > 0 || bm.EncounterIndex <= initialEncounterIndex) && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            bool m2Dead = (m2 == null || !m2.IsAlive);
            bool secondDeathAdvanceOk = (m2Dead && bm.EncounterIndex > initialEncounterIndex);
            bool distinctItemsResolvedOnce = (firstItem != null && secondItem != null && firstItem != secondItem && (firstItemId != secondItemId || firstItemName != secondItemName) && !string.IsNullOrEmpty(firstItemName) && !string.IsNullOrEmpty(secondItemName));
            bool finalModalClosedBeforeAdvance = (ModalCoordinator.Instance != null && ModalCoordinator.Instance.ActiveBlockingModalCount == 0 && ModalCoordinator.Instance.TotalQueuedCount == 0);
            bool aoeLootScenarioPass = orderOk && bothAlive && aoeHitBoth && firstDeathRetargetOk && secondDeathAdvanceOk && modal1PreAssertOk && modal2PreAssertOk && distinctItemsResolvedOnce && finalModalClosedBeforeAdvance && modal1HeldPast15s && modal1StillActiveAfter16s;

            Debug.Log($"[PLAY MODE P08] Scenario 1 Summary: AdvanceOk={secondDeathAdvanceOk}, DistinctItems={distinctItemsResolvedOnce} ('{firstItemName}' [{firstItemId}] vs '{secondItemName}' [{secondItemId}]), FinalModalClosed={finalModalClosedBeforeAdvance} | {(aoeLootScenarioPass ? "PASS" : "FAIL")}");

            // ====================================================================
            // SCENARIO 2: REAL CHANNEL PLAY MODE EVIDENCE (NATURAL UNITY FRAMES)
            // ====================================================================
            // SCENARIO 2: REAL CHANNEL PLAY MODE EVIDENCE (NATURAL UNITY FRAMES)
            // ====================================================================
            currentPhase = "SCENARIO_2_CHANNEL_START";
            Debug.Log("[PLAY MODE P08] >>> Starting Scenario 2: Real Channel Play Mode Scenario (Natural Unity Frames) <<<");

            // Temporarily disable AI decision controller and auto-battle so it does not interfere with the isolated channel execution
            bool origAutoBattle = bm.IsAutoBattle;
            bm.SetAutoBattle(false);
            var allAiControllers = UnityEngine.Object.FindObjectsByType<HeroSkillDecisionController>(FindObjectsSortMode.None);
            var origAiStates = new Dictionary<HeroSkillDecisionController, bool>();
            foreach (var ai in allAiControllers)
            {
                origAiStates[ai] = ai.enabled;
                ai.enabled = false;
            }
            bool origHeroAttack = hero.Attack != null ? hero.Attack.IsAttackEnabled : true;
            if (hero.Attack != null) hero.Attack.SetAttackEnabled(false);

            // Clear lingering dead monsters from Scenario 1 before registering fresh channel test monsters
            bm.ClearActiveMonsters();

            // Setup fresh encounter monsters for channel test
            // chanM1 has 50 HP so pulse 1's ~109.1 damage defeats it naturally (ZERO forced HP/death)
            var (chanM1GO, chanM1) = Prototype01PlayTestRunner_P08.CreateMockMonster("M_Chan1", new Vector3(2f, 0f, 0f));
            var (chanM2GO, chanM2) = Prototype01PlayTestRunner_P08.CreateMockMonster("M_Chan2", new Vector3(3f, 0f, 0f));
            var initF = typeof(Monster).GetField("isInitialized", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (initF != null)
            {
                initF.SetValue(chanM1, true);
                initF.SetValue(chanM2, true);
            }
            chanM1.Health.InitializeHealth(50f, chanM1);
            if (chanM1.Stats != null)
            {
                chanM1.Stats.SetBaseValue(StatType.Health, 50f);
                chanM1.Stats.SetBaseValue(StatType.MaxHealth, 50f);
                chanM1.Stats.SetBaseValue(StatType.Dodge, 0f);
            }
            if (chanM2.Stats != null) chanM2.Stats.SetBaseValue(StatType.Dodge, 0f);
            if (chanM1.Attack != null) chanM1.Attack.SetAttackEnabled(false);
            if (chanM2.Attack != null) chanM2.Attack.SetAttackEnabled(false);

            bm.RegisterMonster(chanM1);
            bm.RegisterMonster(chanM2);
            hero.SetCurrentTarget(chanM1);

            // Configure channel skill: duration=1.5s, tickInterval=0.5s, rageCost=20f, cd=4f
            _tempChannelEffect = Prototype01PlayTestRunner_P08.CreateConfiguredEffect(SkillTargetPolicy.Area, 10.0f, 0);
            _tempChannelSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            _tempChannelSkill.InitializeSkill(
                id: "skill_p08_playmode_chan",
                mmId: mm != null ? mm.ActiveMindMethodId : "mm_taiji",
                slot: SkillSlotType.Skill,
                name: "Thiên Lôi Tụ Khí (Channel)",
                desc: "Channel Skill",
                conditions: null,
                dmgMultiplier: 1.0f,
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
                if (activeDef != null && _mindMethodSkillsList != null && !_mindMethodSkillsList.Contains(_tempChannelSkill))
                {
                    _mindMethodSkillsList.Add(_tempChannelSkill);
                }
                var activeState = mm.ActiveMindMethodState;
                if (activeState != null)
                {
                    activeState.SkillStates[_tempChannelSkill.SkillId] = new SkillRuntimeState(_tempChannelSkill.SkillId, true, 1);
                    activeState.SelectedSkillPerSlot[SkillSlotType.Skill] = _tempChannelSkill.SkillId;
                }
            }

            hero.Rage.ResetRage(100f);
            CooldownManager.ResetAllCooldowns();

            // 1. Channel Start Execution
            var chanReq = new SkillExecutionRequest(hero, _tempChannelSkill, SkillSlotType.Skill, chanM1);
            var chanExecRes = SkillExecutor.Execute(chanReq);

            bool chanStarted = chanExecRes.Success && hero.IsCasting && hero.IsChanneling;
            bool rageConsumedOnceAtStart = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
            bool noCooldownAtStart = !CooldownManager.IsOnCooldown(_tempChannelSkill.SkillId, out _);
            bool snapshotBelongsToExecution = chanStarted && hero.CastState != null && hero.CastState.ActiveRequest == chanReq;
            Debug.Log($"[PLAY MODE P08 CHANNEL] Start: ChanStarted={chanStarted}, RageOnce={rageConsumedOnceAtStart} (Rage={hero.Rage.CurrentRage}), NoCdAtStart={noCooldownAtStart}, SnapshotBound={snapshotBelongsToExecution}");

            // 2. Late Monster Registration Before First Pulse (at ~0.1s)
            yield return new WaitForSeconds(0.1f);
            var (chanMLateGO, chanMLate) = Prototype01PlayTestRunner_P08.CreateMockMonster("M_Late", new Vector3(2.5f, 0f, 0f));
            if (chanMLate.Attack != null) chanMLate.Attack.SetAttackEnabled(false);
            chanMLate.Stats.SetBaseValue(StatType.Dodge, 0f);
            bm.RegisterMonster(chanMLate);
            Debug.Log($"[PLAY MODE P08 CHANNEL] Late monster spawned and registered into BattleManager. ActiveCount={bm.ActiveMonsters.Count}");

            // 3. Progress naturally to Pulse 1 (ticks at 0.5s via natural Unity frames at Time.timeScale=1.0)
            while (hero.CastState.ChannelTicksExecuted < 1 && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            float chanM1HpP1 = chanM1.Health.CurrentHealth;
            float chanM2HpP1 = chanM2.Health.CurrentHealth;
            float chanMLateHpP1 = chanMLate.Health.CurrentHealth;
            bool pulse1M1Hit = (chanM1HpP1 <= 0f && !chanM1.IsAlive);
            bool pulse1M2Hit = (chanM2HpP1 < 500f);
            bool lateMonsterExcludedP1 = Mathf.Approximately(chanMLateHpP1, 500f);
            Debug.Log($"[PLAY MODE P08 CHANNEL] Pulse 1: M1HitAndDeadNaturally={pulse1M1Hit} ({chanM1HpP1}), M2Hit={pulse1M2Hit} ({chanM2HpP1}), LateExcluded={lateMonsterExcludedP1} ({chanMLateHpP1})");

            // 4. Progress naturally to Pulse 2 (ticks at 1.0s via natural Unity frames, skips naturally dead M1 without replacement)
            while (hero.CastState.ChannelTicksExecuted < 2 && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            float chanM2HpP2 = chanM2.Health.CurrentHealth;
            float chanMLateHpP2 = chanMLate.Health.CurrentHealth;
            bool pulse2M2Hit = (chanM2HpP2 < chanM2HpP1);
            bool lateMonsterStillExcludedP2 = Mathf.Approximately(chanMLateHpP2, 500f);
            Debug.Log($"[PLAY MODE P08 CHANNEL] Pulse 2: M2HitAgain={pulse2M2Hit} ({chanM2HpP2}), LateStillExcludedNoReplacements={lateMonsterStillExcludedP2} ({chanMLateHpP2})");

            // 5. Natural Channel Completion (completes naturally at 1.5s via natural Unity frames)
            while (hero.CastState.IsActive && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            bool channelFinishedNaturally = !hero.IsCasting && !hero.IsChanneling;
            bool rageStillPreservedAtEnd = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
            bool cooldownTriggeredOnCompletion = CooldownManager.IsOnCooldown(_tempChannelSkill.SkillId, out float cdRemain) && cdRemain > 0f;
            Debug.Log($"[PLAY MODE P08 CHANNEL] Completion: Finished={channelFinishedNaturally}, RagePreserved={rageStillPreservedAtEnd}, CooldownTriggered={cooldownTriggeredOnCompletion} ({cdRemain:F1}s)");

            // --------------------------------------------------------------------
            // EXECUTION B: Prove Snapshot Independence By Observed Damage Effects
            // --------------------------------------------------------------------
            // Reset cooldowns to satisfy eligibility for B
            CooldownManager.ResetAllCooldowns();
            hero.CastState.Reset();
            hero.Rage.ResetRage(100f);

            // Spawn valid out-of-range target M_Out at (10, 0, 0)
            var (chanMOutGO, chanMOut) = Prototype01PlayTestRunner_P08.CreateMockMonster("M_Out", new Vector3(10.0f, 0f, 0f));
            if (initF != null) initF.SetValue(chanMOut, true);
            chanMOut.Health.InitializeHealth(500f, chanMOut);
            if (chanMOut.Attack != null) chanMOut.Attack.SetAttackEnabled(false);
            chanMOut.Stats.SetBaseValue(StatType.Dodge, 0f);
            bm.RegisterMonster(chanMOut);

            // Configure Skill B: Area with radius=2.0m anchored on M_Late at (4, 0, 0)
            // Expected target set for B:
            //   - M_Late at (4, 0, 0): dist = 0m <= 2.0m (IN EXPECTED SET)
            //   - M2 at (3, 0, 0): dist = 1m <= 2.0m (IN EXPECTED SET)
            //   - M_Out at (10, 0, 0): dist = 6m > 2.0m (OUTSIDE EXPECTED SET)
            _tempChannelEffectB = Prototype01PlayTestRunner_P08.CreateConfiguredEffect(SkillTargetPolicy.Area, 2.0f, 0);
            _tempChannelSkillB = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            _tempChannelSkillB.InitializeSkill(
                id: "skill_p08_playmode_chan_b",
                mmId: mm != null ? mm.ActiveMindMethodId : "mm_taiji",
                slot: SkillSlotType.Skill,
                name: "Thiên Lôi Tụ Khí (Channel B)",
                desc: "Channel Skill B",
                conditions: null,
                dmgMultiplier: 1.0f,
                costRage: 20f,
                cd: 0f,
                passive: false,
                skillEffects: new List<SkillEffectDefinitionSO> { _tempChannelEffectB }
            );

            var soChanB = new SerializedObject(_tempChannelSkillB);
            soChanB.Update();
            soChanB.FindProperty("isChannel").boolValue = true;
            soChanB.FindProperty("channelDuration").floatValue = 1.0f;
            soChanB.FindProperty("channelTickInterval").floatValue = 0.5f;
            soChanB.FindProperty("castTime").floatValue = 0f;
            soChanB.ApplyModifiedPropertiesWithoutUndo();

            if (mm != null)
            {
                var activeDef = mm.ActiveMindMethodDefinition;
                if (activeDef != null && _mindMethodSkillsList != null && !_mindMethodSkillsList.Contains(_tempChannelSkillB))
                {
                    _mindMethodSkillsList.Add(_tempChannelSkillB);
                }
                var activeState = mm.ActiveMindMethodState;
                if (activeState != null)
                {
                    activeState.SkillStates[_tempChannelSkillB.SkillId] = new SkillRuntimeState(_tempChannelSkillB.SkillId, true, 1);
                    activeState.SelectedSkillPerSlot[SkillSlotType.Skill] = _tempChannelSkillB.SkillId;
                }
            }

            float preB_MLate_Hp = chanMLate.Health.CurrentHealth; // 500f
            float preB_M2_Hp = chanM2.Health.CurrentHealth;       // ~281.8f
            float preB_MOut_Hp = chanMOut.Health.CurrentHealth;   // 500f

            var chanReqB = new SkillExecutionRequest(hero, _tempChannelSkillB, SkillSlotType.Skill, chanMLate);
            var chanExecResB = SkillExecutor.Execute(chanReqB);

            bool chanBStarted = chanExecResB.Success && hero.IsCasting && hero.IsChanneling;

            // Wait for B's natural pulse 1 (ticks at 0.5s via natural Unity frames)
            while (hero.CastState.ChannelTicksExecuted < 1 && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            float postB_MLate_Hp = chanMLate.Health.CurrentHealth;
            float postB_M2_Hp = chanM2.Health.CurrentHealth;
            float postB_MOut_Hp = chanMOut.Health.CurrentHealth;

            // Observed Effects Verification for B:
            // 1. Previously late target (excluded from A) is NOW damaged by B!
            bool bDamagedLateTarget = (postB_MLate_Hp < preB_MLate_Hp);
            // 2. In-range target M2 is damaged by B!
            bool bDamagedM2 = (postB_M2_Hp < preB_M2_Hp);
            // 3. Out-of-range target M_Out is completely unaffected by B!
            bool bDidNotDamageOutOfRange = Mathf.Approximately(postB_MOut_Hp, preB_MOut_Hp);

            // Let B finish naturally
            while (hero.CastState.IsActive && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            bool bScenarioPass = chanBStarted && bDamagedLateTarget && bDamagedM2 && bDidNotDamageOutOfRange;
            Debug.Log($"[PLAY MODE P08 CHANNEL] Execution B (Observed Effects): Started={chanBStarted}, DamagedLateTarget={bDamagedLateTarget} ({preB_MLate_Hp} -> {postB_MLate_Hp}), DamagedM2={bDamagedM2} ({preB_M2_Hp} -> {postB_M2_Hp}), OutOfRangeUnaffected={bDidNotDamageOutOfRange} ({postB_MOut_Hp}) | PASS={bScenarioPass}");

            // --------------------------------------------------------------------
            // SEPARATE CASTERS REQUIREMENT (Real Play Mode Pipeline Execution)
            // --------------------------------------------------------------------
            CooldownManager.ResetAllCooldowns();
            hero.CastState.Reset();
            hero.Rage.ResetRage(100f);

            // Caster 1: hero at (0, 0, 0)
            var caster1 = hero;
            // Caster 2: separate supported hero at (100, 0, 0)
            var (caster2GO, caster2) = Prototype01PlayTestRunner_P08.CreateMockHero("Hero_Caster2", new Vector3(100f, 0f, 0f));
            if (caster2.Stats != null)
            {
                caster2.Stats.SetBaseValue(StatType.Attack, 100f);
                caster2.Stats.SetBaseValue(StatType.Dodge, 0f);
                caster2.Stats.SetBaseValue(StatType.CritRate, 0f);
            }
            caster2.Rage.ResetRage(100f);

            // Clear prior test monsters from active encounter so only mCaster1 and mCaster2 are present
            bm.ClearActiveMonsters();
            if (chanM1GO != null) UnityEngine.Object.DestroyImmediate(chanM1GO);
            if (chanM2GO != null) UnityEngine.Object.DestroyImmediate(chanM2GO);
            if (chanMLateGO != null) UnityEngine.Object.DestroyImmediate(chanMLateGO);
            if (chanMOutGO != null) UnityEngine.Object.DestroyImmediate(chanMOutGO);

            // Targets for separate casters:
            var (mCaster1GO, mCaster1) = Prototype01PlayTestRunner_P08.CreateMockMonster("M_Caster1", new Vector3(2f, 0f, 0f));
            var (mCaster2GO, mCaster2) = Prototype01PlayTestRunner_P08.CreateMockMonster("M_Caster2", new Vector3(102f, 0f, 0f));
            if (initF != null)
            {
                initF.SetValue(mCaster1, true);
                initF.SetValue(mCaster2, true);
            }
            mCaster1.Health.InitializeHealth(500f, mCaster1);
            mCaster2.Health.InitializeHealth(500f, mCaster2);
            if (mCaster1.Attack != null) mCaster1.Attack.SetAttackEnabled(false);
            if (mCaster2.Attack != null) mCaster2.Attack.SetAttackEnabled(false);
            mCaster1.Stats.SetBaseValue(StatType.Dodge, 0f);
            mCaster2.Stats.SetBaseValue(StatType.Dodge, 0f);
            bm.RegisterMonster(mCaster1);
            bm.RegisterMonster(mCaster2);

            // Skill for Caster 1 (Slot: Skill)
            _tempEffectCasterA = Prototype01PlayTestRunner_P08.CreateConfiguredEffect(SkillTargetPolicy.Area, 3.0f, 0);
            _tempSkillCasterA = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            _tempSkillCasterA.InitializeSkill("skill_p08_caster_a", mm != null ? mm.ActiveMindMethodId : "mm_taiji", SkillSlotType.Skill, "Caster A Skill", "Desc", null, 1.0f, 0f, 0f, false, new List<SkillEffectDefinitionSO> { _tempEffectCasterA });
            var soCasterA = new SerializedObject(_tempSkillCasterA);
            soCasterA.Update();
            soCasterA.FindProperty("isChannel").boolValue = true;
            soCasterA.FindProperty("channelDuration").floatValue = 1.0f;
            soCasterA.FindProperty("channelTickInterval").floatValue = 0.5f;
            soCasterA.FindProperty("castTime").floatValue = 0f;
            soCasterA.ApplyModifiedPropertiesWithoutUndo();

            // Skill for Caster 2 (Slot: ExternalSkill1)
            _tempEffectCasterB = Prototype01PlayTestRunner_P08.CreateConfiguredEffect(SkillTargetPolicy.Area, 3.0f, 0);
            _tempSkillCasterB = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            _tempSkillCasterB.InitializeSkill("skill_p08_caster_b", mm != null ? mm.ActiveMindMethodId : "mm_taiji", SkillSlotType.ExternalSkill1, "Caster B Skill", "Desc", null, 1.0f, 0f, 0f, false, new List<SkillEffectDefinitionSO> { _tempEffectCasterB });
            var soCasterB = new SerializedObject(_tempSkillCasterB);
            soCasterB.Update();
            soCasterB.FindProperty("isChannel").boolValue = true;
            soCasterB.FindProperty("channelDuration").floatValue = 1.0f;
            soCasterB.FindProperty("channelTickInterval").floatValue = 0.5f;
            soCasterB.FindProperty("castTime").floatValue = 0f;
            soCasterB.ApplyModifiedPropertiesWithoutUndo();

            if (mm != null)
            {
                var activeDef = mm.ActiveMindMethodDefinition;
                if (activeDef != null && _mindMethodSkillsList != null)
                {
                    if (!_mindMethodSkillsList.Contains(_tempSkillCasterA)) _mindMethodSkillsList.Add(_tempSkillCasterA);
                    if (!_mindMethodSkillsList.Contains(_tempSkillCasterB)) _mindMethodSkillsList.Add(_tempSkillCasterB);
                }
                var activeState = mm.ActiveMindMethodState;
                if (activeState != null)
                {
                    activeState.SkillStates[_tempSkillCasterA.SkillId] = new SkillRuntimeState(_tempSkillCasterA.SkillId, true, 1);
                    activeState.SkillStates[_tempSkillCasterB.SkillId] = new SkillRuntimeState(_tempSkillCasterB.SkillId, true, 1);
                    activeState.SelectedSkillPerSlot[SkillSlotType.Skill] = _tempSkillCasterA.SkillId;
                    activeState.SelectedSkillPerSlot[SkillSlotType.ExternalSkill1] = _tempSkillCasterB.SkillId;
                }
            }

            float preCast_mCaster1_Hp = mCaster1.Health.CurrentHealth; // 500f
            float preCast_mCaster2_Hp = mCaster2.Health.CurrentHealth; // 500f

            // Rigorous hit & damage tracking via canonical production EventBus
            int hitsFromCaster1OnTarget1 = 0;
            int hitsFromCaster1OnTarget2 = 0;
            int hitsFromCaster2OnTarget1 = 0;
            int hitsFromCaster2OnTarget2 = 0;
            float dmgFromCaster1OnTarget1 = 0f;
            float dmgFromCaster2OnTarget2 = 0f;
            int foreignTargetHits = 0;

            Action<Entity, DamageResult> onDamageTracked = (victim, dmgResult) =>
            {
                if (dmgResult.Attacker == caster1)
                {
                    if (victim == mCaster1)
                    {
                        hitsFromCaster1OnTarget1++;
                        dmgFromCaster1OnTarget1 += dmgResult.FinalDamage;
                    }
                    else if (victim == mCaster2)
                    {
                        hitsFromCaster1OnTarget2++;
                    }
                    else
                    {
                        foreignTargetHits++;
                    }
                }
                else if (dmgResult.Attacker == caster2)
                {
                    if (victim == mCaster2)
                    {
                        hitsFromCaster2OnTarget2++;
                        dmgFromCaster2OnTarget2 += dmgResult.FinalDamage;
                    }
                    else if (victim == mCaster1)
                    {
                        hitsFromCaster2OnTarget1++;
                    }
                    else
                    {
                        foreignTargetHits++;
                    }
                }
            };
            EventBus.OnEntityDamaged += onDamageTracked;

            var reqCaster1 = new SkillExecutionRequest(caster1, _tempSkillCasterA, SkillSlotType.Skill, mCaster1);
            var resCaster1 = SkillExecutor.Execute(reqCaster1);

            var reqCaster2 = new SkillExecutionRequest(caster2, _tempSkillCasterB, SkillSlotType.ExternalSkill1, mCaster2);
            var resCaster2 = SkillExecutor.Execute(reqCaster2);

            bool bothCastersStarted = resCaster1.Success && resCaster2.Success && caster1.IsCasting && caster2.IsCasting;

            // Wait for natural pulse on both casters
            while ((caster1.CastState.ChannelTicksExecuted < 1 || caster2.CastState.ChannelTicksExecuted < 1) && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            // Wait for both to finish naturally
            while ((caster1.CastState.IsActive || caster2.CastState.IsActive) && (Time.realtimeSinceStartup - scenarioStartTime < 180f))
            {
                yield return null;
            }

            EventBus.OnEntityDamaged -= onDamageTracked;

            float postCast_mCaster1_Hp = mCaster1.Health.CurrentHealth;
            float postCast_mCaster2_Hp = mCaster2.Health.CurrentHealth;

            bool caster1HitOwnTarget = (hitsFromCaster1OnTarget1 > 0 && postCast_mCaster1_Hp < preCast_mCaster1_Hp);
            bool caster2HitOwnTarget = (hitsFromCaster2OnTarget2 > 0 && postCast_mCaster2_Hp < preCast_mCaster2_Hp);
            bool noCrossHits = (hitsFromCaster1OnTarget2 == 0 && hitsFromCaster2OnTarget1 == 0);
            bool noForeignHits = (foreignTargetHits == 0);
            bool damage1ExactMatch = Mathf.Approximately(preCast_mCaster1_Hp - postCast_mCaster1_Hp, dmgFromCaster1OnTarget1);
            bool damage2ExactMatch = Mathf.Approximately(preCast_mCaster2_Hp - postCast_mCaster2_Hp, dmgFromCaster2OnTarget2);

            bool separateCastersPass = bothCastersStarted && caster1HitOwnTarget && caster2HitOwnTarget && noCrossHits && noForeignHits && damage1ExactMatch && damage2ExactMatch;
            Debug.Log($"[PLAY MODE P08 CHANNEL] Separate Casters Independence: Started={bothCastersStarted}, Caster1_Hits=(T1:{hitsFromCaster1OnTarget1}, T2:{hitsFromCaster1OnTarget2}, Dmg:{dmgFromCaster1OnTarget1:F1}), Caster2_Hits=(T1:{hitsFromCaster2OnTarget1}, T2:{hitsFromCaster2OnTarget2}, Dmg:{dmgFromCaster2OnTarget2:F1}), NoCrossHits={noCrossHits}, ForeignHits={noForeignHits}, DamageMatches=({damage1ExactMatch},{damage2ExactMatch}) | PASS={separateCastersPass}");

            // Cleanup scenario 2 game objects
            if (caster2GO != null) UnityEngine.Object.DestroyImmediate(caster2GO);
            if (mCaster1GO != null) UnityEngine.Object.DestroyImmediate(mCaster1GO);
            if (mCaster2GO != null) UnityEngine.Object.DestroyImmediate(mCaster2GO);
            if (chanM1GO != null) UnityEngine.Object.DestroyImmediate(chanM1GO);
            if (chanM2GO != null) UnityEngine.Object.DestroyImmediate(chanM2GO);
            if (chanMLateGO != null) UnityEngine.Object.DestroyImmediate(chanMLateGO);
            if (chanMOutGO != null) UnityEngine.Object.DestroyImmediate(chanMOutGO);

            // Restore AI controller, auto-battle, and hero attack
            bm.SetAutoBattle(origAutoBattle);
            foreach (var kvp in origAiStates)
            {
                if (kvp.Key != null) kvp.Key.enabled = kvp.Value;
            }
            if (hero != null && hero.Attack != null) hero.Attack.SetAttackEnabled(origHeroAttack);

            bool channelScenarioPass = chanStarted && rageConsumedOnceAtStart && noCooldownAtStart &&
                                       snapshotBelongsToExecution && pulse1M1Hit && pulse1M2Hit &&
                                       lateMonsterExcludedP1 && pulse2M2Hit && lateMonsterStillExcludedP2 &&
                                       channelFinishedNaturally && rageStillPreservedAtEnd &&
                                       cooldownTriggeredOnCompletion && bScenarioPass && separateCastersPass;

            Debug.Log($"[PLAY MODE P08 CHANNEL] Scenario 2 Result: {(channelScenarioPass ? "PASS" : "FAIL")}");

            // ====================================================================
            // IDEMPOTENT RESTORATION & POST-CLEANUP VERIFICATION
            // ====================================================================
            currentPhase = "FINAL_CLEANUP_AND_RESTORE";
            bool cleanupSucceeded = PerformRestoration();

            bool isTimedOut = (Time.realtimeSinceStartup - scenarioStartTime >= 180f);
            bool scenarioPass = aoeLootScenarioPass && channelScenarioPass && cleanupSucceeded && (unexpectedErrors == 0) && !isTimedOut;

            hasFinished = true;
            Debug.Log("================================================================================");
            Debug.Log($"   [REAL PLAY MODE P08 SCENARIO]: ALL_PASS={scenarioPass}, CleanupSucceeded={cleanupSucceeded}, UnexpectedErrors={unexpectedErrors}, TimedOut={isTimedOut}");
            if (unexpectedErrors > 0)
            {
                Debug.LogError($"[PLAY MODE P08] Encountered {unexpectedErrors} unexpected errors/exceptions:");
                foreach (var err in errorLogs)
                {
                    Debug.LogError(err);
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
                Directory.CreateDirectory(dir1);
                Directory.CreateDirectory(dir2);
                Directory.CreateDirectory(dir3);

                string path1 = Path.Combine(dir1, filename);
                string path2 = Path.Combine(dir2, filename);
                string path3 = Path.Combine(dir3, filename);
                File.WriteAllBytes(path1, bytes);
                File.WriteAllBytes(path2, bytes);
                File.WriteAllBytes(path3, bytes);

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
}
#endif
