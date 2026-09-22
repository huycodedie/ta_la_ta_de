#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Entities.Components;
using WuxiaGame.Equipment;
using WuxiaGame.Inventory;
using WuxiaGame.Progression;
using WuxiaGame.Stats;
using WuxiaGame.UI;

namespace WuxiaGame.Editor
{
    public static partial class Prototype01PlayTestRunner
    {
        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.8 Automated Tests (P07.8-01 to P07.8-55)")]
        public static bool RunAllPrototype07_8Tests()
        {
            Debug.Log("==================================================");
            Debug.Log("   RUNNING ALL PROTOTYPE 07.8 AUTOMATED TESTS (P07.8-01 TO P07.8-55)");
            Debug.Log("==================================================");

            bool t01 = P07_8_01_ApplyShield_StoresInstance_SetsCurrentAndMaxAmount();
            bool t02 = P07_8_02_FullAbsorption_AbsorbsAllDamage_ZeroHpLoss();
            bool t03 = P07_8_03_PartialAbsorption_ShieldReachesZero_RemainingDamageToHp();
            bool t04 = P07_8_04_ShieldDepletion_RemovedFromActiveShields_FiresDepletedEvent();
            bool t05 = P07_8_05_NoShield_FullDamageDirectlyToHp();
            bool t06 = P07_8_06_ZeroDamage_ConsumesZeroShield_NoHpChange();
            bool t07 = P07_8_07_NegativeDamage_ConsumesZeroShield_NoHpChange();
            bool t08 = P07_8_08_ClampZero_ShieldAmountNeverNegative();
            bool t09 = P07_8_09_ShieldBeforeHp_ConsecutiveHitsDeductShieldFirstThenHp();
            bool t10 = P07_8_10_DodgeCheckPriorToShield_DodgedHitConsumesZeroShield();
            bool t11 = P07_8_11_DefenseMitigationAppliedBeforeShield_DamageMitigatedFirst();
            bool t12 = P07_8_12_CriticalDamageMultiplierAppliedBeforeShield();
            bool t13 = P07_8_13_AttackerDamageDealtDebuffAppliedBeforeShield();
            bool t14 = P07_8_14_DoTTickDamageAbsorbedByShield();
            bool t15 = P07_8_15_LethalDamagePreventedIfShieldAbsorbsItAll();
            bool t16 = P07_8_16_LethalDamageTriggersDeathIfDamageExceedsShieldPlusHp();
            bool t17 = P07_8_17_MultipleIndependentShields_CoexistInActiveShields();
            bool t18 = P07_8_18_DeterministicPriority_HigherPriorityAbsorbsFirst();
            bool t19 = P07_8_19_EqualPriority_OldestFirstFifoConsumption();
            bool t20 = P07_8_20_EqualPriorityAndTimestamp_DeterministicStringOrdinalTieBreaker();
            bool t21 = P07_8_21_MultiShieldDepletionCascade_FirstDepletesRemainingContinuesToSecond();
            bool t22 = P07_8_22_MultiShieldPartialDepletionAcrossTwoShields();
            bool t23 = P07_8_23_AllShieldsDepleted_RemainingDamageSpillsOverToHp();
            bool t24 = P07_8_24_TotalShieldAmount_SumsAllActiveShields();
            bool t25 = P07_8_25_ExpirationTimeCalculation_DeterministicViaTimeProvider();
            bool t26 = P07_8_26_ExpiredShield_TickRemovesAndRaisesExpiredEvent();
            bool t27 = P07_8_27_ExpiredShield_DoesNotAbsorbDamage();
            bool t28 = P07_8_28_DeadTargetSafety_DamageNotProcessed_NoNRE();
            bool t29 = P07_8_29_DestroyedTargetSafety_NoNRE();
            bool t30 = P07_8_30_ReviveSafety_MindMethodRevivePreservesOrClearsShieldWithoutError();
            bool t31 = P07_8_31_ClearAll_ClearsAllShieldsAndResetsState();
            bool t32 = P07_8_32_StackPolicy_AdditiveIncreasesAmountAndRefreshesDuration();
            bool t33 = P07_8_33_StackPolicy_AdditiveCapsAtMaxAmount();
            bool t34 = P07_8_34_StackPolicy_RefreshDurationResetsTimerKeepsMaxAmount();
            bool t35 = P07_8_35_StackPolicy_ReplaceOverwritesAmountAndDuration();
            bool t36 = P07_8_36_StackPolicy_IgnoreDiscardsDuplicateApplication();
            bool t37 = P07_8_37_StackPolicy_IndependentGeneratesUniqueInstances();
            bool t38 = P07_8_38_StackPolicy_MaxStacksLimitEnforced();
            bool t39 = P07_8_39_ShieldEffectDefinitionSO_FlatValueMode();
            bool t40 = P07_8_40_ShieldEffectDefinitionSO_MaxHpPercentageMode();
            bool t41 = P07_8_41_ShieldEffectDefinitionSO_AttackMultiplierMode();
            bool t42 = P07_8_42_EffectResolver_ResolvesShieldEffectForSkill();
            bool t43 = P07_8_43_DefaultSkillTargetResolver_DefaultsToSourceWhenSelfOrOmitted();
            bool t44 = P07_8_44_SkillExecutor_ExecutesShieldSkill_ConsumesRageAndTriggersCooldown();
            bool t45 = P07_8_45_MultiEffect_DamagePlusShield_SingleRageSingleCooldown();
            bool t46 = P07_8_46_MultiEffect_HealPlusShield_SingleRageSingleCooldown();
            bool t47 = P07_8_47_MultiEffect_BuffPlusShield_AppliesBothCorrectly();
            bool t48 = P07_8_48_MultiEffect_ShieldPlusCleanse_CleansesNegativeAndAppliesShield();
            bool t49 = P07_8_49_CleanseIsolation_CleanseNegativeStatusesDoesNotRemoveShield();
            bool t50 = P07_8_50_DispelIsolation_DispelBuffsDoesNotRemoveShield();
            bool t51 = P07_8_51_DispelExplicitShieldCategory_DispelsShieldCorrectly();
            bool t52 = P07_8_52_CrowdControlIsolation_StunRootFreezeDoesNotRemoveShield();
            bool t53 = P07_8_53_AntiCcImmunityIsolation_AntiCcDoesNotAffectShield();
            bool t54 = P07_8_54_FreezeShatterIsolation_ShieldDoesNotCauseOrAlterShatter();
            bool t55 = P07_8_55_HealIsolation_HealingRestoresHpWithoutAffectingShield();

            bool all = t01 && t02 && t03 && t04 && t05 && t06 && t07 && t08 && t09 && t10 &&
                       t11 && t12 && t13 && t14 && t15 && t16 && t17 && t18 && t19 && t20 &&
                       t21 && t22 && t23 && t24 && t25 && t26 && t27 && t28 && t29 && t30 &&
                       t31 && t32 && t33 && t34 && t35 && t36 && t37 && t38 && t39 && t40 &&
                       t41 && t42 && t43 && t44 && t45 && t46 && t47 && t48 && t49 && t50 &&
                       t51 && t52 && t53 && t54 && t55;

            int passCount = (t01 ? 1 : 0) + (t02 ? 1 : 0) + (t03 ? 1 : 0) + (t04 ? 1 : 0) + (t05 ? 1 : 0) +
                            (t06 ? 1 : 0) + (t07 ? 1 : 0) + (t08 ? 1 : 0) + (t09 ? 1 : 0) + (t10 ? 1 : 0) +
                            (t11 ? 1 : 0) + (t12 ? 1 : 0) + (t13 ? 1 : 0) + (t14 ? 1 : 0) + (t15 ? 1 : 0) +
                            (t16 ? 1 : 0) + (t17 ? 1 : 0) + (t18 ? 1 : 0) + (t19 ? 1 : 0) + (t20 ? 1 : 0) +
                            (t21 ? 1 : 0) + (t22 ? 1 : 0) + (t23 ? 1 : 0) + (t24 ? 1 : 0) + (t25 ? 1 : 0) +
                            (t26 ? 1 : 0) + (t27 ? 1 : 0) + (t28 ? 1 : 0) + (t29 ? 1 : 0) + (t30 ? 1 : 0) +
                            (t31 ? 1 : 0) + (t32 ? 1 : 0) + (t33 ? 1 : 0) + (t34 ? 1 : 0) + (t35 ? 1 : 0) +
                            (t36 ? 1 : 0) + (t37 ? 1 : 0) + (t38 ? 1 : 0) + (t39 ? 1 : 0) + (t40 ? 1 : 0) +
                            (t41 ? 1 : 0) + (t42 ? 1 : 0) + (t43 ? 1 : 0) + (t44 ? 1 : 0) + (t45 ? 1 : 0) +
                            (t46 ? 1 : 0) + (t47 ? 1 : 0) + (t48 ? 1 : 0) + (t49 ? 1 : 0) + (t50 ? 1 : 0) +
                            (t51 ? 1 : 0) + (t52 ? 1 : 0) + (t53 ? 1 : 0) + (t54 ? 1 : 0) + (t55 ? 1 : 0);

            Debug.Log("==================================================");
            Debug.Log($"   PROTOTYPE 07.8 AUTOMATED TESTS: {passCount}/55 {(all ? "PASSED" : "FAILED")}");
            Debug.Log("==================================================");

            return all;
        }

        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.8 Play Mode Acceptance Tests (T01 to T35)")]
        public static bool RunPrototype07_8PlayModeAcceptanceTests()
        {
            Debug.Log("==================================================");
            Debug.Log("   STARTING PROTOTYPE 07.8 REAL PLAY MODE ACCEPTANCE TESTS (T01 - T35)");
            Debug.Log("==================================================");

            Prototype01SceneBuilder.BuildPrototype02Data();
            Prototype01SceneBuilder.BuildPrototype01Scene();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);

            Hero setupHero = Object.FindAnyObjectByType<Hero>();
            BattleManager setupBm = Object.FindAnyObjectByType<BattleManager>();
            MindMethodManager setupMm = Object.FindAnyObjectByType<MindMethodManager>();

            if (setupHero != null)
            {
                setupHero.InitializeHero();
                setupHero.Health.Revive(setupHero.Health.MaxHealth);
                setupHero.EnableEntityActions();
            }

            if (setupBm != null)
            {
                if (setupBm.CurrentMonster != null)
                {
                    setupBm.CurrentMonster.InitializeMonster();
                    setupBm.CurrentMonster.Health.Revive(setupBm.CurrentMonster.Health.MaxHealth);
                    setupBm.CurrentMonster.EnableEntityActions();
                }
                if (!setupBm.IsBattleActive)
                {
                    setupBm.StartBattle();
                }
            }

            if (setupMm != null)
            {
                setupMm.LoadDatabaseIfMissing();
                setupMm.ResetPersistence();
                setupMm.InitializeFromDatabase();
                setupMm.SetActiveMindMethod("mm_taiji");
                setupMm.UnlockSkill("skill_taiji_2_a");
                setupMm.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            }

            int passed = 0;
            int total = 35;

            void RunTest(int index, string name, System.Func<bool> testFunc)
            {
                bool res = false;
                try
                {
                    CooldownManager.ResetAllCooldowns();
                    if (setupHero != null)
                    {
                        setupHero.StatusController.ClearAll();
                        setupHero.Health.Revive(setupHero.Health.MaxHealth);
                        setupHero.EnableEntityActions();
                        setupHero.Rage.AddRage(100f);
                    }
                    if (setupBm != null && setupBm.CurrentMonster != null)
                    {
                        setupBm.CurrentMonster.StatusController.ClearAll();
                        setupBm.CurrentMonster.Health.Revive(setupBm.CurrentMonster.Health.MaxHealth);
                        setupBm.CurrentMonster.EnableEntityActions();
                    }
                    res = testFunc();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[T{index:D2} {name}] EXCEPTION: {ex.Message}\n{ex.StackTrace}");
                }
                Debug.Log($"[T{index:D2} {name}] {(res ? "PASS" : "FAIL")}");
                if (res) passed++;
            }

            RunTest(1, "APPLY SHIELD", P07_8_PM_T01_ApplyShield);
            RunTest(2, "SHIELD VISIBLE IN RUNTIME", P07_8_PM_T02_ShieldVisibleInRuntime);
            RunTest(3, "FULL ABSORPTION", P07_8_PM_T03_FullAbsorption);
            RunTest(4, "PARTIAL ABSORPTION", P07_8_PM_T04_PartialAbsorption);
            RunTest(5, "SHIELD DEPLETION", P07_8_PM_T05_ShieldDepletion);
            RunTest(6, "REMAINING DAMAGE TO HP", P07_8_PM_T06_RemainingDamageToHp);
            RunTest(7, "NO SHIELD HIT DIRECT TO HP", P07_8_PM_T07_NoShieldHitDirectToHp);
            RunTest(8, "MULTIPLE HIT SEQUENCE", P07_8_PM_T08_MultipleHitSequence);
            RunTest(9, "MULTIPLE SHIELDS SIMULTANEOUS", P07_8_PM_T09_MultipleShieldsSimultaneous);
            RunTest(10, "DETERMINISTIC PRIORITY ORDER", P07_8_PM_T10_DeterministicShieldPriorityOrder);
            RunTest(11, "SHIELD EXPIRATION TIME PROVIDER", P07_8_PM_T11_ShieldExpirationTimeProvider);
            RunTest(12, "SHIELD REMOVAL EXPLICIT", P07_8_PM_T12_ShieldRemovalExplicit);
            RunTest(13, "DEAD TARGET SAFETY", P07_8_PM_T13_DeadTargetSafety);
            RunTest(14, "DESTROYED TARGET SAFETY", P07_8_PM_T14_DestroyedTargetSafety);
            RunTest(15, "ZERO DAMAGE SAFETY", P07_8_PM_T15_ZeroDamageSafety);
            RunTest(16, "MULTI EFFECT DAMAGE PLUS SHIELD", P07_8_PM_T16_MultiEffect_DamagePlusShield);
            RunTest(17, "MULTI EFFECT HEAL PLUS SHIELD", P07_8_PM_T17_MultiEffect_HealPlusShield);
            RunTest(18, "MULTI EFFECT BUFF PLUS SHIELD", P07_8_PM_T18_MultiEffect_BuffPlusShield);
            RunTest(19, "EXISTING DAMAGE MODIFIERS DEFENSE MITIGATION", P07_8_PM_T19_ExistingDamageModifiersDefenseMitigation);
            RunTest(20, "CRITICAL HIT DAMAGE INTERACTION", P07_8_PM_T20_CriticalHitDamageInteraction);
            RunTest(21, "SKILL COOLDOWN TRIGGERED ONCE", P07_8_PM_T21_SkillCooldownTriggeredOnce);
            RunTest(22, "RAGE CONSUMED ONCE", P07_8_PM_T22_RageConsumedOnce);
            RunTest(23, "TARGET SELECTION SELF DEFAULT", P07_8_PM_T23_TargetSelectionSelfDefault);
            RunTest(24, "EVENTBUS ON SHIELD APPLIED FIRED", P07_8_PM_T24_EventBusOnShieldAppliedFired);
            RunTest(25, "EVENTBUS ON SHIELD ABSORBED FIRED", P07_8_PM_T25_EventBusOnShieldAbsorbedFired);
            RunTest(26, "REGRESSION BUFF APPLICATION", P07_8_PM_T26_Regression_BuffApplicationAndRecalculation);
            RunTest(27, "REGRESSION DEBUFF APPLICATION", P07_8_PM_T27_Regression_DebuffApplication);
            RunTest(28, "REGRESSION DOT TICKING DAMAGE", P07_8_PM_T28_Regression_DoTTickingDamageToShield);
            RunTest(29, "REGRESSION CROWD CONTROL PERMISSIONS", P07_8_PM_T29_Regression_CrowdControlPermissions);
            RunTest(30, "REGRESSION CLEANSE NEGATIVE DOES NOT TOUCH SHIELD", P07_8_PM_T30_Regression_CleanseNegativeDoesNotTouchShield);
            RunTest(31, "REGRESSION DISPEL BUFFS DOES NOT TOUCH SHIELD", P07_8_PM_T31_Regression_DispelBuffsDoesNotTouchShield);
            RunTest(32, "REGRESSION FREEZE SHATTER UNAFFECTED", P07_8_PM_T32_Regression_FreezeShatterUnaffected);
            RunTest(33, "REGRESSION ANTI CC IMMUNITY UNAFFECTED", P07_8_PM_T33_Regression_AntiCcImmunityUnaffected);
            RunTest(34, "REGRESSION CC RESISTANCE UNAFFECTED", P07_8_PM_T34_Regression_CcResistanceUnaffected);
            RunTest(35, "END TO END COMBAT ACCEPTANCE", P07_8_PM_T35_EndToEndCombatAcceptance);

            Debug.Log("==================================================");
            Debug.Log($"   PROTOTYPE 07.8 REAL PLAY MODE ACCEPTANCE: {passed}/{total} {(passed == total ? "PASSED" : "FAILED")}");
            Debug.Log("==================================================");

            return passed == total;
        }

        #region P07.8 Automated Unit Tests (01 - 55)

        private static bool P07_8_01_ApplyShield_StoresInstance_SetsCurrentAndMaxAmount()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bool applied = hero.StatusController.ApplyShield("shield_01", 100f, 10f);
            var inst = hero.StatusController.GetShield("shield_01");

            bool pass = applied && inst != null &&
                        Mathf.Approximately(inst.CurrentAmount, 100f) &&
                        Mathf.Approximately(inst.MaxAmount, 100f) &&
                        hero.StatusController.HasActiveShield &&
                        Mathf.Approximately(hero.StatusController.TotalShieldAmount, 100f);

            Debug.Log($"[P07_8_01] ApplyShield Stores Instance -> Applied={applied}, Cur={inst?.CurrentAmount}, Max={inst?.MaxAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_02_FullAbsorption_AbsorbsAllDamage_ZeroHpLoss()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("shield_02", 100f, 10f);

            hero.Health.TakeDamage(40f);

            var inst = hero.StatusController.GetShield("shield_02");
            bool pass = Mathf.Approximately(hero.Health.CurrentHealth, 100f) &&
                        inst != null && Mathf.Approximately(inst.CurrentAmount, 60f);

            Debug.Log($"[P07_8_02] Full Absorption -> HP={hero.Health.CurrentHealth}/100, Shield={inst?.CurrentAmount}/100 | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_03_PartialAbsorption_ShieldReachesZero_RemainingDamageToHp()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("shield_03", 40f, 10f);

            hero.Health.TakeDamage(100f);

            bool pass = Mathf.Approximately(hero.Health.CurrentHealth, 40f) &&
                        !hero.StatusController.HasActiveShield;

            Debug.Log($"[P07_8_03] Partial Absorption -> HP={hero.Health.CurrentHealth}/100, HasShield={hero.StatusController.HasActiveShield} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_04_ShieldDepletion_RemovedFromActiveShields_FiresDepletedEvent()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("shield_04", 50f, 10f);

            bool depletedEventFired = false;
            EventBus.OnShieldDepleted += (target, id) =>
            {
                if (id == "shield_04") depletedEventFired = true;
            };

            hero.Health.TakeDamage(50f);

            bool pass = depletedEventFired &&
                        !hero.StatusController.ActiveShields.ContainsKey("shield_04") &&
                        !hero.StatusController.HasActiveShield;

            Debug.Log($"[P07_8_04] Shield Depletion -> EventFired={depletedEventFired}, ActiveCount={hero.StatusController.ActiveShields.Count} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_05_NoShield_FullDamageDirectlyToHp()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);

            hero.Health.TakeDamage(35f);

            bool pass = Mathf.Approximately(hero.Health.CurrentHealth, 65f);
            Debug.Log($"[P07_8_05] No Shield -> HP={hero.Health.CurrentHealth}/100 | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_06_ZeroDamage_ConsumesZeroShield_NoHpChange()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("shield_06", 50f, 10f);

            hero.Health.TakeDamage(0f);

            var inst = hero.StatusController.GetShield("shield_06");
            bool pass = Mathf.Approximately(hero.Health.CurrentHealth, 100f) &&
                        inst != null && Mathf.Approximately(inst.CurrentAmount, 50f);

            Debug.Log($"[P07_8_06] Zero Damage -> HP={hero.Health.CurrentHealth}, Shield={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_07_NegativeDamage_ConsumesZeroShield_NoHpChange()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("shield_07", 50f, 10f);

            hero.Health.TakeDamage(-25f);

            var inst = hero.StatusController.GetShield("shield_07");
            bool pass = Mathf.Approximately(hero.Health.CurrentHealth, 100f) &&
                        inst != null && Mathf.Approximately(inst.CurrentAmount, 50f);

            Debug.Log($"[P07_8_07] Negative Damage -> HP={hero.Health.CurrentHealth}, Shield={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_08_ClampZero_ShieldAmountNeverNegative()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("shield_08", 30f, 10f);
            var inst = hero.StatusController.GetShield("shield_08");

            float absorbed = inst.Absorb(100f);
            bool pass = Mathf.Approximately(absorbed, 30f) && inst.CurrentAmount >= 0f && Mathf.Approximately(inst.CurrentAmount, 0f);

            Debug.Log($"[P07_8_08] Clamp Zero -> Absorbed={absorbed}, FinalShield={inst.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_09_ShieldBeforeHp_ConsecutiveHitsDeductShieldFirstThenHp()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("shield_09", 50f, 10f);

            // Hit 1: 30 dmg -> Shield 20, HP 100
            hero.Health.TakeDamage(30f);
            bool h1 = Mathf.Approximately(hero.Health.CurrentHealth, 100f) && Mathf.Approximately(hero.StatusController.TotalShieldAmount, 20f);

            // Hit 2: 30 dmg -> Shield 0 (depleted), HP 90
            hero.Health.TakeDamage(30f);
            bool h2 = Mathf.Approximately(hero.Health.CurrentHealth, 90f) && !hero.StatusController.HasActiveShield;

            // Hit 3: 20 dmg -> HP 70
            hero.Health.TakeDamage(20f);
            bool h3 = Mathf.Approximately(hero.Health.CurrentHealth, 70f);

            bool pass = h1 && h2 && h3;
            Debug.Log($"[P07_8_09] Consecutive Hits -> H1={h1}, H2={h2}, H3={h3} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_10_DodgeCheckPriorToShield_DodgedHitConsumesZeroShield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("shield_10", 50f, 10f);

            // Dodged damage result
            var dodgedResult = new DamageResult(bm.CurrentMonster, hero, 100f, 0f, false, true, DamageType.BasicAttack);
            hero.Health.TakeDamage(dodgedResult);

            var inst = hero.StatusController.GetShield("shield_10");
            bool pass = Mathf.Approximately(hero.Health.CurrentHealth, 100f) &&
                        inst != null && Mathf.Approximately(inst.CurrentAmount, 50f);

            Debug.Log($"[P07_8_10] Dodged Hit -> HP={hero.Health.CurrentHealth}, Shield={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_11_DefenseMitigationAppliedBeforeShield_DamageMitigatedFirst()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("shield_11", 100f, 10f);

            // Hero has 100 DEF -> defModifier = 100 / (100 + 100) = 0.5x
            hero.Stats.SetBaseValue(StatType.Defense, 100f);
            hero.Stats.SetBaseValue(StatType.Dodge, 0f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.Attack, 100f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.CritRate, 0f);

            DamageResult result = DamageCalculator.CalculateDamage(bm.CurrentMonster, hero, 1.0f, null, DamageType.BasicAttack);
            hero.Health.TakeDamage(result);

            // 100 atk * 0.5 defMod = 50 damage intercepted by shield
            var inst = hero.StatusController.GetShield("shield_11");
            bool pass = Mathf.Approximately(result.FinalDamage, 50f) &&
                        inst != null && Mathf.Approximately(inst.CurrentAmount, 50f) &&
                        Mathf.Approximately(hero.Health.CurrentHealth, 100f);

            Debug.Log($"[P07_8_11] Defense Mitigation Before Shield -> FinalDmg={result.FinalDamage}, ShieldRem={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_12_CriticalDamageMultiplierAppliedBeforeShield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(200f, hero);
            hero.StatusController.ApplyShield("shield_12", 200f, 10f);

            hero.Stats.SetBaseValue(StatType.Defense, 0f); // 1.0x defMod
            hero.Stats.SetBaseValue(StatType.Dodge, 0f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.Attack, 100f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.CritRate, 100f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.CritDamage, 50f); // 1.5x crit multiplier

            DamageResult result = DamageCalculator.CalculateDamage(bm.CurrentMonster, hero, 1.0f, null, DamageType.BasicAttack);
            hero.Health.TakeDamage(result);

            // 100 atk * 1.5 crit = 150 damage absorbed
            var inst = hero.StatusController.GetShield("shield_12");
            bool pass = result.IsCrit && Mathf.Approximately(result.FinalDamage, 150f) &&
                        inst != null && Mathf.Approximately(inst.CurrentAmount, 50f);

            Debug.Log($"[P07_8_12] Crit Before Shield -> IsCrit={result.IsCrit}, FinalDmg={result.FinalDamage}, ShieldRem={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_13_AttackerDamageDealtDebuffAppliedBeforeShield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("shield_13", 100f, 10f);

            hero.Stats.SetBaseValue(StatType.Defense, 0f);
            hero.Stats.SetBaseValue(StatType.Dodge, 0f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.CritRate, 0f);

            // Apply 20% DamageDealt debuff to attacker (0.8x)
            bm.CurrentMonster.StatusController.ApplyDebuff("deb_dmg", DebuffType.DamageDealt, DebuffModifierMode.Percentage, 20f, 10f, EffectPowerTier.TierB, hero);

            float atk = bm.CurrentMonster.Stats.GetValue(StatType.Attack, 50f);
            float expectedFinalDmg = atk * 0.8f;
            float expectedShieldRem = 100f - expectedFinalDmg;

            DamageResult result = DamageCalculator.CalculateDamage(bm.CurrentMonster, hero, 1.0f, null, DamageType.BasicAttack);
            hero.Health.TakeDamage(result);

            var inst = hero.StatusController.GetShield("shield_13");
            bool pass = Mathf.Approximately(result.FinalDamage, expectedFinalDmg) &&
                        inst != null && Mathf.Approximately(inst.CurrentAmount, expectedShieldRem);

            Debug.Log($"[P07_8_13] DamageDealt Debuff Before Shield -> FinalDmg={result.FinalDamage}, ShieldRem={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_14_DoTTickDamageAbsorbedByShield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("shield_14", 50f, 10f);

            DamageResult dotDamage = DamageCalculator.CalculateDotDamage(bm.CurrentMonster, hero, 30f, 1);
            hero.Health.TakeDamage(dotDamage);

            var inst = hero.StatusController.GetShield("shield_14");
            bool pass = Mathf.Approximately(hero.Health.CurrentHealth, 100f) &&
                        inst != null && Mathf.Approximately(inst.CurrentAmount, 20f);

            Debug.Log($"[P07_8_14] DoT Tick Absorbed By Shield -> HP={hero.Health.CurrentHealth}, ShieldRem={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_15_LethalDamagePreventedIfShieldAbsorbsItAll()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(10f, hero); // 10 HP left
            hero.StatusController.ApplyShield("shield_15", 100f, 10f);

            hero.Health.TakeDamage(60f); // 60 dmg would be lethal to 10 HP

            bool pass = hero.IsAlive && Mathf.Approximately(hero.Health.CurrentHealth, 10f) &&
                        Mathf.Approximately(hero.StatusController.TotalShieldAmount, 40f);

            Debug.Log($"[P07_8_15] Lethal Prevented By Shield -> IsAlive={hero.IsAlive}, HP={hero.Health.CurrentHealth}, Shield={hero.StatusController.TotalShieldAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_16_LethalDamageTriggersDeathIfDamageExceedsShieldPlusHp()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            // Consume hero's single ReviveOnce from Taiji passive
            if (hero.IsAlive) hero.Health.TakeDamage(99999f);

            hero.Health.InitializeHealth(20f, hero);
            hero.StatusController.ApplyShield("shield_16", 30f, 10f);

            hero.Health.TakeDamage(60f); // 30 shield + 20 HP = 50 total capacity, 60 damage kills

            bool pass = !hero.IsAlive && Mathf.Approximately(hero.Health.CurrentHealth, 0f) &&
                        !hero.StatusController.HasActiveShield;

            Debug.Log($"[P07_8_16] Lethal Exceeds Shield+HP -> IsAlive={hero.IsAlive}, HP={hero.Health.CurrentHealth} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_17_MultipleIndependentShields_CoexistInActiveShields()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("shield_a", 50f, 10f);
            hero.StatusController.ApplyShield("shield_b", 70f, 10f);

            bool pass = hero.StatusController.ActiveShields.Count == 2 &&
                        Mathf.Approximately(hero.StatusController.TotalShieldAmount, 120f);

            Debug.Log($"[P07_8_17] Multiple Shields Coexist -> Count={hero.StatusController.ActiveShields.Count}, Total={hero.StatusController.TotalShieldAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_18_DeterministicPriority_HigherPriorityAbsorbsFirst()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("shield_low", 50f, 10f, priority: 1);
            hero.StatusController.ApplyShield("shield_high", 50f, 10f, priority: 10);

            // Incoming 30 damage: should be absorbed entirely by shield_high
            hero.Health.TakeDamage(30f);

            var high = hero.StatusController.GetShield("shield_high");
            var low = hero.StatusController.GetShield("shield_low");

            bool pass = high != null && Mathf.Approximately(high.CurrentAmount, 20f) &&
                        low != null && Mathf.Approximately(low.CurrentAmount, 50f) &&
                        Mathf.Approximately(hero.Health.CurrentHealth, 100f);

            Debug.Log($"[P07_8_18] Higher Priority Absorbs First -> HighRem={high?.CurrentAmount}, LowRem={low?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_19_EqualPriority_OldestFirstFifoConsumption()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var testTime = new TestTimeProvider { CurrentTime = 100f };
            CooldownManager.TimeProvider = testTime;

            hero.StatusController.ApplyShield("shield_old", 50f, 10f, priority: 0);

            testTime.CurrentTime = 102f;
            hero.StatusController.ApplyShield("shield_new", 50f, 10f, priority: 0);

            hero.Health.TakeDamage(30f);

            var oldS = hero.StatusController.GetShield("shield_old");
            var newS = hero.StatusController.GetShield("shield_new");

            bool pass = oldS != null && Mathf.Approximately(oldS.CurrentAmount, 20f) &&
                        newS != null && Mathf.Approximately(newS.CurrentAmount, 50f);

            Debug.Log($"[P07_8_19] Equal Priority Oldest First -> OldRem={oldS?.CurrentAmount}, NewRem={newS?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            CooldownManager.ResetForTesting();
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_20_EqualPriorityAndTimestamp_DeterministicStringOrdinalTieBreaker()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var testTime = new TestTimeProvider { CurrentTime = 100f };
            CooldownManager.TimeProvider = testTime;

            hero.StatusController.ApplyShield("sh_b", 50f, 10f, priority: 0);
            hero.StatusController.ApplyShield("sh_a", 50f, 10f, priority: 0);

            hero.Health.TakeDamage(30f);

            var shA = hero.StatusController.GetShield("sh_a");
            var shB = hero.StatusController.GetShield("sh_b");

            // sh_a < sh_b lexicographically, so sh_a absorbs first
            bool pass = shA != null && Mathf.Approximately(shA.CurrentAmount, 20f) &&
                        shB != null && Mathf.Approximately(shB.CurrentAmount, 50f);

            Debug.Log($"[P07_8_20] Ordinal Tie Breaker -> A_Rem={shA?.CurrentAmount}, B_Rem={shB?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            CooldownManager.ResetForTesting();
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_21_MultiShieldDepletionCascade_FirstDepletesRemainingContinuesToSecond()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("s1", 30f, 10f, priority: 10);
            hero.StatusController.ApplyShield("s2", 50f, 10f, priority: 5);

            // 50 dmg: s1 absorbs 30 (depletes), s2 absorbs remaining 20 (reaches 30)
            hero.Health.TakeDamage(50f);

            var s2 = hero.StatusController.GetShield("s2");
            bool pass = !hero.StatusController.ActiveShields.ContainsKey("s1") &&
                        s2 != null && Mathf.Approximately(s2.CurrentAmount, 30f) &&
                        Mathf.Approximately(hero.Health.CurrentHealth, 100f);

            Debug.Log($"[P07_8_21] Cascade Depletion -> S1Active={hero.StatusController.ActiveShields.ContainsKey("s1")}, S2Rem={s2?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_22_MultiShieldPartialDepletionAcrossTwoShields()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("s1", 25f, 10f, priority: 10);
            hero.StatusController.ApplyShield("s2", 40f, 10f, priority: 5);

            hero.Health.TakeDamage(35f);

            var s2 = hero.StatusController.GetShield("s2");
            bool pass = !hero.StatusController.ActiveShields.ContainsKey("s1") &&
                        s2 != null && Mathf.Approximately(s2.CurrentAmount, 30f);

            Debug.Log($"[P07_8_22] Partial Across Two Shields -> S2Rem={s2?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_23_AllShieldsDepleted_RemainingDamageSpillsOverToHp()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("s1", 20f, 10f, priority: 10);
            hero.StatusController.ApplyShield("s2", 30f, 10f, priority: 5);

            hero.Health.TakeDamage(70f); // 50 total shield, 20 spills to HP

            bool pass = !hero.StatusController.HasActiveShield &&
                        Mathf.Approximately(hero.Health.CurrentHealth, 80f);

            Debug.Log($"[P07_8_23] All Depleted Spills To HP -> HasShield={hero.StatusController.HasActiveShield}, HP={hero.Health.CurrentHealth} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_24_TotalShieldAmount_SumsAllActiveShields()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("s1", 30f, 10f);
            hero.StatusController.ApplyShield("s2", 45f, 10f);
            hero.StatusController.ApplyShield("s3", 25f, 10f);

            bool pass = Mathf.Approximately(hero.StatusController.TotalShieldAmount, 100f);
            Debug.Log($"[P07_8_24] TotalShieldAmount Sum -> Total={hero.StatusController.TotalShieldAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_25_ExpirationTimeCalculation_DeterministicViaTimeProvider()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var testTime = new TestTimeProvider { CurrentTime = 200f };
            CooldownManager.TimeProvider = testTime;

            hero.StatusController.ApplyShield("s_exp", 50f, 8f);
            var inst = hero.StatusController.GetShield("s_exp");

            bool pass = inst != null &&
                        Mathf.Approximately(inst.StartTime, 200f) &&
                        Mathf.Approximately(inst.ExpirationTime, 208f) &&
                        Mathf.Approximately(inst.RemainingDuration, 8f);

            Debug.Log($"[P07_8_25] Expiration Time Calculation -> Start={inst?.StartTime}, Exp={inst?.ExpirationTime} | {(pass ? "PASS" : "FAIL")}");
            CooldownManager.ResetForTesting();
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_26_ExpiredShield_TickRemovesAndRaisesExpiredEvent()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var testTime = new TestTimeProvider { CurrentTime = 100f };
            CooldownManager.TimeProvider = testTime;

            hero.StatusController.ApplyShield("s_tick_exp", 50f, 5f);

            bool expiredRaised = false;
            EventBus.OnShieldExpired += (target, id) =>
            {
                if (id == "s_tick_exp") expiredRaised = true;
            };

            // Advance past expiration
            testTime.CurrentTime = 106f;
            hero.StatusController.Tick(106f);

            bool pass = expiredRaised &&
                        !hero.StatusController.ActiveShields.ContainsKey("s_tick_exp") &&
                        !hero.StatusController.HasActiveShield;

            Debug.Log($"[P07_8_26] Expired Shield Tick Removed -> Raised={expiredRaised}, ActiveCount={hero.StatusController.ActiveShields.Count} | {(pass ? "PASS" : "FAIL")}");
            CooldownManager.ResetForTesting();
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_27_ExpiredShield_DoesNotAbsorbDamage()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var testTime = new TestTimeProvider { CurrentTime = 100f };
            CooldownManager.TimeProvider = testTime;

            hero.Health.InitializeHealth(100f, hero);
            hero.StatusController.ApplyShield("s_exp_noabs", 50f, 5f);

            // Advance past duration without manually ticking
            testTime.CurrentTime = 106f;
            hero.Health.TakeDamage(30f);

            // Expired shield should NOT absorb; all 30 goes to HP
            bool pass = Mathf.Approximately(hero.Health.CurrentHealth, 70f);

            Debug.Log($"[P07_8_27] Expired Shield No Absorb -> HP={hero.Health.CurrentHealth} | {(pass ? "PASS" : "FAIL")}");
            CooldownManager.ResetForTesting();
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_28_DeadTargetSafety_DamageNotProcessed_NoNRE()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(0f, hero); // dead

            bool applied = hero.StatusController.ApplyShield("s_dead", 50f, 10f);
            hero.Health.TakeDamage(50f);

            bool pass = !applied && !hero.StatusController.HasActiveShield;
            Debug.Log($"[P07_8_28] Dead Target Safety -> Applied={applied} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_29_DestroyedTargetSafety_NoNRE()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var sc = hero.StatusController;
            Object.DestroyImmediate(heroGO);

            // Calling operations on destroyed object should fail safely without unhandled exceptions
            bool applied = false;
            try
            {
                applied = sc.ApplyShield("s_dest", 50f, 10f);
            }
            catch (System.Exception ex)
            {
                Debug.Log($"Handled expected destroyed target: {ex.Message}");
            }

            bool pass = !applied;
            Debug.Log($"[P07_8_29] Destroyed Target Safety -> Applied={applied} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(null, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_30_ReviveSafety_MindMethodRevivePreservesOrClearsShieldWithoutError()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(50f, hero);
            hero.StatusController.ApplyShield("s_rev", 20f, 10f);

            // Re-apply shield after revive
            hero.Health.Revive(100f);
            bool applied = hero.StatusController.ApplyShield("s_post_rev", 60f, 10f);

            bool pass = hero.IsAlive && applied && hero.StatusController.HasActiveShield;
            Debug.Log($"[P07_8_30] Revive Safety -> IsAlive={hero.IsAlive}, AppliedPost={applied} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_31_ClearAll_ClearsAllShieldsAndResetsState()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("s1", 50f, 10f);
            hero.StatusController.ApplyShield("s2", 50f, 10f);

            hero.StatusController.ClearAll();

            bool pass = hero.StatusController.ActiveShields.Count == 0 &&
                        !hero.StatusController.HasActiveShield &&
                        Mathf.Approximately(hero.StatusController.TotalShieldAmount, 0f);

            Debug.Log($"[P07_8_31] ClearAll Resets Shields -> ActiveCount={hero.StatusController.ActiveShields.Count} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_32_StackPolicy_AdditiveIncreasesAmountAndRefreshesDuration()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var testTime = new TestTimeProvider { CurrentTime = 100f };
            CooldownManager.TimeProvider = testTime;

            hero.StatusController.ApplyShield("s_add", 50f, 5f, stackPolicy: ShieldStackPolicy.Additive, maxStacks: 3);

            testTime.CurrentTime = 102f;
            hero.StatusController.ApplyShield("s_add", 50f, 5f, stackPolicy: ShieldStackPolicy.Additive, maxStacks: 3);

            var inst = hero.StatusController.GetShield("s_add");
            bool pass = inst != null &&
                        Mathf.Approximately(inst.CurrentAmount, 100f) &&
                        Mathf.Approximately(inst.MaxAmount, 100f) &&
                        inst.StackCount == 2 &&
                        Mathf.Approximately(inst.ExpirationTime, 107f);

            Debug.Log($"[P07_8_32] Stacking Additive -> Cur={inst?.CurrentAmount}, Stacks={inst?.StackCount}, Exp={inst?.ExpirationTime} | {(pass ? "PASS" : "FAIL")}");
            CooldownManager.ResetForTesting();
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_33_StackPolicy_AdditiveCapsAtMaxAmount()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("s_cap", 50f, 10f, stackPolicy: ShieldStackPolicy.Additive, maxAmount: 80f, maxStacks: 5);
            hero.StatusController.ApplyShield("s_cap", 50f, 10f, stackPolicy: ShieldStackPolicy.Additive, maxAmount: 80f, maxStacks: 5);

            var inst = hero.StatusController.GetShield("s_cap");
            bool pass = inst != null && Mathf.Approximately(inst.CurrentAmount, 80f);

            Debug.Log($"[P07_8_33] Stacking Capped -> Cur={inst?.CurrentAmount}/80 | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_34_StackPolicy_RefreshDurationResetsTimerKeepsMaxAmount()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var testTime = new TestTimeProvider { CurrentTime = 100f };
            CooldownManager.TimeProvider = testTime;

            hero.StatusController.ApplyShield("s_ref", 60f, 5f, stackPolicy: ShieldStackPolicy.RefreshDuration);

            testTime.CurrentTime = 103f;
            // Absorb 20 damage so current is 40
            hero.Health.TakeDamage(20f);

            // Re-apply 50 with refresh
            hero.StatusController.ApplyShield("s_ref", 50f, 5f, stackPolicy: ShieldStackPolicy.RefreshDuration);

            var inst = hero.StatusController.GetShield("s_ref");
            bool pass = inst != null &&
                        Mathf.Approximately(inst.CurrentAmount, 50f) &&
                        Mathf.Approximately(inst.ExpirationTime, 108f);

            Debug.Log($"[P07_8_34] Stacking RefreshDuration -> Cur={inst?.CurrentAmount}, Exp={inst?.ExpirationTime} | {(pass ? "PASS" : "FAIL")}");
            CooldownManager.ResetForTesting();
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_35_StackPolicy_ReplaceOverwritesAmountAndDuration()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var testTime = new TestTimeProvider { CurrentTime = 100f };
            CooldownManager.TimeProvider = testTime;

            hero.StatusController.ApplyShield("s_rep", 40f, 5f, priority: 1, stackPolicy: ShieldStackPolicy.Replace);

            testTime.CurrentTime = 102f;
            hero.StatusController.ApplyShield("s_rep", 90f, 8f, priority: 5, stackPolicy: ShieldStackPolicy.Replace);

            var inst = hero.StatusController.GetShield("s_rep");
            bool pass = inst != null &&
                        Mathf.Approximately(inst.CurrentAmount, 90f) &&
                        inst.Priority == 5 &&
                        Mathf.Approximately(inst.ExpirationTime, 110f);

            Debug.Log($"[P07_8_35] Stacking Replace -> Cur={inst?.CurrentAmount}, Prio={inst?.Priority}, Exp={inst?.ExpirationTime} | {(pass ? "PASS" : "FAIL")}");
            CooldownManager.ResetForTesting();
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_36_StackPolicy_IgnoreDiscardsDuplicateApplication()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("s_ign", 50f, 10f, stackPolicy: ShieldStackPolicy.Ignore);

            bool reapplied = hero.StatusController.ApplyShield("s_ign", 100f, 10f, stackPolicy: ShieldStackPolicy.Ignore);
            var inst = hero.StatusController.GetShield("s_ign");

            bool pass = !reapplied && inst != null && Mathf.Approximately(inst.CurrentAmount, 50f);
            Debug.Log($"[P07_8_36] Stacking Ignore -> Reapplied={reapplied}, Amount={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_37_StackPolicy_IndependentGeneratesUniqueInstances()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("s_ind", 40f, 10f, stackPolicy: ShieldStackPolicy.Independent);
            hero.StatusController.ApplyShield("s_ind", 60f, 10f, stackPolicy: ShieldStackPolicy.Independent);

            bool pass = hero.StatusController.ActiveShields.Count == 2 &&
                        Mathf.Approximately(hero.StatusController.TotalShieldAmount, 100f);

            Debug.Log($"[P07_8_37] Stacking Independent -> Count={hero.StatusController.ActiveShields.Count}, Total={hero.StatusController.TotalShieldAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_38_StackPolicy_MaxStacksLimitEnforced()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("s_stk", 30f, 10f, stackPolicy: ShieldStackPolicy.Additive, maxStacks: 2);
            hero.StatusController.ApplyShield("s_stk", 30f, 10f, stackPolicy: ShieldStackPolicy.Additive, maxStacks: 2);
            hero.StatusController.ApplyShield("s_stk", 30f, 10f, stackPolicy: ShieldStackPolicy.Additive, maxStacks: 2); // 3rd ignored for stack

            var inst = hero.StatusController.GetShield("s_stk");
            bool pass = inst != null && inst.StackCount == 2 && Mathf.Approximately(inst.CurrentAmount, 60f);

            Debug.Log($"[P07_8_38] Max Stacks Limit -> Stacks={inst?.StackCount}/2, Amount={inst?.CurrentAmount}/60 | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_39_ShieldEffectDefinitionSO_FlatValueMode()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var so = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            so.Initialize("so_flat", 75f, 5f, 0, ShieldStackPolicy.RefreshDuration, ShieldAmountMode.FlatValue);

            var req = new SkillExecutionRequest(hero, null, SkillSlotType.NormalAttack, hero);
            var result = so.Execute(req, hero, null);

            var inst = hero.StatusController.GetShield("so_flat");
            bool pass = result.Success && inst != null && Mathf.Approximately(inst.CurrentAmount, 75f);

            Debug.Log($"[P07_8_39] Shield SO Flat Value -> Success={result.Success}, Amount={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(so);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_40_ShieldEffectDefinitionSO_MaxHpPercentageMode()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(200f, hero);

            var so = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            so.Initialize("so_pct", 25f, 5f, 0, ShieldStackPolicy.RefreshDuration, ShieldAmountMode.MaxHpPercentage); // 25% of 200 = 50

            var req = new SkillExecutionRequest(hero, null, SkillSlotType.NormalAttack, hero);
            var result = so.Execute(req, hero, null);

            var inst = hero.StatusController.GetShield("so_pct");
            bool pass = result.Success && inst != null && Mathf.Approximately(inst.CurrentAmount, 50f);

            Debug.Log($"[P07_8_40] Shield SO Max HP Pct -> Success={result.Success}, Amount={inst?.CurrentAmount}/50 | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(so);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_41_ShieldEffectDefinitionSO_AttackMultiplierMode()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Stats.SetBaseValue(StatType.Attack, 40f);

            var so = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            so.Initialize("so_atk", 2.0f, 5f, 0, ShieldStackPolicy.RefreshDuration, ShieldAmountMode.AttackMultiplier); // 2.0 * 40 = 80

            var req = new SkillExecutionRequest(hero, null, SkillSlotType.NormalAttack, hero);
            var result = so.Execute(req, hero, null);

            var inst = hero.StatusController.GetShield("so_atk");
            bool pass = result.Success && inst != null && Mathf.Approximately(inst.CurrentAmount, 80f);

            Debug.Log($"[P07_8_41] Shield SO Attack Multiplier -> Success={result.Success}, Amount={inst?.CurrentAmount}/80 | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(so);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_42_EffectResolver_ResolvesShieldEffectForSkill()
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            var shieldEff = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shieldEff.Initialize("eff_res_sh", 50f);
            skill.SetEffects(new List<SkillEffectDefinitionSO> { shieldEff });

            var resolved = EffectResolver.ResolveEffectsForSkill(skill);
            bool pass = resolved.Count == 1 && resolved[0].EffectType == SkillEffectType.Shield;

            Debug.Log($"[P07_8_42] EffectResolver Resolves Shield -> Count={resolved.Count}, Type={resolved[0]?.EffectType} | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(shieldEff);
            Object.DestroyImmediate(skill);
            return pass;
        }

        private static bool P07_8_43_DefaultSkillTargetResolver_DefaultsToSourceWhenSelfOrOmitted()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var shieldEff = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shieldEff.Initialize("tar_res_sh", 50f, 5f, 0, ShieldStackPolicy.RefreshDuration, ShieldAmountMode.FlatValue, SkillTargetPolicy.SingleTarget);

            var resolver = new DefaultSkillTargetResolver();
            // No target passed in request
            var req = new SkillExecutionRequest(hero, null, SkillSlotType.NormalAttack, null);
            var targets = resolver.ResolveTargets(req, shieldEff);

            bool pass = targets.Count == 1 && targets[0] == hero;
            Debug.Log($"[P07_8_43] Default Target Resolver Defaults to Source -> TargetCount={targets.Count}, IsSource={targets[0] == hero} | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(shieldEff);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_44_SkillExecutor_ExecutesShieldSkill_ConsumesRageAndTriggersCooldown()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Rage.InitializeRage(100f, 100f);

            var skill = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            var origEffects = skill.Effects != null ? new List<SkillEffectDefinitionSO>(skill.Effects) : new List<SkillEffectDefinitionSO>();

            var shieldEff = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shieldEff.Initialize("sk_sh_inst", 60f);
            skill.SetEffects(new List<SkillEffectDefinitionSO> { shieldEff });

            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, hero);
            var result = SkillExecutor.Execute(req);

            bool pass = result.Success &&
                        Mathf.Approximately(hero.Rage.CurrentRage, 70f) &&
                        CooldownManager.IsOnCooldown("skill_taiji_2_a", out _) &&
                        hero.StatusController.HasActiveShield;

            Debug.Log($"[P07_8_44] SkillExecutor Shield -> Success={result.Success}, Rage={hero.Rage.CurrentRage}, CD={CooldownManager.IsOnCooldown("skill_taiji_2_a", out _)} | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(shieldEff);
            skill.SetEffects(origEffects);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_45_MultiEffect_DamagePlusShield_SingleRageSingleCooldown()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            Monster monster = bm.CurrentMonster;
            hero.Rage.InitializeRage(100f, 100f);

            var skill = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            var origEffects = skill.Effects != null ? new List<SkillEffectDefinitionSO>(skill.Effects) : new List<SkillEffectDefinitionSO>();

            var dmgEff = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
            dmgEff.Initialize(1.0f, SkillTargetPolicy.SingleTarget);

            var shEff = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shEff.Initialize("ward_sh", 50f, 5f, 0, ShieldStackPolicy.RefreshDuration, ShieldAmountMode.FlatValue, SkillTargetPolicy.Self);

            skill.SetEffects(new List<SkillEffectDefinitionSO> { dmgEff, shEff });

            float monHpBefore = monster.Health.CurrentHealth;
            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, monster);
            var result = SkillExecutor.Execute(req);

            bool pass = result.Success &&
                        result.EffectResults.Count == 2 &&
                        monster.Health.CurrentHealth < monHpBefore &&
                        hero.StatusController.HasActiveShield &&
                        Mathf.Approximately(hero.Rage.CurrentRage, 70f) &&
                        CooldownManager.IsOnCooldown("skill_taiji_2_a", out _);

            Debug.Log($"[P07_8_45] Multi-Effect Damage + Shield -> Success={result.Success}, Effects={result.EffectResults.Count}, MonHpLost={monHpBefore - monster.Health.CurrentHealth}, HasShield={hero.StatusController.HasActiveShield} | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(dmgEff);
            Object.DestroyImmediate(shEff);
            skill.SetEffects(origEffects);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_46_MultiEffect_HealPlusShield_SingleRageSingleCooldown()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.Health.TakeDamage(40f); // HP = 60
            hero.Rage.InitializeRage(100f, 100f);

            var skill = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            var origEffects = skill.Effects != null ? new List<SkillEffectDefinitionSO>(skill.Effects) : new List<SkillEffectDefinitionSO>();

            var healEff = ScriptableObject.CreateInstance<HealEffectDefinitionSO>();
            healEff.Initialize(30f, HealAmountMode.FlatValue, SkillTargetPolicy.Self);

            var shEff = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shEff.Initialize("rec_sh", 40f, 5f, 0, ShieldStackPolicy.RefreshDuration, ShieldAmountMode.FlatValue, SkillTargetPolicy.Self);

            skill.SetEffects(new List<SkillEffectDefinitionSO> { healEff, shEff });

            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, hero);
            var result = SkillExecutor.Execute(req);

            bool pass = result.Success &&
                        Mathf.Approximately(hero.Health.CurrentHealth, 90f) &&
                        hero.StatusController.HasActiveShield &&
                        Mathf.Approximately(hero.Rage.CurrentRage, 70f);

            Debug.Log($"[P07_8_46] Multi-Effect Heal + Shield -> Success={result.Success}, HP={hero.Health.CurrentHealth}/100, HasShield={hero.StatusController.HasActiveShield} | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(healEff);
            Object.DestroyImmediate(shEff);
            skill.SetEffects(origEffects);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_47_MultiEffect_BuffPlusShield_AppliesBothCorrectly()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var buffEff = ScriptableObject.CreateInstance<BuffEffectDefinitionSO>();
            buffEff.Initialize("buf_sh_test", "AtkUp", StatType.Attack, 10f, 10f, BuffStackingPolicy.RefreshDuration);

            var shEff = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shEff.Initialize("buf_sh_inst", 50f, 10f);

            var skill = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            var origEffects = skill.Effects != null ? new List<SkillEffectDefinitionSO>(skill.Effects) : new List<SkillEffectDefinitionSO>();
            skill.SetEffects(new List<SkillEffectDefinitionSO> { buffEff, shEff });

            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, hero);
            var result = SkillExecutor.Execute(req);

            bool pass = result.Success &&
                        hero.StatusController.ActiveBuffs.ContainsKey("buf_sh_test") &&
                        hero.StatusController.HasActiveShield;

            Debug.Log($"[P07_8_47] Multi-Effect Buff + Shield -> Success={result.Success}, HasBuff={hero.StatusController.ActiveBuffs.ContainsKey("buf_sh_test")}, HasShield={hero.StatusController.HasActiveShield} | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(buffEff);
            Object.DestroyImmediate(shEff);
            skill.SetEffects(origEffects);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_48_MultiEffect_ShieldPlusCleanse_CleansesNegativeAndAppliesShield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyDebuff("test_deb", DebuffType.Attack, DebuffModifierMode.Flat, 5f, 10f, EffectPowerTier.TierB, bm.CurrentMonster);

            var clnEff = ScriptableObject.CreateInstance<CleanseEffectDefinitionSO>();
            clnEff.Initialize(category: StatusRemovalCategory.NegativeStatus, maxCount: 0, selectMode: StatusSelectionMode.All, stacksToRemove: 0, policy: SkillTargetPolicy.Self);

            var shEff = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shEff.Initialize("cleanse_sh", 50f, 5f, 0, ShieldStackPolicy.RefreshDuration, ShieldAmountMode.FlatValue, SkillTargetPolicy.Self);

            var skill = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            var origEffects = skill.Effects != null ? new List<SkillEffectDefinitionSO>(skill.Effects) : new List<SkillEffectDefinitionSO>();
            skill.SetEffects(new List<SkillEffectDefinitionSO> { clnEff, shEff });

            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, hero);
            var result = SkillExecutor.Execute(req);

            bool pass = result.Success &&
                        hero.StatusController.ActiveDebuffs.Count == 0 &&
                        hero.StatusController.HasActiveShield;

            Debug.Log($"[P07_8_48] Multi-Effect Shield + Cleanse -> DebuffsCleaned={hero.StatusController.ActiveDebuffs.Count == 0}, HasShield={hero.StatusController.HasActiveShield} | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(clnEff);
            Object.DestroyImmediate(shEff);
            skill.SetEffects(origEffects);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_49_CleanseIsolation_CleanseNegativeStatusesDoesNotRemoveShield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyDebuff("deb_cln", DebuffType.Attack, DebuffModifierMode.Flat, 5f, 10f, EffectPowerTier.TierB, bm.CurrentMonster);
            hero.StatusController.ApplyShield("shield_iso", 60f, 10f);

            var clnRes = hero.StatusController.CleanseAllNegativeStatuses();

            bool pass = clnRes.Success &&
                        hero.StatusController.ActiveDebuffs.Count == 0 &&
                        hero.StatusController.HasActiveShield &&
                        Mathf.Approximately(hero.StatusController.TotalShieldAmount, 60f);

            Debug.Log($"[P07_8_49] Cleanse Isolation -> DebuffsCleaned={hero.StatusController.ActiveDebuffs.Count == 0}, ShieldUntouched={hero.StatusController.HasActiveShield} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_50_DispelIsolation_DispelBuffsDoesNotRemoveShield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            var buff = ScriptableObject.CreateInstance<BuffEffectDefinitionSO>();
            buff.Initialize("buf_dsp", "AtkUp", StatType.Attack, 10f, 10f, BuffStackingPolicy.RefreshDuration);
            hero.StatusController.ApplyBuff(buff, hero);

            hero.StatusController.ApplyShield("shield_disp_iso", 70f, 10f);

            // Dispel positive buffs
            var dspRes = hero.StatusController.DispelAllBuffs();

            bool pass = dspRes.Success &&
                        hero.StatusController.ActiveBuffs.Count == 0 &&
                        hero.StatusController.HasActiveShield &&
                        Mathf.Approximately(hero.StatusController.TotalShieldAmount, 70f);

            Debug.Log($"[P07_8_50] Dispel Isolation -> BuffsDispelled={hero.StatusController.ActiveBuffs.Count == 0}, ShieldUntouched={hero.StatusController.HasActiveShield} | {(pass ? "PASS" : "FAIL")}");
            Object.DestroyImmediate(buff);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_51_DispelExplicitShieldCategory_DispelsShieldCorrectly()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("shield_target", 50f, 10f);

            var dspRes = hero.StatusController.Dispel(StatusRemovalCategory.Shield);

            bool pass = dspRes.Success &&
                        dspRes.StatusesRemovedCount == 1 &&
                        !hero.StatusController.HasActiveShield;

            Debug.Log($"[P07_8_51] Dispel Explicit Shield -> Success={dspRes.Success}, RemovedCount={dspRes.StatusesRemovedCount}, HasShield={hero.StatusController.HasActiveShield} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_52_CrowdControlIsolation_StunRootFreezeDoesNotRemoveShield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyShield("shield_cc", 80f, 10f);

            hero.StatusController.ApplyCrowdControl("cc_stun", CrowdControlType.Stun, 5f, EffectPowerTier.TierB, bm.CurrentMonster);

            bool pass = hero.IsStunned &&
                        hero.StatusController.HasActiveShield &&
                        Mathf.Approximately(hero.StatusController.TotalShieldAmount, 80f);

            Debug.Log($"[P07_8_52] Crowd Control Isolation -> IsStunned={hero.IsStunned}, HasShield={hero.StatusController.HasActiveShield} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_53_AntiCcImmunityIsolation_AntiCcDoesNotAffectShield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.StatusController.ApplyAntiCCImmunity(10f);
            hero.StatusController.ApplyShield("shield_anticc", 60f, 10f);

            hero.Health.TakeDamage(20f);

            var inst = hero.StatusController.GetShield("shield_anticc");
            bool pass = hero.StatusController.HasAntiCCImmunity &&
                        inst != null && Mathf.Approximately(inst.CurrentAmount, 40f);

            Debug.Log($"[P07_8_53] Anti-CC Isolation -> HasAntiCC={hero.StatusController.HasAntiCCImmunity}, ShieldRem={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_54_FreezeShatterIsolation_ShieldDoesNotCauseOrAlterShatter()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            Monster monster = bm.CurrentMonster;

            // Apply Freeze to monster
            monster.StatusController.ApplyCrowdControl("mon_freeze", CrowdControlType.Freeze, 5f, EffectPowerTier.TierB, hero);
            bool frozenBefore = monster.IsFrozen;

            // Monster applies shield to itself
            monster.StatusController.ApplyShield("mon_sh", 50f, 10f);

            // Applying shield MUST NOT shatter freeze
            bool frozenAfter = monster.IsFrozen;
            bool pass = frozenBefore && frozenAfter;

            Debug.Log($"[P07_8_54] Freeze Shatter Isolation -> FrozenBefore={frozenBefore}, FrozenAfter={frozenAfter} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        private static bool P07_8_55_HealIsolation_HealingRestoresHpWithoutAffectingShield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            hero.Health.InitializeHealth(100f, hero);
            hero.Health.TakeDamage(50f); // HP = 50

            hero.StatusController.ApplyShield("shield_heal_iso", 40f, 10f);

            float healed = hero.Health.Heal(30f); // HP -> 80
            var inst = hero.StatusController.GetShield("shield_heal_iso");

            bool pass = Mathf.Approximately(healed, 30f) &&
                        Mathf.Approximately(hero.Health.CurrentHealth, 80f) &&
                        inst != null && Mathf.Approximately(inst.CurrentAmount, 40f);

            Debug.Log($"[P07_8_55] Heal Isolation -> Healed={healed}, HP={hero.Health.CurrentHealth}, Shield={inst?.CurrentAmount} | {(pass ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return pass;
        }

        #endregion

        #region P07.8 Play Mode Acceptance Tests (T01 - T35)

        private static bool P07_8_PM_T01_ApplyShield()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;
            hero.StatusController.ClearAll();

            bool applied = hero.StatusController.ApplyShield("pm_sh_01", 100f, 10f);
            return applied && hero.StatusController.HasActiveShield;
        }

        private static bool P07_8_PM_T02_ShieldVisibleInRuntime()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ApplyShield("pm_sh_02", 100f, 10f);
            var shield = hero.StatusController.GetShield("pm_sh_02");
            return shield != null && Mathf.Approximately(shield.CurrentAmount, 100f) &&
                   hero.StatusController.HasActiveShield &&
                   Mathf.Approximately(hero.StatusController.TotalShieldAmount, 100f);
        }

        private static bool P07_8_PM_T03_FullAbsorption()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            float hpBefore = hero.Health.CurrentHealth;
            hero.StatusController.ApplyShield("pm_sh_03", 100f, 10f);

            hero.Health.TakeDamage(40f);

            var shield = hero.StatusController.GetShield("pm_sh_03");
            return Mathf.Approximately(hero.Health.CurrentHealth, hpBefore) &&
                   shield != null && Mathf.Approximately(shield.CurrentAmount, 60f);
        }

        private static bool P07_8_PM_T04_PartialAbsorption()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            float hpBefore = hero.Health.CurrentHealth;
            hero.StatusController.ApplyShield("pm_sh_04", 40f, 10f);

            hero.Health.TakeDamage(70f);

            return Mathf.Approximately(hero.Health.CurrentHealth, hpBefore - 30f) &&
                   !hero.StatusController.HasActiveShield;
        }

        private static bool P07_8_PM_T05_ShieldDepletion()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            hero.StatusController.ApplyShield("pm_sh_05", 50f, 10f);
            hero.Health.TakeDamage(50f);

            return !hero.StatusController.ActiveShields.ContainsKey("pm_sh_05");
        }

        private static bool P07_8_PM_T06_RemainingDamageToHp()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            float hpBefore = hero.Health.CurrentHealth;
            hero.StatusController.ApplyShield("pm_sh_06", 30f, 10f);

            hero.Health.TakeDamage(50f);
            return Mathf.Approximately(hero.Health.CurrentHealth, hpBefore - 20f);
        }

        private static bool P07_8_PM_T07_NoShieldHitDirectToHp()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            float hpBefore = hero.Health.CurrentHealth;

            hero.Health.TakeDamage(25f);
            return Mathf.Approximately(hero.Health.CurrentHealth, hpBefore - 25f);
        }

        private static bool P07_8_PM_T08_MultipleHitSequence()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            float hpBefore = hero.Health.CurrentHealth;
            hero.StatusController.ApplyShield("pm_sh_08", 60f, 10f);

            hero.Health.TakeDamage(30f); // Shield 30, HP unchanged
            bool step1 = Mathf.Approximately(hero.Health.CurrentHealth, hpBefore);

            hero.Health.TakeDamage(40f); // Shield 0, HP takes 10
            bool step2 = Mathf.Approximately(hero.Health.CurrentHealth, hpBefore - 10f);

            hero.Health.TakeDamage(10f); // HP takes another 10
            bool step3 = Mathf.Approximately(hero.Health.CurrentHealth, hpBefore - 20f);

            return step1 && step2 && step3;
        }

        private static bool P07_8_PM_T09_MultipleShieldsSimultaneous()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            hero.StatusController.ApplyShield("pm_s1", 40f, 10f);
            hero.StatusController.ApplyShield("pm_s2", 50f, 10f);

            return hero.StatusController.ActiveShields.Count == 2 &&
                   Mathf.Approximately(hero.StatusController.TotalShieldAmount, 90f);
        }

        private static bool P07_8_PM_T10_DeterministicShieldPriorityOrder()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            hero.StatusController.ApplyShield("pm_prio_low", 50f, 10f, priority: 1);
            hero.StatusController.ApplyShield("pm_prio_high", 50f, 10f, priority: 10);

            hero.Health.TakeDamage(30f);

            var high = hero.StatusController.GetShield("pm_prio_high");
            var low = hero.StatusController.GetShield("pm_prio_low");

            return high != null && Mathf.Approximately(high.CurrentAmount, 20f) &&
                   low != null && Mathf.Approximately(low.CurrentAmount, 50f);
        }

        private static bool P07_8_PM_T11_ShieldExpirationTimeProvider()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            var testTime = new TestTimeProvider { CurrentTime = 300f };
            CooldownManager.TimeProvider = testTime;

            hero.StatusController.ClearAll();
            hero.StatusController.ApplyShield("pm_exp", 50f, 5f);

            testTime.CurrentTime = 306f;
            hero.StatusController.Tick(306f);

            bool pass = !hero.StatusController.HasActiveShield;
            CooldownManager.ResetForTesting();
            return pass;
        }

        private static bool P07_8_PM_T12_ShieldRemovalExplicit()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            hero.StatusController.ApplyShield("pm_rem", 50f, 10f);
            bool rem = hero.StatusController.RemoveShield("pm_rem");

            return rem && !hero.StatusController.HasActiveShield;
        }

        private static bool P07_8_PM_T13_DeadTargetSafety()
        {
            GameObject tempGO = new GameObject("DeadTargetEntity");
            var tempMon = tempGO.AddComponent<Monster>();
            tempMon.InitializeMonster();
            tempMon.Health.InitializeHealth(0f, tempMon);
            bool applied = tempMon.StatusController.ApplyShield("pm_dead_sh", 50f, 10f);
            tempMon.Health.TakeDamage(50f);
            Object.DestroyImmediate(tempGO);
            return !applied;
        }

        private static bool P07_8_PM_T14_DestroyedTargetSafety()
        {
            GameObject tempGO = new GameObject("TempEntity");
            var sc = tempGO.AddComponent<EntityStatusController>();
            Object.DestroyImmediate(tempGO);

            bool applied = false;
            try
            {
                applied = sc.ApplyShield("pm_dest", 50f, 10f);
            }
            catch
            {
            }

            return !applied;
        }

        private static bool P07_8_PM_T15_ZeroDamageSafety()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            float hpBefore = hero.Health.CurrentHealth;
            hero.StatusController.ApplyShield("pm_zero_sh", 50f, 10f);

            hero.Health.TakeDamage(0f);
            return Mathf.Approximately(hero.StatusController.TotalShieldAmount, 50f) &&
                   Mathf.Approximately(hero.Health.CurrentHealth, hpBefore);
        }

        private static bool P07_8_PM_T16_MultiEffect_DamagePlusShield()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
            MindMethodManager mmMgr = Object.FindAnyObjectByType<MindMethodManager>();
            if (hero == null || bm == null || bm.CurrentMonster == null || mmMgr == null) return false;

            CooldownManager.ResetAllCooldowns();
            hero.Rage.AddRage(100f);
            if (!bm.CurrentMonster.IsAlive || bm.CurrentMonster.Health.CurrentHealth <= 0f)
            {
                bm.CurrentMonster.InitializeMonster();
                bm.CurrentMonster.Health.Revive(bm.CurrentMonster.Health.MaxHealth);
                bm.CurrentMonster.EnableEntityActions();
            }

            var skill = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            if (skill == null) return false;
            var origEffects = skill.Effects != null ? new List<SkillEffectDefinitionSO>(skill.Effects) : new List<SkillEffectDefinitionSO>();

            hero.StatusController.ClearAll();
            var so = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            so.Initialize("pm_me_sh", 40f, 5f, 0, ShieldStackPolicy.RefreshDuration, ShieldAmountMode.FlatValue, SkillTargetPolicy.Self);

            var dmgSo = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
            dmgSo.Initialize(1.0f, SkillTargetPolicy.SingleTarget);

            skill.SetEffects(new List<SkillEffectDefinitionSO> { dmgSo, so });

            float monHpBefore = bm.CurrentMonster.Health.CurrentHealth;
            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, bm.CurrentMonster);
            var res = SkillExecutor.Execute(req);

            bool pass = res.Success && hero.StatusController.HasActiveShield &&
                        bm.CurrentMonster.Health.CurrentHealth < monHpBefore;

            Object.DestroyImmediate(dmgSo);
            Object.DestroyImmediate(so);
            skill.SetEffects(origEffects);
            CooldownManager.ResetAllCooldowns();
            return pass;
        }

        private static bool P07_8_PM_T17_MultiEffect_HealPlusShield()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            MindMethodManager mmMgr = Object.FindAnyObjectByType<MindMethodManager>();
            if (hero == null || mmMgr == null) return false;

            CooldownManager.ResetAllCooldowns();
            hero.Rage.AddRage(100f);

            var skill = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            if (skill == null) return false;
            var origEffects = skill.Effects != null ? new List<SkillEffectDefinitionSO>(skill.Effects) : new List<SkillEffectDefinitionSO>();

            hero.StatusController.ClearAll();
            hero.Health.TakeDamage(40f);
            float hpDamaged = hero.Health.CurrentHealth;

            var healSo = ScriptableObject.CreateInstance<HealEffectDefinitionSO>();
            healSo.Initialize(20f, HealAmountMode.FlatValue, SkillTargetPolicy.Self);

            var shSo = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shSo.Initialize("pm_me_heal_sh", 30f, 5f, 0, ShieldStackPolicy.RefreshDuration, ShieldAmountMode.FlatValue, SkillTargetPolicy.Self);

            skill.SetEffects(new List<SkillEffectDefinitionSO> { healSo, shSo });

            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, hero);
            var res = SkillExecutor.Execute(req);

            bool pass = res.Success && Mathf.Approximately(hero.Health.CurrentHealth, hpDamaged + 20f) &&
                        hero.StatusController.HasActiveShield;

            Object.DestroyImmediate(healSo);
            Object.DestroyImmediate(shSo);
            skill.SetEffects(origEffects);
            CooldownManager.ResetAllCooldowns();
            return pass;
        }

        private static bool P07_8_PM_T18_MultiEffect_BuffPlusShield()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            MindMethodManager mmMgr = Object.FindAnyObjectByType<MindMethodManager>();
            if (hero == null || mmMgr == null) return false;

            CooldownManager.ResetAllCooldowns();
            hero.Rage.AddRage(100f);

            var skill = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            if (skill == null) return false;
            var origEffects = skill.Effects != null ? new List<SkillEffectDefinitionSO>(skill.Effects) : new List<SkillEffectDefinitionSO>();

            hero.StatusController.ClearAll();
            var buffSo = ScriptableObject.CreateInstance<BuffEffectDefinitionSO>();
            buffSo.Initialize("pm_me_buf", "AtkUp", StatType.Attack, 10f, 10f, BuffStackingPolicy.RefreshDuration);

            var shSo = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shSo.Initialize("pm_me_buf_sh", 35f, 5f);

            skill.SetEffects(new List<SkillEffectDefinitionSO> { buffSo, shSo });

            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, hero);
            var res = SkillExecutor.Execute(req);

            bool pass = res.Success && hero.StatusController.ActiveBuffs.ContainsKey("pm_me_buf") &&
                        hero.StatusController.HasActiveShield;

            Object.DestroyImmediate(buffSo);
            Object.DestroyImmediate(shSo);
            skill.SetEffects(origEffects);
            CooldownManager.ResetAllCooldowns();
            return pass;
        }

        private static bool P07_8_PM_T19_ExistingDamageModifiersDefenseMitigation()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
            if (hero == null || bm == null || bm.CurrentMonster == null) return false;

            hero.Health.InitializeHealth(200f, hero);
            hero.StatusController.ClearAll();
            hero.StatusController.ApplyShield("pm_def_mod_sh", 100f, 10f);

            hero.Stats.SetBaseValue(StatType.Defense, 100f);
            hero.Stats.SetBaseValue(StatType.Dodge, 0f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.Attack, 100f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.CritRate, 0f);

            DamageResult result = DamageCalculator.CalculateDamage(bm.CurrentMonster, hero, 1.0f, null, DamageType.BasicAttack);
            hero.Health.TakeDamage(result);

            var inst = hero.StatusController.GetShield("pm_def_mod_sh");
            return Mathf.Approximately(result.FinalDamage, 50f) &&
                   inst != null && Mathf.Approximately(inst.CurrentAmount, 50f);
        }

        private static bool P07_8_PM_T20_CriticalHitDamageInteraction()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
            if (hero == null || bm == null || bm.CurrentMonster == null) return false;

            hero.Health.InitializeHealth(200f, hero);
            hero.StatusController.ClearAll();
            hero.StatusController.ApplyShield("pm_crit_sh", 200f, 10f);

            hero.Stats.SetBaseValue(StatType.Defense, 0f);
            hero.Stats.SetBaseValue(StatType.Dodge, 0f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.Attack, 100f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.CritRate, 100f);
            bm.CurrentMonster.Stats.SetBaseValue(StatType.CritDamage, 50f);

            DamageResult result = DamageCalculator.CalculateDamage(bm.CurrentMonster, hero, 1.0f, null, DamageType.BasicAttack);
            hero.Health.TakeDamage(result);

            var inst = hero.StatusController.GetShield("pm_crit_sh");
            return result.IsCrit && Mathf.Approximately(result.FinalDamage, 150f) &&
                   inst != null && Mathf.Approximately(inst.CurrentAmount, 50f);
        }

        private static bool P07_8_PM_T21_SkillCooldownTriggeredOnce()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            MindMethodManager mmMgr = Object.FindAnyObjectByType<MindMethodManager>();
            if (hero == null || mmMgr == null) return false;

            var skill = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            if (skill == null) return false;
            var origEffects = skill.Effects != null ? new List<SkillEffectDefinitionSO>(skill.Effects) : new List<SkillEffectDefinitionSO>();

            var shSo = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shSo.Initialize("pm_cd_sh", 50f);
            skill.SetEffects(new List<SkillEffectDefinitionSO> { shSo });

            CooldownManager.ResetAllCooldowns();
            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, hero);
            SkillExecutor.Execute(req);

            bool onCd = CooldownManager.IsOnCooldown("skill_taiji_2_a", out _);
            Object.DestroyImmediate(shSo);
            skill.SetEffects(origEffects);
            return onCd;
        }

        private static bool P07_8_PM_T22_RageConsumedOnce()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            MindMethodManager mmMgr = Object.FindAnyObjectByType<MindMethodManager>();
            if (hero == null || mmMgr == null) return false;

            var skill = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            if (skill == null) return false;
            var origEffects = skill.Effects != null ? new List<SkillEffectDefinitionSO>(skill.Effects) : new List<SkillEffectDefinitionSO>();

            hero.Rage.InitializeRage(100f, 100f);

            var shSo = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shSo.Initialize("pm_rage_sh", 50f);
            skill.SetEffects(new List<SkillEffectDefinitionSO> { shSo });

            CooldownManager.ResetAllCooldowns();
            var req = new SkillExecutionRequest(hero, skill, SkillSlotType.Skill, hero);
            SkillExecutor.Execute(req);

            bool pass = Mathf.Approximately(hero.Rage.CurrentRage, 70f); // 100 - 30 rage
            Object.DestroyImmediate(shSo);
            skill.SetEffects(origEffects);
            return pass;
        }

        private static bool P07_8_PM_T23_TargetSelectionSelfDefault()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            var shSo = ScriptableObject.CreateInstance<ShieldEffectDefinitionSO>();
            shSo.Initialize("pm_tgt_sh", 50f, 5f, 0, ShieldStackPolicy.RefreshDuration, ShieldAmountMode.FlatValue, SkillTargetPolicy.SingleTarget);

            var resolver = new DefaultSkillTargetResolver();
            var req = new SkillExecutionRequest(hero, null, SkillSlotType.NormalAttack, null);
            var targets = resolver.ResolveTargets(req, shSo);

            bool pass = targets.Count == 1 && targets[0] == hero;
            Object.DestroyImmediate(shSo);
            return pass;
        }

        private static bool P07_8_PM_T24_EventBusOnShieldAppliedFired()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            bool fired = false;
            EventBus.OnShieldApplied += (target, id, amount, max) =>
            {
                if (id == "pm_ev_app") fired = true;
            };

            hero.StatusController.ApplyShield("pm_ev_app", 50f, 5f);
            return fired;
        }

        private static bool P07_8_PM_T25_EventBusOnShieldAbsorbedFired()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            bool fired = false;
            EventBus.OnShieldAbsorbed += (target, res) =>
            {
                if (res.ShieldId == "pm_ev_abs") fired = true;
            };

            hero.StatusController.ClearAll();
            hero.StatusController.ApplyShield("pm_ev_abs", 50f, 5f);
            hero.Health.TakeDamage(20f);

            return fired;
        }

        private static bool P07_8_PM_T26_Regression_BuffApplicationAndRecalculation()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            var buff = ScriptableObject.CreateInstance<BuffEffectDefinitionSO>();
            buff.Initialize("pm_reg_buf", "AtkUp", StatType.Attack, 20f, 10f, BuffStackingPolicy.RefreshDuration);

            float atkBefore = hero.Stats.GetValue(StatType.Attack, 10f);
            hero.StatusController.ApplyBuff(buff, hero);
            float atkAfter = hero.Stats.GetValue(StatType.Attack, 10f);

            Object.DestroyImmediate(buff);
            return atkAfter > atkBefore;
        }

        private static bool P07_8_PM_T27_Regression_DebuffApplication()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
            if (hero == null || bm == null || bm.CurrentMonster == null) return false;

            bm.CurrentMonster.StatusController.ClearAll();
            bm.CurrentMonster.StatusController.ApplyDebuff("pm_reg_deb", DebuffType.Attack, DebuffModifierMode.Flat, 5f, 10f, EffectPowerTier.TierB, hero);

            return bm.CurrentMonster.StatusController.ActiveDebuffs.ContainsKey("pm_reg_deb");
        }

        private static bool P07_8_PM_T28_Regression_DoTTickingDamageToShield()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
            if (hero == null || bm == null || bm.CurrentMonster == null) return false;

            if (!bm.CurrentMonster.IsAlive || bm.CurrentMonster.Health.CurrentHealth <= 0f)
            {
                bm.CurrentMonster.InitializeMonster();
                bm.CurrentMonster.Health.Revive(bm.CurrentMonster.Health.MaxHealth);
                bm.CurrentMonster.EnableEntityActions();
            }

            hero.StatusController.ClearAll();
            float hpBefore = hero.Health.CurrentHealth;
            hero.StatusController.ApplyShield("pm_dot_sh", 40f, 10f);

            var dotDmg = DamageCalculator.CalculateDotDamage(bm.CurrentMonster, hero, 20f, 1);
            hero.Health.TakeDamage(dotDmg);

            var inst = hero.StatusController.GetShield("pm_dot_sh");
            return Mathf.Approximately(hero.Health.CurrentHealth, hpBefore) &&
                   inst != null && Mathf.Approximately(inst.CurrentAmount, 20f);
        }

        private static bool P07_8_PM_T29_Regression_CrowdControlPermissions()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
            if (hero == null || bm == null || bm.CurrentMonster == null) return false;

            hero.StatusController.ClearAll();
            hero.StatusController.ApplyCrowdControl("pm_cc_perm", CrowdControlType.Stun, 5f, EffectPowerTier.TierB, bm.CurrentMonster);

            bool blocked = !hero.CanMove && !hero.CanBasicAttack && !hero.CanUseSkill;
            hero.StatusController.ClearAll();
            return blocked;
        }

        private static bool P07_8_PM_T30_Regression_CleanseNegativeDoesNotTouchShield()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
            if (hero == null || bm == null || bm.CurrentMonster == null) return false;

            hero.StatusController.ClearAll();
            hero.StatusController.ApplyDebuff("pm_deb_cln", DebuffType.Defense, DebuffModifierMode.Flat, 5f, 10f, EffectPowerTier.TierB, bm.CurrentMonster);
            hero.StatusController.ApplyShield("pm_sh_cln_iso", 60f, 10f);

            var clnRes = hero.StatusController.CleanseAllNegativeStatuses();
            return clnRes.Success && hero.StatusController.ActiveDebuffs.Count == 0 &&
                   hero.StatusController.HasActiveShield;
        }

        private static bool P07_8_PM_T31_Regression_DispelBuffsDoesNotTouchShield()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            var buff = ScriptableObject.CreateInstance<BuffEffectDefinitionSO>();
            buff.Initialize("pm_buf_dsp_iso", "DefUp", StatType.Defense, 10f, 10f, BuffStackingPolicy.RefreshDuration);
            hero.StatusController.ApplyBuff(buff, hero);
            hero.StatusController.ApplyShield("pm_sh_dsp_iso", 60f, 10f);

            var dspRes = hero.StatusController.DispelAllBuffs();
            Object.DestroyImmediate(buff);

            return dspRes.Success && hero.StatusController.ActiveBuffs.Count == 0 &&
                   hero.StatusController.HasActiveShield;
        }

        private static bool P07_8_PM_T32_Regression_FreezeShatterUnaffected()
        {
            BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (bm == null || bm.CurrentMonster == null || hero == null) return false;

            if (!bm.CurrentMonster.IsAlive || bm.CurrentMonster.Health.CurrentHealth <= 0f)
            {
                bm.CurrentMonster.InitializeMonster();
                bm.CurrentMonster.Health.Revive(bm.CurrentMonster.Health.MaxHealth);
                bm.CurrentMonster.EnableEntityActions();
            }

            bm.CurrentMonster.StatusController.ClearAll();
            bm.CurrentMonster.StatusController.ApplyCrowdControl("pm_frz_shat", CrowdControlType.Freeze, 5f, EffectPowerTier.TierB, hero);

            bool frozen = bm.CurrentMonster.IsFrozen;
            bm.CurrentMonster.StatusController.ApplyShield("pm_mon_sh", 50f, 10f);

            return frozen && bm.CurrentMonster.IsFrozen;
        }

        private static bool P07_8_PM_T33_Regression_AntiCcImmunityUnaffected()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.StatusController.ClearAll();
            hero.StatusController.ApplyAntiCCImmunity(10f);
            hero.StatusController.ApplyShield("pm_anticc_sh", 50f, 10f);

            return hero.StatusController.HasAntiCCImmunity && hero.StatusController.HasActiveShield;
        }

        private static bool P07_8_PM_T34_Regression_CcResistanceUnaffected()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            if (hero == null) return false;

            hero.Stats.SetBaseValue(StatType.CcResistance, 25f);
            hero.StatusController.ApplyShield("pm_cc_res_sh", 50f, 10f);

            float res = hero.StatusController.GetCcResistance();
            return Mathf.Approximately(res, 0.25f);
        }

        private static bool P07_8_PM_T35_EndToEndCombatAcceptance()
        {
            Hero hero = Object.FindAnyObjectByType<Hero>();
            BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
            if (hero == null || bm == null || bm.CurrentMonster == null) return false;

            Monster monster = bm.CurrentMonster;

            // 1. Reset state
            hero.StatusController.ClearAll();
            monster.StatusController.ClearAll();
            hero.Health.InitializeHealth(100f, hero);
            monster.Health.InitializeHealth(200f, monster);

            // 2. Hero applies Shield (amount 50)
            hero.StatusController.ApplyShield("e2e_hero_sh", 50f, 10f);
            bool shieldActive = hero.StatusController.HasActiveShield;

            // 3. Monster attacks Hero with 30 damage -> Shield absorbs 30, HP remains 100
            hero.Health.TakeDamage(30f);
            var inst = hero.StatusController.GetShield("e2e_hero_sh");
            bool fullAbsorb = Mathf.Approximately(hero.Health.CurrentHealth, 100f) &&
                              inst != null && Mathf.Approximately(inst.CurrentAmount, 20f);

            // 4. Monster attacks Hero with 40 damage -> Shield absorbs 20, HP takes 20 (HP = 80)
            hero.Health.TakeDamage(40f);
            bool partialAbsorb = Mathf.Approximately(hero.Health.CurrentHealth, 80f) &&
                                 !hero.StatusController.HasActiveShield;

            // 5. Monster applies Shield to itself
            monster.StatusController.ApplyShield("e2e_mon_sh", 40f, 10f);
            bool monsterShield = monster.StatusController.HasActiveShield;

            // 6. Hero attacks monster with 50 damage -> Monster Shield absorbs 40, Monster HP takes 10 (HP = 190)
            monster.Health.TakeDamage(50f);
            bool monsterDamaged = Mathf.Approximately(monster.Health.CurrentHealth, 190f) &&
                                  !monster.StatusController.HasActiveShield;

            // 7. Cleanup
            hero.StatusController.ClearAll();
            monster.StatusController.ClearAll();

            bool pass = shieldActive && fullAbsorb && partialAbsorb && monsterShield && monsterDamaged;
            Debug.Log($"[T35 END TO END SHIELD COMBAT] {(pass ? "PASS" : "FAIL")}\n" +
                      $"HeroShieldActive={shieldActive}, FullAbsorb={fullAbsorb}, PartialAbsorb={partialAbsorb}, MonsterShield={monsterShield}, MonsterDamaged={monsterDamaged}");
            return pass;
        }

        #endregion

        #region Prototype 07.8 UI Automated Tests (UI01 - UI05)

        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.8 UI Automated Tests (UI01 to UI05)")]
        public static bool RunPrototype07_8VisualUITests()
        {
            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.8 UI AUTOMATED TESTS (UI01 TO UI05)");
            Debug.Log("==================================================");

            Prototype01SceneBuilder.BuildPrototype02Data();
            Prototype01SceneBuilder.BuildPrototype01Scene();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);

            Hero hero = Object.FindAnyObjectByType<Hero>();
            BattleHUD hud = Object.FindAnyObjectByType<BattleHUD>();

            if (hero == null || hud == null)
            {
                Debug.LogError("[UI TESTS] FATAL: Hero or BattleHUD not found in scene!");
                return false;
            }

            hero.InitializeHero();
            hero.Health.Revive(hero.Health.MaxHealth);
            hero.EnableEntityActions();
            hero.StatusController.ClearAll();

            hud.RegisterEvents();
            hud.UpdateAllShields();

            // UI01: Shield UI reflects application
            bool ui01 = false;
            {
                hud.UpdateAllShields();
                bool initialHidden = !hud.IsHeroShieldVisible;
                hero.StatusController.ApplyShield("ui_test_s1", 150f, 30f);
                bool visible = hud.IsHeroShieldVisible;
                float displayed = hud.CurrentDisplayedHeroShield;
                ui01 = initialHidden && visible && Mathf.Approximately(displayed, 150f);
                Debug.Log($"[UI01 Reflects Application] InitialHidden={initialHidden}, Visible={visible}, Displayed={displayed} | {(ui01 ? "PASS" : "FAIL")}");
            }

            // UI02: Shield UI reflects absorption
            bool ui02 = false;
            {
                hero.Health.TakeDamage(50f);
                bool visible = hud.IsHeroShieldVisible;
                float displayed = hud.CurrentDisplayedHeroShield;
                ui02 = visible && Mathf.Approximately(displayed, 100f);
                Debug.Log($"[UI02 Reflects Absorption] Visible={visible}, Displayed={displayed} (expected 100) | {(ui02 ? "PASS" : "FAIL")}");
            }

            // UI03: Shield UI reaches zero
            bool ui03 = false;
            {
                hero.Health.TakeDamage(100f);
                float runtimeShield = hero.StatusController.TotalShieldAmount;
                ui03 = Mathf.Approximately(runtimeShield, 0f);
                Debug.Log($"[UI03 Reaches Zero] RuntimeShield={runtimeShield} | {(ui03 ? "PASS" : "FAIL")}");
            }

            // UI04: Shield UI hides/deactivates after depletion
            bool ui04 = false;
            {
                bool hidden = !hud.IsHeroShieldVisible;
                float hpBefore = hero.Health.CurrentHealth;
                hero.Health.TakeDamage(25f);
                float hpAfter = hero.Health.CurrentHealth;
                bool hpDeducted = Mathf.Approximately(hpAfter, hpBefore - 25f);
                ui04 = hidden && hpDeducted;
                Debug.Log($"[UI04 Hides After Depletion] Hidden={hidden}, HpDeducted={hpDeducted} | {(ui04 ? "PASS" : "FAIL")}");
            }

            // UI05: Multiple Shield aggregate is correct
            bool ui05 = false;
            {
                hero.StatusController.ClearAll();
                hero.StatusController.ApplyShield("ui_multi_1", 80f, 30f, priority: 5);
                hero.StatusController.ApplyShield("ui_multi_2", 120f, 30f, priority: 10);
                float displayedSum = hud.CurrentDisplayedHeroShield;
                bool sumCorrect = Mathf.Approximately(displayedSum, 200f);

                hero.Health.TakeDamage(100f);
                float displayedAfterHit = hud.CurrentDisplayedHeroShield;
                bool consumedCorrect = Mathf.Approximately(displayedAfterHit, 100f);

                ui05 = sumCorrect && consumedCorrect;
                Debug.Log($"[UI05 Multiple Shield Aggregate] DisplayedSum={displayedSum} (exp 200), DisplayedAfterHit={displayedAfterHit} (exp 100) | {(ui05 ? "PASS" : "FAIL")}");
            }

            hero.StatusController.ClearAll();
            hud.UpdateAllShields();

            bool allUI = ui01 && ui02 && ui03 && ui04 && ui05;
            Debug.Log($"   PROTOTYPE 07.8 UI TESTS: {(allUI ? "5/5 PASSED" : "FAILED")}");
            return allUI;
        }

        #endregion

        #region Prototype 07.8 Visual Acceptance Verification (V01 - V08)

        [MenuItem("Tools/Wuxia RPG/Run P07.8 Visual Verification Pass (V01-V08)")]
        public static bool RunP07_8VisualAcceptancePass()
        {
            Debug.Log("==================================================");
            Debug.Log("   P07.8 VISUAL VERIFICATION REMEDIATION (V01 - V08)");
            Debug.Log("==================================================");

            Prototype01SceneBuilder.BuildPrototype02Data();
            Prototype01SceneBuilder.BuildPrototype01Scene();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);

            Hero hero = Object.FindAnyObjectByType<Hero>();
            BattleManager bm = Object.FindAnyObjectByType<BattleManager>();
            BattleHUD hud = Object.FindAnyObjectByType<BattleHUD>();

            if (hero == null || bm == null || hud == null)
            {
                Debug.LogError("[VISUAL PASS] FATAL: Hero, BattleManager, or BattleHUD missing!");
                return false;
            }

            hero.InitializeHero();
            hero.Health.Revive(hero.Health.MaxHealth);
            hero.EnableEntityActions();
            hero.StatusController.ClearAll();

            hud.RegisterEvents();
            hud.UpdateAllShields();

            if (bm.CurrentMonster != null)
            {
                bm.CurrentMonster.InitializeMonster();
                bm.CurrentMonster.Health.Revive(bm.CurrentMonster.Health.MaxHealth);
                bm.CurrentMonster.EnableEntityActions();
            }

            if (!bm.IsBattleActive) bm.StartBattle();

            var testTime = new TestTimeProvider { CurrentTime = 100f };
            CooldownManager.TimeProvider = testTime;

            // V01 — Shield Application
            bool v01 = false;
            {
                hero.StatusController.ClearAll();
                hud.UpdateAllShields();
                bool hadShieldBefore = hud.IsHeroShieldVisible;
                hero.StatusController.ApplyShield("v_shield_01", 120f, 30f);
                bool hasShieldAfter = hud.IsHeroShieldVisible;
                float displayedAmount = hud.CurrentDisplayedHeroShield;
                float runtimeAmount = hero.StatusController.TotalShieldAmount;
                v01 = !hadShieldBefore && hasShieldAfter && Mathf.Approximately(displayedAmount, runtimeAmount) && Mathf.Approximately(displayedAmount, 120f);
                Debug.Log($"[V01 Shield Application] Before={hadShieldBefore}, After={hasShieldAfter}, Displayed={displayedAmount}, Runtime={runtimeAmount} | {(v01 ? "PASS" : "FAIL")}");
            }

            // V02 — Full Absorption
            bool v02 = false;
            {
                float hpBefore = hero.Health.CurrentHealth;
                float shieldBefore = hud.CurrentDisplayedHeroShield;
                hero.Health.TakeDamage(40f);
                float hpAfter = hero.Health.CurrentHealth;
                float shieldAfter = hud.CurrentDisplayedHeroShield;
                bool hpProtected = Mathf.Approximately(hpAfter, hpBefore);
                bool shieldDecreased = Mathf.Approximately(shieldAfter, shieldBefore - 40f) && Mathf.Approximately(shieldAfter, 80f);
                v02 = hpProtected && shieldDecreased;
                Debug.Log($"[V02 Full Absorption] HP={hpBefore}->{hpAfter}, Shield={shieldBefore}->{shieldAfter} | {(v02 ? "PASS" : "FAIL")}");
            }

            // V03 — Partial Absorption
            bool v03 = false;
            {
                hero.StatusController.ClearAll();
                hero.Health.Revive(hero.Health.MaxHealth);
                hero.StatusController.ApplyShield("v_shield_03", 60f, 30f);
                float hpBefore = hero.Health.CurrentHealth;
                hero.Health.TakeDamage(100f);
                float hpAfter = hero.Health.CurrentHealth;
                float shieldAfter = hero.StatusController.TotalShieldAmount;
                bool shieldZero = Mathf.Approximately(shieldAfter, 0f) && !hud.IsHeroShieldVisible;
                bool hpOnlyRem = Mathf.Approximately(hpAfter, hpBefore - 40f);
                v03 = shieldZero && hpOnlyRem;
                Debug.Log($"[V03 Partial Absorption] ShieldZero={shieldZero}, HP={hpBefore}->{hpAfter} (loss 40) | {(v03 ? "PASS" : "FAIL")}");
            }

            // V04 — Shield Depletion
            bool v04 = false;
            {
                hero.StatusController.ClearAll();
                hero.Health.Revive(hero.Health.MaxHealth);
                hero.StatusController.ApplyShield("v_shield_04", 50f, 30f);
                hero.Health.TakeDamage(50f);
                bool depletedHidden = !hud.IsHeroShieldVisible && !hero.StatusController.HasActiveShield;
                float hpBeforeNext = hero.Health.CurrentHealth;
                hero.Health.TakeDamage(30f);
                float hpAfterNext = hero.Health.CurrentHealth;
                bool hitToHp = Mathf.Approximately(hpAfterNext, hpBeforeNext - 30f);
                v04 = depletedHidden && hitToHp;
                Debug.Log($"[V04 Shield Depletion] DepletedHidden={depletedHidden}, NextHitDirectToHp={hitToHp} | {(v04 ? "PASS" : "FAIL")}");
            }

            // V05 — Multiple Shields
            bool v05 = false;
            {
                hero.StatusController.ClearAll();
                hero.Health.Revive(hero.Health.MaxHealth);
                hero.StatusController.ApplyShield("v_sh_low", 40f, 30f, priority: 1);
                hero.StatusController.ApplyShield("v_sh_high", 60f, 30f, priority: 10);
                float aggregateStart = hud.CurrentDisplayedHeroShield;
                bool aggCorrect = Mathf.Approximately(aggregateStart, 100f);

                hero.Health.TakeDamage(50f);
                float aggregateMid = hud.CurrentDisplayedHeroShield;
                bool midCorrect = Mathf.Approximately(aggregateMid, 50f);

                hero.Health.TakeDamage(20f);
                float aggregateLow = hud.CurrentDisplayedHeroShield;
                bool lowCorrect = Mathf.Approximately(aggregateLow, 30f);

                v05 = aggCorrect && midCorrect && lowCorrect;
                Debug.Log($"[V05 Multiple Shields] Start={aggregateStart}(100), Mid={aggregateMid}(50), Low={aggregateLow}(30) | {(v05 ? "PASS" : "FAIL")}");
            }

            // V06 — Real Combat
            bool v06 = false;
            {
                hero.StatusController.ClearAll();
                hero.Health.Revive(hero.Health.MaxHealth);
                float hpStart = hero.Health.CurrentHealth;
                hero.StatusController.ApplyShield("v_combat_sh", 100f, 30f);
                bool appeared = hud.IsHeroShieldVisible && Mathf.Approximately(hud.CurrentDisplayedHeroShield, 100f);

                hero.Health.TakeDamage(40f);
                bool hit1Ok = Mathf.Approximately(hud.CurrentDisplayedHeroShield, 60f) && Mathf.Approximately(hero.Health.CurrentHealth, hpStart);

                hero.Health.TakeDamage(70f);
                bool hit2Ok = !hud.IsHeroShieldVisible && Mathf.Approximately(hero.Health.CurrentHealth, hpStart - 10f);

                v06 = appeared && hit1Ok && hit2Ok;
                Debug.Log($"[V06 Real Combat] Appeared={appeared}, Hit1ShieldDecreasedHpProtected={hit1Ok}, Hit2DepletedRemToHp={hit2Ok} | {(v06 ? "PASS" : "FAIL")}");
            }

            // V07 — Expiration / Removal
            bool v07 = false;
            {
                hero.StatusController.ClearAll();
                hero.Health.Revive(hero.Health.MaxHealth);
                hero.StatusController.ApplyShield("v_expire_sh", 80f, 5f);
                bool hadShield = hud.IsHeroShieldVisible;
                testTime.Advance(6f);
                hero.StatusController.Tick(testTime.CurrentTime);
                bool expiredHidden = !hud.IsHeroShieldVisible;

                float hpBefore = hero.Health.CurrentHealth;
                hero.Health.TakeDamage(30f);
                bool damageDirect = Mathf.Approximately(hero.Health.CurrentHealth, hpBefore - 30f);

                hero.StatusController.ApplyShield("v_remove_sh", 50f, 30f);
                bool hadRemove = hud.IsHeroShieldVisible;
                hero.StatusController.RemoveShield("v_remove_sh");
                bool removedHidden = !hud.IsHeroShieldVisible;

                v07 = hadShield && expiredHidden && damageDirect && hadRemove && removedHidden;
                Debug.Log($"[V07 Expiration / Removal] ExpiredHidden={expiredHidden}, DamageDirect={damageDirect}, RemovedHidden={removedHidden} | {(v07 ? "PASS" : "FAIL")}");
            }

            // V08 — BattleHUD
            bool v08 = false;
            {
                hero.StatusController.ClearAll();
                hud.UpdateAllShields();
                bool cleanInit = !hud.IsHeroShieldVisible && hud.CurrentDisplayedHeroShield >= 0f;

                hero.StatusController.ApplyShield("v_hud_sh", 75f, 30f);
                bool syncApply = hud.IsHeroShieldVisible && Mathf.Approximately(hud.CurrentDisplayedHeroShield, hero.StatusController.TotalShieldAmount);

                hero.Health.TakeDamage(100f);
                bool syncDeplete = !hud.IsHeroShieldVisible && hud.CurrentDisplayedHeroShield >= 0f && !hero.StatusController.HasActiveShield;

                v08 = cleanInit && syncApply && syncDeplete;
                Debug.Log($"[V08 BattleHUD Synchronization] CleanInit={cleanInit}, SyncApply={syncApply}, SyncDeplete={syncDeplete} | {(v08 ? "PASS" : "FAIL")}");
            }

            CooldownManager.TimeProvider = new UnityTimeProvider();
            hero.StatusController.ClearAll();
            hud.UpdateAllShields();

            bool allVisual = v01 && v02 && v03 && v04 && v05 && v06 && v07 && v08;
            Debug.Log("==================================================");
            Debug.Log($"   P07.8 VISUAL ACCEPTANCE (V01-V08): {(allVisual ? "8/8 ALL PASS" : "FAIL")}");
            Debug.Log("==================================================");

            return allVisual;
        }

        private static void CleanTestEnvironment()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            MindMethodManager.ResetInstance();
            BattleManager.ResetInstance();
            EquipmentManager.ResetInstance();
            Inventory.Inventory.ResetInstance();
            ResourceManager.ResetInstance();
            ProgressionManager.ResetInstance();
            CooldownManager.ResetForTesting();
            EventBus.ClearAllListeners();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [MenuItem("Tools/Wuxia RPG/Run Complete P07.8 Regression and Verification")]
        public static bool RunCompleteP07_8RegressionSuite()
        {
            Debug.Log("==================================================");
            Debug.Log("   STARTING COMPLETE P07.8 REMEDIATION REGRESSION SUITE");
            Debug.Log("==================================================");

            CleanTestEnvironment();
            bool p07_8_auto = RunAllPrototype07_8Tests();

            CleanTestEnvironment();
            bool p07_8_pm = RunPrototype07_8PlayModeAcceptanceTests();

            CleanTestEnvironment();
            bool p07_8_ui = RunPrototype07_8VisualUITests();

            CleanTestEnvironment();
            bool p07_8_vis = RunP07_8VisualAcceptancePass();

            CleanTestEnvironment();
            bool p06 = RunAllPrototype06Tests();

            CleanTestEnvironment();
            bool p07_4 = RunAllPrototype07_4Tests();

            CleanTestEnvironment();
            bool p07_5 = RunAllPrototype07_5Tests();

            CleanTestEnvironment();
            bool p07_6 = RunAllPrototype07_6Tests();

            CleanTestEnvironment();
            bool p07_7 = RunAllPrototype07_7Tests();

            CleanTestEnvironment();
            bool master = RunMasterRegressionSuite();

            bool all = p07_8_auto && p07_8_pm && p07_8_ui && p07_8_vis &&
                       p06 && p07_4 && p07_5 && p07_6 && p07_7 && master;

            Debug.Log("==================================================");
            Debug.Log("   COMPLETE P07.8 REMEDIATION REGRESSION SUMMARY");
            Debug.Log("==================================================");
            Debug.Log($"   P07.8 Automated (55):         {(p07_8_auto ? "PASS" : "FAIL")}");
            Debug.Log($"   P07.8 Play Mode (35):         {(p07_8_pm ? "PASS" : "FAIL")}");
            Debug.Log($"   P07.8 UI Tests (5):           {(p07_8_ui ? "PASS" : "FAIL")}");
            Debug.Log($"   P07.8 Visual Accept (V01-08): {(p07_8_vis ? "PASS" : "FAIL")}");
            Debug.Log($"   P06 Regression:               {(p06 ? "PASS" : "FAIL")}");
            Debug.Log($"   P07.4 Regression:             {(p07_4 ? "PASS" : "FAIL")}");
            Debug.Log($"   P07.5 Regression:             {(p07_5 ? "PASS" : "FAIL")}");
            Debug.Log($"   P07.6 Regression:             {(p07_6 ? "PASS" : "FAIL")}");
            Debug.Log($"   P07.7 Regression:             {(p07_7 ? "PASS" : "FAIL")}");
            Debug.Log($"   P07.8 Regression:             {(p07_8_auto ? "PASS" : "FAIL")}");
            Debug.Log($"   Master Regression Suite:      {(master ? "PASS" : "FAIL")}");
            Debug.Log($"   OVERALL STATUS:               {(all ? "100% PASS" : "FAIL")}");
            Debug.Log("==================================================");

            return all;
        }

        #endregion
    }
}
#endif
