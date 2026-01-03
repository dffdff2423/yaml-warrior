// SPDX-FileCopyrightText: (C) 2026 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: GPL-3.0-only

using System.Runtime.CompilerServices;
using System.Text;

using JetBrains.Annotations;

namespace YamlWarrior.Common.Logger;

/// <summary>
/// Shitty simple logger using global state (i know, light me on fire). I am not pulling in several thousend lines of
/// code for a fucking logging framework. This logger is written with the assumption that all the setup will be done before
/// any threads are launched.
/// </summary>
[PublicAPI]
public static class Log {
    /// <summary>
    /// Wheter or not the log has been initialized. Technically optional. Will be force-initialized to the default
    /// values after the first log is called.
    /// </summary>
    private static bool _logInitialized;

    /// <summary>
    /// The log output file. This should be set by the application.
    /// </summary>
    private static TextWriter _log = Console.Error;

    /// <summary>
    /// Minimum log level to be printed.
    /// </summary>
    public static LogLevel RequiredLevel { get; private set; } = LogLevel.Debug;

    /// <summary>
    /// Set the log file. Will do nothing if the log file is already set. This must be called before any threads are
    /// created and any log methods are called. If this is not called the log will be written to <see cref="Console.Out"/>
    /// and <see cref="RequiredLevel"/> will be set to <see cref="LogLevel.Debug"/>
    /// </summary>
    public static void SetupLogger(LogLevel requiredLevel = LogLevel.Debug, TextWriter? writer = null) {
        if (_logInitialized)
            return;

        _logInitialized = true;

        RequiredLevel = requiredLevel;
        if (writer != null)
            _log = writer;
    }

    /// <summary>
    /// Log a message to the log file. Applies fancy coloring if output is a tty.
    /// </summary>
    public static void WriteLog(string file, int line, LogLevel level, string txt) {
        _logInitialized = true;
        if (RequiredLevel > level)
            return;

        var colors = _log == Console.Out && !Console.IsOutputRedirected;

        var sb = new StringBuilder();
        sb.Append(DateTime.Now);
        sb.Append(' ');
        sb.Append($"[{file}:{line}] ");

        // Colorize log level
        if (colors) {
            switch (level) {
            case LogLevel.Debug:
                sb.Append("\e[38;5;m");
                break;
            case LogLevel.Info:
                sb.Append("\e[38;5;34m");
                break;
            case LogLevel.Warn:
                sb.Append("\e[38;5;255m");
                break;
            case LogLevel.Error:
                sb.Append("\e[38;5;160m");
                break;
            case LogLevel.Fatal:
                sb.Append("\e[38;5;52m");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }

        // Level to string
        switch (level) {
        case LogLevel.Debug:
            sb.Append("DBG");
            break;
        case LogLevel.Info:
            sb.Append("INF");
            break;
        case LogLevel.Warn:
            sb.Append("WRN");
            break;
        case LogLevel.Error:
            sb.Append("ERR");
            break;
        case LogLevel.Fatal:
            sb.Append("FTL");
            break;
        default:
            throw new ArgumentOutOfRangeException(nameof(level), level, null);
        }

        if (colors)
            sb.Append("\e[39m"); // ANSI default

        sb.Append(": ");

        sb.Append(txt);
        sb.Append('\n');

        _log.Write(sb);
    }

    public static void D(string txt, [CallerMemberName] string file = "", [CallerLineNumber] int line = 0)
        => WriteLog(file, line, LogLevel.Debug, txt);
    public static void I(string txt, [CallerMemberName] string file = "", [CallerLineNumber] int line = 0)
        => WriteLog(file, line, LogLevel.Info, txt);
    public static void W(string txt, [CallerMemberName] string file = "", [CallerLineNumber] int line = 0)
        => WriteLog(file, line, LogLevel.Warn, txt);
    public static void E(string txt, [CallerMemberName] string file = "", [CallerLineNumber] int line = 0)
        => WriteLog(file, line, LogLevel.Error, txt);
    public static void F(string txt, [CallerMemberName] string file = "", [CallerLineNumber] int line = 0)
        => WriteLog(file, line, LogLevel.Fatal, txt);
}

[PublicAPI]
public enum LogLevel {
    Debug = 0,
    Info = 1,
    Warn = 2,
    Error = 3,
    Fatal = 4,
}
