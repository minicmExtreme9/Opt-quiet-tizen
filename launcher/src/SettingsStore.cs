using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class LauncherSettings
{
    [JsonPropertyName("lang")]
    public string Lang { get; set; } = "es";

    [JsonPropertyName("sound")]
    public bool Sound { get; set; } = true;

    [JsonPropertyName("animations")]
    public bool Animations { get; set; } = true;

    [JsonPropertyName("dark_mode")]
    public bool DarkMode { get; set; } = true;

    [JsonPropertyName("preset0")]
    public int Preset0 { get; set; } = 0;

    [JsonPropertyName("preset1")]
    public int Preset1 { get; set; } = 2;

    [JsonPropertyName("preset2")]
    public int Preset2 { get; set; } = 8;

    [JsonPropertyName("hdmi1_name")]
    public string Hdmi1Name { get; set; }

    [JsonPropertyName("hdmi1_desc")]
    public string Hdmi1Desc { get; set; }

    [JsonPropertyName("hdmi2_name")]
    public string Hdmi2Name { get; set; }

    [JsonPropertyName("hdmi2_desc")]
    public string Hdmi2Desc { get; set; }

    [JsonPropertyName("hdmi3_name")]
    public string Hdmi3Name { get; set; }

    [JsonPropertyName("hdmi3_desc")]
    public string Hdmi3Desc { get; set; }
}

public static class SettingsStore
{
    static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public static LauncherSettings Load(string primaryPath, string fallbackTemplatePath = null)
    {
        AtomicFileWriter.CleanOrphanTmp(primaryPath);
        string path = File.Exists(primaryPath) ? primaryPath : 
                      (!string.IsNullOrEmpty(fallbackTemplatePath) && File.Exists(fallbackTemplatePath) ? fallbackTemplatePath : null);
        if (string.IsNullOrEmpty(path))
            return new LauncherSettings();

        try
        {
            string raw = File.ReadAllText(path).Trim();
            if (string.IsNullOrEmpty(raw))
                return new LauncherSettings();

            // Try standard typed deserialization first
            try
            {
                var settings = JsonSerializer.Deserialize<LauncherSettings>(raw, JsonOptions);
                if (settings != null)
                    return settings;
            }
            catch (Exception ex)
            {
                DebugLog.Warn("SettingsStore", $"Typed deserialization failed on {path}, attempting legacy migration", ex);
            }

            // Legacy config detected: migrate and persist in new format
            var migrated = MigrateLegacyJson(raw);
            Save(migrated, primaryPath);
            return migrated;
        }
        catch (Exception ex)
        {
            DebugLog.Error("SettingsStore", $"Failed reading settings from {path}", ex);
            return new LauncherSettings();
        }
    }

    public static void Save(LauncherSettings settings, string primaryPath)
    {
        if (settings == null || string.IsNullOrEmpty(primaryPath)) return;
        try
        {
            string json = JsonSerializer.Serialize(settings, JsonOptions);
            AtomicFileWriter.WriteAllTextSafe(primaryPath, json);
        }
        catch (Exception ex)
        {
            DebugLog.Error("SettingsStore", $"Failed saving settings to {primaryPath}", ex);
        }
    }

    public static LauncherSettings MigrateLegacyJson(string json)
    {
        var s = new LauncherSettings();
        if (string.IsNullOrEmpty(json)) return s;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return s;

            if (root.TryGetProperty("lang", out var el) && el.ValueKind == JsonValueKind.String)
                s.Lang = el.GetString();

            if (root.TryGetProperty("sound", out el))
                s.Sound = el.ValueKind == JsonValueKind.True || (el.ValueKind == JsonValueKind.Number && el.GetInt32() != 0);

            if (root.TryGetProperty("animations", out el))
                s.Animations = el.ValueKind == JsonValueKind.True || (el.ValueKind == JsonValueKind.Number && el.GetInt32() != 0);

            if (root.TryGetProperty("dark_mode", out el))
                s.DarkMode = el.ValueKind == JsonValueKind.True || (el.ValueKind == JsonValueKind.Number && el.GetInt32() != 0);

            if (root.TryGetProperty("preset0", out el) && el.TryGetInt32(out var p0)) s.Preset0 = p0;
            if (root.TryGetProperty("preset1", out el) && el.TryGetInt32(out var p1)) s.Preset1 = p1;
            if (root.TryGetProperty("preset2", out el) && el.TryGetInt32(out var p2)) s.Preset2 = p2;

            if (root.TryGetProperty("hdmi1_name", out el) && el.ValueKind == JsonValueKind.String) s.Hdmi1Name = el.GetString();
            if (root.TryGetProperty("hdmi1_desc", out el) && el.ValueKind == JsonValueKind.String) s.Hdmi1Desc = el.GetString();
            if (root.TryGetProperty("hdmi2_name", out el) && el.ValueKind == JsonValueKind.String) s.Hdmi2Name = el.GetString();
            if (root.TryGetProperty("hdmi2_desc", out el) && el.ValueKind == JsonValueKind.String) s.Hdmi2Desc = el.GetString();
            if (root.TryGetProperty("hdmi3_name", out el) && el.ValueKind == JsonValueKind.String) s.Hdmi3Name = el.GetString();
            if (root.TryGetProperty("hdmi3_desc", out el) && el.ValueKind == JsonValueKind.String) s.Hdmi3Desc = el.GetString();
        }
        catch (Exception ex)
        {
            DebugLog.Warn("SettingsStore", "MigrateLegacyJson failed", ex);
        }

        return s;
    }
}
