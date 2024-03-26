using Godot;
using Godot.NodeExtensions;

namespace Players;

public partial class Tutorial : Control
{
    [Export] PackedScene _player;
    float _playerScale = 4f;
    bool _previousVisibleState = false;

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Visible != _previousVisibleState)
        {
            if (Visible) OnShow();
            else OnHide();
            _previousVisibleState = Visible;
        }
    }

    void OnShow()
    {
        var node = GetNode("Node2D");
        foreach(var device in PlayerInputHandler.GetOpenDevices())
        {
            var newPlayer = _player.Instantiate<Node2D>();
            var input = newPlayer.GetChild<PlayerInputHandler>();
            PlayerInputHandler.SetDevice(input, device);
            node.AddChild(newPlayer);
        }
        node.GetChild<StaticBody2D>().SetCollisionLayerValue(1, true);
    }

    void OnHide()
    {
        var node = GetNode("Node2D");
        var players = node.GetChildren<PlayerController>();
        foreach (var player in players)
        {
            player.QueueFree();
            node.RemoveChild(player);
        }
        node.GetChild<StaticBody2D>().SetCollisionLayerValue(1, false);
    }

    public void OnClose()
    {
        Visible = false;
    }

    public void OnOpen()
    {
        Visible = true;
    }
}
