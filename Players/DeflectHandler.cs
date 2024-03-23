using Godot;
using Godot.NodeExtensions;
using System.Collections.Generic;
using WorldGeneration;

namespace Players;

public partial class DeflectHandler : Node, IDisableableControl
{
    [Export] Sprite2D _sprite;
    [Export] CharacterBody2D _body;
    [Export] float _slomoTime = 5f;
    [Export] float _velocityToKnockbackMultiplier = 1.2f;
    [Export] DashHandler _dashHandler;
    [Export] AnimationPlayer _aniPlayer;
    [Export] StringName _dashAnimationName;
    float _remainingSlomoTime;
    public bool IsActive => _remainingDeflectTime > 0f;
    float _remainingDeflectTime;
    bool _isEnabled;
    CharacterBody2D _bodyToRedirect;

    HashSet<StringName> _activeInputsForRedirection = new();

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
        _remainingSlomoTime -= (float)delta;

        if (_body.IsOnFloor() && _remainingDeflectTime > 0f)
        {
            _body.Velocity = new(_body.Velocity.X / 2f, _body.Velocity.Y);
        }

        if (_remainingSlomoTime > 0f)
        {
            _remainingDeflectTime = 0f;
        }
        else _bodyToRedirect = null;
    }

    public void OnReceivedHitHandler(OnHitArgs args)
    {
        if (!IsActive) return;
        args.Deflected = true;
        _bodyToRedirect = args.HitBy.HurtboxOwner as CharacterBody2D;
        GD.Print("Deflecting.");
        SlowTime();
    }

    public void SlowTime()
    {
        _activeInputsForRedirection.Clear();
        _remainingSlomoTime = _slomoTime;
        EngineTimeManipulator.OverrideTimeTransition(new(0.01, 0));
        EngineTimeManipulator.QueueTimeTransition(new(_slomoTime));
        EngineTimeManipulator.QueueTimeTransition(new(1, 0.2));
    }

    public void RedirectDeflectedEnemy(params StringName[] inputs)
    {
        Vector2 deflectDir = new();

        foreach(var input in inputs)
        {
            deflectDir += input.ToDirection();
        }
        deflectDir = deflectDir.Normalized();

        _bodyToRedirect.GetChild<KnockbackHandler>()
            .ApplyKnockback(-deflectDir * _bodyToRedirect.Velocity.Normalized() * _velocityToKnockbackMultiplier);

        EngineTimeManipulator.OverrideTimeTransition(new(1, 0.2));

        _remainingSlomoTime = 0f;
        _bodyToRedirect = null;
    }

    public void InputEventHandler(StringName input, bool pressed)
    {
        if (_remainingSlomoTime <= 0f) return;
        if (pressed && input == InputNames.ACTION)
        {
            GD.Print("Performing dash after block.");
            EngineTimeManipulator.OverrideTimeTransition(new(1, 0.2));
            _dashHandler.DashCharges = 2;
        }
    }
}
