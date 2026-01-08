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

internal readonly record struct Orientation(Direction Direction, Flip Flip);