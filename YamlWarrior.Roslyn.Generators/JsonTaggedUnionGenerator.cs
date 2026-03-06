// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using YamlWarrior.Common.Serialization;

namespace YamlWarrior.Roslyn.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class JsonTaggedUnionGenerator : IIncrementalGenerator {
    private const string ParentAttributeName = "YamlWarrior.Common.Serialization.JsonUnionAttribute";
    private const string MemberAttributeName = "YamlWarrior.Common.Serialization.JsonUnionVariantAttribute";
    private const string ValuePropertyName = "Value";
    // I swear there is some way for this to work:
    // private static readonly string ParentAttributeName = typeof(JsonUnionAttribute).FullName!;
    // private static readonly string MemberAttributeName = typeof(JsonUnionVariantKind).FullName!;


    public void Initialize(IncrementalGeneratorInitializationContext ctx) {
        var provider = ctx.SyntaxProvider
            .ForAttributeWithMetadataName<GeneratorOutput>(
                ParentAttributeName,
                static (node, _) => node is TypeDeclarationSyntax,
                static (ctx, _) => Generate((ITypeSymbol)ctx.TargetSymbol))
            .Where(static type => type != null);

        ctx.RegisterSourceOutput(
            provider,
            static (ctx, output) => {
                foreach (var diag in output.Diagnostics) {
                    ctx.ReportDiagnostic(diag);
                }
                foreach (var (name, code) in output.Files) {
                    ctx.AddSource(name, code);
                }
            });
    }

    private static GeneratorOutput Generate(ITypeSymbol sym) {
        var ns = sym.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : $"namespace {sym.ContainingNamespace.ToDisplayString()};";
        var variants = new List<(INamedTypeSymbol sym, JsonUnionVariantKind kind)>();

        foreach (var member in sym.GetTypeMembers()) {
            var attr = member
                .GetAttributes()
                .SingleOrDefault(attr => attr.AttributeClass?.ToDisplayString() == MemberAttributeName);
            if (attr == null)
                continue;

            var rawKind = attr.ConstructorArguments.First();
            if (rawKind.Value == null)
                return new GeneratorOutput(); // Should already be enforced by the c# compiler
            var kind = (JsonUnionVariantKind)rawKind.Value;
            variants.Add((member, kind));
        }

        var diag = new List<Diagnostic>();
        if (variants.Count(v => v.kind == JsonUnionVariantKind.Number) > 1) {
            diag.AddRange(variants
                .Where(v => v.kind == JsonUnionVariantKind.Number)
                .Select(v => Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "YW1001",
                        title: "Unsupported Duplicate Union Variant",
                        messageFormat: "Cannot have multiple number variants",
                        category: "YamlWarrior.Roslyn.Generators",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    v.sym.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None)));
        }
        if (variants.Count(v => v.kind == JsonUnionVariantKind.String) > 1) {
            diag.AddRange(variants
                .Where(v => v.kind == JsonUnionVariantKind.String)
                .Select(v => Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "YW1001",
                        title: "Unsupported Duplicate Union Variant",
                        messageFormat: "Cannot have multiple string variants",
                        category: "YamlWarrior.Roslyn.Generators",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    v.sym.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None)));
        }

        var (readVariants, readDiag) = GenerateReadVariants(variants, sym.Name);

        if (readDiag != null)
            diag.Add(readDiag);

        if (diag.Count > 0)
            return new GeneratorOutput { Diagnostics = diag };

        var converter = $"{sym.Name}Converter";

        var txt =
            $$"""
              #nullable enable
              using System;
              using System.Text.Json;
              using System.Text.Json.Serialization;

              {{ns}}

              [JsonConverter(typeof({{converter}}))]
              public partial record {{sym.Name}} {
              {{AnnotateVariants(variants.Select(v => v.sym), converter)}}
              }

              internal sealed class {{converter}} : JsonConverter<{{sym.Name}}> {
                  public override bool CanConvert(Type t)
                        => {{
                            variants
                                .Select(v=> $"t == typeof({sym.Name}.{v.sym.Name})")
                                .Aggregate($"t == typeof({sym.Name})", (lhs, rhs) => $"{lhs} || {rhs}")
                        }};

                  public override {{sym.Name}}? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                      switch (reader.TokenType) {
              {{readVariants}}
                          default:
                              throw new FormatException($"Unsupported variant type: {reader.TokenType.ToString()}");
                      }
                  }

                  public override void Write(Utf8JsonWriter writer, {{sym.Name}} value, JsonSerializerOptions options) {
                      switch (value) {
              {{GenerateWriteVariants(variants, sym.Name)}}
                          default:
                              throw new ArgumentException("Value is not a known variant value");
                      }
                  }
              }
              """;

        return new GeneratorOutput {
            Files = [($"{sym}_jsonTaggedUnion.g.cs", txt)],
            Diagnostics = diag,
        };
    }

    private static string AnnotateVariants(IEnumerable<INamedTypeSymbol> variants, string converter) {
        var sb = new StringBuilder();
        foreach (var ty in variants) {
            sb.AppendLine($"    [JsonConverter(typeof({converter}))]");
            sb.AppendLine($"    public partial record {ty.Name};");
        }

        return sb.ToString();
    }

    private static (string, Diagnostic?) GenerateReadVariants(IEnumerable<(INamedTypeSymbol, JsonUnionVariantKind)> variants, string parentName) {
        var sb = new StringBuilder();
        foreach (var (ty, kind) in variants) {
            var value = (IPropertySymbol?)ty.GetMembers().SingleOrDefault(mem => mem is IPropertySymbol && mem.Name == ValuePropertyName);
            if (value == null) {
                return ("", Diagnostic.Create(new DiagnosticDescriptor(
                        id: "YW1011",
                        title: "No Value Property Detected",
                        messageFormat: "Union variant must have a property called Value with the correct type",
                        category: "YamlWarrior.Roslyn.Generators",
                        defaultSeverity: DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    ty.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ?? Location.None));
            }

            switch (kind) {
            case JsonUnionVariantKind.String:
                sb.AppendLine($"            case JsonTokenType.String:");
                sb.AppendLine("                var str = reader.GetString();");
                sb.AppendLine($"                return str == null ? null : new {parentName}.{ty.Name}(str);");
                break;
            case JsonUnionVariantKind.Number:
                sb.AppendLine($"            case JsonTokenType.Number:");
                var type = value.Type.SpecialType;
                switch (type) {
                case SpecialType.System_Int32:
                    sb.AppendLine($"                return new {parentName}.{ty.Name}(reader.GetInt32());");
                    break;
                case SpecialType.System_UInt32:
                    sb.AppendLine($"                return new {parentName}.{ty.Name}(reader.GetUInt32());");
                    break;
                case SpecialType.System_Int64:
                    sb.AppendLine($"                return new {parentName}.{ty.Name}(reader.GetInt64());");
                    break;
                case SpecialType.System_UInt64:
                    sb.AppendLine($"                return new {parentName}.{ty.Name}(reader.GetUInt64());");
                    break;
                case SpecialType.System_Double:
                    sb.AppendLine($"                return new {parentName}.{ty.Name}(reader.GetDouble());");
                    break;
                default:
                    // TODO: Support other integral types
                    return ("", Diagnostic.Create(new DiagnosticDescriptor(
                            id: "YW1011",
                            title: "Unsupported Duplicate Union Variant",
                            messageFormat: "Cannot have multiple number variants",
                            category: "YamlWarrior.Roslyn.Generators",
                            defaultSeverity: DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        value.Type.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().GetLocation() ??
                        Location.None));
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        sb.AppendLine("            case JsonTokenType.Null:");
        sb.AppendLine("                return null;");
        return (sb.ToString(), null);
    }

    private static string GenerateWriteVariants(IEnumerable<(INamedTypeSymbol, JsonUnionVariantKind)> variants, string parentName) {
        var sb = new StringBuilder();
        foreach (var (ty, kind) in variants) {
            switch (kind) {
            case JsonUnionVariantKind.String:
                sb.AppendLine($"            case {parentName}.{ty.Name} str:");
                sb.AppendLine($"                writer.WriteStringValue(str.{ValuePropertyName});");
                break;
            case JsonUnionVariantKind.Number:
                sb.AppendLine($"            case {parentName}.{ty.Name} num:");
                sb.AppendLine($"                writer.WriteNumberValue(num.{ValuePropertyName});");
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
            sb.AppendLine("                break;");
        }
        return sb.ToString();
    }
}
