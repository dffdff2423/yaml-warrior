// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using JetBrains.Annotations;

namespace YamlWarrior.Robust.TypeInfo;

[PublicAPI]
public sealed record AssemblyTypes {
    /// <summary>
    /// Map of KindId to Prototype
    /// </summary>
    public Dictionary<string, PrototypeInfo> Prototypes { get; init; } = new();
}
