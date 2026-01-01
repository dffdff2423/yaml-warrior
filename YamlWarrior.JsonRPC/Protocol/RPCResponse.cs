// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace YamlWarrior.JsonRPC.Protocol;

/// <summary>
/// Base type for JsonRPC 2.0 responses
/// </summary>
[PublicAPI]
public abstract record RPCResponse {
    /// <summary>
    /// A String specifying the version of the JSON-RPC protocol. MUST be exactly "2.0".
    /// </summary>
    [JsonPropertyName("jsonrpc"), JsonRequired]
    public string JsonRPC { get; init; } = "2.0";

    /// <summary>
    /// <list type="bullet">
    /// <item>This member is REQUIRED.</item>
    /// <item>It MUST be the same as the value of the id member in the Request Object.</item>
    /// <item>If there was an error in detecting the id in the Request object (e.g. Parse error/Invalid Request), it MUST be (json) Null.</item>
    /// </list>
    /// </summary>
    [JsonPropertyName("id"), JsonRequired]
    public JsonElement Id { get; init; }
}
