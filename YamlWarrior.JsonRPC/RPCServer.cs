// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Diagnostics;
using System.Text.Json;

using JetBrains.Annotations;

using YamlWarrior.Common.Logger;
using YamlWarrior.JsonRPC.Annotations;
using YamlWarrior.JsonRPC.Protocol;

namespace YamlWarrior.JsonRPC;

[PublicAPI]
public sealed class RPCServer {
    private readonly Dictionary<string, RPCMethodData> _methods = new();

    /// <summary>
    /// Register a given RPCMethod to the server.
    /// </summary>
    /// <param name="lambda">The method</param>
    /// <exception cref="ArgumentException">If the method lacks <see cref="RPCMethodAttribute"/></exception>
    public void RegisterMethod(RPCMethod lambda) {
        var ty = lambda.Method;
        var attr = (RPCMethodAttribute?)Attribute.GetCustomAttribute(ty, typeof(RPCMethodAttribute));
        if (attr == null)
            throw new ArgumentException("RPC methods require the RPCMethodAttribute annotation");

        var data = new RPCMethodData {
            Lambda = lambda,
            Args = attr.Args,
            Return = attr.Return,
        };

        _methods.Add(attr.MethodName, data);
        Log.D($"Registered `{attr.MethodName}` to RPCServer`");
    }

    /// <summary>
    /// Invoke the method with the given name.
    /// </summary>
    /// <param name="name">Name of the method</param>
    /// <param name="args">Arguments. Must match the type specified in the <see cref="RPCMethodAttribute"/> or null</param>
    /// <returns>The return type specified in <see cref="RPCMethodAttribute"/> or null</returns>
    /// <exception cref="RPCErrorException">If type validation fails</exception>
    public async Task<object?> InvokeMethod(string name, object? args) {
        if (!_methods.TryGetValue(name, out var method))
            throw new RPCErrorException(RPCError.MethodNotFound with { Data = JsonSerializer.SerializeToElement(name)});

        // Check args
        if (!IsCompatableWithType(method.Args, args)) {
            Log.W($"Method `{name}` called with invalid argument `{args}`");
            throw new RPCErrorException(RPCError.InvalidMethodParameters with {
                Data = JsonSerializer.SerializeToElement(args),
            });
        }

        Log.D($"Invoking method `{name}` on RPCServer`");
        // Call the method
        var ret = await method.Lambda(args);

        // Check return
        if (!IsCompatableWithType(method.Return, ret)) {
            Log.F($"Method `{name}` violated it's return value contract");
            Debug.Assert(false);
            throw new RPCErrorException(RPCError.InternalError);
        }

        return ret;
    }

    /// <summary>
    /// Check if <paramref name="obj"/> can be used with a method {argument,return} of type <paramref name="ty" />
    /// </summary>
    [Pure]
    private static bool IsCompatableWithType(Type? ty, object? obj) {
        if (obj == null) {
            return ty == null;
        }

        return ty != null && ty.IsInstanceOfType(obj);
    }

    public IEnumerable<string> AvalibleMethods => _methods.Keys;
}

/// <summary>
/// RPC method. Must be annotated with <see cref="RPCMethodAttribute"/>
/// </summary>
public delegate Task<object?> RPCMethod(object? args);

/// <summary>
/// Data type for RPCMethods
/// </summary>
internal sealed record RPCMethodData {
    public required RPCMethod Lambda { get; init; }

    /// <summary>
    /// Arguments for the RPC Method. Null if the method does not accept arguments
    /// </summary>
    public Type? Args { get; init; }

    /// <summary>
    /// Return value for the method. Null if the method does not return anything
    /// </summary>
    public Type? Return { get; init; }
}
