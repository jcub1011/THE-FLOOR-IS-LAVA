using Godot;
using Players;
using System;
using UI;

namespace WorldGeneration;

public partial class LavaDistanceReadout : PanelContainer
{
    UIFPSLimiter _limiter;
    float _lavaDistance;

    public override void _Ready()
    {
        base._Ready();
        Visible = false;
        _limiter = new(UpdateText, 15);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        _limiter.Update(delta);
    }

    public void UpdateReadout(float newLavaDistanceTiles)
    {
        _lavaDistance = newLavaDistanceTiles;
    }

    public void UpdateText(double delta)
    {
        if (_lavaDistance <= 0f)
        {
            Visible = false;
            return;
        }
        else Visible = true;

        GetChild(0).GetChild<Label>(0).Text = $"Lava is {Mathf.CeilToInt(_lavaDistance)} tiles away.";
    }
}
