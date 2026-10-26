using UnityEngine;

namespace Solaler.Player.Animation
{
    /// <summary>
    /// Coordinates Top-Down Animator parameters and states efficiently.
    /// Uses pre-cached hashes to achieve zero-allocation per-frame updates.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Pre-cached Animator Parameter Hashes
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int IsDashingHash = Animator.StringToHash("IsDashing");
        private static readonly int DieTriggerHash = Animator.StringToHash("Die");

        public Animator Animator => animator;
        public SpriteRenderer SpriteRenderer => spriteRenderer;

        private void Reset()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public void UpdateMovement(float speed)
        {
            if (animator == null) return;
            bool isMoving = speed > 0.05f;
            animator.SetFloat(SpeedHash, speed);
            animator.SetBool(IsMovingHash, isMoving);
        }

        public void SetDashing(bool isDashing)
        {
            if (animator == null) return;
            animator.SetBool(IsDashingHash, isDashing);
        }

        public void TriggerDeath()
        {
            if (animator == null) return;
            animator.SetTrigger(DieTriggerHash);
        }
    }
}
