using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "HeroConfig", menuName = "WuxiaGame/Data/HeroConfig")]
    public class HeroConfigSO : ScriptableObject
    {
        [Header("Entity Info")]
        [SerializeField] private string heroName = "Wuxia Hero";

        [Header("Base Stats")]
        [SerializeField] private float maxHealth = 1000f;
        [SerializeField] private float attack = 100f;
        [SerializeField] private float defense = 20f;
        [SerializeField] private float movementSpeed = 5f; // Normalized 2D speed
        [SerializeField] private float attackInterval = 1.5f; // Seconds between basic attacks
        [SerializeField] private float attackRange = 1.8f; // Range to initiate attack

        [Header("Rage Settings")]
        [SerializeField] private float maxRage = 100f;
        [SerializeField] private float initialRage = 100f;
        [SerializeField] private float ragePerAttack = 1f;
        [SerializeField] private float ragePerDamageTaken = 1f;

        [Header("Combat Rates Overrides (Negative to use global defaults)")]
        [SerializeField] private float critRate = 1f; // 1%
        [SerializeField] private float critDamage = 1f; // 1%

        [Header("Breakthrough Profile (Optional Hero-Specific Overrides)")]
        [SerializeField] private TitleBreakthroughDatabaseSO breakthroughProfile;

        public string HeroName => heroName;
        public float MaxHealth => maxHealth;
        public float Attack => attack;
        public float Defense => defense;
        public float MovementSpeed => movementSpeed;
        public float AttackInterval => attackInterval;
        public float AttackRange => attackRange;

        public float MaxRage => maxRage;
        public float InitialRage => initialRage;
        public float RagePerAttack => ragePerAttack;
        public float RagePerDamageTaken => ragePerDamageTaken;

        public float CritRate => critRate;
        public float CritDamage => critDamage;
        public TitleBreakthroughDatabaseSO BreakthroughProfile => breakthroughProfile;

        public void SetBreakthroughProfile(TitleBreakthroughDatabaseSO profile)
        {
            breakthroughProfile = profile;
        }
    }
}
