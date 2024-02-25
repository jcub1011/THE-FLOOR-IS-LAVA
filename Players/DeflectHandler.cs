using Godot;
using System;

namespace Players;

public partial class DeflectHandler : Node, IDisableableControl
{
    [Export] Sprite2D _sprite;
    [Export] CharacterBody2D _body;
    float _remainingDeflectTime;
    bool _isEnabled;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.DEFLECT; }

    public void SetControlState(bool enabled)
    {
        _remainingDeflectTime = 0f;
    }
    #endregion

    public void EnableDeflect(float duration)
    {
        _remainingDeflectTime = duration;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _remainingDeflectTime -= (float)delta;
    }

    public void OnReceivedHitHandler(OnHitArgs args)
    {
        if (_remainingDeflectTime <= 0f) return;
        var direction = args.HitBy.HurtboxOwner.GlobalPosition
            .RelativeTo(_body.GlobalPosition);
        bool isTowardsLeft = direction.X < 0f;
        args.ReturnKnockback = _sprite.FlipH == isTowardsLeft;
    }
}
