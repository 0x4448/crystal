// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using UnityEngine;

namespace UnitySamples.Core
{
    /// <summary>
    /// A base class for state machines that can be attached to GameObjects.
    /// </summary>
    /// <remarks>
    /// Derive from this class to configure a state machine in the inspector.
    /// State machine implementation: <see cref="SimpleStateMachine{TState, TTrigger}"/>
    /// </remarks>
    /// <typeparam name="TTrigger">A flags enum of the available transition triggers.</typeparam>
    public abstract class StateMachine<TTrigger> : MonoBehaviour where TTrigger : Enum
    {
        [field: SerializeField, ReadOnly] public BaseState CurrentState { get; private set; }

        [Tooltip("The first transition's from state is the initial state.")]
        [SerializeField] private Transition[] _transitions;

        private static TTrigger[] _triggers;
        private SimpleStateMachine<IState, TTrigger> _stateMachine;

        /// <summary>
        /// Configure the state machine. Derived classes that override <c>Awake()</c> must call <c>base.Awake()</c>.
        /// </summary>
        protected virtual void Awake()
        {
            // Setup the state machine.
            var initialState = _transitions[0].From;
            _stateMachine = new(initialState, initialState.Enter, initialState.Exit);
            initialState.enabled = true;

            // Get all unique states (except initial state) from the list of transitions.
            var states = _transitions.Select(t => t.From)
                .Where(s => s != initialState).ToHashSet();

            states.UnionWith(
                _transitions.Select(t => t.To)
                .Where(s => s != initialState).ToHashSet()
            );

            // Add the remaining states and all the transitions.
            foreach (var state in states)
            {
                _stateMachine.AddState(state, state.Enter, state.Exit);
                state.enabled = false;
            }

            _triggers ??= (TTrigger[])Enum.GetValues(typeof(TTrigger));

            foreach (var transition in _transitions)
            {
                foreach (var trigger in _triggers)
                {
                    if (transition.Trigger.HasFlag(trigger))
                    {
                        _stateMachine.AddTransition(transition.From, transition.To, trigger);
                    }
                }
            }

            // Show current state in the inspector.
            CurrentState = initialState;
            _stateMachine.StateChanged += UpdateInspector;
        }

        public void Activate(TTrigger trigger)
        {
            _stateMachine.Activate(trigger);
        }



        /*
         * Inspector methods and classes.
         */

        private void OnValidate()
        {
            // Set the name of each transition in the inspector.
            foreach (var transition in _transitions)
            {
                var triggerName = transition.Trigger.ToString().TitleCase();
                transition.name = $"{transition.From} to {transition.To} on {triggerName}";
            }
        }

        private void UpdateInspector()
        {
            CurrentState = (BaseState)_stateMachine.CurrentState;
        }

        /// <summary>
        /// Provides an interface to configure transitions in the inspector.
        /// </summary>
        [Serializable]
        private class Transition
        {
            [HideInInspector] public string name;

            public BaseState From;
            public BaseState To;
            public TTrigger Trigger;
        }
    }
}
