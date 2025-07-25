using System.IO;
using System.Text.Json;
using Geisha.Engine.Core.Math;

namespace SQ2;

internal static class DevConfig
{
    private const string ConfigPath = "dev-config.json";

    public static Size? WindowSize => ReadConfig()?.WindowSize;
    public static string? MapFile => ReadConfig()?.MapFile;

    private static DevConfigFile? ReadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            return null;
        }

        var fileContent = File.ReadAllText(ConfigPath);
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        return JsonSerializer.Deserialize<DevConfigFile>(fileContent, jsonSerializerOptions);
    }

    private sealed record DevConfigFile
    {
        public Size? WindowSize { get; init; }
        public string? MapFile { get; init; }
    }
}