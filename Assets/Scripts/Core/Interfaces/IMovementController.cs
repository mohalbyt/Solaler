using UnityEngine;

namespace Solaler.Player.Core
{
    /// <summary>
    /// Contract for manipulating top-down 2D player physics and velocity.
    /// Provides an abstraction layer over Rigidbody2D.
    /// </summary>
    public interface IMovementController
    {
        Vector2 Velocity { get; }
        bool IsFacingRight { get; }

        void SetVelocity(Vector2 velocity);
        void Accelerate(Vector2 targetVelocity, float rate);
        void Stop();
        void FaceDirection(float directionX);
    }
}
