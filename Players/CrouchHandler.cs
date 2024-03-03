using Godot;
using System;

namespace Players;

public partial class CrouchHandler : Node
{
    public bool IsCrouched { get; private set; }

    public void OnCrouchStart()
    {
        IsCrouched = true;
    }

    public void OnCrouchEnd()
    {
        IsCrouched = false;
    }
}
