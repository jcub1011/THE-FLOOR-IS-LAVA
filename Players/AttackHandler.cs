using Godot;
using Physics;
using System;

namespace Players;

public partial class AttackHandler : Node
{
    [Export] CharacterBody2D _body;
    [Export] HorizontalMovementHandler _movementHandler;
    [Export] JumpHandler _jumpHandler;
    [Export] VelocityBasedAnimationSelector _velocityAnimationSelector;
    [Export] GravityApplicator _gravityApplicator;
    [Export] float _dropkickDrag;

    float _remainingDisableTime;
    float _previousRemainingDisableTime;

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
            GD.Print("Re-enabling handlers.");
            _movementHandler.SetIfEnabled(true);
            _jumpHandler.SetIfEnabled(true);
            _velocityAnimationSelector.IsEnabled = true;
        }
    }

    void EnableHandlers()
    {
        GD.Print("Re-enabling handlers.");
        _movementHandler.SetIfEnabled(true);
        _jumpHandler.SetIfEnabled(true);
        _velocityAnimationSelector.IsEnabled = true;
    }

    void DisableHandlers(float time)
    {
        _remainingDisableTime = time;
        GD.Print($"Disabling handlers for {_remainingDisableTime}s.");
        _movementHandler.SetIfEnabled(false);
        _jumpHandler.SetIfEnabled(false);
        _velocityAnimationSelector.IsEnabled = false;
    }

    bool JustEnabled()
    {
        return _previousRemainingDisableTime > 0f && _remainingDisableTime <= 0f;
    }

    public void OnAction()
    {
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
                DisableHandlers(0.5f);
            }
            else
            {
                // Perform punch.
                GD.Print("Performing punch.");
                DisableHandlers(1f);
                _body.Velocity = new(0f, _body.Velocity.Y);
            }
        }
        else if (_body.Velocity.X != 0f)
        {
            // Perform dropkick.
            GD.Print("Performing dropkick.");
            DisableHandlers(1f);
        }
    }
}
