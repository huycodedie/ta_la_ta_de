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
using WuxiaGame.Entities;
using WuxiaGame.Progression;
using WuxiaGame.Stats;

namespace WuxiaGame.Editor
{
    public static partial class Prototype01PlayTestRunner
    {
        private class TestPhase4Hero : Hero
        {
            public float SimulatedDeltaTime = 0.05f;

            protected override float GetDeltaTime()
            {
                return SimulatedDeltaTime;
            }

            public void CallUpdate(float dt = -1f)
            {
                if (dt > 0f) SimulatedDeltaTime = dt;
                base.Update();
            }
        }

        private static bool ApplyCC(Entity entity, CrowdControlType ccType, float duration, string ccId = null)
        {
            if (entity == null || entity.StatusController == null) return false;
            if (string.IsNullOrEmpty(ccId))
            {
                ccId = "cc_" + ccType.ToString().ToLower() + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            }
            return entity.StatusController.ApplyCrowdControl(ccId, ccType, duration);
        }

        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9 Phase 4 Tests (TEST 01 to TEST 30 + Play Mode Scenarios A-E)")]
        public static bool RunAllPrototype07_9_Phase4_Tests()
        {
            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 PHASE 4 TESTS (HARD-CC INTERRUPT)");
            Debug.Log("==================================================");

            int passed = 0;
            int total = 35; // 30 Automated Tests + 5 Play Mode Scenarios

            bool t01 = P07_9_P4_01_NormalCastCanStart();
            if (t01) passed++;

            bool t02 = P07_9_P4_02_NormalCastCanCompleteWithoutCC();
            if (t02) passed++;

            bool t03 = P07_9_P4_03_StunInterruptsActiveNormalCast();
            if (t03) passed++;

            bool t04 = P07_9_P4_04_InterruptedNormalCastExecutesNoDeferredEffects();
            if (t04) passed++;

            bool t05 = P07_9_P4_05_InterruptedNormalCastStartsNoCooldown();
            if (t05) passed++;

            bool t06 = P07_9_P4_06_InterruptedNormalCastDoesNotRefundRage();
            if (t06) passed++;

            bool t07 = P07_9_P4_07_FreezeInterruptsActiveNormalCast();
            if (t07) passed++;

            bool t08 = P07_9_P4_08_RootDoesNotInterruptNormalCast();
            if (t08) passed++;

            bool t09 = P07_9_P4_09_OrdinaryDamageDoesNotInterruptNormalCast();
            if (t09) passed++;

            bool t10 = P07_9_P4_10_StunInterruptsActiveChannel();
            if (t10) passed++;

            bool t11 = P07_9_P4_11_FreezeInterruptsActiveChannel();
            if (t11) passed++;

            bool t12 = P07_9_P4_12_ChannelTicksExecutedBeforeInterruptRemainValid();
            if (t12) passed++;

            bool t13 = P07_9_P4_13_ChannelTicksAfterInterruptDoNotExecute();
            if (t13) passed++;

            bool t14 = P07_9_P4_14_InterruptedChannelStartsNoCooldown();
            if (t14) passed++;

            bool t15 = P07_9_P4_15_InterruptedChannelDoesNotRefundRage();
            if (t15) passed++;

            bool t16 = P07_9_P4_16_ActiveCastReentryRemainsBlocked();
            if (t16) passed++;

            bool t17 = P07_9_P4_17_SecondInterruptIsIdempotent();
            if (t17) passed++;

            bool t18 = P07_9_P4_18_InterruptAfterCompletionDoesNothing();
            if (t18) passed++;

            bool t19 = P07_9_P4_19_AntiCCPreventsStunInterruption();
            if (t19) passed++;

            bool t20 = P07_9_P4_20_AntiCCPreventsFreezeInterruption();
            if (t20) passed++;

            bool t21 = P07_9_P4_21_CCResistanceBehaviorRemainsIntact();
            if (t21) passed++;

            bool t22 = P07_9_P4_22_CleanseAfterInterruptDoesNotResumeCast();
            if (t22) passed++;

            bool t23 = P07_9_P4_23_DispelDoesNotInterruptCast();
            if (t23) passed++;

            bool t24 = P07_9_P4_24_LargeDeltaAfterInterruptCannotTriggerCompletion();
            if (t24) passed++;

            bool t25 = P07_9_P4_25_CompletionBoundaryBeforeLaterCCRemainsCompleted();
            if (t25) passed++;

            bool t26 = P07_9_P4_26_AlreadyExecutedChannelTickNotRolledBack();
            if (t26) passed++;

            bool t27 = P07_9_P4_27_P07_8DamagePipelineRemainsIntact();
            if (t27) passed++;

            bool t28 = P07_9_P4_28_P07_8ShieldPipelineRemainsIntact();
            if (t28) passed++;

            bool t29 = P07_9_P4_29_InstantSkillBackwardCompatibility();
            if (t29) passed++;

            bool t30 = P07_9_P4_30_NormalCastTimeSkillBackwardCompatibility();
            if (t30) passed++;

            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 PHASE 4 PLAY MODE SCENARIOS A-E");
            Debug.Log("==================================================");

            bool pma = P07_9_P4_ScenarioA_StunCastInterrupt_PlayMode();
            if (pma) passed++;

            bool pmb = P07_9_P4_ScenarioB_FreezeCastInterrupt_PlayMode();
            if (pmb) passed++;

            bool pmc = P07_9_P4_ScenarioC_RootNonInterrupt_PlayMode();
            if (pmc) passed++;

            bool pmd = P07_9_P4_ScenarioD_DamageNonInterrupt_PlayMode();
            if (pmd) passed++;

            bool pme = P07_9_P4_ScenarioE_ChannelInterrupt_PlayMode();
            if (pme) passed++;

            Debug.Log($"[PROTOTYPE 07.9 PHASE 4 SUMMARY] Passed: {passed}/{total} ({(passed == total ? "ALL PASS" : "FAILURES DETECTED")})");
            return passed == total;
        }

        /// <summary>
        /// TEST 01: Normal cast can start.
        /// </summary>
        private static bool P07_9_P4_01_NormalCastCanStart()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool ok = res.Success && hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Casting) &&
                      Mathf.Approximately(hero.CastState.RemainingTime, 1.0f);
            Debug.Log($"[P07_9_P4_01] TEST 01 -> Success: {res.Success}, IsCasting: {hero.IsCasting}, Phase: {hero.CastState.CurrentPhase} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 02: Normal cast can complete without CC.
        /// </summary>
        private static bool P07_9_P4_02_NormalCastCanCompleteWithoutCC()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(1.0f);

            bool ok = !hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Recovery) &&
                      hero.CastState.HasExecutedEffects && CooldownManager.IsOnCooldown(skillDef.SkillId, out _);
            Debug.Log($"[P07_9_P4_02] TEST 02 -> Completed: {!hero.IsCasting}, Effects: {hero.CastState.HasExecutedEffects}, Cooldown: {CooldownManager.IsOnCooldown(skillDef.SkillId, out _)} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 03: Stun interrupts active normal cast.
        /// </summary>
        private static bool P07_9_P4_03_StunInterruptsActiveNormalCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            bool eventReceived = false;
            SkillCastInterruptSource receivedSource = SkillCastInterruptSource.None;
            Action<Entity, SkillDefinitionSO, SkillCastInterruptSource> handler = (src, sk, intSrc) =>
            {
                if (src == hero && sk == skillDef)
                {
                    eventReceived = true;
                    receivedSource = intSrc;
                }
            };
            EventBus.OnSkillCastInterrupted += handler;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.4f);

            // Apply Stun via existing CC authority
            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            EventBus.OnSkillCastInterrupted -= handler;

            bool ok = !hero.IsCasting && hero.CastState.IsInterrupted &&
                      (hero.CastState.InterruptSource == SkillCastInterruptSource.Stun) &&
                      eventReceived && (receivedSource == SkillCastInterruptSource.Stun);
            Debug.Log($"[P07_9_P4_03] TEST 03 -> IsCasting: {hero.IsCasting}, Interrupted: {hero.CastState.IsInterrupted}, Source: {hero.CastState.InterruptSource}, Event: {eventReceived} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 04: Interrupted normal cast executes NO deferred effects.
        /// </summary>
        private static bool P07_9_P4_04_InterruptedNormalCastExecutesNoDeferredEffects()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f);

            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            // Subsequent ticks should not execute effects
            hero.TickActiveCast(1.0f);

            bool ok = !hero.CastState.HasExecutedEffects &&
                      Mathf.Approximately(bm.CurrentMonster.Health.CurrentHealth, initialMonsterHp);
            Debug.Log($"[P07_9_P4_04] TEST 04 -> HasExecutedEffects: {hero.CastState.HasExecutedEffects}, MonsterHP: {bm.CurrentMonster.Health.CurrentHealth} (initial: {initialMonsterHp}) | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 05: Interrupted normal cast starts NO cooldown.
        /// </summary>
        private static bool P07_9_P4_05_InterruptedNormalCastStartsNoCooldown()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f);

            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            bool onCd = CooldownManager.IsOnCooldown(skillDef.SkillId, out float remain);
            bool ok = !onCd && (remain <= 0f);
            Debug.Log($"[P07_9_P4_05] TEST 05 -> OnCooldown: {onCd}, Remain: {remain} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 06: Interrupted normal cast does NOT refund Rage.
        /// </summary>
        private static bool P07_9_P4_06_InterruptedNormalCastDoesNotRefundRage()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);
            float rageBefore = hero.Rage.CurrentRage;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float rageAfterStart = hero.Rage.CurrentRage;

            hero.TickActiveCast(0.5f);
            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            float rageAfterInterrupt = hero.Rage.CurrentRage;

            bool ok = (rageAfterStart == rageBefore - skillDef.RageCost) &&
                      Mathf.Approximately(rageAfterInterrupt, rageAfterStart);
            Debug.Log($"[P07_9_P4_06] TEST 06 -> Before: {rageBefore}, AfterStart: {rageAfterStart}, AfterInterrupt: {rageAfterInterrupt} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 07: Freeze interrupts active normal cast.
        /// </summary>
        private static bool P07_9_P4_07_FreezeInterruptsActiveNormalCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.4f);

            // Apply Freeze via existing CC authority
            ApplyCC(hero, CrowdControlType.Freeze, 2.0f);

            bool ok = !hero.IsCasting && hero.CastState.IsInterrupted &&
                      (hero.CastState.InterruptSource == SkillCastInterruptSource.Freeze);
            Debug.Log($"[P07_9_P4_07] TEST 07 -> IsCasting: {hero.IsCasting}, Interrupted: {hero.CastState.IsInterrupted}, Source: {hero.CastState.InterruptSource} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 08: Root does NOT interrupt normal cast.
        /// </summary>
        private static bool P07_9_P4_08_RootDoesNotInterruptNormalCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.4f);

            // Apply Root via existing CC authority
            ApplyCC(hero, CrowdControlType.Root, 2.0f);

            bool stillCasting = hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Casting);

            // Complete the remaining 0.6s
            hero.TickActiveCast(0.6f);

            bool completed = !hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Recovery) &&
                             hero.CastState.HasExecutedEffects;

            bool ok = stillCasting && completed;
            Debug.Log($"[P07_9_P4_08] TEST 08 -> StillCastingAfterRoot: {stillCasting}, CompletedNormally: {completed} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 09: Ordinary damage does NOT interrupt normal cast.
        /// </summary>
        private static bool P07_9_P4_09_OrdinaryDamageDoesNotInterruptNormalCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.4f);

            // Deal non-lethal ordinary damage
            hero.Health.TakeDamage(10f);

            bool stillCasting = hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Casting);

            hero.TickActiveCast(0.6f);
            bool completed = !hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Recovery);

            bool ok = stillCasting && completed;
            Debug.Log($"[P07_9_P4_09] TEST 09 -> StillCastingAfterDamage: {stillCasting}, CompletedNormally: {completed} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 10: Stun interrupts active Channel.
        /// </summary>
        private static bool P07_9_P4_10_StunInterruptsActiveChannel()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.6f); // Tick 1 executes

            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            bool ok = !hero.IsCasting && hero.CastState.IsInterrupted &&
                      (hero.CastState.InterruptSource == SkillCastInterruptSource.Stun);
            Debug.Log($"[P07_9_P4_10] TEST 10 -> ChannelInterrupted: {hero.CastState.IsInterrupted}, Source: {hero.CastState.InterruptSource} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 11: Freeze interrupts active Channel.
        /// </summary>
        private static bool P07_9_P4_11_FreezeInterruptsActiveChannel()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.6f); // Tick 1 executes

            ApplyCC(hero, CrowdControlType.Freeze, 2.0f);

            bool ok = !hero.IsCasting && hero.CastState.IsInterrupted &&
                      (hero.CastState.InterruptSource == SkillCastInterruptSource.Freeze);
            Debug.Log($"[P07_9_P4_11] TEST 11 -> ChannelInterrupted: {hero.CastState.IsInterrupted}, Source: {hero.CastState.InterruptSource} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 12: Channel ticks executed BEFORE interrupt remain valid.
        /// </summary>
        private static bool P07_9_P4_12_ChannelTicksExecutedBeforeInterruptRemainValid()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.6f); // Tick 1 executed
            float hpAfterTick1 = bm.CurrentMonster.Health.CurrentHealth;

            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            bool ok = (hero.CastState.ChannelTicksExecuted == 1) && (hpAfterTick1 < initialMonsterHp) &&
                      Mathf.Approximately(bm.CurrentMonster.Health.CurrentHealth, hpAfterTick1);
            Debug.Log($"[P07_9_P4_12] TEST 12 -> TicksBefore: {hero.CastState.ChannelTicksExecuted}, MonsterHP: {bm.CurrentMonster.Health.CurrentHealth} (afterTick1: {hpAfterTick1}) | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 13: Channel ticks AFTER interrupt do NOT execute.
        /// </summary>
        private static bool P07_9_P4_13_ChannelTicksAfterInterruptDoNotExecute()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.6f); // Tick 1 executed
            float hpAfterTick1 = bm.CurrentMonster.Health.CurrentHealth;

            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            // Tick past what would have been tick 2, 3, and completion
            hero.TickActiveCast(2.0f);

            bool ok = (hero.CastState.ChannelTicksExecuted == 1) &&
                      Mathf.Approximately(bm.CurrentMonster.Health.CurrentHealth, hpAfterTick1);
            Debug.Log($"[P07_9_P4_13] TEST 13 -> TicksAfter: {hero.CastState.ChannelTicksExecuted} (expected 1), MonsterHP: {bm.CurrentMonster.Health.CurrentHealth} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 14: Interrupted Channel starts NO cooldown.
        /// </summary>
        private static bool P07_9_P4_14_InterruptedChannelStartsNoCooldown()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.6f);

            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            bool onCd = CooldownManager.IsOnCooldown(skillDef.SkillId, out float remain);
            bool ok = !onCd && (remain <= 0f);
            Debug.Log($"[P07_9_P4_14] TEST 14 -> Channel OnCd: {onCd}, Remain: {remain} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 15: Interrupted Channel does NOT refund Rage.
        /// </summary>
        private static bool P07_9_P4_15_InterruptedChannelDoesNotRefundRage()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);
            float rageBefore = hero.Rage.CurrentRage;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float rageAfterStart = hero.Rage.CurrentRage;

            hero.TickActiveCast(0.6f);
            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            float rageAfterInterrupt = hero.Rage.CurrentRage;

            bool ok = (rageAfterStart == rageBefore - skillDef.RageCost) &&
                      Mathf.Approximately(rageAfterInterrupt, rageAfterStart);
            Debug.Log($"[P07_9_P4_15] TEST 15 -> Before: {rageBefore}, AfterStart: {rageAfterStart}, AfterInterrupt: {rageAfterInterrupt} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 16: Active cast re-entry remains blocked.
        /// </summary>
        private static bool P07_9_P4_16_ActiveCastReentryRemainsBlocked()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDefA = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            SkillDefinitionSO skillDefB = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            skillDefA.SetCastTime(1.0f);
            skillDefA.SetChannel(false, 0f, 0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            var resB = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);

            bool ok = !resB.Success && (resB.FailureReason == SkillExecutionFailureReason.SourceAlreadyCasting);
            Debug.Log($"[P07_9_P4_16] TEST 16 -> ReentryRejected: {!resB.Success}, Reason: {resB.FailureReason} | {(ok ? "PASS" : "FAIL")}");
            skillDefA.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 17: Second interrupt is idempotent.
        /// </summary>
        private static bool P07_9_P4_17_SecondInterruptIsIdempotent()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            int interruptEventCount = 0;
            Action<Entity, SkillDefinitionSO, SkillCastInterruptSource> handler = (src, sk, intSrc) =>
            {
                if (src == hero) interruptEventCount++;
            };
            EventBus.OnSkillCastInterrupted += handler;

            // First interrupt
            hero.InterruptCurrentAction(SkillCastInterruptSource.Stun);
            // Second interrupt (should be a safe no-op)
            hero.InterruptCurrentAction(SkillCastInterruptSource.Freeze);
            hero.CastState.Interrupt(SkillCastInterruptSource.ManualCancel);

            EventBus.OnSkillCastInterrupted -= handler;

            bool ok = hero.CastState.IsInterrupted &&
                      (hero.CastState.InterruptSource == SkillCastInterruptSource.Stun) &&
                      (interruptEventCount == 1);
            Debug.Log($"[P07_9_P4_17] TEST 17 -> Phase: {hero.CastState.CurrentPhase}, Source: {hero.CastState.InterruptSource}, Events: {interruptEventCount} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 18: Interrupt after completion does nothing.
        /// </summary>
        private static bool P07_9_P4_18_InterruptAfterCompletionDoesNothing()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(1.0f); // completes

            bool wasRecovery = (hero.CastState.CurrentPhase == SkillCastPhase.Recovery);

            // Attempt interrupt on completed state
            hero.InterruptCurrentAction(SkillCastInterruptSource.Stun);

            bool ok = wasRecovery && (hero.CastState.CurrentPhase == SkillCastPhase.Recovery) &&
                      !hero.CastState.IsInterrupted && CooldownManager.IsOnCooldown(skillDef.SkillId, out _);
            Debug.Log($"[P07_9_P4_18] TEST 18 -> Phase: {hero.CastState.CurrentPhase} (expected Recovery), IsInterrupted: {hero.CastState.IsInterrupted} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 19: Anti-CC prevents Stun interruption when CC is prevented.
        /// </summary>
        private static bool P07_9_P4_19_AntiCCPreventsStunInterruption()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            // Give hero Anti-CC Immunity
            hero.StatusController.ApplyAntiCCImmunity(5.0f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            // Attempt Stun - should be blocked by Anti-CC
            bool ccApplied = ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            bool stillCasting = hero.IsCasting && !hero.IsStunned;

            // Complete the cast
            hero.TickActiveCast(0.7f);
            bool completed = !hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Recovery);

            bool ok = !ccApplied && stillCasting && completed;
            Debug.Log($"[P07_9_P4_19] TEST 19 -> CcApplied: {ccApplied}, StillCasting: {stillCasting}, Completed: {completed} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 20: Anti-CC prevents Freeze interruption when CC is prevented.
        /// </summary>
        private static bool P07_9_P4_20_AntiCCPreventsFreezeInterruption()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            // Give hero Anti-CC Immunity
            hero.StatusController.ApplyAntiCCImmunity(5.0f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            // Attempt Freeze - should be blocked by Anti-CC
            bool ccApplied = ApplyCC(hero, CrowdControlType.Freeze, 2.0f);

            bool stillCasting = hero.IsCasting && !hero.IsFrozen;

            // Complete the cast
            hero.TickActiveCast(0.7f);
            bool completed = !hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Recovery);

            bool ok = !ccApplied && stillCasting && completed;
            Debug.Log($"[P07_9_P4_20] TEST 20 -> CcApplied: {ccApplied}, StillCasting: {stillCasting}, Completed: {completed} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 21: CC Resistance behavior remains intact.
        /// </summary>
        private static bool P07_9_P4_21_CCResistanceBehaviorRemainsIntact()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            // Set CC resistance to 50%
            hero.Stats.SetBaseValue(StatType.CcResistance, 0.5f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            // Stun applied with 2.0s base duration -> effective duration 1.0s
            bool applied = ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            bool ok = applied && hero.IsStunned && !hero.IsCasting && hero.CastState.IsInterrupted;
            Debug.Log($"[P07_9_P4_21] TEST 21 -> Applied: {applied}, IsStunned: {hero.IsStunned}, Interrupted: {hero.CastState.IsInterrupted} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 22: Cleanse after interrupt does NOT resume cast.
        /// </summary>
        private static bool P07_9_P4_22_CleanseAfterInterruptDoesNotResumeCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC(hero, CrowdControlType.Stun, 2.0f);
            bool wasInterrupted = hero.CastState.IsInterrupted;

            // Cleanse Stun
            hero.StatusController.Cleanse(StatusRemovalCategory.CrowdControl);
            bool stunRemoved = !hero.IsStunned;

            // Tick more time - cast must NOT resume
            hero.TickActiveCast(1.0f);

            bool ok = wasInterrupted && stunRemoved && !hero.IsCasting &&
                      hero.CastState.IsInterrupted && !hero.CastState.HasExecutedEffects;
            Debug.Log($"[P07_9_P4_22] TEST 22 -> WasInterrupted: {wasInterrupted}, StunRemoved: {stunRemoved}, RemainsInterrupted: {hero.CastState.IsInterrupted} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 23: Dispel does NOT interrupt cast.
        /// </summary>
        private static bool P07_9_P4_23_DispelDoesNotInterruptCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            // Apply a buff to hero
            var buff = ScriptableObject.CreateInstance<BuffEffectDefinitionSO>();
            buff.Initialize("buff_dispel_test", "Test Buff", StatType.Attack, 10f, 10f, BuffStackingPolicy.RefreshDuration);
            hero.StatusController.ApplyBuff(buff, hero);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.4f);

            // Dispel the buff
            hero.StatusController.Dispel();

            bool stillCasting = hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Casting);

            hero.TickActiveCast(0.6f);
            bool completed = !hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Recovery);

            bool ok = stillCasting && completed;
            Debug.Log($"[P07_9_P4_23] TEST 23 -> StillCastingAfterDispel: {stillCasting}, Completed: {completed} | {(ok ? "PASS" : "FAIL")}");
            UnityEngine.Object.DestroyImmediate(buff);
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 24: Large delta after interrupt cannot trigger completion.
        /// </summary>
        private static bool P07_9_P4_24_LargeDeltaAfterInterruptCannotTriggerCompletion()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            // Large delta tick: 100 seconds
            hero.TickActiveCast(100.0f);

            bool ok = !hero.IsCasting && hero.CastState.IsInterrupted &&
                      !hero.CastState.HasExecutedEffects &&
                      !CooldownManager.IsOnCooldown(skillDef.SkillId, out _);
            Debug.Log($"[P07_9_P4_24] TEST 24 -> InterruptedAfterLargeDelta: {hero.CastState.IsInterrupted}, NoEffects: {!hero.CastState.HasExecutedEffects} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 25: Completion boundary before later CC remains completed.
        /// </summary>
        private static bool P07_9_P4_25_CompletionBoundaryBeforeLaterCCRemainsCompleted()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(1.0f); // Reaches completion boundary

            bool wasCompleted = (hero.CastState.CurrentPhase == SkillCastPhase.Recovery) && hero.CastState.HasExecutedEffects;

            // Stun applied afterwards
            ApplyCC(hero, CrowdControlType.Stun, 2.0f);

            bool ok = wasCompleted && (hero.CastState.CurrentPhase == SkillCastPhase.Recovery) &&
                      !hero.CastState.IsInterrupted && CooldownManager.IsOnCooldown(skillDef.SkillId, out _);
            Debug.Log($"[P07_9_P4_25] TEST 25 -> CompletedRemainsCompleted: {ok}, Phase: {hero.CastState.CurrentPhase} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 26: Already executed Channel tick is not rolled back.
        /// </summary>
        private static bool P07_9_P4_26_AlreadyExecutedChannelTickNotRolledBack()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(1.1f); // 2 ticks executed (0.5s, 1.0s)

            float hpAfter2Ticks = bm.CurrentMonster.Health.CurrentHealth;

            ApplyCC(hero, CrowdControlType.Freeze, 2.0f);

            bool ok = (hero.CastState.ChannelTicksExecuted == 2) &&
                      Mathf.Approximately(bm.CurrentMonster.Health.CurrentHealth, hpAfter2Ticks);
            Debug.Log($"[P07_9_P4_26] TEST 26 -> TicksExecuted: {hero.CastState.ChannelTicksExecuted} (expected 2), HP: {bm.CurrentMonster.Health.CurrentHealth} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 27: P07.8 damage pipeline remains intact.
        /// </summary>
        private static bool P07_9_P4_27_P07_8DamagePipelineRemainsIntact()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f); // 1 tick

            float damagedHp = bm.CurrentMonster.Health.CurrentHealth;
            bool ok = (damagedHp < initialMonsterHp) && (damagedHp > 0f);
            Debug.Log($"[P07_9_P4_27] TEST 27 -> InitialHP: {initialMonsterHp}, DamagedHP: {damagedHp} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 28: P07.8 Shield pipeline remains intact.
        /// </summary>
        private static bool P07_9_P4_28_P07_8ShieldPipelineRemainsIntact()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            // Apply 500 shield to monster
            bm.CurrentMonster.StatusController.ApplyShield("shield_p4_test", 500f, 30f);
            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f); // 1 tick

            var shield = bm.CurrentMonster.StatusController.GetShield("shield_p4_test");
            float remShield = shield != null ? shield.CurrentAmount : -1f;

            // Shield should have absorbed the tick damage, leaving Monster HP intact
            bool ok = (remShield < 500f) && (remShield >= 0f) &&
                      Mathf.Approximately(bm.CurrentMonster.Health.CurrentHealth, initialMonsterHp);

            // Now interrupt with Stun and verify shield stays at absorbed amount
            ApplyCC(hero, CrowdControlType.Stun, 2.0f);
            hero.TickActiveCast(1.0f);

            var shieldAfterInterrupt = bm.CurrentMonster.StatusController.GetShield("shield_p4_test");
            float remShieldAfter = shieldAfterInterrupt != null ? shieldAfterInterrupt.CurrentAmount : -1f;

            bool ok2 = ok && Mathf.Approximately(remShield, remShieldAfter);
            Debug.Log($"[P07_9_P4_28] TEST 28 -> ShieldAbsorbed: {remShield} (initial 500), RemShieldAfter: {remShieldAfter} | {(ok2 ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok2;
        }

        /// <summary>
        /// TEST 29: Instant skill backward compatibility remains intact.
        /// </summary>
        private static bool P07_9_P4_29_InstantSkillBackwardCompatibility()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(false, 0f, 0f);
            hero.Rage.AddRage(100f);

            float initialHp = bm.CurrentMonster.Health.CurrentHealth;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool ok = res.Success && !hero.IsCasting &&
                      (bm.CurrentMonster.Health.CurrentHealth < initialHp) &&
                      CooldownManager.IsOnCooldown(skillDef.SkillId, out _);
            Debug.Log($"[P07_9_P4_29] TEST 29 -> Instant Success: {res.Success}, IsCasting: {hero.IsCasting}, Cooldown: {CooldownManager.IsOnCooldown(skillDef.SkillId, out _)} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 30: Normal cast-time skill backward compatibility remains intact.
        /// </summary>
        private static bool P07_9_P4_30_NormalCastTimeSkillBackwardCompatibility()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            skillDef.SetChannel(false, 0f, 0f);
            hero.Rage.AddRage(100f);

            float initialHp = bm.CurrentMonster.Health.CurrentHealth;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool startsCasting = res.Success && hero.IsCasting;
            hero.TickActiveCast(1.0f);

            bool ok = startsCasting && !hero.IsCasting &&
                      (hero.CastState.CurrentPhase == SkillCastPhase.Recovery) &&
                      (bm.CurrentMonster.Health.CurrentHealth < initialHp) &&
                      CooldownManager.IsOnCooldown(skillDef.SkillId, out _);
            Debug.Log($"[P07_9_P4_30] TEST 30 -> CastTime Success: {res.Success}, CompletedNormally: {ok} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        // =========================================================================
        // PLAY MODE ACCEPTANCE SCENARIOS A - E (Section 29)
        // =========================================================================

        private static bool SetupPlayModeScene(out TestPhase4Hero testHero, out BattleManager bm, out MindMethodManager mmMgr)
        {
            testHero = null;
            bm = null;
            mmMgr = null;

            Prototype01SceneBuilder.BuildPrototype02Data();
            Prototype01SceneBuilder.BuildPrototype01Scene();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);

            Hero sceneHero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            mmMgr = UnityEngine.Object.FindAnyObjectByType<MindMethodManager>();

            if (sceneHero == null || bm == null || mmMgr == null)
            {
                Debug.LogError("[PLAY MODE SETUP] Missing essential scene components!");
                return false;
            }

            GameObject heroGO = sceneHero.gameObject;
            UnityEngine.Object.DestroyImmediate(sceneHero);
            testHero = heroGO.AddComponent<TestPhase4Hero>();
            testHero.InitializeHero();
            testHero.Health.Revive(testHero.Health.MaxHealth);
            testHero.EnableEntityActions();
            testHero.Rage.AddRage(100f);
            bm.RegisterHero(testHero);

            if (bm.CurrentMonster == null)
            {
                bm.SpawnMonster();
            }
            bm.CurrentMonster.Health.Revive(bm.CurrentMonster.Health.MaxHealth);
            bm.CurrentMonster.EnableEntityActions();

            if (!bm.IsBattleActive)
            {
                bm.StartBattle();
            }

            mmMgr.LoadDatabaseIfMissing();
            mmMgr.ResetPersistence();
            mmMgr.InitializeFromDatabase();
            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            return true;
        }

        /// <summary>
        /// SCENARIO A: STUN CAST INTERRUPT (Section 29)
        /// 1. Start cast with test fixture CastTime.
        /// 2. Allow partial cast progression through actual runtime (Entity.Update).
        /// 3. Apply Stun through EXISTING CC authority.
        /// 4. Verify cast terminates.
        /// 5. Verify deferred effects do not execute.
        /// 6. Verify cooldown does not start.
        /// 7. Verify Rage remains consumed.
        /// </summary>
        private static bool P07_9_P4_ScenarioA_StunCastInterrupt_PlayMode()
        {
            if (!SetupPlayModeScene(out TestPhase4Hero testHero, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_CAST_TIME = 1.0f; // TEST FIXTURE ONLY per Section 29
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(TEST_CAST_TIME);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;
            float initialHeroRage = testHero.Rage.CurrentRage;

            // 1. Start cast
            var startRes = testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool startOk = startRes.Success && testHero.IsCasting;
            float rageAfterStart = testHero.Rage.CurrentRage;

            // 2. Allow partial cast progression through actual runtime Entity.Update()
            testHero.CallUpdate(0.4f);
            bool partialOk = testHero.IsCasting && (testHero.CastState.ElapsedTime >= 0.4f);

            // 3. Apply Stun through EXISTING CC authority
            ApplyCC(testHero, CrowdControlType.Stun, 2.0f);

            // 4. Verify cast terminates
            bool terminatedOk = !testHero.IsCasting && testHero.CastState.IsInterrupted &&
                                (testHero.CastState.InterruptSource == SkillCastInterruptSource.Stun);

            // 5. Verify deferred effects do not execute even after further updates
            testHero.CallUpdate(1.0f);
            bool noDeferredEffects = !testHero.CastState.HasExecutedEffects &&
                                     Mathf.Approximately(bm.CurrentMonster.Health.CurrentHealth, initialMonsterHp);

            // 6. Verify cooldown does not start
            bool noCooldown = !CooldownManager.IsOnCooldown(skillDef.SkillId, out _);

            // 7. Verify Rage remains consumed
            bool rageConsumed = Mathf.Approximately(testHero.Rage.CurrentRage, rageAfterStart) &&
                                (rageAfterStart < initialHeroRage);

            bool scenarioPass = startOk && partialOk && terminatedOk && noDeferredEffects && noCooldown && rageConsumed;
            Debug.Log($"[SCENARIO A — STUN CAST INTERRUPT] Result: {(scenarioPass ? "PASS" : "FAIL")}\n" +
                      $"1. StartCast: {startOk}\n" +
                      $"2. PartialProgression: {partialOk}\n" +
                      $"3-4. TerminatedByStun: {terminatedOk}\n" +
                      $"5. NoDeferredEffects: {noDeferredEffects}\n" +
                      $"6. NoCooldown: {noCooldown}\n" +
                      $"7. RageRemainsConsumed: {rageConsumed} ({initialHeroRage}->{testHero.Rage.CurrentRage})");

            skillDef.SetCastTime(0f);
            return scenarioPass;
        }

        /// <summary>
        /// SCENARIO B: FREEZE CAST INTERRUPT (Section 29)
        /// Same structure using Freeze.
        /// </summary>
        private static bool P07_9_P4_ScenarioB_FreezeCastInterrupt_PlayMode()
        {
            if (!SetupPlayModeScene(out TestPhase4Hero testHero, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_CAST_TIME = 1.0f; // TEST FIXTURE ONLY per Section 29
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(TEST_CAST_TIME);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;
            float initialHeroRage = testHero.Rage.CurrentRage;

            // 1. Start cast
            var startRes = testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool startOk = startRes.Success && testHero.IsCasting;
            float rageAfterStart = testHero.Rage.CurrentRage;

            // 2. Allow partial cast progression through actual runtime Entity.Update()
            testHero.CallUpdate(0.4f);
            bool partialOk = testHero.IsCasting && (testHero.CastState.ElapsedTime >= 0.4f);

            // 3. Apply Freeze through EXISTING CC authority
            ApplyCC(testHero, CrowdControlType.Freeze, 2.0f);

            // 4. Verify cast terminates
            bool terminatedOk = !testHero.IsCasting && testHero.CastState.IsInterrupted &&
                                (testHero.CastState.InterruptSource == SkillCastInterruptSource.Freeze);

            // 5. Verify deferred effects do not execute even after further updates
            testHero.CallUpdate(1.0f);
            bool noDeferredEffects = !testHero.CastState.HasExecutedEffects &&
                                     Mathf.Approximately(bm.CurrentMonster.Health.CurrentHealth, initialMonsterHp);

            // 6. Verify cooldown does not start
            bool noCooldown = !CooldownManager.IsOnCooldown(skillDef.SkillId, out _);

            // 7. Verify Rage remains consumed
            bool rageConsumed = Mathf.Approximately(testHero.Rage.CurrentRage, rageAfterStart) &&
                                (rageAfterStart < initialHeroRage);

            bool scenarioPass = startOk && partialOk && terminatedOk && noDeferredEffects && noCooldown && rageConsumed;
            Debug.Log($"[SCENARIO B — FREEZE CAST INTERRUPT] Result: {(scenarioPass ? "PASS" : "FAIL")}\n" +
                      $"1. StartCast: {startOk}\n" +
                      $"2. PartialProgression: {partialOk}\n" +
                      $"3-4. TerminatedByFreeze: {terminatedOk}\n" +
                      $"5. NoDeferredEffects: {noDeferredEffects}\n" +
                      $"6. NoCooldown: {noCooldown}\n" +
                      $"7. RageRemainsConsumed: {rageConsumed} ({initialHeroRage}->{testHero.Rage.CurrentRage})");

            skillDef.SetCastTime(0f);
            return scenarioPass;
        }

        /// <summary>
        /// SCENARIO C: ROOT NON-INTERRUPT (Section 29)
        /// 1. Start cast.
        /// 2. Apply Root.
        /// 3. Verify cast continues.
        /// 4. Verify completion.
        /// 5. Verify cooldown starts normally.
        /// </summary>
        private static bool P07_9_P4_ScenarioC_RootNonInterrupt_PlayMode()
        {
            if (!SetupPlayModeScene(out TestPhase4Hero testHero, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_CAST_TIME = 1.0f; // TEST FIXTURE ONLY per Section 29
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(TEST_CAST_TIME);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            // 1. Start cast
            var startRes = testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool startOk = startRes.Success && testHero.IsCasting;

            // 2. Apply Root
            ApplyCC(testHero, CrowdControlType.Root, 2.0f);

            // 3. Verify cast continues through Entity.Update()
            testHero.CallUpdate(0.5f);
            bool continuesOk = testHero.IsCasting && (testHero.CastState.CurrentPhase == SkillCastPhase.Casting);

            // 4. Verify completion through Entity.Update()
            testHero.CallUpdate(0.5f);
            bool completedOk = !testHero.IsCasting && (testHero.CastState.CurrentPhase == SkillCastPhase.Recovery) &&
                               testHero.CastState.HasExecutedEffects &&
                               (bm.CurrentMonster.Health.CurrentHealth < initialMonsterHp);

            // 5. Verify cooldown starts normally
            bool cdStarted = CooldownManager.IsOnCooldown(skillDef.SkillId, out _);

            bool scenarioPass = startOk && continuesOk && completedOk && cdStarted;
            Debug.Log($"[SCENARIO C — ROOT NON-INTERRUPT] Result: {(scenarioPass ? "PASS" : "FAIL")}\n" +
                      $"1. StartCast: {startOk}\n" +
                      $"2-3. ContinuesUnderRoot: {continuesOk}\n" +
                      $"4. CompletedNormally: {completedOk}\n" +
                      $"5. CooldownStarted: {cdStarted}");

            skillDef.SetCastTime(0f);
            return scenarioPass;
        }

        /// <summary>
        /// SCENARIO D: DAMAGE NON-INTERRUPT (Section 29)
        /// 1. Start cast.
        /// 2. Deal ordinary damage.
        /// 3. Verify cast continues.
        /// 4. Verify completion.
        /// </summary>
        private static bool P07_9_P4_ScenarioD_DamageNonInterrupt_PlayMode()
        {
            if (!SetupPlayModeScene(out TestPhase4Hero testHero, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_CAST_TIME = 1.0f; // TEST FIXTURE ONLY per Section 29
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(TEST_CAST_TIME);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            // 1. Start cast
            var startRes = testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool startOk = startRes.Success && testHero.IsCasting;

            // 2. Deal ordinary damage
            testHero.Health.TakeDamage(10f);

            // 3. Verify cast continues through Entity.Update()
            testHero.CallUpdate(0.5f);
            bool continuesOk = testHero.IsCasting && (testHero.CastState.CurrentPhase == SkillCastPhase.Casting);

            // 4. Verify completion through Entity.Update()
            testHero.CallUpdate(0.5f);
            bool completedOk = !testHero.IsCasting && (testHero.CastState.CurrentPhase == SkillCastPhase.Recovery) &&
                               testHero.CastState.HasExecutedEffects &&
                               (bm.CurrentMonster.Health.CurrentHealth < initialMonsterHp);

            bool scenarioPass = startOk && continuesOk && completedOk;
            Debug.Log($"[SCENARIO D — DAMAGE NON-INTERRUPT] Result: {(scenarioPass ? "PASS" : "FAIL")}\n" +
                      $"1. StartCast: {startOk}\n" +
                      $"2-3. ContinuesUnderDamage: {continuesOk}\n" +
                      $"4. CompletedNormally: {completedOk}");

            skillDef.SetCastTime(0f);
            return scenarioPass;
        }

        /// <summary>
        /// SCENARIO E: CHANNEL INTERRUPT (Section 29)
        /// 1. Start Channel.
        /// 2. Allow at least one tick.
        /// 3. Apply Stun or Freeze.
        /// 4. Verify future ticks stop.
        /// 5. Verify no completion.
        /// 6. Verify no cooldown.
        /// 7. Verify previous tick remains valid.
        /// </summary>
        private static bool P07_9_P4_ScenarioE_ChannelInterrupt_PlayMode()
        {
            if (!SetupPlayModeScene(out TestPhase4Hero testHero, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_DURATION = 2.0f; // TEST FIXTURE ONLY per Section 29
            const float TEST_INTERVAL = 0.5f; // TEST FIXTURE ONLY per Section 29
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, TEST_DURATION, TEST_INTERVAL);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            // 1. Start Channel
            var startRes = testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool startOk = startRes.Success && testHero.IsCasting;

            // 2. Allow at least one tick through Entity.Update()
            testHero.CallUpdate(0.6f);
            bool tick1Ok = (testHero.CastState.ChannelTicksExecuted == 1);
            float hpAfterTick1 = bm.CurrentMonster.Health.CurrentHealth;
            bool damageDealt = hpAfterTick1 < initialMonsterHp;

            // 3. Apply Stun
            ApplyCC(testHero, CrowdControlType.Stun, 2.0f);
            bool interruptedOk = !testHero.IsCasting && testHero.CastState.IsInterrupted;

            // 4. Verify future ticks stop through Entity.Update()
            testHero.CallUpdate(1.0f);
            testHero.CallUpdate(1.0f);
            bool ticksStopped = (testHero.CastState.ChannelTicksExecuted == 1);

            // 5. Verify no completion
            bool noCompletion = (testHero.CastState.CurrentPhase == SkillCastPhase.Interrupted);

            // 6. Verify no cooldown
            bool noCooldown = !CooldownManager.IsOnCooldown(skillDef.SkillId, out _);

            // 7. Verify previous tick remains valid
            bool tickRemainsValid = damageDealt && Mathf.Approximately(bm.CurrentMonster.Health.CurrentHealth, hpAfterTick1);

            bool scenarioPass = startOk && tick1Ok && interruptedOk && ticksStopped &&
                                noCompletion && noCooldown && tickRemainsValid;

            Debug.Log($"[SCENARIO E — CHANNEL INTERRUPT] Result: {(scenarioPass ? "PASS" : "FAIL")}\n" +
                      $"1. StartChannel: {startOk}\n" +
                      $"2. Tick 1 Executed: {tick1Ok} (HP: {initialMonsterHp}->{hpAfterTick1})\n" +
                      $"3. Interrupted: {interruptedOk}\n" +
                      $"4. FutureTicksStopped: {ticksStopped} (Ticks={testHero.CastState.ChannelTicksExecuted})\n" +
                      $"5. NoCompletion: {noCompletion}\n" +
                      $"6. NoCooldown: {noCooldown}\n" +
                      $"7. PreviousTickRemainsValid: {tickRemainsValid}");

            skillDef.SetChannel(false, 0f, 0f);
            return scenarioPass;
        }
    }
}
#endif
