using System;
using UnityEngine;

public static class LoginFeedback
{
    public static void build(RectTransform root)
    {
        LoginUi.view("login_feedback", root, LoginModeToggle.ALL_MODES);
    }

    public static void toast(string message, bool is_error)
    {
        var clean = LobbyUi.sanitize(message);
        if (is_error) Debug.LogWarning($"login: {clean}");
        else Debug.Log($"login: {clean}");
    }

    public static void ask_guest_name(Action<string> on_name)
    {
        Debug.LogWarning("login: guest name prompt is not built yet");
    }
}
