using Geisha.Engine.Windowing;
using System;

namespace SQ2.Core;

internal static class Settings
{
    public static void ToggleDisplayMode(IWindowingSystem windowingSystem)
    {
        windowingSystem.DisplayMode = windowingSystem.DisplayMode switch
        {
            DisplayMode.Windowed => DisplayMode.Fullscreen,
            DisplayMode.Fullscreen => DisplayMode.Windowed,
            _ => throw new InvalidOperationException("Invalid display mode.")
        };

        windowingSystem.CursorVisible = windowingSystem.DisplayMode is DisplayMode.Windowed;
    }
}