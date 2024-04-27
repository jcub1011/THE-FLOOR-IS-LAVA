using Godot;
using Godot.NodeExtensions;
using Players;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldGeneration.Sections;

namespace WorldGeneration;

public static class CameraFocusChangedChannel
{
    public static void SetNewFocus(CollisionShape2D boundingBox)
    {
        FocusBoxSet?.Invoke(boundingBox);
    }

    public static event Action<CollisionShape2D> FocusBoxSet;
}

public partial class NewWorldGenerator : Node2D
{
    [Export] string[] _starterSections;
    [Export] PackedScene _playerTemplate;
    SectionPreloader _preloader;
    List<LockKeySection> _activeSections;
    List<PlayerController> _players;
    List<InputDevice> _inputDevices;
    bool _subscribedToReady;
    const string SECTION_CONTAINER_NAME = "SectionContainer";

    public override void _Ready()
    {
        base._Ready();
        _preloader = new(_starterSections.PickRandom());
        _activeSections = new()
        {
            _preloader.GetNextSection<LockKeySection>()
        };

        AddSection(_activeSections.First());
        _activeSections.First().GlobalPosition = Vector2.Zero;
        GetNextSection();
        SetPlayers(new() { new(DeviceType.KeyboardLeft, 0) });
    }

    void GetNextSection()
    {
        var newSection = _preloader.GetNextSection<LockKeySection>();
        AddSection(newSection);
        newSection.StitchTo(_activeSections.Last());
        _activeSections.Add(newSection);
    }

    void AddSection(Node2D section)
    {
        GetNode(SECTION_CONTAINER_NAME).AddChild(section);
    }

    /// <summary>
    /// Sets the players.
    /// </summary>
    /// <param name="inputDevices"></param>
    public void SetPlayers(List<InputDevice> inputDevices)
    {
        _inputDevices = inputDevices;

        if (IsNodeReady())
        {
            SpawnPlayers();
        }
        else Ready += SpawnPlayers;
    }

    void SpawnPlayers()
    {
        if (_subscribedToReady) 
        { 
            Ready -= SpawnPlayers; 
            _subscribedToReady = false;
        }

        var playerContainer = GetNode("PlayerContainer");
        var spawnLocations = _activeSections.First().GetSpawnLocations();

        for(int i = 0; i < _inputDevices.Count; i++)
        {
            var newPlayer = _playerTemplate.Instantiate<Node2D>();
            PlayerInputHandler.SetDevice(newPlayer.GetDirectChild<PlayerInputHandler>(), _inputDevices[i]);
            playerContainer.AddChild(newPlayer);
            newPlayer.GlobalPosition = spawnLocations[i].GlobalPosition;
        }
    }

    /// <summary>
    /// Shifts all the child sections by the given world origin delta.
    /// </summary>
    /// <param name="deltaOrigin"></param>
    void ShiftOrigin(Vector2 deltaOrigin)
    {
        foreach(var section in GetNode(SECTION_CONTAINER_NAME).GetChildren())
        {
            if (section is Node2D moveable) moveable.GlobalPosition += deltaOrigin;
        }
    }
}
