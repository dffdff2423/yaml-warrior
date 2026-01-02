// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using YamlWarrior.JsonRPC.Annotations;
using YamlWarrior.JsonRPC.Protocol;

namespace YamlWarrior.JsonRPC.Tests;

public sealed class RPCServerTest {
    private const string TestAName = "test/a";

    [RPCMethod(TestAName)]
    private static Task<object?> MethodTestA(object? _) => Task.FromResult<object?>(null);

    public record TestBArgs {
        public required int A { get; init; }
    }

    private const string TestBName = "test/b";

    [RPCMethod(TestBName, Args = typeof(TestBArgs), Return = typeof(int))]
    private static Task<object?> MethodTestB(object? args) {
        Assert.That(args, !Is.Null);
        Assert.That(args is TestBArgs);
        var bargs = (TestBArgs)args;
        return Task.FromResult<object?>(bargs.A);
    }

    [Test]
    public void RegistrationWorks() {
        var s = new RPCServer();
        s.RegisterMethod(MethodTestA);
        s.RegisterMethod(MethodTestB);

        Assert.That(s.AvalibleMethods, Is.EquivalentTo([TestAName, TestBName]));
    }

    [Test]
    public async Task BasicMethodCallable() {
        var s = new RPCServer();
        s.RegisterMethod(MethodTestA);

        var res = await s.InvokeMethod(TestAName, null);
        Assert.That(res, Is.Null);
    }

    [Test]
    public void BasicMethodValidatesArgs() {
        var s = new RPCServer();
        s.RegisterMethod(MethodTestA);

        var e = Assert.ThrowsAsync<RPCErrorException>(async () => {
            _ = await s.InvokeMethod(TestAName, "blah");
        });

        Assert.That(e.Error.Code, Is.EqualTo(RPCError.InvalidMethodParameters.Code));
    }

    [Test]
    public void InvokeThrowsOnUnkownMethod() {
        var s = new RPCServer();

        var e = Assert.ThrowsAsync<RPCErrorException>(async () => {
            _ = await s.InvokeMethod("blah", "asdf");
        });

        Assert.That(e.Error.Code, Is.EqualTo(RPCError.MethodNotFound.Code));
    }

    [Test]
    public async Task MethodWithReturnCallable() {
        var s = new RPCServer();
        s.RegisterMethod(MethodTestB);

        var res = (int?)await s.InvokeMethod(TestBName, new TestBArgs { A = 42 });

        Assert.That(res, Is.EqualTo(42));
    }

    [Test]
    public void MethodWithReturnValidatesArgs() {
        var s = new RPCServer();
        s.RegisterMethod(MethodTestB);

        var e = Assert.ThrowsAsync<RPCErrorException>(async () => {
            _ = await s.InvokeMethod(TestBName, "blah");
        });

        Assert.That(e.Error.Code, Is.EqualTo(RPCError.InvalidMethodParameters.Code));
    }
}
