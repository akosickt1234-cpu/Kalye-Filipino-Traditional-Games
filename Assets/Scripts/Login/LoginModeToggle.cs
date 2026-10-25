using System;
using UnityEngine;

public class LoginModeToggle : MonoBehaviour
{
    public static readonly LoginMode[] ALL_MODES = { LoginMode.SignIn, LoginMode.Register, LoginMode.Forgot, LoginMode.Guest };

    LoginMode[] visible_modes = ALL_MODES;

    public static LoginModeToggle attach(GameObject view, params LoginMode[] modes)
    {
        var toggle = view.AddComponent<LoginModeToggle>();
        toggle.visible_modes = modes != null && modes.Length > 0 ? modes : ALL_MODES;
        LoginState.on_mode_changed += toggle.apply;
        toggle.apply(LoginState.mode);
        return toggle;
    }

    void apply(LoginMode value)
    {
        gameObject.SetActive(Array.IndexOf(visible_modes, value) >= 0);
    }

    void OnDestroy()
    {
        LoginState.on_mode_changed -= apply;
    }
}
