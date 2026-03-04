// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

namespace YamlWarrior.Common.Platform;

public static class PathUtil {
    public static string ExpandTilde(string path) {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (path.First() == '~') {
            return Path.Join(home, path[1..]);
        }

        return path;
    }
}
