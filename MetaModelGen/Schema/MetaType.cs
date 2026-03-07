// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json.Serialization;

using YamlWarrior.Common.Serialization;

namespace MetaModelGen.Schema;

[JsonUnion]
public abstract partial record MetaType {
    private MetaType() { }

    [JsonRequired, JsonPropertyName("kind")]
    [JsonUnionObjectKindProperty]
    public abstract string Kind { get; init; }

    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record Base : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "base"; init { } }

        [JsonRequired, JsonPropertyName("name")]
        public required BaseTypeKind BaseTypeKind { get; init; }
    }

    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record Reference : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "reference"; init { } }

        [JsonRequired, JsonPropertyName("name")]
        public required string Name { get; init; }
    }

    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record Array : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "array"; init { } }

        [JsonRequired, JsonPropertyName("element")]
        public required MetaType Element { get; init; }
    }

    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record Map : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "map"; init { } }

        /// <summary>
        /// Must be a <see cref="MetaType.Base"/>  or <see cref="MetaType.Reference"/> which ultimately resolves to one of the
        /// following:
        /// - URI
        /// - DocumentUri
        /// - String
        /// - Integer
        /// </summary>
        [JsonRequired, JsonPropertyName("key")]
        public required MetaType Key { get; init; }

        [JsonRequired, JsonPropertyName("value")]
        public required MetaType Value { get; init; }
    }

    /// <summary>
    /// Can probably be implemented as inheritance. I don't think the current metamodel ands more than two types
    /// </summary>
    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record And : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "and"; init { } }

        [JsonRequired, JsonPropertyName("items")]
        public required MetaType[] Items { get; init; }
    }

    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record Or : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "or"; init { } }

        [JsonRequired, JsonPropertyName("items")]
        public required MetaType[] Items { get; init; }
    }

    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record Tuple : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "tuple"; init { } }

        [JsonRequired, JsonPropertyName("items")]
        public required MetaType[] Items { get; init; }
    }

    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record StructureLiteral : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "literal"; init { } }

        [JsonRequired, JsonPropertyName("value")]
        public required StructureLiteralDef[] Value { get; init; }
    }

    /// <summary>
    /// Represents a string literal (eg. `kind: 'rename'`)
    /// </summary>
    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record StringLiteral : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "stringLiteral"; init { } }

        [JsonRequired, JsonPropertyName("value")]
        public required string Value { get; init; }
    }

    /// <summary>
    /// Represents an int literal (eg. `kind: 1`)
    /// </summary>
    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record IntLiteral : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "integerLiteral"; init { } }

        [JsonRequired, JsonPropertyName("value")]
        public required long Value { get; init; }
    }

    /// <summary>
    /// Represents a boolean literal (eg. `kind: true`)
    /// </summary>
    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record BooleanLiteral : MetaType {
        [JsonRequired, JsonPropertyName("kind")]
        public override string Kind { get => "booleanLiteral"; init { } }

        [JsonRequired, JsonPropertyName("value")]
        public required bool Value { get; init; }
    }
}

public enum BaseTypeKind {
    [JsonStringEnumMemberName("URI")]
    Uri,
    DocumentUri,
    [JsonStringEnumMemberName("integer")]
    Int32,
    [JsonStringEnumMemberName("uinteger")]
    UInt32,
    [JsonStringEnumMemberName("decimal")]
    Decimal,
    RegExp,
    [JsonStringEnumMemberName("string")]
    String,
    [JsonStringEnumMemberName("boolean")]
    Boolean,
    [JsonStringEnumMemberName("null")]
    Null,
}

public sealed record StructureLiteralDef : MetaInfoDef {
    [JsonRequired, JsonPropertyName("properties")]
    public required Property[] Properties;
}

public sealed record Property : MetaInfoDef {
    [JsonRequired, JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonRequired, JsonPropertyName("optional")]
    public required bool Optional { get; init; }

    [JsonRequired, JsonPropertyName("type")]
    public required Type Type { get; init; }
}
