// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.IO.Pipelines;
using System.Text;
using System.Text.Json;

using JetBrains.Annotations;

using Newtonsoft.Json.Linq;

using YamlWarrior.Lsp;
using YamlWarrior.Lsp.Annotations;
using YamlWarrior.Lsp.JsonRPC;

namespace YamlWarrior.Tests;

public sealed class RPCServerTest {
    private const string TestAName = "test/a";

    [RPCMethod(TestAName)]
    private static Task<object?> MethodTestA(MethodInvocationContext ctx, object? _) => Task.FromResult<object?>(null);

    public record TestBArgs {
        [UsedImplicitly]
        public required int A { get; init; }
    }

    private const string TestBName = "test/b";

    [RPCMethod(TestBName, Args = typeof(TestBArgs), Return = typeof(int), Cancelable = true)]
    private static async Task<object?> MethodTestB(MethodInvocationContext ctx, object? args) {
        Assert.That(args, !Is.Null);
        Assert.That(args is TestBArgs);
        var bargs = (TestBArgs)args;
        await Task.Delay(1000);
        ctx.Token?.ThrowIfCancellationRequested();
        return bargs.A;
    }

    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": \"test/a\", \"id\": 1}", "{\"jsonrpc\": \"2.0\", \"result\": null, \"id\": 1}")]
    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": \"test/b\", \"params\": { \"A\": 19 }, \"id\": 1 }", "{\"jsonrpc\": \"2.0\", \"result\": 19, \"id\": 1}")]
    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": \"test/b\", \"params\": { \"A\": 19 }, \"id\": \"string\" }", "{\"jsonrpc\": \"2.0\", \"result\": 19, \"id\": \"string\"}")]
    public async Task SingleMethodCall(string inputJson, string outputJson) {
        var istream = new MemoryStream(Encoding.UTF8.GetBytes(inputJson));
        var ostream = new MemoryStream();

        var s = new RPCServer(PipeReader.Create(istream), ostream, [MethodTestA, MethodTestB]);

        await s.InvokeNext();
        await s.WaitForMethods();

        AssertJsonEq(ostream.GetBuffer(), outputJson);
    }

    [Test]
    public async Task CancelableMethodCall() {
        const string txt = "{\"jsonrpc\": \"2.0\", \"method\": \"test/b\", \"params\": { \"A\": 19 }, \"id\": 1 }";

        var istream = new MemoryStream(Encoding.UTF8.GetBytes(txt));
        var ostream = new MemoryStream();

        var s = new RPCServer(PipeReader.Create(istream), ostream, [MethodTestA, MethodTestB]);

        await s.InvokeNext();
        await s.CancelInvocation(new RPCId.IntId(1));
        await s.WaitForMethods();

        await Console.Error.WriteAsync(Encoding.UTF8.GetString(ostream.GetBuffer()));
        Assert.That(ostream.Position, Is.Zero);
    }

    [RPCMethod("subtract", Args = typeof(int[]), Return = typeof(int))]
    private static Task<object?> SubtractMethod(MethodInvocationContext? ctx, object? obj) {
        Assert.That(obj, !Is.Null);

        var args = (int[])obj;
        return Task.FromResult<object?>(args[0] - args[1]);
    }

    [Test]
    public async Task MultipleMethodCalls() {
        const string reqa = "{\"jsonrpc\": \"2.0\", \"method\": \"subtract\", \"params\": [42, 23], \"id\": 1}";
        const string rspa = "{\"jsonrpc\": \"2.0\", \"result\": 19, \"id\": 1}";

        const string reqb = "{\"jsonrpc\": \"2.0\", \"method\": \"subtract\", \"params\": [23, 42], \"id\": 2}";
        const string rspb = "{\"jsonrpc\": \"2.0\", \"result\": -19, \"id\": 2}";

        var istream = new MemoryStream(Encoding.UTF8.GetBytes(reqa).Concat(Encoding.UTF8.GetBytes(reqb)).ToArray());
        var ostream = new MemoryStream();

        var reader = PipeReader.Create(istream);

        var s = new RPCServer(reader, ostream, [SubtractMethod]);

        await s.InvokeNext();
        await s.InvokeNext();

        await s.WaitForMethods();

        var obuf = ostream.GetBuffer();
        var len = obuf.IndexOf((byte)0);
        if (len < 0) len = obuf.Length;
        var outstr = Encoding.UTF8.GetString(obuf[..len]);

        await Console.Error.WriteLineAsync(outstr);

        var split = outstr.Split('\n');

        using (Assert.EnterMultipleScope()) {
            AssertJsonArrayContains(split, rspa);
            AssertJsonArrayContains(split, rspb);
        }
    }

    [Test]
    public async Task Notifications() {
        const string reqa = "{\"jsonrpc\": \"2.0\", \"method\": \"subtract\", \"params\": [1, 2]}";
        const string reqb = "{\"jsonrpc\": \"2.0\", \"method\": \"test/a\"}";

        var istream = new MemoryStream(Encoding.UTF8.GetBytes(reqa).Concat(Encoding.UTF8.GetBytes(reqb)).ToArray());
        var ostream = new MemoryStream();

        var reader = PipeReader.Create(istream);

        var s = new RPCServer(reader, ostream, [SubtractMethod, MethodTestA]);

        await s.InvokeNext();
        await s.InvokeNext();

        await s.WaitForMethods();

        Assert.That(ostream.Position, Is.Zero);
    }

    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": \"doesnotexist\", \"params\": [1, 2], \"id\": 1}")]
    public async Task MethodNotFound(string json) {
        var outputjson = await DoSingleInvoke(json);
        var resp = JsonSerializer.Deserialize<RPCResponse.Failure>(outputjson);

        using (Assert.EnterMultipleScope()) {
            Assert.That(resp?.Id, Is.EqualTo(new RPCId.IntId(1)));
            Assert.That(resp?.Error.Code, Is.EqualTo(RPCError.MethodNotFound.Code));
        }
    }

    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": \"doesnotexist\", \"params\": [1, 2], \"id\": nonsense}")]
    [TestCase("[{\"jsonrpc\": \"2.0\", \"method\": \"doesnotexist\", \"params\": [1, 2], \"id\": nonsense]")]
    [TestCase("[{\"jsonrpc\": \"2.0\", \"method\": \"doesnotexist\", \"params\": [1, 2], \"id\": 1}, {\"asdf ]")]
    [TestCase("[{\"jsonrpc\": \"2.0\", \"method\": \"doesnotexist\", \"params\": [1, 2 }, \"id\": 1 }")]
    public async Task InvaidJson(string json) {
        var outputjson = await DoSingleInvoke(json);
        var resp = JsonSerializer.Deserialize<RPCResponse.Failure>(outputjson);

        using (Assert.EnterMultipleScope()) {
            Assert.That(resp?.Id, Is.Null);
            Assert.That(resp?.Error.Code, Is.EqualTo(RPCError.ParseError.Code));
        }
    }

    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": \"test/a\", \"params\": [1, 2], \"id\": 1}")]
    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": \"test/b\", \"params\": null, \"id\": 1}")]
    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": \"test/b\", \"params\": [1, 2], \"id\": 1}")]
    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": \"test/b\", \"params\": { \"something\": 38 }, \"id\": 1}")]
    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": \"subtract\", \"params\": { \"something\": 38 }, \"id\": 1}")]
    public async Task InvaidParameters(string json) {
        var outputjson = await DoSingleInvoke(json);
        var resp = JsonSerializer.Deserialize<RPCResponse.Failure>(outputjson);

        using (Assert.EnterMultipleScope()) {
            Assert.That(resp?.Id, Is.EqualTo(new RPCId.IntId(1)));
            Assert.That(resp?.Error.Code, Is.EqualTo(RPCError.InvalidMethodParameters.Code));
        }
    }

    [TestCase("{\"jsonrpc\": \"1.0\", \"method\": \"test/b\", \"params\": { \"something\": 38 }}")]
    [TestCase("{\"jsonrpc\": \"2.0\", \"method\": 1, \"params\": { \"something\": 38 }, \"id\": 1}")]
    [TestCase("[1]")]
    [TestCase("[1, 2, 3]")]
    public async Task InvaidRequest(string json) {
        var outputjson = await DoSingleInvoke(json);
        var resp = JsonSerializer.Deserialize<RPCResponse.Failure>(outputjson);

        using (Assert.EnterMultipleScope()) {
            Assert.That(resp?.Id, Is.Null.Or.EqualTo(new RPCId.IntId(1)));
            Assert.That(resp?.Error.Code, Is.EqualTo(RPCError.InvalidRequest.Code));
        }
    }

    [Test]
    public async Task MixedBatch() {
        const string batch = """
                             [
                                 {"jsonrpc": "2.0", "method": "subtract", "params": [1,2], "id": "1"},
                                 {"jsonrpc": "2.0", "method": "test/a", "params": null},
                                 {"jsonrpc": "2.0", "method": "subtract", "params": [42,23], "id": "2"},
                                 {"foo": "boo"},
                                 {"jsonrpc": "2.0", "method": "foo.get", "params": {"name": "myself"}, "id": "5"},
                                 {"jsonrpc": "2.0", "method": "subtract", "params": [42,23], "id": 5}
                             ]
                             """;
        Assert.DoesNotThrow(() => JsonSerializer.Deserialize<JsonElement>(batch));

        string[] expected = [
            "{\"jsonrpc\": \"2.0\", \"result\": -1, \"id\": \"1\"}",
            "{\"jsonrpc\": \"2.0\", \"result\": 19, \"id\": \"2\"}",
            "{\"jsonrpc\": \"2.0\", \"error\": {\"code\": -32600, \"message\": \"Invalid Request\"}, \"id\": null}",
            "{\"jsonrpc\": \"2.0\", \"error\": {\"code\": -32601, \"message\": \"Method not found\", \"data\": \"foo.get\" }, \"id\": \"5\"}",
        ];

        var output = await DoSingleInvoke(batch);
        var outstr = Encoding.UTF8.GetString(output);

        var split = outstr.Split('\n').Where(s => s.Length > 0).ToArray();

        using (Assert.EnterMultipleScope()) {
            foreach (var e in expected) {
                AssertJsonArrayContains(split, e);
            }
        }
    }

    private static void AssertJsonEq(Span<byte> lhs, string rhs) {
        var expected = JToken.Parse(rhs);
        var len = lhs.IndexOf((byte)0);
        if (len < 0) len = lhs.Length;
        var actual = JToken.Parse(Encoding.UTF8.GetString(lhs[..len]));

        Assert.That(JToken.DeepEquals(actual, expected));
    }

    private static void AssertJsonArrayContains(IEnumerable<string> lhs, string rhs) {
        var expected = JToken.Parse(rhs);
        if (lhs.Select(JToken.Parse).Any(val => JToken.DeepEquals(expected, val)))
            return;
        Assert.Fail($"Server response does not contain JSON value {rhs}");
    }

    private static async Task<byte[]> DoSingleInvoke(string json) {
        var istream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var ostream = new MemoryStream();

        var reader = PipeReader.Create(istream);

        var s = new RPCServer(reader, ostream, [SubtractMethod, MethodTestA, MethodTestB]);

        await s.InvokeNext();
        await s.WaitForMethods();

        var obuf = ostream.GetBuffer();
        var len = obuf.IndexOf((byte)0);
        if (len < 0) len = obuf.Length;

        var outputjson = obuf[..len];

        await Console.Error.WriteAsync(Encoding.UTF8.GetString(outputjson));

        return outputjson;
    }
}
