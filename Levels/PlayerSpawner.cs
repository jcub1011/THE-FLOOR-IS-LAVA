using Godot;
using System.Collections.Generic;
using System.Linq;

namespace WorldGeneration;

public partial class PlayerSpawner : Node2D
{
    #region Static
    static List<Vector2> _spawnLocations;
    public static List<Vector2> SpawnLocations
    {
        get => _spawnLocations.ToList();
    }
    #endregion

    public override void _Ready()
    {
        base._Ready();
        var spawnLocs = new List<Vector2>();

        foreach (var child in GetChildren())
        {
            if (spawnLocs.Count == 4) break;
            else if (child is Marker2D marker)
            {
                spawnLocs.Add(marker.GlobalPosition);
            }
        }
        _spawnLocations = spawnLocs;
        if (_spawnLocations.Count < 4) GD.PushError($"Insufficient spawn " +
            $"locations. Make sure to add spawn locations via " +
            $"Marker2D.");
    }
}
