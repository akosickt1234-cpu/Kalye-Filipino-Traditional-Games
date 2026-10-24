using System;
using UnityEngine;

public static class LoginFeedback
{
    static RectTransform holder;
    static LoginToast toast_view;

    public static void build(RectTransform root)
    {
        holder = LoginUi.view("login_feedback", root, LoginModeToggle.ALL_MODES);
    }

    public static void toast(string message, bool is_error)
    {
        show_toast(message, is_error ? LoginTheme.error : LoginTheme.success, null);
    }

    public static void toast_retry(string message, Action retry)
    {
        show_toast(message, LoginTheme.error, retry);
    }

    static void show_toast(string message, Color accent, Action retry)
    {
        if (holder == null) return;

        var clean = LobbyUi.sanitize(message);
        if (clean.Length == 0) return;
        if (retry == null && toast_view != null && toast_view.has_retry && toast_view.message == clean) return;

        if (toast_view != null)
        {
            toast_view.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(toast_view.gameObject);
        }

        toast_view = LoginToast.create(holder, clean, accent, retry);
    }
}
