using Godot;
using Players;
using System.Collections.Generic;
using System.Linq;
using Weapons;

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
        //GD.Print("Printing orphaned nodes.");
        //Node.PrintOrphanNodes();
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
    //[Export] Camera2D _camera;
    [Export] StringName PlayerTemplatePath;
    [Export] string _speedRegionName = "SpeedRegion";
    [Export] string _slowRegionName = "SlowRegion";
    [Export] LavaRaiseHandler _lava;
    [Export] CameraSimulator _camera;
    [Export] StringName _sectionToStartWith = "starter_section_2";

    Queue<WorldSection> _activeWorldSections;
    [Export] Godot.Collections.Array<StringName> _templates;

    SectionPreloader _preloader;

    List<Vector2> _spawnLocs;
    List<PlayerController> _players;
    List<Projectile> _projectiles;
    bool _alreadyWarnedForLackingSections = false;

    public override void _Ready()
    {
        base._Ready();
        _players = new();
        Engine.TimeScale = 0f;
        ResourceLoader.LoadThreadedRequest(PlayerTemplatePath);
        _activeWorldSections = new();

        _preloader = new(_sectionToStartWith);

        var newSection = _preloader.GetNextSection();
        AddChild(newSection);
        newSection.Position = new(0f, - newSection.LowerBoundary + _camera.GetCameraLowerY());
        _activeWorldSections.Enqueue(newSection);

        _spawnLocs = newSection.GetSpawnLocations();

        _projectiles = new();

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
            _players.Add((PlayerController)temp);
        }

        foreach (var person in _players)
        {
            var generator = person.GetChild<ProjectileGenerator>();
            if (generator != null)
            {
                generator.ProjectileCreated += OnProjectileCreated;
            }
        }

        Engine.TimeScale = 1f;
    }

    void OnProjectileCreated(Node proj)
    {
        AddChild(proj);
        _projectiles.Add(proj as Projectile);
    }

    void RemoveDeletedScenes()
    {
        while(_activeWorldSections.Count != 0 
            && !IsInstanceValid(_activeWorldSections.Peek()))
        {
            _activeWorldSections.Dequeue();
        }

        int deleteIndex;
        while(_projectiles.Count != 0)
        {
            deleteIndex = _projectiles.FindIndex(x => !IsInstanceValid(x));
            if (deleteIndex == -1) break;
            else _projectiles.RemoveAt(deleteIndex);
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
        CallDeferred("UpdateSectionPositions", delta);
    }

    void UpdateSectionPositions(double delta)
    {
        if (!EnsureUpcommingSection()) return;

        List<PlayerController> players = _players.Where(x => x.IsAlive).ToList();
        if (players.Count == 0)
        {
            if (_lava.Position.Y < _camera.GetCameraUpperY() - 15)
            {
                _lava.Position = new Vector2(0, _camera.GetCameraUpperY() - 15);
            }
            return;
        }

        _camera.UpdateCamera(_activeWorldSections, players, delta, _lava);

        GetChild<LavaDistanceReadout>(0).UpdateReadout(_camera.GetCameraLowerY(), _lava.Position.Y);
    }

    /// <summary>
    /// Returns false if there are no active world sections to add onto.
    /// </summary>
    /// <returns></returns>
    bool EnsureUpcommingSection()
    {
        var last = _activeWorldSections.LastOrDefault();
        if (last == null || !IsInstanceValid(last))
        {
            if (!_alreadyWarnedForLackingSections)
            {
                GD.PushWarning("No more active world sections.");
                _alreadyWarnedForLackingSections = true;
            }
            return false;
        }

        if (last.Position.Y + last.UpperBoundary >= _camera.GetCameraUpperY())
        {
            var newSection = _preloader.GetNextSection();
            _activeWorldSections.Enqueue(newSection);

            Vector2 newPos = Vector2.Zero;
            newPos.Y = last.Position.Y + last.UpperBoundary - newSection.LowerBoundary;

            newSection.Position = newPos;
            AddChild(newSection);
        }

        return true;
    }

    double ForceCameraAboveLava(double deltaY, double amountOfLavaToKeepInFrame, double deltaTime)
    {
        double newLavaPos = _lava.Position.Y + deltaY;
        double lowerBound = _camera.GetCameraLowerY() - amountOfLavaToKeepInFrame;

        if (newLavaPos < lowerBound)
        {
            return (lowerBound - newLavaPos) * deltaTime;
        }
        return 0;
    }

    int PlayersInUpperCameraLimit(List<PlayerController> players)
    {
        var upperRegion = GetNode<Area2D>(_speedRegionName);
        return players.Count(x => upperRegion.OverlapsBody(x));
    }

    int PlayersInLowerCameraLimit(List<PlayerController> players)
    {
        var lowerRegion = GetNode<Area2D>(_slowRegionName);
        return players.Count(x => lowerRegion.OverlapsBody(x));
    }
}
