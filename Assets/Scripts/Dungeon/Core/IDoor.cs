using System;
using UnityEngine;

namespace Solaler.Dungeon.Core
{
    /// <summary>
    /// Direction identifier for room connectivity and doorway placement.
    /// </summary>
    public enum Direction
    {
        North,
        South,
        East,
        West
    }

    /// <summary>
    /// Contract for doors connecting dungeon rooms.
    /// </summary>
    public interface IDoor
    {
        Direction DoorDirection { get; }
        bool IsLocked { get; }
        bool IsOpen { get; }
        void Open();
        void Close();
        void Lock();
        void Unlock();
        Vector2 SpawnPosition { get; }
    }

    /// <summary>
    /// Contract for puzzle elements that evaluate completion conditions.
    /// </summary>
    public interface IPuzzleElement
    {
        bool IsSatisfied { get; }
        event Action<IPuzzleElement> OnStateChanged;
        void ResetElement();
    }
}
