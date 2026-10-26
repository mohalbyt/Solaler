using UnityEngine;

namespace Solaler.Player.StateMachine.States
{
    /// <summary>
    /// State when the player character performs a brief dodge roll / dash in top-down space.
    /// </summary>
    public class PlayerDashState : PlayerBaseState
    {
        private Vector2 _dashDirection;

        public PlayerDashState(PlayerController player, PlayerStateMachine stateMachine) 
            : base(player, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            Player.Input.ConsumeDash();
            Player.StartDashCooldown();
            Player.Anim.SetDashing(true);

            Vector2 input = Player.Input.MoveInput;
            if (input.sqrMagnitude > 0.01f)
            {
                _dashDirection = input.normalized;
            }
            else
            {
                _dashDirection = Player.Movement.IsFacingRight ? Vector2.right : Vector2.left;
            }

            Player.Movement.SetVelocity(_dashDirection * Player.Data.dashSpeed);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();

            if (TimeInState >= Player.Data.dashDuration)
            {
                if (Player.Input.MoveInput.sqrMagnitude > 0.01f)
                {
                    StateMachine.ChangeState(Player.MoveState);
                }
                else
                {
                    StateMachine.ChangeState(Player.IdleState);
                }
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            // Slightly decelerate during the final frames of the dash
            Player.Movement.Accelerate(_dashDirection * Player.Data.moveSpeed, Player.Data.deceleration * 0.5f);
        }

        public override void Exit()
        {
            base.Exit();
            Player.Anim.SetDashing(false);
        }
    }
}
