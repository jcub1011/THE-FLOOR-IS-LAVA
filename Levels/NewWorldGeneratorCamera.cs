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
    Vector2 _startPos = Vector2.Zero;
    float _startZoom = 1f;
    float _timeSinceFocusStart = 0f;
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
        if (_target == focusBox) return;
        if (focusBox.Shape is not RectangleShape2D focusRect)
        {
            GD.PushError($"Focus box must be a {nameof(RectangleShape2D)}.");
            return;
        }

        _target = focusBox;
        _startPos = GlobalPosition;
        _startZoom = Zoom.X;
        _timeSinceFocusStart = 0f;

        Vector2 viewport = GodotExtensions.GetViewportSize();

        _targetZoom = new Vector2(1f, 1f) * Mathf.Min(
            viewport.X / (focusRect.Size.X + 2 * padding.X.ToPixels()),
            viewport.Y / (focusRect.Size.Y + 2 * padding.Y.ToPixels())
            );
    }

    void SmoothDampToTarget(float delta)
    {
        _timeSinceFocusStart += delta;
        GlobalPosition = _startPos.EaseOutQuintToTarget(Mathf.Clamp((_timeSinceFocusStart / 1f), 0f, 1f), _target.GlobalPosition);
        float smoothed = _startZoom.EaseOutToTarget(Mathf.Clamp((_timeSinceFocusStart / 1f), 0f, 1f), _targetZoom.X);
        //float smoothed = Zoom.X.SmoothDamp(_targetZoom.X, ref _zoomVel, 0.1f, (float)delta);
        Zoom = new Vector2(smoothed, smoothed);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_target == null || _targetZoom == Vector2.Zero) return;
        SmoothDampToTarget((float)delta);
    }
}
