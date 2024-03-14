using Godot;
using System;

namespace Players;

public partial class BallHorizontalMovementHandler : Node, IDisableableControl
{
    [Export] PlayerController _body;
    [Export] float _moveSpeed = 80;
    [Export] bool _isEnabled = true;
    [Export] float _groundAcceleration = 1200f;
    [Export] float _airAcceleration = 900f;
    bool _isLeftButtonDown;
    bool _isRightButtonDown;

    public string ControlID => ControlIDs.MOVEMENT;

    public void SetControlState(bool enabled)
    {
        _isEnabled = enabled;
    }

    public void InputEventHandler(StringName input, bool pressed)
    {
        if (input == InputNames.LEFT) _isLeftButtonDown = pressed;
        if (input == InputNames.RIGHT) _isRightButtonDown = pressed;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!_isEnabled) return;
        Vector2 newVel = _body.Velocity;
        float speedLim = _body.SpeedLimit.X;

        float deltaX = _body.IsOnFloor() ?
            _groundAcceleration : _airAcceleration;
        deltaX *= (float)delta;

        if (_isLeftButtonDown == _isRightButtonDown)
        {
            if (deltaX > Mathf.Abs(_body.Velocity.X))
            {
                newVel.X = 0f;
            }
            else
            {
                newVel.X += _body.Velocity.X < 0f ? deltaX : -deltaX;
            }
        }
        else if (_isLeftButtonDown)
        {
            if (newVel.X > -_moveSpeed)
            {
                newVel.X = Mathf.Clamp(newVel.X - deltaX,
                    -_moveSpeed, _moveSpeed);
            }
            else
            {
                newVel.X = Mathf.Clamp(newVel.X + deltaX,
                    float.NegativeInfinity, _moveSpeed);
            }
        }
        else if (_isRightButtonDown)
        {
            if (newVel.X < _moveSpeed)
            {
                newVel.X = Mathf.Clamp(newVel.X + deltaX,
                    -_moveSpeed, _moveSpeed);
            }
            else
            {
                newVel.X = Mathf.Clamp(newVel.X - deltaX,
                    -_moveSpeed, float.PositiveInfinity);
            }
        }

        newVel.X = Mathf.Clamp(newVel.X, -speedLim, speedLim);
        _body.Velocity = newVel;
    }
}
