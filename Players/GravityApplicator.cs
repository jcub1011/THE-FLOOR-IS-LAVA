using Godot;
using Players;
using WorldGeneration;

namespace Physics;

public partial class GravityApplicator : Node, IDisableableControl
{
    CharacterBody2D _body;
    [Export] public float GravityInTiles { get; private set; } = 93.75f;
    float _gravity = 12000f;
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
        _gravity = GravityInTiles.ToPixels();
        _body  = GetParent<CharacterBody2D>();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!_isEnabled) return;
        _body.Velocity += new Vector2(0f, _gravity * (float)delta);
    }

    public void SetIfEnabled(bool enabled) => _isEnabled = enabled;
}
