// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Text.Json;

using MetaModelGen.Schema;

namespace MetaModelGen;

public static class LspMetaGenerator {

    public static MetaModel? TryLoadModel(string path) {
        using var f= File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return JsonSerializer.Deserialize<MetaModel>(f);
    }
}
