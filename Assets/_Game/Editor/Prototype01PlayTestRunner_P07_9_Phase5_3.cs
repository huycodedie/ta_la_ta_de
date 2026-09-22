#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Progression;
using WuxiaGame.UI;

namespace WuxiaGame.Editor
{
    public static partial class Prototype01PlayTestRunner
    {
        private class TestPhase5_3Hero : Hero
        {
            public float SimulatedDeltaTime = 0.05f;
            public BattleHUD BoundHUD;

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
                if (BoundHUD != null && BoundHUD.HeroCastBarUI != null)
                {
                    BoundHUD.HeroCastBarUI.ManualUpdate(SimulatedDeltaTime);
                }
            }
        }

        private static bool ApplyCC_P5_3(Entity entity, CrowdControlType ccType, float duration, string ccId = null)
        {
            if (entity == null || entity.StatusController == null) return false;
            if (string.IsNullOrEmpty(ccId))
            {
                ccId = "cc_p5_3_" + ccType.ToString().ToLower() + "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            }
            return entity.StatusController.ApplyCrowdControl(ccId, ccType, duration);
        }

        private static void SetupSkillTestEncounterWithCastBar(
            out GameObject heroGO,
            out Hero hero,
            out GameObject bmGO,
            out BattleManager bm,
            out GameObject mmMgrGO,
            out MindMethodManager mmMgr,
            out GameObject canvasGO,
            out CastBarUI castBarUI)
        {
            SetupSkillTestEncounter(out heroGO, out hero, out bmGO, out bm, out mmMgrGO, out mmMgr);
            CooldownManager.ResetAllCooldowns();
            mmMgr.SetActiveMindMethod("mm_taiji");
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
            hero.SetCurrentTarget(bm.CurrentMonster);
            hero.Rage.AddRage(100f);

            // Create Canvas and CastBarUI
            canvasGO = new GameObject("TestCanvas_P5_3");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject castBarRoot = new GameObject("HeroCastBar");
            castBarRoot.transform.SetParent(canvasGO.transform, false);

            GameObject fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(castBarRoot.transform, false);
            Image fillImg = fillGO.AddComponent<Image>();
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0f;

            GameObject castTextGO = new GameObject("CastText");
            castTextGO.transform.SetParent(castBarRoot.transform, false);
            TextMeshProUGUI cText = castTextGO.AddComponent<TextMeshProUGUI>();

            GameObject interruptTextGO = new GameObject("InterruptText");
            interruptTextGO.transform.SetParent(castBarRoot.transform, false);
            TextMeshProUGUI iText = interruptTextGO.AddComponent<TextMeshProUGUI>();
            interruptTextGO.SetActive(false);

            // Attach CastBarUI to canvasGO so it remains active and receives EventBus events
            castBarUI = canvasGO.AddComponent<CastBarUI>();
            castBarUI.SetReferences(castBarRoot, fillImg, cText, iText, hero, EntityType.Hero);
            castBarUI.RegisterEvents();
            castBarRoot.SetActive(false);
        }

        private static void TeardownSkillTestEncounterWithCastBar(
            GameObject heroGO,
            GameObject bmGO,
            GameObject mmMgrGO,
            GameObject canvasGO)
        {
            if (canvasGO != null)
            {
                CastBarUI ui = canvasGO.GetComponent<CastBarUI>();
                if (ui != null) ui.UnregisterEvents();
                UnityEngine.Object.DestroyImmediate(canvasGO);
            }
            TeardownSkillTestEncounter(heroGO, bmGO, mmMgrGO);
            CooldownManager.ResetAllCooldowns();
        }

        [MenuItem("Tools/Wuxia RPG/Run Prototype 07.9 Phase 5.3 Tests (TEST 01 to TEST 28 + Visual Scenarios A-H)")]
        public static bool RunAllPrototype07_9_Phase5_3_Tests()
        {
            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 PHASE 5.3 TESTS (PRESENTATION / VISUAL)");
            Debug.Log("==================================================");

            int passed = 0;
            int total = 36; // 28 Automated Tests + 8 Visual Acceptance Scenarios

            bool t01 = P07_9_P5_3_01_InstantSkillNoCastBar();
            if (t01) passed++;

            bool t02 = P07_9_P5_3_02_CastSkillBarAppears();
            if (t02) passed++;

            bool t03 = P07_9_P5_3_03_CastProgressIncreases();
            if (t03) passed++;

            bool t04 = P07_9_P5_3_04_CastCompletesBarDisappears();
            if (t04) passed++;

            bool t05 = P07_9_P5_3_05_CastInterruptStunBarStops();
            if (t05) passed++;

            bool t06 = P07_9_P5_3_06_CastInterruptFreezeBarStops();
            if (t06) passed++;

            bool t07 = P07_9_P5_3_07_RootDuringCastBarContinues();
            if (t07) passed++;

            bool t08 = P07_9_P5_3_08_DamageDuringCastBarContinues();
            if (t08) passed++;

            bool t09 = P07_9_P5_3_09_AntiCCStunCastContinues();
            if (t09) passed++;

            bool t10 = P07_9_P5_3_10_ChannelBarAppears();
            if (t10) passed++;

            bool t11 = P07_9_P5_3_11_ChannelProgressIncreases();
            if (t11) passed++;

            bool t12 = P07_9_P5_3_12_ChannelCompletesBarDisappears();
            if (t12) passed++;

            bool t13 = P07_9_P5_3_13_ChannelStunInterruptBarStops();
            if (t13) passed++;

            bool t14 = P07_9_P5_3_14_ChannelFreezeInterruptBarStops();
            if (t14) passed++;

            bool t15 = P07_9_P5_3_15_ChannelRootContinues();
            if (t15) passed++;

            bool t16 = P07_9_P5_3_16_ChannelOrdinaryDamageContinues();
            if (t16) passed++;

            bool t17 = P07_9_P5_3_17_ChannelTickContinuesBeforeInterrupt();
            if (t17) passed++;

            bool t18 = P07_9_P5_3_18_NoChannelTickAfterInterrupt();
            if (t18) passed++;

            bool t19 = P07_9_P5_3_19_InterruptFeedbackSourceStun();
            if (t19) passed++;

            bool t20 = P07_9_P5_3_20_InterruptFeedbackSourceFreeze();
            if (t20) passed++;

            bool t21 = P07_9_P5_3_21_InterruptFeedbackNotShownForRoot();
            if (t21) passed++;

            bool t22 = P07_9_P5_3_22_InterruptFeedbackNotShownForOrdinaryDamage();
            if (t22) passed++;

            bool t23 = P07_9_P5_3_23_RepeatedInterruptDoesNotDuplicateFeedback();
            if (t23) passed++;

            bool t24 = P07_9_P5_3_24_CompletedCastDoesNotShowInterruptFeedback();
            if (t24) passed++;

            bool t25 = P07_9_P5_3_25_DeathDuringCastNoGhostUI();
            if (t25) passed++;

            bool t26 = P07_9_P5_3_26_DeathDuringChannelNoGhostUI();
            if (t26) passed++;

            bool t27 = P07_9_P5_3_27_P07_8DamagePipelineRemainsIntact();
            if (t27) passed++;

            bool t28 = P07_9_P5_3_28_P07_8ShieldPipelineRemainsIntact();
            if (t28) passed++;

            Debug.Log("==================================================");
            Debug.Log("   RUNNING PROTOTYPE 07.9 PHASE 5.3 VISUAL ACCEPTANCE SCENARIOS A-H");
            Debug.Log("==================================================");

            bool va = P07_9_P5_3_VisualA_CastBar_PlayMode();
            if (va) passed++;

            bool vb = P07_9_P5_3_VisualB_ChannelBar_PlayMode();
            if (vb) passed++;

            bool vc = P07_9_P5_3_VisualC_StunInterrupt_PlayMode();
            if (vc) passed++;

            bool vd = P07_9_P5_3_VisualD_FreezeInterrupt_PlayMode();
            if (vd) passed++;

            bool ve = P07_9_P5_3_VisualE_Root_PlayMode();
            if (ve) passed++;

            bool vf = P07_9_P5_3_VisualF_OrdinaryDamage_PlayMode();
            if (vf) passed++;

            bool vg = P07_9_P5_3_VisualG_ChannelInterrupt_PlayMode();
            if (vg) passed++;

            bool vh = P07_9_P5_3_VisualH_Instant_PlayMode();
            if (vh) passed++;

            Debug.Log($"[PROTOTYPE 07.9 PHASE 5.3 SUMMARY] Passed: {passed}/{total} ({(passed == total ? "ALL PASS" : "FAILURES DETECTED")})");
            return passed == total;
        }

        // =========================================================================
        // AUTOMATED TESTS 01 - 28
        // =========================================================================

        private static bool P07_9_P5_3_01_InstantSkillNoCastBar()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(false, 0f, 0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            castBarUI.ManualUpdate();

            bool ok = !castBarUI.IsVisible && castBarUI.CurrentFill == 0f && !hero.IsCasting;
            Debug.Log($"[P07_9_P5_3_01] TEST 01 -> IsVisible: {castBarUI.IsVisible}, Fill: {castBarUI.CurrentFill}, IsCasting: {hero.IsCasting} | {(ok ? "PASS" : "FAIL")}");
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_02_CastSkillBarAppears()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            castBarUI.ManualUpdate();

            bool ok = castBarUI.IsVisible && hero.IsCasting && castBarUI.CurrentText.Contains("CAST");
            Debug.Log($"[P07_9_P5_3_02] TEST 02 -> IsVisible: {castBarUI.IsVisible}, IsCasting: {hero.IsCasting}, Text: '{castBarUI.CurrentText}' | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_03_CastProgressIncreases()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);
            castBarUI.ManualUpdate();
            float fill1 = castBarUI.CurrentFill;

            hero.TickActiveCast(0.4f);
            castBarUI.ManualUpdate();
            float fill2 = castBarUI.CurrentFill;

            bool ok = fill2 > fill1 && Mathf.Approximately(fill2, 0.7f);
            Debug.Log($"[P07_9_P5_3_03] TEST 03 -> Fill1: {fill1:F2}, Fill2: {fill2:F2} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_04_CastCompletesBarDisappears()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(1.0f);
            castBarUI.ManualUpdate();

            bool ok = !castBarUI.IsVisible && !hero.IsCasting && castBarUI.CurrentFill == 0f;
            Debug.Log($"[P07_9_P5_3_04] TEST 04 -> IsVisible: {castBarUI.IsVisible}, IsCasting: {hero.IsCasting}, Fill: {castBarUI.CurrentFill} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_05_CastInterruptStunBarStops()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);
            castBarUI.ManualUpdate();

            ApplyCC_P5_3(hero, CrowdControlType.Stun, 2.0f, "cc_stun_t05");
            castBarUI.ManualUpdate();

            bool ok = !hero.IsCasting && hero.CastState.IsInterrupted && (!castBarUI.FillImage.gameObject.activeSelf || castBarUI.CurrentFill == 0f);
            Debug.Log($"[P07_9_P5_3_05] TEST 05 -> Interrupted: {hero.CastState.IsInterrupted}, FillStopped: {!castBarUI.FillImage.gameObject.activeSelf || castBarUI.CurrentFill == 0f} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_06_CastInterruptFreezeBarStops()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC_P5_3(hero, CrowdControlType.Freeze, 2.0f, "cc_freeze_t06");
            castBarUI.ManualUpdate();

            bool ok = !hero.IsCasting && hero.CastState.IsInterrupted && (!castBarUI.FillImage.gameObject.activeSelf || castBarUI.CurrentFill == 0f);
            Debug.Log($"[P07_9_P5_3_06] TEST 06 -> Interrupted: {hero.CastState.IsInterrupted}, FillStopped: {!castBarUI.FillImage.gameObject.activeSelf || castBarUI.CurrentFill == 0f} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_07_RootDuringCastBarContinues()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);
            ApplyCC_P5_3(hero, CrowdControlType.Root, 2.0f, "cc_root_t07");
            castBarUI.ManualUpdate();

            bool ok1 = castBarUI.IsVisible && hero.IsCasting && hero.IsRooted;

            hero.TickActiveCast(0.7f);
            castBarUI.ManualUpdate();
            bool ok2 = !hero.IsCasting && !castBarUI.IsVisible;

            bool ok = ok1 && ok2;
            Debug.Log($"[P07_9_P5_3_07] TEST 07 -> ContinuedUnderRoot: {ok1}, CompletedNormally: {ok2} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_08_DamageDuringCastBarContinues()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);
            hero.Health.TakeDamage(10f); // non-lethal damage
            castBarUI.ManualUpdate();

            bool ok1 = castBarUI.IsVisible && hero.IsCasting;

            hero.TickActiveCast(0.7f);
            castBarUI.ManualUpdate();
            bool ok2 = !hero.IsCasting && !castBarUI.IsVisible;

            bool ok = ok1 && ok2;
            Debug.Log($"[P07_9_P5_3_08] TEST 08 -> ContinuedUnderDmg: {ok1}, CompletedNormally: {ok2} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_09_AntiCCStunCastContinues()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.StatusController.ApplyAntiCCImmunity(5.0f);
            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            bool ccApplied = ApplyCC_P5_3(hero, CrowdControlType.Stun, 2.0f);
            castBarUI.ManualUpdate();

            bool stillCasting = !ccApplied && hero.IsCasting && castBarUI.IsVisible && !castBarUI.IsShowingInterruptFeedback;

            hero.TickActiveCast(0.7f);
            castBarUI.ManualUpdate();
            bool completed = !hero.IsCasting && !castBarUI.IsVisible;

            bool ok = stillCasting && completed;
            Debug.Log($"[P07_9_P5_3_09] TEST 09 -> CcBlocked: {!ccApplied}, StillCasting: {stillCasting}, Completed: {completed} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_10_ChannelBarAppears()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            castBarUI.ManualUpdate();

            bool ok = castBarUI.IsVisible && hero.IsCasting && hero.CastState.CurrentPhase == SkillCastPhase.Channeling && castBarUI.CurrentText.Contains("CHANNEL");
            Debug.Log($"[P07_9_P5_3_10] TEST 10 -> ChannelBarVisible: {castBarUI.IsVisible}, Text: '{castBarUI.CurrentText}' | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_11_ChannelProgressIncreases()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f);
            castBarUI.ManualUpdate();
            float fill1 = castBarUI.CurrentFill;

            hero.TickActiveCast(0.5f);
            castBarUI.ManualUpdate();
            float fill2 = castBarUI.CurrentFill;

            bool ok = fill2 > fill1 && Mathf.Approximately(fill2, 0.5f);
            Debug.Log($"[P07_9_P5_3_11] TEST 11 -> Fill1: {fill1:F2}, Fill2: {fill2:F2} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_12_ChannelCompletesBarDisappears()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(2.0f);
            castBarUI.ManualUpdate();

            bool ok = !castBarUI.IsVisible && !hero.IsCasting && castBarUI.CurrentFill == 0f;
            Debug.Log($"[P07_9_P5_3_12] TEST 12 -> IsVisible: {castBarUI.IsVisible}, IsCasting: {hero.IsCasting}, Fill: {castBarUI.CurrentFill} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_13_ChannelStunInterruptBarStops()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.6f);
            castBarUI.ManualUpdate();

            ApplyCC_P5_3(hero, CrowdControlType.Stun, 2.0f, "cc_stun_t13");
            castBarUI.ManualUpdate();

            bool ok = !hero.IsCasting && hero.CastState.IsInterrupted && (!castBarUI.FillImage.gameObject.activeSelf || castBarUI.CurrentFill == 0f);
            Debug.Log($"[P07_9_P5_3_13] TEST 13 -> Interrupted: {hero.CastState.IsInterrupted}, FillStopped: {!castBarUI.FillImage.gameObject.activeSelf || castBarUI.CurrentFill == 0f} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_14_ChannelFreezeInterruptBarStops()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.6f);

            ApplyCC_P5_3(hero, CrowdControlType.Freeze, 2.0f, "cc_freeze_t14");
            castBarUI.ManualUpdate();

            bool ok = !hero.IsCasting && hero.CastState.IsInterrupted && (!castBarUI.FillImage.gameObject.activeSelf || castBarUI.CurrentFill == 0f);
            Debug.Log($"[P07_9_P5_3_14] TEST 14 -> Interrupted: {hero.CastState.IsInterrupted}, FillStopped: {!castBarUI.FillImage.gameObject.activeSelf || castBarUI.CurrentFill == 0f} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_15_ChannelRootContinues()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.4f);
            ApplyCC_P5_3(hero, CrowdControlType.Root, 2.0f, "cc_root_t15");
            castBarUI.ManualUpdate();

            bool ok1 = castBarUI.IsVisible && hero.IsCasting && hero.IsRooted;

            hero.TickActiveCast(1.6f);
            castBarUI.ManualUpdate();
            bool ok2 = !hero.IsCasting && !castBarUI.IsVisible;

            bool ok = ok1 && ok2;
            Debug.Log($"[P07_9_P5_3_15] TEST 15 -> ChannelUnderRoot: {ok1}, CompletedNormally: {ok2} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_16_ChannelOrdinaryDamageContinues()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.4f);
            hero.Health.TakeDamage(10f); // non-lethal damage
            castBarUI.ManualUpdate();

            bool ok1 = castBarUI.IsVisible && hero.IsCasting;

            hero.TickActiveCast(1.6f);
            castBarUI.ManualUpdate();
            bool ok2 = !hero.IsCasting && !castBarUI.IsVisible;

            bool ok = ok1 && ok2;
            Debug.Log($"[P07_9_P5_3_16] TEST 16 -> ChannelUnderDmg: {ok1}, CompletedNormally: {ok2} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_17_ChannelTickContinuesBeforeInterrupt()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.6f);

            bool ok = hero.CastState.ChannelTicksExecuted == 1;
            Debug.Log($"[P07_9_P5_3_17] TEST 17 -> TicksExecutedBeforeInterrupt: {hero.CastState.ChannelTicksExecuted} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_18_NoChannelTickAfterInterrupt()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.6f);

            ApplyCC_P5_3(hero, CrowdControlType.Stun, 2.0f, "cc_stun_t18");
            hero.TickActiveCast(1.0f); // attempt to progress while interrupted

            bool ok = hero.CastState.ChannelTicksExecuted == 1 && !hero.IsCasting && hero.CastState.IsInterrupted;
            Debug.Log($"[P07_9_P5_3_18] TEST 18 -> TicksRemained: {hero.CastState.ChannelTicksExecuted}, IsCasting: {hero.IsCasting} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_19_InterruptFeedbackSourceStun()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC_P5_3(hero, CrowdControlType.Stun, 2.0f, "cc_stun_t19");
            castBarUI.ManualUpdate();

            bool ok = castBarUI.IsShowingInterruptFeedback && castBarUI.CurrentInterruptText.Contains("STUN");
            Debug.Log($"[P07_9_P5_3_19] TEST 19 -> ShowingFeedback: {castBarUI.IsShowingInterruptFeedback}, Text: '{castBarUI.CurrentInterruptText}' | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_20_InterruptFeedbackSourceFreeze()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC_P5_3(hero, CrowdControlType.Freeze, 2.0f, "cc_freeze_t20");
            castBarUI.ManualUpdate();

            bool ok = castBarUI.IsShowingInterruptFeedback && castBarUI.CurrentInterruptText.Contains("FREEZE");
            Debug.Log($"[P07_9_P5_3_20] TEST 20 -> ShowingFeedback: {castBarUI.IsShowingInterruptFeedback}, Text: '{castBarUI.CurrentInterruptText}' | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_21_InterruptFeedbackNotShownForRoot()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC_P5_3(hero, CrowdControlType.Root, 2.0f, "cc_root_t21");
            castBarUI.ManualUpdate();

            bool ok = !castBarUI.IsShowingInterruptFeedback && string.IsNullOrEmpty(castBarUI.CurrentInterruptText);
            Debug.Log($"[P07_9_P5_3_21] TEST 21 -> ShowingFeedback: {castBarUI.IsShowingInterruptFeedback}, Text: '{castBarUI.CurrentInterruptText}' | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_22_InterruptFeedbackNotShownForOrdinaryDamage()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            hero.Health.TakeDamage(10f); // non-lethal damage
            castBarUI.ManualUpdate();

            bool ok = !castBarUI.IsShowingInterruptFeedback && string.IsNullOrEmpty(castBarUI.CurrentInterruptText);
            Debug.Log($"[P07_9_P5_3_22] TEST 22 -> ShowingFeedback: {castBarUI.IsShowingInterruptFeedback}, Text: '{castBarUI.CurrentInterruptText}' | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_23_RepeatedInterruptDoesNotDuplicateFeedback()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            ApplyCC_P5_3(hero, CrowdControlType.Stun, 2.0f, "cc_stun_t23");
            hero.InterruptCurrentAction(SkillCastInterruptSource.Stun); // repeated
            castBarUI.ManualUpdate();

            bool ok = castBarUI.CurrentInterruptText == "SKILL CAST INTERRUPTED (STUN)";
            Debug.Log($"[P07_9_P5_3_23] TEST 23 -> Text: '{castBarUI.CurrentInterruptText}' | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_24_CompletedCastDoesNotShowInterruptFeedback()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(1.0f);
            castBarUI.ManualUpdate();

            bool ok = !castBarUI.IsShowingInterruptFeedback && string.IsNullOrEmpty(castBarUI.CurrentInterruptText);
            Debug.Log($"[P07_9_P5_3_24] TEST 24 -> ShowingFeedback: {castBarUI.IsShowingInterruptFeedback}, Text: '{castBarUI.CurrentInterruptText}' | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_25_DeathDuringCastNoGhostUI()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(1.0f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.3f);

            hero.Health.TakeDamage(hero.Health.MaxHealth + 100f);
            if (hero.IsAlive)
            {
                hero.Health.TakeDamage(hero.Health.MaxHealth + 100f);
            }
            hero.TickActiveCast(0.1f);
            castBarUI.ManualUpdate();

            bool ok = !castBarUI.IsVisible && !castBarUI.IsShowingInterruptFeedback && !hero.IsAlive;
            Debug.Log($"[P07_9_P5_3_25] TEST 25 -> IsVisible: {castBarUI.IsVisible}, IsShowingFeedback: {castBarUI.IsShowingInterruptFeedback}, IsAlive: {hero.IsAlive} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetCastTime(0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_26_DeathDuringChannelNoGhostUI()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.4f);

            hero.Health.TakeDamage(hero.Health.MaxHealth + 100f);
            if (hero.IsAlive)
            {
                hero.Health.TakeDamage(hero.Health.MaxHealth + 100f);
            }
            hero.TickActiveCast(0.1f);
            castBarUI.ManualUpdate();

            bool ok = !castBarUI.IsVisible && !castBarUI.IsShowingInterruptFeedback && !hero.IsAlive;
            Debug.Log($"[P07_9_P5_3_26] TEST 26 -> IsVisible: {castBarUI.IsVisible}, IsShowingFeedback: {castBarUI.IsShowingInterruptFeedback}, IsAlive: {hero.IsAlive} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_27_P07_8DamagePipelineRemainsIntact()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
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
            Debug.Log($"[P07_9_P5_3_27] TEST 27 -> InitialHP: {initialMonsterHp}, DamagedHP: {damagedHp} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        private static bool P07_9_P5_3_28_P07_8ShieldPipelineRemainsIntact()
        {
            SetupSkillTestEncounterWithCastBar(out GameObject heroGO, out Hero hero, out GameObject bmGO, out BattleManager bm, out GameObject mmMgrGO, out MindMethodManager mmMgr, out GameObject canvasGO, out CastBarUI castBarUI);
            mmMgr.SetActiveMindMethod("mm_taiji");
            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);
            hero.Rage.AddRage(100f);

            // Apply 500 shield to monster
            bm.CurrentMonster.StatusController.ApplyShield("shield_p5_3_test", 500f, 30f);
            float initialMonsterHp = bm.CurrentMonster.Health.CurrentHealth;

            hero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            hero.TickActiveCast(0.5f); // 1 tick

            var shield = bm.CurrentMonster.StatusController.GetShield("shield_p5_3_test");
            float remShield = shield != null ? shield.CurrentAmount : -1f;

            // Shield should have absorbed the tick damage, leaving Monster HP intact
            bool ok = (remShield < 500f) && (remShield >= 0f) &&
                      Mathf.Approximately(bm.CurrentMonster.Health.CurrentHealth, initialMonsterHp);

            Debug.Log($"[P07_9_P5_3_28] TEST 28 -> ShieldAbsorbed: {remShield} (initial 500), HpUntouched: {Mathf.Approximately(bm.CurrentMonster.Health.CurrentHealth, initialMonsterHp)} | {(ok ? "PASS" : "FAIL")}");
            skillDef.SetChannel(false, 0f, 0f);
            TeardownSkillTestEncounterWithCastBar(heroGO, bmGO, mmMgrGO, canvasGO);
            return ok;
        }

        // =========================================================================
        // PLAY MODE VISUAL ACCEPTANCE SCENARIOS A - H
        // =========================================================================

        private static bool SetupPlayModeScene_P5_3(out TestPhase5_3Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD battleHUD)
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
                Debug.LogError("[PLAY MODE SETUP P5.3] Missing essential scene components!");
                return false;
            }

            GameObject heroGO = sceneHero.gameObject;
            UnityEngine.Object.DestroyImmediate(sceneHero);
            testHero = heroGO.AddComponent<TestPhase5_3Hero>();
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
            mmMgr.UnlockSkill("skill_taiji_2_a");
            mmMgr.SelectSkillForSlot(SkillSlotType.Skill, "skill_taiji_2_a");
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
        /// VISUAL A: CAST BAR (Play Mode)
        /// 1. Trigger Cast-time skill.
        /// 2. Cast Bar appears.
        /// 3. Progress increases.
        /// 4. Cast complete -> Bar disappears, effect occurs.
        /// </summary>
        private static bool P07_9_P5_3_VisualA_CastBar_PlayMode()
        {
            if (!SetupPlayModeScene_P5_3(out TestPhase5_3Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);

            // 1. Trigger cast
            testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            testHero.CallUpdate(0.1f);

            bool barAppeared = hud.IsHeroCastBarVisible && testHero.IsCasting;
            float fill1 = hud.CurrentHeroCastProgress;

            // 2. Progress
            testHero.CallUpdate(0.4f);
            float fill2 = hud.CurrentHeroCastProgress;
            bool progressIncreased = fill2 > fill1;

            // 3. Complete
            testHero.CallUpdate(0.6f);
            bool barDisappeared = !hud.IsHeroCastBarVisible && !testHero.IsCasting;

            bool pass = barAppeared && progressIncreased && barDisappeared;
            Debug.Log($"[VISUAL A — CAST BAR] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. BarAppeared: {barAppeared} (Fill: {fill1:F2})\n" +
                      $"2. ProgressIncreased: {progressIncreased} ({fill1:F2} -> {fill2:F2})\n" +
                      $"3. BarDisappearedOnComplete: {barDisappeared}");

            skillDef.SetCastTime(0f);
            return pass;
        }

        /// <summary>
        /// VISUAL B: CHANNEL BAR (Play Mode)
        /// 1. Trigger Channel skill.
        /// 2. Channel Bar appears.
        /// 3. Progress increases across channel ticks.
        /// 4. Channel complete -> Bar disappears.
        /// </summary>
        private static bool P07_9_P5_3_VisualB_ChannelBar_PlayMode()
        {
            if (!SetupPlayModeScene_P5_3(out TestPhase5_3Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);

            // 1. Trigger channel
            testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            testHero.CallUpdate(0.1f);

            bool barAppeared = hud.IsHeroCastBarVisible && testHero.IsCasting && hud.CurrentHeroCastText.Contains("CHANNEL");
            float fill1 = hud.CurrentHeroCastProgress;

            // 2. Progress across ticks
            testHero.CallUpdate(0.6f); // Tick 1
            float fill2 = hud.CurrentHeroCastProgress;
            int ticks1 = testHero.CastState.ChannelTicksExecuted;

            testHero.CallUpdate(0.6f); // Tick 2
            float fill3 = hud.CurrentHeroCastProgress;
            int ticks2 = testHero.CastState.ChannelTicksExecuted;

            bool progressOk = (fill2 > fill1) && (fill3 > fill2) && (ticks2 > ticks1);

            // 3. Complete channel
            testHero.CallUpdate(0.8f);
            bool barDisappeared = !hud.IsHeroCastBarVisible && !testHero.IsCasting;

            bool pass = barAppeared && progressOk && barDisappeared;
            Debug.Log($"[VISUAL B — CHANNEL BAR] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. ChannelBarAppeared: {barAppeared}\n" +
                      $"2. ProgressAcrossTicks: {progressOk} (Fills: {fill1:F2} -> {fill2:F2} -> {fill3:F2}, Ticks: {ticks1} -> {ticks2})\n" +
                      $"3. BarDisappearedOnComplete: {barDisappeared}");

            skillDef.SetChannel(false, 0f, 0f);
            return pass;
        }

        /// <summary>
        /// VISUAL C: STUN INTERRUPT (Play Mode)
        /// 1. Start Cast.
        /// 2. Cast Bar running.
        /// 3. Apply Stun.
        /// 4. Cast Bar stops, Interrupt feedback ("STUN") shown.
        /// 5. No cooldown, no rage refund.
        /// </summary>
        private static bool P07_9_P5_3_VisualC_StunInterrupt_PlayMode()
        {
            if (!SetupPlayModeScene_P5_3(out TestPhase5_3Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);

            testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            testHero.CallUpdate(0.3f);

            bool barRunning = hud.IsHeroCastBarVisible && testHero.IsCasting;

            // Apply Stun
            ApplyCC_P5_3(testHero, CrowdControlType.Stun, 2.0f, "pm_vis_stun");
            testHero.CallUpdate(0.1f);

            bool castInterrupted = !testHero.IsCasting && testHero.CastState.IsInterrupted;
            bool interruptFeedbackShown = hud.IsHeroShowingInterruptFeedback && hud.CurrentHeroInterruptText.Contains("STUN");
            bool fillStopped = (hud.CurrentHeroCastProgress == 0f);
            bool noCooldown = !CooldownManager.IsOnCooldown(skillDef.SkillId, out _);

            bool pass = barRunning && castInterrupted && interruptFeedbackShown && fillStopped && noCooldown;
            Debug.Log($"[VISUAL C — STUN INTERRUPT] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. BarRunning: {barRunning}\n" +
                      $"2. CastInterrupted: {castInterrupted}\n" +
                      $"3. FeedbackShown: {interruptFeedbackShown} ('{hud.CurrentHeroInterruptText}')\n" +
                      $"4. FillStopped: {fillStopped}\n" +
                      $"5. NoCooldown: {noCooldown}");

            skillDef.SetCastTime(0f);
            return pass;
        }

        /// <summary>
        /// VISUAL D: FREEZE INTERRUPT (Play Mode)
        /// 1. Start Cast.
        /// 2. Apply Freeze.
        /// 3. Bar stops, Interrupt feedback ("FREEZE") shown.
        /// </summary>
        private static bool P07_9_P5_3_VisualD_FreezeInterrupt_PlayMode()
        {
            if (!SetupPlayModeScene_P5_3(out TestPhase5_3Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);

            testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            testHero.CallUpdate(0.3f);

            // Apply Freeze
            ApplyCC_P5_3(testHero, CrowdControlType.Freeze, 2.0f, "pm_vis_freeze");
            testHero.CallUpdate(0.1f);

            bool castInterrupted = !testHero.IsCasting && testHero.CastState.IsInterrupted;
            bool interruptFeedbackShown = hud.IsHeroShowingInterruptFeedback && hud.CurrentHeroInterruptText.Contains("FREEZE");
            bool fillStopped = (hud.CurrentHeroCastProgress == 0f);

            bool pass = castInterrupted && interruptFeedbackShown && fillStopped;
            Debug.Log($"[VISUAL D — FREEZE INTERRUPT] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. CastInterrupted: {castInterrupted}\n" +
                      $"2. FeedbackShown: {interruptFeedbackShown} ('{hud.CurrentHeroInterruptText}')\n" +
                      $"3. FillStopped: {fillStopped}");

            skillDef.SetCastTime(0f);
            return pass;
        }

        /// <summary>
        /// VISUAL E: ROOT (Play Mode)
        /// 1. Start Cast.
        /// 2. Apply Root.
        /// 3. Bar continues.
        /// 4. Cast completes, no interrupt feedback.
        /// </summary>
        private static bool P07_9_P5_3_VisualE_Root_PlayMode()
        {
            if (!SetupPlayModeScene_P5_3(out TestPhase5_3Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);

            testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            testHero.CallUpdate(0.3f);

            // Apply Root
            ApplyCC_P5_3(testHero, CrowdControlType.Root, 2.0f, "pm_vis_root");
            testHero.CallUpdate(0.2f);

            bool barContinuesUnderRoot = hud.IsHeroCastBarVisible && testHero.IsCasting && testHero.IsRooted;
            bool noInterruptFeedback = !hud.IsHeroShowingInterruptFeedback;

            // Complete cast
            testHero.CallUpdate(0.6f);
            bool castCompleted = !testHero.IsCasting && !hud.IsHeroCastBarVisible;

            bool pass = barContinuesUnderRoot && noInterruptFeedback && castCompleted;
            Debug.Log($"[VISUAL E — ROOT] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. BarContinuesUnderRoot: {barContinuesUnderRoot}\n" +
                      $"2. NoInterruptFeedback: {noInterruptFeedback}\n" +
                      $"3. CastCompletedNormally: {castCompleted}");

            skillDef.SetCastTime(0f);
            return pass;
        }

        /// <summary>
        /// VISUAL F: ORDINARY DAMAGE (Play Mode)
        /// 1. Start Cast.
        /// 2. Apply ordinary damage.
        /// 3. Cast Bar continues, no interrupt feedback.
        /// 4. Cast completes.
        /// </summary>
        private static bool P07_9_P5_3_VisualF_OrdinaryDamage_PlayMode()
        {
            if (!SetupPlayModeScene_P5_3(out TestPhase5_3Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetChannel(false, 0f, 0f);
            skillDef.SetCastTime(1.0f);

            testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            testHero.CallUpdate(0.3f);

            // Deal ordinary non-lethal damage
            testHero.Health.TakeDamage(10f);
            testHero.CallUpdate(0.2f);

            bool barContinuesUnderDmg = hud.IsHeroCastBarVisible && testHero.IsCasting;
            bool noInterruptFeedback = !hud.IsHeroShowingInterruptFeedback;

            // Complete cast
            testHero.CallUpdate(0.6f);
            bool castCompleted = !testHero.IsCasting && !hud.IsHeroCastBarVisible;

            bool pass = barContinuesUnderDmg && noInterruptFeedback && castCompleted;
            Debug.Log($"[VISUAL F — ORDINARY DAMAGE] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. BarContinuesUnderDmg: {barContinuesUnderDmg}\n" +
                      $"2. NoInterruptFeedback: {noInterruptFeedback}\n" +
                      $"3. CastCompletedNormally: {castCompleted}");

            skillDef.SetCastTime(0f);
            return pass;
        }

        /// <summary>
        /// VISUAL G: CHANNEL INTERRUPT (Play Mode)
        /// 1. Start Channel.
        /// 2. Observe at least 1 tick.
        /// 3. Apply Stun.
        /// 4. Channel Bar stops, interrupt feedback shown, no new ticks.
        /// </summary>
        private static bool P07_9_P5_3_VisualG_ChannelInterrupt_PlayMode()
        {
            if (!SetupPlayModeScene_P5_3(out TestPhase5_3Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(true, 2.0f, 0.5f);

            testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            testHero.CallUpdate(0.6f); // 1 tick executes

            int ticksBefore = testHero.CastState.ChannelTicksExecuted;
            bool hadTick = ticksBefore == 1;

            // Apply Stun
            ApplyCC_P5_3(testHero, CrowdControlType.Stun, 2.0f, "pm_vis_chan_stun");
            testHero.CallUpdate(0.1f);

            bool channelInterrupted = !testHero.IsCasting && testHero.CastState.IsInterrupted;
            bool interruptFeedbackShown = hud.IsHeroShowingInterruptFeedback && hud.CurrentHeroInterruptText.Contains("STUN");

            // Further time progression -> no new ticks
            testHero.CallUpdate(0.8f);
            int ticksAfter = testHero.CastState.ChannelTicksExecuted;
            bool noMoreTicks = (ticksAfter == ticksBefore);

            bool pass = hadTick && channelInterrupted && interruptFeedbackShown && noMoreTicks;
            Debug.Log($"[VISUAL G — CHANNEL INTERRUPT] Result: {(pass ? "PASS" : "FAIL")}\n" +
                      $"1. HadTickBeforeInterrupt: {hadTick} ({ticksBefore})\n" +
                      $"2. ChannelInterrupted: {channelInterrupted}\n" +
                      $"3. InterruptFeedbackShown: {interruptFeedbackShown} ('{hud.CurrentHeroInterruptText}')\n" +
                      $"4. NoMoreTicksAfterInterrupt: {noMoreTicks} ({ticksAfter})");

            skillDef.SetChannel(false, 0f, 0f);
            return pass;
        }

        /// <summary>
        /// VISUAL H: INSTANT (Play Mode)
        /// Trigger Instant skill. Confirm no Cast Bar / Channel Bar appears.
        /// </summary>
        private static bool P07_9_P5_3_VisualH_Instant_PlayMode()
        {
            if (!SetupPlayModeScene_P5_3(out TestPhase5_3Hero testHero, out BattleManager bm, out MindMethodManager mmMgr, out BattleHUD hud))
                return false;

            SkillDefinitionSO skillDef = mmMgr.GetSelectedSkillForSlot(SkillSlotType.Skill);
            skillDef.SetCastTime(0f);
            skillDef.SetChannel(false, 0f, 0f);

            testHero.ExecuteSelectedSkill(SkillSlotType.Skill, bm.CurrentMonster);
            testHero.CallUpdate(0.1f);

            bool noBar = !hud.IsHeroCastBarVisible && !hud.IsHeroShowingInterruptFeedback && !testHero.IsCasting;
            Debug.Log($"[VISUAL H — INSTANT] Result: {(noBar ? "PASS" : "FAIL")}\n" +
                      $"1. NoBarAppeared: {noBar} (IsVisible: {hud.IsHeroCastBarVisible}, IsCasting: {testHero.IsCasting})");

            return noBar;
        }
    }
}
#endif
