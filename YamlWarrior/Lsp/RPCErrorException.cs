// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using YamlWarrior.Lsp.JsonRPC;

namespace YamlWarrior.Lsp;

/// <summary>
/// Exception representing an RPC error.
/// </summary>
public sealed class RPCErrorException(RPCError err, RPCId? id = null, Exception? inner = null) : Exception(err.Message, inner) {
    /// <summary>
    /// The RPC error
    /// </summary>
    public readonly RPCError Error = err;

    /// <summary>
    /// The Id of the error (if known)
    /// </summary>
    public readonly RPCId? Id = id;
}
