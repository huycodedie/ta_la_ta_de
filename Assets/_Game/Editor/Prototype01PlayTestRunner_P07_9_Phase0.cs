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
        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9 Phase 0 Tests (TEST A to TEST F)")]
        public static bool RunAllPrototype07_9_Phase0_Tests()
        {
            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 PHASE 0 TESTS (TEST A -> TEST F)");
            Debug.Log("   Scope: Regression Guard + Ultimate CC Validation");
            Debug.Log("==================================================");

            bool tA = P07_9_01_UltimateRejectedWhenCanUseUltimateFalse();
            bool tB = P07_9_02_UltimateAllowedWhenCanUseUltimateTrue();
            bool tC = P07_9_03_RegularSkillContinuesToUseCanUseSkill();
            bool tD = P07_9_04_RootDoesNotRejectUltimate();
            bool tE = P07_9_05_StunAndFreezeBlockUltimateWithoutBypass();
            bool tF = P07_9_06_AntiCCProtectsUltimateFromInterruption();

            bool allPassed = tA && tB && tC && tD && tE && tF;

            Debug.Log("==================================================");
            Debug.Log("   PROTOTYPE 07.9 PHASE 0 TEST SUMMARY");
            Debug.Log("==================================================");
            Debug.Log($"   TEST A (Ultimate rejected when CanUseUltimate == false): {(tA ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST B (Ultimate allowed when CanUseUltimate == true):   {(tB ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST C (Regular skill uses CanUseSkill):                {(tC ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST D (Root does not reject Ultimate):                 {(tD ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST E (Stun and Freeze block Ultimate without bypass): {(tE ? "PASS" : "FAIL")}");
            Debug.Log($"   TEST F (Anti-CC protects Ultimate from interruption):   {(tF ? "PASS" : "FAIL")}");
            Debug.Log($"   OVERALL PHASE 0 STATUS: {(allPassed ? "100% PASS" : "FAIL")}");
            Debug.Log("==================================================");

            return allPassed;
        }

        private class MockPermissionEntity : Entity
        {
            public bool MockCanUseSkill = true;
            public bool MockCanUseUltimate = true;
            public override bool CanUseSkill => MockCanUseSkill;
            public override bool CanUseUltimate => MockCanUseUltimate;
        }

        /// <summary>
        /// TEST A: Ultimate is rejected when CanUseUltimate() == false.
        /// Verifies both full Hero execution under Stun and direct validation check where CanUseUltimate is false while CanUseSkill is true.
        /// </summary>
        private static bool P07_9_01_UltimateRejectedWhenCanUseUltimateFalse()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            // 1. Live Hero check: Stun sets CanUseUltimate = false
            hero.StatusController.ApplyCrowdControl("cc_stun_test", CrowdControlType.Stun, 3f, EffectPowerTier.TierB, bm.CurrentMonster);
            var resHero = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool heroBlocked = !resHero.Success && resHero.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled;

            // 2. Direct discrimination check: entity where CanUseSkill == true but CanUseUltimate == false
            GameObject mockGO = new GameObject("MockEntity");
            mockGO.AddComponent<EntityStatsComponent>();
            var health = mockGO.AddComponent<HealthComponent>();
            health.InitializeHealth(100f);
            var mock = mockGO.AddComponent<MockPermissionEntity>();
            mock.MockCanUseSkill = true;
            mock.MockCanUseUltimate = false;

            SkillDefinitionSO ultDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Ultimate);
            var reqUlt = new SkillExecutionRequest(mock, ultDef, SkillSlotType.Ultimate, bm.CurrentMonster);
            bool ultValidation = SkillExecutionValidator.Validate(reqUlt, out var reasonUlt, out var msgUlt);
            bool discriminationOk = !ultValidation && reasonUlt == SkillExecutionFailureReason.SourceCrowdControlled;

            bool ok = heroBlocked && discriminationOk;
            Debug.Log($"[P07_9_01] TEST A -> HeroBlocked: {heroBlocked}, DiscriminationOk: {discriminationOk} | {(ok ? "PASS" : "FAIL")}");

            Object.DestroyImmediate(mockGO);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST B: Ultimate is permitted when CanUseUltimate() == true and other conditions are valid.
        /// </summary>
        private static bool P07_9_02_UltimateAllowedWhenCanUseUltimateTrue()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            bool canUltBefore = hero.CanUseUltimate;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool ok = canUltBefore && res.Success && res.FailureReason == SkillExecutionFailureReason.None;

            Debug.Log($"[P07_9_02] TEST B -> CanUltBefore: {canUltBefore}, Success: {res.Success}, FailureReason: {res.FailureReason} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST C: Regular skill continues to validate source.CanUseSkill as before.
        /// </summary>
        private static bool P07_9_03_RegularSkillContinuesToUseCanUseSkill()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(50f);

            // Stun blocks CanUseSkill
            hero.StatusController.ApplyCrowdControl("cc_stun_test", CrowdControlType.Stun, 3f, EffectPowerTier.TierB, bm.CurrentMonster);
            var resSkill = hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            bool skillBlocked = !resSkill.Success && resSkill.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled;

            // Direct discrimination check: entity where CanUseSkill == false but CanUseUltimate == true
            GameObject mockGO = new GameObject("MockEntity");
            mockGO.AddComponent<EntityStatsComponent>();
            var health = mockGO.AddComponent<HealthComponent>();
            health.InitializeHealth(100f);
            var mock = mockGO.AddComponent<MockPermissionEntity>();
            mock.MockCanUseSkill = false;
            mock.MockCanUseUltimate = true;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            var reqSkill = new SkillExecutionRequest(mock, skillDef, SkillSlotType.Skill, bm.CurrentMonster);
            bool skillValidation = SkillExecutionValidator.Validate(reqSkill, out var reasonSkill, out _);
            bool regularUsesCanSkill = !skillValidation && reasonSkill == SkillExecutionFailureReason.SourceCrowdControlled;

            bool ok = skillBlocked && regularUsesCanSkill;
            Debug.Log($"[P07_9_03] TEST C -> SkillBlocked: {skillBlocked}, RegularUsesCanSkill: {regularUsesCanSkill} | {(ok ? "PASS" : "FAIL")}");

            Object.DestroyImmediate(mockGO);
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST D: Root does not reject Ultimate if CanUseUltimate() is true.
        /// Under Root, CanMove/CanDash are false, but CanBasicAttack, CanUseSkill, and CanUseUltimate remain true.
        /// </summary>
        private static bool P07_9_04_RootDoesNotRejectUltimate()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            hero.StatusController.ApplyCrowdControl("cc_root_test", CrowdControlType.Root, 3f, EffectPowerTier.TierB, bm.CurrentMonster);

            bool rootPerms = !hero.CanMove && hero.CanUseSkill && hero.CanUseUltimate;
            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool ok = rootPerms && res.Success && res.FailureReason == SkillExecutionFailureReason.None;

            Debug.Log($"[P07_9_04] TEST D -> RootPerms: {rootPerms}, Success: {res.Success}, FailureReason: {res.FailureReason} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST E: Stun and Freeze both block Ultimate without bypass.
        /// </summary>
        private static bool P07_9_05_StunAndFreezeBlockUltimateWithoutBypass()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            // 1. Stun check
            hero.StatusController.ApplyCrowdControl("cc_stun_test", CrowdControlType.Stun, 2f, EffectPowerTier.TierB, bm.CurrentMonster);
            var resStun = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool stunBlocked = !resStun.Success && resStun.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled;

            // Clear CC
            hero.StatusController.ClearAllCrowdControl();
            hero.Rage.AddRage(100f);
            CooldownManager.ResetAllCooldowns();

            // 2. Freeze check
            hero.StatusController.ApplyCrowdControl("cc_freeze_test", CrowdControlType.Freeze, 2f, EffectPowerTier.TierB, bm.CurrentMonster);
            var resFreeze = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool freezeBlocked = !resFreeze.Success && resFreeze.FailureReason == SkillExecutionFailureReason.SourceCrowdControlled;

            bool ok = stunBlocked && freezeBlocked;
            Debug.Log($"[P07_9_05] TEST E -> StunBlocked: {stunBlocked}, FreezeBlocked: {freezeBlocked} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }

        /// <summary>
        /// TEST F: Anti-CC immunity prevents CC from applying, allowing Ultimate to execute.
        /// </summary>
        private static bool P07_9_06_AntiCCProtectsUltimateFromInterruption()
        {
            SetupSkillTestEncounter(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr);
            mmMgr.SetActiveMindMethod("mm_taiji");
            hero.Rage.AddRage(100f);

            // Apply Anti-CC
            hero.StatusController.ApplyAntiCCImmunity(5f);

            // Attempt to apply Stun -> blocked by Anti-CC
            bool ccApplied = hero.StatusController.ApplyCrowdControl("cc_stun_test", CrowdControlType.Stun, 3f, EffectPowerTier.TierB, bm.CurrentMonster);
            bool notStunned = !hero.IsStunned && hero.CanUseUltimate;

            var res = hero.ExecuteSelectedSkill(SkillSlotType.Ultimate, bm.CurrentMonster);
            bool ok = !ccApplied && notStunned && res.Success && res.FailureReason == SkillExecutionFailureReason.None;

            Debug.Log($"[P07_9_06] TEST F -> CcApplied: {ccApplied}, NotStunned: {notStunned}, UltSuccess: {res.Success} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            return ok;
        }
    }
}
#endif
