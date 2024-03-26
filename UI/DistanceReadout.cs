using Godot;
using Godot.NodeExtensions;
using System;
using TheFloorIsLava.Subscriptions;
using WorldGeneration;

namespace UI;

public partial class DistanceReadout : PanelContainer
{
    double _trueBottom;
    double _distanceCovered;
    double _maxDistCovered = double.NegativeInfinity;
    CameraSimulator _cameraSimulator;
    int frameCount;
    int frameInterval; // Target = 5fps.

    public override void _Ready()
    {
        base._Ready();
        _cameraSimulator = GetParent().GetParent().GetChild<CameraSimulator>();
        _trueBottom = _cameraSimulator.GetCameraLowerY();
        OriginShiftChannel.OriginShifted += OnOriginShift;
        frameInterval = Engine.PhysicsTicksPerSecond / 10;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        frameCount++;
        if (frameCount % frameInterval == 0)
        {
            UpdateReadout();
        }
    }

    public void OnOriginShift(Vector2 shift)
    {
        _trueBottom += shift.Y;
        var box = _cameraSimulator.FocusBox;
        float additionalPos = -box.TopY();
        _distanceCovered = _trueBottom.ToTiles() + additionalPos.ToTiles();
        if (_distanceCovered < 0) _distanceCovered = 0;
        if (_distanceCovered > _maxDistCovered) _maxDistCovered = _distanceCovered;
    }

    void UpdateReadout()
    {
        GetChild(0).GetChild<Label>(0).Text = $"Dist Traveled: {Mathf.Floor(_distanceCovered)} Tiles";
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        OriginShiftChannel.OriginShifted -= OnOriginShift;
    }
}
