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
    [Export] float _airExcessSpeedDeceleration = 0.4f;
    [Export] float _groundExcessSpeedDeceleration = 2f;
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
        float excessSpeedDeceleration = _body.IsOnFloor() ?
            _groundExcessSpeedDeceleration : _airExcessSpeedDeceleration;
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
            // If not already going past max movement speed.
            if (!(newVel.X < -_moveSpeed))
            {
                newVel.X = Mathf.Clamp(newVel.X - deltaX,
                    -_moveSpeed, _moveSpeed);
            }
            else
            {
                // Apply deceleration.
                newVel.X -= newVel.X * excessSpeedDeceleration * (float)delta;
                newVel.X = Mathf.Clamp(newVel.X, float.NegativeInfinity, -_moveSpeed);
            }
        }
        else if (_isRightButtonDown)
        {
            // If not already going past max movement speed.
            if (!(newVel.X > _moveSpeed))
            {
                newVel.X = Mathf.Clamp(newVel.X + deltaX,
                    -_moveSpeed, _moveSpeed);
            }
            else
            {
                // Apply deceleration.
                newVel.X -= newVel.X * excessSpeedDeceleration * (float)delta;
                newVel.X = Mathf.Clamp(newVel.X, _moveSpeed, float.PositiveInfinity);
            }
        }

        newVel.X = Mathf.Clamp(newVel.X, -speedLim, speedLim);
        _body.Velocity = newVel;
    }
}
