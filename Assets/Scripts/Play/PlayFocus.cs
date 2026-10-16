using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public static class PlayFocus
{
    const float STICK_THRESHOLD = 0.5f;

    static bool show_focus_value;

    public static bool show_focus
    {
        get { return show_focus_value; }
    }

    public static GameObject default_target { get; set; }

    public static bool lit(bool hovered, bool focused)
    {
        return hovered || (focused && show_focus_value);
    }

    public static void clear_selection()
    {
        var event_system = EventSystem.current;
        if (event_system != null) event_system.SetSelectedGameObject(null);
    }

    public static void reset()
    {
        show_focus_value = false;
        default_target = null;
    }

    public static void tick()
    {
        if (pointer_used())
        {
            show_focus_value = false;
            return;
        }

        if (!navigation_used()) return;

        var was_showing = show_focus_value;
        show_focus_value = true;
        if (!was_showing) select_default_target();
    }

    static bool pointer_used()
    {
        var mouse = Mouse.current;
        if (mouse == null) return false;

        return mouse.delta.ReadValue().sqrMagnitude > 0f
            || mouse.leftButton.wasPressedThisFrame
            || mouse.rightButton.wasPressedThisFrame
            || mouse.middleButton.wasPressedThisFrame;
    }

    static bool navigation_used()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null
            && (keyboard.upArrowKey.wasPressedThisFrame
                || keyboard.downArrowKey.wasPressedThisFrame
                || keyboard.leftArrowKey.wasPressedThisFrame
                || keyboard.rightArrowKey.wasPressedThisFrame
                || keyboard.tabKey.wasPressedThisFrame))
        {
            return true;
        }

        var gamepad = Gamepad.current;
        if (gamepad == null) return false;

        return gamepad.dpad.up.wasPressedThisFrame
            || gamepad.dpad.down.wasPressedThisFrame
            || gamepad.dpad.left.wasPressedThisFrame
            || gamepad.dpad.right.wasPressedThisFrame
            || gamepad.leftStick.ReadValue().sqrMagnitude > STICK_THRESHOLD * STICK_THRESHOLD;
    }

    static void select_default_target()
    {
        var event_system = EventSystem.current;
        if (event_system == null || event_system.currentSelectedGameObject != null) return;

        var target = default_target;
        if (target == null || !target.activeInHierarchy) return;

        event_system.SetSelectedGameObject(target);
    }
}
