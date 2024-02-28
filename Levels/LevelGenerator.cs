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
    [Export] double _scrollSpeed = 25f;
    [Export] StringName PlayerTemplatePath;

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

    public override void _Ready()
    {
        base._Ready();
        ResourceLoader.LoadThreadedRequest(PlayerTemplatePath);
        _worldBottomY = GetWorldBottomY();
        _activeWorldSections = new();

        _preloader = new("starter_section_1");

        var newSection = _preloader.GetNextSection();
        newSection.Position = Vector2.Zero;
        AddChild(newSection);
        newSection.Position = new(0f, WorldBottomY - newSection.LowerBoundary);
        _activeWorldSections.Enqueue(newSection);

        List<Vector2> spawnLocs = newSection.GetSpawnLocations();

        GetParent().Ready += () =>
        {
            SpawnPlayers(spawnLocs);
        };
    }

    void SpawnPlayers(List<Vector2> spawnLocs)
    {
        int playerCount = PlayerInputHandler.GetOpenDeviceCount();
        var player = ResourceLoader.LoadThreadedGet(PlayerTemplatePath) as PackedScene;

        for (int i = 0; i < playerCount; i++)
        {
            var temp = player.Instantiate() as Node2D;
            temp.GlobalPosition = spawnLocs[i % spawnLocs.Count];
            AddSibling(temp);
        }
    }

    void RemoveDeletedScenes()
    {
        List<WorldSection> toDelete = new();

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
        UpdateSectionPositions(_scrollSpeed, delta);
    }

    float GetWorldBottomY()
    {
        float cameraPos = _camera.GetScreenCenterPosition().Y;
        var cameraRect =
            GetCanvasTransform().AffineInverse().BasisXform(GetViewportRect().Size);
        float returnVal = cameraPos + cameraRect.Y / 2;
        GD.Print($"World bottom = {returnVal}");
        return returnVal;
    }

    void UpdateSectionPositions(double velocity, double delta)
    {
        var last = _activeWorldSections.Last();
        if (last.Position.Y >= 0f)
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
    }
}
