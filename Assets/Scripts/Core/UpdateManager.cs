// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using System.Collections.ObjectModel;
using UnityEngine;

namespace UnitySamples.Core
{
    /*
     * The UpdateManger class is intended to solve two different problems:
     *
     *   - Expensive Update methods, such as pathfinding, raycasts, etc.
     *   - Thousands of objects running update: https://blog.unity.com/technology/1k-update-calls
     *
     *  A MonoBehaviour can register with UpdateManager in its Awake method and specify how
     *  frequently its Update method should be called. For example, you can spawn 100k objects with
     *  a frame skip of 20. Only 5k objects (on average) will be updated each frame.
     */

    /// <summary>
    /// Distributes MonoBehaviour updates across multiple frames.
    /// </summary>
    public static class UpdateManager
    {
        static UpdateManager()
        {
            _ = new GameObject("UpdateManager", typeof(Updater));
        }

        // KeyedCollection is 10-15% slower than List but allows for unregistration in constant time.
        private static readonly BehaviourCollection _behaviours = new();

        /// <summary>
        /// Add a behaviour to UpdateManager.
        /// </summary>
        /// <param name="behaviour">The behaviour to be managed.</param>
        /// <param name="frameSkip">The number of frames between updates.</param>
        public static void Register(IManagedBehaviour behaviour, byte frameSkip)
        {
            _behaviours.Add(new ManagedBehaviour(behaviour, frameSkip));
        }

        /// <remarks>
        /// Managed behaviours must unregister before they are destroyed.
        /// </remarks>
        /// <param name="behaviour"></param>
        public static void Unregister(IManagedBehaviour behaviour)
        {
            _ = _behaviours.Remove(behaviour);
        }

        /// <summary>
        /// Encapsulates the data required to process each IManagedBehaviour.
        /// </summary>
        private class ManagedBehaviour
        {
            public ManagedBehaviour(IManagedBehaviour behaviour, byte frameSkip)
            {
                Behaviour = behaviour;
                FrameSkip = (byte)Mathf.Max(1, frameSkip);
                Offset = (byte)Random.Range(0, frameSkip);
            }

            /// <summary>
            /// A random offset to ensure an even distribution across frames.
            /// </summary>
            public byte Offset { get; }
            public byte FrameSkip { get; }
            public bool Enabled => Behaviour.enabled;

            public readonly IManagedBehaviour Behaviour;

            public void Update()
            {
                Behaviour.ManagedUpdate();
            }
        }

        private class Updater : MonoBehaviour
        {
            private void Awake()
            {
                DontDestroyOnLoad(gameObject);
            }

            private void Update()
            {
                // Store Time.frameCount because it can be expensive with many managed behaviours.
                var frameCount = Time.frameCount;

                foreach (var behaviour in _behaviours)
                {
                    var frame = frameCount + behaviour.Offset;
                    var skip = behaviour.FrameSkip;

                    // Modulo operation first because it is slightly faster than null check.
                    if (frame % skip == 0 && behaviour.Enabled)
                    {
                        behaviour.Update();
                    }
                }
            }
        }

        /// <remarks>
        /// Provides close to O(1) retrieval of behaviours by key.
        /// </remarks>
        private class BehaviourCollection : KeyedCollection<IManagedBehaviour, ManagedBehaviour>
        {
            protected override IManagedBehaviour GetKeyForItem(ManagedBehaviour item)
            {
                return item.Behaviour;
            }
        }
    }

    /// <summary>
    /// A behaviour that uses UpdateManager for updates.
    /// </summary>
    public interface IManagedBehaviour
    {
#pragma warning disable IDE1006 // Naming Styles
        /// <inheritdoc cref="Behaviour.enabled"/>
        public bool enabled { get; }
#pragma warning restore IDE1006 // Naming Styles

        /// <summary>
        /// ManagedUpdate is called by UpdateManager.
        /// </summary>
        public void ManagedUpdate();
    }
}
