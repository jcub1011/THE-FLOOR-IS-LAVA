using Godot;
using System;
using TheFloorIsLava.Subscriptions;

public partial class Projectile : CharacterBody2D
{
	public float RemainingLifeTime { get; set; }

    public override void _Ready()
    {
        base._Ready();
        OriginShiftChannel.OriginShifted += OnOriginShift;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        OriginShiftChannel.OriginShifted -= OnOriginShift;
    }

    public override void _Process(double delta)
	{
        MoveAndSlide();
	}

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        RemainingLifeTime -= (float)delta;
        if (RemainingLifeTime <= 0f) QueueFree();
    }

    void OnOriginShift(Vector2 shift)
    {
        Position += shift;
    }
}
