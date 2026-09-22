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
using WuxiaGame.Entities.Components;
using WuxiaGame.Progression;
using WuxiaGame.Stats;

namespace WuxiaGame.Editor
{
    public static partial class Prototype01PlayTestRunner
    {
        private class TestPhase5Hero : Hero
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
                if (Movement != null)
                {
                    Movement.ManualTick(SimulatedDeltaTime);
                }
            }
        }

        private static bool ApplyCC_P5(Entity entity, CrowdControlType ccType, float duration, string ccId = null)
        {
            if (entity == null || entity.StatusController == null) return false;
            if (string.IsNullOrEmpty(ccId))
            {
                ccId = "cc_p5_" + ccType.ToString().ToLower() + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            }
            return entity.StatusController.ApplyCrowdControl(ccId, ccType, duration);
        }

        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9 Phase 5.2 Tests (TEST 01 to TEST 20 + Play Mode Scenarios A-D)")]
        public static bool RunAllPrototype07_9_Phase5_Tests()
        {
            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 PHASE 5.2 TESTS (MOVEMENT LOCK)");
            Debug.Log("==================================================");

            int passed = 0;
            int total = 24; // 20 Automated Tests + 4 Play Mode Scenarios

            bool t01 = P07_9_P5_01_NonCastingEntityCanMoveNormally();
            if (t01) passed++;

            bool t02 = P07_9_P5_02_CastTimeSkillSetsCanMoveFalse();
            if (t02) passed++;

            bool t03 = P07_9_P5_03_MoveTowardTargetCannotChangePosDuringCast();
            if (t03) passed++;

            bool t04 = P07_9_P5_04_ManualTickCannotChangePosDuringCast();
            if (t04) passed++;

            bool t05 = P07_9_P5_05_CastCompletionRestoresCanMove();
            if (t05) passed++;

            bool t06 = P07_9_P5_06_EntityResumesMovementAfterCastCompletion();
            if (t06) passed++;

            bool t07 = P07_9_P5_07_ChannelSkillSetsCanMoveFalse();
            if (t07) passed++;

            bool t08 = P07_9_P5_08_MovementBlockedBetweenChannelTicks();
            if (t08) passed++;

            bool t09 = P07_9_P5_09_ChannelCompletionRestoresMovement();
            if (t09) passed++;

            bool t10 = P07_9_P5_10_StunInterruptsCastAndKeepsMovementBlocked();
            if (t10) passed++;

            bool t11 = P07_9_P5_11_AfterStunExpiresMovementRestores();
            if (t11) passed++;

            bool t12 = P07_9_P5_12_FreezeInterruptsCastAndKeepsMovementBlocked();
            if (t12) passed++;

            bool t13 = P07_9_P5_13_AfterFreezeExpiresMovementRestores();
            if (t13) passed++;

            bool t14 = P07_9_P5_14_RootDuringCastDoesNotInterruptAndBlocksMovement();
            if (t14) passed++;

            bool t15 = P07_9_P5_15_RootPersistsAfterCastAndBlocksMovement();
            if (t15) passed++;

            bool t16 = P07_9_P5_16_AfterRootExpiresMovementRestores();
            if (t16) passed++;

            bool t17 = P07_9_P5_17_OrdinaryDamageDoesNotInterruptOrRestoreMovement();
            if (t17) passed++;

            bool t18 = P07_9_P5_18_InstantSkillDoesNotLeaveLingeringMovementLock();
            if (t18) passed++;

            bool t19 = P07_9_P5_19_DeathDuringCastReleasesCastStateAndReviveRestores();
            if (t19) passed++;

            bool t20 = P07_9_P5_20_CanDashFalseDuringCastAndChannel();
            if (t20) passed++;

            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 PHASE 5.2 PLAY MODE SCENARIOS A-D");
            Debug.Log("==================================================");

            bool pma = P07_9_P5_ScenarioA_CastMovementLock_PlayMode();
            if (pma) passed++;

            bool pmb = P07_9_P5_ScenarioB_ChannelMovementLock_PlayMode();
            if (pmb) passed++;

            bool pmc = P07_9_P5_ScenarioC_StunInterrupt_PlayMode();
            if (pmc) passed++;

            bool pmd = P07_9_P5_ScenarioD_RootCoexistence_PlayMode();
            if (pmd) passed++;

            Debug.Log($"[PROTOTYPE 07.9 PHASE 5.2 SUMMARY] Passed: {passed}/{total} ({(passed == total ? "ALL PASS" : "FAILURES DETECTED")})");
            return passed == total;
        }

        /// <summary>
        /// TEST 01: Non-casting entity can move normally when target is outside attack range.
        /// </summary>
        private static bool P07_9_P5_01_NonCastingEntityCanMoveNormally()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            bool canMoveInit = hero.CanMove && !hero.IsCasting;
            float startX = hero.transform.position.x;

            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float endX = hero.transform.position.x;

            bool moved = endX > startX;
            bool ok = canMoveInit && moved;
            Debug.Log($"[P07_9_P5_01] TEST 01 -> CanMoveInit: {canMoveInit}, Moved: {moved} ({startX:F2} -> {endX:F2}) | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 02: Cast-time skill immediately sets CanMove == false.
        /// </summary>
        private static bool P07_9_P5_02_CastTimeSkillSetsCanMoveFalse()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            bool canMoveBefore = hero.CanMove;
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool ok = canMoveBefore && hero.IsCasting && !hero.CanMove;
            Debug.Log($"[P07_9_P5_02] TEST 02 -> CanMoveBefore: {canMoveBefore}, IsCasting: {hero.IsCasting}, CanMoveDuring: {hero.CanMove} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 03: MovementComponent.MoveTowardTarget cannot change position during active cast.
        /// </summary>
        private static bool P07_9_P5_03_MoveTowardTargetCannotChangePosDuringCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float posBefore = hero.transform.position.x;

            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = hero.IsCasting && Mathf.Approximately(posBefore, posAfter);
            Debug.Log($"[P07_9_P5_03] TEST 03 -> PosBefore: {posBefore}, PosAfter: {posAfter}, Frozen: {Mathf.Approximately(posBefore, posAfter)} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 04: MovementComponent.ManualTick cannot change position during active cast.
        /// </summary>
        private static bool P07_9_P5_04_ManualTickCannotChangePosDuringCast()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            float posBefore = hero.transform.position.x;

            hero.Movement.ManualTick(0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = hero.IsCasting && Mathf.Approximately(posBefore, posAfter);
            Debug.Log($"[P07_9_P5_04] TEST 04 -> PosBefore: {posBefore}, PosAfter: {posAfter}, Frozen: {Mathf.Approximately(posBefore, posAfter)} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 05: Cast completion restores movement permission when no CC exists.
        /// </summary>
        private static bool P07_9_P5_05_CastCompletionRestoresCanMove()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool lockedDuring = !hero.CanMove;

            hero.TickActiveCast(1.0f); // completes

            bool restoredAfter = hero.CanMove && !hero.IsCasting;
            bool ok = lockedDuring && restoredAfter;
            Debug.Log($"[P07_9_P5_05] TEST 05 -> LockedDuring: {lockedDuring}, RestoredAfter: {restoredAfter} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 06: Entity resumes movement toward target after cast completion.
        /// </summary>
        private static bool P07_9_P5_06_EntityResumesMovementAfterCastCompletion()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(1.0f); // completes

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool moved = posAfter > posBefore;
            bool ok = hero.CanMove && moved;
            Debug.Log($"[P07_9_P5_06] TEST 06 -> MovedAfterComplete: {moved} ({posBefore:F2} -> {posAfter:F2}) | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 07: Channel skill immediately sets CanMove == false.
        /// </summary>
        private static bool P07_9_P5_07_ChannelSkillSetsCanMoveFalse()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            bool canMoveBefore = hero.CanMove;
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool ok = canMoveBefore && hero.IsCasting && !hero.CanMove;
            Debug.Log($"[P07_9_P5_07] TEST 07 -> CanMoveBefore: {canMoveBefore}, ChannelIsCasting: {hero.IsCasting}, CanMoveDuring: {hero.CanMove} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 08: Movement remains blocked between channel ticks.
        /// </summary>
        private static bool P07_9_P5_08_MovementBlockedBetweenChannelTicks()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            // Tick 1 (0.6s)
            hero.TickActiveCast(0.6f);
            bool lockedAtTick1 = !hero.CanMove;

            float posBefore = hero.transform.position.x;
            hero.Movement.ManualTick(0.25f);
            float posMid = hero.transform.position.x;

            // Tick 2 (0.5s more -> 1.1s total)
            hero.TickActiveCast(0.5f);
            bool lockedAtTick2 = !hero.CanMove;

            hero.Movement.ManualTick(0.25f);
            float posEnd = hero.transform.position.x;

            bool ok = lockedAtTick1 && lockedAtTick2 &&
                      Mathf.Approximately(posBefore, posMid) && Mathf.Approximately(posMid, posEnd);
            Debug.Log($"[P07_9_P5_08] TEST 08 -> LockedT1: {lockedAtTick1}, LockedT2: {lockedAtTick2}, PosFrozen: {Mathf.Approximately(posBefore, posEnd)} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 09: Channel completion restores movement permission.
        /// </summary>
        private static bool P07_9_P5_09_ChannelCompletionRestoresMovement()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(2.0f); // completes

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = !hero.IsCasting && hero.CanMove && (posAfter > posBefore);
            Debug.Log($"[P07_9_P5_09] TEST 09 -> CanMoveAfterChannel: {hero.CanMove}, Moved: {posAfter > posBefore} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 10: Stun interrupts cast and keeps movement blocked.
        /// </summary>
        private static bool P07_9_P5_10_StunInterruptsCastAndKeepsMovementBlocked()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC_P5(hero, CrowdControlType.Stun, 2.0f, "cc_stun_t10");

            bool castInterrupted = !hero.IsCasting && hero.CastState.IsInterrupted;
            bool stillBlocked = !hero.CanMove && hero.IsStunned;

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = castInterrupted && stillBlocked && Mathf.Approximately(posBefore, posAfter);
            Debug.Log($"[P07_9_P5_10] TEST 10 -> CastInterrupted: {castInterrupted}, StillBlockedByStun: {stillBlocked}, PosFrozen: {Mathf.Approximately(posBefore, posAfter)} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 11: After Stun expires, movement restores.
        /// </summary>
        private static bool P07_9_P5_11_AfterStunExpiresMovementRestores()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC_P5(hero, CrowdControlType.Stun, 2.0f, "cc_stun_t11");
            hero.StatusController.RemoveCrowdControl("cc_stun_t11"); // expire stun

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = !hero.IsStunned && !hero.IsCasting && hero.CanMove && (posAfter > posBefore);
            Debug.Log($"[P07_9_P5_11] TEST 11 -> StunExpired: {!hero.IsStunned}, CanMove: {hero.CanMove}, Moved: {posAfter > posBefore} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 12: Freeze interrupts cast and keeps movement blocked.
        /// </summary>
        private static bool P07_9_P5_12_FreezeInterruptsCastAndKeepsMovementBlocked()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC_P5(hero, CrowdControlType.Freeze, 2.0f, "cc_freeze_t12");

            bool castInterrupted = !hero.IsCasting && hero.CastState.IsInterrupted;
            bool stillBlocked = !hero.CanMove && hero.IsFrozen;

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = castInterrupted && stillBlocked && Mathf.Approximately(posBefore, posAfter);
            Debug.Log($"[P07_9_P5_12] TEST 12 -> CastInterrupted: {castInterrupted}, StillBlockedByFreeze: {stillBlocked}, PosFrozen: {Mathf.Approximately(posBefore, posAfter)} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 13: After Freeze expires, movement restores.
        /// </summary>
        private static bool P07_9_P5_13_AfterFreezeExpiresMovementRestores()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC_P5(hero, CrowdControlType.Freeze, 2.0f, "cc_freeze_t13");
            hero.StatusController.RemoveCrowdControl("cc_freeze_t13"); // expire freeze

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = !hero.IsFrozen && !hero.IsCasting && hero.CanMove && (posAfter > posBefore);
            Debug.Log($"[P07_9_P5_13] TEST 13 -> FreezeExpired: {!hero.IsFrozen}, CanMove: {hero.CanMove}, Moved: {posAfter > posBefore} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 14: Root during active cast does not interrupt and blocks movement.
        /// </summary>
        private static bool P07_9_P5_14_RootDuringCastDoesNotInterruptAndBlocksMovement()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.4f);

            ApplyCC_P5(hero, CrowdControlType.Root, 2.0f, "cc_root_t14");

            bool stillCasting = hero.IsCasting && hero.IsRooted;
            bool moveBlocked = !hero.CanMove;

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = stillCasting && moveBlocked && Mathf.Approximately(posBefore, posAfter);
            Debug.Log($"[P07_9_P5_14] TEST 14 -> StillCasting: {stillCasting}, MoveBlocked: {moveBlocked}, PosFrozen: {Mathf.Approximately(posBefore, posAfter)} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 15: Root persists after cast completion and continues blocking movement.
        /// </summary>
        private static bool P07_9_P5_15_RootPersistsAfterCastAndBlocksMovement()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.4f);

            ApplyCC_P5(hero, CrowdControlType.Root, 2.0f, "cc_root_t15");

            // Complete cast
            hero.TickActiveCast(0.6f);

            bool castCompleted = !hero.IsCasting;
            bool stillRooted = hero.IsRooted;
            bool stillBlocked = !hero.CanMove;

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = castCompleted && stillRooted && stillBlocked && Mathf.Approximately(posBefore, posAfter);
            Debug.Log($"[P07_9_P5_15] TEST 15 -> CastCompleted: {castCompleted}, Rooted: {stillRooted}, Blocked: {stillBlocked} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 16: After Root expires, movement restores.
        /// </summary>
        private static bool P07_9_P5_16_AfterRootExpiresMovementRestores()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            ApplyCC_P5(hero, CrowdControlType.Root, 2.0f, "cc_root_t16");
            hero.TickActiveCast(1.0f); // cast complete

            hero.StatusController.RemoveCrowdControl("cc_root_t16"); // expire root

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = !hero.IsRooted && !hero.IsCasting && hero.CanMove && (posAfter > posBefore);
            Debug.Log($"[P07_9_P5_16] TEST 16 -> RootExpired: {!hero.IsRooted}, CanMove: {hero.CanMove}, Moved: {posAfter > posBefore} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 17: Ordinary non-lethal damage during cast does not interrupt or restore movement.
        /// </summary>
        private static bool P07_9_P5_17_OrdinaryDamageDoesNotInterruptOrRestoreMovement()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            hero.Health.TakeDamage(10f); // non-lethal damage

            bool stillCasting = hero.IsCasting;
            bool stillBlocked = !hero.CanMove;

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool ok = stillCasting && stillBlocked && Mathf.Approximately(posBefore, posAfter);
            Debug.Log($"[P07_9_P5_17] TEST 17 -> StillCasting: {stillCasting}, MoveBlocked: {stillBlocked}, PosFrozen: {Mathf.Approximately(posBefore, posAfter)} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 18: CastTime == 0 instant skill does not leave lingering movement lock.
        /// </summary>
        private static bool P07_9_P5_18_InstantSkillDoesNotLeaveLingeringMovementLock()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(false, 0f, 0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);

            bool ok = !hero.IsCasting && hero.CanMove;
            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;

            bool moved = posAfter > posBefore;
            bool ok2 = ok && moved;
            Debug.Log($"[P07_9_P5_18] TEST 18 -> Instant CanMove: {hero.CanMove}, Moved: {moved} | {(ok2 ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok2;
        }

        /// <summary>
        /// TEST 19: Death during cast releases cast state; revived entity has no residual cast movement lock.
        /// </summary>
        private static bool P07_9_P5_19_DeathDuringCastReleasesCastStateAndReviveRestores()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            heroGO.transform.position = new Vector3(0f, 0f, 0f);
            bm.CurrentMonster.transform.position = new Vector3(10f, 0f, 0f);

            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.Health.TakeDamage(hero.Health.MaxHealth + 100f); // lethal damage
            if (hero.IsAlive)
            {
                // MindMethod passive revive once may have been consumed; kill again to ensure death
                hero.Health.TakeDamage(hero.Health.MaxHealth + 100f);
            }
            hero.TickActiveCast(0.1f); // triggers death interrupt

            bool deadNotCasting = !hero.IsAlive && !hero.IsCasting;
            float deadPosBefore = hero.transform.position.x;
            hero.Movement.ManualTick(0.5f);
            float deadPosAfter = hero.transform.position.x;
            bool deadMovementBlocked = Mathf.Approximately(deadPosBefore, deadPosAfter);

            // Revive hero
            hero.Health.Revive(hero.Health.MaxHealth);

            bool revivedCanMove = hero.IsAlive && !hero.IsCasting && hero.CanMove;

            float posBefore = hero.transform.position.x;
            hero.Movement.MoveTowardTarget(bm.CurrentMonster.transform.position, hero.AttackRange, 0.5f);
            float posAfter = hero.transform.position.x;
            bool moved = posAfter > posBefore;

            bool ok = deadNotCasting && deadMovementBlocked && revivedCanMove && moved;
            Debug.Log($"[P07_9_P5_19] TEST 19 -> DeadNotCasting: {deadNotCasting}, DeadMovementBlocked: {deadMovementBlocked}, RevivedCanMove: {revivedCanMove}, Moved: {moved} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST 20: CanDash == false during Cast/Channel.
        /// </summary>
        private static bool P07_9_P5_20_CanDashFalseDuringCastAndChannel()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            hero.Rage.AddRage(100f);

            // 1. Cast-time skill
            skillDef.SetCastTime(1.0f);
            skillDef.SetChannel(false, 0f, 0f);
            bool canDashBefore = hero.CanDash;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool canDashDuringCast = hero.CanDash;

            hero.TickActiveCast(1.0f);
            bool canDashAfterCast = hero.CanDash;

            // 2. Channel skill (reset cooldown & rage)
            CooldownManager.ResetAllCooldowns();
            hero.Rage.AddRage(100f);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool canDashDuringChannel = hero.CanDash;

            hero.TickActiveCast(2.0f);
            bool canDashAfterChannel = hero.CanDash;

            bool ok = canDashBefore && !canDashDuringCast && canDashAfterCast &&
                      !canDashDuringChannel && canDashAfterChannel;
            Debug.Log($"[P07_9_P5_20] TEST 20 -> Before: {canDashBefore}, DuringCast: {canDashDuringCast}, AfterCast: {canDashAfterCast}, DuringChan: {canDashDuringChannel}, AfterChan: {canDashAfterChannel} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        // =========================================================================
        // PLAY MODE ACCEPTANCE SCENARIOS A - D
        // =========================================================================

        private static bool SetupPlayModeScene_P5(out TestPhase5Hero testHero, out BattleManager bm, out MindMethodManager mmMgr)
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
            testHero = heroGO.AddComponent<TestPhase5Hero>();
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

            // Set positions for distance testing
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
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            CooldownManager.ResetAllCooldowns();
            testHero.Rage.AddRage(100f);
            return true;
        }

        /// <summary>
        /// SCENARIO A: CAST MOVEMENT LOCK (Play Mode)
        /// Hero initially moves toward Monster.
        /// Start CastTime > 0 skill -> Position frozen during cast.
        /// Cast completes -> Hero resumes movement toward Monster.
        /// </summary>
        private static bool P07_9_P5_ScenarioA_CastMovementLock_PlayMode()
        {
            if (!SetupPlayModeScene_P5(out TestPhase5Hero testHero, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_CAST_TIME = 1.0f;
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(TEST_CAST_TIME);

            // 1. Initial movement
            float initX = testHero.transform.position.x;
            testHero.CallUpdate(0.2f);
            float movedX = testHero.transform.position.x;
            bool initialMoveOk = movedX > initX;

            // 2. Start cast
            var startRes = testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool castStarted = startRes.Success && testHero.IsCasting && !testHero.CanMove;

            // 3. Progress while casting
            float castStartX = testHero.transform.position.x;
            testHero.CallUpdate(0.4f);
            float castMidX = testHero.transform.position.x;
            bool positionFrozen = Mathf.Approximately(castStartX, castMidX);

            // 4. Complete cast
            testHero.CallUpdate(0.6f);
            bool castCompleted = !testHero.IsCasting && testHero.CanMove;

            // 5. Resume movement
            float completeX = testHero.transform.position.x;
            testHero.CallUpdate(0.3f);
            float resumeX = testHero.transform.position.x;
            bool resumedMoveOk = resumeX > completeX;

            bool pass = initialMoveOk && castStarted && positionFrozen && castCompleted && resumedMoveOk;
            Debug.Log($"[SCENARIO A — CAST MOVEMENT LOCK] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. InitialMove: {initialMoveOk} ({initX:F2} -> {movedX:F2})\n" +
                      $"2. CastStarted: {castStarted}\n" +
                      $"3. PositionFrozenDuringCast: {positionFrozen} ({castStartX:F2} == {castMidX:F2})\n" +
                      $"4. CastCompleted: {castCompleted}\n" +
                      $"5. ResumedMove: {resumedMoveOk} ({completeX:F2} -> {resumeX:F2})");

            skillDef.SetCastTime(0f);
            return pass;
        }

        /// <summary>
        /// SCENARIO B: CHANNEL MOVEMENT LOCK (Play Mode)
        /// Position frozen across channel ticks, resumes after completion.
        /// </summary>
        private static bool P07_9_P5_ScenarioB_ChannelMovementLock_PlayMode()
        {
            if (!SetupPlayModeScene_P5(out TestPhase5Hero testHero, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            const float TEST_DURATION = 2.0f;
            const float TEST_INTERVAL = 0.5f;
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, TEST_DURATION, TEST_INTERVAL);

            // Start Channel
            var startRes = testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool chanStarted = startRes.Success && testHero.IsCasting && !testHero.CanMove;

            float chanStartX = testHero.transform.position.x;

            // Step through ticks
            testHero.CallUpdate(0.6f); // Tick 1
            float tick1X = testHero.transform.position.x;

            testHero.CallUpdate(0.5f); // Tick 2
            float tick2X = testHero.transform.position.x;

            testHero.CallUpdate(0.5f); // Tick 3
            float tick3X = testHero.transform.position.x;

            bool ticksFrozen = Mathf.Approximately(chanStartX, tick1X) &&
                               Mathf.Approximately(tick1X, tick2X) &&
                               Mathf.Approximately(tick2X, tick3X);

            // Complete channel
            testHero.CallUpdate(0.5f); // Duration reached
            bool chanCompleted = !testHero.IsCasting && testHero.CanMove;

            // Resume movement
            float postChanX = testHero.transform.position.x;
            testHero.CallUpdate(0.3f);
            float resumeX = testHero.transform.position.x;
            bool resumedMove = resumeX > postChanX;

            bool pass = chanStarted && ticksFrozen && chanCompleted && resumedMove;
            Debug.Log($"[SCENARIO B — CHANNEL MOVEMENT LOCK] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. ChannelStarted: {chanStarted}\n" +
                      $"2. TicksPositionFrozen: {ticksFrozen}\n" +
                      $"3. ChannelCompleted: {chanCompleted}\n" +
                      $"4. ResumedMove: {resumedMove} ({postChanX:F2} -> {resumeX:F2})");

            skillDef.SetChannel(false, 0f, 0f);
            return pass;
        }

        /// <summary>
        /// SCENARIO C: STUN INTERRUPT & RESTORATION (Play Mode)
        /// Start cast -> locked. Apply Stun -> interrupted, locked. Stun expires -> resumes.
        /// </summary>
        private static bool P07_9_P5_ScenarioC_StunInterrupt_PlayMode()
        {
            if (!SetupPlayModeScene_P5(out TestPhase5Hero testHero, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);

            testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            testHero.CallUpdate(0.3f);

            // Apply Stun
            ApplyCC_P5(testHero, CrowdControlType.Stun, 2.0f, "pm_stun_c");
            bool castInterrupted = !testHero.IsCasting && testHero.IsStunned && !testHero.CanMove;

            float stunStartX = testHero.transform.position.x;
            testHero.CallUpdate(0.4f);
            float stunMidX = testHero.transform.position.x;
            bool positionFrozenStun = Mathf.Approximately(stunStartX, stunMidX);

            // Expire Stun
            testHero.StatusController.RemoveCrowdControl("pm_stun_c");
            bool stunRemoved = !testHero.IsStunned && testHero.CanMove;

            // Movement resumes
            testHero.CallUpdate(0.3f);
            float resumeX = testHero.transform.position.x;
            bool resumedMove = resumeX > stunMidX;

            bool pass = castInterrupted && positionFrozenStun && stunRemoved && resumedMove;
            Debug.Log($"[SCENARIO C — STUN INTERRUPT] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. CastInterruptedByStun: {castInterrupted}\n" +
                      $"2. FrozenDuringStun: {positionFrozenStun}\n" +
                      $"3. StunRemoved: {stunRemoved}\n" +
                      $"4. ResumedMove: {resumedMove} ({stunMidX:F2} -> {resumeX:F2})");

            skillDef.SetCastTime(0f);
            return pass;
        }

        /// <summary>
        /// SCENARIO D: ROOT COEXISTENCE (Play Mode)
        /// Start cast -> apply Root -> cast completes, position frozen. Root expires -> resumes.
        /// </summary>
        private static bool P07_9_P5_ScenarioD_RootCoexistence_PlayMode()
        {
            if (!SetupPlayModeScene_P5(out TestPhase5Hero testHero, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);

            testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            testHero.CallUpdate(0.3f);

            // Apply Root during cast
            ApplyCC_P5(testHero, CrowdControlType.Root, 2.0f, "pm_root_d");
            bool stillCasting = testHero.IsCasting && testHero.IsRooted && !testHero.CanMove;

            float rootStartX = testHero.transform.position.x;

            // Complete cast
            testHero.CallUpdate(0.7f);
            bool castCompleted = !testHero.IsCasting && testHero.IsRooted && !testHero.CanMove;
            float castEndRootX = testHero.transform.position.x;
            bool frozenAfterComplete = Mathf.Approximately(rootStartX, castEndRootX);

            // Still cannot move while Root remains
            testHero.CallUpdate(0.3f);
            float stillRootedX = testHero.transform.position.x;
            bool stillFrozen = Mathf.Approximately(castEndRootX, stillRootedX);

            // Expire Root
            testHero.StatusController.RemoveCrowdControl("pm_root_d");
            bool rootExpired = !testHero.IsRooted && testHero.CanMove;

            // Movement resumes
            testHero.CallUpdate(0.3f);
            float resumedX = testHero.transform.position.x;
            bool resumedMove = resumedX > stillRootedX;

            bool pass = stillCasting && castCompleted && frozenAfterComplete && stillFrozen && rootExpired && resumedMove;
            Debug.Log($"[SCENARIO D — ROOT COEXISTENCE] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. CastContinuesUnderRoot: {stillCasting}\n" +
                      $"2. CastCompletedStillRooted: {castCompleted}\n" +
                      $"3. FrozenAfterCastComplete: {frozenAfterComplete}\n" +
                      $"4. StillFrozenWhileRootActive: {stillFrozen}\n" +
                      $"5. RootExpired: {rootExpired}\n" +
                      $"6. ResumedMoveAfterRoot: {resumedMove} ({stillRootedX:F2} -> {resumedX:F2})");

            skillDef.SetCastTime(0f);
            return pass;
        }
    }
}
#endif
