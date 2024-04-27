using Godot;
using Players;
using System;

namespace WorldGeneration.Paths;

public enum PathDirections
{
    Up,
    Left,
    Right
}

public static class PathDirectionsExtensions
{
    public static Vector2 ToVector2(this PathDirections direction)
    {
        return direction switch
        {
            PathDirections.Left => Vector2.Left,
            PathDirections.Right => Vector2.Right,
            PathDirections.Up => Vector2.Up,
            _ => Vector2.Up,
        };
    }
}

public partial class ExitPath : Area2D
{
    [Export] public PathDirections PathDirection = PathDirections.Up;
    /// <summary>
    /// Units are tiles.
    /// </summary>
    [Export] public int PathWidth = 4;
    /// <summary>
    /// Units are tiles.
    /// </summary>
    [Export] public int PathHeight = 1;

    public override void _Ready()
    {
        base._Ready();
    }
}
