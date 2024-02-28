using Godot;
using Players;

namespace Physics;

public partial class GravityApplicator : Node, IDisableableControl
{
    [Export] CharacterBody2D _body;
    [Export] public float Gravity { get; private set; } = 2000f;
    [Export] bool _isEnabled = true;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.GRAVITY; }

    public void SetControlState(bool enabled)
    {
        SetIfEnabled(enabled);
    }
    #endregion

    public override void _Ready()
    {
        base._Ready();
        _body ??= GetParent() as CharacterBody2D;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!_isEnabled) return;
        _body.Velocity += new Vector2(0f, Gravity * (float)delta);
    }

    public void SetIfEnabled(bool enabled) => _isEnabled = enabled;
}
