using Godot;
using System;

namespace Players;

public partial class SpriteControls : Sprite2D
{
    public void SetFlipH(bool flipH) => FlipH = flipH;
}
