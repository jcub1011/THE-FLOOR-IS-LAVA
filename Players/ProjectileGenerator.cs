using Godot;
using Players;
using System;

namespace Weapons;

public class InputToDirectionHandler
{
    bool _leftPressed;
    bool _rightPressed;
    bool _upPressed;
    bool _downPressed;

    public void UpdateDirection(StringName input, bool pressed)
    {
        if (input == InputNames.LEFT) _leftPressed = pressed;
        else if (input == InputNames.RIGHT) _rightPressed = pressed;
        else if (input == InputNames.JUMP) _upPressed = pressed;
        else if (input == InputNames.CROUCH) _downPressed = pressed;
    }

    public Vector2 GetDirection()
    {
        Vector2 direction = new();

        if (_leftPressed) direction.X--;
        if (_rightPressed) direction.X++;
        if (_upPressed) direction.Y--;
        if (_downPressed) direction.Y++;

        return direction.LengthSquared() == 0f ? Vector2.Zero : direction.Normalized();
    }
}

public partial class ProjectileGenerator : Node2D, IDisableableControl
{
    public string ControlID => ControlIDs.PROJECTILE_GENERATOR;
    bool _enabled = true;
    InputToDirectionHandler _directionHandler;
    PlayerController _body { get => GetParent<PlayerController>(); }
    [Export] float _projectileSpeed = 400f;
    [Export] float _recoil = 200f;
    [Export] PackedScene _projectile;
    [Export] ControlDisablerHandler _disabler;

    [Signal] public delegate void ProjectileCreatedEventHandler(Node projectile);

    public override void _Ready()
    {
        base._Ready();
        _directionHandler = new InputToDirectionHandler();
    }

    public void SetControlState(bool enabled)
    {
        _enabled = enabled;
    }

    public void SetFlipState(bool flip)
    {
        if (flip != Scale.X < 0f) Scale = new(Scale.X * -1f, Scale.Y);
    }

    public void InputHandler(StringName input, bool pressed)
    {
        _directionHandler.UpdateDirection(input, pressed);
    }

    public void LaunchProjectile()
    {
        if (!_enabled) return;
        var newProjectile = _projectile.Instantiate<Projectile>();
        var projDir = _directionHandler.GetDirection();

        if (projDir == Vector2.Zero)
        {
            projDir = Scale.X < 0f ? Vector2.Left : Vector2.Right;
        }
        newProjectile.SetVelocity(projDir * _projectileSpeed);
        newProjectile.GlobalPosition = GetChild<Node2D>(0).GlobalPosition;
        GD.Print("Launching projectile.");

        _body.Velocity = -projDir.Normalized() * _recoil;

        EmitSignal(SignalName.ProjectileCreated, newProjectile);

        _disabler.SetControlStatesExcept(
            false,
            0.1f,
            ControlIDs.GRAVITY);
    }
}
