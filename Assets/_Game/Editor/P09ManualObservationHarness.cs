using System;
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

namespace WuxiaGame.Editor
{
    /// <summary>
    /// P09-A Manual Observation Controller (Static Management Core).
    /// Manages transient RAM scene fixture, arena visualization, camera, combatants (Hero, Target A, Target B),
    /// skill casting (Instant and Cast-Time projectiles), target switching, and real game pause/resume.
    /// Operates strictly with natural frames (Time.timeScale = 1).
    /// Enforces strict session authorization to prevent any side-effects in normal Editor gameplay.
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

        public static bool IsFixtureReady =>
            _fixtureRoot != null &&
            HeroRef != null &&
            TargetARef != null &&
            TargetBRef != null &&
            CameraRef != null &&
            InstantSkill != null &&
            CastSkill != null;

        private static void HandleEntityDamaged(Entity victim, DamageResult result)
        {
            if (victim == null) return;
            float curHp = victim.Health != null ? victim.Health.CurrentHealth : 0f;
            float maxHp = victim.Health != null ? victim.Health.MaxHealth : 0f;
            string critStr = result.IsCrit ? " (BẠO KÍCH!)" : "";
            LastDamageLog = $"Gây {result.FinalDamage:F0}{critStr} sát thương vào {victim.EntityName} (HP còn: {curHp:F0}/{maxHp:F0})";
            Debug.Log($"[P09 MANUAL EVENT] {LastDamageLog}");
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
            Debug.Log($"[P09 MANUAL] Initializing Fixture (Cycle #{ResetCount}) under isolated root in Play Mode...");

            // 1. Create dedicated root for all fixture objects
            _fixtureRoot = new GameObject("[P09_Manual_Observation_Fixture_Root]");

            // 2. Setup Services under fixture root
            var servicesGO = new GameObject("Fixture_Services");
            servicesGO.transform.SetParent(_fixtureRoot.transform, false);
            servicesGO.AddComponent<EquipmentManager>();
            servicesGO.AddComponent<Inventory.Inventory>();
            servicesGO.AddComponent<ResourceManager>();
            servicesGO.AddComponent<ProgressionManager>();

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
            AttachVisualPrimitive(heroGO, PrimitiveType.Capsule, Color.cyan, new Vector3(1.0f, 1.2f, 1.0f), "HERO (Cyan)");

            // 6. Setup Target A (Red Sphere, X = 4.5, Y = 1.0)
            var targetAGO = new GameObject("Monster_Target_A");
            targetAGO.transform.SetParent(_fixtureRoot.transform, false);
            targetAGO.transform.position = new Vector3(4.5f, 1.0f, 0f);
            TargetARef = targetAGO.AddComponent<Monster>();
            _mCfgA = ScriptableObject.CreateInstance<MonsterConfigSO>();
            _mCfgA.InitializeMonsterConfig("Target A (Top)", 500f, 10f, 0f, 2f, 2f, 2f, 50);
            _cCfgA = ScriptableObject.CreateInstance<CombatConfigSO>();
            TargetARef.InitializeMonster(_mCfgA, _cCfgA);
            AttachVisualPrimitive(targetAGO, PrimitiveType.Sphere, Color.red, new Vector3(1.2f, 1.2f, 1.2f), "TARGET A (Red)");

            // 7. Setup Target B (Orange Sphere, X = 4.5, Y = -1.0)
            var targetBGO = new GameObject("Monster_Target_B");
            targetBGO.transform.SetParent(_fixtureRoot.transform, false);
            targetBGO.transform.position = new Vector3(4.5f, -1.0f, 0f);
            TargetBRef = targetBGO.AddComponent<Monster>();
            _mCfgB = ScriptableObject.CreateInstance<MonsterConfigSO>();
            _mCfgB.InitializeMonsterConfig("Target B (Bottom)", 500f, 10f, 0f, 2f, 2f, 2f, 50);
            _cCfgB = ScriptableObject.CreateInstance<CombatConfigSO>();
            TargetBRef.InitializeMonster(_mCfgB, _cCfgB);
            AttachVisualPrimitive(targetBGO, PrimitiveType.Sphere, new Color(1f, 0.5f, 0f), new Vector3(1.2f, 1.2f, 1.2f), "TARGET B (Orange)");

            // 8. Register combatants into BattleManager
            BattleManagerRef.RegisterHero(HeroRef);
            BattleManagerRef.RegisterMonster(TargetARef);
            BattleManagerRef.RegisterMonster(TargetBRef);
            BattleManagerRef.StartBattle();

            // Disable ambient auto-attacks so observations are 100% isolated to manual button clicks
            if (HeroRef.Attack != null) HeroRef.Attack.SetAttackEnabled(false);
            if (TargetARef.Attack != null) TargetARef.Attack.SetAttackEnabled(false);
            if (TargetBRef.Attack != null) TargetBRef.Attack.SetAttackEnabled(false);

            // Subscribe to damage events
            EventBus.OnEntityDamaged -= HandleEntityDamaged;
            EventBus.OnEntityDamaged += HandleEntityDamaged;

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
            Debug.Log("[P09 MANUAL] Fixture setup complete under dedicated root [P09_Manual_Observation_Fixture_Root].");
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
            LastActionLog = $"[Instant Cast] Kết quả: {resStr} | Target: {CurrentTargetRef?.EntityName} | Đạn đang bay: {ProjectileController.ActiveProjectiles.Count}";
            Debug.Log($"[P09 MANUAL] Cast Instant via Hero.ExecuteSelectedSkill: Success={res?.Success}, Target={CurrentTargetRef?.EntityName}, ProjCount={ProjectileController.ActiveProjectiles.Count}");
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
            LastActionLog = $"[Cast-Time Cast] Kết quả: {resStr} | Target: {CurrentTargetRef?.EntityName} | IsCasting={HeroRef.IsCasting}";
            Debug.Log($"[P09 MANUAL] Cast Cast-Time via Hero.ExecuteSelectedSkill: Success={res?.Success}, Target={CurrentTargetRef?.EntityName}, IsCasting={HeroRef.IsCasting}");
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
            win.minSize = new Vector2(460, 680);
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
                    P09ManualTestWindow.ShowWindow();
                    P09ManualObservationController.SetupOrResetFixture();
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
}
