// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace YamlWarrior.Lsp.JsonRPC;

/// <summary>
/// Base type for JsonRPC 2.0 responses
/// </summary>
[PublicAPI]
[JsonConverter(typeof(RPCResponseConverter))]
public abstract record RPCResponse {
    private RPCResponse() { } // Don't allow anything outside of this class to inherit from this.

    /// <summary>
    /// A String specifying the version of the JSON-RPC protocol. MUST be exactly "2.0".
    /// </summary>
    [JsonPropertyName("jsonrpc")]
    public string JsonRPC { get; init; } = "2.0";

    /// <summary>
    /// <list type="bullet">
    /// <item>This member is REQUIRED.</item>
    /// <item>It MUST be the same as the value of the id member in the Request Object.</item>
    /// <item>If there was an error in detecting the id in the Request object (e.g. Parse error/Invalid Request), it MUST be (json) Null.</item>
    /// </list>
    /// </summary>
    [JsonPropertyName("id"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required RPCId? Id { get; init; }

    /// <summary>
    /// Was this RPC Request successful?
    /// </summary>
    // Probably not the best way of doing this, but should be fine.
    [JsonIgnore]
    public abstract bool IsSuccess { get; }

    /// <summary>
    /// Response representing a successful method invocation.
    /// </summary>
    [PublicAPI]
    public sealed record Success : RPCResponse {
        /// <summary>
        /// This member is REQUIRED on success.
        /// This member MUST NOT exist if there was an error invoking the method.
        /// The value of this member is determined by the method invoked on the Server.
        /// </summary>
        [JsonPropertyName("result"), JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public JsonElement? Result { get; init; }

        [JsonIgnore]
        public override bool IsSuccess => true;
    }

    /// <summary>
    /// Response representing a failed RPC request
    /// </summary>
    [PublicAPI]
    public sealed record Failure : RPCResponse {
        /// <summary>
        /// This member is REQUIRED if there is an error. This member MUST NOT exist if there was no error triggered during
        /// invocation.
        /// </summary>
        [JsonPropertyName("error")]
        public required RPCError Error { get; init; }

        [JsonIgnore]
        public override bool IsSuccess => false;
    }
}

internal class RPCResponseConverter : JsonConverter<RPCResponse> {
    // We don't override CanConvert since this converter should only really be applied when reading.
    // We also can't use the builtin way of deserializing derived types since that relies on metadata in the JSON

    public override RPCResponse? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var elem = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
        if (elem.ValueKind != JsonValueKind.Object)
            throw new JsonException("RPCReponse must be an object");

        if (elem.TryGetProperty("result", out _)) {
            return elem.Deserialize<RPCResponse.Success>();
        }

        if (elem.TryGetProperty("error", out _)) {
            return elem.Deserialize<RPCResponse.Failure>();
        }

        throw new JsonException("Malformed Response");
    }

    public override void Write(Utf8JsonWriter writer, RPCResponse value, JsonSerializerOptions options) {
        if (value.IsSuccess) {
            JsonSerializer.Serialize(writer, (RPCResponse.Success)value, options);
        } else {
            JsonSerializer.Serialize(writer, (RPCResponse.Failure)value, options);
        }
    }
}
