using Godot;
using System;

namespace WorldGeneration;

public partial class GameStarterButton : Button
{
    [Signal] public delegate void OnStartGameEventHandler();

    [Export] float _timeBetweenClicks = 1f;
    float _remainingTimeUntilNextClick;

    void OnClick()
    {
        if (_remainingTimeUntilNextClick > 0f) return;
        EmitSignal(SignalName.OnStartGame);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _remainingTimeUntilNextClick -= (float)delta;
    }
}
