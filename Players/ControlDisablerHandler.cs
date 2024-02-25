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
    public const string FLIPPER = "Flipper";
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

    SceneTreeTimer _curTimer = null;

    public void SetControlStates(bool enabled, float undoAfterTime,
        params string[] controls)
    {
        if (_curTimer != null && _curTimer.TimeLeft > 0f) return;

        foreach(var control in controls)
        {
            SetControlStateByID(control, enabled);
        }
        if (!float.IsNaN(undoAfterTime) && undoAfterTime > 0f)
        {
            _curTimer = GetTree().CreateTimer(undoAfterTime, false);
            _curTimer.Timeout += () => {
                SetControlStates(!enabled, float.NaN, controls);
            };
        }
    }

    void SetControlStateByID(string controlID, bool enabled)
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
