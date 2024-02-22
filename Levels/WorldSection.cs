using Godot;
using System;

namespace WorldGeneration;

public partial class WorldSection : CharacterBody2D
{
    [Export] Marker2D _sectionUpperBound;
    [Export] Marker2D _sectionLowerBound;
    [Export] public Godot.Collections.Array<StringName> PossibleContinuations;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        MoveAndSlide();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (IsInstanceValid(this) && !IsVisible())
        {
            QueueFree();
        }
    }

    float GetWorldBottomY()
    {
        return GetParent<LevelGenerator>().WorldBottomY;
    }

    public bool IsVisible()
    {
        return _sectionUpperBound.GlobalPosition.Y < GetWorldBottomY();
    }
}
