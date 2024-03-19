using Godot;
using System;
namespace TheFloorIsLava.Subscriptions;

public static class OriginShiftChannel
{
    public static event Action<Vector2> OriginShifted;

    public static void ShiftOrigin(Vector2 shift)
    {
        //GD.Print($"Shifting Origin: {shift}");
        OriginShifted?.Invoke(shift);
    }
}
