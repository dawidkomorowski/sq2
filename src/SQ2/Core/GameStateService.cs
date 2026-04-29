using System;
using System.IO;

namespace SQ2.Core;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GameStateService
{
    private readonly GameSaveService _gameSaveService;
    private int _selectedLevel = 1;

    public GameStateService(GameSaveService gameSaveService)
    {
        _gameSaveService = gameSaveService;
    }

    public bool IsContinueAvailable => _gameSaveService.GameSave.NewGameStarted;

    public void StartNewGame()
    {
        _gameSaveService.GameSave.NewGameStarted = true;
        _gameSaveService.GameSave.CurrentLevel = 1;
        _gameSaveService.SaveGame();
        _selectedLevel = _gameSaveService.GameSave.CurrentLevel;
    }

    public void ContinueGame()
    {
        _selectedLevel = _gameSaveService.GameSave.CurrentLevel;
    }

    public string GetMapFile()
    {
        return _selectedLevel switch
        {
            1 => GetMapPath("level_01.tmx"),
            2 => GetMapPath("level_02.tmx"),
            _ => throw new InvalidOperationException($"Invalid level: {_selectedLevel}")
        };
    }

    private static string GetMapPath(string mapFileName)
    {
        return Path.Combine("Assets", "Maps", mapFileName);
    }
}