using UnityEngine;
using WuxiaGame.Core;
using WuxiaGame.Data;
using WuxiaGame.Entities;
using WuxiaGame.Entities.Components;

namespace WuxiaGame.Combat
{
    public static class BasicAttackProcessor
    {
        public static void ExecuteBasicAttack(Entity attacker, Entity target, CombatConfigSO combatConfig = null, bool canShatterFreeze = false)
        {
            // 0. Rigorous Target & Attacker Validation
            if (attacker == null || target == null ||
                !attacker.gameObject.activeInHierarchy || !target.gameObject.activeInHierarchy ||
                attacker.Health == null || target.Health == null ||
                !attacker.IsAlive || !target.IsAlive || target.Health.CurrentHealth <= 0f)
            {
                if (attacker is Monster)
                {
                    Debug.LogWarning("[COMBAT] Monster target invalid");
                    Debug.LogWarning("[COMBAT] Monster combat paused");
                }
                Debug.LogWarning("[ERROR] Combat target invalid. Combat loop stopped to prevent damage against stale target.");
                return;
            }

            Debug.Log($"[COMBAT] Target Validation = PASS | Target = {target.EntityName}");

            string attackerName = attacker.EntityName;
            string targetName = target.EntityName;
            bool attackerIsMonster = attacker is Monster;
            int enc = BattleManager.Instance != null ? BattleManager.Instance.EncounterIndex : 1;
            float maxHp = target.Health != null ? target.Health.MaxHealth : 0f;

            float targetHpBefore = target.Health.CurrentHealth;

            // 1. Calculate Damage
            DamageResult result = DamageCalculator.CalculateDamage(attacker, target, 1.0f, combatConfig, DamageType.BasicAttack, canShatterFreeze);

            // 2. Apply Damage to Target HealthComponent
            target.Health.TakeDamage(result);

            // 3. Rage Gain (Attacker basic attack +1, Defender taking damage +1)
            float attackRage = combatConfig != null ? combatConfig.RagePerBasicAttack : 1f;
            float damageRage = combatConfig != null ? combatConfig.RagePerDamageTaken : 1f;

            if (attacker != null && attacker.IsAlive && attacker.Rage != null)
            {
                float finalAttackRage = attackRage;
                if (attacker is Hero && WuxiaGame.Progression.MindMethodManager.Instance != null)
                {
                    finalAttackRage += WuxiaGame.Progression.MindMethodManager.Instance.GetActiveAttackRageBonus();
                }

                if (attacker.StatusController != null)
                {
                    finalAttackRage *= attacker.StatusController.GetRageGainMultiplier();
                }

                if (attacker is Hero)
                {
                    Debug.Log($"[RAGE] Hero Basic Attack -> +{finalAttackRage:F0}");
                }
                attacker.Rage.AddRage(finalAttackRage);
            }

            if (target != null && target.IsAlive && target.Rage != null && !result.IsDodged)
            {
                float finalDamageRage = damageRage;
                if (target is Hero && WuxiaGame.Progression.MindMethodManager.Instance != null)
                {
                    finalDamageRage += WuxiaGame.Progression.MindMethodManager.Instance.GetActiveDamageRageBonus();
                }

                if (target.StatusController != null)
                {
                    finalDamageRage *= target.StatusController.GetRageGainMultiplier();
                }

                if (target is Hero)
                {
                    Debug.Log($"[RAGE] Hero Damaged -> +{finalDamageRage:F0}");
                }
                target.Rage.AddRage(finalDamageRage);
            }

            // 4. Raise Event for Damage Feedback / UI / VFX
            if (target != null)
            {
                EventBus.RaiseEntityDamaged(target, result);
            }

            // 5. Clear Development Combat Log
            float currentHp = (target != null && target.IsAlive && target.Health != null) ? target.Health.CurrentHealth : 0f;

            if (attackerIsMonster)
            {
                Debug.Log($"[REAL PLAY TEST]\nMonster #{enc} ATTACK\nDamage: {result.FinalDamage:F2}\nHero HP Before: {targetHpBefore:F2}\nHero HP After: {currentHp:F2}");
                Debug.Log($"[COMBAT ATTACK]\nMonster #{enc} -> Hero\nDamage: {result.FinalDamage:F2}\nHero HP: {currentHp:F2}/{maxHp:F2}");
                Debug.Log($"[COMBAT] Monster #{enc} ATTACK -> {targetName}");
                Debug.Log($"[COMBAT] Monster #{enc} -> {targetName}");
                Debug.Log($"[COMBAT] Damage = {result.FinalDamage:F2}");
                Debug.Log($"[COMBAT] Monster #{enc} DAMAGE = {result.FinalDamage:F2} (Crit: {result.IsCrit})");
                Debug.Log($"[COMBAT] Hero HP = {currentHp:F2} / {maxHp:F2}");
                Debug.Log($"[COMBAT] {targetName} HP = {currentHp:F2} / {maxHp:F2}");
            }
            else
            {
                Debug.Log($"[REAL PLAY TEST]\nHero ATTACK\nDamage: {result.FinalDamage:F2}\nMonster HP Before: {targetHpBefore:F2}\nMonster HP After: {currentHp:F2}");
                Debug.Log($"[COMBAT ATTACK]\nHero -> Monster #{enc}\nDamage: {result.FinalDamage:F2}\nMonster HP: {currentHp:F2}/{maxHp:F2}");
                Debug.Log($"[COMBAT] {attackerName} ATTACK -> Monster #{enc}");
                Debug.Log($"[COMBAT] {targetName} DAMAGE = {result.FinalDamage:F2} (Crit: {result.IsCrit})");
                Debug.Log($"[COMBAT] {targetName} HP = {currentHp:F2} / {maxHp:F2}");
            }
        }
    }
}
