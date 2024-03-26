using Godot;
using Godot.NodeExtensions;
using System;

namespace UI;

public static class PauseEventChannel
{
    /// <summary>
    /// The bool parameter is true when pause is enabled and false when pause is disabled.
    /// </summary>
    public static event Action<bool> OnPauseEvent;

    /// <summary>
    /// This value is not updated until after the OnPauseEvent is invoked.
    /// </summary>
    public static bool IsPaused { get; private set; }

    public static void SetPauseState(bool paused)
    {
        OnPauseEvent?.Invoke(paused);
        IsPaused = paused;
    }
}

public partial class PauseScreen : Control
{
    [Signal] public delegate void OnRestartPressedEventHandler();
    [Signal] public delegate void OnMainMenuPressedEventHandler();
    [Signal] public delegate void OnResumePressedEventHandler();

    [Export] StringName _restartText = "Restart";
    [Export] StringName _mainMenuText = "Main Menu";
    [Export] StringName _resumeText = "Resume";
    [Export] StringName _pauseInputName = "pause";

    bool _enabled = false;

    public override void _Ready()
    {
        base._Ready();
        SetIfEnabled(_enabled);

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
        PauseEventChannel.OnPauseEvent += OnPauseCallback;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        PauseEventChannel.OnPauseEvent -= OnPauseCallback;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event.IsActionPressed(_pauseInputName)) PauseEventChannel.SetPauseState(!PauseEventChannel.IsPaused);
    }

    void OnPauseCallback(bool paused)
    {
        if (_enabled)
            Visible = paused;
        else Visible = false;
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
        Visible = false;
    }

    void OnResumeButtonPressed()
    {
        GD.Print("Resume pressed.");
        EmitSignal(SignalName.OnResumePressed);
        PauseEventChannel.SetPauseState(false);
    }

    public void SetIfEnabled(bool isEnabled)
    {
        _enabled = isEnabled;
        if (isEnabled)
        {
            Visible = PauseEventChannel.IsPaused;
        }
        else
        {
            Visible = false;
        }
    }
}
