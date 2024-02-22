using Godot;
using System;
using System.Collections.Generic;

namespace WorldGeneration;

public partial class LevelGenerator : Node2D
{
    [Export] float _startDelay = 5f;
    [Export] Camera2D _camera;
    [Export] float _scrollSpeed = 25f;

    List<WorldSection> _activeWorldSections;

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
        _activeWorldSections = new();

        foreach(var child in GetChildren())
        {
            if (child is WorldSection section)
            {
                _activeWorldSections.Add(section);
            }
        }

        SetSectionsScrollVelocity(_scrollSpeed);
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

    void SetSectionsScrollVelocity(float velocity)
    {
        foreach(var section in _activeWorldSections)
        {
            section.Velocity = new(0f, velocity);
        }
    }
}
