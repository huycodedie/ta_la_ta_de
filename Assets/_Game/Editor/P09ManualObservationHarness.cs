using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Entities.Components;
using WuxiaGame.Equipment;
using WuxiaGame.Inventory;
using WuxiaGame.Progression;
using WuxiaGame.Stats;

namespace WuxiaGame.Editor
{
    /// <summary>
    /// P09-A Manual Observation Controller (Static Management Core).
    /// Manages transient RAM scene fixture, arena visualization, camera, combatants (Hero, Target A, Target B),
    /// skill casting (Instant and Cast-Time projectiles), target switching, and real game pause/resume.
    /// Operates strictly with natural frames (Time.timeScale = 1).
    /// Enforces strict session authorization to prevent any side-effects in normal Editor gameplay.
    /// Complete autonomy isolation: Hero AI, basic attacks, auto-movement, and wave transitions are disabled.
    /// </summary>
    public static class P09ManualObservationController
    {
        // Root container for all fixture GameObjects
        private static GameObject _fixtureRoot;

        // References to runtime entities
        public static Camera CameraRef;
        public static Hero HeroRef;
        public static Monster TargetARef;
        public static Monster TargetBRef;
        public static Monster CurrentTargetRef;
        public static BattleManager BattleManagerRef;
        public static MindMethodManager MindMethodManagerRef;

        // Transient RAM skills and configs
        public static SkillDefinitionSO InstantSkill;
        public static SkillDefinitionSO CastSkill;
        private static DamageEffectDefinitionSO _effInstant;
        private static DamageEffectDefinitionSO _effCast;
        private static MindMethodDefinitionSO _tempDef;
        private static MindMethodDatabaseSO _tempDb;
        private static MonsterConfigSO _mCfgA;
        private static CombatConfigSO _cCfgA;
        private static MonsterConfigSO _mCfgB;
        private static CombatConfigSO _cCfgB;

        public static string LastActionLog = "Sẵn sàng (Chưa bấm)";
        public static string LastDamageLog = "Chưa có sát thương nào (Chờ đạn va chạm)";
        public static int ResetCount = 0;

        // Telemetry counters
        private static int _commandCounter = 0;
        public static int TotalCommandsIssued = 0;
        public static int TotalProjectileReleases = 0;
        public static int TotalDamageEvents = 0;
        public static int TotalTargetDeaths = 0;
        public static int TotalUnexpectedViolations = 0;
        private class ProjectileReferenceComparer : IEqualityComparer<ProjectileController>
        {
            public bool Equals(ProjectileController x, ProjectileController y) => object.ReferenceEquals(x, y);
            public int GetHashCode(ProjectileController obj) => obj != null ? System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj) : 0;
        }

        public static readonly HashSet<ProjectileController> ObservedProjectiles = new HashSet<ProjectileController>(new ProjectileReferenceComparer());

        public static bool IsFixtureReady =>
            _fixtureRoot != null &&
            HeroRef != null &&
            TargetARef != null &&
            TargetBRef != null &&
            CameraRef != null &&
            BattleManagerRef != null &&
            InstantSkill != null &&
            CastSkill != null;

        public static void ResetTelemetry()
        {
            _commandCounter = 0;
            TotalCommandsIssued = 0;
            TotalProjectileReleases = 0;
            TotalDamageEvents = 0;
            TotalTargetDeaths = 0;
            TotalUnexpectedViolations = 0;
            ObservedProjectiles.Clear();
        }

        private static void HandleEntityDamaged(Entity victim, DamageResult result)
        {
            if (victim == null) return;
            TotalDamageEvents++;
            float curHp = victim.Health != null ? victim.Health.CurrentHealth : 0f;
            float maxHp = victim.Health != null ? victim.Health.MaxHealth : 0f;
            string critStr = result.IsCrit ? " (BẠO KÍCH!)" : "";
            LastDamageLog = $"Gây {result.FinalDamage:F0}{critStr} sát thương vào {victim.EntityName} (HP còn: {curHp:F0}/{maxHp:F0})";
            Debug.Log($"[P09 MANUAL EVENT] Frame={Time.frameCount} Time={Time.time:F2}s DamageType={result.DamageType} Attacker={result.Attacker?.EntityName} Victim={victim.EntityName} FinalDamage={result.FinalDamage:F1}{critStr}");

            if (result.DamageType == DamageType.BasicAttack)
            {
                TotalUnexpectedViolations++;
                Debug.LogError($"[P09 AUTONOMY VIOLATION] Basic attack damage detected! Attacker={result.Attacker?.EntityName} Victim={victim.EntityName}");
            }
        }

        private static void HandleEntityDied(Entity victim)
        {
            if (victim == null) return;
            TotalTargetDeaths++;
            Debug.Log($"[P09 MANUAL TARGET DEATH] Frame={Time.frameCount} Time={Time.time:F2}s Victim={victim.EntityName}");
        }

        private static void HandleEntitySpawned(Entity entity)
        {
            if (entity == null) return;
            Debug.Log($"[P09 MANUAL ENTITY SPAWNED] Frame={Time.frameCount} Time={Time.time:F2}s Entity={entity.EntityName} ({entity.name})");
            if (_fixtureRoot != null && !entity.transform.IsChildOf(_fixtureRoot.transform))
            {
                TotalUnexpectedViolations++;
                Debug.LogError($"[P09 AUTONOMY VIOLATION] Unexpected entity spawned outside fixture root! Entity={entity.EntityName}");
            }
        }

        /// <summary>
        /// Strictly disables all autonomous decision and attack components for Hero and Monsters.
        /// Guaranteed to maintain isolation after BattleManager.StartBattle, ResumeCombat, or Reset.
        /// </summary>
        public static void EnforceAutonomousDisabling()
        {
            if (BattleManagerRef != null)
            {
                BattleManagerRef.SetAutoBattle(false);
                if (BattleManagerRef.enabled) BattleManagerRef.enabled = false;
            }

            if (HeroRef != null)
            {
                if (HeroRef.SkillDecisionController != null && HeroRef.SkillDecisionController.enabled)
                {
                    HeroRef.SkillDecisionController.enabled = false;
                }
                if (HeroRef.Attack != null)
                {
                    HeroRef.Attack.SetAttackEnabled(false);
                    if (HeroRef.Attack.enabled) HeroRef.Attack.enabled = false;
                }
                if (HeroRef.Movement != null)
                {
                    HeroRef.Movement.SetMovementEnabled(false);
                    if (HeroRef.Movement.enabled) HeroRef.Movement.enabled = false;
                }
            }

            if (TargetARef != null)
            {
                if (TargetARef.Attack != null)
                {
                    TargetARef.Attack.SetAttackEnabled(false);
                    if (TargetARef.Attack.enabled) TargetARef.Attack.enabled = false;
                }
                if (TargetARef.Movement != null)
                {
                    TargetARef.Movement.SetMovementEnabled(false);
                    if (TargetARef.Movement.enabled) TargetARef.Movement.enabled = false;
                }
                TargetARef.HasAwardedExp = true;
            }

            if (TargetBRef != null)
            {
                if (TargetBRef.Attack != null)
                {
                    TargetBRef.Attack.SetAttackEnabled(false);
                    if (TargetBRef.Attack.enabled) TargetBRef.Attack.enabled = false;
                }
                if (TargetBRef.Movement != null)
                {
                    TargetBRef.Movement.SetMovementEnabled(false);
                    if (TargetBRef.Movement.enabled) TargetBRef.Movement.enabled = false;
                }
                TargetBRef.HasAwardedExp = true;
            }
        }

        public static void TeardownFixture()
        {
            if (_fixtureRoot == null && !P09ManualSessionBootstrap.IsSessionAuthorized())
            {
                return;
            }

            Debug.Log("[P09 MANUAL] Tearing down RAM Observation Fixture...");

            // Unsubscribe from EventBus
            EventBus.OnEntityDamaged -= HandleEntityDamaged;
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntitySpawned -= HandleEntitySpawned;

            // 1. Clear any active projectiles
            ProjectileController.ClearAllProjectiles();

            // 2. Clear EventBus listeners
            EventBus.ClearAllListeners();

            // 3. Reset runtime singletons
            BattleManager.ResetInstance();
            MindMethodManager.ResetInstance();
            EquipmentManager.ResetInstance();
            Inventory.Inventory.ResetInstance();
            ResourceManager.ResetInstance();
            ProgressionManager.ResetInstance();

            // 4. Destroy transient ScriptableObjects
            if (InstantSkill != null) UnityEngine.Object.DestroyImmediate(InstantSkill);
            if (CastSkill != null) UnityEngine.Object.DestroyImmediate(CastSkill);
            if (_effInstant != null) UnityEngine.Object.DestroyImmediate(_effInstant);
            if (_effCast != null) UnityEngine.Object.DestroyImmediate(_effCast);
            if (_tempDef != null) UnityEngine.Object.DestroyImmediate(_tempDef);
            if (_tempDb != null) UnityEngine.Object.DestroyImmediate(_tempDb);
            if (_mCfgA != null) UnityEngine.Object.DestroyImmediate(_mCfgA);
            if (_cCfgA != null) UnityEngine.Object.DestroyImmediate(_cCfgA);
            if (_mCfgB != null) UnityEngine.Object.DestroyImmediate(_mCfgB);
            if (_cCfgB != null) UnityEngine.Object.DestroyImmediate(_cCfgB);

            InstantSkill = null;
            CastSkill = null;
            _effInstant = null;
            _effCast = null;
            _tempDef = null;
            _tempDb = null;
            _mCfgA = null;
            _cCfgA = null;
            _mCfgB = null;
            _cCfgB = null;

            // 5. Destroy fixture root and all its children synchronously
            if (_fixtureRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(_fixtureRoot);
                _fixtureRoot = null;
            }

            CameraRef = null;
            HeroRef = null;
            TargetARef = null;
            TargetBRef = null;
            CurrentTargetRef = null;
            BattleManagerRef = null;
            MindMethodManagerRef = null;
            ObservedProjectiles.Clear();
        }

        public static void SetupOrResetFixture()
        {
            if (!P09ManualSessionBootstrap.IsSessionAuthorized())
            {
                Debug.LogWarning("[P09 MANUAL SECURITY] Refused SetupOrResetFixture: Session is not authorized by Save Guard launcher.");
                return;
            }

            // Always teardown any previous fixture completely before recreating
            TeardownFixture();

            ResetCount++;
            ResetTelemetry();

            Debug.Log($"[P09 MANUAL] Initializing Fixture (Cycle #{ResetCount}) under isolated root in Play Mode...");

            // 1. Create dedicated root for all fixture objects
            _fixtureRoot = new GameObject("[P09_Manual_Observation_Fixture_Root]");

            // 2. Setup Services under fixture root
            var servicesGO = new GameObject("Fixture_Services");
            servicesGO.transform.SetParent(_fixtureRoot.transform, false);
            servicesGO.AddComponent<EquipmentManager>();
            servicesGO.AddComponent<Inventory.Inventory>();
            servicesGO.AddComponent<ResourceManager>();
            var progMgr = servicesGO.AddComponent<ProgressionManager>();
            progMgr.enabled = false; // Disable ProgressionManager component to prevent EXP award / Level up during observation

            // 3. Setup Camera & Visual Environment
            var camGO = new GameObject("Fixture_Camera");
            camGO.transform.SetParent(_fixtureRoot.transform, false);
            camGO.tag = "MainCamera";
            CameraRef = camGO.AddComponent<Camera>();
            CameraRef.orthographic = true;
            CameraRef.orthographicSize = 4.5f;
            CameraRef.nearClipPlane = 0.3f;
            CameraRef.farClipPlane = 100f;
            CameraRef.transform.position = new Vector3(0f, 0f, -10f);
            CameraRef.transform.rotation = Quaternion.identity;
            CameraRef.clearFlags = CameraClearFlags.SolidColor;
            CameraRef.backgroundColor = new Color(0.12f, 0.15f, 0.20f, 1f);
            CameraRef.enabled = true;

            // Light
            var lightGO = new GameObject("Fixture_Light");
            lightGO.transform.SetParent(_fixtureRoot.transform, false);
            var l = lightGO.AddComponent<Light>();
            l.type = LightType.Directional;
            l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            l.intensity = 1.2f;
            l.color = Color.white;

            // Ground marker bar
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Fixture_Ground_Marker";
            ground.transform.SetParent(_fixtureRoot.transform, false);
            ground.transform.position = new Vector3(0f, -2.5f, 0f);
            ground.transform.localScale = new Vector3(16f, 0.3f, 1f);
            var gr = ground.GetComponent<Renderer>();
            if (gr != null) gr.material.color = new Color(0.25f, 0.30f, 0.35f);
            var gc = ground.GetComponent<Collider>();
            if (gc != null) UnityEngine.Object.DestroyImmediate(gc);

            // 4. Setup BattleManager
            var bmGO = new GameObject("Fixture_BattleManager");
            bmGO.transform.SetParent(_fixtureRoot.transform, false);
            BattleManagerRef = bmGO.AddComponent<BattleManager>();

            // 5. Setup Hero (Cyan Capsule, X = -4.5, Y = 0)
            var heroGO = new GameObject("Hero_Observation");
            heroGO.transform.SetParent(_fixtureRoot.transform, false);
            heroGO.transform.position = new Vector3(-4.5f, 0f, 0f);
            HeroRef = heroGO.AddComponent<Hero>();
            HeroRef.InitializeHero();
            HeroRef.Health.InitializeHealth(1000f, HeroRef);
            HeroRef.Rage.InitializeRage(100f, 100f, HeroRef);
            // Deterministic stats: Attack = 100, CritRate = 0, CritDamage = 0
            HeroRef.Stats.SetBaseValue(StatType.Attack, 100f);
            HeroRef.Stats.SetBaseValue(StatType.CritRate, 0f);
            HeroRef.Stats.SetBaseValue(StatType.CritDamage, 0f);
            AttachVisualPrimitive(heroGO, PrimitiveType.Capsule, Color.cyan, new Vector3(1.0f, 1.2f, 1.0f), "HERO (Cyan)");

            // 6. Setup Target A (Red Sphere, X = 4.5, Y = 1.0)
            var targetAGO = new GameObject("Monster_Target_A");
            targetAGO.transform.SetParent(_fixtureRoot.transform, false);
            targetAGO.transform.position = new Vector3(4.5f, 1.0f, 0f);
            TargetARef = targetAGO.AddComponent<Monster>();
            _mCfgA = ScriptableObject.CreateInstance<MonsterConfigSO>();
            _mCfgA.InitializeMonsterConfig("Target A (Top)", 500f, 10f, 0f, 2f, 2f, 2f, 0);
            _cCfgA = ScriptableObject.CreateInstance<CombatConfigSO>();
            TargetARef.InitializeMonster(_mCfgA, _cCfgA);
            // Deterministic stats: Defense = 0, Dodge = 0, HasAwardedExp = true
            TargetARef.Stats.SetBaseValue(StatType.Defense, 0f);
            TargetARef.Stats.SetBaseValue(StatType.Dodge, 0f);
            TargetARef.HasAwardedExp = true;
            AttachVisualPrimitive(targetAGO, PrimitiveType.Sphere, Color.red, new Vector3(1.2f, 1.2f, 1.2f), "TARGET A (Red)");

            // 7. Setup Target B (Orange Sphere, X = 4.5, Y = -1.0)
            var targetBGO = new GameObject("Monster_Target_B");
            targetBGO.transform.SetParent(_fixtureRoot.transform, false);
            targetBGO.transform.position = new Vector3(4.5f, -1.0f, 0f);
            TargetBRef = targetBGO.AddComponent<Monster>();
            _mCfgB = ScriptableObject.CreateInstance<MonsterConfigSO>();
            _mCfgB.InitializeMonsterConfig("Target B (Bottom)", 500f, 10f, 0f, 2f, 2f, 2f, 0);
            _cCfgB = ScriptableObject.CreateInstance<CombatConfigSO>();
            TargetBRef.InitializeMonster(_mCfgB, _cCfgB);
            // Deterministic stats: Defense = 0, Dodge = 0, HasAwardedExp = true
            TargetBRef.Stats.SetBaseValue(StatType.Defense, 0f);
            TargetBRef.Stats.SetBaseValue(StatType.Dodge, 0f);
            TargetBRef.HasAwardedExp = true;
            AttachVisualPrimitive(targetBGO, PrimitiveType.Sphere, new Color(1f, 0.5f, 0f), new Vector3(1.2f, 1.2f, 1.2f), "TARGET B (Orange)");

            // 8. Register combatants into BattleManager & start battle
            BattleManagerRef.RegisterHero(HeroRef);
            BattleManagerRef.RegisterMonster(TargetARef);
            BattleManagerRef.RegisterMonster(TargetBRef);

            // Explicitly set auto-battle to false before starting battle
            BattleManagerRef.SetAutoBattle(false);
            BattleManagerRef.StartBattle();
            // Explicitly set auto-battle to false again after starting battle
            BattleManagerRef.SetAutoBattle(false);

            // Disable BattleManager component: invokes OnDisable(), which unregisters EventBus.OnEntityDied -= HandleEntityDied
            // and cancels loot lifecycles, completely isolating wave progression (no DeferEncounterAdvanceWithoutLoot, no Wild Monster)
            // while preserving BattleManager.Instance, EncounterIndex, ActiveMonsters, and Pause/Resume APIs
            BattleManagerRef.enabled = false;

            // Enforce disabling of auto-decision, basic attack, and movement
            EnforceAutonomousDisabling();

            // Attach guardian component to maintain suppression and track projectile releases
            if (_fixtureRoot.GetComponent<P09ManualFixtureGuardian>() == null)
            {
                _fixtureRoot.AddComponent<P09ManualFixtureGuardian>();
            }

            // Subscribe to EventBus telemetry
            EventBus.OnEntityDamaged -= HandleEntityDamaged;
            EventBus.OnEntityDamaged += HandleEntityDamaged;

            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntityDied += HandleEntityDied;

            EventBus.OnEntitySpawned -= HandleEntitySpawned;
            EventBus.OnEntitySpawned += HandleEntitySpawned;

            // Initial target is Target A
            CurrentTargetRef = TargetARef;
            HeroRef.SetCurrentTarget(TargetARef);

            // 9. Setup MindMethodManager & Transient Skills
            var mmGO = new GameObject("Fixture_MindMethodManager");
            mmGO.transform.SetParent(_fixtureRoot.transform, false);
            MindMethodManagerRef = mmGO.AddComponent<MindMethodManager>();
            typeof(MindMethodManager).GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.SetValue(null, MindMethodManagerRef);

            _tempDb = ScriptableObject.CreateInstance<MindMethodDatabaseSO>();
            _tempDef = ScriptableObject.CreateInstance<MindMethodDefinitionSO>();
            _tempDef.InitializeMindMethod("mm_p09_manual", "P09 Quan Sát", "Tâm Pháp Quan Sát Đạn Bay", 10, true, new MindMethodPassiveData());
            _tempDb.SetMindMethods(new List<MindMethodDefinitionSO> { _tempDef });
            MindMethodManagerRef.SetDatabase(_tempDb);
            MindMethodManagerRef.SetActiveMindMethod("mm_p09_manual");

            // Skill 1: Instant Projectile (Speed = 6, Lifetime = 8s, CD = 4s, Rage = 15, Multiplier = 2.0x)
            InstantSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            _effInstant = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
            _effInstant.Initialize(2.0f, SkillTargetPolicy.SingleTarget, DamageType.Skill);
            InstantSkill.InitializeSkill(
                id: "p09_manual_instant",
                mmId: "mm_p09_manual",
                slot: SkillSlotType.Skill,
                name: "Thái Cực Kiếm Khí (Instant Đạn Bay)",
                desc: "Đạn bay tức thời: Speed 6.0 u/s, Lifetime 8.0s, CD 4.0s, Nộ 15, Dmg Mult 2.0x",
                conditions: null,
                dmgMultiplier: 2.0f,
                costRage: 15f,
                cd: 4.0f,
                passive: false,
                skillEffects: new List<SkillEffectDefinitionSO> { _effInstant },
                shatterFreeze: false,
                skillCastTime: 0f,
                channel: false,
                chDuration: 0f,
                chTickInterval: 0f,
                skillPriority: 50,
                projectile: true,
                projSpeed: 6f,
                projLifetime: 8f
            );

            // Skill 2: Cast-Time Projectile (1.5s cast, Speed = 6, Lifetime = 8s, CD = 6s, Rage = 25, Multiplier = 3.5x)
            CastSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            _effCast = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
            _effCast.Initialize(3.5f, SkillTargetPolicy.SingleTarget, DamageType.Skill);
            CastSkill.InitializeSkill(
                id: "p09_manual_cast",
                mmId: "mm_p09_manual",
                slot: SkillSlotType.Skill,
                name: "Huyền Vũ Thần Tiễn (1.5s Vận Khí + Đạn Bay)",
                desc: "Vận khí 1.5s rồi phóng đạn: Speed 6.0 u/s, Lifetime 8.0s, CD 6.0s, Nộ 25, Dmg Mult 3.5x",
                conditions: null,
                dmgMultiplier: 3.5f,
                costRage: 25f,
                cd: 6.0f,
                passive: false,
                skillEffects: new List<SkillEffectDefinitionSO> { _effCast },
                shatterFreeze: false,
                skillCastTime: 1.5f,
                channel: false,
                chDuration: 0f,
                chTickInterval: 0f,
                skillPriority: 50,
                projectile: true,
                projSpeed: 6f,
                projLifetime: 8f
            );

            var skillsField = typeof(MindMethodDefinitionSO).GetField("skills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = skillsField != null ? skillsField.GetValue(_tempDef) as List<SkillDefinitionSO> : null;
            if (list != null)
            {
                list.Add(InstantSkill);
                list.Add(CastSkill);
            }

            var activeState = MindMethodManagerRef.ActiveMindMethodState;
            if (activeState != null)
            {
                activeState.SkillStates[InstantSkill.SkillId] = new SkillRuntimeState(InstantSkill.SkillId, true, 1);
                activeState.SkillStates[CastSkill.SkillId] = new SkillRuntimeState(CastSkill.SkillId, true, 1);
                activeState.SelectedSkillPerSlot[SkillSlotType.Skill] = InstantSkill.SkillId;
            }

            Time.timeScale = 1f;
            LastActionLog = $"Fixture đã khởi tạo thành công (Lượt #{ResetCount}). Sẵn sàng thi triển.";

            // FIXED CHECKPOINT: Log FIXTURE_READY with exhaustive status dump
            Debug.Log($"[FIXTURE_READY] Cycle=#{ResetCount} Frame={Time.frameCount} Time={Time.time:F2}s " +
                $"AutoBattle={BattleManagerRef.IsAutoBattle} " +
                $"BattleManagerEnabled={BattleManagerRef.enabled} " +
                $"HeroSkillAIEnabled={(HeroRef.SkillDecisionController != null && HeroRef.SkillDecisionController.enabled)} " +
                $"HeroAttackEnabled={(HeroRef.Attack != null && HeroRef.Attack.IsAttackEnabled)} " +
                $"HeroMovementEnabled={(HeroRef.Movement != null && HeroRef.Movement.IsMovementEnabled)} " +
                $"TargetAAttackEnabled={(TargetARef.Attack != null && TargetARef.Attack.IsAttackEnabled)} " +
                $"TargetBAttackEnabled={(TargetBRef.Attack != null && TargetBRef.Attack.IsAttackEnabled)} " +
                $"RosterCount={(BattleManagerRef.ActiveMonsters != null ? BattleManagerRef.ActiveMonsters.Count : 0)} " +
                $"HeroHP={HeroRef.Health.CurrentHealth}/{HeroRef.Health.MaxHealth} " +
                $"HeroRage={HeroRef.Rage.CurrentRage}/{HeroRef.Rage.MaxRage} " +
                $"TargetAHP={TargetARef.Health.CurrentHealth}/{TargetARef.Health.MaxHealth} " +
                $"TargetBHP={TargetBRef.Health.CurrentHealth}/{TargetBRef.Health.MaxHealth}");
        }

        private static void AttachVisualPrimitive(GameObject parent, PrimitiveType type, Color color, Vector3 scale, string label)
        {
            var prim = GameObject.CreatePrimitive(type);
            prim.name = "Visual_" + label;
            prim.transform.SetParent(parent.transform, false);
            prim.transform.localPosition = Vector3.zero;
            prim.transform.localScale = scale;
            var r = prim.GetComponent<Renderer>();
            if (r != null) r.material.color = color;
            var c = prim.GetComponent<Collider>();
            if (c != null) UnityEngine.Object.DestroyImmediate(c);

            // Attach 3D Text Label above entity
            var textObj = new GameObject("Label_" + label);
            textObj.transform.SetParent(parent.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var tm = textObj.AddComponent<TextMesh>();
            tm.text = label;
            tm.characterSize = 0.15f;
            tm.fontSize = 24;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = Color.white;
        }

        /// <summary>
        /// Cast Instant Projectile via production Hero.ExecuteSelectedSkill(SkillSlotType.Skill, target).
        /// </summary>
        public static SkillExecutionResult CastInstant()
        {
            if (!P09ManualSessionBootstrap.IsSessionAuthorized() || !IsFixtureReady)
            {
                Debug.LogWarning("[P09 MANUAL] Cast blocked: Session not authorized or fixture not ready.");
                return null;
            }

            int cmdId = ++_commandCounter;
            TotalCommandsIssued++;
            int frame = Time.frameCount;
            float time = Time.time;

            if (CurrentTargetRef == null || !CurrentTargetRef.IsAlive)
            {
                CurrentTargetRef = TargetARef != null && TargetARef.IsAlive ? TargetARef : TargetBRef;
            }
            HeroRef.SetCurrentTarget(CurrentTargetRef);

            // Select InstantSkill in SkillSlotType.Skill
            if (MindMethodManagerRef != null && MindMethodManagerRef.ActiveMindMethodState != null)
            {
                MindMethodManagerRef.ActiveMindMethodState.SelectedSkillPerSlot[SkillSlotType.Skill] = InstantSkill.SkillId;
            }

            // Call production API: Hero.ExecuteSelectedSkill
            var res = HeroRef.ExecuteSelectedSkill(SkillSlotType.Skill, CurrentTargetRef);
            string resStr = res != null && res.Success ? "SUCCESS" : $"FAIL ({res?.ReasonDescription})";
            LastActionLog = $"[Instant Cast] CMD #{cmdId} | Kết quả: {resStr} | Target: {CurrentTargetRef?.EntityName} | Đạn đang bay: {ProjectileController.ActiveProjectiles.Count}";

            if (res != null && res.Success)
            {
                Debug.Log($"[P09 MANUAL CMD #{cmdId}] Frame={frame} Time={time:F2}s Skill=p09_manual_instant Target={CurrentTargetRef?.EntityName} Result=SUCCESS ActiveProjectiles={ProjectileController.ActiveProjectiles.Count}");
            }
            else
            {
                Debug.LogWarning($"[P09 MANUAL CMD #{cmdId} REJECTED] Frame={frame} Time={time:F2}s Skill=p09_manual_instant Target={CurrentTargetRef?.EntityName} Reason={res?.ReasonDescription}");
            }

            return res;
        }

        /// <summary>
        /// Cast Cast-Time Projectile via production Hero.ExecuteSelectedSkill(SkillSlotType.Skill, target).
        /// </summary>
        public static SkillExecutionResult CastCastTime()
        {
            if (!P09ManualSessionBootstrap.IsSessionAuthorized() || !IsFixtureReady)
            {
                Debug.LogWarning("[P09 MANUAL] Cast blocked: Session not authorized or fixture not ready.");
                return null;
            }

            int cmdId = ++_commandCounter;
            TotalCommandsIssued++;
            int frame = Time.frameCount;
            float time = Time.time;

            if (CurrentTargetRef == null || !CurrentTargetRef.IsAlive)
            {
                CurrentTargetRef = TargetARef != null && TargetARef.IsAlive ? TargetARef : TargetBRef;
            }
            HeroRef.SetCurrentTarget(CurrentTargetRef);

            // Select CastSkill in SkillSlotType.Skill
            if (MindMethodManagerRef != null && MindMethodManagerRef.ActiveMindMethodState != null)
            {
                MindMethodManagerRef.ActiveMindMethodState.SelectedSkillPerSlot[SkillSlotType.Skill] = CastSkill.SkillId;
            }

            // Call production API: Hero.ExecuteSelectedSkill
            var res = HeroRef.ExecuteSelectedSkill(SkillSlotType.Skill, CurrentTargetRef);
            string resStr = res != null && res.Success ? "BẮT ĐẦU VẬN KHÍ 1.5s" : $"FAIL ({res?.ReasonDescription})";
            LastActionLog = $"[Cast-Time Cast] CMD #{cmdId} | Kết quả: {resStr} | Target: {CurrentTargetRef?.EntityName} | IsCasting={HeroRef.IsCasting}";

            if (res != null && res.Success)
            {
                Debug.Log($"[P09 MANUAL CMD #{cmdId}] Frame={frame} Time={time:F2}s Skill=p09_manual_cast Target={CurrentTargetRef?.EntityName} Result=SUCCESS IsCasting={HeroRef.IsCasting}");
            }
            else
            {
                Debug.LogWarning($"[P09 MANUAL CMD #{cmdId} REJECTED] Frame={frame} Time={time:F2}s Skill=p09_manual_cast Target={CurrentTargetRef?.EntityName} Reason={res?.ReasonDescription}");
            }

            return res;
        }

        public static void SelectTargetA()
        {
            if (!P09ManualSessionBootstrap.IsSessionAuthorized() || !IsFixtureReady) return;
            CurrentTargetRef = TargetARef;
            if (HeroRef != null) HeroRef.SetCurrentTarget(TargetARef);
            LastActionLog = "Đã chuyển mục tiêu sang Target A (Đỏ, Phía trên).";
            Debug.Log("[P09 MANUAL] Selected Target A (Red, Top).");
        }

        public static void SelectTargetB()
        {
            if (!P09ManualSessionBootstrap.IsSessionAuthorized() || !IsFixtureReady) return;
            CurrentTargetRef = TargetBRef;
            if (HeroRef != null) HeroRef.SetCurrentTarget(TargetBRef);
            LastActionLog = "Đã chuyển mục tiêu sang Target B (Cam, Phía dưới).";
            Debug.Log("[P09 MANUAL] Selected Target B (Orange, Bottom).");
        }

        public static void TogglePause()
        {
            if (!P09ManualSessionBootstrap.IsSessionAuthorized() || BattleManagerRef == null) return;
            if (BattleManagerRef.IsCombatPausedByUI)
            {
                BattleManagerRef.ResumeCombat();
                // Crucial: ResumeCombat re-enables entity actions; immediately re-enforce autonomous suppression!
                EnforceAutonomousDisabling();
                LastActionLog = "Đã tiếp tục combat (BattleManager.ResumeCombat).";
                Debug.Log("[P09 MANUAL] Resumed Combat (BattleManager.ResumeCombat).");
            }
            else
            {
                BattleManagerRef.PauseCombat();
                LastActionLog = "Đã tạm dừng combat (BattleManager.PauseCombat).";
                Debug.Log("[P09 MANUAL] Paused Combat (BattleManager.PauseCombat).");
            }
        }
    }

    /// <summary>
    /// Guardian attached to _fixtureRoot to continuously enforce autonomous suppression every frame,
    /// track projectile releases by stable instance identity, and detect unauthorized spawns.
    /// Note on frame-sampling observer: Projectiles in this fixture travel over natural frames (~1.5s flight,
    /// 90-450 frames), allowing reliable tracking of each new instance via reference identity even when an old
    /// projectile impacts and a new projectile releases in the same observation interval.
    /// </summary>
    public class P09ManualFixtureGuardian : MonoBehaviour
    {
        private void Update()
        {
            // Maintain autonomous suppression every frame
            P09ManualObservationController.EnforceAutonomousDisabling();

            // Track projectile releases by stable instance identity
            var activeProjectiles = ProjectileController.ActiveProjectiles;
            for (int i = 0; i < activeProjectiles.Count; i++)
            {
                var proj = activeProjectiles[i];
                if (proj != null && P09ManualObservationController.ObservedProjectiles.Add(proj))
                {
                    P09ManualObservationController.TotalProjectileReleases++;
                    Debug.Log($"[P09 MANUAL RELEASE] Frame={Time.frameCount} Time={Time.time:F2}s Target={proj.BoundTarget?.EntityName} ActiveCount={activeProjectiles.Count} TotalReleases={P09ManualObservationController.TotalProjectileReleases}");
                }
            }
        }
    }

    /// <summary>
    /// Interactive EditorWindow control panel for manual observation of P09-A projectiles.
    /// Provides clickable buttons for instant cast, cast-time cast, retargeting A/B, pause/resume, and fixture reset.
    /// Displays live runtime stats: HP, Rage, Cooldowns, Casting progress, Bound Target, and Active Projectiles.
    /// Strictly blocked from any execution when launched outside Save Guard.
    /// </summary>
    public class P09ManualTestWindow : EditorWindow
    {
        private Vector2 _scrollPos;

        [MenuItem("Tools/Wuxia RPG/P09 Projectile Manual Observation Window")]
        public static void ShowWindow()
        {
            var win = GetWindow<P09ManualTestWindow>("P09 Manual Observation");
            win.minSize = new Vector2(460, 720);
            win.Show();
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            try
            {
                bool isAuthorized = P09ManualSessionBootstrap.IsSessionAuthorized();

                // Title Banner
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("P09-A PROJECTILE OBSERVATION CONTROL", EditorStyles.boldLabel);

                if (!isAuthorized)
                {
                    EditorGUILayout.HelpBox(
                        "PHIÊN CHƯA ĐƯỢC CẤP PHÉP (UNAUTHORIZED SESSION)\n\n" +
                        "Cửa sổ này bị KHÓA hoàn toàn để bảo vệ workspace và tránh làm bẩn PlayerPrefs khi chạy Editor bình thường.\n\n" +
                        "Để mở phiên quan sát an toàn với đầy đủ RAM fixture và Save Guard bảo vệ, vui lòng thực thi lệnh duy nhất trên PowerShell:\n\n" +
                        "  powershell -ExecutionPolicy Bypass -File Tools\\Verification\\P09\\launch_manual_p09a_session.ps1\n\n" +
                        "Mọi thao tác can thiệp trực tiếp từ Editor thường đều bị từ chối.",
                        MessageType.Error);
                    return;
                }

                EditorGUILayout.HelpBox(
                    "SAVE GUARD PROTECTED SESSION (ĐÃ CẤP PHÉP)\n" +
                    "Phiên chạy được quản lý bởi launch_manual_p09a_session.ps1. Toàn bộ PlayerPrefs đã được backup bền vững. Khi đóng Editor, hệ thống sẽ tự động đối chiếu và phục hồi nguyên vẹn (Diff = 0).",
                    MessageType.Info);

                EditorGUILayout.Space(8);

                // Play Mode Check
                if (!EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Phiên cần ở chế độ PLAY MODE để quan sát khung hình tự nhiên của đạn bay.", MessageType.Warning);
                    return;
                }

                if (!P09ManualObservationController.IsFixtureReady)
                {
                    EditorGUILayout.HelpBox("Fixture RAM chưa được khởi tạo trong Play Mode.", MessageType.Warning);
                    if (GUILayout.Button("Khởi tạo Fixture Ngay", GUILayout.Height(36)))
                    {
                        P09ManualObservationController.SetupOrResetFixture();
                    }
                    return;
                }

                // 1. Fixture & Combat Lifecycle Controls
                EditorGUILayout.LabelField("1. ĐIỀU KHIỂN PHIÊN & TRẠNG THÁI COMBAT", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Tạo / Reset Fixture (Khôi phục ban đầu)", GUILayout.Height(32)))
                {
                    P09ManualObservationController.SetupOrResetFixture();
                }

                bool isPaused = P09ManualObservationController.BattleManagerRef != null &&
                                P09ManualObservationController.BattleManagerRef.IsCombatPausedByUI;
                GUI.backgroundColor = isPaused ? Color.yellow : Color.cyan;
                if (GUILayout.Button(isPaused ? "Tiếp tục Combat (Resume)" : "Tạm dừng Combat (Pause)", GUILayout.Height(32)))
                {
                    P09ManualObservationController.TogglePause();
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(8);

                // 2. Target Selection Controls
                EditorGUILayout.LabelField("2. CHỌN MỤC TIÊU (RETARGETING)", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                bool isTargetA = P09ManualObservationController.CurrentTargetRef == P09ManualObservationController.TargetARef;
                bool isTargetB = P09ManualObservationController.CurrentTargetRef == P09ManualObservationController.TargetBRef;

                GUI.backgroundColor = isTargetA ? Color.green : Color.white;
                if (GUILayout.Button("Chọn Target A (Đỏ, Y = +1.0)", GUILayout.Height(28)))
                {
                    P09ManualObservationController.SelectTargetA();
                }

                GUI.backgroundColor = isTargetB ? Color.green : Color.white;
                if (GUILayout.Button("Chọn Target B (Cam, Y = -1.0)", GUILayout.Height(28)))
                {
                    P09ManualObservationController.SelectTargetB();
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                var curTarget = P09ManualObservationController.CurrentTargetRef;
                string targetName = curTarget != null ? curTarget.EntityName : "Chưa chọn";
                string targetHpStr = (curTarget != null && curTarget.Health != null)
                    ? $"{curTarget.Health.CurrentHealth:F0} / {curTarget.Health.MaxHealth:F0}"
                    : "N/A";
                EditorGUILayout.LabelField($"   Mục tiêu đang chọn: {targetName} | HP: {targetHpStr}");

                EditorGUILayout.Space(8);

                // 3. Skill Casting Controls (Invokes Hero.ExecuteSelectedSkill)
                EditorGUILayout.LabelField("3. THI TRIỂN KỸ NĂNG ĐẠN BAY (qua Hero.ExecuteSelectedSkill)", EditorStyles.boldLabel);
                if (GUILayout.Button("Thi triển Instant Đạn Bay (Speed 6, CD 4.0s, Nộ 15, Mult 2.0x)", GUILayout.Height(34)))
                {
                    P09ManualObservationController.CastInstant();
                }
                if (GUILayout.Button("Thi triển Cast-Time Đạn Bay (Vận khí 1.5s, Speed 6, CD 6.0s, Nộ 25, Mult 3.5x)", GUILayout.Height(34)))
                {
                    P09ManualObservationController.CastCastTime();
                }

                EditorGUILayout.HelpBox($"Nhật ký thao tác gần nhất: {P09ManualObservationController.LastActionLog}", MessageType.None);

                EditorGUILayout.Space(8);

                // 4. Real-time Runtime Statistics
                EditorGUILayout.LabelField("4. THÔNG SỐ RUNTIME THẬT (READ-ONLY)", EditorStyles.boldLabel);
                var hero = P09ManualObservationController.HeroRef;
                if (hero != null)
                {
                    float heroHp = hero.Health != null ? hero.Health.CurrentHealth : 0f;
                    float heroMaxHp = hero.Health != null ? hero.Health.MaxHealth : 1f;
                    float heroRage = hero.Rage != null ? hero.Rage.CurrentRage : 0f;
                    float heroMaxRage = hero.Rage != null ? hero.Rage.MaxRage : 100f;

                    EditorGUILayout.LabelField($"Hero HP: {heroHp:F0} / {heroMaxHp:F0}  |  Nộ: {heroRage:F0} / {heroMaxRage:F0}");

                    bool isCasting = hero.IsCasting;
                    float castProgress = hero.CastProgress;
                    EditorGUILayout.LabelField($"Trạng thái Casting: {(isCasting ? $"ĐANG VẬN KHÍ ({castProgress * 100:F0}%)" : "Nhàn rỗi (Ready)")}");

                    float cdInstant = CooldownManager.GetRemainingCooldown("p09_manual_instant");
                    float cdCast = CooldownManager.GetRemainingCooldown("p09_manual_cast");
                    EditorGUILayout.LabelField($"Hồi chiêu Instant: {cdInstant:F1}s  |  Hồi chiêu Cast-Time: {cdCast:F1}s");
                }

                var targetA = P09ManualObservationController.TargetARef;
                var targetB = P09ManualObservationController.TargetBRef;
                float hpA = targetA != null && targetA.Health != null ? targetA.Health.CurrentHealth : 0f;
                float hpB = targetB != null && targetB.Health != null ? targetB.Health.CurrentHealth : 0f;
                EditorGUILayout.LabelField($"Target A HP: {hpA:F0} / 500  |  Target B HP: {hpB:F0} / 500");
                EditorGUILayout.LabelField($"Sát thương thực đo (EventBus): {P09ManualObservationController.LastDamageLog}", EditorStyles.boldLabel);

                // Telemetry summary
                EditorGUILayout.LabelField(
                    $"Telemetry: Lệnh={P09ManualObservationController.TotalCommandsIssued} | " +
                    $"Đạn phóng={P09ManualObservationController.TotalProjectileReleases} | " +
                    $"Dmg Events={P09ManualObservationController.TotalDamageEvents} | " +
                    $"Target chết={P09ManualObservationController.TotalTargetDeaths} | " +
                    $"Vi phạm={P09ManualObservationController.TotalUnexpectedViolations}");

                int projCount = ProjectileController.ActiveProjectiles.Count;
                EditorGUILayout.LabelField($"Số Projectile đang bay trong Game View: {projCount}", EditorStyles.boldLabel);
                for (int i = 0; i < projCount; i++)
                {
                    var p = ProjectileController.ActiveProjectiles[i];
                    if (p != null)
                    {
                        float dist = p.BoundTarget != null ? Vector3.Distance(p.transform.position, p.BoundTarget.transform.position) : 0f;
                        EditorGUILayout.LabelField($"  [{i + 1}] Target: {p.BoundTarget?.EntityName} | T: {p.ElapsedTime:F1}s/{p.Lifetime:F1}s | Khoảng cách: {dist:F2}u | Vị trí: {p.transform.position:F1}");
                    }
                }

                EditorGUILayout.Space(12);

                // 5. Scenario Mapping and Guidance
                EditorGUILayout.LabelField("5. DANH MỤC SCENARIO QUAN SÁT (P09-A CHECKLIST)", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "CÁC BÀI ĐÃ SẴN SÀNG THAO TÁC TRỰC TIẾP (READY):\n" +
                    "• Scenario 1: Thi triển Instant Đạn Bay -> Quan sát đạn bay mượt mà ở natural frames -> Nộ giảm 15 -> CD bắt đầu đếm 4.0s -> Va chạm trừ 200 HP (500 -> 300).\n" +
                    "• Scenario 2: Thi triển Cast-Time Đạn Bay -> Quan sát thanh vận khí 1.5s (Nộ trừ 25 lúc bắt đầu) -> Hết 1.5s đạn mới phóng ra và CD bắt đầu đếm 6.0s -> Va chạm trừ 350 HP (500 -> 150).\n" +
                    "• Scenario 3: Đổi mục tiêu trong lúc đạn đang bay -> Bấm 'Chọn Target B' khi đạn đang bay tới Target A -> Đạn vẫn giữ nguyên khóa mục tiêu gốc (Target A) và trúng Target A.\n" +
                    "• Scenario 4: Tạm dừng trong lúc đạn đang bay -> Bấm 'Tạm dừng Combat' -> Đạn đứng yên giữa không trung -> Bấm 'Tiếp tục Combat' -> Đạn tiếp tục bay tới đích.\n" +
                    "• Scenario 5: Reset Fixture -> Bấm 'Tạo / Reset Fixture' -> Khôi phục nguyên vẹn 100% về vị trí và HP ban đầu.\n\n" +
                    "CÁC BÀI TỰ ĐỘNG HÓA (NOT AVAILABLE TRONG UI THỦ CÔNG - ĐÃ CÓ TEST GATE 1):\n" +
                    "• Target / Caster chết khi đạn đang bay: Kiểm chứng bởi Automated Test T04 & T05.\n" +
                    "• Chuyển Encounter khi đạn đang bay: Kiểm chứng bởi Automated Test T07.\n" +
                    "• Đổi/Đóng Scene khi đạn đang bay: Kiểm chứng bởi Automated Test T21.",
                    MessageType.None);
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }

        private void Update()
        {
            if (EditorApplication.isPlaying && P09ManualSessionBootstrap.IsSessionAuthorized())
            {
                Repaint();
            }
        }
    }

    /// <summary>
    /// Bootstrap entry point invoked when launcher starts Unity for manual observation under Save Guard protection.
    /// Strictly guards session authorization.
    /// </summary>
    [InitializeOnLoad]
    public static class P09ManualSessionBootstrap
    {
        private static bool _pendingDelayCall = false;

        static P09ManualSessionBootstrap()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += () =>
            {
                if (EditorApplication.isPlaying && SessionState.GetBool("P09_Run_Autonomy_Verification", false))
                {
                    SessionState.SetBool("P09_Run_Autonomy_Verification", false);
                    P09ManualAutonomyVerifier.StartRunner();
                }
            };
        }

        public static bool IsSessionAuthorized()
        {
            bool argPresent = false;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "-p09ManualSession", StringComparison.OrdinalIgnoreCase))
                {
                    argPresent = true;
                    break;
                }
            }
            bool sessionStateAuth = SessionState.GetBool("P09_Manual_Session_Authorized", false);
            return argPresent && sessionStateAuth;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // CRITICAL SECURITY CHECK: If session is not authorized by launcher, DO NOTHING!
                if (!IsSessionAuthorized())
                {
                    return;
                }

                _pendingDelayCall = true;
                EditorApplication.delayCall += () =>
                {
                    if (!_pendingDelayCall) return;
                    _pendingDelayCall = false;
                    if (!IsSessionAuthorized()) return;

                    if (SessionState.GetBool("P09_Run_Autonomy_Verification", false))
                    {
                        SessionState.SetBool("P09_Run_Autonomy_Verification", false);
                        P09ManualAutonomyVerifier.StartRunner();
                    }
                    else
                    {
                        P09ManualTestWindow.ShowWindow();
                        P09ManualObservationController.SetupOrResetFixture();
                    }
                };
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _pendingDelayCall = false;
                if (IsSessionAuthorized())
                {
                    P09ManualObservationController.TeardownFixture();
                }
                // Revoke session authorization upon exiting Play Mode so subsequent ordinary Play Mode cannot auto-activate!
                SessionState.EraseBool("P09_Manual_Session_Authorized");
                SessionState.EraseBool("P09_Run_Autonomy_Verification");
            }
        }

        /// <summary>
        /// Entry point invoked strictly by launch_manual_p09a_session.ps1 via -executeMethod after Save Guard backup is secured.
        /// </summary>
        public static void LaunchFromSaveGuard()
        {
            Debug.Log("[P09 BOOTSTRAP] LaunchFromSaveGuard invoked by Save Guard launcher.");
            SessionState.SetBool("P09_Manual_Session_Authorized", true);

            // Create a dedicated in-memory untitled scene fixture so no user scene or production scene is modified or overwritten
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Open control window
            P09ManualTestWindow.ShowWindow();

            // Enter Play Mode (or setup immediately if already playing)
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
            }
            else
            {
                P09ManualObservationController.SetupOrResetFixture();
            }
        }
    }

    /// <summary>
    /// Automated security verifier executing Requirement 5A:
    /// Proves that when run without -p09ManualSession authorization, the manual harness:
    /// - IsSessionAuthorized returns FALSE
    /// - SetupOrResetFixture refuses execution
    /// - Mutators refuse execution
    /// - No fixture root or entities exist in hierarchy
    /// - PlayerPrefs TLTD_MM_ActiveId is not modified
    /// </summary>
    public static class P09ManualObservationSecurityVerifier
    {
        public static void RunSecurityVerification_CLI()
        {
            try
            {
                Debug.Log("============================================================");
                Debug.Log("   STARTING P09 MANUAL SECURITY & ISOLATION VERIFICATION");
                Debug.Log("============================================================");
                bool pass = true;

                // 1. IsSessionAuthorized must be false when launched without -p09ManualSession
                bool isAuth = P09ManualSessionBootstrap.IsSessionAuthorized();
                if (isAuth)
                {
                    Debug.LogError("[FAIL] IsSessionAuthorized returned true without launcher authorization!");
                    pass = false;
                }
                else
                {
                    Debug.Log("[PASS] Check 1: IsSessionAuthorized is FALSE in unauthorized environment.");
                }

                // 2. SetupOrResetFixture must refuse execution
                int resetBefore = P09ManualObservationController.ResetCount;
                P09ManualObservationController.SetupOrResetFixture();
                int resetAfter = P09ManualObservationController.ResetCount;
                if (resetAfter != resetBefore || P09ManualObservationController.IsFixtureReady)
                {
                    Debug.LogError("[FAIL] SetupOrResetFixture mutated state without authorization!");
                    pass = false;
                }
                else
                {
                    Debug.Log("[PASS] Check 2: SetupOrResetFixture refused execution (FixtureReady = FALSE, ResetCount unchanged).");
                }

                // 3. Mutator methods must refuse execution
                var instantRes = P09ManualObservationController.CastInstant();
                var castRes = P09ManualObservationController.CastCastTime();
                if (instantRes != null || castRes != null)
                {
                    Debug.LogError("[FAIL] Cast methods executed without authorization!");
                    pass = false;
                }
                else
                {
                    Debug.Log("[PASS] Check 3: CastInstant and CastCastTime refused execution (null result).");
                }

                // 4. Verify no fixture root exists in scene
                var fixtureRoot = GameObject.Find("[P09_Manual_Observation_Fixture_Root]");
                var hero = GameObject.Find("Hero_Observation");
                var targetA = GameObject.Find("Monster_Target_A");
                if (fixtureRoot != null || hero != null || targetA != null)
                {
                    Debug.LogError("[FAIL] Fixture GameObjects found in scene without authorization!");
                    pass = false;
                }
                else
                {
                    Debug.Log("[PASS] Check 4: Zero fixture objects found in hierarchy.");
                }

                // 5. Verify SetupOrResetFixture does not mutate PlayerPrefs
                string origMm = PlayerPrefs.GetString("TLTD_MM_ActiveId", "");
                try
                {
                    PlayerPrefs.SetString("TLTD_MM_ActiveId", "test_baseline_sentinel");
                    P09ManualObservationController.SetupOrResetFixture();
                    string testMm = PlayerPrefs.GetString("TLTD_MM_ActiveId", "");
                    if (testMm != "test_baseline_sentinel")
                    {
                        Debug.LogError($"[FAIL] SetupOrResetFixture modified PlayerPrefs! Expected 'test_baseline_sentinel', got '{testMm}'");
                        pass = false;
                    }
                    else
                    {
                        Debug.Log("[PASS] Check 5: SetupOrResetFixture does NOT mutate PlayerPrefs (Sentinel verified).");
                    }
                }
                finally
                {
                    PlayerPrefs.SetString("TLTD_MM_ActiveId", origMm);
                }

                Debug.Log("============================================================");
                Debug.Log($"   SECURITY VERIFICATION RESULT: {(pass ? "ALL 5 CHECKS PASSED" : "FAIL")}");
                Debug.Log("============================================================");

                EditorApplication.Exit(pass ? 0 : 1);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SECURITY VERIFIER FATAL] Exception: {ex}");
                EditorApplication.Exit(1);
            }
        }
    }

    /// <summary>
    /// Automated Autonomy & Isolation Verifier for P09-A (F-MANUAL-AUTONOMY).
    /// Executes the 4 required natural-frame Play Mode checks under Save Guard:
    /// Check 1: 10s idle post FIXTURE_READY (zero casts, zero damage, zero wild monsters, roster unchanged).
    /// Check 2: Single Instant command + in-flight freeze/resume + cooldown wait (zero auto recast).
    /// Check 3: Single Cast-Time command on reset fixture + cooldown wait (zero auto recast).
    /// Check 4: Natural death of Target A & B via skill commands, wave advance isolation (>= 2.5s), 3 consecutive resets, idle post-reset.
    /// </summary>
    public static class P09ManualAutonomyVerifier
    {
        public static void RunAutonomyVerification_CLI()
        {
            try
            {
                Debug.Log("============================================================");
                Debug.Log("   STARTING P09 MANUAL AUTONOMY & ISOLATION VERIFICATION");
                Debug.Log("============================================================");

                // Authorize session
                SessionState.SetBool("P09_Manual_Session_Authorized", true);
                SessionState.SetBool("P09_Run_Autonomy_Verification", true);

                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = true;
                }
                else
                {
                    StartRunner();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AUTONOMY VERIFIER FATAL] Exception: {ex}");
                EditorApplication.Exit(1);
            }
        }

        public static void StartRunner()
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<P09AutonomyTestRunner>();
            if (existing != null) return;

            var go = new GameObject("[P09_Autonomy_Test_Runner]");
            var runner = go.AddComponent<P09AutonomyTestRunner>();
            runner.StartCoroutine(runner.RunAll4ChecksCoroutine());
        }
    }

    /// <summary>
    /// Real Play Mode test runner executing natural-frame verifications.
    /// </summary>
    public class P09AutonomyTestRunner : MonoBehaviour
    {
        public IEnumerator RunAll4ChecksCoroutine()
        {
            bool allPassed = false;
            try
            {
                Debug.Log("[AUTONOMY RUNNER] Initializing Fixture for automated verification...");
                P09ManualObservationController.SetupOrResetFixture();
                yield return null;

                if (!P09ManualObservationController.IsFixtureReady)
                {
                    Debug.LogError("[AUTONOMY VERIFIER ERROR] Fixture failed to initialize to Ready state!");
                    yield break;
                }

                // =========================================================================
                // CHECK 1: Idle >= 10s after ready: zero casts, zero damage, zero wild monsters
                // =========================================================================
                Debug.Log("============================================================");
                Debug.Log("[CHECK 1 START] Testing 10-second idle autonomy suppression...");
                Debug.Log("============================================================");

                float check1Start = Time.time;
                bool check1Failed = false;
                while (Time.time - check1Start < 10.2f)
                {
                    yield return null;

                    if (P09ManualObservationController.TotalProjectileReleases > 0 ||
                        P09ManualObservationController.TotalDamageEvents > 0 ||
                        P09ManualObservationController.TotalTargetDeaths > 0 ||
                        P09ManualObservationController.TotalUnexpectedViolations > 0)
                    {
                        Debug.LogError($"[AUTONOMY VERIFIER ERROR] Unexpected autonomous activity during 10s idle! " +
                            $"Releases={P09ManualObservationController.TotalProjectileReleases}, " +
                            $"DamageEvents={P09ManualObservationController.TotalDamageEvents}, " +
                            $"Violations={P09ManualObservationController.TotalUnexpectedViolations}");
                        check1Failed = true;
                        break;
                    }

                    if (!Mathf.Approximately(P09ManualObservationController.HeroRef.Health.CurrentHealth, 1000f) ||
                        !Mathf.Approximately(P09ManualObservationController.HeroRef.Rage.CurrentRage, 100f))
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Hero HP or Rage mutated autonomously during 10s idle!");
                        check1Failed = true;
                        break;
                    }

                    if (!Mathf.Approximately(P09ManualObservationController.TargetARef.Health.CurrentHealth, 500f) ||
                        !Mathf.Approximately(P09ManualObservationController.TargetBRef.Health.CurrentHealth, 500f))
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Target A or B HP mutated autonomously during 10s idle!");
                        check1Failed = true;
                        break;
                    }
                }

                if (check1Failed) yield break;

                int monsterCountCheck1 = P09ManualObservationController.BattleManagerRef.ActiveMonsters.Count;
                if (monsterCountCheck1 != 2)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 1 failed: Expected roster count 2, got {monsterCountCheck1}");
                    yield break;
                }

                Debug.Log("[AUTONOMY CHECK 1 PASS] 10s Idle: Zero casts, zero releases, zero damage, HP/Nộ unchanged, zero wild monsters, roster strictly preserved.");

                // =========================================================================
                // CHECK 2: Single Instant command + in-flight freeze/resume + cooldown wait (4s)
                // =========================================================================
                Debug.Log("============================================================");
                Debug.Log("[CHECK 2 START] Testing single Instant command + in-flight pause/resume + cooldown...");
                Debug.Log("============================================================");

                P09ManualObservationController.SelectTargetA();
                yield return null;

                float initTargetAHp = P09ManualObservationController.TargetARef.Health.CurrentHealth; // 500
                float initHeroRage = P09ManualObservationController.HeroRef.Rage.CurrentRage; // 100
                int cmdBefore = P09ManualObservationController.TotalCommandsIssued;
                int relBefore = P09ManualObservationController.TotalProjectileReleases;
                int dmgBefore = P09ManualObservationController.TotalDamageEvents;

                var castRes = P09ManualObservationController.CastInstant();
                if (castRes == null || !castRes.Success)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 2 failed: CastInstant returned fail ({castRes?.ReasonDescription})");
                    yield break;
                }

                if (P09ManualObservationController.TotalCommandsIssued != cmdBefore + 1)
                {
                    Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 2 failed: TotalCommandsIssued did not increment by 1.");
                    yield break;
                }

                if (!Mathf.Approximately(P09ManualObservationController.HeroRef.Rage.CurrentRage, initHeroRage - 15f))
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 2 failed: Hero Rage not deducted by 15. Got {P09ManualObservationController.HeroRef.Rage.CurrentRage}");
                    yield break;
                }

                // Wait until projectile is active
                float releaseWait = 0f;
                while (ProjectileController.ActiveProjectiles.Count == 0 && releaseWait < 1.0f)
                {
                    yield return null;
                    releaseWait += Time.deltaTime;
                }

                if (ProjectileController.ActiveProjectiles.Count != 1)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 2 failed: Expected exactly 1 active projectile, got {ProjectileController.ActiveProjectiles.Count}");
                    yield break;
                }

                var proj = ProjectileController.ActiveProjectiles[0];

                // Mid-flight pause test: wait 0.2s of flight
                yield return new WaitForSeconds(0.2f);
                if (ProjectileController.ActiveProjectiles.Count == 0)
                {
                    Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 2 failed: Projectile impacted too early!");
                    yield break;
                }

                // Pause
                P09ManualObservationController.TogglePause();
                Vector3 pausedPos = proj.transform.position;

                // Wait 0.5s natural time while paused to confirm projectile freeze
                float pauseWait = 0f;
                bool pauseFailed = false;
                while (pauseWait < 0.5f)
                {
                    yield return null;
                    pauseWait += Time.deltaTime;
                    if (Vector3.Distance(proj.transform.position, pausedPos) > 0.001f)
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 2 failed: Projectile moved while combat paused!");
                        pauseFailed = true;
                        break;
                    }
                }
                if (pauseFailed) yield break;

                // Resume
                P09ManualObservationController.TogglePause();
                // Verify autonomous suppression survived resume
                if (P09ManualObservationController.HeroRef.Attack.IsAttackEnabled ||
                    P09ManualObservationController.TargetARef.Attack.IsAttackEnabled ||
                    (P09ManualObservationController.HeroRef.SkillDecisionController != null && P09ManualObservationController.HeroRef.SkillDecisionController.enabled))
                {
                    Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 2 failed: ResumeCombat re-enabled attack or AI!");
                    yield break;
                }

                // Wait for impact
                float flightWait = 0f;
                while (ProjectileController.ActiveProjectiles.Count > 0 && flightWait < 3.0f)
                {
                    yield return null;
                    flightWait += Time.deltaTime;
                }

                if (P09ManualObservationController.TotalDamageEvents != dmgBefore + 1)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 2 failed: Expected exactly 1 damage event, got {P09ManualObservationController.TotalDamageEvents - dmgBefore}");
                    yield break;
                }

                if (!Mathf.Approximately(P09ManualObservationController.TargetARef.Health.CurrentHealth, 300f))
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 2 failed: Target A HP expected 300 (500 - 200), got {P09ManualObservationController.TargetARef.Health.CurrentHealth}");
                    yield break;
                }

                // Wait past 4.0s cooldown + 1.0s buffer
                float cdWait = 0f;
                bool recastFailed2 = false;
                while (cdWait < 5.0f)
                {
                    yield return null;
                    cdWait += Time.deltaTime;
                    if (P09ManualObservationController.TotalCommandsIssued != cmdBefore + 1 ||
                        P09ManualObservationController.TotalProjectileReleases != relBefore + 1)
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 2 failed: Hero auto-recast instant skill after cooldown!");
                        recastFailed2 = true;
                        break;
                    }
                }
                if (recastFailed2) yield break;

                Debug.Log("[AUTONOMY CHECK 2 PASS] Instant Command: 1 command, 1 release, 1 damage event (200 dmg), freeze/resume verified, no auto-recast after cooldown.");

                // =========================================================================
                // CHECK 3: Single Cast-Time command on reset fixture + cooldown wait (6s)
                // =========================================================================
                Debug.Log("============================================================");
                Debug.Log("[CHECK 3 START] Testing single Cast-Time command on reset fixture + cooldown...");
                Debug.Log("============================================================");

                P09ManualObservationController.SetupOrResetFixture();
                yield return null;

                P09ManualObservationController.SelectTargetA();
                yield return null;

                initTargetAHp = P09ManualObservationController.TargetARef.Health.CurrentHealth; // 500
                initHeroRage = P09ManualObservationController.HeroRef.Rage.CurrentRage; // 100
                cmdBefore = P09ManualObservationController.TotalCommandsIssued;
                relBefore = P09ManualObservationController.TotalProjectileReleases;
                dmgBefore = P09ManualObservationController.TotalDamageEvents;

                var castRes3 = P09ManualObservationController.CastCastTime();
                if (castRes3 == null || !castRes3.Success)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 3 failed: CastCastTime returned fail ({castRes3?.ReasonDescription})");
                    yield break;
                }

                if (P09ManualObservationController.TotalCommandsIssued != cmdBefore + 1)
                {
                    Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 3 failed: TotalCommandsIssued did not increment by 1.");
                    yield break;
                }

                if (!Mathf.Approximately(P09ManualObservationController.HeroRef.Rage.CurrentRage, initHeroRage - 25f))
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 3 failed: Hero Rage not deducted by 25 at start. Got {P09ManualObservationController.HeroRef.Rage.CurrentRage}");
                    yield break;
                }

                if (!P09ManualObservationController.HeroRef.IsCasting)
                {
                    Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 3 failed: Hero is not in casting state after CastCastTime.");
                    yield break;
                }

                // While casting (1.5s): verify NO projectile released and NO damage dealt
                float castWait = 0f;
                float castStartTime = Time.time;
                bool castWindowFailed = false;
                while (castWait < 2.5f)
                {
                    if (!P09ManualObservationController.HeroRef.IsCasting)
                    {
                        // Casting completed! Verify that approximately 1.5s elapsed in natural time
                        float castElapsed = Time.time - castStartTime;
                        if (castElapsed < 1.35f)
                        {
                            Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 3 failed: Cast finished too early ({castElapsed:F2}s < 1.5s)!");
                            castWindowFailed = true;
                        }
                        break;
                    }

                    if (ProjectileController.ActiveProjectiles.Count > 0)
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 3 failed: Projectile released while still in casting state!");
                        castWindowFailed = true;
                        break;
                    }
                    if (P09ManualObservationController.TotalDamageEvents > dmgBefore)
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 3 failed: Damage dealt during cast-time window!");
                        castWindowFailed = true;
                        break;
                    }

                    yield return null;
                    castWait += Time.deltaTime;
                }
                if (castWindowFailed) yield break;

                // Once casting finished, projectile should be released
                float relWait3 = 0f;
                while (ProjectileController.ActiveProjectiles.Count == 0 && relWait3 < 1.0f)
                {
                    yield return null;
                    relWait3 += Time.deltaTime;
                }

                if (P09ManualObservationController.TotalProjectileReleases != relBefore + 1)
                {
                    Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 3 failed: Projectile was not released after cast completed.");
                    yield break;
                }

                // Verify cooldown triggered at release
                if (!CooldownManager.IsOnCooldown("p09_manual_cast", out float cdRemain) || cdRemain <= 0f)
                {
                    Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 3 failed: Cast-time skill cooldown did not trigger at release.");
                    yield break;
                }

                // Wait for projectile arrival
                float flightWait3 = 0f;
                while (ProjectileController.ActiveProjectiles.Count > 0 && flightWait3 < 3.0f)
                {
                    yield return null;
                    flightWait3 += Time.deltaTime;
                }

                if (P09ManualObservationController.TotalDamageEvents != dmgBefore + 1)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 3 failed: Expected exactly 1 damage event, got {P09ManualObservationController.TotalDamageEvents - dmgBefore}");
                    yield break;
                }

                if (!Mathf.Approximately(P09ManualObservationController.TargetARef.Health.CurrentHealth, 150f))
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 3 failed: Target A HP expected 150 (500 - 350), got {P09ManualObservationController.TargetARef.Health.CurrentHealth}");
                    yield break;
                }

                // Wait past 6.0s cooldown + 1.0s buffer
                float cdWait3 = 0f;
                bool recastFailed3 = false;
                while (cdWait3 < 7.0f)
                {
                    yield return null;
                    cdWait3 += Time.deltaTime;
                    if (P09ManualObservationController.TotalCommandsIssued != cmdBefore + 1 ||
                        P09ManualObservationController.TotalProjectileReleases != relBefore + 1)
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 3 failed: Hero auto-recast cast skill after cooldown!");
                        recastFailed3 = true;
                        break;
                    }
                }
                if (recastFailed3) yield break;

                Debug.Log("[AUTONOMY CHECK 3 PASS] Cast-Time Command: Rage deducted at start, release after 1.5s cast-time, cooldown at release, 350 dmg, no auto-recast after cooldown.");

                // =========================================================================
                // CHECK 4: Target death & wave isolation, 3 consecutive resets
                // =========================================================================
                Debug.Log("============================================================");
                Debug.Log("[CHECK 4 START] Testing Target death, wave isolation, and 3 consecutive resets...");
                Debug.Log("============================================================");

                // Finish Target A (HP is 150) using Instant (200 dmg)
                P09ManualObservationController.SelectTargetA();
                var resA = P09ManualObservationController.CastInstant();
                if (resA == null || !resA.Success)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 failed: Failed to cast Instant to finish Target A ({resA?.ReasonDescription})");
                    yield break;
                }

                // Wait for impact on Target A
                float killAWait = 0f;
                while (P09ManualObservationController.TargetARef.IsAlive && killAWait < 3.0f)
                {
                    yield return null;
                    killAWait += Time.deltaTime;
                }

                if (P09ManualObservationController.TargetARef.IsAlive)
                {
                    Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 4 failed: Target A did not die from legitimate skill damage!");
                    yield break;
                }

                // Wait for Instant cooldown: 4.2s
                float cdWaitA = 0f;
                while (CooldownManager.IsOnCooldown("p09_manual_instant", out _) && cdWaitA < 5.0f)
                {
                    yield return null;
                    cdWaitA += Time.deltaTime;
                }

                // Target B has 500 HP: weaken with Cast-Time (350 dmg)
                P09ManualObservationController.SelectTargetB();
                float cdWaitCast = 0f;
                while (CooldownManager.IsOnCooldown("p09_manual_cast", out _) && cdWaitCast < 7.0f)
                {
                    yield return null;
                    cdWaitCast += Time.deltaTime;
                }

                var resB1 = P09ManualObservationController.CastCastTime();
                if (resB1 == null || !resB1.Success)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 failed: Failed to cast CastTime on Target B ({resB1?.ReasonDescription})");
                    yield break;
                }

                // Wait for cast + impact on Target B
                float castFlightB = 0f;
                while (castFlightB < 4.5f && P09ManualObservationController.TargetBRef.Health.CurrentHealth > 150f)
                {
                    yield return null;
                    castFlightB += Time.deltaTime;
                }

                if (!Mathf.Approximately(P09ManualObservationController.TargetBRef.Health.CurrentHealth, 150f))
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 failed: Target B HP expected 150, got {P09ManualObservationController.TargetBRef.Health.CurrentHealth}");
                    yield break;
                }

                // Wait for Instant cooldown
                float cdWaitB2 = 0f;
                while (CooldownManager.IsOnCooldown("p09_manual_instant", out _) && cdWaitB2 < 5.0f)
                {
                    yield return null;
                    cdWaitB2 += Time.deltaTime;
                }

                // Finish Target B with Instant (200 dmg > 150 HP)
                P09ManualObservationController.SelectTargetB();
                var resB2 = P09ManualObservationController.CastInstant();
                if (resB2 == null || !resB2.Success)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 failed: Failed to cast Instant to finish Target B ({resB2?.ReasonDescription})");
                    yield break;
                }

                float killBWait = 0f;
                while (P09ManualObservationController.TargetBRef.IsAlive && killBWait < 3.0f)
                {
                    yield return null;
                    killBWait += Time.deltaTime;
                }

                if (P09ManualObservationController.TargetBRef.IsAlive)
                {
                    Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 4 failed: Target B did not die from legitimate skill damage!");
                    yield break;
                }

                // Observe wave isolation for >= 2.5 seconds
                float waveIsoWait = 0f;
                int initialWaveId = P09ManualObservationController.BattleManagerRef.CurrentWaveId;
                bool waveIsoFailed = false;
                while (waveIsoWait < 2.8f)
                {
                    yield return null;
                    waveIsoWait += Time.deltaTime;

                    if (P09ManualObservationController.BattleManagerRef.CurrentWaveId != initialWaveId)
                    {
                        Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 failed: BattleManager advanced wave id ({P09ManualObservationController.BattleManagerRef.CurrentWaveId})!");
                        waveIsoFailed = true;
                        break;
                    }
                    if (P09ManualObservationController.BattleManagerRef.CompletedNormalWaveCount > 0)
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 4 failed: CompletedNormalWaveCount incremented!");
                        waveIsoFailed = true;
                        break;
                    }
                    if (P09ManualObservationController.TotalUnexpectedViolations > 0)
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 4 failed: Unexpected violation occurred post-target death!");
                        waveIsoFailed = true;
                        break;
                    }
                    var allMonsters = UnityEngine.Object.FindObjectsByType<Monster>(FindObjectsSortMode.None);
                    if (allMonsters.Length > 2)
                    {
                        Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 failed: Wild Monster detected! Monster count in scene: {allMonsters.Length}");
                        waveIsoFailed = true;
                        break;
                    }
                }
                if (waveIsoFailed) yield break;

                // Verify complete cycle telemetry (from Check 3 reset through Check 4 target deaths):
                // Exactly 4 successful commands, 4 projectile releases, 4 damage events, and 2 target deaths
                if (P09ManualObservationController.TotalCommandsIssued != 4)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 failed: Expected exactly 4 commands issued, got {P09ManualObservationController.TotalCommandsIssued}");
                    yield break;
                }
                if (P09ManualObservationController.TotalProjectileReleases != 4)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 failed: Expected exactly 4 projectile releases, got {P09ManualObservationController.TotalProjectileReleases}");
                    yield break;
                }
                if (P09ManualObservationController.TotalDamageEvents != 4)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 failed: Expected exactly 4 damage events, got {P09ManualObservationController.TotalDamageEvents}");
                    yield break;
                }
                if (P09ManualObservationController.TotalTargetDeaths != 2)
                {
                    Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 failed: Expected exactly 2 target deaths, got {P09ManualObservationController.TotalTargetDeaths}");
                    yield break;
                }

                // Perform 3 consecutive resets
                bool resetFailed = false;
                for (int cycle = 1; cycle <= 3; cycle++)
                {
                    P09ManualObservationController.SetupOrResetFixture();
                    yield return null;

                    var allRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                    int rootCount = 0;
                    foreach (var r in allRoots)
                    {
                        if (r != null && r.name == "[P09_Manual_Observation_Fixture_Root]") rootCount++;
                    }
                    if (rootCount != 1)
                    {
                        Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 Reset #{cycle} failed: Expected exactly 1 fixture root, found {rootCount}");
                        resetFailed = true;
                        break;
                    }

                    var heroes = UnityEngine.Object.FindObjectsByType<Hero>(FindObjectsSortMode.None);
                    if (heroes.Length != 1)
                    {
                        Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 Reset #{cycle} failed: Expected exactly 1 hero, found {heroes.Length}");
                        resetFailed = true;
                        break;
                    }

                    var monsters = UnityEngine.Object.FindObjectsByType<Monster>(FindObjectsSortMode.None);
                    if (monsters.Length != 2)
                    {
                        Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 Reset #{cycle} failed: Expected exactly 2 monsters, found {monsters.Length}");
                        resetFailed = true;
                        break;
                    }

                    var cams = UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
                    if (cams.Length != 1)
                    {
                        Debug.LogError($"[AUTONOMY VERIFIER ERROR] Check 4 Reset #{cycle} failed: Expected exactly 1 camera, found {cams.Length}");
                        resetFailed = true;
                        break;
                    }
                }
                if (resetFailed) yield break;

                // Observe idle post-reset for 2.0 seconds
                float postResetWait = 0f;
                bool postResetFailed = false;
                while (postResetWait < 2.0f)
                {
                    yield return null;
                    postResetWait += Time.deltaTime;

                    if (!Mathf.Approximately(P09ManualObservationController.HeroRef.Health.CurrentHealth, 1000f) ||
                        !Mathf.Approximately(P09ManualObservationController.HeroRef.Rage.CurrentRage, 100f))
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 4 failed: Hero HP or Rage mutated during post-reset idle!");
                        postResetFailed = true;
                        break;
                    }
                    if (P09ManualObservationController.TotalProjectileReleases > 0 ||
                        P09ManualObservationController.TotalDamageEvents > 0 ||
                        P09ManualObservationController.TotalUnexpectedViolations > 0)
                    {
                        Debug.LogError("[AUTONOMY VERIFIER ERROR] Check 4 failed: Activity detected during post-reset idle!");
                        postResetFailed = true;
                        break;
                    }
                }
                if (postResetFailed) yield break;

                Debug.Log("[AUTONOMY CHECK 4 PASS] Target Death & Wave Isolation: Both targets killed naturally, zero wave transitions, zero Wild Monsters, 3 clean resets, idle post-reset quiet.");

                allPassed = true;
                Debug.Log("============================================================");
                Debug.Log("   [AUTONOMY VERIFIER RESULT]: ALL 4 CHECKS PASSED");
                Debug.Log("============================================================");
            }
            finally
            {
                P09ManualObservationController.TeardownFixture();
                if (Application.isBatchMode)
                {
                    EditorApplication.isPlaying = false;
                    EditorApplication.Exit(allPassed ? 0 : 1);
                }
            }
        }
    }
}
