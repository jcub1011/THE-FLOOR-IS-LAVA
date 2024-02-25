using Godot;
using System;

namespace Players;

public partial class KnockbackHandler : Node, IDisableableControl
{
    [Export] float _recoveryTime = 0.2f;
    [Export] CharacterBody2D _body;
    [Export] ControlDisablerHandler _disabler;

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

    void DisableHandlers(float time)
    {
        //EmitSignal(SignalName.OnDisableMovementControl);
        _disabler.SetControlStates(false,
            time,
            ControlIDs.ATTACK_HANDLER,
            ControlIDs.HURTBOX, 
            ControlIDs.MOVEMENT,
            ControlIDs.AUTO_ANIMATION,
            ControlIDs.DEFLECT,
            ControlIDs.FLIPPER);
    }
}
