// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace YamlWarrior.JsonRPC.Protocol;

/// <summary>
/// JSON RPC error object. The static members represent errors specified in the protocol.
/// </summary>
[PublicAPI]
public sealed record RPCError {
    /// <summary>
    /// A Number that indicates the error type that occurred.
    /// </summary>
    /// <remarks>
    /// Codes -32768 to -32000 are reserved for use by the JSON RPC spec. -32000 through -32099 are for internal server errors.
    /// </remarks>
    [JsonPropertyName("code"), JsonRequired]
    public required int Code { get; init; }

    /// <summary>
    /// A String providing a short description of the error.
    /// The message SHOULD be limited to a concise single sentence.
    /// </summary>
    [JsonPropertyName("message"), JsonRequired]
    public required string Message { get; init; }

    /// <summary>
    /// A Primitive or Structured value that contains additional information about the error.
    /// This may be omitted.
    /// The value of this member is defined by the Server (e.g. detailed error information, nested errors etc.).
    /// </summary>
    [JsonPropertyName("data")]
    public JsonElement? Data { get; init; }

    /// <summary>
    /// Invalid JSON was received by the server or an error occurred on the server while parsing the JSON text.
    /// </summary>
    public static readonly RPCError ParseError = new() {
        Code = -32700,
        Message = "Parse Error",
    };

    /// <summary>
    /// The JSON sent is not a valid Request object.
    /// </summary>
    public static readonly RPCError InvalidRequest = new() {
        Code = -32600,
        Message = "Invalid Request",
    };

    /// <summary>
    /// The method does not exist or is not avalible.
    /// </summary>
    public static readonly RPCError MethodNotFound = new() {
        Code = -32601,
        Message = "Method not found",
    };

    /// <summary>
    /// Invalid method parameter(s). Recomended to specify the invalid parameters in data.
    /// </summary>
    public static readonly RPCError InvalidMethodParameters = new() {
        Code = -32602,
        Message = "Invalid params",
    };

    /// <summary>
    /// Internal JSON-RPC error.
    /// </summary>
    public static readonly RPCError InternalError = new() {
        Code = -32603,
        Message = "Method not found",
    };
}
