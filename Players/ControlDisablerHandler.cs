using Godot;
using Players;
using System;

namespace Players;

public readonly struct ControlIDs
{
    public const string AUTO_ANIMATION = "AutoAnimation";
    public const string MOVEMENT = "Movement";
    public const string HURTBOX = "Hurtbox";
    public const string HITBOX = "Hitbox";
    public const string ATTACK_HANDLER = "AttackHandler";
    public const string INPUT = "InputHandler";
    public const string GRAVITY = "Gravity";
    public const string KNOCKBACK = "Knockback";
    public const string DEFLECT = "Deflect";
}

public interface IDisableableControl
{
    public void SetControlState(bool enabled);
    public string ControlID { get; }
}

public partial class ControlDisablerHandler : Node
{
    [Signal] public delegate void OnAutoAnimationDisabledEventHandler();
    [Signal] public delegate void OnAutoAnimationEnabledEventHandler();

    [Signal] public delegate void OnMovementDisabledEventHandler();
    [Signal] public delegate void OnMovementEnabledEventHandler();

    public void SetControlStates(bool enabled, params string[] controls)
    {
        foreach(var control in controls)
        {
            SetControlStateByID(control, enabled);
        }
    }

    public void SetControlStateByID(string controlID, bool enabled)
    {
        foreach(var child in GetParent().GetChildren())
        {
            if (child is IDisableableControl control)
            {
                if (control.ControlID == controlID)
                {
                    control.SetControlState(enabled);
                }
            }
        }
    }

    /// <summary>
    /// Gets the first sibling matching the type T, where it returns 
    /// null if the sibling does not exist.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetFirstSiblingOfType<T>() where T : class
    {
        foreach(var child in GetParent().GetChildren())
        {
            if (child is T target) return target;
        }

        return null;
    }

}
