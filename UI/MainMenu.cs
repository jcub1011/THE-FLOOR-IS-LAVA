using Godot;
using System;

namespace UI;

public partial class MainMenu : Control
{
    [Export] Container StartPage;
    [Export] Control TutorialPage;

    public void OnShowTutorial()
    {
        StartPage.Visible = false;
        TutorialPage.Visible = true;
    }

    public void OnHideTutorial()
    {
        StartPage.Visible = true;
        TutorialPage.Visible = false;
    }
}
