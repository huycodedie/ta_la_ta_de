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
using WuxiaGame.Equipment;
using WuxiaGame.Inventory;
using WuxiaGame.Progression;
using WuxiaGame.Stats;

namespace WuxiaGame.Editor
{
    /// <summary>
    /// P09-B Manual Observation Controller (Static Management Core).
    /// Manages transient RAM scene fixture, arena visualization (Game View 3D debug primitives),
    /// combatants (Hero Cyan, Target A Red, Target B Orange), dash skill execution,
    /// target positions, CC arming helpers, UI pause/resume, and telemetry tracking.
    /// Operates strictly with natural frames (Time.timeScale = 1).
    /// </summary>
    public static class P09BManualObservationController
    {
        private static GameObject _fixtureRoot;

        public static Camera CameraRef;
        public static Hero HeroRef;
        public static Monster TargetARef;
        public static Monster TargetBRef;
        public static Monster CurrentTargetRef;
        public static BattleManager BattleManagerRef;
        public static MindMethodManager MindMethodManagerRef;

        public static SkillDefinitionSO DashSkill;
        private static MindMethodDefinitionSO _tempDef;
        private static MindMethodDatabaseSO _tempDb;
        private static MonsterConfigSO _mCfgA;
        private static CombatConfigSO _cCfgA;
        private static MonsterConfigSO _mCfgB;
        private static CombatConfigSO _cCfgB;
        private static HeroConfigSO _heroCfg;

        public static string LastActionLog = "Sẵn sàng (Chưa bấm)";
        public static int ResetCount = 0;
        public static int CommandSequenceId = 0;

        public static int TotalDashCommands = 0;
        public static int TotalDashStarts = 0;
        public static int TotalDashCompletes = 0;
        public static int TotalDashAborts = 0;

        // Armed helpers for repeatable mid-dash triggers on natural frames
        public static bool ArmPauseOnNextTick = false;
        public static bool ArmStunOnNextTick = false;
        public static bool ArmRootOnNextTick = false;
        public static bool ArmFreezeOnNextTick = false;

        private static GameObject _markerA;
        private static GameObject _markerB;
        private static readonly List<Material> _createdMaterials = new List<Material>();

        private static int _resetGeneration = 0;
        public static int ResetGeneration => _resetGeneration;

        private static int _fixtureGeneration = 0;
        public static int FixtureGeneration => _fixtureGeneration;

        public static bool IsInitializing { get; private set; } = false;
        private static bool _isResetPending = false;
        public static bool IsResetPending => _isResetPending;

        private static bool _isStabilizing = false;
        public static bool IsStabilizing => _isStabilizing;

        private static bool _isLifecycleReady = false;
        public static bool IsLifecycleReady => _isLifecycleReady;

        private static int _stabilizationFramesRemaining = 0;
        private static EditorApplication.CallbackFunction _scheduledResetDelegate = null;

        /// <summary>
        /// Fixture lifecycle validity (G2):
        /// Requires:
        /// 1) Not initializing (synchronous build finished)
        /// 2) Not pending reset (no pending delayed callback)
        /// 3) Not stabilizing (natural frame lifecycle barrier settled)
        /// 4) Lifecycle ready flag set
        /// 5) All core game objects and components alive and valid
        /// 6) BattleManager active OR paused by UI (F-GUI-PAUSE)
        /// Fails closed if any reference is destroyed or missing.
        /// </summary>
        public static bool IsFixtureReady =>
            !IsInitializing &&
            !_isResetPending &&
            !_isStabilizing &&
            _isLifecycleReady &&
            _fixtureRoot != null &&
            HeroRef != null &&
            TargetARef != null &&
            TargetBRef != null &&
            BattleManagerRef != null &&
            (BattleManagerRef.IsBattleActive || BattleManagerRef.IsCombatPausedByUI) &&
            DashSkill != null &&
            CameraRef != null;

        /// <summary>
        /// Schedules fixture teardown and re-creation outside IMGUI passes (G1).
        /// Real cancellation, duplicate coalescing, generation token, named delegate.
        /// </summary>
        public static void ScheduleResetFixture()
        {
            if (_isResetPending || IsInitializing)
            {
                Debug.Log($"[P09-B HARNESS] Coalescing duplicate ScheduleResetFixture request (Pending={_isResetPending}, Initializing={IsInitializing}).");
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("[P09-B HARNESS] ScheduleResetFixture rejected: Editor is not in Play Mode.");
                return;
            }

            _isResetPending = true;
            int currentGen = ++_resetGeneration;
            LastActionLog = $"Đang lên lịch reset fixture an toàn (Gen={currentGen})...";

            // Unregister any previous delegate reference
            if (_scheduledResetDelegate != null)
            {
                EditorApplication.delayCall -= _scheduledResetDelegate;
                _scheduledResetDelegate = null;
            }

            _scheduledResetDelegate = () => OnScheduledResetDispatched(currentGen);
            EditorApplication.delayCall += _scheduledResetDelegate;
        }

        public static void CancelPendingReset()
        {
            _isResetPending = false;
            _resetGeneration++; // Invalidate any in-flight callback token immediately
            if (_scheduledResetDelegate != null)
            {
                EditorApplication.delayCall -= _scheduledResetDelegate;
                _scheduledResetDelegate = null;
            }
        }

        private static void OnScheduledResetDispatched(int requestGen)
        {
            _scheduledResetDelegate = null;

            // 1. Generation token match
            if (requestGen != _resetGeneration)
            {
                Debug.Log($"[P09-B HARNESS] Stale reset callback ignored: RequestGen={requestGen}, CurrentGen={_resetGeneration}");
                return;
            }

            // 2. Pending flag match
            if (!_isResetPending)
            {
                Debug.Log($"[P09-B HARNESS] Cancelled reset callback ignored: RequestGen={requestGen}");
                return;
            }

            _isResetPending = false;

            // 3. Play Mode check: Never execute in Edit Mode!
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning($"[P09-B HARNESS] Reset callback aborted: Editor is not in Play Mode (isPlaying={EditorApplication.isPlaying}).");
                return;
            }

            // 4. Session authorization check
            if (!P09BManualSessionBootstrap.IsSessionAuthorized())
            {
                Debug.LogError("[P09-B HARNESS] Reset callback aborted: Session not authorized under Save Guard.");
                return;
            }

            // 5. Execute Setup
            SetupOrResetFixture();

            if (P09BManualTestWindow.Instance != null)
            {
                P09BManualTestWindow.Instance.Repaint();
            }
        }

        public static void ResetTelemetry()
        {
            TotalDashCommands = 0;
            TotalDashStarts = 0;
            TotalDashCompletes = 0;
            TotalDashAborts = 0;
            ArmPauseOnNextTick = false;
            ArmStunOnNextTick = false;
            ArmRootOnNextTick = false;
            ArmFreezeOnNextTick = false;
            LastActionLog = "Telemetry đã reset về 0.";
            Debug.Log("[P09-B HARNESS] Telemetry reset completed.");
        }

        private static void HandleDashStarted(long txId)
        {
            TotalDashStarts++;
            float startX = HeroRef != null ? HeroRef.transform.position.x : 0f;
            string targetName = CurrentTargetRef != null ? CurrentTargetRef.name : "null";
            LastActionLog = $"[DASH STARTED #{txId}] StartX={startX:F3}, Target={targetName}, TotalStarts={TotalDashStarts}";
            Debug.Log($"[P09-B HARNESS TELEMETRY] Dash Started: TxId={txId}, StartX={startX:F3}, TotalStarts={TotalDashStarts}");
        }

        private static void HandleDashCompleted(long txId)
        {
            TotalDashCompletes++;
            float endX = HeroRef != null ? HeroRef.transform.position.x : 0f;
            float distToTarget = CurrentTargetRef != null ? Mathf.Abs(CurrentTargetRef.transform.position.x - endX) : 0f;
            if (HeroRef != null)
            {
                HeroRef.SetCurrentTarget(null);
            }
            LastActionLog = $"[DASH COMPLETED #{txId}] EndX={endX:F3}, DistToTarget={distToTarget:F2}m, TotalCompletes={TotalDashCompletes}";
            Debug.Log($"[P09-B HARNESS TELEMETRY] Dash Completed: TxId={txId}, EndX={endX:F3}, DistToTarget={distToTarget:F3}m, TotalCompletes={TotalDashCompletes}");
        }

        private static void HandleDashAborted(long txId)
        {
            TotalDashAborts++;
            float abortX = HeroRef != null ? HeroRef.transform.position.x : 0f;
            if (HeroRef != null)
            {
                HeroRef.SetCurrentTarget(null);
            }
            LastActionLog = $"[DASH ABORTED #{txId}] AbortX={abortX:F3}, TotalAborts={TotalDashAborts}";
            Debug.Log($"[P09-B HARNESS TELEMETRY] Dash Aborted: TxId={txId}, AbortX={abortX:F3}, TotalAborts={TotalDashAborts}");
        }

        public static void SetupOrResetFixture()
        {
            _isResetPending = false;
            _isLifecycleReady = false;
            _isStabilizing = false;
            int currentFixtureGen = ++_fixtureGeneration;

            if (HeroRef != null && HeroRef.Movement != null && HeroRef.Movement.IsDashing)
            {
                HeroRef.Movement.AbortDash();
            }

            IsInitializing = true;
            try
            {
                TeardownFixtureInternal();
                ResetCount++;
                ResetTelemetry();

                _fixtureRoot = new GameObject("[P09B_Manual_Observation_Fixture]");

                // 1. Camera - Positioned to view the horizontal arena in Game View
                var camGo = new GameObject("P09B_Camera", typeof(Camera));
                camGo.transform.SetParent(_fixtureRoot.transform, false);
                camGo.transform.position = new Vector3(0f, 0.5f, -9f);
                CameraRef = camGo.GetComponent<Camera>();
                CameraRef.orthographic = true;
                CameraRef.orthographicSize = 3.5f;
                CameraRef.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
                CameraRef.clearFlags = CameraClearFlags.SolidColor;

                // 2. Services
                var servicesGo = new GameObject("P09B_Services");
                servicesGo.transform.SetParent(_fixtureRoot.transform, false);
                servicesGo.AddComponent<EquipmentManager>();
                servicesGo.AddComponent<Inventory.Inventory>();
                servicesGo.AddComponent<ResourceManager>();
                var prog = servicesGo.AddComponent<ProgressionManager>();
                prog.enabled = false;

                // 3. BattleManager
                var bmGo = new GameObject("BattleManager", typeof(BattleManager));
                bmGo.transform.SetParent(_fixtureRoot.transform, false);
                BattleManagerRef = bmGo.GetComponent<BattleManager>();

                // 4. MindMethodManager
                var mmGo = new GameObject("MindMethodManager", typeof(MindMethodManager));
                mmGo.transform.SetParent(_fixtureRoot.transform, false);
                MindMethodManagerRef = mmGo.GetComponent<MindMethodManager>();
                typeof(MindMethodManager).GetField("instance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.SetValue(null, MindMethodManagerRef);

                // 5. Arena Visuals (Ground Line & Endpoint Indicators)
                CreateArenaVisuals();

                // 6. Hero (Preset: x = 0.0m, y = -0.30m, z = 0m)
                var heroGo = new GameObject("Hero_P09B");
                heroGo.transform.SetParent(_fixtureRoot.transform, false);
                heroGo.transform.position = new Vector3(0.0f, -0.30f, 0f);

                HeroRef = heroGo.AddComponent<Hero>();
                _heroCfg = ScriptableObject.CreateInstance<HeroConfigSO>();
                typeof(HeroConfigSO).GetField("movementSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_heroCfg, 0f);
                typeof(HeroConfigSO).GetField("attackRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_heroCfg, 1.5f);
                typeof(HeroConfigSO).GetField("maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_heroCfg, 1000f);
                typeof(HeroConfigSO).GetField("maxRage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_heroCfg, 100f);
                typeof(HeroConfigSO).GetField("initialRage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(_heroCfg, 100f);
                HeroRef.SetHeroConfig(_heroCfg);
                HeroRef.InitializeHero();
                HeroRef.Health.InitializeHealth(1000f, HeroRef);
                HeroRef.Rage.InitializeRage(100f, 100f, HeroRef);
                typeof(Hero).GetField("attackRange", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(HeroRef, 1.5f);

                // Suppress auto-pursuit without disabling MovementComponent: base moveSpeed = 0
                HeroRef.Stats.SetBaseValue(StatType.MoveSpeed, 0f);
                HeroRef.Movement.InitializeSpeed(0f, HeroRef);
                HeroRef.Stats.SetBaseValue(StatType.Attack, 100f);

                var heroAi = heroGo.GetComponent<HeroSkillDecisionController>();
                if (heroAi != null) heroAi.enabled = false;
                if (HeroRef.Attack != null) HeroRef.Attack.SetAttackEnabled(false);

                CreateVisualEntity(heroGo, PrimitiveType.Capsule, new Vector3(0.5f, 0.9f, 0.5f), Color.cyan, "Hero_Cyan");

                // 7. Target A (Preset: x = 4.5m, y = -0.30m, z = 0m)
                var targetAGO = new GameObject("Monster_Target_A");
                targetAGO.transform.SetParent(_fixtureRoot.transform, false);
                targetAGO.transform.position = new Vector3(4.5f, -0.30f, 0f);
                TargetARef = targetAGO.AddComponent<Monster>();
                _mCfgA = ScriptableObject.CreateInstance<MonsterConfigSO>();
                _mCfgA.InitializeMonsterConfig("Target A (Mặt trước x=4.5)", 2000f, 10f, 0f, 0f, 1f, 1f, 0);
                _cCfgA = ScriptableObject.CreateInstance<CombatConfigSO>();
                TargetARef.InitializeMonster(_mCfgA, _cCfgA);
                if (TargetARef.Attack != null) TargetARef.Attack.SetAttackEnabled(false);
                CreateVisualEntity(targetAGO, PrimitiveType.Sphere, new Vector3(0.7f, 0.7f, 0.7f), new Color(0.9f, 0.2f, 0.2f), "TargetA_Red");

                // 8. Target B (Preset: x = -4.5m, y = -0.30m, z = 0m)
                var targetBGO = new GameObject("Monster_Target_B");
                targetBGO.transform.SetParent(_fixtureRoot.transform, false);
                targetBGO.transform.position = new Vector3(-4.5f, -0.30f, 0f);
                TargetBRef = targetBGO.AddComponent<Monster>();
                _mCfgB = ScriptableObject.CreateInstance<MonsterConfigSO>();
                _mCfgB.InitializeMonsterConfig("Target B (Mặt sau x=-4.5)", 2000f, 10f, 0f, 0f, 1f, 1f, 0);
                _cCfgB = ScriptableObject.CreateInstance<CombatConfigSO>();
                TargetBRef.InitializeMonster(_mCfgB, _cCfgB);
                if (TargetBRef.Attack != null) TargetBRef.Attack.SetAttackEnabled(false);
                CreateVisualEntity(targetBGO, PrimitiveType.Sphere, new Vector3(0.7f, 0.7f, 0.7f), new Color(1.0f, 0.55f, 0.1f), "TargetB_Orange");

                // 9. BattleManager Registration
                BattleManagerRef.RegisterHero(HeroRef);
                BattleManagerRef.RegisterMonster(TargetARef);
                BattleManagerRef.RegisterMonster(TargetBRef);
                BattleManagerRef.SetAutoBattle(false);
                BattleManagerRef.StartBattle();
                BattleManagerRef.SetAutoBattle(false);
                BattleManagerRef.enabled = false; // Isolate automatic wave cycling

                // Set Current Target reference (Hero disengaged while idle)
                CurrentTargetRef = TargetARef;
                HeroRef.SetCurrentTarget(null);

                // Re-apply isolation after StartBattle
                ReapplyAutonomyIsolation();

                // 10. Create Test Dash Skill SO (Preset: Distance=4.0, Speed=12.0, Rage=20, CD=5, isManualOnly=true)
                DashSkill = ScriptableObject.CreateInstance<SkillDefinitionSO>();
                DashSkill.InitializeSkill(
                    id: "skill_dash_manual_p09b",
                    mmId: "mm_dash_test",
                    slot: SkillSlotType.Skill,
                    name: "Tật Phong Tuyệt Kỹ (Dash Test)",
                    desc: "Dash TowardTarget test",
                    conditions: null,
                    dmgMultiplier: 0f,
                    costRage: 20f,
                    cd: 5f,
                    passive: false,
                    skillEffects: null,
                    shatterFreeze: false,
                    skillCastTime: 0f,
                    channel: false,
                    chDuration: 0f,
                    chTickInterval: 0f,
                    skillPriority: 50,
                    projectile: false,
                    projSpeed: 0f,
                    projLifetime: 0f
                );
                DashSkill.ConfigureDash(true, 4.0f, 12.0f, true); // Strict isManualOnly = true

                // Setup MindMethodManager
                _tempDef = ScriptableObject.CreateInstance<MindMethodDefinitionSO>();
                _tempDef.InitializeMindMethod("mm_dash_test", "Thái Hư Bộ", "Dash Skill", 10, true, new MindMethodPassiveData());
                var defSkills = typeof(MindMethodDefinitionSO).GetField("skills", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (defSkills != null)
                {
                    var list = new List<SkillDefinitionSO> { DashSkill };
                    defSkills.SetValue(_tempDef, list);
                }

                _tempDb = ScriptableObject.CreateInstance<MindMethodDatabaseSO>();
                _tempDb.SetMindMethods(new List<MindMethodDefinitionSO> { _tempDef });
                MindMethodManagerRef.SetDatabase(_tempDb);
                MindMethodManagerRef.SetActiveMindMethod("mm_dash_test");

                var activeState = MindMethodManagerRef.ActiveMindMethodState;
                if (activeState != null)
                {
                    activeState.SkillStates[DashSkill.SkillId] = new SkillRuntimeState(DashSkill.SkillId, true, 1);
                    activeState.SelectedSkillPerSlot[SkillSlotType.Skill] = DashSkill.SkillId;
                }

                // Subscribe transaction-aware events
                if (HeroRef != null && HeroRef.Movement != null)
                {
                    HeroRef.Movement.OnDashStarted += HandleDashStarted;
                    HeroRef.Movement.OnDashCompleted += HandleDashCompleted;
                    HeroRef.Movement.OnDashAborted += HandleDashAborted;
                }

                // In-Scene Harness Updater for Natural Frame Arming and Telemetry
                var updaterGo = new GameObject("P09B_Harness_Updater");
                updaterGo.transform.SetParent(_fixtureRoot.transform, false);
                updaterGo.AddComponent<P09BHarnessUpdater>();

                _isStabilizing = true;
                _stabilizationFramesRemaining = 2; // Natural frame lifecycle barrier for Start() and layout to settle

                LastActionLog = $"Fixture đã khởi tạo (Gen={currentFixtureGen}). Đang ổn định lifecycle...";
                Debug.Log($"[P09-B HARNESS] Fixture setup complete (Gen={currentFixtureGen}, ResetCount={ResetCount}). Stabilizing lifecycle...");
            }
            catch (Exception ex)
            {
                _isLifecycleReady = false;
                _isStabilizing = false;
                Debug.LogError($"[P09-B HARNESS ERROR] SetupOrResetFixture failed with exception: {ex}");
                TeardownFixtureInternal();
                throw;
            }
            finally
            {
                IsInitializing = false;
            }
        }

        public static void TeardownFixture()
        {
            CancelPendingReset();
            TeardownFixtureInternal();
        }

        private static void TeardownFixtureInternal()
        {
            _isLifecycleReady = false;
            _isStabilizing = false;
            _fixtureGeneration++;

            if (HeroRef != null && HeroRef.Movement != null && HeroRef.Movement.IsDashing)
            {
                HeroRef.Movement.AbortDash();
            }

            if (HeroRef != null && HeroRef.Movement != null)
            {
                HeroRef.Movement.OnDashStarted -= HandleDashStarted;
                HeroRef.Movement.OnDashCompleted -= HandleDashCompleted;
                HeroRef.Movement.OnDashAborted -= HandleDashAborted;
            }

            if (_createdMaterials.Count > 0)
            {
                for (int i = 0; i < _createdMaterials.Count; i++)
                {
                    if (_createdMaterials[i] != null) UnityEngine.Object.DestroyImmediate(_createdMaterials[i]);
                }
                _createdMaterials.Clear();
            }

            if (_fixtureRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(_fixtureRoot);
                _fixtureRoot = null;
            }

            if (DashSkill != null) UnityEngine.Object.DestroyImmediate(DashSkill);
            if (_tempDef != null) UnityEngine.Object.DestroyImmediate(_tempDef);
            if (_tempDb != null) UnityEngine.Object.DestroyImmediate(_tempDb);
            if (_mCfgA != null) UnityEngine.Object.DestroyImmediate(_mCfgA);
            if (_cCfgA != null) UnityEngine.Object.DestroyImmediate(_cCfgA);
            if (_mCfgB != null) UnityEngine.Object.DestroyImmediate(_mCfgB);
            if (_cCfgB != null) UnityEngine.Object.DestroyImmediate(_cCfgB);
            if (_heroCfg != null) { UnityEngine.Object.DestroyImmediate(_heroCfg); _heroCfg = null; }

            HeroRef = null;
            TargetARef = null;
            TargetBRef = null;
            CurrentTargetRef = null;
            CameraRef = null;
            BattleManagerRef = null;
            MindMethodManagerRef = null;
            _markerA = null;
            _markerB = null;
        }

        public static void ReapplyAutonomyIsolation()
        {
            if (HeroRef != null)
            {
                HeroRef.Stats.SetBaseValue(StatType.MoveSpeed, 0f);
                if (HeroRef.Movement != null) HeroRef.Movement.InitializeSpeed(0f, HeroRef);
                if (HeroRef.Attack != null) HeroRef.Attack.SetAttackEnabled(false);
                var ai = HeroRef.GetComponent<HeroSkillDecisionController>();
                if (ai != null) ai.enabled = false;
            }
            if (TargetARef != null && TargetARef.Attack != null) TargetARef.Attack.SetAttackEnabled(false);
            if (TargetBRef != null && TargetBRef.Attack != null) TargetBRef.Attack.SetAttackEnabled(false);
        }

        private static void CreateArenaVisuals()
        {
            // Ground Plane Indicator at Y = -0.30m
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Visual_GroundLine";
            ground.transform.SetParent(_fixtureRoot.transform, false);
            ground.transform.position = new Vector3(0f, -0.35f, 0f);
            ground.transform.localScale = new Vector3(16f, 0.05f, 1f);
            var col = ground.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);
            SetPrimitiveColor(ground, new Color(0.28f, 0.32f, 0.40f));

            // Endpoint Marker for Target A (dynamically positioned by UpdateStopMarkers)
            var markerA = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            markerA.name = "Visual_StopMarker_A";
            markerA.transform.SetParent(_fixtureRoot.transform, false);
            markerA.transform.position = new Vector3(3.0f, -0.30f, 0f);
            markerA.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
            var colA = markerA.GetComponent<Collider>();
            if (colA != null) UnityEngine.Object.DestroyImmediate(colA);
            SetPrimitiveColor(markerA, Color.yellow);
            _markerA = markerA;

            // Endpoint Marker for Target B (dynamically positioned by UpdateStopMarkers)
            var markerB = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            markerB.name = "Visual_StopMarker_B";
            markerB.transform.SetParent(_fixtureRoot.transform, false);
            markerB.transform.position = new Vector3(-3.0f, -0.30f, 0f);
            markerB.transform.localScale = new Vector3(0.15f, 0.4f, 0.15f);
            var colB = markerB.GetComponent<Collider>();
            if (colB != null) UnityEngine.Object.DestroyImmediate(colB);
            SetPrimitiveColor(markerB, Color.yellow);
            _markerB = markerB;
        }

        public static void UpdateStopMarkers()
        {
            if (HeroRef == null || _markerA == null || _markerB == null) return;

            // When Dash is active, lock the active target marker to the transaction's bound endpoint
            if (HeroRef.Movement != null && HeroRef.Movement.IsDashing && HeroRef.Movement.ActiveDashPlan != null)
            {
                var plan = HeroRef.Movement.ActiveDashPlan;
                float boundEndX = plan.PlannedEndpointX;
                if (plan.BoundTarget == TargetARef)
                {
                    _markerA.transform.position = new Vector3(boundEndX, -0.30f, 0f);
                    if (TargetBRef != null)
                    {
                        float dir = Mathf.Sign(TargetBRef.transform.position.x - plan.PlannedStartPosition.x);
                        float dist = Mathf.Max(0f, Mathf.Abs(TargetBRef.transform.position.x - plan.PlannedStartPosition.x) - HeroRef.AttackRange);
                        float clamped = Mathf.Min(dist, 4.0f);
                        _markerB.transform.position = new Vector3(plan.PlannedStartPosition.x + dir * clamped, -0.30f, 0f);
                    }
                }
                else if (plan.BoundTarget == TargetBRef)
                {
                    _markerB.transform.position = new Vector3(boundEndX, -0.30f, 0f);
                    if (TargetARef != null)
                    {
                        float dir = Mathf.Sign(TargetARef.transform.position.x - plan.PlannedStartPosition.x);
                        float dist = Mathf.Max(0f, Mathf.Abs(TargetARef.transform.position.x - plan.PlannedStartPosition.x) - HeroRef.AttackRange);
                        float clamped = Mathf.Min(dist, 4.0f);
                        _markerA.transform.position = new Vector3(plan.PlannedStartPosition.x + dir * clamped, -0.30f, 0f);
                    }
                }
                return;
            }

            // Idle state: preview endpoint computed from current Hero position and targets
            float heroX = HeroRef.transform.position.x;
            float attackRange = HeroRef.AttackRange;

            // Marker A
            if (TargetARef != null)
            {
                float targetX = TargetARef.transform.position.x;
                float dir = Mathf.Sign(targetX - heroX);
                float dist = Mathf.Max(0f, Mathf.Abs(targetX - heroX) - attackRange);
                float clamped = Mathf.Min(dist, 4.0f);
                float stopX = heroX + dir * clamped;
                _markerA.transform.position = new Vector3(stopX, -0.30f, 0f);
            }

            // Marker B
            if (TargetBRef != null)
            {
                float targetX = TargetBRef.transform.position.x;
                float dir = Mathf.Sign(targetX - heroX);
                float dist = Mathf.Max(0f, Mathf.Abs(targetX - heroX) - attackRange);
                float clamped = Mathf.Min(dist, 4.0f);
                float stopX = heroX + dir * clamped;
                _markerB.transform.position = new Vector3(stopX, -0.30f, 0f);
            }
        }

        private static void CreateVisualEntity(GameObject parent, PrimitiveType type, Vector3 scale, Color color, string name)
        {
            var visual = GameObject.CreatePrimitive(type);
            visual.name = name;
            visual.transform.SetParent(parent.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = scale;
            var col = visual.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.DestroyImmediate(col);
            SetPrimitiveColor(visual, color);
        }

        private static void SetPrimitiveColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color"));
                mat.color = color;
                renderer.material = mat;
                _createdMaterials.Add(mat);
            }
        }

        public static void SwitchTarget(Monster newTarget)
        {
            if (newTarget == null || HeroRef == null) return;
            CurrentTargetRef = newTarget;
            LastActionLog = $"Đã chuyển mục tiêu sang: {newTarget.name} tại x={newTarget.transform.position.x:F2}";
            Debug.Log($"[P09-B HARNESS] Target switched to {newTarget.name} (x={newTarget.transform.position.x:F2}).");
        }

        public static bool ResetEntityPositions()
        {
            if (HeroRef != null && HeroRef.IsDashing)
            {
                Debug.LogWarning("[P09-B HARNESS] Cannot reset positions while Dash is active.");
                return false;
            }
            if (HeroRef != null) HeroRef.transform.position = new Vector3(0.0f, -0.30f, 0f);
            if (TargetARef != null) TargetARef.transform.position = new Vector3(4.5f, -0.30f, 0f);
            if (TargetBRef != null) TargetBRef.transform.position = new Vector3(-4.5f, -0.30f, 0f);
            LastActionLog = "Đã đặt lại vị trí chuẩn: Hero=(0, -0.30), A=(4.5, -0.30), B=(-4.5, -0.30).";
            Debug.Log("[P09-B HARNESS] Standard positions restored.");
            return true;
        }

        public static bool RefillRage()
        {
            if (HeroRef != null && HeroRef.IsDashing)
            {
                Debug.LogWarning("[P09-B HARNESS] Cannot refill rage while Dash is active.");
                return false;
            }
            if (HeroRef == null || HeroRef.Rage == null) return false;
            HeroRef.Rage.ResetRage(100f);
            LastActionLog = "Đã hồi đầy Nộ = 100.";
            Debug.Log("[P09-B HARNESS] Rage refilled to 100.");
            return true;
        }

        public static bool DrainRage()
        {
            if (HeroRef != null && HeroRef.IsDashing)
            {
                Debug.LogWarning("[P09-B HARNESS] Cannot drain rage while Dash is active.");
                return false;
            }
            if (HeroRef == null || HeroRef.Rage == null) return false;
            HeroRef.Rage.ResetRage(0f);
            LastActionLog = "Đã xả hết Nộ = 0 (Thử từ chối do thiếu Nộ).";
            Debug.Log("[P09-B HARNESS] Rage drained to 0.");
            return true;
        }

        public static bool ResetCooldown()
        {
            if (HeroRef != null && HeroRef.IsDashing)
            {
                Debug.LogWarning("[P09-B HARNESS] Cannot reset cooldown while Dash is active.");
                return false;
            }
            CooldownManager.ResetAllCooldowns();
            LastActionLog = "Đã xóa toàn bộ hồi chiêu (Reset Cooldown).";
            Debug.Log("[P09-B HARNESS] Cooldowns reset.");
            return true;
        }

        public static bool SetTargetAPresetNear()
        {
            if (HeroRef != null && HeroRef.IsDashing)
            {
                Debug.LogWarning("[P09-B HARNESS] Cannot change target preset while Dash is active.");
                return false;
            }
            if (TargetARef != null)
            {
                TargetARef.transform.position = new Vector3(1.5f, -0.30f, 0f);
                LastActionLog = "Target A đặt tại x=1.5m (Bằng đúng tầm đánh 1.5m, khoảng cách hữu ích = 0m).";
                Debug.Log("[P09-B HARNESS] Target A preset set to Near (x=1.5).");
                return true;
            }
            return false;
        }

        public static bool SetTargetAPresetFar()
        {
            if (HeroRef != null && HeroRef.IsDashing)
            {
                Debug.LogWarning("[P09-B HARNESS] Cannot change target preset while Dash is active.");
                return false;
            }
            if (TargetARef != null)
            {
                TargetARef.transform.position = new Vector3(6.0f, -0.30f, 0f);
                LastActionLog = "Target A đặt tại x=6.0m (Khoảng cách hữu ích 4.5m > Max Travel 4m, Hero sẽ dừng tại x=4.0m).";
                Debug.Log("[P09-B HARNESS] Target A preset set to Far (x=6.0).");
                return true;
            }
            return false;
        }

        public static void ExecuteManualDash()
        {
            if (!IsFixtureReady) return;
            CommandSequenceId++;
            TotalDashCommands++;

            float rageBefore = HeroRef.Rage != null ? HeroRef.Rage.CurrentRage : 0f;
            float startX = HeroRef.transform.position.x;
            string targetName = CurrentTargetRef != null ? CurrentTargetRef.name : "null";

            Debug.Log($"[CMD #{CommandSequenceId}] Request Dash -> Target={targetName}, HeroX={startX:F3}, Rage={rageBefore:F1}, Frame={Time.frameCount}, Time={Time.time:F3}");

            // Bind current target for execution
            if (HeroRef != null && CurrentTargetRef != null)
            {
                HeroRef.SetCurrentTarget(CurrentTargetRef);
            }

            var res = HeroRef.ExecuteSelectedSkill(SkillSlotType.Skill, CurrentTargetRef);
            if (res != null && res.Success)
            {
                LastActionLog = $"[CMD #{CommandSequenceId}] DASH REQUEST SUCCESS: StartX={startX:F2}, Target={targetName}, Cooldown={res.CooldownRemaining:F1}s";
                Debug.Log($"[CMD #{CommandSequenceId}] Request Accepted (Dash Pending/Started).");
            }
            else
            {
                if (HeroRef != null && !HeroRef.IsDashing)
                {
                    HeroRef.SetCurrentTarget(null);
                }
                string reason = res != null ? res.FailureReason.ToString() : "NullResult";
                string message = res != null ? res.ReasonDescription : "Unknown";
                LastActionLog = $"[CMD #{CommandSequenceId}] DASH REJECTED: Reason={reason}, Msg={message}";
                Debug.LogWarning($"[CMD #{CommandSequenceId}] Result: REJECTED (Reason={reason}, Msg={message})");
            }
        }

        public static void TogglePause()
        {
            if (BattleManagerRef == null) return;
            if (BattleManagerRef.IsCombatPausedByUI)
            {
                BattleManagerRef.ResumeCombat();
                ReapplyAutonomyIsolation();
                LastActionLog = $"[UI RESUME] Frame={Time.frameCount}, Time={Time.time:F3}. Đã tiếp tục combat.";
                Debug.Log($"[P09-B HARNESS] Combat Resumed at Frame={Time.frameCount}, Time={Time.time:F3}.");
            }
            else
            {
                BattleManagerRef.PauseCombat();
                LastActionLog = $"[UI PAUSE] Frame={Time.frameCount}, Time={Time.time:F3}. Combat bị đóng băng.";
                Debug.Log($"[P09-B HARNESS] Combat Paused at Frame={Time.frameCount}, Time={Time.time:F3}.");
            }
        }

        public static void ApplyCC(CrowdControlType type, float duration)
        {
            if (HeroRef == null) return;
            HeroRef.StatusController.ApplyCrowdControl($"manual_{type}", type, duration);
            LastActionLog = $"[CC APPLIED] {type} trong {duration:F1}s lên Hero tại Frame={Time.frameCount}.";
            Debug.Log($"[P09-B HARNESS] Applied {type} ({duration:F1}s) to Hero at Frame={Time.frameCount}, Time={Time.time:F3}.");
        }

        public static void ClearAllCC()
        {
            if (HeroRef == null) return;
            HeroRef.StatusController.ClearAllCrowdControl();
            LastActionLog = "Đã xóa toàn bộ hiệu ứng khống chế trên Hero.";
            Debug.Log($"[P09-B HARNESS] Cleared all CC from Hero at Frame={Time.frameCount}.");
        }

        public static void OnHarnessUpdate()
        {
            if (_isStabilizing)
            {
                _stabilizationFramesRemaining--;
                if (_stabilizationFramesRemaining <= 0)
                {
                    _isStabilizing = false;
                    _isLifecycleReady = true;
                    LastActionLog = "Fixture đã ổn định lifecycle hoàn tất. Sẵn sàng quan sát!";
                    Debug.Log($"[P09-B HARNESS] Fixture lifecycle stabilized (Gen={_fixtureGeneration}). READY.");
                }
                return;
            }

            if (!IsFixtureReady) return;

            // Enforce idle isolation within tooling scope: suppress hero autonomous creep if stat recalculation ran
            if (HeroRef != null && !HeroRef.IsDashing)
            {
                if (HeroRef.Movement != null && HeroRef.Movement.CurrentMoveSpeed > 0f)
                {
                    HeroRef.Movement.InitializeSpeed(0f, HeroRef);
                }
                if (HeroRef.CurrentTarget != null)
                {
                    HeroRef.SetCurrentTarget(null);
                }
            }

            UpdateStopMarkers();

            // Detect transition: Started Dashing -> Intermediate Frame Check for Armed Actions
            if (HeroRef.IsDashing)
            {
                if (ArmPauseOnNextTick)
                {
                    ArmPauseOnNextTick = false;
                    TogglePause();
                    Debug.Log($"[P09-B ARMED] Triggered Armed Pause during active dash at Frame={Time.frameCount}, HeroX={HeroRef.transform.position.x:F3}.");
                }
                else if (ArmStunOnNextTick)
                {
                    ArmStunOnNextTick = false;
                    ApplyCC(CrowdControlType.Stun, 2.0f);
                    Debug.Log($"[P09-B ARMED] Triggered Armed Stun during active dash at Frame={Time.frameCount}, HeroX={HeroRef.transform.position.x:F3}.");
                }
                else if (ArmRootOnNextTick)
                {
                    ArmRootOnNextTick = false;
                    ApplyCC(CrowdControlType.Root, 2.0f);
                    Debug.Log($"[P09-B ARMED] Triggered Armed Root during active dash at Frame={Time.frameCount}, HeroX={HeroRef.transform.position.x:F3}.");
                }
                else if (ArmFreezeOnNextTick)
                {
                    ArmFreezeOnNextTick = false;
                    ApplyCC(CrowdControlType.Freeze, 2.0f);
                    Debug.Log($"[P09-B ARMED] Triggered Armed Freeze during active dash at Frame={Time.frameCount}, HeroX={HeroRef.transform.position.x:F3}.");
                }
            }
        }
    }

    /// <summary>
    /// Lightweight MonoBehaviour runner placed inside fixture scene to drive natural frame updates and armed helpers.
    /// </summary>
    public class P09BHarnessUpdater : MonoBehaviour
    {
        private void Update()
        {
            P09BManualObservationController.OnHarnessUpdate();
        }
    }

    /// <summary>
    /// Interactive EditorWindow control panel for manual observation of P09-B TowardTarget Dash.
    /// Strictly blocked from execution outside Save Guard.
    /// </summary>
    public class P09BManualTestWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        public static P09BManualTestWindow Instance { get; private set; }

        [MenuItem("Tools/Wuxia RPG/P09-B Dash Manual Observation Window")]
        public static void ShowWindow()
        {
            var win = GetWindow<P09BManualTestWindow>("P09-B Dash Observation");
            win.minSize = new Vector2(500, 800);
            win.Show();
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
            P09BManualObservationController.CancelPendingReset();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
            P09BManualObservationController.CancelPendingReset();
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            try
            {
                bool isAuthorized = P09BManualSessionBootstrap.IsSessionAuthorized();

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("P09-B TOWARDTARGET DASH MANUAL OBSERVATION", EditorStyles.boldLabel);

                if (!isAuthorized)
                {
                    EditorGUILayout.HelpBox(
                        "PHIÊN CHƯA ĐƯỢC CẤP PHÉP (UNAUTHORIZED SESSION)\n\n" +
                        "Cửa sổ này bị KHÓA hoàn toàn để bảo vệ workspace và tránh làm bẩn PlayerPrefs khi chạy Editor bình thường.\n\n" +
                        "Để mở phiên quan sát an toàn với đầy đủ RAM fixture và Save Guard bảo vệ, vui lòng thực thi lệnh PowerShell:\n\n" +
                        "  powershell -ExecutionPolicy Bypass -File Tools\\Verification\\P09B\\launch_manual_p09b_session.ps1\n\n" +
                        "Mọi thao tác can thiệp trực tiếp từ Editor thường đều bị từ chối.",
                        MessageType.Error);
                    return;
                }

                EditorGUILayout.HelpBox(
                    "SAVE GUARD PROTECTED SESSION (ĐÃ CẤP PHÉP)\n" +
                    "Phiên chạy được bảo vệ bởi launch_manual_p09b_session.ps1. Toàn bộ PlayerPrefs đã được backup an toàn. Khi đóng Editor, hệ thống sẽ tự động đối chiếu và phục hồi nguyên vẹn.",
                    MessageType.Info);

                EditorGUILayout.Space(8);

                if (!EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("Phiên cần ở chế độ PLAY MODE để quan sát khung hình chuyển động tự nhiên của Dash.", MessageType.Warning);
                    return;
                }

                // F-GUI-RESET & G2: When reset is pending, initializing, or stabilizing, show clean status and do NOT access stale entity references
                if (P09BManualObservationController.IsInitializing || P09BManualObservationController.IsResetPending || P09BManualObservationController.IsStabilizing)
                {
                    EditorGUILayout.HelpBox("Fixture đang trong quá trình khởi tạo / ổn định lifecycle...", MessageType.Info);
                    return;
                }

                // F-GUI-PAUSE: IsFixtureReady remains true during UI Pause so control panel and Resume button stay visible
                if (!P09BManualObservationController.IsFixtureReady)
                {
                    EditorGUILayout.HelpBox("Fixture RAM chưa sẵn sàng hoặc chưa khởi tạo trong Play Mode.", MessageType.Warning);
                    if (GUILayout.Button("Khởi tạo Fixture Ngay", GUILayout.Height(36)))
                    {
                        P09BManualObservationController.ScheduleResetFixture();
                    }
                    return;
                }

                var hero = P09BManualObservationController.HeroRef;
                var target = P09BManualObservationController.CurrentTargetRef;
                var dashSkill = P09BManualObservationController.DashSkill;
                var bm = P09BManualObservationController.BattleManagerRef;

                // Defensive null guard: if any reference was invalidated, do not attempt to access properties
                if (hero == null || !hero || target == null || !target || dashSkill == null || bm == null)
                {
                    EditorGUILayout.HelpBox("Đang chờ tham chiếu Entity hoàn tất khởi tạo...", MessageType.Info);
                    return;
                }

                bool isDashing = hero.IsDashing;
                bool isPaused = bm.IsCombatPausedByUI;

                // 1. Fixture & Combat Controls
                EditorGUILayout.LabelField("1. ĐIỀU KHIỂN FIXTURE & COMBAT", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset Fixture Toàn Diện", GUILayout.Height(30)))
                {
                    // F-GUI-RESET: Defer reset outside OnGUI layout/repaint loop to prevent MissingReferenceException
                    P09BManualObservationController.ScheduleResetFixture();
                }
                GUI.enabled = !isDashing;
                if (GUILayout.Button("Đặt Lại Vị Trí Chuẩn", GUILayout.Height(30)))
                {
                    P09BManualObservationController.ResetEntityPositions();
                }
                GUI.enabled = true;

                // F-GUI-PAUSE: Resume button remains prominently visible while combat is paused
                GUI.backgroundColor = isPaused ? Color.yellow : Color.cyan;
                if (GUILayout.Button(isPaused ? "Tiếp Tục Combat (Resume)" : "Tạm Dừng Combat (UI Pause)", GUILayout.Height(30)))
                {
                    P09BManualObservationController.TogglePause();
                }
                GUI.backgroundColor = Color.white;

                bool isAtkEnabled = hero.Attack != null && hero.Attack.IsAttackEnabled;
                GUI.backgroundColor = isAtkEnabled ? Color.green : Color.white;
                if (GUILayout.Button(isAtkEnabled ? "Tắt Đánh Thường" : "Bật Đánh Thường", GUILayout.Height(30)))
                {
                    if (hero.Attack != null)
                    {
                        hero.Attack.SetAttackEnabled(!isAtkEnabled);
                        P09BManualObservationController.LastActionLog = $"Đã {(isAtkEnabled ? "TẮT" : "BẬT")} Basic Attack trên Hero.";
                    }
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(8);

                // 2. Dash Execution
                EditorGUILayout.LabelField("2. KÍCH HOẠT DASH TIẾP CẬN MỤC TIÊU", EditorStyles.boldLabel);

                float distToTarget = Mathf.Abs(target.transform.position.x - hero.transform.position.x);
                bool onCd = CooldownManager.IsOnCooldown(dashSkill.SkillId, out float cdRem);
                float currentRage = (hero.Rage != null) ? hero.Rage.CurrentRage : 0f;
                bool hasEnoughRage = currentRage >= dashSkill.RageCost;

                // F-GUI-REJECTION: Keep active transaction guard (!isDashing). When idle, allow clicking even if onCd to test production rejection!
                GUI.enabled = !isDashing;
                if (onCd)
                {
                    GUI.backgroundColor = new Color(1f, 0.75f, 0.3f); // Amber for CD rejection test
                    string btnLabel = $"GỬI LỆNH DASH (Đang CD {cdRem:F1}s - Thử CooldownNotReady)";
                    if (GUILayout.Button(btnLabel, GUILayout.Height(40)))
                    {
                        P09BManualObservationController.ExecuteManualDash();
                    }
                }
                else
                {
                    bool canDashNormal = hasEnoughRage && !isPaused;
                    GUI.backgroundColor = canDashNormal ? Color.green : Color.gray;
                    string btnLabel = $"LƯỚT TỚI MỤC TIÊU (Dist={dashSkill.DashDistance:F1}m, Spd={dashSkill.DashSpeed:F1}m/s, RageCost={dashSkill.RageCost:F0})";
                    if (GUILayout.Button(btnLabel, GUILayout.Height(40)))
                    {
                        P09BManualObservationController.ExecuteManualDash();
                    }
                }
                GUI.enabled = true;
                GUI.backgroundColor = Color.white;

                // Dedicated rejection button when on cooldown
                if (onCd)
                {
                    EditorGUILayout.Space(2);
                    GUI.enabled = !isDashing;
                    GUI.backgroundColor = new Color(1f, 0.85f, 0.4f);
                    if (GUILayout.Button($"[THỬ TỪ CHỐI CD] Gửi Lệnh Dash Khi Đang Hồi Chiêu ({cdRem:F2}s)", GUILayout.Height(28)))
                    {
                        P09BManualObservationController.ExecuteManualDash();
                    }
                    GUI.backgroundColor = Color.white;
                    GUI.enabled = true;
                }

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = !isDashing;
                if (GUILayout.Button("Hồi Đầy Nộ (Refill 100)", GUILayout.Height(26)))
                {
                    P09BManualObservationController.RefillRage();
                }
                if (GUILayout.Button("Xả Hết Nộ (Set Rage 0)", GUILayout.Height(26)))
                {
                    P09BManualObservationController.DrainRage();
                }
                if (GUILayout.Button("Xóa Hồi Chiêu (Reset CD)", GUILayout.Height(26)))
                {
                    P09BManualObservationController.ResetCooldown();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                if (onCd)
                {
                    EditorGUILayout.LabelField($"Đang trong thời gian hồi chiêu: còn lại {cdRem:F2}s", EditorStyles.miniBoldLabel);
                }

                EditorGUILayout.Space(8);

                // 3. Target Management
                EditorGUILayout.LabelField("3. CHỌN MỤC TIÊU & TÌNH HUỐNG KHOẢNG CÁCH", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = (target == P09BManualObservationController.TargetARef) ? Color.cyan : Color.white;
                if (GUILayout.Button("Chọn Target A (Trước: x=4.5m)", GUILayout.Height(28)))
                {
                    P09BManualObservationController.SwitchTarget(P09BManualObservationController.TargetARef);
                }
                GUI.backgroundColor = (target == P09BManualObservationController.TargetBRef) ? Color.cyan : Color.white;
                if (GUILayout.Button("Chọn Target B (Sau: x=-4.5m)", GUILayout.Height(28)))
                {
                    P09BManualObservationController.SwitchTarget(P09BManualObservationController.TargetBRef);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUI.enabled = !isDashing;
                if (GUILayout.Button("Target A gần x=1.5m (Zero Useful Dist)", GUILayout.Height(26)))
                {
                    P09BManualObservationController.SetTargetAPresetNear();
                }
                if (GUILayout.Button("Target A xa x=6.0m (Max Range 4m)", GUILayout.Height(26)))
                {
                    P09BManualObservationController.SetTargetAPresetFar();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(8);

                // 4. CC Injection & Repeatable Armed Helpers
                EditorGUILayout.LabelField("4. KHỐNG CHẾ & TỰ ĐỘNG TRIGGER TRÊN NATURAL FRAME", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                if (GUILayout.Button("Áp Trói Chân (Root 2s)", GUILayout.Height(26)))
                {
                    P09BManualObservationController.ApplyCC(CrowdControlType.Root, 2.0f);
                }
                if (GUILayout.Button("Áp Choáng (Stun 2s)", GUILayout.Height(26)))
                {
                    P09BManualObservationController.ApplyCC(CrowdControlType.Stun, 2.0f);
                }
                if (GUILayout.Button("Áp Đóng Băng (Freeze 2s)", GUILayout.Height(26)))
                {
                    P09BManualObservationController.ApplyCC(CrowdControlType.Freeze, 2.0f);
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Xóa Toàn Bộ CC (Clear CC)", GUILayout.Height(24)))
                {
                    P09BManualObservationController.ClearAllCC();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Arm Trigger trên frame kế tiếp sau khi bắt đầu Dash:", EditorStyles.miniBoldLabel);
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = P09BManualObservationController.ArmPauseOnNextTick ? Color.yellow : Color.white;
                if (GUILayout.Button(P09BManualObservationController.ArmPauseOnNextTick ? "[ARMED] Pause Mid-Dash" : "Arm Pause Mid-Dash", GUILayout.Height(26)))
                {
                    P09BManualObservationController.ArmPauseOnNextTick = !P09BManualObservationController.ArmPauseOnNextTick;
                }

                GUI.backgroundColor = P09BManualObservationController.ArmStunOnNextTick ? Color.yellow : Color.white;
                if (GUILayout.Button(P09BManualObservationController.ArmStunOnNextTick ? "[ARMED] Stun Mid-Dash" : "Arm Stun Mid-Dash", GUILayout.Height(26)))
                {
                    P09BManualObservationController.ArmStunOnNextTick = !P09BManualObservationController.ArmStunOnNextTick;
                }

                GUI.backgroundColor = P09BManualObservationController.ArmRootOnNextTick ? Color.yellow : Color.white;
                if (GUILayout.Button(P09BManualObservationController.ArmRootOnNextTick ? "[ARMED] Root Mid-Dash" : "Arm Root Mid-Dash", GUILayout.Height(26)))
                {
                    P09BManualObservationController.ArmRootOnNextTick = !P09BManualObservationController.ArmRootOnNextTick;
                }

                GUI.backgroundColor = P09BManualObservationController.ArmFreezeOnNextTick ? Color.yellow : Color.white;
                if (GUILayout.Button(P09BManualObservationController.ArmFreezeOnNextTick ? "[ARMED] Freeze Mid-Dash" : "Arm Freeze Mid-Dash", GUILayout.Height(26)))
                {
                    P09BManualObservationController.ArmFreezeOnNextTick = !P09BManualObservationController.ArmFreezeOnNextTick;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(8);

                // 5. Telemetry & State
                EditorGUILayout.LabelField("5. TRẠNG THÁI RUNTIME & TELEMETRY", EditorStyles.boldLabel);
                if (hero != null && target != null)
                {
                    EditorGUILayout.LabelField($"Hero Position: x={hero.transform.position.x:F3}, y={hero.transform.position.y:F2}, z={hero.transform.position.z:F2}");
                    EditorGUILayout.LabelField($"Target Position: x={target.transform.position.x:F3} ({target.name})");
                    EditorGUILayout.LabelField($"Khoảng cách tới mục tiêu: {distToTarget:F3}m (Tầm đánh AttackRange: {hero.AttackRange:F2}m)");
                    EditorGUILayout.LabelField($"Trạng thái Hero: IsDashing={hero.IsDashing}, CanBasicAttack={hero.CanBasicAttack}, CanMove={hero.CanMove}, CanDash={hero.CanDash}");
                    string rageText = (hero.Rage != null) ? $"{hero.Rage.CurrentRage:F1} / {hero.Rage.MaxRage:F1}" : "N/A";
                    EditorGUILayout.LabelField($"Nộ: {rageText} | Hồi chiêu: {(onCd ? cdRem.ToString("F2") + "s" : "Sẵn sàng")}");

                    if (hero.Movement != null && hero.Movement.IsDashing && hero.Movement.ActiveDashPlan != null)
                    {
                        var plan = hero.Movement.ActiveDashPlan;
                        EditorGUILayout.LabelField($"Dash Đang Diễn Ra: Hướng={hero.Movement.DashDirection.x:F1}, Tốc độ={hero.Movement.DashSpeed:F1}m/s, Còn lại={hero.Movement.DashRemainingDistance:F2}m/{plan.TotalPlannedDistance:F2}m");
                    }

                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField($"Telemetry: Lệnh={P09BManualObservationController.TotalDashCommands} | Bắt đầu={P09BManualObservationController.TotalDashStarts} | Hoàn tất={P09BManualObservationController.TotalDashCompletes} | Hủy={P09BManualObservationController.TotalDashAborts}");
                }

                EditorGUILayout.Space(8);

                // 6. Log
                EditorGUILayout.LabelField("6. NHẬT KÝ THAO TÁC GẦN NHẤT", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(P09BManualObservationController.LastActionLog, MessageType.None);

                EditorGUILayout.Space(8);

                // 7. Checklist for Owner
                EditorGUILayout.LabelField("7. CHECKLIST CHỦ DỰ ÁN (P09-B DASH)", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "S-DASH-01: 1 Lần bấm = 1 Lần lướt tiếp cận mục tiêu (Không tự động lướt lặp lại sau CD)\n" +
                    "S-DASH-02: Điểm dừng chính xác (Hero x=0, Target A x=4.5, Tầm đánh 1.5m -> Điểm dừng x=3.0, cách mục tiêu 1.5m)\n" +
                    "S-DASH-03: Tạm dừng UI Pause / Tiếp tục (Đóng băng giữa đường, không bước bù hoặc đánh thường, tiếp tục mượt mà)\n" +
                    "S-DASH-04: Bị CC ngắt dash (Áp dụng Stun/Root/Freeze giữa chừng -> Hủy lướt lập tức, không hoàn Nộ)\n" +
                    "S-DASH-05: Hết CC hành động bình thường (Hết thời gian CC -> Hero hồi phục đánh thường và hành động bình thường)\n" +
                    "S-DASH-06: Thử từ chối Cooldown (Sau khi Dash Target A, chọn Target B và bấm nút Dash khi CD còn -> Nhận CooldownNotReady từ production validator)",
                    MessageType.Info);
            }
            finally
            {
                EditorGUILayout.EndScrollView();
            }
        }
    }

    /// <summary>
    /// Bootstrap entry point invoked when launcher starts Unity for P09-B manual observation under Save Guard protection.
    /// Strictly guards session authorization.
    /// </summary>
    [InitializeOnLoad]
    public static class P09BManualSessionBootstrap
    {
        private static bool _pendingDelayCall = false;

        static P09BManualSessionBootstrap()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static bool IsSessionAuthorized()
        {
            bool argPresent = false;
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "-p09bManualSession", StringComparison.OrdinalIgnoreCase))
                {
                    argPresent = true;
                    break;
                }
            }
            bool sessionStateAuth = SessionState.GetBool("P09B_Manual_Session_Authorized", false);
            return argPresent && sessionStateAuth;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
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

                    P09BManualObservationController.SetupOrResetFixture();
                    P09BManualTestWindow.ShowWindow();
                };
            }
            else if (state == PlayModeStateChange.ExitingPlayMode)
            {
                _pendingDelayCall = false;
                P09BManualObservationController.CancelPendingReset();
                P09BManualObservationController.TeardownFixture();
            }
        }

        public static void LaunchFromSaveGuard()
        {
            Debug.Log("[P09-B BOOTSTRAP] LaunchFromSaveGuard invoked by Save Guard launcher.");
            SessionState.SetBool("P09B_Manual_Session_Authorized", true);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            P09BManualTestWindow.ShowWindow();

            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
            }
            else
            {
                P09BManualObservationController.SetupOrResetFixture();
            }
        }
    }

    /// <summary>
    /// Automated Smoke Verifier for P09-B Manual Observation Harness (Section E.5).
    /// Tests 10s idle stability, 1 command -> 1 start -> 1 completion, pause/resume, CC abort, and 3x reset without leaks.
    /// Does NOT assign USER_VERIFIED (visual check remains required for human eyes).
    /// </summary>
    public class P09BHarnessSmokeRunner : MonoBehaviour
    {
        public bool Passed { get; private set; } = false;
        public string FailureMessage { get; private set; } = "";

        public static void RunHarnessSmoke_CLI()
        {
            Debug.Log("[P09-B HARNESS SMOKE] Initializing automated harness smoke verification under Save Guard...");
            SessionState.SetBool("P09B_Manual_Session_Authorized", true);
            SessionState.SetBool("RunP09BHarnessSmoke", true);

            if (EditorApplication.isPlaying)
            {
                SpawnSmokeHarnessIfMissing();
            }
            else
            {
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
                EditorApplication.isPlaying = true;
            }
        }

        [InitializeOnLoadMethod]
        private static void RegisterSmokeWatcher()
        {
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("RunP09BHarnessSmoke", false))
                {
                    SessionState.SetBool("RunP09BHarnessSmoke", false);
                    SpawnSmokeHarnessIfMissing();
                }
            };
        }

        private static void SpawnSmokeHarnessIfMissing()
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<P09BHarnessSmokeRunner>();
            if (existing != null) return;

            var go = new GameObject("P09B_Harness_Smoke_Runner");
            var runner = go.AddComponent<P09BHarnessSmokeRunner>();
            runner.StartCoroutine(runner.RunSmokeCoroutine());
        }

        private static System.Collections.IEnumerator WaitForFixtureReady(float timeout = 5.0f)
        {
            float timer = 0f;
            while (!P09BManualObservationController.IsFixtureReady && timer < timeout)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        public System.Collections.IEnumerator RunSmokeCoroutine()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING P09-B MANUAL HARNESS AUTOMATED SMOKE VERIFICATION (Section E.5)");
            Debug.Log("================================================================================");

            // Step 0: Initialize fixture and wait for Start/lifecycle to settle
            P09BManualObservationController.SetupOrResetFixture();
            yield return WaitForFixtureReady();

            if (!P09BManualObservationController.IsFixtureReady)
            {
                FailureMessage = "Fixture failed to initialize READY state!";
                Debug.LogError($"[SMOKE ERROR] {FailureMessage}");
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            var hero = P09BManualObservationController.HeroRef;
            var targetA = P09BManualObservationController.TargetARef;
            var targetB = P09BManualObservationController.TargetBRef;
            var bm = P09BManualObservationController.BattleManagerRef;

            Debug.Log($"[SMOKE STEP 0: READY] Hero=(0, -0.30), HP={hero.Health.CurrentHealth:F0}, Rage={hero.Rage.CurrentRage:F0}, TargetA=({targetA.transform.position.x:F1}, -0.30), TargetB=({targetB.transform.position.x:F1}, -0.30)");

            // -------------------------------------------------------------
            // Step 1: READY -> idle >= 10s: positions/HP/Rage/roster stable, no auto dash/attack/wave
            // -------------------------------------------------------------
            Debug.Log("[SMOKE STEP 1] Verifying 10 seconds of idle autonomy isolation...");
            float idleElapsed = 0f;
            float initialHeroX = hero.transform.position.x;
            float initialHeroHp = hero.Health.CurrentHealth;
            float initialHeroRage = hero.Rage.CurrentRage;

            while (idleElapsed < 10.0f)
            {
                yield return null;
                idleElapsed += Time.deltaTime;

                if (hero.IsDashing)
                {
                    FailureMessage = "Hero autonomously initiated dash during idle period!";
                    Debug.LogError($"[SMOKE ERROR] {FailureMessage}");
                    if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                    yield break;
                }
                if (Mathf.Abs(hero.transform.position.x - initialHeroX) > 0.01f)
                {
                    FailureMessage = $"Hero position drifted during idle period! Delta={hero.transform.position.x - initialHeroX:F4}";
                    Debug.LogError($"[SMOKE ERROR] {FailureMessage}");
                    if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                    yield break;
                }
            }

            bool idleStable = Mathf.Approximately(hero.Health.CurrentHealth, initialHeroHp) &&
                              Mathf.Approximately(hero.Rage.CurrentRage, initialHeroRage) &&
                              !hero.IsDashing &&
                              P09BManualObservationController.TotalDashCommands == 0;
            Debug.Log($"[SMOKE STEP 1 RESULT] Idle 10s Completed: Stable={idleStable}, Commands={P09BManualObservationController.TotalDashCommands}, Starts={P09BManualObservationController.TotalDashStarts}");

            if (!idleStable)
            {
                FailureMessage = "Step 1 Idle Stability check failed!";
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            // Step 1B: Verify fixture remains idle after intentional stat recalculation (hero minimum speed clamp handled in tooling scope)
            Debug.Log("[SMOKE STEP 1B] Triggering intentional hero stat recalculation to verify tooling isolation...");
            hero.RecalculateStats(true);
            yield return null;
            yield return null;
            float postRecalcElapsed = 0f;
            while (postRecalcElapsed < 1.0f)
            {
                yield return null;
                postRecalcElapsed += Time.deltaTime;
                if (Mathf.Abs(hero.transform.position.x - initialHeroX) > 0.01f)
                {
                    FailureMessage = "Hero drifted after intentional stat recalculation!";
                    Debug.LogError($"[SMOKE ERROR] {FailureMessage}");
                    if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                    yield break;
                }
            }
            Debug.Log("[SMOKE STEP 1B RESULT] Intentional stat recalculation did not cause drift. Fixture remained idle!");

            // -------------------------------------------------------------
            // Step 2: 1 command -> 1 start -> 1 completion, wait past cooldown (5s), no self-repeat
            // -------------------------------------------------------------
            Debug.Log("[SMOKE STEP 2] Verifying 1 command -> 1 start -> 1 completion -> no auto repeat...");
            P09BManualObservationController.ResetTelemetry();
            P09BManualObservationController.ExecuteManualDash();
            yield return null; // Dash started on this frame

            // Test controller entry points during active dash
            if (hero.IsDashing)
            {
                bool resetPosBlocked = !P09BManualObservationController.ResetEntityPositions();
                bool refillRageBlocked = !P09BManualObservationController.RefillRage();
                bool drainRageBlocked = !P09BManualObservationController.DrainRage();
                bool resetCdBlocked = !P09BManualObservationController.ResetCooldown();
                bool presetNearBlocked = !P09BManualObservationController.SetTargetAPresetNear();
                bool presetFarBlocked = !P09BManualObservationController.SetTargetAPresetFar();

                // Test switching target during active dash does not divert active dash plan's bound target
                P09BManualObservationController.SwitchTarget(targetB);
                bool bindingRetained = (hero.Movement.ActiveDashPlan != null && hero.Movement.ActiveDashPlan.BoundTarget == targetA);
                P09BManualObservationController.SwitchTarget(targetA); // Restore Target A

                if (!resetPosBlocked || !refillRageBlocked || !drainRageBlocked || !resetCdBlocked || !presetNearBlocked || !presetFarBlocked || !bindingRetained)
                {
                    FailureMessage = $"Step 2 Active Dash Controller Guards failed! resetPos={resetPosBlocked}, refill={refillRageBlocked}, drain={drainRageBlocked}, cd={resetCdBlocked}, presetNear={presetNearBlocked}, presetFar={presetFarBlocked}, bindingRetained={bindingRetained}";
                    Debug.LogError($"[SMOKE ERROR] {FailureMessage}");
                    if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                    yield break;
                }
                Debug.Log("[SMOKE STEP 2 GUARDS] All controller entry points correctly rejected during active dash, and target switching verified binding retained.");
            }

            float dashWait = 0f;
            while (hero.IsDashing && dashWait < 3.0f)
            {
                yield return null;
                dashWait += Time.deltaTime;
            }

            bool singleDashCompleted = (P09BManualObservationController.TotalDashCommands == 1) &&
                                       (P09BManualObservationController.TotalDashStarts == 1) &&
                                       (P09BManualObservationController.TotalDashCompletes == 1) &&
                                       (P09BManualObservationController.TotalDashAborts == 0) &&
                                       Mathf.Abs(hero.transform.position.x - 3.0f) <= 0.05f;

            Debug.Log($"[SMOKE STEP 2 DASH] Completed={singleDashCompleted}, HeroX={hero.transform.position.x:F3}, Testing Cooldown Rejection...");

            // Step 2B (F-GUI-REJECTION automated verification): While on CD, switch to Target B and request dash
            P09BManualObservationController.SwitchTarget(targetB);
            P09BManualObservationController.ExecuteManualDash();
            bool cdRejected = (P09BManualObservationController.TotalDashCommands == 2) &&
                              (P09BManualObservationController.TotalDashStarts == 1) &&
                              P09BManualObservationController.LastActionLog.Contains("CooldownNotReady");
            Debug.Log($"[SMOKE STEP 2B CD REJECTION] CdRejected={cdRejected}, LastLog='{P09BManualObservationController.LastActionLog}'");
            P09BManualObservationController.SwitchTarget(targetA); // Restore Target A

            // Wait past cooldown (5s) to prove zero autonomous repetition
            float cdWait = 0f;
            while (cdWait < 5.5f)
            {
                yield return null;
                cdWait += Time.deltaTime;
            }

            bool noAutoRepeat = (P09BManualObservationController.TotalDashCommands == 2) &&
                                (P09BManualObservationController.TotalDashStarts == 1) &&
                                !hero.IsDashing;
            Debug.Log($"[SMOKE STEP 2 RESULT] SingleDash={singleDashCompleted}, CdRejected={cdRejected}, NoAutoRepeatAfterCD={noAutoRepeat}");

            if (!singleDashCompleted || !cdRejected || !noAutoRepeat)
            {
                FailureMessage = "Step 2 Single Dash / Cooldown Rejection / No Auto Repeat failed!";
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            // -------------------------------------------------------------
            // Step 3: Reset -> arm pause/resume freeze/continue. Reset -> arm CC abort, no refund after commit
            // -------------------------------------------------------------
            Debug.Log("[SMOKE STEP 3A] Testing Reset -> Arm Pause / Resume...");
            P09BManualObservationController.SetupOrResetFixture();
            yield return WaitForFixtureReady();
            hero = P09BManualObservationController.HeroRef;
            bm = P09BManualObservationController.BattleManagerRef;

            P09BManualObservationController.ArmPauseOnNextTick = true;
            P09BManualObservationController.ExecuteManualDash();
            yield return null;
            yield return null;

            bool pauseFrozen = bm.IsCombatPausedByUI;
            bool fixtureReadyInPause = P09BManualObservationController.IsFixtureReady;
            float pauseX = hero.transform.position.x;
            for (int p = 0; p < 5; p++) yield return null;
            bool posHeldInPause = Mathf.Approximately(hero.transform.position.x, pauseX);

            P09BManualObservationController.TogglePause(); // Resume
            float resumeWait = 0f;
            while (hero.IsDashing && resumeWait < 3.0f)
            {
                yield return null;
                resumeWait += Time.deltaTime;
            }
            bool resumeCompleted = (P09BManualObservationController.TotalDashCompletes == 1);
            Debug.Log($"[SMOKE STEP 3A RESULT] PauseFrozen={pauseFrozen}, FixtureReadyInPause={fixtureReadyInPause}, PosHeld={posHeldInPause}, ResumeCompleted={resumeCompleted}");

            Debug.Log("[SMOKE STEP 3B] Testing Reset -> Arm CC Abort (No refund after commit)...");
            P09BManualObservationController.SetupOrResetFixture();
            yield return WaitForFixtureReady();
            hero = P09BManualObservationController.HeroRef;
            bm = P09BManualObservationController.BattleManagerRef;

            P09BManualObservationController.ArmStunOnNextTick = true;
            P09BManualObservationController.ExecuteManualDash();
            yield return null;
            yield return null;

            bool ccAborted = (P09BManualObservationController.TotalDashAborts == 1) &&
                             (P09BManualObservationController.TotalDashCompletes == 0) &&
                             !hero.IsDashing;
            bool noRefundPostCommit = Mathf.Approximately(hero.Rage.CurrentRage, 80f);
            Debug.Log($"[SMOKE STEP 3B RESULT] CCAborted={ccAborted}, NoRefundPostCommit={noRefundPostCommit} (Rage={hero.Rage.CurrentRage:F1})");

            if (!pauseFrozen || !fixtureReadyInPause || !posHeldInPause || !resumeCompleted || !ccAborted || !noRefundPostCommit)
            {
                FailureMessage = "Step 3 Pause/Resume & CC Abort failed!";
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            // -------------------------------------------------------------
            // Step 4: Full reset during active dash cleans transaction, then 3 consecutive resets leak check
            // -------------------------------------------------------------
            Debug.Log("[SMOKE STEP 4A] Testing Full Reset during active dash to verify transaction abort before rebuild...");
            P09BManualObservationController.SetupOrResetFixture();
            yield return WaitForFixtureReady();
            hero = P09BManualObservationController.HeroRef;
            P09BManualObservationController.ExecuteManualDash();
            yield return null;
            if (hero != null && hero.IsDashing)
            {
                P09BManualObservationController.SetupOrResetFixture();
                yield return WaitForFixtureReady();
                hero = P09BManualObservationController.HeroRef;
                if (hero == null || hero.IsDashing || !P09BManualObservationController.IsFixtureReady)
                {
                    FailureMessage = "Step 4A Full reset during active dash failed to clean up or restore ready state!";
                    Debug.LogError($"[SMOKE ERROR] {FailureMessage}");
                    if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                    yield break;
                }
                Debug.Log("[SMOKE STEP 4A RESULT] Full reset during active dash cleanly cleaned up transaction and restored READY fixture.");
            }

            Debug.Log("[SMOKE STEP 4B] Testing 3 consecutive fixture resets for leak prevention...");
            for (int r = 0; r < 3; r++)
            {
                P09BManualObservationController.SetupOrResetFixture();
                yield return WaitForFixtureReady();
            }

            var fixtureRoots = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            int rootCount = 0;
            int heroCount = 0;
            int monsterCount = 0;
            int cameraCount = 0;

            foreach (var t in fixtureRoots)
            {
                if (t.name == "[P09B_Manual_Observation_Fixture]") rootCount++;
                if (t.name == "Hero_P09B") heroCount++;
                if (t.name == "Monster_Target_A" || t.name == "Monster_Target_B") monsterCount++;
                if (t.name == "P09B_Camera") cameraCount++;
            }

            bool singleRoot = (rootCount == 1);
            bool singleHero = (heroCount == 1);
            bool twoMonsters = (monsterCount == 2);
            bool singleCamera = (cameraCount == 1);
            bool cleanTelemetry = (P09BManualObservationController.TotalDashCommands == 0) &&
                                  (P09BManualObservationController.TotalDashStarts == 0) &&
                                  (P09BManualObservationController.TotalDashCompletes == 0) &&
                                  (P09BManualObservationController.TotalDashAborts == 0);

            Debug.Log($"[SMOKE STEP 4 RESULT] 3x Reset: Roots={rootCount}, Hero={heroCount}, Monsters={monsterCount}, Cameras={cameraCount}, CleanTelemetry={cleanTelemetry}");

            if (!singleRoot || !singleHero || !twoMonsters || !singleCamera || !cleanTelemetry)
            {
                FailureMessage = "Step 4 3x Reset Cleanliness / Leak check failed!";
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            // -------------------------------------------------------------
            // Step 5: Screen capture of Game View
            // -------------------------------------------------------------
            if (!System.IO.Directory.Exists("Screenshots"))
            {
                System.IO.Directory.CreateDirectory("Screenshots");
            }
            string screenshotPath = "Screenshots/P09B_ManualHarness_GameView.png";
            var cam = P09BManualObservationController.CameraRef;
            if (cam != null)
            {
                var rt = new RenderTexture(1920, 1080, 24);
                cam.targetTexture = rt;
                var currentRT = RenderTexture.active;
                RenderTexture.active = rt;
                cam.Render();
                Texture2D image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                image.Apply();
                RenderTexture.active = currentRT;
                cam.targetTexture = null;
                byte[] bytes = image.EncodeToPNG();
                UnityEngine.Object.DestroyImmediate(image);
                UnityEngine.Object.DestroyImmediate(rt);
                System.IO.File.WriteAllBytes(screenshotPath, bytes);
            }
            else
            {
                ScreenCapture.CaptureScreenshot(screenshotPath);
            }
            yield return null;
            Debug.Log($"[SMOKE STEP 5] Visual snapshot saved to: {screenshotPath}");
            Debug.Log("[SMOKE STEP 5 NOTE] Human eye visual observation remains required by Project Owner (GUI observation marked NOT VISUALLY VERIFIED until GUI tested).");

            Passed = true;
            Debug.Log("================================================================================");
            Debug.Log("   [P09-B HARNESS SMOKE RESULT]: ALL 5 SMOKE STEPS PASSED NATURALLY!");
            Debug.Log("================================================================================");

            if (Application.isBatchMode)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.update += () =>
                {
                    EditorApplication.Exit(Passed ? 0 : 1);
                };
                EditorApplication.Exit(Passed ? 0 : 1);
            }
        }
    }

    /// <summary>
    /// Automated Queue & Lifecycle Test Suite for P09-B Manual Observation Harness (G1 & G2).
    /// Tests:
    /// Q1: Schedule -> dispatch valid: exactly 1 reset, READY only after lifecycle barrier, 1 root/hero/camera, 2 targets.
    /// Q2: Schedule -> Cancel before dispatch: setup/root count does not increase, no resurrected fixture.
    /// Q3: Schedule -> teardown / exit Play Mode before dispatch: no fixture built in Edit Mode, 0 exceptions.
    /// Q4: Schedule -> close window before dispatch: window close cancels pending reset, no auto-reset after close.
    /// Q5: Cancel old request -> schedule new request: only new request executes once, readiness/generation intact.
    /// </summary>
    public class P09BQueueLifecycleRunner : MonoBehaviour
    {
        public bool Passed { get; private set; } = false;
        public string FailureMessage { get; private set; } = "";

        public static void RunQueueLifecycleTests_CLI()
        {
            Debug.Log("[P09-B QUEUE TESTS] Initializing automated queue & lifecycle verification under Save Guard...");
            SessionState.SetBool("P09B_Manual_Session_Authorized", true);
            SessionState.SetBool("RunP09BQueueTests", true);

            if (EditorApplication.isPlaying)
            {
                SpawnQueueRunnerIfMissing();
            }
            else
            {
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
                EditorApplication.isPlaying = true;
            }
        }

        [InitializeOnLoadMethod]
        private static void RegisterQueueWatcher()
        {
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("RunP09BQueueTests", false))
                {
                    SessionState.SetBool("RunP09BQueueTests", false);
                    SpawnQueueRunnerIfMissing();
                }
            };
        }

        private static void SpawnQueueRunnerIfMissing()
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<P09BQueueLifecycleRunner>();
            if (existing != null) return;

            var go = new GameObject("P09B_Queue_Lifecycle_Runner");
            var runner = go.AddComponent<P09BQueueLifecycleRunner>();
            runner.StartCoroutine(runner.RunQueueTestsCoroutine());
        }

        public System.Collections.IEnumerator RunQueueTestsCoroutine()
        {
            Debug.Log("================================================================================");
            Debug.Log("   STARTING P09-B QUEUE & LIFECYCLE VERIFICATION SUITE (G1 & G2)");
            Debug.Log("================================================================================");

            // -------------------------------------------------------------
            // TEST Q1: Schedule -> dispatch valid: exactly 1 reset, READY only after lifecycle barrier
            // -------------------------------------------------------------
            Debug.Log("[QUEUE TEST Q1] Testing Schedule -> valid dispatch and lifecycle barrier...");
            P09BManualObservationController.TeardownFixture();
            yield return null;

            int initialResetCount = P09BManualObservationController.ResetCount;
            P09BManualObservationController.ScheduleResetFixture();

            // Immediately assert pending flag and unready
            bool q1PendingImmediate = P09BManualObservationController.IsResetPending;
            bool q1UnreadyImmediate = !P09BManualObservationController.IsFixtureReady;

            // Wait 1 frame for delayCall to dispatch SetupOrResetFixture
            yield return null;

            // After SetupOrResetFixture runs: pending is false, but lifecycle must be stabilizing (NOT ready yet!)
            bool q1Stabilizing = P09BManualObservationController.IsStabilizing;
            bool q1NotReadyYet = !P09BManualObservationController.IsFixtureReady;

            // Wait for stabilization
            float q1Wait = 0f;
            while (!P09BManualObservationController.IsFixtureReady && q1Wait < 3.0f)
            {
                q1Wait += Time.unscaledDeltaTime;
                yield return null;
            }

            bool q1Ready = P09BManualObservationController.IsFixtureReady;
            bool q1ResetCountInc = (P09BManualObservationController.ResetCount == initialResetCount + 1);

            int rootCount = 0, heroCount = 0, targetCount = 0, cameraCount = 0;
            foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t.name == "[P09B_Manual_Observation_Fixture]") rootCount++;
                if (t.name == "Hero_P09B") heroCount++;
                if (t.name == "Monster_Target_A" || t.name == "Monster_Target_B") targetCount++;
                if (t.name == "P09B_Camera") cameraCount++;
            }

            bool q1Hierarchy = (rootCount == 1) && (heroCount == 1) && (targetCount == 2) && (cameraCount == 1);
            bool q1Pass = q1PendingImmediate && q1UnreadyImmediate && q1Stabilizing && q1NotReadyYet && q1Ready && q1ResetCountInc && q1Hierarchy;

            Debug.Log($"[QUEUE TEST Q1 RESULT] PendingImm={q1PendingImmediate}, UnreadyImm={q1UnreadyImmediate}, Stabilizing={q1Stabilizing}, NotReadyYet={q1NotReadyYet}, Ready={q1Ready}, ResetInc={q1ResetCountInc}, Hierarchy={q1Hierarchy} | Pass={q1Pass}");
            if (!q1Pass)
            {
                FailureMessage = "Queue Test Q1 (Valid Schedule & Lifecycle Barrier) failed!";
                Debug.LogError($"[QUEUE ERROR] {FailureMessage}");
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            // -------------------------------------------------------------
            // TEST Q2: Schedule -> Cancel before dispatch: setup count does not increase, no resurrected fixture
            // -------------------------------------------------------------
            Debug.Log("[QUEUE TEST Q2] Testing Schedule -> Cancel before dispatch...");
            int resetCountBeforeQ2 = P09BManualObservationController.ResetCount;
            P09BManualObservationController.ScheduleResetFixture();
            bool q2Pending = P09BManualObservationController.IsResetPending;

            // Cancel immediately before dispatch
            P09BManualObservationController.CancelPendingReset();
            bool q2NotPending = !P09BManualObservationController.IsResetPending;

            // Wait 3 frames to allow any stale delayCall to pump
            yield return null;
            yield return null;
            yield return null;

            bool q2ResetCountUnchanged = (P09BManualObservationController.ResetCount == resetCountBeforeQ2);
            bool q2Pass = q2Pending && q2NotPending && q2ResetCountUnchanged;
            Debug.Log($"[QUEUE TEST Q2 RESULT] Pending={q2Pending}, Cancelled={q2NotPending}, ResetCountUnchanged={q2ResetCountUnchanged} | Pass={q2Pass}");
            if (!q2Pass)
            {
                FailureMessage = "Queue Test Q2 (Cancel before dispatch) failed!";
                Debug.LogError($"[QUEUE ERROR] {FailureMessage}");
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            // -------------------------------------------------------------
            // TEST Q3: Schedule -> teardown before dispatch
            // -------------------------------------------------------------
            Debug.Log("[QUEUE TEST Q3] Testing Schedule -> Teardown before dispatch...");
            P09BManualObservationController.ScheduleResetFixture();
            P09BManualObservationController.TeardownFixture();

            // Wait 3 frames to pump delayCall
            yield return null;
            yield return null;
            yield return null;

            int rootsAfterTeardown = 0;
            foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t.name == "[P09B_Manual_Observation_Fixture]") rootsAfterTeardown++;
            }

            bool q3NoResurrection = (rootsAfterTeardown == 0);
            bool q3Unready = !P09BManualObservationController.IsFixtureReady;
            bool q3Pass = q3NoResurrection && q3Unready;
            Debug.Log($"[QUEUE TEST Q3 RESULT] NoResurrection={q3NoResurrection}, Unready={q3Unready} | Pass={q3Pass}");
            if (!q3Pass)
            {
                FailureMessage = "Queue Test Q3 (Teardown before dispatch) failed!";
                Debug.LogError($"[QUEUE ERROR] {FailureMessage}");
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            // -------------------------------------------------------------
            // TEST Q4: Schedule -> close window before dispatch
            // -------------------------------------------------------------
            Debug.Log("[QUEUE TEST Q4] Testing Schedule -> Close window before dispatch...");
            var win = EditorWindow.GetWindow<P09BManualTestWindow>("P09-B Dash Observation");
            int resetCountBeforeQ4 = P09BManualObservationController.ResetCount;

            P09BManualObservationController.ScheduleResetFixture();
            bool q4Pending = P09BManualObservationController.IsResetPending;

            // Close window -> OnDisable/OnDestroy cancels pending reset
            win.Close();
            bool q4CancelledOnClose = !P09BManualObservationController.IsResetPending;

            // Wait 3 frames to pump delayCall
            yield return null;
            yield return null;
            yield return null;

            bool q4NoSetupAfterClose = (P09BManualObservationController.ResetCount == resetCountBeforeQ4);
            bool q4Pass = q4Pending && q4CancelledOnClose && q4NoSetupAfterClose;
            Debug.Log($"[QUEUE TEST Q4 RESULT] Pending={q4Pending}, CancelledOnClose={q4CancelledOnClose}, NoSetupAfterClose={q4NoSetupAfterClose} | Pass={q4Pass}");
            if (!q4Pass)
            {
                FailureMessage = "Queue Test Q4 (Close window before dispatch) failed!";
                Debug.LogError($"[QUEUE ERROR] {FailureMessage}");
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            // -------------------------------------------------------------
            // TEST Q5: Cancel old request -> schedule new request
            // -------------------------------------------------------------
            Debug.Log("[QUEUE TEST Q5] Testing Cancel old request -> schedule new request...");
            int resetCountBeforeQ5 = P09BManualObservationController.ResetCount;

            // Request 1
            P09BManualObservationController.ScheduleResetFixture();
            // Cancel Request 1
            P09BManualObservationController.CancelPendingReset();
            // Immediately schedule Request 2
            P09BManualObservationController.ScheduleResetFixture();

            // Wait for Request 2 to execute and stabilize
            float q5Wait = 0f;
            while (!P09BManualObservationController.IsFixtureReady && q5Wait < 4.0f)
            {
                q5Wait += Time.unscaledDeltaTime;
                yield return null;
            }

            bool q5Ready = P09BManualObservationController.IsFixtureReady;
            bool q5ExecutedExactlyOnce = (P09BManualObservationController.ResetCount == resetCountBeforeQ5 + 1);
            bool q5Pass = q5Ready && q5ExecutedExactlyOnce;
            Debug.Log($"[QUEUE TEST Q5 RESULT] Ready={q5Ready}, ExecutedExactlyOnce={q5ExecutedExactlyOnce} (Before={resetCountBeforeQ5}, After={P09BManualObservationController.ResetCount}) | Pass={q5Pass}");
            if (!q5Pass)
            {
                FailureMessage = "Queue Test Q5 (Cancel old -> schedule new request) failed!";
                Debug.LogError($"[QUEUE ERROR] {FailureMessage}");
                if (Application.isBatchMode) { EditorApplication.isPlaying = false; EditorApplication.Exit(1); }
                yield break;
            }

            Passed = true;
            Debug.Log("================================================================================");
            Debug.Log("   [P09-B QUEUE LIFECYCLE TESTS RESULT]: ALL 5 QUEUE TESTS PASSED NATURALLY!");
            Debug.Log("================================================================================");

            if (Application.isBatchMode)
            {
                EditorApplication.isPlaying = false;
                EditorApplication.update += () =>
                {
                    EditorApplication.Exit(Passed ? 0 : 1);
                };
                EditorApplication.Exit(Passed ? 0 : 1);
            }
        }
    }
}
