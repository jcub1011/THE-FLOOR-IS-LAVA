using Godot;
using System;

namespace Weapons;

public partial class Projectile : Area2D
{
    Vector2 _velocity;

    public void SetVelocity(Vector2 velocity)
    {
        _velocity = velocity;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        GlobalPosition += _velocity * (float)delta;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (GlobalPosition.Length() > 1000) QueueFree();
    }

    public override void _Ready()
    {
        base._Ready();
        GD.Print("Projectile created.");
    }
}
