using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SQ2.Core;

internal sealed class GameSaveData
{
    public bool NewGameStarted { get; set; }
    public HashSet<string> CollectedDiamondIds { get; init; } = new();
}

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GameSaveService
{
    private readonly string _saveFilePath = Path.Combine("Saves", "game-save.json");
    public GameSaveData GameSave { get; private set; } = new();

    public void LoadGame()
    {
        if (File.Exists(_saveFilePath))
        {
            var json = File.ReadAllText(_saveFilePath);
            GameSave = JsonSerializer.Deserialize<GameSaveData>(json) ?? new GameSaveData();
        }
    }

    public void SaveGame()
    {
        var json = JsonSerializer.Serialize(GameSave);
        Directory.CreateDirectory(Path.GetDirectoryName(_saveFilePath) ?? string.Empty);
        File.WriteAllText(_saveFilePath, json);
    }

    public void ClearSave()
    {
        GameSave = new GameSaveData();
        SaveGame();
    }
}