using Godot;
using System.Threading;

namespace Players;

public partial class PlayerController : CharacterBody2D
{
    [Export] PlayerInputHandler InputHandler;
    [Export] public Vector2 SpeedLimit { get; private set; } = new(400, 400);

    public bool IsAlive { get; private set; } = true;

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
        if (IsAlive) MoveAndSlide();
    }

    public void OnTouchedLava(Area2D area)
    {
        GD.Print($"Player {Name} touched lava.");
        IsAlive = false;
        Visible = false;

        foreach (var child in GetChildren())
        {
            if (child is ControlDisablerHandler handler)
            {
                handler.SetControlStates(false, float.PositiveInfinity);

                //var player = GetNode<AnimationPlayer>("AnimationPlayer");
                //player.Play("death");

                //float length = player.GetAnimation("death").Length;

                //var timer = GetTree().CreateTimer(length, false);
                //timer.Timeout += () => {
                //    GD.Print("Death animation ended.");
                //    Visible = false;
                //};
                break;
            }
        }

        //Visible = false;
    }

    public void OnStart()
    {
        Visible = true;
        IsAlive = true;

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
