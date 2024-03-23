using Godot;
using System;
using WorldGeneration;

namespace Players;

public partial class BallHorizontalMovementHandler : Node, IDisableableControl
{
    [Export] PlayerController _body;
    [Export] float _moveSpeedInTiles = 10;
    [Export] bool _isEnabled = true;
    [Export] float _groundAccelerationInTiles = 150;
    [Export] float _airAccelerationInTiles = 87.5f;
    [Export] float _airExcessSpeedDeceleration = 0.4f;
    [Export] float _groundExcessSpeedDeceleration = 3f;
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="curVelX">In Pixels/S</param>
    /// <param name="moveSpeed">In Pixels/S</param>
    /// <returns></returns>
    float GetNewVelX(float delta, float curVelX, float moveSpeed)
    {
        float deltaV = _body.IsOnFloor() ? 
            _groundAccelerationInTiles.ToPixels() : _airAccelerationInTiles.ToPixels();
        float excessSpeedDeceleration = _body.IsOnFloor() ? 
            _groundExcessSpeedDeceleration : _airExcessSpeedDeceleration;
        deltaV *= delta;

        if (_isLeftButtonDown == _isRightButtonDown)
        {
            if (deltaV > Mathf.Abs(curVelX))
            {
                curVelX = 0f;
            }
            else
            {
                curVelX += curVelX < 0f ? deltaV : -deltaV;
            }
        }
        else if (_isLeftButtonDown)
        {
            // If not already going past max movement speed.
            if (!(curVelX < -moveSpeed))
            {
                curVelX = Mathf.Clamp(curVelX - deltaV,
                    -moveSpeed, moveSpeed);
            }
            else
            {
                // Apply deceleration.
                curVelX -= curVelX * excessSpeedDeceleration * delta;
                curVelX = Mathf.Clamp(curVelX, float.NegativeInfinity, -moveSpeed);
            }
        }
        else if (_isRightButtonDown)
        {
            // If not already going past max movement speed.
            if (!(curVelX > moveSpeed))
            {
                curVelX = Mathf.Clamp(curVelX + deltaV,
                    -moveSpeed, moveSpeed);
            }
            else
            {
                // Apply deceleration.
                curVelX -= curVelX * excessSpeedDeceleration * delta;
                curVelX = Mathf.Clamp(curVelX, moveSpeed, float.PositiveInfinity);
            }
        }

        return curVelX;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!_isEnabled) return;
        Vector2 newVel = _body.Velocity;
        newVel.X = GetNewVelX((float)delta, newVel.X, _moveSpeedInTiles.ToPixels());
        //float deltaX = _body.IsOnFloor() ?
        //    _groundAcceleration : _airAcceleration;
        //float excessSpeedDeceleration = _body.IsOnFloor() ?
        //    _groundExcessSpeedDeceleration : _airExcessSpeedDeceleration;
        //deltaX *= (float)delta;

        //if (_isLeftButtonDown == _isRightButtonDown)
        //{
        //    if (deltaX > Mathf.Abs(_body.Velocity.X))
        //    {
        //        newVel.X = 0f;
        //    }
        //    else
        //    {
        //        newVel.X += _body.Velocity.X < 0f ? deltaX : -deltaX;
        //    }
        //}
        //else if (_isLeftButtonDown)
        //{
        //    // If not already going past max movement speed.
        //    if (!(newVel.X < -_moveSpeed))
        //    {
        //        newVel.X = Mathf.Clamp(newVel.X - deltaX,
        //            -_moveSpeed, _moveSpeed);
        //    }
        //    else
        //    {
        //        // Apply deceleration.
        //        newVel.X -= newVel.X * excessSpeedDeceleration * (float)delta;
        //        newVel.X = Mathf.Clamp(newVel.X, float.NegativeInfinity, -_moveSpeed);
        //    }
        //}
        //else if (_isRightButtonDown)
        //{
        //    // If not already going past max movement speed.
        //    if (!(newVel.X > _moveSpeed))
        //    {
        //        newVel.X = Mathf.Clamp(newVel.X + deltaX,
        //            -_moveSpeed, _moveSpeed);
        //    }
        //    else
        //    {
        //        // Apply deceleration.
        //        newVel.X -= newVel.X * excessSpeedDeceleration * (float)delta;
        //        newVel.X = Mathf.Clamp(newVel.X, _moveSpeed, float.PositiveInfinity);
        //    }
        //}

        _body.Velocity = newVel;
    }
}
