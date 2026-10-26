using System;
using UnityEngine;
using Solaler.Player.Core;
using Solaler.Player.Movement;

namespace Solaler.Player.Combat
{
    /// <summary>
    /// Handles top-down shooting combat (inspired by The Binding of Isaac).
    /// Fires projectiles towards the aim direction, with forward spawn offset to prevent clipping.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Projectile Reference")]
        [Tooltip("Prefab instantiated when shooting.")]
        [SerializeField] private GameObject projectilePrefab;

        [Header("Fire Point")]
        [Tooltip("Transform from which projectiles are spawned. Defaults to character position if null.")]
        [SerializeField] private Transform firePoint;

        [Header("Tuning")]
        [Tooltip("Forward distance from player center to spawn the projectile.")]
        [SerializeField] private float spawnOffsetDistance = 0.35f;

        private float _lastFireTime = -999f;
        private Sprite _cachedProjectileSprite;

        public event Action OnShotFired;

        private void Awake()
        {
            // Cache projectile sprite from sheet if needed for fallback
            LoadCachedProjectileSprite();
        }

        public bool CanShoot(PlayerMovementData data)
        {
            if (data == null) return false;
            return Time.time - _lastFireTime >= data.FireCooldown;
        }

        public void TryShoot(IPlayerInput input, PlayerMovementData data)
        {
            if (input == null || data == null) return;

            if ((input.AttackHeld || input.AttackPressed) && CanShoot(data))
            {
                Shoot(input.AimDirection, data);
                input.ConsumeAttack();
            }
        }

        public void Shoot(Vector2 direction, PlayerMovementData data)
        {
            _lastFireTime = Time.time;

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector2.right;
            }
            direction.Normalize();

            // Offset spawn position slightly in front of player towards aim so it never collides with player body
            Vector3 basePos = firePoint != null ? firePoint.position : transform.position;
            Vector3 spawnPos = basePos + new Vector3(direction.x, direction.y, 0f) * spawnOffsetDistance;

            GameObject projGO = null;
            if (projectilePrefab != null)
            {
                projGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                projGO = CreateFallbackProjectile(spawnPos);
            }

            if (projGO != null && projGO.TryGetComponent<PlayerProjectile>(out var projectile))
            {
                projectile.Initialize(direction, data.projectileSpeed, data.projectileDamage, data.projectileLifetime, gameObject);
            }

            OnShotFired?.Invoke();
        }

        private void LoadCachedProjectileSprite()
        {
            var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (var s in allSprites)
            {
                if (s.name == "14-Sheet_21")
                {
                    _cachedProjectileSprite = s;
                    break;
                }
            }
        }

        private GameObject CreateFallbackProjectile(Vector3 position)
        {
            GameObject go = new GameObject("PlayerProjectile");
            go.transform.position = position;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _cachedProjectileSprite;
            sr.sortingOrder = 15;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.08f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            go.AddComponent<PlayerProjectile>();
            return go;
        }
    }
}
