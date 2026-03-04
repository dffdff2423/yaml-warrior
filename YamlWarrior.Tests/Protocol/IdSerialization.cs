// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;

using YamlWarrior.Lsp.JsonRPC;

namespace YamlWarrior.Tests.Protocol;

public sealed class IdSerialization {
    [Test]
    public void WriteId() {
        var str = new RPCId.StringId("asdf");
        Assert.That(JsonSerializer.Serialize(str), Is.EqualTo("\"asdf\""));

        var integer = new RPCId.IntId(49);
        Assert.That(JsonSerializer.Serialize(integer), Is.EqualTo("49"));
    }

    [Test]
    public void ReadId() {
        const string str = "\"asdf\"";
        Assert.That(JsonSerializer.Deserialize<RPCId>(str), Is.EqualTo(new RPCId.StringId("asdf")));

        const int integer = 49;
        Assert.That(JsonSerializer.Deserialize<RPCId>(integer), Is.EqualTo(new RPCId.IntId(49)));
    }
}
