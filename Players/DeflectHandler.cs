using Godot;
using System;

namespace Players;

public partial class DeflectHandler : Node
{
    [Export] Sprite2D _sprite;
    float _remainingDeflectTime;

    public void EnableDeflect(float duration)
    {
        _remainingDeflectTime = duration;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _remainingDeflectTime -= (float)delta;
    }
}
