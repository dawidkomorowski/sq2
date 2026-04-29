using System;
using System.IO;

namespace SQ2.Core;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GameStateService
{
    public int SelectedLevel { get; set; } = 1;

    public string GetMapFile()
    {
        return SelectedLevel switch
        {
            1 => GetMapPath("level_01.tmx"),
            2 => GetMapPath("level_02.tmx"),
            _ => throw new InvalidOperationException($"Invalid level: {SelectedLevel}")
        };
    }

    private static string GetMapPath(string mapFileName)
    {
        return Path.Combine("Assets", "Maps", mapFileName);
    }
}