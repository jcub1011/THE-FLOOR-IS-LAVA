using Godot;
using System;
using System.Collections.Generic;

namespace Players;

public partial class MeleeHurtboxHandler : Area2D, IDisableableControl
{
    [Signal] public delegate void OnHurtboxDeflectedEventHandler(CharacterBody2D deflector, float knockback);

    /// <summary>
    /// Defaults to length of frame @ 24fps.
    /// </summary>
    [Export] float _defaultHitboxLifeTime = 0.04f;
    bool _hasActiveHitboxes;
    public float _remainingHitboxTime;
    CollisionShape2D _activeCollider;
    bool _isFlipped = false;
    public Node2D HurtboxOwner
    {
        get => GetParent<CharacterBody2D>();
    }
    public float Knockback { get; private set; }

    #region Interface Implementation
    public string ControlID { get => ControlIDs.HURTBOX; }

    public void SetControlState(bool enabled)
    {
        if (!enabled) {
            DisableHitboxes();
        }
    }
    #endregion

    public override void _Ready()
    {
        base._Ready();
        DisableHitboxes();
    }

    public void EnableHitbox(StringName hitboxName, float duration, float knockback)
    {
        Knockback = knockback;
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
        _isFlipped = flipState;

        Scale = new(Scale.X * -1f, Scale.Y);
    }

    public void HandleAttackDeflected(CharacterBody2D deflector, float knockback)
    {
        GD.Print($"{HurtboxOwner.Name} emitting attack deflected.");
        EmitSignal(SignalName.OnHurtboxDeflected, deflector, knockback);
    }

    public void OnLeftPressed() => SetFlipState(true);
    public void OnRightPressed() => SetFlipState(false);
}
