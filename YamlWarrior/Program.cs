// SPDX-FileCopyrightText: (C) 2025 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Diagnostics;

using YamlWarrior.Common.CommandLine;

namespace YamlWarrior;

internal static class Program {
    private static void Main(string[] argv) {
        var flags = new[] {
            // TODO: set log level
            new Flag(Type: ArgumentType.Bool, Long: "dump", HelpText: "Dump assemblies then exit"),
            // TODO: allow repeating this flag
            new Flag(Type: ArgumentType.String, Long: "assembly", Short: 'a', HelpText: "Path to assembly that should be loaded"),
        };
        var opts = ArgumentParser.ParseArguments(flags, "-a <assembly> [options...]", argv);

        Debug.Assert((bool)opts["dump"]);

        var assembly = (string)opts["assembly"];
        Console.WriteLine(assembly);
    }
}
