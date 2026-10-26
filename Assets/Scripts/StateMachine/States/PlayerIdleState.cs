using UnityEngine;

namespace Solaler.Player.StateMachine.States
{
    /// <summary>
    /// State when the player character is stationary in top-down view.
    /// Smoothly decelerates to 0 velocity and plays the idle breathing animation.
    /// </summary>
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine) 
            : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Player.Anim.SetDashing(false);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            if (CheckDashTransition()) return;

            // Transition to Move state when movement input is received
            if (Player.Input.MoveInput.sqrMagnitude > 0.01f)
            {
                StateMachine.ChangeState(Player.MoveState);
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            // Decelerate smoothly to a full stop
            Player.Movement.Accelerate(Vector2.zero, Player.Data.deceleration);

            // Orient facing towards aim direction when stationary
            if (Mathf.Abs(Player.Input.AimDirection.x) > 0.1f)
            {
                Player.Movement.FaceDirection(Player.Input.AimDirection.x);
            }
        }
    }
}
