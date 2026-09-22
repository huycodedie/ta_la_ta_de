using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities.Components;
using WuxiaGame.Equipment;
using WuxiaGame.Items;
using WuxiaGame.Stats;

namespace WuxiaGame.Entities
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(RageComponent))]
    [RequireComponent(typeof(MovementComponent))]
    [RequireComponent(typeof(AttackComponent))]
    public class Hero : Entity
    {
        [SerializeField] private HeroConfigSO heroConfig;
        [SerializeField] private CombatConfigSO combatConfig;
        [SerializeField] private HeroProgressionConfigSO progressionConfig;

        private readonly Dictionary<StatType, float> baseStats = new Dictionary<StatType, float>();

        public HeroConfigSO HeroConfig => heroConfig;
        public CombatConfigSO CombatConfig => combatConfig;
        public HeroProgressionConfigSO ProgressionConfig => progressionConfig;
        public IReadOnlyDictionary<StatType, float> BaseStats => baseStats;
        
        public float GetBaseStat(StatType stat) => baseStats.TryGetValue(stat, out float val) ? val : 0f;
        public TitleBreakthroughDatabaseSO GetBreakthroughProfile() => heroConfig != null ? heroConfig.BreakthroughProfile : null;

        public void SetHeroConfig(HeroConfigSO cfg)
        {
            heroConfig = cfg;
            EnsureDefaultBaseStats();
        }

        private WuxiaGame.Combat.HeroSkillDecisionController skillDecisionController;
        public WuxiaGame.Combat.HeroSkillDecisionController SkillDecisionController
        {
            get
            {
                if (skillDecisionController == null)
                {
                    skillDecisionController = GetComponent<WuxiaGame.Combat.HeroSkillDecisionController>();
                    if (skillDecisionController == null && gameObject != null)
                    {
                        skillDecisionController = gameObject.AddComponent<WuxiaGame.Combat.HeroSkillDecisionController>();
                        skillDecisionController.Initialize(this);
                    }
                }
                return skillDecisionController;
            }
        }

        protected override void Awake()
        {
            entityType = EntityType.Hero;
            base.Awake();
            LoadConfigsIfMissing();
            EnsureDefaultBaseStats();
            if (skillDecisionController == null)
            {
                var _ = SkillDecisionController;
            }
        }

        private void OnEnable()
        {
            EventBus.OnEquipmentEquipped += HandleEquipmentEquipped;
            EventBus.OnTitleChanged += HandleTitleChanged;
            EventBus.OnHeroLevelUp += HandleHeroLevelUp;
            EventBus.OnActiveMindMethodChanged += HandleActiveMindMethodChanged;
            EventBus.OnMindMethodLevelChanged += HandleMindMethodLevelChanged;
        }

        private void OnDisable()
        {
            EventBus.OnEquipmentEquipped -= HandleEquipmentEquipped;
            EventBus.OnTitleChanged -= HandleTitleChanged;
            EventBus.OnHeroLevelUp -= HandleHeroLevelUp;
            EventBus.OnActiveMindMethodChanged -= HandleActiveMindMethodChanged;
            EventBus.OnMindMethodLevelChanged -= HandleMindMethodLevelChanged;
        }

        private void OnDestroy()
        {
            EventBus.OnEquipmentEquipped -= HandleEquipmentEquipped;
            EventBus.OnTitleChanged -= HandleTitleChanged;
            EventBus.OnHeroLevelUp -= HandleHeroLevelUp;
            EventBus.OnActiveMindMethodChanged -= HandleActiveMindMethodChanged;
            EventBus.OnMindMethodLevelChanged -= HandleMindMethodLevelChanged;
        }

        private void Start()
        {
            InitializeHero();
            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.RegisterHero(this);
            }
            EventBus.RaiseEntitySpawned(this);
        }

        public void LoadConfigsIfMissing()
        {
            if (heroConfig == null)
            {
                heroConfig = Resources.Load<HeroConfigSO>("Data/HeroConfig");
#if UNITY_EDITOR
                if (heroConfig == null)
                {
                    heroConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroConfigSO>("Assets/_Game/Data/HeroConfig.asset");
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

            if (progressionConfig == null)
            {
                progressionConfig = Resources.Load<HeroProgressionConfigSO>("Data/HeroProgressionConfig");
#if UNITY_EDITOR
                if (progressionConfig == null)
                {
                    progressionConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<HeroProgressionConfigSO>("Assets/_Game/Data/HeroProgressionConfig.asset");
                }
#endif
            }
        }

        [SerializeField] private TitleConfigSO currentTitleConfig;

        public TitleConfigSO CurrentTitleConfig => currentTitleConfig;

        private void EnsureDefaultBaseStats()
        {
            if (baseStats.Count == 0)
            {
                ApplyBaseStatsFromTitle(currentTitleConfig, false);
            }
        }

        public void InitializeHero(HeroConfigSO config = null, CombatConfigSO globalCombat = null, HeroProgressionConfigSO progConfig = null, TitleConfigSO titleConfig = null)
        {
            if (config != null) heroConfig = config;
            if (globalCombat != null) combatConfig = globalCombat;
            if (progConfig != null) progressionConfig = progConfig;
            if (titleConfig != null) currentTitleConfig = titleConfig;

            LoadConfigsIfMissing();

            if (heroConfig == null)
            {
                Debug.LogWarning($"[Hero] {gameObject.name} missing HeroConfigSO!");
                return;
            }

            entityType = EntityType.Hero;
            entityName = heroConfig.HeroName;
            attackRange = heroConfig.AttackRange;

            // Initialize Base Stats from Title/HeroConfig
            ApplyBaseStatsFromTitle(currentTitleConfig, false);

            // Initialize Components
            if (Health != null) Health.InitializeHealth(Stats.GetValue(StatType.MaxHealth), this);
            float startRage = heroConfig != null ? heroConfig.InitialRage : (combatConfig != null ? combatConfig.InitialRage : 100f);
            if (Rage != null) Rage.InitializeRage(heroConfig != null ? heroConfig.MaxRage : 100f, startRage, this);
            if (Movement != null) Movement.InitializeSpeed(heroConfig.MovementSpeed, this);
            if (Attack != null) Attack.Initialize(heroConfig.AttackInterval, combatConfig, this);

            EventBus.OnEquipmentEquipped -= HandleEquipmentEquipped;
            EventBus.OnEquipmentEquipped += HandleEquipmentEquipped;
            EventBus.OnTitleChanged -= HandleTitleChanged;
            EventBus.OnTitleChanged += HandleTitleChanged;
            EventBus.OnHeroLevelUp -= HandleHeroLevelUp;
            EventBus.OnHeroLevelUp += HandleHeroLevelUp;
        }

        public void ApplyBaseStatsFromTitle(TitleConfigSO title, bool preserveHpDelta = false)
        {
            LoadConfigsIfMissing();

            if (title != null)
            {
                currentTitleConfig = title;
            }

            float hpBase = currentTitleConfig != null ? currentTitleConfig.BaseMaxHealth : (heroConfig != null ? heroConfig.MaxHealth : 1000f);
            float atkBase = currentTitleConfig != null ? currentTitleConfig.BaseAttack : (heroConfig != null ? heroConfig.Attack : 100f);
            float defBase = currentTitleConfig != null ? currentTitleConfig.BaseDefense : (heroConfig != null ? heroConfig.Defense : 20f);

            baseStats[StatType.MaxHealth] = hpBase;
            baseStats[StatType.Health] = hpBase;
            baseStats[StatType.Attack] = atkBase;
            baseStats[StatType.Defense] = defBase;
            baseStats[StatType.MoveSpeed] = heroConfig != null ? heroConfig.MovementSpeed : 3.5f;
            baseStats[StatType.AttackInterval] = heroConfig != null ? heroConfig.AttackInterval : 1.0f;
            baseStats[StatType.MaxRage] = heroConfig != null ? heroConfig.MaxRage : 100f;
            baseStats[StatType.CritRate] = heroConfig != null && heroConfig.CritRate >= 0 ? heroConfig.CritRate : (combatConfig ? combatConfig.DefaultCritRate : 5f);
            baseStats[StatType.CritDamage] = heroConfig != null && heroConfig.CritDamage >= 0 ? heroConfig.CritDamage : (combatConfig ? combatConfig.DefaultCritDamage : 150f);
            baseStats[StatType.ComboRate] = 0f;
            baseStats[StatType.CounterRate] = 0f;
            baseStats[StatType.Dodge] = 0f;
            baseStats[StatType.Lifesteal] = 0f;

            RecalculateStats(preserveHpDelta);
        }

        public void ApplyBaseStatsFromBreakthrough(TitleBreakthroughConfigSO breakthrough, bool preserveHpDelta = false)
        {
            LoadConfigsIfMissing();

            float hpBase = breakthrough != null ? breakthrough.BaseMaxHealth : (heroConfig != null ? heroConfig.MaxHealth : 1000f);
            float atkBase = breakthrough != null ? breakthrough.BaseAttack : (heroConfig != null ? heroConfig.Attack : 100f);
            float defBase = breakthrough != null ? breakthrough.BaseDefense : (heroConfig != null ? heroConfig.Defense : 20f);
            float defaultCritRate = heroConfig != null && heroConfig.CritRate >= 0 ? heroConfig.CritRate : (combatConfig ? combatConfig.DefaultCritRate : 5f);
            float defaultCritDmg = heroConfig != null && heroConfig.CritDamage >= 0 ? heroConfig.CritDamage : (combatConfig ? combatConfig.DefaultCritDamage : 150f);

            baseStats[StatType.MaxHealth] = hpBase;
            baseStats[StatType.Health] = hpBase;
            baseStats[StatType.Attack] = atkBase;
            baseStats[StatType.Defense] = defBase;
            baseStats[StatType.MoveSpeed] = heroConfig != null ? heroConfig.MovementSpeed : 3.5f;
            baseStats[StatType.AttackInterval] = heroConfig != null ? heroConfig.AttackInterval : 1.0f;
            baseStats[StatType.MaxRage] = heroConfig != null ? heroConfig.MaxRage : 100f;
            baseStats[StatType.CritRate] = defaultCritRate + (breakthrough != null ? breakthrough.BonusCritRate : 0f);
            baseStats[StatType.CritDamage] = defaultCritDmg + (breakthrough != null ? breakthrough.BonusCritDamage : 0f);
            baseStats[StatType.ComboRate] = 0f;
            baseStats[StatType.CounterRate] = 0f;
            baseStats[StatType.Dodge] = breakthrough != null ? breakthrough.BonusDodge : 0f;
            baseStats[StatType.Lifesteal] = 0f;

            RecalculateStats(preserveHpDelta);
        }

        public void ApplyLevelBaseStats(int level, bool isLevelUp = false)
        {
            // Level change does NOT modify Base Stats. Base stats derive from Title / Rank Breakthrough.
            RecalculateStats(isLevelUp);
        }

        public void ApplyTitleStats(TitleConfigSO title)
        {
            ApplyBaseStatsFromTitle(title, true);
        }

        public override void RecalculateStats(bool preserveHpDelta = true)
        {
            EquipmentManager eqMgr = EquipmentManager.Instance != null ? EquipmentManager.Instance : UnityEngine.Object.FindAnyObjectByType<EquipmentManager>();
            Dictionary<StatType, float> mmStats = WuxiaGame.Progression.MindMethodManager.Instance != null ? WuxiaGame.Progression.MindMethodManager.Instance.GetActivePassiveStats() : null;
            Dictionary<StatType, float> buffStats = StatusController != null ? StatusController.GetActiveBuffModifiers() : null;
            Dictionary<StatType, float> debuffStats = StatusController != null ? StatusController.GetActiveDebuffModifiers() : null;
            Dictionary<StatType, float> finalStats = StatAggregator.CalculateFinalStats(baseStats, eqMgr, mmStats, buffStats, debuffStats);

            foreach (var kvp in finalStats)
            {
                Stats.SetStat(kvp.Key, kvp.Value);
            }

            if (Health != null && Stats.Container.HasStat(StatType.MaxHealth))
            {
                float maxHp = Stats.GetValue(StatType.MaxHealth);
                Health.UpdateMaxHealth(maxHp, preserveHpDelta);
            }

            if (Movement != null && Stats.Container.HasStat(StatType.MoveSpeed))
            {
                Movement.InitializeSpeed(Mathf.Max(0.1f, Stats.GetValue(StatType.MoveSpeed)), this);
            }
        }

        private void HandleEquipmentEquipped(EquipmentSlotType slot, EquipmentInstance item)
        {
            if (this == null || gameObject == null) return;
            RecalculateStats(true);
        }

        private void HandleTitleChanged(TitleConfigSO title)
        {
            if (this == null || gameObject == null) return;
            ApplyTitleStats(title);
        }

        private void HandleHeroLevelUp(int newLevel, int levelDelta, Dictionary<StatType, float> statDeltas)
        {
            if (this == null || gameObject == null) return;
            // Level change does not change base stats
            RecalculateStats(true);
        }

        private void HandleActiveMindMethodChanged(MindMethodDefinitionSO newMindMethod)
        {
            if (this == null || gameObject == null) return;
            RecalculateStats(true);
        }

        private void HandleMindMethodLevelChanged(string mindMethodId, int newLevel)
        {
            if (this == null || gameObject == null) return;
            RecalculateStats(true);
        }

        #region Skill Execution (Prototype 07.1)

        public SkillExecutionResult ExecuteSelectedSkill(SkillSlotType slot, Entity target = null)
        {
            if (this == null || !IsAlive)
            {
                var fail = SkillExecutionResult.CreateFailure(
                    null,
                    SkillExecutionFailureReason.SourceInvalidOrDead,
                    "Hero is null or dead.");
                EventBus.RaiseSkillExecutionFailed(null, fail);
                return fail;
            }

            var mmMgr = WuxiaGame.Progression.MindMethodManager.Instance;
            SkillDefinitionSO skillDef = mmMgr != null ? mmMgr.GetSelectedSkillForSlot(slot) : null;

            if (target == null)
            {
                target = (CurrentTarget != null && CurrentTarget.IsAlive)
                    ? CurrentTarget
                    : new NearestEnemyTargetResolver().ResolveTarget(this);
                if (target != null) SetCurrentTarget(target);
            }

            var request = new SkillExecutionRequest(this, skillDef, slot, target, combatConfig);
            return SkillExecutor.Execute(request);
        }

        public SkillExecutionResult ExecuteSkill(SkillDefinitionSO skill, Entity target = null)
        {
            if (this == null || !IsAlive)
            {
                var fail = SkillExecutionResult.CreateFailure(
                    null,
                    SkillExecutionFailureReason.SourceInvalidOrDead,
                    "Hero is null or dead.");
                EventBus.RaiseSkillExecutionFailed(null, fail);
                return fail;
            }

            if (target == null)
            {
                target = (CurrentTarget != null && CurrentTarget.IsAlive)
                    ? CurrentTarget
                    : new NearestEnemyTargetResolver().ResolveTarget(this);
                if (target != null) SetCurrentTarget(target);
            }

            SkillSlotType slot = skill != null ? skill.SlotType : SkillSlotType.NormalAttack;
            var request = new SkillExecutionRequest(this, skill, slot, target, combatConfig);
            return SkillExecutor.Execute(request);
        }

        #endregion
    }
}
