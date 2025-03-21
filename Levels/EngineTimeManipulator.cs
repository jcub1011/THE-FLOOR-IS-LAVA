using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

namespace WorldGeneration;

public readonly struct TimescaleTransition
{
    public readonly double TargetTimeScale;
    public readonly double TransitionTime;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetTimeScale">Clamped from 0.0001 to 1.</param>
    /// <param name="transitionTime"></param>
    public TimescaleTransition(double targetTimeScale, double transitionTime)
    {
        TargetTimeScale = Mathf.Clamp(targetTimeScale, 0.0001, 1);
        TransitionTime = Mathf.Clamp(transitionTime, 0.0001, double.PositiveInfinity);
    }

    /// <summary>
    /// How long to wait before performing the next queued transition.
    /// </summary>
    /// <param name="wait"></param>
    public TimescaleTransition(double wait)
    {
        TransitionTime = Mathf.Clamp(wait, 0, double.PositiveInfinity);
        TargetTimeScale = double.NaN;
    }
}

public partial class EngineTimeManipulator : Node
{
    #region Static
    static EngineTimeManipulator Instance { get; set; }

    static Queue<TimescaleTransition> _transitions = new();
    static double _timescaleROC;
    static double _remainingTransitionTime;
    static double _targetTimescale;

    static void UpdateTimeInfo(TimescaleTransition transition)
    {
        _targetTimescale = transition.TargetTimeScale;
        _remainingTransitionTime = transition.TransitionTime;

        if (!double.IsNaN(_targetTimescale))
        {
            _timescaleROC = (Engine.TimeScale - _targetTimescale)
                / _remainingTransitionTime;
            GD.Print(_timescaleROC);
        }
    }

    /// <summary>
    /// Adds the transition to the queue.
    /// </summary>
    /// <param name="transition"></param>
    public static void QueueTimeTransition(TimescaleTransition transition)
    {
        _transitions.Enqueue(transition);
    }

    /// <summary>
    /// Skips the queue and instantly performs the transition. This removes 
    /// any transitions currently in the queue.
    /// </summary>
    /// <param name="transition"></param>
    public static void OverrideTimeTransition(TimescaleTransition transition)
    {
        _transitions.Clear();
        UpdateTimeInfo(transition);
    }
    #endregion

    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        Engine.TimeScale = 1;
        if (Instance == this) Instance = null;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        CallDeferred("UpdateEngineSpeed", delta, Engine.TimeScale);
    }

    void UpdateEngineSpeed(double delta, double timeScale)
    {
        // Protect against duplicate instances.
        if (Instance != this) return;

        if (_remainingTransitionTime <= 0)
        {
            if (_transitions.Count == 0)
            {
                return;
            }
            else
            {
                UpdateTimeInfo(_transitions.Dequeue());
            }
        }

        double trueDelta = delta / timeScale;
        _remainingTransitionTime -= trueDelta;

        if (double.IsNaN(_targetTimescale)) return;
        timeScale -= _timescaleROC * trueDelta;
        if (_timescaleROC < 0)
        {
            if (timeScale > _targetTimescale) timeScale = _targetTimescale;
        }
        else
        {
            if (timeScale < _targetTimescale) timeScale = _targetTimescale;
        }
        timeScale = Mathf.Clamp(timeScale, 0.0001, 1);
        //GD.Print($"NewTimeScale: {timeScale}");
        Engine.TimeScale = timeScale;
    }
}
