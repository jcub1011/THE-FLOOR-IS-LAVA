using Godot;
using Players;
using System;
using System.Collections.Generic;

namespace WorldGeneration.Paths;

public partial class EnterPath : Area2D
{
    [Export] PathDirections PathDirection = PathDirections.Up;
    /// <summary>
    /// Units are tiles.
    /// </summary>
    [Export] int PathWidth = 4;
    /// <summary>
    /// Units are tiles.
    /// </summary>
    [Export] int PathHeight = 1;
    List<PlayerController> _playersInArea;

    public override void _Ready()
    {
        base._Ready();
        _playersInArea = new();
    }

    void OnBodyEnter(Node body)
    {
        GD.Print(body);
        if (body is PlayerController controller)
        {
            _playersInArea.Add(controller);
        }
    }

    void OnBodyExit(Node body)
    {
        if (body is PlayerController controller)
        {
            _playersInArea.Remove(controller);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Vector2 force = PathDirection.ToVector2() * ((float)delta * 10f);
        foreach(var player in _playersInArea)
        {
            player.Velocity = force;
        }
    }
}
