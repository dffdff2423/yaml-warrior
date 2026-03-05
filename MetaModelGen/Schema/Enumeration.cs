// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;
using System.Text.Json.Serialization;

namespace MetaModelGen.Schema;

public sealed record Enumeration {
    /// <summary>
    /// Set to a string if the enum is deprecated
    /// </summary>
    [JsonPropertyName("deprecated"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Deprecated { get; init; }

    [JsonPropertyName("documentation"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Documentation { get; init; }

    [JsonPropertyName("name"), JsonRequired]
    public required string Name { get; init; }

    /// <summary>
    /// Weather or not the enum is experimental
    /// </summary>
    [JsonPropertyName("proposed"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Proposed { get; init; }

    /// <summary>
    /// The version this enum was added in
    /// </summary>
    [JsonPropertyName("since"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Since { get; init; }

    /// <summary>
    /// Type of enum
    /// </summary>
    [JsonPropertyName("type"), JsonRequired]
    public required EnumerationType Type { get; init; }

    /// <summary>
    /// Enumeration values
    /// </summary>
    [JsonPropertyName("values"), JsonRequired]
    public required EnumerationEntry[] Values { get; init; }
}

public sealed record EnumerationType {
    /// <summary>
    /// Should always be base under the current version of the metamodel
    /// </summary>
    [JsonPropertyName("kind"), JsonRequired]
    public string Kind { get; init; } = "base";

    [JsonPropertyName("values"), JsonRequired]
    public required EnumerationTypeName Name { get; init; }
}

public enum EnumerationTypeName {
    [JsonStringEnumMemberName("string")]
    String,
    [JsonStringEnumMemberName("integer")]
    Integer,
    [JsonStringEnumMemberName("uinteger")]
    UInteger,
}

public sealed record EnumerationEntry {
    /// <summary>
    /// Set to a string if depricated
    /// </summary>
    [JsonPropertyName("deprecated"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Deprecated { get; init; }

    [JsonPropertyName("documentation"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Documentation { get; init; }

    [JsonPropertyName("name"), JsonRequired]
    public required string Name { get; init; }

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

// TODO: Json serializers
public abstract record EnumerationEntryValue {
    private EnumerationEntryValue() {} // Disallow inheritience outside of this class

    public sealed record String(string Value) : EnumerationEntryValue;
    public sealed record Number(long Value) : EnumerationEntryValue;
}
