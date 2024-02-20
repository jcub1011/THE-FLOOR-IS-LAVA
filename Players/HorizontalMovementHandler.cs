using Godot;
using System;

namespace Players;

public partial class HorizontalMovementHandler : Node
{
    [Export] CharacterBody2D _body;
    [Export] float _moveSpeed;
    [Export] bool _isEnabled = true;
    bool _isLeftButtonDown;
    bool _isRightButtonDown;

    public void OnLeftPressed() => _isLeftButtonDown = true;
    public void OnLeftReleased() => _isLeftButtonDown = false;
    public void OnRightPressed() => _isRightButtonDown = true;
    public void OnRightReleased() => _isRightButtonDown = false;

    public override void _Ready()
    {
        base._Ready();
        _body ??= GetParent() as CharacterBody2D;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!_isEnabled) return;
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

    public void SetIfEnabled(bool enabled)
    {
        _isEnabled = enabled;
    }
}
