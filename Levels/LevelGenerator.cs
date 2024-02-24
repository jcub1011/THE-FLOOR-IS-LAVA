using Godot;
using System;
using System.Collections.Generic;

namespace WorldGeneration;

public partial class LevelGenerator : Node2D
{
    [Export] float _startDelay = 5f;
    [Export] Camera2D _camera;
    [Export] double _scrollSpeed = 25f;

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
    }

    void RemoveDeletedScenes()
    {
        LinkedListNode<WorldSection> iterator = _activeWorldSections.First;

        while (iterator != null)
        {
            if (!IsInstanceValid(iterator.Value)) {
                _activeWorldSections.Remove(iterator);
            }
            iterator = iterator.Next;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        RemoveDeletedScenes();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        UpdateSectionPositions(_scrollSpeed, delta);
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

    void UpdateSectionPositions(double velocity, double delta)
    {
        Vector2 deltaPos = new(0f, (float)(velocity * delta));
        foreach(var section in _activeWorldSections)
        {
            if (!IsInstanceValid(section)) continue;
            section.Position += deltaPos;
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
