// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace YamlWarrior.JsonRPC.Protocol;

/// <summary>
/// Response representing a successful method invocation.
/// </summary>
[PublicAPI]
public sealed record RPCSuccessResponse : RPCResponse {
    /// <summary>
    /// This member is REQUIRED on success.
    /// This member MUST NOT exist if there was an error invoking the method.
    /// The value of this member is determined by the method invoked on the Server.
    /// </summary>
    [JsonPropertyName("result"), JsonRequired]
    public JsonElement Result { get; init; }
}
