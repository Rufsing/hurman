using Godot;

namespace Hurman.Core;

/// <summary>
/// Registers all input actions in code (keyboard/mouse + gamepad), so the input map
/// lives in version control as readable C# instead of serialized project settings.
/// </summary>
public static class InputSetup
{
    public static void Register()
    {
        // Movement
        AddKey("move_forward", Key.W); AddAxis("move_forward", JoyAxis.LeftY, -1);
        AddKey("move_back", Key.S); AddAxis("move_back", JoyAxis.LeftY, 1);
        AddKey("move_left", Key.A); AddAxis("move_left", JoyAxis.LeftX, -1);
        AddKey("move_right", Key.D); AddAxis("move_right", JoyAxis.LeftX, 1);
        AddKey("sprint", Key.Shift); AddButton("sprint", JoyButton.LeftStick);
        AddKey("crouch", Key.C); AddButton("crouch", JoyButton.B);
        AddKey("jump", Key.Space); AddButton("jump", JoyButton.A);

        // Camera (gamepad; the mouse is handled directly)
        AddAxis("look_left", JoyAxis.RightX, -1);
        AddAxis("look_right", JoyAxis.RightX, 1);
        AddAxis("look_up", JoyAxis.RightY, -1);
        AddAxis("look_down", JoyAxis.RightY, 1);

        // Combat
        AddMouse("aim", MouseButton.Right); AddAxis("aim", JoyAxis.TriggerLeft, 1);
        AddMouse("fire", MouseButton.Left); AddAxis("fire", JoyAxis.TriggerRight, 1);
        AddKey("reload", Key.R); AddButton("reload", JoyButton.X);

        // World
        AddKey("interact", Key.E); AddButton("interact", JoyButton.Y);
        AddKey("pause", Key.Escape); AddButton("pause", JoyButton.Start);

        // Debug
        AddKey("debug_reset", Key.F9);
    }

    private static void Ensure(string action)
    {
        if (!InputMap.HasAction(action))
            InputMap.AddAction(action, 0.2f);
    }

    private static void AddKey(string action, Key key)
    {
        Ensure(action);
        InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }

    private static void AddMouse(string action, MouseButton button)
    {
        Ensure(action);
        InputMap.ActionAddEvent(action, new InputEventMouseButton { ButtonIndex = button });
    }

    private static void AddButton(string action, JoyButton button)
    {
        Ensure(action);
        InputMap.ActionAddEvent(action, new InputEventJoypadButton { ButtonIndex = button });
    }

    private static void AddAxis(string action, JoyAxis axis, float direction)
    {
        Ensure(action);
        InputMap.ActionAddEvent(action, new InputEventJoypadMotion { Axis = axis, AxisValue = direction });
    }
}
