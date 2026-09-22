using System;
using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Entities;

namespace WuxiaGame.Combat
{
    public enum SkillEffectType
    {
        Damage = 1,
        Heal = 2,
        Buff = 3,
        Debuff = 4,
        Shield = 5,
        CrowdControl = 6,
        SpecialEffect = 7,
        Cleanse = 8,
        Dispel = 9
    }

    public enum SkillTargetPolicy
    {
        SingleTarget = 1,
        Self = 2,
        AllEnemies = 3,
        AllAllies = 4,
        Area = 5,
        MultipleTargets = 6,
        RandomTarget = 7
    }

    public abstract class SkillEffectDefinitionSO : ScriptableObject
    {
        [Header("Effect Configuration")]
        [SerializeField] protected SkillEffectType effectType = SkillEffectType.Damage;
        [SerializeField] protected SkillTargetPolicy targetPolicy = SkillTargetPolicy.SingleTarget;

        [Header("Target Selection Configuration (P08)")]
        [SerializeField] protected float targetRadius = 0f;
        [SerializeField] protected int maxTargetCount = 0;

        public SkillEffectType EffectType => effectType;
        public SkillTargetPolicy TargetPolicy => targetPolicy;
        public float TargetRadius => targetRadius;
        public int MaxTargetCount => maxTargetCount;

        public abstract SkillEffectExecutionResult Execute(
            SkillExecutionRequest request,
            Entity target,
            CombatConfigSO combatConfig);
    }
}
