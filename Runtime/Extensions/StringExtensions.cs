// Copyright 2023 0x4448
// SPDX-License-Identifier: Apache-2.0

using System.Text.RegularExpressions;

namespace UnitySamples.Core
{
    public static class StringExtensions
    {
        /// <summary>
        /// Returns text in title case with spaces between words.
        /// </summary>
        public static string TitleCase(this string input)
        {
            return Regex.Replace(input, "[A-Z][a-z]", " $0").Trim();
        }
    }
}
