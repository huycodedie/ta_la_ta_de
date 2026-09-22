using UnityEngine;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Stats;

namespace WuxiaGame.Combat
{
    public static class DamageCalculator
    {
        public static DamageResult CalculateDamage(
            Entity attacker,
            Entity defender,
            float skillMultiplier,
            CombatConfigSO combatConfig,
            DamageType damageType = DamageType.BasicAttack,
            bool canShatterFreeze = false)
        {
            if (attacker == null || defender == null)
            {
                return new DamageResult(attacker, defender, 0, 0, false, false, damageType);
            }

            float atk = attacker.Stats.GetValue(StatType.Attack, 10f);
            float def = defender.Stats.GetValue(StatType.Defense, 0f);
            float critRate = attacker.Stats.GetValue(StatType.CritRate, combatConfig != null ? combatConfig.DefaultCritRate : 1f);
            float critDamage = attacker.Stats.GetValue(StatType.CritDamage, combatConfig != null ? combatConfig.DefaultCritDamage : 1f);
            float dodgeRate = defender.Stats.GetValue(StatType.Dodge, combatConfig != null ? combatConfig.DefaultDodgeRate : 1f);

            // 1. Dodge Check
            bool isDodged = Random.Range(0f, 100f) < dodgeRate;
            if (isDodged)
            {
                return new DamageResult(attacker, defender, atk * skillMultiplier, 0f, false, true, damageType);
            }

            // 2. Base Damage & Defense Modifier (incorporating outgoing DamageDealt debuff)
            float dmgDealtMult = attacker.StatusController != null ? attacker.StatusController.GetDamageDealtMultiplier() : 1f;
            float rawDamage = atk * skillMultiplier * dmgDealtMult;
            float defModifier = combatConfig != null ? combatConfig.CalculateDefenseModifier(def) : (100f / (100f + def));

            // 3. Crit Check
            // Crit Damage is additive percentage over base (e.g. 1% -> 101% / 1.01x multiplier)
            bool isCrit = Random.Range(0f, 100f) < critRate;
            float critMultiplier = isCrit ? (1.0f + (critDamage / 100f)) : 1.0f;

            // 4. Final Damage Calculation
            float finalDamage = rawDamage * defModifier * critMultiplier;
            finalDamage = Mathf.Max(1f, finalDamage); // Minimum 1 damage if hit

            // 5. Freeze Shatter extension check
            if (canShatterFreeze && defender.IsFrozen && defender.StatusController != null)
            {
                defender.StatusController.TryTriggerFreezeShatter(attacker);
            }

            return new DamageResult(attacker, defender, rawDamage, finalDamage, isCrit, false, damageType);
        }

        public static DamageResult CalculateDotDamage(
            Entity attacker,
            Entity defender,
            float damagePerTick,
            int stackCount = 1,
            CombatConfigSO combatConfig = null,
            DamageType damageType = DamageType.Skill)
        {
            if (defender == null)
            {
                return new DamageResult(attacker, defender, 0, 0, false, false, damageType);
            }

            int effectiveStacks = Mathf.Max(1, stackCount);
            float dmgDealtMult = attacker != null && attacker.StatusController != null ? attacker.StatusController.GetDamageDealtMultiplier() : 1f;
            float finalDamage = Mathf.Max(0f, damagePerTick * effectiveStacks * dmgDealtMult);

            return new DamageResult(
                attacker,
                defender,
                finalDamage,
                finalDamage,
                false,
                false,
                damageType);
        }
    }
}
