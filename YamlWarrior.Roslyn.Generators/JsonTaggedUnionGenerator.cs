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
        var variants = new List<(ITypeSymbol sym, JsonUnionVariantKind kind)>();

        foreach (var member in sym.GetMembers()) {
            if (member is ITypeSymbol memberTy) {
                var attr = memberTy
                    .GetAttributes()
                    .SingleOrDefault(attr => attr.AttributeClass?.ToDisplayString() == MemberAttributeName);
                if (attr == null)
                    continue;

                var rawKind = attr.ConstructorArguments.First();
                if (rawKind.Value == null)
                    return new GeneratorOutput(); // Should already be enforced by the c# compiler
                var kind = (JsonUnionVariantKind)rawKind.Value;
                variants.Add((memberTy, kind));
            }
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
              {{AnnotateVariants(variants.Select(v => v.Item1), converter)}}
              }

              internal sealed class {{converter}} : JsonConverter<{{sym.Name}}> {
                  public override bool CanConvert(Type t)
                        => {{
                            variants
                                .Select(v=> $"t == typeof({sym.Name}.{v.Item1.Name})")
                                .Aggregate($"t == typeof({sym.Name})", (lhs, rhs) => $"{lhs} || {rhs}")
                        }};

                  public override {{sym.Name}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                      switch (reader.TokenType) {
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

    private static string AnnotateVariants(IEnumerable<ITypeSymbol> variants, string converter) {
        var sb = new StringBuilder();
        foreach (var ty in variants) {
            sb.AppendLine($"    [JsonConverter(typeof({converter}))]");
            sb.AppendLine($"    public partial record {ty.Name};");
        }

        return sb.ToString();
    }

    private static string GenerateWriteVariants(IEnumerable<(ITypeSymbol, JsonUnionVariantKind)> variants, string parentName) {
        var sb = new StringBuilder();
        foreach (var (ty, kind) in variants) {
            switch (kind) {
            case JsonUnionVariantKind.String:
                sb.AppendLine($"            case {parentName}.{ty.Name} str:");
                sb.AppendLine("                writer.WriteStringValue(str.Value);");
                break;
            case JsonUnionVariantKind.Number:
                sb.AppendLine($"            case {parentName}.{ty.Name} num:");
                sb.AppendLine("                writer.WriteNumberValue(num.Value);");
                break;
            default:
                throw new ArgumentOutOfRangeException();
            }
            sb.AppendLine("                break;");
        }
        return sb.ToString();
    }
}
