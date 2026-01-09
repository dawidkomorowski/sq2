using System;
using Geisha.Engine.Core.Math;

namespace SQ2.Core;

internal enum Direction
{
    Up,
    Down,
    Left,
    Right
}

internal enum Flip
{
    None,
    Horizontal,
    Vertical,
    HorizontalAndVertical
}

internal readonly record struct Orientation(Direction Direction, Flip Flip)
{
    public double GetRotation() => Direction switch
    {
        Direction.Up => 0,
        Direction.Right => -Math.PI / 2,
        Direction.Down => Math.PI,
        Direction.Left => Math.PI / 2,
        _ => throw new ArgumentOutOfRangeException()
    };

    public Vector2 GetScale() => Flip switch
    {
        Flip.None => new Vector2(1, 1),
        Flip.Horizontal => new Vector2(-1, 1),
        Flip.Vertical => new Vector2(1, -1),
        Flip.HorizontalAndVertical => new Vector2(-1, -1),
        _ => throw new ArgumentOutOfRangeException()
    };
}