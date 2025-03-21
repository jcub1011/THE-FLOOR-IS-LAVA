using Godot;
using Godot.NodeExtensions;
using WorldGeneration;

namespace Players;

public partial class OnHitArgs : GodotObject
{
    public MeleeHurtboxHandler HitBy { get; set; }
    public bool Deflected = false;
}

public partial class Hitbox : Area2D, IDisableableControl
{
    [Signal] public delegate void OnReceivedHitEventHandler(OnHitArgs args);
    [Signal] public delegate void OnReceivedDamageEventHandler(Node2D source);
    CharacterBody2D _body
    {
        get => GetParent<CharacterBody2D>();
    }
    DeflectHandler _deflectHandler;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.HITBOX; }

    public void SetControlState(bool enabled)
    {
        SetDeferred("monitoring", enabled);
        SetDeferred("monitorable", enabled);
    }
    #endregion

    public override void _Ready()
    {
        base._Ready();
        AreaEntered += OnAreaEntered;
        _deflectHandler = this.GetSibling<DeflectHandler>();
    }

    void OnAreaEntered(Area2D area)
    {
        if (area is MeleeHurtboxHandler collider)
        {
            if (_body == collider.HurtboxOwner) return;
            var args = new OnHitArgs
            {
                HitBy = collider
            };
            EmitSignal(SignalName.OnReceivedHit, args);

            if (args.Deflected)
            {
                GD.Print($"{_body.Name} deflected " +
                    $"{collider.HurtboxOwner.Name}.");
                args.HitBy.HandleAttackDeflected(_body);
            }
            else
            {
                GD.Print($"{_body.Name} was hit by {collider.HurtboxOwner.Name}.");
                EmitSignal(SignalName.OnReceivedDamage, collider.HurtboxOwner);
                args.HitBy.OnHitLanded(_body);

                GD.Print("Applying hitstop.");
                EngineTimeManipulator.QueueTimeTransition(new(0.1, 0));
                EngineTimeManipulator.QueueTimeTransition(new(0.1));
                EngineTimeManipulator.QueueTimeTransition(new(1, 0));
            }
        }
    }
}
