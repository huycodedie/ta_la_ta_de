using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.UI.Core;

namespace WuxiaGame.Core
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [SerializeField] private Hero currentHero;
        [SerializeField] private Monster currentMonster;
        [SerializeField] private MonsterConfigSO defaultMonsterConfig;
        [SerializeField] private CombatConfigSO defaultCombatConfig;
        [SerializeField] private Vector3 monsterSpawnPosition = new Vector3(4f, -0.3f, 0f);

        [Header("Auto Battle Authority (P07.9.1)")]
        [SerializeField] private bool isAutoBattle = true;

        // P08 Runtime-owned monster registry (not serialized into scene asset)
        private readonly List<Monster> activeMonsters = new();
        public IReadOnlyList<Monster> ActiveMonsters => activeMonsters;

        private readonly HashSet<Monster> _processedDeaths = new();
        private readonly Queue<WuxiaGame.Items.EquipmentInstance> _pendingLootQueue = new();
        private Coroutine _activeLootLifecycleCoroutine;
        private int _lootTransactionCounter = 0;
        private Coroutine _activeEncounterTransitionCoroutine;
        private int _encounterTransitionCounter = 0;
        private readonly Dictionary<Entity, WuxiaGame.Combat.SkillExecutionRequest> _finishingCasters = new();
        private int _finishingEncounterIndex = 0;

        // P08 Normal Wave Spawning & Exponential Stat Growth State
        private int _completedNormalWaveCount = 0;
        public int CompletedNormalWaveCount => _completedNormalWaveCount;

        private int _currentWaveId = 0;
        public int CurrentWaveId => _currentWaveId;

        private int _lastCompletedWaveId = -1;

        private int _currentWaveTier = 0;
        public int CurrentWaveTier => _currentWaveTier;

        private float _currentWaveStatMultiplier = 1.0f;
        public float CurrentWaveStatMultiplier => _currentWaveStatMultiplier;

        public float NextWaveStatMultiplier => Mathf.Pow(1.01f, _completedNormalWaveCount);

        private int _lastWaveMonsterCount = 0;
        public int LastWaveMonsterCount => _lastWaveMonsterCount;

        private MonsterConfigSO _activeWaveMonsterConfig;
        public MonsterConfigSO ActiveWaveMonsterConfig => _activeWaveMonsterConfig;

        public static readonly Vector3[] WaveSpawnPositions5 = new Vector3[]
        {
            new Vector3(2.2f, -0.3f, 0f),
            new Vector3(3.0f, -0.3f, 0f),
            new Vector3(3.8f, -0.3f, 0f),
            new Vector3(4.6f, -0.3f, 0f),
            new Vector3(5.4f, -0.3f, 0f)
        };

        public static readonly Vector3[] WaveSpawnPositions4 = new Vector3[]
        {
            new Vector3(2.2f, -0.3f, 0f),
            new Vector3(3.2f, -0.3f, 0f),
            new Vector3(4.2f, -0.3f, 0f),
            new Vector3(5.2f, -0.3f, 0f)
        };

        public bool CanEntityTickDuringTransition(Entity entity)
        {
            if (isCombatPausedByUI) return false;
            if (entity == null || !entity.gameObject.activeInHierarchy || !entity.enabled || !entity.IsAlive || !entity.IsCasting) return false;
            if (EncounterIndex != _finishingEncounterIndex) return false;
            if (!_finishingCasters.TryGetValue(entity, out var recordedRequest)) return false;
            if (entity.CastState == null || entity.CastState.ActiveRequest != recordedRequest || entity.CastState.IsFinished) return false;
            return true;
        }

        private void CancelLootLifecycle()
        {
            _lootTransactionCounter++;
            if (_activeLootLifecycleCoroutine != null)
            {
                StopCoroutine(_activeLootLifecycleCoroutine);
                _activeLootLifecycleCoroutine = null;
            }
            CancelEncounterTransition();
        }

        private void CancelEncounterTransition()
        {
            _encounterTransitionCounter++;
            if (_activeEncounterTransitionCoroutine != null)
            {
                StopCoroutine(_activeEncounterTransitionCoroutine);
                _activeEncounterTransitionCoroutine = null;
            }
            _finishingCasters.Clear();
        }

        public Hero CurrentHero => currentHero;
        public Monster CurrentMonster => currentMonster;
        public int EncounterIndex { get; private set; } = 1;
        public bool IsBattleActive { get; set; }
        public bool IsAutoBattle => isAutoBattle;

        public int GetRegistrationOrder(Monster monster)
        {
            if (monster == null) return int.MaxValue;
            int idx = activeMonsters.IndexOf(monster);
            return idx >= 0 ? idx : int.MaxValue;
        }

        public Monster GetNextLivingMonster()
        {
            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m != null && m.gameObject.activeInHierarchy && m.IsAlive && m.Health != null && m.Health.CurrentHealth > 0f)
                {
                    return m;
                }
            }
            return null;
        }

        public bool HasLivingMonster()
        {
            return GetNextLivingMonster() != null;
        }

        public int GetLivingMonsterCount()
        {
            int count = 0;
            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m != null && m.gameObject.activeInHierarchy && m.IsAlive && m.Health != null && m.Health.CurrentHealth > 0f)
                {
                    count++;
                }
            }
            return count;
        }

        public bool IsWaveFullyDefeated()
        {
            if (activeMonsters.Count == 0) return false;

            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m == null) continue;
                if (m.IsAlive && m.Health != null && m.Health.CurrentHealth > 0f)
                {
                    return false;
                }
                if (!_processedDeaths.Contains(m))
                {
                    return false;
                }
            }
            return true;
        }

        public void DeactivateAmbientSceneMonsters()
        {
            var allSceneMonsters = Object.FindObjectsByType<Monster>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < allSceneMonsters.Length; i++)
            {
                var sm = allSceneMonsters[i];
                if (sm != null && !activeMonsters.Contains(sm))
                {
                    sm.gameObject.SetActive(false);
                }
            }
        }

        public void ResetWaveProgression()
        {
            _completedNormalWaveCount = 0;
            _lastCompletedWaveId = -1;
            _currentWaveTier = 0;
            _currentWaveStatMultiplier = 1.0f;
            _lastWaveMonsterCount = 0;
            _currentWaveId = 0;
            if (_activeWaveMonsterConfig != null)
            {
                if (Application.isPlaying) Destroy(_activeWaveMonsterConfig);
                else DestroyImmediate(_activeWaveMonsterConfig);
                _activeWaveMonsterConfig = null;
            }
        }

        public void SetAutoBattle(bool enabled)
        {
            if (isAutoBattle != enabled)
            {
                isAutoBattle = enabled;
                EventBus.RaiseAutoBattleChanged(enabled);
                Debug.Log($"[BATTLE] Auto Battle set to: {enabled}");
            }
        }

        public static void ResetInstance()
        {
            Instance = null;
        }

        public void SetAsInstance()
        {
            Instance = this;
            LoadConfigsIfMissing();
            ClearActiveMonsters();
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntityDied += HandleEntityDied;
        }

        public int PendingLootQueueCount => _pendingLootQueue.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadConfigsIfMissing();
            DeactivateAmbientSceneMonsters();
        }

        public void EstablishCanonicalEncounterMonsters()
        {
            if (activeMonsters.Count > 0) return;
            PrepareAndStartNormalWave();
        }

        private void OnEnable()
        {
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntityDied += HandleEntityDied;
        }

        private void OnDisable()
        {
            CancelLootLifecycle();
            EventBus.OnEntityDied -= HandleEntityDied;
        }

        private void OnDestroy()
        {
            CancelLootLifecycle();
            if (_activeWaveMonsterConfig != null)
            {
                if (Application.isPlaying) Destroy(_activeWaveMonsterConfig);
                else DestroyImmediate(_activeWaveMonsterConfig);
                _activeWaveMonsterConfig = null;
            }
            if (Instance == this)
            {
                Instance = null;
            }
            EventBus.OnEntityDied -= HandleEntityDied;
        }

        public void LoadConfigsIfMissing()
        {
            if (defaultMonsterConfig == null)
            {
                defaultMonsterConfig = Resources.Load<MonsterConfigSO>("Data/MonsterConfig");
#if UNITY_EDITOR
                if (defaultMonsterConfig == null)
                {
                    defaultMonsterConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<MonsterConfigSO>("Assets/_Game/Data/MonsterConfig.asset");
                }
#endif
            }

            if (defaultCombatConfig == null)
            {
                defaultCombatConfig = Resources.Load<CombatConfigSO>("Data/CombatConfig");
#if UNITY_EDITOR
                if (defaultCombatConfig == null)
                {
                    defaultCombatConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<CombatConfigSO>("Assets/_Game/Data/CombatConfig.asset");
                }
#endif
            }
        }

        public void SetConfigs(MonsterConfigSO monsterCfg, CombatConfigSO combatCfg)
        {
            defaultMonsterConfig = monsterCfg;
            defaultCombatConfig = combatCfg;
        }

        public void RegisterHero(Hero hero)
        {
            currentHero = hero;
            if (currentMonster != null && currentHero != null)
            {
                currentHero.SetCurrentTarget(currentMonster);
                currentMonster.SetCurrentTarget(currentHero);
            }
        }

        public void RegisterMonster(Monster monster)
        {
            if (monster == null) return;
            if (!activeMonsters.Contains(monster))
            {
                activeMonsters.Add(monster);
                int idx = activeMonsters.Count - 1;
                string role = idx == 0 ? "primary" : (idx == 1 ? "secondary" : $"monster_{idx}");
                Debug.Log($"RegistrationOrder[{idx}] = {role} ({monster.EntityName}: {monster.name})");
            }

            if (currentMonster == null || !currentMonster.IsAlive)
            {
                currentMonster = monster;
            }

            if (currentHero != null)
            {
                if (currentHero.CurrentTarget == null || !currentHero.CurrentTarget.IsAlive)
                {
                    currentHero.SetCurrentTarget(currentMonster);
                }
                if (monster.CurrentTarget == null || !monster.CurrentTarget.IsAlive)
                {
                    monster.SetCurrentTarget(currentHero);
                }
            }
        }

        public void UnregisterMonster(Monster monster)
        {
            if (monster == null) return;
            activeMonsters.Remove(monster);
            if (currentMonster == monster)
            {
                currentMonster = GetNextLivingMonster();
            }
        }

        public void ClearActiveMonsters()
        {
            activeMonsters.Clear();
            _processedDeaths.Clear();
            currentMonster = null;
        }

        public void PrepareAndStartNormalWave(int monsterCount = 0)
        {
            CancelLootLifecycle();
            LoadConfigsIfMissing();
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntityDied += HandleEntityDied;

            // 1. Safely clean up previous wave monsters
            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m != null && m.gameObject != null)
                {
                    m.DisableEntityActions();
                    m.SetCurrentTarget(null);
                    if (Application.isPlaying) Destroy(m.gameObject);
                    else DestroyImmediate(m.gameObject);
                }
            }
            activeMonsters.Clear();
            _processedDeaths.Clear();
            currentMonster = null;

            // 2. Clean up previous runtime monster config if exists
            if (_activeWaveMonsterConfig != null)
            {
                if (Application.isPlaying) Destroy(_activeWaveMonsterConfig);
                else DestroyImmediate(_activeWaveMonsterConfig);
                _activeWaveMonsterConfig = null;
            }

            // 3. Deactivate any unmanaged ambient scene monsters
            DeactivateAmbientSceneMonsters();

            // 4. Update wave ID
            _currentWaveId++;

            // 5. Invariant tier & multiplier for this wave
            _currentWaveTier = _completedNormalWaveCount;
            _currentWaveStatMultiplier = Mathf.Pow(1.01f, _currentWaveTier);

            // 6. Create scaled runtime MonsterConfigSO clone
            _activeWaveMonsterConfig = ScriptableObject.Instantiate(defaultMonsterConfig);
            _activeWaveMonsterConfig.name = $"MonsterConfig_Wave_{_currentWaveId}_Tier{_currentWaveTier}_x{_currentWaveStatMultiplier:F4}";
            _activeWaveMonsterConfig.hideFlags = HideFlags.DontSave;
            _activeWaveMonsterConfig.InitializeMonsterConfig(
                defaultMonsterConfig.MonsterName,
                defaultMonsterConfig.MaxHealth * _currentWaveStatMultiplier,
                defaultMonsterConfig.Attack * _currentWaveStatMultiplier,
                defaultMonsterConfig.Defense * _currentWaveStatMultiplier,
                defaultMonsterConfig.MovementSpeed,
                defaultMonsterConfig.AttackInterval,
                defaultMonsterConfig.AttackRange,
                defaultMonsterConfig.ExpReward
            );

            // 7. Choose count in {4, 5}
            int count = (monsterCount == 4 || monsterCount == 5) ? monsterCount : Random.Range(4, 6);
            _lastWaveMonsterCount = count;
            Vector3[] spawnPositions = count == 5 ? WaveSpawnPositions5 : WaveSpawnPositions4;

            if (currentHero == null)
            {
                currentHero = Object.FindAnyObjectByType<Hero>();
            }

            // 8. Spawn, initialize, and register all wave monsters
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = i < spawnPositions.Length ? spawnPositions[i] : new Vector3(2.2f + i * 0.8f, -0.3f, 0f);
                GameObject monsterGO = new GameObject($"Monster_Wave_{_currentWaveId}_{i + 1}");
                monsterGO.transform.position = pos;
                monsterGO.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

                SpriteRenderer sr = monsterGO.AddComponent<SpriteRenderer>();
                sr.sprite = UIProceduralTextureFactory.GetMonsterStandeeSprite();
                sr.color = Color.white;
                sr.sortingOrder = 10;
                sr.flipX = true;

                Monster monster = monsterGO.AddComponent<Monster>();
                monster.InitializeMonster(_activeWaveMonsterConfig, defaultCombatConfig);

                RegisterMonster(monster);
                EventBus.RaiseEntitySpawned(monster);
            }

            // 9. Assign targets before enabling combat
            if (activeMonsters.Count > 0)
            {
                currentMonster = activeMonsters[0];
            }

            if (currentHero != null)
            {
                currentHero.SetCurrentTarget(currentMonster);
            }

            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m != null && m.IsAlive && currentHero != null)
                {
                    m.SetCurrentTarget(currentHero);
                }
            }

            // 10. Enable entity actions
            if (currentHero != null)
            {
                currentHero.EnableEntityActions();
            }

            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m != null && m.IsAlive)
                {
                    m.EnableEntityActions();
                }
            }

            // 11. Enter combat state
            IsBattleActive = true;
            CurrentBattleState = BattleState.InProgress;
            EventBus.RaiseBattleStateChanged(BattleState.InProgress);

            Debug.Log($"[WAVE START] Wave #{_currentWaveId} (Encounter #{EncounterIndex}, Tier {_currentWaveTier}) Started with {activeMonsters.Count} monsters. StatMultiplier: {_currentWaveStatMultiplier:F6}. Base HP: {_activeWaveMonsterConfig.MaxHealth:F2}, Atk: {_activeWaveMonsterConfig.Attack:F3}, Def: {_activeWaveMonsterConfig.Defense:F3}");
        }

        public void StartBattle()
        {
            CancelLootLifecycle();
            if (Instance == null) Instance = this;
            LoadConfigsIfMissing();
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntityDied += HandleEntityDied;

            if (currentHero == null)
            {
                currentHero = Object.FindAnyObjectByType<Hero>();
            }

            // If combat already active and valid, don't recreate wave
            if (IsBattleActive && CurrentBattleState == BattleState.InProgress && activeMonsters.Count > 0 && HasLivingMonster())
            {
                return;
            }

            // Production wave creation
            if (activeMonsters.Count == 0)
            {
                PrepareAndStartNormalWave();
                return;
            }

            // If a unit test pre-registered fixture monsters into activeMonsters:
            if (currentMonster == null && activeMonsters.Count > 0)
            {
                currentMonster = activeMonsters[0];
            }

            if (currentHero != null)
            {
                currentHero.EnableEntityActions();
                if (currentMonster != null) currentHero.SetCurrentTarget(currentMonster);
            }

            if (_currentWaveId <= 0)
            {
                _currentWaveId = 1;
            }

            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m != null && m.IsAlive)
                {
                    m.EnableEntityActions();
                    if (currentHero != null) m.SetCurrentTarget(currentHero);
                }
            }

            IsBattleActive = true;
            CurrentBattleState = BattleState.InProgress;
            EventBus.RaiseBattleStateChanged(BattleState.InProgress);
            Debug.Log("[BattleManager] Battle Started!");
        }

        public Monster SpawnMonster(MonsterConfigSO config = null, Vector3? position = null)
        {
            LoadConfigsIfMissing();
            MonsterConfigSO cfg = config != null ? config : (_activeWaveMonsterConfig != null ? _activeWaveMonsterConfig : defaultMonsterConfig);
            Vector3 pos = position.HasValue ? position.Value : monsterSpawnPosition;

            Debug.Log($"[COMBAT] Spawning Monster #{EncounterIndex}");

            GameObject monsterGO = new GameObject($"Monster_Enc_{EncounterIndex}_{activeMonsters.Count + 1}");
            monsterGO.transform.position = pos;
            monsterGO.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

            SpriteRenderer sr = monsterGO.AddComponent<SpriteRenderer>();
            sr.sprite = UIProceduralTextureFactory.GetMonsterStandeeSprite();
            sr.color = Color.white;
            sr.sortingOrder = 10;
            sr.flipX = true;

            if (currentHero == null)
            {
                currentHero = FindAnyObjectByType<Hero>();
            }

            Monster monster = monsterGO.AddComponent<Monster>();
            monster.InitializeMonster(cfg, defaultCombatConfig);

            RegisterMonster(monster);

            if (currentHero != null)
            {
                monster.SetCurrentTarget(currentHero);
                if (currentHero.CurrentTarget == null || !currentHero.CurrentTarget.IsAlive)
                {
                    currentHero.SetCurrentTarget(monster);
                }
            }

            EventBus.RaiseEntitySpawned(monster);
            return monster;
        }

        public void EndEncounterAndStartNext()
        {
            Debug.Log($"[ENCOUNTER] Wave #{_currentWaveId} Defeated");
            Debug.Log($"[REAL PLAY TEST]\nWave #{_currentWaveId} DEAD");

            if (currentHero != null)
            {
                currentHero.SetCurrentTarget(null);
            }

            EncounterIndex++;
            PrepareAndStartNormalWave();
        }

        public void RestartBattle()
        {
            CancelLootLifecycle();
            Debug.Log("[BattleManager] Restarting combat encounter (Preserving persistent hero progression)...");

            _pendingLootQueue.Clear();
            pendingLootItem = null;

            if (currentHero != null)
            {
                currentHero.SetCurrentTarget(null);
                if (currentHero.Health != null)
                {
                    currentHero.Health.SetCurrentHealth(currentHero.Health.MaxHealth);
                }
            }

            int retryCount = _lastWaveMonsterCount > 0 ? _lastWaveMonsterCount : 0;
            PrepareAndStartNormalWave(retryCount);
        }

        public BattleState CurrentBattleState { get; private set; } = BattleState.None;

        public bool CanStartCombat(out string reason)
        {
            LoadConfigsIfMissing();
            if (currentHero == null)
            {
                currentHero = Object.FindAnyObjectByType<Hero>();
            }

            if (currentHero == null)
            {
                reason = "Hero entity is missing.";
                return false;
            }

            if (defaultMonsterConfig == null)
            {
                reason = "Default monster configuration is missing.";
                return false;
            }

            reason = "Combat ready.";
            return true;
        }

        public bool StartCombatAfterHeroDeath()
        {
            return ExecutePlayerStartCommand();
        }

        public bool ExecutePlayerStartCommand()
        {
            Debug.Log("[PLAYER COMMAND] START pressed");
            if (!CanStartCombat(out string reason))
            {
                Debug.LogWarning($"[PLAYER COMMAND] Cannot start combat: {reason}");
                return false;
            }

            if (IsBattleActive && CurrentBattleState == BattleState.InProgress && HasLivingMonster())
            {
                return true;
            }

            Debug.Log("[RESUME] Resume conditions validated");
            LoadConfigsIfMissing();

            if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
            {
                currentHero.Health.Revive();
            }

            if (activeMonsters.Count == 0 || !HasLivingMonster())
            {
                int retryCount = _lastWaveMonsterCount > 0 ? _lastWaveMonsterCount : 0;
                PrepareAndStartNormalWave(retryCount);
            }
            else
            {
                Monster targetMonster = GetNextLivingMonster();
                currentMonster = targetMonster;

                if (currentHero != null)
                {
                    currentHero.EnableEntityActions();
                    if (currentMonster != null) currentHero.SetCurrentTarget(currentMonster);
                    Debug.Log("[RESUME] Hero combat re-enabled");
                }

                for (int i = 0; i < activeMonsters.Count; i++)
                {
                    var m = activeMonsters[i];
                    if (m != null && m.IsAlive)
                    {
                        m.EnableEntityActions();
                        if (currentHero != null) m.SetCurrentTarget(currentHero);
                    }
                }

                Debug.Log("[RESUME] Monster combat re-enabled");
                Debug.Log("[RESUME] Target references rebuilt");

                IsBattleActive = true;
                CurrentBattleState = BattleState.InProgress;
                EventBus.OnEntityDied -= HandleEntityDied;
                EventBus.OnEntityDied += HandleEntityDied;
                EventBus.RaiseBattleStateChanged(BattleState.InProgress);
            }

            Debug.Log("[COMBAT] Combat resumed");
            return true;
        }

        [SerializeField] private WuxiaGame.Items.EquipmentInstance pendingLootItem;
        public WuxiaGame.Items.EquipmentInstance PendingLootItem => pendingLootItem;

        public void EnqueuePendingLoot(WuxiaGame.Items.EquipmentInstance item)
        {
            if (item != null)
            {
                _pendingLootQueue.Enqueue(item);
            }
        }

        private void HandleEntityDied(Entity entity)
        {
            if (entity is Hero)
            {
                CancelLootLifecycle();
                IsBattleActive = false;
                pendingLootItem = null;
                _pendingLootQueue.Clear();
                CurrentBattleState = BattleState.AwaitingPlayerStart;
                Debug.Log("[HERO DEATH] Hero HP reached 0");
                Debug.Log("[BATTLE STATE] CombatActive -> AwaitingPlayerStart");
                Debug.Log("[BattleManager] Hero has fallen! Entering HeroDead / AwaitingPlayerStart state.");
                StopAllCombatants();
                EventBus.RaiseBattleStateChanged(BattleState.AwaitingPlayerStart);
                return;
            }

            if (!IsBattleActive && CurrentBattleState != BattleState.InProgress) return;

            if (entity is Monster monster)
            {
                // Strict encounter membership check BEFORE _processedDeaths.Add, GenerateDrop, enqueue or retarget
                if (!activeMonsters.Contains(monster)) return;

                // Guard duplicate death processing
                if (!_processedDeaths.Add(monster)) return;

                Debug.Log($"[P05.7.1] MonsterDeath ({monster.EntityName})");

                // Generate drop no more than once
                var dropSys = WuxiaGame.Drop.DropSystem.Instance != null ? WuxiaGame.Drop.DropSystem.Instance : Object.FindAnyObjectByType<WuxiaGame.Drop.DropSystem>();
                WuxiaGame.Items.EquipmentInstance droppedItem = null;
                if (dropSys != null)
                {
                    droppedItem = dropSys.GenerateDrop(monster);
                }

                // Enqueue only non-null drop
                if (droppedItem != null)
                {
                    _pendingLootQueue.Enqueue(droppedItem);
                    Debug.Log($"[LOOT ENQUEUED] Item={droppedItem.ItemName}, QueueCount={_pendingLootQueue.Count}");
                }

                // Keep dead monsters in encounter registry so registration order indices of remaining monsters never change
                Monster nextLiving = GetNextLivingMonster();

                if (nextLiving != null)
                {
                    // Non-final death: encounter remains active
                    currentMonster = nextLiving;
                    if (currentHero != null)
                    {
                        currentHero.SetCurrentTarget(nextLiving);
                    }
                    if (nextLiving.CurrentTarget == null || !nextLiving.CurrentTarget.IsAlive)
                    {
                        nextLiving.SetCurrentTarget(currentHero);
                    }
                    Debug.Log($"[COMBAT RETARGET] Monster defeated. Hero retargeted to {nextLiving.EntityName}. Auto-combat continues. Living remaining: {GetLivingMonsterCount()}");
                }
                else if (IsWaveFullyDefeated())
                {
                    // Final death: all registered encounter monsters are dead
                    if (_lastCompletedWaveId != _currentWaveId)
                    {
                        _lastCompletedWaveId = _currentWaveId;
                        _completedNormalWaveCount++;
                        Debug.Log($"[WAVE COMPLETE] Wave #{_currentWaveId} fully defeated! CompletedNormalWaveCount={_completedNormalWaveCount}, NextWaveMultiplier={NextWaveStatMultiplier:F6}");
                    }

                    CurrentBattleState = BattleState.MonsterDead;
                    EventBus.RaiseBattleStateChanged(BattleState.MonsterDead);
                    StopAllCombatants();

                    // Cancel any previous transition and track ALL living combatants / supported casters belonging to encounter
                    CancelEncounterTransition();
                    _finishingEncounterIndex = EncounterIndex;

                    var allEntities = Object.FindObjectsByType<Entity>(FindObjectsSortMode.None);
                    for (int i = 0; i < allEntities.Length; i++)
                    {
                        var ent = allEntities[i];
                        if (ent != null && ent.gameObject.activeInHierarchy && ent.enabled && ent.IsAlive &&
                            ent.IsCasting && ent.CastState != null && ent.CastState.IsActive && ent.CastState.ActiveRequest != null)
                        {
                            if (ent.EntityType != EntityType.Monster)
                            {
                                _finishingCasters[ent] = ent.CastState.ActiveRequest;
                            }
                        }
                    }

                    if (Application.isPlaying)
                    {
                        if (_finishingCasters.Count > 0)
                        {
                            IsBattleActive = false;
                            _activeEncounterTransitionCoroutine = StartCoroutine(WaitForFinishingExecutionsThenProceed(EncounterIndex, _encounterTransitionCounter));
                        }
                        else
                        {
                            if (_pendingLootQueue.Count > 0)
                            {
                                pendingLootItem = _pendingLootQueue.Dequeue();
                                IsBattleActive = false;
                                CurrentBattleState = BattleState.LootPending;
                                Debug.Log($"[LOOT] Equipment dropped: {pendingLootItem.ItemName}. Entering LOOT_PENDING. Remaining: {_pendingLootQueue.Count}");
                                Debug.Log("[P05.7.1] LootPending Entered");
                                EventBus.RaiseBattleStateChanged(BattleState.LootPending);
                                EventBus.RaiseLootDecisionRequested(pendingLootItem);
                            }
                            else
                            {
                                CurrentBattleState = BattleState.EncounterTransition;
                                EventBus.RaiseBattleStateChanged(BattleState.EncounterTransition);
                                IsBattleActive = false;
                                _activeEncounterTransitionCoroutine = StartCoroutine(DeferEncounterAdvanceWithoutLoot(EncounterIndex, _encounterTransitionCounter));
                            }
                        }
                    }
                    else
                    {
                        if (_pendingLootQueue.Count > 0)
                        {
                            pendingLootItem = _pendingLootQueue.Dequeue();
                            IsBattleActive = false;
                            CurrentBattleState = BattleState.LootPending;
                            Debug.Log($"[LOOT] Equipment dropped: {pendingLootItem.ItemName}. Entering LOOT_PENDING. Remaining: {_pendingLootQueue.Count}");
                            Debug.Log("[P05.7.1] LootPending Entered");
                            EventBus.RaiseBattleStateChanged(BattleState.LootPending);
                            EventBus.RaiseLootDecisionRequested(pendingLootItem);
                        }
                        else
                        {
                            // In Edit Mode without loot, transition battle state without synchronously mutating encounter/targets inside death callback stack
                            CurrentBattleState = BattleState.EncounterTransition;
                            EventBus.RaiseBattleStateChanged(BattleState.EncounterTransition);
                            IsBattleActive = false;
                        }
                    }
                }
            }
        }

        public bool CompleteLootDecisionAndResume(bool equip = false, bool dismantle = false)
        {
            if (pendingLootItem == null)
            {
                if (_pendingLootQueue.Count > 0)
                {
                    pendingLootItem = _pendingLootQueue.Dequeue();
                }
                else
                {
                    Debug.LogWarning("[LOOT TRANSACTION INVALID] CompleteLootDecision called but no pending loot item!");
                    return false;
                }
            }

            WuxiaGame.Items.EquipmentInstance item = pendingLootItem;
            pendingLootItem = null;

            if (currentHero == null)
            {
                currentHero = Object.FindAnyObjectByType<Hero>();
            }

            if (equip)
            {
                Debug.Log("[P05.7.1] Equip Started");
                var eqMgr = WuxiaGame.Equipment.EquipmentManager.Instance != null ? WuxiaGame.Equipment.EquipmentManager.Instance : Object.FindAnyObjectByType<WuxiaGame.Equipment.EquipmentManager>();
                if (eqMgr != null)
                {
                    eqMgr.Equip(item);
                }
                if (currentHero != null)
                {
                    currentHero.RecalculateStats(true);
                }
                Debug.Log("[P05.7.1] Equip Completed");
            }
            else if (dismantle)
            {
                Debug.Log("[P05.7.1] Dismantle Started");
                var resMgr = WuxiaGame.Progression.ResourceManager.Instance != null ? WuxiaGame.Progression.ResourceManager.Instance : Object.FindAnyObjectByType<WuxiaGame.Progression.ResourceManager>();
                if (resMgr != null)
                {
                    resMgr.DismantleEquipment(item);
                }
                Debug.Log("[P05.7.1] Dismantle Completed");
            }

            EventBus.RaiseLootDecisionCompleted(item);
            Debug.Log("[P05.7.1] LootDecision Completed");

            // Hero Death check: if Hero died during or prior to transaction completion, do NOT resume combat automatically
            if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
            {
                CancelLootLifecycle();
                IsBattleActive = false;
                CurrentBattleState = BattleState.AwaitingPlayerStart;
                StopAllCombatants();
                EventBus.RaiseBattleStateChanged(BattleState.AwaitingPlayerStart);
                Debug.Log("[BattleManager] Hero is dead. Staying in AwaitingPlayerStart state after loot decision.");
                return true;
            }

            int currentEncounter = EncounterIndex;
            int txId = ++_lootTransactionCounter;

            // Sequential loot resolution: check if more drops remain in queue
            if (_pendingLootQueue.Count > 0)
            {
                pendingLootItem = _pendingLootQueue.Dequeue();
                CurrentBattleState = BattleState.LootPending;
                Debug.Log($"[LOOT] Presenting next queued drop: {pendingLootItem.ItemName}. Remaining in queue: {_pendingLootQueue.Count}");
                EventBus.RaiseBattleStateChanged(BattleState.LootPending);
                if (Application.isPlaying && WuxiaGame.UI.Modal.ModalCoordinator.Instance != null)
                {
                    if (_activeLootLifecycleCoroutine != null) StopCoroutine(_activeLootLifecycleCoroutine);
                    _activeLootLifecycleCoroutine = StartCoroutine(DeferNextLootDecisionRequest(pendingLootItem, currentEncounter, txId));
                }
                else
                {
                    EventBus.RaiseLootDecisionRequested(pendingLootItem);
                }
                return true;
            }

            // Final drop resolved: wait for final modal completion before advancing encounter
            if (Application.isPlaying && WuxiaGame.UI.Modal.ModalCoordinator.Instance != null)
            {
                if (_activeLootLifecycleCoroutine != null) StopCoroutine(_activeLootLifecycleCoroutine);
                _activeLootLifecycleCoroutine = StartCoroutine(DeferEncounterAdvanceAfterFinalModal(currentEncounter, txId));
                return true;
            }

            AdvanceEncounterAfterLoot();
            return true;
        }

        private void AdvanceEncounterAfterLoot()
        {
            Debug.Log("[P05.7.1] Combat Resume Requested");

            CurrentBattleState = BattleState.EncounterTransition;
            EventBus.RaiseBattleStateChanged(BattleState.EncounterTransition);

            EncounterIndex++;
            PrepareAndStartNormalWave();

            Debug.Log($"[P05.7.1] Next Encounter Created");
            Debug.Log("[P05.7.1] Combat Resumed");
            Debug.Log($"[LOOT] Loot decision completed. Resuming combat into Encounter #{EncounterIndex}");
        }

        private bool isCombatPausedByUI = false;
        public bool IsCombatPausedByUI => isCombatPausedByUI;

        public void PauseCombat()
        {
            isCombatPausedByUI = true;
            IsBattleActive = false;
            StopAllCombatants();
        }

        public void ResumeCombat()
        {
            isCombatPausedByUI = false;
            if (CurrentBattleState == BattleState.InProgress || CurrentBattleState == BattleState.None || CurrentBattleState == BattleState.WaitingForPlayerCommand)
            {
                Monster targetMonster = currentMonster != null && currentMonster.IsAlive ? currentMonster : GetNextLivingMonster();
                if (currentHero != null && targetMonster != null && currentHero.IsAlive && targetMonster.IsAlive)
                {
                    currentHero.EnableEntityActions();
                    currentHero.SetCurrentTarget(targetMonster);
                    for (int i = 0; i < activeMonsters.Count; i++)
                    {
                        var m = activeMonsters[i];
                        if (m != null && m.IsAlive)
                        {
                            m.EnableEntityActions();
                            m.SetCurrentTarget(currentHero);
                        }
                    }
                    if (currentMonster != null && currentMonster.IsAlive)
                    {
                        currentMonster.EnableEntityActions();
                        currentMonster.SetCurrentTarget(currentHero);
                    }
                    IsBattleActive = true;
                    CurrentBattleState = BattleState.InProgress;
                }
            }
        }

        private void StopAllCombatants()
        {
            if (currentHero != null)
            {
                currentHero.DisableEntityActions();
                currentHero.SetCurrentTarget(null);
            }
            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m != null)
                {
                    m.DisableEntityActions();
                    m.SetCurrentTarget(null);
                }
            }
            if (currentMonster != null)
            {
                currentMonster.DisableEntityActions();
                currentMonster.SetCurrentTarget(null);
            }
        }

        private System.Collections.IEnumerator WaitForFinishingExecutionsThenProceed(int expectedEncounter, int transitionId)
        {
            var keysToRemove = new List<Entity>();

            while (_finishingCasters.Count > 0)
            {
                if (transitionId != _encounterTransitionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
                {
                    _finishingCasters.Clear();
                    yield break;
                }

                if (currentHero != null && (!currentHero.IsAlive || currentHero.Health == null || currentHero.Health.CurrentHealth <= 0f))
                {
                    _finishingCasters.Clear();
                    yield break;
                }

                keysToRemove.Clear();
                foreach (var kvp in _finishingCasters)
                {
                    var c = kvp.Key;
                    var req = kvp.Value;
                    if (c == null || !c.gameObject.activeInHierarchy || !c.enabled || !c.IsAlive || !c.IsCasting ||
                        c.CastState == null || c.CastState.ActiveRequest != req || c.CastState.IsFinished)
                    {
                        keysToRemove.Add(c);
                    }
                }

                for (int i = 0; i < keysToRemove.Count; i++)
                {
                    _finishingCasters.Remove(keysToRemove[i]);
                }

                if (_finishingCasters.Count > 0)
                {
                    yield return null;
                }
            }

            if (transitionId != _encounterTransitionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
            {
                yield break;
            }

            if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
            {
                yield break;
            }

            _finishingCasters.Clear();

            if (_pendingLootQueue.Count > 0)
            {
                pendingLootItem = _pendingLootQueue.Dequeue();
                IsBattleActive = false;
                CurrentBattleState = BattleState.LootPending;
                Debug.Log($"[LOOT] Equipment dropped: {pendingLootItem.ItemName}. Entering LOOT_PENDING. Remaining: {_pendingLootQueue.Count}");
                Debug.Log("[P05.7.1] LootPending Entered");
                EventBus.RaiseBattleStateChanged(BattleState.LootPending);
                EventBus.RaiseLootDecisionRequested(pendingLootItem);
            }
            else
            {
                CurrentBattleState = BattleState.EncounterTransition;
                EventBus.RaiseBattleStateChanged(BattleState.EncounterTransition);
                IsBattleActive = false;
                _activeEncounterTransitionCoroutine = StartCoroutine(DeferEncounterAdvanceWithoutLoot(EncounterIndex, _encounterTransitionCounter));
            }
        }

        private System.Collections.IEnumerator DeferNextLootDecisionRequest(WuxiaGame.Items.EquipmentInstance expectedItem, int expectedEncounter, int txId)
        {
            var coordinator = WuxiaGame.UI.Modal.ModalCoordinator.Instance;

            while (true)
            {
                while (coordinator != null && (coordinator.ActiveBlockingModalCount > 0 || coordinator.TotalQueuedCount > 0))
                {
                    if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
                    {
                        yield break;
                    }

                    if (currentHero != null && (!currentHero.IsAlive || currentHero.Health == null || currentHero.Health.CurrentHealth <= 0f))
                    {
                        yield break;
                    }

                    yield return null;
                }

                if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
                {
                    yield break;
                }

                if (currentHero != null && (!currentHero.IsAlive || currentHero.Health == null || currentHero.Health.CurrentHealth <= 0f))
                {
                    yield break;
                }

                if (pendingLootItem != expectedItem)
                {
                    Debug.LogWarning($"[LOOT LIFECYCLE] Pending loot item changed before request. Expected: {expectedItem?.ItemName}, Current: {pendingLootItem?.ItemName}");
                    yield break;
                }

                yield return null;

                if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
                {
                    yield break;
                }

                if (currentHero != null && (!currentHero.IsAlive || currentHero.Health == null || currentHero.Health.CurrentHealth <= 0f))
                {
                    yield break;
                }

                if (pendingLootItem != expectedItem)
                {
                    yield break;
                }

                if (coordinator != null && (coordinator.ActiveBlockingModalCount > 0 || coordinator.TotalQueuedCount > 0))
                {
                    continue;
                }

                break;
            }

            _activeLootLifecycleCoroutine = null;
            EventBus.RaiseLootDecisionRequested(expectedItem);
        }

        private System.Collections.IEnumerator DeferEncounterAdvanceAfterFinalModal(int expectedEncounter, int txId)
        {
            var coordinator = WuxiaGame.UI.Modal.ModalCoordinator.Instance;

            while (true)
            {
                while (coordinator != null && (coordinator.ActiveBlockingModalCount > 0 || coordinator.TotalQueuedCount > 0))
                {
                    if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
                    {
                        yield break;
                    }

                    if (currentHero != null && (!currentHero.IsAlive || currentHero.Health == null || currentHero.Health.CurrentHealth <= 0f))
                    {
                        yield break;
                    }

                    yield return null;
                }

                if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
                {
                    yield break;
                }

                if (currentHero != null && (!currentHero.IsAlive || currentHero.Health == null || currentHero.Health.CurrentHealth <= 0f))
                {
                    yield break;
                }

                yield return null;

                if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
                {
                    yield break;
                }

                if (currentHero != null && (!currentHero.IsAlive || currentHero.Health == null || currentHero.Health.CurrentHealth <= 0f))
                {
                    yield break;
                }

                if (coordinator != null && (coordinator.ActiveBlockingModalCount > 0 || coordinator.TotalQueuedCount > 0))
                {
                    continue;
                }

                break;
            }

            _activeLootLifecycleCoroutine = null;
            AdvanceEncounterAfterLoot();
        }

        private System.Collections.IEnumerator DeferEncounterAdvanceWithoutLoot(int expectedEncounter, int transitionId)
        {
            yield return null;

            if (transitionId != _encounterTransitionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
            {
                yield break;
            }

            if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
            {
                yield break;
            }

            _activeEncounterTransitionCoroutine = null;
            EndEncounterAndStartNext();
        }
    }
}
