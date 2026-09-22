using System;
using UnityEngine;
using WuxiaGame.Core;

namespace WuxiaGame.Entities.Components
{
    public class RageComponent : MonoBehaviour
    {
        [SerializeField] private float maxRage = 100f;
        [SerializeField] private float currentRage = 0f;

        private Entity ownerEntity;

        public float MaxRage => maxRage;
        public float CurrentRage => currentRage;
        public Entity OwnerEntity => ownerEntity != null ? ownerEntity : (ownerEntity = GetComponent<Entity>());

        public event Action<float, float> OnRageChanged; // (current, max)

        private void Awake()
        {
            if (ownerEntity == null)
            {
                ownerEntity = GetComponent<Entity>();
            }
        }

        public void InitializeRage(float maxRageVal, float initialRage = 100f, Entity owner = null)
        {
            maxRage = maxRageVal > 0f ? maxRageVal : 100f;
            currentRage = Mathf.Clamp(initialRage, 0f, maxRage);
            if (owner != null) ownerEntity = owner;
            else if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            NotifyRageChanged();
        }

        public void AddRage(float amount)
        {
            if (amount <= 0f) return;

            float prevRage = currentRage;
            float newRage = Mathf.Clamp(currentRage + amount, 0f, maxRage);

            currentRage = newRage;
            NotifyRageChanged();

            Debug.Log($"[RAGE] Current: {currentRage:F0} / {maxRage:F0}");
            if (currentRage >= maxRage)
            {
                Debug.Log("[RAGE] Clamped at 100");
            }
        }

        public void ResetRage(float val = 0f)
        {
            currentRage = Mathf.Clamp(val, 0f, maxRage);
            NotifyRageChanged();
        }

        public bool ConsumeRage(float amount)
        {
            if (currentRage < amount) return false;

            currentRage -= amount;
            NotifyRageChanged();
            return true;
        }

        private void NotifyRageChanged()
        {
            OnRageChanged?.Invoke(currentRage, maxRage);
            if (ownerEntity != null)
            {
                EventBus.RaiseRageChanged(ownerEntity, currentRage, maxRage);
            }
        }
    }
}
