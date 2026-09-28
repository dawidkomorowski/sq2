using System;
using Geisha.Engine.Windows;
using SQ2.Development;

namespace SQ2;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        DevConfig.InitializeFromArgs(args);

        var options = new WindowsApplicationOptions
        {
            DirectX = new DirectXOptions
            {
                ResizeBuffersAfterVSyncChange = true
            }
        };

        WindowsApplication.Run(new SQ2Game(), options);
    }
}