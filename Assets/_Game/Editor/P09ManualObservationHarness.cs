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
    /// NOTE: Must NOT be a MonoBehaviour since it lives in an Editor assembly.
    /// </summary>
    public static class P09ManualObservationController
    {
        public static Hero HeroRef;
        public static Monster TargetARef;
        public static Monster TargetBRef;
        public static Monster CurrentTargetRef;
        public static BattleManager BattleManagerRef;
        public static MindMethodManager MindMethodManagerRef;
        public static Camera CameraRef;

        public static SkillDefinitionSO InstantSkill;
        public static SkillDefinitionSO CastSkill;

        public static bool IsFixtureReady =>
            HeroRef != null &&
            TargetARef != null &&
            TargetBRef != null &&
            CameraRef != null &&
            InstantSkill != null;

        public static void CleanupSkills()
        {
            if (InstantSkill != null)
            {
                if (InstantSkill.Effects != null)
                {
                    foreach (var eff in InstantSkill.Effects)
                    {
                        if (eff != null) UnityEngine.Object.DestroyImmediate(eff);
                    }
                }
                UnityEngine.Object.DestroyImmediate(InstantSkill);
                InstantSkill = null;
            }
            if (CastSkill != null)
            {
                if (CastSkill.Effects != null)
                {
                    foreach (var eff in CastSkill.Effects)
                    {
                        if (eff != null) UnityEngine.Object.DestroyImmediate(eff);
                    }
                }
                UnityEngine.Object.DestroyImmediate(CastSkill);
                CastSkill = null;
            }
        }

        public static void SetupOrResetFixture()
        {
            Debug.Log("[P09 MANUAL] Setting up / Resetting RAM Observation Fixture in Play Mode...");

            // 1. Clear existing projectiles and managers
            ProjectileController.ClearAllProjectiles();
            MindMethodManager.ResetInstance();
            BattleManager.ResetInstance();
            EquipmentManager.ResetInstance();
            Inventory.Inventory.ResetInstance();
            ResourceManager.ResetInstance();
            ProgressionManager.ResetInstance();
            EventBus.ClearAllListeners();

            // Clean up any previously created fixture GameObjects
            DestroyFixtureObject("Main Camera");
            DestroyFixtureObject("Directional Light");
            DestroyFixtureObject("Arena_Ground_Marker");
            DestroyFixtureObject("Hero_Observation");
            DestroyFixtureObject("Monster_Target_A");
            DestroyFixtureObject("Monster_Target_B");
            DestroyFixtureObject("P09_BattleManager");
            DestroyFixtureObject("P09_MindMethodManager");
            DestroyFixtureObject("TestServices");
            CleanupSkills();

            // 2. Setup Services
            var servicesGO = new GameObject("TestServices");
            servicesGO.AddComponent<EquipmentManager>();
            servicesGO.AddComponent<Inventory.Inventory>();
            servicesGO.AddComponent<ResourceManager>();
            servicesGO.AddComponent<ProgressionManager>();

            // 3. Setup Camera & Visual Environment (Crucial for Game View rendering!)
            SetupCameraAndEnvironment();

            // 4. Setup BattleManager
            var bmGO = new GameObject("P09_BattleManager");
            BattleManagerRef = bmGO.AddComponent<BattleManager>();

            // 5. Setup Hero (Cyan Capsule, X = -4.5, Y = 0)
            var heroGO = new GameObject("Hero_Observation");
            heroGO.transform.position = new Vector3(-4.5f, 0f, 0f);
            HeroRef = heroGO.AddComponent<Hero>();
            HeroRef.InitializeHero();
            HeroRef.Health.InitializeHealth(1000f, HeroRef);
            HeroRef.Rage.InitializeRage(100f, 100f, HeroRef);
            AttachVisualPrimitive(heroGO, PrimitiveType.Capsule, Color.cyan, new Vector3(1.0f, 1.2f, 1.0f), "HERO (Cyan)");

            // 6. Setup Target A (Red Sphere, X = 4.5, Y = 1.0)
            var targetAGO = new GameObject("Monster_Target_A");
            targetAGO.transform.position = new Vector3(4.5f, 1.0f, 0f);
            TargetARef = targetAGO.AddComponent<Monster>();
            var mCfgA = ScriptableObject.CreateInstance<MonsterConfigSO>();
            mCfgA.InitializeMonsterConfig("Target A (Top)", 500f, 10f, 0f, 2f, 2f, 2f, 50);
            var cCfgA = ScriptableObject.CreateInstance<CombatConfigSO>();
            TargetARef.InitializeMonster(mCfgA, cCfgA);
            AttachVisualPrimitive(targetAGO, PrimitiveType.Sphere, Color.red, new Vector3(1.2f, 1.2f, 1.2f), "TARGET A (Red)");

            // 7. Setup Target B (Orange Sphere, X = 4.5, Y = -1.0)
            var targetBGO = new GameObject("Monster_Target_B");
            targetBGO.transform.position = new Vector3(4.5f, -1.0f, 0f);
            TargetBRef = targetBGO.AddComponent<Monster>();
            var mCfgB = ScriptableObject.CreateInstance<MonsterConfigSO>();
            mCfgB.InitializeMonsterConfig("Target B (Bottom)", 500f, 10f, 0f, 2f, 2f, 2f, 50);
            var cCfgB = ScriptableObject.CreateInstance<CombatConfigSO>();
            TargetBRef.InitializeMonster(mCfgB, cCfgB);
            AttachVisualPrimitive(targetBGO, PrimitiveType.Sphere, new Color(1f, 0.5f, 0f), new Vector3(1.2f, 1.2f, 1.2f), "TARGET B (Orange)");

            // 8. Register combatants into BattleManager
            BattleManagerRef.RegisterHero(HeroRef);
            BattleManagerRef.RegisterMonster(TargetARef);
            BattleManagerRef.RegisterMonster(TargetBRef);
            BattleManagerRef.StartBattle();

            // Set initial target to Target A
            CurrentTargetRef = TargetARef;
            HeroRef.SetCurrentTarget(TargetARef);

            // 9. Setup MindMethodManager & Transient Skills
            var mmGO = new GameObject("P09_MindMethodManager");
            MindMethodManagerRef = mmGO.AddComponent<MindMethodManager>();
            typeof(MindMethodManager).GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.SetValue(null, MindMethodManagerRef);

            var tempDb = ScriptableObject.CreateInstance<MindMethodDatabaseSO>();
            var tempDef = ScriptableObject.CreateInstance<MindMethodDefinitionSO>();
            tempDef.InitializeMindMethod("mm_p09_manual", "P09 Quan Sát", "Tâm Pháp Quan Sát Đạn Bay", 10, true, new MindMethodPassiveData());
            tempDb.SetMindMethods(new List<MindMethodDefinitionSO> { tempDef });
            MindMethodManagerRef.SetDatabase(tempDb);
            MindMethodManagerRef.SetActiveMindMethod("mm_p09_manual");

            // Skill 1: Instant Projectile (Speed = 6, Lifetime = 8s, CD = 4s, Rage = 15, Dmg = 100)
            InstantSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            var effInstant = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
            effInstant.Initialize(2.0f, SkillTargetPolicy.SingleTarget, DamageType.Skill);
            InstantSkill.InitializeSkill(
                id: "p09_manual_instant",
                mmId: "mm_p09_manual",
                slot: SkillSlotType.Skill,
                name: "Thái Cực Kiếm Khí (Instant Đạn Bay)",
                desc: "Đạn bay tức thời tốc độ 6 unit/s quan sát trực quan",
                conditions: null,
                dmgMultiplier: 2.0f,
                costRage: 15f,
                cd: 4.0f,
                passive: false,
                skillEffects: new List<SkillEffectDefinitionSO> { effInstant },
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

            // Skill 2: Cast-Time Projectile (1.5s cast, Speed = 6, Lifetime = 8s, CD = 6s, Rage = 25, Dmg = 150)
            CastSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
            var effCast = ScriptableObject.CreateInstance<DamageEffectDefinitionSO>();
            effCast.Initialize(3.5f, SkillTargetPolicy.SingleTarget, DamageType.Skill);
            CastSkill.InitializeSkill(
                id: "p09_manual_cast",
                mmId: "mm_p09_manual",
                slot: SkillSlotType.Skill,
                name: "Huyền Vũ Thần Tiễn (1.5s Vận Khí + Đạn Bay)",
                desc: "Vận khí 1.5s sau đó phóng đạn tốc độ 6 unit/s",
                conditions: null,
                dmgMultiplier: 3.5f,
                costRage: 25f,
                cd: 6.0f,
                passive: false,
                skillEffects: new List<SkillEffectDefinitionSO> { effCast },
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

            // Register skills into active definition
            var skillsField = typeof(MindMethodDefinitionSO).GetField("skills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = skillsField != null ? skillsField.GetValue(tempDef) as List<SkillDefinitionSO> : null;
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

            // Ensure natural frame execution
            Time.timeScale = 1f;

            Debug.Log("[P09 MANUAL] Fixture setup complete: Camera active, Hero at (-4.5, 0), Target A at (4.5, 1.0), Target B at (4.5, -1.0). Initial Target: Target A.");
        }

        private static void DestroyFixtureObject(string name)
        {
            var obj = GameObject.Find(name);
            if (obj != null)
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(obj);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(obj);
                }
            }
        }

        private static void SetupCameraAndEnvironment()
        {
            // Camera
            var camObj = GameObject.FindWithTag("MainCamera");
            if (camObj == null) camObj = GameObject.Find("Main Camera");

            if (camObj == null)
            {
                camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                CameraRef = camObj.AddComponent<Camera>();
            }
            else
            {
                CameraRef = camObj.GetComponent<Camera>();
                if (CameraRef == null) CameraRef = camObj.AddComponent<Camera>();
            }

            CameraRef.transform.position = new Vector3(0f, 0f, -10f);
            CameraRef.transform.rotation = Quaternion.identity;
            CameraRef.orthographic = true;
            CameraRef.orthographicSize = 4.5f;
            CameraRef.nearClipPlane = 0.3f;
            CameraRef.farClipPlane = 100f;
            CameraRef.clearFlags = CameraClearFlags.SolidColor;
            CameraRef.backgroundColor = new Color(0.12f, 0.15f, 0.20f, 1f);
            CameraRef.enabled = true;

            // Directional Light
            var lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                lightObj = new GameObject("Directional Light");
                var l = lightObj.AddComponent<Light>();
                l.type = LightType.Directional;
                l.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                l.intensity = 1.2f;
                l.color = Color.white;
            }

            // Ground reference bar
            var ground = GameObject.Find("Arena_Ground_Marker");
            if (ground == null)
            {
                ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ground.name = "Arena_Ground_Marker";
                ground.transform.position = new Vector3(0f, -2.5f, 0f);
                ground.transform.localScale = new Vector3(16f, 0.3f, 1f);
                var r = ground.GetComponent<Renderer>();
                if (r != null) r.material.color = new Color(0.25f, 0.30f, 0.35f);
                var c = ground.GetComponent<Collider>();
                if (c != null)
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(c);
                    else UnityEngine.Object.DestroyImmediate(c);
                }
            }
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
            if (c != null)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(c);
                else UnityEngine.Object.DestroyImmediate(c);
            }

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

        public static SkillExecutionResult CastInstant()
        {
            if (HeroRef == null || InstantSkill == null) return null;
            if (CurrentTargetRef == null || !CurrentTargetRef.IsAlive)
            {
                CurrentTargetRef = TargetARef != null && TargetARef.IsAlive ? TargetARef : TargetBRef;
            }
            HeroRef.SetCurrentTarget(CurrentTargetRef);
            var req = new SkillExecutionRequest(HeroRef, InstantSkill, SkillSlotType.Skill, CurrentTargetRef);
            var res = SkillExecutor.Execute(req);
            Debug.Log($"[P09 MANUAL] Cast Instant: Success={res.Success}, Target={CurrentTargetRef?.EntityName}, ProjCount={ProjectileController.ActiveProjectiles.Count}");
            return res;
        }

        public static SkillExecutionResult CastCastTime()
        {
            if (HeroRef == null || CastSkill == null) return null;
            if (CurrentTargetRef == null || !CurrentTargetRef.IsAlive)
            {
                CurrentTargetRef = TargetARef != null && TargetARef.IsAlive ? TargetARef : TargetBRef;
            }
            HeroRef.SetCurrentTarget(CurrentTargetRef);
            var req = new SkillExecutionRequest(HeroRef, CastSkill, SkillSlotType.Skill, CurrentTargetRef);
            var res = SkillExecutor.Execute(req);
            Debug.Log($"[P09 MANUAL] Cast Cast-Time: Success={res.Success}, Target={CurrentTargetRef?.EntityName}, IsCasting={HeroRef.IsCasting}");
            return res;
        }

        public static void SelectTargetA()
        {
            CurrentTargetRef = TargetARef;
            if (HeroRef != null) HeroRef.SetCurrentTarget(TargetARef);
            Debug.Log("[P09 MANUAL] Selected Target A (Red, Top).");
        }

        public static void SelectTargetB()
        {
            CurrentTargetRef = TargetBRef;
            if (HeroRef != null) HeroRef.SetCurrentTarget(TargetBRef);
            Debug.Log("[P09 MANUAL] Selected Target B (Orange, Bottom).");
        }

        public static void TogglePause()
        {
            if (BattleManagerRef == null) return;
            if (BattleManagerRef.IsCombatPausedByUI)
            {
                BattleManagerRef.ResumeCombat();
                Debug.Log("[P09 MANUAL] Resumed Combat (BattleManager.ResumeCombat).");
            }
            else
            {
                BattleManagerRef.PauseCombat();
                Debug.Log("[P09 MANUAL] Paused Combat (BattleManager.PauseCombat).");
            }
        }
    }

    /// <summary>
    /// Interactive EditorWindow control panel for manual observation of P09-A projectiles.
    /// Provides clickable buttons for instant cast, cast-time cast, retargeting A/B, pause/resume, and fixture reset.
    /// Displays live runtime stats: HP, Rage, Cooldowns, Casting progress, Bound Target, and Active Projectiles.
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
                bool isProtected = SessionState.GetBool("P09_Manual_Session_Authorized", false);

                // Title Banner
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("P09-A PROJECTILE OBSERVATION CONTROL", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    isProtected
                        ? "SAVE GUARD PROTECTED SESSION\nThis session was initiated via launch_manual_p09a_session.ps1. Full pre-run backup secured. Exact restore will execute automatically upon session closure."
                        : "DIRECT EDITOR SESSION (Unprotected)\nSave Guard wrapper is not active. To run with full workspace persistence protection and comparison verification, launch via Tools\\Verification\\P09\\launch_manual_p09a_session.ps1.",
                    isProtected ? MessageType.Info : MessageType.Warning);

                EditorGUILayout.Space(8);

                // Play Mode Check
                if (!EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Session must be in PLAY MODE to observe natural projectile movement frames.", MessageType.Warning);
                    if (GUILayout.Button("Vào Play Mode & Mở Fixture Quan Sát", GUILayout.Height(38)))
                    {
                        EditorApplication.isPlaying = true;
                    }
                    return;
                }

                // If in Play Mode but fixture not yet initialized, initialize it automatically
                if (!P09ManualObservationController.IsFixtureReady)
                {
                    P09ManualObservationController.SetupOrResetFixture();
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
                    ? $"{curTarget.Health.CurrentHealth:F0}/{curTarget.Health.MaxHealth:F0}"
                    : "N/A";
                EditorGUILayout.LabelField($"   Mục tiêu đang chọn: {targetName} | HP: {targetHpStr}");

                EditorGUILayout.Space(8);

                // 3. Skill Casting Controls
                EditorGUILayout.LabelField("3. THI TRIỂN KỸ NĂNG ĐẠN BAY", EditorStyles.boldLabel);
                if (GUILayout.Button("Thi triển Instant Đạn Bay (Speed=6, CD=4s, Rage=15)", GUILayout.Height(32)))
                {
                    P09ManualObservationController.CastInstant();
                }
                if (GUILayout.Button("Thi triển Cast-Time Đạn Bay (1.5s Vận Khí, Speed=6, CD=6s, Rage=25)", GUILayout.Height(32)))
                {
                    P09ManualObservationController.CastCastTime();
                }

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

                    EditorGUILayout.LabelField($"Hero HP: {heroHp:F0} / {heroMaxHp:F0}  |  Rage: {heroRage:F0} / {heroMaxRage:F0}");

                    bool isCasting = hero.IsCasting;
                    float castProgress = hero.CastProgress;
                    EditorGUILayout.LabelField($"Trạng thái Casting: {(isCasting ? $"ĐANG VẬN KHÍ ({castProgress * 100:F0}%)" : "Nhàn rỗi (Ready)")}");

                    float cdInstant = CooldownManager.GetRemainingCooldown("p09_manual_instant");
                    float cdCast = CooldownManager.GetRemainingCooldown("p09_manual_cast");
                    EditorGUILayout.LabelField($"Hồi chiêu Instant: {cdInstant:F1}s  |  Hồi chiêu Cast-Time: {cdCast:F1}s");
                }

                int projCount = ProjectileController.ActiveProjectiles.Count;
                EditorGUILayout.LabelField($"Số Projectile đang bay trong Game View: {projCount}", EditorStyles.boldLabel);
                for (int i = 0; i < projCount; i++)
                {
                    var p = ProjectileController.ActiveProjectiles[i];
                    if (p != null)
                    {
                        float dist = p.BoundTarget != null ? Vector3.Distance(p.transform.position, p.BoundTarget.transform.position) : 0f;
                        EditorGUILayout.LabelField($"  [{i + 1}] Target: {p.BoundTarget?.EntityName} | T: {p.ElapsedTime:F1}s/{p.Lifetime:F1}s | Dist: {dist:F2}u | Pos: {p.transform.position:F1}");
                    }
                }

                EditorGUILayout.Space(12);

                // 5. Scenario Mapping and Guidance
                EditorGUILayout.LabelField("5. DANH MỤC SCENARIO QUAN SÁT (P09-A CHECKLIST)", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "CÁC BÀI ĐÃ SẴN SÀNG THAO TÁC TRỰC TIẾP (READY):\n" +
                    "• Scenario 1: Thi triển Instant Đạn Bay -> Quan sát đạn bay mượt mà ở natural frames -> Va chạm gây sát thương -> Tiêu hao nộ & hồi chiêu.\n" +
                    "• Scenario 2: Thi triển Cast-Time Đạn Bay -> Quan sát thanh vận khí 1.5s -> Sau vận khí mới phóng đạn -> Va chạm gây sát thương.\n" +
                    "• Scenario 3: Đổi mục tiêu trong lúc đạn đang bay -> Bấm 'Chọn Target B' khi đạn đang bay tới Target A -> Kiểm chứng đạn KHÔNG đổi hướng vô căn cứ, vẫn bay trúng Target A đã bind.\n" +
                    "• Scenario 4: Tạm dừng trong lúc đạn đang bay -> Bấm 'Tạm dừng Combat' -> Đạn đứng yên giữa không trung -> Bấm 'Tiếp tục Combat' -> Đạn tiếp tục bay tới đích.\n" +
                    "• Scenario 5: Reset Fixture -> Bấm 'Tạo / Reset Fixture' -> Khôi phục nguyên vẹn 100% không cần khởi động lại Unity.\n\n" +
                    "CÁC BÀI TỰ ĐỘNG HÓA (NOT AVAILABLE TRONG UI THỦ CÔNG - ĐÃ CÓ TEST GATE 1 TƯƠNG ỨNG):\n" +
                    "• Target / Caster chết khi đạn đang bay: Kiểm chứng bởi Automated Test T04 & T05 (Hủy đạn không gây sát thương thừa).\n" +
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
            if (EditorApplication.isPlaying)
            {
                Repaint();
            }
        }
    }

    /// <summary>
    /// Bootstrap entry point invoked when launcher starts Unity for manual observation under Save Guard protection.
    /// </summary>
    [InitializeOnLoad]
    public static class P09ManualSessionBootstrap
    {
        static P09ManualSessionBootstrap()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.delayCall += () =>
                {
                    P09ManualTestWindow.ShowWindow();
                    P09ManualObservationController.SetupOrResetFixture();
                };
            }
        }

        /// <summary>
        /// Entry point invoked by launch_manual_p09a_session.ps1 after Save Guard backup is secured.
        /// </summary>
        public static void LaunchFromSaveGuard()
        {
            Debug.Log("[P09 BOOTSTRAP] LaunchFromSaveGuard invoked by Save Guard launcher.");
            SessionState.SetBool("P09_Manual_Session_Authorized", true);

            // Open control window first so user immediately sees it
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
}
