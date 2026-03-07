// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;

namespace YamlWarrior.Tests.Generated;

public sealed class SpecificObjectUnionTest {
    [Test]
    public void Write() {
        var obj = new SpecificObjectUnion.Object1(42, "asdf");
        var txt = JsonSerializer.Serialize<SpecificObjectUnion>(obj);

        Assert.That(txt, Is.EqualTo("{\"Value1\":42,\"Value2\":\"asdf\",\"blah\":\"object1\"}"));
    }
    [Test]
    public void Read() {
        const string txt = "{\"Value1\":4.20,\"Value2\":true,\"blah\":\"object2\"}";
        var obj = JsonSerializer.Deserialize<SpecificObjectUnion>(txt);

        Assert.That(obj, Is.EqualTo(new SpecificObjectUnion.Object2(4.20f, true)));
    }
}
