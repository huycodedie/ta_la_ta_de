using System;

namespace WuxiaGame.Data
{
    [Serializable]
    public struct EquipmentUpgradeCostData
    {
        public int FromLevel;
        public int ToLevel;
        public int GoldCost;
        public int MaterialCost;

        public EquipmentUpgradeCostData(int fromLevel, int toLevel, int gold, int material)
        {
            FromLevel = fromLevel;
            ToLevel = toLevel;
            GoldCost = gold;
            MaterialCost = material;
        }
    }
}
