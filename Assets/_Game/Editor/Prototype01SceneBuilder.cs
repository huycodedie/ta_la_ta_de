#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Drop;
using WuxiaGame.Entities;
using WuxiaGame.Entities.Components;
using WuxiaGame.Equipment;
using WuxiaGame.Inventory;
using WuxiaGame.Items;
using WuxiaGame.Progression;
using WuxiaGame.Stats;
using WuxiaGame.UI;
using WuxiaGame.UI.Core;
using WuxiaGame.UI.HUD;
using WuxiaGame.UI.Modal;

namespace WuxiaGame.Editor
{
    public static class Prototype01SceneBuilder
    {
        private const string RelativeFolderData = "Assets/_Game/Data";
        private const string RelativeFolderResourcesData = "Assets/_Game/Resources/Data";
        private const string RelativeFolderScenes = "Assets/_Game/Scenes";
        private const string SceneAssetPath = "Assets/_Game/Scenes/Prototype01.unity";

        private static string DiskFolderData => Path.Combine(Directory.GetCurrentDirectory(), "Assets", "_Game", "Data");
        private static string DiskFolderResourcesData => Path.Combine(Directory.GetCurrentDirectory(), "Assets", "_Game", "Resources", "Data");
        private static string DiskFolderScenes => Path.Combine(Directory.GetCurrentDirectory(), "Assets", "_Game", "Scenes");
        private static string DiskScenePath => Path.Combine(Directory.GetCurrentDirectory(), "Assets", "_Game", "Scenes", "Prototype01.unity");

        [MenuItem("Tools/Wuxia RPG/Open Prototype 01 Scene", priority = 0)]
        public static void OpenPrototype01Scene()
        {
            if (File.Exists(DiskScenePath))
            {
                EditorSceneManager.OpenScene(SceneAssetPath, OpenSceneMode.Single);
                Debug.Log($"[Prototype01SceneBuilder] Opened prototype scene '{SceneAssetPath}'.");
            }
            else
            {
                BuildPrototype01Scene();
            }
        }

        [InitializeOnLoadMethod]
        private static void AutoBuildIfMissing()
        {
            try
            {
                EnsureBuildSettingsContainsScene();

                if (!File.Exists(DiskScenePath))
                {
                    BuildPrototype01Scene();
                }
                else
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (!Application.isPlaying && !Application.isBatchMode)
                        {
                            var activeScene = EditorSceneManager.GetActiveScene();
                            if (string.IsNullOrEmpty(activeScene.path) || activeScene.name == "Untitled")
                            {
                                if (File.Exists(DiskScenePath))
                                {
                                    EditorSceneManager.OpenScene(SceneAssetPath, OpenSceneMode.Single);
                                    Debug.Log($"[Prototype01SceneBuilder] Auto-opened active prototype scene '{SceneAssetPath}'.");
                                }
                            }
                        }
                    };
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Prototype01SceneBuilder] Error in AutoBuildIfMissing: {ex.Message}");
            }
        }

        private static void EnsureBuildSettingsContainsScene()
        {
            var scenes = EditorBuildSettings.scenes;
            bool found = false;
            foreach (var s in scenes)
            {
                if (s.path == SceneAssetPath)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
                newScenes[0] = new EditorBuildSettingsScene(SceneAssetPath, true);
                for (int i = 0; i < scenes.Length; i++)
                {
                    newScenes[i + 1] = scenes[i];
                }
                EditorBuildSettings.scenes = newScenes;
                Debug.Log($"[Prototype01SceneBuilder] Added '{SceneAssetPath}' as primary scene in Build Settings.");
            }
        }

        [MenuItem("Tools/Wuxia RPG/Build Prototype 02 Data")]
        public static void BuildPrototype02Data()
        {
            EnsureDirectories();

            // Create Data folders
            string rFolder = $"{RelativeFolderData}/Rarities";
            string resRFolder = $"{RelativeFolderResourcesData}/Rarities";
            string dlFolder = $"{RelativeFolderData}/DropLevels";
            string resDlFolder = $"{RelativeFolderResourcesData}/DropLevels";
            string affFolder = $"{RelativeFolderData}/Affixes";
            string resAffFolder = $"{RelativeFolderResourcesData}/Affixes";
            string eqFolder = $"{RelativeFolderData}/Equipment";
            string resEqFolder = $"{RelativeFolderResourcesData}/Equipment";

            if (!AssetDatabase.IsValidFolder(rFolder)) AssetDatabase.CreateFolder(RelativeFolderData, "Rarities");
            if (!AssetDatabase.IsValidFolder(resRFolder)) AssetDatabase.CreateFolder(RelativeFolderResourcesData, "Rarities");
            if (!AssetDatabase.IsValidFolder(dlFolder)) AssetDatabase.CreateFolder(RelativeFolderData, "DropLevels");
            if (!AssetDatabase.IsValidFolder(resDlFolder)) AssetDatabase.CreateFolder(RelativeFolderResourcesData, "DropLevels");
            if (!AssetDatabase.IsValidFolder(affFolder)) AssetDatabase.CreateFolder(RelativeFolderData, "Affixes");
            if (!AssetDatabase.IsValidFolder(resAffFolder)) AssetDatabase.CreateFolder(RelativeFolderResourcesData, "Affixes");
            if (!AssetDatabase.IsValidFolder(eqFolder)) AssetDatabase.CreateFolder(RelativeFolderData, "Equipment");
            if (!AssetDatabase.IsValidFolder(resEqFolder)) AssetDatabase.CreateFolder(RelativeFolderResourcesData, "Equipment");

            // 1. Create Rarities
            var rarities = CreateRarityAssets(rFolder);
            CreateRarityAssets(resRFolder);

            // 2. Create Drop Levels / Loot Tiers
            var dropLevels = CreateDropLevelAssets(dlFolder, rarities);
            var resRarities = CreateRarityAssets(resRFolder);
            var resDropLevels = CreateDropLevelAssets(resDlFolder, resRarities);

            DropLevelDatabaseSO dlDb = GetOrCreateDropLevelDatabase($"{RelativeFolderData}/DropLevelDatabase.asset");
            dlDb.SetDropLevels(dropLevels);
            EditorUtility.SetDirty(dlDb);

            DropLevelDatabaseSO resDlDb = GetOrCreateDropLevelDatabase($"{RelativeFolderResourcesData}/DropLevelDatabase.asset");
            resDlDb.SetDropLevels(resDropLevels);
            EditorUtility.SetDirty(resDlDb);

            // Create LootTier Configs & EquipmentDropConfig
            string ltFolder = $"{RelativeFolderData}/LootTiers";
            string resLtFolder = $"{RelativeFolderResourcesData}/LootTiers";
            if (!AssetDatabase.IsValidFolder(ltFolder)) AssetDatabase.CreateFolder(RelativeFolderData, "LootTiers");
            if (!AssetDatabase.IsValidFolder(resLtFolder)) AssetDatabase.CreateFolder(RelativeFolderResourcesData, "LootTiers");

            var lt1 = GetOrCreateLootTierConfig($"{ltFolder}/LootTier_01.asset", 1, "Cấp Rơi 1", new List<RarityWeightData>
            {
                new RarityWeightData(rarities[0], 80f),
                new RarityWeightData(rarities[1], 20f)
            }, 1000, 60f, 1000, 5);

            var lt2 = GetOrCreateLootTierConfig($"{ltFolder}/LootTier_02.asset", 2, "Cấp Rơi 2", new List<RarityWeightData>
            {
                new RarityWeightData(rarities[0], 55f),
                new RarityWeightData(rarities[1], 35f),
                new RarityWeightData(rarities[2], 10f)
            }, 2500, 900f, 5000, 10);

            var lt3 = GetOrCreateLootTierConfig($"{ltFolder}/LootTier_03.asset", 3, "Cấp Rơi 3", new List<RarityWeightData>
            {
                new RarityWeightData(rarities[0], 35f),
                new RarityWeightData(rarities[1], 40f),
                new RarityWeightData(rarities[2], 20f),
                new RarityWeightData(rarities[3], 5f)
            }, 5000, 1800f, 10000, 20);

            var lt16 = GetOrCreateLootTierConfig($"{ltFolder}/LootTier_16.asset", 16, "Cấp Rơi 16", new List<RarityWeightData>(dropLevels[0].RarityWeights), 10000, 3600f, 50000, 100);
            var lt17 = GetOrCreateLootTierConfig($"{ltFolder}/LootTier_17.asset", 17, "Cấp Rơi 17", new List<RarityWeightData>(dropLevels[1].RarityWeights), 20000, 7200f, 100000, 200);

            var resLt1 = GetOrCreateLootTierConfig($"{resLtFolder}/LootTier_01.asset", 1, "Cấp Rơi 1", new List<RarityWeightData>
            {
                new RarityWeightData(resRarities[0], 80f),
                new RarityWeightData(resRarities[1], 20f)
            }, 1000, 60f, 1000, 5);

            var resLt2 = GetOrCreateLootTierConfig($"{resLtFolder}/LootTier_02.asset", 2, "Cấp Rơi 2", new List<RarityWeightData>
            {
                new RarityWeightData(resRarities[0], 55f),
                new RarityWeightData(resRarities[1], 35f),
                new RarityWeightData(resRarities[2], 10f)
            }, 2500, 900f, 5000, 10);

            var resLt3 = GetOrCreateLootTierConfig($"{resLtFolder}/LootTier_03.asset", 3, "Cấp Rơi 3", new List<RarityWeightData>
            {
                new RarityWeightData(resRarities[0], 35f),
                new RarityWeightData(resRarities[1], 40f),
                new RarityWeightData(resRarities[2], 20f),
                new RarityWeightData(resRarities[3], 5f)
            }, 5000, 1800f, 10000, 20);

            var resLt16 = GetOrCreateLootTierConfig($"{resLtFolder}/LootTier_16.asset", 16, "Cấp Rơi 16", new List<RarityWeightData>(resDropLevels[0].RarityWeights), 10000, 3600f, 50000, 100);
            var resLt17 = GetOrCreateLootTierConfig($"{resLtFolder}/LootTier_17.asset", 17, "Cấp Rơi 17", new List<RarityWeightData>(resDropLevels[1].RarityWeights), 20000, 7200f, 100000, 200);

            var ltList = new List<LootTierConfigSO> { lt1, lt2, lt3, lt16, lt17 };
            var resLtList = new List<LootTierConfigSO> { resLt1, resLt2, resLt3, resLt16, resLt17 };

            LootTierDatabaseSO ltDb = GetOrCreateLootTierDatabase($"{RelativeFolderData}/LootTierDatabase.asset");
            ltDb.SetTiers(ltList);
            EditorUtility.SetDirty(ltDb);

            LootTierDatabaseSO resLtDb = GetOrCreateLootTierDatabase($"{RelativeFolderResourcesData}/LootTierDatabase.asset");
            resLtDb.SetTiers(resLtList);
            EditorUtility.SetDirty(resLtDb);

            var tierWeights = new List<LootTierWeightData>
            {
                new LootTierWeightData(lt16, 100f)
            };
            var dropCfg = GetOrCreateEquipmentDropConfig($"{RelativeFolderData}/EquipmentDropConfig.asset", 100f, tierWeights);
            EditorUtility.SetDirty(dropCfg);

            var resTierWeights = new List<LootTierWeightData>
            {
                new LootTierWeightData(resLt16, 100f)
            };
            var resDropCfg = GetOrCreateEquipmentDropConfig($"{RelativeFolderResourcesData}/EquipmentDropConfig.asset", 100f, resTierWeights);
            EditorUtility.SetDirty(resDropCfg);

            // 3. Create Affixes
            var affixes = CreateAffixAssets(affFolder);
            CreateAffixAssets(resAffFolder);

            AffixDatabaseSO affDb = GetOrCreateAffixDatabase($"{RelativeFolderData}/AffixDatabase.asset");
            affDb.SetAffixes(affixes);
            EditorUtility.SetDirty(affDb);

            AffixDatabaseSO resAffDb = GetOrCreateAffixDatabase($"{RelativeFolderResourcesData}/AffixDatabase.asset");
            resAffDb.SetAffixes(CreateAffixAssets(resAffFolder));
            EditorUtility.SetDirty(resAffDb);

            // 4. Create Equipment Definitions (12 Slots)
            var eqDefs = CreateEquipmentDefinitionAssets(eqFolder);
            CreateEquipmentDefinitionAssets(resEqFolder);

            EquipmentDatabaseSO eqDb = GetOrCreateEquipmentDatabase($"{RelativeFolderData}/EquipmentDatabase.asset");
            eqDb.SetDefinitions(eqDefs);
            EditorUtility.SetDirty(eqDb);

            EquipmentDatabaseSO resEqDb = GetOrCreateEquipmentDatabase($"{RelativeFolderResourcesData}/EquipmentDatabase.asset");
            resEqDb.SetDefinitions(CreateEquipmentDefinitionAssets(resEqFolder));
            EditorUtility.SetDirty(resEqDb);

            // 5. Create Equipment Upgrade Config
            var upgradeCosts = new List<EquipmentUpgradeCostData>
            {
                new EquipmentUpgradeCostData(1, 2, 1000, 2),
                new EquipmentUpgradeCostData(2, 3, 2000, 4),
                new EquipmentUpgradeCostData(3, 4, 3000, 6),
                new EquipmentUpgradeCostData(4, 5, 4000, 8)
            };

            EquipmentUpgradeConfigSO upgConfig = GetOrCreateUpgradeConfig($"{RelativeFolderData}/EquipmentUpgradeConfig.asset");
            upgConfig.InitializeConfig(5, 0.20f, upgradeCosts);
            EditorUtility.SetDirty(upgConfig);

            EquipmentUpgradeConfigSO resUpgConfig = GetOrCreateUpgradeConfig($"{RelativeFolderResourcesData}/EquipmentUpgradeConfig.asset");
            resUpgConfig.InitializeConfig(5, 0.20f, upgradeCosts);
            EditorUtility.SetDirty(resUpgConfig);

            // 6. Create Hero Progression Config
            HeroProgressionConfigSO heroProgConfig = GetOrCreateHeroProgressionConfig($"{RelativeFolderData}/HeroProgressionConfig.asset");
            heroProgConfig.InitializeConfig(1, 100, 1000f, 100f, 20f, 50f, 10f, 2f);
            EditorUtility.SetDirty(heroProgConfig);

            HeroProgressionConfigSO resHeroProgConfig = GetOrCreateHeroProgressionConfig($"{RelativeFolderResourcesData}/HeroProgressionConfig.asset");
            resHeroProgConfig.InitializeConfig(1, 100, 1000f, 100f, 20f, 50f, 10f, 2f);
            // 7. Create Title Database and Exp Config
            TitleDatabaseSO titleDb = GetOrCreateTitleDatabase($"{RelativeFolderData}/TitleDatabase.asset");
            TitleDatabaseSO resTitleDb = GetOrCreateTitleDatabase($"{RelativeFolderResourcesData}/TitleDatabase.asset");

            ExpConfigSO expConfig = GetOrCreateExpConfig($"{RelativeFolderData}/ExpConfig.asset");
            ExpConfigSO resExpConfig = GetOrCreateExpConfig($"{RelativeFolderResourcesData}/ExpConfig.asset");

            // 8. Create Title Breakthrough Database & Stages (D24)
            TitleBreakthroughDatabaseSO titleBkDb = GetOrCreateTitleBreakthroughDatabase($"{RelativeFolderData}/TitleBreakthroughDatabase.asset");
            TitleBreakthroughDatabaseSO resTitleBkDb = GetOrCreateTitleBreakthroughDatabase($"{RelativeFolderResourcesData}/TitleBreakthroughDatabase.asset");

            // 9. Create Mind Method Database & Skills (Prototype 06)
            MindMethodDatabaseSO mmDb = GetOrCreateMindMethodDatabase($"{RelativeFolderData}/MindMethodDatabase.asset");
            MindMethodDatabaseSO resMmDb = GetOrCreateMindMethodDatabase($"{RelativeFolderResourcesData}/MindMethodDatabase.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Prototype01SceneBuilder] Successfully built Prototype 02, Upgrade, Progression & MindMethod Data assets!");
        }

        [MenuItem("Tools/Wuxia RPG/Build Prototype 01 Scene")]
        public static void BuildPrototype01Scene()
        {
            EnsureDirectories();
            BuildPrototype02Data();

            if (File.Exists(DiskScenePath))
            {
                AssetDatabase.DeleteAsset(SceneAssetPath);
            }

            // Load persistent asset instances
            CombatConfigSO combatConfig = AssetDatabase.LoadAssetAtPath<CombatConfigSO>($"{RelativeFolderData}/CombatConfig.asset");
            HeroConfigSO heroConfig = AssetDatabase.LoadAssetAtPath<HeroConfigSO>($"{RelativeFolderData}/HeroConfig.asset");
            MonsterConfigSO monsterConfig = AssetDatabase.LoadAssetAtPath<MonsterConfigSO>($"{RelativeFolderData}/MonsterConfig.asset");
            ExpConfigSO expConfig = AssetDatabase.LoadAssetAtPath<ExpConfigSO>($"{RelativeFolderData}/ExpConfig.asset");
            TitleDatabaseSO titleDatabase = AssetDatabase.LoadAssetAtPath<TitleDatabaseSO>($"{RelativeFolderData}/TitleDatabase.asset");
            DropLevelDatabaseSO dropLevelDatabase = AssetDatabase.LoadAssetAtPath<DropLevelDatabaseSO>($"{RelativeFolderData}/DropLevelDatabase.asset");
            EquipmentDatabaseSO equipmentDatabase = AssetDatabase.LoadAssetAtPath<EquipmentDatabaseSO>($"{RelativeFolderData}/EquipmentDatabase.asset");
            AffixDatabaseSO affixDatabase = AssetDatabase.LoadAssetAtPath<AffixDatabaseSO>($"{RelativeFolderData}/AffixDatabase.asset");
            EquipmentDropConfigSO dropConfig = AssetDatabase.LoadAssetAtPath<EquipmentDropConfigSO>($"{RelativeFolderData}/EquipmentDropConfig.asset");

            // Create new 2D Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Sprite defaultSprite = CreateWhiteSprite();

            // Setup Main Camera
            GameObject camGO = new GameObject("Main Camera");
            Camera cam = camGO.AddComponent<Camera>();
            camGO.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = 8.5f; // Framed for 1080x1920 portrait combat viewport
            cam.transform.position = new Vector3(0, 0.5f, -10f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.08f, 0.12f);

            // Setup Managers GameObject
            GameObject managersGO = new GameObject("--- MANAGERS ---");
            GameManager gameMgr = managersGO.AddComponent<GameManager>();
            BattleManager battleMgr = managersGO.AddComponent<BattleManager>();
            DamagePopupManager popupMgr = managersGO.AddComponent<DamagePopupManager>();
            GameBootstrap bootstrap = managersGO.AddComponent<GameBootstrap>();
            ProgressionManager progressionMgr = managersGO.AddComponent<ProgressionManager>();
            Inventory.Inventory inventory = managersGO.AddComponent<Inventory.Inventory>();
            EquipmentManager equipmentMgr = managersGO.AddComponent<EquipmentManager>();
            DropSystem dropSys = managersGO.AddComponent<DropSystem>();
            ResourceManager resMgr = managersGO.AddComponent<ResourceManager>();
            EquipmentUpgradeService upgSvc = managersGO.AddComponent<EquipmentUpgradeService>();

            EquipmentUpgradeConfigSO upgConfig = GetOrCreateUpgradeConfig($"{RelativeFolderData}/EquipmentUpgradeConfig.asset");
            SerializedObject upgSO = new SerializedObject(upgSvc);
            upgSO.Update();
            upgSO.FindProperty("upgradeConfig").objectReferenceValue = upgConfig;
            upgSO.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject battleSO = new SerializedObject(battleMgr);
            battleSO.Update();
            battleSO.FindProperty("defaultMonsterConfig").objectReferenceValue = monsterConfig;
            battleSO.FindProperty("defaultCombatConfig").objectReferenceValue = combatConfig;
            battleSO.FindProperty("monsterSpawnPosition").vector3Value = new Vector3(4f, -0.3f, 0f);
            battleSO.ApplyModifiedPropertiesWithoutUndo();

            HeroProgressionConfigSO heroProgConfig = AssetDatabase.LoadAssetAtPath<HeroProgressionConfigSO>($"{RelativeFolderData}/HeroProgressionConfig.asset");

            SerializedObject bootstrapSO = new SerializedObject(bootstrap);
            bootstrapSO.Update();
            bootstrapSO.FindProperty("globalCombatConfig").objectReferenceValue = combatConfig;
            bootstrapSO.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject progressionSO = new SerializedObject(progressionMgr);
            progressionSO.Update();
            progressionSO.FindProperty("expConfig").objectReferenceValue = expConfig;
            progressionSO.FindProperty("progressionConfig").objectReferenceValue = heroProgConfig;
            progressionSO.FindProperty("titleDatabase").objectReferenceValue = titleDatabase;
            progressionSO.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject dropSO = new SerializedObject(dropSys);
            dropSO.Update();
            dropSO.FindProperty("dropLevelDatabase").objectReferenceValue = dropLevelDatabase;
            dropSO.FindProperty("equipmentDatabase").objectReferenceValue = equipmentDatabase;
            dropSO.FindProperty("affixDatabase").objectReferenceValue = affixDatabase;
            dropSO.FindProperty("dropConfig").objectReferenceValue = dropConfig;
            dropSO.ApplyModifiedPropertiesWithoutUndo();

            LootTierDatabaseSO lootTierDatabase = AssetDatabase.LoadAssetAtPath<LootTierDatabaseSO>($"{RelativeFolderData}/LootTierDatabase.asset");
            LootTierProgressionManager lootProgMgr = managersGO.AddComponent<LootTierProgressionManager>();
            SerializedObject lootProgSO = new SerializedObject(lootProgMgr);
            lootProgSO.Update();
            lootProgSO.FindProperty("lootTierDatabase").objectReferenceValue = lootTierDatabase;
            lootProgSO.ApplyModifiedPropertiesWithoutUndo();

            TitleBreakthroughDatabaseSO titleBkDatabase = AssetDatabase.LoadAssetAtPath<TitleBreakthroughDatabaseSO>($"{RelativeFolderData}/TitleBreakthroughDatabase.asset");
            TitleBreakthroughManager titleBkMgr = managersGO.AddComponent<TitleBreakthroughManager>();
            SerializedObject titleBkSO = new SerializedObject(titleBkMgr);
            titleBkSO.Update();
            titleBkSO.FindProperty("breakthroughDatabase").objectReferenceValue = titleBkDatabase;
            titleBkSO.ApplyModifiedPropertiesWithoutUndo();

            MindMethodDatabaseSO mmDatabase = AssetDatabase.LoadAssetAtPath<MindMethodDatabaseSO>($"{RelativeFolderData}/MindMethodDatabase.asset");
            MindMethodManager mmMgr = managersGO.AddComponent<MindMethodManager>();
            SerializedObject mmSO = new SerializedObject(mmMgr);
            mmSO.Update();
            mmSO.FindProperty("database").objectReferenceValue = mmDatabase;
            mmSO.ApplyModifiedPropertiesWithoutUndo();

            // Misty Mountain Combat Background
            GameObject bgGO = new GameObject("CombatBackground");
            bgGO.transform.position = new Vector3(0, 1.2f, 6f);
            bgGO.transform.localScale = new Vector3(14f, 20f, 1f);
            SpriteRenderer bgSR = bgGO.AddComponent<SpriteRenderer>();
            bgSR.sprite = UIProceduralTextureFactory.GetCombatBackgroundSprite();
            bgSR.color = new Color(0.88f, 0.90f, 0.96f, 1f);
            bgSR.sortingOrder = -50;

            // Ground Platform Visual (Dark Wuxia Stone Platform)
            GameObject groundGO = new GameObject("GroundVisual");
            groundGO.transform.position = new Vector3(0, -0.8f, 0);
            groundGO.transform.localScale = new Vector3(12f, 1.6f, 1f);
            SpriteRenderer groundSR = groundGO.AddComponent<SpriteRenderer>();
            groundSR.sprite = UIProceduralTextureFactory.GetPlatformSprite();
            groundSR.color = Color.white;
            groundSR.sortingOrder = -10;

            // Hero Visual & Components (Wuxia Hero Standee with Qi Aura)
            GameObject heroGO = new GameObject("Hero");
            heroGO.transform.position = new Vector3(-2.2f, -0.3f, 0);
            heroGO.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            SpriteRenderer heroSR = heroGO.AddComponent<SpriteRenderer>();
            heroSR.sprite = UIProceduralTextureFactory.GetHeroStandeeSprite();
            heroSR.color = Color.white;
            heroSR.sortingOrder = 10;

            Hero heroComp = heroGO.AddComponent<Hero>();
            heroGO.AddComponent<WuxiaGame.Combat.HeroSkillDecisionController>();
            SerializedObject heroSO = new SerializedObject(heroComp);
            heroSO.Update();
            heroSO.FindProperty("heroConfig").objectReferenceValue = heroConfig;
            heroSO.FindProperty("combatConfig").objectReferenceValue = combatConfig;
            heroSO.FindProperty("progressionConfig").objectReferenceValue = heroProgConfig;
            heroSO.ApplyModifiedPropertiesWithoutUndo();

            // Monster Visual & Components (Crimson Demon Beast Standee with Fiery Aura)
            GameObject monsterGO = new GameObject("Monster_Wild");
            monsterGO.transform.position = new Vector3(2.2f, -0.3f, 0);
            monsterGO.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            SpriteRenderer monsterSR = monsterGO.AddComponent<SpriteRenderer>();
            monsterSR.sprite = UIProceduralTextureFactory.GetMonsterStandeeSprite();
            monsterSR.color = Color.white;
            monsterSR.sortingOrder = 10;
            monsterSR.flipX = true;

            Monster monsterComp = monsterGO.AddComponent<Monster>();
            SerializedObject monsterSO = new SerializedObject(monsterComp);
            monsterSO.Update();
            monsterSO.FindProperty("entityType").enumValueIndex = (int)EntityType.Monster;
            monsterSO.FindProperty("monsterConfig").objectReferenceValue = monsterConfig;
            monsterSO.FindProperty("combatConfig").objectReferenceValue = combatConfig;
            monsterSO.ApplyModifiedPropertiesWithoutUndo();

            // Monster 2: Secondary (Monster_Wild_2) - P08 Two-Monster Production Fixture
            GameObject monster2GO = new GameObject("Monster_Wild_2");
            monster2GO.transform.position = new Vector3(3.8f, -0.3f, 0);
            monster2GO.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            SpriteRenderer monster2SR = monster2GO.AddComponent<SpriteRenderer>();
            monster2SR.sprite = UIProceduralTextureFactory.GetMonsterStandeeSprite();
            monster2SR.color = new Color(0.9f, 0.75f, 0.75f, 1f);
            monster2SR.sortingOrder = 10;
            monster2SR.flipX = true;

            Monster monster2Comp = monster2GO.AddComponent<Monster>();
            SerializedObject monster2SO = new SerializedObject(monster2Comp);
            monster2SO.Update();
            monster2SO.FindProperty("entityType").enumValueIndex = (int)EntityType.Monster;
            monster2SO.FindProperty("monsterConfig").objectReferenceValue = monsterConfig;
            monster2SO.FindProperty("combatConfig").objectReferenceValue = combatConfig;
            monster2SO.ApplyModifiedPropertiesWithoutUndo();

            battleMgr.RegisterHero(heroComp);
            SerializedObject bmSO = new SerializedObject(battleMgr);
            bmSO.Update();
            bmSO.FindProperty("currentMonster").objectReferenceValue = monsterComp;
            bmSO.ApplyModifiedPropertiesWithoutUndo();

            // Canvas & UI HUD (Portrait 1080x1920)
            GameObject canvasGO = new GameObject("UI Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UIStyleConfig.ReferenceResolution; // 1080x1920
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f; // Portrait: Match width
            canvasGO.AddComponent<GraphicRaycaster>();
            canvasGO.AddComponent<UIRaycastDebugger>();

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            var inputModule = eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();

            // Core Controllers attached directly to Canvas
            BattleHUD battleHUD = canvasGO.AddComponent<BattleHUD>();
            DebugProgressionUI progressionUI = canvasGO.AddComponent<DebugProgressionUI>();
            EquipmentDropDebugUI dropUI = canvasGO.AddComponent<EquipmentDropDebugUI>();

            // UI-01 Portrait Shell: SafeAreaRoot -> MainGameShell -> (MainContentArea + GlobalBottomNavigation)
            GameObject safeAreaRootGO = new GameObject("SafeAreaRoot");
            safeAreaRootGO.transform.SetParent(canvasGO.transform, false);
            RectTransform safeAreaRect = safeAreaRootGO.AddComponent<RectTransform>();
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;
            WuxiaGame.UI.Core.SafeArea safeAreaComp = safeAreaRootGO.AddComponent<WuxiaGame.UI.Core.SafeArea>();

            GameObject mainGameShellGO = new GameObject("MainGameShell");
            mainGameShellGO.transform.SetParent(safeAreaRootGO.transform, false);
            RectTransform mainShellRect = mainGameShellGO.AddComponent<RectTransform>();
            mainShellRect.anchorMin = Vector2.zero;
            mainShellRect.anchorMax = Vector2.one;
            mainShellRect.offsetMin = Vector2.zero;
            mainShellRect.offsetMax = Vector2.zero;
            MainGameShell mainGameShellComp = mainGameShellGO.AddComponent<MainGameShell>();

            GameObject contentAreaGO = new GameObject("MainContentArea");
            contentAreaGO.transform.SetParent(mainGameShellGO.transform, false);
            RectTransform contentAreaRect = contentAreaGO.AddComponent<RectTransform>();
            contentAreaRect.anchorMin = Vector2.zero;
            contentAreaRect.anchorMax = Vector2.one;
            contentAreaRect.offsetMin = new Vector2(0f, 160f); // 160px reserved for Global Bottom Navigation
            contentAreaRect.offsetMax = Vector2.zero;

            GameObject bottomNavGO = new GameObject("GlobalBottomNavigation");
            bottomNavGO.transform.SetParent(mainGameShellGO.transform, false);
            RectTransform bottomNavRect = bottomNavGO.AddComponent<RectTransform>();
            bottomNavRect.anchorMin = new Vector2(0f, 0f);
            bottomNavRect.anchorMax = new Vector2(1f, 0f);
            bottomNavRect.pivot = new Vector2(0.5f, 0f);
            bottomNavRect.anchoredPosition = Vector2.zero;
            bottomNavRect.sizeDelta = new Vector2(0f, 160f);
            CreateUIImage(bottomNavGO.transform, "NavBg", UIStyleConfig.InkBlackTranslucent);

            // Separator border on top of bottom navigation
            GameObject navBorderGO = CreateUIRect(bottomNavGO.transform, "TopBorder", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -1f), new Vector2(0f, 2f));
            CreateUIImage(navBorderGO.transform, "BorderLine", UIStyleConfig.BorderBronzeDark);

            GlobalBottomNavigation bottomNavComp = bottomNavGO.AddComponent<GlobalBottomNavigation>();
            CreateBottomNavigationSlots(bottomNavGO.transform, bottomNavComp, defaultSprite);

            mainGameShellComp.SetReferences(safeAreaComp, contentAreaRect, bottomNavComp);
            SerializedObject shellSO = new SerializedObject(mainGameShellComp);
            shellSO.Update();
            shellSO.FindProperty("safeArea").objectReferenceValue = safeAreaComp;
            shellSO.FindProperty("mainContentArea").objectReferenceValue = contentAreaRect;
            shellSO.FindProperty("navigation").objectReferenceValue = bottomNavComp;
            shellSO.ApplyModifiedPropertiesWithoutUndo();

            // ==========================================
            // UI-POLISH-01 Phase B1: MODAL LAYER & BACKDROP
            // Direct child of UI Canvas (renders above SafeAreaRoot and GlobalBottomNavigation)
            // ==========================================
            GameObject modalLayerGO = new GameObject("ModalLayer");
            modalLayerGO.transform.SetParent(canvasGO.transform, false);
            RectTransform modalLayerRect = modalLayerGO.AddComponent<RectTransform>();
            modalLayerRect.anchorMin = Vector2.zero;
            modalLayerRect.anchorMax = Vector2.one;
            modalLayerRect.offsetMin = Vector2.zero;
            modalLayerRect.offsetMax = Vector2.zero;
            modalLayerRect.pivot = new Vector2(0.5f, 0.5f);

            // ModalBackdrop: Sibling 0 inside ModalLayer, stretches to root Canvas
            GameObject backdropGO = new GameObject("ModalBackdrop");
            backdropGO.transform.SetParent(modalLayerGO.transform, false);
            RectTransform backdropRect = backdropGO.AddComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;
            backdropRect.pivot = new Vector2(0.5f, 0.5f);
            Image backdropImg = backdropGO.AddComponent<Image>();
            backdropImg.color = new Color(0f, 0f, 0f, 0.65f);
            backdropImg.raycastTarget = true;
            ModalBackdrop modalBackdropComp = backdropGO.AddComponent<ModalBackdrop>();
            modalBackdropComp.SetReferences(backdropImg);
            backdropGO.SetActive(false);

            // ModalSafeContent: Sibling 1 inside ModalLayer, runtime Safe Area constrained
            GameObject modalSafeContentGO = new GameObject("ModalSafeContent");
            modalSafeContentGO.transform.SetParent(modalLayerGO.transform, false);
            RectTransform modalSafeContentRect = modalSafeContentGO.AddComponent<RectTransform>();
            modalSafeContentRect.anchorMin = Vector2.zero;
            modalSafeContentRect.anchorMax = Vector2.one;
            modalSafeContentRect.offsetMin = Vector2.zero;
            modalSafeContentRect.offsetMax = Vector2.zero;
            modalSafeContentRect.pivot = new Vector2(0.5f, 0.5f);
            ModalSafeContent modalSafeContentComp = modalSafeContentGO.AddComponent<ModalSafeContent>();
            modalSafeContentComp.ApplySafeArea();

            ModalCoordinator modalCoordinator = canvasGO.AddComponent<ModalCoordinator>();
            modalCoordinator.SetReferences(modalLayerGO, modalBackdropComp);
            SerializedObject coordSO = new SerializedObject(modalCoordinator);
            coordSO.Update();
            coordSO.FindProperty("modalLayer").objectReferenceValue = modalLayerGO;
            coordSO.FindProperty("backdrop").objectReferenceValue = modalBackdropComp;
            coordSO.ApplyModifiedPropertiesWithoutUndo();

            // ==========================================
            // TOP HEADER & LEVEL DISPLAY (Framed Wuxia Header)
            // ==========================================
            GameObject headerPanel = CreateWuxiaPanel(contentAreaGO.transform, "TopHeader", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -55), new Vector2(1020, 96), UIStyleConfig.PanelBackgroundDark, UIStyleConfig.BorderGold);
            TextMeshProUGUI titleText = CreateUIText(headerPanel.transform, "TitleText", "PROTOTYPE 05", 24, TextAlignmentOptions.Center, UIStyleConfig.GoldAccent);
            titleText.rectTransform.anchoredPosition = new Vector2(0, 20);

            TextMeshProUGUI levelTmp = CreateUIText(headerPanel.transform, "LevelText", "HERO LV. 1", 16, TextAlignmentOptions.Left, Color.white);
            levelTmp.rectTransform.sizeDelta = new Vector2(240f, 30f);
            levelTmp.rectTransform.anchoredPosition = new Vector2(-360, -18);

            TextMeshProUGUI rankTitleTmp = CreateUIText(headerPanel.transform, "RankTitleText", "Novice Disciple", 16, TextAlignmentOptions.Right, UIStyleConfig.GoldAccent);
            rankTitleTmp.rectTransform.sizeDelta = new Vector2(240f, 30f);
            rankTitleTmp.rectTransform.anchoredPosition = new Vector2(360, -18);

            var (expBarGO, expFillImg, expTmp) = CreateWuxiaBar(headerPanel.transform, "ExpBar", new Vector2(0, -18), new Vector2(360, 22), UIStyleConfig.CyanHighlight, UIStyleConfig.BorderBronze);

            // Integrated Header Resource Badges
            TextMeshProUGUI headerGoldTmp = CreateUIText(headerPanel.transform, "HeaderGold", "0 G", 13, TextAlignmentOptions.Right, Color.yellow);
            headerGoldTmp.rectTransform.sizeDelta = new Vector2(180f, 26f);
            headerGoldTmp.rectTransform.anchoredPosition = new Vector2(380, 20);

            HeaderGoldHUDUI goldHUD = headerGoldTmp.gameObject.AddComponent<HeaderGoldHUDUI>();
            SerializedObject goldHUDSO = new SerializedObject(goldHUD);
            goldHUDSO.Update();
            goldHUDSO.FindProperty("goldText").objectReferenceValue = headerGoldTmp;
            goldHUDSO.ApplyModifiedPropertiesWithoutUndo();
            goldHUD.SetTargetText(headerGoldTmp);

            TextMeshProUGUI headerMatTmp = CreateUIText(headerPanel.transform, "HeaderMaterial", "10 MAT", 13, TextAlignmentOptions.Left, Color.cyan);
            headerMatTmp.rectTransform.sizeDelta = new Vector2(180f, 26f);
            headerMatTmp.rectTransform.anchoredPosition = new Vector2(-380, 20);

            // Level Up Notification Overlay Text
            TextMeshProUGUI levelUpNotifTmp = CreateUIText(contentAreaGO.transform, "LevelUpNotification", "LEVEL UP!", 24, TextAlignmentOptions.Center, Color.yellow);
            levelUpNotifTmp.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            levelUpNotifTmp.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            levelUpNotifTmp.rectTransform.anchoredPosition = new Vector2(0, -110);
            levelUpNotifTmp.fontStyle = FontStyles.Bold;
            levelUpNotifTmp.gameObject.SetActive(false);

            // ==========================================
            // SUB-HEADER QUICK TOGGLE BUTTONS
            // ==========================================
            Button lootTierToggleBtn = CreateUIButton(contentAreaGO.transform, "LootTierToggleButton", "CẤP RƠI", new Vector2(-130, -118), new Vector2(230, 32));
            lootTierToggleBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
            lootTierToggleBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
            lootTierToggleBtn.GetComponent<Image>().color = UIStyleConfig.ButtonPrimary;

            Button titleBkToggleBtn = CreateUIButton(contentAreaGO.transform, "TitleBreakthroughToggleBtn", "DANH HIỆU", new Vector2(130, -118), new Vector2(230, 32));
            titleBkToggleBtn.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1f);
            titleBkToggleBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1f);
            titleBkToggleBtn.GetComponent<Image>().color = UIStyleConfig.ButtonGold;

            // ==========================================
            // MONSTER COMBAT UI (Upper Combat Area - Framed Wuxia Card)
            // ==========================================
            GameObject monsterUIGO = CreateWuxiaPanel(contentAreaGO.transform, "MonsterUI", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -225), new Vector2(1000, 155), UIStyleConfig.PanelBackgroundDark, UIStyleConfig.BorderBronze);
            var monsterLabelTmp = CreateUIText(monsterUIGO.transform, "MonsterLabel", "YÊU THÚ - HOANG DÃ", 18, TextAlignmentOptions.Left, UIStyleConfig.HealthRed);
            monsterLabelTmp.rectTransform.sizeDelta = new Vector2(400f, 32f);
            monsterLabelTmp.rectTransform.anchoredPosition = new Vector2(-260, 48);

            var monsterTierTmp = CreateUIText(monsterUIGO.transform, "MonsterTier", "CẤP 1 • HUYNH TRƯỞNG", 14, TextAlignmentOptions.Right, UIStyleConfig.TextSecondary);
            monsterTierTmp.rectTransform.sizeDelta = new Vector2(400f, 32f);
            monsterTierTmp.rectTransform.anchoredPosition = new Vector2(260, 48);

            var (monsterHpBarGO, monsterHpFill, monsterHpTmp) = CreateWuxiaBar(monsterUIGO.transform, "MonsterHPBar", new Vector2(0, 16), new Vector2(940, 32), UIStyleConfig.HealthRed, UIStyleConfig.BorderBronze);
            HealthBarUI monsterHpUI = monsterHpBarGO.AddComponent<HealthBarUI>();
            monsterHpUI.SetReferences(monsterHpFill, monsterHpTmp, EntityType.Monster);
            monsterHpUI.BindEntity(monsterComp);

            SerializedObject monsterHpSO = new SerializedObject(monsterHpUI);
            monsterHpSO.Update();
            monsterHpSO.FindProperty("fillImage").objectReferenceValue = monsterHpFill;
            monsterHpSO.FindProperty("hpText").objectReferenceValue = monsterHpTmp;
            monsterHpSO.FindProperty("boundEntityType").enumValueIndex = (int)EntityType.Monster;
            monsterHpSO.FindProperty("targetEntity").objectReferenceValue = monsterComp;
            monsterHpSO.ApplyModifiedPropertiesWithoutUndo();

            var (monsterShieldBarGO, monsterShieldFill, monsterShieldTmp) = CreateWuxiaBar(monsterUIGO.transform, "MonsterShieldBar", new Vector2(0, -18), new Vector2(940, 22), UIStyleConfig.ShieldBlue, UIStyleConfig.CyanHighlight);
            monsterShieldBarGO.SetActive(false);

            // Monster Status Icon Row
            GameObject monsterStatusRowGO = CreateUIRect(monsterUIGO.transform, "MonsterStatusRow", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 14), new Vector2(940, 22));
            HorizontalLayoutGroup mHlg = monsterStatusRowGO.AddComponent<HorizontalLayoutGroup>();
            mHlg.childAlignment = TextAnchor.MiddleCenter;
            mHlg.spacing = 6f;
            StatusIconRowUI monsterStatusUI = monsterStatusRowGO.AddComponent<StatusIconRowUI>();
            monsterStatusUI.BindEntity(monsterComp);
            SerializedObject monsterStatusSO = new SerializedObject(monsterStatusUI);
            monsterStatusSO.Update();
            monsterStatusSO.FindProperty("targetEntity").objectReferenceValue = monsterComp;
            monsterStatusSO.FindProperty("boundEntityType").enumValueIndex = (int)EntityType.Monster;
            monsterStatusSO.ApplyModifiedPropertiesWithoutUndo();

            // ==========================================
            // COMPANION PRESENTATION (Upper Left Combat Viewport)
            // ==========================================
            GameObject compHUDGO = CreateWuxiaPanel(contentAreaGO.transform, "CompanionHUD", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(140, 160), new Vector2(240, 80), UIStyleConfig.PanelBackgroundMedium, UIStyleConfig.BorderBronze);
            TextMeshProUGUI compNameTmp = CreateUIText(compHUDGO.transform, "Name", "ĐỒNG ĐỘI", 14, TextAlignmentOptions.Center, UIStyleConfig.GoldAccent);
            compNameTmp.rectTransform.sizeDelta = new Vector2(210f, 24f);
            compNameTmp.rectTransform.anchoredPosition = new Vector2(0, 22);
            var (compHpBarGO, compHpFill, compHpTmp) = CreateWuxiaBar(compHUDGO.transform, "HPBar", new Vector2(0, -6), new Vector2(210, 18), UIStyleConfig.HealthJade, UIStyleConfig.BorderBronze);
            TextMeshProUGUI compStatusTmp = CreateUIText(compHUDGO.transform, "Status", "CHIẾN ĐẤU", 12, TextAlignmentOptions.Center, UIStyleConfig.StatusBuff);
            compStatusTmp.rectTransform.sizeDelta = new Vector2(210f, 20f);
            compStatusTmp.rectTransform.anchoredPosition = new Vector2(0, -28);
            CompanionHUDUI compUI = compHUDGO.AddComponent<CompanionHUDUI>();
            compUI.SetReferences(compHUDGO, compNameTmp, compHpFill, compHpTmp, compStatusTmp);
            SerializedObject compSO = new SerializedObject(compUI);
            compSO.Update();
            compSO.FindProperty("cardRoot").objectReferenceValue = compHUDGO;
            compSO.FindProperty("companionNameText").objectReferenceValue = compNameTmp;
            compSO.FindProperty("hpFill").objectReferenceValue = compHpFill;
            compSO.FindProperty("hpText").objectReferenceValue = compHpTmp;
            compSO.FindProperty("statusText").objectReferenceValue = compStatusTmp;
            compSO.ApplyModifiedPropertiesWithoutUndo();
            compHUDGO.SetActive(false); // Automatically shown if a companion spawns

            // ==========================================
            // COMBAT VIEWPORT BOUNDS (Non-visual Layout Region for Combat Presentation)
            // ==========================================
            GameObject combatBoundsGO = new GameObject("CombatViewportBounds");
            combatBoundsGO.transform.SetParent(contentAreaGO.transform, false);
            RectTransform combatBoundsRect = combatBoundsGO.AddComponent<RectTransform>();
            combatBoundsRect.anchorMin = new Vector2(0f, 0f);
            combatBoundsRect.anchorMax = new Vector2(1f, 1f);
            combatBoundsRect.offsetMin = new Vector2(0f, 590f); // Ends above HeroCombatHUD (yMax = 587.5)
            combatBoundsRect.offsetMax = new Vector2(0f, 0f);

            // ==========================================
            // MIDDLE ZONE: EQUIPMENT VIEW CONTAINER (Tabs-Switchable, Hidden on Combat View)
            // ==========================================
            GameObject equipViewContainer = CreateUIRect(contentAreaGO.transform, "EquipmentViewContainer", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            // ==========================================
            // CÔNG PHÁP SYSTEM CONTAINER (Dedicated Main Content Area System Screen)
            // ==========================================
            GameObject congPhapContainer = CreateUIRect(contentAreaGO.transform, "CongPhapContainer", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image cpBg = congPhapContainer.AddComponent<Image>();
            cpBg.sprite = defaultSprite;
            cpBg.color = new Color(0.06f, 0.08f, 0.14f, 1f);
            cpBg.raycastTarget = true;
            congPhapContainer.SetActive(false);

            // Left Column: EquipmentStatsPanel (Upper)
            GameObject equipStatsPanel = CreateUIRect(equipViewContainer.transform, "EquipmentStatsPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250, 80), new Vector2(480, 250));
            CreateUIImage(equipStatsPanel.transform, "Bg", new Color(0.04f, 0.06f, 0.1f, 0.92f));

            TextMeshProUGUI heroStatsTmp = CreateUIText(equipStatsPanel.transform, "HeroStats", "HERO TOTAL STATS\nHP: 1000\nATK: 100\nDEF: 20", 14, TextAlignmentOptions.Left, Color.white);
            heroStatsTmp.rectTransform.anchoredPosition = new Vector2(15, 0);

            TextMeshProUGUI helmetTmp = CreateUIText(equipStatsPanel.transform, "HelmetSlot", "Helmet: [Empty]", 12, TextAlignmentOptions.Left, Color.gray);
            helmetTmp.rectTransform.anchoredPosition = new Vector2(250, 85);

            TextMeshProUGUI weaponTmp = CreateUIText(equipStatsPanel.transform, "WeaponSlot", "Weapon: [Empty]", 12, TextAlignmentOptions.Left, Color.gray);
            weaponTmp.rectTransform.anchoredPosition = new Vector2(250, 60);

            TextMeshProUGUI armorTmp = CreateUIText(equipStatsPanel.transform, "ArmorSlot", "Armor: [Empty]", 12, TextAlignmentOptions.Left, Color.gray);
            armorTmp.rectTransform.anchoredPosition = new Vector2(250, 35);

            TextMeshProUGUI glovesTmp = CreateUIText(equipStatsPanel.transform, "GlovesSlot", "Gloves: [Empty]", 12, TextAlignmentOptions.Left, Color.gray);
            glovesTmp.rectTransform.anchoredPosition = new Vector2(250, 10);

            TextMeshProUGUI bootsTmp = CreateUIText(equipStatsPanel.transform, "BootsSlot", "Boots: [Empty]", 12, TextAlignmentOptions.Left, Color.gray);
            bootsTmp.rectTransform.anchoredPosition = new Vector2(250, -15);

            TextMeshProUGUI ringTmp = CreateUIText(equipStatsPanel.transform, "RingSlot", "Ring: [Empty]", 12, TextAlignmentOptions.Left, Color.gray);
            ringTmp.rectTransform.anchoredPosition = new Vector2(250, -40);

            TextMeshProUGUI necklaceTmp = CreateUIText(equipStatsPanel.transform, "NecklaceSlot", "Necklace: [Empty]", 12, TextAlignmentOptions.Left, Color.gray);
            necklaceTmp.rectTransform.anchoredPosition = new Vector2(250, -65);

            TextMeshProUGUI accTmp = CreateUIText(equipStatsPanel.transform, "AccessorySlot", "Accessory: [Empty]", 12, TextAlignmentOptions.Left, Color.gray);
            accTmp.rectTransform.anchoredPosition = new Vector2(250, -90);

            // Left Column: EquipmentDropPanel (Lower)
            GameObject dropPanel = CreateUIRect(equipViewContainer.transform, "EquipmentDropPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-250, -185), new Vector2(480, 260));
            CreateUIImage(dropPanel.transform, "Bg", new Color(0.05f, 0.08f, 0.12f, 0.92f));

            CreateUIText(dropPanel.transform, "Header", "--- SELECTED ITEM DETAILS ---", 15, TextAlignmentOptions.Center, UIStyleConfig.GoldAccent).rectTransform.anchoredPosition = new Vector2(0, 105);
            TextMeshProUGUI dropNameTmp = CreateUIText(dropPanel.transform, "ItemName", "Item Name: -", 14, TextAlignmentOptions.Left, Color.white);
            dropNameTmp.rectTransform.anchoredPosition = new Vector2(15, 80);

            TextMeshProUGUI dropTypeTmp = CreateUIText(dropPanel.transform, "ItemType", "Equipment Type: -", 14, TextAlignmentOptions.Left, Color.white);
            dropTypeTmp.rectTransform.anchoredPosition = new Vector2(15, 58);

            TextMeshProUGUI dropLevelTmp = CreateUIText(dropPanel.transform, "ItemLevel", "Equipment Level: -", 14, TextAlignmentOptions.Left, Color.white);
            dropLevelTmp.rectTransform.anchoredPosition = new Vector2(15, 36);

            TextMeshProUGUI dropRarityTmp = CreateUIText(dropPanel.transform, "Rarity", "Rarity: -", 14, TextAlignmentOptions.Left, UIStyleConfig.CyanHighlight);
            dropRarityTmp.rectTransform.anchoredPosition = new Vector2(15, 14);

            TextMeshProUGUI statusTmp = CreateUIText(dropPanel.transform, "Status", "Status: NOT EQUIPPED", 14, TextAlignmentOptions.Left, Color.yellow);
            statusTmp.rectTransform.anchoredPosition = new Vector2(15, -8);

            TextMeshProUGUI dropAffixesTmp = CreateUIText(dropPanel.transform, "Affixes", "Affixes:\n  (None)", 12, TextAlignmentOptions.Left, Color.white);
            dropAffixesTmp.rectTransform.anchoredPosition = new Vector2(15, -55);

            TextMeshProUGUI dropCountTmp = CreateUIText(dropPanel.transform, "InvCount", "Inventory: 0 item(s)", 14, TextAlignmentOptions.Center, Color.yellow);
            dropCountTmp.rectTransform.anchoredPosition = new Vector2(0, -105);

            // Right Column: ComparisonPanel (Upper)
            GameObject compPanel = CreateUIRect(equipViewContainer.transform, "ComparisonPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250, 115), new Vector2(480, 180));
            CreateUIImage(compPanel.transform, "Bg", new Color(0.04f, 0.06f, 0.1f, 0.92f));
            TextMeshProUGUI compTmp = CreateUIText(compPanel.transform, "ComparisonText", "ITEM COMPARISON:\nSelect an unequipped item to compare.", 13, TextAlignmentOptions.Left, Color.white);
            compTmp.rectTransform.anchoredPosition = new Vector2(10, 0);

            // Right Column: UpgradePanel (Lower)
            GameObject upgPanel = CreateUIRect(equipViewContainer.transform, "UpgradePanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250, -85), new Vector2(480, 180));
            CreateUIImage(upgPanel.transform, "Bg", new Color(0.06f, 0.05f, 0.12f, 0.92f));
            TextMeshProUGUI upgPreviewTmp = CreateUIText(upgPanel.transform, "UpgradePreviewText", "UPGRADE PREVIEW:\nSelect an item to view upgrade costs.", 13, TextAlignmentOptions.Left, new Color(0.9f, 0.8f, 1f));
            upgPreviewTmp.rectTransform.anchoredPosition = new Vector2(10, 0);

            // Inactive by default on Combat screen so Battle Viewport is 100% visible
            equipViewContainer.SetActive(false);

            // ==========================================
            // HERO COMBAT HUD (Lower Combat Area - Framed Wuxia Card)
            // Contains HeroVitalsRegion (left) and NewEquipmentArea (right)
            // ==========================================
            GameObject heroCombatHUDGO = CreateUIRect(contentAreaGO.transform, "HeroCombatHUD", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 480), new Vector2(1000, 215));

            // 1. LEFT REGION: HeroVitalsRegion (width ~520)
            GameObject vitalsGO = CreateWuxiaPanel(heroCombatHUDGO.transform, "HeroVitalsRegion", new Vector2(0f, 0f), new Vector2(0.53f, 1f), Vector2.zero, Vector2.zero, UIStyleConfig.PanelBackgroundDark, UIStyleConfig.BorderGold);
            RectTransform vitalsRect = vitalsGO.GetComponent<RectTransform>();
            vitalsRect.offsetMin = new Vector2(0f, 0f);
            vitalsRect.offsetMax = new Vector2(-10f, 0f);

            // Backward compatibility marker: child named "HeroUI" for FindInScene("HeroUI")
            GameObject heroUIMarker = new GameObject("HeroUI");
            heroUIMarker.transform.SetParent(vitalsGO.transform, false);

            var heroLabelTmp = CreateUIText(vitalsGO.transform, "HeroLabel", "ANH HÙNG", 18, TextAlignmentOptions.Left, UIStyleConfig.GoldAccent);
            heroLabelTmp.rectTransform.anchorMin = new Vector2(0f, 1f);
            heroLabelTmp.rectTransform.anchorMax = new Vector2(0f, 1f);
            heroLabelTmp.rectTransform.pivot = new Vector2(0f, 1f);
            heroLabelTmp.rectTransform.sizeDelta = new Vector2(160f, 28f);
            heroLabelTmp.rectTransform.anchoredPosition = new Vector2(20, -12);

            // Hero Status Icon Row
            GameObject heroStatusRowGO = CreateUIRect(vitalsGO.transform, "HeroStatusRow", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-15, -24), new Vector2(300, 22));
            heroStatusRowGO.GetComponent<RectTransform>().pivot = new Vector2(1f, 0.5f);
            HorizontalLayoutGroup hHlg = heroStatusRowGO.AddComponent<HorizontalLayoutGroup>();
            hHlg.childAlignment = TextAnchor.MiddleRight;
            hHlg.spacing = 6f;
            StatusIconRowUI heroStatusUI = heroStatusRowGO.AddComponent<StatusIconRowUI>();
            heroStatusUI.BindEntity(heroComp);
            SerializedObject heroStatusSO = new SerializedObject(heroStatusUI);
            heroStatusSO.Update();
            heroStatusSO.FindProperty("targetEntity").objectReferenceValue = heroComp;
            heroStatusSO.FindProperty("boundEntityType").enumValueIndex = (int)EntityType.Hero;
            heroStatusSO.ApplyModifiedPropertiesWithoutUndo();

            // Compact Bars inside HeroVitalsRegion (~500 width)
            var (heroHpBarGO, heroHpFill, heroHpTmp) = CreateWuxiaBar(vitalsGO.transform, "HeroHPBar", new Vector2(0, 26), new Vector2(500, 30), UIStyleConfig.HealthJade, UIStyleConfig.BorderBronze);
            HealthBarUI heroHpUI = heroHpBarGO.AddComponent<HealthBarUI>();
            heroHpUI.SetReferences(heroHpFill, heroHpTmp, EntityType.Hero);
            heroHpUI.BindEntity(heroComp);

            SerializedObject heroHpSO = new SerializedObject(heroHpUI);
            heroHpSO.Update();
            heroHpSO.FindProperty("fillImage").objectReferenceValue = heroHpFill;
            heroHpSO.FindProperty("hpText").objectReferenceValue = heroHpTmp;
            heroHpSO.FindProperty("boundEntityType").enumValueIndex = (int)EntityType.Hero;
            heroHpSO.FindProperty("targetEntity").objectReferenceValue = heroComp;
            heroHpSO.ApplyModifiedPropertiesWithoutUndo();

            var (heroShieldBarGO, heroShieldFill, heroShieldTmp) = CreateWuxiaBar(vitalsGO.transform, "HeroShieldBar", new Vector2(0, -3), new Vector2(500, 20), UIStyleConfig.ShieldBlue, UIStyleConfig.CyanHighlight);
            heroShieldBarGO.SetActive(false);

            var (heroRageBarGO, heroRageFill, heroRageTmp) = CreateWuxiaBar(vitalsGO.transform, "HeroRageBar", new Vector2(0, -30), new Vector2(500, 22), UIStyleConfig.RageOrange, UIStyleConfig.BorderBronze);
            RageBarUI heroRageUI = heroRageBarGO.AddComponent<RageBarUI>();
            heroRageUI.SetReferences(heroRageFill, heroRageTmp);
            heroRageUI.BindEntity(heroComp);

            SerializedObject heroRageSO = new SerializedObject(heroRageUI);
            heroRageSO.Update();
            heroRageSO.FindProperty("fillImage").objectReferenceValue = heroRageFill;
            heroRageSO.FindProperty("rageText").objectReferenceValue = heroRageTmp;
            heroRageSO.FindProperty("targetEntity").objectReferenceValue = heroComp;
            heroRageSO.ApplyModifiedPropertiesWithoutUndo();

            // Hero Cast Bar (P07.9 Phase 5.3)
            var (heroCastBarGO, heroCastFill, heroCastTmp) = CreateWuxiaBar(vitalsGO.transform, "HeroCastBar", new Vector2(0, -60), new Vector2(500, 22), UIStyleConfig.CastCyan, UIStyleConfig.CyanHighlight);
            heroCastBarGO.SetActive(false);

            GameObject interruptTxtGO = CreateUIRect(heroCastBarGO.transform, "InterruptText", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 22));
            TextMeshProUGUI heroInterruptTmp = interruptTxtGO.AddComponent<TextMeshProUGUI>();
            heroInterruptTmp.fontSize = 12;
            heroInterruptTmp.fontStyle = FontStyles.Bold;
            heroInterruptTmp.alignment = TextAlignmentOptions.Center;
            heroInterruptTmp.color = new Color(1f, 0.3f, 0.3f);
            interruptTxtGO.SetActive(false);

            CastBarUI heroCastUI = vitalsGO.AddComponent<CastBarUI>();
            heroCastUI.SetReferences(heroCastBarGO, heroCastFill, heroCastTmp, heroInterruptTmp, heroComp, EntityType.Hero);

            SerializedObject heroCastSO = new SerializedObject(heroCastUI);
            heroCastSO.Update();
            heroCastSO.FindProperty("rootObject").objectReferenceValue = heroCastBarGO;
            heroCastSO.FindProperty("fillImage").objectReferenceValue = heroCastFill;
            heroCastSO.FindProperty("castText").objectReferenceValue = heroCastTmp;
            heroCastSO.FindProperty("interruptText").objectReferenceValue = heroInterruptTmp;
            heroCastSO.FindProperty("targetEntity").objectReferenceValue = heroComp;
            heroCastSO.FindProperty("boundEntityType").enumValueIndex = (int)EntityType.Hero;
            heroCastSO.ApplyModifiedPropertiesWithoutUndo();

            // 2. RIGHT REGION: EquipmentLoadoutArea (width ~440)
            GameObject newEquipGO = CreateWuxiaPanel(heroCombatHUDGO.transform, "NewEquipmentArea", new Vector2(0.55f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, UIStyleConfig.PanelBackgroundDark, UIStyleConfig.BorderBronze);
            RectTransform newEquipRect = newEquipGO.GetComponent<RectTransform>();
            newEquipRect.offsetMin = new Vector2(10f, 0f);
            newEquipRect.offsetMax = new Vector2(0f, 0f);

            var loadoutTitleTmp = CreateUIText(newEquipGO.transform, "Title", "TRANG BỊ ĐANG DÙNG", 13, TextAlignmentOptions.Center, UIStyleConfig.GoldAccent);
            loadoutTitleTmp.fontStyle = FontStyles.Bold;
            loadoutTitleTmp.rectTransform.anchorMin = new Vector2(0f, 1f);
            loadoutTitleTmp.rectTransform.anchorMax = new Vector2(1f, 1f);
            loadoutTitleTmp.rectTransform.pivot = new Vector2(0.5f, 1f);
            loadoutTitleTmp.rectTransform.sizeDelta = new Vector2(0f, 22f);
            loadoutTitleTmp.rectTransform.anchoredPosition = new Vector2(0, -6);

            // Title Separator
            GameObject eqSepGO = CreateUIRect(newEquipGO.transform, "Separator", new Vector2(0.05f, 1f), new Vector2(0.95f, 1f), new Vector2(0, -28), new Vector2(0, 2));
            CreateUIImage(eqSepGO.transform, "Line", UIStyleConfig.BorderBronzeDark);

            // Grid Container for 12 permanent slots (6 columns x 2 rows)
            GameObject gridContainer = CreateUIRect(newEquipGO.transform, "GridContainer", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0, -10), new Vector2(-16, -38));

            List<EquipmentSlotView> slotViews = new List<EquipmentSlotView>();
            EquipmentSlotType[] supportedSlots = HeroEquipmentLoadoutUI.SupportedSlots;
            float colWidth = 66f;
            float rowHeight = 78f;

            for (int i = 0; i < supportedSlots.Length; i++)
            {
                EquipmentSlotType slot = supportedSlots[i];
                int col = i % 6;
                int row = i / 6;
                float posX = -180f + col * 72f;
                float posY = row == 0 ? 43f : -43f;

                GameObject slotGO = CreateUIRect(gridContainer.transform, $"Slot_{slot}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(posX, posY), new Vector2(colWidth, rowHeight));
                Image slotBg = CreateUIImage(slotGO.transform, "Bg", new Color(0.08f, 0.07f, 0.12f, 0.9f));
                Image slotBorder = CreateUIImage(slotGO.transform, "Border", UIStyleConfig.BorderBronzeDark);

                TextMeshProUGUI slotLabel = CreateUIText(slotGO.transform, "TypeLabel", HeroEquipmentLoadoutUI.GetSlotShortLabel(slot), 10, TextAlignmentOptions.Center, UIStyleConfig.GoldAccent);
                slotLabel.fontStyle = FontStyles.Bold;
                slotLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
                slotLabel.rectTransform.anchorMax = new Vector2(1f, 1f);
                slotLabel.rectTransform.pivot = new Vector2(0.5f, 1f);
                slotLabel.rectTransform.sizeDelta = new Vector2(colWidth, 16f);
                slotLabel.rectTransform.anchoredPosition = new Vector2(0, -3);

                TextMeshProUGUI symbolTmp = CreateUIText(slotGO.transform, "Symbol", HeroEquipmentLoadoutUI.GetSlotSymbolGlyph(slot), 18, TextAlignmentOptions.Center, new Color(0.5f, 0.5f, 0.5f, 0.6f));
                symbolTmp.rectTransform.anchorMin = Vector2.zero;
                symbolTmp.rectTransform.anchorMax = Vector2.one;
                symbolTmp.rectTransform.offsetMin = Vector2.zero;
                symbolTmp.rectTransform.offsetMax = Vector2.zero;

                GameObject emptyInd = CreateUIRect(slotGO.transform, "EmptyInd", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(20, 20));
                CreateUIImage(emptyInd.transform, "Dot", new Color(0.3f, 0.3f, 0.35f, 0.4f));

                GameObject lvlBadge = CreateUIRect(slotGO.transform, "LevelBadge", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0, 10), new Vector2(0, 16));
                CreateUIImage(lvlBadge.transform, "BadgeBg", new Color(0.1f, 0.08f, 0.05f, 0.85f));
                TextMeshProUGUI lvlTmp = CreateUIText(lvlBadge.transform, "LevelText", "Lv.1", 10, TextAlignmentOptions.Center, Color.white);
                lvlTmp.fontStyle = FontStyles.Bold;
                lvlTmp.rectTransform.anchorMin = Vector2.zero;
                lvlTmp.rectTransform.anchorMax = Vector2.one;
                lvlTmp.rectTransform.offsetMin = Vector2.zero;
                lvlTmp.rectTransform.offsetMax = Vector2.zero;
                lvlBadge.SetActive(false);

                EquipmentSlotView view = new EquipmentSlotView();
                view.BindReferences(slot, slotGO, slotBg, slotBorder, slotLabel, symbolTmp, lvlBadge, lvlTmp, emptyInd);
                view.SetEmpty(UIStyleConfig.BorderBronzeDark, new Color(0.08f, 0.07f, 0.12f, 0.9f));
                slotViews.Add(view);
            }

            HeroEquipmentLoadoutUI loadoutComp = newEquipGO.AddComponent<HeroEquipmentLoadoutUI>();
            loadoutComp.SetReferences(slotViews);

            // ==========================================
            // SKILL / ACTION AREA (Combat Action Cluster)
            // ==========================================
            GameObject skillBarGO = CreateWuxiaPanel(contentAreaGO.transform, "SkillBarUI", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 245), new Vector2(1000, 200), UIStyleConfig.PanelBackgroundDark, UIStyleConfig.BorderBronze);
            SkillBarUI skillBarComp = skillBarGO.AddComponent<SkillBarUI>();

            var s1 = CreateSkillButton(skillBarGO.transform, "Slot1_NormalAttack", new Vector2(-340, 8), 76f, UIStyleConfig.PanelBackgroundMedium, UIStyleConfig.BorderBronze, "Đánh Thường", "swords");
            var s2 = CreateSkillButton(skillBarGO.transform, "Slot2_Skill", new Vector2(-230, 8), 76f, UIStyleConfig.PanelBackgroundMedium, UIStyleConfig.BorderBronze, "Tuyệt Kỹ", "palm");
            var s3 = CreateSkillButton(skillBarGO.transform, "Slot3_External1", new Vector2(-120, 8), 76f, UIStyleConfig.PanelBackgroundMedium, UIStyleConfig.BorderBronze, "Ngoại Công 1", "blade");
            var s4 = CreateSkillButton(skillBarGO.transform, "Slot4_External2", new Vector2(-10, 8), 76f, UIStyleConfig.PanelBackgroundMedium, UIStyleConfig.BorderBronze, "Ngoại Công 2", "gale");
            var s5 = CreateSkillButton(skillBarGO.transform, "Slot5_Ultimate", new Vector2(145, 14), 108f, new Color(0.18f, 0.12f, 0.06f, 0.95f), UIStyleConfig.BorderGold, "THẦN CÔNG", "dragon", isUltimate: true);

            Button autoBtn = CreateUIButton(skillBarGO.transform, "AutoToggleBtn", "AUTO: TẮT", new Vector2(330, 42), new Vector2(120, 38));
            TextMeshProUGUI autoTxt = autoBtn.GetComponentInChildren<TextMeshProUGUI>();
            autoTxt.fontSize = 14;
            autoBtn.GetComponent<Image>().color = UIStyleConfig.ButtonPrimary;

            Button speedBtn = CreateUIButton(skillBarGO.transform, "SpeedToggleBtn", "1X", new Vector2(330, -18), new Vector2(120, 38));
            TextMeshProUGUI speedTxt = speedBtn.GetComponentInChildren<TextMeshProUGUI>();
            speedTxt.fontSize = 16;
            speedBtn.GetComponent<Image>().color = UIStyleConfig.ButtonSecondary;

            skillBarComp.RegisterSlot(SkillSlotType.NormalAttack, s1.btn, s1.bg, s1.border, s1.cdFill, s1.nameTxt, s1.cdTxt);
            skillBarComp.RegisterSlot(SkillSlotType.Skill, s2.btn, s2.bg, s2.border, s2.cdFill, s2.nameTxt, s2.cdTxt);
            skillBarComp.RegisterSlot(SkillSlotType.ExternalSkill1, s3.btn, s3.bg, s3.border, s3.cdFill, s3.nameTxt, s3.cdTxt);
            skillBarComp.RegisterSlot(SkillSlotType.ExternalSkill2, s4.btn, s4.bg, s4.border, s4.cdFill, s4.nameTxt, s4.cdTxt);
            skillBarComp.RegisterSlot(SkillSlotType.Ultimate, s5.btn, s5.bg, s5.border, s5.cdFill, s5.nameTxt, s5.cdTxt, s5.pulse);
            skillBarComp.SetToggleButtons(autoBtn, autoTxt, speedBtn, speedTxt);
            skillBarComp.BindHero(heroComp);

            // ==========================================
            // DEVELOPER DEBUG CONTROL PANEL (Bottom Center Drawer)
            // ==========================================
            GameObject debugPanel = CreateUIRect(contentAreaGO.transform, "DeveloperDebugPanel", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 95), new Vector2(1040, 150));
            CreateUIImage(debugPanel.transform, "Bg", new Color(0.02f, 0.04f, 0.08f, 0.95f));
            CreateUIText(debugPanel.transform, "DebugHeader", "--- DEVELOPER DEBUG PANEL ---", 13, TextAlignmentOptions.Center, new Color(0.8f, 0.8f, 0.8f)).rectTransform.anchoredPosition = new Vector2(0, 60);

            // Row 1 Debug Buttons
            Button add10Btn = CreateUIButton(debugPanel.transform, "Add10ExpBtn", "+10 EXP", new Vector2(-430, 15), new Vector2(80, 40));
            Button add100Btn = CreateUIButton(debugPanel.transform, "Add100ExpBtn", "+100 EXP", new Vector2(-340, 15), new Vector2(85, 40));
            Button add1000Btn = CreateUIButton(debugPanel.transform, "Add1000ExpBtn", "+1K EXP", new Vector2(-245, 15), new Vector2(85, 40));
            Button bkBtn = CreateUIButton(debugPanel.transform, "BreakthroughBtn", "BREAKTHROUGH", new Vector2(-135, 15), new Vector2(115, 40));
            Button dropBtn = CreateUIButton(debugPanel.transform, "TestDropBtn", "DROP", new Vector2(-25, 15), new Vector2(85, 40));
            Button eqBtn = CreateUIButton(debugPanel.transform, "EquipBtn", "EQUIP", new Vector2(70, 15), new Vector2(85, 40));

            // Row 2 Debug Buttons
            Button uneqBtn = CreateUIButton(debugPanel.transform, "UnequipBtn", "UNEQUIP", new Vector2(-430, -35), new Vector2(80, 40));
            Button upgBtn = CreateUIButton(debugPanel.transform, "UpgradeBtn", "UPGRADE", new Vector2(-340, -35), new Vector2(85, 40));
            Button addGBtn = CreateUIButton(debugPanel.transform, "AddGoldBtn", "+1K G", new Vector2(-245, -35), new Vector2(85, 40));
            Button addMBtn = CreateUIButton(debugPanel.transform, "AddMatBtn", "+5 MAT", new Vector2(-150, -35), new Vector2(85, 40));
            Button drop1000Btn = CreateUIButton(debugPanel.transform, "Test1000DropsBtn", "SIM 1000", new Vector2(-50, -35), new Vector2(95, 40));
            Button resetProgBtn = CreateUIButton(debugPanel.transform, "ResetProgressionBtn", "RESET PROG", new Vector2(65, -35), new Vector2(115, 40));

            TextMeshProUGUI playerResTmp = CreateUIText(debugPanel.transform, "PlayerResText", "Gold: 5,000 | Material: 10", 14, TextAlignmentOptions.Left, Color.yellow);
            playerResTmp.rectTransform.anchoredPosition = new Vector2(-470, 48);

            // Collapsed by default for clean combat presentation
            debugPanel.SetActive(false);

            // Toggle Debug Panel Button (Unobtrusive bottom-right pull tab)
            Button toggleDebugBtn = CreateUIButton(contentAreaGO.transform, "ToggleDebugBtn", "[DEV]", new Vector2(-55, 24), new Vector2(70, 28));
            RectTransform toggleRect = toggleDebugBtn.GetComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(1f, 0f);
            toggleRect.anchorMax = new Vector2(1f, 0f);
            toggleRect.anchoredPosition = new Vector2(-55, 24);
            toggleDebugBtn.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.24f, 0.65f);
            TextMeshProUGUI toggleTxt = toggleDebugBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (toggleTxt != null)
            {
                toggleTxt.fontSize = 11;
                toggleTxt.color = new Color(0.6f, 0.65f, 0.75f, 0.85f);
            }
            toggleDebugBtn.onClick.AddListener(() => debugPanel.SetActive(!debugPanel.activeSelf));

            // Bottom Navigation view switching
            bottomNavComp.SetRoutingContainers(null, congPhapContainer);
            bottomNavComp.OnNavigationSelected += (tabIndex) =>
            {
                if (tabIndex == 0) // Túi Đồ - disabled / not implemented
                {
                    bottomNavComp.SelectPosition(GlobalBottomNavigation.MainHubIndex);
                    return;
                }
                else if (tabIndex == 1) // Công Pháp
                {
                    equipViewContainer.SetActive(false);
                    congPhapContainer.SetActive(true);
                    if (MindMethodUI.Instance != null) MindMethodUI.Instance.ShowPanel();
                }
                else // Main Hub (2) or others
                {
                    equipViewContainer.SetActive(false);
                    congPhapContainer.SetActive(false);
                    if (MindMethodUI.Instance != null) MindMethodUI.Instance.HidePanel();
                }
            };

            progressionUI.SetReferences(levelTmp, expTmp, rankTitleTmp, expFillImg, add10Btn, add100Btn, bkBtn, add1000Btn, levelUpNotifTmp, resetProgBtn);
            SerializedObject progSO = new SerializedObject(progressionUI);
            progSO.Update();
            progSO.FindProperty("levelText").objectReferenceValue = levelTmp;
            progSO.FindProperty("expText").objectReferenceValue = expTmp;
            progSO.FindProperty("titleText").objectReferenceValue = rankTitleTmp;
            progSO.FindProperty("expFillImage").objectReferenceValue = expFillImg;
            progSO.FindProperty("add10ExpBtn").objectReferenceValue = add10Btn;
            progSO.FindProperty("add100ExpBtn").objectReferenceValue = add100Btn;
            progSO.FindProperty("add1000ExpBtn").objectReferenceValue = add1000Btn;
            progSO.FindProperty("breakthroughBtn").objectReferenceValue = bkBtn;
            progSO.FindProperty("levelUpNotificationText").objectReferenceValue = levelUpNotifTmp;
            progSO.FindProperty("resetProgressionBtn").objectReferenceValue = resetProgBtn;
            progSO.ApplyModifiedPropertiesWithoutUndo();

            dropUI.SetReferences(
                dropNameTmp, dropTypeTmp, dropLevelTmp, dropRarityTmp, dropAffixesTmp, dropCountTmp,
                dropBtn, drop1000Btn, eqBtn, uneqBtn, heroStatsTmp,
                helmetTmp, weaponTmp, armorTmp, glovesTmp, bootsTmp, ringTmp, necklaceTmp, accTmp,
                statusTmp, compTmp, upgPreviewTmp, playerResTmp, upgBtn, addGBtn, addMBtn);

            SerializedObject dropUISerialized = new SerializedObject(dropUI);
            dropUISerialized.Update();
            dropUISerialized.FindProperty("itemNameText").objectReferenceValue = dropNameTmp;
            dropUISerialized.FindProperty("itemTypeText").objectReferenceValue = dropTypeTmp;
            dropUISerialized.FindProperty("itemLevelText").objectReferenceValue = dropLevelTmp;
            dropUISerialized.FindProperty("rarityText").objectReferenceValue = dropRarityTmp;
            dropUISerialized.FindProperty("affixesText").objectReferenceValue = dropAffixesTmp;
            dropUISerialized.FindProperty("inventoryCountText").objectReferenceValue = dropCountTmp;
            dropUISerialized.FindProperty("testDropButton").objectReferenceValue = dropBtn;
            dropUISerialized.FindProperty("test1000DropsButton").objectReferenceValue = drop1000Btn;
            dropUISerialized.FindProperty("equipButton").objectReferenceValue = eqBtn;
            dropUISerialized.FindProperty("unequipButton").objectReferenceValue = uneqBtn;
            dropUISerialized.FindProperty("heroStatsText").objectReferenceValue = heroStatsTmp;
            dropUISerialized.FindProperty("helmetSlotText").objectReferenceValue = helmetTmp;
            dropUISerialized.FindProperty("weaponSlotText").objectReferenceValue = weaponTmp;
            dropUISerialized.FindProperty("armorSlotText").objectReferenceValue = armorTmp;
            dropUISerialized.FindProperty("glovesSlotText").objectReferenceValue = glovesTmp;
            dropUISerialized.FindProperty("bootsSlotText").objectReferenceValue = bootsTmp;
            dropUISerialized.FindProperty("ringSlotText").objectReferenceValue = ringTmp;
            dropUISerialized.FindProperty("necklaceSlotText").objectReferenceValue = necklaceTmp;
            dropUISerialized.FindProperty("accessorySlotText").objectReferenceValue = accTmp;
            dropUISerialized.FindProperty("statusText").objectReferenceValue = statusTmp;
            dropUISerialized.FindProperty("comparisonText").objectReferenceValue = compTmp;
            dropUISerialized.FindProperty("upgradePreviewText").objectReferenceValue = upgPreviewTmp;
            dropUISerialized.FindProperty("playerResourcesText").objectReferenceValue = playerResTmp;
            dropUISerialized.FindProperty("upgradeButton").objectReferenceValue = upgBtn;
            dropUISerialized.FindProperty("addGoldButton").objectReferenceValue = addGBtn;
            dropUISerialized.FindProperty("addMaterialButton").objectReferenceValue = addMBtn;
            dropUISerialized.ApplyModifiedPropertiesWithoutUndo();

            // Victory Panel Overlay
            GameObject victoryPanel = CreateUIRect(contentAreaGO.transform, "VictoryPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 320));
            CreateUIImage(victoryPanel.transform, "Bg", new Color(0f, 0f, 0f, 0.90f));
            CreateUIText(victoryPanel.transform, "VictoryText", "VICTORY!", 48, TextAlignmentOptions.Center, Color.green).rectTransform.anchoredPosition = new Vector2(0, 45);
            victoryPanel.SetActive(false);

            // Defeat / Hero Death Panel Overlay
            GameObject defeatPanel = CreateUIRect(contentAreaGO.transform, "DefeatPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 320));
            CreateUIImage(defeatPanel.transform, "Bg", new Color(0f, 0f, 0f, 0.90f));
            TextMeshProUGUI defeatText = CreateUIText(defeatPanel.transform, "DefeatText", "ANH HÙNG ĐÃ GỤC", 40, TextAlignmentOptions.Center, Color.red);
            defeatText.rectTransform.anchoredPosition = new Vector2(0, 45);
            defeatPanel.SetActive(false);

            // Start / Resume Button ("BẮT ĐẦU")
            Button startBtn = CreateUIButton(defeatPanel.transform, "StartButton", "BẮT ĐẦU", new Vector2(0, -40), new Vector2(260, 60));

            // Ensure CongPhapContainer is the topmost sibling inside MainContentArea above all combat siblings
            congPhapContainer.transform.SetAsLastSibling();

            // Loot Decision Panel Overlay (Equipment Power Comparison Modal)
            GameObject lootPanel = CreateUIRect(modalSafeContentGO.transform, "LootDecisionPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 720));
            CreateUIImage(lootPanel.transform, "Bg", new Color(0.04f, 0.06f, 0.12f, 0.98f));

            // 1. Fixed Header Region (Top Region: Height 120)
            GameObject lootHeaderRegion = CreateUIRect(lootPanel.transform, "HeaderRegion", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -60f), new Vector2(0f, 120f));
            GameObject headerGO = CreateUIRect(lootHeaderRegion.transform, "Header", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            TextMeshProUGUI lootHeaderTmp = headerGO.AddComponent<TextMeshProUGUI>();
            lootHeaderTmp.text = "SO SÁNH TRANG BỊ";
            lootHeaderTmp.fontSize = 24;
            lootHeaderTmp.fontStyle = FontStyles.Bold;
            lootHeaderTmp.alignment = TextAlignmentOptions.Center;
            lootHeaderTmp.color = UIStyleConfig.GoldAccent;
            lootHeaderTmp.margin = new Vector4(24f, 20f, 24f, 20f);

            // 2. Sticky Footer Region (Bottom Region: Height 120, Never scrolls off screen)
            GameObject lootFooterRegion = CreateUIRect(lootPanel.transform, "FooterRegion", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 60f), new Vector2(0f, 120f));
            GameObject buttonsGO = CreateUIRect(lootFooterRegion.transform, "Buttons", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            Button lootEquipBtn = CreateUIButton(buttonsGO.transform, "LootEquipButton", "EQUIP", Vector2.zero, new Vector2(0f, 88f));
            RectTransform equipBtnRt = lootEquipBtn.GetComponent<RectTransform>();
            equipBtnRt.anchorMin = new Vector2(0f, 0.5f);
            equipBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
            equipBtnRt.offsetMin = new Vector2(20f, -44f);
            equipBtnRt.offsetMax = new Vector2(-10f, 44f);
            lootEquipBtn.GetComponent<Image>().color = UIStyleConfig.ButtonSuccess;

            Button lootDismantleBtn = CreateUIButton(buttonsGO.transform, "LootDismantleButton", "TÁCH", Vector2.zero, new Vector2(0f, 88f));
            RectTransform disBtnRt = lootDismantleBtn.GetComponent<RectTransform>();
            disBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
            disBtnRt.anchorMax = new Vector2(1f, 0.5f);
            disBtnRt.offsetMin = new Vector2(10f, -44f);
            disBtnRt.offsetMax = new Vector2(-20f, 44f);
            lootDismantleBtn.GetComponent<Image>().color = UIStyleConfig.ButtonDanger;

            // 3. Scrollable Body Container (Strictly bounded between HeaderRegion y=-120 and FooterRegion y=120)
            GameObject lootScrollGO = CreateUIRect(lootPanel.transform, "ModalBodyScrollView", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform lootScrollRt = lootScrollGO.GetComponent<RectTransform>();
            lootScrollRt.offsetMin = new Vector2(16f, 120f);
            lootScrollRt.offsetMax = new Vector2(-16f, -120f);

            ScrollRect lootScroll = lootScrollGO.AddComponent<ScrollRect>();
            lootScroll.horizontal = false;
            lootScroll.vertical = true;
            RectMask2D lootMask = lootScrollGO.AddComponent<RectMask2D>();

            GameObject lootScrollContent = CreateUIRect(lootScrollGO.transform, "ModalBodyContent", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 460f));
            lootScrollContent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            lootScroll.content = lootScrollContent.GetComponent<RectTransform>();
            lootScroll.viewport = lootScrollGO.GetComponent<RectTransform>();

            // ComparisonContent inside Scroll Content (Responsive column split 48.5% / 51.5%)
            GameObject compContentGO = CreateUIRect(lootScrollContent.transform, "ComparisonContent", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform compContentRt = compContentGO.GetComponent<RectTransform>();
            compContentRt.pivot = new Vector2(0.5f, 1f);
            compContentRt.offsetMin = new Vector2(0f, -190f);
            compContentRt.offsetMax = new Vector2(0f, -10f);

            // Left Column: CurrentEquipmentColumn ("ĐANG TRANG BỊ")
            GameObject curColGO = CreateUIRect(compContentGO.transform, "CurrentEquipmentColumn", new Vector2(0f, 0f), new Vector2(0.485f, 1f), Vector2.zero, Vector2.zero);
            CreateUIImage(curColGO.transform, "ColBg", new Color(0.08f, 0.11f, 0.18f, 0.85f));
            GameObject curEqTextGO = CreateUIRect(curColGO.transform, "CurrentEquipped", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform curEqTextRt = curEqTextGO.GetComponent<RectTransform>();
            curEqTextRt.offsetMin = new Vector2(12f, 10f);
            curEqTextRt.offsetMax = new Vector2(-12f, -10f);
            TextMeshProUGUI curEqTmp = curEqTextGO.AddComponent<TextMeshProUGUI>();
            curEqTmp.text = "<color=#FFD700>ĐANG TRANG BỊ</color>\n(TRỐNG)";
            curEqTmp.fontSize = 14;
            curEqTmp.alignment = TextAlignmentOptions.TopLeft;
            curEqTmp.color = Color.white;
            curEqTmp.textWrappingMode = TextWrappingModes.Normal;

            // Right Column: NewEquipmentColumn ("VẬT PHẨM MỚI")
            GameObject newColGO = CreateUIRect(compContentGO.transform, "NewEquipmentColumn", new Vector2(0.515f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            CreateUIImage(newColGO.transform, "ColBg", new Color(0.08f, 0.14f, 0.20f, 0.85f));
            GameObject newDrTextGO = CreateUIRect(newColGO.transform, "NewDrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform newDrTextRt = newDrTextGO.GetComponent<RectTransform>();
            newDrTextRt.offsetMin = new Vector2(12f, 10f);
            newDrTextRt.offsetMax = new Vector2(-12f, -10f);
            TextMeshProUGUI newDrTmp = newDrTextGO.AddComponent<TextMeshProUGUI>();
            newDrTmp.text = "<color=#00FFFF>VẬT PHẨM MỚI</color>\nItem Name";
            newDrTmp.fontSize = 14;
            newDrTmp.alignment = TextAlignmentOptions.TopLeft;
            newDrTmp.color = Color.white;
            newDrTmp.textWrappingMode = TextWrappingModes.Normal;

            // PowerChangeSection inside Scroll Content (Stretches 0% to 100%)
            GameObject pwrSecGO = CreateUIRect(lootScrollContent.transform, "PowerChangeSection", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform pwrSecRt = pwrSecGO.GetComponent<RectTransform>();
            pwrSecRt.pivot = new Vector2(0.5f, 1f);
            pwrSecRt.offsetMin = new Vector2(0f, -265f);
            pwrSecRt.offsetMax = new Vector2(0f, -200f);
            CreateUIImage(pwrSecGO.transform, "SecBg", new Color(0.06f, 0.09f, 0.15f, 0.7f));
            GameObject pwrTextGO = CreateUIRect(pwrSecGO.transform, "PowerChange", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform pwrTextRt = pwrTextGO.GetComponent<RectTransform>();
            pwrTextRt.offsetMin = new Vector2(12f, 4f);
            pwrTextRt.offsetMax = new Vector2(-12f, -4f);
            TextMeshProUGUI pwrChgTmp = pwrTextGO.AddComponent<TextMeshProUGUI>();
            pwrChgTmp.text = "THAY ĐỔI LỰC CHIẾN\nHiện tại: 1,000 -> Sau khi mang: 1,200\nChênh lệch: +200 (+)";
            pwrChgTmp.fontSize = 14;
            pwrChgTmp.alignment = TextAlignmentOptions.Center;
            pwrChgTmp.color = Color.white;
            pwrChgTmp.textWrappingMode = TextWrappingModes.Normal;

            // StatChangesSection inside Scroll Content (Stretches 0% to 100%)
            GameObject statSecGO = CreateUIRect(lootScrollContent.transform, "StatChangesSection", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform statSecRt = statSecGO.GetComponent<RectTransform>();
            statSecRt.pivot = new Vector2(0.5f, 1f);
            statSecRt.offsetMin = new Vector2(0f, -445f);
            statSecRt.offsetMax = new Vector2(0f, -275f);
            CreateUIImage(statSecGO.transform, "SecBg", new Color(0.06f, 0.09f, 0.15f, 0.7f));
            GameObject statTextGO = CreateUIRect(statSecGO.transform, "StatChanges", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform statTextRt = statTextGO.GetComponent<RectTransform>();
            statTextRt.offsetMin = new Vector2(12f, 8f);
            statTextRt.offsetMax = new Vector2(-12f, -8f);
            TextMeshProUGUI statChgTmp = statTextGO.AddComponent<TextMeshProUGUI>();
            statChgTmp.text = "THỐNG KÊ THUỘC TÍNH\nHP: 1000 -> 1120   +120\nATK: 100 -> 125   +25";
            statChgTmp.fontSize = 13;
            statChgTmp.alignment = TextAlignmentOptions.Center;
            statChgTmp.color = Color.white;
            statChgTmp.textWrappingMode = TextWrappingModes.Normal;

            // Legacy elements for full backwards compatibility
            TextMeshProUGUI lootItemNameTmp = CreateUIText(lootPanel.transform, "ItemName", "Item Name", 1, TextAlignmentOptions.Center, Color.clear);
            lootItemNameTmp.rectTransform.sizeDelta = Vector2.zero;
            TextMeshProUGUI lootSlotTmp = CreateUIText(lootPanel.transform, "ItemSlot", "Slot: Weapon", 1, TextAlignmentOptions.Center, Color.clear);
            lootSlotTmp.rectTransform.sizeDelta = Vector2.zero;
            TextMeshProUGUI lootRarityTmp = CreateUIText(lootPanel.transform, "Rarity", "Rarity: Standard", 1, TextAlignmentOptions.Center, Color.clear);
            lootRarityTmp.rectTransform.sizeDelta = Vector2.zero;
            TextMeshProUGUI lootLevelTmp = CreateUIText(lootPanel.transform, "Level", "Level: 1", 1, TextAlignmentOptions.Center, Color.clear);
            lootLevelTmp.rectTransform.sizeDelta = Vector2.zero;
            TextMeshProUGUI lootAffixesTmp = CreateUIText(lootPanel.transform, "Affixes", "", 1, TextAlignmentOptions.Center, Color.clear);
            lootAffixesTmp.rectTransform.sizeDelta = Vector2.zero;
            TextMeshProUGUI lootStatsTmp = CreateUIText(lootPanel.transform, "Stats", "", 1, TextAlignmentOptions.Center, Color.clear);
            lootStatsTmp.rectTransform.sizeDelta = Vector2.zero;

            LootDecisionUI lootUI = canvasGO.AddComponent<LootDecisionUI>();
            lootUI.SetReferences(lootPanel, lootItemNameTmp, lootSlotTmp, lootRarityTmp, lootLevelTmp, lootAffixesTmp, lootStatsTmp, lootEquipBtn, lootDismantleBtn, curEqTmp, newDrTmp, pwrChgTmp, statChgTmp, lootHeaderTmp);
            lootPanel.SetActive(false);

            SerializedObject lootSO = new SerializedObject(lootUI);
            lootSO.Update();
            lootSO.FindProperty("panel").objectReferenceValue = lootPanel;
            lootSO.FindProperty("headerText").objectReferenceValue = lootHeaderTmp;
            lootSO.FindProperty("currentEquippedText").objectReferenceValue = curEqTmp;
            lootSO.FindProperty("newDropText").objectReferenceValue = newDrTmp;
            lootSO.FindProperty("powerChangeText").objectReferenceValue = pwrChgTmp;
            lootSO.FindProperty("statChangesText").objectReferenceValue = statChgTmp;
            lootSO.FindProperty("itemNameText").objectReferenceValue = lootItemNameTmp;
            lootSO.FindProperty("itemSlotText").objectReferenceValue = lootSlotTmp;
            lootSO.FindProperty("rarityText").objectReferenceValue = lootRarityTmp;
            lootSO.FindProperty("itemLevelText").objectReferenceValue = lootLevelTmp;
            lootSO.FindProperty("affixesText").objectReferenceValue = lootAffixesTmp;
            lootSO.FindProperty("statsText").objectReferenceValue = lootStatsTmp;
            lootSO.FindProperty("equipButton").objectReferenceValue = lootEquipBtn;
            lootSO.FindProperty("dismantleButton").objectReferenceValue = lootDismantleBtn;
            lootSO.ApplyModifiedPropertiesWithoutUndo();

            // ==========================================
            // LOOT TIER PROGRESSION PANEL (Portrait Modal)
            // ==========================================
            GameObject lootTierPanel = CreateUIRect(modalSafeContentGO.transform, "LootTierProgressionPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 800));
            CreateUIImage(lootTierPanel.transform, "Bg", new Color(0.04f, 0.06f, 0.12f, 0.98f));

            // 1. Fixed Header Region (Top Region: Height 120)
            GameObject ltHeaderRegion = CreateUIRect(lootTierPanel.transform, "HeaderRegion", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -60f), new Vector2(0f, 120f));

            // Title "CẤP RƠI" centered inside HeaderRegion
            GameObject ltHeaderGO = CreateUIRect(ltHeaderRegion.transform, "Header", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            TextMeshProUGUI ltTitleTmp = ltHeaderGO.AddComponent<TextMeshProUGUI>();
            ltTitleTmp.text = "CẤP RƠI";
            ltTitleTmp.fontSize = 24;
            ltTitleTmp.fontStyle = FontStyles.Bold;
            ltTitleTmp.alignment = TextAlignmentOptions.Center;
            ltTitleTmp.color = UIStyleConfig.GoldAccent;
            ltTitleTmp.margin = new Vector4(112f, 20f, 112f, 20f);

            // Close button: anchored to top-right of the card/header, fully contained inside card bounds
            // Inset 16 reference units from top and right, touch target 88x88
            Button ltCloseBtn = CreateUIButton(ltHeaderRegion.transform, "CloseButton", "X", Vector2.zero, new Vector2(88f, 88f));
            RectTransform ltCloseRt = ltCloseBtn.GetComponent<RectTransform>();
            ltCloseRt.anchorMin = new Vector2(1f, 1f);
            ltCloseRt.anchorMax = new Vector2(1f, 1f);
            ltCloseRt.pivot = new Vector2(1f, 1f);
            ltCloseRt.anchoredPosition = new Vector2(-16f, -16f);
            ltCloseRt.sizeDelta = new Vector2(88f, 88f);
            ltCloseBtn.GetComponent<Image>().color = UIStyleConfig.ButtonDanger;

            // 2. Sticky Footer Region (Bottom Region: Height 120)
            GameObject ltFooterRegion = CreateUIRect(lootTierPanel.transform, "FooterRegion", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 60f), new Vector2(0f, 120f));
            Button ltUpgBtn = CreateUIButton(ltFooterRegion.transform, "UpgradeButton", "TĂNG BẬC", Vector2.zero, new Vector2(300f, 88f));
            RectTransform ltUpgBtnRt = ltUpgBtn.GetComponent<RectTransform>();
            ltUpgBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
            ltUpgBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
            ltUpgBtnRt.anchoredPosition = Vector2.zero;
            ltUpgBtnRt.sizeDelta = new Vector2(300f, 88f);
            ltUpgBtn.GetComponent<Image>().color = UIStyleConfig.ButtonSuccess;
            TextMeshProUGUI ltUpgBtnTmp = ltUpgBtn.GetComponentInChildren<TextMeshProUGUI>();

            // 3. Scrollable Body Container (Strictly bounded between HeaderRegion y=-120 and FooterRegion y=120)
            GameObject ltScrollGO = CreateUIRect(lootTierPanel.transform, "ModalBodyScrollView", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform ltScrollRt = ltScrollGO.GetComponent<RectTransform>();
            ltScrollRt.offsetMin = new Vector2(16f, 120f);
            ltScrollRt.offsetMax = new Vector2(-16f, -120f);

            ScrollRect ltScroll = ltScrollGO.AddComponent<ScrollRect>();
            ltScroll.horizontal = false;
            ltScroll.vertical = true;
            RectMask2D ltMask = ltScrollGO.AddComponent<RectMask2D>();

            GameObject ltScrollContent = CreateUIRect(ltScrollGO.transform, "ModalBodyContent", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 500f));
            ltScrollContent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            ltScroll.content = ltScrollContent.GetComponent<RectTransform>();
            ltScroll.viewport = ltScrollGO.GetComponent<RectTransform>();

            // Two Columns: Current Tier & Next Tier inside Scroll Content (Stretches 0% to 100%)
            GameObject ltColsGO = CreateUIRect(ltScrollContent.transform, "TiersHeader", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform ltColsRt = ltColsGO.GetComponent<RectTransform>();
            ltColsRt.pivot = new Vector2(0.5f, 1f);
            ltColsRt.offsetMin = new Vector2(0f, -40f);
            ltColsRt.offsetMax = new Vector2(0f, -8f);

            GameObject curTierGO = CreateUIRect(ltColsGO.transform, "CurrentTier", new Vector2(0f, 0f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            RectTransform curTierRt = curTierGO.GetComponent<RectTransform>();
            curTierRt.offsetMin = new Vector2(8f, 0f);
            curTierRt.offsetMax = new Vector2(-8f, 0f);
            TextMeshProUGUI curTierTmp = curTierGO.AddComponent<TextMeshProUGUI>();
            curTierTmp.text = "Cấp hiện tại: <color=#00FFFF>1</color>";
            curTierTmp.fontSize = 15;
            curTierTmp.alignment = TextAlignmentOptions.Left;
            curTierTmp.color = Color.white;

            GameObject nextTierGO = CreateUIRect(ltColsGO.transform, "NextTier", new Vector2(0.5f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform nextTierRt = nextTierGO.GetComponent<RectTransform>();
            nextTierRt.offsetMin = new Vector2(8f, 0f);
            nextTierRt.offsetMax = new Vector2(-8f, 0f);
            TextMeshProUGUI nextTierTmp = nextTierGO.AddComponent<TextMeshProUGUI>();
            nextTierTmp.text = "Cấp tiếp theo: <color=#FFD700>2</color>";
            nextTierTmp.fontSize = 15;
            nextTierTmp.alignment = TextAlignmentOptions.Right;
            nextTierTmp.color = Color.white;

            // Rarity Comparison Table inside Scroll Content (Stretches across viewport width)
            GameObject ltTableGO = CreateUIRect(ltScrollContent.transform, "RarityComparisonTable", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform ltTableRt = ltTableGO.GetComponent<RectTransform>();
            ltTableRt.pivot = new Vector2(0.5f, 1f);
            ltTableRt.offsetMin = new Vector2(0f, -360f);
            ltTableRt.offsetMax = new Vector2(0f, -48f);
            CreateUIImage(ltTableGO.transform, "TableBg", new Color(0.07f, 0.10f, 0.16f, 0.8f));

            GameObject ltTableTextGO = CreateUIRect(ltTableGO.transform, "RarityComparisonText", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform ltTableTextRt = ltTableTextGO.GetComponent<RectTransform>();
            ltTableTextRt.offsetMin = new Vector2(16f, 8f);
            ltTableTextRt.offsetMax = new Vector2(-16f, -8f);
            TextMeshProUGUI ltTableTmp = ltTableTextGO.AddComponent<TextMeshProUGUI>();
            ltTableTmp.text = "<b>                      HIỆN TẠI      TIẾP THEO</b>\nThô Sơ               50.0%   ->   35.0%\nThường               25.0%   ->   30.0%";
            ltTableTmp.fontSize = 13;
            ltTableTmp.alignment = TextAlignmentOptions.TopLeft;
            ltTableTmp.color = Color.white;
            ltTableTmp.textWrappingMode = TextWrappingModes.Normal;

            // Progress & Timer Section inside Scroll Content (Stretches 0% to 100%)
            GameObject ltUpgInfoGO = CreateUIRect(ltScrollContent.transform, "UpgradeInfoText", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform ltUpgInfoRt = ltUpgInfoGO.GetComponent<RectTransform>();
            ltUpgInfoRt.pivot = new Vector2(0.5f, 1f);
            ltUpgInfoRt.offsetMin = new Vector2(0f, -396f);
            ltUpgInfoRt.offsetMax = new Vector2(0f, -368f);
            TextMeshProUGUI ltUpgInfoTmp = ltUpgInfoGO.AddComponent<TextMeshProUGUI>();
            ltUpgInfoTmp.text = "Cấp nâng cấp: <color=#00FFFF>Cấp Rơi 1 -> 2</color>";
            ltUpgInfoTmp.fontSize = 14;
            ltUpgInfoTmp.alignment = TextAlignmentOptions.Center;
            ltUpgInfoTmp.color = Color.white;

            GameObject ltTimerGO = CreateUIRect(ltScrollContent.transform, "TimerText", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform ltTimerRt = ltTimerGO.GetComponent<RectTransform>();
            ltTimerRt.pivot = new Vector2(0.5f, 1f);
            ltTimerRt.offsetMin = new Vector2(0f, -428f);
            ltTimerRt.offsetMax = new Vector2(0f, -402f);
            TextMeshProUGUI ltTimerTmp = ltTimerGO.AddComponent<TextMeshProUGUI>();
            ltTimerTmp.text = "Thời gian tăng cấp: 00:15:00";
            ltTimerTmp.fontSize = 14;
            ltTimerTmp.alignment = TextAlignmentOptions.Center;
            ltTimerTmp.color = new Color(0.9f, 0.9f, 0.9f);

            // Progress Bar inside Scroll Content (Stretches with 16px margins)
            GameObject ltBarGO = CreateUIRect(ltScrollContent.transform, "ProgressBar", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform ltBarRt = ltBarGO.GetComponent<RectTransform>();
            ltBarRt.pivot = new Vector2(0.5f, 1f);
            ltBarRt.offsetMin = new Vector2(16f, -468f);
            ltBarRt.offsetMax = new Vector2(-16f, -438f);
            CreateUIImage(ltBarGO.transform, "BarBg", new Color(0.15f, 0.18f, 0.25f, 1f));

            GameObject ltFillGO = CreateUIRect(ltBarGO.transform, "BarFill", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            Image ltFillImg = ltFillGO.AddComponent<Image>();
            ltFillImg.sprite = defaultSprite;
            ltFillImg.type = Image.Type.Filled;
            ltFillImg.fillMethod = Image.FillMethod.Horizontal;
            ltFillImg.fillOrigin = 0;
            ltFillImg.fillAmount = 0.35f;
            ltFillImg.color = UIStyleConfig.CyanHighlight;

            GameObject ltProgNumGO = CreateUIRect(ltBarGO.transform, "ProgressNumbers", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI ltProgNumTmp = ltProgNumGO.AddComponent<TextMeshProUGUI>();
            ltProgNumTmp.text = "890 / 2500";
            ltProgNumTmp.fontSize = 13;
            ltProgNumTmp.fontStyle = FontStyles.Bold;
            ltProgNumTmp.alignment = TextAlignmentOptions.Center;
            ltProgNumTmp.color = Color.white;

            LootTierProgressionUI ltUI = canvasGO.AddComponent<LootTierProgressionUI>();
            ltUI.SetReferences(lootTierPanel, lootTierToggleBtn, ltCloseBtn, ltTitleTmp, curTierTmp, nextTierTmp, ltTableTmp, ltUpgInfoTmp, ltTimerTmp, ltFillImg, ltProgNumTmp, ltUpgBtn, ltUpgBtnTmp);
            lootTierPanel.SetActive(false);

            // ==========================================
            // TITLE BREAKTHROUGH PANEL (Portrait Modal)
            // ==========================================
            GameObject titleBkPanel = CreateUIRect(modalSafeContentGO.transform, "TitleBreakthroughPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 800));
            Image tbkBg = titleBkPanel.AddComponent<Image>();
            tbkBg.sprite = defaultSprite;
            tbkBg.color = new Color(0.06f, 0.08f, 0.12f, 0.96f);

            // 1. Fixed Header Region (Top Region: Height 120)
            GameObject tbkHeaderRegion = CreateUIRect(titleBkPanel.transform, "HeaderRegion", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -60f), new Vector2(0f, 120f));

            // Title Header centered inside HeaderRegion
            GameObject tbkTitleGO = CreateUIRect(tbkHeaderRegion.transform, "TitleText", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            TextMeshProUGUI tbkTitleTmp = tbkTitleGO.AddComponent<TextMeshProUGUI>();
            tbkTitleTmp.text = "ĐỘT PHÁ DANH HIỆU";
            tbkTitleTmp.fontSize = 24;
            tbkTitleTmp.fontStyle = FontStyles.Bold;
            tbkTitleTmp.alignment = TextAlignmentOptions.Center;
            tbkTitleTmp.color = UIStyleConfig.GoldAccent;
            tbkTitleTmp.margin = new Vector4(112f, 20f, 112f, 20f);

            // Close button: anchored top-right, inset 16 reference units, touch target 88x88
            Button tbkCloseBtn = CreateUIButton(tbkHeaderRegion.transform, "CloseButton", "X", Vector2.zero, new Vector2(88f, 88f));
            RectTransform tbkCloseRt = tbkCloseBtn.GetComponent<RectTransform>();
            tbkCloseRt.anchorMin = new Vector2(1f, 1f);
            tbkCloseRt.anchorMax = new Vector2(1f, 1f);
            tbkCloseRt.pivot = new Vector2(1f, 1f);
            tbkCloseRt.anchoredPosition = new Vector2(-16f, -16f);
            tbkCloseRt.sizeDelta = new Vector2(88f, 88f);
            tbkCloseBtn.GetComponent<Image>().color = UIStyleConfig.ButtonDanger;

            // 2. Sticky Footer Region (Bottom Region: Height 120)
            GameObject tbkFooterRegion = CreateUIRect(titleBkPanel.transform, "FooterRegion", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 60f), new Vector2(0f, 120f));
            Button tbkActionBtn = CreateUIButton(tbkFooterRegion.transform, "BreakthroughActionButton", "ĐỘT PHÁ", Vector2.zero, new Vector2(300f, 88f));
            RectTransform tbkActionRt = tbkActionBtn.GetComponent<RectTransform>();
            tbkActionRt.anchorMin = new Vector2(0.5f, 0.5f);
            tbkActionRt.anchorMax = new Vector2(0.5f, 0.5f);
            tbkActionRt.anchoredPosition = Vector2.zero;
            tbkActionRt.sizeDelta = new Vector2(300f, 88f);
            tbkActionBtn.GetComponent<Image>().color = UIStyleConfig.ButtonGold;
            TextMeshProUGUI tbkActionBtnTmp = tbkActionBtn.GetComponentInChildren<TextMeshProUGUI>();

            // 3. Scrollable Body Container (Strictly bounded between HeaderRegion y=-120 and FooterRegion y=120)
            GameObject tbkScrollGO = CreateUIRect(titleBkPanel.transform, "ModalBodyScrollView", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tbkScrollRt = tbkScrollGO.GetComponent<RectTransform>();
            tbkScrollRt.offsetMin = new Vector2(16f, 120f);
            tbkScrollRt.offsetMax = new Vector2(-16f, -120f);

            ScrollRect tbkScroll = tbkScrollGO.AddComponent<ScrollRect>();
            tbkScroll.horizontal = false;
            tbkScroll.vertical = true;
            RectMask2D tbkMask = tbkScrollGO.AddComponent<RectMask2D>();

            GameObject tbkScrollContent = CreateUIRect(tbkScrollGO.transform, "ModalBodyContent", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 520f));
            tbkScrollContent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            tbkScroll.content = tbkScrollContent.GetComponent<RectTransform>();
            tbkScroll.viewport = tbkScrollGO.GetComponent<RectTransform>();

            // Current Title & Next Title (Stretches 0% to 100%)
            GameObject tbkTitlesGO = CreateUIRect(tbkScrollContent.transform, "TitlesRow", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tbkTitlesRt = tbkTitlesGO.GetComponent<RectTransform>();
            tbkTitlesRt.pivot = new Vector2(0.5f, 1f);
            tbkTitlesRt.offsetMin = new Vector2(0f, -40f);
            tbkTitlesRt.offsetMax = new Vector2(0f, -8f);

            GameObject tbkCurTitleGO = CreateUIRect(tbkTitlesGO.transform, "CurrentTitleText", new Vector2(0f, 0f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tbkCurTitleRt = tbkCurTitleGO.GetComponent<RectTransform>();
            tbkCurTitleRt.offsetMin = new Vector2(8f, 0f);
            tbkCurTitleRt.offsetMax = new Vector2(-8f, 0f);
            TextMeshProUGUI tbkCurTitleTmp = tbkCurTitleGO.AddComponent<TextMeshProUGUI>();
            tbkCurTitleTmp.text = "Danh hiệu hiện tại: <color=#00FFFF>[ No Title ]</color>";
            tbkCurTitleTmp.fontSize = 15;
            tbkCurTitleTmp.alignment = TextAlignmentOptions.Center;
            tbkCurTitleTmp.color = Color.white;

            GameObject tbkNextTitleGO = CreateUIRect(tbkTitlesGO.transform, "NextTitleText", new Vector2(0.5f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tbkNextTitleRt = tbkNextTitleGO.GetComponent<RectTransform>();
            tbkNextTitleRt.offsetMin = new Vector2(8f, 0f);
            tbkNextTitleRt.offsetMax = new Vector2(-8f, 0f);
            TextMeshProUGUI tbkNextTitleTmp = tbkNextTitleGO.AddComponent<TextMeshProUGUI>();
            tbkNextTitleTmp.text = "Danh hiệu tiếp theo: <color=#FFD700>[ Novice Disciple ]</color>";
            tbkNextTitleTmp.fontSize = 15;
            tbkNextTitleTmp.alignment = TextAlignmentOptions.Center;
            tbkNextTitleTmp.color = Color.white;

            // Current Level & Level Cap (Stretches 0% to 100%)
            GameObject tbkLevelsGO = CreateUIRect(tbkScrollContent.transform, "LevelsRow", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tbkLevelsRt = tbkLevelsGO.GetComponent<RectTransform>();
            tbkLevelsRt.pivot = new Vector2(0.5f, 1f);
            tbkLevelsRt.offsetMin = new Vector2(0f, -75f);
            tbkLevelsRt.offsetMax = new Vector2(0f, -45f);

            GameObject tbkCurLvlGO = CreateUIRect(tbkLevelsGO.transform, "CurrentLevelText", new Vector2(0f, 0f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tbkCurLvlRt = tbkCurLvlGO.GetComponent<RectTransform>();
            tbkCurLvlRt.offsetMin = new Vector2(8f, 0f);
            tbkCurLvlRt.offsetMax = new Vector2(-8f, 0f);
            TextMeshProUGUI tbkCurLvlTmp = tbkCurLvlGO.AddComponent<TextMeshProUGUI>();
            tbkCurLvlTmp.text = "Cấp hiện tại: <color=#FFFFFF>50</color>";
            tbkCurLvlTmp.fontSize = 14;
            tbkCurLvlTmp.alignment = TextAlignmentOptions.Center;
            tbkCurLvlTmp.color = Color.white;

            GameObject tbkCapGO = CreateUIRect(tbkLevelsGO.transform, "LevelCapText", new Vector2(0.5f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tbkCapRt = tbkCapGO.GetComponent<RectTransform>();
            tbkCapRt.offsetMin = new Vector2(8f, 0f);
            tbkCapRt.offsetMax = new Vector2(-8f, 0f);
            TextMeshProUGUI tbkCapTmp = tbkCapGO.AddComponent<TextMeshProUGUI>();
            tbkCapTmp.text = "Cấp tối đa: <color=#00FFFF>50</color> → <color=#FFD700>100</color>";
            tbkCapTmp.fontSize = 14;
            tbkCapTmp.alignment = TextAlignmentOptions.Center;
            tbkCapTmp.color = Color.white;

            // Requirements box (Stretches 0% to 100%)
            GameObject tbkReqGO = CreateUIRect(tbkScrollContent.transform, "RequirementsText", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tbkReqRt = tbkReqGO.GetComponent<RectTransform>();
            tbkReqRt.pivot = new Vector2(0.5f, 1f);
            tbkReqRt.offsetMin = new Vector2(0f, -200f);
            tbkReqRt.offsetMax = new Vector2(0f, -85f);
            CreateUIImage(tbkReqGO.transform, "ReqBg", new Color(0.08f, 0.10f, 0.16f, 0.8f));
            GameObject tbkReqTextGO = CreateUIRect(tbkReqGO.transform, "ReqContent", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform tbkReqTextRt = tbkReqTextGO.GetComponent<RectTransform>();
            tbkReqTextRt.offsetMin = new Vector2(12f, 8f);
            tbkReqTextRt.offsetMax = new Vector2(-12f, -8f);
            TextMeshProUGUI tbkReqTmp = tbkReqTextGO.AddComponent<TextMeshProUGUI>();
            tbkReqTmp.text = "<b><color=#FFD700>ĐIỀU KIỆN ĐỘT PHÁ</color></b>\nHero Level: 50 / 50 ✓\nGold: 5,000 / 5,000 ✓\nNguyên liệu: 30 / 30 ✓";
            tbkReqTmp.fontSize = 14;
            tbkReqTmp.alignment = TextAlignmentOptions.Center;
            tbkReqTmp.color = Color.white;
            tbkReqTmp.textWrappingMode = TextWrappingModes.Normal;

            // Rewards box (Stretches 0% to 100%)
            GameObject tbkRewardGO = CreateUIRect(tbkScrollContent.transform, "RewardsText", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tbkRewardRt = tbkRewardGO.GetComponent<RectTransform>();
            tbkRewardRt.pivot = new Vector2(0.5f, 1f);
            tbkRewardRt.offsetMin = new Vector2(0f, -325f);
            tbkRewardRt.offsetMax = new Vector2(0f, -210f);
            CreateUIImage(tbkRewardGO.transform, "RewBg", new Color(0.08f, 0.12f, 0.18f, 0.8f));
            GameObject tbkRewardTextGO = CreateUIRect(tbkRewardGO.transform, "RewContent", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform tbkRewardTextRt = tbkRewardTextGO.GetComponent<RectTransform>();
            tbkRewardTextRt.offsetMin = new Vector2(12f, 8f);
            tbkRewardTextRt.offsetMax = new Vector2(-12f, -8f);
            TextMeshProUGUI tbkRewardTmp = tbkRewardTextGO.AddComponent<TextMeshProUGUI>();
            tbkRewardTmp.text = "<b><color=#FFD700>THƯỞNG ĐỘT PHÁ</color></b>\nHP: +200  |  ATK: +50  |  DEF: +20\nCrit Rate: +1.0%  |  Crit Damage: +5.0%";
            tbkRewardTmp.fontSize = 14;
            tbkRewardTmp.alignment = TextAlignmentOptions.Center;
            tbkRewardTmp.color = Color.white;
            tbkRewardTmp.textWrappingMode = TextWrappingModes.Normal;

            // Stat comparison box (Stretches 0% to 100%)
            GameObject tbkStatCompGO = CreateUIRect(tbkScrollContent.transform, "StatComparisonText", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            RectTransform tbkStatCompRt = tbkStatCompGO.GetComponent<RectTransform>();
            tbkStatCompRt.pivot = new Vector2(0.5f, 1f);
            tbkStatCompRt.offsetMin = new Vector2(0f, -450f);
            tbkStatCompRt.offsetMax = new Vector2(0f, -335f);
            CreateUIImage(tbkStatCompGO.transform, "StatBg", new Color(0.08f, 0.10f, 0.16f, 0.8f));
            GameObject tbkStatCompTextGO = CreateUIRect(tbkStatCompGO.transform, "StatContent", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform tbkStatCompTextRt = tbkStatCompTextGO.GetComponent<RectTransform>();
            tbkStatCompTextRt.offsetMin = new Vector2(12f, 8f);
            tbkStatCompTextRt.offsetMax = new Vector2(-12f, -8f);
            TextMeshProUGUI tbkStatCompTmp = tbkStatCompTextGO.AddComponent<TextMeshProUGUI>();
            tbkStatCompTmp.text = "<b><color=#00FFFF>SO SÁNH THUỘC TÍNH (PREVIEW)</color></b>\nHP:   1000  →  1200 (+200)\nATK:   100  →   150 (+50)\nDEF:    20  →    40 (+20)";
            tbkStatCompTmp.fontSize = 14;
            tbkStatCompTmp.alignment = TextAlignmentOptions.Center;
            tbkStatCompTmp.color = Color.white;
            tbkStatCompTmp.textWrappingMode = TextWrappingModes.Normal;

            TitleBreakthroughUI tbkUI = canvasGO.AddComponent<TitleBreakthroughUI>();
            tbkUI.SetReferences(titleBkPanel, titleBkToggleBtn, tbkCloseBtn, tbkTitleTmp, tbkCurTitleTmp, tbkNextTitleTmp, tbkCurLvlTmp, tbkCapTmp, tbkReqTmp, tbkRewardTmp, tbkStatCompTmp, tbkActionBtn, tbkActionBtnTmp);
            titleBkPanel.SetActive(false);

            // ==========================================
            // CÔNG PHÁP SYSTEM SCREEN (Dedicated System Screen inside CongPhapContainer)
            // ==========================================
            GameObject mmPanel = CreateUIRect(congPhapContainer.transform, "MindMethodPanel", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image mmBg = mmPanel.AddComponent<Image>();
            mmBg.sprite = defaultSprite;
            mmBg.color = new Color(0.06f, 0.08f, 0.14f, 1.0f);
            mmBg.raycastTarget = true;


            // Title
            GameObject mmTitleGO = CreateUIRect(mmPanel.transform, "TitleText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -35), new Vector2(600, 40));
            TextMeshProUGUI mmTitleTmp = mmTitleGO.AddComponent<TextMeshProUGUI>();
            mmTitleTmp.text = "CÔNG PHÁP & TÂM PHÁP";
            mmTitleTmp.fontSize = 22;
            mmTitleTmp.fontStyle = FontStyles.Bold;
            mmTitleTmp.alignment = TextAlignmentOptions.Center;
            mmTitleTmp.color = UIStyleConfig.CyanHighlight;

            // Active MM Name & Level
            GameObject mmActiveTitleGO = CreateUIRect(mmPanel.transform, "ActiveTitleText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-180, -75), new Vector2(380, 32));
            TextMeshProUGUI mmActiveTitleTmp = mmActiveTitleGO.AddComponent<TextMeshProUGUI>();
            mmActiveTitleTmp.text = "TÂM PHÁP: <color=#FFD700>Thái Cực Thần Công</color>";
            mmActiveTitleTmp.fontSize = 15;
            mmActiveTitleTmp.alignment = TextAlignmentOptions.Left;

            GameObject mmActiveLvlGO = CreateUIRect(mmPanel.transform, "ActiveLevelText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(180, -75), new Vector2(340, 32));
            TextMeshProUGUI mmActiveLvlTmp = mmActiveLvlGO.AddComponent<TextMeshProUGUI>();
            mmActiveLvlTmp.text = "Cấp độ: <color=#00FFFF>Lv.1 / 10</color>";
            mmActiveLvlTmp.fontSize = 15;
            mmActiveLvlTmp.alignment = TextAlignmentOptions.Right;

            // Active Desc
            GameObject mmActiveDescGO = CreateUIRect(mmPanel.transform, "ActiveDescText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -115), new Vector2(740, 38));
            TextMeshProUGUI mmActiveDescTmp = mmActiveDescGO.AddComponent<TextMeshProUGUI>();
            mmActiveDescTmp.text = "Tâm pháp Đạo gia dĩ nhu chế cương, hộ thể trường cửu.";
            mmActiveDescTmp.fontSize = 13;
            mmActiveDescTmp.alignment = TextAlignmentOptions.Left;

            // Passives & Rage box
            GameObject mmPassiveGO = CreateUIRect(mmPanel.transform, "PassiveEffectsText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-185, -190), new Vector2(360, 95));
            TextMeshProUGUI mmPassiveTmp = mmPassiveGO.AddComponent<TextMeshProUGUI>();
            mmPassiveTmp.text = "<b><color=#FFD700>HIỆU ỨNG NỘI TẠI:</color></b>\n• HP: +200  |  DEF: +30\n• Né tránh: +5%\n• Quy Nhất Hồi Sinh (30% HP)";
            mmPassiveTmp.fontSize = 12;
            mmPassiveTmp.alignment = TextAlignmentOptions.Left;

            GameObject mmRageGO = CreateUIRect(mmPanel.transform, "RageModifiersText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(185, -190), new Vector2(360, 95));
            TextMeshProUGUI mmRageTmp = mmRageGO.AddComponent<TextMeshProUGUI>();
            mmRageTmp.text = "<b><color=#FF9900>TÙY CHỈNH NỘ KHÍ:</color></b>\n• Nộ khi Đánh: +0\n• Nộ khi Bị đánh: +1";
            mmRageTmp.fontSize = 12;
            mmRageTmp.alignment = TextAlignmentOptions.Left;

            // Summary List
            GameObject mmSummaryGO = CreateUIRect(mmPanel.transform, "MindMethodSummaryText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -280), new Vector2(740, 65));
            TextMeshProUGUI mmSummaryTmp = mmSummaryGO.AddComponent<TextMeshProUGUI>();
            mmSummaryTmp.text = "<b><color=#FFD700>DANH SÁCH TÂM PHÁP:</color></b>\n• Thái Cực (Lv.1) [ĐANG DÙNG]  |  • Cửu Dương (Lv.1) [CHƯA MỞ]  |  • Cửu Âm (Lv.1) [CHƯA MỞ]";
            mmSummaryTmp.fontSize = 12;
            mmSummaryTmp.alignment = TextAlignmentOptions.Left;

            // 5 Skill Slots text
            GameObject s1GO = CreateUIRect(mmPanel.transform, "Slot1Text", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-185, -345), new Vector2(360, 24));
            TextMeshProUGUI s1Tmp = s1GO.AddComponent<TextMeshProUGUI>();
            s1Tmp.fontSize = 12;

            GameObject s2GO = CreateUIRect(mmPanel.transform, "Slot2Text", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(185, -345), new Vector2(360, 24));
            TextMeshProUGUI s2Tmp = s2GO.AddComponent<TextMeshProUGUI>();
            s2Tmp.fontSize = 12;

            GameObject s3GO = CreateUIRect(mmPanel.transform, "Slot3Text", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-185, -375), new Vector2(360, 24));
            TextMeshProUGUI s3Tmp = s3GO.AddComponent<TextMeshProUGUI>();
            s3Tmp.fontSize = 12;

            GameObject s4GO = CreateUIRect(mmPanel.transform, "Slot4Text", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(185, -375), new Vector2(360, 24));
            TextMeshProUGUI s4Tmp = s4GO.AddComponent<TextMeshProUGUI>();
            s4Tmp.fontSize = 12;

            GameObject s5GO = CreateUIRect(mmPanel.transform, "Slot5Text", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -405), new Vector2(740, 24));
            TextMeshProUGUI s5Tmp = s5GO.AddComponent<TextMeshProUGUI>();
            s5Tmp.fontSize = 12;

            // Alternatives Box
            GameObject altHeaderGO = CreateUIRect(mmPanel.transform, "AltHeaderText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -445), new Vector2(740, 26));
            TextMeshProUGUI altHeaderTmp = altHeaderGO.AddComponent<TextMeshProUGUI>();
            altHeaderTmp.fontSize = 13;
            altHeaderTmp.fontStyle = FontStyles.Bold;
            altHeaderTmp.color = UIStyleConfig.GoldAccent;

            GameObject altListGO = CreateUIRect(mmPanel.transform, "AltListText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -560), new Vector2(740, 190));
            TextMeshProUGUI altListTmp = altListGO.AddComponent<TextMeshProUGUI>();
            altListTmp.fontSize = 12;
            altListTmp.alignment = TextAlignmentOptions.TopLeft;

            MindMethodUI mmUI = canvasGO.AddComponent<MindMethodUI>();
            mmUI.SetReferences(mmPanel, null, null, mmActiveTitleTmp, mmActiveLvlTmp, mmActiveDescTmp, mmPassiveTmp, mmRageTmp, mmSummaryTmp, s1Tmp, s2Tmp, s3Tmp, s4Tmp, s5Tmp, altHeaderTmp, altListTmp);
            mmPanel.SetActive(false);

            // Register all modal views with ModalCoordinator (MindMethodUI is dedicated screen, not a modal view)
            modalCoordinator.RegisterModalView(lootUI);
            modalCoordinator.RegisterModalView(ltUI);
            modalCoordinator.RegisterModalView(tbkUI);

            // Link BattleHUD references
            SerializedObject hudSO = new SerializedObject(battleHUD);
            hudSO.Update();
            hudSO.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
            hudSO.FindProperty("defeatPanel").objectReferenceValue = defeatPanel;
            hudSO.FindProperty("restartButton").objectReferenceValue = startBtn;
            hudSO.FindProperty("startButton").objectReferenceValue = startBtn;
            hudSO.FindProperty("statusText").objectReferenceValue = titleText;
            hudSO.FindProperty("defeatText").objectReferenceValue = defeatText;
            hudSO.FindProperty("heroShieldRoot").objectReferenceValue = heroShieldBarGO;
            hudSO.FindProperty("heroShieldFill").objectReferenceValue = heroShieldFill;
            hudSO.FindProperty("heroShieldText").objectReferenceValue = heroShieldTmp;
            hudSO.FindProperty("monsterShieldRoot").objectReferenceValue = monsterShieldBarGO;
            hudSO.FindProperty("monsterShieldFill").objectReferenceValue = monsterShieldFill;
            hudSO.FindProperty("monsterShieldText").objectReferenceValue = monsterShieldTmp;
            hudSO.FindProperty("heroCastBarUI").objectReferenceValue = heroCastUI;
            hudSO.FindProperty("heroCastBarRoot").objectReferenceValue = heroCastBarGO;
            hudSO.FindProperty("heroCastBarFill").objectReferenceValue = heroCastFill;
            hudSO.FindProperty("heroCastBarText").objectReferenceValue = heroCastTmp;
            hudSO.FindProperty("heroInterruptText").objectReferenceValue = heroInterruptTmp;
            hudSO.ApplyModifiedPropertiesWithoutUndo();

            battleHUD.SetShieldReferences(heroShieldBarGO, heroShieldFill, heroShieldTmp, monsterShieldBarGO, monsterShieldFill, monsterShieldTmp);
            battleHUD.SetCastBarReferences(heroCastUI, heroCastBarGO, heroCastFill, heroCastTmp, heroInterruptTmp);


            // Save Scene
            EditorSceneManager.SaveScene(scene, SceneAssetPath);
            EnsureBuildSettingsContainsScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.OpenScene(SceneAssetPath);

            Debug.Log($"[Prototype01SceneBuilder] Successfully built, saved, and opened scene at '{SceneAssetPath}'!");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(DiskFolderData)) Directory.CreateDirectory(DiskFolderData);
            if (!Directory.Exists(DiskFolderResourcesData)) Directory.CreateDirectory(DiskFolderResourcesData);
            if (!Directory.Exists(DiskFolderScenes)) Directory.CreateDirectory(DiskFolderScenes);
            AssetDatabase.Refresh();
        }

        private static List<RarityDefinitionSO> CreateRarityAssets(string parentDir)
        {
            var list = new List<RarityDefinitionSO>
            {
                GetOrCreateRarity($"{parentDir}/Rarity_01_Pure.asset", "tinh_khieut", "Tinh Khiết", Color.white, 1, 1, 2, 1.0f),
                GetOrCreateRarity($"{parentDir}/Rarity_02_Supreme.asset", "toi_thuong", "Tối Thượng", Color.green, 2, 2, 3, 1.2f),
                GetOrCreateRarity($"{parentDir}/Rarity_03_Perfect.asset", "hoan_my", "Hoàn Mỹ", Color.cyan, 3, 3, 4, 1.5f),
                GetOrCreateRarity($"{parentDir}/Rarity_04_Relic1.asset", "chi_bao_1", "Chí Bảo Bậc 1", Color.magenta, 4, 3, 4, 1.8f),
                GetOrCreateRarity($"{parentDir}/Rarity_05_Relic2.asset", "chi_bao_2", "Chí Bảo Bậc 2", new Color(1f, 0.5f, 0f), 5, 4, 5, 2.2f),
                GetOrCreateRarity($"{parentDir}/Rarity_06_Relic3.asset", "chi_bao_3", "Chí Bảo Bậc 3", Color.yellow, 6, 4, 5, 2.6f),
                GetOrCreateRarity($"{parentDir}/Rarity_07_Relic4.asset", "chi_bao_4", "Chí Bảo Bậc 4", new Color(1f, 0.2f, 0.2f), 7, 5, 5, 3.0f),
                GetOrCreateRarity($"{parentDir}/Rarity_08_Relic5.asset", "chi_bao_5", "Chí Bảo Bậc 5", new Color(0.8f, 0.2f, 1f), 8, 5, 6, 3.5f),
                GetOrCreateRarity($"{parentDir}/Rarity_09_Transcendent1.asset", "phi_pham_1", "Phi Phẩm Bậc 1", new Color(1f, 0.84f, 0f), 9, 6, 6, 4.2f)
            };
            return list;
        }

        private static List<DropLevelConfigSO> CreateDropLevelAssets(string parentDir, List<RarityDefinitionSO> r)
        {
            var list = new List<DropLevelConfigSO>();

            if (r == null || r.Count < 9) return list;

            // Drop Level 16
            var weights16 = new List<RarityWeightData>
            {
                new RarityWeightData(r[0], 50.78f),
                new RarityWeightData(r[1], 29.44f),
                new RarityWeightData(r[2], 10.66f),
                new RarityWeightData(r[3], 6.02f),
                new RarityWeightData(r[4], 2.36f),
                new RarityWeightData(r[5], 0.64f),
                new RarityWeightData(r[6], 0.09f),
                new RarityWeightData(r[7], 0.01f),
                new RarityWeightData(r[8], 0.00f)
            };
            DropLevelConfigSO dl16 = GetOrCreateDropLevel($"{parentDir}/DropLevel_16.asset", 16, weights16);

            // Drop Level 17
            var weights17 = new List<RarityWeightData>
            {
                new RarityWeightData(r[0], 0.00f),
                new RarityWeightData(r[1], 53.01f),
                new RarityWeightData(r[2], 28.44f),
                new RarityWeightData(r[3], 10.15f),
                new RarityWeightData(r[4], 5.56f),
                new RarityWeightData(r[5], 2.12f),
                new RarityWeightData(r[6], 0.62f),
                new RarityWeightData(r[7], 0.09f),
                new RarityWeightData(r[8], 0.01f)
            };
            DropLevelConfigSO dl17 = GetOrCreateDropLevel($"{parentDir}/DropLevel_17.asset", 17, weights17);

            list.Add(dl16);
            list.Add(dl17);
            return list;
        }

        private static List<AffixDefinitionSO> CreateAffixAssets(string parentDir)
        {
            var list = new List<AffixDefinitionSO>
            {
                GetOrCreateAffix($"{parentDir}/affix_atk.asset", "affix_atk", "+ ATK", StatType.Attack, 10f, 25f, 100f),
                GetOrCreateAffix($"{parentDir}/affix_hp.asset", "affix_hp", "+ HP", StatType.MaxHealth, 50f, 120f, 100f),
                GetOrCreateAffix($"{parentDir}/affix_def.asset", "affix_def", "+ DEF", StatType.Defense, 2f, 8f, 80f),
                GetOrCreateAffix($"{parentDir}/affix_crit_rate.asset", "affix_crit_rate", "+ Crit Rate", StatType.CritRate, 0.5f, 2.5f, 60f),
                GetOrCreateAffix($"{parentDir}/affix_crit_dmg.asset", "affix_crit_dmg", "+ Crit Damage", StatType.CritDamage, 1.0f, 5.0f, 50f),
                GetOrCreateAffix($"{parentDir}/affix_dodge.asset", "affix_dodge", "+ Dodge", StatType.Dodge, 0.5f, 2.0f, 40f),
                GetOrCreateAffix($"{parentDir}/affix_lifesteal.asset", "affix_lifesteal", "+ Lifesteal", StatType.Lifesteal, 0.5f, 1.5f, 30f)
            };
            return list;
        }

        private static List<EquipmentDefinitionSO> CreateEquipmentDefinitionAssets(string parentDir)
        {
            var list = new List<EquipmentDefinitionSO>
            {
                GetOrCreateEquipmentDef($"{parentDir}/eq_weapon.asset", "eq_weapon_01", "Wuxia Sword", EquipmentSlotType.Weapon),
                GetOrCreateEquipmentDef($"{parentDir}/eq_armor.asset", "eq_armor_01", "Dragon Robe", EquipmentSlotType.Armor),
                GetOrCreateEquipmentDef($"{parentDir}/eq_helmet.asset", "eq_helmet_01", "Jade Crown", EquipmentSlotType.Helmet),
                GetOrCreateEquipmentDef($"{parentDir}/eq_gloves.asset", "eq_gloves_01", "Iron Bracers", EquipmentSlotType.Gloves),
                GetOrCreateEquipmentDef($"{parentDir}/eq_boots.asset", "eq_boots_01", "Shadow Steps", EquipmentSlotType.Boots),
                GetOrCreateEquipmentDef($"{parentDir}/eq_ring.asset", "eq_ring_01", "Jade Ring", EquipmentSlotType.LeftRing),
                GetOrCreateEquipmentDef($"{parentDir}/eq_right_ring.asset", "eq_right_ring_01", "Phoenix Ring", EquipmentSlotType.RightRing),
                GetOrCreateEquipmentDef($"{parentDir}/eq_necklace.asset", "eq_necklace_01", "Phoenix Pendant", EquipmentSlotType.Necklace),
                GetOrCreateEquipmentDef($"{parentDir}/eq_accessory.asset", "eq_accessory_01", "Phoenix Amulet", EquipmentSlotType.Accessory),
                GetOrCreateEquipmentDef($"{parentDir}/eq_belt.asset", "eq_belt_01", "Cloud Belt", EquipmentSlotType.Belt),
                GetOrCreateEquipmentDef($"{parentDir}/eq_talisman.asset", "eq_talisman_01", "Spirit Talisman", EquipmentSlotType.Talisman),
                GetOrCreateEquipmentDef($"{parentDir}/eq_shoulder.asset", "eq_shoulder_01", "Tiger Pauldron", EquipmentSlotType.Shoulder)
            };
            return list;
        }

        private static CombatConfigSO GetOrCreateCombatConfig(string path)
        {
            CombatConfigSO asset = AssetDatabase.LoadAssetAtPath<CombatConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CombatConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static HeroConfigSO GetOrCreateHeroConfig(string path)
        {
            HeroConfigSO asset = AssetDatabase.LoadAssetAtPath<HeroConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HeroConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static MonsterConfigSO GetOrCreateMonsterConfig(string path)
        {
            MonsterConfigSO asset = AssetDatabase.LoadAssetAtPath<MonsterConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<MonsterConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static ExpConfigSO GetOrCreateExpConfig(string path)
        {
            ExpConfigSO asset = AssetDatabase.LoadAssetAtPath<ExpConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ExpConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static TitleDatabaseSO GetOrCreateTitleDatabase(string path)
        {
            TitleDatabaseSO db = AssetDatabase.LoadAssetAtPath<TitleDatabaseSO>(path);
            string parentDir = Path.GetDirectoryName(path).Replace("\\", "/");

            TitleConfigSO t1 = GetOrCreateTitleConfig($"{parentDir}/Title_01.asset", "title_01", "Novice Disciple", 1, 1000f, 100f, 20f, 5, new List<ProgressionRequirementData>
            {
                new ProgressionRequirementData(RequirementType.PlayerLevel, 5, "Reach Level 5", true),
                new ProgressionRequirementData(RequirementType.ClearStage, 1, "Clear Stage 1-1", true),
                new ProgressionRequirementData(RequirementType.DropLevel, 2, "Drop Level 2", true)
            });

            TitleConfigSO t2 = GetOrCreateTitleConfig($"{parentDir}/Title_02.asset", "title_02", "Iron Warrior", 2, 2000f, 200f, 40f, 10, new List<ProgressionRequirementData>
            {
                new ProgressionRequirementData(RequirementType.PlayerLevel, 10, "Reach Level 10", true),
                new ProgressionRequirementData(RequirementType.ClearStage, 2, "Clear Stage 2-1", true),
                new ProgressionRequirementData(RequirementType.DropLevel, 3, "Drop Level 3", true)
            });

            TitleConfigSO t3 = GetOrCreateTitleConfig($"{parentDir}/Title_03.asset", "title_03", "Bronze Champion", 3, 4000f, 400f, 80f, 20, new List<ProgressionRequirementData>
            {
                new ProgressionRequirementData(RequirementType.PlayerLevel, 20, "Reach Level 20", true),
                new ProgressionRequirementData(RequirementType.ClearStage, 3, "Clear Stage 3-1", true),
                new ProgressionRequirementData(RequirementType.DropLevel, 4, "Drop Level 4", true)
            });

            TitleConfigSO t4 = GetOrCreateTitleConfig($"{parentDir}/Title_04.asset", "title_04", "Gold Master", 4, 8000f, 800f, 160f, 50, new List<ProgressionRequirementData>());

            if (db == null)
            {
                db = ScriptableObject.CreateInstance<TitleDatabaseSO>();
                AssetDatabase.CreateAsset(db, path);
            }

            db.SetTitles(new List<TitleConfigSO> { t1, t2, t3, t4 });
            EditorUtility.SetDirty(db);
            return db;
        }

        private static TitleConfigSO GetOrCreateTitleConfig(string path, string id, string name, int order, float hp, float atk, float def, int cap, List<ProgressionRequirementData> reqs)
        {
            TitleConfigSO asset = AssetDatabase.LoadAssetAtPath<TitleConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<TitleConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.InitializeTitle(id, name, order, hp, atk, def, cap, reqs);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static TitleBreakthroughDatabaseSO GetOrCreateTitleBreakthroughDatabase(string path)
        {
            TitleBreakthroughDatabaseSO db = AssetDatabase.LoadAssetAtPath<TitleBreakthroughDatabaseSO>(path);
            string parentDir = Path.GetDirectoryName(path).Replace("\\", "/");

            TitleBreakthroughConfigSO tb0 = GetOrCreateTitleBreakthroughConfig(
                $"{parentDir}/TitleBreakthrough_00.asset", "title_00", "Novice Disciple", 0, 5, 10, 5, 5000, 30,
                1000f, 100f, 20f, 0f, 0f, 0f, "Initial Stage", 0, "",
                new List<BreakthroughRequirement>
                {
                    new BreakthroughRequirement(BreakthroughRequirementType.HeroLevel, 5),
                    new BreakthroughRequirement(BreakthroughRequirementType.LootTier, 2),
                    new BreakthroughRequirement(BreakthroughRequirementType.Gold, 5000),
                    new BreakthroughRequirement(BreakthroughRequirementType.Material, 30)
                });

            TitleBreakthroughConfigSO tb1 = GetOrCreateTitleBreakthroughConfig(
                $"{parentDir}/TitleBreakthrough_01.asset", "title_01", "Iron Warrior", 1, 10, 20, 10, 10000, 60,
                1200f, 150f, 40f, 1f, 5f, 0f, "Warrior Stage", 0, "",
                new List<BreakthroughRequirement>
                {
                    new BreakthroughRequirement(BreakthroughRequirementType.HeroLevel, 10),
                    new BreakthroughRequirement(BreakthroughRequirementType.LootTier, 3),
                    new BreakthroughRequirement(BreakthroughRequirementType.Gold, 10000),
                    new BreakthroughRequirement(BreakthroughRequirementType.Material, 60)
                });

            TitleBreakthroughConfigSO tb2 = GetOrCreateTitleBreakthroughConfig(
                $"{parentDir}/TitleBreakthrough_02.asset", "title_02", "Bronze Champion", 2, 20, 35, 20, 20000, 120,
                1500f, 220f, 70f, 2f, 10f, 0f, "Champion Stage", 0, "",
                new List<BreakthroughRequirement>
                {
                    new BreakthroughRequirement(BreakthroughRequirementType.HeroLevel, 20),
                    new BreakthroughRequirement(BreakthroughRequirementType.LootTier, 4),
                    new BreakthroughRequirement(BreakthroughRequirementType.Gold, 20000),
                    new BreakthroughRequirement(BreakthroughRequirementType.Material, 120)
                });

            TitleBreakthroughConfigSO tb3 = GetOrCreateTitleBreakthroughConfig(
                $"{parentDir}/TitleBreakthrough_03.asset", "title_03", "Silver Guardian", 3, 35, 55, 35, 40000, 240,
                2000f, 320f, 110f, 3f, 15f, 0f, "Guardian Stage", 0, "",
                new List<BreakthroughRequirement>
                {
                    new BreakthroughRequirement(BreakthroughRequirementType.HeroLevel, 35),
                    new BreakthroughRequirement(BreakthroughRequirementType.LootTier, 5),
                    new BreakthroughRequirement(BreakthroughRequirementType.Gold, 40000),
                    new BreakthroughRequirement(BreakthroughRequirementType.Material, 240)
                });

            TitleBreakthroughConfigSO tb4 = GetOrCreateTitleBreakthroughConfig(
                $"{parentDir}/TitleBreakthrough_04.asset", "title_04", "Gold Master", 4, 55, 80, 55, 80000, 480,
                2800f, 450f, 160f, 5f, 20f, 0f, "Master Stage", 0, "",
                new List<BreakthroughRequirement>
                {
                    new BreakthroughRequirement(BreakthroughRequirementType.HeroLevel, 55),
                    new BreakthroughRequirement(BreakthroughRequirementType.LootTier, 6),
                    new BreakthroughRequirement(BreakthroughRequirementType.Gold, 80000),
                    new BreakthroughRequirement(BreakthroughRequirementType.Material, 480)
                });

            TitleBreakthroughConfigSO tb5 = GetOrCreateTitleBreakthroughConfig(
                $"{parentDir}/TitleBreakthrough_05.asset", "title_05", "Platinum Grandmaster", 5, 80, 110, 80, 160000, 960,
                3800f, 600f, 220f, 7f, 25f, 0f, "Grandmaster Stage", 0, "",
                new List<BreakthroughRequirement>
                {
                    new BreakthroughRequirement(BreakthroughRequirementType.HeroLevel, 80),
                    new BreakthroughRequirement(BreakthroughRequirementType.LootTier, 7),
                    new BreakthroughRequirement(BreakthroughRequirementType.Gold, 160000),
                    new BreakthroughRequirement(BreakthroughRequirementType.Material, 960)
                });

            TitleBreakthroughConfigSO tb6 = GetOrCreateTitleBreakthroughConfig(
                $"{parentDir}/TitleBreakthrough_06.asset", "title_06", "Diamond Warlord", 6, 110, 145, 110, 320000, 1920,
                5000f, 800f, 300f, 10f, 30f, 0f, "Warlord Stage", 0, "",
                new List<BreakthroughRequirement>
                {
                    new BreakthroughRequirement(BreakthroughRequirementType.HeroLevel, 110),
                    new BreakthroughRequirement(BreakthroughRequirementType.LootTier, 8),
                    new BreakthroughRequirement(BreakthroughRequirementType.Gold, 320000),
                    new BreakthroughRequirement(BreakthroughRequirementType.Material, 1920)
                });

            TitleBreakthroughConfigSO tb7 = GetOrCreateTitleBreakthroughConfig(
                $"{parentDir}/TitleBreakthrough_07.asset", "title_07", "Immortal Sovereign", 7, 145, 200, 145, 640000, 3840,
                7000f, 1100f, 400f, 15f, 40f, 0f, "Immortal Stage", 0, "",
                new List<BreakthroughRequirement>
                {
                    new BreakthroughRequirement(BreakthroughRequirementType.HeroLevel, 145)
                });

            if (db == null)
            {
                db = ScriptableObject.CreateInstance<TitleBreakthroughDatabaseSO>();
                AssetDatabase.CreateAsset(db, path);
            }

            db.SetBreakthroughs(new List<TitleBreakthroughConfigSO> { tb0, tb1, tb2, tb3, tb4, tb5, tb6, tb7 });
            EditorUtility.SetDirty(db);
            return db;
        }

        private static TitleBreakthroughConfigSO GetOrCreateTitleBreakthroughConfig(
            string path,
            string id,
            string name,
            int order,
            int curCap,
            int nextCap,
            int reqLevel,
            int reqGold,
            int reqMat,
            float hp,
            float atk,
            float def,
            float critRate = 0f,
            float critDmg = 0f,
            float dodge = 0f,
            string desc = "",
            int reqStage = 0,
            string reqPrev = "",
            List<BreakthroughRequirement> customReqs = null)
        {
            TitleBreakthroughConfigSO asset = AssetDatabase.LoadAssetAtPath<TitleBreakthroughConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<TitleBreakthroughConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.InitializeBreakthrough(id, name, order, curCap, nextCap, reqLevel, reqGold, reqMat, hp, atk, def, critRate, critDmg, dodge, desc, reqStage, reqPrev, customReqs);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static RarityDefinitionSO GetOrCreateRarity(string path, string id, string name, Color color, int order, int minAff, int maxAff, float mult)
        {
            RarityDefinitionSO asset = AssetDatabase.LoadAssetAtPath<RarityDefinitionSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<RarityDefinitionSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.InitializeRarity(id, name, color, order, minAff, maxAff, mult);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static DropLevelConfigSO GetOrCreateDropLevel(string path, int level, List<RarityWeightData> weights)
        {
            DropLevelConfigSO asset = AssetDatabase.LoadAssetAtPath<DropLevelConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DropLevelConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.InitializeDropLevel(level, weights);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static DropLevelDatabaseSO GetOrCreateDropLevelDatabase(string path)
        {
            DropLevelDatabaseSO db = AssetDatabase.LoadAssetAtPath<DropLevelDatabaseSO>(path);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<DropLevelDatabaseSO>();
                AssetDatabase.CreateAsset(db, path);
            }
            return db;
        }

        private static AffixDefinitionSO GetOrCreateAffix(string path, string id, string name, StatType stat, float minVal, float maxVal, float weight)
        {
            AffixDefinitionSO asset = AssetDatabase.LoadAssetAtPath<AffixDefinitionSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<AffixDefinitionSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.InitializeAffix(id, name, stat, minVal, maxVal, weight);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static AffixDatabaseSO GetOrCreateAffixDatabase(string path)
        {
            AffixDatabaseSO db = AssetDatabase.LoadAssetAtPath<AffixDatabaseSO>(path);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<AffixDatabaseSO>();
                AssetDatabase.CreateAsset(db, path);
            }
            return db;
        }

        private static EquipmentDefinitionSO GetOrCreateEquipmentDef(string path, string id, string name, EquipmentSlotType slot)
        {
            EquipmentDefinitionSO asset = AssetDatabase.LoadAssetAtPath<EquipmentDefinitionSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EquipmentDefinitionSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.InitializeDefinition(id, name, slot);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static EquipmentDatabaseSO GetOrCreateEquipmentDatabase(string path)
        {
            EquipmentDatabaseSO db = AssetDatabase.LoadAssetAtPath<EquipmentDatabaseSO>(path);
            if (db == null)
            {
                db = ScriptableObject.CreateInstance<EquipmentDatabaseSO>();
                AssetDatabase.CreateAsset(db, path);
            }
            return db;
        }

        private static EquipmentUpgradeConfigSO GetOrCreateUpgradeConfig(string path)
        {
            EquipmentUpgradeConfigSO asset = AssetDatabase.LoadAssetAtPath<EquipmentUpgradeConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EquipmentUpgradeConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static HeroProgressionConfigSO GetOrCreateHeroProgressionConfig(string path)
        {
            HeroProgressionConfigSO asset = AssetDatabase.LoadAssetAtPath<HeroProgressionConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HeroProgressionConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static LootTierConfigSO GetOrCreateLootTierConfig(string path, int tierId, string name, List<RarityWeightData> weights, int reqProgress = 2500, float durationSeconds = 900f, int costGold = 0, int costMaterial = 0)
        {
            LootTierConfigSO asset = AssetDatabase.LoadAssetAtPath<LootTierConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LootTierConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.InitializeTier(tierId, name, weights, reqProgress, durationSeconds, costGold, costMaterial);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static LootTierDatabaseSO GetOrCreateLootTierDatabase(string path)
        {
            LootTierDatabaseSO asset = AssetDatabase.LoadAssetAtPath<LootTierDatabaseSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LootTierDatabaseSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static EquipmentDropConfigSO GetOrCreateEquipmentDropConfig(string path, float dropRate, List<LootTierWeightData> weights)
        {
            EquipmentDropConfigSO asset = AssetDatabase.LoadAssetAtPath<EquipmentDropConfigSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EquipmentDropConfigSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.InitializeConfig(dropRate, weights);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static Sprite CreateWhiteSprite()
        {
            Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        }

        private static GameObject CreateUIRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            return go;
        }

        private static Image CreateUIImage(Transform parent, string name, Color color, bool raycastTarget = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycastTarget;
            return img;
        }

        private static TextMeshProUGUI CreateUIText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions align, Color color, bool raycastTarget = false)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(500f, 40f);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.raycastTarget = raycastTarget;
            return tmp;
        }

        private static (GameObject bgGO, Image fillImg, TextMeshProUGUI tmpText) CreateUIBar(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor)
        {
            GameObject bgGO = CreateUIRect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
            bgImg.raycastTarget = false;

            GameObject fillGO = CreateUIRect(bgGO.transform, "Fill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image fillImg = fillGO.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.raycastTarget = false;

            GameObject textGO = CreateUIRect(bgGO.transform, "Text", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 16;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return (bgGO, fillImg, tmp);
        }

        private static Button CreateUIButton(Transform parent, string name, string label, Vector2 pos, Vector2 size)
        {
            GameObject btnGO = CreateUIRect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            Image img = btnGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.55f, 0.9f);
            img.raycastTarget = true;
            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = true;

            GameObject textGO = CreateUIRect(btnGO.transform, "Text", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 20;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return btn;
        }

        private static GameObject CreateWuxiaPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color bgColor, Color borderColor, float borderWidth = 2f)
        {
            GameObject panelGO = CreateUIRect(parent, name, anchorMin, anchorMax, pos, size);
            
            // 9-sliced ornate wuxia panel background with inner shadow & beveled border
            GameObject bgGO = CreateUIRect(panelGO.transform, "Bg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.sprite = UIProceduralTextureFactory.GetPanelSprite();
            bgImg.type = Image.Type.Sliced;
            bgImg.color = bgColor;
            bgImg.raycastTarget = false;

            // 4 ornamental border lines (Top, Bottom, Left, Right)
            GameObject topB = CreateUIRect(panelGO.transform, "Border_Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -borderWidth * 0.5f), new Vector2(0f, borderWidth));
            CreateUIImage(topB.transform, "Line", borderColor);

            GameObject botB = CreateUIRect(panelGO.transform, "Border_Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, borderWidth * 0.5f), new Vector2(0f, borderWidth));
            CreateUIImage(botB.transform, "Line", borderColor);

            GameObject leftB = CreateUIRect(panelGO.transform, "Border_Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(borderWidth * 0.5f, 0f), new Vector2(borderWidth, 0f));
            CreateUIImage(leftB.transform, "Line", borderColor);

            GameObject rightB = CreateUIRect(panelGO.transform, "Border_Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-borderWidth * 0.5f, 0f), new Vector2(borderWidth, 0f));
            CreateUIImage(rightB.transform, "Line", borderColor);

            return panelGO;
        }

        private static (GameObject bgGO, Image fillImg, TextMeshProUGUI tmpText) CreateWuxiaBar(
            Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor, Color borderColor)
        {
            GameObject bgGO = CreateUIRect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.sprite = UIProceduralTextureFactory.GetBarBgSprite();
            bgImg.type = Image.Type.Sliced;
            bgImg.color = new Color(0.06f, 0.08f, 0.12f, 0.95f);
            bgImg.raycastTarget = false;

            // Border lines
            GameObject borderTop = CreateUIRect(bgGO.transform, "B_Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -0.5f), new Vector2(0f, 1.5f));
            CreateUIImage(borderTop.transform, "Line", borderColor);
            GameObject borderBot = CreateUIRect(bgGO.transform, "B_Bot", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0.5f), new Vector2(0f, 1.5f));
            CreateUIImage(borderBot.transform, "Line", borderColor);

            // Fill with beveled glossy highlight
            GameObject fillGO = CreateUIRect(bgGO.transform, "Fill", new Vector2(0f, 0.05f), new Vector2(1f, 0.95f), Vector2.zero, Vector2.zero);
            Image fillImg = fillGO.AddComponent<Image>();
            fillImg.sprite = UIProceduralTextureFactory.GetBarFillSprite();
            fillImg.color = fillColor;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.raycastTarget = false;

            // Text
            GameObject textGO = CreateUIRect(bgGO.transform, "Text", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 15;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return (bgGO, fillImg, tmp);
        }

        private static (Button btn, Image bg, Image border, Image cdFill, TextMeshProUGUI nameTxt, TextMeshProUGUI cdTxt, GameObject pulse) CreateSkillButton(
            Transform parent, string name, Vector2 pos, float size, Color bgColor, Color borderColor, string label, string iconKey = null, bool isUltimate = false)
        {
            GameObject btnGO = CreateUIRect(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, new Vector2(size, size));

            // Pulse glow for Ultimate
            GameObject pulseGO = CreateUIRect(btnGO.transform, "PulseGlow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size * 1.35f, size * 1.35f));
            CreateUIImage(pulseGO.transform, "Glow", new Color(1f, 0.85f, 0.25f, 0.40f));
            pulseGO.SetActive(false);

            // Background (circular wuxia slot)
            Image bg = btnGO.AddComponent<Image>();
            bg.sprite = UIProceduralTextureFactory.GetSkillSlotBgSprite();
            bg.color = bgColor;
            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = bg;

            // Center Martial Arts Silhouette Icon
            if (!string.IsNullOrEmpty(iconKey))
            {
                float iconSize = size * 0.52f;
                GameObject iconGO = CreateUIRect(btnGO.transform, "Icon", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 7f), new Vector2(iconSize, iconSize));
                Image iconImg = iconGO.AddComponent<Image>();
                iconImg.sprite = UIProceduralTextureFactory.GetIconSprite(iconKey);
                iconImg.color = isUltimate ? new Color(1f, 0.95f, 0.70f, 0.95f) : new Color(0.85f, 0.90f, 1f, 0.88f);
                iconImg.raycastTarget = false;
            }

            // Border (circular wuxia frame with metal rivets)
            GameObject borderGO = CreateUIRect(btnGO.transform, "Border", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image borderImg = borderGO.AddComponent<Image>();
            borderImg.sprite = UIProceduralTextureFactory.GetSkillFrameSprite();
            borderImg.color = borderColor;
            borderImg.raycastTarget = false;

            // Cooldown Radial Fill
            GameObject cdFillGO = CreateUIRect(btnGO.transform, "CooldownFill", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image cdFill = cdFillGO.AddComponent<Image>();
            cdFill.sprite = UIProceduralTextureFactory.GetRadialCooldownSprite();
            cdFill.color = new Color(0.04f, 0.04f, 0.04f, 0.85f);
            cdFill.type = Image.Type.Filled;
            cdFill.fillMethod = Image.FillMethod.Radial360;
            cdFill.fillOrigin = (int)Image.Origin360.Top;
            cdFill.fillClockwise = false;
            cdFill.fillAmount = 0f;
            cdFill.raycastTarget = false;
            cdFillGO.SetActive(false);

            // Name Label (anchored below icon)
            GameObject nameGO = CreateUIRect(btnGO.transform, "Label", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 10f), new Vector2(0f, 20f));
            TextMeshProUGUI nameTxt = nameGO.AddComponent<TextMeshProUGUI>();
            nameTxt.text = label;
            nameTxt.fontSize = isUltimate ? 12 : 10;
            nameTxt.fontStyle = FontStyles.Bold;
            nameTxt.alignment = TextAlignmentOptions.Center;
            nameTxt.color = isUltimate ? UIStyleConfig.GoldAccent : Color.white;
            nameTxt.raycastTarget = false;

            // Cooldown Text
            GameObject cdTextGO = CreateUIRect(btnGO.transform, "CooldownText", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            TextMeshProUGUI cdTxt = cdTextGO.AddComponent<TextMeshProUGUI>();
            cdTxt.text = "";
            cdTxt.fontSize = isUltimate ? 20 : 16;
            cdTxt.fontStyle = FontStyles.Bold;
            cdTxt.alignment = TextAlignmentOptions.Center;
            cdTxt.color = Color.yellow;
            cdTxt.raycastTarget = false;
            cdTextGO.SetActive(false);

            return (btn, bg, borderImg, cdFill, nameTxt, cdTxt, pulseGO);
        }

        private static void CreateBottomNavigationSlots(Transform parent, GlobalBottomNavigation navComp, Sprite defaultSprite)
        {
            string[] slotLabels = new string[] { "Túi Đồ", "Công Pháp", "Đại Điện", "Bang Hội", "Thiết Lập" };
            string[] slotIconKeys = new string[] { "chest", "scroll", "temple", "banner", "gear" };
            Button[] navButtons = new Button[GlobalBottomNavigation.SlotCount];
            Image[] navIcons = new Image[GlobalBottomNavigation.SlotCount];
            TextMeshProUGUI[] navLabels = new TextMeshProUGUI[GlobalBottomNavigation.SlotCount];
            GameObject[] navActiveIndicators = new GameObject[GlobalBottomNavigation.SlotCount];

            float slotFraction = 1f / GlobalBottomNavigation.SlotCount;

            for (int i = 0; i < GlobalBottomNavigation.SlotCount; i++)
            {
                bool isMainHub = (i == GlobalBottomNavigation.MainHubIndex);
                float minX = i * slotFraction;
                float maxX = (i + 1) * slotFraction;

                GameObject slotGO = new GameObject($"NavSlot_{i}");
                slotGO.transform.SetParent(parent, false);
                RectTransform slotRect = slotGO.AddComponent<RectTransform>();
                slotRect.anchorMin = new Vector2(minX, 0f);
                slotRect.anchorMax = new Vector2(maxX, 1f);
                slotRect.offsetMin = Vector2.zero;
                slotRect.offsetMax = Vector2.zero;

                // Slot background / button target
                Button btn = slotGO.AddComponent<Button>();
                Image btnImg = slotGO.AddComponent<Image>();
                btnImg.color = isMainHub ? new Color(0.20f, 0.15f, 0.08f, 0.70f) : Color.clear;
                btn.targetGraphic = btnImg;
                if (i == 0) // Slot 0: Túi Đồ is not implemented yet; make non-interactable
                {
                    btn.interactable = false;
                }
                navButtons[i] = btn;

                // Active indicator (bar at bottom of slot)
                GameObject indicatorGO = CreateUIRect(slotGO.transform, "ActiveIndicator", new Vector2(0.15f, 0f), new Vector2(0.85f, 0f), new Vector2(0f, 4f), new Vector2(0f, 4f));
                Image indicatorImg = CreateUIImage(indicatorGO.transform, "Bar", isMainHub ? UIStyleConfig.GoldAccent : UIStyleConfig.CyanHighlight);
                indicatorGO.SetActive(i == GlobalBottomNavigation.MainHubIndex);
                navActiveIndicators[i] = indicatorGO;

                // Icon
                GameObject iconGO = CreateUIRect(slotGO.transform, "Icon", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, isMainHub ? 16f : 12f), isMainHub ? new Vector2(50f, 50f) : new Vector2(42f, 42f));
                Image iconImg = iconGO.AddComponent<Image>();
                iconImg.sprite = UIProceduralTextureFactory.GetIconSprite(slotIconKeys[i]);
                iconImg.color = isMainHub ? UIStyleConfig.GoldAccent : (i == 0 ? new Color(0.4f, 0.4f, 0.4f, 0.5f) : UIStyleConfig.IconMuted);
                iconImg.raycastTarget = false;
                navIcons[i] = iconImg;

                // Label
                GameObject labelGO = CreateUIRect(slotGO.transform, "Label", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 18f), new Vector2(0f, 26f));
                TextMeshProUGUI labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
                labelTmp.text = slotLabels[i];
                labelTmp.fontSize = isMainHub ? 15 : 13;
                labelTmp.fontStyle = isMainHub ? FontStyles.Bold : FontStyles.Normal;
                labelTmp.alignment = TextAlignmentOptions.Center;
                labelTmp.color = isMainHub ? UIStyleConfig.GoldAccent : (i == 0 ? new Color(0.4f, 0.4f, 0.4f, 0.5f) : UIStyleConfig.TextMuted);
                labelTmp.raycastTarget = false;
                navLabels[i] = labelTmp;
            }

            navComp.SetReferences(navButtons, navIcons, navLabels, navActiveIndicators);
            SerializedObject navSO = new SerializedObject(navComp);
            navSO.Update();
            for (int i = 0; i < GlobalBottomNavigation.SlotCount; i++)
            {
                navSO.FindProperty("navButtons").GetArrayElementAtIndex(i).objectReferenceValue = navButtons[i];
                navSO.FindProperty("navIcons").GetArrayElementAtIndex(i).objectReferenceValue = navIcons[i];
                navSO.FindProperty("navLabels").GetArrayElementAtIndex(i).objectReferenceValue = navLabels[i];
                navSO.FindProperty("navActiveIndicators").GetArrayElementAtIndex(i).objectReferenceValue = navActiveIndicators[i];
            }
            navSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateWorldLabel(Transform parent, string text, Vector3 localPos, Color color)
        {
            GameObject go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            TextMeshPro tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = 5;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.sortingOrder = 50;
        }

        private static SkillDefinitionSO GetOrCreateSkillDef(
            string path,
            string id,
            string mmId,
            SkillSlotType slot,
            string name,
            string desc,
            List<SkillUnlockRequirement> conditions,
            float dmgMultiplier,
            float costRage,
            float cd,
            bool isPassive,
            int priority = 0)
        {
            SkillDefinitionSO asset = AssetDatabase.LoadAssetAtPath<SkillDefinitionSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.InitializeSkill(id, mmId, slot, name, desc, conditions, dmgMultiplier, costRage, cd, isPassive, null, false, 0f, false, 0f, 0f, priority);

            if (!isPassive && dmgMultiplier > 0f)
            {
                var dmgEffect = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
                dmgEffect.name = $"{id}_DamageEffect";
                dmgEffect.Initialize(dmgMultiplier, SkillTargetPolicy.SingleTarget, DamageType.Skill);
                AssetDatabase.AddObjectToAsset(dmgEffect, asset);
                asset.SetEffects(new List<SkillEffectDefinitionSO> { dmgEffect });
            }
            else
            {
                asset.ClearEffects();
            }

            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static MindMethodDefinitionSO GetOrCreateMindMethodDef(
            string path,
            string id,
            string name,
            string desc,
            int maxLvl,
            bool unlockedByDefault,
            MindMethodPassiveData passive,
            List<MindMethodUnlockRequirement> conditions,
            List<SkillDefinitionSO> skills)
        {
            MindMethodDefinitionSO asset = AssetDatabase.LoadAssetAtPath<MindMethodDefinitionSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<MindMethodDefinitionSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.InitializeMindMethod(id, name, desc, maxLvl, unlockedByDefault, passive, conditions, skills);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static MindMethodDatabaseSO GetOrCreateMindMethodDatabase(string path)
        {
            MindMethodDatabaseSO db = AssetDatabase.LoadAssetAtPath<MindMethodDatabaseSO>(path);
            string parentDir = Path.GetDirectoryName(path).Replace("\\", "/");
            string mmDir = $"{parentDir}/MindMethods";
            string skDir = $"{parentDir}/Skills";

            if (!AssetDatabase.IsValidFolder(mmDir)) AssetDatabase.CreateFolder(parentDir, "MindMethods");
            if (!AssetDatabase.IsValidFolder(skDir)) AssetDatabase.CreateFolder(parentDir, "Skills");

            // --- 1. Thái Cực Thần Công (mm_taiji) ---
            var taijiSkills = new List<SkillDefinitionSO>
            {
                // Slot 1: Basic (Priority 0)
                GetOrCreateSkillDef($"{skDir}/skill_taiji_1_a.asset", "skill_taiji_1_a", "mm_taiji", SkillSlotType.NormalAttack, "Thái Cực Quyền", "Thế quyền nhu hòa tá lực đả lực, không tốn Nộ.", null, 1.0f, 0f, 0f, false, 0),
                GetOrCreateSkillDef($"{skDir}/skill_taiji_1_b.asset", "skill_taiji_1_b", "mm_taiji", SkillSlotType.NormalAttack, "Nhu Quyền Biến Hóa", "Biến chiêu liên hoàn tăng uy lực đòn đánh thường.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 3) }, 1.15f, 0f, 0f, false, 0),

                // Slot 2: Tuyệt Kỹ (Priority 60)
                GetOrCreateSkillDef($"{skDir}/skill_taiji_2_a.asset", "skill_taiji_2_a", "mm_taiji", SkillSlotType.Skill, "Bát Quái Chưởng", "Chưởng phong tuần hoàn theo đồ hình Bát Quái gây 150% sát thương.", null, 1.5f, 30f, 3f, false, 60),
                GetOrCreateSkillDef($"{skDir}/skill_taiji_2_b.asset", "skill_taiji_2_b", "mm_taiji", SkillSlotType.Skill, "Vân Thủ Hóa Kình", "Chưởng thế như mây cuộn hóa giải công kích gây 170% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 5) }, 1.7f, 35f, 3.5f, false, 60),

                // Slot 3: Ngoại Công 1 (Priority 40)
                GetOrCreateSkillDef($"{skDir}/skill_taiji_3_a.asset", "skill_taiji_3_a", "mm_taiji", SkillSlotType.ExternalSkill1, "Thái Cực Kiếm", "Kiếm ý liên miên bất tuyệt gây 180% sát thương.", null, 1.8f, 40f, 5f, false, 40),
                GetOrCreateSkillDef($"{skDir}/skill_taiji_3_b.asset", "skill_taiji_3_b", "mm_taiji", SkillSlotType.ExternalSkill1, "Thuần Dương Kiếm Pháp", "Kiếm khí cương nhu tịnh tế gây 210% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 8) }, 2.1f, 45f, 5f, false, 40),

                // Slot 4: Ngoại Công 2 (Priority 30)
                GetOrCreateSkillDef($"{skDir}/skill_taiji_4_a.asset", "skill_taiji_4_a", "mm_taiji", SkillSlotType.ExternalSkill2, "Lãnh Kình Bộc Phát", "Phát kình bất ngờ khiến kẻ địch tổn thương 220% sát thương.", null, 2.2f, 50f, 6f, false, 30),
                GetOrCreateSkillDef($"{skDir}/skill_taiji_4_b.asset", "skill_taiji_4_b", "mm_taiji", SkillSlotType.ExternalSkill2, "Triền Ty Kình", "Nội kình quấn xoắn như tơ nhện gây 260% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 10) }, 2.6f, 55f, 6.5f, false, 30),

                // Slot 5: Ultimate (Priority 100)
                GetOrCreateSkillDef($"{skDir}/skill_taiji_5_a.asset", "skill_taiji_5_a", "mm_taiji", SkillSlotType.Ultimate, "Thái Cực Vô Cực", "Tuyệt đỉnh Thái Cực dung hòa trời đất gây 350% sát thương cực đại.", null, 3.5f, 100f, 10f, false, 100),
                GetOrCreateSkillDef($"{skDir}/skill_taiji_5_b.asset", "skill_taiji_5_b", "mm_taiji", SkillSlotType.Ultimate, "Âm Dương Quy Nhất", "Âm dương hợp nhất hủy thiên diệt địa gây 420% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.MindMethodLevel, 5) }, 4.2f, 100f, 12f, false, 100)
            };

            MindMethodPassiveData taijiPassive = new MindMethodPassiveData(
                hp: 200f, atk: 20f, def: 30f, critRate: 2f, critDmg: 0f, dodge: 5f, moveSpeed: 0f,
                basicRageMod: 0f, dmgRageMod: 1f, rageMult: 1f,
                reviveOnce: true, reviveHpPct: 30f);

            MindMethodDefinitionSO mmTaiji = GetOrCreateMindMethodDef(
                $"{mmDir}/MindMethod_Taiji.asset", "mm_taiji", "Thái Cực Thần Công",
                "Tâm pháp Đạo gia dĩ nhu chế cương, hộ thể trường cửu, sở hữu nội tại Quy Nhất Hồi Sinh khi nguy cấp.",
                10, true, taijiPassive, null, taijiSkills);

            // --- 2. Cửu Dương Thần Công (mm_nine_yang) ---
            var nineYangSkills = new List<SkillDefinitionSO>
            {
                // Slot 1
                GetOrCreateSkillDef($"{skDir}/skill_ny_1_a.asset", "skill_ny_1_a", "mm_nine_yang", SkillSlotType.NormalAttack, "Cửu Dương Liệt Hỏa Quyền", "Quyền mang nhiệt khí thiêu đốt địch nhân.", null, 1.1f, 0f, 0f, false),
                GetOrCreateSkillDef($"{skDir}/skill_ny_1_b.asset", "skill_ny_1_b", "mm_nine_yang", SkillSlotType.NormalAttack, "Viêm Dương Quyền", "Viêm hỏa bao bọc nắm đấm tăng mạnh uy lực.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 12) }, 1.25f, 0f, 0f, false),

                // Slot 2: Tuyệt Kỹ (Priority 60)
                GetOrCreateSkillDef($"{skDir}/skill_ny_2_a.asset", "skill_ny_2_a", "mm_nine_yang", SkillSlotType.Skill, "Liệt Diễm Chưởng", "Chưởng khí bốc cháy gây 160% sát thương.", null, 1.6f, 30f, 3f, false, 60),
                GetOrCreateSkillDef($"{skDir}/skill_ny_2_b.asset", "skill_ny_2_b", "mm_nine_yang", SkillSlotType.Skill, "Cửu Dương Phần Thiên", "Nhiệt lượng tỏa khắp bốn phương gây 190% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 15) }, 1.9f, 35f, 3.5f, false, 60),

                // Slot 3: Ngoại Công 1 (Priority 40)
                GetOrCreateSkillDef($"{skDir}/skill_ny_3_a.asset", "skill_ny_3_a", "mm_nine_yang", SkillSlotType.ExternalSkill1, "Xích Diễm Chỉ", "Chỉ lực nóng rực xuyên phá phòng ngự gây 200% sát thương.", null, 2.0f, 40f, 5f, false, 40),
                GetOrCreateSkillDef($"{skDir}/skill_ny_3_b.asset", "skill_ny_3_b", "mm_nine_yang", SkillSlotType.ExternalSkill1, "Dương Viêm Đao Pháp", "Đao khí rực lửa trảm kích gây 240% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 18) }, 2.4f, 45f, 5f, false, 40),

                // Slot 4: Ngoại Công 2 (Priority 30)
                GetOrCreateSkillDef($"{skDir}/skill_ny_4_a.asset", "skill_ny_4_a", "mm_nine_yang", SkillSlotType.ExternalSkill2, "Cửu Dương Hộ Thể Cương Khí", "Cương khí nóng rực đẩy lùi đối thủ gây 250% sát thương.", null, 2.5f, 50f, 6f, false, 30),
                GetOrCreateSkillDef($"{skDir}/skill_ny_4_b.asset", "skill_ny_4_b", "mm_nine_yang", SkillSlotType.ExternalSkill2, "Phần Thiên Chấn Kình", "Chấn động nhiệt khí bùng nổ gây 300% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 22) }, 3.0f, 55f, 6.5f, false, 30),

                // Slot 5: Ultimate (Priority 100)
                GetOrCreateSkillDef($"{skDir}/skill_ny_5_a.asset", "skill_ny_5_a", "mm_nine_yang", SkillSlotType.Ultimate, "Cửu Dương Phổ Chiếu", "Chí dương thần công bùng nổ như vầng thái dương gây 380% sát thương.", null, 3.8f, 100f, 10f, false, 100),
                GetOrCreateSkillDef($"{skDir}/skill_ny_5_b.asset", "skill_ny_5_b", "mm_nine_yang", SkillSlotType.Ultimate, "Thiên Địa Viêm Long", "Hóa chân khí thành Viêm Long hủy diệt tất cả gây 460% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.MindMethodLevel, 5) }, 4.6f, 100f, 12f, false, 100)
            };

            MindMethodPassiveData nyPassive = new MindMethodPassiveData(
                hp: 100f, atk: 60f, def: 15f, critRate: 6f, critDmg: 25f, dodge: 0f, moveSpeed: 0f,
                basicRageMod: 1f, dmgRageMod: 0f, rageMult: 1f,
                reviveOnce: false, reviveHpPct: 0f);

            var nyConditions = new List<MindMethodUnlockRequirement>
            {
                new MindMethodUnlockRequirement(BreakthroughRequirementType.HeroLevel, 10, "", "Yêu cầu Hero đạt Cấp 10")
            };

            MindMethodDefinitionSO mmNineYang = GetOrCreateMindMethodDef(
                $"{mmDir}/MindMethod_NineYang.asset", "mm_nine_yang", "Cửu Dương Thần Công",
                "Chí dương chí cương thần công, tích lũy nộ khí cuồn cuộn khi công kích và cường hóa sát thương bạo kích mãnh liệt.",
                10, false, nyPassive, nyConditions, nineYangSkills);

            // --- 3. Cửu Âm Chân Kinh (mm_nine_yin) ---
            var nineYinSkills = new List<SkillDefinitionSO>
            {
                // Slot 1: Basic (Priority 0)
                GetOrCreateSkillDef($"{skDir}/skill_nyin_1_a.asset", "skill_nyin_1_a", "mm_nine_yin", SkillSlotType.NormalAttack, "Cửu Âm Bạch Cốt Trảo", "Trảo pháp âm hàn quỷ dị xuyên phá hộ thân.", null, 1.15f, 0f, 0f, false, 0),
                GetOrCreateSkillDef($"{skDir}/skill_nyin_1_b.asset", "skill_nyin_1_b", "mm_nine_yin", SkillSlotType.NormalAttack, "U Minh Đoạt Hồn Trảo", "Trảo thế biến ảo khôn lường gây sát thương lớn.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 25) }, 1.3f, 0f, 0f, false, 0),

                // Slot 2: Tuyệt Kỹ (Priority 60)
                GetOrCreateSkillDef($"{skDir}/skill_nyin_2_a.asset", "skill_nyin_2_a", "mm_nine_yin", SkillSlotType.Skill, "Đại Ma Bàn Chưởng", "Chưởng lực âm lãnh như cối xay nghiền nát đối thủ gây 170% sát thương.", null, 1.7f, 30f, 3f, false, 60),
                GetOrCreateSkillDef($"{skDir}/skill_nyin_2_b.asset", "skill_nyin_2_b", "mm_nine_yin", SkillSlotType.Skill, "Huyền Âm Thần Chưởng", "Âm khí thấu cốt gây 200% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 28) }, 2.0f, 35f, 3.5f, false, 60),

                // Slot 3: Ngoại Công 1 (Priority 40)
                GetOrCreateSkillDef($"{skDir}/skill_nyin_3_a.asset", "skill_nyin_3_a", "mm_nine_yin", SkillSlotType.ExternalSkill1, "Tồi Tâm Chưởng Pháp", "Chưởng lực xuyên thấu nội tạng gây 210% sát thương.", null, 2.1f, 40f, 5f, false, 40),
                GetOrCreateSkillDef($"{skDir}/skill_nyin_3_b.asset", "skill_nyin_3_b", "mm_nine_yin", SkillSlotType.ExternalSkill1, "Hàn Băng Kiếm Quyết", "Kiếm pháp băng hàn đông kết địch thủ gây 250% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 32) }, 2.5f, 45f, 5f, false, 40),

                // Slot 4: Ngoại Công 2 (Priority 30)
                GetOrCreateSkillDef($"{skDir}/skill_nyin_4_a.asset", "skill_nyin_4_a", "mm_nine_yin", SkillSlotType.ExternalSkill2, "Xà Hành Trảo Pháp", "Thân pháp như rắn trườn né tránh và phản kích gây 270% sát thương.", null, 2.7f, 50f, 6f, false, 30),
                GetOrCreateSkillDef($"{skDir}/skill_nyin_4_b.asset", "skill_nyin_4_b", "mm_nine_yin", SkillSlotType.ExternalSkill2, "Cửu Âm Du Thân Chưởng", "Biến hóa thần tốc tung đòn liên tiếp gây 320% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.HeroLevel, 35) }, 3.2f, 55f, 6.5f, false, 30),

                // Slot 5: Ultimate (Priority 100)
                GetOrCreateSkillDef($"{skDir}/skill_nyin_5_a.asset", "skill_nyin_5_a", "mm_nine_yin", SkillSlotType.Ultimate, "Cửu Âm Thần Trảo", "Tuyệt học đỉnh cao Cửu Âm xé rách hư không gây 400% sát thương.", null, 4.0f, 100f, 10f, false, 100),
                GetOrCreateSkillDef($"{skDir}/skill_nyin_5_b.asset", "skill_nyin_5_b", "mm_nine_yin", SkillSlotType.Ultimate, "Vạn Khiếu Huyền Âm Quyết", "Toàn thân phát xuất âm hàn khí kình hủy diệt vạn vật gây 480% sát thương.", new List<SkillUnlockRequirement>{ new SkillUnlockRequirement(SkillRequirementType.MindMethodLevel, 5) }, 4.8f, 100f, 12f, false, 100)
            };

            MindMethodPassiveData nyinPassive = new MindMethodPassiveData(
                hp: 150f, atk: 80f, def: 20f, critRate: 8f, critDmg: 30f, dodge: 10f, moveSpeed: 0.5f,
                basicRageMod: 1f, dmgRageMod: 1f, rageMult: 1.2f,
                reviveOnce: false, reviveHpPct: 0f);

            var nyinConditions = new List<MindMethodUnlockRequirement>
            {
                new MindMethodUnlockRequirement(BreakthroughRequirementType.HeroLevel, 20, "", "Yêu cầu Hero đạt Cấp 20"),
                new MindMethodUnlockRequirement(BreakthroughRequirementType.LootTier, 3, "", "Yêu cầu Cấp Rơi (LootTier) đạt Bậc 3")
            };

            MindMethodDefinitionSO mmNineYin = GetOrCreateMindMethodDef(
                $"{mmDir}/MindMethod_NineYin.asset", "mm_nine_yin", "Cửu Âm Chân Kinh",
                "Chân kinh chí âm vô thượng võ lâm, gia tăng né tránh, tốc độ di chuyển và sát thương chí mạng phi phàm.",
                10, false, nyinPassive, nyinConditions, nineYinSkills);

            if (db == null)
            {
                db = ScriptableObject.CreateInstance<MindMethodDatabaseSO>();
                AssetDatabase.CreateAsset(db, path);
            }

            db.SetMindMethods(new List<MindMethodDefinitionSO> { mmTaiji, mmNineYang, mmNineYin });
            EditorUtility.SetDirty(db);
            return db;
        }

        [MenuItem("Tools/Wuxia RPG/Reset Progression State (PlayerPrefs)")]
        public static void ResetProgressionStatePlayerPrefs()
        {
            PlayerPrefs.DeleteKey("TLTD_Title_StageIndex");
            PlayerPrefs.DeleteKey("TLTD_Hero_Level");
            PlayerPrefs.DeleteKey("TLTD_Hero_Exp");
            PlayerPrefs.DeleteKey("TLTD_Hero_TitleIndex");
            PlayerPrefs.DeleteKey("TLTD_MM_ActiveId");
            PlayerPrefs.Save();
            Debug.Log("[Wuxia RPG] Progression PlayerPrefs successfully reset to default initial state (BreakthroughCount=0, Level=1, Cap=5, ActiveMindMethod=Default).");
        }
    }
}
#endif
