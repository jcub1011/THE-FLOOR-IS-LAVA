using Godot;
using System;
using System.Collections.Generic;

namespace WorldGeneration;

public partial class LevelGenerator : Node2D
{
    [Export] float _startDelay = 5f;
    [Export] Camera2D _camera;
    [Export] float _scrollSpeed = 25f;

    LinkedList<WorldSection> _activeWorldSections;
    [Export] Godot.Collections.Array<StringName> _templates;

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
                _activeWorldSections.AddFirst(section);
            }
        }

        SetSectionsScrollVelocity(_scrollSpeed);
    }

    void RemoveDeletedScenes()
    {
        foreach (var scene in _activeWorldSections)
        {
            if (IsInstanceValid(scene)) continue;
            else
            {
                _activeWorldSections.Remove(scene);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        RemoveDeletedScenes();
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

    StringName GetNextSection()
    {
        var curSection = _activeWorldSections.Last.Value;
        int index = Mathf.Abs((int)(GD.Randi() - uint.MaxValue / 2)) 
            % curSection.PossibleContinuations.Count;
        return curSection.PossibleContinuations[index];
    }
}
