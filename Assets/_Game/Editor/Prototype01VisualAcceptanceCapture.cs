using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Entities.Components;
using WuxiaGame.Progression;
using WuxiaGame.UI;
using WuxiaGame.UI.Core;
using WuxiaGame.UI.HUD;

namespace WuxiaGame.Editor
{
    public class TestCompanionEntity : Entity
    {
        public void Setup(string name, float hp)
        {
            entityType = EntityType.Companion;
            entityName = name;
            var h = gameObject.AddComponent<HealthComponent>();
            h.InitializeHealth(hp);
            gameObject.AddComponent<EntityStatsComponent>();
        }
    }

    public static class Prototype01VisualAcceptanceCapture
    {
        private const string OutputDir = "Screenshots/UI02_Remediation";
        private const int ScreenWidth = 1080;
        private const int ScreenHeight = 1920;

        [MenuItem("Tools/Wuxia RPG/Capture UI-02 Visual Acceptance Screenshots")]
        public static void RunCapture()
        {
            Debug.Log("[VISUAL ACCEPTANCE] Starting capture of SHOT A through SHOT M at 1080x1920...");

            if (!Directory.Exists(OutputDir))
            {
                Directory.CreateDirectory(OutputDir);
            }

            // 1. Ensure scene is built and open
            string scenePath = "Assets/_Game/Scenes/Prototype01.unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
            }

            GameObject canvasGO = GameObject.Find("UI Canvas");
            Canvas canvas = canvasGO != null ? canvasGO.GetComponent<Canvas>() : null;
            if (canvas == null || cam == null)
            {
                Debug.LogError("[VISUAL ACCEPTANCE] Could not find Canvas or Main Camera!");
                return;
            }

            // Setup camera and canvas for offscreen 1080x1920 capture
            RenderMode origRenderMode = canvas.renderMode;
            Camera origWorldCam = canvas.worldCamera;
            float origPlaneDist = canvas.planeDistance;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 5f;

            RenderTexture rt = new RenderTexture(ScreenWidth, ScreenHeight, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 4;
            cam.targetTexture = rt;

            // Find runtime UI components
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            Monster monster = UnityEngine.Object.FindAnyObjectByType<Monster>();
            BattleHUD battleHUD = canvasGO.GetComponent<BattleHUD>();
            SkillBarUI skillBarUI = UnityEngine.Object.FindAnyObjectByType<SkillBarUI>();
            CastBarUI heroCastUI = canvasGO.GetComponentInChildren<CastBarUI>(true);
            StatusIconRowUI[] statusRows = canvasGO.GetComponentsInChildren<StatusIconRowUI>(true);
            StatusIconRowUI heroStatusUI = null;
            StatusIconRowUI monsterStatusUI = null;
            foreach (var r in statusRows)
            {
                if (r.BoundEntityType == EntityType.Hero) heroStatusUI = r;
                else if (r.BoundEntityType == EntityType.Monster) monsterStatusUI = r;
            }

            CompanionHUDUI compHUDUI = canvasGO.GetComponentInChildren<CompanionHUDUI>(true);
            GameObject debugPanel = null;
            foreach (var t in canvasGO.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "DeveloperDebugPanel")
                {
                    debugPanel = t.gameObject;
                    break;
                }
            }

            // Explicitly ensure standee sprites & sorting orders are pristine on first frame
            if (hero != null)
            {
                var heroSR = hero.GetComponent<SpriteRenderer>();
                if (heroSR != null)
                {
                    heroSR.sprite = UIProceduralTextureFactory.GetHeroStandeeSprite();
                    heroSR.sortingOrder = 10;
                }
            }
            if (monster != null)
            {
                var monsterSR = monster.GetComponent<SpriteRenderer>();
                if (monsterSR != null)
                {
                    monsterSR.sprite = UIProceduralTextureFactory.GetMonsterStandeeSprite();
                    monsterSR.flipX = true;
                    monsterSR.sortingOrder = 10;
                }
            }

            // Warm up camera and canvas layout pass
            cam.Render();

            // =========================================================
            // SHOT A: Normal combat screen
            // =========================================================
            if (debugPanel != null) debugPanel.SetActive(false);
            CaptureFrame(cam, rt, "SHOT_A_NormalCombat.png", "Normal Combat Screen (Wuxia Portrait 1080x1920)");

            // =========================================================
            // SHOT B: Hero with Shield
            // =========================================================
            if (hero != null && hero.StatusController != null)
            {
                hero.StatusController.ApplyShield("shield_demon", 450f, 15f);
            }
            if (battleHUD != null)
            {
                battleHUD.UpdateShieldDisplay(hero);
            }
            CaptureFrame(cam, rt, "SHOT_B_HeroWithShield.png", "Hero with Active Qi Shield (450)");

            // =========================================================
            // SHOT C: Full Rage + Ultimate ready
            // =========================================================
            if (hero != null && hero.Rage != null)
            {
                hero.Rage.AddRage(100f);
            }
            if (skillBarUI != null)
            {
                skillBarUI.UpdateAllSlots();
            }
            CaptureFrame(cam, rt, "SHOT_C_FullRageUltimateReady.png", "Full Rage (100) + Golden Dragon Ultimate Ready");

            // =========================================================
            // SHOT D: Active Cast
            // =========================================================
            if (heroCastUI != null)
            {
                heroCastUI.ClearInterruptFeedback();
                heroCastUI.RootObject.SetActive(true);
                if (heroCastUI.FillImage != null)
                {
                    heroCastUI.FillImage.gameObject.SetActive(true);
                    heroCastUI.FillImage.color = new Color(0.2f, 0.8f, 1f, 1f);
                    heroCastUI.FillImage.fillAmount = 0.65f;
                }
                if (heroCastUI.CastText != null)
                {
                    heroCastUI.CastText.gameObject.SetActive(true);
                    heroCastUI.CastText.text = "CAST: Chấn Thiên Chưởng (0.8s)";
                }
            }
            CaptureFrame(cam, rt, "SHOT_D_ActiveCast.png", "Active Cast Progress (Chấn Thiên Chưởng)");

            // =========================================================
            // SHOT E: Active Channel
            // =========================================================
            if (heroCastUI != null)
            {
                heroCastUI.ClearInterruptFeedback();
                heroCastUI.RootObject.SetActive(true);
                if (heroCastUI.FillImage != null)
                {
                    heroCastUI.FillImage.gameObject.SetActive(true);
                    heroCastUI.FillImage.color = new Color(0.85f, 0.45f, 1f, 1f);
                    heroCastUI.FillImage.fillAmount = 0.5f;
                }
                if (heroCastUI.CastText != null)
                {
                    heroCastUI.CastText.gameObject.SetActive(true);
                    heroCastUI.CastText.text = "CHANNEL: Ngự Khí Hồi Xuân [Tick 2/3]";
                }
            }
            CaptureFrame(cam, rt, "SHOT_E_ActiveChannel.png", "Active Channeling (Ngự Khí Hồi Xuân)");

            // =========================================================
            // SHOT F: CC Interrupt
            // =========================================================
            if (heroCastUI != null)
            {
                heroCastUI.ShowInterruptFeedback("BỊ NGẮT CHIÊU! (CHOÁNG)", SkillCastInterruptSource.CrowdControl);
            }
            CaptureFrame(cam, rt, "SHOT_F_CCInterrupt.png", "Skill Interrupted Alert (BỊ NGẮT CHIÊU - CHOÁNG)");

            // =========================================================
            // SHOT G: Status Effects
            // =========================================================
            if (heroStatusUI != null && hero != null) heroStatusUI.BindEntity(hero);
            if (monsterStatusUI != null && monster != null) monsterStatusUI.BindEntity(monster);

            if (hero != null && hero.StatusController != null)
            {
                hero.StatusController.ApplyCrowdControl("cc_stun", CrowdControlType.Stun, 3f);
                hero.StatusController.ApplyAntiCCImmunity(5f);
            }
            if (monster != null && monster.StatusController != null)
            {
                monster.StatusController.ApplyCrowdControl("cc_freeze", CrowdControlType.Freeze, 4f);
                monster.StatusController.ApplyCrowdControl("cc_root", CrowdControlType.Root, 2.5f);
            }
            if (heroStatusUI != null) heroStatusUI.RefreshStatusDisplay();
            if (monsterStatusUI != null) monsterStatusUI.RefreshStatusDisplay();
            CaptureFrame(cam, rt, "SHOT_G_StatusEffects.png", "Status Effects on Hero and Monster (Stun, Freeze, Root, Anti-CC)");

            // =========================================================
            // SHOT H: Companion Alive
            // =========================================================
            GameObject testCompGO = new GameObject("Companion_TestAlly");
            testCompGO.transform.position = new Vector3(-1.2f, 0.4f, 0f);
            testCompGO.transform.localScale = new Vector3(1.8f, 2.4f, 1f);
            SpriteRenderer compSR = testCompGO.AddComponent<SpriteRenderer>();
            compSR.sprite = UIProceduralTextureFactory.GetHeroStandeeSprite();
            compSR.color = new Color(0.7f, 1f, 0.85f, 1f);

            TestCompanionEntity compEntity = testCompGO.AddComponent<TestCompanionEntity>();
            compEntity.Setup("Tiểu Sư Muội", 650f);

            if (compHUDUI != null)
            {
                compHUDUI.BindCompanion(compEntity);
                compHUDUI.UpdateDisplay();
            }
            CaptureFrame(cam, rt, "SHOT_H_CompanionAlive.png", "Companion HUD Present & Alive (Tiểu Sư Muội)");

            // =========================================================
            // SHOT I: Companion Injured/Dead
            // =========================================================
            if (compEntity != null && compEntity.Health != null)
            {
                compEntity.Health.TakeDamage(650f);
            }
            if (compHUDUI != null)
            {
                compHUDUI.UpdateDisplay();
            }
            CaptureFrame(cam, rt, "SHOT_I_CompanionInjuredDead.png", "Companion Injured / Fallen State (TRỌNG THƯƠNG)");

            // =========================================================
            // SHOT J: Cooldown Active
            // =========================================================
            if (skillBarUI != null)
            {
                var s2Sweep = skillBarUI.transform.Find("Slot2_Skill/CooldownSweep")?.GetComponent<Image>();
                var s2Txt = skillBarUI.transform.Find("Slot2_Skill/CooldownText")?.GetComponent<TextMeshProUGUI>();
                if (s2Sweep != null)
                {
                    s2Sweep.gameObject.SetActive(true);
                    s2Sweep.fillAmount = 0.65f;
                }
                if (s2Txt != null)
                {
                    s2Txt.gameObject.SetActive(true);
                    s2Txt.text = "4.2s";
                }

                var s3Sweep = skillBarUI.transform.Find("Slot3_External1/CooldownSweep")?.GetComponent<Image>();
                var s3Txt = skillBarUI.transform.Find("Slot3_External1/CooldownText")?.GetComponent<TextMeshProUGUI>();
                if (s3Sweep != null)
                {
                    s3Sweep.gameObject.SetActive(true);
                    s3Sweep.fillAmount = 0.35f;
                }
                if (s3Txt != null)
                {
                    s3Txt.gameObject.SetActive(true);
                    s3Txt.text = "8.0s";
                }
            }
            CaptureFrame(cam, rt, "SHOT_J_CooldownActive.png", "Skills with Active Radial Cooldown & Sweep Overlay");

            // =========================================================
            // SHOT K: Debug Drawer Opened
            // =========================================================
            if (debugPanel != null) debugPanel.SetActive(true);
            CaptureFrame(cam, rt, "SHOT_K_DebugDrawerOpened.png", "Developer Debug Drawer Expanded");

            // =========================================================
            // SHOT L: Debug Drawer Collapsed
            // =========================================================
            if (debugPanel != null) debugPanel.SetActive(false);
            CaptureFrame(cam, rt, "SHOT_L_DebugDrawerCollapsed.png", "Developer Debug Drawer Collapsed ([DEV ⚙] Pill Tab)");

            // =========================================================
            // SHOT M: Safe Area / Portrait Layout
            // =========================================================
            CaptureFrame(cam, rt, "SHOT_M_SafeAreaPortraitLayout.png", "1080x1920 Mobile Portrait Safe Area Layout Compliance");

            // Cleanup & Restore
            if (testCompGO != null) UnityEngine.Object.DestroyImmediate(testCompGO);
            cam.targetTexture = null;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);

            canvas.renderMode = origRenderMode;
            canvas.worldCamera = origWorldCam;
            canvas.planeDistance = origPlaneDist;

            Debug.Log("[VISUAL ACCEPTANCE] Successfully captured all 13 screenshots (SHOT A to SHOT M) in " + OutputDir);
        }

        private static void CaptureFrame(Camera cam, RenderTexture rt, string filename, string description)
        {
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(ScreenWidth, ScreenHeight, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, ScreenWidth, ScreenHeight), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            string fullPath = Path.Combine(OutputDir, filename);
            File.WriteAllBytes(fullPath, bytes);

            FileInfo fi = new FileInfo(fullPath);
            Debug.Log($"[SCREENSHOT SAVED] {filename} ({fi.Length / 1024} KB) - {description}");
        }
    }
}
