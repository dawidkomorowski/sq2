using System.IO;
using System.Text.Json;
using Geisha.Engine.Core.Math;

namespace SQ2.Development;

internal static class DevConfig
{
    private const string ConfigPath = "dev-config.json";
    private static DevConfigArgs? _devConfigArgs;
    public static Size? WindowSize => ReadConfig()?.WindowSize;
    public static string? MapFile => _devConfigArgs?.MapFile ?? ReadConfig()?.MapFile;

    public static class DebugDraw
    {
        public static bool YellowEnemy => ReadConfig()?.DebugDraw?.YellowEnemy ?? false;
        public static bool Ladders => ReadConfig()?.DebugDraw?.Ladders ?? false;
        public static bool MovingPlatforms => ReadConfig()?.DebugDraw?.MovingPlatforms ?? false;
    }

    public static void InitializeFromArgs(string[] args)
    {
        var devConfigArgs = new DevConfigArgs();
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--map-file" && i + 1 < args.Length)
            {
                devConfigArgs = devConfigArgs with { MapFile = args[i + 1] };
                i++; // Skip next argument as it's the value for --map-file
            }
        }

        _devConfigArgs = devConfigArgs;
    }

    private sealed record DevConfigArgs
    {
        public string? MapFile { get; init; }
    }

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
        public DebugDrawFileSection? DebugDraw { get; init; }
    }

    private sealed record DebugDrawFileSection
    {
        public bool? YellowEnemy { get; init; }
        public bool? Ladders { get; init; }
        public bool? MovingPlatforms { get; init; }
    }
}