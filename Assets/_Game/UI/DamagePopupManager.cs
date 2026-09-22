using TMPro;
using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Entities;

namespace WuxiaGame.UI
{
    public class DamagePopupManager : MonoBehaviour
    {
        public static DamagePopupManager Instance { get; private set; }

        [SerializeField] private DamagePopup damagePopupPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnEntityDamaged += HandleEntityDamaged;
            EventBus.OnShieldAbsorbed += HandleShieldAbsorbed;
        }

        private void OnDisable()
        {
            EventBus.OnEntityDamaged -= HandleEntityDamaged;
            EventBus.OnShieldAbsorbed -= HandleShieldAbsorbed;
        }

        private void HandleEntityDamaged(Entity target, DamageResult result)
        {
            if (target == null) return;

            Vector3 spawnPos = target.transform.position + new Vector3(0, 1.5f, 0);
            CreatePopup(spawnPos, result);
        }

        private void HandleShieldAbsorbed(Entity target, ShieldAbsorbResult result)
        {
            if (target == null || result.AbsorbedAmount <= 0.5f) return;

            Vector3 spawnPos = target.transform.position + new Vector3(0, 1.8f, 0);
            CreateShieldPopup(spawnPos, result.AbsorbedAmount);
        }

        public void CreateShieldPopup(Vector3 position, float absorbed)
        {
            DamagePopup popupInstance = null;

            if (damagePopupPrefab != null)
            {
                popupInstance = Instantiate(damagePopupPrefab, position, Quaternion.identity);
            }
            else
            {
                GameObject popupGO = new GameObject("DynamicShieldPopup");
                popupGO.transform.position = position;
                TextMeshPro tm = popupGO.AddComponent<TextMeshPro>();
                tm.alignment = TextAlignmentOptions.Center;
                tm.sortingOrder = 101;
                popupInstance = popupGO.AddComponent<DamagePopup>();
            }

            popupInstance.SetupShieldAbsorb(absorbed);
        }

        public void CreatePopup(Vector3 position, DamageResult result)
        {
            DamagePopup popupInstance = null;

            if (damagePopupPrefab != null)
            {
                popupInstance = Instantiate(damagePopupPrefab, position, Quaternion.identity);
            }
            else
            {
                // Dynamic fallback if prefab is not assigned
                GameObject popupGO = new GameObject("DynamicDamagePopup");
                popupGO.transform.position = position;
                TextMeshPro tm = popupGO.AddComponent<TextMeshPro>();
                tm.alignment = TextAlignmentOptions.Center;
                tm.sortingOrder = 100;
                popupInstance = popupGO.AddComponent<DamagePopup>();
            }

            popupInstance.Setup(result);
        }
    }
}
