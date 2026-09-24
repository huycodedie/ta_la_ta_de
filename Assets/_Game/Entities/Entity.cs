using UnityEngine;
using WuxiaGame.Combat;
using WuxiaGame.Core;
using WuxiaGame.Entities.Components;
using WuxiaGame.Stats;

namespace WuxiaGame.Entities
{
    [RequireComponent(typeof(EntityStatsComponent))]
    public abstract class Entity : MonoBehaviour
    {
        [Header("Entity Info")]
        [SerializeField] protected EntityType entityType;
        [SerializeField] protected string entityName = "Entity";
        [SerializeField] protected float attackRange = 1.8f;

        private EntityStatsComponent statsComp;
        private HealthComponent healthComp;
        private RageComponent rageComp;
        private MovementComponent movementComp;
        private AttackComponent attackComp;
        private EntityStatusController statusComp;

        public EntityType EntityType => entityType;
        public string EntityName => entityName;
        public float AttackRange => attackRange;

        public EntityStatsComponent Stats => statsComp != null ? statsComp : (statsComp = GetComponent<EntityStatsComponent>());
        public HealthComponent Health => healthComp != null ? healthComp : (healthComp = GetComponent<HealthComponent>());
        public RageComponent Rage => rageComp != null ? rageComp : (rageComp = GetComponent<RageComponent>());
        public MovementComponent Movement => movementComp != null ? movementComp : (movementComp = GetComponent<MovementComponent>());
        public AttackComponent Attack => attackComp != null ? attackComp : (attackComp = GetComponent<AttackComponent>());
        public EntityStatusController StatusController
        {
            get
            {
                if (statusComp == null)
                {
                    statusComp = GetComponent<EntityStatusController>() ?? gameObject.AddComponent<EntityStatusController>();
                    statusComp.Initialize(this);
                }
                return statusComp;
            }
        }

        public Entity CurrentTarget { get; private set; }
        public bool IsAlive => Health == null || Health.IsAlive;

        // P07.6 Action permissions
        public virtual bool CanMove => (StatusController == null || StatusController.CanMove) && !IsCasting;
        public virtual bool CanBasicAttack => (StatusController == null || StatusController.CanBasicAttack) && !IsCasting;
        public virtual bool CanUseSkill => StatusController == null || StatusController.CanUseSkill;
        public virtual bool CanUseUltimate => StatusController == null || StatusController.CanUseUltimate;
        public virtual bool CanDash => (StatusController == null || StatusController.CanDash) && !IsCasting;
        public virtual bool IsStunned => StatusController != null && StatusController.IsStunned;
        public virtual bool IsRooted => StatusController != null && StatusController.IsRooted;
        public virtual bool IsFrozen => StatusController != null && StatusController.IsFrozen;
        public virtual float CcResistance => Stats != null ? Stats.GetValue(StatType.CcResistance, 0f) : 0f;

        private SkillCastState castState;
        public SkillCastState CastState => castState ?? (castState = new SkillCastState());
        public virtual bool IsCasting => CastState != null && CastState.IsActive;
        public virtual bool IsChanneling => CastState != null && CastState.CurrentPhase == SkillCastPhase.Channeling;
        public virtual float CastProgress => CastState != null ? CastState.Progress : 0f;

        public virtual void TickActiveCast(float deltaTime)
        {
            if (CastState != null && CastState.IsActive)
            {
                CastState.Tick(deltaTime);
            }
        }

        protected virtual void Update()
        {
            bool canTick = false;
            if (BattleManager.Instance == null)
            {
                canTick = true;
            }
            else
            {
                if (BattleManager.Instance.IsBattleActive)
                {
                    canTick = true;
                }
                else if (BattleManager.Instance.CanEntityTickDuringTransition(this))
                {
                    canTick = true;
                }
            }

            if (!canTick)
                return;

            if (!IsAlive || !IsCasting)
                return;

            TickActiveCast(GetDeltaTime());
        }

        protected virtual float GetDeltaTime() => Time.deltaTime;

        protected virtual void Awake()
        {
            statsComp = GetComponent<EntityStatsComponent>();
            healthComp = GetComponent<HealthComponent>();
            rageComp = GetComponent<RageComponent>();
            movementComp = GetComponent<MovementComponent>();
            attackComp = GetComponent<AttackComponent>();
            statusComp = GetComponent<EntityStatusController>();
            if (statusComp == null)
            {
                statusComp = gameObject.AddComponent<EntityStatusController>();
            }
            statusComp.Initialize(this);
            castState = new SkillCastState();
        }

        public void SetCurrentTarget(Entity target)
        {
            CurrentTarget = target;
        }

        public virtual void DisableEntityActions()
        {
            if (Movement != null) Movement.SetMovementEnabled(false);
            if (Attack != null) Attack.SetAttackEnabled(false);
        }

        public virtual void EnableEntityActions()
        {
            if (Movement != null) Movement.SetMovementEnabled(true);
            if (Attack != null) Attack.SetAttackEnabled(true);
        }

        public virtual void InterruptCurrentAction(SkillCastInterruptSource source = SkillCastInterruptSource.CrowdControl)
        {
            if (Attack != null)
            {
                Attack.ResetAttackTimer();
            }

            if (IsCasting && CastState != null)
            {
                if (source == SkillCastInterruptSource.CrowdControl)
                {
                    if (IsStunned) source = SkillCastInterruptSource.Stun;
                    else if (IsFrozen) source = SkillCastInterruptSource.Freeze;
                }
                CastState.Interrupt(source);
            }
        }

        public virtual void RecalculateStats(bool preserveHpDelta = true)
        {
        }
    }
}
