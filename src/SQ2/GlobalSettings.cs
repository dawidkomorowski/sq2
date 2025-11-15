using Geisha.Engine.Core.Math;

namespace SQ2;

internal static class GlobalSettings
{
    public static readonly Size WindowSize = new(1280, 720);
    public static readonly Vector2 ViewSize = new(1280 / 3d, 720 / 3d);
    public static readonly SizeD TileSize = new(18, 18);
    public const string BackgroundSortingLayer = "Background";
}