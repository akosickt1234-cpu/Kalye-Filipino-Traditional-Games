using UnityEngine;

public static class LoginForgot
{
    public static void build(RectTransform root)
    {
        LoginUi.view("login_forgot", root, LoginMode.Forgot);
    }
}
