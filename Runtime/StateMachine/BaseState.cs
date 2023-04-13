// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace DoubleHelix.Crystal
{
    /// <summary>
    /// Parent class of MonoBehaviours that represent a state.
    /// The behaviour is enabled/disabled when entering/exiting the state.
    /// </summary>
    public abstract class BaseState : MonoBehaviour, IState
    {
        public void Enter()
        {
            OnEnter();
            enabled = true;
        }

        /// <summary>
        /// Derived classes may override this method to customize state enter behaviour.
        /// It is called immediately before enabling the behaviour.
        /// </summary>
        public virtual void OnEnter() { }

        public void Exit()
        {
            enabled = false;
            OnExit();
        }

        /// <summary>
        /// Derived classes may override this method to customize state exit behaviour.
        /// It is called immediately after disabling the behaviour.
        /// </summary>
        public virtual void OnExit() { }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
