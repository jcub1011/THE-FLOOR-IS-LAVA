using Godot;
using System;

namespace Physics;

public partial class GravityApplicator : Node
{
    [Export] CharacterBody2D _body;
    [Export] float _gravity;
    bool _isEnabled;

    public override void _Ready()
    {
        base._Ready();
        _body ??= GetParent() as CharacterBody2D;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!_isEnabled) return;
        _body.Velocity += new Vector2(0f, _gravity * (float)delta);
    }

    public void SetIfEnabled(bool enabled) => _isEnabled = enabled;
}
