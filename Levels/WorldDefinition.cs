using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

namespace WorldGeneration;

public static class WorldCoordExtensions
{
    public static float ToTiles(this float val) => val / WorldDefinition.TileHeightInPixels;
    public static float ToPixels(this float val) => val * WorldDefinition.TileHeightInPixels;
    public static double ToTiles(this double val) => val / WorldDefinition.TileHeightInPixels;
    public static double ToPixels(this double val) => val * WorldDefinition.TileHeightInPixels;
    public static Vector2 ToTiles(this Vector2 val) => val / WorldDefinition.TileHeightInPixels;
    public static Vector2 ToPixels(this Vector2 val) => val * WorldDefinition.TileHeightInPixels;
}

public partial class WorldDefinition : Node2D
{
    #region Static Members
    public static int TileHeightInPixels { get; private set; }

    public static float PixelsToTiles(float pixels) => pixels / TileHeightInPixels;
    public static float TilesToPixels(float tiles) => tiles * TileHeightInPixels;

    #endregion

    WorldDefinition()
    {
        TileHeightInPixels = _tileHeightInPixels;
    }

    [Export] public int _tileHeightInPixels = 128;
}
