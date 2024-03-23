using Godot;
using Godot.NodeExtensions;
using System;
using System.Linq;
using WorldGeneration;
using static Godot.TextServer;

namespace Players;

public partial class DashHandler : Node, IDisableableControl
{
    [Export] PlayerController _body;
    [Export] KnockbackHandler _knockback;
    [Export] FlipHandler _flip;
    [Export] float _dashSpeedInTiles = 27.5f;
    [Export] float _maxDashSpeedInTiles = 43.75f;
    [Export] float _dashGravityDisableTime = 0.08f;
    [Export] float _movementDisableTime = 0.1f;
    [Export] float _dashSpeedFromDeflectingInTiles = 40f;
    [Export] StringName _dashAnimationName;
    [Export] AnimationPlayer _aniPlayer;
    [Export] ControlDisablerHandler _disabler;
    bool _nextDashIsDeflectDash;
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


    float _remainingDashHoldTime;
    const float MAX_DASH_HOLD_TIME = 0.5f;
    float _initalSpeed;
    bool _holdingDash;

    #region Interface Implementation
    string IDisableableControl.ControlID => ControlIDs.DASH;

    bool _isEnabled = true;
    void IDisableableControl.SetControlState(bool enabled)
    {
        _isEnabled = enabled;
        if (!enabled)
        {
            _holdingDash = false;
            _remainingDashHoldTime = 0f;
        }
    }
    #endregion

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_body.IsOnFloor())
        {
            if (DashCharges <= 0) DashCharges = 1;
        }
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

    public void InputEventHandler(StringName input, bool pressed)
    {
        if (input != InputNames.ACTION || !_isEnabled) return;

        if (!pressed)
        {
            if (_holdingDash) CompleteHeldDash();
        }
        else
        {
            if (_nextDashIsDeflectDash) PerformInstaDash();
            StartHeldDash();
        }
    }

    public void PerformInstaDash()
    {
        DashCharges = 1;
        PerformDash(_dashSpeedFromDeflectingInTiles.ToPixels(), GetDashDirection());
        _nextDashIsDeflectDash = false;
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
            ControlIDs.HITBOX,
            ControlIDs.DASH);

        return true;
    }

    void CompleteHeldDash()
    {
        _holdingDash = false;
        float speed = _dashSpeedInTiles 
            + (_maxDashSpeedInTiles - _dashSpeedInTiles) 
            * (MAX_DASH_HOLD_TIME - _remainingDashHoldTime) / MAX_DASH_HOLD_TIME;
        PerformDash(speed.ToPixels(), GetDashDirection());
    }

    Vector2 GetDashDirection()
    {
        Vector2 inputdir = this.GetSibling<PlayerInputHandler>().InputAxis;
        if (inputdir.LengthSquared() == 0f)
        {
            return new(_flip.FacingLeft ? -1f : 1f, 0f);
        }
        inputdir.Y *= 1.2f;
        return inputdir.Normalized();
    }

    public void OnHitLandedHandler(Node2D thingHit)
    {
        if (DashCharges <= 0) DashCharges = 1;
    }

    public void OnBlockLandedHandler()
    {
        _nextDashIsDeflectDash = true;
    }
}
