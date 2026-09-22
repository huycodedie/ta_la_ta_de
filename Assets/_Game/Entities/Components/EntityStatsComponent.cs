using UnityEngine;
using WuxiaGame.Stats;

namespace WuxiaGame.Entities.Components
{
    public class EntityStatsComponent : MonoBehaviour
    {
        public StatContainer Container { get; private set; } = new StatContainer();

        public float GetValue(StatType type, float defaultValue = 0f)
        {
            return Container.GetValue(type, defaultValue);
        }

        public void SetStat(StatType type, float baseValue)
        {
            Container.SetStat(type, baseValue);
        }

        public void SetBaseValue(StatType type, float baseValue)
        {
            Container.SetStat(type, baseValue);
        }

        public void ModifyValue(StatType type, float delta)
        {
            Container.ModifyValue(type, delta);
        }
    }
}
