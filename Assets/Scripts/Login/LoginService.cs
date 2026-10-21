using System;

public static class LoginService
{
    const string NOT_READY = "Login is not available yet.";

    public static void login(string identifier, string password, Action on_ok, Action<string> on_fail)
    {
        fail(on_fail);
    }

    public static void guest(string name, Action on_ok, Action<string> on_fail)
    {
        fail(on_fail);
    }

    public static void register(string username, string email, string password, Action on_ok, Action<string> on_fail)
    {
        fail(on_fail);
    }

    public static void forgot(string email, Action on_ok, Action<string> on_fail)
    {
        fail(on_fail);
    }

    public static void restore(Action on_ok, Action<string> on_fail)
    {
        fail(on_fail);
    }

    public static void sign_out()
    {
        LoginSession.clear();
        LoginStore.clear();
    }

    static void fail(Action<string> on_fail)
    {
        if (on_fail != null) on_fail(NOT_READY);
    }
}
