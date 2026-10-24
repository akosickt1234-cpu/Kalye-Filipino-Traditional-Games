using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class LoginUi
{
    // InputFieldFrameClear.png is the user's frame art with the navy backdrop erased to
    // transparency and the content pre-cropped; sliced borders keep crisp edges at any
    // field aspect.
    const float INPUT_BORDER_SCALE = 3f;
    static readonly Vector4 input_art_border = new Vector4(56f, 34f, 56f, 34f);

    static Sprite frame_sprite_cache;

    public static RectTransform view(string name, RectTransform parent, params LoginMode[] modes)
    {
        if (parent == null) return null;

        var holder = PlayUi.rect(name, parent);
        PlayUi.stretch(holder);
        LoginModeToggle.attach(holder.gameObject, modes);
        return holder;
    }

    public static Image rounded_box(string name, Transform parent, Color border_color, Color fill_color, float border_width, bool raycast)
    {
        var ring = PlayUi.image(name, parent, PlayUi.pill(), raycast);
        ring.type = Image.Type.Sliced;
        ring.pixelsPerUnitMultiplier = LoginTheme.CORNER_SCALE;
        ring.color = border_color;

        var fill = PlayUi.image(name + "_fill", ring.rectTransform, PlayUi.pill());
        fill.type = Image.Type.Sliced;
        fill.pixelsPerUnitMultiplier = LoginTheme.CORNER_SCALE;
        fill.color = fill_color;
        PlayUi.stretch(fill.rectTransform);
        fill.rectTransform.offsetMin = new Vector2(border_width, border_width);
        fill.rectTransform.offsetMax = new Vector2(-border_width, -border_width);
        return ring;
    }

    public static Image input_box(string name, Transform parent, bool raycast)
    {
        var sprite = input_frame();
        if (sprite == null) return rounded_box(name, parent, LoginTheme.field_border, LoginTheme.field_fill, LoginTheme.FIELD_BORDER, raycast);

        var ring = PlayUi.image(name, parent, sprite, raycast);
        ring.type = Image.Type.Sliced;
        ring.pixelsPerUnitMultiplier = INPUT_BORDER_SCALE;
        ring.color = PlayTheme.white;
        return ring;
    }

    static Sprite input_frame()
    {
        if (frame_sprite_cache != null) return frame_sprite_cache;

        var source = PlayUi.sprite(LoginTheme.INPUT_FRAME_PATH);
        if (source == null) return null;

        var tex = source.texture;
        var region = new Rect(0f, 0f, tex.width, tex.height);
        frame_sprite_cache = Sprite.Create(tex, region, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, input_art_border);
        frame_sprite_cache.name = "login_input_frame";
        return frame_sprite_cache;
    }

    public static Image icon(string name, Transform parent, string path, Color color)
    {
        var sprite = PlayUi.sprite(path);
        var picture = PlayUi.image(name, parent, sprite);
        picture.enabled = sprite != null;
        picture.preserveAspect = true;
        picture.color = color;
        return picture;
    }

    // Same bare icon close button on the login card and the guest dialog: no ring or
    // fill, the glyph tints on hover.
    public static Button close_button(RectTransform card, UnityAction on_click)
    {
        if (card == null) return null;

        var rect = PlayUi.rect("login_close", card);
        PlayUi.image("login_close_hit", rect, null, true).color = LoginTheme.clear;
        rect.anchorMin = new Vector2(LoginTheme.CLOSE_LEFT, LoginTheme.CLOSE_CENTER_Y);
        rect.anchorMax = new Vector2(LoginTheme.CLOSE_RIGHT, LoginTheme.CLOSE_CENTER_Y);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;

        var fitter = rect.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        fitter.aspectRatio = 1f;

        var symbol = icon("login_close_icon", rect, LoginTheme.ICON_CLOSE_PATH, PlayTheme.icon);
        LobbyUi.place_fraction(symbol.rectTransform, LoginTheme.CLOSE_ICON_INSET, LoginTheme.CLOSE_ICON_INSET, 1f - LoginTheme.CLOSE_ICON_INSET, 1f - LoginTheme.CLOSE_ICON_INSET);

        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = symbol;
        LobbyUi.no_transition(button);
        if (on_click != null) button.onClick.AddListener(on_click);

        LoginFocusGlow.attach(button, symbol, PlayTheme.icon, LoginTheme.gold_soft, null, LoginTheme.clear);
        return button;
    }
}
