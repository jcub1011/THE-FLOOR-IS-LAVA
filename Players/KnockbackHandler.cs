using Godot;
using System.Reflection;

namespace Players;

public partial class KnockbackHandler : Node, IDisableableControl
{
    [Export] float _recoveryTime = 0.2f;
    [Export] CharacterBody2D _body;
    [Export] ControlDisablerHandler _disabler;
    [Export] AnimationPlayer _aniPlayer;
    [Export] StringName _staggerAnimationName = "stagger";
    [Export] float _staggeredKnockbackMultiplier = 2f;
    bool _inStaggerState = false;
    float _remainingStagger;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.KNOCKBACK; }

    public void SetControlState(bool enabled)
    {
        //GD.PushWarning($"{nameof(KnockbackHandler)}.{nameof(SetControlState)} " +
        //    $"is not implemented.");
    }
    #endregion

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _remainingStagger -= (float)delta;
        _inStaggerState = _remainingStagger > 0f;
    }

    public void OnApplyKnockback(float knockback, Node2D source)
    {
        knockback *= _inStaggerState ? _staggeredKnockbackMultiplier : 1f;
        DisableHandlers(_recoveryTime);
        Vector2 newVel = _body.GlobalPosition.RelativeTo(source.GlobalPosition)
            .Normalized() * knockback;
        if (_body.IsOnFloor()) newVel.Y = -knockback;
        _body.Velocity = newVel;
    }

    //public void OnApplyDeflectedKnockback(CharacterBody2D deflector, float knockback)
    //{
    //    knockback *= _inStaggerState ? _staggeredKnockbackMultiplier : 1f;
    //    Vector2 newVel = deflector.GlobalPosition.RelativeTo(_body.GlobalPosition)
    //        .Normalized() * knockback;

    //    DisableHandlers(_recoveryTime);
    //    if (_body.IsOnFloor()) newVel.Y = -knockback;
    //    _body.Velocity = newVel;
    //    _remainingStagger = 0f;
    //}

    void DisableHandlers(float time)
    {
        //EmitSignal(SignalName.OnDisableMovementControl);
        _disabler.SetControlStates(false,
            time,
            ControlIDs.ATTACK_HANDLER,
            ControlIDs.HITBOX,
            ControlIDs.HURTBOX,
            ControlIDs.MOVEMENT,
            ControlIDs.AUTO_ANIMATION,
            ControlIDs.DEFLECT,
            ControlIDs.FLIPPER,
            ControlIDs.CROUCHER);
    }

    public void SetInStaggerState()
    {
        float staggerTime = _aniPlayer.GetAnimation(_staggerAnimationName).Length;
        _disabler.SetControlStates(false,
            staggerTime,
            ControlIDs.ATTACK_HANDLER,
            ControlIDs.HURTBOX,
            ControlIDs.MOVEMENT,
            ControlIDs.AUTO_ANIMATION,
            ControlIDs.DEFLECT,
            ControlIDs.FLIPPER,
            ControlIDs.CROUCHER);
        _remainingStagger = staggerTime;
        _aniPlayer.PlayIfExists(_staggerAnimationName);
        _inStaggerState = true;
    }
}
