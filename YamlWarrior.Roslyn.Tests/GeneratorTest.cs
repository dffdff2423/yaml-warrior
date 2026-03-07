// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Runtime.CompilerServices;
using System.Text.Json;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using YamlWarrior.Common.Serialization;
using YamlWarrior.Roslyn.Generators;

namespace YamlWarrior.Roslyn.Tests;

public static class GeneratorTest {
    public static Task Verify(string src, bool genShouldError = false) {
        var tree = CSharpSyntaxTree.ParseText(src);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(asm => !asm.IsDynamic && !string.IsNullOrWhiteSpace(asm.Location))
            .Select(asm => MetadataReference.CreateFromFile(asm.Location))
            .Concat([
                MetadataReference.CreateFromFile(typeof(JsonUnionVariantAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(JsonSerializer).Assembly.Location),
            ]);

        var compilation = CSharpCompilation.Create(
            assemblyName: "Test",
            syntaxTrees: [tree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Assert.That(compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray(), Is.Empty);

        var gen = new JsonTaggedUnionGenerator();
        var driver = CSharpGeneratorDriver.Create(gen);

        var outDriver = driver.RunGenerators(compilation);
        var results = outDriver.GetRunResult();

        if (genShouldError) {
            return Verifier.Verify(results.Diagnostics).UseDirectory("snapshots");
        }

        var newComp = compilation.AddSyntaxTrees(results.GeneratedTrees);
        Assert.That(newComp.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray(), Is.Empty);
        return Verifier.Verify(outDriver).UseDirectory("snapshots");
    }

    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Initialize();
}
