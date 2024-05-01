using Godot;
using System;
using WorldGeneration;

namespace Players;

internal class CoyoteTimer
{
    float _coyoteTime;
    float _remainingCoyoteTime;
    bool _isCurrentlyOnGround;

    public CoyoteTimer(float coyoteTime)
    {
        _coyoteTime = coyoteTime;
    }

    public void UpdateTimer(float deltaTime, bool isOnGround)
    {
        _remainingCoyoteTime -= deltaTime;
        _isCurrentlyOnGround = isOnGround;
        if (_isCurrentlyOnGround)
        {
            _remainingCoyoteTime = _coyoteTime;
        }
    }

    public bool IsOnGround()
    {
        return _isCurrentlyOnGround || _remainingCoyoteTime > 0f;
    }

    public void ConsumeCoyoteTime()
    {
        _remainingCoyoteTime = 0f;
    }
}

public partial class BallJumpHandler : Node, IDisableableControl
{
    CharacterBody2D _body;
    [Export] float _jumpVelocityInTiles = 12f;
    [Export] float _maxJumpTime = 0.2f;
    [Export] bool _isEnabled = true;
    [Export] float _coyoteTime = 0.05f;
    [Export] float _jumpBuffer = 0.1f;
    [Export] float _excessJumpSpeedDecell = 200f;
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
        _body = GetParent<CharacterBody2D>();
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
                _body.Velocity = new(_body.Velocity.X, -_jumpVelocityInTiles.ToPixels());
            }
        }
        else if (InputBuffer.IsBuffered(_body, InputNames.JUMP, _jumpBuffer)
            && _coyoteTimer.IsOnGround())
        {
            _coyoteTimer.ConsumeCoyoteTime();
            InputBuffer.ConsumeBuffer(_body, InputNames.JUMP);
            _body.Velocity = new(_body.Velocity.X, -_jumpVelocityInTiles.ToPixels());
        }

        //if (_body.Velocity.Y < -_jumpVelocityInTiles.ToPixels())
        //{
        //    float yVel = _body.Velocity.Y;
        //    yVel += _excessJumpSpeedDecell.ToPixels() * (float)delta;
        //    yVel = Mathf.Clamp(yVel, float.NegativeInfinity, -_jumpVelocityInTiles.ToPixels());
        //    _body.Velocity = new(_body.Velocity.X, yVel);
        //}
    }
}
