// SPDX-License-Identifier: CC0-1.0

using System;
using UnityEngine;

namespace DoubleHelix.Crystal
{
    /// <summary>
    /// Makes a field read-only in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class ReadOnlyAttribute : PropertyAttribute { }
}
