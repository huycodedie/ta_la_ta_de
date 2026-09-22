using UnityEngine;
using WuxiaGame.Data;

namespace WuxiaGame.Combat
{
    public enum StatusCategory
    {
        None = 0,
        Buff = 1,
        Debuff = 2,
        Dot = 3,
        CrowdControl = 4,
        Special = 5
    }

    public enum StatusStackPolicy
    {
        RefreshDuration = 1,
        Replace = 2,
        Stack = 3
    }

    [CreateAssetMenu(fileName = "StatusEffect", menuName = "WuxiaGame/Combat/Effects/StatusEffect")]
    public class StatusEffectDefinitionSO : ScriptableObject
    {
        [Header("Status Configuration")]
        [SerializeField] private string statusId = "status_bleed";
        [SerializeField] private string statusName = "Chảy Máu";
        [SerializeField] private float defaultDuration = 5f;
        [SerializeField] private float tickInterval = 1f;
        [SerializeField] private int maxStacks = 1;
        [SerializeField] private StatusStackPolicy stackPolicy = StatusStackPolicy.RefreshDuration;
        [SerializeField] private StatusCategory category = StatusCategory.Dot;

        [Header("Tick / DoT Configuration")]
        [SerializeField] private float damagePerTick = 0f;
        [SerializeField] private DamageType damageType = DamageType.Skill;
        [SerializeField] private CombatConfigSO combatConfig;

        [SerializeField] private EffectPowerTier powerTier = EffectPowerTier.None;
        [SerializeField] private CrowdControlType ccType = CrowdControlType.None;
        [SerializeField] private bool canShatterFreeze = false;
        [SerializeField] private int removalPriority = 0;

        public string StatusId => statusId;
        public string StatusName => statusName;
        public float DefaultDuration => defaultDuration;
        public float TickInterval => tickInterval;
        public int MaxStacks => maxStacks;
        public StatusStackPolicy StackPolicy => stackPolicy;
        public StatusCategory Category => category;
        public float DamagePerTick => damagePerTick;
        public DamageType DamageType => damageType;
        public CombatConfigSO CombatConfig => combatConfig;
        public EffectPowerTier PowerTier => powerTier;
        public CrowdControlType CcType => ccType;
        public bool CanShatterFreeze => canShatterFreeze;
        public int RemovalPriority => removalPriority > 0 ? removalPriority : (int)powerTier;

        public void SetRemovalPriority(int priority) => removalPriority = priority;

        public void Initialize(
            string id,
            string name,
            float duration = 5f,
            float interval = 1f,
            int stacks = 1,
            StatusStackPolicy policy = StatusStackPolicy.RefreshDuration,
            StatusCategory cat = StatusCategory.Dot,
            float dmgPerTick = 0f,
            DamageType dmgType = DamageType.Skill,
            CombatConfigSO config = null,
            EffectPowerTier tier = EffectPowerTier.None,
            CrowdControlType crowdControl = CrowdControlType.None,
            bool shatterFreeze = false)
        {
            statusId = id;
            statusName = name;
            defaultDuration = duration;
            tickInterval = interval;
            maxStacks = stacks;
            stackPolicy = policy;
            category = cat;
            damagePerTick = dmgPerTick;
            damageType = dmgType;
            combatConfig = config;
            powerTier = tier;
            ccType = crowdControl;
            canShatterFreeze = shatterFreeze;
        }
    }
}
