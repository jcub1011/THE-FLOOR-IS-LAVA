using Godot;
using Godot.NodeExtensions;
using Players;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using TheFloorIsLava.Camera;
using WorldGeneration.Sections;

namespace WorldGeneration;

[Flags]
public enum Players : byte
{
    None = 0,
    Player1 = 1,
    Player2 = 2,
    Player3 = 4,
    Player4 = 8,
    LastPlayer = 16
}

/// <summary>
/// This class exists because I though it would be fun to mess with bit flags.
/// Also, it helps improve performance by an imperceptible amount.
/// </summary>
public static class PlayerUtilityFlags
{
    static PlayerController[] _players = { null, null, null, null };
    public static Players RegisteredPlayersMask { get; private set; }
    public static Players LivingPlayersMask { get; private set; }

    /// <summary>
    /// First player in list is Player1, second is Player2, ...etc.
    /// </summary>
    /// <param name="players"></param>
    public static void SetPlayers(List<PlayerController> players)
    {
        ClearPlayers();
        for(int i = 0; i < _players.Length; i++)
        {
            if (i < players.Count) {
                _players[i] = players[i];
                UpdatePlayerLivingState(players[i]);
                RegisteredPlayersMask |= (Players)(1 << i);
            }
        }
    }

    /// <summary>
    /// Updates the player's living state in the LivingPlayersFlag.
    /// </summary>
    /// <param name="player"></param>
    public static void UpdatePlayerLivingState(PlayerController player)
    {
        Players mask = PlayerToMask(player);
        if (mask == Players.None) return;

        if (player.IsAlive)
        {
            LivingPlayersMask |= mask;
        }
        else
        {
            LivingPlayersMask &= ~mask;
        }
    }

    /// <summary>
    /// Clears the player list. (note: this does not destroy the player nodes)
    /// </summary>
    public static void ClearPlayers()
    {
        for (int i = 0; i < _players.Length; i++) _players[i] = null;
        RegisteredPlayersMask = Players.None;
        LivingPlayersMask = Players.None;
    }

    /// <summary>
    /// Converts the internal player list to a bitmask of all the players.
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    public static Players PlayersToMask(PlayerController[] players)
    {
        Players playersFlag = Players.None;

        for(int i = 0; i < players.Length; i++)
        {
            playersFlag |= (Players)(1 << i + 1)
                & (players[i] != null ? ~Players.None : Players.None);
        }

        return playersFlag;
    }

    /// <summary>
    /// Converts the internal player list to a bitmask of all the players.
    /// </summary>
    /// <param name="players"></param>
    /// <returns></returns>
    public static Players PlayersToMask(List<PlayerController> players)
    {
        Players playersFlag = Players.None;

        for (int i = 0; i < players.Count; i++)
        {
            playersFlag |= (Players)(1 << i)
                & (players[i] != null ? ~Players.None : Players.None);
        }

        return playersFlag;
    }

    /// <summary>
    /// Returns Players.None if the given player is not in the list of players.
    /// </summary>
    /// <param name="playerController"></param>
    /// <returns></returns>
    public static Players PlayerToMask(PlayerController playerController)
    {
        for (int i = 0; i < _players.Length; i++)
        {
            if (_players[i] == playerController) return (Players)(1 << i);
        }
        return Players.None;
    }

    /// <summary>
    /// Returns the player associated with the given player flag.
    /// </summary>
    /// <param name="player">If multiple player flags are set, only the first player will be returned.</param>
    /// <returns></returns>
    public static PlayerController FlagToPlayer(Players player)
    {
        if (player.HasFlag(Players.Player1)) return _players[0];
        else if (player.HasFlag(Players.Player2)) return _players[1];
        else if (player.HasFlag(Players.Player3)) return _players[2];
        else if (player.HasFlag(Players.Player4)) return _players[3];
        else return null;
    }
    
    /// <summary>
    /// Converts the mask to a list of flags set.
    /// </summary>
    /// <param name="mask"></param>
    /// <returns></returns>
    public static string MaskToString(Players mask)
    {
        if (mask == Players.None) return "None";

        string toReturn = "Has: ";
        if (mask.HasFlag(Players.Player1)) toReturn += "Player1 ";
        if (mask.HasFlag(Players.Player2)) toReturn += "Player2 ";
        if (mask.HasFlag(Players.Player3)) toReturn += "Player3 ";
        if (mask.HasFlag(Players.Player4)) toReturn += "Player4 ";
        return toReturn;
    }
}

public partial class NewWorldGenerator : Node2D
{
    [Export] string[] _starterSections;
    [Export] PackedScene _playerTemplate;
    SectionPreloader _preloader;
    List<LockKeySection> _activeSections;
    List<InputDevice> _inputDevices;
    bool _subscribedToReady;
    const string SECTION_CONTAINER_NAME = "SectionContainer";
    const string PLAYER_CONTAINER = "PlayerContainer";

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
        //this.GetDirectChild<NewWorldGeneratorCamera>()
        //    .SetFocusTarget(
        //    _activeSections.First().GetFocusBox(), 
        //    new Vector2(1, 1));
        SetPlayers(new() { new(DeviceType.KeyboardLeft, 0), new(DeviceType.KeyboardRight, 0) });
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (PlayerUtilityFlags.LivingPlayersMask == Players.None) return;

        if (_activeSections == null || _activeSections.Count == 0) return;
        bool addNewSection = false;

        foreach(var section in _activeSections)
        {
            if ((~section.PlayersInSection & PlayerUtilityFlags.LivingPlayersMask) == 0)
            {
                this.GetDirectChild<NewWorldGeneratorCamera>()
                    .SetFocusTarget(
                    section.GetFocusBox(),
                    new Vector2(1, 1));

                if (_activeSections.Last() == section)
                {
                    addNewSection = true;
                }
            }
        }

        if (addNewSection) GetNextSection();
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

        var playerContainer = GetNode(PLAYER_CONTAINER);
        var spawnLocations = _activeSections.First().GetSpawnLocations();

        for(int i = 0; i < _inputDevices.Count; i++)
        {
            var newPlayer = _playerTemplate.Instantiate<Node2D>();
            PlayerInputHandler.SetDevice(newPlayer.GetDirectChild<PlayerInputHandler>(), _inputDevices[i]);
            playerContainer.AddChild(newPlayer);
            newPlayer.GlobalPosition = spawnLocations[i].GlobalPosition;
        }

        PlayerUtilityFlags.SetPlayers(playerContainer.GetChildren<PlayerController>());
        GD.Print(PlayerUtilityFlags.MaskToString(PlayerUtilityFlags.RegisteredPlayersMask));
        GD.Print(PlayerUtilityFlags.MaskToString(PlayerUtilityFlags.LivingPlayersMask));
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
