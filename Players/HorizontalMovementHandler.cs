using Godot;

namespace Players;

public partial class HorizontalMovementHandler : Node, IDisableableControl
{
    [Export] CharacterBody2D _body;
    [Export] float _moveSpeed = 80;
    [Export] bool _isEnabled = true;
    [Export] float _groundAcceleration = 1200f;
    [Export] float _airAcceleration = 900f;
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
        Vector2 newVel = _body.Velocity;
        float speedLim = (_body as PlayerController).SpeedLimit.X;

        float deltaX = _body.IsOnFloor() ?
            _groundAcceleration : _airAcceleration;
        deltaX *= (float)delta;

        if (_isLeftButtonDown == _isRightButtonDown
            || GetParent().GetChild<CrouchHandler>().IsCrouched)
        {
            if (deltaX > Mathf.Abs(_body.Velocity.X))
            {
                newVel.X = 0f;
            }
            else
            {
                newVel.X += _body.Velocity.X < 0f ? deltaX : -deltaX;
            }
        }
        else if (_isLeftButtonDown)
        {
            if (newVel.X > -_moveSpeed)
            {
                newVel.X = Mathf.Clamp(newVel.X - deltaX,
                    -_moveSpeed, _moveSpeed);
            }
            else
            {
                newVel.X = Mathf.Clamp(newVel.X + deltaX,
                    float.NegativeInfinity, _moveSpeed);
            }
        }
        else
        {
            if (newVel.X < _moveSpeed)
            {
                newVel.X = Mathf.Clamp(newVel.X + deltaX,
                    -_moveSpeed, _moveSpeed);
            }
            else
            {
                newVel.X = Mathf.Clamp(newVel.X - deltaX,
                    -_moveSpeed, float.PositiveInfinity);
            }
        }

        newVel.X = Mathf.Clamp(newVel.X, -speedLim, speedLim);
        _body.Velocity = newVel;
    }

    public void SetIfEnabled(bool enabled)
    {
        _isEnabled = enabled;
    }
}
