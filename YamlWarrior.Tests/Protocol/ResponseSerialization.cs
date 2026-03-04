// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;

using YamlWarrior.Lsp.JsonRPC;

namespace YamlWarrior.Tests.Protocol;

public sealed class ResponseSerialization {
    private readonly JsonSerializerOptions PP = new() { WriteIndented = true };

    private const string SuccessExampleTxt =
        """
        {
          "result": "blah",
          "jsonrpc": "2.0",
          "id": 42
        }
        """;

   private static readonly RPCResponse.Success SuccessExample = new() {
        Id = new RPCId.IntId(42),
        Result = JsonSerializer.SerializeToElement("blah"),
    };

    [Test]
    public void WriteSuccess() {
        var str = JsonSerializer.Serialize(SuccessExample, PP);
        Console.WriteLine(str);
        Assert.That(str, Is.EqualTo(SuccessExampleTxt));
    }

    private const string FailureExampleTxt =
        """
        {
          "error": {
            "code": -32603,
            "message": "Method not found",
            "data": "blah"
          },
          "jsonrpc": "2.0",
          "id": 42
        }
        """;

    private static readonly RPCResponse.Failure FailExample = new() {
        Id = new RPCId.IntId(42),
        Error = RPCError.InternalError with { Data = JsonSerializer.SerializeToElement("blah") },
    };
    [Test]
    public void WriteFailure() {
        var str = JsonSerializer.Serialize(FailExample, PP);
        Assert.That(str, Is.EqualTo(FailureExampleTxt));
    }

    [Test]
    public void ReadSuccess() {
        var val = (RPCResponse.Success)JsonSerializer.Deserialize<RPCResponse>(SuccessExampleTxt)!;
        using (Assert.EnterMultipleScope()) {
            Assert.That(val.Id, Is.EqualTo(SuccessExample.Id));
            Assert.That(val.Result?.GetString(), Is.EqualTo("blah"));
        }
    }

    [Test]
    public void ReadFailure() {
        var val = (RPCResponse.Failure)JsonSerializer.Deserialize<RPCResponse>(FailureExampleTxt)!;
        using (Assert.EnterMultipleScope()) {
            Assert.That(val.Id, Is.EqualTo(FailExample.Id));
            Assert.That(val.Error.Code, Is.EqualTo(FailExample.Error.Code));
            Assert.That(val.Error.Message, Is.EqualTo(FailExample.Error.Message));
            Assert.That(val.Error.Data!.Value.GetString(), Is.EqualTo("blah"));
        }
    }
}
