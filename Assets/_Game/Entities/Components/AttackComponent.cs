using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Data;

namespace WuxiaGame.Entities.Components
{
    public class AttackComponent : MonoBehaviour
    {
        [SerializeField] private float attackInterval = 1.5f;
        [SerializeField] private CombatConfigSO combatConfig;

        private Entity ownerEntity;
        private float attackTimer = 0f;
        private bool isAttackEnabled = true;

        public float AttackInterval => attackInterval;
        public float AttackTimer => attackTimer;
        public bool IsAttackEnabled => isAttackEnabled;
        public Entity OwnerEntity => ownerEntity != null ? ownerEntity : (ownerEntity = GetComponent<Entity>());

        private void Awake()
        {
            if (ownerEntity == null)
            {
                ownerEntity = GetComponent<Entity>();
            }
        }

        public void Initialize(float interval, CombatConfigSO config = null, Entity owner = null)
        {
            attackInterval = interval;
            combatConfig = config;
            if (owner != null) ownerEntity = owner;
            else if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            attackTimer = 0f; // Ready to attack or start timer
        }

        public void ResetAttackTimer()
        {
            attackTimer = 0f;
        }

        public void SetAttackEnabled(bool enabled)
        {
            isAttackEnabled = enabled;
            if (!enabled)
            {
                attackTimer = 0f;
            }
        }

        public void ManualTick(float deltaTime)
        {
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (!isAttackEnabled || ownerEntity == null || !ownerEntity.IsAlive || !ownerEntity.CanBasicAttack) return;

            if (BattleManager.Instance != null && !BattleManager.Instance.IsBattleActive) return;

            attackTimer += deltaTime;

            float effectiveInterval = attackInterval;
            if (ownerEntity.StatusController != null)
            {
                effectiveInterval += ownerEntity.StatusController.GetAttackIntervalModifier();
            }

            if (attackTimer >= effectiveInterval)
            {
                TryExecuteAttack();
            }
        }

        private void Update()
        {
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (!isAttackEnabled || ownerEntity == null || !ownerEntity.IsAlive || !ownerEntity.CanBasicAttack) return;

            // Check if battle is active
            if (BattleManager.Instance != null && !BattleManager.Instance.IsBattleActive) return;

            attackTimer += Time.deltaTime;

            float effectiveInterval = attackInterval;
            if (ownerEntity.StatusController != null)
            {
                effectiveInterval += ownerEntity.StatusController.GetAttackIntervalModifier();
            }

            if (attackTimer >= effectiveInterval)
            {
                TryExecuteAttack();
            }
        }

        private void TryExecuteAttack()
        {
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity == null || !ownerEntity.IsAlive || !ownerEntity.CanBasicAttack) return;

            Entity target = ownerEntity.CurrentTarget;
            if (target == null || !target.IsAlive)
            {
                // Resolve target dynamically if missing
                ITargetResolver resolver = new NearestEnemyTargetResolver();
                target = resolver.ResolveTarget(ownerEntity);
                ownerEntity.SetCurrentTarget(target);
            }

            if (target != null && target.IsAlive)
            {
                Vector3 delta = target.transform.position - transform.position;
                delta.y = 0; // Maintain 2D ground plane (consistent with MovementComponent)
                float distance = delta.magnitude;
                if (distance <= ownerEntity.AttackRange + 0.1f)
                {
                    attackTimer = 0f;
                    BasicAttackProcessor.ExecuteBasicAttack(ownerEntity, target, combatConfig);
                }
            }
        }
    }
}
