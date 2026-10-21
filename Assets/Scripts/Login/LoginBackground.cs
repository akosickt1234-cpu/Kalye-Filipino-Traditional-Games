using UnityEngine;
using UnityEngine.UI;

public static class LoginBackground
{
    public static void build(RectTransform root)
    {
        if (root == null) return;

        var holder = PlayUi.rect("login_background", root);
        PlayUi.stretch(holder);

        var blur_holder = PlayUi.rect("login_blur", holder);
        PlayUi.stretch(blur_holder);
        var blur = blur_holder.gameObject.AddComponent<RawImage>();
        blur.raycastTarget = false;
        blur.enabled = false;

        var dim = PlayUi.image("login_dim", holder, null);
        dim.color = PlayTheme.with_alpha(PlayTheme.overlay_dark, LoginTheme.BLUR_DIM_ALPHA);
        dim.enabled = false;
        PlayUi.stretch(dim.rectTransform);

        var capture = root.gameObject.GetComponent<LoginBlurCapture>();
        if (capture == null) capture = root.gameObject.AddComponent<LoginBlurCapture>();
        capture.begin(blur, dim);
    }
}
