using System;
using UnityEngine;
using Solaler.Dungeon.Core;

namespace Solaler.Dungeon
{
    /// <summary>
    /// Pressure plate puzzle element triggered by the player, pushable blocks, or projectiles.
    /// Implements IPuzzleElement adhering to the Single Responsibility Principle.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PressurePlate : MonoBehaviour, IPuzzleElement
    {
        [Header("Settings")]
        [Tooltip("If true, remains pressed down permanently once triggered.")]
        [SerializeField] private bool staysPressedPermanently = true;

        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer plateRenderer;
        [SerializeField] private Color unpressedColor = new Color(0.6f, 0.6f, 0.7f, 1f);
        [SerializeField] private Color pressedColor = new Color(0.2f, 0.9f, 0.4f, 1f);

        private int _occupantCount = 0;
        private bool _isPressed = false;

        public bool IsSatisfied => _isPressed;
        public event Action<IPuzzleElement> OnStateChanged;

        public void Configure(SpriteRenderer renderer, bool permanent = true)
        {
            plateRenderer = renderer;
            staysPressedPermanently = permanent;
            UpdateVisuals();
        }

        private void Reset()
        {
            plateRenderer = GetComponent<SpriteRenderer>();
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Awake()
        {
            if (plateRenderer == null) plateRenderer = GetComponent<SpriteRenderer>();
            UpdateVisuals();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") || other.GetComponent<PushableBlock>() != null)
            {
                _occupantCount++;
                if (!_isPressed)
                {
                    _isPressed = true;
                    UpdateVisuals();
                    OnStateChanged?.Invoke(this);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (staysPressedPermanently && _isPressed) return;

            if (other.CompareTag("Player") || other.GetComponent<PushableBlock>() != null)
            {
                _occupantCount = Mathf.Max(0, _occupantCount - 1);
                if (_occupantCount == 0 && _isPressed)
                {
                    _isPressed = false;
                    UpdateVisuals();
                    OnStateChanged?.Invoke(this);
                }
            }
        }

        public void ResetElement()
        {
            _occupantCount = 0;
            _isPressed = false;
            UpdateVisuals();
            OnStateChanged?.Invoke(this);
        }

        private void UpdateVisuals()
        {
            if (plateRenderer != null)
            {
                plateRenderer.color = _isPressed ? pressedColor : unpressedColor;
            }
        }
    }
}
