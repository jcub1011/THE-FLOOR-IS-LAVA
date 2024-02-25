using Godot;
using Physics;
using System;

namespace Players;

public partial class AttackHandler : Node, IDisableableControl
{
    [Export] CharacterBody2D _body;
    [Export] AnimationPlayer _aniPlayer;
    [Export] ControlDisablerHandler _controlDisabler;
    [Export] float _dropkickDrag;

    [Export] StringName _dropkickAnimation = "dropkick";
    [Export] StringName _punchAnimation = "punch";
    [Export] StringName _deflectAnimation = "deflect";

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
                newVel.X *= ( 1 - _dropkickDrag * (float)delta);
                _body.Velocity = newVel;
            }
        }
    }

    public void OnAction()
    {
        if (_isDisabled)
        {
            GD.Print($"{GetParent().Name} unable to perform action, currently " +
                $"disabled.");
            return;
        }

        if (_body.IsOnFloor())
        {
            if (_body.Velocity.X == 0f)
            {
                // Perform deflect.
                GD.Print("Performing deflect.");
                _controlDisabler.SetControlStates(false, 
                    _aniPlayer.GetAnimation(_deflectAnimation).Length,
                    ControlIDs.ATTACK_HANDLER,
                    ControlIDs.HURTBOX,
                    ControlIDs.MOVEMENT,
                    ControlIDs.AUTO_ANIMATION,
                    ControlIDs.FLIPPER);
                //DisableHandlers(_aniPlayer.GetAnimation(_deflectAnimation).Length);
                _aniPlayer.Play(_deflectAnimation);
            }
            else
            {
                // Perform punch.
                GD.Print("Performing punch.");
                _controlDisabler.SetControlStates(false,
                    _aniPlayer.GetAnimation(_punchAnimation).Length,
                    ControlIDs.ATTACK_HANDLER,
                    ControlIDs.MOVEMENT,
                    ControlIDs.AUTO_ANIMATION,
                    ControlIDs.FLIPPER);
                //DisableHandlers(_aniPlayer.GetAnimation(_punchAnimation).Length);
                _body.Velocity = new(0f, _body.Velocity.Y);
                _aniPlayer.Play(_punchAnimation);
            }
        }
        else if (_body.Velocity.X != 0f)
        {
            // Perform dropkick.
            GD.Print("Performing dropkick.");
            _controlDisabler.SetControlStates(false,
                _aniPlayer.GetAnimation(_dropkickAnimation).Length,
                ControlIDs.ATTACK_HANDLER,
                ControlIDs.MOVEMENT,
                ControlIDs.AUTO_ANIMATION, 
                ControlIDs.FLIPPER);
            //DisableHandlers(_aniPlayer.GetAnimation(_dropkickAnimation).Length);
            _aniPlayer.Play(_dropkickAnimation);
        }
    }
}
