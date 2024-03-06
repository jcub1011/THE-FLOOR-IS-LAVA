using Godot;
using System;

namespace Players;

public partial class BallActionHandler : Node, IDisableableControl
{
    [Export] CharacterBody2D _body;
    [Export] AnimationPlayer _aniPlayer;
    [Export] ControlDisablerHandler _controlDisabler;
    [Export] StringName _blockAnimation;
    [Export] StringName _dashAnimation;
    [Export] DashHandler _dashHandler;
    bool _enabled = true;

    public string ControlID => ControlIDs.ACTION_HANDLER;

    public void SetControlState(bool enabled)
    {
        _enabled = enabled;
    }

    public void InputEventHandler(StringName input, bool pressed)
    {
        if (!_enabled) return;
        if (!pressed) return;

        if (input == InputNames.ACTION)
        {
            PerformDash();
        }
        else if (input == InputNames.BLOCK)
        {
            PerformBlock();
        }
    }

    void PerformDash()
    {
        GD.Print("Performing Dash.");
        if (_dashHandler.PerformDash(_body.Velocity))
        {
            _aniPlayer.Play(_dashAnimation);
            _controlDisabler.SetControlStatesExcept(
                false, _aniPlayer.GetAnimation(_dashAnimation).Length,
                ControlIDs.INPUT);
        }
        else GD.Print("Insufficient dash charges.");
    }

    void PerformBlock()
    {
        GD.Print("Performing Block.");
        _aniPlayer.Play(_blockAnimation);
        _controlDisabler.SetControlStatesExcept(
            false, _aniPlayer.GetAnimation(_blockAnimation).Length,
            ControlIDs.GRAVITY,
            ControlIDs.INPUT);
    }
}
