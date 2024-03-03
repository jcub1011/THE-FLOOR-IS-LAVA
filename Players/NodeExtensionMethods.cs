using Godot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Players;

public static class NodeExtensionMethods
{
    public static float GetDistance(this Node2D src, Node2D dest)
    {
        return (src.GlobalPosition - dest.GlobalPosition).Length();
    }

    public static float GetXDistance(this Node2D src, Node2D dest)
    {
        return Mathf.Abs(src.GlobalPosition.X - dest.GlobalPosition.X);
    }

    public static Vector2 GetDisplacement(this Node2D src, Node2D dest)
    {
        return src.GlobalPosition - dest.GlobalPosition;
    }

    public static Vector2 RelativeTo(this Vector2 src, Vector2 origin)
    {
        return src - origin;
    }

    /// <summary>
    /// Is not normalized.
    /// </summary>
    /// <param name="src"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public static Vector2 GetDirectionFrom(this Node2D src, Vector2 origin)
    {
        return src.GlobalPosition - origin;
    }

    public static void PlayIfNotPlaying(this AnimationPlayer player, StringName animation)
    {
        if (player.CurrentAnimation != animation) player.Play(animation);
    }

    /// <summary>
    /// Returns true if the animation exists.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="animation"></param>
    /// <param name="restartIfAlreadyPlaying"></param>
    /// <returns></returns>
    public static bool PlayIfExists(
        this AnimationPlayer player,
        StringName animation,
        bool restartIfAlreadyPlaying = false)
    {
        if (animation == null
            || !player.HasAnimation(animation)) return false;
        else
        {
            if (restartIfAlreadyPlaying) player.Play(animation);
            else player.PlayIfNotPlaying(animation);
            return true;
        }
    }

    /// <summary>
    /// Sets the disabled property of the children collision shapes to the 
    /// value provided.
    /// </summary>
    /// <param name="area"></param>
    /// <param name="disabled"></param>
    public static void SetColliderState(this Area2D area, bool disabled)
    {
        area.Monitoring = !disabled;
        area.Monitorable = !disabled;

        foreach (var child in area.GetChildren())
        {
            if (child is CollisionShape2D obj)
            {
                obj.Disabled = disabled;
            }
        }
    }

    public static string ConvertToFilename(string godotPath)
    {
        return godotPath.Split('/').Last();
    }

    public static Vector2 GetViewportSize()
    {
        return new Vector2(
            (float)ProjectSettings.GetSetting("display/window/size/viewport_width"),
            (float)ProjectSettings.GetSetting("display/window/size/viewport_height"));
    }
}

public static class MathExtensions
{
    /// <summary>
    /// Assumes velocities are constant and directions don't change.
    /// Thanks to https://www.codeproject.com/articles/990452/interception-of-two-moving-objects-in-d-space 
    /// for the algorithm.
    /// </summary>
    /// <param name="currentSpeed"></param>
    /// <param name="currentPosition"></param>
    /// <param name="targetPostition"></param>
    /// <param name="targetVelocity"></param>
    /// <returns></returns>
    public static Vector2 CalculateHomingDirection(float currentSpeed,
        Vector2 currentPosition, Vector2 targetPosition, Vector2 targetVelocity)
    {
        Vector2 dirFromTarget = currentPosition.RelativeTo(targetPosition);
        float distance = dirFromTarget.Length();
        float targetSpeed = targetVelocity.Length();

        if (distance < 0.1f)
        {
            return Vector2.Zero;
        }
        else if (targetSpeed < 0.1f || currentSpeed < 0.1f)
        {
            return targetPosition.RelativeTo(currentPosition);
        }

        float a = currentSpeed * currentSpeed - targetSpeed * targetSpeed;
        float b = 2 * dirFromTarget.Dot(targetVelocity);
        float c = -(distance * distance);
        QuadraticSolver(a, b, c, out var root1, out var root2);
        if (root1 < 0f && root2 < 0f)
        {
            return targetPosition.RelativeTo(currentPosition);
        }

        float timeToIntercept;
        if (root1 > 0f && root2 > 0f)
        {
            timeToIntercept = Mathf.Min(root1, root2);
        }
        else
        {
            timeToIntercept = Mathf.Max(root1, root2);
        }

        Vector2 interceptPos = targetPosition + targetVelocity * timeToIntercept;
        return interceptPos.RelativeTo(currentPosition);
    }

    /// <summary>
    /// Returns false if the solution has complex numbers.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="x1">The negative sqr root.</param>
    /// <param name="x2">The positive sqr root.</param>
    /// <returns></returns>
    public static bool QuadraticSolver(float a, float b, float c,
        out float x1, out float x2)
    {
        float preRoot = b * b - 4 * a * c;
        if (preRoot < 0f)
        {
            x1 = float.NaN;
            x2 = float.NaN;
            return false;
        }
        else
        {
            float root = Mathf.Sqrt(preRoot);
            x1 = (-root - b) / (2f * a);
            x2 = (root - b) / (2f * a);
            return true;
        }
    }

    /// <summary>
    /// Gets all children of the given type via depth first search. 
    /// Note: This function does not check children of a node that 
    /// is the target type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="node"></param>
    /// <returns></returns>
    public static List<T> GetChildren<T>(this Node node)
    {
        List<T> targets = new();

        foreach(var child in node.GetChildren())
        {
            if (child is T target) targets.Add(target);
            else targets.AddRange(node.GetChildren<T>());
        }

        return targets;
    }
}
