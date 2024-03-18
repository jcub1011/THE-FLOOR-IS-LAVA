using Godot;
using Godot.NodeExtensions;
using Godot.MathExtensions;

namespace Players;

public partial class KnockbackHandler : Node, IDisableableControl
{
    [Export] float _recoveryTime = 0.2f;
    [Export] CharacterBody2D _body;
    [Export] ControlDisablerHandler _disabler;
    [Export] AnimationPlayer _aniPlayer;
    [Export] StringName _staggerAnimationName = "stagger";
    [Export] float _staggeredKnockbackMultiplier = 2f;
    [Export] float _hitLandedKnockbackStrength = 80f;
    [Export] float _hitLandedRecoveryTime = 0.08f;
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

    public void ApplyKnockback(Vector2 knockback)
    {
        ApplyKnockback(knockback, _recoveryTime);
    }

    public void ApplyKnockback(Vector2 knockback, float recoveryTime)
    {
        DisableHandlers(recoveryTime);
        _remainingStagger = 0f;
        _body.Velocity = knockback;
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

    void DisableHandlers(float time)
    {
        //EmitSignal(SignalName.OnDisableMovementControl);
        _disabler.DisableControlsExcept(
            time,
            ControlIDs.INPUT,
            ControlIDs.GRAVITY);
    }

    public void SetInStaggerState(CharacterBody2D body)
    {
        float staggerTime = _aniPlayer.GetAnimation(_staggerAnimationName).Length;
        DisableHandlers(staggerTime);
        _remainingStagger = staggerTime;
        _aniPlayer.PlayIfExists(_staggerAnimationName);
        _inStaggerState = true;
    }

    public void OnHitLanded(Node2D node)
    {
        Vector2 direction = node.GlobalPosition.RelativeTo(_body.GlobalPosition);
        Vector2 knockback = new(direction.X < 0f ? 1f : -1f, -3f);
        ApplyKnockback(knockback * _hitLandedKnockbackStrength, _hitLandedRecoveryTime);
    }
}
