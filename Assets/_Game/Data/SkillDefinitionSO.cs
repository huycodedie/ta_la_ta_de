using System;
using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Stats;

namespace WuxiaGame.Data
{
    public enum SkillSlotType
    {
        NormalAttack = 1,   // Slot 1: Đánh thường
        Skill = 2,          // Slot 2: Tuyệt kỹ
        ExternalSkill1 = 3, // Slot 3: Ngoại công 1
        ExternalSkill2 = 4, // Slot 4: Ngoại công 2
        Ultimate = 5        // Slot 5: Thần công / Bí kỹ
    }

    public enum SkillRequirementType
    {
        HeroLevel = 0,
        MindMethodLevel = 1,
        TitleRank = 2,
        LootTier = 3,
        Equipment = 4,
        Currency = 5,
        OtherSkill = 6,
        Custom = 7
    }

    [Serializable]
    public class SkillUnlockRequirement
    {
        [SerializeField] private SkillRequirementType type = SkillRequirementType.HeroLevel;
        [SerializeField] private int requiredValue = 1;
        [SerializeField] private string targetId = "";
        [SerializeField] private string description = "";

        public SkillRequirementType Type => type;
        public int RequiredValue => requiredValue;
        public string TargetId => targetId;
        public string Description => description;

        public SkillUnlockRequirement() { }

        public SkillUnlockRequirement(SkillRequirementType reqType, int value, string target = "", string desc = "")
        {
            type = reqType;
            requiredValue = value;
            targetId = target;
            description = desc;
        }
    }

    [CreateAssetMenu(fileName = "SkillDefinition", menuName = "WuxiaGame/Data/SkillDefinition")]
    public class SkillDefinitionSO : ScriptableObject
    {
        [Header("Meta")]
        [SerializeField] private string skillId = "skill_01";
        [SerializeField] private string mindMethodId = "mm_taiji";
        [SerializeField] private SkillSlotType slotType = SkillSlotType.NormalAttack;
        [SerializeField] private string skillName = "Thái Cực Quyền";
        [SerializeField] [TextArea(2, 4)] private string description = "";
        [SerializeField] private Sprite icon;

        [Header("Unlock Conditions")]
        [SerializeField] private List<SkillUnlockRequirement> unlockConditions = new List<SkillUnlockRequirement>();

        [Header("Combat Foundation")]
        [SerializeField] private float damageMultiplier = 1.0f;
        [SerializeField] private float rageCost = 0f;
        [SerializeField] private float cooldown = 0f;
        [SerializeField] private bool isPassive = false;

        [Header("Effects Foundation (P07.3)")]
        [SerializeField] private List<WuxiaGame.Combat.SkillEffectDefinitionSO> effects = new List<WuxiaGame.Combat.SkillEffectDefinitionSO>();
        [SerializeField] private bool hasExplicitEffects = false;

        [Header("Freeze Shatter (P07.6)")]
        [SerializeField] private bool canShatterFreeze = false;

        [Header("Cast Time (P07.9)")]
        [SerializeField] private float castTime = 0f;

        [Header("Channel (P07.9 Phase 3)")]
        [SerializeField] private bool isChannel = false;
        [SerializeField] private float channelDuration = 0f;
        [SerializeField] private float channelTickInterval = 0f;

        [Header("Skill Decision Priority (P07.9.1)")]
        [SerializeField] private int priority = 0;

        public string SkillId => skillId;
        public string MindMethodId => mindMethodId;
        public SkillSlotType SlotType => slotType;
        public string SkillName => skillName;
        public string Description => description;
        public Sprite Icon => icon;
        public IReadOnlyList<SkillUnlockRequirement> UnlockConditions => unlockConditions;

        public float DamageMultiplier => damageMultiplier;
        public float RageCost => rageCost;
        public float Cooldown => cooldown;
        public bool IsPassive => isPassive;
        public int Priority => priority;

        public float CastTime => castTime;
        public bool IsChannel => isChannel;
        public float ChannelDuration => channelDuration;
        public float ChannelTickInterval => channelTickInterval;
        public bool IsInstant => castTime <= 0f && !isChannel;

        public IReadOnlyList<WuxiaGame.Combat.SkillEffectDefinitionSO> Effects => effects;
        public bool HasExplicitEffects => hasExplicitEffects;
        public bool CanShatterFreeze => canShatterFreeze;

        public void SetCastTime(float time)
        {
            castTime = Mathf.Max(0f, time);
        }

        public void SetChannel(bool channel, float duration = 0f, float tickInterval = 0f)
        {
            isChannel = channel;
            channelDuration = Mathf.Max(0f, duration);
            channelTickInterval = Mathf.Max(0f, tickInterval);
        }

        public void SetCanShatterFreeze(bool shatter)
        {
            canShatterFreeze = shatter;
        }

        public void SetPriority(int newPriority)
        {
            priority = newPriority;
        }

        public void SetEffects(List<WuxiaGame.Combat.SkillEffectDefinitionSO> newEffects)
        {
            effects = newEffects != null ? new List<WuxiaGame.Combat.SkillEffectDefinitionSO>(newEffects) : new List<WuxiaGame.Combat.SkillEffectDefinitionSO>();
            hasExplicitEffects = true;
        }

        public void AddEffect(WuxiaGame.Combat.SkillEffectDefinitionSO effect)
        {
            if (effect != null)
            {
                effects.Add(effect);
                hasExplicitEffects = true;
            }
        }

        public void ClearEffects()
        {
            effects.Clear();
            damageMultiplier = 0f;
            hasExplicitEffects = true;
        }

        public void InitializeSkill(
            string id,
            string mmId,
            SkillSlotType slot,
            string name,
            string desc = "",
            List<SkillUnlockRequirement> conditions = null,
            float dmgMultiplier = 1.0f,
            float costRage = 0f,
            float cd = 0f,
            bool passive = false,
            List<WuxiaGame.Combat.SkillEffectDefinitionSO> skillEffects = null,
            bool shatterFreeze = false,
            float skillCastTime = 0f,
            bool channel = false,
            float chDuration = 0f,
            float chTickInterval = 0f,
            int skillPriority = 0)
        {
            skillId = id;
            mindMethodId = mmId;
            slotType = slot;
            skillName = name;
            description = desc;
            damageMultiplier = dmgMultiplier;
            rageCost = costRage;
            cooldown = cd;
            isPassive = passive;
            unlockConditions = conditions != null ? new List<SkillUnlockRequirement>(conditions) : new List<SkillUnlockRequirement>();
            canShatterFreeze = shatterFreeze;
            castTime = Mathf.Max(0f, skillCastTime);
            isChannel = channel;
            channelDuration = Mathf.Max(0f, chDuration);
            channelTickInterval = Mathf.Max(0f, chTickInterval);
            priority = skillPriority;
            if (skillEffects != null)
            {
                effects = new List<WuxiaGame.Combat.SkillEffectDefinitionSO>(skillEffects);
                hasExplicitEffects = true;
            }
        }
    }
}
