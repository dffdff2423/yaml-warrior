// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using JetBrains.Annotations;

namespace YamlWarrior.Robust.TypeInfo;

[PublicAPI]
public sealed record PrototypeInfo {
    /// <summary>
    /// Location for the c# source.
    /// </summary>
    public required string SourceLocation { get; init; }

    /// <summary>
    /// `entity` in `-type: entity`
    /// </summary>
    public required string Kind { get; init; }
}
