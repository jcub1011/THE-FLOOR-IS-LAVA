using Godot;
using System;

namespace Players;

public partial class TimedCollider : CollisionShape2D
{
    public MeleeHurtboxHandler ColliderOwner
    {
        get => GetParent<MeleeHurtboxHandler>();
    }
    float _remainingLifeTime;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _remainingLifeTime -= (float)delta;
        SetDeferred("disabled", _remainingLifeTime <= 0f);
    }

    public void EnableCollider(float timeToLive)
    {
        _remainingLifeTime = timeToLive;
        SetDeferred("disabled", false);
    }

    public void ForceDisableCollider()
    {
        _remainingLifeTime = 0f;
        SetDeferred("disabled", true);
    }
}
