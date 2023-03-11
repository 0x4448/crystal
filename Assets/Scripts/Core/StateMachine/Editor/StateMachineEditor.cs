// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using System;
using UnityEditor;
using UnityEngine;

namespace UnitySamples.Core
{
    /// <summary>
    /// Base class for adding buttons for each trigger to the inspector.
    /// </summary>
    /// <remarks>
    /// Derived classes need the CustomEditor attribute. No further implementation is required.
    /// </remarks>
    /// <typeparam name="TStateMachine">The type of state machine.</typeparam>
    /// <typeparam name="TTrigger">The type of trigger.</typeparam>
    public abstract class StateMachineEditor<TStateMachine, TTrigger> : Editor
        where TStateMachine : StateMachine<TTrigger>
        where TTrigger : Enum
    {
        private TStateMachine _stateMachine;
        private bool _showTriggers;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            _stateMachine = target as TStateMachine;

            _showTriggers = EditorGUILayout.Foldout(_showTriggers, "Triggers");
            if (_showTriggers)
            {
                DrawTriggerButtons();
            }
        }

        private void DrawTriggerButtons()
        {
            var triggers = (TTrigger[])Enum.GetValues(typeof(TTrigger));
            foreach (var trigger in triggers)
            {
                var buttonName = trigger.ToString().TitleCase();
                if (GUILayout.Button(buttonName) && Application.isPlaying)
                {
                    _stateMachine.Activate(trigger);
                }
            }
        }
    }
}
