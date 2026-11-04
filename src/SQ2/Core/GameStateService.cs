using System;
using System.Collections.Generic;
using System.IO;

namespace SQ2.Core;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GameStateService
{
    private readonly GameSaveService _gameSaveService;
    private int _selectedLevel;
    private LevelCollectibles _completedLevelCollectibles;

    public GameStateService(GameSaveService gameSaveService)
    {
        _gameSaveService = gameSaveService;
    }

    public bool IsContinueAvailable => _gameSaveService.GameSave.NewGameStarted;

    public int TotalDeaths => _gameSaveService.GameSave.GameStats.TotalDeaths;
    public TimeSpan TotalPlaytime => _gameSaveService.GameSave.GameStats.TotalPlaytime;

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

    public void CompleteLevel(int collectedCoins)
    {
        var maxLevelIndex = LevelInfo.Levels.Length - 1;
        var completedLevel = _selectedLevel;
        _selectedLevel = Math.Min(_selectedLevel + 1, maxLevelIndex);
        _gameSaveService.GameSave.CurrentLevel = _selectedLevel;

        var topCollectedCoins = _gameSaveService.GameSave.CollectedCoins.GetValueOrDefault(completedLevel, 0);
        if (collectedCoins > topCollectedCoins)
        {
            _gameSaveService.GameSave.CollectedCoins[completedLevel] = collectedCoins;
        }

        _gameSaveService.SaveGame();

        _completedLevelCollectibles = new LevelCollectibles
        {
            CollectedCoins = collectedCoins,
            HasDiamond = !string.IsNullOrEmpty(LevelInfo.Levels[completedLevel].DiamondId),
            IsDiamondCollected = IsDiamondCollected(LevelInfo.Levels[completedLevel].DiamondId)
        };
    }

    public int GetCollectedCoins(int level) => _gameSaveService.GameSave.CollectedCoins.GetValueOrDefault(level, 0);

    public bool IsDiamondCollected(string id)
    {
        return _gameSaveService.GameSave.CollectedDiamondIds.Contains(id);
    }

    public void CollectDiamond(string id)
    {
        _gameSaveService.GameSave.CollectedDiamondIds.Add(id);
        _gameSaveService.SaveGame();
    }

    public LevelCollectibles GetCompletedLevelCollectibles() => _completedLevelCollectibles;

    public void RegisterPlayerDeath()
    {
        _gameSaveService.GameSave.GameStats.TotalDeaths++;
        _gameSaveService.SaveGame();
    }

    public void RegisterAdditionalPlaytime(TimeSpan additionalPlaytime)
    {
        _gameSaveService.GameSave.GameStats.TotalPlaytime += additionalPlaytime;
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

internal readonly record struct LevelCollectibles
{
    public int CollectedCoins { get; init; }
    public bool HasDiamond { get; init; }
    public bool IsDiamondCollected { get; init; }
}