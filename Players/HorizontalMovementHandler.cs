using Godot;

namespace Players;

public partial class HorizontalMovementHandler : Node, IDisableableControl
{
    [Export] CharacterBody2D _body;
    [Export] float _moveSpeed;
    [Export] bool _isEnabled = true;
    [Export] float _airAcceleration = 800f;
    bool _isLeftButtonDown;
    bool _isRightButtonDown;

    public void OnLeftPressed() => _isLeftButtonDown = true;
    public void OnLeftReleased() => _isLeftButtonDown = false;
    public void OnRightPressed() => _isRightButtonDown = true;
    public void OnRightReleased() => _isRightButtonDown = false;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.MOVEMENT; }

    public void SetControlState(bool enabled)
    {
        SetIfEnabled(enabled);
    }
    #endregion

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

        if (GetParent().GetChild<CrouchHandler>().IsCrouched
            && _body.IsOnFloor())
        {
            _body.Velocity = newVel;
            return;
        }

        if (_isLeftButtonDown)
        {
            if (_body.IsOnFloor())
                newVel.X += -_moveSpeed;
            else
            {
                float deltaV = _airAcceleration * (float)delta;
                newVel.X = _body.Velocity.X - deltaV;
                newVel.X = Mathf.Clamp(newVel.X, -_moveSpeed, _moveSpeed);
            }
        }
        if (_isRightButtonDown)
        {
            if (_body.IsOnFloor())
                newVel.X += _moveSpeed;
            else
            {
                float deltaV = _airAcceleration * (float)delta;
                newVel.X = _body.Velocity.X + deltaV;
                newVel.X = Mathf.Clamp(newVel.X, -_moveSpeed, _moveSpeed);
            }
        }
        if (!_isLeftButtonDown && !_isRightButtonDown
            && !_body.IsOnFloor())
        {
            float deltaV = _airAcceleration * (float)delta
                * (_body.Velocity.X < 0f ? 1f : -1f);

            if (Mathf.Abs(deltaV) > Mathf.Abs(_body.Velocity.X))
            {
                newVel.X = 0f;
            }
            else
            {
                newVel.X = _body.Velocity.X + deltaV;
            }
        }

        _body.Velocity = newVel;
    }

    public void SetIfEnabled(bool enabled)
    {
        _isEnabled = enabled;
    }
}
