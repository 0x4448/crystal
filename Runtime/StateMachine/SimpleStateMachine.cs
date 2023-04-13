// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace UnitySamples.Core
{
    /// <summary>
    /// Implements the core functionality of a state machine.
    /// </summary>
    /// <remarks>
    /// This class has zero dependencies on Unity and can be used on its own.
    /// For creating state machines on GameObjects, use <see cref="StateMachine{TTrigger}"></see>
    /// </remarks>
    /// <typeparam name="TState">The type of states in the state machine.</typeparam>
    /// <typeparam name="TTrigger">The type of triggers in the state machine.</typeparam>
    public class SimpleStateMachine<TState, TTrigger>
    {
        public TState CurrentState { get; private set; }
        public event Action StateChanged;

        /// <summary>
        /// Mapping of (previous state, trigger) to the next state.
        /// </summary>
        private readonly Dictionary<(TState Previous, TTrigger), TState> _transitions = new();

        /// <summary>
        /// Mapping of a state to its enter and exit actions.
        /// </summary>
        private readonly Dictionary<TState, (Action Enter, Action Exit)> _actions = new();

        public SimpleStateMachine(TState initialState, Action enter, Action exit)
        {
            CurrentState = initialState;
            AddState(initialState, enter, exit);
            _actions[CurrentState].Enter?.Invoke();
        }

        public void AddTransition(TState previous, TState next, TTrigger trigger)
        {
            _transitions.Add((previous, trigger), next);
        }

        public void AddState(TState state, Action enter, Action exit)
        {
            _actions.Add(state, (enter, exit));
        }

        public void Activate(TTrigger trigger)
        {
            if (_transitions.TryGetValue((CurrentState, trigger), out var newState))
            {
                ChangeState(newState);
            }
        }

        private void ChangeState(TState newState)
        {
            _actions[CurrentState].Exit?.Invoke();
            CurrentState = newState;
            _actions[CurrentState].Enter?.Invoke();
            StateChanged?.Invoke();
        }
    }
}
