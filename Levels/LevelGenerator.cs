using Godot;
using Players;
using System.Collections.Generic;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

namespace WorldGeneration;

public static class ListExtensions
{
    public static T PickRandom<T>(this IEnumerable<T> list)
    {
        int count = list.Count();
        if (count == 0) return default;

        var rand = new RandomNumberGenerator();

        return list.ElementAt((System.Index)(rand.Randi() % count));
    }

    public static T PickRandom<T>(this IEnumerable<T> list, RandomNumberGenerator rand)
    {
        int count = list.Count();
        if (count == 0) return default;

        return list.ElementAt((System.Index)(rand.Randi() % count));
    }
}

internal class SectionPreloader
{
    const string SECTION_PATH = "res://Levels/Sections/";

    string _sectionToLoad;

    public SectionPreloader(string sectionStarter)
    {
        StartNextSection(sectionStarter);
    }

    void StartNextSection(string sectionName)
    {
        _sectionToLoad = ToPath(sectionName);
        ResourceLoader.LoadThreadedRequest(_sectionToLoad);
    }

    public WorldSection GetNextSection()
    {
        var section = ResourceLoader.LoadThreadedGet(_sectionToLoad) as PackedScene;
        var worldSection = section.Instantiate<WorldSection>();
        StartNextSection(worldSection.PossibleContinuations.PickRandom());
        return worldSection;
    }

    static string ToPath(string name)
    {
        return SECTION_PATH + name + ".tscn";
    }
}

public partial class LevelGenerator : Node2D
{
    [Export] float _startDelay = 5f;
    [Export] Camera2D _camera;
    [Export] public double ScrollSpeed { get; set; } = 25f;
    [Export] StringName PlayerTemplatePath;
    [Export] string _speedRegionName = "SpeedRegion";
    [Export] string _slowRegionName = "SlowRegion";
    [Export] double _speedupFactor = 0.3;
    [Export] double _slowdownFactor = 0.2;

    Queue<WorldSection> _activeWorldSections;
    [Export] Godot.Collections.Array<StringName> _templates;

    SectionPreloader _preloader;

    public float? _worldBottomY;
    public float WorldBottomY
    {
        get
        {
            if (_worldBottomY == null) return float.PositiveInfinity;
            else return _worldBottomY.Value;
        }
    }

    List<Vector2> _spawnLocs;
    List<Node2D> _players;

    public override void _Ready()
    {
        base._Ready();
        _players = new();
        Engine.TimeScale = 0f;
        ResourceLoader.LoadThreadedRequest(PlayerTemplatePath);
        _worldBottomY = GetWorldBottomY();
        _activeWorldSections = new();

        _preloader = new("starter_section_1");

        var newSection = _preloader.GetNextSection();
        AddChild(newSection);
        newSection.Position = new(0f, - newSection.LowerBoundary + WorldBottomY);
        _activeWorldSections.Enqueue(newSection);

        _spawnLocs = newSection.GetSpawnLocations();

        GetParent().Ready += () =>
        {
            SpawnPlayers(_spawnLocs);
        };
    }

    void SpawnPlayers(List<Vector2> spawnLocs)
    {
        var player = ResourceLoader.LoadThreadedGet(PlayerTemplatePath) as PackedScene;

        var devices = PlayerInputHandler.GetDevicesToUse();
        for (int i = 0; i < devices.Count; i++)
        {
            var temp = player.Instantiate() as Node2D;
            temp.GlobalPosition = spawnLocs[i % spawnLocs.Count];
            PlayerInputHandler.SetDevice(temp.GetChildren<PlayerInputHandler>().First(), devices[i]);
            AddSibling(temp);
            _players.Add(temp);
        }

        Engine.TimeScale = 1f;
    }

    void RemoveDeletedScenes()
    {
        while(!IsInstanceValid(_activeWorldSections.Peek()))
        {
            _activeWorldSections.Dequeue();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        RemoveDeletedScenes();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        UpdateSectionPositions(ScrollSpeed, delta);
    }

    float GetWorldBottomY()
    {
        float cameraPos = _camera.GetScreenCenterPosition().Y;
        //var cameraRect =
        //    GetCanvasTransform().AffineInverse().BasisXform(GetViewportRect().Size);

        //Vector2 winSize = DisplayServer.WindowGetSize();
        //float yHeight = NodeExtensionMethods.GetViewportSize().Y;
        float yHeight = NodeExtensionMethods.GetViewportSize().Y / _camera.Zoom.Y / 2f;
        //float returnVal = cameraPos + cameraRect.Y / 2f;
        float returnVal = cameraPos + yHeight;
        GD.Print($"World bottom = {returnVal}");
        return returnVal;
    }

    float GetWorldTopY()
    {
        float cameraPos = _camera.GetScreenCenterPosition().Y;
        var cameraRect =
            GetCanvasTransform().AffineInverse().BasisXform(GetViewportRect().Size);
        float returnVal = cameraPos - cameraRect.Y / 2;
        //GD.Print($"World top = {returnVal}");
        return returnVal;
    }

    void UpdateSectionPositions(double velocity, double delta)
    {
        if (_startDelay > 0f)
        {
            _startDelay -= (float)delta;
            return;
        }
        velocity = GetModifiedVelocity(velocity);

        var last = _activeWorldSections.Last();
        if (last.Position.Y + last.UpperBoundary >= GetWorldTopY())
        {
            var newSection = _preloader.GetNextSection();
            _activeWorldSections.Enqueue(newSection);

            Vector2 newPos = Vector2.Zero;
            newPos.Y = last.Position.Y + last.UpperBoundary - newSection.LowerBoundary;

            newSection.Position = newPos;
            AddChild(newSection);
        }

        Vector2 deltaPos = new(0f, (float)(velocity * delta));
        foreach (var section in _activeWorldSections)
        {
            if (!IsInstanceValid(section)) continue;
            section.Position += deltaPos;
        }

        foreach(var player in _players)
        {
            if (!IsInstanceValid(player)) continue;
            player.Position += deltaPos;
        }
    }

    double GetModifiedVelocity(double baseVel)
    {
        var upperRegion = GetNode<Area2D>(_speedRegionName);
        var lowerRegion = GetNode<Area2D>(_slowRegionName);
        List<Node2D> livingPlayers = _players.Where(x => x.Visible).ToList();
        double modifiedVel = baseVel;
        double speedup = baseVel * _speedupFactor * 1 / livingPlayers.Count;
        double slowdown = baseVel * _slowdownFactor * 1 / livingPlayers.Count;

        foreach(var player in livingPlayers)
        {
            if (upperRegion.OverlapsBody(player))
            {
                modifiedVel += speedup * (1 - upperRegion.GetNormalizedDistFromTop(player));
            }
            else if (lowerRegion.OverlapsBody(player))
            {
                modifiedVel -= slowdown * lowerRegion.GetNormalizedDistFromTop(player);
            }
        }

        return modifiedVel;
    }
}
