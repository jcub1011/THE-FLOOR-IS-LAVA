using Godot;

namespace Players;

public partial class AttackHandler : Node, IDisableableControl
{
    [Export] CharacterBody2D _body;
    [Export] AnimationPlayer _aniPlayer;
    [Export] ControlDisablerHandler _controlDisabler;
    [Export] float _dropkickDrag;
    [Export] float _actionBufferTime;

    [Export] StringName _dropkickAnimation = "dropkick";
    [Export] StringName _punchAnimation = "punch";
    [Export] StringName _deflectAnimation = "deflect";
    [Export] StringName _crouchedDeflectAnimation = "crouched_deflect";

    bool _isDisabled;

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
                GD.Print($"Performing buffered input.");
                InputBuffer.ConsumeBuffer(_body, InputNames.ACTION);
                OnAttack();
            }
        }
    }

    public void OnDeflect()
    {
        if (_isDisabled)
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
        _controlDisabler.SetControlStates(false,
            _aniPlayer.GetAnimation(animationToUse).Length,
                ControlIDs.ATTACK_HANDLER,
                ControlIDs.HURTBOX,
                ControlIDs.MOVEMENT,
                ControlIDs.AUTO_ANIMATION,
                ControlIDs.FLIPPER,
                ControlIDs.CROUCHER);
        _aniPlayer.Play(animationToUse);
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
        _controlDisabler.SetControlStates(false,
            _aniPlayer.GetAnimation(animationToUse).Length,
            ControlIDs.ATTACK_HANDLER,
            ControlIDs.MOVEMENT,
            ControlIDs.AUTO_ANIMATION,
            ControlIDs.FLIPPER,
            ControlIDs.CROUCHER);
        //_body.Velocity = new(0f, _body.Velocity.Y);
        _aniPlayer.Play(animationToUse);
    }
}
