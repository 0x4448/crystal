// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace UnitySamples.Core
{
    public static class ObjectExtensions
    {
        /// <inheritdoc cref="Debug.Log"/>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Log(this Object context, object message)
        {
            Debug.Log($"[{context.name}] {message}", context);
        }
    }
}
