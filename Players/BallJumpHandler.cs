using Godot;
using System;

namespace Players;

public partial class BallJumpHandler : Node, IDisableableControl
{
    [Export] CharacterBody2D _body;
    [Export] float _jumpVelocity = 2272f;
    [Export] float _maxJumpTime = 0.2f;
    [Export] bool _isEnabled = true;
    [Export] float _coyoteTime = 0.05f;
    [Export] float _jumpBuffer = 0.1f;
    bool _isJumpPressed;
    float _remainingJumpTime;
    CoyoteTimer _coyoteTimer;

    public string ControlID => ControlIDs.MOVEMENT;

    public void SetControlState(bool enabled)
    {
        _isEnabled = enabled;
    }

    public override void _Ready()
    {
        base._Ready();
        _body ??= GetParent() as CharacterBody2D;
        _coyoteTimer = new(_coyoteTime);
    }

    public void InputEventHandler(StringName input, bool pressed)
    {
        if (input == InputNames.JUMP)
        {
            if (pressed)
            {
                InputBuffer.BufferInput(_body, InputNames.JUMP);
            }
            _isJumpPressed = pressed;
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        _coyoteTimer.UpdateTimer((float)delta, _body.IsOnFloor());

        _remainingJumpTime -= (float)delta;
        if (!_isEnabled) return;
        if (_isJumpPressed)
        {
            if (_coyoteTimer.IsOnGround())
            {
                _remainingJumpTime = _maxJumpTime;
                _coyoteTimer.ConsumeCoyoteTime();
            }
            if (_remainingJumpTime > 0f)
            {
                _body.Velocity = new(_body.Velocity.X, -_jumpVelocity);
            }
        }
        else if (InputBuffer.IsBuffered(_body, InputNames.JUMP, _jumpBuffer)
            && _coyoteTimer.IsOnGround())
        {
            _coyoteTimer.ConsumeCoyoteTime();
            InputBuffer.ConsumeBuffer(_body, InputNames.JUMP);
            _body.Velocity = new(_body.Velocity.X, -_jumpVelocity);
        }
    }
}
