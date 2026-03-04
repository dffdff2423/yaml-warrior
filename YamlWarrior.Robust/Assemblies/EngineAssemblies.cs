// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Reflection;

using YamlWarrior.Common.Logger;

namespace YamlWarrior.Robust.Assemblies;

public sealed class EngineAssemblies {
    public readonly Assembly Shared;

    public readonly Type PrototypeAttribute;
    public readonly PropertyInfo PrototypeAttributeTypeProperty;

    public readonly Type IInheritingPrototype;

    public EngineAssemblies(string sharedPath) {
        Log.I($"Loading engine assembly: {sharedPath}");
        Shared = Assembly.LoadFrom(sharedPath);

        var prototype = Shared.GetType(RobustNames.PrototypeAttribute);
        if (prototype == null) {
            Log.F($"Failed to load `{RobustNames.PrototypeAttribute} from {sharedPath}");
            throw new InvalidDataException(nameof(sharedPath));
        }
        PrototypeAttribute = prototype;

        var typeField = prototype.GetProperty("Type");
        if (typeField == null) {
            Log.F($"Failed to load field `Type` from `{RobustNames.PrototypeAttribute} in assembly {sharedPath}");
            throw new InvalidDataException(nameof(sharedPath));
        }
        PrototypeAttributeTypeProperty = typeField;

        var iInheriting = Shared.GetType(RobustNames.IInheritingPrototype);
        if (iInheriting == null) {
            Log.F($"Failed to load `{RobustNames.IInheritingPrototype} from {sharedPath}");
            throw new InvalidDataException(nameof(sharedPath));
        }
        IInheritingPrototype = iInheriting;
    }
}
