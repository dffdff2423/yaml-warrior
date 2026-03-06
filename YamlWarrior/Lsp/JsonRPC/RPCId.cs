// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using YamlWarrior.Common.Serialization;

namespace YamlWarrior.Lsp.JsonRPC;

/// <summary>
/// JSON RPC message id variant. Can be a string or an int.
/// </summary>
[JsonUnion]
public abstract partial record RPCId {
    private RPCId() {} // Disallow inheritance outside of this class

    /// <summary>
    /// String RPC request Id
    /// </summary>
    [JsonUnionVariant(JsonUnionVariantKind.String)]
    public sealed partial record StringId(string Value) : RPCId;

    /// <summary>
    /// String RPC request Id
    /// </summary>
    [JsonUnionVariant(JsonUnionVariantKind.Number)]
    public sealed partial record IntId(int Value) : RPCId;
}
