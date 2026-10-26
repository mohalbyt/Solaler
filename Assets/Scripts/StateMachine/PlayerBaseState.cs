using UnityEngine;
using Solaler.Player.Core;

namespace Solaler.Player.StateMachine
{
    /// <summary>
    /// Abstract base class for top-down player states.
    /// Follows the State Pattern to cleanly encapsulate state-specific behavior.
    /// </summary>
    public abstract class PlayerBaseState : IPlayerState
    {
        protected readonly PlayerController Player;
        protected readonly PlayerStateMachine StateMachine;

        protected float StateStartTime;
        public float TimeInState => Time.time - StateStartTime;

        protected PlayerBaseState(PlayerController player, PlayerStateMachine stateMachine)
        {
            Player = player;
            StateMachine = stateMachine;
        }

        public virtual void Enter()
        {
            StateStartTime = Time.time;
        }

        public virtual void HandleInput()
        {
        }

        public virtual void LogicUpdate()
        {
        }

        public virtual void PhysicsUpdate()
        {
        }

        public virtual void Exit()
        {
        }

        protected bool CheckDashTransition()
        {
            if (Player.Input.DashTriggered && Player.CanDash)
            {
                StateMachine.ChangeState(Player.DashState);
                return true;
            }
            return false;
        }
    }
}
