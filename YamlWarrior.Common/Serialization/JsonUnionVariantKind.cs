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
}
