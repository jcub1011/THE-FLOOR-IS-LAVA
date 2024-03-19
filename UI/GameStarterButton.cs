using Godot;
using Players;
using Godot.NodeExtensions;
using System.Collections.Generic;

namespace WorldGeneration;

public partial class GameStarterButton : Button
{
    [Signal] public delegate void OnPressedWithTimerEventHandler();

    [Export] float _timeBetweenClicks = 1f;
    float _remainingTimeUntilNextClick;

    void OnClick()
    {
        if (_remainingTimeUntilNextClick > 0f) return;

        var inputs = GetParent().GetChildren<PlayerControllerSelector>();
        if (inputs.Count != 0)
        {
            List<InputDevice> devices = new();
            foreach (var selector in inputs)
            {
                if (selector.SelectedDevice.Type != DeviceType.None)
                {
                    devices.Add(selector.SelectedDevice);
                }
            }

            PlayerInputHandler.SetDevicesToUse(devices);
        }

        EmitSignal(SignalName.OnPressedWithTimer);
        _remainingTimeUntilNextClick = _timeBetweenClicks;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _remainingTimeUntilNextClick -= (float)delta;
    }
}
