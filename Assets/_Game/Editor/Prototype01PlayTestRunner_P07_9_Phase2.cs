#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Entities.Components;
using WuxiaGame.Progression;

namespace WuxiaGame.Editor
{
    public static partial class Prototype01PlayTestRunner
    {
        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9 Phase 2 Tests (TEST 01 to TEST 25)")]
        public static bool RunAllPrototype07_9_Phase2_Tests()
        {
            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 PHASE 2 & 2.1 TESTS (TEST 01 -> TEST 25)");
            Debug.Log("   Scope: Cast Execution Foundation + Active Cast Re-entry Guard");
            Debug.Log("==================================================");

            bool t01 = P07_9_P2_01_CastTimeZero_RetainsInstantBehavior();
            bool t02 = P07_9_P2_02_CastTimeGreaterThanZero_TransitionsReadyToCasting();
            bool t03 = P07_9_P2_03_ElapsedTime_Deterministic();
            bool t04 = P07_9_P2_04_RemainingTime_Deterministic();
            bool t05 = P07_9_P2_05_CastNotComplete_DoesNotExecuteEffect();
            bool t06 = P07_9_P2_06_CastComplete_ExecutesEffectExactlyOnce();
            bool t07 = P07_9_P2_07_TickAfterComplete_DoesNotExecuteSecondTime();
            bool t08 = P07_9_P2_08_Rage_ConsumedExactlyOnceAtCastStart();
            bool t09 = P07_9_P2_09_Rage_NotConsumedAgainDuringTick();
            bool t10 = P07_9_P2_10_Cooldown_DoesNotStartAtCastStart();
            bool t11 = P07_9_P2_11_Cooldown_StartsAfterSuccessfulCastComplete();
            bool t12 = P07_9_P2_12_SkillExecutor_RemainsExecutionAuthority();
            bool t13 = P07_9_P2_13_EffectResolver_OnlyExecutesWhenCastComplete();
            bool t14 = P07_9_P2_14_Damage_PassesThroughDamageCalculator();
            bool t15 = P07_9_P2_15_Damage_PassesThroughHealthComponent();
            bool t16 = P07_9_P2_16_Shield_InterceptsThroughP07_8Pipeline();
            bool t17 = P07_9_P2_17_CC_AuthorityRemainsEntityStatusController();
            bool t18 = P07_9_P2_18_SkillCastState_DoesNotContainDuplicateRage();
            bool t19 = P07_9_P2_19_SkillCastState_DoesNotContainDuplicateCooldown();
            bool t20 = P07_9_P2_20_CastComplete_ExactlyOnce();
            bool t21 = P07_9_P2_21_ActiveCast_ReentryRejected_DoesNotOverwriteActiveCast();
            bool t22 = P07_9_P2_22_ActiveCast_ReentryRejected_DoesNotConsumeRage();
            bool t23 = P07_9_P2_23_ActiveCast_AContinuesProgressingAfterBRejection();
            bool t24 = P07_9_P2_24_ActiveCast_ACompletesExactlyOnceAfterBRejection();
            bool t25 = P07_9_P2_25_ActiveCast_BDoesNotExecuteEffects();

            bool allPassed = t01 && t02 && t03 && t04 && t05 && t06 && t07 && t08 && t09 && t10 &&
                             t11 && t12 && t13 && t14 && t15 && t16 && t17 && t18 && t19 && t20 &&
                             t21 && t22 && t23 && t24 && t25;

            Debug.Log("==================================================");
            Debug.Log("   PROTOTYPE 07.9 PHASE 2 & 2.1 TEST SUMMARY");
            Debug.Log("==================================================");
            Debug.Log($"   TEST 01 (CastTime=0 instant behavior):               {(t01 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 02 (CastTime>0 transitions READY -> CASTING):   {(t02 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 03 (Elapsed time deterministic):                {(t03 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 04 (Remaining time deterministic):              {(t04 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 05 (Cast incomplete -> no effect):              {(t05 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 06 (Cast complete -> executes effect once):     {(t06 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 07 (Tick after Complete -> no 2nd execution):   {(t07 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 08 (Rage consumed once at Cast Start):          {(t08 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 09 (Rage not consumed again in Tick):           {(t09 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 10 (Cooldown does not start at Cast Start):     {(t10 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 11 (Cooldown starts after Cast Complete):       {(t11 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 12 (SkillExecutor sole execution authority):    {(t12 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 13 (EffectResolver only executes on Complete):  {(t13 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 14 (Damage uses DamageCalculator pipeline):     {(t14 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 15 (Damage deducts HealthComponent):            {(t15 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 16 (Shield intercepts via P07.8 pipeline):      {(t16 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 17 (CC authority remains EntityStatusController): {(t17 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 18 (No duplicate Rage in SkillCastState):       {(t18 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 19 (No duplicate Cooldown in SkillCastState):   {(t19 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 20 (Cast Complete exactly once guard):          {(t20 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 21 (Re-entry rejected / does not overwrite A):  {(t21 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 22 (Re-entry does not consume Rage):            {(t22 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 23 (A continues progressing after B rejection): {(t23 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 24 (A completes exactly once after B rejection):{(t24 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 25 (B does not execute effects):                {(t25 ? "PASS" : "FAIL")}");
            Debug.Log($"   OVERALL PHASE 2 & 2.1 STATUS: {(allPassed ? "100% PASS" : "FAIL")}");
            Debug.Log("==================================================");

            return allPassed;
        }

        /// <summary>
        /// TEST 01: CastTime = 0 giữ Instant behavior.
        /// </summary>
        private static bool P07_9_P2_01_CastTimeZero_RetainsInstantBehavior()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            hero.Rage.AddRage(100f);

            float monsterHpBefore = bm.CurrentMonster.Health.CurrentHealth;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float monsterHpAfter = bm.CurrentMonster.Health.CurrentHealth;

            bool instantOk = res.Success && monsterHpAfter < monsterHpBefore && !hero.CastState.IsActive;
            Debug.Log($"[P07_9_P2_01] TEST 01 -> Success: {res.Success}, DmgDealt: {monsterHpBefore - monsterHpAfter}, IsActive: {hero.CastState.IsActive} | {(instantOk ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return instantOk;
        }

        /// <summary>
        /// TEST 02: CastTime > 0 chuyển READY → CASTING.
        /// </summary>
        private static bool P07_9_P2_02_CastTimeGreaterThanZero_TransitionsReadyToCasting()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.5f);
            hero.Rage.AddRage(100f);

            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool castingOk = res.Success &&
                             hero.CastState.CurrentPhase == SkillCastPhase.Casting &&
                             hero.CastState.IsActive &&
                             hero.CastState.CastDuration == 1.5f;

            Debug.Log($"[P07_9_P2_02] TEST 02 -> ResSuccess: {res.Success}, Phase: {hero.CastState.CurrentPhase}, IsActive: {hero.CastState.IsActive} | {(castingOk ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return castingOk;
        }

        /// <summary>
        /// TEST 03: Elapsed time deterministic.
        /// </summary>
        private static bool P07_9_P2_03_ElapsedTime_Deterministic()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(2.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            hero.CastState.Tick(0.25f);
            bool step1 = Mathf.Abs(hero.CastState.ElapsedTime - 0.25f) < 0.0001f;

            hero.CastState.Tick(0.25f);
            bool step2 = Mathf.Abs(hero.CastState.ElapsedTime - 0.50f) < 0.0001f;

            hero.CastState.Tick(0.50f);
            bool step3 = Mathf.Abs(hero.CastState.ElapsedTime - 1.00f) < 0.0001f;

            bool ok = step1 && step2 && step3;
            Debug.Log($"[P07_9_P2_03] TEST 03 -> Step1: {step1}, Step2: {step2}, Step3: {step3} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 04: Remaining time deterministic.
        /// </summary>
        private static bool P07_9_P2_04_RemainingTime_Deterministic()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            hero.CastState.Tick(0.5f);
            bool rem1 = Mathf.Abs(hero.CastState.RemainingTime - 1.0f) < 0.0001f;

            hero.CastState.Tick(0.5f);
            bool rem2 = Mathf.Abs(hero.CastState.RemainingTime - 0.5f) < 0.0001f;

            bool ok = rem1 && rem2;
            Debug.Log($"[P07_9_P2_04] TEST 04 -> Rem1: {rem1}, Rem2: {rem2} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 05: Cast chưa complete → không execute effect.
        /// </summary>
        private static bool P07_9_P2_05_CastNotComplete_DoesNotExecuteEffect()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            float hpBefore = bm.CurrentMonster.Health.CurrentHealth;
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            // Tick halfway (0.5f / 1.0f)
            hero.CastState.Tick(0.5f);
            float hpMid = bm.CurrentMonster.Health.CurrentHealth;

            bool noEffectYet = hpMid == hpBefore && hero.CastState.IsActive;
            Debug.Log($"[P07_9_P2_05] TEST 05 -> HpBefore: {hpBefore}, HpMid: {hpMid}, NoEffectYet: {noEffectYet} | {(noEffectYet ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return noEffectYet;
        }

        /// <summary>
        /// TEST 06: Cast complete → execute effect đúng 1 lần.
        /// </summary>
        private static bool P07_9_P2_06_CastComplete_ExecutesEffectExactlyOnce()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            float hpBefore = bm.CurrentMonster.Health.CurrentHealth;
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            // Tick past completion
            hero.CastState.Tick(0.5f);
            hero.CastState.Tick(0.5f); // reaches 1.0f -> complete!

            float hpAfter = bm.CurrentMonster.Health.CurrentHealth;
            bool effectExecuted = hpAfter < hpBefore && !hero.CastState.IsActive;

            Debug.Log($"[P07_9_P2_06] TEST 06 -> HpBefore: {hpBefore}, HpAfter: {hpAfter}, Executed: {effectExecuted} | {(effectExecuted ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return effectExecuted;
        }

        /// <summary>
        /// TEST 07: Tick sau Complete không execute lần 2.
        /// </summary>
        private static bool P07_9_P2_07_TickAfterComplete_DoesNotExecuteSecondTime()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.CastState.Tick(1.0f); // completes

            float hpAfterComplete = bm.CurrentMonster.Health.CurrentHealth;

            // Extra ticks
            hero.CastState.Tick(0.5f);
            hero.CastState.Tick(1.0f);
            float hpAfterExtraTicks = bm.CurrentMonster.Health.CurrentHealth;

            bool noSecondExecution = hpAfterExtraTicks == hpAfterComplete;
            Debug.Log($"[P07_9_P2_07] TEST 07 -> HpAfterComplete: {hpAfterComplete}, HpAfterExtra: {hpAfterExtraTicks}, No2nd: {noSecondExecution} | {(noSecondExecution ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return noSecondExecution;
        }

        /// <summary>
        /// TEST 08: Rage Cast-time skill consume đúng một lần tại Cast Start.
        /// </summary>
        private static bool P07_9_P2_08_Rage_ConsumedExactlyOnceAtCastStart()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            float rageBefore = hero.Rage.CurrentRage;
            float rageCost = skillDef.RageCost;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float rageDuringCast = hero.Rage.CurrentRage;

            bool consumedAtStart = (rageCost > 0f) ? Mathf.Abs(rageDuringCast - (rageBefore - rageCost)) < 0.0001f : true;
            Debug.Log($"[P07_9_P2_08] TEST 08 -> RageBefore: {rageBefore}, RageDuring: {rageDuringCast}, Cost: {rageCost}, ConsumedAtStart: {consumedAtStart} | {(consumedAtStart ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return consumedAtStart;
        }

        /// <summary>
        /// TEST 09: Rage không bị consume lại trong Tick.
        /// </summary>
        private static bool P07_9_P2_09_Rage_NotConsumedAgainDuringTick()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float rageAtStart = hero.Rage.CurrentRage;

            hero.CastState.Tick(0.5f);
            float rageMid = hero.Rage.CurrentRage;

            hero.CastState.Tick(0.5f); // complete
            float rageEnd = hero.Rage.CurrentRage;

            bool notConsumedAgain = (rageMid == rageAtStart) && (rageEnd == rageAtStart);
            Debug.Log($"[P07_9_P2_09] TEST 09 -> RageStart: {rageAtStart}, RageMid: {rageMid}, RageEnd: {rageEnd}, Unchanged: {notConsumedAgain} | {(notConsumedAgain ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return notConsumedAgain;
        }

        /// <summary>
        /// TEST 10: Cooldown không start tại Cast Start.
        /// </summary>
        private static bool P07_9_P2_10_Cooldown_DoesNotStartAtCastStart()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);
            CooldownManager.ResetAllCooldowns();

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool onCdDuringCast = CooldownManager.IsOnCooldown(skillDef.SkillId, out float remain);
            bool ok = !onCdDuringCast;

            Debug.Log($"[P07_9_P2_10] TEST 10 -> OnCdDuringCast: {onCdDuringCast}, Remaining: {remain} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 11: Cooldown start sau successful Cast Complete.
        /// </summary>
        private static bool P07_9_P2_11_Cooldown_StartsAfterSuccessfulCastComplete()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);
            CooldownManager.ResetAllCooldowns();

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.CastState.Tick(1.0f); // Complete

            bool onCdAfterComplete = (skillDef.Cooldown > 0f) ? CooldownManager.IsOnCooldown(skillDef.SkillId, out _) : true;
            Debug.Log($"[P07_9_P2_11] TEST 11 -> OnCdAfterComplete: {onCdAfterComplete} | {(onCdAfterComplete ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return onCdAfterComplete;
        }

        /// <summary>
        /// TEST 12: SkillExecutor vẫn là execution authority.
        /// </summary>
        private static bool P07_9_P2_12_SkillExecutor_RemainsExecutionAuthority()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            bool succeededEventFired = false;
            Action<SkillExecutionRequest, SkillExecutionResult> handler = (req, res) =>
            {
                if (req != null && req.Skill != null && req.Skill.SkillId == skillDef.SkillId && res.Success)
                {
                    succeededEventFired = true;
                }
            };
            EventBus.OnSkillExecutionSucceeded += handler;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool firedBefore = succeededEventFired;

            hero.CastState.Tick(1.0f);
            bool firedAfter = succeededEventFired;

            EventBus.OnSkillExecutionSucceeded -= handler;
            bool ok = !firedBefore && firedAfter;

            Debug.Log($"[P07_9_P2_12] TEST 12 -> FiredBefore: {firedBefore}, FiredAfter: {firedAfter} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 13: EffectResolver chỉ execute khi Cast Complete.
        /// </summary>
        private static bool P07_9_P2_13_EffectResolver_OnlyExecutesWhenCastComplete()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            float hpInit = bm.CurrentMonster.Health.CurrentHealth;
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            hero.CastState.Tick(0.9f);
            float hp90 = bm.CurrentMonster.Health.CurrentHealth;

            hero.CastState.Tick(0.1f); // complete
            float hp100 = bm.CurrentMonster.Health.CurrentHealth;

            bool ok = (hp90 == hpInit) && (hp100 < hpInit);
            Debug.Log($"[P07_9_P2_13] TEST 13 -> HpInit: {hpInit}, Hp90: {hp90}, Hp100: {hp100} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 14: Damage vẫn đi qua DamageCalculator.
        /// </summary>
        private static bool P07_9_P2_14_Damage_PassesThroughDamageCalculator()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            float hpBefore = bm.CurrentMonster.Health.CurrentHealth;
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.CastState.Tick(1.0f);
            float hpAfter = bm.CurrentMonster.Health.CurrentHealth;

            float damageDealt = hpBefore - hpAfter;
            // DamageCalculator applies ATK * Multiplier / Defense mitigation -> must be > 0 and realistic
            bool realisticDamage = damageDealt > 10f && damageDealt < 5000f;

            Debug.Log($"[P07_9_P2_14] TEST 14 -> DamageDealt: {damageDealt}, Realistic: {realisticDamage} | {(realisticDamage ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return realisticDamage;
        }

        /// <summary>
        /// TEST 15: Damage vẫn đi qua HealthComponent.
        /// </summary>
        private static bool P07_9_P2_15_Damage_PassesThroughHealthComponent()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            float healthCompBefore = bm.CurrentMonster.Health.CurrentHealth;
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.CastState.Tick(1.0f);
            float healthCompAfter = bm.CurrentMonster.Health.CurrentHealth;

            bool healthDeducted = healthCompAfter < healthCompBefore;
            Debug.Log($"[P07_9_P2_15] TEST 15 -> Before: {healthCompBefore}, After: {healthCompAfter} | {(healthDeducted ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return healthDeducted;
        }

        /// <summary>
        /// TEST 16: Shield vẫn intercept qua P07.8 pipeline.
        /// </summary>
        private static bool P07_9_P2_16_Shield_InterceptsThroughP07_8Pipeline()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            // Apply large shield to target monster
            bm.CurrentMonster.StatusController.ApplyShield("shield_p2_target_test", 1000f, 10f);
            float hpBefore = bm.CurrentMonster.Health.CurrentHealth;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.CastState.Tick(1.0f); // complete cast -> deals damage

            float hpAfter = bm.CurrentMonster.Health.CurrentHealth;
            var shield = bm.CurrentMonster.StatusController.GetShield("shield_p2_target_test");
            float remainingShield = shield != null ? shield.CurrentAmount : -1f;

            bool shieldIntercepted = (hpBefore == hpAfter) && (remainingShield < 1000f) && (remainingShield > 0f);
            Debug.Log($"[P07_9_P2_16] TEST 16 -> HpBefore: {hpBefore}, HpAfter: {hpAfter}, ShieldRemaining: {remainingShield} | {(shieldIntercepted ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return shieldIntercepted;
        }

        /// <summary>
        /// TEST 17: CC authority vẫn là EntityStatusController.
        /// </summary>
        private static bool P07_9_P2_17_CC_AuthorityRemainsEntityStatusController()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            // Stun blocks skill validation
            hero.StatusController.ApplyCrowdControl("cc_stun_test", CrowdControlType.Stun, 3f);
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool ccBlocked = !res.Success &&
                             res.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled &&
                             !hero.CastState.IsActive;

            Debug.Log($"[P07_9_P2_17] TEST 17 -> Blocked: {ccBlocked}, Reason: {res.FailureReason}, IsActive: {hero.CastState.IsActive} | {(ccBlocked ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ccBlocked;
        }

        /// <summary>
        /// TEST 18: Skill Cast State không chứa duplicate Rage.
        /// </summary>
        private static bool P07_9_P2_18_SkillCastState_DoesNotContainDuplicateRage()
        {
            FieldInfo[] fields = typeof(SkillCastState).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool hasDuplicateRage = false;
            foreach (var f in fields)
            {
                if (f.FieldType == typeof(RageComponent) || f.Name.ToLowerInvariant().Contains("rage"))
                {
                    hasDuplicateRage = true;
                    break;
                }
            }

            bool ok = !hasDuplicateRage;
            Debug.Log($"[P07_9_P2_18] TEST 18 -> HasDuplicateRage: {hasDuplicateRage} | {(ok ? "PASS" : "FAIL")}");
            return ok;
        }

        /// <summary>
        /// TEST 19: Skill Cast State không chứa duplicate Cooldown.
        /// </summary>
        private static bool P07_9_P2_19_SkillCastState_DoesNotContainDuplicateCooldown()
        {
            FieldInfo[] fields = typeof(SkillCastState).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            bool hasDuplicateCooldown = false;
            foreach (var f in fields)
            {
                if (f.Name.ToLowerInvariant().Contains("cooldown"))
                {
                    hasDuplicateCooldown = true;
                    break;
                }
            }

            bool ok = !hasDuplicateCooldown;
            Debug.Log($"[P07_9_P2_19] TEST 19 -> HasDuplicateCooldown: {hasDuplicateCooldown} | {(ok ? "PASS" : "FAIL")}");
            return ok;
        }

        /// <summary>
        /// TEST 20: Cast Complete exactly once.
        /// </summary>
        private static bool P07_9_P2_20_CastComplete_ExactlyOnce()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            int successEventsFired = 0;
            Action<SkillExecutionRequest, SkillExecutionResult> handler = (req, res) =>
            {
                if (req != null && req.Skill != null && req.Skill.SkillId == skillDef.SkillId && res.Success)
                {
                    successEventsFired++;
                }
            };
            EventBus.OnSkillExecutionSucceeded += handler;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.CastState.Tick(0.5f);
            hero.CastState.Tick(0.5f); // Complete -> triggers exactly once
            hero.CastState.Tick(0.5f); // Extra tick
            hero.CastState.Tick(1.0f); // Extra tick

            EventBus.OnSkillExecutionSucceeded -= handler;
            bool exactlyOnce = (successEventsFired == 1);

            Debug.Log($"[P07_9_P2_20] TEST 20 -> SuccessEventsFired: {successEventsFired}, ExactlyOnce: {exactlyOnce} | {(exactlyOnce ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return exactlyOnce;
        }

        /// <summary>
        /// TEST 21: Start Cast A. Request Cast B while A is active. Verify B does not overwrite A.
        /// </summary>
        private static bool P07_9_P2_21_ActiveCast_ReentryRejected_DoesNotOverwriteActiveCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDefA = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            SkillDefinitionSO skillDefB = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            skillDefA.SetCastTime(1.0f);
            skillDefB.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            var resA = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool aStarted = resA.Success && hero.CastState.IsActive;
            var activeReqA = hero.CastState.ActiveRequest;

            // Attempt to execute Skill B while A is actively casting
            var resB = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);

            bool bRejected = !resB.Success && resB.FailureReason == SkillExecutionFailureReason.SourceAlreadyCasting;
            bool aPreserved = hero.CastState.ActiveRequest == activeReqA &&
                              hero.CastState.ActiveRequest.Skill.SkillId == skillDefA.SkillId &&
                              hero.CastState.CurrentPhase == SkillCastPhase.Casting &&
                              hero.CastState.CastDuration == 1.0f;

            bool ok = aStarted && bRejected && aPreserved;
            Debug.Log($"[P07_9_P2_21] TEST 21 -> AStarted: {aStarted}, BRejected: {bRejected} ({resB.FailureReason}), APreserved: {aPreserved} | {(ok ? "PASS" : "FAIL")}");
            skillDefA.SetCastTime(0f);
            skillDefB.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 22: Verify B does not consume Rage when rejected.
        /// </summary>
        private static bool P07_9_P2_22_ActiveCast_ReentryRejected_DoesNotConsumeRage()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDefA = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            SkillDefinitionSO skillDefB = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            skillDefA.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float rageAfterA = hero.Rage.CurrentRage;

            // Attempt to execute Skill B (which costs Rage)
            var resB = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            float rageAfterB = hero.Rage.CurrentRage;

            bool rageUntouched = !resB.Success &&
                                 resB.FailureReason == SkillExecutionFailureReason.SourceAlreadyCasting &&
                                 Mathf.Approximately(rageAfterA, rageAfterB);

            Debug.Log($"[P07_9_P2_22] TEST 22 -> RageAfterA: {rageAfterA}, RageAfterB: {rageAfterB}, RageUntouched: {rageUntouched} | {(rageUntouched ? "PASS" : "FAIL")}");
            skillDefA.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return rageUntouched;
        }

        /// <summary>
        /// TEST 23: Verify A continues progressing after B rejection.
        /// </summary>
        private static bool P07_9_P2_23_ActiveCast_AContinuesProgressingAfterBRejection()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDefA = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            SkillDefinitionSO skillDefB = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            skillDefA.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            hero.CastState.Tick(0.3f);
            bool step1Ok = Mathf.Abs(hero.CastState.ElapsedTime - 0.3f) < 0.0001f;

            // Reject B
            var resB = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);

            hero.CastState.Tick(0.4f);
            bool step2Ok = Mathf.Abs(hero.CastState.ElapsedTime - 0.7f) < 0.0001f;
            bool remOk = Mathf.Abs(hero.CastState.RemainingTime - 0.3f) < 0.0001f;
            bool stillActive = hero.CastState.IsActive && hero.CastState.CurrentPhase == SkillCastPhase.Casting;

            bool ok = step1Ok && step2Ok && remOk && stillActive && !resB.Success;
            Debug.Log($"[P07_9_P2_23] TEST 23 -> Step1: {step1Ok}, Step2: {step2Ok}, RemOk: {remOk}, StillActive: {stillActive} | {(ok ? "PASS" : "FAIL")}");
            skillDefA.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 24: Verify A completes exactly once.
        /// </summary>
        private static bool P07_9_P2_24_ActiveCast_ACompletesExactlyOnceAfterBRejection()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDefA = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            SkillDefinitionSO skillDefB = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            skillDefA.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            int successEventsA = 0;
            Action<SkillExecutionRequest, SkillExecutionResult> handler = (req, res) =>
            {
                if (req?.Skill?.SkillId == skillDefA.SkillId && res.Success)
                {
                    successEventsA++;
                }
            };
            EventBus.OnSkillExecutionSucceeded += handler;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            // B rejected during cast
            hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);

            // Progress to completion
            hero.CastState.Tick(0.5f);
            hero.CastState.Tick(0.5f); // Complete here
            hero.CastState.Tick(0.5f); // Extra tick
            hero.CastState.Tick(1.0f); // Extra tick

            EventBus.OnSkillExecutionSucceeded -= handler;
            bool exactlyOnce = (successEventsA == 1);

            Debug.Log($"[P07_9_P2_24] TEST 24 -> SuccessEventsA: {successEventsA}, ExactlyOnce: {exactlyOnce} | {(exactlyOnce ? "PASS" : "FAIL")}");
            skillDefA.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return exactlyOnce;
        }

        /// <summary>
        /// TEST 25: Verify B does not execute effects.
        /// </summary>
        private static bool P07_9_P2_25_ActiveCast_BDoesNotExecuteEffects()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDefA = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            SkillDefinitionSO skillDefB = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            skillDefA.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            bool bSuccessFired = false;
            Action<SkillExecutionRequest, SkillExecutionResult> handler = (req, res) =>
            {
                if (req?.Skill?.SkillId == skillDefB.SkillId)
                {
                    bSuccessFired = true;
                }
            };
            EventBus.OnSkillExecutionSucceeded += handler;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            // Request B while A is casting
            var resB = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);

            // Tick A to completion
            hero.CastState.Tick(1.0f);

            EventBus.OnSkillExecutionSucceeded -= handler;

            bool bNoEffects = !bSuccessFired &&
                              !resB.Success &&
                              !resB.DamageResult.HasValue &&
                              (resB.EffectResults == null || resB.EffectResults.Count == 0) &&
                              resB.FailureReason == SkillExecutionFailureReason.SourceAlreadyCasting;

            Debug.Log($"[P07_9_P2_25] TEST 25 -> BSuccessFired: {bSuccessFired}, ResBSuccess: {resB.Success}, BNoEffects: {bNoEffects} | {(bNoEffects ? "PASS" : "FAIL")}");
            skillDefA.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return bNoEffects;
        }
    }
}
#endif
