using Godot;
using System;

namespace Players;

public readonly struct InputNames
{
    public const string JUMP = "jump";
    public const string LEFT = "left";
    public const string RIGHT = "right";
    public const string ACTION = "action";
}

public partial class PlayerInputHandler : Node
{
    int _deviceID;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.Device != _deviceID) return;
    }

    public void SetDeviceID(int deviceID)
    {
        GD.Print($"Setting {Name} device ID to {deviceID}.");
        _deviceID = deviceID;
    }
}
