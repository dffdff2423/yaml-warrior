// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json.Serialization;

namespace MetaModelGen.Schema;

public sealed record TypeAlias : MetaInfoDef {
    /// <summary>
    /// The name of the alias
    /// </summary>
    [JsonRequired, JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The aliased type name.
    /// </summary>
    [JsonRequired, JsonPropertyName("type")]
    public required MetaType Type { get; init; }
}
