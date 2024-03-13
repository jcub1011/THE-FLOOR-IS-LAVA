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
    /// <param name="objects"></param>
    /// <param name="focusPoints"></param>
    /// <param name="deltaTime"></param>
    public void UpdateObjectPositions(
        IEnumerable<Node2D> objects,
        IEnumerable<Node2D> focusPoints,
        double deltaTime,
        params Node2D[] additionalItems)
    {
        Vector2 deltaPos = new(0f, -GetFocusAvgPos(focusPoints).Y * (float)deltaTime);

        foreach(var node in objects)
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
