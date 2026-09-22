using System.Collections.Generic;
using UnityEngine;
using WuxiaGame.Stats;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "HeroProgressionConfig", menuName = "WuxiaGame/Data/HeroProgressionConfig")]
    public class HeroProgressionConfigSO : ScriptableObject
    {
        [Header("Level Settings")]
        [SerializeField] private int startingLevel = 1;
        [SerializeField] private int maxLevel = 100;

        [Header("Base Stats at Level 1")]
        [SerializeField] private float baseMaxHealth = 1000f;
        [SerializeField] private float baseAttack = 100f;
        [SerializeField] private float baseDefense = 20f;

        [Header("Stat Growth Per Level (Decoupled in D1-D23 baseline - Base stats derive from Title)")]
        [SerializeField] private float hpGrowthPerLevel = 0f;
        [SerializeField] private float atkGrowthPerLevel = 0f;
        [SerializeField] private float defGrowthPerLevel = 0f;

        public int StartingLevel => startingLevel;
        public int MaxLevel => maxLevel;
        public float BaseMaxHealth => baseMaxHealth;
        public float BaseAttack => baseAttack;
        public float BaseDefense => baseDefense;
        public float HpGrowthPerLevel => hpGrowthPerLevel;
        public float AtkGrowthPerLevel => atkGrowthPerLevel;
        public float DefGrowthPerLevel => defGrowthPerLevel;

        public void InitializeConfig(int startLvl, int maxLvl, float hp, float atk, float def, float hpGrowth = 0f, float atkGrowth = 0f, float defGrowth = 0f)
        {
            startingLevel = startLvl;
            maxLevel = maxLvl;
            baseMaxHealth = hp;
            baseAttack = atk;
            baseDefense = def;
            hpGrowthPerLevel = hpGrowth;
            atkGrowthPerLevel = atkGrowth;
            defGrowthPerLevel = defGrowth;
        }

        public Dictionary<StatType, float> GetBaseStatsForLevel(int level)
        {
            Dictionary<StatType, float> stats = new Dictionary<StatType, float>
            {
                [StatType.MaxHealth] = baseMaxHealth,
                [StatType.Attack] = baseAttack,
                [StatType.Defense] = baseDefense,
                [StatType.MoveSpeed] = 3.5f,
                [StatType.AttackInterval] = 1.0f,
                [StatType.MaxRage] = 100f,
                [StatType.CritRate] = 5f,
                [StatType.CritDamage] = 150f,
                [StatType.ComboRate] = 0f,
                [StatType.CounterRate] = 0f,
                [StatType.Dodge] = 0f,
                [StatType.Lifesteal] = 0f
            };

            return stats;
        }
    }
}
