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
    [Export] float _blockCooldownTime = 0.3f;
    [Export] float _velocityToKnockbackMultiplier = 1.2f;
    [Export] AnimationPlayer _aniPlayer;
    [Export] StringName _blockAnimation = "tank_block";
    [Signal] public delegate void OnSuccessfulDeflectEventHandler();
    float _remainingSlomoTime;
    float _remainingBlockCooldown;
    public bool IsActive => _remainingDeflectTime > 0f;
    float _remainingDeflectTime;
    bool _isEnabled;
    CharacterBody2D _bodyToRedirect;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.DEFLECT; }

    public void SetControlState(bool enabled)
    {
        _remainingDeflectTime = 0f;
    }
    #endregion

    public override void _Ready()
    {
        base._Ready();
    }

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

        _remainingBlockCooldown -= (float)delta;
    }

    public void OnReceivedHitHandler(OnHitArgs args)
    {
        if (!IsActive) return;
        args.Deflected = true;
        _bodyToRedirect = args.HitBy.HurtboxOwner as CharacterBody2D;
        GD.Print("Deflecting.");
        SlowTime();
        EmitSignal(SignalName.OnSuccessfulDeflect);
    }

    public void SlowTime()
    {
        _remainingSlomoTime = _slomoTime;
        EngineTimeManipulator.OverrideTimeTransition(new(0.01, 0));
        EngineTimeManipulator.QueueTimeTransition(new(_slomoTime));
        EngineTimeManipulator.QueueTimeTransition(new(1, 0.2));
    }

    public void RedirectDeflectedEnemy()
    {
        Vector2 dashDir = this.GetSibling<PlayerInputHandler>().InputAxis;

        _bodyToRedirect.GetChild<KnockbackHandler>()
            .ApplyKnockback(-dashDir * _bodyToRedirect.Velocity.Normalized() * _velocityToKnockbackMultiplier);

        EngineTimeManipulator.OverrideTimeTransition(new(1, 0.2));

        _remainingSlomoTime = 0f;
        _bodyToRedirect = null;
    }

    public void InputEventHandler(StringName input, bool pressed)
    {
        if (!pressed) return;

        if (_remainingSlomoTime <= 0f && input == InputNames.BLOCK)
        {
            RequestBlock();
        }
        else if (_remainingSlomoTime > 0f && input == InputNames.ACTION)
        {
            GD.Print("Performing dash after block.");
            RedirectDeflectedEnemy();
            EngineTimeManipulator.OverrideTimeTransition(new(1, 0.2));
        }
    }

    public void RequestBlock()
    {
        if (_remainingBlockCooldown > 0f) return;
        GD.Print("Performing Block.");
        _remainingBlockCooldown = _aniPlayer.GetAnimation(_blockAnimation).Length + _blockCooldownTime;
        _aniPlayer.Play(_blockAnimation);
        GetParent().GetDirectChild<ControlDisablerHandler>().DisableControlsExcept(
            _aniPlayer.GetAnimation(_blockAnimation).Length,
            ControlIDs.GRAVITY,
            ControlIDs.INPUT,
            ControlIDs.HITBOX);
    }
}
