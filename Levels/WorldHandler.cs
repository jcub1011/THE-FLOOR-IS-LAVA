using Godot;
using Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldGeneration;

public partial class WorldHandler : Node2D
{
    [Export] CanvasItem _retryScreen;
    [Export] CanvasItem _mainMenu;
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

        _existingWorld.Free();
        _existingWorld = null;
    }

    public void OnStartGameHandler()
    {
        _mainMenu.Visible = false;
        ReplaceWorld();
    }

    public void OnRestartHandler()
    {
        _retryScreen.Visible = false;
        RemoveChild( _existingWorld );
        ReplaceWorld();
    }
}
