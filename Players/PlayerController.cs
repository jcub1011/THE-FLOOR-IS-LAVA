using Godot;
using System.Threading;

namespace Players;

public partial class PlayerController : CharacterBody2D
{
    [Export] PlayerInputHandler InputHandler;
    [Export] public Vector2 SpeedLimit { get; private set; } = new(400, 400);

    bool _isAlive = true;

    public override void _Ready()
    {
        base._Ready();

    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Vector2 limitedVelocity = Velocity;
        limitedVelocity.X = Mathf.Clamp(Velocity.X, -SpeedLimit.X, SpeedLimit.X);
        limitedVelocity.Y = Mathf.Clamp(Velocity.Y, -SpeedLimit.Y, SpeedLimit.Y);
        Velocity = limitedVelocity;
        if (_isAlive) MoveAndSlide();
    }

    public void OnTouchedLava(Area2D area)
    {
        GD.Print($"Player {Name} touched lava.");
        _isAlive = false;

        foreach (var child in GetChildren())
        {
            if (child is ControlDisablerHandler handler)
            {
                handler.SetControlStates(false, float.PositiveInfinity);

                var player = GetNode<AnimationPlayer>("AnimationPlayer");
                player.Play("death");

                float length = player.GetAnimation("death").Length;

                var timer = GetTree().CreateTimer(length, false);
                timer.Timeout += () => {
                    GD.Print("Death animation ended.");
                    Visible = false;
                };
                break;
            }
        }
    }

    public void OnStart()
    {
        Visible = true;
        _isAlive = true;

        foreach (var child in GetChildren())
        {
            if (child is ControlDisablerHandler handler)
            {
                handler.SetControlStates(true, float.PositiveInfinity);
                break;
            }
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        InputHandler.ReleaseInput();
    }
}
