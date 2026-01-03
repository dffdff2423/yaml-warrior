// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text.Json;

using JetBrains.Annotations;

using YamlWarrior.Common.Logger;
using YamlWarrior.JsonRPC.Annotations;
using YamlWarrior.JsonRPC.Protocol;

namespace YamlWarrior.JsonRPC;

[PublicAPI]
public sealed class RPCServer {
    private readonly PipeReader _istream;
    private readonly Stream _ostream;

    private readonly ImmutableDictionary<string, RPCMethodData> _methods;

    /// <summary>
    /// Dictionary of "in-flight" method requests. Mapping of task Id -> data
    /// </summary>
    private readonly ConcurrentDictionary<int, RPCInvocationData> _invocations = [];

    /// <summary>
    /// User provided invocation context. Will be placed into <see cref="MethodInvocationContext.User"/> in method calls.
    /// </summary>
    public object? UserContext { get; init; }

    public RPCServer(PipeReader input, Stream output, IEnumerable<RPCMethod> methods) {
        Debug.Assert(output.CanWrite);
        _istream = input;
        _ostream = output;

        _methods = methods.Select(lambda => {
                var ty = lambda.Method;
                var attr = (RPCMethodAttribute?)Attribute.GetCustomAttribute(ty, typeof(RPCMethodAttribute));
                if (attr == null)
                    throw new ArgumentException("RPC methods require the RPCMethodAttribute annotation");

                var data = new RPCMethodData {
                    Lambda = lambda,
                    Attr = attr,
                };

                return new KeyValuePair<string,RPCMethodData>(attr.MethodName, data);
            })
            .ToImmutableDictionary();
    }

    /// <summary>
    /// Launch a request and add it to <see cref="_invocations"/>
    /// </summary>
    /// <exception cref="RPCErrorException">If the request is malformed</exception>
    private void InvokeRequest(RPCRequest req) {
        if (req.JsonRPC != "2.0") { // We don't support 1.0
            Log.E($"RPC Request with bad version {req}");

            throw new RPCErrorException(RPCError.InvalidRequest with {
                Data = JsonSerializer.SerializeToElement("Only RPC 2.0 is supported"),
            }, req.Id);
        }

        if (!_methods.TryGetValue(req.Method, out var method)) {
            Log.E($"Tried to invoke unregistered method {req.Method}");

            throw new RPCErrorException(RPCError.MethodNotFound with {
                Data = JsonSerializer.SerializeToElement(req.Method),
            }, req.Id);
        }

        try {
            object? args = null;
            if (method.Attr.Args != null) {
                if (req.Params == null) {
                    Log.E($"Tried to invoke method {req.Method} with null parameters");

                    throw new RPCErrorException(RPCError.InvalidMethodParameters with {
                            Data = JsonSerializer.SerializeToElement(req.Method),
                        },
                        req.Id);
                }

                args = req.Params.Value.Deserialize(method.Attr.Args);
            } else {
                if (req.Params != null) {
                    Log.E($"Tried to invoke method {req.Method} with non-null parameters");

                    throw new RPCErrorException(RPCError.InvalidMethodParameters with {
                            Data = JsonSerializer.SerializeToElement(req.Method),
                        },
                        req.Id);
                }
            }

            var tokenSource = method.Attr.Cancelable ? new CancellationTokenSource() : null;
            var ctx = new MethodInvocationContext {
                Token = tokenSource?.Token,
                User = UserContext,
            };

            var task = method.Lambda.Invoke(ctx, args);

            var data = new RPCInvocationData {
                Id = req.Id,
                Task = task,
                ProgressHandler = null, // TODO
                TokenSource = tokenSource,
                ReturnType = method.Attr.Return,
            };

            if (!_invocations.TryAdd(data.Task.Id, data)) {
                Log.F("Somehow we tried to store a method invocation task twice");
                throw new RPCErrorException(RPCError.InternalError, req.Id);
            }
        } catch (FormatException e) {
            throw new RPCErrorException(RPCError.InvalidMethodParameters with {
                    Data = JsonSerializer.SerializeToElement(req.Method),
                },
                id: req.Id,
                inner: e);
        } catch (JsonException e) {
            throw new RPCErrorException(RPCError.InvalidMethodParameters with {
                    Data = JsonSerializer.SerializeToElement(req.Method),
                },
                id: req.Id,
                inner: e);
        } catch (RPCErrorException) {
            throw; // Do not fallthrough to the below case
        }catch (Exception e) {
            Log.E($"Got exception while invoking {req.Method}: {e.Message}");
            throw new RPCErrorException(RPCError.InternalError, id: req.Id, inner: e);
        }
    }

    /// <summary>
    /// Read the first Json object from the stream and launch a single request if it is a object or multiple if an array.
    /// </summary>
    /// <exception cref="RPCErrorException">If the JSON failed to parse or the request is invalid</exception>
    public async Task InvokeNextInternal() {
        var doc = await ReadFirstJsonAsync(_istream);
        var elem = doc.RootElement;

        switch (elem.ValueKind) {
        case JsonValueKind.Object: {
            RPCRequest? req;
            try {
                req = elem.Deserialize<RPCRequest>();
            } catch (Exception e) {
                // Ensure this is detected as an invalid request and not a parse error
                throw new RPCErrorException(RPCError.InvalidRequest, inner: e);
            }
            if (req == null)
                throw new RPCErrorException(RPCError.InvalidRequest with { Data = elem });
            InvokeRequest(req);
            break;
        }
        case JsonValueKind.Array: {
            var exceptions = new List<Exception>();
            foreach (var reqelem in elem.EnumerateArray()) {
                // Only return 1 error if this is an invalid array
                if (reqelem.ValueKind != JsonValueKind.Object)
                    throw new RPCErrorException(RPCError.InvalidRequest);
            }

            foreach (var reqelem in elem.EnumerateArray()) {
                try {
                    RPCRequest? req;
                    try {
                        req = reqelem.Deserialize<RPCRequest>();
                    } catch (Exception e) {
                        // Ensure this is detected as an invalid request and not a parse error
                        throw new RPCErrorException(RPCError.InvalidRequest, inner: e);
                    }

                    if (req == null)
                        throw new RPCErrorException(RPCError.InvalidRequest with { Data = reqelem });
                    InvokeRequest(req);
                } catch (Exception e) {
                    exceptions.Add(e);
                }
            }

            foreach (var e in exceptions) {
                switch (e) {
                case RPCErrorException rpc: {
                    await WriteRPCRepose(_ostream,
                        new RPCResponse.Failure {
                            Id = rpc.Id,
                            Error = rpc.Error,
                        });
                    break;
                }
                default: {
                    Log.E($"Got exception while processing batch: {e.Message}");
                    await WriteRPCRepose(_ostream, new RPCResponse.Failure {
                        Id = null,
                        Error = RPCError.InternalError,
                    });
                    break;
                }
                }
            }
            break;
        }
        default:
            throw new RPCErrorException(RPCError.InvalidRequest with { Data = elem });
        }
    }

    public async Task InvokeNext() {
        try {
            await InvokeNextInternal();
        } catch (RPCErrorException e) {
            await WriteRPCRepose(_ostream,
                new RPCResponse.Failure {
                    Id = e.Id,
                    Error = e.Error,
                });
        } catch(FormatException) {
            await WriteRPCRepose(_ostream,
                new RPCResponse.Failure {
                    Id = null,
                    Error = RPCError.ParseError,
                });
        } catch(JsonException) {
            await WriteRPCRepose(_ostream,
                new RPCResponse.Failure {
                    Id = null,
                    Error = RPCError.ParseError,
                });
        } catch (Exception e) {
            Log.E($"Exception during method invoke of unkown method {e.Message}");
            await WriteRPCRepose(_ostream, new RPCResponse.Failure {
                    Id = null,
                    Error = RPCError.InternalError,
            });
        }
    }

    /// <summary>
    /// Wait for and reap all methods, then write their result. Will return once there is no more methods to reap.
    /// </summary>
    public async Task WaitForMethods() {
        Debug.Assert(_ostream.CanWrite);

        while (!_invocations.IsEmpty) {
            var task = await Task.WhenAny(_invocations.Values.Select(v => v.Task));

            if (!_invocations.Remove(task.Id, out var data)) {
                Log.F("Failed to remove task from _invocations. This should always succeed");
                Debug.Assert(false, "This should be the only thing removing tasks so it should always succeed");
                await WriteRPCRepose(_ostream, new RPCResponse.Failure {
                        Id = null,
                        Error = RPCError.InternalError,
                });
                continue;
            }

            try {
                if (task.IsCanceled || data.TokenSource?.IsCancellationRequested == true)
                    continue;

                var ret = await task;
                if (data.Id == null)
                    continue;

                if (!IsCompatibleWithType(data.ReturnType, ret)) {
                    Log.F("Method violated return type contract");
                    Debug.Assert(false);
                    throw new RPCErrorException(RPCError.InternalError);
                }

                JsonElement? retJ = ret != null ? JsonSerializer.SerializeToElement(ret, data.ReturnType!) : null;

                await WriteRPCRepose(_ostream,
                    new RPCResponse.Success {
                        Id = data.Id,
                        Result = retJ,
                    });

            } catch (RPCErrorException e) {
                if (data.Id == null)
                    continue;

                await WriteRPCRepose(_ostream,
                    new RPCResponse.Failure {
                        Id = e.Id ?? data.Id,
                        Error = e.Error,
                    });
            } catch (Exception e) {
                if (data.Id == null)
                    continue;

                Log.E($"Uncaught RPC exception {e.Message}");

                await WriteRPCRepose(_ostream,
                    new RPCResponse.Failure {
                        Id = data.Id,
                        Error = RPCError.InternalError with {
                            Data = JsonSerializer.SerializeToElement(e.Message),
                        },
                    });
            } finally {
                data.TokenSource?.Dispose();
            }
        }
    }

    private static async Task WriteRPCRepose(Stream ostream, RPCResponse resp) {
        await JsonSerializer.SerializeAsync(ostream, resp);
        await ostream.WriteAsync(new[] { (byte)0x0A }); // newline
    }

    /// <summary>
    /// Check if <paramref name="obj"/> can be used with a method {argument,return} of type <paramref name="ty" />
    /// </summary>
    [Pure]
    private static bool IsCompatibleWithType(Type? ty, object? obj) {
        if (obj == null) {
            return ty == null;
        }

        return ty != null && ty.IsInstanceOfType(obj);
    }

    // We can't use normal deserialization stuff since they don't support streaming
    private static async Task<JsonDocument> ReadFirstJsonAsync(PipeReader istream) {
        var jsonState = new JsonReaderState();

        while (true) {
            var read = await istream.ReadAsync();
            if (read.Buffer.Length == 0 && read.IsCompleted)
                throw new EndOfStreamException("End of stream from client");

            var jsonRead = new Utf8JsonReader(read.Buffer, false, jsonState);
            if (JsonDocument.TryParseValue(ref jsonRead, out var doc)) {
                istream.AdvanceTo(read.Buffer.GetPosition(jsonRead.BytesConsumed));
                return doc;
            }

            jsonState = jsonRead.CurrentState;
            istream.AdvanceTo(read.Buffer.GetPosition(jsonRead.BytesConsumed), read.Buffer.End);

            if (read.IsCompleted)
                throw new JsonException("Incomplete JSON object and End of Stream");
        }
    }

    public IEnumerable<string> AvailableMethods => _methods.Keys;

    /// <summary>
    /// Cancel all invocations with the given id (if any exists)
    /// </summary>
    /// <returns>Task that will complete once they are fully canceled</returns>
    public Task CancelInvocation(RPCId id) =>
         Task.WhenAll(_invocations.Values
            .Where(v => v.Id == id && v.TokenSource != null)
            .Select(v => v.TokenSource!.CancelAsync()));
}

[PublicAPI]
public sealed class MethodInvocationContext {
    /// <summary>
    /// Token to cancel the request
    /// </summary>
    public CancellationToken? Token { get; init; }

    /// <summary>
    /// Value in [0, 100] (percentage)
    /// </summary>
    public IProgress<ushort>? Progress { get; init; }

    /// <summary>
    /// User provided Context.
    /// </summary>
    public object? User;
}

/// <summary>
/// RPC method. Must be annotated with <see cref="RPCMethodAttribute"/>
/// </summary>
[PublicAPI]
public delegate Task<object?> RPCMethod(MethodInvocationContext ctx, object? args);

internal sealed record RPCMethodData {
    public required RPCMethod Lambda { get; init; }

    public required RPCMethodAttribute Attr { get; init; }
}

internal sealed record RPCInvocationData {
    public CancellationTokenSource? TokenSource { get; init; }

    public IProgress<ushort>? ProgressHandler { get; init; }

    public required Task<object?> Task { get; init; }

    public required Type? ReturnType { get; init; }

    public RPCId? Id { get; init; }
}
