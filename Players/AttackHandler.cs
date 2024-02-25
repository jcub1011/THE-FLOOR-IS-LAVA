using Godot;
using Physics;
using System;

namespace Players;

public partial class AttackHandler : Node
{
    [Export] CharacterBody2D _body;
    [Export] AnimationPlayer _aniPlayer;
    [Export] float _dropkickDrag;

    [Export] StringName _dropkickAnimation = "dropkick";
    [Export] StringName _punchAnimation = "punch";
    [Export] StringName _deflectAnimation = "deflect";

    [Signal] public delegate void OnDisableMovementControlEventHandler();
    [Signal] public delegate void OnEnableMovementControlEventHandler();

    float _remainingDisableTime;
    float _previousRemainingDisableTime;

    bool _isDisabled;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.ATTACK_HANDLER; }

    public void SetControlState(bool enabled)
    {
        if (enabled) OnEnable();
        else OnDisable();
    }
    #endregion

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _previousRemainingDisableTime = _remainingDisableTime;
        _remainingDisableTime -= (float)delta;

        if (_remainingDisableTime > 0f)
        {
            if (_body.IsOnFloor())
            {
                Vector2 newVel = _body.Velocity;
                newVel.X *= ( 1 - _dropkickDrag * (float)delta);
                _body.Velocity = newVel;
            }
        }

        if (JustEnabled())
        {
            EnableHandlers();
        }
    }

    void EnableHandlers()
    {
        GD.Print("Re-enabling handlers.");
        EmitSignal(SignalName.OnEnableMovementControl);
    }

    void DisableHandlers(float time)
    {
        _remainingDisableTime = time;
        GD.Print($"Disabling handlers for {_remainingDisableTime}s.");
        EmitSignal(SignalName.OnDisableMovementControl);
    }

    bool JustEnabled()
    {
        return _previousRemainingDisableTime > 0f && _remainingDisableTime <= 0f;
    }

    public void OnAction()
    {
        if (_isDisabled)
        {
            GD.Print($"{GetParent().Name} unable to perform action, currently " +
                $"disabled.");
            _remainingDisableTime = 0f;
            _previousRemainingDisableTime = 0f;
            return;
        }
        if (_remainingDisableTime > 0f)
        {
            GD.Print($"{GetParent().Name} unable to perform action, currently " +
                $"in action.");
            return;
        }

        if (_body.IsOnFloor())
        {
            if (_body.Velocity.X == 0f)
            {
                // Perform deflect.
                GD.Print("Performing deflect.");
                DisableHandlers(_aniPlayer.GetAnimation(_deflectAnimation).Length);
                _aniPlayer.Play(_deflectAnimation);
            }
            else
            {
                // Perform punch.
                GD.Print("Performing punch.");
                DisableHandlers(_aniPlayer.GetAnimation(_punchAnimation).Length);
                _body.Velocity = new(0f, _body.Velocity.Y);
                _aniPlayer.Play(_punchAnimation);
            }
        }
        else if (_body.Velocity.X != 0f)
        {
            // Perform dropkick.
            GD.Print("Performing dropkick.");
            DisableHandlers(_aniPlayer.GetAnimation(_dropkickAnimation).Length);
            _aniPlayer.Play(_dropkickAnimation);
        }
    }

    public void OnEnable() => _isDisabled = false;
    public void OnDisable() => _isDisabled = true;
}
