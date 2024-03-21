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
            var collisionInfo = MoveAndCollide(Velocity * (float)delta);

            if (collisionInfo != null)
            {
                Velocity = Velocity.Bounce(collisionInfo.GetNormal()) * 0.3f;
            }
            else GD.Print("Not bouncing");
        }
        else MoveAndSlide();
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
}
