using UnityEngine;

public static class LoginRegister
{
    public static void build(RectTransform root)
    {
        LoginUi.view("login_register", root, LoginMode.Register);
    }
}
