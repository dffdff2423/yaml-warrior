// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json.Serialization;

namespace MetaModelGen.Schema;

/// <summary>
/// Common meta info shared by a lot of types in the metamodel schema
/// </summary>
public abstract record MetaInfoDef {
    /// <summary>
    /// Set to a string if this type is deprecated
    /// </summary>
    [JsonPropertyName("deprecated"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Deprecated { get; init; }

    /// <summary>
    /// Documentation for the type
    /// </summary>
    [JsonPropertyName("documentation"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Documentation { get; init; }

    /// <summary>
    /// Weather or not this is experimental
    /// </summary>
    [JsonPropertyName("proposed"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Proposed { get; init; }

    /// <summary>
    /// The version this was added in
    /// </summary>
    [JsonPropertyName("since"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Since { get; init; }
}
