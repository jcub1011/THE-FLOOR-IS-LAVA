using Godot;
using Godot.NodeExtensions;
using System;
using TheFloorIsLava.Subscriptions;
using WorldGeneration;

namespace UI;

public class UIFPSLimiter
{
    int _frameCount;
    int _updateInterval;
    double _accumulatedDelta;

    /// <summary>
    /// How many times per second the update callback is called.
    /// </summary>
    public int FPS
    {
        get => Engine.PhysicsTicksPerSecond / _updateInterval;
        set => _updateInterval = Engine.PhysicsTicksPerSecond / value;
    }

    /// <summary>
    /// The parameter is delta time since the last update callback.
    /// </summary>
    public event Action<double> UpdateCallback;

    /// <summary>
    /// Make sure to call update every physics frame.
    /// </summary>
    /// <param name="updateCallback"></param>
    /// <param name="fps"></param>
    public UIFPSLimiter(Action<double> updateCallback, int fps)
    {
        FPS = fps;
        _frameCount = 0;
        UpdateCallback += updateCallback;
    }

    /// <summary>
    /// Call every physics update.
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(double deltaTime)
    {
        _accumulatedDelta += deltaTime;
        _frameCount++;

        if (_frameCount == _updateInterval)
        {
            _frameCount = 0;
            UpdateCallback.Invoke(_accumulatedDelta);
            _accumulatedDelta = 0;
        }
    }
}

public partial class DistanceReadout : PanelContainer
{
    double _trueBottom;
    double _distanceCovered;
    double _maxDistCovered = double.NegativeInfinity;
    CameraSimulator _cameraSimulator;
    UIFPSLimiter _limiter;

    public override void _Ready()
    {
        base._Ready();
        _cameraSimulator = GetParent().GetParent().GetChild<CameraSimulator>();
        _trueBottom = _cameraSimulator.GetCameraLowerY();
        OriginShiftChannel.OriginShifted += OnOriginShift;
        //frameInterval = Engine.PhysicsTicksPerSecond / 10;
        _limiter = new(UpdateReadout, 15);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _limiter.Update(delta);
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

    void UpdateReadout(double delta)
    {
        GetChild(0).GetChild<Label>(0).Text = $"Dist Traveled: {Mathf.Floor(_distanceCovered)} Tiles";
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        OriginShiftChannel.OriginShifted -= OnOriginShift;
    }
}
