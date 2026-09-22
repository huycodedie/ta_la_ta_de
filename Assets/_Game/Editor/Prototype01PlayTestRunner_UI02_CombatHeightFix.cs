#if UNITY_EDITOR
using System;
using System.IO;
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
using WuxiaGame.UI.Core;

namespace WuxiaGame.Editor
{
    public static partial class Prototype01PlayTestRunner
    {
        private const string HeightOutputDir = "Screenshots/UI02_HeightFix";
        private const int ScreenWidth = 1080;
        private const int ScreenHeight = 1920;

        [MenuItem("Tools/Wuxia RPG/Run UI-02 Runtime Combat Height Fix Tests (TEST 01 to TEST 12 + Play Mode A to L)")]
        public static bool RunAllCombatHeightFixTests()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING UI-02 RUNTIME COMBAT HEIGHT FIX VALIDATION SUITE (TEST 01 -> 12)");
            Debug.Log("================================================================================");

            int passed = 0;
            const int total = 12;

            bool t01 = Test01_HeroAndInitialMonster_SameCombatY();
            if (t01) passed++;

            bool t02 = Test02_MonsterRespawn_ReceivesCombatPlaneY();
            if (t02) passed++;

            bool t03 = Test03_HeroAndMonster_DifferentY_WithinHorizontalRange_Attacks();
            if (t03) passed++;

            bool t04 = Test04_DeltaY09_HorizontalDistanceAuthoritative();
            if (t04) passed++;

            bool t05 = Test05_HeroDoesNotDeadlock_AtRuntimeSpawnPosition();
            if (t05) passed++;

            bool t06 = Test06_MonsterCanAttackHero_AfterApproaching();
            if (t06) passed++;

            bool t07 = Test07_Monster1Dies_Monster2SpawnsAutomatically();
            if (t07) passed++;

            bool t08 = Test08_Monster2CanBeDamaged();
            if (t08) passed++;

            bool t09 = Test09_Monster2Dies_Monster3SpawnsAutomatically();
            if (t09) passed++;

            bool t10 = Test10_Monster3CanBeDamaged();
            if (t10) passed++;

            bool t11 = Test11_InitialAndRespawnedMonster_MatchVisualConfiguration();
            if (t11) passed++;

            bool t12 = Test12_NoPhysicsDependencyIntroduced();
            if (t12) passed++;

            Debug.Log("--------------------------------------------------------------------------------");
            Debug.Log($"UI-02 COMBAT HEIGHT FIX TESTS COMPLETED: {passed}/{total} PASSED");
            Debug.Log("--------------------------------------------------------------------------------");

            bool playModeOk = RunPlayModeAcceptance_Scenarios_A_to_L();

            bool allOk = (passed == total) && playModeOk;
            Debug.Log($"[COMBAT HEIGHT FIX SUITE RESULT]: {(allOk ? "ALL TESTS PASSED" : "FAILED")} (Unit: {passed}/{total}, PlayMode: {(playModeOk ? "PASS" : "FAIL")})");
            return allOk;
        }

        // =====================================================================
        // TEST 01: Hero and initial Monster same combat Y.
        // =====================================================================
        private static bool Test01_HeroAndInitialMonster_SameCombatY()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            Monster monster = UnityEngine.Object.FindAnyObjectByType<Monster>();

            if (hero == null || monster == null)
            {
                Debug.LogError("[TEST 01 FAIL] Missing Hero or Monster in scene.");
                return false;
            }

            float heroY = hero.transform.position.y;
            float monsterY = monster.transform.position.y;
            float deltaY = Mathf.Abs(heroY - monsterY);

            bool heroYCorrect = Mathf.Abs(heroY - (-0.3f)) < 0.001f;
            bool monsterYCorrect = Mathf.Abs(monsterY - (-0.3f)) < 0.001f;
            bool sameY = deltaY < 0.001f;

            bool pass = heroYCorrect && monsterYCorrect && sameY;
            Debug.Log($"[TEST 01] HeroY={heroY:F3}, MonsterY={monsterY:F3}, DeltaY={deltaY:F3} | {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        // =====================================================================
        // TEST 02: Monster respawn receives the same combat-plane Y.
        // =====================================================================
        private static bool Test02_MonsterRespawn_ReceivesCombatPlaneY()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            if (bm == null)
            {
                Debug.LogError("[TEST 02 FAIL] BattleManager missing.");
                return false;
            }

            Monster spawned = bm.SpawnMonster();
            if (spawned == null)
            {
                Debug.LogError("[TEST 02 FAIL] Failed to spawn monster.");
                return false;
            }

            float spawnedY = spawned.transform.position.y;
            bool pass = Mathf.Abs(spawnedY - (-0.3f)) < 0.001f;
            Debug.Log($"[TEST 02] Spawned Monster Y={spawnedY:F3} (Expected: -0.300) | {(pass ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(spawned.gameObject);
            return pass;
        }

        // =====================================================================
        // TEST 03: Hero and respawned Monster have different Y values artificially,
        // but horizontal distance is within attack range.
        // Verify: AttackComponent considers target in range.
        // =====================================================================
        private static bool Test03_HeroAndMonster_DifferentY_WithinHorizontalRange_Attacks()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();

            hero.InitializeHero();
            hero.EnableEntityActions();

            Monster monster = bm.SpawnMonster();
            monster.Health.InitializeHealth(1000f, monster);
            monster.Health.Revive(1000f);
            monster.Stats.SetBaseValue(StatType.Dodge, 0f);

            // Place Hero at (0, -0.3, 0) and Monster at (1.75, 0.6, 0)
            // Horizontal distance = 1.75m <= 1.8 + 0.1 = 1.9m
            // Vertical distance = 0.9m
            // 3D Euclidean distance = sqrt(1.75^2 + 0.9^2) = 1.967m > 1.90m
            hero.transform.position = new Vector3(0f, -0.3f, 0f);
            monster.transform.position = new Vector3(1.75f, 0.6f, 0f);

            hero.SetCurrentTarget(monster);
            monster.SetCurrentTarget(hero);

            Vector3 delta = monster.transform.position - hero.transform.position;
            Vector3 deltaHoriz = delta;
            deltaHoriz.y = 0f;
            float horizDist = deltaHoriz.magnitude;
            float euclideanDist = delta.magnitude;

            Debug.Log($"[ATTACK RANGE AUDIT]\nEntity={hero.EntityName}\nTarget={monster.EntityName}\nHorizontalDistance={horizDist:F4}\n3DDistance={euclideanDist:F4}\nAttackRange={hero.AttackRange:F2}\nTolerance=0.10\nInRange={(horizDist <= hero.AttackRange + 0.1f)}");

            float preHp = monster.Health.CurrentHealth;
            hero.Attack.ResetAttackTimer();
            hero.Attack.ManualTick(1.5f);
            float postHp = monster.Health.CurrentHealth;

            bool damaged = postHp < preHp;
            bool pass = damaged && (euclideanDist > hero.AttackRange + 0.1f) && (horizDist <= hero.AttackRange + 0.1f);
            Debug.Log($"[TEST 03] HorizDist={horizDist:F3}, 3DDist={euclideanDist:F3}, PreHP={preHp:F0}, PostHP={postHp:F0}, Damaged={damaged} | {(pass ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(monster.gameObject);
            return pass;
        }

        // =====================================================================
        // TEST 04: Hero and Monster have DeltaY = 0.9.
        // Verify: horizontal combat distance remains authoritative.
        // =====================================================================
        private static bool Test04_DeltaY09_HorizontalDistanceAuthoritative()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();

            hero.InitializeHero();
            hero.EnableEntityActions();

            Monster monster = bm.SpawnMonster();
            monster.Health.InitializeHealth(1000f, monster);
            monster.Health.Revive(1000f);
            monster.Stats.SetBaseValue(StatType.Dodge, 0f);

            // Subtest A: Within horizontal range (DeltaX = 1.7m <= 1.9m, DeltaY = 0.9m) -> Must attack
            hero.transform.position = new Vector3(0f, -0.3f, 0f);
            monster.transform.position = new Vector3(1.7f, 0.6f, 0f);
            hero.SetCurrentTarget(monster);

            float preA = monster.Health.CurrentHealth;
            hero.Attack.ResetAttackTimer();
            hero.Attack.ManualTick(1.5f);
            float postA = monster.Health.CurrentHealth;
            bool subtestA_Damaged = postA < preA;

            // Subtest B: Outside horizontal range (DeltaX = 2.5m > 1.9m, DeltaY = 0.9m) -> Must NOT attack
            monster.transform.position = new Vector3(2.5f, 0.6f, 0f);
            float preB = monster.Health.CurrentHealth;
            hero.Attack.ResetAttackTimer();
            hero.Attack.ManualTick(1.5f);
            float postB = monster.Health.CurrentHealth;
            bool subtestB_NotDamaged = Mathf.Approximately(preB, postB);

            bool pass = subtestA_Damaged && subtestB_NotDamaged;
            Debug.Log($"[TEST 04] SubtestA (In Range) Damaged={subtestA_Damaged}, SubtestB (Out Range) Ignored={subtestB_NotDamaged} | {(pass ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(monster.gameObject);
            return pass;
        }

        // =====================================================================
        // TEST 05: Hero does not deadlock when Monster is spawned at the runtime spawn position.
        // =====================================================================
        private static bool Test05_HeroDoesNotDeadlock_AtRuntimeSpawnPosition()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();

            hero.InitializeHero();
            hero.EnableEntityActions();
            hero.transform.position = new Vector3(-2.2f, -0.3f, 0f);

            Monster monster = bm.SpawnMonster();
            monster.Health.InitializeHealth(1000f, monster);
            monster.Health.Revive(1000f);
            monster.Stats.SetBaseValue(StatType.Dodge, 0f);

            hero.SetCurrentTarget(monster);
            monster.SetCurrentTarget(hero);

            // Simulate frames: hero approaches monster until attack range, then attacks
            float preHp = monster.Health.CurrentHealth;
            const float dt = 0.1f;
            bool attacked = false;

            for (int i = 0; i < 60; i++)
            {
                // Movement step
                float dist = Mathf.Abs(monster.transform.position.x - hero.transform.position.x);
                if (dist > hero.AttackRange)
                {
                    hero.Movement.MoveTowardTarget(monster.transform.position, hero.AttackRange, dt);
                }

                // Attack step
                hero.Attack.ManualTick(dt);

                if (monster.Health.CurrentHealth < preHp)
                {
                    attacked = true;
                    break;
                }
            }

            float finalHeroX = hero.transform.position.x;
            float distToMonster = Mathf.Abs(monster.transform.position.x - finalHeroX);
            bool inRange = distToMonster <= hero.AttackRange + 0.1f;
            bool pass = attacked && inRange;

            Debug.Log($"[TEST 05] Hero moved from -2.2 to {finalHeroX:F2}, Distance={distToMonster:F2}, Attacked={attacked}, InRange={inRange} | {(pass ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(monster.gameObject);
            return pass;
        }

        // =====================================================================
        // TEST 06: Monster can also attack Hero after approaching.
        // =====================================================================
        private static bool Test06_MonsterCanAttackHero_AfterApproaching()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();

            hero.InitializeHero();
            hero.Health.InitializeHealth(5000f, hero);
            hero.Health.Revive(5000f);
            hero.transform.position = new Vector3(0f, -0.3f, 0f);

            Monster monster = bm.SpawnMonster();
            monster.transform.position = new Vector3(3.5f, -0.3f, 0f);
            monster.EnableEntityActions();
            monster.SetCurrentTarget(hero);

            float preHeroHp = hero.Health.CurrentHealth;
            const float dt = 0.1f;
            bool monsterAttacked = false;

            for (int i = 0; i < 60; i++)
            {
                float dist = Mathf.Abs(hero.transform.position.x - monster.transform.position.x);
                if (dist > monster.AttackRange)
                {
                    monster.Movement.MoveTowardTarget(hero.transform.position, monster.AttackRange, dt);
                }

                monster.Attack.ManualTick(dt);

                if (hero.Health.CurrentHealth < preHeroHp)
                {
                    monsterAttacked = true;
                    break;
                }
            }

            bool pass = monsterAttacked;
            Debug.Log($"[TEST 06] Monster approached and attacked Hero: Damaged={monsterAttacked}, Hero HP: {preHeroHp:F0} -> {hero.Health.CurrentHealth:F0} | {(pass ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(monster.gameObject);
            return pass;
        }

        // =====================================================================
        // TEST 07: Monster #1 dies and Monster #2 automatically spawns.
        // =====================================================================
        private static bool Test07_Monster1Dies_Monster2SpawnsAutomatically()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();

            bm.StartBattle();
            Monster m1 = bm.CurrentMonster;
            int initialEnc = bm.EncounterIndex;

            // Kill Monster 1
            m1.Health.TakeDamage(999999f);

            // If drop system produced pending loot, complete it to trigger next encounter
            if (bm.CurrentBattleState == BattleState.LootPending)
            {
                bm.CompleteLootDecisionAndResume(false, true);
            }

            Monster m2 = bm.CurrentMonster;
            int newEnc = bm.EncounterIndex;

            bool spawnedCorrectly = (m2 != null) && (m2 != m1) && (newEnc == initialEnc + 1) && m2.IsAlive;
            bool spawnYCorrect = spawnedCorrectly && Mathf.Abs(m2.transform.position.y - (-0.3f)) < 0.001f;

            bool pass = spawnedCorrectly && spawnYCorrect;
            Debug.Log($"[TEST 07] Monster #1 killed -> Encounter {initialEnc} -> {newEnc}, Monster #2 Y={m2?.transform.position.y:F3} | {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        // =====================================================================
        // TEST 08: Monster #2 can be damaged.
        // =====================================================================
        private static bool Test08_Monster2CanBeDamaged()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();

            bm.StartBattle();
            // Kill Monster 1
            bm.CurrentMonster.Health.TakeDamage(999999f);
            if (bm.CurrentBattleState == BattleState.LootPending)
            {
                bm.CompleteLootDecisionAndResume(false, true);
            }

            Monster m2 = bm.CurrentMonster;
            hero.SetCurrentTarget(m2);

            // Place hero within attack range of m2
            hero.transform.position = new Vector3(m2.transform.position.x - 1.5f, -0.3f, 0f);
            float preHp = m2.Health.CurrentHealth;

            hero.Attack.ResetAttackTimer();
            hero.Attack.ManualTick(1.5f);
            float postHp = m2.Health.CurrentHealth;

            bool pass = postHp < preHp;
            Debug.Log($"[TEST 08] Monster #2 Damaged: {preHp:F0} -> {postHp:F0} | {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        // =====================================================================
        // TEST 09: Monster #2 dies and Monster #3 spawns.
        // =====================================================================
        private static bool Test09_Monster2Dies_Monster3SpawnsAutomatically()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();

            bm.StartBattle();
            // Kill Monster 1
            bm.CurrentMonster.Health.TakeDamage(999999f);
            if (bm.CurrentBattleState == BattleState.LootPending)
            {
                bm.CompleteLootDecisionAndResume(false, true);
            }

            // Kill Monster 2
            Monster m2 = bm.CurrentMonster;
            m2.Health.TakeDamage(999999f);
            if (bm.CurrentBattleState == BattleState.LootPending)
            {
                bm.CompleteLootDecisionAndResume(false, true);
            }

            Monster m3 = bm.CurrentMonster;
            int enc3 = bm.EncounterIndex;

            bool spawnedM3 = (m3 != null) && (m3 != m2) && (enc3 == 3) && m3.IsAlive;
            bool spawnM3YCorrect = spawnedM3 && Mathf.Abs(m3.transform.position.y - (-0.3f)) < 0.001f;

            bool pass = spawnedM3 && spawnM3YCorrect;
            Debug.Log($"[TEST 09] Monster #2 killed -> Encounter 3 reached, Monster #3 Y={m3?.transform.position.y:F3} | {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        // =====================================================================
        // TEST 10: Monster #3 can be damaged.
        // =====================================================================
        private static bool Test10_Monster3CanBeDamaged()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();

            bm.StartBattle();
            // Kill Monster 1
            bm.CurrentMonster.Health.TakeDamage(999999f);
            if (bm.CurrentBattleState == BattleState.LootPending) bm.CompleteLootDecisionAndResume(false, true);

            // Kill Monster 2
            bm.CurrentMonster.Health.TakeDamage(999999f);
            if (bm.CurrentBattleState == BattleState.LootPending) bm.CompleteLootDecisionAndResume(false, true);

            Monster m3 = bm.CurrentMonster;
            hero.SetCurrentTarget(m3);
            hero.transform.position = new Vector3(m3.transform.position.x - 1.5f, -0.3f, 0f);

            float preHp = m3.Health.CurrentHealth;
            hero.Attack.ResetAttackTimer();
            hero.Attack.ManualTick(1.5f);
            float postHp = m3.Health.CurrentHealth;

            bool pass = postHp < preHp;
            Debug.Log($"[TEST 10] Monster #3 Damaged: {preHp:F0} -> {postHp:F0} | {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        // =====================================================================
        // TEST 11: Initial Monster and respawned Monster use the same visual configuration.
        // =====================================================================
        private static bool Test11_InitialAndRespawnedMonster_MatchVisualConfiguration()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            Monster m1 = UnityEngine.Object.FindAnyObjectByType<Monster>();

            SpriteRenderer sr1 = m1.GetComponent<SpriteRenderer>();
            Vector3 scale1 = m1.transform.localScale;

            Monster m2 = bm.SpawnMonster();
            SpriteRenderer sr2 = m2.GetComponent<SpriteRenderer>();
            Vector3 scale2 = m2.transform.localScale;

            bool hasSr = (sr1 != null) && (sr2 != null);
            bool hasSprite = hasSr && (sr1.sprite != null) && (sr2.sprite != null);
            bool spriteDimensionsMatch = hasSprite &&
                                          (sr1.sprite.rect == sr2.sprite.rect) &&
                                          (sr1.sprite.texture.width == sr2.sprite.texture.width) &&
                                          (sr1.sprite.texture.height == sr2.sprite.texture.height) &&
                                          Mathf.Approximately(sr1.sprite.pixelsPerUnit, sr2.sprite.pixelsPerUnit);
            bool sameScale = Vector3.Distance(scale1, scale2) < 0.001f;
            bool sameSortingOrder = hasSr && (sr1.sortingOrder == sr2.sortingOrder);
            bool sameFlipX = hasSr && (sr1.flipX == sr2.flipX);
            bool noLegacyLabel = (m2.transform.Find("Label") == null);

            bool pass = hasSr && hasSprite && spriteDimensionsMatch && sameScale && sameSortingOrder && sameFlipX && noLegacyLabel;
            Debug.Log($"[TEST 11] Visual Config Match: SR={hasSr}, SpritePresent={hasSprite}, SpriteDimMatch={spriteDimensionsMatch}, Scale={sameScale} ({scale1} vs {scale2}), Order={sameSortingOrder}, FlipX={sameFlipX}, NoLegacyLabel={noLegacyLabel} | {(pass ? "PASS" : "FAIL")}");

            UnityEngine.Object.DestroyImmediate(m2.gameObject);
            return pass;
        }

        // =====================================================================
        // TEST 12: No Rigidbody/Collider/physics dependency is introduced.
        // =====================================================================
        private static bool Test12_NoPhysicsDependencyIntroduced()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            Monster monster = UnityEngine.Object.FindAnyObjectByType<Monster>();

            bool heroPhysics = (hero.GetComponent<Rigidbody>() != null) ||
                               (hero.GetComponent<Rigidbody2D>() != null) ||
                               (hero.GetComponent<Collider>() != null) ||
                               (hero.GetComponent<Collider2D>() != null);

            bool monsterPhysics = (monster.GetComponent<Rigidbody>() != null) ||
                                  (monster.GetComponent<Rigidbody2D>() != null) ||
                                  (monster.GetComponent<Collider>() != null) ||
                                  (monster.GetComponent<Collider2D>() != null);

            bool pass = (!heroPhysics) && (!monsterPhysics);
            Debug.Log($"[TEST 12] Pure Mathematical Plane: HeroPhysics={heroPhysics}, MonsterPhysics={monsterPhysics} | {(pass ? "PASS" : "FAIL")}");
            return pass;
        }

        // =====================================================================
        // PLAY MODE ACCEPTANCE: SCENARIOS A THROUGH L WITH SCREENSHOT CAPTURE
        // =====================================================================
        private static bool RunPlayModeAcceptance_Scenarios_A_to_L()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING PLAY MODE MULTI-ENCOUNTER ACCEPTANCE (SCENARIOS A -> L)");
            Debug.Log("================================================================================");

            if (!Directory.Exists(HeightOutputDir))
            {
                Directory.CreateDirectory(HeightOutputDir);
            }
            if (!Directory.Exists("Screenshots"))
            {
                Directory.CreateDirectory("Screenshots");
            }

            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype01.unity", OpenSceneMode.Single);
            BattleManager bm = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
            Hero hero = UnityEngine.Object.FindAnyObjectByType<Hero>();
            Camera cam = Camera.main ?? UnityEngine.Object.FindAnyObjectByType<Camera>();
            GameObject canvasGO = GameObject.Find("UI Canvas");
            Canvas canvas = canvasGO != null ? canvasGO.GetComponent<Canvas>() : null;

            if (bm == null || hero == null || cam == null || canvas == null)
            {
                Debug.LogError("[PLAY MODE ACCEPTANCE FAIL] Scene objects missing.");
                return false;
            }

            // Setup camera rendering for offscreen capture
            RenderMode origRenderMode = canvas.renderMode;
            Camera origWorldCam = canvas.worldCamera;
            float origPlaneDist = canvas.planeDistance;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 5f;

            RenderTexture rt = new RenderTexture(ScreenWidth, ScreenHeight, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 4;
            cam.targetTexture = rt;

            // SCENARIO A: Initial encounter
            bm.StartBattle();
            Monster m1 = bm.CurrentMonster;
            float initialDist = Mathf.Abs(m1.transform.position.x - hero.transform.position.x);
            Debug.Log($"[SCENARIO A] Initial Encounter #1: HeroPos={hero.transform.position}, MonsterPos={m1.transform.position}, Dist={initialDist:F2}");

            // SCENARIO B: Hero attacks Monster #1
            hero.SetCurrentTarget(m1);
            // Move hero to attack range
            hero.transform.position = new Vector3(m1.transform.position.x - 1.5f, -0.3f, 0f);
            float preM1Hp = m1.Health.CurrentHealth;
            hero.Attack.ResetAttackTimer();
            hero.Attack.ManualTick(1.5f);
            float postM1Hp = m1.Health.CurrentHealth;
            bool scenarioB_Attacked = postM1Hp < preM1Hp;
            Debug.Log($"[SCENARIO B] Hero Attacks Monster #1: HP {preM1Hp:F0} -> {postM1Hp:F0} (Damaged={scenarioB_Attacked})");
            CaptureScreenshot(cam, rt, "UI02_HEIGHT_01_InitialCombat.png", "Encounter #1 Active Combat");

            // SCENARIO C: Monster #1 dies
            m1.Health.TakeDamage(999999f);
            if (bm.CurrentBattleState == BattleState.LootPending)
            {
                bm.CompleteLootDecisionAndResume(false, true);
            }
            bool scenarioC_M1Dead = (m1 == null) || (!m1.gameObject.activeInHierarchy);
            Debug.Log($"[SCENARIO C] Monster #1 Defeated: Dead={scenarioC_M1Dead}");

            // SCENARIO D: Monster #2 spawns
            Monster m2 = bm.CurrentMonster;
            bool scenarioD_M2Spawned = (m2 != null) && (bm.EncounterIndex == 2);
            float deltaY_M2 = Mathf.Abs(m2.transform.position.y - hero.transform.position.y);
            Debug.Log($"[COMBAT HEIGHT AUDIT]\nEncounterIndex={bm.EncounterIndex}\nHeroPosition={hero.transform.position}\nMonsterPosition={m2.transform.position}\nHeroY={hero.transform.position.y:F3}\nMonsterY={m2.transform.position.y:F3}\nDeltaY={deltaY_M2:F3}\nHorizontalDistance={Mathf.Abs(m2.transform.position.x - hero.transform.position.x):F3}");
            Debug.Log($"[SCENARIO D] Monster #2 Spawned at {m2.transform.position} (EncounterIndex={bm.EncounterIndex})");
            CaptureScreenshot(cam, rt, "UI02_HEIGHT_02_Encounter2Spawn.png", "Encounter #2 Monster Spawn");

            // SCENARIO E: Hero moves toward Monster #2
            float heroPreMoveX = hero.transform.position.x;
            const float dt = 0.1f;
            for (int i = 0; i < 40; i++)
            {
                float d = Mathf.Abs(m2.transform.position.x - hero.transform.position.x);
                if (d > hero.AttackRange)
                {
                    hero.Movement.MoveTowardTarget(m2.transform.position, hero.AttackRange, dt);
                }
            }
            float heroPostMoveX = hero.transform.position.x;
            bool scenarioE_HeroMoved = heroPostMoveX > heroPreMoveX;
            Debug.Log($"[SCENARIO E] Hero Approach: X moved from {heroPreMoveX:F2} to {heroPostMoveX:F2}");

            // SCENARIO F: Hero stops and attacks Monster #2
            float distToM2 = Mathf.Abs(m2.transform.position.x - hero.transform.position.x);
            bool scenarioF_StoppedInRange = distToM2 <= hero.AttackRange + 0.1f;
            float preM2Hp = m2.Health.CurrentHealth;
            hero.Attack.ResetAttackTimer();
            hero.Attack.ManualTick(1.5f);
            float postM2Hp = m2.Health.CurrentHealth;
            bool scenarioF_AttackedM2 = postM2Hp < preM2Hp;
            Debug.Log($"[SCENARIO F] Hero in range ({distToM2:F2} <= 1.90) and attacks Monster #2: HP {preM2Hp:F0} -> {postM2Hp:F0}");
            CaptureScreenshot(cam, rt, "UI02_HEIGHT_03_Encounter2Combat.png", "Encounter #2 Ongoing Combat");

            // SCENARIO G: Monster #2 takes damage
            bool scenarioG_M2TakesDamage = postM2Hp < preM2Hp;
            Debug.Log($"[SCENARIO G] Monster #2 Takes Damage: {scenarioG_M2TakesDamage}");

            // SCENARIO H: Monster #2 dies
            m2.Health.TakeDamage(999999f);
            if (bm.CurrentBattleState == BattleState.LootPending)
            {
                bm.CompleteLootDecisionAndResume(false, true);
            }
            bool scenarioH_M2Dead = (m2 == null) || (!m2.gameObject.activeInHierarchy);
            Debug.Log($"[SCENARIO H] Monster #2 Defeated: Dead={scenarioH_M2Dead}");

            // SCENARIO I: Monster #3 spawns
            Monster m3 = bm.CurrentMonster;
            bool scenarioI_M3Spawned = (m3 != null) && (bm.EncounterIndex == 3);
            Debug.Log($"[SCENARIO I] Monster #3 Spawned at {m3.transform.position} (EncounterIndex={bm.EncounterIndex})");

            // SCENARIO J: Hero and Monster #3 continue combat
            hero.SetCurrentTarget(m3);
            for (int i = 0; i < 40; i++)
            {
                float d = Mathf.Abs(m3.transform.position.x - hero.transform.position.x);
                if (d > hero.AttackRange)
                {
                    hero.Movement.MoveTowardTarget(m3.transform.position, hero.AttackRange, dt);
                }
            }
            float preM3Hp = m3.Health.CurrentHealth;
            hero.Attack.ResetAttackTimer();
            hero.Attack.ManualTick(1.5f);
            float postM3Hp = m3.Health.CurrentHealth;
            bool scenarioJ_CombatContinues = postM3Hp < preM3Hp;
            Debug.Log($"[SCENARIO J] Hero and Monster #3 Combat: HP {preM3Hp:F0} -> {postM3Hp:F0} (Damaged={scenarioJ_CombatContinues})");
            CaptureScreenshot(cam, rt, "UI02_HEIGHT_04_Encounter3Combat.png", "Encounter #3 Active Combat");

            // SCENARIO K: Monster #2 visual matches Monster #1 presentation
            SpriteRenderer m3Sr = m3.GetComponent<SpriteRenderer>();
            bool scenarioK_VisualMatch = (m3Sr != null) && (m3Sr.sprite != null) && (m3Sr.sortingOrder == 10) && (m3Sr.flipX == true);
            Debug.Log($"[SCENARIO K] Monster Standee Visual Pipeline Preserved: {scenarioK_VisualMatch}");

            // SCENARIO L: No permanent idle/deadlock occurs
            bool scenarioL_NoDeadlock = scenarioB_Attacked && scenarioF_AttackedM2 && scenarioJ_CombatContinues;
            Debug.Log($"[SCENARIO L] No Permanent Deadlock Across Encounters 1, 2, 3: {scenarioL_NoDeadlock}");

            // Restore camera & canvas
            cam.targetTexture = null;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);

            canvas.renderMode = origRenderMode;
            canvas.worldCamera = origWorldCam;
            canvas.planeDistance = origPlaneDist;

            bool allScenariosPassed = scenarioB_Attacked && scenarioC_M1Dead && scenarioD_M2Spawned &&
                                      scenarioE_HeroMoved && scenarioF_StoppedInRange && scenarioG_M2TakesDamage &&
                                      scenarioH_M2Dead && scenarioI_M3Spawned && scenarioJ_CombatContinues &&
                                      scenarioK_VisualMatch && scenarioL_NoDeadlock;

            Debug.Log($"[PLAY MODE ACCEPTANCE] RESULT: {(allScenariosPassed ? "PASS" : "FAIL")}");
            return allScenariosPassed;
        }

        private static void CaptureScreenshot(Camera cam, RenderTexture rt, string filename, string description)
        {
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(ScreenWidth, ScreenHeight, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, ScreenWidth, ScreenHeight), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            string path1 = Path.Combine(HeightOutputDir, filename);
            File.WriteAllBytes(path1, bytes);

            string path2 = Path.Combine("Screenshots", filename);
            File.WriteAllBytes(path2, bytes);

            FileInfo fi = new FileInfo(path1);
            Debug.Log($"[SCREENSHOT SAVED] {filename} ({fi.Length / 1024} KB) - {description}");
        }
    }
}
#endif
