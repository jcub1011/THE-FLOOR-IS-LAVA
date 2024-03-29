using Godot;
using Players;
using System;

namespace WorldGeneration.Paths;

public partial class ExitPath : Area2D
{
    [Signal] public delegate void OnPlayerEnteredEventHandler(PlayerController player);

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += OnObjectEntered;
    }

    void OnObjectEntered(Node2D node)
    {
        if (node is PlayerController player) EmitSignal(SignalName.OnPlayerEntered, player);
    }
}
