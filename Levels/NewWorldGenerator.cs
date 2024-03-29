using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldGeneration.Sections;

namespace WorldGeneration;

public partial class NewWorldGenerator : Node2D
{
    [Export] string[] _starterSections;
    SectionPreloader _preloader;
    List<LockKeySection> _activeSections;

    public override void _Ready()
    {
        base._Ready();
        _preloader = new(_starterSections.PickRandom());
        _activeSections = new()
        {
            _preloader.GetNextSection<LockKeySection>()
        };

        AddChild(_activeSections.First());
        _activeSections.First().GlobalPosition = Vector2.Zero;
        GetNextSection();
    }

    void GetNextSection()
    {
        var newSection = _preloader.GetNextSection<LockKeySection>();
        AddChild(newSection);
        MoveChild(newSection, 1);
        newSection.StitchTo(_activeSections.Last());
        _activeSections.Add(newSection);
    }
}
