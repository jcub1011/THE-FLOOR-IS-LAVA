using Godot;
using Godot.NodeExtensions;

namespace Players;

public partial class AttackHandler : Node, IDisableableControl
{
    [Export] CharacterBody2D _body;
    [Export] AnimationPlayer _aniPlayer;
    [Export] ControlDisablerHandler _controlDisabler;
    [Export] float _dropkickDrag;
    [Export] float _actionBufferTime = 0.5f;
    [Export] float _blockBufferTime = 0.5f;
    [Export] float _blockCooldown = 0.3f;

    [Export] StringName _dropkickAnimation = "dropkick";
    [Export] StringName _punchAnimation = "punch";
    [Export] StringName _deflectAnimation = "deflect";
    [Export] StringName _crouchedDeflectAnimation = "crouched_deflect";

    bool _isDisabled;
    float _remainingBlockCooldown = 0f;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.ATTACK_HANDLER; }

    public void SetControlState(bool enabled)
    {
        _isDisabled = !enabled;
    }
    #endregion

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _remainingBlockCooldown -= (float)delta;
        if (_isDisabled)
        {
            if (_body.IsOnFloor())
            {
                Vector2 newVel = _body.Velocity;
                newVel.X *= (1 - _dropkickDrag * (float)delta);
                _body.Velocity = newVel;
            }
        }
        else
        {
            if (InputBuffer.IsBuffered(_body, InputNames.ACTION,
                _actionBufferTime))
            {
                GD.Print($"Performing buffered attack input.");
                InputBuffer.ConsumeBuffer(_body, InputNames.ACTION);
                OnAttack();
            }
            else if (InputBuffer.IsBuffered(_body, InputNames.BLOCK,
                _blockBufferTime))
            {
                GD.Print($"Performing buffered block input.");
                InputBuffer.ConsumeBuffer(_body, InputNames.ACTION);
                OnDeflect();
            }
        }
    }

    public void OnDeflect()
    {
        if (_isDisabled || _remainingBlockCooldown > 0f)
        {
            GD.Print($"{GetParent().Name} unable to perform action, currently " +
                $"disabled.");
            InputBuffer.BufferInput(_body, InputNames.BLOCK);
            return;
        }
        else
        {
            InputBuffer.ConsumeBuffer(_body, InputNames.BLOCK);
        }

        StringName animationToUse = GetParent().GetChild<CrouchHandler>().IsCrouched
            ? _crouchedDeflectAnimation : _deflectAnimation;

        GD.Print($"Performing {animationToUse}.");
        _controlDisabler.DisableControlsExcept(
            _aniPlayer.GetAnimation(animationToUse).Length + 0.05f,
                ControlIDs.GRAVITY,
                ControlIDs.INPUT,
                ControlIDs.HITBOX,
                ControlIDs.DEFLECT);
        _aniPlayer.Play(animationToUse);
        _remainingBlockCooldown = _aniPlayer.GetAnimation(animationToUse).Length + _blockCooldown;
    }

    public void OnAttack()
    {
        if (_isDisabled)
        {
            GD.Print($"{GetParent().Name} unable to perform action, currently " +
                $"disabled.");
            InputBuffer.BufferInput(_body, InputNames.ACTION);
            return;
        }
        else
        {
            InputBuffer.ConsumeBuffer(_body, InputNames.ACTION);
        }

        StringName animationToUse = GetParent().GetChild<CrouchHandler>().IsCrouched
            ? _dropkickAnimation : _punchAnimation;

        GD.Print($"Performing {animationToUse}.");
        _controlDisabler.DisableControlsExcept(
            _aniPlayer.GetAnimation(animationToUse).Length + 0.05f,
                ControlIDs.GRAVITY,
                ControlIDs.INPUT,
                ControlIDs.HITBOX,
                ControlIDs.HURTBOX);
        //_body.Velocity = new(0f, _body.Velocity.Y);
        _aniPlayer.Play(animationToUse);
    }
}
