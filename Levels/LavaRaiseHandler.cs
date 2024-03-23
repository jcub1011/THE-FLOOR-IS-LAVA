using Godot;
using Godot.NodeExtensions;
using System;
using TheFloorIsLava.Subscriptions;

namespace WorldGeneration;

public partial class LavaRaiseHandler : Area2D
{
	[Export] double _raiseSpeed = 20;
	[Export] float _raiseSpeedInTiles = 1;
	[Export] float _maxRaiseSpeedInTiles = 3;
	[Export] float _raiseAccelerationInTiles = 0.01f;
	[Export] double _raiseAcceleration = 1;
	[Export] double _maxRaiseSpeed = 50;
	[Export] float _waitTimer = 2f;
	[Export] float _speedMultiplerPerTileOffscreen = 0.1f;
	[Export] float _offscreenSpeedFactor = 0.1f;
	[Export] CameraSimulator _cameraSimulator;
	[Export] int _tileHeight = 128;
	[Signal] public delegate void LavaDistanceFromScreenChangedEventHandler(float newDistanceTiles);

	public double AdditionalVelocity { get; set; }

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_waitTimer > 0f)
		{
			_waitTimer -= (float)delta;
			return;
		}

		Position -= new Vector2(0f, 
			(float)(WorldDefinition.TilesToPixels(_raiseSpeedInTiles) * delta));

		_raiseSpeed += _raiseAcceleration * delta;
		if (_raiseSpeed > _maxRaiseSpeed) _raiseSpeed = _maxRaiseSpeed;
		_raiseSpeedInTiles += _raiseAccelerationInTiles * (float)delta;
		if (_raiseSpeedInTiles > _maxRaiseSpeedInTiles) _raiseSpeedInTiles = _maxRaiseSpeedInTiles;
		AddAdditionalRaiseSpeed(_cameraSimulator.GetCameraLowerY(), delta);

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

    public void AddAdditionalRaiseSpeed(float cameraBottomPos, double deltaTime)
	{
		float dist = cameraBottomPos - Position.Y;
		if (dist > 0) return;
		dist = WorldDefinition.PixelsToTiles(dist);
		float deltaPos = WorldDefinition.TilesToPixels(
			_raiseSpeedInTiles * dist * _speedMultiplerPerTileOffscreen
            * (float)deltaTime);

		Position += new Vector2(0f, deltaPos);
	}

	//float PixelsToTiles(float pixels) => pixels / _tileHeight;
	//float TilesToPixels(float tiles) => tiles * _tileHeight;
}
