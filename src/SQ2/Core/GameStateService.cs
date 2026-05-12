using System;
using System.IO;

namespace SQ2.Core;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GameStateService
{
    private readonly GameSaveService _gameSaveService;
    private int _selectedLevel;

    public GameStateService(GameSaveService gameSaveService)
    {
        _gameSaveService = gameSaveService;
    }

    public bool IsContinueAvailable => _gameSaveService.GameSave.NewGameStarted;

    public void InitializeGameSave()
    {
        _gameSaveService.LoadGame();
        _gameSaveService.SaveGame();
    }

    public void StartNewGame()
    {
        _gameSaveService.GameSave.NewGameStarted = true;
        _gameSaveService.GameSave.CurrentLevel = 0;
        _gameSaveService.SaveGame();
        _selectedLevel = _gameSaveService.GameSave.CurrentLevel;
    }

    public void ContinueGame()
    {
        _selectedLevel = _gameSaveService.GameSave.CurrentLevel;
    }

    public void SelectLevel(int level)
    {
        _selectedLevel = level;
    }

    public void CompleteLevel()
    {
        var maxLevelIndex = LevelInfo.Levels.Length - 1;
        _selectedLevel = Math.Min(_selectedLevel + 1, maxLevelIndex);
        _gameSaveService.GameSave.CurrentLevel = _selectedLevel;
        _gameSaveService.SaveGame();
    }

    public bool IsDiamondCollected(string id)
    {
        return _gameSaveService.GameSave.CollectedDiamondIds.Contains(id);
    }

    public void CollectDiamond(string id)
    {
        _gameSaveService.GameSave.CollectedDiamondIds.Add(id);
        _gameSaveService.SaveGame();
    }

    public string GetMapFile()
    {
        return GetMapPath(LevelInfo.Levels[_selectedLevel].MapFileName);
    }

    private static string GetMapPath(string mapFileName)
    {
        return Path.Combine("Assets", "Maps", mapFileName);
    }
}