// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json.Serialization;

namespace MetaModelGen.Schema;

/// <summary>
/// Defines the structure of an object literal.
/// </summary>
public sealed record Structure : MetaInfoDef {
    /// <summary>
    /// Structures extended from. This structures form a polymorphic type hierarchy
    /// </summary>
    [JsonPropertyName("extends")]
    public MetaType[]? Extends { get; init; }

    /// <summary>
    /// Structures to mix in. The properties of these structures are `copied` into this structure. Mixins don't form a
    /// polymorphic type hierarchy in LSP.
    /// </summary>
    [JsonPropertyName("mixins")]
    public MetaType[]? Mixins { get; init; }

    /// <summary>
    /// Structure name
    /// </summary>
    [JsonRequired, JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Properties of the structure
    /// </summary>
    [JsonRequired, JsonPropertyName("properties")]
    public required Property[] Properties { get; init; }
}
