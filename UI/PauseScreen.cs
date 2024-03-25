using Godot;
using System;

namespace UI;

public partial class PauseScreen : Control
{
    [Signal] public delegate void OnRestartPressedEventHandler();
    [Signal] public delegate void OnMainMenuPressedEventHandler();
    [Signal] public delegate void OnResumePressedEventHandler();

    [Export] StringName _restartText = "Restart";
    [Export] StringName _mainMenuText = "Main Menu";
    [Export] StringName _resumeText = "Resume";

    public override void _Ready()
    {
        base._Ready();
    }
}
