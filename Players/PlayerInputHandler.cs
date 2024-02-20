using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Players;

public readonly struct InputNames
{
    public readonly static StringName JUMP = "jump";
    public readonly static StringName LEFT = "left";
    public readonly static StringName RIGHT = "right";
    public readonly static StringName ACTION = "action";
    public readonly static StringName LEFT_KB_ID = "_kb_l";
    public readonly static StringName RIGHT_KB_ID = "_kb_r";
}

public enum DeviceType
{
    None,
    Gamepad,
    KeyboardRight,
    KeyboardLeft
}

public readonly struct InputDevice
{
    public readonly DeviceType Type;
    public readonly int DeviceID;

    public InputDevice(DeviceType type, int deviceID)
    {
        Type = type;
        DeviceID = deviceID;
    }

    public InputDevice()
    {
        Type = DeviceType.None;
        DeviceID = int.MinValue;
    }

    public override string ToString()
    {
        return $"Type: {Type} | ID: {DeviceID}";
    }

    public static bool operator == (InputDevice left, InputDevice right)
    {
        return (left.Type == right.Type) && (left.DeviceID == right.DeviceID);
    }

    public static bool operator != (InputDevice left, InputDevice right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        return obj is InputDevice device && this == device;
    }

    public override int GetHashCode()
    {
        return Type.GetHashCode() ^ DeviceID.GetHashCode();
    }
}

public static class InputDeviceExtensions
{
    public static bool IsEventForDevice(this InputDevice device, InputEvent @event)
    {
        if (@event.Device != device.DeviceID) return false;
        else if (@event is InputEventKey key)
        {
            return device.Type == GetInputDeviceType(key);
        }
        else
        {
            return true;
        }
    }

    public static DeviceType GetInputDeviceType(this InputEventKey key)
    {
        return (key.IsAction(InputNames.LEFT + InputNames.LEFT_KB_ID)
                || key.IsAction(InputNames.RIGHT + InputNames.LEFT_KB_ID)
                || key.IsAction(InputNames.JUMP + InputNames.LEFT_KB_ID)
                || key.IsAction(InputNames.ACTION + InputNames.LEFT_KB_ID))
                ? DeviceType.KeyboardLeft : DeviceType.KeyboardRight;
    }

    public static StringName ConvertInputName(this StringName name, DeviceType type)
    {
        switch(type)
        {
            case DeviceType.KeyboardLeft:
                return name + InputNames.LEFT_KB_ID;
            case DeviceType.KeyboardRight:
                return name + InputNames.RIGHT_KB_ID;
            default:
                return name;
        }
    }
}

public partial class PlayerInputHandler : Node
{
    #region Static
    static readonly List<InputDevice> RegisteredDevices = new();

    static InputDevice GetNextOpenDevice()
    {
        var openDevices = GetOpenDevices();
        if (openDevices.Count == 0) return default;
        else return openDevices[0];
    }

    static void ReleaseDevice(PlayerInputHandler handler)
    {
        RegisteredDevices.Remove(handler._device);
        handler._device = default;
    }

    static void SetDevice(PlayerInputHandler handler, InputDevice newDevice)
    {
        ReleaseDevice(handler);
        handler._device = newDevice;
        RegisteredDevices.Add(handler._device);
        GD.Print($"Changed {handler.Name} to new device {newDevice}.");
    }

    static List<InputDevice> GetOpenDevices()
    {
        List<InputDevice> openDevices = new();
        List<int> takenControllerIds = new();
        bool hasLeftKeyboard = false;
        bool hasRightKeyboard = false;

        GD.Print(RegisteredDevices);
        foreach(var device in RegisteredDevices)
        {
            if (device.Type == DeviceType.KeyboardLeft)
            {
                hasLeftKeyboard = true;
            }
            else if (device.Type == DeviceType.KeyboardRight)
            {
                hasRightKeyboard = true;
            }
            else
            {
                takenControllerIds.Add(device.DeviceID);
            }
        }

        if (!hasLeftKeyboard)
        {
            openDevices.Add(new(DeviceType.KeyboardLeft, 0));
        }
        if (!hasRightKeyboard)
        {
            openDevices.Add(new(DeviceType.KeyboardRight, 0));
        }
        foreach(var id in Input.GetConnectedJoypads())
        {
            if (!takenControllerIds.Contains(id))
            {
                openDevices.Add(new(DeviceType.Gamepad, id));
            }
        }

        return openDevices;
    }
    #endregion

    /// <summary>
    /// Use the static methods to change device.
    /// </summary>
    InputDevice _device;

    #region Signals
    [Signal] public delegate void MoveLeftPressedEventHandler();
    [Signal] public delegate void MoveLeftReleasedEventHandler();
    [Signal] public delegate void MoveRightPressedEventHandler();
    [Signal] public delegate void MoveRightReleasedEventHandler();
    [Signal] public delegate void JumpPressedEventHandler();
    [Signal] public delegate void JumpReleasedEventHandler();
    [Signal] public delegate void ActionPressedEventHandler();
    [Signal] public delegate void ActionReleasedEventHandler();
    #endregion

    //Dictionary<StringName, bool> _previousStateMap;

    public override void _Ready()
    {
        base._Ready();
        SetDevice(this, GetNextOpenDevice());
        GD.Print(_device);
        //_previousStateMap = new()
        //{
        //    { InputNames.LEFT, false },
        //    { InputNames.RIGHT, false },
        //    { InputNames.JUMP, false },
        //    { InputNames.ACTION, false }
        //};
    }

    //void SetState(StringName name, bool state)
    //{
    //    if (!_previousStateMap.TryAdd(name, state))
    //    {
    //        _previousStateMap[name] = state;
    //    }
    //}
    
    //bool StateChanged(StringName name, bool newState)
    //{
    //    if (!_previousStateMap.TryGetValue(name, out var state))
    //    {
    //        return newState;
    //    }
    //    else
    //    {
    //        return state != newState;
    //    }
    //}

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (!_device.IsEventForDevice(@event)) return;
        //else
        //{
        //    GD.Print(_device);
        //}

        if (@event.IsActionPressed(InputNames.LEFT.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Left Input");
            EmitSignal(SignalName.MoveLeftPressed);
        }
        if (@event.IsActionReleased(InputNames.LEFT.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Stopped Left Input");
            EmitSignal(SignalName.MoveLeftReleased);
        }
        if (@event.IsActionPressed(InputNames.RIGHT.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Right Input");
            EmitSignal(SignalName.MoveRightPressed);
        }
        if (@event.IsActionReleased(InputNames.RIGHT.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Stopped Right Input");
            EmitSignal(SignalName.MoveRightReleased);
        }
        if (@event.IsActionPressed(InputNames.JUMP.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Jump Input");
            EmitSignal(SignalName.JumpPressed);
        }
        if (@event.IsActionReleased(InputNames.JUMP.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Stopped Jump Input");
            EmitSignal(SignalName.JumpReleased);
        }
        if (@event.IsActionPressed(InputNames.ACTION.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Action Input");
            EmitSignal(SignalName.ActionPressed);
        }
        if (@event.IsActionReleased(InputNames.ACTION.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Stopped Action Input");
            EmitSignal(SignalName.ActionReleased);
        }
    }
}
