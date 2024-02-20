using Godot;
using System;

namespace Players;

public partial class Hitbox : Area2D
{
    [Signal] public delegate void OnReceivedHitEventHandler(MeleeHurtboxHandler hitBy);
    CharacterBody2D _body
    {
        get => GetParent<CharacterBody2D>();
    }

    public override void _Ready()
    {
        base._Ready();
        AreaEntered += OnAreaEntered;
    }

    void OnAreaEntered(Area2D area)
    {
        if (area is MeleeHurtboxHandler collider)
        {
            if (_body == collider.HurtboxOwner) return;
            GD.Print($"{_body.Name} was hit by {collider.HurtboxOwner.Name}.");
            EmitSignal(SignalName.OnReceivedHit, collider);
        }
    }
}
