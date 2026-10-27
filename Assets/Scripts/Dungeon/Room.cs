using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solaler.Dungeon
{
    /// <summary>
    /// Represents an individual room in a top-down dungeon (The Binding of Isaac style).
    /// Tracks boundaries, contained doors, player occupancy, and puzzle states.
    /// </summary>
    [DisallowMultipleComponent]
    public class Room : MonoBehaviour
    {
        [Header("Room Identity")]
        [SerializeField] private Vector2Int gridCoordinates;
        [SerializeField] private Vector2 roomDimensions = new Vector2(18f, 10f);

        [Header("Doors")]
        [SerializeField] private List<Doorway> doorways = new List<Doorway>();

        [Header("Puzzle Reference")]
        [SerializeField] private PuzzleController roomPuzzle;

        private bool _isCleared = false;
        private bool _isPlayerInside = false;

        public Vector2Int GridCoordinates => gridCoordinates;
        public Vector2 RoomDimensions => roomDimensions;
        public Vector3 CenterPosition => transform.position;
        public bool IsCleared => _isCleared;
        public bool IsPlayerInside => _isPlayerInside;
        public IReadOnlyList<Doorway> Doorways => doorways;

        public event Action<Room> OnRoomEntered;
        public event Action<Room> OnRoomCleared;

        private void Awake()
        {
            if (doorways.Count == 0)
            {
                doorways.AddRange(GetComponentsInChildren<Doorway>());
            }

            if (roomPuzzle != null)
            {
                roomPuzzle.OnPuzzleSolved += HandlePuzzleSolved;
            }
            else
            {
                _isCleared = true;
            }
        }

        public void Initialize(Vector2Int coords)
        {
            gridCoordinates = coords;
        }

        public void OnPlayerEnterRoom()
        {
            _isPlayerInside = true;
            OnRoomEntered?.Invoke(this);

            if (roomPuzzle != null && !_isCleared)
            {
                // Lock doors until puzzle is solved
                LockAllDoors();
            }
            else
            {
                OpenAllDoors();
            }
        }

        public void OnPlayerExitRoom()
        {
            _isPlayerInside = false;
        }

        public void LockAllDoors()
        {
            foreach (var door in doorways)
            {
                door.Lock();
            }
        }

        public void OpenAllDoors()
        {
            foreach (var door in doorways)
            {
                door.Open();
            }
        }

        private void HandlePuzzleSolved()
        {
            _isCleared = true;
            OpenAllDoors();
            OnRoomCleared?.Invoke(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isCleared ? Color.cyan : Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(roomDimensions.x, roomDimensions.y, 0.1f));
        }
    }
}
