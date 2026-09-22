using System;
using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Stats;

namespace WuxiaGame.Entities.Components
{
    public class HealthComponent : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        private bool isDead = false;

        private Entity ownerEntity;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0 && !isDead;

        public Entity OwnerEntity => ownerEntity != null ? ownerEntity : (ownerEntity = GetComponent<Entity>());

        public event Action<float, float> OnHealthChanged; // (current, max)
        public event Action OnDeath;

        private void Awake()
        {
            if (ownerEntity == null)
            {
                ownerEntity = GetComponent<Entity>();
            }
        }

        public void InitializeHealth(float maxHp, Entity owner = null)
        {
            maxHealth = maxHp;
            currentHealth = maxHp;
            isDead = false;
            if (owner != null) ownerEntity = owner;
            else if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void UpdateMaxHealth(float newMaxHp, bool preserveHpDelta = true)
        {
            float oldMax = maxHealth;
            maxHealth = Mathf.Max(1f, newMaxHp);

            if (preserveHpDelta)
            {
                float delta = maxHealth - oldMax;
                if (delta > 0f)
                {
                    currentHealth = Mathf.Min(maxHealth, currentHealth + delta);
                }
                else
                {
                    currentHealth = Mathf.Min(maxHealth, currentHealth);
                }
            }
            else
            {
                currentHealth = maxHealth;
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void SetCurrentHealth(float hp)
        {
            currentHealth = Mathf.Clamp(hp, 0f, maxHealth);
            if (currentHealth > 0f) isDead = false;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void Revive(float healthAmount = -1f)
        {
            isDead = false;
            currentHealth = healthAmount > 0f ? Mathf.Min(maxHealth, healthAmount) : maxHealth;
            if (ownerEntity != null)
            {
                ownerEntity.EnableEntityActions();
            }
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void TakeDamage(DamageResult result)
        {
            if (!IsAlive || isDead || currentHealth <= 0f) return;

            if (result.IsDodged) return;

            float remainingDamage = result.FinalDamage;
            float absorbedDamage = 0f;

            if (remainingDamage > 0f && OwnerEntity != null && OwnerEntity.StatusController != null && OwnerEntity.StatusController.HasActiveShield)
            {
                OwnerEntity.StatusController.AbsorbDamage(
                    remainingDamage,
                    out absorbedDamage,
                    out remainingDamage,
                    result.Attacker,
                    result.DamageType);
            }

            if (remainingDamage > 0f)
            {
                currentHealth -= remainingDamage;
                currentHealth = Mathf.Max(0f, currentHealth);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (absorbedDamage > 0f)
            {
                Debug.Log($"[COMBAT:DAMAGE] Target={OwnerEntity?.EntityName}, FinalDmg={result.FinalDamage:F1}, ShieldAbsorbed={absorbedDamage:F1}, RemHpDmg={remainingDamage:F1}, PostHp={currentHealth:F1}/{maxHealth:F1}");
            }

            if (currentHealth <= 0f)
            {
                if (OwnerEntity is Hero && WuxiaGame.Progression.MindMethodManager.Instance != null && WuxiaGame.Progression.MindMethodManager.Instance.TryTriggerReviveOnce(out float reviveHp))
                {
                    currentHealth = Mathf.Clamp(reviveHp, 1f, maxHealth);
                    isDead = false;
                    OnHealthChanged?.Invoke(currentHealth, maxHealth);
                    Debug.Log($"[MIND METHOD PASSIVE] ReviveOnce triggered! Hero revived with {currentHealth:F0} HP.");
                    return;
                }

                currentHealth = 0f;
                Die();
            }
        }

        public void TakeDamage(float damageAmount)
        {
            if (!IsAlive || isDead || currentHealth <= 0f) return;

            float remainingDamage = damageAmount;
            float absorbedDamage = 0f;

            if (remainingDamage > 0f && OwnerEntity != null && OwnerEntity.StatusController != null && OwnerEntity.StatusController.HasActiveShield)
            {
                OwnerEntity.StatusController.AbsorbDamage(
                    remainingDamage,
                    out absorbedDamage,
                    out remainingDamage,
                    null,
                    DamageType.BasicAttack);
            }

            if (remainingDamage > 0f)
            {
                currentHealth -= remainingDamage;
                currentHealth = Mathf.Max(0f, currentHealth);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (absorbedDamage > 0f)
            {
                Debug.Log($"[COMBAT:DAMAGE] Target={OwnerEntity?.EntityName}, Damage={damageAmount:F1}, ShieldAbsorbed={absorbedDamage:F1}, RemHpDmg={remainingDamage:F1}, PostHp={currentHealth:F1}/{maxHealth:F1}");
            }

            if (currentHealth <= 0f)
            {
                if (OwnerEntity is Hero && WuxiaGame.Progression.MindMethodManager.Instance != null && WuxiaGame.Progression.MindMethodManager.Instance.TryTriggerReviveOnce(out float reviveHp))
                {
                    currentHealth = Mathf.Clamp(reviveHp, 1f, maxHealth);
                    isDead = false;
                    OnHealthChanged?.Invoke(currentHealth, maxHealth);
                    Debug.Log($"[MIND METHOD PASSIVE] ReviveOnce triggered! Hero revived with {currentHealth:F0} HP.");
                    return;
                }

                currentHealth = 0f;
                Die();
            }
        }

        public float Heal(float amount)
        {
            if (!IsAlive || isDead || amount <= 0f) return 0f;

            float healMult = OwnerEntity != null && OwnerEntity.StatusController != null
                ? OwnerEntity.StatusController.GetHealingReceivedMultiplier()
                : 1f;
            float effectiveAmount = amount * healMult;
            if (effectiveAmount <= 0f) return 0f;

            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + effectiveAmount);
            float actualHealed = currentHealth - previousHealth;

            if (actualHealed > 0f)
            {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }

            return actualHealed;
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;
            currentHealth = 0f;

            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity != null)
            {
                ownerEntity.DisableEntityActions();
                ownerEntity.SetCurrentTarget(null);
            }

            int enc = BattleManager.Instance != null ? BattleManager.Instance.EncounterIndex : 1;
            string name = ownerEntity != null ? ownerEntity.EntityName : gameObject.name;
            if (ownerEntity is Monster)
            {
                Debug.Log($"[COMBAT] Monster #{enc} DEAD");
                Debug.Log($"[COMBAT] Monster #{enc} Final HP = 0 / {maxHealth:F0}");
                Debug.Log($"[COMBAT] Monster #{enc} Combat State = STOPPED");
            }
            Debug.Log($"[COMBAT] {name} DEAD");
            Debug.Log($"[COMBAT] {name} Final HP = 0 / {maxHealth:F0}");

            OnDeath?.Invoke();
            if (ownerEntity != null)
            {
                EventBus.RaiseEntityDied(ownerEntity);
            }
        }
    }
}
