// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace UnitySamples
{
    /// <summary>
    /// Base class for GameObjects that use the <see cref="ObjectPool"/>.
    /// </summary>
    public abstract class PooledObject : MonoBehaviour
    {
        /// <summary>
        /// The prefab that the GameObject was created from.
        /// </summary>
        /// <remarks>
        /// A reference to the prefab is required for the object to return itself to the pool.
        /// </remarks>
        public GameObject Prefab
        {
            get => _prefab;
            set
            {
                if (!_prefab)
                {
                    _prefab = value;
                }
            }

        }

        private GameObject _prefab;
    }
}
