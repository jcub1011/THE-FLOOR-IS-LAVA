using Godot;
using System;

namespace Players;

public partial class TimedCollider : CollisionShape2D
{
    float _remainingLifeTime;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _remainingLifeTime -= (float)delta;
        Disabled = _remainingLifeTime <= 0f;
    }

    public void EnableCollider(float timeToLive)
    {
        _remainingLifeTime = timeToLive;
        Disabled = false;
    }
}
