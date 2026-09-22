using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "RarityDefinition", menuName = "WuxiaGame/Data/RarityDefinition")]
    public class RarityDefinitionSO : ScriptableObject
    {
        [Header("Rarity Info")]
        [SerializeField] private string rarityId = "tinh_khieut";
        [SerializeField] private string displayName = "Tinh Khiết";
        [SerializeField] private Color rarityColor = Color.white;
        [SerializeField] private int orderIndex = 1;

        [Header("Affix Count Rules")]
        [SerializeField] private int minAffixCount = 1;
        [SerializeField] private int maxAffixCount = 2;

        [Header("Stat Scaling")]
        [SerializeField] private float statMultiplier = 1.0f;

        public string RarityId => rarityId;
        public string DisplayName => displayName;
        public Color RarityColor => rarityColor;
        public int OrderIndex => orderIndex;
        public int MinAffixCount => minAffixCount;
        public int MaxAffixCount => maxAffixCount;
        public float StatMultiplier => statMultiplier;

        public void InitializeRarity(string id, string name, Color color, int order, int minAffixes, int maxAffixes, float multiplier)
        {
            rarityId = id;
            displayName = name;
            rarityColor = color;
            orderIndex = order;
            minAffixCount = minAffixes;
            maxAffixCount = maxAffixes;
            statMultiplier = multiplier;
        }
    }
}
