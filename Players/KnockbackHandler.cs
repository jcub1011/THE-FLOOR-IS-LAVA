using Godot;
using System;

namespace Players;

public partial class KnockbackHandler : Node, IDisableableControl
{
    [Signal] public delegate void OnEnableMovementControlEventHandler();
    [Signal] public delegate void OnDisableMovementControlEventHandler();
    [Export] float _recoveryTime = 0.2f;
    [Export] CharacterBody2D _body;

    float _remainingDisableTime;
    float _previousRemainingDisableTime;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.KNOCKBACK; }

    public void SetControlState(bool enabled)
    {
        GD.PushWarning($"{nameof(KnockbackHandler)}.{nameof(SetControlState)} " +
            $"is not implemented.");
    }
    #endregion

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _previousRemainingDisableTime = _remainingDisableTime;
        _remainingDisableTime -= (float)delta;

        if (JustEnabled())
        {
            EnableHandlers();
        }
    }

    public void OnApplyKnockback(float knockback, Node2D source)
    {
        DisableHandlers(_recoveryTime);
        Vector2 newVel = _body.GlobalPosition.RelativeTo(source.GlobalPosition)
            .Normalized() * knockback;
        if (_body.IsOnFloor()) newVel.Y = -knockback;
        _body.Velocity = newVel;
    }

    public void OnApplyDeflectedKnockback(CharacterBody2D deflector, float knockback)
    {
        DisableHandlers(_recoveryTime);
        Vector2 newVel = deflector.GlobalPosition.RelativeTo(_body.GlobalPosition)
            .Normalized() * knockback * 2f;
        if (_body.IsOnFloor()) newVel.Y = -knockback;
        _body.Velocity = newVel;
    }

    void EnableHandlers()
    {
        GD.Print("Re-enabling handlers.");
        EmitSignal(SignalName.OnEnableMovementControl);
    }

    void DisableHandlers(float time)
    {
        _remainingDisableTime = time;
        GD.Print($"Disabling handlers for {_remainingDisableTime}s.");
        EmitSignal(SignalName.OnDisableMovementControl);
    }

    bool JustEnabled()
    {
        return _previousRemainingDisableTime > 0f && _remainingDisableTime <= 0f;
    }
}
