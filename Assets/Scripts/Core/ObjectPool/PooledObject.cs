// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace UnitySamples.Core
{
    /// <summary>
    /// Base class for prefabs that use the <see cref="ObjectPool"/>.
    /// This must be attached to the root of the prefab.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class PooledObject : MonoBehaviour { }
}
