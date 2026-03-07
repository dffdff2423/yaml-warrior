// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

namespace YamlWarrior.Roslyn.Tests.JsonTaggedUnion;

public static class JsonUnionTests {
    private static readonly string TestCode = """
                           using YamlWarrior.Common.Serialization;

                           [JsonUnion]
                           public abstract partial record DummyUnion {
                               private DummyUnion() {}

                               [JsonUnionVariant(JsonUnionVariantKind.Number)]
                               public sealed partial record Integer(ulong Value) : DummyUnion;

                               [JsonUnionVariant(JsonUnionVariantKind.String)]
                               public sealed partial record String(string Value) : DummyUnion;

                               [JsonUnionVariant(JsonUnionVariantKind.Array)]
                               public sealed partial record Array(string[] Value) : DummyUnion;

                               [JsonUnionVariant(JsonUnionVariantKind.ExclusiveObject)]
                               public sealed partial record ExclusiveObj(string Value1, int Value2) : DummyUnion;
                           }
                           """;

    [Test]
    public static Task DummyClass()
        => GeneratorTest.Verify(TestCode);

    private static readonly string TestCode2 = """
                                              using YamlWarrior.Common.Serialization;

                                              [JsonUnion]
                                              public abstract partial record DummyUnion {
                                                  private DummyUnion() {}

                                                  [JsonUnionVariant(JsonUnionVariantKind.Number)]
                                                  public sealed partial record Integer(int Value) : DummyUnion;

                                                  [JsonUnionVariant(JsonUnionVariantKind.Number)]
                                                  public sealed partial record Integer2(int Value) : DummyUnion;
                                              }
                                              """;

    [Test]
    public static Task RepeatedNumber()
        => GeneratorTest.Verify(TestCode2, genShouldError: true);

    private static readonly string TestCode3 = """
                                               using System.Text.Json.Serialization;

                                               using YamlWarrior.Common.Serialization;

                                               namespace YamlWarrior.Tests.Generated;

                                               [JsonUnion]
                                               public abstract partial record SpecificObjectUnion {
                                                   private SpecificObjectUnion() {}

                                                   [JsonRequired]
                                                   [JsonUnionObjectKindProperty]
                                                   public abstract string Kind { get; init; }

                                                   [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
                                                   public sealed partial record Object1(int Value1, string Value2) : SpecificObjectUnion {
                                                       public override string Kind {
                                                           get => "object1";
                                                           init { }
                                                       }
                                                   }

                                                   [JsonUnionVariant(JsonUnionVariantKind.SpecificObject)]
                                                   public sealed partial record Object2(float Value1, bool Value2) : SpecificObjectUnion {
                                                       public override string Kind {
                                                           get => "object2";
                                                           init { }
                                                       }
                                                   }
                                               }
                                               """;

    [Test]
    public static Task SpecificObject()
        => GeneratorTest.Verify(TestCode3);
}
