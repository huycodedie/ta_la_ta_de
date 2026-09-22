#if UNITY_EDITOR
using System.Collections.Generic;
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
        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9 Phase 1 Tests (TEST 01 to TEST 12)")]
        public static bool RunAllPrototype07_9_Phase1_Tests()
        {
            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 PHASE 1 TESTS (TEST 01 -> TEST 12)");
            Debug.Log("   Scope: Runtime Cast State Foundation");
            Debug.Log("==================================================");

            bool t01 = P07_9_P1_01_CastState_InitializesAtReady();
            bool t02 = P07_9_P1_02_StartCast_TransitionsReadyToCasting();
            bool t03 = P07_9_P1_03_Tick_Deterministic_ElapsedTimeIncreasesAccurately();
            bool t04 = P07_9_P1_04_RemainingTime_ClampedNeverNegative();
            bool t05 = P07_9_P1_05_CastState_DoesNotExecuteEffectResolver();
            bool t06 = P07_9_P1_06_InstantSkillPath_Unaffected();
            bool t07 = P07_9_P1_07_SkillRuntimeState_CooldownNotDuplicatedByCastState();
            bool t08 = P07_9_P1_08_RageComponent_NotDuplicated();
            bool t09 = P07_9_P1_09_EntityStatusController_RemainsCCAuthority();
            bool t10 = P07_9_P1_10_Entity_InterruptCurrentAction_PreservesP07_6Behavior();
            bool t11 = P07_9_P1_11_RootFreezeStun_BehaviorUnchanged();
            bool t12 = P07_9_P1_12_P07_8_ShieldPipeline_Unchanged();

            bool allPassed = t01 && t02 && t03 && t04 && t05 && t06 && t07 && t08 && t09 && t10 && t11 && t12;

            Debug.Log("==================================================");
            Debug.Log("   PROTOTYPE 07.9 PHASE 1 TEST SUMMARY");
            Debug.Log("==================================================");
            Debug.Log($"   TEST 01 (Cast state initializes at READY):            {(t01 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 02 (StartCast transitions READY -> CASTING):     {(t02 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 03 (Deterministic Tick progression):              {(t03 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 04 (RemainingTime clamped non-negative):          {(t04 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 05 (Cast state does not self-execute effects):    {(t05 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 06 (Instant skill path unchanged):               {(t06 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 07 (Cooldown authority not duplicated):           {(t07 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 08 (Rage authority not duplicated):               {(t08 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 09 (EntityStatusController sole CC authority):    {(t09 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 10 (InterruptCurrentAction preserves P07.6):      {(t10 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 11 (Root/Freeze/Stun permissions preserved):      {(t11 ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST 12 (P07.8 Shield pipeline preserved):             {(t12 ? "PASS" : "FAIL")}");
            Debug.Log($"   OVERALL PHASE 1 STATUS: {(allPassed ? "100% PASS" : "FAIL")}");
            Debug.Log("==================================================");

            return allPassed;
        }

        /// <summary>
        /// TEST 01: Runtime Cast State có thể khởi tạo ở READY.
        /// </summary>
        private static bool P07_9_P1_01_CastState_InitializesAtReady()
        {
            var cast = new SkillCastState();
            bool ok = cast.CurrentPhase == SkillCastPhase.Ready &&
                      cast.ElapsedTime == 0f &&
                      cast.CastDuration == 0f &&
                      cast.RemainingTime == 0f &&
                      !cast.IsActive &&
                      !cast.IsInterrupted &&
                      !cast.IsFinished &&
                      cast.ActiveRequest == null;

            Debug.Log($"[P07_9_P1_01] TEST 01 -> InitAtReady: {ok} | {(ok ? "PASS" : "FAIL")}");
            return ok;
        }

        /// <summary>
        /// TEST 02: StartCast chuyển state: READY → CASTING.
        /// </summary>
        private static bool P07_9_P1_02_StartCast_TransitionsReadyToCasting()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            var req = new SkillExecutionRequest(hero, skillDef, SkillSlotType.Skill, bm.CurrentMonster);

            var cast = new SkillCastState();
            bool started = cast.StartCast(req, 1.5f, 0f, true);
            bool ok = started &&
                      cast.CurrentPhase == SkillCastPhase.Casting &&
                      cast.IsActive &&
                      cast.CastDuration == 1.5f &&
                      cast.ActiveRequest == req &&
                      cast.IsInterruptible;

            Debug.Log($"[P07_9_P1_02] TEST 02 -> Started: {started}, Phase: {cast.CurrentPhase}, Active: {cast.IsActive} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 03: Tick deterministic: elapsed time tăng chính xác.
        /// </summary>
        private static bool P07_9_P1_03_Tick_Deterministic_ElapsedTimeIncreasesAccurately()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            var req = new SkillExecutionRequest(hero, skillDef, SkillSlotType.Skill, bm.CurrentMonster);

            var cast = new SkillCastState();
            cast.StartCast(req, 2.0f);

            cast.Tick(0.1f);
            bool step1 = Mathf.Abs(cast.ElapsedTime - 0.1f) < 0.0001f;

            cast.Tick(0.1f);
            bool step2 = Mathf.Abs(cast.ElapsedTime - 0.2f) < 0.0001f;

            cast.Tick(0.8f);
            bool step3 = Mathf.Abs(cast.ElapsedTime - 1.0f) < 0.0001f;
            bool progressHalf = Mathf.Abs(cast.Progress - 0.5f) < 0.0001f;

            bool ok = step1 && step2 && step3 && progressHalf;
            Debug.Log($"[P07_9_P1_03] TEST 03 -> Step1: {step1}, Step2: {step2}, Step3: {step3}, ProgressHalf: {progressHalf} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 04: remaining time không âm (clamped non-negative).
        /// </summary>
        private static bool P07_9_P1_04_RemainingTime_ClampedNeverNegative()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            var req = new SkillExecutionRequest(hero, skillDef, SkillSlotType.Skill, bm.CurrentMonster);

            var cast = new SkillCastState();
            cast.StartCast(req, 0.5f);

            cast.Tick(0.3f);
            bool midRemain = Mathf.Abs(cast.RemainingTime - 0.2f) < 0.0001f;

            // Tick past duration
            cast.Tick(0.3f);
            bool clampedZero = cast.RemainingTime == 0f && cast.RemainingTime >= 0f;

            bool ok = midRemain && clampedZero;
            Debug.Log($"[P07_9_P1_04] TEST 04 -> MidRemain: {midRemain}, ClampedZero: {clampedZero} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 05: Cast state không tự execute EffectResolver.
        /// </summary>
        private static bool P07_9_P1_05_CastState_DoesNotExecuteEffectResolver()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            var req = new SkillExecutionRequest(hero, skillDef, SkillSlotType.Skill, bm.CurrentMonster);

            float monsterHpBefore = bm.CurrentMonster.Health.CurrentHealth;

            var cast = new SkillCastState();
            cast.StartCast(req, 1.0f);

            // Tick extensively past duration
            cast.Tick(1.0f);
            cast.Tick(5.0f);

            float monsterHpAfter = bm.CurrentMonster.Health.CurrentHealth;
            bool noEffectExecution = monsterHpBefore == monsterHpAfter;

            Debug.Log($"[P07_9_P1_05] TEST 05 -> HpBefore: {monsterHpBefore}, HpAfter: {monsterHpAfter}, NoEffectExec: {noEffectExecution} | {(noEffectExecution ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return noEffectExecution;
        }

        /// <summary>
        /// TEST 06: Instant skill path không bị thay đổi.
        /// </summary>
        private static bool P07_9_P1_06_InstantSkillPath_Unaffected()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(50f);

            float monsterHpBefore = bm.CurrentMonster.Health.CurrentHealth;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.NormalAttack, bm.CurrentMonster);
            float monsterHpAfter = bm.CurrentMonster.Health.CurrentHealth;

            bool executedSynchronously = res.Success && monsterHpAfter < monsterHpBefore && res.DamageResult.HasValue;
            Debug.Log($"[P07_9_P1_06] TEST 06 -> SyncSuccess: {res.Success}, DamageDealt: {monsterHpBefore - monsterHpAfter} | {(executedSynchronously ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return executedSynchronously;
        }

        /// <summary>
        /// TEST 07: SkillRuntimeState cooldown không bị duplicate bởi cast state.
        /// </summary>
        private static bool P07_9_P1_07_SkillRuntimeState_CooldownNotDuplicatedByCastState()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            var req = new SkillExecutionRequest(hero, skillDef, SkillSlotType.Skill, bm.CurrentMonster);

            var runtimeState = mmMgr.GetSkillState(skillDef.SkillId);
            float cdBefore = runtimeState != null ? runtimeState.CooldownEndTime : 0f;
            bool cdReadyBefore = CooldownManager.IsReady(skillDef.SkillId);

            var cast = new SkillCastState();
            cast.StartCast(req, 2.0f);
            cast.Tick(1.0f);

            float cdAfter = runtimeState != null ? runtimeState.CooldownEndTime : 0f;
            bool cdReadyAfter = CooldownManager.IsReady(skillDef.SkillId);

            bool noCooldownInterference = (cdBefore == cdAfter) && (cdReadyBefore == cdReadyAfter);
            Debug.Log($"[P07_9_P1_07] TEST 07 -> NoCooldownInterference: {noCooldownInterference} | {(noCooldownInterference ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return noCooldownInterference;
        }

        /// <summary>
        /// TEST 08: RageComponent không bị duplicate.
        /// </summary>
        private static bool P07_9_P1_08_RageComponent_NotDuplicated()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(45f);

            float rageBefore = hero.Rage.CurrentRage;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            var req = new SkillExecutionRequest(hero, skillDef, SkillSlotType.Skill, bm.CurrentMonster);

            var cast = new SkillCastState();
            cast.StartCast(req, 2.0f);
            cast.Tick(1.0f);

            float rageAfter = hero.Rage.CurrentRage;
            bool rageUnchanged = (rageBefore == rageAfter);

            Debug.Log($"[P07_9_P1_08] TEST 08 -> RageBefore: {rageBefore}, RageAfter: {rageAfter}, Unchanged: {rageUnchanged} | {(rageUnchanged ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return rageUnchanged;
        }

        /// <summary>
        /// TEST 09: EntityStatusController vẫn là CC authority.
        /// </summary>
        private static bool P07_9_P1_09_EntityStatusController_RemainsCCAuthority()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");

            bool canSkillInit = hero.CanUseSkill && hero.StatusController.CanUseSkill;

            // Apply Stun via EntityStatusController
            hero.StatusController.ApplyCrowdControl("cc_stun_test", CrowdControlType.Stun, 2f, EffectPowerTier.TierB, bm.CurrentMonster);
            bool stunnedBlocked = !hero.CanUseSkill && !hero.StatusController.CanUseSkill && hero.IsStunned;

            // Clear CC
            hero.StatusController.ClearAllCrowdControl();
            bool restored = hero.CanUseSkill && hero.StatusController.CanUseSkill && !hero.IsStunned;

            bool ok = canSkillInit && stunnedBlocked && restored;
            Debug.Log($"[P07_9_P1_09] TEST 09 -> CanSkillInit: {canSkillInit}, StunnedBlocked: {stunnedBlocked}, Restored: {restored} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 10: Entity.InterruptCurrentAction hiện tại vẫn giữ behavior P07.6.
        /// </summary>
        private static bool P07_9_P1_10_Entity_InterruptCurrentAction_PreservesP07_6Behavior()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);

            // Advance attack timer
            hero.Attack.ManualTick(0.8f);
            float timerBefore = hero.Attack.AttackTimer;

            // Call InterruptCurrentAction
            hero.InterruptCurrentAction();
            float timerAfter = hero.Attack.AttackTimer;

            bool ok = timerBefore > 0f && timerAfter == 0f;
            Debug.Log($"[P07_9_P1_10] TEST 10 -> TimerBefore: {timerBefore}, TimerAfter: {timerAfter} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 11: Root/Freeze/Stun behavior hiện tại không thay đổi.
        /// </summary>
        private static bool P07_9_P1_11_RootFreezeStun_BehaviorUnchanged()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);

            // 1. Root: CanMove false, CanUseSkill true, CanUseUltimate true
            hero.StatusController.ApplyCrowdControl("test_root", CrowdControlType.Root, 2f, EffectPowerTier.TierB, bm.CurrentMonster);
            bool rootOk = !hero.CanMove && hero.CanUseSkill && hero.CanUseUltimate;
            hero.StatusController.ClearAllCrowdControl();

            // 2. Stun: CanMove false, CanUseSkill false, CanUseUltimate false
            hero.StatusController.ApplyCrowdControl("test_stun", CrowdControlType.Stun, 2f, EffectPowerTier.TierB, bm.CurrentMonster);
            bool stunOk = !hero.CanMove && !hero.CanUseSkill && !hero.CanUseUltimate;
            hero.StatusController.ClearAllCrowdControl();

            // 3. Freeze: CanMove false, CanUseSkill false, CanUseUltimate false
            hero.StatusController.ApplyCrowdControl("test_freeze", CrowdControlType.Freeze, 2f, EffectPowerTier.TierB, bm.CurrentMonster);
            bool freezeOk = !hero.CanMove && !hero.CanUseSkill && !hero.CanUseUltimate;
            hero.StatusController.ClearAllCrowdControl();

            bool ok = rootOk && stunOk && freezeOk;
            Debug.Log($"[P07_9_P1_11] TEST 11 -> RootOk: {rootOk}, StunOk: {stunOk}, FreezeOk: {freezeOk} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 12: P07.8 Shield pipeline không thay đổi.
        /// </summary>
        private static bool P07_9_P1_12_P07_8_ShieldPipeline_Unchanged()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);

            float hpBefore = hero.Health.CurrentHealth;
            hero.StatusController.ApplyShield("shield_p1_test", 50f, 10f);

            // Take 20 damage -> fully absorbed by shield, 0 damage to HP
            hero.Health.TakeDamage(20f);

            float hpAfter = hero.Health.CurrentHealth;
            var shield = hero.StatusController.GetShield("shield_p1_test");
            float remainingShield = shield != null ? shield.CurrentAmount : -1f;

            bool shieldAbsorbed = (hpBefore == hpAfter) && Mathf.Abs(remainingShield - 30f) < 0.0001f;
            Debug.Log($"[P07_9_P1_12] TEST 12 -> HpBefore: {hpBefore}, HpAfter: {hpAfter}, RemainShield: {remainingShield} | {(shieldAbsorbed ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return shieldAbsorbed;
        }
    }
}
#endif
