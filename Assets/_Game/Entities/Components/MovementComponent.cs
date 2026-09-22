using UnityEngine;
using WuxiaGame.Core;

namespace WuxiaGame.Entities.Components
{
    public class MovementComponent : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Entity ownerEntity;
        private bool isMovementEnabled = true;

        public float MoveSpeed => moveSpeed;
        public float CurrentMoveSpeed => moveSpeed;
        public bool IsMovementEnabled => isMovementEnabled;
        public Entity OwnerEntity => ownerEntity != null ? ownerEntity : (ownerEntity = GetComponent<Entity>());

        private void Awake()
        {
            if (ownerEntity == null)
            {
                ownerEntity = GetComponent<Entity>();
            }
        }

        public void InitializeSpeed(float speed, Entity owner = null)
        {
            moveSpeed = speed;
            if (owner != null) ownerEntity = owner;
            else if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
        }

        public void SetMovementEnabled(bool enabled)
        {
            isMovementEnabled = enabled;
        }

        public void ManualTick(float deltaTime)
        {
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (!isMovementEnabled || ownerEntity == null || !ownerEntity.IsAlive || !ownerEntity.CanMove) return;
            if (BattleManager.Instance != null && !BattleManager.Instance.IsBattleActive) return;

            if (ownerEntity.CurrentTarget != null && ownerEntity.CurrentTarget.IsAlive)
            {
                MoveTowardTarget(ownerEntity.CurrentTarget.transform.position, ownerEntity.AttackRange, deltaTime);
            }
        }

        private void Update()
        {
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (!isMovementEnabled || ownerEntity == null || !ownerEntity.IsAlive || !ownerEntity.CanMove) return;
            if (BattleManager.Instance != null && !BattleManager.Instance.IsBattleActive) return;

            // Auto-move toward active target if set
            if (ownerEntity.CurrentTarget != null && ownerEntity.CurrentTarget.IsAlive)
            {
                MoveTowardTarget(ownerEntity.CurrentTarget.transform.position, ownerEntity.AttackRange);
            }
        }

        public void MoveTowardTarget(Vector3 targetPosition, float stoppingDistance, float customDeltaTime = -1f)
        {
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity != null && !ownerEntity.CanMove) return;

            float dt = customDeltaTime >= 0f ? customDeltaTime : Time.deltaTime;
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0; // Maintain 2D ground plane (X axis movement)
            float distance = direction.magnitude;

            if (distance > stoppingDistance)
            {
                Vector3 moveDelta = direction.normalized * (CurrentMoveSpeed * dt);
                transform.position += moveDelta;
            }
        }

        public void MoveDirection(float directionX, float customDeltaTime = -1f)
        {
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity != null && !ownerEntity.CanMove) return;

            float dt = customDeltaTime >= 0f ? customDeltaTime : Time.deltaTime;
            Vector3 moveDelta = new Vector3(Mathf.Sign(directionX), 0, 0) * (CurrentMoveSpeed * dt);
            transform.position += moveDelta;
        }
    }
}
