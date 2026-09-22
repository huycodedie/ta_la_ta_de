using System.Collections.Generic;
using System.Text;
using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Inventory;
using WuxiaGame.Items;

namespace WuxiaGame.Drop
{
    public static class ItemGenerationTester
    {
        public static void Run1000DropSimulation()
        {
            Debug.Log("==================================================");
            Debug.Log("   STARTING 1000-ITEM DROP SIMULATION (PROTOTYPE 02)");
            Debug.Log("==================================================");

            GameObject sysGO = new GameObject("TestDropSys");
            DropSystem dropSys = sysGO.AddComponent<DropSystem>();
            dropSys.LoadDatabasesIfMissing();
            dropSys.NormalMonsterDropRate = 100f; // 100% drop rate for test
            dropSys.CurrentDropLevel = 16;

            GameObject invGO = new GameObject("TestInv");
            Inventory.Inventory inv = invGO.AddComponent<Inventory.Inventory>();

            Dictionary<string, int> rarityCounts = new Dictionary<string, int>();
            int totalItems = 1000;
            int totalAffixes = 0;
            int minAffixes = int.MaxValue;
            int maxAffixes = int.MinValue;
            int duplicateAffixCount = 0;

            for (int i = 0; i < totalItems; i++)
            {
                EquipmentInstance item = dropSys.GenerateDrop(null, 16);
                if (item != null)
                {
                    string rName = item.Rarity != null ? item.Rarity.DisplayName : "Unknown";
                    if (!rarityCounts.ContainsKey(rName)) rarityCounts[rName] = 0;
                    rarityCounts[rName]++;

                    int affCount = item.Affixes.Count;
                    totalAffixes += affCount;
                    if (affCount < minAffixes) minAffixes = affCount;
                    if (affCount > maxAffixes) maxAffixes = affCount;

                    // Check duplicate affixes
                    HashSet<string> seenAffixes = new HashSet<string>();
                    foreach (var a in item.Affixes)
                    {
                        if (seenAffixes.Contains(a.AffixId))
                        {
                            duplicateAffixCount++;
                        }
                        seenAffixes.Add(a.AffixId);
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("==================================================");
            sb.AppendLine($"   1000-ITEM SIMULATION RESULTS (Drop Level 16)");
            sb.AppendLine("==================================================");
            sb.AppendLine($"Total Generated Items: {totalItems}");
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine("Rarity Breakdown:");
            foreach (var kvp in rarityCounts)
            {
                float pct = (kvp.Value / (float)totalItems) * 100f;
                sb.AppendLine($" - {kvp.Key,-18}: {kvp.Value,4} ({pct:F2}%)");
            }
            sb.AppendLine("--------------------------------------------------");
            sb.AppendLine($"Average Affix Count   : {(totalAffixes / (float)totalItems):F2}");
            sb.AppendLine($"Minimum Affix Count   : {minAffixes}");
            sb.AppendLine($"Maximum Affix Count   : {maxAffixes}");
            sb.AppendLine($"Duplicate Affix Count : {duplicateAffixCount}");
            sb.AppendLine("==================================================");

            Debug.Log(sb.ToString());

            if (duplicateAffixCount == 0)
            {
                Debug.Log("[PASS] Duplicate Affix Count = 0! Affix generation is 100% unique per item.");
            }
            else
            {
                Debug.LogError($"[FAIL] Found {duplicateAffixCount} duplicate affixes!");
            }

            Object.DestroyImmediate(sysGO);
            Object.DestroyImmediate(invGO);
        }
    }
}
