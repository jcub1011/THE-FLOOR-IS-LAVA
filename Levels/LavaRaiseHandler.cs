using Godot;
using System;
using TheFloorIsLava.Subscriptions;

namespace WorldGeneration;

public partial class LavaRaiseHandler : Area2D
{
	[Export] double _raiseSpeed = 20;
	[Export] double _raiseAcceleration = 1;
	[Export] double _maxRaiseSpeed = 50;
	[Export] float _waitTimer = 2f;
	[Export] float _offscreenSpeedFactor = 0.1f;
	[Export] CameraSimulator _cameraSimulator;

	public double AdditionalVelocity { get; set; }

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_waitTimer > 0f)
		{
			_waitTimer -= (float)delta;
			return;
		}

		Position -= new Vector2(0f, (float)((_raiseSpeed) * delta));

		_raiseSpeed += _raiseAcceleration * delta;
		if (_raiseSpeed > _maxRaiseSpeed) _raiseSpeed = _maxRaiseSpeed;
		AddAdditionalRaiseSpeed(_cameraSimulator.GetCameraLowerY(), delta);
    }

    public override void _Ready()
    {
        base._Ready();
        OriginShiftChannel.OriginShifted += OriginShifted;
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

		Position += new Vector2(0f, dist * _offscreenSpeedFactor * (float)deltaTime);
	}
}
