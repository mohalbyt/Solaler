using UnityEngine;

namespace Solaler.Dungeon
{
    /// <summary>
    /// Pushable block that the player can push onto pressure plates or into puzzle configurations.
    /// Uses 2D physics with heavy linear drag and locked rotation for clean grid-aligned pushing.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PushableBlock : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private Rigidbody2D rb;

        private void Reset()
        {
            rb = GetComponent<Rigidbody2D>();
            ConfigurePhysics();
        }

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            ConfigurePhysics();
        }

        private void ConfigurePhysics()
        {
            if (rb == null) return;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearDamping = 12f; // Dampen quickly so block doesn't slide endlessly
            rb.angularDamping = 10f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }
}
