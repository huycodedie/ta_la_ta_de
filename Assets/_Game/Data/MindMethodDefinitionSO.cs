using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Stats;

namespace WuxiaGame.Data
{
    [Serializable]
    public class MindMethodPassiveData
    {
        [Header("Stat Modifiers")]
        [SerializeField] private float bonusMaxHealth = 0f;
        [SerializeField] private float bonusAttack = 0f;
        [SerializeField] private float bonusDefense = 0f;
        [SerializeField] private float bonusCritRate = 0f;
        [SerializeField] private float bonusCritDamage = 0f;
        [SerializeField] private float bonusDodge = 0f;
        [SerializeField] private float bonusMoveSpeed = 0f;

        [Header("Rage Modifiers")]
        [SerializeField] private float basicAttackRageModifier = 0f;
        [SerializeField] private float damageTakenRageModifier = 0f;
        [SerializeField] private float rageGainMultiplier = 1.0f;

        [Header("Special Passives")]
        [SerializeField] private bool hasReviveOnce = false;
        [SerializeField] private float reviveHealthPercent = 30f;

        public float BonusMaxHealth => bonusMaxHealth;
        public float BonusAttack => bonusAttack;
        public float BonusDefense => bonusDefense;
        public float BonusCritRate => bonusCritRate;
        public float BonusCritDamage => bonusCritDamage;
        public float BonusDodge => bonusDodge;
        public float BonusMoveSpeed => bonusMoveSpeed;

        public float BasicAttackRageModifier => basicAttackRageModifier;
        public float DamageTakenRageModifier => damageTakenRageModifier;
        public float RageGainMultiplier => rageGainMultiplier;

        public bool HasReviveOnce => hasReviveOnce;
        public float ReviveHealthPercent => reviveHealthPercent;

        public MindMethodPassiveData() { }

        public MindMethodPassiveData(
            float hp, float atk, float def, float critRate, float critDmg, float dodge, float moveSpeed,
            float basicRageMod = 0f, float dmgRageMod = 0f, float rageMult = 1.0f,
            bool reviveOnce = false, float reviveHpPct = 30f)
        {
            bonusMaxHealth = hp;
            bonusAttack = atk;
            bonusDefense = def;
            bonusCritRate = critRate;
            bonusCritDamage = critDmg;
            bonusDodge = dodge;
            bonusMoveSpeed = moveSpeed;
            basicAttackRageModifier = basicRageMod;
            damageTakenRageModifier = dmgRageMod;
            rageGainMultiplier = rageMult;
            hasReviveOnce = reviveOnce;
            reviveHealthPercent = reviveHpPct;
        }

        public Dictionary<StatType, float> GetStatModifiers(int level = 1)
        {
            Dictionary<StatType, float> dict = new Dictionary<StatType, float>();
            float lvlMult = Mathf.Max(1, level);

            if (bonusMaxHealth != 0f) dict[StatType.MaxHealth] = bonusMaxHealth * lvlMult;
            if (bonusAttack != 0f) dict[StatType.Attack] = bonusAttack * lvlMult;
            if (bonusDefense != 0f) dict[StatType.Defense] = bonusDefense * lvlMult;
            if (bonusCritRate != 0f) dict[StatType.CritRate] = bonusCritRate;
            if (bonusCritDamage != 0f) dict[StatType.CritDamage] = bonusCritDamage;
            if (bonusDodge != 0f) dict[StatType.Dodge] = bonusDodge;
            if (bonusMoveSpeed != 0f) dict[StatType.MoveSpeed] = bonusMoveSpeed;

            return dict;
        }
    }

    [Serializable]
    public class MindMethodUnlockRequirement
    {
        [SerializeField] private BreakthroughRequirementType type = BreakthroughRequirementType.HeroLevel;
        [SerializeField] private int requiredValue = 1;
        [SerializeField] private string targetId = "";
        [SerializeField] private string description = "";

        public BreakthroughRequirementType Type => type;
        public int RequiredValue => requiredValue;
        public string TargetId => targetId;
        public string Description => description;

        public MindMethodUnlockRequirement() { }

        public MindMethodUnlockRequirement(BreakthroughRequirementType reqType, int value, string target = "", string desc = "")
        {
            type = reqType;
            requiredValue = value;
            targetId = target;
            description = desc;
        }
    }

    [CreateAssetMenu(fileName = "MindMethodDefinition", menuName = "WuxiaGame/Data/MindMethodDefinition")]
    public class MindMethodDefinitionSO : ScriptableObject
    {
        [Header("Meta")]
        [SerializeField] private string mindMethodId = "mm_taiji";
        [SerializeField] private string mindMethodName = "Thái Cực Thần Công";
        [SerializeField] [TextArea(2, 4)] private string description = "";
        [SerializeField] private Sprite icon;
        [SerializeField] private int maxLevel = 10;
        [SerializeField] private bool isUnlockedByDefault = false;

        [Header("Unlock Conditions")]
        [SerializeField] private List<MindMethodUnlockRequirement> unlockConditions = new List<MindMethodUnlockRequirement>();

        [Header("Passive Data")]
        [SerializeField] private MindMethodPassiveData passiveData = new MindMethodPassiveData();

        [Header("Skills (Exclusive to this Mind Method)")]
        [SerializeField] private List<SkillDefinitionSO> skills = new List<SkillDefinitionSO>();

        public string MindMethodId => mindMethodId;
        public string MindMethodName => mindMethodName;
        public string Description => description;
        public Sprite Icon => icon;
        public int MaxLevel => maxLevel;
        public bool IsUnlockedByDefault => isUnlockedByDefault;
        public IReadOnlyList<MindMethodUnlockRequirement> UnlockConditions => unlockConditions;
        public MindMethodPassiveData PassiveData => passiveData;
        public IReadOnlyList<SkillDefinitionSO> Skills => skills;

        public void InitializeMindMethod(
            string id,
            string name,
            string desc,
            int maxLvl,
            bool unlockedByDefault,
            MindMethodPassiveData passive,
            List<MindMethodUnlockRequirement> conditions = null,
            List<SkillDefinitionSO> skillList = null)
        {
            mindMethodId = id;
            mindMethodName = name;
            description = desc;
            maxLevel = maxLvl;
            isUnlockedByDefault = unlockedByDefault;
            passiveData = passive ?? new MindMethodPassiveData();
            unlockConditions = conditions != null ? new List<MindMethodUnlockRequirement>(conditions) : new List<MindMethodUnlockRequirement>();
            skills = skillList != null ? new List<SkillDefinitionSO>(skillList) : new List<SkillDefinitionSO>();
        }

        public List<SkillDefinitionSO> GetSkillsForSlot(SkillSlotType slot)
        {
            List<SkillDefinitionSO> result = new List<SkillDefinitionSO>();
            if (skills != null)
            {
                foreach (var s in skills)
                {
                    if (s != null && s.SlotType == slot)
                    {
                        result.Add(s);
                    }
                }
            }
            return result;
        }

        public SkillDefinitionSO GetDefaultSkillForSlot(SkillSlotType slot)
        {
            var list = GetSkillsForSlot(slot);
            return (list != null && list.Count > 0) ? list[0] : null;
        }
    }
}
