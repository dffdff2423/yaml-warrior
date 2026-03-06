// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using JetBrains.Annotations;

namespace YamlWarrior.Common.Serialization;

/// <summary>
/// Used to automatically generate serializer boilerplate for tagged unions like YamlWarrior.Lsp.JsonRPC.RPCId
///
/// Do not include both this and <see cref="JsonUnionVariantAttribute"/> on the same type
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class JsonUnionAttribute : Attribute;

/// <summary>
/// Marks a subclass as a tagged union member of its parent. Members must be defined inside their parent's namespace
///
/// Do not include both this and <see cref="JsonUnionAttribute"/> on the same type
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class JsonUnionVariantAttribute(JsonUnionVariantKind kind) : Attribute {
    [UsedImplicitly]
    public JsonUnionVariantKind Kind { get; } = kind;
}
