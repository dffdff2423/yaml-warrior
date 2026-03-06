// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using Microsoft.CodeAnalysis;

namespace YamlWarrior.Roslyn.Generators;

public sealed record GeneratorOutput {
    public IEnumerable<(string file, string code)> Files { get; set; } = [];
    public IEnumerable<Diagnostic> Diagnostics { get; set; } = [];
};
