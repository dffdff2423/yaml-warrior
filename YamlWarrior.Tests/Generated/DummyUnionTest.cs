// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;

using YamlWarrior.Common.Serialization;

namespace YamlWarrior.Tests.Generated;

public sealed class DummyUnionTest {
    [Test]
    public void AttributeApplies() {
        var ty = typeof(DummyUnion.Integer);

        var attr = (JsonUnionVariantAttribute?)Attribute.GetCustomAttribute(ty, typeof(JsonUnionVariantAttribute));
        Assert.That(attr?.Kind, Is.EqualTo(JsonUnionVariantKind.Number));
    }

    [Test]
    public void WriteInt() {
        var i = new DummyUnion.Integer(42);

        Assert.That(JsonSerializer.Serialize(i), Is.EqualTo("42"));
    }

    [Test]
    public void WriteStr() {
        var i = new DummyUnion.String("asdf");

        Assert.That(JsonSerializer.Serialize(i), Is.EqualTo("\"asdf\""));
    }

    [Test]
    public void ReadInt() {
        const string i = "42";

        Assert.That(JsonSerializer.Deserialize<DummyUnion>(i), Is.EqualTo(new DummyUnion.Integer(42)));
    }

    [Test]
    public void ReadStr() {
        const string i = "\"asdf\"";

        Assert.That(JsonSerializer.Deserialize<DummyUnion>(i), Is.EqualTo(new DummyUnion.String("asdf")));
    }
}
