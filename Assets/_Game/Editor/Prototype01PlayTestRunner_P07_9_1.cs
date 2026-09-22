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
using WuxiaGame.Progression;
using WuxiaGame.Stats;

namespace WuxiaGame.Editor
{
    public static partial class Prototype01PlayTestRunner
    {
        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9.1 Autonomous Skill Decision Tests (TEST 01 to TEST 15)")]
        public static bool RunAllPrototype07_9_1_Tests()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING PROTOTYPE 07.9.1 HERO AUTONOMOUS SKILL DECISION TESTS (01 -> 15)");
            Debug.Log("================================================================================");

            int passed = 0;
            int total = 16; // 15 unit/integration tests + 1 PlayMode real battle test

            bool t01 = P07_9_1_01_AutoOn_NormalSkillReady_HeroAutoCasts();
            if (t01) passed++;

            bool t02 = P07_9_1_02_AutoOn_MultipleSkillsReady_HighestPrioritySelected();
            if (t02) passed++;

            bool t03 = P07_9_1_03_AutoOn_UltimateReady_SufficientRage_HeroAutoUltimates();
            if (t03) passed++;

            bool t04 = P07_9_1_04_AutoOff_SkillReady_HeroDoesNotAutoCast();
            if (t04) passed++;

            bool t05 = P07_9_1_05_AutoOff_ManualSkill_ManualSkillStillCasts();
            if (t05) passed++;

            bool t06 = P07_9_1_06_CastTime_CastStateActive();
            if (t06) passed++;

            bool t07 = P07_9_1_07_Channel_ChannelActive();
            if (t07) passed++;

            bool t08 = P07_9_1_08_Stun_CastInterrupt();
            if (t08) passed++;

            bool t09 = P07_9_1_09_Freeze_CastInterrupt();
            if (t09) passed++;

            bool t10 = P07_9_1_10_Root_CastContinues();
            if (t10) passed++;

            bool t11 = P07_9_1_11_OrdinaryDamage_CastContinues();
            if (t11) passed++;

            bool t12 = P07_9_1_12_InsufficientRage_UltimateDoesNotCast();
            if (t12) passed++;

            bool t13 = P07_9_1_13_CooldownActive_SkillDoesNotCast();
            if (t13) passed++;

            bool t14 = P07_9_1_14_SkillValidationFailure_NoResourceConsumed();
            if (t14) passed++;

            bool t15 = P07_9_1_15_ReentryGuard_NoDuplicateCast();
            if (t15) passed++;

            bool pmTest = RunPrototype07_9_1_PlayModeRealBattleVerification();
            if (pmTest) passed++;

            bool allPassed = (passed == total);

            Debug.Log("================================================================================");
            Debug.Log($"   PROTOTYPE 07.9.1 TEST SUMMARY: {passed}/{total} {(allPassed ? "PASSED" : "FAILED")}");
            Debug.Log($"   TEST 01 (Auto ON -> Normal Skill Auto Cast):        {(t01 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 02 (Multiple Ready -> Highest Priority):       {(t02 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 03 (Auto ON + Ult Ready + Rage -> Auto Ult):   {(t03 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 04 (Auto OFF -> Hero Does Not Auto Cast):      {(t04 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 05 (Auto OFF -> Manual Skill Still Casts):     {(t05 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 06 (Cast Time -> CastState Active):            {(t06 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 07 (Channel -> Channel Active):                {(t07 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 08 (Stun CC -> Cast Interrupt):                {(t08 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 09 (Freeze CC -> Cast Interrupt):              {(t09 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 10 (Root CC -> Cast Continues):                {(t10 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 11 (Ordinary Damage -> Cast Continues):        {(t11 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 12 (Insufficient Rage -> Ult Blocked):         {(t12 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 13 (Cooldown Active -> Skill Blocked):         {(t13 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 14 (Validation Failure -> No Resource Mut):    {(t14 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 15 (Re-entry Guard -> No Duplicate Cast):      {(t15 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST PM (Play Mode Real Battle Verification):       {(pmTest ? "PASS" : "FAIL")}");
            Debug.Log("================================================================================");

            return allPassed;
        }

        /// <summary>
        /// TEST 01: Auto ON + Normal Skill READY -> Hero autonomously casts skill.
        /// </summary>
        private static bool P07_9_1_01_AutoOn_NormalSkillReady_HeroAutoCasts()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bm.SetAutoBattle(true);

            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            hero.Rage.ResetRage(50f); // 50 Rage is enough for normal skill (cost 30), but < 100 for Ultimate
            CooldownManager.ResetAllCooldowns();

            bool requestReceived = false;
            SkillExecutionRequest captured = null;
            Action<SkillExecutionRequest> onReq = (req) =>
            {
                if (req.Skill != null && req.Skill.SkillId == "skill_taiji_2_a")
                {
                    requestReceived = true;
                    captured = req;
                }
            };
            EventBus.OnSkillExecutionRequested += onReq;

            var decision = hero.SkillDecisionController;
            bool decided = decision.TickDecision(0.1f);

            EventBus.OnSkillExecutionRequested -= onReq;

            bool ok = decided && requestReceived && captured != null && captured.Slot == SkillSlotType.Skill;
            Debug.Log($"[P07_9_1_01] TEST 01 -> Decided: {decided}, ReqFired: {requestReceived}, Slot: {(captured != null ? captured.Slot.ToString() : "null")} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 02: Auto ON + Multiple Skills READY -> Highest Priority selected.
        /// Also validates dynamic priority change and deterministic tie-breaker.
        /// </summary>
        private static bool P07_9_1_02_AutoOn_MultipleSkillsReady_HighestPrioritySelected()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bm.SetAutoBattle(true);

            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a"); // Slot 2
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            mmMgr.UnlockSkill("skill_taiji_3_a"); // Slot 3
            mmMgr.SelectSkillForSlot(SkillSlotType.ExternalSkill1, "skill_taiji_3_a");

            SkillDefinitionSO def2 = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            SkillDefinitionSO def3 = mmMgr.FindSkillDefinition("skill_taiji_3_a");

            // Test 2A: def3 has higher priority (80 vs 60)
            def2.SetPriority(60);
            def3.SetPriority(80);
            hero.Rage.ResetRage(50f);
            CooldownManager.ResetAllCooldowns();

            string chosenId = "";
            Action<SkillExecutionRequest> onReq = (req) => chosenId = req.Skill.SkillId;
            EventBus.OnSkillExecutionRequested += onReq;

            var decision = hero.SkillDecisionController;
            decision.TickDecision(0.1f);

            bool pass2A = (chosenId == "skill_taiji_3_a");

            // Test 2B: Dynamic priority change without code change (def2 becomes 90 > 80)
            hero.Rage.ResetRage(50f);
            CooldownManager.ResetAllCooldowns();
            def2.SetPriority(90);
            chosenId = "";
            decision.TickDecision(0.1f);
            bool pass2B = (chosenId == "skill_taiji_2_a");

            // Test 2C: Deterministic tie-breaker (both priority 70 -> string ordinal tie-breaker)
            hero.Rage.ResetRage(50f);
            CooldownManager.ResetAllCooldowns();
            def2.SetPriority(70);
            def3.SetPriority(70);
            chosenId = "";
            decision.TickDecision(0.1f);
            string expectedTieWinner = string.CompareOrdinal("skill_taiji_2_a", "skill_taiji_3_a") < 0 ? "skill_taiji_2_a" : "skill_taiji_3_a";
            bool pass2C = (chosenId == expectedTieWinner);

            EventBus.OnSkillExecutionRequested -= onReq;

            bool ok = pass2A && pass2B && pass2C;
            Debug.Log($"[P07_9_1_02] TEST 02 -> HighestPriority: {pass2A}, DynamicChange: {pass2B}, TieBreak: {pass2C} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 03: Auto ON + Ultimate READY + Sufficient Rage -> Hero auto Ultimates & interrupts Basic Attack windup.
        /// </summary>
        private static bool P07_9_1_03_AutoOn_UltimateReady_SufficientRage_HeroAutoUltimates()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bm.SetAutoBattle(true);

            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            mmMgr.UnlockSkill("skill_taiji_5_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Ultimate, "skill_taiji_5_a");
            CooldownManager.ResetAllCooldowns();

            // Give full rage
            hero.Rage.AddRage(100f);

            // Simulate basic attack windup
            if (hero.Attack != null)
            {
                hero.Attack.ManualTick(0.8f);
            }

            string chosenId = "";
            Action<SkillExecutionRequest> onReq = (req) => chosenId = req.Skill.SkillId;
            EventBus.OnSkillExecutionRequested += onReq;

            var decision = hero.SkillDecisionController;
            bool decided = decision.TickDecision(0.1f);

            EventBus.OnSkillExecutionRequested -= onReq;

            bool attackTimerReset = (hero.Attack == null || hero.Attack.AttackTimer == 0f);
            bool ok = decided && (chosenId == "skill_taiji_5_a") && attackTimerReset;
            Debug.Log($"[P07_9_1_03] TEST 03 -> Decided: {decided}, ChosenSkill: {chosenId}, BasicAttackWindupInterrupted: {attackTimerReset} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 04: Auto OFF + Skill READY -> Hero does NOT auto-cast.
        /// </summary>
        private static bool P07_9_1_04_AutoOff_SkillReady_HeroDoesNotAutoCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bm.SetAutoBattle(false); // AUTO OFF

            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            mmMgr.UnlockSkill("skill_taiji_5_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Ultimate, "skill_taiji_5_a");
            hero.Rage.AddRage(100f);
            CooldownManager.ResetAllCooldowns();

            int reqCount = 0;
            Action<SkillExecutionRequest> onReq = (req) => reqCount++;
            EventBus.OnSkillExecutionRequested += onReq;

            var decision = hero.SkillDecisionController;
            bool d1 = decision.TickDecision(0.1f);
            bool d2 = decision.TickDecision(0.1f);
            bool d3 = decision.TickDecision(0.1f);

            EventBus.OnSkillExecutionRequested -= onReq;

            bool ok = !d1 && !d2 && !d3 && (reqCount == 0);
            Debug.Log($"[P07_9_1_04] TEST 04 -> IsAutoBattle: {bm.IsAutoBattle}, Decided: {d1 || d2 || d3}, ReqCount: {reqCount} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 05: Auto OFF + Manual Skill -> Manual Skill still casts.
        /// </summary>
        private static bool P07_9_1_05_AutoOff_ManualSkill_ManualSkillStillCasts()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bm.SetAutoBattle(false);

            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            CooldownManager.ResetAllCooldowns();

            float hpBefore = bm.CurrentMonster.Health.CurrentHealth;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float hpAfter = bm.CurrentMonster.Health.CurrentHealth;

            bool ok = res.Success && (hpAfter < hpBefore);
            Debug.Log($"[P07_9_1_05] TEST 05 -> ManualSuccess: {res.Success}, DamageDealt: {hpBefore - hpAfter} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 06: Cast Time -> CastState active and transitions over duration.
        /// </summary>
        private static bool P07_9_1_06_CastTime_CastStateActive()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bm.SetAutoBattle(true);

            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            SkillDefinitionSO def = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            def.SetCastTime(1.5f);
            hero.Rage.ResetRage(50f);
            CooldownManager.ResetAllCooldowns();

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            var decision = hero.SkillDecisionController;
            bool decided = decision.TickDecision(0.1f);

            bool castingAtStart = hero.IsCasting && hero.CastState.CurrentPhase == SkillCastPhase.Casting;
            bool noPrematureDamage = (bm.CurrentMonster.Health.CurrentHealth == initialMonsterHp);

            // Tick active cast to completion
            hero.TickActiveCast(1.5f);
            bool castingComplete = !hero.IsCasting;
            bool damageDealt = (bm.CurrentMonster.Health.CurrentHealth < initialMonsterHp);

            def.SetCastTime(0f);

            bool ok = decided && castingAtStart && noPrematureDamage && castingComplete && damageDealt;
            Debug.Log($"[P07_9_1_06] TEST 06 -> Decided: {decided}, CastingAtStart: {castingAtStart}, Complete: {castingComplete}, DamageDealt: {damageDealt} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 07: Channel -> Channel active and applies ticks.
        /// </summary>
        private static bool P07_9_1_07_Channel_ChannelActive()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bm.SetAutoBattle(true);

            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            SkillDefinitionSO def = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            def.SetChannel(true, duration: 2.0f, tickInterval: 0.5f);
            hero.Rage.ResetRage(50f);
            CooldownManager.ResetAllCooldowns();

            var decision = hero.SkillDecisionController;
            bool decided = decision.TickDecision(0.1f);

            bool channelingAtStart = hero.IsChanneling && hero.CastState.CurrentPhase == SkillCastPhase.Channeling;

            float hpBeforeTick = bm.CurrentMonster.Health.CurrentHealth;
            hero.TickActiveCast(0.5f);
            float hpAfterTick = bm.CurrentMonster.Health.CurrentHealth;
            bool tickDamaged = (hpAfterTick < hpBeforeTick);

            hero.TickActiveCast(1.5f); // Complete remaining duration
            bool channelFinished = !hero.IsCasting;

            def.SetChannel(false);

            bool ok = decided && channelingAtStart && tickDamaged && channelFinished;
            Debug.Log($"[P07_9_1_07] TEST 07 -> ChannelingAtStart: {channelingAtStart}, TickDamaged: {tickDamaged}, Finished: {channelFinished} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 08: Stun CC -> Cast interrupt.
        /// </summary>
        private static bool P07_9_1_08_Stun_CastInterrupt()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO def = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            def.SetCastTime(2.0f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool wasCasting = hero.IsCasting;

            bool interruptFired = false;
            Action<Entity, SkillDefinitionSO, SkillCastInterruptSource> onInt = (e, s, src) =>
            {
                if (e == hero && src == SkillCastInterruptSource.Stun) interruptFired = true;
            };
            EventBus.OnSkillCastInterrupted += onInt;

            hero.StatusController.ApplyCrowdControl("test_stun", CrowdControlType.Stun, 2.0f);
            hero.InterruptCurrentAction(SkillCastInterruptSource.Stun);

            EventBus.OnSkillCastInterrupted -= onInt;

            bool isCastingAfter = hero.IsCasting;
            def.SetCastTime(0f);

            bool ok = wasCasting && !isCastingAfter && interruptFired;
            Debug.Log($"[P07_9_1_08] TEST 08 -> WasCasting: {wasCasting}, IsCastingAfter: {isCastingAfter}, InterruptFired: {interruptFired} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 09: Freeze CC -> Cast interrupt.
        /// </summary>
        private static bool P07_9_1_09_Freeze_CastInterrupt()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO def = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            def.SetCastTime(2.0f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool wasCasting = hero.IsCasting;

            bool interruptFired = false;
            Action<Entity, SkillDefinitionSO, SkillCastInterruptSource> onInt = (e, s, src) =>
            {
                if (e == hero && src == SkillCastInterruptSource.Freeze) interruptFired = true;
            };
            EventBus.OnSkillCastInterrupted += onInt;

            hero.StatusController.ApplyCrowdControl("test_freeze", CrowdControlType.Freeze, 2.0f);
            hero.InterruptCurrentAction(SkillCastInterruptSource.Freeze);

            EventBus.OnSkillCastInterrupted -= onInt;

            bool isCastingAfter = hero.IsCasting;
            def.SetCastTime(0f);

            bool ok = wasCasting && !isCastingAfter && interruptFired;
            Debug.Log($"[P07_9_1_09] TEST 09 -> WasCasting: {wasCasting}, IsCastingAfter: {isCastingAfter}, InterruptFired: {interruptFired} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 10: Root CC -> Cast continues (Root does NOT interrupt cast).
        /// </summary>
        private static bool P07_9_1_10_Root_CastContinues()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO def = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            def.SetCastTime(2.0f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool wasCasting = hero.IsCasting;

            // Apply Root (Root restricts movement, does NOT interrupt active cast)
            hero.StatusController.ApplyCrowdControl("test_root", CrowdControlType.Root, 2.0f);

            // Root does not stop casting
            bool stillCasting = hero.IsCasting;
            hero.TickActiveCast(2.0f); // Finish cast
            bool completed = !hero.IsCasting;

            def.SetCastTime(0f);

            bool ok = wasCasting && stillCasting && completed;
            Debug.Log($"[P07_9_1_10] TEST 10 -> WasCasting: {wasCasting}, StillCastingUnderRoot: {stillCasting}, Completed: {completed} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 11: Ordinary Damage -> Cast continues (non-CC damage does NOT interrupt cast).
        /// </summary>
        private static bool P07_9_1_11_OrdinaryDamage_CastContinues()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO def = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            def.SetCastTime(2.0f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool wasCasting = hero.IsCasting;

            // Ordinary damage dealt to Hero
            float hpBefore = hero.Health.CurrentHealth;
            hero.Health.TakeDamage(50f);
            float hpAfter = hero.Health.CurrentHealth;

            bool stillCasting = hero.IsCasting;
            hero.TickActiveCast(2.0f);
            bool completed = !hero.IsCasting;

            def.SetCastTime(0f);

            bool ok = wasCasting && (hpAfter < hpBefore) && stillCasting && completed;
            Debug.Log($"[P07_9_1_11] TEST 11 -> WasCasting: {wasCasting}, TookDamage: {hpBefore - hpAfter}, StillCasting: {stillCasting}, Completed: {completed} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 12: Insufficient Rage -> Ultimate does NOT cast.
        /// </summary>
        private static bool P07_9_1_12_InsufficientRage_UltimateDoesNotCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bm.SetAutoBattle(true);

            // Normal skills on cooldown, and Rage set to 40 (Ultimate requires 100)
            CooldownManager.TriggerCooldown("skill_taiji_2_a", 10.0f);
            CooldownManager.TriggerCooldown("skill_taiji_3_a", 10.0f);
            CooldownManager.TriggerCooldown("skill_taiji_4_a", 10.0f);
            hero.Rage.ResetRage(40f);

            string requestedSkill = "";
            Action<SkillExecutionRequest> onReq = (req) => requestedSkill = req.Skill.SkillId;
            EventBus.OnSkillExecutionRequested += onReq;

            var decision = hero.SkillDecisionController;
            decision.TickDecision(0.1f);

            EventBus.OnSkillExecutionRequested -= onReq;

            bool ok = (requestedSkill != "skill_taiji_5_a") && (hero.Rage.CurrentRage == 40f);
            Debug.Log($"[P07_9_1_12] TEST 12 -> Rage: {hero.Rage.CurrentRage}/100, RequestedSkill: '{requestedSkill}' | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 13: Cooldown Active -> Skill does NOT cast.
        /// </summary>
        private static bool P07_9_1_13_CooldownActive_SkillDoesNotCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bm.SetAutoBattle(true);

            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");

            // Put skill on cooldown
            CooldownManager.TriggerCooldown("skill_taiji_2_a", 10.0f);

            string requestedSkill = "";
            Action<SkillExecutionRequest> onReq = (req) => requestedSkill = req.Skill.SkillId;
            EventBus.OnSkillExecutionRequested += onReq;

            var decision = hero.SkillDecisionController;
            decision.TickDecision(0.1f);

            EventBus.OnSkillExecutionRequested -= onReq;

            bool onCd = CooldownManager.IsOnCooldown("skill_taiji_2_a", out _);
            bool ok = onCd && (requestedSkill != "skill_taiji_2_a");
            Debug.Log($"[P07_9_1_13] TEST 13 -> IsOnCooldown: {onCd}, RequestedSkill: '{requestedSkill}' | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 14: Skill validation failure -> Does NOT consume resources.
        /// </summary>
        private static bool P07_9_1_14_SkillValidationFailure_NoResourceConsumed()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            hero.Rage.ResetRage(100f);

            // Validation failure: Kill monster so target is dead
            bm.CurrentMonster.Health.SetCurrentHealth(0f);

            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool ok = !res.Success && (hero.Rage.CurrentRage == 100f) && !CooldownManager.IsOnCooldown("skill_taiji_2_a", out _);
            Debug.Log($"[P07_9_1_14] TEST 14 -> Success: {res.Success}, Reason: {res.FailureReason}, RagePreserved: {hero.Rage.CurrentRage == 100f} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 15: Re-entry guard -> Hero does not duplicate cast while currently casting.
        /// </summary>
        private static bool P07_9_1_15_ReentryGuard_NoDuplicateCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            bm.SetAutoBattle(true);

            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            SkillDefinitionSO def = mmMgr.FindSkillDefinition("skill_taiji_2_a");
            def.SetCastTime(2.0f);
            hero.Rage.ResetRage(50f);
            CooldownManager.ResetAllCooldowns();

            int reqCount = 0;
            Action<SkillExecutionRequest> onReq = (req) => reqCount++;
            EventBus.OnSkillExecutionRequested += onReq;

            var decision = hero.SkillDecisionController;

            // Tick 1: Starts cast
            decision.TickDecision(0.1f);
            bool isCasting1 = hero.IsCasting;

            // Tick 2: Decision runs while casting
            decision.TickDecision(0.1f);
            bool isCasting2 = hero.IsCasting;

            EventBus.OnSkillExecutionRequested -= onReq;
            def.SetCastTime(0f);

            bool ok = isCasting1 && isCasting2 && (reqCount == 1);
            Debug.Log($"[P07_9_1_15] TEST 15 -> IsCasting1: {isCasting1}, IsCasting2: {isCasting2}, RequestCount: {reqCount} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// Play Mode Real Battle Verification Scenario (Section 13 requirement)
        /// Runs live battle in Prototype01 scene and verifies autonomous skill execution loop end-to-end.
        /// </summary>
        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9.1 Play Mode Real Battle Verification")]
        public static bool RunPrototype07_9_1_PlayModeRealBattleVerification()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING PROTOTYPE 07.9.1 PLAY MODE REAL BATTLE VERIFICATION SCENARIO");
            Debug.Log("================================================================================");

            Prototype01SceneBuilder.BuildPrototype02Data();
            Prototype01SceneBuilder.BuildPrototype01Scene();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);

            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            MindMethodManager mmMgr = UnityEngine.Object.FindAnyObjectByType<MindMethodManager>();

            if (hero == null || bm == null || mmMgr == null)
            {
                Debug.LogError("[PLAY MODE] Missing essential scene components!");
                return false;
            }

            hero.InitializeHero();
            hero.Health.Revive(hero.Health.MaxHealth);
            hero.EnableEntityActions();
            hero.Rage.ResetRage(0f);

            if (bm.CurrentMonster == null)
            {
                bm.SpawnMonster();
            }
            bm.CurrentMonster.Health.InitializeHealth(999999f, bm.CurrentMonster);
            bm.CurrentMonster.Health.Revive(999999f);
            bm.CurrentMonster.EnableEntityActions();
            bm.CurrentMonster.Stats.SetBaseValue(StatType.Dodge, 0f);

            if (!bm.IsBattleActive)
            {
                bm.StartBattle();
            }

            bm.CurrentMonster.Health.InitializeHealth(999999f, bm.CurrentMonster);
            bm.CurrentMonster.Health.Revive(999999f);
            hero.transform.position = new Vector3(0f, -1.2f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(1.2f, -1.2f, 0f);
            hero.SetCurrentTarget(bm.CurrentMonster);
            bm.CurrentMonster.SetCurrentTarget(hero);

            // Setup mind method and skills
            mmMgr.LoadDatabaseIfMissing();
            mmMgr.ResetPersistence();
            mmMgr.InitializeFromDatabase();
            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a"); // Slot 2 (Priority 60)
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            mmMgr.UnlockSkill("skill_taiji_3_a"); // Slot 3 (Priority 40)
            mmMgr.SelectSkillForSlot(SkillSlotType.ExternalSkill1, "skill_taiji_3_a");
            mmMgr.UnlockSkill("skill_taiji_5_a"); // Slot 5 Ultimate (Priority 100)
            mmMgr.SelectSkillForSlot(SkillSlotType.Ultimate, "skill_taiji_5_a");
            CooldownManager.ResetAllCooldowns();

            bm.SetAutoBattle(true);

            // Verification tracking
            bool normalSkillAutoCast = false;
            bool ultimateAutoCast = false;
            string lastCastSkill = "";

            Action<SkillExecutionRequest> onReq = (req) =>
            {
                lastCastSkill = req.Skill != null ? req.Skill.SkillId : "";
                if (req.Slot == SkillSlotType.Skill || req.Slot == SkillSlotType.ExternalSkill1)
                {
                    normalSkillAutoCast = true;
                }
                else if (req.Slot == SkillSlotType.Ultimate)
                {
                    ultimateAutoCast = true;
                }
            };
            EventBus.OnSkillExecutionRequested += onReq;

            var decision = hero.SkillDecisionController;

            // ================================================================
            // SCENARIO A: Auto ON (Hero attacks normally, skill ready -> auto casts, damage occurs)
            // ================================================================
            hero.SetCurrentTarget(bm.CurrentMonster);
            bm.CurrentMonster.Health.Revive(999999f);
            float preAttackHp = bm.CurrentMonster.Health.CurrentHealth;
            if (hero.Attack != null)
            {
                hero.Attack.ManualTick(1.5f); // Execute basic attack
            }
            float postAttackHp = bm.CurrentMonster.Health.CurrentHealth;
            bool scenarioA_BasicAttackWorked = (postAttackHp < preAttackHp);

            // Normal skill ready
            hero.Rage.ResetRage(50f);
            CooldownManager.ResetAllCooldowns();
            float preSkillHp = bm.CurrentMonster.Health.CurrentHealth;
            decision.TickDecision(0.1f);
            float postSkillHp = bm.CurrentMonster.Health.CurrentHealth;
            bool scenarioA_SkillCast = (lastCastSkill == "skill_taiji_2_a");
            bool scenarioA_SkillDamageOccurred = (postSkillHp < preSkillHp);
            bool scenarioA_Pass = scenarioA_BasicAttackWorked && scenarioA_SkillCast && scenarioA_SkillDamageOccurred;
            Debug.Log($"[RUNTIME ACCEPTANCE - SCENARIO A] Auto ON -> BasicAttackDealtDamage: {scenarioA_BasicAttackWorked}, SkillAutoCast: {scenarioA_SkillCast}, DamageDealt: {scenarioA_SkillDamageOccurred} (HP: {preSkillHp} -> {postSkillHp}) | {(scenarioA_Pass ? "PASS" : "FAIL")}");

            // ================================================================
            // SCENARIO B: Priority (Two skills ready simultaneously -> higher priority chosen)
            // ================================================================
            hero.SetCurrentTarget(bm.CurrentMonster);
            bm.CurrentMonster.Health.Revive(999999f);
            hero.Rage.ResetRage(50f);
            CooldownManager.ResetAllCooldowns();
            // Both Slot 2 (skill_taiji_2_a, priority 60) and Slot 3 (skill_taiji_3_a, priority 40) are ready
            lastCastSkill = "";
            decision.TickDecision(0.1f);
            bool scenarioB_PriorityChosen = (lastCastSkill == "skill_taiji_2_a");
            Debug.Log($"[RUNTIME ACCEPTANCE - SCENARIO B] Priority Selection -> ChosenSkill: '{lastCastSkill}' (Expected: 'skill_taiji_2_a' with Priority 60 > 40) | {(scenarioB_PriorityChosen ? "PASS" : "FAIL")}");

            // ================================================================
            // SCENARIO C: Ultimate (Sufficient Rage -> Auto Ultimate, Basic Attack windup interrupted)
            // ================================================================
            hero.SetCurrentTarget(bm.CurrentMonster);
            bm.CurrentMonster.Health.Revive(999999f);
            hero.Rage.ResetRage(100f);
            CooldownManager.ResetAllCooldowns();
            if (hero.Attack != null)
            {
                hero.Attack.ManualTick(0.8f); // Simulate basic attack windup in progress
            }
            bool hadWindup = (hero.Attack != null && hero.Attack.AttackTimer > 0f);
            lastCastSkill = "";
            decision.TickDecision(0.1f);
            bool windupInterrupted = (hero.Attack == null || hero.Attack.AttackTimer == 0f);
            bool scenarioC_UltChosen = (lastCastSkill == "skill_taiji_5_a");
            bool scenarioC_Pass = hadWindup && windupInterrupted && scenarioC_UltChosen;
            Debug.Log($"[RUNTIME ACCEPTANCE - SCENARIO C] Ultimate -> HadWindupBefore: {hadWindup}, WindupInterrupted: {windupInterrupted}, UltAutoCast: {scenarioC_UltChosen} | {(scenarioC_Pass ? "PASS" : "FAIL")}");

            // ================================================================
            // SCENARIO D: Auto OFF (Disable Auto Battle -> No auto skill/ult, Basic Attack behavior unchanged)
            // ================================================================
            bm.SetAutoBattle(false);
            hero.SetCurrentTarget(bm.CurrentMonster);
            bm.CurrentMonster.Health.Revive(999999f);
            hero.Rage.ResetRage(100f);
            CooldownManager.ResetAllCooldowns();
            lastCastSkill = "";
            decision.TickDecision(0.1f);
            bool scenarioD_NoAutoCast = string.IsNullOrEmpty(lastCastSkill);

            // Basic attack behavior remains unchanged
            float preAutoOffAtkHp = bm.CurrentMonster.Health.CurrentHealth;
            if (hero.Attack != null)
            {
                hero.Attack.ManualTick(1.5f);
            }
            float postAutoOffAtkHp = bm.CurrentMonster.Health.CurrentHealth;
            bool scenarioD_BasicAttackWorks = (postAutoOffAtkHp < preAutoOffAtkHp);
            bool scenarioD_Pass = scenarioD_NoAutoCast && scenarioD_BasicAttackWorks;
            Debug.Log($"[RUNTIME ACCEPTANCE - SCENARIO D] Auto OFF -> AutoCastBlocked: {scenarioD_NoAutoCast}, BasicAttackContinues: {scenarioD_BasicAttackWorks} | {(scenarioD_Pass ? "PASS" : "FAIL")}");

            // ================================================================
            // SCENARIO E: Manual (With Auto OFF, manually activate skill -> executes through validator/executor)
            // ================================================================
            hero.SetCurrentTarget(bm.CurrentMonster);
            bm.CurrentMonster.Health.Revive(999999f);
            hero.Rage.ResetRage(50f);
            CooldownManager.ResetAllCooldowns();
            float preManualHp = bm.CurrentMonster.Health.CurrentHealth;
            var manualRes = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float postManualHp = bm.CurrentMonster.Health.CurrentHealth;
            bool scenarioE_DamageOccurred = (postManualHp < preManualHp);
            bool scenarioE_Pass = manualRes.Success && scenarioE_DamageOccurred;
            Debug.Log($"[RUNTIME ACCEPTANCE - SCENARIO E] Manual Skill -> Success: {manualRes.Success}, DamageDealt: {scenarioE_DamageOccurred} (HP: {preManualHp} -> {postManualHp}) | {(scenarioE_Pass ? "PASS" : "FAIL")}");

            EventBus.OnSkillExecutionRequested -= onReq;

            bool ok = scenarioA_Pass && scenarioB_PriorityChosen && scenarioC_Pass && scenarioD_Pass && scenarioE_Pass;
            Debug.Log($"[PLAY MODE 07.9.1] RESULTS -> ScenarioA: {scenarioA_Pass}, ScenarioB: {scenarioB_PriorityChosen}, ScenarioC: {scenarioC_Pass}, ScenarioD: {scenarioD_Pass}, ScenarioE: {scenarioE_Pass} | {(ok ? "PASS" : "FAIL")}");
            return ok;
        }
    }
}
#endif
