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
    [Export] float _dashSpeed = 3520;
    [Export] float _maxDashSpeed = 5600;
    [Export] float _dashGravityDisableTime = 0.08f;
    [Export] float _movementDisableTime = 0.1f;
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

    //float _remainingDashTime;

    float _remainingDashHoldTime;
    const float MAX_DASH_HOLD_TIME = 0.5f;
    float _initalSpeed;
    bool _holdingDash;

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

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _remainingDashHoldTime -= (float)delta;

        if (_holdingDash)
        {
            if (!float.IsNaN(_remainingDashHoldTime) && _remainingDashHoldTime <= 0)
            {
                CompleteHeldDash();
                _remainingDashHoldTime = float.NaN;
            }
            else
            {
                //_body.Velocity = _body.Velocity.Normalized() 
                //    * (_remainingDashHoldTime / MAX_DASH_HOLD_TIME) * _initalSpeed;

                float newSpeed = Mathf.Clamp(
                    Mathf.Pow(
                        Mathf.Clamp(_remainingDashHoldTime / MAX_DASH_HOLD_TIME, 0f, 1f), 3f) 
                    * _initalSpeed,
                    0f, _initalSpeed);
                _body.Velocity = newSpeed * _body.Velocity.Normalized();
            }
        }
    }

    void PerformDash(float speed, Vector2 direction)
    {
        float dashLength = _aniPlayer.GetAnimation(_dashAnimationName).Length;
        _body.Velocity = direction.Normalized() * speed;
        _disabler.DisableControlsExcept(
            dashLength,
            ControlIDs.HITBOX);
        _disabler.DisableControls(
            _movementDisableTime,
            ControlIDs.MOVEMENT);
        _disabler.DisableControls(
            _dashGravityDisableTime,
            ControlIDs.GRAVITY);
        _aniPlayer.Play(_dashAnimationName);
    }

    void HandleDashNudging(float delta)
    {
        //if (_upPressed == _downPressed) return;
        //Vector2 moveDir = _body.Velocity;
        //_body.Velocity += new Vector2(0f, (_upPressed 
        //    ? -_dashAngleAdjustSpeed : _dashAngleAdjustSpeed) * delta);
    }

    public void InputEventHandler(StringName input, bool pressed)
    {
        if (input == InputNames.LEFT) _leftPressed = pressed;
        if (input == InputNames.RIGHT) _rightPressed = pressed;
        if (input == InputNames.JUMP) _upPressed = pressed;
        if (input == InputNames.CROUCH) _downPressed = pressed;

        if (input != InputNames.ACTION) return;

        if (!pressed)
        {
            if (_holdingDash) CompleteHeldDash();
        }
        else
        {
            StartHeldDash();
        }
    }

    public bool PerformInstaDash()
    {
        if (DashCharges <= 0) return false;
        DashCharges--;

        PerformDash(_dashSpeed, GetDashDirection());
        return true;
    }

    public bool StartHeldDash()
    {
        if (DashCharges <= 0) return false;
        DashCharges--;
        _holdingDash = true;
        _remainingDashHoldTime = MAX_DASH_HOLD_TIME;

        float speed = _body.Velocity.Length();
        _initalSpeed = speed;
        _body.Velocity = _body.Velocity.Normalized() * Mathf.Clamp(speed, -10f, 10f);
        _disabler.DisableControlsExcept(
            MAX_DASH_HOLD_TIME,
            ControlIDs.HITBOX);

        return true;
    }

    void CompleteHeldDash()
    {
        _holdingDash = false;
        float speed = _dashSpeed + (_maxDashSpeed - _dashSpeed) * (MAX_DASH_HOLD_TIME - _remainingDashHoldTime) / MAX_DASH_HOLD_TIME;
        PerformDash(speed, GetDashDirection());
    }

    /// <summary>
    /// Returns true if the dash was able to be performed.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool PerformDash(Vector2 direction, float duration)
    {
        return DashCharges > 0;
        return StartHeldDash();
        if (DashCharges <= 0) return false;
        DashCharges--;

        float speed = _body.Velocity.Length();
        _body.Velocity = _body.Velocity.Normalized() * Mathf.Clamp(speed, -30f, 30f);
        _disabler.DisableControlsExcept(
            1f,
            ControlIDs.INPUT,
            //ControlIDs.HURTBOX,
            ControlIDs.HITBOX);
        _disabler.DisableControls(
            _movementDisableTime,
            ControlIDs.MOVEMENT);
        _disabler.DisableControls(
            _dashGravityDisableTime,
            ControlIDs.GRAVITY);

        _remainingDashHoldTime = 0.05f;

        return true;
    }

    Vector2 GetDashDirection()
    {
        Vector2 direction = new();
        if (_leftPressed) direction.X -= 0.8f;
        if (_rightPressed) direction.X += 0.8f;
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
