using Godot;
using System;
using System.Collections.Generic;

namespace Players;

public partial class PlayerControllerSelector : HBoxContainer
{
    public static List<InputDevice> _selectedDevices = new();

    public const string EMPTY_CONTROLLER = "None";

    public InputDevice SelectedDevice
    {
        get
        {
            if (_currentSelection == -1) return default;
            else return _deviceList[_currentSelection];
        }
    }

    List<InputDevice> _deviceList = new();
    int _currentSelection = -1;

    public override void _Ready()
    {
        base._Ready();
        var dropdown = GetDropdown();
        PopulateList();
        SetInitialPick();
        dropdown.MouseEntered += UpdateDisabledOptions;
        dropdown.FocusEntered += UpdateDisabledOptions;
        dropdown.ItemSelected += OnItemSelected;
        Input.JoyConnectionChanged += HandleJoypadConnectionChanged;
    }

    void HandleJoypadConnectionChanged(long device, bool connected)
    {
        var dropdown = GetDropdown();
        if (_currentSelection == -1)
        {
            ClearCurrentSelection();
            PopulateList();
            dropdown.Select(0);
            OnItemSelected(0);
            return;
        }

        var currentDevice = _deviceList[_currentSelection];
        ClearCurrentSelection();
        if (currentDevice.Type == DeviceType.Gamepad
            && currentDevice.DeviceID == device
            && connected == false)
        {
            PopulateList();
            dropdown.Select(0);
            OnItemSelected(0);
            return;
        }

        PopulateList();
        int index = _deviceList.IndexOf(currentDevice);
        dropdown.Select(index);
        OnItemSelected(index);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        var dropdown = GetDropdown();
        dropdown.MouseEntered -= UpdateDisabledOptions;
        dropdown.FocusEntered -= UpdateDisabledOptions;
        dropdown.ItemSelected -= OnItemSelected;
    }

    void OnItemSelected(long index)
    {
        //throw new NotImplementedException();
        GD.Print("Selected " + index.ToString());

        ClearCurrentSelection();
        _selectedDevices.Add(_deviceList[(int)index]);
        _currentSelection = (int)index;
    }

    void ClearCurrentSelection()
    {
        if (_currentSelection != -1)
        {
            _selectedDevices.Remove(SelectedDevice);
        }
        _currentSelection = -1;
    }

    /// <summary>
    /// Returns null if dropdown doesn't exist.
    /// </summary>
    /// <returns></returns>
    public OptionButton GetDropdown()
    {
        foreach (var child in GetChildren())
        {
            if (child is OptionButton options)
            {
                return options;
            }
        }
        return null;
    }

    /// <summary>
    /// Populates the item list with the available devices.
    /// </summary>
    public void PopulateList()
    {
        var dropdown = GetDropdown();
        dropdown.Clear();
        _deviceList.Clear();

        dropdown.AddItem(EMPTY_CONTROLLER);
        _deviceList.Add(default);

        foreach (var item in PlayerInputHandler.GetOpenDevices())
        {
            dropdown.AddItem(item.ToString());
            _deviceList.Add(item);
        }
    }

    void UpdateDisabledOptions()
    {
        var dropdown = GetDropdown();
        for(int i = 1; i < dropdown.ItemCount; i++)
        {
            if (_currentSelection == i)
            {
                dropdown.SetItemDisabled(i, false);
            }
            else
            {
                dropdown.SetItemDisabled(i, _selectedDevices.Contains(_deviceList[i]));
            }
        }
    }

    void SetInitialPick()
    {
        for (int i = 1; i < _deviceList.Count; i++)
        {
            if (!_selectedDevices.Contains(_deviceList[i]))
            {
                GetDropdown().Select(i);
                OnItemSelected(i);
                UpdateDisabledOptions();
                return;
            }
        }
        GetDropdown().Select(0);
        OnItemSelected(0);
    }
}
