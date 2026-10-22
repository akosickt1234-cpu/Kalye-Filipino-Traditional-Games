using System;

public enum LoginMode
{
    SignIn,
    Register,
    Forgot
}

public static class LoginState
{
    public static event Action<LoginMode> on_mode_changed;
    public static event Action on_submit;
    public static event Action<bool> on_busy_changed;

    static LoginMode current_mode;

    public static LoginMode mode
    {
        get { return current_mode; }
    }

    public static bool is_busy { get; private set; }

    public static void set_mode(LoginMode value)
    {
        if (is_busy || current_mode == value) return;

        current_mode = value;
        if (on_mode_changed != null) on_mode_changed(value);
    }

    public static void set_busy(bool value)
    {
        if (is_busy == value) return;

        is_busy = value;
        if (on_busy_changed != null) on_busy_changed(value);
    }

    public static void submit()
    {
        if (is_busy) return;
        if (on_submit != null) on_submit();
    }

    public static void reset()
    {
        current_mode = LoginMode.SignIn;
        is_busy = false;
    }
}
