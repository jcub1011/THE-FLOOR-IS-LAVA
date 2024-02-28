using Godot;

namespace Players;

internal class CoyoteTimer
{
    float _coyoteTime;
    float _remainingCoyoteTime;
    bool _isCurrentlyOnGround;

    public CoyoteTimer(float coyoteTime)
    {
        _coyoteTime = coyoteTime;
    }

    public void UpdateTimer(float deltaTime, bool isOnGround)
    {
        _remainingCoyoteTime -= deltaTime;
        _isCurrentlyOnGround = isOnGround;
        if (_isCurrentlyOnGround)
        {
            _remainingCoyoteTime = _coyoteTime;
        }
    }

    public bool IsOnGround()
    {
        return _isCurrentlyOnGround || _remainingCoyoteTime > 0f;
    }

    public void ConsumeCoyoteTime()
    {
        _remainingCoyoteTime = 0f;
    }
}

public partial class JumpHandler : Node, IDisableableControl
{
    [Export] CharacterBody2D _body;
    [Export] float _jumpVelocity;
    [Export] float _maxJumpTime;
    [Export] bool _isEnabled = true;
    [Export] float _coyoteTime = 0.05f;
    [Export] float _jumpBuffer = 0.1f;
    bool _isJumpPressed;
    float _remainingJumpTime;
    CoyoteTimer _coyoteTimer;

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
        _coyoteTimer = new(_coyoteTime);
    }

    public void OnJumpPressed()
    {
        _isJumpPressed = true;
        InputBuffer.BufferInput(_body, InputNames.JUMP);
    }
    public void OnJumpReleased() => _isJumpPressed = false;

    public override void _Process(double delta)
    {
        base._Process(delta);
        _coyoteTimer.UpdateTimer((float)delta, _body.IsOnFloor());

        _remainingJumpTime -= (float)delta;
        if (!_isEnabled) return;
        if (_isJumpPressed)
        {
            if (_coyoteTimer.IsOnGround())
            {
                _remainingJumpTime = _maxJumpTime;
                _coyoteTimer.ConsumeCoyoteTime();
            }
            if (_remainingJumpTime > 0f)
            {
                _body.Velocity = new(_body.Velocity.X, -_jumpVelocity);
            }
        }
        else if (InputBuffer.IsBuffered(_body, InputNames.JUMP, _jumpBuffer)
            && _coyoteTimer.IsOnGround())
        {
            _coyoteTimer.ConsumeCoyoteTime();
            InputBuffer.ConsumeBuffer(_body, InputNames.JUMP);
            _body.Velocity = new(_body.Velocity.X, -_jumpVelocity);
        }
    }

    public void SetIfEnabled(bool enabled)
    {
        _isEnabled = enabled;
    }
}
