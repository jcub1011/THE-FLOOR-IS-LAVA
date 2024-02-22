using Godot;
using System;
using System.Collections.Specialized;

namespace Players;

public partial class OnHitArgs : GodotObject
{
    public MeleeHurtboxHandler HitBy { get; set; }
    public bool ReturnKnockback = false;
}

public partial class Hitbox : Area2D
{
    [Signal] public delegate void OnReceivedHitEventHandler(OnHitArgs args);
    [Signal] public delegate void OnReceivedDamageEventHandler(float knockback, Node2D source);
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
            //GD.Print($"{_body.Name} was hit by {collider.HurtboxOwner.Name}.");
            var args = new OnHitArgs
            {
                HitBy = collider
            };
            EmitSignal(SignalName.OnReceivedHit, args);

            if (args.ReturnKnockback)
            {
                GD.Print($"{_body.Name} deflected " +
                    $"{collider.HurtboxOwner.Name}.");
                args.HitBy.HandleAttackDeflected(_body, collider.Knockback);
            }
            else
            {
                GD.Print($"{_body.Name} was hit by {collider.HurtboxOwner.Name}.");
                EmitSignal(SignalName.OnReceivedDamage, collider.Knockback, collider.HurtboxOwner);
            }
        }
    }
}
