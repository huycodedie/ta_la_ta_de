using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "MonsterConfig", menuName = "WuxiaGame/Data/MonsterConfig")]
    public class MonsterConfigSO : ScriptableObject
    {
        [Header("Entity Info")]
        [SerializeField] private string monsterName = "Wild Monster";

        [Header("Base Stats")]
        [SerializeField] private float maxHealth = 500f;
        [SerializeField] private float attack = 50f;
        [SerializeField] private float defense = 10f;
        [SerializeField] private float movementSpeed = 3f;
        [SerializeField] private float attackInterval = 2.0f;
        [SerializeField] private float attackRange = 1.8f;

        [Header("Combat Rates")]
        [SerializeField] private float critRate = 1f;
        [SerializeField] private float critDamage = 1f;

        [Header("Rewards")]
        [SerializeField] private float expReward = 10f;

        public string MonsterName => monsterName;
        public float MaxHealth => maxHealth;
        public float Attack => attack;
        public float Defense => defense;
        public float MovementSpeed => movementSpeed;
        public float AttackInterval => attackInterval;
        public float AttackRange => attackRange;

        public float CritRate => critRate;
        public float CritDamage => critDamage;
        public float ExpReward => expReward > 0f ? expReward : 10f;

        public void InitializeMonsterConfig(string name, float hp, float atk, float def, float speed, float interval, float range, float exp)
        {
            monsterName = name;
            maxHealth = hp;
            attack = atk;
            defense = def;
            movementSpeed = speed;
            attackInterval = interval;
            attackRange = range;
            expReward = exp;
        }
    }
}
