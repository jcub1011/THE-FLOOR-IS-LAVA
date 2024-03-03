using Godot;
using System;
using System.Collections.Generic;

namespace Players;

public partial class PlayerControllerSelector : HBoxContainer
{
    public static List<InputDevice> _selectedDevices = new();

    public const string EMPTY_CONTROLLER = "None";

    List<InputDevice> _deviceList = new();
    int _currentSelection = -1;

    public override void _Ready()
    {
        base._Ready();
        var dropdown = GetDropdown();
        PopulateList();
        dropdown.MouseEntered += OnHover;
        dropdown.FocusEntered += OnHover;
        dropdown.ItemSelected += OnItemSelected;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        var dropdown = GetDropdown();
        dropdown.MouseEntered -= OnHover;
        dropdown.FocusEntered -= OnHover;
        dropdown.ItemSelected -= OnItemSelected;
    }

    private void OnHover()
    {
        //throw new NotImplementedException();
        GD.Print("Hovered");
        UpdateDisabledOptions();
    }

    private void OnItemSelected(long index)
    {
        //throw new NotImplementedException();
        GD.Print("Selected " + index.ToString());

        if (_currentSelection != -1)
        {
            _selectedDevices.Remove(_deviceList[_currentSelection]);
        }
        _selectedDevices.Add(_deviceList[(int)index]);
        _currentSelection = (int)index;
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
}
