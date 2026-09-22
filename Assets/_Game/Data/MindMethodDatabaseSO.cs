using System.Collections.Generic;
using UnityEngine;

namespace WuxiaGame.Data
{
    [CreateAssetMenu(fileName = "MindMethodDatabase", menuName = "WuxiaGame/Data/MindMethodDatabase")]
    public class MindMethodDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<MindMethodDefinitionSO> mindMethods = new List<MindMethodDefinitionSO>();

        public IReadOnlyList<MindMethodDefinitionSO> MindMethods => mindMethods;

        public MindMethodDefinitionSO DefaultMindMethod => (mindMethods != null && mindMethods.Count > 0) ? mindMethods[0] : null;

        public void SetMindMethods(List<MindMethodDefinitionSO> methods)
        {
            mindMethods = methods != null ? new List<MindMethodDefinitionSO>(methods) : new List<MindMethodDefinitionSO>();
        }

        public MindMethodDefinitionSO GetMindMethod(string id)
        {
            if (mindMethods == null || string.IsNullOrEmpty(id)) return null;
            return mindMethods.Find(m => m != null && m.MindMethodId == id);
        }
    }
}
