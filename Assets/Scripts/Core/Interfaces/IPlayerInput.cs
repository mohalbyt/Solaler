using System;
using UnityEngine;

namespace Solaler.Player.Core
{
    /// <summary>
    /// Contract for top-down player input providers.
    /// Supports full 2D movement and mouse/stick directional aiming.
    /// </summary>
    public interface IPlayerInput
    {
        Vector2 MoveInput { get; }
        Vector2 AimDirection { get; }
        Vector2 MouseWorldPosition { get; }
        bool AttackPressed { get; }
        bool AttackHeld { get; }
        bool DashTriggered { get; }

        event Action OnAttackTriggered;
        event Action OnDashTriggered;

        void Enable();
        void Disable();
        void ConsumeAttack();
        void ConsumeDash();
    }
}
