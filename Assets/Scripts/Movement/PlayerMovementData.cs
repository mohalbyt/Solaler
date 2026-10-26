using UnityEngine;

namespace Solaler.Player.Movement
{
    /// <summary>
    /// ScriptableObject defining all movement and combat parameters for the top-down player.
    /// Tuned for a deliberate, responsive feel inspired by The Binding of Isaac.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Solaler/Player/TopDown Movement Data", order = 0)]
    public class PlayerMovementData : ScriptableObject
    {
        [Header("Top-Down Movement (Isaac Style)")]
        [Tooltip("Standard walking speed in units/sec. Isaac base is ~4.8.")]
        [Range(2f, 12f)]
        public float moveSpeed = 4.8f;

        [Tooltip("Acceleration rate towards input direction.")]
        public float acceleration = 45f;

        [Tooltip("Deceleration rate when input is released.")]
        public float deceleration = 50f;

        [Header("Dash / Dodge Roll")]
        [Tooltip("Burst velocity when performing a dash.")]
        [Range(5f, 20f)]
        public float dashSpeed = 11f;

        [Tooltip("Duration of the dash in seconds.")]
        [Range(0.1f, 0.5f)]
        public float dashDuration = 0.22f;

        [Tooltip("Cooldown between consecutive dashes.")]
        public float dashCooldown = 0.6f;

        [Header("Combat (Projectile Shooting)")]
        [Tooltip("Number of shots per second (fire rate).")]
        [Range(1f, 10f)]
        public float fireRate = 3.5f;

        [Tooltip("Speed of projectiles fired.")]
        [Range(5f, 25f)]
        public float projectileSpeed = 11f;

        [Tooltip("Lifetime of projectiles before auto-dissipating.")]
        [Range(0.5f, 5f)]
        public float projectileLifetime = 1.2f;

        [Tooltip("Damage dealt by each projectile.")]
        public float projectileDamage = 10f;

        [Header("Health & Defense")]
        [Tooltip("Maximum player health (e.g. 6 HP = 3 full hearts).")]
        public int maxHealth = 6;

        [Tooltip("Invincibility duration in seconds after taking damage.")]
        public float invincibilityDuration = 1.0f;

        public float FireCooldown => 1f / Mathf.Max(0.1f, fireRate);
    }
}
