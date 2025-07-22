using Geisha.Engine.Core.Math;

namespace SQ2;

internal static class GlobalSettings
{
    public static readonly Size WindowSize = new(1280, 720);
    public static readonly Vector2 ViewSize = new(1280 / 4d, 720 / 4d);
    public static readonly SizeD TileSize = new(18, 18);
}