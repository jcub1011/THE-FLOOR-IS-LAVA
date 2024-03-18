using Godot;
using Godot.NodeExtensions;

namespace Players;

public partial class CrouchHandler : Node, IDisableableControl
{
    PlayerController _playerController
    {
        get => GetParent().GetChild<PlayerController>();
    }

    public bool IsCrouched { get; private set; }

    public string ControlID => ControlIDs.CROUCHER;

    bool? _cachedChange;
    bool _enabled = true;

    public void OnCrouchStart()
    {
        _cachedChange = true;
    }

    public void OnCrouchEnd()
    {
        _cachedChange = false;
    }

    public override void _Process(double delta)
    {
        base._PhysicsProcess(delta);
        if (!_enabled) return;

        if (_cachedChange != null)
        {
            IsCrouched = _cachedChange.Value;
            _cachedChange = null;
        }
    }

    public void SetControlState(bool enabled)
    {
        _enabled = enabled;
    }
}
