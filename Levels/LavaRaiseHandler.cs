using Godot;
using System;

namespace WorldGeneration;

public partial class LavaRaiseHandler : Area2D
{
	[Export] double _raiseSpeed = 20;
	[Export] double _raiseAcceleration = 1;
	[Export] double _maxRaiseSpeed = 50;
	[Export] float _waitTimer = 2f;

	public double AdditionalVelocity { get; set; }

	public override void _Ready()
	{

	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (_waitTimer > 0f)
		{
			_waitTimer -= (float)delta;
			return;
		}

		Position -= new Vector2(0f, (float)((_raiseSpeed + AdditionalVelocity) * delta));

		_raiseSpeed += _raiseAcceleration * delta;
		if (_raiseSpeed > _maxRaiseSpeed) _raiseSpeed = _maxRaiseSpeed;
	}
}
