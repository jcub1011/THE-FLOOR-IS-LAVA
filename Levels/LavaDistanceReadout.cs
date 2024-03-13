using Godot;
using Players;
using System;

namespace WorldGeneration;

public partial class LavaDistanceReadout : PanelContainer
{
    public void UpdateReadout(float cameraBottomY, float lavaPosY)
    {
        if (cameraBottomY >= lavaPosY)
        {
            Visible = false;
            return;
        }
        else Visible = true;

        GetChild<Label>(0).Text = $"Lava is {Mathf.FloorToInt(lavaPosY - cameraBottomY)} units away.";
    }
}
