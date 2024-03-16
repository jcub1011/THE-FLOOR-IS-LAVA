using Godot;
using Players;
using System;

namespace Weapons;

public partial class ProjectileGenerator : Node2D, IDisableableControl
{
    public string ControlID => ControlIDs.PROJECTILE_GENERATOR;
    bool _enabled = false;

    public void SetControlState(bool enabled)
    {
        _enabled = enabled;
    }

    public void SetFlipState(bool flip)
    {
        if (flip != Scale.X < 0f) Scale = new(Scale.X * -1f, Scale.Y);
    }
}
