//HintName: YamlWarrior.Tests.Generated.SpecificObjectUnion_jsonTaggedUnion.g.cs
#nullable enable
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace YamlWarrior.Tests.Generated;

[JsonConverter(typeof(SpecificObjectUnionConverter))]
public partial record SpecificObjectUnion {
    public partial record Object1;
    public partial record Object2;

}

internal sealed class SpecificObjectUnionConverter : JsonConverter<SpecificObjectUnion> {
    private static readonly string? KindFieldJsonName = ((JsonPropertyNameAttribute?)Attribute.GetCustomAttribute(
            typeof(SpecificObjectUnion).GetMember("Kind").Single(),
            typeof(JsonPropertyNameAttribute)))?.Name;
    public override bool CanConvert(Type t)
          => t == typeof(SpecificObjectUnion);

    public override SpecificObjectUnion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var elem = JsonSerializer.Deserialize<JsonElement>(ref reader, options);

        switch (elem.ValueKind) {
            case JsonValueKind.Object:
               SpecificObjectUnion? obj = null;
                var objKind = elem.GetProperty(KindFieldJsonName ?? "Kind").GetString();
                try { obj = elem.Deserialize<SpecificObjectUnion.Object1>(options); } catch { }
                if (obj != null && obj.Kind == objKind) return obj;
                try { obj = elem.Deserialize<SpecificObjectUnion.Object2>(options); } catch { }
                if (obj != null && obj.Kind == objKind) return obj;
            return null;
            break;
            case JsonValueKind.Null:
                return null;
            default:
                throw new FormatException($"Unsupported variant type: {reader.TokenType.ToString()}");
        }
    }

    public override void Write(Utf8JsonWriter writer, SpecificObjectUnion value, JsonSerializerOptions options) {
        switch (value) {
            case SpecificObjectUnion.Object1 var:
                JsonSerializer.Serialize(writer, (SpecificObjectUnion.Object1)var, options);
                break;
            case SpecificObjectUnion.Object2 var:
                JsonSerializer.Serialize(writer, (SpecificObjectUnion.Object2)var, options);
                break;

            default:
                throw new ArgumentException("Value is not a known variant value");
        }
    }
}