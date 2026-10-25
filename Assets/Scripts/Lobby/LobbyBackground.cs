using UnityEngine;
using UnityEngine.UI;

public static class LobbyBackground
{
    const float DIM_ALPHA = 0.3f;
    const float BOTTOM_HEIGHT_RATIO = 0.45f;
    const float BOTTOM_ALPHA = 0.85f;
    const float TOP_HEIGHT_RATIO = 0.25f;
    const float TOP_ALPHA = 0.55f;

    public static void build(RectTransform root)
    {
        if (root == null) return;

        var holder = PlayUi.rect("lobby_background", root);
        holder.SetAsFirstSibling();
        PlayUi.stretch(holder);
        holder.gameObject.AddComponent<RectMask2D>();

        create_fill("bg_base", holder, PlayTheme.overlay_dark);
        create_backdrop(holder);
        create_fill("bg_dim", holder, PlayTheme.with_alpha(PlayTheme.overlay_dark, DIM_ALPHA));
        create_gradient("bg_gradient_bottom", holder, 0f, BOTTOM_HEIGHT_RATIO, BOTTOM_ALPHA, false);
        create_gradient("bg_gradient_top", holder, 1f - TOP_HEIGHT_RATIO, 1f, TOP_ALPHA, true);
    }

    static void create_fill(string name, RectTransform parent, Color color)
    {
        var fill = PlayUi.image(name, parent, null);
        fill.color = color;
        PlayUi.stretch(fill.rectTransform);
    }

    static void create_backdrop(RectTransform parent)
    {
        var sprite = PlayUi.sprite(LobbyUi.BACKDROP_PATH);

        var layer = PlayUi.rect("bg_backdrop", parent);
        layer.anchorMin = new Vector2(0.5f, 0.5f);
        layer.anchorMax = new Vector2(0.5f, 0.5f);
        layer.pivot = new Vector2(0.5f, 0.5f);
        layer.sizeDelta = Vector2.zero;

        var picture = layer.gameObject.AddComponent<Image>();
        picture.sprite = sprite;
        picture.raycastTarget = false;
        picture.enabled = sprite != null;

        var fitter = layer.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        if (sprite != null) fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
    }

    static void create_gradient(string name, RectTransform parent, float y_min, float y_max, float alpha, bool flip)
    {
        var gradient = PlayUi.image(name, parent, PlayUi.vertical_fade());
        gradient.color = PlayTheme.with_alpha(PlayTheme.overlay_dark, alpha);
        LobbyUi.place_fraction(gradient.rectTransform, 0f, y_min, 1f, y_max);
        if (flip) gradient.rectTransform.localScale = new Vector3(1f, -1f, 1f);
    }
}
