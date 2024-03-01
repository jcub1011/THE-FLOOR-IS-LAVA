using Godot;
using System;

namespace WorldGeneration;

public partial class WorldHandler : Node2D
{
    [Export] PackedScene _world;
    Node2D _existingWorld;

    public override void _Ready()
    {
        base._Ready();
       GD.Print( GetTree().Root.GetNode($"{Name}").Name);
    }

    public void ReplaceWorld()
    {
        DeleteExistingWorld();

        _existingWorld = _world.Instantiate<Node2D>();
        AddChild( _existingWorld );
    }

    void DeleteExistingWorld()
    {
        if (_existingWorld == null
            || !IsInstanceValid(_existingWorld))
        {
            _existingWorld = null;
            return;
        }

        _existingWorld.QueueFree();
        _existingWorld = null;
    }

    public void OnStartGameHandler()
    {
        GetNode<CanvasItem>("MainMenu").Visible = false;
        ReplaceWorld();
    }

    public void OnRestartHandler()
    {
        GetNode<CanvasItem>("RetryScreen").Visible = false;
        RemoveChild( _existingWorld );
        ReplaceWorld();
    }
}
