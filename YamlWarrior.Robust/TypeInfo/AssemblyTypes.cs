// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using JetBrains.Annotations;

using YamlWarrior.Common.Logger;

namespace YamlWarrior.Robust.TypeInfo;

[PublicAPI]
public sealed record AssemblyTypes {
    /// <summary>
    /// Map of KindId to Prototype
    /// </summary>
    public Dictionary<string, PrototypeInfo> Prototypes { get; init; } = new();

    public static AssemblyTypes Merge(AssemblyTypes lhs, AssemblyTypes rhs) {
        var joined = new AssemblyTypes {
            Prototypes = new Dictionary<string, PrototypeInfo>(lhs.Prototypes)
        };

        foreach (var (kind, proto) in rhs.Prototypes) {
            if (!joined.Prototypes.TryAdd(kind, proto)) {
                Log.E($"Received prototypes with duplicate kind id: {proto.FullName} and {joined.Prototypes[kind].FullName}. Using {joined.Prototypes[kind].FullName} but this is likely an error in your project.");
            }
        }

        return joined;
    }
}
