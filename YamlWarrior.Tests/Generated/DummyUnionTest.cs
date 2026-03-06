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

    [Test]
    public void ReadArray() {
        const string i = "[\"asdf\", \"uiop\"]";
        var arr = JsonSerializer.Deserialize<DummyUnion>(i) as DummyUnion.Array;

        Assert.Multiple(() => {
            Assert.That(arr, !Is.Null);
            Assert.That(arr!.Value, Is.EquivalentTo(["asdf", "uiop"]));
        });
    }

    [Test]
    public void ReadExclusiveObj() {
        const string i = "{ \"Value1\": \"asdf\", \"Value2\": 42 }";
        var obj = JsonSerializer.Deserialize<DummyUnion>(i) as DummyUnion.ExclusiveObj;

        Assert.Multiple(() => {
            Assert.That(obj, !Is.Null);
            Assert.That(obj, Is.EqualTo(new DummyUnion.ExclusiveObj("asdf", 42)));
        });
    }
}
