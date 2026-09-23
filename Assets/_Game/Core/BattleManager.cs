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
        public bool IsBattleActive { get; private set; }
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
            EstablishCanonicalEncounterMonsters();
        }

        public void EstablishCanonicalEncounterMonsters()
        {
            if (activeMonsters.Count > 0) return;

            // 1. Spawn/obtain primary monster
            Monster primary = currentMonster;
            if (primary == null)
            {
                var pObj = GameObject.Find("Monster_Wild");
                if (pObj != null) primary = pObj.GetComponent<Monster>();
            }
            if (primary != null)
            {
                RegisterMonster(primary);
            }

            // 2. Spawn/obtain secondary monster
            var sObj = GameObject.Find("Monster_Wild_2");
            Monster secondary = sObj != null ? sObj.GetComponent<Monster>() : null;
            if (secondary != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying || UnityEditor.SessionState.GetBool("RunP08PlayModeScenario", false))
                {
                    sObj.SetActive(true);
                    RegisterMonster(secondary);
                }
                else
                {
                    sObj.SetActive(false);
                }
#else
                RegisterMonster(secondary);
#endif
            }
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

        public void StartBattle()
        {
            CancelLootLifecycle();
            if (Instance == null) Instance = this;
            LoadConfigsIfMissing();
            IsBattleActive = true;
            EventBus.OnEntityDied -= HandleEntityDied;
            EventBus.OnEntityDied += HandleEntityDied;

            if (currentHero == null)
            {
                currentHero = Object.FindAnyObjectByType<Hero>();
            }

            // Establish explicit canonical encounter registration order
            if (activeMonsters.Count == 0)
            {
                EstablishCanonicalEncounterMonsters();

                if (activeMonsters.Count == 0)
                {
                    Monster primary = SpawnMonster(position: new Vector3(2.2f, -0.3f, 0f));
#if UNITY_EDITOR
                    if (Application.isPlaying || UnityEditor.SessionState.GetBool("RunP08PlayModeScenario", false))
                    {
                        Monster secondary = SpawnMonster(position: new Vector3(3.8f, -0.3f, 0f));
                    }
#endif
                }
            }

            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                string role = i == 0 ? "primary" : (i == 1 ? "secondary" : $"monster_{i}");
                Debug.Log($"RegistrationOrder[{i}] = {role} ({(m != null ? m.EntityName : "null")})");
            }

            if (currentMonster == null && activeMonsters.Count > 0)
            {
                currentMonster = activeMonsters[0];
            }

            if (currentHero != null)
            {
                currentHero.EnableEntityActions();
                if (currentMonster != null) currentHero.SetCurrentTarget(currentMonster);
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

            Debug.Log($"[REAL PLAY TEST]\nEncounter #{EncounterIndex} START\nHero HP: {(currentHero != null && currentHero.Health != null ? currentHero.Health.CurrentHealth.ToString("F2") : "1000.00")}\nMonster HP: {(currentMonster != null && currentMonster.Health != null ? currentMonster.Health.CurrentHealth.ToString("F2") : "500.00")}");
            Debug.Log($"[ENCOUNTER] Monster #{EncounterIndex} Started");
            if (currentMonster != null && currentMonster.Health != null)
            {
                Debug.Log($"[HP] Monster HP: {currentMonster.Health.CurrentHealth:F2} / {currentMonster.Health.MaxHealth:F2}");
            }
            if (currentHero != null && currentHero.Health != null)
            {
                Debug.Log($"[HP] Hero HP: {currentHero.Health.CurrentHealth:F2} / {currentHero.Health.MaxHealth:F2}");
            }

            CurrentBattleState = BattleState.InProgress;
            EventBus.RaiseBattleStateChanged(BattleState.InProgress);
            Debug.Log("[BattleManager] Battle Started!");
        }

        public Monster SpawnMonster(MonsterConfigSO config = null, Vector3? position = null)
        {
            LoadConfigsIfMissing();
            MonsterConfigSO cfg = config != null ? config : defaultMonsterConfig;
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
            float curHp = monster.Health != null ? monster.Health.CurrentHealth : 0f;
            float maxHp = monster.Health != null ? monster.Health.MaxHealth : 0f;

            Debug.Log($"[MONSTER LIFECYCLE]\nSpawned:\nEncounterIndex={EncounterIndex}\nHP={curHp:F0}/{maxHp:F0}\nTarget={(monster.CurrentTarget != null ? monster.CurrentTarget.EntityName : "None")}\nCombatReady={monster.CombatReady}");
            if (currentHero != null)
            {
                Debug.Log($"[MONSTER TARGET]\nMonster #{EncounterIndex} -> Hero\nTargetValid: {monster.CurrentTarget != null && monster.CurrentTarget.IsAlive}");
            }

            Debug.Log($"[COMBAT] Monster #{EncounterIndex} SPAWNED");
            if (currentHero != null)
            {
                Debug.Log($"[COMBAT] Monster #{EncounterIndex} Target = {currentHero.EntityName}");
            }
            Debug.Log($"[COMBAT] Monster #{EncounterIndex} Combat State = READY");

            Debug.Log($"[COMBAT] Monster #{EncounterIndex} HP = {curHp:F0} / {maxHp:F0}");
            Debug.Log($"[COMBAT] CurrentTarget = Monster #{EncounterIndex}");

            EventBus.RaiseEntitySpawned(monster);
            return monster;
        }

        public void EndEncounterAndStartNext()
        {
            Debug.Log($"[ENCOUNTER] Monster #{EncounterIndex} Defeated");
            Debug.Log($"[REAL PLAY TEST]\nMonster #{EncounterIndex} DEAD");

            if (currentHero != null)
            {
                currentHero.SetCurrentTarget(null);
            }

            // Clean up dead monsters in activeMonsters
            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m != null && m.gameObject != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(m.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(m.gameObject);
                    }
                }
            }
            activeMonsters.Clear();
            _processedDeaths.Clear();
            currentMonster = null;

            EncounterIndex++;

            Monster nextMonster = SpawnMonster();

            if (currentHero != null)
            {
                currentHero.EnableEntityActions();
                currentHero.SetCurrentTarget(nextMonster);
            }

            if (nextMonster != null)
            {
                nextMonster.EnableEntityActions();
                if (currentHero != null) nextMonster.SetCurrentTarget(currentHero);
            }

            Debug.Log($"[REAL PLAY TEST]\nEncounter #{EncounterIndex} START\nHero HP: {(currentHero != null && currentHero.Health != null ? currentHero.Health.CurrentHealth.ToString("F2") : "1000.00")}\nMonster HP: {(nextMonster != null && nextMonster.Health != null ? nextMonster.Health.CurrentHealth.ToString("F2") : "500.00")}");
            Debug.Log($"[COMBAT] Encounter #{EncounterIndex} START");

            IsBattleActive = true;
            CurrentBattleState = BattleState.InProgress;
            EventBus.RaiseBattleStateChanged(BattleState.InProgress);
        }

        public void RestartBattle()
        {
            CancelLootLifecycle();
            Debug.Log("[BattleManager] Restarting combat encounter (Preserving persistent hero progression)...");

            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m != null)
                {
                    m.DisableEntityActions();
                    m.SetCurrentTarget(null);
                    if (m.gameObject != null)
                    {
                        if (Application.isPlaying) Destroy(m.gameObject);
                        else DestroyImmediate(m.gameObject);
                    }
                }
            }
            activeMonsters.Clear();
            _processedDeaths.Clear();
            _pendingLootQueue.Clear();
            pendingLootItem = null;
            currentMonster = null;

            if (currentHero != null)
            {
                currentHero.SetCurrentTarget(null);
                if (currentHero.Health != null)
                {
                    currentHero.Health.SetCurrentHealth(currentHero.Health.MaxHealth);
                }
            }

            Monster nextMonster = SpawnMonster();

            if (currentHero != null)
            {
                currentHero.EnableEntityActions();
                currentHero.SetCurrentTarget(nextMonster);
            }

            if (nextMonster != null)
            {
                nextMonster.EnableEntityActions();
                if (currentHero != null) nextMonster.SetCurrentTarget(currentHero);
            }

            Debug.Log($"[COMBAT] Encounter #{EncounterIndex} START");

            IsBattleActive = true;
            CurrentBattleState = BattleState.InProgress;
            EventBus.RaiseBattleStateChanged(BattleState.InProgress);
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

            Debug.Log("[RESUME] Resume conditions validated");
            LoadConfigsIfMissing();

            if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
            {
                currentHero.Health.Revive();
            }

            if (activeMonsters.Count == 0 || !HasLivingMonster())
            {
                for (int i = 0; i < activeMonsters.Count; i++)
                {
                    var m = activeMonsters[i];
                    if (m != null && m.gameObject != null)
                    {
                        if (Application.isPlaying) Destroy(m.gameObject);
                        else DestroyImmediate(m.gameObject);
                    }
                }
                activeMonsters.Clear();
                _processedDeaths.Clear();
                currentMonster = null;

                SpawnMonster();
            }

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
            if (!IsBattleActive && CurrentBattleState != BattleState.InProgress) return;

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
            }
            else if (entity is Monster monster)
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
                    Debug.Log($"[COMBAT RETARGET] Primary defeated. Hero retargeted to {nextLiving.EntityName}. Auto-combat continues.");
                }
                else
                {
                    // Final death: all registered encounter monsters are dead
                    CurrentBattleState = BattleState.MonsterDead;
                    EventBus.RaiseBattleStateChanged(BattleState.MonsterDead);
                    StopAllCombatants();

                    // Cancel any previous transition and track living combatants currently executing an active skill/channel
                    CancelEncounterTransition();
                    _finishingEncounterIndex = EncounterIndex;

                    if (currentHero != null && currentHero.IsAlive && currentHero.IsCasting && currentHero.CastState != null && currentHero.CastState.IsActive)
                    {
                        _finishingCasters[currentHero] = currentHero.CastState.ActiveRequest;
                    }
                    for (int i = 0; i < activeMonsters.Count; i++)
                    {
                        var m = activeMonsters[i];
                        if (m != null && m.IsAlive && m.IsCasting && m.CastState != null && m.CastState.IsActive)
                        {
                            _finishingCasters[m] = m.CastState.ActiveRequest;
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
                Debug.LogWarning("[LOOT TRANSACTION INVALID] CompleteLootDecision called but no pending loot item!");
                return false;
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

            // Destroy the dead encounter monsters
            for (int i = 0; i < activeMonsters.Count; i++)
            {
                var m = activeMonsters[i];
                if (m != null && m.gameObject != null)
                {
                    if (Application.isPlaying) Destroy(m.gameObject);
                    else DestroyImmediate(m.gameObject);
                }
            }
            activeMonsters.Clear();
            _processedDeaths.Clear();
            currentMonster = null;

            // Advance encounter & spawn next monster
            EncounterIndex++;
            Monster nextMonster = SpawnMonster();

            if (currentHero != null)
            {
                currentHero.EnableEntityActions();
                currentHero.SetCurrentTarget(nextMonster);
            }

            if (nextMonster != null)
            {
                nextMonster.EnableEntityActions();
                if (currentHero != null) nextMonster.SetCurrentTarget(currentHero);
            }

            Debug.Log($"[P05.7.1] Next Encounter Created");

            IsBattleActive = true;
            CurrentBattleState = BattleState.InProgress;
            EventBus.RaiseBattleStateChanged(BattleState.InProgress);
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

            while (coordinator != null && (coordinator.ActiveBlockingModalCount > 0 || coordinator.TotalQueuedCount > 0))
            {
                if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
                {
                    yield break;
                }

                if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
                {
                    yield break;
                }

                yield return null;
            }

            if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
            {
                yield break;
            }

            if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
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

            if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
            {
                yield break;
            }

            if (pendingLootItem != expectedItem)
            {
                yield break;
            }

            if (coordinator != null && (coordinator.ActiveBlockingModalCount > 0 || coordinator.TotalQueuedCount > 0))
            {
                yield break;
            }

            _activeLootLifecycleCoroutine = null;
            EventBus.RaiseLootDecisionRequested(expectedItem);
        }

        private System.Collections.IEnumerator DeferEncounterAdvanceAfterFinalModal(int expectedEncounter, int txId)
        {
            var coordinator = WuxiaGame.UI.Modal.ModalCoordinator.Instance;

            while (coordinator != null && (coordinator.ActiveBlockingModalCount > 0 || coordinator.TotalQueuedCount > 0))
            {
                if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
                {
                    yield break;
                }

                if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
                {
                    yield break;
                }

                yield return null;
            }

            if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
            {
                yield break;
            }

            if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
            {
                yield break;
            }

            yield return null;

            if (txId != _lootTransactionCounter || expectedEncounter != EncounterIndex || CurrentBattleState == BattleState.AwaitingPlayerStart)
            {
                yield break;
            }

            if (currentHero != null && (!currentHero.IsAlive || currentHero.Health.CurrentHealth <= 0f))
            {
                yield break;
            }

            if (coordinator != null && (coordinator.ActiveBlockingModalCount > 0 || coordinator.TotalQueuedCount > 0))
            {
                yield break;
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
