using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Solaler.Player.Core;

namespace Solaler.Player.Input
{
    /// <summary>
    /// Reads top-down movement and aiming inputs.
    /// Resilient: supports both Unity New Input System and direct legacy input fallbacks,
    /// and correctly converts mouse screen position to 2D world coordinates for accurate Isaac-style aiming.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerInputReader : MonoBehaviour, IPlayerInput
    {
        [Header("Input Action Asset Reference")]
        [Tooltip("The InputActionAsset defining the control bindings.")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Buffer Configuration")]
        [SerializeField] private float attackBufferDuration = 0.15f;
        [SerializeField] private float dashBufferDuration = 0.12f;

        private InputAction _moveAction;
        private InputAction _attackAction;
        private InputAction _dashAction;

        private Camera _cachedCamera;

        private Vector2 _moveInput;
        private Vector2 _aimDirection = Vector2.right;
        private Vector2 _mouseWorldPosition;
        private bool _attackHeld;

        private float _attackBufferTimer;
        private float _dashBufferTimer;

        public event Action OnAttackTriggered;
        public event Action OnDashTriggered;

        public Vector2 MoveInput => _moveInput;
        public Vector2 AimDirection => _aimDirection;
        public Vector2 MouseWorldPosition => _mouseWorldPosition;
        public bool AttackPressed => _attackBufferTimer > 0f;
        public bool AttackHeld => _attackHeld;
        public bool DashTriggered => _dashBufferTimer > 0f;

        private void Awake()
        {
            _cachedCamera = Camera.main;
            InitializeInputActions();
        }

        private void OnEnable()
        {
            Enable();
        }

        private void OnDisable()
        {
            Disable();
        }

        private void Update()
        {
            PollFallbackInputs();
            UpdateAimDirection();
            UpdateBuffers();
        }

        public void Enable()
        {
            if (inputActions != null)
            {
                inputActions.Enable();
            }
            HookEvents();
        }

        public void Disable()
        {
            UnhookEvents();
            if (inputActions != null)
            {
                inputActions.Disable();
            }
            _moveInput = Vector2.zero;
            _attackHeld = false;
            _attackBufferTimer = 0f;
            _dashBufferTimer = 0f;
        }

        public void ConsumeAttack()
        {
            _attackBufferTimer = 0f;
        }

        public void ConsumeDash()
        {
            _dashBufferTimer = 0f;
        }

        private void InitializeInputActions()
        {
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            }

            if (inputActions != null)
            {
                _moveAction = inputActions.FindAction("Player/Move");
                _attackAction = inputActions.FindAction("Player/Attack");
                _dashAction = inputActions.FindAction("Player/Jump") ?? inputActions.FindAction("Player/Sprint");
            }
        }

        private void HookEvents()
        {
            if (_moveAction != null)
            {
                _moveAction.performed += HandleMove;
                _moveAction.canceled += HandleMove;
            }

            if (_attackAction != null)
            {
                _attackAction.started += HandleAttackStarted;
                _attackAction.canceled += HandleAttackCanceled;
            }

            if (_dashAction != null)
            {
                _dashAction.started += HandleDashStarted;
            }
        }

        private void UnhookEvents()
        {
            if (_moveAction != null)
            {
                _moveAction.performed -= HandleMove;
                _moveAction.canceled -= HandleMove;
            }

            if (_attackAction != null)
            {
                _attackAction.started -= HandleAttackStarted;
                _attackAction.canceled -= HandleAttackCanceled;
            }

            if (_dashAction != null)
            {
                _dashAction.started -= HandleDashStarted;
            }
        }

        private void HandleMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
            if (_moveInput.sqrMagnitude > 1f)
            {
                _moveInput.Normalize();
            }
        }

        private void HandleAttackStarted(InputAction.CallbackContext context)
        {
            _attackHeld = true;
            _attackBufferTimer = attackBufferDuration;
            OnAttackTriggered?.Invoke();
        }

        private void HandleAttackCanceled(InputAction.CallbackContext context)
        {
            _attackHeld = false;
        }

        private void HandleDashStarted(InputAction.CallbackContext context)
        {
            _dashBufferTimer = dashBufferDuration;
            OnDashTriggered?.Invoke();
        }

        /// <summary>
        /// Direct polling fallback ensures shooting and moving work seamlessly even if Input Actions asset wasn't active.
        /// </summary>
        private void PollFallbackInputs()
        {
            // If Input System action didn't update moveInput, poll keyboard/mouse directly
            if (Keyboard.current != null)
            {
                float x = 0f;
                float y = 0f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;

                if (x != 0f || y != 0f)
                {
                    _moveInput = new Vector2(x, y).normalized;
                }
                else if (_moveAction == null || !_moveAction.enabled)
                {
                    _moveInput = Vector2.zero;
                }

                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    _dashBufferTimer = dashBufferDuration;
                    OnDashTriggered?.Invoke();
                }
            }

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _attackBufferTimer = attackBufferDuration;
                    OnAttackTriggered?.Invoke();
                }
                _attackHeld = Mouse.current.leftButton.isPressed;
            }
        }

        private void UpdateAimDirection()
        {
            if (_cachedCamera == null)
            {
                _cachedCamera = Camera.main;
                if (_cachedCamera == null) return;
            }

            Vector2 mouseScreen = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)UnityEngine.Input.mousePosition;
            Vector3 worldPos = _cachedCamera.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -_cachedCamera.transform.position.z));
            _mouseWorldPosition = new Vector2(worldPos.x, worldPos.y);

            Vector2 playerPos = (Vector2)transform.position;
            Vector2 diff = _mouseWorldPosition - playerPos;
            if (diff.sqrMagnitude > 0.0001f)
            {
                _aimDirection = diff.normalized;
            }
        }

        private void UpdateBuffers()
        {
            if (_attackBufferTimer > 0f)
            {
                _attackBufferTimer -= Time.deltaTime;
            }

            if (_dashBufferTimer > 0f)
            {
                _dashBufferTimer -= Time.deltaTime;
            }
        }
    }
}
