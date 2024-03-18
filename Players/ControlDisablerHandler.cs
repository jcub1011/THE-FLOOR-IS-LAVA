using Godot;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

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
    public const string CROUCHER = "Croucher";
    public const string ACTION_HANDLER = "ActionHandler";
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
    Dictionary<string, float> _controlDisableTimeMap;

    public override void _Ready()
    {
        base._Ready();
        _controlDisableTimeMap = new();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _controlDisableTimeMap = null;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        UpdateControlDisableTimeMap((float)delta);
    }

    void UpdateControlDisableTimeMap(float deltaTime)
    {
        var keys = _controlDisableTimeMap.Keys.ToList();

        foreach (var key in keys)
        {
            if (_controlDisableTimeMap[key] <= 0f)
            {
                var control = GetControl(key);
                control?.SetControlState(true);
                _controlDisableTimeMap.Remove(key);
            }
            else _controlDisableTimeMap[key] -= deltaTime;
        }
    }

    public void DisableControls(float duration, params string[] controls)
    {
        foreach(var control in GetControlsByID(controls))
        {
            control.SetControlState(false);
            SetDisableDuration(control, duration);
        }
    }

    public void DisableControlsExcept(float duration, params string[] exceptions)
    {
        foreach(var control in GetControls())
        {
            if (!exceptions.Contains(control.ControlID))
            {
                control.SetControlState(false);
                SetDisableDuration(control, duration);
            }
        }
    }

    public void EnableControls(params string[] controls)
    {
        foreach(var control in GetControlsByID(controls))
        {
            SetDisableDuration(control, 0f);
        }
    }

    public void EnableControlsExcept(params string[] exceptions)
    {
        foreach (var control in GetControls())
        {
            if (!exceptions.Contains(control.ControlID))
                SetDisableDuration(control, 0f);
        }
    }

    public void SetControlStates(bool enabled, float undoAfterTime,
        params string[] controls)
    {
        DeleteCurTimer();

        foreach (var control in controls)
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

    public void SetControlStatesExcept(bool enabled, float undoAfterTime,
        params string[] controls)
    {
        List<string> toSet = new();
        foreach(var child in GetParent().GetChildren())
        {
            if (child is IDisableableControl control)
            {
                if (!controls.Contains(control.ControlID)) toSet.Add(control.ControlID);
            }
        }
        SetControlStates(enabled, undoAfterTime, toSet.ToArray());
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

        foreach (var child in GetParent().GetChildren())
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
        foreach (var child in GetParent().GetChildren())
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

    List<IDisableableControl> GetControlsByID(IEnumerable<string> controlIDs)
    {
        List<IDisableableControl> returnVal = new();

        foreach(var child in GetParent().GetChildren())
        {
            if (child is IDisableableControl control)
            {
                if (controlIDs.Contains(control.ControlID)) 
                    returnVal.Add(control);
            }
        }

        return returnVal;
    }

    List<IDisableableControl> GetControls()
    {
        List<IDisableableControl> returnVal = new();

        foreach (var child in GetParent().GetChildren())
        {
            if (child is IDisableableControl control)
            {
                returnVal.Add(control);
            }
        }

        return returnVal;
    }

    IDisableableControl GetControl(string controlID)
    {
        return GetControlsByID(new string[1] { controlID }).FirstOrDefault();
    }

    void SetDisableDuration(string controlID, float duration)
    {
        if (!_controlDisableTimeMap.TryAdd(controlID, duration))
        {
            _controlDisableTimeMap[controlID] = duration;
        }
    }

    void SetDisableDuration(IDisableableControl control, float duration)
    {
        if (!_controlDisableTimeMap.TryAdd(control.ControlID, duration))
        {
            _controlDisableTimeMap[control.ControlID] = duration;
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
        foreach (var child in GetParent().GetChildren())
        {
            if (child is T target) return target;
        }

        return null;
    }

}
