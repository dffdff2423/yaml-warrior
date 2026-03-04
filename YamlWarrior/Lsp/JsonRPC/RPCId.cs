// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;
using System.Text.Json.Serialization;

namespace YamlWarrior.Lsp.JsonRPC;

/// <summary>
/// JSON RPC message id varient. Can be a string or an int.
/// </summary>
[JsonConverter(typeof(RPCIdConverter))]
public abstract record RPCId {
    private RPCId() {} // Disallow inheritience outside of this class

    /// <summary>
    /// String RPC request Id
    /// </summary>
    [JsonConverter(typeof(RPCIdConverter))]
    public sealed record StringId(string Value) : RPCId;

    /// <summary>
    /// String RPC request Id
    /// </summary>
    [JsonConverter(typeof(RPCIdConverter))]
    public sealed record IntId(int Value) : RPCId;
}

internal class RPCIdConverter : JsonConverter<RPCId> {
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(RPCId) ||
                                                           typeToConvert == typeof(RPCId.StringId) ||
                                                           typeToConvert == typeof(RPCId.IntId);

    public override RPCId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        switch (reader.TokenType) {
            case JsonTokenType.String:
                var str = reader.GetString();
                return str == null ? null : new RPCId.StringId(str);
            case JsonTokenType.Number:
                return new RPCId.IntId(reader.GetInt32());
            case JsonTokenType.Null:
                return null;
            default:
                throw new FormatException("RPCId must be a number, string, or null");
        }
    }

    public override void Write(Utf8JsonWriter writer, RPCId value, JsonSerializerOptions options) {
        switch (value) {
            case RPCId.StringId str:
                writer.WriteStringValue(str.Value);
                break;
            case RPCId.IntId i:
                writer.WriteNumberValue(i.Value);
                break;
            default:
                throw new ArgumentException("Value is not a kn own RPCId value");
        }
    }
}
