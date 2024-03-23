using Godot;
using Players;
using System;

namespace WorldGeneration;

public partial class LavaDistanceReadout : PanelContainer
{
    public override void _Ready()
    {
        base._Ready();
        Visible = false;
    }

    public void UpdateReadout(float newLavaDistanceTiles)
    {
        if (newLavaDistanceTiles < 0f)
        {
            Visible = false;
            return;
        }
        else Visible = true;

        GetChild<Label>(0).Text = $"Lava is {Mathf.CeilToInt(newLavaDistanceTiles)} tiles away.";
    }
}
