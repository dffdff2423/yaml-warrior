// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using JetBrains.Annotations;

namespace YamlWarrior.JsonRPC.Annotations;

[PublicAPI]
[AttributeUsage(AttributeTargets.Method)]
public sealed class RPCMethodAttribute(string methodName) : Attribute {
    /// <summary>
    /// Name of the RPC method
    /// </summary>
    public readonly string MethodName = methodName;

    /// <summary>
    /// Argument of the RPC method. Null for no args
    /// </summary>
    public Type? Args { get; set; }

    /// <summary>
    /// Return value for the RPC method. Null for no return value.
    /// </summary>
    public Type? Return { get; set; }
}
