using Godot;

namespace Players;

public class InputToDirectionConverter
{
    bool _leftPressed;
    bool _rightPressed;
    bool _upPressed;
    bool _downPressed;

    public void UpdateDirection(StringName input, bool pressed)
    {
        if (input == InputNames.LEFT) _leftPressed = pressed;
        else if (input == InputNames.RIGHT) _rightPressed = pressed;
        else if (input == InputNames.JUMP) _upPressed = pressed;
        else if (input == InputNames.CROUCH) _downPressed = pressed;
    }

    public Vector2 GetDirection()
    {
        Vector2 direction = new();

        if (_leftPressed) direction.X--;
        if (_rightPressed) direction.X++;
        if (_upPressed) direction.Y--;
        if (_downPressed) direction.Y++;

        return direction.LengthSquared() == 0f ? Vector2.Zero : direction.Normalized();
    }
}
