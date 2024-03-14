using Godot;
using Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldGeneration;

public partial class CameraSimulator : Node
{
    [Export] Camera2D _camera;
    [Export] double _maxSpeed = 500;
    [Export] float _paddingPercent = 0.5f;
    [Export] float _lookAhead = 100f;
    [Export] float _lookBehind = 100f;
    [Export] Vector2 _minCameraZoom = new(2, 2);
    [Export] Vector2 _maxCameraZoom = new(4, 4);

    float _canvasTotalUnits = NodeExtensionMethods.GetViewportSize().Y;

    public override void _Ready()
    {
        base._Ready();
    }

    public float GetCameraUpperY()
    {
        return _camera.GetScreenCenterPosition().Y 
            - NodeExtensionMethods.GetViewportSize().Y / _camera.Zoom.Y / 2f;
    }

    public float GetCameraLowerY()
    {
        return _camera.GetScreenCenterPosition().Y
            + NodeExtensionMethods.GetViewportSize().Y / _camera.Zoom.Y / 2f;
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
        IEnumerable<Node2D> objects,
        IEnumerable<Node2D> focusPoints,
        double deltaTime,
        params Node2D[] additionalItems)
    {
        Rect2 focusBox = GetFocusBoundingBox(focusPoints);
        _camera.Zoom = GetNewZoom(focusBox, _minCameraZoom.Y, _maxCameraZoom.Y, _lookAhead, _lookBehind);

        float dist = GetNormDistFromCamCenter(GetCameraLowerY() - _lookBehind - focusBox.GetBottomY());

        Vector2 deltaPos = new(0f, (float)_maxSpeed * dist * (float)deltaTime);
        foreach (var node in objects)
        {
            if (!IsInstanceValid(node)) continue;
            else node.Position += deltaPos;
        }

        foreach(var node in focusPoints)
        {
            if (!IsInstanceValid(node)) continue;
            else node.Position += deltaPos;
        }

        foreach(var node in additionalItems)
        {
            if (!IsInstanceValid(node)) continue;
            else node.Position += deltaPos;
        }
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

    Rect2 GetFocusBoundingBox(IEnumerable<Node2D> focusPoints)
    {
        return new()
        {
            Size = GetFocusBox(focusPoints),
            Position = GetFocusAvgPos(focusPoints)
        };
    }

    float GetNormDistFromCamCenter(float yPos)
    {
        float top = GetCameraUpperY();
        float bottom = GetCameraLowerY();
        float halfRange = Mathf.Abs(top - bottom) / 2f;
        float displacement = yPos - (top + bottom) / 2f;
        if (Mathf.Abs(displacement) > halfRange) 
            return displacement < 0 ? -1f : 1f;
        else return displacement / halfRange;
    }

    float GetCameraYCoverage(float cameraPaddingPercent)
    {
        float coverage = Mathf.Abs(GetCameraLowerY() - GetCameraUpperY());
        return coverage;
    }

    Vector2 GetNewZoom(Rect2 focusBox, float minZoom, float maxZoom,
        float topPadding, float bottomPadding)
    {
        float padding = topPadding + bottomPadding;
        float coverage = Mathf.Abs(focusBox.GetTopY() - focusBox.GetBottomY()) + padding;
        if (coverage <= 0) coverage = 1;

        float requiredZoom = _canvasTotalUnits / coverage;

        requiredZoom = Mathf.Clamp(requiredZoom, minZoom, maxZoom);
        return new(requiredZoom, requiredZoom);
    }

    public Vector2 GetNewCameraZoom(IEnumerable<Node2D> focusPoints)
    {
        float maxY = float.NegativeInfinity;
        float minY = float.PositiveInfinity;

        foreach(var point in focusPoints)
        {
            if (point.GlobalPosition.Y < minY) minY = point.GlobalPosition.Y;
            if (point.GlobalPosition.Y > maxY) maxY = point.GlobalPosition.Y;
        }

        // No change to zoom in there are no focus points.
        if (maxY == float.NegativeInfinity) return _camera.Zoom;

        float curCoverage = GetCameraYCoverage(_paddingPercent);
        float requiredCoverage = maxY - minY;

        // No zoom change necessary.
        if (_camera.Zoom == _minCameraZoom && requiredCoverage <= curCoverage) return _camera.Zoom;

        float curZoom = _camera.Zoom.Y;
        float zoomMin = _minCameraZoom.Y;
        float zoomMax = _maxCameraZoom.Y;

        float deltaZoom = (requiredCoverage - curCoverage) / requiredCoverage;
        float newZoom = Mathf.Clamp(curZoom - deltaZoom, zoomMin, zoomMax);

        return new(newZoom, newZoom);
    }
}
