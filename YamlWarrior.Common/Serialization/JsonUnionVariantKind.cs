// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

namespace YamlWarrior.Common.Serialization;

public enum JsonUnionVariantKind {
    /// <summary>
    /// Variant is a record with a single property with type String called Value
    /// </summary>
    String,

    /// <summary>
    /// Variant is a record with a single property with an integral type called Value
    /// </summary>
    Number,

    /// <summary>
    /// Variant is a record with a single property with an Array type called Value
    /// </summary>
    Array,

    /// <summary>
    /// Variant is the only object subclass
    /// </summary>
    ExclusiveObject,

    /// <summary>
    /// Variant is a JSON object. Supports multiple object subclasses but requires <see cref="JsonUnionObjectKindPropertyAttribute"/>
    /// </summary>
    SpecificObject,
}
