using Godot;
using Godot.NodeExtensions;
using System.Linq;
using TheFloorIsLava.Subscriptions;
using UI;
using WorldGeneration;

namespace Players;

public partial class PlayerController : CharacterBody2D
{
    [Export] public Vector2 SpeedLimitInTiles { get; private set; } = new(50, 50);
    float _remainingBounceTime;

    bool _isAlive = true;
    public bool IsAlive
    {
        get => _isAlive;
        set
        {
            _isAlive = value;
            PlayerUtilityFlags.UpdatePlayerLivingState(this);
        }
    }

    public override void _Ready()
    {
        base._Ready();
        OriginShiftChannel.OriginShifted += OriginShifted;
        PlayerInputHandler.SetDevice(this.GetDirectChild<PlayerInputHandler>(), new(DeviceType.KeyboardLeft, 0));
        PauseEventChannel.SetPauseState(false);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!IsAlive) return;
        _remainingBounceTime -= (float)delta;

        LimitVelocity();

        if (_remainingBounceTime > 0f)
        {
            ApplyBounceIfApplicable((float)delta);
            if (IsOnFloor()) ApplyFriction((float)delta);
        }
        
        MoveAndSlide();
    }

    void LimitVelocity()
    {
        Vector2 newVel = Velocity;
        Vector2 speedLim = SpeedLimitInTiles.ToPixels();

        newVel.X = Mathf.Clamp(Velocity.X, -speedLim.X, speedLim.X);
        newVel.Y = Mathf.Clamp(Velocity.Y, -speedLim.Y, speedLim.Y);
        Velocity = newVel;
    }

    void ApplyBounceIfApplicable(float delta)
    {
        var collisionInfo = MoveAndCollide(Velocity * delta, true);

        if (collisionInfo != null)
        {
            Vector2 normal = collisionInfo.GetNormal();
            float collisionAngle = normal.AngleTo(Velocity) * 180f / Mathf.Pi;
            float fixedCollisionAngle = Mathf.Abs((collisionAngle - 90) % 180);
            //GD.Print($"Collision angle: {fixedCollisionAngle} deg");
            if (fixedCollisionAngle > 30f)
                Velocity = Velocity.Bounce(normal) * 0.8f;
        }
    }

    void OriginShifted(Vector2 shift) => Position += shift;

    public void OnTouchedLava(Area2D area)
    {
        GD.Print($"Player {Name} touched lava.");
        IsAlive = false;
        Visible = false;

        this.GetDirectChild<ControlDisablerHandler>().DisableControlsExcept(float.PositiveInfinity);
    }

    public void OnStart()
    {
        Visible = true;
        IsAlive = true;

        this.GetDirectChild<ControlDisablerHandler>().EnableControls();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        this.GetChild<PlayerInputHandler>().ReleaseInput();
        OriginShiftChannel.OriginShifted -= OriginShifted;
    }

    public void EnableBouncing(float duration)
    {
        _remainingBounceTime = duration;
    }

    void ApplyFriction(float deltaTime)
    {
        float speed = Velocity.Length();
        Velocity -= Velocity.Normalized() 
            * Mathf.Clamp(speed * 5f * (float)deltaTime, -speed, speed);
    }
}
