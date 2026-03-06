// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json.Serialization;

namespace MetaModelGen.Schema;

public sealed record Notification : TypeMetaInfo {
    [JsonPropertyName("messageDirection"), JsonRequired]
    public required MessageDirection MessageDirection { get; init; }

    /// <summary>
    /// The request's method name
    /// </summary>
    [JsonPropertyName("method")]
    public required string Method { get; init; }

    // TODO: Params

    /// <summary>
    /// Optional a dynamic registration method if it different from the request's method.
    /// </summary>
    [JsonPropertyName("registrationMethod")]
    public string? RegistrationMethod { get; init; }

    // TODO: registrationOptions
}
