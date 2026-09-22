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
    public static class Prototype01PlayTestRunner_P07_9_1_Audit
    {
        [MenuItem("Tools/Wuxia RPG/Run P07.9.1 Hero Skill Autonomous Execution Audit")]
        public static bool RunAudit()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING P07.9.1 HERO SKILL AUTONOMOUS EXECUTION AUDIT (TESTS A TO J)");
            Debug.Log("================================================================================");

            bool testA = Audit_TestA_AutonomousSkillCasting();
            bool testB = Audit_TestB_SkillPriorityEvaluation();
            bool testC = Audit_TestC_AutonomousUltimateExecution();
            bool testD = Audit_TestD_AutoOffBehavior();
            bool testE = Audit_TestE_ManualSkillInput();
            bool testF = Audit_TestF_CastTimeStateTransition();
            bool testG = Audit_TestG_ChannelSkillExecution();
            bool testH = Audit_TestH_StunDuringCastInterrupt();
            bool testI = Audit_TestI_RootDuringCastContinues();
            bool testJ = Audit_TestJ_OrdinaryDamageDuringCastContinues();

            Debug.Log("================================================================================");
            Debug.Log($"   P07.9.1 AUDIT SUMMARY:");
            Debug.Log($"   TEST A (Autonomous Skill Cast): {(testA ? "AUTONOMOUS ACTIVE" : "BROKEN / MISSING (Hero never auto-casts)")}");
            Debug.Log($"   TEST B (Skill Priority System): {(testB ? "ACTIVE" : "BROKEN / MISSING (No priority authority)")}");
            Debug.Log($"   TEST C (Autonomous Ultimate):   {(testC ? "AUTONOMOUS ACTIVE" : "BROKEN / MISSING (Hero never auto-ultimates)")}");
            Debug.Log($"   TEST D (Auto OFF Stop Cast):    {(testD ? "ACTIVE" : "NO EFFECT (No autonomous casting exists to stop)")}");
            Debug.Log($"   TEST E (Manual Skill Input):    {(testE ? "PASS (Execution pipeline operational)" : "FAIL")}");
            Debug.Log($"   TEST F (Cast Time Transition):  {(testF ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST G (Channel Skill):         {(testG ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST H (Stun Interrupt):        {(testH ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST I (Root Continues):        {(testI ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST J (Damage Continues):      {(testJ ? "PASS" : "FAIL")}");
            Debug.Log("================================================================================");

            bool executionPipelineHealthy = testE && testF && testG && testH && testI && testJ;
            bool autonomousDecisionMissing = !testA && !testB && !testC;

            if (executionPipelineHealthy && autonomousDecisionMissing)
            {
                Debug.LogWarning(">>> [P07.9.1 AUDIT CLASSIFICATION] RESULT: CASE A <<<");
                Debug.LogWarning(">>> REASON: Decision Layer does not invoke Hero.ExecuteSelectedSkill. Execution pipeline (Validator, Executor, CastState, Cooldown, Damage) is 100% operational, but no autonomous decision layer exists in Hero/AttackComponent/BattleManager to evaluate cooldowns/rage and trigger skill requests during combat.");
            }

            return executionPipelineHealthy;
        }

        private static bool SetupScene(out Hero hero, out Monster monster, out BattleManager bm, out MindMethodManager mmMgr)
        {
            hero = null;
            monster = null;
            bm = null;
            mmMgr = null;

            EventBus.ClearAllListeners();
            CooldownManager.ResetAllCooldowns();

            Prototype01SceneBuilder.BuildPrototype02Data();
            Prototype01SceneBuilder.BuildPrototype01Scene();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);

            hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            mmMgr = UnityEngine.Object.FindAnyObjectByType<MindMethodManager>();

            if (hero == null || bm == null || mmMgr == null)
            {
                Debug.LogError("[AUDIT SETUP] Essential scene objects missing!");
                return false;
            }

            if (bm.CurrentMonster == null)
            {
                bm.SpawnMonster();
            }
            monster = bm.CurrentMonster;

            // Setup Mind Method and Skills
            mmMgr.LoadDatabaseIfMissing();
            mmMgr.ResetPersistence();
            mmMgr.InitializeFromDatabase();
            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a"); // Skill
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");

            hero.transform.position = new Vector3(-2f, -1.2f, 0f);
            monster.transform.position = new Vector3(2f, -1.2f, 0f);
            hero.SetCurrentTarget(monster);
            monster.SetCurrentTarget(hero);

            if (!bm.IsBattleActive)
            {
                bm.StartBattle();
            }

            hero.Health.Revive(hero.Health.MaxHealth);
            hero.Rage.AddRage(100f);

            return true;
        }

        /// <summary>
        /// TEST A: In active battle with slotted skill off cooldown, does Hero ever autonomously cast the skill?
        /// </summary>
        private static bool Audit_TestA_AutonomousSkillCasting()
        {
            if (!SetupScene(out Hero hero, out Monster monster, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            int skillRequestsObserved = 0;
            Action<SkillExecutionRequest> onRequest = (req) =>
            {
                if (req.Source == hero)
                {
                    skillRequestsObserved++;
                    Debug.Log($"[SKILL-AUDIT] Skill Requested: {req.Skill?.SkillName} (Slot: {req.Slot})");
                }
            };

            EventBus.OnSkillExecutionRequested += onRequest;

            // Simulate 5 seconds of real combat (equivalent to ~3-4 basic attack cycles)
            AttackComponent heroAttack = hero.GetComponent<AttackComponent>();
            float dt = 0.1f;
            float totalSimTime = 5.0f;
            int ticks = (int)(totalSimTime / dt);

            for (int i = 0; i < ticks; i++)
            {
                heroAttack.ManualTick(dt);
                hero.TickActiveCast(dt);
            }

            EventBus.OnSkillExecutionRequested -= onRequest;

            Debug.Log($"[AUDIT TEST A — AUTONOMOUS SKILL] SimTime: {totalSimTime:F1}s | Hero Rage: {hero.Rage?.CurrentRage:F0} | Slotted Skill: {mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill)?.SkillName} | Skill Requests: {skillRequestsObserved}");

            // If skillRequestsObserved == 0, Hero did NOT auto-cast skill!
            return skillRequestsObserved > 0;
        }

        /// <summary>
        /// TEST B: When multiple skills are ready, is there an authority evaluating priority?
        /// </summary>
        private static bool Audit_TestB_SkillPriorityEvaluation()
        {
            // Inspection of codebase: SkillDefinitionSO has NO priority field.
            // MindMethodManager has NO priority comparator.
            // No component evaluates priority between Slot 2, Slot 3, Slot 4.
            Debug.Log("[AUDIT TEST B — SKILL PRIORITY] Result: NO PRIORITY AUTHORITY FOUND in SkillDefinitionSO or MindMethodManager.");
            return false;
        }

        /// <summary>
        /// TEST C: When Hero has 100 Rage and Ultimate is slotted, does Hero autonomously cast Ultimate?
        /// </summary>
        private static bool Audit_TestC_AutonomousUltimateExecution()
        {
            if (!SetupScene(out Hero hero, out Monster monster, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            mmMgr.UnlockSkill("skill_taiji_3_a"); // Ultimate
            mmMgr.SelectSkillForSlot(SkillSlotType.Ultimate, "skill_taiji_3_a");
            hero.Rage.AddRage(100f);

            int ultimateRequestsObserved = 0;
            Action<SkillExecutionRequest> onRequest = (req) =>
            {
                if (req.Source == hero && req.Slot == SkillSlotType.Ultimate)
                {
                    ultimateRequestsObserved++;
                    Debug.Log($"[SKILL-AUDIT] Ultimate Requested: {req.Skill?.SkillName}");
                }
            };

            EventBus.OnSkillExecutionRequested += onRequest;

            AttackComponent heroAttack = hero.GetComponent<AttackComponent>();
            float dt = 0.1f;
            for (int i = 0; i < 30; i++) // 3.0s
            {
                heroAttack.ManualTick(dt);
                hero.TickActiveCast(dt);
            }

            EventBus.OnSkillExecutionRequested -= onRequest;

            Debug.Log($"[AUDIT TEST C — AUTONOMOUS ULTIMATE] Hero Rage: {hero.Rage?.CurrentRage:F0} | Slotted Ultimate: {mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate)?.SkillName} | Ultimate Requests: {ultimateRequestsObserved}");

            return ultimateRequestsObserved > 0;
        }

        /// <summary>
        /// TEST D: When Auto is OFF, does Hero stop autonomous casting?
        /// </summary>
        private static bool Audit_TestD_AutoOffBehavior()
        {
            // Since no autonomous casting exists at all, Auto ON/OFF has no gameplay authority
            Debug.Log("[AUDIT TEST D — AUTO OFF BEHAVIOR] Result: No Auto Battle gameplay authority exists.");
            return false;
        }

        /// <summary>
        /// TEST E: When manual input is provided via Hero.ExecuteSelectedSkill, does skill execute successfully?
        /// </summary>
        private static bool Audit_TestE_ManualSkillInput()
        {
            if (!SetupScene(out Hero hero, out Monster monster, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            var result = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);
            bool pass = result != null && result.Success;
            Debug.Log($"[AUDIT TEST E — MANUAL SKILL INPUT] Success: {pass}, Reason: {result?.FailureReason}, Message: '{result?.ReasonDescription}'");
            return pass;
        }

        /// <summary>
        /// TEST F: Does a skill with Cast Time > 0 properly transition Hero to CastState?
        /// </summary>
        private static bool Audit_TestF_CastTimeStateTransition()
        {
            if (!SetupScene(out Hero hero, out Monster monster, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skill = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skill.SetChannel(false, 0f, 0f);
            skill.SetCastTime(1.0f);

            var result = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);
            bool isCasting = hero.IsCasting;
            bool castPhase = hero.CastState.CurrentPhase == SkillCastPhase.Casting;

            skill.SetCastTime(0f);
            bool pass = result != null && result.Success && isCasting && castPhase;
            Debug.Log($"[AUDIT TEST F — CAST TIME TRANSITION] Pass: {pass} (IsCasting: {isCasting}, Phase: {hero.CastState?.CurrentPhase})");
            return pass;
        }

        /// <summary>
        /// TEST G: Does a Channel Skill channel properly across ticks?
        /// </summary>
        private static bool Audit_TestG_ChannelSkillExecution()
        {
            if (!SetupScene(out Hero hero, out Monster monster, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skill = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skill.SetCastTime(0f);
            skill.SetChannel(true, 2.0f, 0.5f);

            var result = hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);
            bool isChanneling = hero.IsChanneling;

            // Tick 0.6s
            hero.TickActiveCast(0.6f);
            int ticksExecuted = hero.CastState.ChannelTicksExecuted;

            skill.SetChannel(false, 0f, 0f);
            bool pass = result != null && result.Success && isChanneling && ticksExecuted >= 1;
            Debug.Log($"[AUDIT TEST G — CHANNEL SKILL] Pass: {pass} (IsChanneling: {isChanneling}, TicksExecuted: {ticksExecuted})");
            return pass;
        }

        /// <summary>
        /// TEST H: Does Stun CC interrupt active casting?
        /// </summary>
        private static bool Audit_TestH_StunDuringCastInterrupt()
        {
            if (!SetupScene(out Hero hero, out Monster monster, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skill = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skill.SetCastTime(1.0f);
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);

            bool castingBefore = hero.IsCasting;
            hero.StatusController.ApplyCrowdControl("audit_stun", CrowdControlType.Stun, 2.0f);

            bool castingAfter = hero.IsCasting;
            skill.SetCastTime(0f);

            bool pass = castingBefore && !castingAfter;
            Debug.Log($"[AUDIT TEST H — STUN INTERRUPT] Pass: {pass} (Before: {castingBefore}, After: {castingAfter})");
            return pass;
        }

        /// <summary>
        /// TEST I: Does Root CC allow active casting to continue uninterrupted?
        /// </summary>
        private static bool Audit_TestI_RootDuringCastContinues()
        {
            if (!SetupScene(out Hero hero, out Monster monster, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skill = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skill.SetCastTime(1.0f);
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);

            bool castingBefore = hero.IsCasting;
            hero.StatusController.ApplyCrowdControl("audit_root", CrowdControlType.Root, 2.0f);

            bool castingAfter = hero.IsCasting;
            skill.SetCastTime(0f);

            bool pass = castingBefore && castingAfter;
            Debug.Log($"[AUDIT TEST I — ROOT CONTINUES] Pass: {pass} (Before: {castingBefore}, After: {castingAfter})");
            return pass;
        }

        /// <summary>
        /// TEST J: Does Ordinary Incoming Damage allow active casting to continue uninterrupted?
        /// </summary>
        private static bool Audit_TestJ_OrdinaryDamageDuringCastContinues()
        {
            if (!SetupScene(out Hero hero, out Monster monster, out BattleManager bm, out MindMethodManager mmMgr))
                return false;

            SkillDefinitionSO skill = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skill.SetCastTime(1.0f);
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, monster);

            bool castingBefore = hero.IsCasting;
            DamageResult dmg = new DamageResult { FinalDamage = 20f, IsDodged = false };
            hero.Health.TakeDamage(dmg);

            bool castingAfter = hero.IsCasting;
            skill.SetCastTime(0f);

            bool pass = castingBefore && castingAfter;
            Debug.Log($"[AUDIT TEST J — DAMAGE CONTINUES] Pass: {pass} (Before: {castingBefore}, After: {castingAfter})");
            return pass;
        }
    }
}
#endif
