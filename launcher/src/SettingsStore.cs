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

    public static LauncherSettings Load(string primaryPath, string fallbackPath = null)
    {
        string path = File.Exists(primaryPath) ? primaryPath : (File.Exists(fallbackPath) ? fallbackPath : null);
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
            catch
            {
                // Fall through to legacy migrator
            }

            // Legacy config detected: migrate and persist in new format
            var migrated = MigrateLegacyJson(raw);
            Save(migrated, primaryPath, fallbackPath);
            return migrated;
        }
        catch
        {
            return new LauncherSettings();
        }
    }

    public static void Save(LauncherSettings settings, string primaryPath, string fallbackPath = null)
    {
        if (settings == null) return;
        try
        {
            string json = JsonSerializer.Serialize(settings, JsonOptions);
            if (!string.IsNullOrEmpty(primaryPath))
            {
                try { File.WriteAllText(primaryPath, json); } catch {}
            }
            if (!string.IsNullOrEmpty(fallbackPath))
            {
                try { File.WriteAllText(fallbackPath, json); } catch {}
            }
        }
        catch {}
    }

    public static LauncherSettings MigrateLegacyJson(string json)
    {
        var s = new LauncherSettings();
        if (string.IsNullOrEmpty(json)) return s;

        try
        {
            string lang = ExtractJsonValue(json, "lang");
            if (!string.IsNullOrEmpty(lang)) s.Lang = lang;

            string snd = ExtractJsonValue(json, "sound");
            if (!string.IsNullOrEmpty(snd)) s.Sound = snd == "true" || snd == "1";

            string anim = ExtractJsonValue(json, "animations");
            if (!string.IsNullOrEmpty(anim)) s.Animations = anim == "true" || anim == "1";

            string p0 = ExtractJsonValue(json, "preset0");
            if (int.TryParse(p0, out var v0)) s.Preset0 = v0;

            string p1 = ExtractJsonValue(json, "preset1");
            if (int.TryParse(p1, out var v1)) s.Preset1 = v1;

            string p2 = ExtractJsonValue(json, "preset2");
            if (int.TryParse(p2, out var v2)) s.Preset2 = v2;

            string h1n = ExtractJsonValue(json, "hdmi1_name");
            if (!string.IsNullOrEmpty(h1n)) s.Hdmi1Name = h1n;

            string h1d = ExtractJsonValue(json, "hdmi1_desc");
            if (!string.IsNullOrEmpty(h1d)) s.Hdmi1Desc = h1d;

            string h2n = ExtractJsonValue(json, "hdmi2_name");
            if (!string.IsNullOrEmpty(h2n)) s.Hdmi2Name = h2n;

            string h2d = ExtractJsonValue(json, "hdmi2_desc");
            if (!string.IsNullOrEmpty(h2d)) s.Hdmi2Desc = h2d;

            string h3n = ExtractJsonValue(json, "hdmi3_name");
            if (!string.IsNullOrEmpty(h3n)) s.Hdmi3Name = h3n;

            string h3d = ExtractJsonValue(json, "hdmi3_desc");
            if (!string.IsNullOrEmpty(h3d)) s.Hdmi3Desc = h3d;
        }
        catch {}

        return s;
    }

    public static string ExtractJsonValue(string json, string key)
    {
        int k = json.IndexOf("\"" + key + "\"");
        if (k < 0) return "";
        int colon = json.IndexOf(':', k);
        if (colon < 0) return "";
        int start = colon + 1;
        while (start < json.Length && (json[start] == ' ' || json[start] == '\t' || json[start] == '\r' || json[start] == '\n')) start++;
        if (start >= json.Length) return "";
        if (json[start] == '"') {
            int end = json.IndexOf('"', start + 1);
            return end > start ? json.Substring(start + 1, end - start - 1) : "";
        } else {
            int end = start;
            while (end < json.Length && json[end] != ',' && json[end] != '}' && json[end] != '\n') end++;
            return json.Substring(start, end - start).Trim();
        }
    }

    public static string EscapeJson(string s) => (s ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
}
