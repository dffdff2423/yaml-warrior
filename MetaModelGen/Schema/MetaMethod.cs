// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json.Serialization;

namespace MetaModelGen.Schema;

/// <summary>
/// Base class for requests and messages. Not defined by schema but used here.
/// </summary>
public abstract record MetaMethod : MetaInfoDef {
    [JsonPropertyName("messageDirection"), JsonRequired]
    public required MessageDirection MessageDirection { get; init; }

    /// <summary>
    /// The request's method name
    /// </summary>
    [JsonPropertyName("method"), JsonRequired]
    public required string Method { get; init; }

    /// <summary>
    /// Method params
    /// </summary>
    [JsonPropertyName("params")]
    public Params? Params { get; init; }

    /// <summary>
    /// Optional a dynamic registration method if it different from the request's method.
    /// </summary>
    [JsonPropertyName("registrationMethod")]
    public string? RegistrationMethod { get; init; }

    /// <summary>
    /// Optional registration options if the notification supports dynamic registration.
    /// </summary>
    [JsonPropertyName("registrationOptions")]
    public MetaType? RegistrationOptions { get; init; }
}


public sealed record Notification : MetaMethod;

public sealed record Request : MetaMethod {
    /// <summary>
    /// An optional error data type.
    /// </summary>
    [JsonPropertyName("errorData")]
    public MetaType? ErrorData { get; init; }
}
