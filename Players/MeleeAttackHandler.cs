using Godot;
using System;
using System.Collections.Generic;

namespace Players;

public partial class MeleeAttackHandler : Area2D
{
    /// <summary>
    /// Defaults to length of frame @ 24fps.
    /// </summary>
    [Export] float _defaultHitboxLifeTime = 0.04f;
    bool _hasActiveHitboxes;
    public float _remainingHitboxTime;
    CollisionShape2D _activeCollider;
    bool _isFlipped = false;

    public override void _Ready()
    {
        base._Ready();
    }

    public void OnHitboxEnable(StringName hitboxName)
    {
        EnableHitbox(hitboxName);
    }

    void EnableHitbox(StringName hitboxName)
    {
        foreach (var child in GetChildren())
        {
            if (child is CollisionShape2D collider)
            {
                collider.Disabled = !(collider.Name == hitboxName);
                if (!collider.Disabled) _activeCollider = collider;
            }
        }
    }

    void DisableHitboxes()
    {
        foreach(var child in GetChildren())
        {
            if (child is CollisionShape2D collider)
            {
                collider.Disabled = true;
            }
        }
        _activeCollider = null;
    }

    public void SetFlipState(bool flipState)
    {
        if (_isFlipped == flipState) return;
        else _isFlipped = flipState;

        Scale = new(Scale.X * -1f, Scale.Y);
    }
}
