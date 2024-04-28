using Godot;
using Players;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WorldGeneration.Sections;

namespace WorldGeneration.Paths;

public partial class EnterPath : Area2D
{
    public event Action<LockKeySection> OnSectionReached;
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
    bool _closed = false;

    public override void _Ready()
    {
        base._Ready();
        _playersInArea = new();
        BodyEntered += OnBodyEnter;
        BodyExited += OnBodyExit;
    }

    void OnBodyEnter(Node body)
    {
        GD.Print($"{body.Name} entered the enter path.");
        if (body is PlayerController controller)
        {
            _playersInArea.Add(controller);
        }
    }

    void OnBodyExit(Node body)
    {
        GD.Print($"{body.Name} exited the enter path.");
        if (body is PlayerController controller)
        {
            _playersInArea.Remove(controller);
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        //if (!_closed &&
        //    (PlayerUtilityFlags.LivingPlayersMask & ~PlayerUtilityFlags.PlayersToMask(_playersInArea)) == 0)
        //{
        //    GD.Print("All players have reached the next section.");
        //    _closed = true;
        //    OnSectionReached?.Invoke(GetParent<LockKeySection>());
        //}

        //foreach (var player in _playersInArea)
        //{
        //    if (_closed)
        //    {
        //        player.Velocity = PathDirection.ToVector2() * 15f.ToPixels();
        //    }
        //    else player.Velocity = Vector2.Zero;
        //}
    }
}
