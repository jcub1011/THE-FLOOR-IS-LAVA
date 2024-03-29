using Godot;
using Godot.NodeExtensions;
using Players;
using System;
using WorldGeneration.Paths;

namespace WorldGeneration.Sections;

public partial class LockKeySection : TileMap
{
    [Signal] public delegate void OnPlayerEnterExitEventHandler(PlayerController player);

    /// <summary>
    /// Tiles per second.
    /// </summary>
    [Export] float _lavaRaiseSpeed = 1;
    public float LavaRaiseSpeed { get => _lavaRaiseSpeed; }

    public override void _Ready()
    {
        base._Ready();
        this.GetDirectChild<ExitPath>().OnPlayerEntered += OnPlayerEnteredExit;
    }

    void OnPlayerEnteredExit(PlayerController player)
    {
        EmitSignal(SignalName.OnPlayerEnterExit, player);
    }
}
