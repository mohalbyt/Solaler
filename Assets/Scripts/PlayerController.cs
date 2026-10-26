using System;
using UnityEngine;
using Solaler.Player.Animation;
using Solaler.Player.Combat;
using Solaler.Player.Core;
using Solaler.Player.Input;
using Solaler.Player.Movement;
using Solaler.Player.StateMachine;
using Solaler.Player.StateMachine.States;

namespace Solaler.Player
{
    /// <summary>
    /// Master coordinator for the top-down Player character (The Binding of Isaac style).
    /// Follows SOLID principles:
    /// - SRP: Coordinates subsystems without handling physics or combat itself.
    /// - OCP: Extensible via state machine and ScriptableObject tuning.
    /// - DIP: Decoupled through abstractions (IPlayerInput, IMovementController).
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerAnimationController))]
    [RequireComponent(typeof(PlayerInputReader))]
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(PlayerHealth))]
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The ScriptableObject containing movement and combat tuning values.")]
        [SerializeField] private PlayerMovementData movementData;

        [Header("Subsystem References (Auto-assigned if empty)")]
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerAnimationController animationController;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private PlayerHealth health;

        // State Machine & Concrete States (pre-allocated once for zero-GC performance)
        public PlayerStateMachine StateMachine { get; private set; }
        public PlayerIdleState IdleState { get; private set; }
        public PlayerMoveState MoveState { get; private set; }
        public PlayerDashState DashState { get; private set; }
        public PlayerDeadState DeadState { get; private set; }

        // Abstraction Accessors
        public IPlayerInput Input => inputReader;
        public PlayerMovement Movement => movement;
        public PlayerAnimationController Anim => animationController;
        public PlayerCombat Combat => combat;
        public PlayerHealth Health => health;
        public PlayerMovementData Data => movementData;

        // Cooldown Timers
        private float _dashCooldownTimer;
        public bool CanDash => _dashCooldownTimer <= 0f && !health.IsDead;

        // Events
        public event Action<PlayerBaseState> OnStateChanged;

        private void Reset()
        {
            movement = GetComponent<PlayerMovement>();
            animationController = GetComponent<PlayerAnimationController>();
            inputReader = GetComponent<PlayerInputReader>();
            combat = GetComponent<PlayerCombat>();
            health = GetComponent<PlayerHealth>();
        }

        private void Awake()
        {
            InitializeDependencies();
            InitializeStateMachine();
        }

        private void Start()
        {
            StateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            if (health.IsDead) return;

            UpdateTimers();

            StateMachine.CurrentState.HandleInput();
            StateMachine.CurrentState.LogicUpdate();

            // Handle shooting independently from movement state (Isaac style: move & shoot freely!)
            combat.TryShoot(Input, Data);

            // Synchronize animation with current physical velocity
            animationController.UpdateMovement(movement.Velocity.magnitude);
        }

        private void FixedUpdate()
        {
            StateMachine.CurrentState.PhysicsUpdate();
        }

        private void InitializeDependencies()
        {
            if (movement == null) movement = GetComponent<PlayerMovement>();
            if (animationController == null) animationController = GetComponent<PlayerAnimationController>();
            if (inputReader == null) inputReader = GetComponent<PlayerInputReader>();
            if (combat == null) combat = GetComponent<PlayerCombat>();
            if (health == null) health = GetComponent<PlayerHealth>();

            if (movementData == null)
            {
                movementData = Resources.Load<PlayerMovementData>("DefaultPlayerMovementData");
                if (movementData == null)
                {
                    movementData = ScriptableObject.CreateInstance<PlayerMovementData>();
                }
            }

            health.OnDeath += HandleDeath;
        }

        private void InitializeStateMachine()
        {
            StateMachine = new PlayerStateMachine();

            IdleState = new PlayerIdleState(this, StateMachine);
            MoveState = new PlayerMoveState(this, StateMachine);
            DashState = new PlayerDashState(this, StateMachine);
            DeadState = new PlayerDeadState(this, StateMachine);

            StateMachine.OnStateChanged += state => OnStateChanged?.Invoke(state);
        }

        private void UpdateTimers()
        {
            if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer -= Time.deltaTime;
            }
        }

        public void StartDashCooldown()
        {
            _dashCooldownTimer = movementData.dashCooldown;
        }

        private void HandleDeath()
        {
            StateMachine.ChangeState(DeadState);
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath -= HandleDeath;
            }
        }
    }
}
