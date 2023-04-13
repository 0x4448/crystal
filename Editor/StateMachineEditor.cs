// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UnitySamples.Core
{
    /// <summary>
    /// Editor for the default concrete state machine class.
    /// </summary>
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : StateMachineEditor<BaseState, Trigger> { }



    /// <summary>
    /// Generic base class for adding buttons for each trigger to the inspector.
    /// </summary>
    /// <remarks>
    /// Derived classes need the CustomEditor attribute. No further implementation is required.
    /// </remarks>
    /// <typeparam name="TState">
    /// <inheritdoc cref="StateMachine{TState, TTrigger}" path="/typeparam[@name='TState']"/>
    /// </typeparam>
    /// <typeparam name="TTrigger">
    /// <inheritdoc cref="StateMachine{TState, TTrigger}" path="/typeparam[@name='TTrigger']"/>
    /// </typeparam>
    public abstract class StateMachineEditor<TState, TTrigger> : Editor
        where TState : Behaviour, IState
        where TTrigger : ScriptableObject
    {
        private StateMachine<TState, TTrigger> _stateMachine;
        private bool _showTriggers;

        private static TTrigger[] _triggers;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _stateMachine = target as StateMachine<TState, TTrigger>;

            // Sort triggers alphabetically.
            _triggers ??= _stateMachine.Triggers.OrderBy(t => t.name).ToArray();

            _showTriggers = EditorGUILayout.Foldout(_showTriggers, "Triggers");
            if (_showTriggers && _stateMachine.Triggers != null)
            {
                DrawTriggerButtons();
            }
        }

        private void DrawTriggerButtons()
        {
            foreach (var trigger in _triggers)
            {
                var buttonName = trigger.name.TitleCase();
                if (GUILayout.Button(buttonName) && Application.isPlaying)
                {
                    _stateMachine.Activate(trigger);
                }
            }
        }
    }
}
