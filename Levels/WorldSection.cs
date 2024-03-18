using Godot;
using Players;
using System.Collections.Generic;
using TheFloorIsLava.Subscriptions;

namespace WorldGeneration;

public partial class WorldSection : TileMap
{
    [Export] Marker2D _sectionUpperBound;
    [Export] Marker2D _sectionLowerBound;
    [Export] public Godot.Collections.Array<StringName> PossibleContinuations;

    /// <summary>
    /// Relative to section center.
    /// </summary>
    public float UpperBoundary { get => _sectionUpperBound.Position.Y; }
    /// <summary>
    /// Relative to section center.
    /// </summary>
    public float LowerBoundary { get => _sectionLowerBound.Position.Y; }

    public override void _Ready()
    {
        base._Ready();
        CollisionAnimatable = false;
        OriginShiftChannel.OriginShifted += OriginShifted;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        OriginShiftChannel.OriginShifted -= OriginShifted;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (IsInstanceValid(this) && !IsVisible())
        {
            GD.Print($"Deleting {Name}.");
            QueueFree();
        }
    }

    void OriginShifted(Vector2 shift)
    {
        Position += shift;
    }

    float GetWorldBottomY()
    {
        return GetParent<LevelGenerator>().GetParent().GetChild<LavaRaiseHandler>().Position.Y;
    }

    public bool IsVisible()
    {
        return _sectionUpperBound.GlobalPosition.Y < GetWorldBottomY();
    }

    public List<Vector2> GetSpawnLocations()
    {
        var locationParent = GetNode("SpawnLocations");

        var locs = new List<Vector2>();
        if (locationParent != null)
        {
            foreach(var child in locationParent.GetChildren())
            {
                if (child is Marker2D marker)
                {
                    locs.Add(marker.GlobalPosition);
                }
            }
        }

        if (locs.Count < 4)
        {
            GD.PushWarning($"Section {Name} does not contain " +
                $"enought player spawn locations.");
        }

        return locs;
    }
}
