// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json.Serialization;

using YamlWarrior.Common.Serialization;

namespace YamlWarrior.Tests.Generated;

[JsonUnion]
public abstract partial record SpecificObjectUnion {
    private SpecificObjectUnion() {
    }

    [JsonRequired, JsonPropertyName("blah")]
    [JsonUnionObjectKindProperty]
    public abstract string Kind { get; init; }

    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record Object1(int Value1, string Value2) : SpecificObjectUnion {
        [JsonRequired, JsonPropertyName("blah")]
        public override string Kind {
            get => "object1";
            init { }
        }
    }

    [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
    public sealed partial record Object2(float Value1, bool Value2) : SpecificObjectUnion {
        [JsonIgnore, JsonPropertyName("blah")]
        public override string Kind {
            get => "object2";
            init { }
        }
    }
}
