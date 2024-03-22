using Godot;
using Godot.NodeExtensions;
using TheFloorIsLava.Subscriptions;

namespace Players;

public partial class PlayerController : CharacterBody2D
{
    [Export] PlayerInputHandler InputHandler;
    [Export] public Vector2 SpeedLimit { get; private set; } = new(400, 400);
    float _remainingBounceTime;

    public bool IsAlive { get; private set; } = true;

    public override void _Ready()
    {
        base._Ready();
        OriginShiftChannel.OriginShifted += OriginShifted;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!IsAlive) return;
        _remainingBounceTime -= (float)delta;

        Vector2 limitedVelocity = Velocity;
        limitedVelocity.X = Mathf.Clamp(Velocity.X, -SpeedLimit.X, SpeedLimit.X);
        limitedVelocity.Y = Mathf.Clamp(Velocity.Y, -SpeedLimit.Y, SpeedLimit.Y);
        Velocity = limitedVelocity;

        if (_remainingBounceTime > 0f)
        {
            var collisionInfo = MoveAndCollide(Velocity * (float)delta, true);

            if (collisionInfo != null)
            {
                Vector2 normal = collisionInfo.GetNormal();
                float collisionAngle = normal.AngleTo(Velocity) * 180f / Mathf.Pi;
                float fixedCollisionAngle = Mathf.Abs((collisionAngle - 90) % 180);
                GD.Print($"Collision angle: {fixedCollisionAngle} deg");
                if (fixedCollisionAngle > 30f)
                    Velocity = Velocity.Bounce(normal) * 0.8f;
            }

            if (IsOnFloor()) ApplyFriction((float)delta);
        }
        
        MoveAndSlide();
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
        InputHandler.ReleaseInput();
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
