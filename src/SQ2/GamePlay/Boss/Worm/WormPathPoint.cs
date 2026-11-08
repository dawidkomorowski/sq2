using Geisha.Engine.Core.Math;

namespace SQ2.GamePlay.Boss.Worm;

internal readonly record struct WormPathPoint
{
    public Vector2 Position { get; init; }
    public double Speed { get; init; }
}