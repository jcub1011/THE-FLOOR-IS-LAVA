using Godot;
using Godot.NodeExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldGeneration;

namespace Players;

internal readonly struct DashInfo
{
    /// <summary>
    /// How fast the dash is.
    /// </summary>
    public readonly float Speed;
    /// <summary>
    /// How long the dash hurtbox should be enabled.
    /// </summary>
    public readonly float HurtboxEnabledTime;
    /// <summary>
    /// How long the dash button should be held to execute.
    /// </summary>
    public readonly float HoldTimeNeeded;
    /// <summary>
    /// The dash animation to use.
    /// </summary>
    public readonly StringName PerformAnimation;
    /// <summary>
    /// The dash charge animation to use.
    /// </summary>
    public readonly StringName ChargeAnimation;

    public DashInfo(float speed, float hurtboxEneabledTime, float holdTimeNeeded, StringName performAnimation, StringName chargeAnimation)
    {
        Speed = speed;
        HurtboxEnabledTime = hurtboxEneabledTime;
        HoldTimeNeeded = holdTimeNeeded;
        PerformAnimation = performAnimation;
        ChargeAnimation = chargeAnimation;
    }

    /// <summary>
    /// All values default to NaN and null.
    /// </summary>
    public DashInfo()
    {
        Speed = float.NaN;
        HurtboxEnabledTime = float.NaN;
        HoldTimeNeeded = float.NaN;
        PerformAnimation = null;
        ChargeAnimation = null;
    }

    public static List<DashInfo> GenerateDashInfo(Vector3[] dashTiers, string[] dashTierPerformAnimations, string[] dashTierChargeAnimations)
    {
        var newInfo = new List<DashInfo>();
        for(int i = 0; i < dashTiers.Length; i++)
        {
            newInfo.Add(
                new(dashTiers[i].X, dashTiers[i].Z, dashTiers[i].Y, 
                dashTierPerformAnimations[Mathf.Clamp(i, 0, dashTierPerformAnimations.Length - 1)],
                dashTierChargeAnimations[Mathf.Clamp(i, 0, dashTierChargeAnimations.Length - 1)]));
        }
        return newInfo;
    }

    public static DashInfo GetBestDashInfo(float timeDashButtonHeld, IEnumerable<DashInfo> dashInfos)
    {
        DashInfo best = default;
        float bestTime = float.NegativeInfinity;

        foreach(var info in dashInfos)
        {
            if (float.IsNaN(info.HoldTimeNeeded)) continue;

            if (info.HoldTimeNeeded <= timeDashButtonHeld && info.HoldTimeNeeded > bestTime)
            {
                best = info;
            }
        }

        return best;
    }
}

public partial class DashHandler : Node, IDisableableControl
{
    [Export] PlayerController _body;
    [Export] KnockbackHandler _knockback;
    [Export] FlipHandler _flip;
    [Export] MeleeHurtboxHandler _meleeHurtbox;
    //[Export] float _dashSpeedInTiles = 27.5f;
    //[Export] float _maxDashSpeedInTiles = 43.75f;
    // X is dash speed, Y is how long you have to hold to reach the tier, and Z is how long the dash hurtbox is active.
    [Export] Vector3[] _dashTiers = { new(25, 0f, 0.15f), new(38, 0.3f, 0.4f) };
    [Export] string[] _dashTierPerformAnimations = { "tank_dash" };
    [Export] string[] _dashTierChargeAnimations = { "tank_dash" };
    [Export] StringName _hurtboxName = "DashHurtbox";
    [Export] float _dashGravityDisableTime = 0.08f;
    [Export] float _movementDisableTime = 0.1f;
    [Export] float _dashSpeedFromDeflectingInTiles = 40f;
    //[Export] StringName _dashAnimationName;
    [Export] AnimationPlayer _aniPlayer;
    [Export] ControlDisablerHandler _disabler;
    [Export] float _dashBufferTime = 0.15f;
    [Signal] public delegate void DashPerformedEventHandler(Vector2 dashVelocity);
    [Signal] public delegate void DashChargeStartedEventHandler(StringName chargeAnimation);
    [Signal] public delegate void DashCanceledEventHandler();
    bool _nextDashIsDeflectDash;
    int _dashCharges;
    public int DashCharges
    {
        get => _dashCharges;
        set
        {
            //GD.Print($"{GetParent().Name} - " +
            //    $"Setting dash charges to {value} from {_dashCharges}.");
            _dashCharges = value;
        }
    }
    List<DashInfo> _processedDashInfo;


    float _remainingDashHoldTime;
    const float MAX_DASH_HOLD_TIME = 0.5f;
    float _initalSpeed;
    Vector2 _initialVelocity;
    bool _holdingDash;
    bool _isActionPressed;
    StringName _currentChargeAnimation;

    #region Interface Implementation
    string IDisableableControl.ControlID => ControlIDs.DASH;

    bool _isEnabled = true;
    void IDisableableControl.SetControlState(bool enabled)
    {
        _isEnabled = enabled;
        if (!enabled)
        {
            _currentChargeAnimation = null;
            if (_holdingDash)
            {
                EmitSignal(SignalName.DashCanceled);
            }
            _holdingDash = false;
            _remainingDashHoldTime = 0f;
        }
    }
    #endregion

    public override void _Ready()
    {
        base._Ready();
        //Array.Sort(_dashTiers, (x, y) => x.Y == y.Y ? 0 : (x.Y < y.Y ? -1 : 1));
        _processedDashInfo = DashInfo.GenerateDashInfo(_dashTiers, _dashTierPerformAnimations, _dashTierChargeAnimations);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_body.IsOnFloor())
        {
            if (DashCharges <= 0) DashCharges = 1;
        }
        
        if (InputBuffer.IsBuffered(_body, InputNames.ACTION, _dashBufferTime)
            && DashCharges > 0)
        {
            InputBuffer.ConsumeBuffer(_body, InputNames.ACTION);
            GD.Print("Performing buffered dash.");
            if (_nextDashIsDeflectDash) PerformInstaDash();
            else
            {
                StartHeldDash();
                if (!_isActionPressed) CompleteHeldDash();
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _remainingDashHoldTime -= (float)delta;

        if (_holdingDash)
        {
            SetChargeAnimation(MAX_DASH_HOLD_TIME - _remainingDashHoldTime);
            if (!float.IsNaN(_remainingDashHoldTime) && _remainingDashHoldTime <= 0)
            {
                CompleteHeldDash();
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

    void SetChargeAnimation(float timeDashHeld)
    {
        StringName dashAnimation = DashInfo.GetBestDashInfo(timeDashHeld, _processedDashInfo).ChargeAnimation;
        if (dashAnimation != _currentChargeAnimation)
        {
            _currentChargeAnimation = dashAnimation;
            EmitSignal(SignalName.DashChargeStarted);
        }
        _aniPlayer.PlayIfNotPlaying(dashAnimation);
    }

    void PerformDash(DashInfo info, Vector2 direction)
    {
        _currentChargeAnimation = null;
        float dashLength = _aniPlayer.GetAnimation(info.PerformAnimation).Length;
        Vector2 dashVelocity = direction.Normalized() * info.Speed.ToPixels();

        // To maintain momentum.
        if (dashVelocity.X != 0f && 
            Mathf.Abs(dashVelocity.X) < Mathf.Abs(_initialVelocity.X))
        {
            dashVelocity.X = dashVelocity.X < 0f ? -Mathf.Abs(_initialVelocity.X) : Mathf.Abs(_initialVelocity.X);
        }

        _body.Velocity = direction.Normalized() * info.Speed.ToPixels();
        _disabler.DisableControlsExcept(
            dashLength,
            ControlIDs.HITBOX);
        _disabler.DisableControls(
            _movementDisableTime,
            ControlIDs.MOVEMENT);
        _disabler.DisableControls(
            _dashGravityDisableTime,
            ControlIDs.GRAVITY);
        _aniPlayer.Play(info.PerformAnimation);
        _meleeHurtbox.EnableHitbox(_hurtboxName, info.HurtboxEnabledTime);
        EmitSignal(SignalName.DashPerformed, _body.Velocity);
    }

    public void InputEventHandler(StringName input, bool pressed)
    {
        if (input != InputNames.ACTION) return;
        _isActionPressed = pressed;
        if (pressed && DashCharges <= 0)
        {
            InputBuffer.BufferInput(_body, InputNames.ACTION);
            return;
        }
        else if (!_isEnabled) return;

        if (!pressed)
        {
            if (_holdingDash) CompleteHeldDash();
        }
        else
        {
            InputBuffer.ConsumeBuffer(_body, InputNames.ACTION);
            if (_nextDashIsDeflectDash) PerformInstaDash();
            else StartHeldDash();
        }
    }

    public void PerformInstaDash()
    {
        EngineTimeManipulator.OverrideTimeTransition(new(1, 0.1));
        DashCharges = 1;
        PerformDash(DashInfo.GetBestDashInfo(MAX_DASH_HOLD_TIME, _processedDashInfo), GetDashDirection());
        _holdingDash = false;
        _nextDashIsDeflectDash = false;
    }

    public bool StartHeldDash()
    {
        if (DashCharges <= 0) return false;
        DashCharges--;
        _holdingDash = true;
        _remainingDashHoldTime = MAX_DASH_HOLD_TIME;

        float speed = _body.Velocity.Length();
        _initialVelocity = _body.Velocity;
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
        PerformDash(DashInfo.GetBestDashInfo(MAX_DASH_HOLD_TIME - _remainingDashHoldTime, _processedDashInfo), GetDashDirection());
        _remainingDashHoldTime = float.NaN;
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
        _disabler.EnableControls(ControlIDs.DASH);
        _nextDashIsDeflectDash = true;
    }
}
