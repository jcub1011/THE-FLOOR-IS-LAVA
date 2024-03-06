using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Players;

public readonly struct InputNames
{
    public readonly static StringName JUMP = "jump";
    public readonly static StringName LEFT = "left";
    public readonly static StringName RIGHT = "right";
    public readonly static StringName ACTION = "action";
    public readonly static StringName BLOCK = "block";
    public readonly static StringName CROUCH = "crouch";
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
        if (Type == DeviceType.Gamepad)
        {
            return $"{DeviceID + 1} : {Input.GetJoyName(DeviceID)}";
        }
        else
        {
            if (Type == DeviceType.KeyboardLeft)
            {
                return $"Keyboard Left";
            }
            else
            {
                return $"Keyboard Right";
            }
        }
    }

    public static bool operator ==(InputDevice left, InputDevice right)
    {
        return (left.Type == right.Type) && (left.DeviceID == right.DeviceID);
    }

    public static bool operator !=(InputDevice left, InputDevice right)
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
                || key.IsAction(InputNames.ACTION + InputNames.LEFT_KB_ID)
                || key.IsAction(InputNames.BLOCK + InputNames.LEFT_KB_ID)
                || key.IsAction(InputNames.CROUCH + InputNames.LEFT_KB_ID))
                ? DeviceType.KeyboardLeft : DeviceType.KeyboardRight;
    }

    public static StringName ConvertInputName(this StringName name, DeviceType type)
    {
        return type switch
        {
            DeviceType.KeyboardLeft => (StringName)(name + InputNames.LEFT_KB_ID),
            DeviceType.KeyboardRight => (StringName)(name + InputNames.RIGHT_KB_ID),
            _ => name,
        };
    }
}

public static class InputBuffer
{
    static readonly Dictionary<string, ulong> _inputTimeMap = new();
    const ulong MAX_BUFFER_TIME = 1000;
    const ulong TIME_BETWEEN_MAP_REFRESHES = 1000;
    static ulong _lastMapRefereshTime = 0;

    /// <summary>
    /// Buffers the provided input.
    /// </summary>
    /// <param name="characterBody"></param>
    /// <param name="inputName"></param>
    public static void BufferInput(Node characterBody, string inputName)
    {
        if (!_inputTimeMap.TryAdd(characterBody.Name + inputName, Time.GetTicksMsec()))
        {
            _inputTimeMap[characterBody.Name + inputName] = Time.GetTicksMsec();
        }
    }

    /// <summary>
    /// Checks if there exists a buffered input within the provided buffer time.
    /// </summary>
    /// <param name="characterBody"></param>
    /// <param name="inputName"></param>
    /// <param name="maxBufferTime"></param>
    /// <returns></returns>
    public static bool IsBuffered(Node characterBody, string inputName,
        float maxBufferTime)
    {
        ulong curTime = Time.GetTicksMsec();
        if (curTime - _lastMapRefereshTime > TIME_BETWEEN_MAP_REFRESHES)
        {
            RemoveExpiredBuffers();
        }

        if (_inputTimeMap.TryGetValue(characterBody.Name + inputName, out var value))
        {
            return curTime - value <= (ulong)(maxBufferTime * 1000);
        }
        else return false;
    }

    /// <summary>
    /// Clears the buffer for the provided input.
    /// </summary>
    /// <param name="characterBody"></param>
    /// <param name="inputName"></param>
    public static void ConsumeBuffer(Node characterBody, string inputName)
    {
        _inputTimeMap.Remove(characterBody.Name + inputName);
    }

    /// <summary>
    /// Deletes any buffered inputs past max buffer time.
    /// </summary>
    static void RemoveExpiredBuffers()
    {
        ulong curTime = Time.GetTicksMsec();
        _lastMapRefereshTime = curTime;
        List<string> toRemove = new();

        foreach (var kvp in _inputTimeMap)
        {
            if (curTime - kvp.Value > MAX_BUFFER_TIME) toRemove.Add(kvp.Key);
        }

        foreach (var key in toRemove)
        {
            _inputTimeMap.Remove(key);
        }
    }
}

public partial class PlayerInputHandler : Node, IDisableableControl
{
    #region Static
    static bool _onJoypadConnectedCallbackSet = false;
    static readonly List<InputDevice> DevicesToUse = new();
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

    /// <summary>
    /// Returns true if successful. No changes are made if supplied device 
    /// is not available.
    /// </summary>
    /// <param name="handler"></param>
    /// <param name="newDevice"></param>
    /// <returns></returns>
    public static bool SetDevice(PlayerInputHandler handler, InputDevice newDevice)
    {
        if (RegisteredDevices.Contains(newDevice)) return false;
        ReleaseDevice(handler);
        handler._device = newDevice;
        RegisteredDevices.Add(handler._device);
        GD.Print($"Changed {handler.Name} to new device {newDevice}.");
        return true;
    }

    public static List<InputDevice> GetOpenDevices()
    {
        List<InputDevice> openDevices = new();
        List<int> takenControllerIds = new();
        bool hasLeftKeyboard = false;
        bool hasRightKeyboard = false;

        GD.Print(RegisteredDevices);
        foreach (var device in RegisteredDevices)
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
        foreach (var id in Input.GetConnectedJoypads())
        {
            if (!takenControllerIds.Contains(id))
            {
                openDevices.Add(new(DeviceType.Gamepad, id));
            }
        }

        return openDevices;
    }

    public static int GetOpenDeviceCount()
    {
        return GetOpenDevices().Count;
    }

    public static void SetDevicesToUse(List<InputDevice> devices)
    {
        DevicesToUse.Clear();
        DevicesToUse.AddRange(devices);
    }

    public static List<InputDevice> GetDevicesToUse()
    {
        return DevicesToUse.ToList();
    }

    static void HandleJoypadConnectionChanged(long device, bool connected)
    {

    }
    #endregion

    /// <summary>
    /// Use the static methods to change device.
    /// </summary>
    InputDevice _device;
    bool _isEnabled = true;

    #region Signals
    [Signal] public delegate void MoveLeftPressedEventHandler();
    [Signal] public delegate void MoveLeftReleasedEventHandler();
    [Signal] public delegate void MoveRightPressedEventHandler();
    [Signal] public delegate void MoveRightReleasedEventHandler();
    [Signal] public delegate void JumpPressedEventHandler();
    [Signal] public delegate void JumpReleasedEventHandler();
    [Signal] public delegate void ActionPressedEventHandler();
    [Signal] public delegate void ActionReleasedEventHandler();
    [Signal] public delegate void BlockPressedEventHandler();
    [Signal] public delegate void BlockReleasedEventHandler();
    [Signal] public delegate void CrouchPressedEventHandler();
    [Signal] public delegate void CrouchReleasedEventHandler();
    #endregion

    //Dictionary<StringName, bool> _previousStateMap;

    #region Interface Implementation
    public string ControlID { get => ControlIDs.INPUT; }

    public void SetControlState(bool enabled)
    {
        _isEnabled = enabled;
    }
    #endregion

    public override void _Ready()
    {
        base._Ready();
        //SetDevice(this, GetNextOpenDevice());
        //GD.Print(_device);
        if (!_onJoypadConnectedCallbackSet)
        {
            _onJoypadConnectedCallbackSet = true;
            Input.JoyConnectionChanged += HandleJoypadConnectionChanged;
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (!_isEnabled) return;
        if (!_device.IsEventForDevice(@event)) return;

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
        if (@event.IsActionPressed(InputNames.BLOCK.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Block Input");
            EmitSignal(SignalName.BlockPressed);
        }
        if (@event.IsActionReleased(InputNames.BLOCK.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Stopped Block Input");
            EmitSignal(SignalName.BlockReleased);
        }
        if (@event.IsActionPressed(InputNames.CROUCH.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Crouch Input");
            EmitSignal(SignalName.CrouchPressed);
        }
        if (@event.IsActionReleased(InputNames.CROUCH.ConvertInputName(_device.Type)))
        {
            GD.Print($"{_device}: Stopped Crouch Input");
            EmitSignal(SignalName.CrouchReleased);
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ReleaseDevice(this);
    }

    public void ReleaseInput()
    {
        ReleaseDevice(this);
    }
}
