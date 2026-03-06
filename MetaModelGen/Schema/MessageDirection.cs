// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json.Serialization;

namespace MetaModelGen.Schema;

public enum MessageDirection {
    [JsonStringEnumMemberName("clientToServer")]
    ClientToServer,
    [JsonStringEnumMemberName("serverToClient")]
    ServerToClient,
    [JsonStringEnumMemberName("both")]
    Bidirectional,
}
