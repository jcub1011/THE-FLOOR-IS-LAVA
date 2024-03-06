using Godot;

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

        if (_body.IsOnFloor() && _remainingDeflectTime > 0f)
        {
            _body.Velocity = new(_body.Velocity.X / 2f, _body.Velocity.Y);
        }
    }

    public void OnReceivedHitHandler(OnHitArgs args)
    {
        if (_remainingDeflectTime <= 0f) return;
        var direction = args.HitBy.HurtboxOwner.GlobalPosition
            .RelativeTo(_body.GlobalPosition);
        args.ReturnKnockback = true;
    }
}
