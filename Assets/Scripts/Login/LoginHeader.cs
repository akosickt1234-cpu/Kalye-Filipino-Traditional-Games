using UnityEngine;

public static class LoginHeader
{
    public static void build(RectTransform root)
    {
        LoginUi.view("login_header", root, LoginModeToggle.ALL_MODES);
    }
}
