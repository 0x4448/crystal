// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using System.Collections.ObjectModel;
using UnityEngine;

namespace DoubleHelix.Crystal
{
    /*
     * The UpdateManger class is intended to solve two categories of performance problems:
     *
     *   - Expensive Update methods, such as pathfinding, raycasts, etc.
     *   - Thousands of objects running update: https://blog.unity.com/technology/1k-update-calls
     *
     *  It distributes updates across multiple frames. For example, you can spawn 100k objects that
     *  update every 20 frames. Only 5k objects will update every frame on average.
     *  
     *  To use UpdateManager, the MonoBehaviour must:
     *  - implement the IManagedBehaviour interface
     *  - call Add and/or AddLate in Awake or OnEnable
     *  - run gameplay code in ManagedUpdate and/or ManagedLateUpdate
     *  - NOT implement Update and LateUpdate
     *  - call Remove and/or RemoveLate in OnDisable or OnDestroy
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

        // KeyedCollection is 10-15% slower than List but allows for removal in constant time.
        private static readonly BehaviourCollection _updateBehaviours = new();
        private static readonly BehaviourCollection _lateUpdateBehaviours = new();


        /// <param name="behaviour">The behaviour to be managed.</param>
        /// <param name="frameSkip">The number of frames between updates.</param>
        public static void Add(IManagedBehaviour behaviour, byte frameSkip)
        {
            _updateBehaviours.Add(new ManagedBehaviour(behaviour, frameSkip));
        }

        /// <remarks>
        /// Managed behaviours must be removed before they are destroyed.
        /// </remarks>
        public static void Remove(IManagedBehaviour behaviour)
        {
            _ = _updateBehaviours.Remove(behaviour);
        }

        /// <inheritdoc cref="Add(IManagedBehaviour, byte)"/>
        public static void AddLate(IManagedBehaviour behaviour, byte frameSkip)
        {
            _lateUpdateBehaviours.Add(new ManagedBehaviour(behaviour, frameSkip));
        }

        /// <inheritdoc cref="Remove(IManagedBehaviour, byte)"/>
        public static void RemoveLate(IManagedBehaviour behaviour)
        {
            _ = _lateUpdateBehaviours.Remove(behaviour);
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

            public void LateUpdate()
            {
                Behaviour.ManagedLateUpdate();
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
                Run(_updateBehaviours, late: false);
            }

            private void LateUpdate()
            {
                Run(_lateUpdateBehaviours, late: true);
            }

            private void Run(BehaviourCollection behaviours, bool late)
            {
                // Cache Time.frameCount because it can be expensive with many managed behaviours.
                var frameCount = Time.frameCount;

                foreach (var behaviour in behaviours)
                {
                    var frame = frameCount + behaviour.Offset;
                    var skip = behaviour.FrameSkip;

                    // Modulo operation first because it is slightly faster than null check.
                    if (frame % skip == 0 && behaviour.Enabled)
                    {
                        if (late)
                        {
                            behaviour.LateUpdate();
                        }
                        else
                        {
                            behaviour.Update();
                        }
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

        /// <summary>
        /// ManagedLateUpdate is called by UpdateManager.
        /// </summary>
        public void ManagedLateUpdate();
    }
}
