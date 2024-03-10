using Godot;
using System;
using static Godot.TextServer;

namespace Players;

public partial class DashHandler : Node
{
    [Export] PlayerController _body;
    [Export] KnockbackHandler _knockback;
    [Export] FlipHandler _flip;
    [Export] float _dashSpeed = 250;
    public int DashCharges { get; set; } = 1;

    bool _leftPressed;
    bool _rightPressed;
    bool _upPressed;
    bool _downPressed;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_body.IsOnFloor())
        {
            DashCharges = 1;
        }
    }

    public void InputEventHandler(StringName input, bool pressed)
    {
        if (input == InputNames.LEFT) _leftPressed = pressed;
        if (input == InputNames.RIGHT) _rightPressed = pressed;
        if (input == InputNames.JUMP) _upPressed = pressed;
        if (input == InputNames.CROUCH) _downPressed = pressed;
    }

    /// <summary>
    /// Returns true if the dash was able to be performed.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool PerformDash(Vector2 direction)
    {
        if (DashCharges <= 0) return false;
        DashCharges--;

        direction = GetDashDirection();

        _body.Velocity = direction.Normalized() * _dashSpeed;
        return true;
    }

    Vector2 GetDashDirection()
    {
        Vector2 direction = new();
        if (_leftPressed) direction.X -= 1;
        if (_rightPressed) direction.X += 1;
        if (_upPressed) direction.Y -= 1;
        if (_downPressed) direction.Y += 1;

        if (direction.LengthSquared() == 0f) 
            direction = ((_flip.FacingLeft) ? Vector2.Left : Vector2.Right);
        return direction;
    }

    public void OnHitLandedHandler(Node2D thingHit)
    {
        DashCharges++;
    }
}
