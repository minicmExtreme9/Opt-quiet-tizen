using System;
using System.Collections.Generic;
using System.IO;

public struct LogEntry
{
    public DateTime Timestamp { get; }
    public string Level { get; }
    public string Category { get; }
    public string Message { get; }
    public string Details { get; }

    public LogEntry(string level, string category, string message, string details = null)
    {
        Timestamp = DateTime.UtcNow;
        Level = level;
        Category = category;
        Message = message;
        Details = details;
    }

    public override string ToString()
    {
        string ts = Timestamp.ToString("HH:mm:ss.fff");
        if (string.IsNullOrEmpty(Details))
            return $"[{ts}] [{Level}] ({Category}) {Message}";
        return $"[{ts}] [{Level}] ({Category}) {Message} -> {Details}";
    }
}

/// <summary>
/// In-memory ring buffer for diagnostics and error tracking (~50 entries).
/// Replaces continuous disk writes on every interaction with zero-I/O memory logging.
/// </summary>
public static class DebugLog
{
    public const int Capacity = 50;
    private static readonly LogEntry[] _entries = new LogEntry[Capacity];
    private static int _head = 0;
    private static int _count = 0;
    private static readonly object _lock = new object();

    public static int ErrorCount { get; private set; } = 0;
    public static int WarnCount { get; private set; } = 0;
    public static string LastState { get; private set; } = "";

    public static event Action<string> OnErrorLogged;

    public static void Log(string level, string category, string message, Exception ex = null)
    {
        string details = null;
        if (ex != null)
        {
            details = $"{ex.GetType().Name}: {ex.Message}";
        }

        lock (_lock)
        {
            if (level == "ERROR") ErrorCount++;
            else if (level == "WARN") WarnCount++;

            _entries[_head] = new LogEntry(level, category, message, details);
            _head = (_head + 1) % Capacity;
            if (_count < Capacity) _count++;
        }

        if (level == "ERROR")
        {
            try { OnErrorLogged?.Invoke(message); } catch {}
        }
    }

    public static void Info(string category, string message) => Log("INFO", category, message);
    public static void Warn(string category, string message, Exception ex = null) => Log("WARN", category, message, ex);
    public static void Error(string category, string message, Exception ex = null) => Log("ERROR", category, message, ex);

    public static void RecordState(string activity, string summary)
    {
        LastState = $"{activity} | {summary}";
        Log("STATE", "State", $"{activity}: {summary}");
    }

    public static List<string> GetAll()
    {
        var list = new List<string>();
        lock (_lock)
        {
            int start = (_count == Capacity) ? _head : 0;
            for (int i = 0; i < _count; i++)
            {
                int idx = (start + i) % Capacity;
                list.Add(_entries[idx].ToString());
            }
        }
        return list;
    }

    public static string Dump()
    {
        var all = GetAll();
        return string.Join("\n", all);
    }

    public static bool DumpToFile(string path = "/tmp/quiet-home-debuglog.txt")
    {
        try
        {
            File.WriteAllText(path, $"--- QUIET HOME DEBUG LOG ({DateTime.UtcNow:o}) ---\n" +
                                   $"Total Errors: {ErrorCount}, Total Warnings: {WarnCount}\n" +
                                   $"Last Recorded State: {LastState}\n\n" +
                                   Dump() + "\n");
            return true;
        }
        catch
        {
            return false;
        }
    }
}
