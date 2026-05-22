// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Runtime.InteropServices.JavaScript;
using System.Text;

namespace YamlWarrior.Common.Text;

public static class StringTools {
    /// <summary>
    /// Prepends the provided text to the start of each line
    /// </summary>
    /// <param name="input">The string to do the operation on</param>
    /// <param name="value">The value to prepend</param>
    public static string PrependEachLine(ReadOnlySpan<char> input, ReadOnlySpan<char> value) {
        var sb = new StringBuilder();
        bool first = true;
        foreach (var line in input.EnumerateLines()) {
            if (!first) {
                sb.Append('\n');
            }
            first = false;

            sb.Append(value);
            sb.Append(line);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Adds <paramref name="num"/> spaces to each line of <paramref name="input"/>
    /// </summary>
    public static string PadEachLine(ReadOnlySpan<char> input, int num) {
        var sb = new StringBuilder();
        bool first = true;
        foreach (var line in input.EnumerateLines()) {
            if (!first) {
                sb.Append('\n');
            }
            first = false;

            for (int i = 0; i < num; ++i) {
                sb.Append(' ');
            }

            sb.Append(line);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Poor man's convert to camel case
    /// </summary>
    public static string EnsureCapitalized(ReadOnlySpan<char> input) {
        if (!char.IsAsciiLetter(input[0]))
            return input.ToString();

        if (char.IsAsciiLetterUpper(input[0]))
            return input.ToString();

        return char.ToUpper(input[0]) + input[1..].ToString();
    }
}
