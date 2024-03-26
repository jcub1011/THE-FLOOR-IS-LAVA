using Godot;
using Godot.NodeExtensions;

namespace Players;

public enum AttackHeight
{
    Standing,
    Crouched
}

public partial class MeleeHurtboxHandler : Area2D, IDisableableControl
{
    [Signal] public delegate void OnHurtboxDeflectedEventHandler(CharacterBody2D deflector);
    [Signal] public delegate void HitLandedEventHandler(Node2D thingHit);

    /// <summary>
    /// Defaults to length of frame @ 24fps.
    /// </summary>
    [Export] float _defaultHitboxLifeTime = 0.04f;
    bool _hasActiveHitboxes;
    public float _remainingHitboxTime;
    CollisionShape2D _activeCollider;
    bool _isFlipped = false;
    public Node2D HurtboxOwner
    {
        get => GetParent<CharacterBody2D>();
    }
    //public float Knockback { get; private set; }
    //public AttackHeight AttackHeight { get; private set; }

    float _colliderEnableDuration;
    TimedCollider _colliderToEnable;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.HURTBOX; }

    public void SetControlState(bool enabled)
    {
        if (!enabled)
        {
            DisableHitboxes();
        }
    }
    #endregion

    public override void _Ready()
    {
        base._Ready();
        DisableHitboxes();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _colliderToEnable?.EnableCollider(_colliderEnableDuration);
        _colliderToEnable = null;
    }

    public void EnableHitbox(StringName hitboxName, float duration)
    {
        _colliderEnableDuration = duration;
        foreach(var child in this.GetDirectChildren<TimedCollider>())
        {
            child.ForceDisableCollider();

            if (child.Name == hitboxName) _colliderToEnable = child;
        }
    }

    void DisableHitboxes()
    {
        foreach (var child in GetChildren())
        {
            if (child is TimedCollider timedCollider)
            {
                timedCollider.ForceDisableCollider();
            }
            else if (child is CollisionShape2D collider)
            {
                collider.SetDeferred("disabled", true);
            }
        }
        _activeCollider = null;
    }

    public void SetFlipState(bool flipState)
    {
        if (_isFlipped == flipState) return;
        _isFlipped = flipState;

        Scale = new(Scale.X * -1f, Scale.Y);
    }

    public void HandleAttackDeflected(CharacterBody2D deflector)
    {
        GD.Print($"{HurtboxOwner.Name} emitting attack deflected.");
        EmitSignal(SignalName.OnHurtboxDeflected, deflector);
    }

    public void OnHitLanded(Node2D thingHit)
    {
        GD.Print($"Hit landed on {thingHit}.");
        EmitSignal(SignalName.HitLanded, thingHit);
    }
}
