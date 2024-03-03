using Godot;
using System;

namespace UI;

public partial class SizeLimiter : Container
{
    [Export] Vector2 MaxSize;

    bool IsValidMaxWidth()
    {
        return MaxSize.X - OffsetLeft > 0 && MaxSize.X - OffsetLeft > CustomMinimumSize.X;
    }

    bool IsValidMaxHeight()
    {
        return MaxSize.Y > 0 && MaxSize.Y > CustomMinimumSize.Y;
    }

    void LimitSize()
    {
        Vector2 newSize = Size;
        if (IsValidMaxHeight() && Size.Y > MaxSize.Y)
        {
            newSize.Y = MaxSize.Y;
            //SetAnchorAndOffset(Side.Bottom, AnchorBottom, MaxSize.Y);
        }

        if (IsValidMaxWidth() && Size.X > MaxSize.X)
        {
            float scale = Size.X / MaxSize.X;
            newSize.Y *= scale;
            OffsetRight = MaxSize.X + OffsetLeft;
            //SetAnchorAndOffset(Side.Right, AnchorRight, MaxSize.X);
        }
        //SetSize(newSize);
    }

    public override void _Draw()
    {
        base._Draw();
        LimitSize();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        LimitSize();
    }
}
