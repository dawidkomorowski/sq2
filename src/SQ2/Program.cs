using System;
using Geisha.Engine.Windows;

namespace SQ2;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        WindowsApplication.Run(new SQ2Game());
    }
}