using Godot;
using Godot.GodotExtensions;
using Godot.MathExtensions;
using System;
using WorldGeneration;

namespace TheFloorIsLava.Camera;

public partial class NewWorldGeneratorCamera : Camera2D
{
    Node2D _target;
    Vector2 _targetZoom = Vector2.Zero;
    /// <summary>
    /// Tiles per second.
    /// </summary>
    float _maxSpeed = 10;
    Vector2 _velocity = Vector2.Zero;
    float _zoomVel = 0f;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="focusBox"></param>
    /// <param name="padding">Minimum tiles of space in the x and y direction.</param>
    public void SetFocusTarget(CollisionShape2D focusBox, Vector2 padding)
    {
        if (focusBox.Shape is not RectangleShape2D focusRect)
        {
            GD.PushError($"Focus box must be a {nameof(RectangleShape2D)}.");
            return;
        }

        _target = focusBox;

        Vector2 viewport = GodotExtensions.GetViewportSize();

        _targetZoom = new Vector2(1f, 1f) * Mathf.Min(
            viewport.X / (focusRect.Size.X + 2 * padding.X.ToPixels()),
            viewport.Y / (focusRect.Size.Y + 2 * padding.Y.ToPixels())
            );
    }

    void SmoothDampToTarget(float delta)
    {
        GlobalPosition = GlobalPosition.SmoothDamp(_target.Position, ref _velocity, 0.5f, (float)delta);
        float smoothed = Zoom.X.SmoothDamp(_targetZoom.X, ref _zoomVel, 0.5f, (float)delta);
        Zoom = new Vector2(smoothed, smoothed);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_target == null || _targetZoom == Vector2.Zero) return;
        SmoothDampToTarget((float)delta);
    }
}
