using UnityEngine;
using UnityEngine.SceneManagement;
using WuxiaGame.Core;

namespace WuxiaGame.Entities.Components
{
    public class DashExecutionPlan
    {
        private static long s_nextTransactionId = 0;
        public long TransactionId { get; }
        public Entity Source { get; }
        public Entity BoundTarget { get; }
        public Vector3 Direction { get; }
        public float TotalPlannedDistance { get; }
        public float Speed { get; }
        public float StoppingDistance { get; }
        public int BoundEncounterIndex { get; }
        public BattleManager BoundBattleManager { get; }
        public Vector3 PlannedStartPosition { get; }
        public float PlannedEndpointX { get; }

        public DashExecutionPlan(
            Entity source,
            Entity boundTarget,
            Vector3 direction,
            float totalPlannedDistance,
            float speed,
            float stoppingDistance,
            int boundEncounterIndex,
            BattleManager boundBattleManager,
            Vector3? plannedStartPosition = null,
            float? plannedEndpointX = null)
        {
            TransactionId = System.Threading.Interlocked.Increment(ref s_nextTransactionId);
            Source = source;
            BoundTarget = boundTarget;
            Direction = direction;
            TotalPlannedDistance = totalPlannedDistance;
            Speed = speed;
            StoppingDistance = stoppingDistance;
            BoundEncounterIndex = boundEncounterIndex;
            BoundBattleManager = boundBattleManager;
            PlannedStartPosition = plannedStartPosition ?? (source != null ? source.transform.position : Vector3.zero);
            PlannedEndpointX = plannedEndpointX ?? (PlannedStartPosition.x + direction.x * totalPlannedDistance);
        }
    }

    public class MovementComponent : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Entity ownerEntity;
        private bool isMovementEnabled = true;

        // Dash Foundation (P09-B)
        private DashExecutionPlan activeDashPlan;
        private DashExecutionPlan reservedDashPlan;
        private float dashRemainingDistance = 0f;
        private Vector3 dashDirection = Vector3.zero;
        private float dashSpeed = 0f;
        private System.Action onDashComplete;
        private System.Action onDashAbort;

        public float MoveSpeed => moveSpeed;
        public float CurrentMoveSpeed => moveSpeed;
        public bool IsMovementEnabled => isMovementEnabled;
        public Entity OwnerEntity => ownerEntity != null ? ownerEntity : (ownerEntity = GetComponent<Entity>());

        public bool IsDashing => activeDashPlan != null;
        public bool IsStartingDash => reservedDashPlan != null;
        public float DashRemainingDistance => dashRemainingDistance;
        public Vector3 DashDirection => dashDirection;
        public float DashSpeed => dashSpeed;
        public DashExecutionPlan ActiveDashPlan => activeDashPlan;
        public DashExecutionPlan ReservedDashPlan => reservedDashPlan;

        public event System.Action<long> OnDashStarted;
        public event System.Action<long> OnDashCompleted;
        public event System.Action<long> OnDashAborted;

        private void Awake()
        {
            if (ownerEntity == null)
            {
                ownerEntity = GetComponent<Entity>();
            }
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosed -= HandleEditorSceneClosed;
#endif
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            if (activeDashPlan != null)
            {
                AbortDash(activeDashPlan.TransactionId);
            }
            ReleaseDashReservation(0);
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosed -= HandleEditorSceneClosed;
#endif
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            if (activeDashPlan != null)
            {
                AbortDash(activeDashPlan.TransactionId);
            }
            ReleaseDashReservation(0);
        }

#if UNITY_EDITOR
        private void HandleEditorSceneClosed(Scene scene)
        {
            if (activeDashPlan != null)
            {
                AbortDash(activeDashPlan.TransactionId);
            }
        }
#endif

        private void HandleSceneUnloaded(Scene scene)
        {
            if (activeDashPlan != null)
            {
                AbortDash(activeDashPlan.TransactionId);
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

        public bool TryReserveDashStartup(DashExecutionPlan plan)
        {
            if (activeDashPlan != null || reservedDashPlan != null) return false;
            if (plan == null) return false;
            if (!enabled || !gameObject.activeInHierarchy || !isMovementEnabled) return false;

            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity == null || !ownerEntity.gameObject.activeInHierarchy || !ownerEntity.IsAlive || (ownerEntity.Health != null && ownerEntity.Health.CurrentHealth <= 0f))
            {
                return false;
            }

            if (!ownerEntity.CanMove || !ownerEntity.CanDash)
            {
                return false;
            }

            if (plan.Source != ownerEntity) return false;

            reservedDashPlan = plan;
            return true;
        }

        public void ReleaseDashReservation(long transactionId = 0)
        {
            if (reservedDashPlan != null)
            {
                if (transactionId == 0 || reservedDashPlan.TransactionId == transactionId)
                {
                    reservedDashPlan = null;
                }
            }
        }

        public bool TryStartDash(DashExecutionPlan plan, System.Action onComplete = null, System.Action onAbort = null, System.Action onCommit = null)
        {
            if (activeDashPlan != null) return false;
            if (plan == null) return false;
            if (!enabled || !gameObject.activeInHierarchy) return false;

            // If a reservation exists, it must match this transaction
            if (reservedDashPlan != null && reservedDashPlan.TransactionId != plan.TransactionId)
            {
                return false;
            }

            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity == null || !ownerEntity.gameObject.activeInHierarchy || !ownerEntity.IsAlive || (ownerEntity.Health != null && ownerEntity.Health.CurrentHealth <= 0f))
            {
                return false;
            }

            if (!isMovementEnabled || !ownerEntity.CanMove || !ownerEntity.CanDash)
            {
                return false;
            }

            if (plan.Source != ownerEntity) return false;

            var target = plan.BoundTarget;
            if (target == null || (target is UnityEngine.Object ut && ut == null) ||
                !target.gameObject.activeInHierarchy || !target.IsAlive || (target.Health != null && target.Health.CurrentHealth <= 0f))
            {
                return false;
            }

            if (plan.Speed <= 0f || float.IsNaN(plan.Speed) || float.IsInfinity(plan.Speed)) return false;
            if (plan.TotalPlannedDistance <= 0.001f || float.IsNaN(plan.TotalPlannedDistance) || float.IsInfinity(plan.TotalPlannedDistance)) return false;
            if (float.IsNaN(plan.Direction.x) || float.IsInfinity(plan.Direction.x) || Mathf.Abs(plan.Direction.x) < 0.0001f) return false;

            // R2 requirement: Dash P09-B requires valid alive BoundBattleManager and Current BattleManager (No managerless mode!)
            var boundBM = plan.BoundBattleManager;
            if (boundBM == null || (boundBM is UnityEngine.Object uBound && uBound == null))
            {
                return false;
            }

            var currentBM = BattleManager.Instance;
            if (currentBM == null || (currentBM is UnityEngine.Object uCurr && uCurr == null))
            {
                return false;
            }

            if (currentBM != boundBM || currentBM.EncounterIndex != plan.BoundEncounterIndex)
            {
                return false;
            }

            if (!currentBM.IsBattleActive || currentBM.IsCombatPausedByUI)
            {
                return false;
            }

            var bState = currentBM.CurrentBattleState;
            if (bState != BattleState.InProgress)
            {
                return false;
            }

            // Verify encounter membership for both source and target
            if (!IsEntityInEncounter(currentBM, target) || !IsEntityInEncounter(currentBM, ownerEntity))
            {
                return false;
            }

            reservedDashPlan = null; // Committed, clear reservation
            activeDashPlan = plan;
            dashRemainingDistance = plan.TotalPlannedDistance;
            dashDirection = new Vector3(Mathf.Sign(plan.Direction.x), 0f, 0f);
            dashSpeed = plan.Speed;
            onDashComplete = onComplete;
            onDashAbort = onAbort;

#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosed += HandleEditorSceneClosed;
#endif
            SceneManager.sceneUnloaded += HandleSceneUnloaded;

            // Execute atomic transaction commit action BEFORE notifying observers
            try
            {
                onCommit?.Invoke();
            }
            catch (System.Exception commitEx)
            {
                Debug.LogError($"[MOVEMENT:DASH] Exception during onCommit callback for transaction {plan.TransactionId}: {commitEx}");
                throw;
            }

            // Observers are notified AFTER commit is established; observer failure cannot rollback committed transaction
            try
            {
                OnDashStarted?.Invoke(plan.TransactionId);
            }
            catch (System.Exception obsEx)
            {
                Debug.LogError($"[MOVEMENT:DASH] Observer exception in OnDashStarted for transaction {plan.TransactionId}: {obsEx}");
            }
            return true;
        }

        public void CompleteDash(long transactionId = 0)
        {
            if (activeDashPlan == null) return;
            if (transactionId != 0 && activeDashPlan.TransactionId != transactionId) return;

            long txId = activeDashPlan.TransactionId;
            var callback = onDashComplete;
            ClearDashState();
            try
            {
                callback?.Invoke();
            }
            finally
            {
                OnDashCompleted?.Invoke(txId);
            }
        }

        public void AbortDash(long transactionId = 0)
        {
            if (activeDashPlan == null) return;
            if (transactionId != 0 && activeDashPlan.TransactionId != transactionId) return;

            long txId = activeDashPlan.TransactionId;
            var callback = onDashAbort;
            ClearDashState();
            try
            {
                callback?.Invoke();
            }
            finally
            {
                OnDashAborted?.Invoke(txId);
            }
        }

        public void ClearDashState()
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.sceneClosed -= HandleEditorSceneClosed;
#endif
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            activeDashPlan = null;
            dashRemainingDistance = 0f;
            dashDirection = Vector3.zero;
            dashSpeed = 0f;
            onDashComplete = null;
            onDashAbort = null;
        }

        public void ManualTick(float deltaTime)
        {
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity == null) return;

            if (IsDashing)
            {
                TickDashStep(deltaTime);
                return;
            }

            if (!isMovementEnabled || !ownerEntity.IsAlive || !ownerEntity.CanMove) return;
            if (BattleManager.Instance != null && !BattleManager.Instance.IsBattleActive) return;

            if (ownerEntity.CurrentTarget != null && ownerEntity.CurrentTarget.IsAlive)
            {
                MoveTowardTarget(ownerEntity.CurrentTarget.transform.position, ownerEntity.AttackRange, deltaTime);
            }
        }

        private void Update()
        {
            if (!Application.isPlaying) return;
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity == null) return;

            if (IsDashing)
            {
                TickDashStep(Time.deltaTime);
                return;
            }

            if (!isMovementEnabled || !ownerEntity.IsAlive || !ownerEntity.CanMove) return;
            if (BattleManager.Instance != null && !BattleManager.Instance.IsBattleActive) return;

            // Auto-move toward active target if set
            if (ownerEntity.CurrentTarget != null && ownerEntity.CurrentTarget.IsAlive)
            {
                MoveTowardTarget(ownerEntity.CurrentTarget.transform.position, ownerEntity.AttackRange);
            }
        }

        private void TickDashStep(float dt)
        {
            if (activeDashPlan == null) return;
            long txId = activeDashPlan.TransactionId;

            // 1. Invalidation / Component / GameObject / Owner liveness
            if (!enabled || !gameObject.activeInHierarchy)
            {
                AbortDash(txId);
                return;
            }

            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity == null || !ownerEntity.gameObject.activeInHierarchy || !ownerEntity.IsAlive || (ownerEntity.Health != null && ownerEntity.Health.CurrentHealth <= 0f))
            {
                AbortDash(txId);
                return;
            }

            // 2. CC check: Stun, Root, Freeze
            if (ownerEntity.IsStunned || ownerEntity.IsRooted || ownerEntity.IsFrozen ||
                (ownerEntity.StatusController != null && !ownerEntity.StatusController.CanDash))
            {
                AbortDash(txId);
                return;
            }

            // 3. Manager Liveness and Identity: FAIL-CLOSED!
            var boundBM = activeDashPlan.BoundBattleManager;
            if (boundBM == null || (boundBM is UnityEngine.Object uBound && uBound == null))
            {
                AbortDash(txId);
                return;
            }

            var currentBM = BattleManager.Instance;
            if (currentBM == null || (currentBM is UnityEngine.Object uCurr && uCurr == null))
            {
                AbortDash(txId);
                return;
            }

            if (currentBM != boundBM || currentBM.EncounterIndex != activeDashPlan.BoundEncounterIndex)
            {
                AbortDash(txId);
                return;
            }

            // 4. Terminal Battle State check
            var bState = currentBM.CurrentBattleState;
            if (bState != BattleState.InProgress)
            {
                AbortDash(txId);
                return;
            }

            // 5. Target validity & liveness
            Entity target = activeDashPlan.BoundTarget;
            if (target == null || (target is UnityEngine.Object uTarget && uTarget == null) ||
                !target.gameObject.activeInHierarchy || !target.IsAlive || (target.Health != null && target.Health.CurrentHealth <= 0f))
            {
                AbortDash(txId);
                return;
            }

            // 6. Source and Target encounter membership check
            if (!IsEntityInEncounter(currentBM, target) || !IsEntityInEncounter(currentBM, ownerEntity))
            {
                AbortDash(txId);
                return;
            }

            // 7. UI Pause check: valid UI pause freezes movement step without aborting or losing distance
            if (currentBM.IsCombatPausedByUI)
            {
                return;
            }

            if (!currentBM.IsBattleActive)
            {
                AbortDash(txId);
                return;
            }

            // 8. Movement permission check (outside UI pause)
            if (!isMovementEnabled || (ownerEntity.StatusController != null && !ownerEntity.StatusController.CanMove))
            {
                AbortDash(txId);
                return;
            }

            if (dt <= 0f) return;

            // 9. Actual 1D displacement on pure horizontal X axis with stopping distance clamping
            float currentHeroX = transform.position.x;
            float currentTargetX = target.transform.position.x;
            float diffX = currentTargetX - currentHeroX;
            float currentDistToTarget = Mathf.Abs(diffX);
            float stoppingDist = activeDashPlan.StoppingDistance;
            float signX = Mathf.Sign(dashDirection.x);

            // Check if target crossed past the hero along the dash direction or is already within stopping distance
            if (diffX * signX <= 0f || currentDistToTarget <= stoppingDist)
            {
                CompleteDash(txId);
                return;
            }

            float remainingSpaceToTarget = Mathf.Max(0f, currentDistToTarget - stoppingDist);
            float stepDistance = dashSpeed * dt;
            float actualStep = Mathf.Min(stepDistance, dashRemainingDistance);
            actualStep = Mathf.Min(actualStep, remainingSpaceToTarget);

            if (actualStep > 0.0001f)
            {
                Vector3 pos = transform.position;
                pos.x += signX * actualStep;
                // Preserve exact Y and Z coordinates
                transform.position = pos;
                dashRemainingDistance -= actualStep;
            }

            if (dashRemainingDistance <= 0.001f || actualStep <= 0.0001f)
            {
                CompleteDash(txId);
            }
        }

        private static bool IsEntityInEncounter(BattleManager bm, Entity entity)
        {
            if (bm == null || entity == null) return false;
            if (entity is UnityEngine.Object uEnt && uEnt == null) return false;

            if (entity is Hero hero)
            {
                if (bm.CurrentHero == hero) return true;
                return bm.BelongsToEncounter(hero);
            }

            if (entity is Monster monster)
            {
                if (bm.ActiveMonsters != null)
                {
                    var activeList = bm.ActiveMonsters;
                    for (int i = 0; i < activeList.Count; i++)
                    {
                        if (activeList[i] == monster) return true;
                    }
                }
                if (bm.CurrentMonster == monster) return true;
                return bm.BelongsToEncounter(monster);
            }

            return bm.BelongsToEncounter(entity);
        }

        public void MoveTowardTarget(Vector3 targetPosition, float stoppingDistance, float customDeltaTime = -1f)
        {
            if (IsDashing) return;
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity != null && !ownerEntity.CanMove) return;

            float dt = customDeltaTime >= 0f ? customDeltaTime : Time.deltaTime;
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0; // Maintain 2D ground plane (X axis movement)
            float distance = direction.magnitude;

            if (distance > stoppingDistance)
            {
                Vector3 moveDelta = direction.normalized * (CurrentMoveSpeed * dt);
                moveDelta.y = 0;
                transform.position += moveDelta;
            }
        }

        public void MoveDirection(float directionX, float customDeltaTime = -1f)
        {
            if (IsDashing) return;
            if (ownerEntity == null) ownerEntity = GetComponent<Entity>();
            if (ownerEntity != null && !ownerEntity.CanMove) return;

            float dt = customDeltaTime >= 0f ? customDeltaTime : Time.deltaTime;
            Vector3 moveDelta = new Vector3(Mathf.Sign(directionX), 0, 0) * (CurrentMoveSpeed * dt);
            moveDelta.y = 0;
            transform.position += moveDelta;
        }
    }
}
