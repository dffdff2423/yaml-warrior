// SPDX-FileCopyrightText: (C) 2025 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;

using YamlWarrior.Common.CommandLine;
using YamlWarrior.Common.Platform;
using YamlWarrior.Robust;
using YamlWarrior.Robust.Assemblies;

namespace YamlWarrior;

internal static class Program {
    private static void Main(string[] argv) {
        var flags = new[] {
            // TODO: set log level
            new Flag(Type: ArgumentType.Bool, Long: "dump", HelpText: "Dump assemblies then exit"),
            // TODO: allow repeating this flag
            new Flag(Type: ArgumentType.String, Long: "assembly-dir", Short: 'a', HelpText: "Path to an ss14 build directory"),
        };
        var opts = ArgumentParser.ParseArguments(flags, "-a <assembly> [options...]", argv);

        if (!opts.ContainsKey("assembly-dir")) {
            Console.Error.WriteLine("Must specify a build directory with -a or --assembly-dir");
            Environment.Exit(1);
        }

        var build = PathUtil.ExpandTilde((string)opts["assembly-dir"]);

        var rtSharedPath = Path.Join(build, AssemblyNames.RobustSharedPath);
        var yamlCtx = new YamlProcessingContext(rtSharedPath);
        foreach (var seg in AssemblyNames.DefaultContentAssemblyPathSegments) {
            yamlCtx.LoadContent(Path.Join(build, seg));
        }

        if ((bool)opts["dump"]) {
            Console.WriteLine(JsonSerializer.Serialize(yamlCtx.RobustTypes, new JsonSerializerOptions { WriteIndented = true }));
            Environment.Exit(0);
        }
    }
}
