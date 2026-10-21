using UnityEngine;
using UnityEngine.UI;

public static class LoginHeader
{
    const string LOGO_PATH = "Background/Menu/MenuLogo";

    const float LOGO_BOTTOM = -0.3f;
    const float LOGO_TOP = 0.85f;

    const float EYEBROW_BOTTOM = 0.78f;
    const float EYEBROW_TOP = 0.98f;
    const float LINE_LEFT_START = 0.02f;
    const float LINE_LEFT_END = 0.2f;
    const float LINE_RIGHT_START = 0.8f;
    const float LINE_RIGHT_END = 0.98f;
    const float DIAMOND_LEFT_X = 0.225f;
    const float DIAMOND_RIGHT_X = 0.775f;
    const float DIAMOND_HALF_HEIGHT = 0.14f;
    const float TEXT_LEFT = 0.25f;
    const float TEXT_RIGHT = 0.75f;
    const float LINE_THICKNESS = 2f;
    const float LINE_ALPHA = 0.85f;
    const int TEXT_SIZE = 26;
    const int TEXT_MIN_SIZE = 10;

    public static void build(RectTransform root)
    {
        var view = LoginUi.view("login_header", root, LoginModeToggle.ALL_MODES);
        if (view == null) return;

        build_logo(view);
        build_eyebrow(view);
    }

    static void build_logo(RectTransform view)
    {
        var logo = PlayUi.image("login_logo", view, PlayUi.sprite(LOGO_PATH));
        logo.preserveAspect = true;
        logo.raycastTarget = false;
        LobbyUi.place_fraction(logo.rectTransform, 0f, LOGO_BOTTOM, 1f, LOGO_TOP);
    }

    static void build_eyebrow(RectTransform view)
    {
        var row = PlayUi.rect("login_eyebrow", view);
        LobbyUi.place_fraction(row, 0f, EYEBROW_BOTTOM, 1f, EYEBROW_TOP);

        build_line("login_eyebrow_line_left", row, LINE_LEFT_START, LINE_LEFT_END);
        build_line("login_eyebrow_line_right", row, LINE_RIGHT_START, LINE_RIGHT_END);
        build_diamond("login_eyebrow_diamond_left", row, DIAMOND_LEFT_X);
        build_diamond("login_eyebrow_diamond_right", row, DIAMOND_RIGHT_X);

        var label = PlayUi.text("login_eyebrow_text", row, LoginHeaderEyebrow.text_for(LoginState.mode), TEXT_SIZE, PlayTheme.gold, TextAnchor.MiddleCenter);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = TEXT_MIN_SIZE;
        label.resizeTextMaxSize = TEXT_SIZE;
        LobbyUi.place_fraction(label.rectTransform, TEXT_LEFT, 0f, TEXT_RIGHT, 1f);
        LoginHeaderEyebrow.attach(label);
    }

    static void build_line(string name, RectTransform row, float x_min, float x_max)
    {
        var line = PlayUi.image(name, row, null);
        line.color = PlayTheme.with_alpha(PlayTheme.gold, LINE_ALPHA);
        PlayUi.place(line.rectTransform, new Vector2(x_min, 0.5f), new Vector2(x_max, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, LINE_THICKNESS));
    }

    static void build_diamond(string name, RectTransform row, float x)
    {
        var diamond = PlayUi.image(name, row, null);
        diamond.color = PlayTheme.gold;
        var rect = diamond.rectTransform;
        rect.anchorMin = new Vector2(x, 0.5f - DIAMOND_HALF_HEIGHT);
        rect.anchorMax = new Vector2(x, 0.5f + DIAMOND_HALF_HEIGHT);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localRotation = Quaternion.Euler(0f, 0f, 45f);

        var fitter = diamond.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        fitter.aspectRatio = 1f;
    }
}

public class LoginHeaderEyebrow : MonoBehaviour
{
    Text label;

    public static string text_for(LoginMode mode)
    {
        switch (mode)
        {
            case LoginMode.Register: return "CREATE ACCOUNT";
            case LoginMode.Forgot: return "FORGOT PASSWORD";
            default: return "WELCOME BACK";
        }
    }

    public static void attach(Text target)
    {
        var host = target.gameObject.AddComponent<LoginHeaderEyebrow>();
        host.label = target;
        LoginState.on_mode_changed += host.apply;
    }

    void apply(LoginMode mode)
    {
        if (label != null) label.text = text_for(mode);
    }

    void OnDestroy()
    {
        LoginState.on_mode_changed -= apply;
    }
}
