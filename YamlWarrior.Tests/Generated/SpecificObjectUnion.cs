// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

// using System.Text.Json.Serialization;
//
// using YamlWarrior.Common.Serialization;
//
// namespace YamlWarrior.Tests.Generated;
//
// [JsonUnion]
// public abstract partial record SpecificObjectUnion {
//     private DummyUnion() {}
//
//     [JsonRequired]
//     [JsonUnionObjectKindProperty]
//     public required string Kind { get; set; }
//
//     [JsonUnionVariant(JsonUnionVariantKind.SpecificObject, ObjectKind = "object1")]
//     public sealed partial record Object1(int Value1, string Value2) : DummyUnion;
//
//     [JsonUnionVariant(JsonUnionVariantKind.SpecificObject, ObjectKind = "object2")]
//     public sealed partial record Object2(float Value1, bool Value2) : DummyUnion;
// }
//
