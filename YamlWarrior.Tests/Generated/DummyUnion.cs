// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using YamlWarrior.Common.Serialization;

namespace YamlWarrior.Tests.Generated;

[JsonUnion]
public abstract partial record DummyUnion {
    private DummyUnion() {}

    [JsonUnionVariant(JsonUnionVariantKind.Number)]
    public sealed partial record Integer(ulong Value) : DummyUnion;

    [JsonUnionVariant(JsonUnionVariantKind.String)]
    public sealed partial record String(string Value) : DummyUnion;
}
