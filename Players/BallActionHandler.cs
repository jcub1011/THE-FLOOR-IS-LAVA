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
    bool _enabled;

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
        _aniPlayer.Play(_dashAnimation);
        _controlDisabler.SetControlStatesExcept(
            false, _aniPlayer.GetAnimation(_dashAnimation).Length);
    }

    void PerformBlock()
    {
        _aniPlayer.Play(_blockAnimation);
        _controlDisabler.SetControlStatesExcept(
            false, _aniPlayer.GetAnimation(_blockAnimation).Length,
            ControlIDs.GRAVITY);
    }
}
