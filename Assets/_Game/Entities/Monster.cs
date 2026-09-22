using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities.Components;
using WuxiaGame.Stats;

namespace WuxiaGame.Entities
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(MovementComponent))]
    [RequireComponent(typeof(AttackComponent))]
    public class Monster : Entity
    {
        [SerializeField] private MonsterConfigSO monsterConfig;
        [SerializeField] private CombatConfigSO combatConfig;
        private bool isInitialized = false;

        public MonsterConfigSO MonsterConfig => monsterConfig;
        public CombatConfigSO CombatConfig => combatConfig;

        public bool HasAwardedExp { get; set; } = false;
        public bool CombatReady => IsAlive && Attack != null && Attack.IsAttackEnabled && CurrentTarget != null;

        protected override void Awake()
        {
            entityType = EntityType.Monster;
            base.Awake();
        }

        private void Start()
        {
            if (!isInitialized)
            {
                InitializeMonster();
            }
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.RegisterMonster(this);
            }
            EventBus.RaiseEntitySpawned(this);
        }

        public void InitializeMonster(MonsterConfigSO config = null, CombatConfigSO globalCombat = null)
        {
            isInitialized = true;
            if (config != null) monsterConfig = config;
            if (globalCombat != null) combatConfig = globalCombat;

            HasAwardedExp = false;

            if (monsterConfig == null)
            {
                monsterConfig = Resources.Load<MonsterConfigSO>("Data/MonsterConfig");
#if UNITY_EDITOR
                if (monsterConfig == null)
                {
                    monsterConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<MonsterConfigSO>("Assets/_Game/Data/MonsterConfig.asset");
                }
#endif
            }

            if (combatConfig == null)
            {
                combatConfig = Resources.Load<CombatConfigSO>("Data/CombatConfig");
#if UNITY_EDITOR
                if (combatConfig == null)
                {
                    combatConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<CombatConfigSO>("Assets/_Game/Data/CombatConfig.asset");
                }
#endif
            }

            if (monsterConfig == null)
            {
                Debug.LogWarning($"[Monster] {gameObject.name} missing MonsterConfigSO!");
                return;
            }

            entityType = EntityType.Monster;
            entityName = monsterConfig.MonsterName;
            attackRange = monsterConfig.AttackRange;

            // Setup Stats Container
            Stats.SetStat(StatType.Health, monsterConfig.MaxHealth);
            Stats.SetStat(StatType.MaxHealth, monsterConfig.MaxHealth);
            Stats.SetStat(StatType.Attack, monsterConfig.Attack);
            Stats.SetStat(StatType.Defense, monsterConfig.Defense);
            Stats.SetStat(StatType.MoveSpeed, monsterConfig.MovementSpeed);
            Stats.SetStat(StatType.AttackInterval, monsterConfig.AttackInterval);
            Stats.SetStat(StatType.CritRate, monsterConfig.CritRate >= 0 ? monsterConfig.CritRate : (combatConfig ? combatConfig.DefaultCritRate : 1f));
            Stats.SetStat(StatType.CritDamage, monsterConfig.CritDamage >= 0 ? monsterConfig.CritDamage : (combatConfig ? combatConfig.DefaultCritDamage : 1f));
            Stats.SetStat(StatType.Dodge, 0f);

            // Initialize Components
            if (Health != null) Health.InitializeHealth(monsterConfig.MaxHealth, this);
            if (Movement != null) Movement.InitializeSpeed(monsterConfig.MovementSpeed, this);
            if (Attack != null) Attack.Initialize(monsterConfig.AttackInterval, combatConfig, this);
        }

        public override void RecalculateStats(bool preserveHpDelta = true)
        {
            if (monsterConfig == null) return;

            var critStat = Stats.Container.GetStat(StatType.CritRate);
            float baseCrit = critStat != null ? critStat.BaseValue : (monsterConfig.CritRate >= 0 ? monsterConfig.CritRate : (combatConfig ? combatConfig.DefaultCritRate : 1f));

            var dodgeStat = Stats.Container.GetStat(StatType.Dodge);
            float baseDodge = dodgeStat != null ? dodgeStat.BaseValue : 0f;

            var ccResStat = Stats.Container.GetStat(StatType.CcResistance);
            float baseCcRes = ccResStat != null ? ccResStat.BaseValue : 0f;

            var baseStats = new System.Collections.Generic.Dictionary<StatType, float>
            {
                { StatType.Health, monsterConfig.MaxHealth },
                { StatType.MaxHealth, monsterConfig.MaxHealth },
                { StatType.Attack, monsterConfig.Attack },
                { StatType.Defense, monsterConfig.Defense },
                { StatType.MoveSpeed, monsterConfig.MovementSpeed },
                { StatType.AttackInterval, monsterConfig.AttackInterval },
                { StatType.CritRate, baseCrit },
                { StatType.CritDamage, monsterConfig.CritDamage >= 0 ? monsterConfig.CritDamage : (combatConfig ? combatConfig.DefaultCritDamage : 1f) },
                { StatType.Dodge, baseDodge },
                { StatType.CcResistance, baseCcRes }
            };

            var buffStats = StatusController != null ? StatusController.GetActiveBuffModifiers() : null;
            var debuffStats = StatusController != null ? StatusController.GetActiveDebuffModifiers() : null;
            var finalStats = StatAggregator.CalculateFinalStats(baseStats, null, null, buffStats, debuffStats);

            foreach (var kvp in finalStats)
            {
                Stats.SetStat(kvp.Key, kvp.Value);
            }

            if (Movement != null && Stats.Container.HasStat(StatType.MoveSpeed))
            {
                Movement.InitializeSpeed(Mathf.Max(0.1f, Stats.GetValue(StatType.MoveSpeed)), this);
            }

            if (Attack != null && Stats.Container.HasStat(StatType.AttackInterval))
            {
                Attack.Initialize(Mathf.Max(0.1f, Stats.GetValue(StatType.AttackInterval)), combatConfig, this);
            }
        }
    }
}
