using System;
using UnityEngine;
using Solaler.Player.Movement;

namespace Solaler.Player.Combat
{
    /// <summary>
    /// Tracks player hit points, damage taking, and invincibility frames.
    /// Triggers death only when HP reaches 0, preventing accidental deaths during attack actions.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private PlayerMovementData movementData;

        private int _currentHealth;
        private float _lastDamageTime = -999f;
        private bool _isDead;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => movementData != null ? movementData.maxHealth : 6;
        public bool IsDead => _isDead;
        public bool IsInvincible => Time.time - _lastDamageTime < (movementData != null ? movementData.invincibilityDuration : 1f);

        public event Action<int, int> OnHealthChanged; // (current, max)
        public event Action OnDamaged;
        public event Action OnDeath;

        private void Awake()
        {
            _currentHealth = MaxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (_isDead || IsInvincible || damage <= 0) return;

            _lastDamageTime = Time.time;
            _currentHealth = Mathf.Max(0, _currentHealth - damage);

            OnDamaged?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (_isDead || amount <= 0) return;

            _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
        }

        public void Die()
        {
            if (_isDead) return;
            _isDead = true;
            OnDeath?.Invoke();
        }
    }
}
