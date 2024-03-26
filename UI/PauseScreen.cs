using Godot;
using Godot.NodeExtensions;
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

        var buttons = this.GetChildren<Button>();
        foreach(var button in buttons)
        {
            if (button.Text == _restartText)
            {
                button.Pressed += OnRestartButtonPressed;
            }
            else if (button.Text == _mainMenuText)
            {
                button.Pressed += OnMainMenuButtonPressed;
            }
            else if (button.Text == _resumeText)
            {
                button.Pressed += OnResumeButtonPressed;
            }
        }
    }

    void OnRestartButtonPressed()
    {
        GD.Print("Restart pressed.");
        EmitSignal(SignalName.OnRestartPressed);
    }

    void OnMainMenuButtonPressed()
    {
        GD.Print("Main menu pressed.");
        EmitSignal(SignalName.OnMainMenuPressed);
    }

    void OnResumeButtonPressed()
    {
        GD.Print("Resume pressed.");
        EmitSignal(SignalName.OnResumePressed);
    }
}
