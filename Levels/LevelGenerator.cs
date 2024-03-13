using Godot;
using Players;
using System.Collections.Generic;
using System.Linq;

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

internal class ScrollSpeedAccelerator
{
    [Export] LavaRaiseHandler _lava;
    const double SPEED_CAP = 1000.0;
    const double TIME_CAP = 1000.0;
    readonly double _maxSpeed;
    readonly double _startSpeed;

    double _remainingTimeToReachMaxSpeed;
    double _timeToReachMaxSpeed;

    /// <summary>
    /// Max speed cannot be lower than start speed.
    /// </summary>
    /// <param name="startSpeed"></param>
    /// <param name="maxSpeed"></param>
    /// <param name="timeToReachEndSpeed"></param>
    public ScrollSpeedAccelerator(double startSpeed, double maxSpeed, double timeToReachEndSpeed)
    {
        _remainingTimeToReachMaxSpeed = Mathf.Clamp(timeToReachEndSpeed, 0.01, TIME_CAP);
        _timeToReachMaxSpeed = _remainingTimeToReachMaxSpeed;
        _startSpeed = startSpeed;
        _maxSpeed = Mathf.Clamp(maxSpeed, startSpeed, SPEED_CAP);
    }

    /// <summary>
    /// Gets the new scrolls speed given the delta time since the last call.
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <returns></returns>
    double GetNewScrollSpeed(double deltaTime)
    {
        _remainingTimeToReachMaxSpeed = 
            Mathf.Clamp(_remainingTimeToReachMaxSpeed - deltaTime, 0, TIME_CAP);
        
        return _startSpeed + (_maxSpeed - _startSpeed) 
            * (_remainingTimeToReachMaxSpeed / _timeToReachMaxSpeed);
    }

    public double GetNewScrollSpeed(double deltaTime, int livingPlayerCount, 
        int numInSpeedupArea, int numInSlowdownArea, double speedUpFactor, double slowdownFactor)
    {
        double unmodifiedScrollSpeed = GetNewScrollSpeed(deltaTime);
        double speedupModifier = unmodifiedScrollSpeed * speedUpFactor * numInSpeedupArea / livingPlayerCount;
        double slowdownModifier = unmodifiedScrollSpeed * slowdownFactor * numInSlowdownArea / livingPlayerCount;
        return unmodifiedScrollSpeed + speedupModifier - slowdownModifier;
    }
}

public partial class LevelGenerator : Node2D
{
    [Export] float _startDelay = 5f;
    [Export] Camera2D _camera;
    [Export] public double ScrollSpeed { get; set; } = 25f;
    [Export] public double MaxScrollSpeed { get; set; } = 50f;
    [Export] public double TimeToReachMaxSpeed { get; set; } = 30f;
    [Export] StringName PlayerTemplatePath;
    [Export] string _speedRegionName = "SpeedRegion";
    [Export] string _slowRegionName = "SlowRegion";
    [Export] double _speedupFactor = 0.3;
    [Export] double _slowdownFactor = 0.2;
    [Export] double _cameraHeightOffset = 40;
    [Export] LavaRaiseHandler _lava;

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
    List<PlayerController> _players;
    ScrollSpeedAccelerator _speedAccelerator;

    public override void _Ready()
    {
        base._Ready();
        _speedAccelerator = new(ScrollSpeed, MaxScrollSpeed, TimeToReachMaxSpeed);
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
            _players.Add((PlayerController)temp);
        }

        Engine.TimeScale = 1f;
    }

    void RemoveDeletedScenes()
    {
        while(_activeWorldSections.Count != 0 
            && !IsInstanceValid(_activeWorldSections.Peek()))
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
        CallDeferred("UpdateSectionPositions", ScrollSpeed, delta);
        //UpdateSectionPositions(ScrollSpeed, delta);
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
        velocity = GetNewScrollspeed(delta);
        //double deltaPos = UpdateCameraPosition(_players.Where(x => x.Visible).ToList());
        //GD.Print(velocity);

        var last = _activeWorldSections.LastOrDefault();
        if (last == null || !IsInstanceValid(last))
        {
            GD.PushWarning("No more active world sections.");
            _startDelay = 1f;
            return;
        }

        List<PlayerController> players = _players.Where(x => x.IsAlive).ToList();
        if (players.Count == 0)
        {
            if (_lava.Position.Y < GetWorldTopY() - 15)
            {
                _lava.Position = new Vector2(0, GetWorldTopY() - 15);
            }
            return;
        }

        if (last.Position.Y + last.UpperBoundary >= GetWorldTopY())
        {
            var newSection = _preloader.GetNextSection();
            _activeWorldSections.Enqueue(newSection);

            Vector2 newPos = Vector2.Zero;
            newPos.Y = last.Position.Y + last.UpperBoundary - newSection.LowerBoundary;

            newSection.Position = newPos;
            AddChild(newSection);
        }

        //Vector2 deltaPos = new(0f, (float)(velocity * delta));
        Vector2 deltaPos = new(0f, (float)(GetCameraDeltaX(players, delta)));
        deltaPos.Y += (float)ForceCameraAboveLava(deltaPos.Y, 10, delta);
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

        _lava.Position += deltaPos;
        _lava.AddAdditionalRaiseSpeed(GetWorldBottomY(), delta);

        GetChild<LavaDistanceReadout>(0).UpdateReadout(GetWorldBottomY(), _lava.Position.Y);
    }

    double ForceCameraAboveLava(double deltaY, double amountOfLavaToKeepInFrame, double deltaTime)
    {
        double newLavaPos = _lava.Position.Y + deltaY;
        double lowerBound = GetWorldBottomY() - amountOfLavaToKeepInFrame;

        if (newLavaPos < lowerBound)
        {
            return (lowerBound - newLavaPos) * deltaTime;
        }
        return 0;
    }

    double GetAveragePlayerPosition(List<PlayerController> players)
    {
        double sum = 0;
        foreach(var player in players)
        {
            sum += player.GlobalPosition.Y;
        }
        return sum / players.Count;
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

    double GetCameraDeltaX(List<PlayerController> players, double deltaTime)
    {
        if (players.Count == 0) return 0;
        double avgPos = GetAveragePlayerPosition(players);
        double deltaX = (_cameraHeightOffset - avgPos) * deltaTime;
        return Mathf.Clamp(deltaX, -MaxScrollSpeed * deltaTime, MaxScrollSpeed * deltaTime);
    }

    double GetNewScrollspeed(double deltaTime)
    {
        var upperRegion = GetNode<Area2D>(_speedRegionName);
        var lowerRegion = GetNode<Area2D>(_slowRegionName);
        List<PlayerController> livingPlayers = _players.Where(x => x.IsAlive).ToList();
        int speedyBois = livingPlayers.Count(x => upperRegion.OverlapsBody(x));
        int slowBois = livingPlayers.Count(x => lowerRegion.OverlapsBody(x));

        return _speedAccelerator.GetNewScrollSpeed(
            deltaTime, 
            livingPlayers.Count,
            speedyBois,
            slowBois,
            _speedupFactor,
            _slowdownFactor);
    }
}
