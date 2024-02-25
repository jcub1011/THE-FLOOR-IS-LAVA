using Godot;
using System;

namespace Players;

public partial class JumpHandler : Node, IDisableableControl
{
    [Export] CharacterBody2D _body;
    [Export] float _jumpVelocity;
    [Export] float _maxJumpTime;
    [Export] bool _isEnabled = true;
    bool _isJumpPressed;
    float _remainingJumpTime;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.MOVEMENT; }

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

    public void OnJumpPressed() => _isJumpPressed = true;
    public void OnJumpReleased() => _isJumpPressed = false;

    public override void _Process(double delta)
    {
        base._Process(delta);
        _remainingJumpTime -= (float)delta;
        if (!_isEnabled) return;
        if (_isJumpPressed)
        {
            if (_body.IsOnFloor())
            {
                _remainingJumpTime = _maxJumpTime;
            }
            if (_remainingJumpTime > 0f)
            {
                _body.Velocity = new(_body.Velocity.X, -_jumpVelocity);
            }
        }
    }

    public void SetIfEnabled(bool enabled)
    {
        _isEnabled = enabled;
    }
}
