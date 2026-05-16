// SPDX-FileCopyrightText: (C) 2025 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using YamlWarrior.Common.CommandLine;

namespace MetaModelGen;

internal sealed class Program {
    static void Main(string[] argv) {
        const string defaultInputDir = "../../../data/metaModel.json";
        const string defaultOutputDir = "../../../YamlWarrior.Lsp.Protocol";

        var flags = new[] {
            new Flag(Type: ArgumentType.String, Long: "input", Short: 'i', HelpText: $"Path to meta model JSON file. Defaults to {defaultOutputDir} (the correct value if running in IDE)"),
            new Flag(Type: ArgumentType.String, Long: "output-dir", Short: 'o',  HelpText: $"Path to output directory. Defaults to {defaultOutputDir}"),
        };
        var opts = ArgumentParser.ParseArguments(flags, "[options...]", argv);

        var modelPath = (string)opts.GetValueOrDefault("input",  defaultInputDir);
        var model = LspMetaGenerator.TryLoadModel(modelPath);
        Console.WriteLine($"Successfully loaded model `{modelPath}`");
        Console.WriteLine("Done!");
    }
}
