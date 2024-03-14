using Godot;
using System;
using System.Linq;
using static Godot.TextServer;

namespace Players;

public partial class DashHandler : Node
{
    [Export] PlayerController _body;
    [Export] KnockbackHandler _knockback;
    [Export] FlipHandler _flip;
    [Export] float _dashSpeed = 250;
    [Export] float _dashAngleAdjustSpeed = 5f;
    [Export] StringName _dashAnimationName;
    [Export] AnimationPlayer _aniPlayer;
    [Export] ControlDisablerHandler _disabler;
    int _dashCharges;
    public int DashCharges
    {
        get => _dashCharges;
        set
        {
            GD.Print($"{GetParent().Name} - " +
                $"Setting dash charges to {value} from {_dashCharges}.");
            _dashCharges = value;
        }
    }

    bool _leftPressed;
    bool _rightPressed;
    bool _upPressed;
    bool _downPressed;

    float _remainingDashTime;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_body.IsOnFloor())
        {
            if (DashCharges <= 0) DashCharges = 1;
        }

        //_remainingDashTime -= (float)delta;
        //if (_remainingDashTime > 0f)
        //{
        //    HandleDashNudging((float)delta);
        //}
    }

    void HandleDashNudging(float delta)
    {
        if (_upPressed == _downPressed) return;
        //Vector2 normal;
        Vector2 moveDir = _body.Velocity;
        //if (moveDir.Y != 0f) return;
        _body.Velocity += new Vector2(0f, (_upPressed 
            ? -_dashAngleAdjustSpeed : _dashAngleAdjustSpeed) * delta);
        //_body.Velocity = moveDir.Rotated((_upPressed 
        //    ? _dashAngleAdjustSpeed : -_dashAngleAdjustSpeed) * delta);

        //// Get first horizontal normal.
        //foreach(var collision in _body.GetCollisions())
        //{
        //    normal = collision.GetNormal();
        //    if (normal == Vector2.Right || normal == Vector2.Left) break;
        //}
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
    public bool PerformDash(Vector2 direction, float duration)
    {
        if (DashCharges <= 0) return false;
        DashCharges--;
        _remainingDashTime = _aniPlayer.GetAnimation(_dashAnimationName).Length;

        direction = GetDashDirection();

        _body.Velocity = direction.Normalized() * _dashSpeed;
        _aniPlayer.Play(_dashAnimationName);
        _disabler.SetControlStatesExcept(
            false, _remainingDashTime,
            ControlIDs.INPUT,
            ControlIDs.HURTBOX,
            ControlIDs.HITBOX);

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
        if (DashCharges <= 0) DashCharges++;
    }
}
