using UnityEngine;

namespace Solaler.Player.StateMachine.States
{
    /// <summary>
    /// State when the player character dies.
    /// Freezes movement and plays the death explosion animation (sprites 22, 26, 30, 32).
    /// </summary>
    public class PlayerDeadState : PlayerBaseState
    {
        public PlayerDeadState(PlayerController player, PlayerStateMachine stateMachine) 
            : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Player.Movement.Stop();
            Player.Anim.TriggerDeath();
            Player.Input.Disable();
        }

        public override void LogicUpdate()
        {
            // Remains in dead state
        }

        public override void PhysicsUpdate()
        {
            Player.Movement.Stop();
        }
    }
}
