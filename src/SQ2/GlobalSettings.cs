using Geisha.Engine.Core.Math;

namespace SQ2;

internal static class GlobalSettings
{
    public static readonly Size WindowSize = new(1280, 720);
    public static readonly Vector2 ViewSize = new(1280 / 3d, 720 / 3d);
    public static readonly SizeD TileSize = new(18, 18);

    public static class SortingLayers
    {
        public const string Background = "Background";
        public const string DecorBackground = "DecorBackground";
        public const string DecorForeground = "DecorForeground";
        public const string CameraEffects = "CameraEffects";
        public const string UI = "UI";
    }

    public static class HudElements
    {
        public const string CoinCounter = "UI_CoinCounter";
    }

    public static class SceneNames
    {
        public const string MainMenu = "MainMenu";
        public const string GameWorld = "GameWorld";
    }

    public static class SpecialEntities
    {
        public const string UIRoot = "UI_Root";
    }
}