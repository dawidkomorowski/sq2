using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace SQ2.Tests;

[Timeout(30_000)]
public class MapLoadingTests
{
    private static readonly string SrcPath = Path.Combine("..", "..", "..", "..");
    private static readonly string GameDirectoryPath = Path.Combine(SrcPath, "SQ2", "bin", "Debug", "net6.0-windows", "win-x64");
    private static readonly string EngineLogPath = Path.Combine(GameDirectoryPath, "GeishaEngine.log");
    private static readonly string ExecutablePath = Path.Combine(GameDirectoryPath, "SQ2.exe");

    private Process? _gameProcess;

    public static string[] GetMapFilePaths()
    {
        var mapsDirPath = Path.Combine(SrcPath, "Assets", "Maps");
        var testMapsDirPath = Path.Combine(SrcPath, "Tiled", "TestMaps");
        var montyPythonTests = Path.Combine(SrcPath, "Tiled", "MontyPythonTests");

        var mapFilePaths = Directory.GetFiles(mapsDirPath, "*.tmx");
        var testMapFilePaths = Directory.GetFiles(testMapsDirPath, "*.tmx");
        var montyPythonTestMapFilePaths = Directory.GetFiles(montyPythonTests, "*.tmx");
        return mapFilePaths.Concat(testMapFilePaths).Concat(montyPythonTestMapFilePaths).Select(Path.GetFullPath).ToArray();
    }

    [TestCaseSource(nameof(GetMapFilePaths))]
    public void LoadMapTest(string mapFilePath)
    {
        _gameProcess = RunGame(mapFilePath);
        WaitForMapLoaded();
        Thread.Sleep(TimeSpan.FromSeconds(1));

        var logContent = GetLogContent();
        Assert.That(logContent, Does.Contain($"Info SQ2.Core.MapLoader Loading map from file: \"{mapFilePath}\""));
        Assert.That(logContent, Does.Not.Contain("Error"));
        Assert.That(logContent, Does.Not.Contain("Fatal"));
    }

    [TearDown]
    public void Teardown()
    {
        if (_gameProcess?.HasExited is false)
        {
            _gameProcess?.CloseMainWindow();
            if (_gameProcess?.WaitForExit(5000) is false)
            {
                _gameProcess?.Kill();
            }
        }

        _gameProcess?.Dispose();
        _gameProcess = null;

        Thread.Sleep(TimeSpan.FromSeconds(1));

        if (File.Exists(EngineLogPath))
        {
            File.Delete(EngineLogPath);
        }
    }

    private static Process RunGame(string mapFilePath)
    {
        File.WriteAllText(EngineLogPath, string.Empty);

        var processStartInfo = new ProcessStartInfo(ExecutablePath, $"--map-file {mapFilePath}")
        {
            WorkingDirectory = GameDirectoryPath
        };

        return Process.Start(processStartInfo) ?? throw new InvalidOperationException("Game failed to start.");
    }

    private static void WaitForMapLoaded()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var logContent = string.Empty;
        while (!logContent.Contains("SQ2.Core.MapLoader Map loading completed."))
        {
            if (cts.IsCancellationRequested)
            {
                Assert.Fail("Map was not loaded by the game.{0}{1}", Environment.NewLine + Environment.NewLine, logContent);
            }

            logContent = GetLogContent();

            Thread.Sleep(TimeSpan.FromMilliseconds(500));
        }
    }

    private static string GetLogContent()
    {
        using var stream = new FileStream(EngineLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}