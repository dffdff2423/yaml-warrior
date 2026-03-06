//HintName: DummyUnion_jsonTaggedUnion.g.cs
#nullable enable
using System;
using System.Text.Json;
using System.Text.Json.Serialization;



[JsonConverter(typeof(DummyUnionConverter))]
public partial record DummyUnion {
    [JsonConverter(typeof(DummyUnionConverter))]
    public partial record Integer;
    [JsonConverter(typeof(DummyUnionConverter))]
    public partial record String;

}

internal sealed class DummyUnionConverter : JsonConverter<DummyUnion> {
    public override bool CanConvert(Type t)
          => t == typeof(DummyUnion) || t == typeof(DummyUnion.Integer) || t == typeof(DummyUnion.String);

    public override DummyUnion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        switch (reader.TokenType) {
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

            default:
                throw new ArgumentException("Value is not a known variant value");
        }
    }
}