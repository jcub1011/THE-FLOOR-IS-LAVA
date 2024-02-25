using Godot;
using System;

namespace Players;

public partial class FlipHandler : Node, IDisableableControl
{
    [Signal] public delegate void OnFlipEventHandler(bool flipped);
    public string ControlID { get => ControlIDs.FLIPPER; }
    bool _isEnabled = true;
    bool? _cachedFlip;

    public void SetControlState(bool enabled)
    {
        _isEnabled = enabled;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_cachedFlip != null && _isEnabled)
        {
            EmitSignal(SignalName.OnFlip, _cachedFlip.Value);
            _cachedFlip = null;
        }
    }

    public void OnFaceLeft()
    {
        _cachedFlip = true;
    }

    public void OnFaceRight()
    {
        _cachedFlip = false;
    }
}
