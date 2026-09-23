using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Geisha.Engine.Windowing;

namespace SQ2.Core;

internal record Settings
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DisplayMode DisplayMode { get; init; } = DisplayMode.Fullscreen;

    [JsonIgnore] public bool CursorVisible => DisplayMode is DisplayMode.Windowed;
}

internal static class SettingsService
{
    private static readonly string SettingsFilePath = Path.Combine("Settings", "game-settings.json");

    public static void ToggleDisplayMode(IWindowingSystem windowingSystem)
    {
        windowingSystem.DisplayMode = windowingSystem.DisplayMode switch
        {
            DisplayMode.Windowed => DisplayMode.Fullscreen,
            DisplayMode.Fullscreen => DisplayMode.Windowed,
            _ => throw new InvalidOperationException("Invalid display mode.")
        };

        windowingSystem.CursorVisible = windowingSystem.DisplayMode is DisplayMode.Windowed;
    }

    public static void SaveSettings(Settings settings)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        var json = JsonSerializer.Serialize(settings, options);
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath) ?? string.Empty);
        File.WriteAllText(SettingsFilePath, json);
    }

    public static Settings LoadSettings()
    {
        if (File.Exists(SettingsFilePath))
        {
            var json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
        }

        return new Settings();
    }
}