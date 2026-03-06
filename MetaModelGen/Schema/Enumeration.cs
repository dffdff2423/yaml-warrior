// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json.Serialization;

using YamlWarrior.Common.Serialization;

namespace MetaModelGen.Schema;

public sealed record Enumeration : TypeMetaInfo {
    [JsonPropertyName("name"), JsonRequired]
    public required string Name { get; init; }

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

public sealed record EnumerationEntry : TypeMetaInfo {
    [JsonPropertyName("name"), JsonRequired]
    public required string Name { get; init; }

    [JsonPropertyName("value"), JsonRequired]
    public required EnumerationEntryValue Value { get; init; }
}

[JsonUnion]
public abstract partial record EnumerationEntryValue {
    private EnumerationEntryValue() {} // Disallow inheritance outside of this class

    [JsonUnionVariant(JsonUnionVariantKind.String)]
    public sealed partial record String(string Value) : EnumerationEntryValue;

    [JsonUnionVariant(JsonUnionVariantKind.Number)]
    public sealed partial record Number(long Value) : EnumerationEntryValue;
}
