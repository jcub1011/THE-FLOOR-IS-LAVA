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
    CameraSimulator _cameraSimulator;

    public override void _Ready()
    {
        base._Ready();
        _cameraSimulator = GetParent().GetParent().GetChild<CameraSimulator>();
        _trueBottom = _cameraSimulator.GetCameraLowerY();
        OriginShiftChannel.OriginShifted += OnOriginShift;
    }

    public void OnOriginShift(Vector2 shift)
    {
        _trueBottom += shift.Y;
        var box = _cameraSimulator.FocusBox;
        float additionalPos = -box.TopY();
        double distanceCovered = Mathf.Floor(_trueBottom.ToTiles() + additionalPos.ToTiles());
        if (distanceCovered < 0) distanceCovered = 0;
        GetChild(0).GetChild<Label>(0).Text = $"Dist Traveled: {distanceCovered} tiles.";
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        OriginShiftChannel.OriginShifted -= OnOriginShift;
    }
}
