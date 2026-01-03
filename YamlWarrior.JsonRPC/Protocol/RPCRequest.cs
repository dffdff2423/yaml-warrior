// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace YamlWarrior.JsonRPC.Protocol;

/// <summary>
/// JsonRPC 2.0 Request
/// </summary>
[PublicAPI]
public sealed record RPCRequest {
    /// <summary>
    /// A String specifying the version of the JSON-RPC protocol. MUST be exactly "2.0".
    /// </summary>
    [JsonPropertyName("jsonrpc"), JsonRequired]
    public string JsonRPC { get; init; } = "2.0";

    /// <summary>
    /// A String containing the name of the method to be invoked. Method names that begin with the word rpc followed by
    /// a period character (U+002E or ASCII 46) are reserved for rpc-internal methods and extensions and MUST NOT be
    /// used for anything else.
    /// </summary>
    [JsonPropertyName("method"), JsonRequired]
    public required string Method { get; init; }

    /// <summary>
    /// A Structured value that holds the parameter values to be used during the invocation of the method. This member MAY be omitted.
    /// </summary>
    /// <remarks>
    /// If present, parameters for the rpc call MUST be provided as a Structured value. Either by-position through an
    /// Array or by-name through an Object.
    /// <list type="bullet">
    ///     <item>
    ///         <description><b>by-position:</b> params MUST be an Array, containing the values in the Server expected order</description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///         <b>by-name:</b> params MUST be an Object, with member names that match the Server expected
    ///         parameter names. The absence of expected names MAY result in an error being generated. The names MUST
    ///         match exactly, including case, to the method's expected parameters.
    ///         </description>
    ///     </item>
    /// </list>
    /// </remarks>
    [JsonPropertyName("params"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? Params { get; init; }

    /// <summary>
    /// An identifier established by the Client that MUST contain a String, Number, or NULL value if included. If it is
    /// not included this object is assumed to be a notification. The value SHOULD normally not be Null [1] and Numbers
    /// SHOULD NOT contain fractional parts [2]
    /// </summary>
    /// <remarks>
    /// In the event the client provides a null value for this field it will be null rather than have a null value. <br />
    /// <br />
    /// Extra Info: <br />
    ///
    /// The Server MUST reply with the same value in the Response object if included. This member is used to correlate
    /// the context between the two objects. <br />
    /// <br />
    /// [1] The use of Null as a value for the id member in a Request object is discouraged, because this specification
    /// uses a value of Null for Responses with an unknown id. Also, because JSON-RPC 1.0 uses an id value of Null for
    /// Notifications this could cause confusion in handling. <br />
    /// [2] Fractional parts may be problematic, since many decimal fractions cannot be represented exactly as binary
    /// fractions.
    /// </remarks>
    [JsonPropertyName("id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public RPCId? Id { get; init; }
}
