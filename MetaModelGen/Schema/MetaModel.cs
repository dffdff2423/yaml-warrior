// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json.Serialization;

namespace MetaModelGen.Schema;

public sealed record MetaModel {
    [JsonPropertyName("enumerations"), JsonRequired]
    public required Enumeration[] Enumerations { get; init; }

    [JsonPropertyName("metaData"), JsonRequired]
    public required MetaData MetaData { get; init; }

    [JsonPropertyName("notifications"), JsonRequired]
    public required Notification[] Notifications { get; init; }
}
