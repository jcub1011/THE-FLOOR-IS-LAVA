using Godot;
using Godot.MathExtensions;
using System;

namespace Weapons;

public partial class ProjectileGenerator : Node2D
{
    [Export] float _projectileSpeed;
    [Export] float _baseKnockback;
    [Export] float _knockbackMultiplierPerUnitSpeed;
    [Export] PackedScene _projectileTemplate;

    Node2D _projectileOrigin
    {
        get => GetChild<Node2D>(0);
    }

    Node2D _projectileRotationOrigin
    {
        get => GetChild<Node2D>(1);
    }

    Vector2 GetProjectileOutputPosition(Vector2 direction)
    {
        return _projectileRotationOrigin.GlobalPosition
            + _projectileOrigin.GlobalPosition.RelativeTo(_projectileRotationOrigin.GlobalPosition)
            .Rotated(direction.Angle());
    }

    public void CreateProjectile(Vector2 direction)
    {
        // TODO: Implement form of aim assist.
        
    }
}
