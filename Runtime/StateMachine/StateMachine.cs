// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoubleHelix.Crystal
{
    /// <summary>
    /// The default concrete state machine class that can be attached to a GameObject.
    /// </summary>
    public class StateMachine : StateMachine<BaseState, Trigger> { }



    /*
     * The base state machine class is generic to allow for flexibility. If the type arguments in
     * the default implementation is not suitable, you may substitute with your own.
     */
    /// <summary>
    /// Generic base class for state machines that can be attached to GameObjects.
    /// </summary>
    /// <remarks>
    /// Inherit from this class to configure a state machine in the inspector.
    /// State machine implementation: <see cref="SimpleStateMachine{TState, TTrigger}"/>
    /// </remarks>
    /// <typeparam name="TState">A base state type that implements Behaviour and IState.</typeparam>
    /// <typeparam name="TTrigger">Any ScriptableObject.</typeparam>
    public abstract class StateMachine<TState, TTrigger> : MonoBehaviour
        where TState : Behaviour, IState
        where TTrigger : ScriptableObject
    {
        [field: SerializeField, ReadOnly] public TState CurrentState { get; private set; }

        [Tooltip("The first transition's from state is the initial state.")]
        [SerializeField] private Transition[] _transitions;

        public HashSet<TTrigger> Triggers { get; private set; } = new();

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

            foreach (var transition in _transitions)
            {
                foreach (var trigger in transition.Triggers)
                {
                    _stateMachine.AddTransition(transition.From, transition.To, trigger);
                    _ = Triggers.Add(trigger);
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
            if (_transitions == null)
            {
                return;
            }

            foreach (var transition in _transitions)
            {
                var name = $"{transition.From} to {transition.To} on";
                foreach (var trigger in transition.Triggers)
                {
                    if (trigger != null)
                    {
                        name = $"{name} {trigger.name.TitleCase()},";
                    }
                }
                transition.name = name.Trim(',');
            }
        }

        private void UpdateInspector()
        {
            CurrentState = (TState)_stateMachine.CurrentState;
        }

        /// <summary>
        /// Provides an interface to configure transitions in the inspector.
        /// </summary>
        [Serializable]
        private class Transition
        {
            [HideInInspector] public string name;

            public TState From;
            public TState To;
            public TTrigger[] Triggers;
        }
    }
}
