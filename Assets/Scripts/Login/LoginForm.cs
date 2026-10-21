using UnityEngine;

public static class LoginForm
{
    public static void build(RectTransform root)
    {
        LoginUi.view("login_form", root, LoginMode.SignIn);
    }
}
