using System;
using Geisha.Engine.Core.Math;

namespace SQ2.Core;

internal static class Geometry
{
    public static Vector2 GetWorldCoordinates(int tx, int ty) => new(tx * GlobalSettings.TileSize.Width, ty * GlobalSettings.TileSize.Height);

    public static (int tx, int ty) GetTileCoordinates(Vector2 position) =>
        ((int)(position.X / GlobalSettings.TileSize.Width), (int)(position.Y / GlobalSettings.TileSize.Height));

    public static AxisAlignedRectangle GetWorldRectangle(int tx1, int ty1, int tx2, int ty2)
    {
        var x1 = tx1 * GlobalSettings.TileSize.Width - GlobalSettings.TileSize.Width * 0.5;
        var y1 = ty1 * GlobalSettings.TileSize.Height + GlobalSettings.TileSize.Height * 0.5;
        var x2 = tx2 * GlobalSettings.TileSize.Width + GlobalSettings.TileSize.Width * 0.5;
        var y2 = ty2 * GlobalSettings.TileSize.Height - GlobalSettings.TileSize.Height * 0.5;

        var left = Math.Min(x1, x2);
        var top = Math.Max(y1, y2);
        var right = Math.Max(x1, x2);
        var bottom = Math.Min(y1, y2);

        Span<Vector2> points = stackalloc Vector2[2];
        points[0] = new Vector2(left, top);
        points[1] = new Vector2(right, bottom);

        return new AxisAlignedRectangle(points);
    }
}