using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldGeneration;

public static class WorldInitializer
{
    const string _worldPath = "res://Levels/world.tscn";

    readonly static PackedScene _world = GD.Load<PackedScene>(_worldPath);
    static Node2D _existingWorld;

    public static Node2D GetNewWorld()
    {
        DeleteExistingWorld();

        _existingWorld = _world.Instantiate<Node2D>();
        return _existingWorld;
    }

    public static void DeleteExistingWorld()
    {
        if (_existingWorld == null
            || !GodotObject.IsInstanceValid(_existingWorld))
        {
            _existingWorld = null;
            return;
        }

        _existingWorld.QueueFree();
        _existingWorld = null;
    }
}
