// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using YamlWarrior.Common.Serialization;

namespace MetaModelGen.Schema;

[JsonUnion]
public abstract partial record Params {
    private Params() {}

    [JsonUnionVariant(JsonUnionVariantKind.Array)]
    public sealed partial record Array(Type[] Value) : Params;

    [JsonUnionVariant(JsonUnionVariantKind.ValueObject)]
    public sealed partial record Object(Type Value) : Params;
}
