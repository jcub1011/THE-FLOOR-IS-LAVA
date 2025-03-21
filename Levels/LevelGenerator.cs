using Godot;
using Players;
using System.Collections.Generic;
using System.Linq;
using Godot.NodeExtensions;

namespace WorldGeneration;

public static class ListExtensions
{
    public static T PickRandom<T>(this IEnumerable<T> list)
    {
        int count = list.Count();
        if (count == 0) return default;
        return list.ElementAt((System.Index)(GD.Randi() % count));
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
    string[] _overridePattern;
    int _overridePatternIndex;

    public SectionPreloader(string sectionStarter)
    {
        StartNextSection(sectionStarter);
    }

    public SectionPreloader(string sectionStarter, string[] sectionGenerationOverride)
    {
        StartNextSection(sectionStarter);
        _overridePatternIndex = 0;
        _overridePattern = sectionGenerationOverride;
    }

    void StartNextSection(string sectionName)
    {
        if (_overridePattern != null && _overridePattern.Length > 0)
        {
            sectionName = _overridePattern[_overridePatternIndex++ % _overridePattern.Length];
        }
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
    [Export] LavaRaiseHandler _lava;
    [Export] CameraSimulator _camera;
    [Export] StringName _sectionToStartWith = "starter_section_2";
    [Export]
    string[] _sectionGenerationOverride;

    WorldSection _latestSection;
    [Export] Godot.Collections.Array<StringName> _templates;

    SectionPreloader _preloader;

    List<Vector2> _spawnLocs;
    List<PlayerController> _players;
    bool _alreadyWarnedForLackingSections = false;

    public override void _Ready()
    {
        base._Ready();
        _players = new();
        Engine.TimeScale = 0f;
        ResourceLoader.LoadThreadedRequest(PlayerTemplatePath);

        if (_sectionGenerationOverride != null && _sectionGenerationOverride.Length > 0)
        {
            _preloader = new(_sectionToStartWith, _sectionGenerationOverride);
        }
        else
        {
            _preloader = new(_sectionToStartWith);
        }

        var newSection = _preloader.GetNextSection();
        AddChild(newSection);
        newSection.Position = new(0f, - newSection.LowerBoundary + _camera.GetCameraLowerY());
        _latestSection = newSection;

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
            _players.Add((PlayerController)temp);
        }

        Engine.TimeScale = 1f;
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

        _camera.UpdateCamera(players, delta);

        //GetParent().GetChild<LavaDistanceReadout>(0)
        //    .UpdateReadout(_camera.GetCameraLowerY(), _lava.Position.Y);
    }

    /// <summary>
    /// Returns false if there are no active world sections to add onto.
    /// </summary>
    /// <returns></returns>
    bool EnsureUpcommingSection()
    {
        if (_latestSection == null || !IsInstanceValid(_latestSection))
        {
            if (!_alreadyWarnedForLackingSections)
            {
                GD.PushWarning("No more active world sections.");
                _alreadyWarnedForLackingSections = true;
            }
            return false;
        }

        if (_latestSection.Position.Y + _latestSection.UpperBoundary >= _camera.GetCameraUpperY())
        {
            var newSection = _preloader.GetNextSection();
            Vector2 newPos = Vector2.Zero;
            newPos.Y = _latestSection.Position.Y + _latestSection.UpperBoundary - newSection.LowerBoundary;
            newSection.Position = newPos;

            _latestSection = newSection;
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
}
