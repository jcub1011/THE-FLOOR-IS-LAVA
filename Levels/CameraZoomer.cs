using Godot;
using Godot.GodotExtensions;
using System;

namespace WorldGeneration;

public partial class CameraZoomer : Camera2D
{
    Vector2 _targetSize;
    Vector2 _initialZoom;
    [Export] bool _keepAspect = true;

    public override void _Ready()
    {
        base._Ready();
        _targetSize = GodotExtensions.GetViewportSize();
        _initialZoom = Zoom;

        GD.Print(_targetSize);
        //AdjustZoom();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        //AdjustZoom();
    }

    void AdjustZoom()
    {
        Vector2 display = DisplayServer.WindowGetSize();
        float scaleX = display.X / _targetSize.X;
        float scaleY = display.Y / _targetSize.Y;

        if (_keepAspect)
        {
            float targetScale = Math.Min(scaleX, scaleY);
            Zoom = new(_initialZoom.X * targetScale,
                _initialZoom.Y * targetScale);
        }
        else
        {
            Zoom = new(_initialZoom.X * scaleX, _initialZoom.Y * scaleY);
        }
    }
}
