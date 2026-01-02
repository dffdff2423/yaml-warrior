// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using YamlWarrior.JsonRPC.Protocol;

namespace YamlWarrior.JsonRPC;

/// <summary>
/// Exception representing an RPC error.
/// </summary>
public sealed class RPCErrorException(RPCError err, Exception? inner = null) : Exception(err.Message, inner) {
    /// <summary>
    /// The RPC error
    /// </summary>
    public readonly RPCError Error = err;
}
