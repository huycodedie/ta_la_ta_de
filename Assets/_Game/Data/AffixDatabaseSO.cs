using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "AffixDatabase", menuName = "WuxiaGame/Data/AffixDatabase")]
    public class AffixDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<AffixDefinitionSO> affixes = new List<AffixDefinitionSO>();

        public IReadOnlyList<AffixDefinitionSO> Affixes => affixes;

        public void SetAffixes(List<AffixDefinitionSO> list)
        {
            affixes = list;
        }
    }
}
