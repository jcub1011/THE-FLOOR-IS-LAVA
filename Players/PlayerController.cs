using Godot;
using System;

namespace Players;

public partial class PlayerController : CharacterBody2D
{
    [Export] PlayerInputHandler InputHandler;
    [Export] Vector2 _speedLimit = new(1000, 1000);

    public override void _Ready()
    {
        base._Ready();

    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Vector2 limitedVelocity = Velocity;
        limitedVelocity.X = Mathf.Clamp(Velocity.X, -_speedLimit.X, _speedLimit.X);
        limitedVelocity.Y = Mathf.Clamp(Velocity.Y, -_speedLimit.Y, _speedLimit.Y);
        Velocity = limitedVelocity;
        MoveAndSlide();
    }
}
