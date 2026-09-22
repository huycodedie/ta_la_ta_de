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

namespace WuxiaGame.Editor
{
    public static partial class Prototype01PlayTestRunner
    {
        private class TestChannelHero : Hero
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

        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9 Phase 3 Tests (TEST 01 to TEST 22 + Large Delta + Play Mode)")]
        public static bool RunAllPrototype07_9_Phase3_Tests()
        {
            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 PHASE 3 TESTS (CHANNEL / PERIODIC CAST)");
            Debug.Log("==================================================");

            int passed = 0;
            int total = 28;

            bool t01 = P07_9_P3_01_InstantSkill_ExecutesImmediately();
            if (t01) passed++;

            bool t02 = P07_9_P3_02_CastTimeSkill_ExecutesAtCompletion();
            if (t02) passed++;

            bool t03 = P07_9_P3_03_ChannelSkill_StartsSuccessfully();
            if (t03) passed++;

            bool t04 = P07_9_P3_04_ChannelSkill_ConsumesRageOnceAtStart();
            if (t04) passed++;

            bool t05 = P07_9_P3_05_Channel_NoPeriodicEffectsBeforeTickBoundary();
            if (t05) passed++;

            bool t06 = P07_9_P3_06_Channel_FirstTickAtConfiguredBoundary();
            if (t06) passed++;

            bool t07 = P07_9_P3_07_Channel_SubsequentTicksDeterministic();
            if (t07) passed++;

            bool t08 = P07_9_P3_08_Channel_DoesNotCompleteBeforeDuration();
            if (t08) passed++;

            bool t09 = P07_9_P3_09_Channel_CompletesAtDurationBoundary();
            if (t09) passed++;

            bool t10 = P07_9_P3_10_Channel_EffectsExecuteOncePerScheduledTick();
            if (t10) passed++;

            bool t11 = P07_9_P3_11_Channel_NoCooldownAtStart();
            if (t11) passed++;

            bool t12 = P07_9_P3_12_Channel_CooldownStartsAfterCompletion();
            if (t12) passed++;

            bool t13 = P07_9_P3_13_ActiveChannel_RejectsReentryWithSourceAlreadyCasting();
            if (t13) passed++;

            bool t14 = P07_9_P3_14_RejectedSkill_DoesNotConsumeRage();
            if (t14) passed++;

            bool t15 = P07_9_P3_15_ActiveCast_RemainsIntactAfterRejection();
            if (t15) passed++;

            bool t16 = P07_9_P3_16_ChannelDamage_UsesDamageCalculator();
            if (t16) passed++;

            bool t17 = P07_9_P3_17_ChannelDamage_ReachesHealthComponent();
            if (t17) passed++;

            bool t18 = P07_9_P3_18_ChannelDamage_AbsorbedByP07_8Shield();
            if (t18) passed++;

            bool t19 = P07_9_P3_19_RemainingDamageAfterShield_ReachesHp();
            if (t19) passed++;

            bool t20 = P07_9_P3_20_ChannelCompletion_CannotFinalizeTwice();
            if (t20) passed++;

            bool t21 = P07_9_P3_21_InstantSkill_BackwardCompatibility();
            if (t21) passed++;

            bool t22 = P07_9_P3_22_CastTimeSkill_BackwardCompatibility();
            if (t22) passed++;

            // Large Delta / Determinism Tests
            bool t23A = P07_9_P3_23A_DeltaSmallerThanTickInterval();
            if (t23A) passed++;

            bool t23B = P07_9_P3_23B_DeltaExactlyReachesTickInterval();
            if (t23B) passed++;

            bool t23C = P07_9_P3_23C_DeltaCrossesTickBoundary();
            if (t23C) passed++;

            bool t23D = P07_9_P3_23D_MultipleUpdatesCumulativelyReachBoundaries();
            if (t23D) passed++;

            bool t23E = P07_9_P3_23E_SingleUpdateCrossesMultipleTickBoundariesWithoutLoss();
            if (t23E) passed++;

            // Real Play Mode Verification Scenario
            bool pm = RunPrototype07_9_Phase3_PlayModeAcceptanceTests();
            if (pm) passed++;

            bool allPassed = (passed == total);

            Debug.Log("==================================================");
            Debug.Log("   PROTOTYPE 07.9 PHASE 3 TEST SUMMARY");
            Debug.Log($"   TEST 01 (Non-channel instant executes immediately):    {(t01 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 02 (Non-channel cast-time works as Phase 2):      {(t02 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 03 (Channel skill starts successfully):            {(t03 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 04 (Channel consumes Rage once at start):          {(t04 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 05 (No periodic effects before first tick):        {(t05 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 06 (First tick executes at configured boundary):   {(t06 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 07 (Subsequent ticks execute deterministically):   {(t07 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 08 (Channel does not complete before duration):    {(t08 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 09 (Channel completes at duration boundary):       {(t09 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 10 (Effects execute exactly once per tick):        {(t10 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 11 (No cooldown at Channel Start):                 {(t11 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 12 (Cooldown starts after Channel Complete):       {(t12 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 13 (Active Channel blocks re-entry):               {(t13 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 14 (Rejected skill does not consume Rage):         {(t14 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 15 (Active cast remains intact after rejection):   {(t15 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 16 (Channel damage uses DamageCalculator):         {(t16 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 17 (Channel damage reaches HealthComponent):       {(t17 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 18 (Channel damage absorbed by P07.8 Shield):      {(t18 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 19 (Remaining damage after shield reaches HP):     {(t19 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 20 (Channel completion cannot finalize twice):     {(t20 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 21 (Instant skill backward compatibility):         {(t21 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 22 (Cast-time backward compatibility):             {(t22 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 23A (Delta smaller than tick interval):            {(t23A ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 23B (Delta exactly reaches tick interval):         {(t23B ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 23C (Delta crosses tick boundary):                 {(t23C ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 23D (Multiple updates cumulatively reach bounds):  {(t23D ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 23E (Single update crosses multiple bounds):       {(t23E ? "PASS" : "FAIL")}");
            Debug.Log($"   PLAY MODE VERIFICATION SCENARIO (Section 24):           {(pm ? "PASS" : "FAIL")}");
            Debug.Log($"   OVERALL PHASE 3 STATUS: {(allPassed ? "100% PASS" : $"{passed}/{total} PASSED")}");
            Debug.Log("==================================================");

            return allPassed;
        }

        /// <summary>
        /// TEST 01: Non-channel instant skill still executes immediately.
        /// </summary>
        private static bool P07_9_P3_01_InstantSkill_ExecutesImmediately()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(false, 0f, 0f);
            hero.Rage.AddRage(100f);

            float hpBefore = bm.CurrentMonster.Health.CurrentHealth;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float hpAfter = bm.CurrentMonster.Health.CurrentHealth;

            bool ok = res.Success && (hpAfter < hpBefore) && !hero.IsCasting;
            Debug.Log($"[P07_9_P3_01] TEST 01 -> Success: {res.Success}, Dmg: {hpBefore - hpAfter}, IsCasting: {hero.IsCasting} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 02: Non-channel cast-time skill still works exactly as Phase 2.
        /// </summary>
        private static bool P07_9_P3_02_CastTimeSkill_ExecutesAtCompletion()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_CAST_TIME = 1.0f; // TEST FIXTURE ONLY
            skillDef.SetCastTime(TEST_CAST_TIME);
            skillDef.SetChannel(false, 0f, 0f);
            hero.Rage.AddRage(100f);

            float hpBefore = bm.CurrentMonster.Health.CurrentHealth;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool beforeOk = res.Success && hero.IsCasting && (bm.CurrentMonster.Health.CurrentHealth == hpBefore);
            hero.TickActiveCast(TEST_CAST_TIME);
            bool afterOk = !hero.IsCasting && (bm.CurrentMonster.Health.CurrentHealth < hpBefore);

            bool ok = beforeOk && afterOk;
            Debug.Log($"[P07_9_P3_02] TEST 02 -> BeforeOk: {beforeOk}, AfterOk: {afterOk} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 03: Channel skill starts successfully.
        /// </summary>
        private static bool P07_9_P3_03_ChannelSkill_StartsSuccessfully()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_DURATION = 2.0f; // TEST FIXTURE ONLY
            const float TEST_INTERVAL = 0.5f; // TEST FIXTURE ONLY
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, TEST_DURATION, TEST_INTERVAL);
            hero.Rage.AddRage(100f);

            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool ok = res.Success &&
                      hero.IsCasting &&
                      hero.CastState.CurrentPhase == SkillCastPhase.Channeling &&
                      hero.CastState.IsChannel &&
                      Mathf.Approximately(hero.CastState.ChannelDuration, TEST_DURATION) &&
                      Mathf.Approximately(hero.CastState.ChannelTickInterval, TEST_INTERVAL);

            Debug.Log($"[P07_9_P3_03] TEST 03 -> Success: {res.Success}, Phase: {hero.CastState.CurrentPhase}, IsChannel: {hero.CastState.IsChannel} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 04: Channel skill consumes Rage exactly once at Channel Start.
        /// </summary>
        private static bool P07_9_P3_04_ChannelSkill_ConsumesRageOnceAtStart()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);
            float initialRage = hero.Rage.CurrentRage;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float rageAfterStart = hero.Rage.CurrentRage;
            bool rageConsumedAtStart = Mathf.Approximately(rageAfterStart, initialRage - skillDef.RageCost);

            // Tick active cast across multiple boundaries
            hero.TickActiveCast(0.5f);
            float rageAfterTick1 = hero.Rage.CurrentRage;
            hero.TickActiveCast(0.5f);
            float rageAfterTick2 = hero.Rage.CurrentRage;

            bool noFurtherRageConsumed = Mathf.Approximately(rageAfterTick1, rageAfterStart) &&
                                         Mathf.Approximately(rageAfterTick2, rageAfterStart);

            bool ok = rageConsumedAtStart && noFurtherRageConsumed;
            Debug.Log($"[P07_9_P3_04] TEST 04 -> RageAtStart: {rageConsumedAtStart}, NoFurtherRage: {noFurtherRageConsumed} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 05: Channel does not execute periodic effects before first tick boundary.
        /// </summary>
        private static bool P07_9_P3_05_Channel_NoPeriodicEffectsBeforeTickBoundary()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            float hpBefore = bm.CurrentMonster.Health.CurrentHealth;
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            // Tick delta smaller than interval (0.2s < 0.5s)
            hero.TickActiveCast(0.2f);
            float hpAfter = bm.CurrentMonster.Health.CurrentHealth;

            bool ok = (hpBefore == hpAfter) && hero.IsCasting && (hero.CastState.ChannelTicksExecuted == 0);
            Debug.Log($"[P07_9_P3_05] TEST 05 -> HpUnchanged: {hpBefore == hpAfter}, TicksExecuted: {hero.CastState.ChannelTicksExecuted} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 06: First Channel Tick executes at the configured tick boundary.
        /// </summary>
        private static bool P07_9_P3_06_Channel_FirstTickAtConfiguredBoundary()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            float hpBefore = bm.CurrentMonster.Health.CurrentHealth;
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            // Advance from 0 to 0.5s boundary
            hero.TickActiveCast(0.5f);
            float hpAfter = bm.CurrentMonster.Health.CurrentHealth;

            bool ok = (hpAfter < hpBefore) && (hero.CastState.ChannelTicksExecuted == 1);
            Debug.Log($"[P07_9_P3_06] TEST 06 -> DmgDealt: {hpBefore - hpAfter}, TicksExecuted: {hero.CastState.ChannelTicksExecuted} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 07: Subsequent Channel Tick executes deterministically.
        /// </summary>
        private static bool P07_9_P3_07_Channel_SubsequentTicksDeterministic()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            hero.TickActiveCast(0.5f); // tick 1
            float hpAfter1 = bm.CurrentMonster.Health.CurrentHealth;
            int ticks1 = hero.CastState.ChannelTicksExecuted;

            hero.TickActiveCast(0.5f); // tick 2 (total 1.0s)
            float hpAfter2 = bm.CurrentMonster.Health.CurrentHealth;
            int ticks2 = hero.CastState.ChannelTicksExecuted;

            bool ok = (ticks1 == 1) && (ticks2 == 2) && (hpAfter2 < hpAfter1);
            Debug.Log($"[P07_9_P3_07] TEST 07 -> Ticks1: {ticks1}, Ticks2: {ticks2}, Dmg1: {hpAfter1}, Dmg2: {hpAfter2} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 08: Channel does not complete before ChannelDuration.
        /// </summary>
        private static bool P07_9_P3_08_Channel_DoesNotCompleteBeforeDuration()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(1.5f); // 1.5s < 2.0s duration

            bool onCd = CooldownManager.IsOnCooldown(skillDef.SkillId, out _);
            bool ok = hero.IsCasting && !onCd && (hero.CastState.CurrentPhase == SkillCastPhase.Channeling);

            Debug.Log($"[P07_9_P3_08] TEST 08 -> IsCasting: {hero.IsCasting}, OnCd: {onCd}, Phase: {hero.CastState.CurrentPhase} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 09: Channel completes at the correct deterministic duration boundary.
        /// </summary>
        private static bool P07_9_P3_09_Channel_CompletesAtDurationBoundary()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(2.0f); // Exactly completes 2.0s duration

            bool onCd = CooldownManager.IsOnCooldown(skillDef.SkillId, out _);
            bool ok = !hero.IsCasting && onCd && (hero.CastState.CurrentPhase == SkillCastPhase.Recovery);

            Debug.Log($"[P07_9_P3_09] TEST 09 -> NotCasting: {!hero.IsCasting}, OnCd: {onCd}, Phase: {hero.CastState.CurrentPhase} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 10: Channel effects execute exactly once per scheduled tick.
        /// </summary>
        private static bool P07_9_P3_10_Channel_EffectsExecuteOncePerScheduledTick()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_DURATION = 1.5f; // TEST FIXTURE ONLY
            const float TEST_INTERVAL = 0.5f; // TEST FIXTURE ONLY -> exactly 3 ticks
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, TEST_DURATION, TEST_INTERVAL);
            hero.Rage.AddRage(100f);

            int tickCallbackCount = 0;
            Action<SkillCastState, int> handler = (st, idx) => tickCallbackCount++;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.CastState.OnChannelTickCallback += handler;

            hero.TickActiveCast(0.5f); // tick 1
            hero.TickActiveCast(0.5f); // tick 2
            hero.TickActiveCast(0.5f); // tick 3 & complete

            bool ok = (tickCallbackCount == 3) && (hero.CastState.ChannelTicksExecuted == 3);
            Debug.Log($"[P07_9_P3_10] TEST 10 -> TickCallbackCount: {tickCallbackCount}, ChannelTicksExecuted: {hero.CastState.ChannelTicksExecuted} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 11: Channel does not start cooldown at Channel Start.
        /// </summary>
        private static bool P07_9_P3_11_Channel_NoCooldownAtStart()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool onCd = CooldownManager.IsOnCooldown(skillDef.SkillId, out float remain);
            bool ok = !onCd && (remain <= 0f);

            Debug.Log($"[P07_9_P3_11] TEST 11 -> OnCdAtStart: {onCd}, Remain: {remain} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 12: Channel starts cooldown only after successful completion.
        /// </summary>
        private static bool P07_9_P3_12_Channel_CooldownStartsAfterCompletion()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 1.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(1.0f); // Complete

            bool onCd = CooldownManager.IsOnCooldown(skillDef.SkillId, out float remain);
            bool ok = onCd && (remain > 0f) && !hero.IsCasting;

            Debug.Log($"[P07_9_P3_12] TEST 12 -> OnCdAfterComplete: {onCd}, Remain: {remain} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 13: Active Channel rejects skill re-entry with SourceAlreadyCasting.
        /// </summary>
        private static bool P07_9_P3_13_ActiveChannel_RejectsReentryWithSourceAlreadyCasting()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDefA = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            SkillDefinitionSO skillDefB = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            skillDefA.SetCastTime(0f);
            skillDefA.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster); // Channel A starts

            var resB = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster); // B requested

            bool ok = !resB.Success && (resB.FailureReason == SkillExecutionFailureReason.SourceAlreadyCasting);
            Debug.Log($"[P07_9_P3_13] TEST 13 -> ReentryRejected: {!resB.Success}, Reason: {resB.FailureReason} | {(ok ? "PASS" : "FAIL")}");
            skillDefA.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 14: Rejected skill does not consume Rage.
        /// </summary>
        private static bool P07_9_P3_14_RejectedSkill_DoesNotConsumeRage()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDefA = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            SkillDefinitionSO skillDefB = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            skillDefA.SetCastTime(0f);
            skillDefA.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float rageAfterA = hero.Rage.CurrentRage;

            hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            float rageAfterB = hero.Rage.CurrentRage;

            bool ok = Mathf.Approximately(rageAfterA, rageAfterB);
            Debug.Log($"[P07_9_P3_14] TEST 14 -> RageAfterA: {rageAfterA}, RageAfterB: {rageAfterB} | {(ok ? "PASS" : "FAIL")}");
            skillDefA.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 15: Existing Phase 2.1 active cast remains intact after rejection.
        /// </summary>
        private static bool P07_9_P3_15_ActiveCast_RemainsIntactAfterRejection()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDefA = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            SkillDefinitionSO skillDefB = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            skillDefA.SetCastTime(0f);
            skillDefA.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f); // tick 1 executed

            hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster); // Rejected

            // Check A remains active and progresses
            hero.TickActiveCast(0.5f); // tick 2 executed
            bool ok = hero.IsCasting && (hero.CastState.ChannelTicksExecuted == 2);

            Debug.Log($"[P07_9_P3_15] TEST 15 -> RemainsActive: {hero.IsCasting}, TicksExecuted: {hero.CastState.ChannelTicksExecuted} | {(ok ? "PASS" : "FAIL")}");
            skillDefA.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 16: Channel damage uses existing DamageCalculator.
        /// </summary>
        private static bool P07_9_P3_16_ChannelDamage_UsesDamageCalculator()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 1.0f, 0.5f);
            hero.Rage.AddRage(100f);

            bool damageEventFired = false;
            DamageResult observedResult = default;
            Action<Entity, DamageResult> handler = (target, dmgRes) =>
            {
                if (target == bm.CurrentMonster)
                {
                    damageEventFired = true;
                    observedResult = dmgRes;
                }
            };
            EventBus.OnEntityDamaged += handler;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f); // 1st channel tick

            EventBus.OnEntityDamaged -= handler;

            bool ok = damageEventFired && (observedResult.FinalDamage > 0f) && (observedResult.Attacker == hero);
            Debug.Log($"[P07_9_P3_16] TEST 16 -> DmgEventFired: {damageEventFired}, FinalDmg: {observedResult.FinalDamage} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 17: Channel damage reaches HealthComponent.TakeDamage().
        /// </summary>
        private static bool P07_9_P3_17_ChannelDamage_ReachesHealthComponent()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 1.0f, 0.5f);
            hero.Rage.AddRage(100f);

            bool healthChanged = false;
            Action<float, float> handler = (cur, max) => healthChanged = true;
            bm.CurrentMonster.Health.OnHealthChanged += handler;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f); // tick 1

            bm.CurrentMonster.Health.OnHealthChanged -= handler;

            bool ok = healthChanged;
            Debug.Log($"[P07_9_P3_17] TEST 17 -> HealthChanged: {healthChanged} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 18: Channel damage is absorbed by P07.8 Shield when shield exists.
        /// </summary>
        private static bool P07_9_P3_18_ChannelDamage_AbsorbedByP07_8Shield()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 1.0f, 0.5f);
            hero.Rage.AddRage(100f);

            // Apply large shield to monster per P07.8
            bm.CurrentMonster.StatusController.ApplyShield("shield_p3_test18", 1000f, 30f);
            float monsterHpBefore = bm.CurrentMonster.Health.CurrentHealth;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f); // tick 1

            float monsterHpAfter = bm.CurrentMonster.Health.CurrentHealth;
            var shield = bm.CurrentMonster.StatusController.GetShield("shield_p3_test18");
            float remainingShield = shield != null ? shield.CurrentAmount : -1f;

            bool ok = (monsterHpBefore == monsterHpAfter) && (remainingShield < 1000f) && (remainingShield > 0f);
            Debug.Log($"[P07_9_P3_18] TEST 18 -> HpUnchanged: {monsterHpBefore == monsterHpAfter}, RemShield: {remainingShield} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 19: Remaining damage after shield absorption reaches HP correctly.
        /// </summary>
        private static bool P07_9_P3_19_RemainingDamageAfterShield_ReachesHp()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 1.0f, 0.5f);
            hero.Rage.AddRage(100f);

            // Apply small shield (e.g. 5 HP) to monster
            bm.CurrentMonster.StatusController.ApplyShield("shield_p3_test19", 5f, 30f);
            float monsterHpBefore = bm.CurrentMonster.Health.CurrentHealth;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f); // tick 1

            float monsterHpAfter = bm.CurrentMonster.Health.CurrentHealth;
            bool shieldDepleted = !bm.CurrentMonster.StatusController.HasActiveShield;
            bool hpReduced = monsterHpAfter < monsterHpBefore;

            bool ok = shieldDepleted && hpReduced;
            Debug.Log($"[P07_9_P3_19] TEST 19 -> ShieldDepleted: {shieldDepleted}, HpReduced: {hpReduced} (Dmg={monsterHpBefore - monsterHpAfter}) | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 20: Channel completion cannot execute finalization twice.
        /// </summary>
        private static bool P07_9_P3_20_ChannelCompletion_CannotFinalizeTwice()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 1.0f, 0.5f);
            hero.Rage.AddRage(100f);

            int successEvents = 0;
            Action<SkillExecutionRequest, SkillExecutionResult> handler = (req, res) =>
            {
                if (req?.Skill?.SkillId == skillDef.SkillId && res.Success)
                {
                    successEvents++;
                }
            };
            EventBus.OnSkillExecutionSucceeded += handler;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(1.0f); // completes

            // Additional ticks after completion
            hero.TickActiveCast(0.5f);
            hero.TickActiveCast(1.0f);

            // Manual duplicate ExecuteCastComplete call
            var secondResult = SkillExecutor.CompleteCast(hero.CastState);

            EventBus.OnSkillExecutionSucceeded -= handler;

            bool ok = (successEvents == 1) && (secondResult == null);
            Debug.Log($"[P07_9_P3_20] TEST 20 -> SuccessEvents: {successEvents}, SecondResultNull: {secondResult == null} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 21: Instant skill backward compatibility remains intact.
        /// </summary>
        private static bool P07_9_P3_21_InstantSkill_BackwardCompatibility()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(false, 0f, 0f);
            hero.Rage.AddRage(100f);

            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool ok = res.Success && !hero.IsCasting && skillDef.IsInstant;
            Debug.Log($"[P07_9_P3_21] TEST 21 -> Success: {res.Success}, IsInstant: {skillDef.IsInstant}, IsCasting: {hero.IsCasting} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 22: Existing normal cast-time skill backward compatibility remains intact.
        /// </summary>
        private static bool P07_9_P3_22_CastTimeSkill_BackwardCompatibility()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            skillDef.SetChannel(false, 0f, 0f);
            hero.Rage.AddRage(100f);

            var res = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool castingBefore = hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Casting);
            hero.TickActiveCast(1.0f);
            bool completedAfter = !hero.IsCasting && (hero.CastState.CurrentPhase == SkillCastPhase.Recovery);

            bool ok = res.Success && castingBefore && completedAfter;
            Debug.Log($"[P07_9_P3_22] TEST 22 -> CastingBefore: {castingBefore}, CompletedAfter: {completedAfter} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 23A: Large Delta / Determinism: deltaTime smaller than tick interval.
        /// </summary>
        private static bool P07_9_P3_23A_DeltaSmallerThanTickInterval()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            hero.TickActiveCast(0.1f);
            hero.TickActiveCast(0.2f);
            hero.TickActiveCast(0.15f); // Total 0.45s < 0.5s

            bool ok = (hero.CastState.ChannelTicksExecuted == 0) && hero.IsCasting;
            Debug.Log($"[P07_9_P3_23A] TEST 23A -> TicksExecuted: {hero.CastState.ChannelTicksExecuted} (expected 0) | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 23B: Large Delta / Determinism: deltaTime exactly reaches tick interval.
        /// </summary>
        private static bool P07_9_P3_23B_DeltaExactlyReachesTickInterval()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            hero.TickActiveCast(0.5f); // exactly reaches 0.5s

            bool ok = (hero.CastState.ChannelTicksExecuted == 1) && hero.IsCasting;
            Debug.Log($"[P07_9_P3_23B] TEST 23B -> TicksExecuted: {hero.CastState.ChannelTicksExecuted} (expected 1) | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 23C: Large Delta / Determinism: deltaTime crosses a tick boundary.
        /// </summary>
        private static bool P07_9_P3_23C_DeltaCrossesTickBoundary()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            hero.TickActiveCast(0.7f); // crosses 0.5s boundary, but less than 1.0s

            bool ok = (hero.CastState.ChannelTicksExecuted == 1) && hero.IsCasting;
            Debug.Log($"[P07_9_P3_23C] TEST 23C -> TicksExecuted: {hero.CastState.ChannelTicksExecuted} (expected 1) | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 23D: Large Delta / Determinism: multiple updates cumulatively reach multiple tick boundaries.
        /// </summary>
        private static bool P07_9_P3_23D_MultipleUpdatesCumulativelyReachBoundaries()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            // 5 frames of 0.25s each = 1.25s total (crosses 0.5s and 1.0s)
            hero.TickActiveCast(0.25f); // 0.25s -> 0 ticks
            hero.TickActiveCast(0.25f); // 0.50s -> 1 tick
            hero.TickActiveCast(0.25f); // 0.75s -> 1 tick
            hero.TickActiveCast(0.25f); // 1.00s -> 2 ticks
            hero.TickActiveCast(0.25f); // 1.25s -> 2 ticks

            bool ok = (hero.CastState.ChannelTicksExecuted == 2) && hero.IsCasting;
            Debug.Log($"[P07_9_P3_23D] TEST 23D -> TicksExecuted: {hero.CastState.ChannelTicksExecuted} (expected 2) | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 23E: Large Delta / Determinism: single update crosses multiple tick boundaries without losing ticks.
        /// </summary>
        private static bool P07_9_P3_23E_SingleUpdateCrossesMultipleTickBoundariesWithoutLoss()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            // Large single delta: 1.2s (crosses 0.5s and 1.0s boundaries in a single tick)
            hero.TickActiveCast(1.2f);

            bool ok = (hero.CastState.ChannelTicksExecuted == 2) && hero.IsCasting;
            Debug.Log($"[P07_9_P3_23E] TEST 23E -> TicksExecuted: {hero.CastState.ChannelTicksExecuted} (expected 2) | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// Real Unity Runtime Play Mode Acceptance Scenario (Section 24).
        /// Drives the channel via Entity.Update(), proving:
        /// 1. Channel starts.
        /// 2. Unity Entity.Update() drives the channel.
        /// 3. Channel remains active between ticks.
        /// 4. Periodic effect occurs at tick boundary.
        /// 5. Channel completes.
        /// 6. Cooldown starts only after completion.
        /// 7. Effects execute through existing combat pipeline.
        /// 8. Shield absorbs channel damage when applicable.
        /// </summary>
        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9 Phase 3 Play Mode Acceptance Tests")]
        public static bool RunPrototype07_9_Phase3_PlayModeAcceptanceTests()
        {
            Debug.Log("==================================================");
            Debug.Log("   STARTING PROTOTYPE 07.9 PHASE 3 PLAY MODE ACCEPTANCE SCENARIO (Section 24)");
            Debug.Log("==================================================");

            Prototype01SceneBuilder.BuildPrototype02Data();
            Prototype01SceneBuilder.BuildPrototype01Scene();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);

            Hero sceneHero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            MindMethodManager mmMgr = UnityEngine.Object.FindAnyObjectByType<MindMethodManager>();

            if (sceneHero == null || bm == null || mmMgr == null)
            {
                Debug.LogError("[PLAY MODE] Missing essential scene components!");
                return false;
            }

            GameObject heroGO = sceneHero.gameObject;
            UnityEngine.Object.DestroyImmediate(sceneHero);
            TestChannelHero testHero = heroGO.AddComponent<TestChannelHero>();
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

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_DURATION = 1.0f; // TEST FIXTURE ONLY per Section 24
            const float TEST_INTERVAL = 0.5f; // TEST FIXTURE ONLY per Section 24
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, TEST_DURATION, TEST_INTERVAL);

            // Apply P07.8 Shield to monster to test shield absorption
            const float INITIAL_SHIELD = 500f;
            bm.CurrentMonster.StatusController.ApplyShield("pm_shield_p3", INITIAL_SHIELD, 30f);

            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;
            float initialHeroRage = testHero.Rage.CurrentRage;

            // --- 1. Channel starts ---
            var startRes = testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool startOk = startRes.Success && testHero.IsCasting && (testHero.Rage.CurrentRage == initialHeroRage - skillDef.RageCost);
            bool noCdAtStart = !CooldownManager.IsOnCooldown(skillDef.SkillId, out _);

            // --- 2. Entity.Update() drives channel: step 1 (0.25s) - between ticks ---
            testHero.CallUpdate(0.25f);
            bool betweenTicksOk = testHero.IsCasting && (testHero.CastState.ChannelTicksExecuted == 0) &&
                                  (bm.CurrentMonster.Health.CurrentHealth == initialMonsterHp);

            // --- 3. Entity.Update() drives channel: step 2 (0.25s) - reaching 0.5s tick boundary ---
            testHero.CallUpdate(0.25f);
            bool tick1Ok = testHero.IsCasting && (testHero.CastState.ChannelTicksExecuted == 1);
            var shieldAfterTick1 = bm.CurrentMonster.StatusController.GetShield("pm_shield_p3");
            float remShield1 = shieldAfterTick1 != null ? shieldAfterTick1.CurrentAmount : -1f;
            bool shieldAbsorbedTick1 = (remShield1 < INITIAL_SHIELD) && (bm.CurrentMonster.Health.CurrentHealth == initialMonsterHp);

            // --- 4. Entity.Update() drives channel: step 3 (0.25s) - between ticks ---
            testHero.CallUpdate(0.25f);
            bool betweenTicks2Ok = testHero.IsCasting && (testHero.CastState.ChannelTicksExecuted == 1);

            // --- 5. Entity.Update() drives channel: step 4 (0.25s) - reaching 1.0s completion boundary ---
            testHero.CallUpdate(0.25f);
            bool tick2Ok = (testHero.CastState.ChannelTicksExecuted == 2);
            bool completedOk = !testHero.IsCasting && (testHero.CastState.CurrentPhase == SkillCastPhase.Recovery);
            bool cdStartedAfter = CooldownManager.IsOnCooldown(skillDef.SkillId, out float remCd) && (remCd > 0f);

            bool pmPassed = startOk && noCdAtStart && betweenTicksOk && tick1Ok && shieldAbsorbedTick1 &&
                            betweenTicks2Ok && tick2Ok && completedOk && cdStartedAfter;

            Debug.Log($"[PLAY MODE ACCEPTANCE RESULTS]\n" +
                      $"1. Channel Starts: {startOk} (Rage consumed: {initialHeroRage}->{testHero.Rage.CurrentRage})\n" +
                      $"2. No Cooldown At Start: {noCdAtStart}\n" +
                      $"3. Active Between Ticks (0.25s): {betweenTicksOk}\n" +
                      $"4. Periodic Effect at Tick 1 (0.50s): {tick1Ok}, Shield Absorbed: {shieldAbsorbedTick1} (Rem={remShield1:F1})\n" +
                      $"5. Active Between Ticks (0.75s): {betweenTicks2Ok}\n" +
                      $"6. Periodic Effect at Tick 2 (1.00s): {tick2Ok}\n" +
                      $"7. Channel Complete at Boundary: {completedOk}\n" +
                      $"8. Cooldown Started After Complete: {cdStartedAfter} (Remain={remCd:F1}s)\n" +
                      $"RESULT: {(pmPassed ? "PASS" : "FAIL")}");

            skillDef.SetChannel(false, 0f, 0f);
            return pmPassed;
        }
    }
}
#endif
