using Godot;
using System;

namespace WorldGeneration;

public partial class LevelGenerator : Node2D
{
    [Export] float _startDelay = 5f;
    [Export] Camera2D _camera;

    public float? _worldBottomY;
    public float WorldBottomY
    {
        get
        {
            if (_worldBottomY == null) return float.PositiveInfinity;
            else return _worldBottomY.Value;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        _worldBottomY = GetWorldBottomY();
    }

    float GetWorldBottomY()
    {
        float cameraPos = _camera.GetScreenCenterPosition().Y;
        var cameraRect =
            GetCanvasTransform().AffineInverse().BasisXform(GetViewportRect().Size);
        float returnVal = cameraPos + cameraRect.Y / 2;
        GD.Print($"World bottom = {returnVal}");
        return returnVal;
    }
}
