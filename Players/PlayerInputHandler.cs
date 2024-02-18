using Godot;
using System;
using System.Collections.Generic;

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
    private static readonly List<InputDevice> RegisteredDevices = new();

    public static InputDevice GetNextOpenDevice()
    {

    }
    #endregion

    InputDevice _device;

    #region Signals
    [Signal] public delegate void MoveLeftEventHandler();
    [Signal] public delegate void MoveRightEventHandler();
    [Signal] public delegate void JumpEventHandler();
    [Signal] public delegate void ActionEventHandler();
    #endregion

    public override void _Ready()
    {
        base._Ready();
        _device = GetNextOpenDevice();
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (_device.IsEventForDevice(@event))
        {
            GD.Print(@event.Device, _device.Type);
        }
        else return;
        //foreach (var device in Input.GetConnectedJoypads())
        //{
        //    GD.Print(Input.GetJoyName(device), device);
        //}

        if (@event.IsActionPressed(InputNames.LEFT.ConvertInputName(_device.Type)))
        {
            EmitSignal(SignalName.MoveLeft);
            GD.Print($"{_device}: Left Input");
        }
        if (@event.IsActionPressed(InputNames.RIGHT.ConvertInputName(_device.Type)))
        {
            EmitSignal(SignalName.MoveRight);
            GD.Print($"{_device}: Right Input");
        }
        if (@event.IsActionPressed(InputNames.JUMP.ConvertInputName(_device.Type)))
        {
            EmitSignal(SignalName.Jump);
            GD.Print($"{_device}: Jump Input");
        }
        if (@event.IsActionPressed(InputNames.ACTION.ConvertInputName(_device.Type)))
        {
            EmitSignal(SignalName.Action);
            GD.Print($"{_device}: Action Input");
        }
    }

    public void SetDevice(InputDevice device)
    {
        GD.Print($"Setting {Name} device to {device}.");
        _device = device;
    }
}
