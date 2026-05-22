// SPDX-FileCopyrightText: (C) 2025 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Runtime.InteropServices.JavaScript;

using YamlWarrior.Common.CommandLine;

namespace MetaModelGen;

internal sealed class Program {
    static void Main(string[] argv) {
        const string defaultInputDir = "../../../data/metaModel.json";
        const string defaultOutputDir = "../../../YamlWarrior.Lsp.Protocol";
        const string defaultNs = "YamlWarrior";

        var flags = new[] {
            new Flag(Type: ArgumentType.String, Long: "input", Short: 'i', HelpText: $"Path to meta model JSON file. Defaults to {defaultOutputDir} (the correct value if running in IDE)"),
            new Flag(Type: ArgumentType.String, Long: "output-dir", Short: 'o',  HelpText: $"Path to output directory. Defaults to {defaultOutputDir}"),
            new Flag(Type: ArgumentType.String, Long: "namespace", Short: 'n',  HelpText: $"Namespace of generated code. Defaults to {defaultNs}"),
        };
        var opts = ArgumentParser.ParseArguments(flags, "[options...]", argv);

        var modelPath = (string)opts.GetValueOrDefault("input",  defaultInputDir);
        var model = LspMetaGenerator.LoadModel(modelPath);
        if (model == null) {
            Console.Error.WriteLine("Failed to load model");
            Environment.Exit(1);
        }

        Console.Error.WriteLine($"Successfully loaded model `{Path.GetFullPath(modelPath)}`");

        var outputDir = (string)opts.GetValueOrDefault("output-dir", defaultOutputDir);
        var ns = (string)opts.GetValueOrDefault("namespace", defaultNs);
        Console.Error.WriteLine($"Writing model to {Path.GetFullPath(outputDir)}/");
        LspMetaGenerator.WriteFiles(model, outputDir, ns);

        Console.Error.WriteLine("Done!");
    }
}
