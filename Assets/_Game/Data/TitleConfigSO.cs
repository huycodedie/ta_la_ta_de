using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "TitleConfig", menuName = "WuxiaGame/Data/TitleConfig")]
    public class TitleConfigSO : ScriptableObject
    {
        [Header("Title Meta")]
        [SerializeField] private string titleId = "title_01";
        [SerializeField] private string titleName = "Novice Disciple";
        [SerializeField] private int orderIndex = 1;

        [Header("Base Stats Replacement")]
        [SerializeField] private float baseMaxHealth = 1000f;
        [SerializeField] private float baseAttack = 100f;
        [SerializeField] private float baseDefense = 20f;

        [Header("Level Cap")]
        [SerializeField] private int maxLevelCap = 5;

        [Header("Breakthrough Requirements")]
        [SerializeField] private List<ProgressionRequirementData> requirements = new List<ProgressionRequirementData>();

        public string TitleId => titleId;
        public string TitleName => titleName;
        public int OrderIndex => orderIndex;
        public float BaseMaxHealth => baseMaxHealth;
        public float BaseAttack => baseAttack;
        public float BaseDefense => baseDefense;
        public int MaxLevelCap => maxLevelCap;
        public IReadOnlyList<ProgressionRequirementData> Requirements => requirements;

        public void InitializeTitle(string id, string name, int order, float hp, float atk, float def, int cap, List<ProgressionRequirementData> reqs)
        {
            titleId = id;
            titleName = name;
            orderIndex = order;
            baseMaxHealth = hp;
            baseAttack = atk;
            baseDefense = def;
            maxLevelCap = cap;
            requirements = reqs ?? new List<ProgressionRequirementData>();
        }
    }
}
