using Godot;
using Godot.NodeExtensions;
using Godot.MathExtensions;
using WorldGeneration;

namespace Players;

public partial class KnockbackHandler : Node, IDisableableControl
{
    [Export] float _recoveryTime = 0.2f;
    [Export] CharacterBody2D _body;
    [Export] ControlDisablerHandler _disabler;
    [Export] AnimationPlayer _aniPlayer;
    [Export] StringName _staggerAnimationName = "stagger";
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
        GetParent<PlayerController>().EnableBouncing(recoveryTime);
    }

    public void OnApplyKnockback(float knockback, Node2D source)
    {
        if (source is PlayerController player)
        {
            _body.Velocity = player.Velocity;
        }
        else
        {
            Vector2 newVel = _body.GlobalPosition.RelativeTo(source.GlobalPosition)
                .Normalized() * knockback;
            _body.Velocity = newVel;
        }
        DisableHandlers(_recoveryTime);
        GetParent<PlayerController>().EnableBouncing(_recoveryTime);
    }

    void DisableHandlers(float time)
    {
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
        //GD.Print("Calling hit landed but it is not defined.");
        Vector2 direction = node.GlobalPosition.RelativeTo(_body.GlobalPosition);
        Vector2 knockback = new(direction.X < 0f ? 1f : -1f, -1f);
        ApplyKnockback(knockback.Normalized() * _body.Velocity.Length(), 0f);
    }
}
