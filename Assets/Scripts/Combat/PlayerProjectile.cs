using UnityEngine;

namespace Solaler.Player.Combat
{
    /// <summary>
    /// Top-down projectile (inspired by tears/shots in The Binding of Isaac).
    /// Travels smoothly in aiming direction with zero gravity and impacts walls/enemies.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public class PlayerProjectile : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private float _damage = 10f;
        private float _lifetime = 1.2f;
        private float _spawnTime;
        private GameObject _shooter;

        private void Reset()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        public void Initialize(Vector2 direction, float speed, float damage, float lifetime, GameObject shooter = null)
        {
            _damage = damage;
            _lifetime = lifetime;
            _spawnTime = Time.time;
            _shooter = shooter;

            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * speed;
            }

            // Align rotation with travel trajectory
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            if (Time.time - _spawnTime >= _lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore shooter and other projectiles
            if (other.gameObject == _shooter || other.transform.root == (_shooter != null ? _shooter.transform.root : null))
            {
                return;
            }

            if (other.TryGetComponent<PlayerProjectile>(out _))
            {
                return;
            }

            // Impact with walls, obstacles, or enemies
            Destroy(gameObject);
        }
    }
}
