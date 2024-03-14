using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;
using WorldGeneration;

namespace Players;

public partial class DeflectHandler : Node, IDisableableControl
{
    [Export] Sprite2D _sprite;
    [Export] CharacterBody2D _body;
    [Export] float _slomoTime = 5f;
    [Export] float _deflectKnockback = 100f;
    [Export] float _successfulKnockbackBounce = 30f;
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
        EngineTimeManipulator.QueueTimeTransition(new(0.0001, 0));
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
            .ApplyKnockback(-deflectDir * _deflectKnockback);

        EngineTimeManipulator.OverrideTimeTransition(new(1, 0.2));

        Vector2 dir = deflectDir;

        _body.Velocity = dir * _successfulKnockbackBounce;

        _remainingSlomoTime = 0f;
        _bodyToRedirect = null;
    }

    public void InputEventHandler(StringName input, bool pressed)
    {
        if (_remainingSlomoTime <= 0f) return;

        if (pressed)
        {
            _activeInputsForRedirection.Add(input);
            if (_activeInputsForRedirection.Count >= 2)
            {
                RedirectDeflectedEnemy(_activeInputsForRedirection.ToArray());
            }
        }
        else
        {
            if (_activeInputsForRedirection.Remove(input))
            {
                RedirectDeflectedEnemy(input);
            }
        }
    }
}
