using Godot;
using Godot.NodeExtensions;
using Players;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using WorldGeneration.Paths;

namespace WorldGeneration.Sections;

public interface IContinuableSection
{
    /// <summary>
    /// Path to next section relative to 
    /// </summary>
    /// <returns></returns>
    public string GetNextSection();
}

public partial class LockKeySection : TileMap, IContinuableSection
{
    //[Signal] public delegate void OnPlayerEnterExitEventHandler(PlayerController player);
    public event Action<PlayerController> OnPlayerEnterExit;
    [Export]
    string[] PossibleContinuations;
    public string GetNextSection() => PossibleContinuations.PickRandom();

    /// <summary>
    /// Tiles per second.
    /// </summary>
    [Export] float _lavaRaiseSpeed = 1;
    public float LavaRaiseSpeed { get => _lavaRaiseSpeed; }

    /// <summary>
    /// Gets a mask of the players in the section.
    /// </summary>
    public Players PlayersInSection
    {
        get
        {
            Players players = PlayerUtilityFlags.LivingPlayersMask;
            Players returnVal = Players.None;

            for (byte mask = 1; mask < (byte)Players.LastPlayer; mask <<= 1)
            {
                if (players.HasFlag((Players)mask))
                {
                    if (this.GetDirectChild<Area2D>()
                        .OverlapsBody(
                        PlayerUtilityFlags.FlagToPlayer((Players)mask)
                        ))
                    {
                        returnVal |= (Players)mask;
                    }
                }
            }

            return returnVal;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.GetDirectChild<ExitPath>().BodyEntered += OnPlayerEnteredExit;
    }

    void OnPlayerEnteredExit(Node node)
    {
        if (node is PlayerController player)
        {
            OnPlayerEnterExit?.Invoke(player);
        }
        //EmitSignal(SignalName.OnPlayerEnterExit, player);
    }

    protected ExitPath GetExitPath()
    {
        return this.GetDirectChild<ExitPath>();
    }

    protected EnterPath GetEnterPath()
    {
        return this.GetDirectChild<EnterPath>();
    }

    protected Area2D GetSectionBoundary()
    {
        return GetNode<Area2D>("Section Boundary");
    }

    public CollisionShape2D GetFocusBox()
    {
        return GetNode<CollisionShape2D>("Focus Box");
    }

    /// <summary>
    /// Stitches this section to the exit path of the provided section.
    /// </summary>
    /// <param name="section"></param>
    public void StitchTo(LockKeySection section)
    {
        ExitPath previousExit = section.GetExitPath();
        Vector2 targetPos;
        GD.Print($"{Name} is being stitched to {section.Name}.");
        GD.Print($"{GetEnterPath().GlobalPosition} is being stitched to {previousExit.GlobalPosition}.");

        if (previousExit.PathDirection == PathDirections.Left)
        {
            targetPos = previousExit.GlobalPosition - new Vector2(previousExit.PathWidth.ToPixels(), 0f);
        }
        else if (previousExit.PathDirection == PathDirections.Right)
        {
            targetPos = previousExit.GlobalPosition + new Vector2(previousExit.PathWidth.ToPixels(), 0f);
        }
        else
        {
            targetPos = previousExit.GlobalPosition - new Vector2(0f, previousExit.PathHeight.ToPixels());
        }

        Node2D enterPath = GetEnterPath();
        if (enterPath == null)
        {
            GD.PushError("Current section does not have an entrance path to stitch with.");
            return;
        }
        GD.Print(enterPath.Name);
        GlobalPosition = SolveForChildPosition(enterPath, targetPos);
    }

    /// <summary>
    /// Returns null if there are no viable spawn locations.
    /// </summary>
    /// <returns></returns>
    public List<Node2D> GetSpawnLocations()
    {
        var locationHolder = GetNode("Player Spawn Locations");
        if (locationHolder == null) return null;
        return locationHolder.GetChildren<Node2D>();
    }

    /// <summary>
    /// Solves for the global position this node needs to be at for 
    /// the given child node to be in the provided global position.
    /// </summary>
    /// <param name="child"></param>
    /// <param name="globalPosition"></param>
    /// <returns></returns>
    Vector2 SolveForChildPosition(Node2D child, Vector2 globalPosition)
    {
        return GlobalPosition + (globalPosition - child.GlobalPosition);
    }
}
