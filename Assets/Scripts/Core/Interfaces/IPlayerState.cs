namespace Solaler.Player.Core
{
    /// <summary>
    /// Contract for player state machine states.
    /// Follows the State Pattern to encapsulate state-specific logic.
    /// </summary>
    public interface IPlayerState
    {
        void Enter();
        void HandleInput();
        void LogicUpdate();
        void PhysicsUpdate();
        void Exit();
    }
}
