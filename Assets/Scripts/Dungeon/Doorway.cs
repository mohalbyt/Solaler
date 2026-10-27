using System;
using UnityEngine;
using Solaler.Dungeon.Core;

namespace Solaler.Dungeon
{
    /// <summary>
    /// Represents an Isaac-style doorway that leads to an adjacent room.
    /// Handles physical passage barriers and transition trigger zones.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Doorway : MonoBehaviour, IDoor
    {
        [Header("Door Configuration")]
        [SerializeField] private Direction direction;
        [SerializeField] private Doorway connectedDoorway;
        [SerializeField] private Room parentRoom;

        [Header("Visuals & Barriers")]
        [SerializeField] private Collider2D barrierCollider;
        [SerializeField] private SpriteRenderer doorSprite;
        [SerializeField] private Color openColor = new Color(0.2f, 0.8f, 0.3f, 0.8f);
        [SerializeField] private Color closedColor = new Color(0.9f, 0.2f, 0.2f, 0.9f);

        [Header("Spawn Offset")]
        [Tooltip("Offset distance where the player is placed when exiting this doorway into the room.")]
        [SerializeField] private float entryOffsetDistance = 1.6f;

        private bool _isOpen = true;
        private bool _isLocked = false;

        public Direction DoorDirection => direction;
        public bool IsLocked => _isLocked;
        public bool IsOpen => _isOpen;
        public Room ParentRoom => parentRoom;
        public Doorway ConnectedDoorway => connectedDoorway;

        public event Action<Doorway, GameObject> OnPlayerEnteredDoor;

        public Vector2 SpawnPosition
        {
            get
            {
                Vector2 dirVec = GetDirectionVector(direction);
                // Offset inside the room away from the wall
                return (Vector2)transform.position - (dirVec * entryOffsetDistance);
            }
        }

        private void Reset()
        {
            parentRoom = GetComponentInParent<Room>();
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Awake()
        {
            if (parentRoom == null) parentRoom = GetComponentInParent<Room>();
            UpdateVisuals();
        }

        public void SetConnectedDoorway(Doorway other)
        {
            connectedDoorway = other;
        }

        public void Open()
        {
            _isOpen = true;
            _isLocked = false;
            if (barrierCollider != null) barrierCollider.enabled = false;
            UpdateVisuals();
        }

        public void Close()
        {
            _isOpen = false;
            if (barrierCollider != null) barrierCollider.enabled = true;
            UpdateVisuals();
        }

        public void Lock()
        {
            _isLocked = true;
            Close();
        }

        public void Unlock()
        {
            _isLocked = false;
            Open();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isOpen || _isLocked) return;

            if (other.CompareTag("Player") && other.transform.root == other.transform)
            {
                OnPlayerEnteredDoor?.Invoke(this, other.gameObject);
            }
        }

        private void UpdateVisuals()
        {
            if (doorSprite != null)
            {
                doorSprite.color = _isOpen ? openColor : closedColor;
            }
        }

        public static Vector2 GetDirectionVector(Direction dir)
        {
            return dir switch
            {
                Direction.North => Vector2.up,
                Direction.South => Vector2.down,
                Direction.East => Vector2.right,
                Direction.West => Vector2.left,
                _ => Vector2.zero
            };
        }

        public static Direction GetOpposite(Direction dir)
        {
            return dir switch
            {
                Direction.North => Direction.South,
                Direction.South => Direction.North,
                Direction.East => Direction.West,
                Direction.West => Direction.East,
                _ => Direction.North
            };
        }
    }
}
