using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "WuxiaGame/Data/CombatConfig")]
    public class CombatConfigSO : ScriptableObject
    {
        [Header("Default Combat Rates (Percentages: 1 = 1%)")]
        [SerializeField, Range(0, 100)] private float defaultComboRate = 1f;
        [SerializeField, Range(0, 100)] private float defaultCounterRate = 1f;
        [SerializeField, Range(0, 100)] private float defaultCritRate = 1f;
        [SerializeField, Range(0, 500)] private float defaultCritDamage = 1f; // 1% additive base
        [SerializeField, Range(0, 100)] private float defaultDodgeRate = 1f;
        [SerializeField, Range(0, 100)] private float defaultLifestealRate = 0f;

        [Header("Rage Settings")]
        [SerializeField] private float maxRage = 100f;
        [SerializeField] private float initialRage = 100f;
        [SerializeField] private float ragePerBasicAttack = 1f;
        [SerializeField] private float ragePerDamageTaken = 1f;
        [SerializeField] private float ultimateRageCost = 80f;

        [Header("Provisional Defense Formula Params")]
        [Tooltip("Formula: DefenseModifier = defenseConstant / (defenseConstant + DEF)")]
        [SerializeField] private float defenseConstant = 100f;

        public float DefaultComboRate => defaultComboRate;
        public float DefaultCounterRate => defaultCounterRate;
        public float DefaultCritRate => defaultCritRate;
        public float DefaultCritDamage => defaultCritDamage;
        public float DefaultDodgeRate => defaultDodgeRate;
        public float DefaultLifestealRate => defaultLifestealRate;

        public float MaxRage => maxRage;
        public float InitialRage => initialRage;
        public float RagePerBasicAttack => ragePerBasicAttack;
        public float RagePerDamageTaken => ragePerDamageTaken;
        public float UltimateRageCost => ultimateRageCost;

        public float DefenseConstant => defenseConstant;

        public float CalculateDefenseModifier(float defense)
        {
            float safeDef = Mathf.Max(0, defense);
            return defenseConstant / (defenseConstant + safeDef);
        }
    }
}
