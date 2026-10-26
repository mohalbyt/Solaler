using System;
using UnityEngine;
using Solaler.Player.Core;

namespace Solaler.Player.StateMachine
{
    /// <summary>
    /// Manages the active state of the player character and handles transitions.
    /// Encapsulates state switching logic, adhering to the State Pattern and Single Responsibility Principle.
    /// </summary>
    public class PlayerStateMachine
    {
        public PlayerBaseState CurrentState { get; private set; }
        public PlayerBaseState PreviousState { get; private set; }

        public event Action<PlayerBaseState> OnStateChanged;

        public void Initialize(PlayerBaseState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
            OnStateChanged?.Invoke(CurrentState);
        }

        public void ChangeState(PlayerBaseState newState)
        {
            if (newState == null || newState == CurrentState)
            {
                return;
            }

            CurrentState.Exit();
            PreviousState = CurrentState;
            CurrentState = newState;
            CurrentState.Enter();
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
