using System;
using System.Collections.Generic;
using System.Linq;

namespace Godot.NodeExtensions
{
    public static class NodeExtensions
    {
        #region AnimationPlayer Extensions
        /// <summary>
        /// Plays the provided animation if it isn't already playing.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="animation">Name of the animation to play.</param>
        public static void PlayIfNotPlaying(this AnimationPlayer player, StringName animation)
        {
            if (player.CurrentAnimation != animation) player.Play(animation);
        }

        /// <summary>
        /// Attempts to play the animation and returns true if the animation exists.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="animation">Name of the animation to play.</param>
        /// <param name="restartIfAlreadyPlaying">Starts the animation from the start when true.</param>
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
        #endregion

        #region Node Extensions
        /// <summary>
        /// Gets all children of the given type via depth first search. 
        /// Note: This function excludes children of a node that 
        /// is the target type. See <c>GetChildrenThorough()</c> if you do want that.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static List<T> GetChildren<T>(this Node node) where T : class
        {
            List<T> targets = new();

            foreach (var child in node.GetChildren())
            {
                if (child is T target) targets.Add(target);
                else targets.AddRange(child.GetChildren<T>());
            }

            return targets;
        }

        /// <summary>
        /// Gets all children of the given type via depth first search.
        /// Note: This also includes children of a node that is the target type. 
        /// See <c>GetChildren()</c> if you don't want that.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static List<T> GetChildrenThorough<T>(this Node node) where T : class
        {
            List<T> targets = new();

            foreach (var child in node.GetChildren())
            {
                if (child is T target) targets.Add(target);
                targets.AddRange(child.GetChildren<T>());
            }

            return targets;
        }

        /// <summary>
        /// Finds the first child of the given type. Uses breadth first search.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static T GetChild<T>(this Node node)
        {
            Queue<Node> queue = new();
            queue.Enqueue(node);

            while (queue.Count > 0)
            {
                if (queue.Peek() is T target) return target;

                foreach (var child in queue.Dequeue().GetChildren())
                {
                    queue.Enqueue(child);
                }
            }

            return default;
        }

        /// <summary>
        /// Gets the first direct child of type T. This is a faster method for getting 
        /// a child when you know the node is a direct child.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static T GetDirectChild<T>(this Node node) where T : class
        {
            foreach (var child in node.GetChildren())
            {
                if (child is T target) return target;
            }
            return default;
        }

        /// <summary>
        /// Gets the direct children matching the type T. 
        /// Faster than <c>GetChildren()</c> when you know the nodes are direct children.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static List<T> GetDirectChildren<T>(this Node node) where T : class
        {
            List<T> values = new();

            foreach (var child in node.GetChildren())
            {
                if (child is T target)
                    values.Add(target);
            }

            return values;
        }

        /// <summary>
        /// Gets the first sibling matching the type T, where it returns 
        /// null if the sibling does not exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetSibling<T>(this Node node) where T : class
        {
            return node.GetParent().GetDirectChild<T>();
        }

        /// <summary>
        /// Gets all of the siblings matching the type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <returns></returns>
        public static List<T> GetSiblings<T>(this Node node) where T : class
        {
            List<T> returnVals = new();

            foreach (var sibling in node.GetParent().GetChildren())
            {
                if (sibling is T target) returnVals.Add(target);
            }

            return returnVals;
        }

        /// <summary>
        /// Inserts the given child at the given index. 
        /// NOTE: The given node cannot be an existing child of this node. 
        /// If you want to change the order of children make sure to remove 
        /// the node you are moving beforehand.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="index"></param>
        /// <exception cref="ArgumentException"></exception>
        //public static void InsertChild(this Node parent, Node child, int index)
        //{
        //    var children = parent.GetChildren();

        //    foreach(var oldChild in children)
        //    {
        //        if (child.Equals(oldChild)) 
        //            throw new ArgumentException($"Node {child.Name} is already a child of this node.");
        //        parent.RemoveChild(oldChild);
        //    }

        //    for (int i = 0; i < children.Count; i++)
        //    {
        //        if (i == index)
        //        {
        //            parent.AddChild(child);
        //        }
        //        parent.AddChild(children[i]);
        //    }
        //}
        #endregion

        #region Node2D Extensions
        /// <summary>
        /// Returns the distance between the origin of this node and the given node.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static float GetDistance(this Node2D src, Node2D dest)
        {
            return (src.GlobalPosition - dest.GlobalPosition).Length();
        }

        /// <summary>
        /// Gets the displacement between the origin of this node and the given node.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static Vector2 GetDisplacement(this Node2D src, Node2D dest)
        {
            return src.GlobalPosition - dest.GlobalPosition;
        }

        /// <summary>
        /// Get a non-normalized direction vector from the provided origin to this node.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static Vector2 GetDirectionFrom(this Node2D src, Vector2 origin)
        {
            return src.GlobalPosition - origin;
        }
        #endregion

        #region CharacterBody2D Extensions
        /// <summary>
        /// Gets all the collisions since the last call to MoveAndSlide().
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static KinematicCollision2D[] GetCollisions(this CharacterBody2D body)
        {
            KinematicCollision2D[] values = new KinematicCollision2D[body.GetSlideCollisionCount()];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = body.GetSlideCollision(i);
            }
            return values;
        }
        #endregion

        #region Rect Extensions
        /// <summary>
        /// Gets the bottom y position of the rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static float GetBottomY(this Rect2 rect)
            => rect.Position.Y + rect.Size.Y;

        /// <summary>
        /// Gets the top y position of the rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static float GetTopY(this Rect2 rect)
            => rect.Position.Y;

        /// <summary>
        /// Gets the midpoint of the rectangle.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static Vector2 GetMidpoint(this Rect2 rect)
            => rect.Position + rect.Size / 2f;

        /// <summary>
        /// Size must be set before hand. This does not update when size is changed.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="midpoint"></param>
        public static void SetMidpoint(this Rect2 rect, Vector2 midpoint)
        {
            rect.Position = midpoint - rect.Size / 2f;
        }
        #endregion
    }
}

namespace Godot.GodotExtensions
{
    public static class GodotExtensions
    {
        /// <summary>
        /// Returns the viewport size defined in project settings.
        /// </summary>
        /// <returns></returns>
        public static Vector2 GetViewportSize()
        {
            return new Vector2(
                (float)ProjectSettings.GetSetting("display/window/size/viewport_width"),
                (float)ProjectSettings.GetSetting("display/window/size/viewport_height"));
        }

        /// <summary>
        /// Converts the godotPath to a filename.
        /// </summary>
        /// <param name="godotPath"></param>
        /// <returns></returns>
        public static string ConvertToFilename(string godotPath)
        {
            return godotPath.Split('/').Last();
        }
    }
}

namespace Godot.MathExtensions
{
    public static class MathExtensions
    {
        #region Vector2 Extensions
        /// <summary>
        /// Gets the lowest value of the vector components.
        /// </summary>
        /// <param name="vect"></param>
        /// <returns></returns>
        public static float MinVal(this Vector2 vect)
        {
            return Mathf.Min(vect.X, vect.Y);
        }

        /// <summary>
        /// Gets the highest value of the vector components.
        /// </summary>
        /// <param name="vect"></param>
        /// <returns></returns>
        public static float MaxVal(this Vector2 vect)
        {
            return Mathf.Max(vect.X, vect.Y);
        }

        /// <summary>
        /// Returns the vector position relative to the provided origin.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public static Vector2 RelativeTo(this Vector2 src, Vector2 origin)
        {
            return src - origin;
        }

        /// <summary>
        /// Shamelessly stolen from https://stackoverflow.com/questions/61372498/how-does-mathf-smoothdamp-work-what-is-it-algorithm.
        /// </summary>
        /// <param name="current">Current value.</param>
        /// <param name="target">Target value.</param>
        /// <param name="currentVel">Current velocity from current to target.</param>
        /// <param name="smoothTime">How long it should take to reach the target value.</param>
        /// <param name="deltaTime">Time since last call.</param>
        /// <param name="maxSpeed">Max speed.</param>
        /// <returns>The smooth damped value.</returns>
        public static Vector2 SmoothDamp(this Vector2 current, Vector2 target, ref Vector2 currentVel, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
        {
            return new Vector2(
                current.X.SmoothDamp(target.X, ref currentVel.X, smoothTime, deltaTime, maxSpeed),
                current.Y.SmoothDamp(target.Y, ref currentVel.Y, smoothTime, deltaTime, maxSpeed)
                );
        }
        #endregion

        #region General Math Extensions
        /// <summary>
        /// Gets the normalized position between the range, where the midpoint is 0, min is -1, and max is 1.
        /// </summary>
        /// <param name="curValue"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float GetNormalizedValInRange(float curValue, float min, float max)
        {
            float halfRange = Mathf.Abs(max - min) / 2f;
            float displacement = curValue - (max + min) / 2f;
            if (Mathf.Abs(displacement) > halfRange)
                return displacement < 0 ? -1f : 1f;
            else return displacement / halfRange;
        }

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
        /// Shamelessly stolen from https://stackoverflow.com/questions/61372498/how-does-mathf-smoothdamp-work-what-is-it-algorithm.
        /// </summary>
        /// <param name="current">Current value.</param>
        /// <param name="target">Target value.</param>
        /// <param name="currentVel">Current velocity from current to target.</param>
        /// <param name="smoothTime">How long it should take to reach the target value.</param>
        /// <param name="deltaTime">Time since last call.</param>
        /// <param name="maxSpeed">Max speed.</param>
        /// <returns>The smooth damped value.</returns>
        public static float SmoothDamp(this float current, float target, ref float currentVel, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
        {
            smoothTime = Mathf.Max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);
            float change = current - target;
            float originalTo = target;

            // Clamp maximum speed
            float maxChange = maxSpeed * smoothTime;
            change = Mathf.Clamp(change, -maxChange, maxChange);
            target = current - change;

            float temp = (currentVel + omega * change) * deltaTime;
            currentVel = (currentVel - omega * temp) * exp;
            float output = target + (change + temp) * exp;

            // Prevent overshooting
            if (originalTo - current > 0.0F == output > originalTo)
            {
                output = originalTo;
                currentVel = (output - originalTo) / deltaTime;
            }

            return output;
        }
        #endregion
    }
}