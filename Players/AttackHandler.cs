using Godot;
using System;

namespace Players;

public partial class AttackHandler : Node
{
    [Export] CharacterBody2D _body;
    [Export] HorizontalMovementHandler _movementHandler;
    [Export] JumpHandler _jumpHandler;
    [Export] VelocityBasedAnimationSelector _velocityAnimationSelector;

    float _remainingDisableTime;
    float _previousRemainingDisableTime;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _previousRemainingDisableTime = _remainingDisableTime;
        _remainingDisableTime -= (float)delta;
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
            }
            else
            {
                // Perform punch.
                GD.Print("Performing punch.");
            }
        }
        else if (_body.Velocity.X != 0f)
        {
            // Perform dropkick.
            GD.Print("Performing dropkick.");
        }
    }
}
