using Godot;
using Godot.NodeExtensions;
using System;
using TheFloorIsLava.Subscriptions;

namespace WorldGeneration;

public partial class LavaRaiseHandler : Area2D
{
	[Export] float _raiseSpeedInTiles = 1;
	[Export] float _maxRaiseSpeedInTiles = 3;
	[Export] float _raiseAccelerationInTiles = 0.01f;
	[Export] float _waitTimer = 2f;
	[Export] float _speedMultiplerPerTileOffscreen = 0.1f;
	[Export] float _offscreenSpeedFactor = 0.1f;
	[Export] CameraSimulator _cameraSimulator;
	[Signal] public delegate void LavaDistanceFromScreenChangedEventHandler(float newDistanceTiles);

	public double AdditionalVelocity { get; set; }

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_waitTimer > 0f)
		{
			_waitTimer -= (float)delta;
		}
		else UpdateLavaPosition((float)delta);

		EmitSignal(SignalName.LavaDistanceFromScreenChanged,
            WorldDefinition.PixelsToTiles(Position.Y - _cameraSimulator.GetCameraLowerY()));
    }

    public override void _Ready()
    {
        base._Ready();
        OriginShiftChannel.OriginShifted += OriginShifted;
		Position = new Vector2(Position.X, GetParent().GetChild<CameraSimulator>().GetCameraLowerY() - 64);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
		OriginShiftChannel.OriginShifted -= OriginShifted;
    }

	void OriginShifted(Vector2 shift) => Position += shift;

    void AddAdditionalRaiseSpeed(float cameraBottomPos, double deltaTime)
	{
		float dist = cameraBottomPos - Position.Y;
		if (dist > 0) return;
		dist = WorldDefinition.PixelsToTiles(dist);
		float deltaPos = WorldDefinition.TilesToPixels(
			_raiseSpeedInTiles * dist * _speedMultiplerPerTileOffscreen
            * (float)deltaTime);

		Position += new Vector2(0f, deltaPos);
	}

	void UpdateLavaPosition(float deltaTime)
    {
        Position -= new Vector2(0f,
            WorldDefinition.TilesToPixels(_raiseSpeedInTiles) * deltaTime);

        _raiseSpeedInTiles += _raiseAccelerationInTiles * deltaTime;
        if (_raiseSpeedInTiles > _maxRaiseSpeedInTiles) _raiseSpeedInTiles = _maxRaiseSpeedInTiles;
        AddAdditionalRaiseSpeed(_cameraSimulator.GetCameraLowerY(), deltaTime);
    }
}
