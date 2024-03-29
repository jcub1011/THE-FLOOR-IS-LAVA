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
    [Signal] public delegate void OnPlayerEnteredEventHandler(PlayerController player);
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
        BodyEntered += OnObjectEntered;
    }

    void OnObjectEntered(Node2D node)
    {
        if (node is PlayerController player) EmitSignal(SignalName.OnPlayerEntered, player);
    }
}
