#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Entities.Components;
using WuxiaGame.Progression;
using WuxiaGame.UI;

namespace WuxiaGame.Editor
{
    public static partial class Prototype01PlayTestRunner
    {
        private class MockRisk04PermissionEntity : Entity
        {
            public bool MockCanUseSkill = true;
            public bool MockCanUseUltimate = true;
            public override bool CanUseSkill => MockCanUseSkill;
            public override bool CanUseUltimate => MockCanUseUltimate;
        }

        private class TestRisk04Hero : Hero
        {
            public float SimulatedDeltaTime = 0.05f;
            public BattleHUD BoundHUD;

            protected override float GetDeltaTime() => SimulatedDeltaTime;

            public void CallUpdate(float dt = -1f)
            {
                if (dt > 0f) SimulatedDeltaTime = dt;
                base.Update();
                if (Movement != null) Movement.ManualTick(SimulatedDeltaTime);
                if (BoundHUD != null && BoundHUD.HeroCastBarUI != null)
                {
                    BoundHUD.HeroCastBarUI.ManualUpdate(SimulatedDeltaTime);
                }
            }
        }

        private static bool ApplyCC_Risk04(Entity entity, CrowdControlType ccType, float duration, string ccId = null)
        {
            if (entity == null || entity.StatusController == null) return false;
            if (string.IsNullOrEmpty(ccId))
            {
                ccId = "cc_r04_" + ccType.ToString().ToLower() + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            }
            return entity.StatusController.ApplyCrowdControl(ccId, ccType, duration);
        }

        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9 RISK-04 Tests (TEST 01 to TEST 12 + Play Mode A to F)")]
        public static bool RunAllPrototype07_9_Risk04_Tests()
        {
            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 RISK-04 VALIDATION TESTS");
            Debug.Log("   Scope: Ultimate Permission Check / CanUseUltimate Validation Gate");
            Debug.Log("==================================================");

            int passed = 0;
            int total = 18; // 12 Automated Tests + 6 Play Mode Visual/Runtime Scenarios

            // 12 AUTOMATED TESTS
            bool t01 = P07_9_R04_01_UltimateNormalAllowed();
            if (t01) passed++;

            bool t02 = P07_9_R04_02_UltimateStunRejected();
            if (t02) passed++;

            bool t03 = P07_9_R04_03_UltimateFreezeRejected();
            if (t03) passed++;

            bool t04 = P07_9_R04_04_UltimateRootAllowed();
            if (t04) passed++;

            bool t05 = P07_9_R04_05_UltimateAntiCCAllowed();
            if (t05) passed++;

            bool t06 = P07_9_R04_06_UltimateValidationFailureNoRageConsumed();
            if (t06) passed++;

            bool t07 = P07_9_R04_07_UltimateRejectedWhileCasting();
            if (t07) passed++;

            bool t08 = P07_9_R04_08_UltimateCastTimeValidationBeforeRage();
            if (t08) passed++;

            bool t09 = P07_9_R04_09_UltimateChannelValidationBeforeRage();
            if (t09) passed++;

            bool t10 = P07_9_R04_10_InstantUltimateBackwardCompatibility();
            if (t10) passed++;

            bool t11 = P07_9_R04_11_NormalSkillBehaviorUnchanged();
            if (t11) passed++;

            bool t12 = P07_9_R04_12_P07_8DamageShieldPipelineUnchanged();
            if (t12) passed++;

            // 6 PLAY MODE SCENARIOS A - F
            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 RISK-04 PLAY MODE SCENARIOS A - F");
            Debug.Log("==================================================");

            bool pmA = P07_9_R04_PMA_UltimateNormal_PlayMode();
            if (pmA) passed++;

            bool pmB = P07_9_R04_PMB_UltimateStun_PlayMode();
            if (pmB) passed++;

            bool pmC = P07_9_R04_PMC_UltimateFreeze_PlayMode();
            if (pmC) passed++;

            bool pmD = P07_9_R04_PMD_UltimateRoot_PlayMode();
            if (pmD) passed++;

            bool pmE = P07_9_R04_PME_UltimateAntiCC_PlayMode();
            if (pmE) passed++;

            bool pmF = P07_9_R04_PMF_UltimateRejectedNoRage_PlayMode();
            if (pmF) passed++;

            Debug.Log("==================================================");
            Debug.Log($"[PROTOTYPE 07.9 RISK-04 SUMMARY] Passed: {passed}/{total} ({(passed == total ? "ALL PASS" : "FAILURES DETECTED")})");
            Debug.Log("==================================================");

            return passed == total;
        }

        // =========================================================================
        // AUTOMATED TESTS 01 - 12
        // =========================================================================

        /// <summary>
        /// TEST 01: Ultimate + normal state -> allowed.
        /// </summary>
        private static bool P07_9_R04_01_UltimateNormalAllowed()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            bool canUltBefore = hero.CanUseUltimate;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool ok = canUltBefore && res.Success && res.FailureReason == SkillExecutionFailureReason.None;

            Debug.Log($"[P07_9_R04_01] TEST 01 -> CanUltBefore: {canUltBefore}, Success: {res.Success}, FailureReason: {res.FailureReason} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 02: Ultimate + Stun -> rejected (SourceCrowdControlled).
        /// </summary>
        private static bool P07_9_R04_02_UltimateStunRejected()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            ApplyCC_Risk04(hero, CrowdControlType.Stun, 2.0f, "cc_stun_r04_02");
            bool canUlt = hero.CanUseUltimate;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool ok = !canUlt && !res.Success && res.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled;

            Debug.Log($"[P07_9_R04_02] TEST 02 -> CanUlt: {canUlt}, Success: {res.Success}, Reason: {res.FailureReason} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 03: Ultimate + Freeze -> rejected (SourceCrowdControlled).
        /// </summary>
        private static bool P07_9_R04_03_UltimateFreezeRejected()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            ApplyCC_Risk04(hero, CrowdControlType.Freeze, 2.0f, "cc_freeze_r04_03");
            bool canUlt = hero.CanUseUltimate;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool ok = !canUlt && !res.Success && res.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled;

            Debug.Log($"[P07_9_R04_03] TEST 03 -> CanUlt: {canUlt}, Success: {res.Success}, Reason: {res.FailureReason} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 04: Ultimate + Root -> allowed.
        /// Under Root, CanMove/CanDash are false, but CanUseUltimate remains true.
        /// </summary>
        private static bool P07_9_R04_04_UltimateRootAllowed()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            ApplyCC_Risk04(hero, CrowdControlType.Root, 2.0f, "cc_root_r04_04");
            bool canMove = hero.CanMove;
            bool canUlt = hero.CanUseUltimate;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool ok = !canMove && canUlt && res.Success && res.FailureReason == SkillExecutionFailureReason.None;

            Debug.Log($"[P07_9_R04_04] TEST 04 -> CanMove: {canMove}, CanUlt: {canUlt}, Success: {res.Success} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 05: Ultimate + Anti-CC -> allowed when Anti-CC blocks CC.
        /// </summary>
        private static bool P07_9_R04_05_UltimateAntiCCAllowed()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            hero.StatusController.ApplyAntiCCImmunity(5.0f);
            bool stunApplied = ApplyCC_Risk04(hero, CrowdControlType.Stun, 2.0f, "cc_stun_r04_05");
            bool canUlt = hero.CanUseUltimate;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool ok = !stunApplied && canUlt && res.Success && res.FailureReason == SkillExecutionFailureReason.None;

            Debug.Log($"[P07_9_R04_05] TEST 05 -> StunApplied: {stunApplied}, CanUlt: {canUlt}, Success: {res.Success} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 06: Ultimate validation failure -> Rage is NOT consumed.
        /// </summary>
        private static bool P07_9_R04_06_UltimateValidationFailureNoRageConsumed()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);
            float initialRage = hero.Rage.CurrentRage;

            ApplyCC_Risk04(hero, CrowdControlType.Stun, 2.0f, "cc_stun_r04_06");
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            float afterRage = hero.Rage.CurrentRage;

            bool ok = !res.Success && Mathf.Approximately(initialRage, afterRage);
            Debug.Log($"[P07_9_R04_06] TEST 06 -> Success: {res.Success}, InitialRage: {initialRage}, AfterRage: {afterRage} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 07: Ultimate rejected while casting -> SourceAlreadyCasting intact.
        /// </summary>
        private static bool P07_9_R04_07_UltimateRejectedWhileCasting()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            // Start casting regular skill
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool isCasting = hero.IsCasting;

            // Attempt to execute Ultimate while actively casting
            var resUlt = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool ok = isCasting && !resUlt.Success && resUlt.FailureReason == SkillExecutionFailureReason.SourceAlreadyCasting;

            Debug.Log($"[P07_9_R04_07] TEST 07 -> IsCasting: {isCasting}, UltSuccess: {resUlt.Success}, Reason: {resUlt.FailureReason} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 08: Ultimate CastTime > 0 -> validation occurs before Rage consumption and before cast start.
        /// </summary>
        private static bool P07_9_R04_08_UltimateCastTimeValidationBeforeRage()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO ultDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            ultDef.SetCastTime(1.5f);
            hero.Rage.AddRage(100f);
            float initialRage = hero.Rage.CurrentRage;

            // Stun blocks execution
            ApplyCC_Risk04(hero, CrowdControlType.Stun, 2.0f, "cc_stun_r04_08");
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            float afterRage = hero.Rage.CurrentRage;
            bool isCasting = hero.IsCasting;

            bool ok = !res.Success && res.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled &&
                      Mathf.Approximately(initialRage, afterRage) && !isCasting;

            Debug.Log($"[P07_9_R04_08] TEST 08 -> Success: {res.Success}, RagePreserved: {Mathf.Approximately(initialRage, afterRage)}, IsCasting: {isCasting} | {(ok ? "PASS" : "FAIL")}");
            ultDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 09: Ultimate Channel -> validation occurs before Rage consumption and before channel start.
        /// </summary>
        private static bool P07_9_R04_09_UltimateChannelValidationBeforeRage()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO ultDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            ultDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);
            float initialRage = hero.Rage.CurrentRage;

            // Freeze blocks execution
            ApplyCC_Risk04(hero, CrowdControlType.Freeze, 2.0f, "cc_freeze_r04_09");
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            float afterRage = hero.Rage.CurrentRage;
            bool isCasting = hero.IsCasting;

            bool ok = !res.Success && res.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled &&
                      Mathf.Approximately(initialRage, afterRage) && !isCasting;

            Debug.Log($"[P07_9_R04_09] TEST 09 -> Success: {res.Success}, RagePreserved: {Mathf.Approximately(initialRage, afterRage)}, IsCasting: {isCasting} | {(ok ? "PASS" : "FAIL")}");
            ultDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 10: Instant Ultimate backward compatibility.
        /// Executes immediately, consumes Rage, leaves no residual casting state.
        /// </summary>
        private static bool P07_9_R04_10_InstantUltimateBackwardCompatibility()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO ultDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            ultDef.SetCastTime(0f);
            ultDef.SetChannel(false, 0f, 0f);
            hero.Rage.AddRage(100f);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            float afterMonsterHp = bm.CurrentMonster.Health.CurrentHealth;
            bool isCasting = hero.IsCasting;

            bool ok = res.Success && afterMonsterHp < initialMonsterHp && !isCasting &&
                      Mathf.Approximately(hero.Rage.CurrentRage, 0f);

            Debug.Log($"[P07_9_R04_10] TEST 10 -> Success: {res.Success}, DamageDealt: {initialMonsterHp - afterMonsterHp}, IsCasting: {isCasting} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 11: Normal Skill behavior unchanged.
        /// Regular skill validates CanUseSkill independently of CanUseUltimate.
        /// </summary>
        private static bool P07_9_R04_11_NormalSkillBehaviorUnchanged()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");

            // Mock entity: CanUseSkill == false, CanUseUltimate == true
            GameObject mockGO = new GameObject("MockEntity_R04_11");
            mockGO.AddComponent<EntityStatsComponent>();
            var health = mockGO.AddComponent<HealthComponent>();
            health.InitializeHealth(100f);
            var mock = mockGO.AddComponent<MockRisk04PermissionEntity>();
            mock.MockCanUseSkill = false;
            mock.MockCanUseUltimate = true;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            var reqSkill = new SkillExecutionRequest(mock, skillDef, SkillSlotType.Skill, bm.CurrentMonster);
            bool skillValidation = SkillExecutionValidator.Validate(reqSkill, out var reasonSkill, out _);
            bool regularUsesCanSkill = !skillValidation && reasonSkill == SkillExecutionFailureReason.SourceCrowdControlled;

            // Mock entity: CanUseSkill == true, CanUseUltimate == false -> regular skill passes CC check
            mock.MockCanUseSkill = true;
            mock.MockCanUseUltimate = false;
            bool skillValidation2 = SkillExecutionValidator.Validate(reqSkill, out var reasonSkill2, out _);
            bool regularIgnoresCanUltimate = skillValidation2 || reasonSkill2 != SkillExecutionFailureReason.SourceCrowdControlled;

            bool ok = regularUsesCanSkill && regularIgnoresCanUltimate;
            Debug.Log($"[P07_9_R04_11] TEST 11 -> RegularUsesCanSkill: {regularUsesCanSkill}, RegularIgnoresCanUltimate: {regularIgnoresCanUltimate} | {(ok ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(mockGO);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 12: P07.8 damage/shield pipeline unchanged.
        /// Ultimate damage respects shield absorption priority correctly.
        /// </summary>
        private static bool P07_9_R04_12_P07_8DamageShieldPipelineUnchanged()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            // Add P07.8 Shield to monster
            var monster = bm.CurrentMonster;
            float initialHp = monster.Health.CurrentHealth;
            monster.StatusController.ApplyShield("shield_r04_12", 500f, 30f);

            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, monster);
            float afterHp = monster.Health.CurrentHealth;
            var shield = monster.StatusController.GetShield("shield_r04_12");
            float remainingShield = shield != null ? shield.CurrentAmount : -1f;

            // Shield absorbed damage first, HP remained 100% untouched
            bool ok = res.Success && Mathf.Approximately(initialHp, afterHp) && remainingShield < 500f && remainingShield >= 0f;
            Debug.Log($"[P07_9_R04_12] TEST 12 -> Success: {res.Success}, HpUntouched: {Mathf.Approximately(initialHp, afterHp)}, ShieldRemaining: {remainingShield} | {(ok ? "PASS" : "FAIL")}");

            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        // =========================================================================
        // PLAY MODE VISUAL / RUNTIME ACCEPTANCE SCENARIOS A - F
        // =========================================================================

        private static bool SetupPlayModeScene_Risk04(out TestRisk04Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD battleHUD)
        {
            testHero = null;
            bm = null;
            mmMgr = null;
            battleHUD = null;

            EventBus.ClearAllListeners();
            CooldownManager.ResetAllCooldowns();

            Prototype01SceneBuilder.BuildPrototype02Data();
            Prototype01SceneBuilder.BuildPrototype01Scene();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);

            Hero sceneHero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            mmMgr = UnityEngine.Object.FindAnyObjectByType<MindMethodManager>();
            battleHUD = UnityEngine.Object.FindAnyObjectByType<BattleHUD>();

            if (sceneHero == null || bm == null || mmMgr == null || battleHUD == null)
            {
                Debug.LogError("[PLAY MODE SETUP RISK-04] Missing essential scene components!");
                return false;
            }

            GameObject heroGO = sceneHero.gameObject;
            UnityEngine.Object.DestroyImmediate(sceneHero);
            testHero = heroGO.AddComponent<TestRisk04Hero>();
            testHero.BoundHUD = battleHUD;
            testHero.InitializeHero();
            testHero.Health.Revive(testHero.Health.MaxHealth);
            testHero.EnableEntityActions();
            testHero.Rage.AddRage(100f);
            bm.RegisterHero(testHero);

            if (bm.CurrentMonster == null)
            {
                bm.SpawnMonster();
            }
            bm.CurrentMonster.Health.InitializeHealth(99999f, bm.CurrentMonster);
            bm.CurrentMonster.Health.Revive(99999f);
            bm.CurrentMonster.EnableEntityActions();

            testHero.transform.position = new Vector3(0f, -1.2f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, -1.2f, 0f);
            testHero.SetCurrentTarget(bm.CurrentMonster);

            if (!bm.IsBattleActive)
            {
                bm.StartBattle();
            }

            mmMgr.LoadDatabaseIfMissing();
            mmMgr.ResetPersistence();
            mmMgr.InitializeFromDatabase();
            mmMgr.SetActiveMindMethod("mm_taiji");
            CooldownManager.ResetAllCooldowns();
            testHero.Rage.AddRage(100f);

            // Re-bind CastBarUI to the new testHero
            if (battleHUD.HeroCastBarUI == null)
            {
                CastBarUI foundUI = UnityEngine.Object.FindAnyObjectByType<CastBarUI>();
                if (foundUI != null)
                {
                    battleHUD.SetCastBarReferences(foundUI);
                }
            }
            if (battleHUD.HeroCastBarUI != null)
            {
                battleHUD.HeroCastBarUI.BindEntity(testHero);
                battleHUD.HeroCastBarUI.RegisterEvents();
            }

            return true;
        }

        /// <summary>
        /// SCENARIO A: Ultimate normal state -> execute in Play Mode.
        /// </summary>
        private static bool P07_9_R04_PMA_UltimateNormal_PlayMode()
        {
            if (!SetupPlayModeScene_Risk04(out TestRisk04Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;
            var res = testHero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            testHero.CallUpdate(0.1f);
            float afterMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            bool pass = res.Success && afterMonsterHp < initialMonsterHp;
            Debug.Log($"[RISK-04 PLAY MODE A — ULTIMATE NORMAL] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. Success: {res.Success}\n" +
                      $"2. DamageDealt: {initialMonsterHp - afterMonsterHp:F1}");
            return pass;
        }

        /// <summary>
        /// SCENARIO B: Stun -> Ultimate does not execute in Play Mode.
        /// </summary>
        private static bool P07_9_R04_PMB_UltimateStun_PlayMode()
        {
            if (!SetupPlayModeScene_Risk04(out TestRisk04Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            ApplyCC_Risk04(testHero, CrowdControlType.Stun, 2.0f, "pm_stun_r04");
            testHero.CallUpdate(0.1f);

            var res = testHero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool pass = !res.Success && res.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled;

            Debug.Log($"[RISK-04 PLAY MODE B — ULTIMATE STUN BLOCKED] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. Blocked: {!res.Success}\n" +
                      $"2. Reason: {res.FailureReason}");
            return pass;
        }

        /// <summary>
        /// SCENARIO C: Freeze -> Ultimate does not execute in Play Mode.
        /// </summary>
        private static bool P07_9_R04_PMC_UltimateFreeze_PlayMode()
        {
            if (!SetupPlayModeScene_Risk04(out TestRisk04Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            ApplyCC_Risk04(testHero, CrowdControlType.Freeze, 2.0f, "pm_freeze_r04");
            testHero.CallUpdate(0.1f);

            var res = testHero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool pass = !res.Success && res.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled;

            Debug.Log($"[RISK-04 PLAY MODE C — ULTIMATE FREEZE BLOCKED] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. Blocked: {!res.Success}\n" +
                      $"2. Reason: {res.FailureReason}");
            return pass;
        }

        /// <summary>
        /// SCENARIO D: Root -> Ultimate still executes in Play Mode.
        /// </summary>
        private static bool P07_9_R04_PMD_UltimateRoot_PlayMode()
        {
            if (!SetupPlayModeScene_Risk04(out TestRisk04Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            ApplyCC_Risk04(testHero, CrowdControlType.Root, 2.0f, "pm_root_r04");
            testHero.CallUpdate(0.1f);

            bool isRooted = testHero.IsRooted;
            bool canMove = testHero.CanMove;
            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            var res = testHero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            testHero.CallUpdate(0.1f);
            float afterMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            bool pass = isRooted && !canMove && res.Success && afterMonsterHp < initialMonsterHp;
            Debug.Log($"[RISK-04 PLAY MODE D — ULTIMATE ROOT ALLOWED] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. IsRooted: {isRooted}, CanMove: {canMove}\n" +
                      $"2. Success: {res.Success}, DamageDealt: {initialMonsterHp - afterMonsterHp:F1}");
            return pass;
        }

        /// <summary>
        /// SCENARIO E: Anti-CC -> blocks Stun/Freeze, Ultimate executes in Play Mode.
        /// </summary>
        private static bool P07_9_R04_PME_UltimateAntiCC_PlayMode()
        {
            if (!SetupPlayModeScene_Risk04(out TestRisk04Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            testHero.StatusController.ApplyAntiCCImmunity(5.0f);
            bool stunApplied = ApplyCC_Risk04(testHero, CrowdControlType.Stun, 2.0f, "pm_anticc_r04");
            testHero.CallUpdate(0.1f);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;
            var res = testHero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            testHero.CallUpdate(0.1f);
            float afterMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            bool pass = !stunApplied && !testHero.IsStunned && res.Success && afterMonsterHp < initialMonsterHp;
            Debug.Log($"[RISK-04 PLAY MODE E — ULTIMATE ANTI-CC] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. StunApplied: {stunApplied}, IsStunned: {testHero.IsStunned}\n" +
                      $"2. Success: {res.Success}, DamageDealt: {initialMonsterHp - afterMonsterHp:F1}");
            return pass;
        }

        /// <summary>
        /// SCENARIO F: Rejected Ultimate -> Rage is NOT consumed in Play Mode.
        /// </summary>
        private static bool P07_9_R04_PMF_UltimateRejectedNoRage_PlayMode()
        {
            if (!SetupPlayModeScene_Risk04(out TestRisk04Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            float initialRage = testHero.Rage.CurrentRage;
            ApplyCC_Risk04(testHero, CrowdControlType.Stun, 2.0f, "pm_rage_r04");
            testHero.CallUpdate(0.1f);

            var res = testHero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            float afterRage = testHero.Rage.CurrentRage;

            bool pass = !res.Success && Mathf.Approximately(initialRage, afterRage);
            Debug.Log($"[RISK-04 PLAY MODE F — REJECTED ULTIMATE NO RAGE] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. Blocked: {!res.Success}\n" +
                      $"2. InitialRage: {initialRage:F1}, AfterRage: {afterRage:F1}");
            return pass;
        }
    }
}
#endif
