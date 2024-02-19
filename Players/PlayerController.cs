using Godot;
using System;

namespace Players;

public partial class PlayerController : CharacterBody2D
{
    [Export] PlayerInputHandler InputHandler;

    public override void _Ready()
    {
        base._Ready();

    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        MoveAndSlide();
    }
}
