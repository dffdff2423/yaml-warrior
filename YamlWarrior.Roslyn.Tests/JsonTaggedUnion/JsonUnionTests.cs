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
                               public sealed partial record Integer(int Value) : DummyUnion;

                               [JsonUnionVariant(JsonUnionVariantKind.String)]
                               public sealed partial record String(string Value) : DummyUnion;
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
}
