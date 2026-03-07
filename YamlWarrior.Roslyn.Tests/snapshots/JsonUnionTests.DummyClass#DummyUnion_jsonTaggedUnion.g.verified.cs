//HintName: DummyUnion_jsonTaggedUnion.g.cs
#nullable enable
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;



[JsonConverter(typeof(DummyUnionConverter))]
public partial record DummyUnion {
    [JsonConverter(typeof(DummyUnionConverter))]
    public partial record Integer;
    [JsonConverter(typeof(DummyUnionConverter))]
    public partial record String;
    [JsonConverter(typeof(DummyUnionConverter))]
    public partial record Array;
    public partial record ExclusiveObj;

}

internal sealed class DummyUnionConverter : JsonConverter<DummyUnion> {

    public override bool CanConvert(Type t)
          => t == typeof(DummyUnion) || t == typeof(DummyUnion.Integer) || t == typeof(DummyUnion.String) || t == typeof(DummyUnion.Array);

    public override DummyUnion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var elem = JsonSerializer.Deserialize<JsonElement>(ref reader, options);

        switch (elem.ValueKind) {
            case JsonValueKind.Number:
                return new DummyUnion.Integer(elem.GetUInt64());
            case JsonValueKind.String:
                var str = elem.GetString();
                return str == null ? null : new DummyUnion.String(str);
            case JsonValueKind.Array:
                var arrVal = elem.Deserialize<string[]>(options);
                return arrVal == null ? null : new DummyUnion.Array(arrVal);
            case JsonValueKind.Object:
                return elem.Deserialize<DummyUnion.ExclusiveObj>(options);
            case JsonValueKind.Null:
                return null;
            default:
                throw new FormatException($"Unsupported variant type: {reader.TokenType.ToString()}");
        }
    }

    public override void Write(Utf8JsonWriter writer, DummyUnion value, JsonSerializerOptions options) {
        switch (value) {
            case DummyUnion.Integer num:
                writer.WriteNumberValue(num.Value);
                break;
            case DummyUnion.String str:
                writer.WriteStringValue(str.Value);
                break;
            case DummyUnion.Array arr:
                JsonSerializer.Serialize(writer, arr.Value, options);
                break;
            case DummyUnion.ExclusiveObj var:
                JsonSerializer.Serialize(writer, (DummyUnion.ExclusiveObj)var, options);
                break;

            default:
                throw new ArgumentException("Value is not a known variant value");
        }
    }
}