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
                foreach(var control in GetControlsByID(new string[1] { key }))
                {
                    control.SetControlState(true);
                }
                _controlDisableTimeMap.Remove(key);
            }
            else _controlDisableTimeMap[key] -= deltaTime;
        }
    }

    /// <summary>
    /// Disables the specified controls for the given duration.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="controls">ControlID</param>
    public void DisableControls(float duration, params string[] controls)
    {
        foreach(var control in GetControlsByID(controls))
        {
            control.SetControlState(false);
            SetDisableDuration(control, duration);
        }
    }

    /// <summary>
    /// Disables all controls except the exeptions for the given duration.
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="exceptions">ControlID</param>
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

    /// <summary>
    /// Enables the provided controls.
    /// </summary>
    /// <param name="controls">ControlID</param>
    public void EnableControls(params string[] controls)
    {
        foreach(var control in GetControlsByID(controls))
        {
            SetDisableDuration(control, 0f);
        }
    }

    /// <summary>
    /// Enables the controls except for the exceptions.
    /// </summary>
    /// <param name="exceptions">ControlID</param>
    public void EnableControlsExcept(params string[] exceptions)
    {
        foreach (var control in GetControls())
        {
            if (!exceptions.Contains(control.ControlID))
                SetDisableDuration(control, 0f);
        }
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
}
