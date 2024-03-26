using Godot;
using Godot.NodeExtensions;
using Godot.MathExtensions;
using Godot.GodotExtensions;
using System.Collections.Generic;
using System.Linq;
using TheFloorIsLava.Subscriptions;

namespace WorldGeneration;

public readonly struct FocusBox
{
    public readonly Vector2 FocusPoint;
    public readonly Vector2 Size;

    public FocusBox(Vector2 focusPoint, Vector2 size)
    {
        FocusPoint = focusPoint;
        Size = size;
    }

    public float TopY() => FocusPoint.Y - Size.Y / 2f;
    public float BottomY() => FocusPoint.Y + Size.Y / 2f;

    public override string ToString()
    {
        return $"{FocusPoint}: {Size.X} x {Size.Y}";
    }
}

public partial class CameraSimulator : Node
{
    [Export] Camera2D _camera;
    [Export] double _maxSpeed = 9600;
    [Export] float _paddingPercent = 0.5f;
    [Export] float _lookAhead = 20f;
    [Export] float _lookBehind = 10f;
    [Export] Vector2 _minCameraZoom = new(0.1f, 0.1f);
    [Export] Vector2 _maxCameraZoom = new(0.222f, 0.222f);
    [Export] float _maxZoomSpeed = 5f;
    [Export] bool _ignoreTimeScale = true;

    public FocusBox FocusBox { get; private set; }

    float _canvasTotalUnits = GodotExtensions.GetViewportSize().Y;

    public override void _Ready()
    {
        base._Ready();
    }

    public float GetCameraUpperY()
    {
        return _camera.GetScreenCenterPosition().Y 
            - GodotExtensions.GetViewportSize().Y / _camera.Zoom.Y / 2f;
    }

    public float GetCameraLowerY()
    {
        return _camera.GetScreenCenterPosition().Y
            + GodotExtensions.GetViewportSize().Y / _camera.Zoom.Y / 2f;
    }

    /// <summary>
    /// Objects are moved then focus points. Make sure focus points are not 
    /// in the objects list.
    /// </summary>
    /// <param name="objects">2D objects in the scene that don't follow the camera.</param>
    /// <param name="focusPoints">2D objects in the scene the camera must keep visible.</param>
    /// <param name="deltaTime"></param>
    /// <param name="additionalItems">Any additional 2D objects that must be moved.</param>
    public void UpdateCamera(
        IEnumerable<Node2D> focusPoints,
        double deltaTime)
    {
        //GD.Print($"Delta Time: {deltaTime}, True Delta Time: {deltaTime / Engine.TimeScale}, Time Scale {Engine.TimeScale}");
        if (_ignoreTimeScale) deltaTime /= Engine.TimeScale;

        var focusBox = GetFocusBoundingBox(focusPoints);
        FocusBox = focusBox;
        _camera.Zoom = GetNewZoom(focusBox, _minCameraZoom.Y, _maxCameraZoom.Y, _lookAhead.ToPixels(), _lookBehind.ToPixels(), (float)deltaTime);

        float dist = -GetNormDistFromCamCenter(focusBox.BottomY() + _lookBehind.ToPixels() - GetCameraLowerY());

        Vector2 deltaPos = new(0f, (float)_maxSpeed.ToPixels() * dist * (float)deltaTime);
        OriginShiftChannel.ShiftOrigin(deltaPos);
    }

    Vector2 GetFocusAvgPos(IEnumerable<Node2D> focusPoints)
    {
        Vector2 sum = new();
        foreach(var node in focusPoints)
        {
            if (!IsInstanceValid(node)) continue;
            sum += node.GlobalPosition;
        }
        return sum / focusPoints.Count();
    }

    Vector2 GetFocusBox(IEnumerable<Node2D> focusPoints)
    {
        float minY = float.PositiveInfinity;
        float maxY = float.NegativeInfinity;
        float minX = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;

        foreach (var node in focusPoints)
        {
            if (!IsInstanceValid(node)) continue;
            if (node.GlobalPosition.Y < minY) minY = node.GlobalPosition.Y;
            if (node.GlobalPosition.Y > maxY) maxY = node.GlobalPosition.Y;
            if (node.GlobalPosition.X < minX) minX = node.GlobalPosition.X;
            if (node.GlobalPosition.X > maxX) maxX = node.GlobalPosition.X;
        }

        if (float.IsInfinity(minY)) return Vector2.Zero;
        else return new(Mathf.Abs(maxX - minX), Mathf.Abs(maxY - minY));
    }

    FocusBox GetFocusBoundingBox(IEnumerable<Node2D> focusPoints)
    {
        FocusBox box = new(GetFocusAvgPos(focusPoints), GetFocusBox(focusPoints));
        return box;
    }

    float GetNormDistFromCamCenter(float yPos)
    {
        return MathExtensions.GetNormalizedValInRange(yPos, GetCameraUpperY(), GetCameraLowerY());
    }

    float GetCameraYCoverage(float cameraPaddingPercent)
    {
        float coverage = Mathf.Abs(GetCameraLowerY() - GetCameraUpperY());
        return coverage;
    }

    Vector2 GetNewZoom(FocusBox focusBox, float minZoom, float maxZoom,
        float topPadding, float bottomPadding, float deltaTime)
    {
        float padding = topPadding + bottomPadding;
        float coverage = Mathf.Abs(focusBox.TopY() - focusBox.BottomY()) + padding;
        if (coverage <= 0) coverage = 1;

        float requiredZoom = _canvasTotalUnits / coverage;

        requiredZoom = Mathf.Clamp(requiredZoom, minZoom, maxZoom);
        float zoomDifference = requiredZoom - _camera.Zoom.Y;
        float deltaZoom = MathExtensions.GetNormalizedValInRange(zoomDifference, -minZoom, minZoom) 
            * _maxZoomSpeed * deltaTime;
        //float deltaZoom = 0f;
        //return new(requiredZoom, requiredZoom);
        return new(_camera.Zoom.Y + deltaZoom, _camera.Zoom.Y + deltaZoom);
    }
}
