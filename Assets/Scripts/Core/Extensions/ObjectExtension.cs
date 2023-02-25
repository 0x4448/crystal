// SPDX-License-Identifier: Apache-2.0
// https://github.com/0x4448/unity-samples/blob/main/LICENSE

using UnityEngine;

namespace UnitySamples.Core
{
    public static class ObjectExtension
    {
        /// <inheritdoc cref="Debug.Log"/>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Log(this Object context, object message)
        {
            Debug.Log($"[{context.name}] {message}", context);
        }
    }
}
