using Godot;
using System;

public partial class RestartScript : Node2D
{
    const string RESTART_KEY = "restart_button";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (@event.IsActionReleased(RESTART_KEY))
        {
            GD.Print("Reset requested.");
        }
    }
}
