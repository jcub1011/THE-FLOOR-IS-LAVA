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
    SceneTreeTimer _curTimer = null;
    string[] _controlsToReenable;

    public void SetControlStates(bool enabled, float undoAfterTime,
        params string[] controls)
    {
        DeleteCurTimer();

        foreach(var control in controls)
        {
            SetControlStateByID(control, enabled);
        }

        if (!float.IsNaN(undoAfterTime) && undoAfterTime > 0f)
        {
            _curTimer = GetTree().CreateTimer(undoAfterTime, false);
            _controlsToReenable = controls;
            _curTimer.Timeout += TimerCallback;
        }
    }

    /// <summary>
    /// Sets all the availiable controls to the desired state.
    /// </summary>
    /// <param name="enabled"></param>
    /// <param name="undoAfterTime"></param>
    public void SetControlStates(bool enabled, float undoAfterTime)
    {
        DeleteCurTimer();
        string[] toReenable = new string[GetParent().GetChildren().Count];
        int index = 0;

        foreach(var child in GetParent().GetChildren())
        {
            if (child is IDisableableControl control)
            {
                control.SetControlState(enabled);
                toReenable[index++] = control.ControlID;
            }
        }

        if (!float.IsNaN(undoAfterTime) && undoAfterTime > 0f)
        {
            _curTimer = GetTree().CreateTimer(undoAfterTime, false);
            _controlsToReenable = toReenable;
            _curTimer.Timeout += TimerCallback;
        }
    }

    void DeleteCurTimer()
    {
        if (_curTimer != null && _curTimer.TimeLeft > 0f)
        {
            _curTimer.Timeout -= TimerCallback;
            foreach (var control in _controlsToReenable)
            {
                SetControlStateByID(control, true);
            }
            _curTimer = null;
            _controlsToReenable = null;
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

    void TimerCallback()
    {
        SetControlStates(true, float.NaN, _controlsToReenable);
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
