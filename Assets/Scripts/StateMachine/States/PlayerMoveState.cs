using UnityEngine;

namespace Solaler.Player.StateMachine.States
{
    /// <summary>
    /// State when the player character is moving in 2D top-down space (Isaac style).
    /// </summary>
    public class PlayerMoveState : PlayerBaseState
    {
        public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine) 
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

            // Return to Idle when input ceases and velocity drops
            if (Player.Input.MoveInput.sqrMagnitude < 0.01f && Player.Movement.Velocity.sqrMagnitude < 0.05f)
            {
                StateMachine.ChangeState(Player.IdleState);
            }
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            Vector2 input = Player.Input.MoveInput;
            Vector2 targetVelocity = input * Player.Data.moveSpeed;

            float accel = input.sqrMagnitude > 0.01f ? Player.Data.acceleration : Player.Data.deceleration;
            Player.Movement.Accelerate(targetVelocity, accel);

            // Orient facing direction: if shooting, face aim; otherwise face movement direction
            if (Player.Input.AttackHeld && Mathf.Abs(Player.Input.AimDirection.x) > 0.1f)
            {
                Player.Movement.FaceDirection(Player.Input.AimDirection.x);
            }
            else if (Mathf.Abs(input.x) > 0.05f)
            {
                Player.Movement.FaceDirection(input.x);
            }
        }
    }
}
