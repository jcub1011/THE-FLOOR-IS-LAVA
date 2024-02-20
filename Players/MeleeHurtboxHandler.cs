using Godot;
using System;
using System.Collections.Generic;

namespace Players;

public partial class MeleeHurtboxHandler : Area2D
{
    /// <summary>
    /// Defaults to length of frame @ 24fps.
    /// </summary>
    [Export] float _defaultHitboxLifeTime = 0.04f;
    [Export] bool _flippingEnabled = true;
    bool _hasActiveHitboxes;
    public float _remainingHitboxTime;
    CollisionShape2D _activeCollider;
    bool _isFlipped = false;
    bool? _cachedFlip;
    public Node2D HurtboxOwner
    {
        get => GetParent<CharacterBody2D>();
    }
    public float Knockback { get; private set; }

    public override void _Ready()
    {
        base._Ready();
        DisableHitboxes();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (_flippingEnabled && _cachedFlip != null)
        {
            SetFlipState(_cachedFlip.Value);
            _cachedFlip = null;
        }
    }

    public void EnableHitbox(StringName hitboxName, float duration, float knockback)
    {
        foreach (var child in GetChildren())
        {
            if (child is TimedCollider collider)
            {
                if (collider.Name != hitboxName)
                {
                    collider.ForceDisableCollider();
                }
                else
                {
                    collider.EnableCollider(duration);
                }
            }
        }
    }

    void DisableHitboxes()
    {
        foreach(var child in GetChildren())
        {
            if (child is TimedCollider timedCollider)
            {
                timedCollider.ForceDisableCollider();
            }
            else if (child is CollisionShape2D collider)
            {
                collider.Disabled = true;
            }
        }
        _activeCollider = null;
    }

    public void SetFlipState(bool flipState)
    {
        if (_isFlipped == flipState) return;

        if (!_flippingEnabled)
        {
            _cachedFlip = flipState;
            return;
        }

        _isFlipped = flipState;

        Scale = new(Scale.X * -1f, Scale.Y);
    }

    public void OnLeftPressed() => SetFlipState(true);
    public void OnRightPressed() => SetFlipState(false);
    public void EnableFlipping() => _flippingEnabled = true;
    public void DisableFlipping() => _flippingEnabled = false;
}
