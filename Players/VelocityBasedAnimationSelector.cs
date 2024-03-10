using Godot;

namespace Players;

public partial class VelocityBasedAnimationSelector : Node, IDisableableControl
{
    [Export] public bool IsEnabled { get; set; } = true;
    [Export] CharacterBody2D _body;
    [Export] Sprite2D _sprite;
    [Export] AnimationPlayer _player;
    //[Export] CrouchHandler _crouchHandler;
    [Export] FlipHandler _flipHandler;
    [Export] float _walkToRunTransitionThreshold = 300f;
    [ExportGroup("Animation Names")]
    [Export] StringName _idleAnimation = "idle";
    [Export] StringName _walkAnimation = "walk";
    [Export] StringName _runAnimation = "run";
    [Export] StringName _fallAnimation = "fall";
    [Export] StringName _riseAnimation = "rise";
    [Export] StringName _crouch = "crouch";

    #region Interface Implementation
    public string ControlID { get => ControlIDs.AUTO_ANIMATION; }

    public void SetControlState(bool enabled)
    {
        SetIfEnabled(enabled);
    }
    #endregion

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (!IsEnabled) return;
        //else
        //{
        //    _player.PlayIfExists("ball_static");
        //    return;
        //}
        if (_body.Velocity.X != 0f)
        {
            //if (_body.Velocity.X < 0f)
            //{
            //    _flipHandler.OnFaceLeft();
            //}
            //else _flipHandler.OnFaceRight();
        }

        if (_body.IsOnFloor())
        {
            //if (_crouchHandler.IsCrouched)
            //{
            //    _player.PlayIfExists(_crouch);
            //}
            if (_body.Velocity.X == 0f)
            {
                _player.PlayIfExists(_idleAnimation);
            }
            else
            {
                if (Mathf.Abs(_body.Velocity.X) > _walkToRunTransitionThreshold)
                {
                    if (!_player.PlayIfExists(_runAnimation))
                        _player.PlayIfExists(_walkAnimation);
                }
                else
                {
                    if (!_player.PlayIfExists(_walkAnimation))
                        _player.PlayIfExists(_runAnimation);
                }
            }
        }
        else
        {
            if (_body.Velocity.Y > 0f)
            {
                _player.PlayIfExists(_fallAnimation);
            }
            else
            {
                _player.PlayIfExists(_riseAnimation);
            }
        }
    }

    public void SetIfEnabled(bool enabled) => IsEnabled = enabled;
}
