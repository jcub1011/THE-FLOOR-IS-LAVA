using Godot;
using System;

namespace Players;

public partial class HorizontalMovementHandler : Node
{
    [Export] PlayerController _body;
    [Export] float _moveSpeed;
    bool _isLeftButtonDown;
    bool _isRightButtonDown;

    public void OnLeftPressed() => _isLeftButtonDown = true;
    public void OnLeftReleased() => _isLeftButtonDown = false;
    public void OnRightPressed() => _isRightButtonDown = true;
    public void OnRightReleased() => _isRightButtonDown = false;

    public override void _Process(double delta)
    {
        base._Process(delta);
        Vector2 newVel = new(0, _body.Velocity.Y);

        if (_isLeftButtonDown)
        {
            newVel.X += -_moveSpeed;
        }
        if (_isRightButtonDown)
        {
            newVel.X += _moveSpeed;
        }

        _body.Velocity = newVel;
    }
}
