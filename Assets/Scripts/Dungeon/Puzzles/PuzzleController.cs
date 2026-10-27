using System;
using System.Collections.Generic;
using UnityEngine;
using Solaler.Dungeon.Core;

namespace Solaler.Dungeon
{
    /// <summary>
    /// Coordinates multiple puzzle elements (pressure plates, switches, targets) within a room.
    /// Emits OnPuzzleSolved when all required elements are satisfied.
    /// </summary>
    [DisallowMultipleComponent]
    public class PuzzleController : MonoBehaviour
    {
        [Header("Puzzle Elements")]
        [Tooltip("List of pressure plates or puzzle elements that must be satisfied.")]
        [SerializeField] private List<PressurePlate> pressurePlates = new List<PressurePlate>();

        private bool _isSolved = false;
        public bool IsSolved => _isSolved;

        public void Configure(List<PressurePlate> plates)
        {
            pressurePlates = new List<PressurePlate>(plates);
        }

        public event Action OnPuzzleSolved;

        private void Awake()
        {
            if (pressurePlates.Count == 0)
            {
                pressurePlates.AddRange(GetComponentsInChildren<PressurePlate>());
            }

            foreach (var plate in pressurePlates)
            {
                plate.OnStateChanged += HandleElementStateChanged;
            }
        }

        private void OnDestroy()
        {
            foreach (var plate in pressurePlates)
            {
                if (plate != null)
                {
                    plate.OnStateChanged -= HandleElementStateChanged;
                }
            }
        }

        private void HandleElementStateChanged(IPuzzleElement element)
        {
            if (_isSolved) return;

            bool allSatisfied = true;
            foreach (var plate in pressurePlates)
            {
                if (!plate.IsSatisfied)
                {
                    allSatisfied = false;
                    break;
                }
            }

            if (allSatisfied)
            {
                _isSolved = true;
                Debug.Log($"<color=green>[PuzzleController] Puzzle solved in {gameObject.name}!</color>");
                OnPuzzleSolved?.Invoke();
            }
        }

        public void ResetPuzzle()
        {
            _isSolved = false;
            foreach (var plate in pressurePlates)
            {
                plate.ResetElement();
            }
        }
    }
}
