using UnityEngine;

public static class LoginActions
{
    public static void build(RectTransform root)
    {
        LoginUi.view("login_actions", root, LoginMode.SignIn);
    }
}
