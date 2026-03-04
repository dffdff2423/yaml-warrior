// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Reflection;

using JetBrains.Annotations;

using YamlWarrior.Common.Logger;

namespace YamlWarrior.Robust.Assemblies;

/// <summary>
/// A content assembly in an RT project (eg. Content.Server in ss14).
/// </summary>
[PublicAPI]
public class ContentAssembly {
    private readonly EngineAssemblies _engine;
    private readonly Assembly _asm;
    private readonly string _path;

    public ContentAssembly(EngineAssemblies engine, string path) {
        _path = path;
        _engine = engine;

        Log.I($"Loading assembly {path}");
        _asm = Assembly.LoadFrom(path);

        var types = _asm.GetTypes();
        foreach (var ty in types) {
            if (ty.GetCustomAttribute(_engine.PrototypeAttribute) != null) {
                Log.D($"Found prototype {ty.AssemblyQualifiedName}");
            }
        }
    }
}
