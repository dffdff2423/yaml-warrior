// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using Microsoft.CodeAnalysis;

namespace YamlWarrior.Roslyn.Generators;

public static class Diagnostics {
    public static readonly DiagnosticDescriptor UnsupportedDuplicateVariant = new(
        id: "YW1001",
        title: "Unsupported Duplicate Union Variant",
        messageFormat: "Cannot have multiple of this type of variant",
        category: "YamlWarrior.Roslyn.Generators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NoValueProperty = new(
        id: "YW1011",
        title: "No Value Property Detected",
        messageFormat: "Union variant must have a property called Value with the correct type",
        category: "YamlWarrior.Roslyn.Generators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateUnionVariant = new(
        id: "YW1012",
        title: "Unsupported Duplicate Union Variant",
        messageFormat: "Cannot have multiple number variants",
        category: "YamlWarrior.Roslyn.Generators",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
