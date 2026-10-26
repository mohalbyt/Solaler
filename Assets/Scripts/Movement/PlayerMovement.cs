using UnityEngine;
using Solaler.Player.Core;

namespace Solaler.Player.Movement
{
    /// <summary>
    /// Handles physical 2D top-down movement and velocity calculations.
    /// Implements IMovementController with zero gravity for top-down gameplay.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public class PlayerMovement : MonoBehaviour, IMovementController
    {
        [Header("Dependencies")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Transform visualTransform;

        // Internal State
        private bool _isFacingRight = true;

        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
        public bool IsFacingRight => _isFacingRight;
        public Rigidbody2D Rigidbody => rb;

        private void Reset()
        {
            rb = GetComponent<Rigidbody2D>();
            if (visualTransform == null)
            {
                visualTransform = transform;
            }
        }

        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }

            if (visualTransform == null)
            {
                visualTransform = transform;
            }

            // Top-down physics configuration
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        public void SetVelocity(Vector2 velocity)
        {
            if (rb == null) return;
            rb.linearVelocity = velocity;
        }

        public void Accelerate(Vector2 targetVelocity, float rate)
        {
            if (rb == null) return;
            Vector2 current = rb.linearVelocity;
            Vector2 newVel = Vector2.MoveTowards(current, targetVelocity, rate * Time.fixedDeltaTime);
            rb.linearVelocity = newVel;
        }

        public void Stop()
        {
            SetVelocity(Vector2.zero);
        }

        public void FaceDirection(float directionX)
        {
            if (Mathf.Abs(directionX) < 0.05f) return;

            bool shouldFaceRight = directionX > 0f;
            if (shouldFaceRight != _isFacingRight)
            {
                _isFacingRight = shouldFaceRight;
                Vector3 scale = visualTransform.localScale;
                scale.x = Mathf.Abs(scale.x) * (_isFacingRight ? 1f : -1f);
                visualTransform.localScale = scale;
            }
        }
    }
}
