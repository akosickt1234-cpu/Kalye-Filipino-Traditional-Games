using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class LobbyCode
{
    const string COPIED_TEXT = "Copied";
    const int LABEL_MIN_SIZE = 10;
    const int LABEL_MAX_SIZE = 22;
    const int CODE_MIN_SIZE = 12;
    const int CODE_MAX_SIZE = 40;
    const float LABEL_BOTTOM = 0.64f;
    const float ROW_TOP = 0.56f;
    const float ROW_BOTTOM = 0.04f;
    const float KEY_LEFT = 0.04f;
    const float KEY_RIGHT = 0.16f;
    const float KEY_VERTICAL_PADDING = 0.2f;
    const float CODE_TEXT_LEFT = 0.19f;
    const float CODE_TEXT_RIGHT = 0.96f;
    const float ICON_GLYPH_RATIO = 0.5f;
    const float GLOW_RATIO = 1.5f;

    public static void build(RectTransform code_slot)
    {
        if (code_slot == null) return;

        var view = code_slot.gameObject.AddComponent<LobbyCodeView>();

        build_code(code_slot, view);

        view.bind();
    }

    static void build_code(RectTransform slot, LobbyCodeView view)
    {
        var label_area = PlayUi.rect("label_area", slot);
        LobbyUi.place_fraction(label_area, 0f, LABEL_BOTTOM, 1f, 1f);

        var copied = fitted_text("copied_text", label_area, COPIED_TEXT, LABEL_MIN_SIZE, LABEL_MAX_SIZE, PlayTheme.gold_soft, TextAnchor.LowerRight);
        LobbyUi.place_fraction(copied.rectTransform, 0.5f, 0f, 1f, 1f);
        copied.color = PlayTheme.clear;

        var row = PlayUi.rect("code_row", slot);
        LobbyUi.place_fraction(row, 0f, ROW_BOTTOM, 1f, ROW_TOP);

        var pill = PlayUi.image("code_pill", row, PlayUi.pill());
        pill.type = Image.Type.Sliced;
        pill.color = PlayTheme.fill_soft;

        var key = PlayUi.image("key_icon", pill.rectTransform, PlayUi.sprite(LobbyUi.ICON_KEY_PATH));
        key.enabled = key.sprite != null;
        key.color = PlayTheme.gold;
        key.preserveAspect = true;
        LobbyUi.place_fraction(key.rectTransform, KEY_LEFT, KEY_VERTICAL_PADDING, KEY_RIGHT, 1f - KEY_VERTICAL_PADDING);

        var code = fitted_text("code_text", pill.rectTransform, LobbyState.lobby_code, CODE_MIN_SIZE, CODE_MAX_SIZE, PlayTheme.text_white, TextAnchor.MiddleCenter);
        LobbyUi.place_fraction(code.rectTransform, CODE_TEXT_LEFT, 0.08f, CODE_TEXT_RIGHT, 0.92f);
        PlayUi.add_shadow(code, 2f);

        var copy_button = build_square_button("copy_button", row, LobbyUi.ICON_COPY_PATH);

        var layout = row.gameObject.AddComponent<LobbyCodeRow>();
        layout.setup(pill.rectTransform, new List<RectTransform> { (RectTransform)copy_button.transform });

        view.code_text = code;
        view.copied_text = copied;
        view.copy_button = copy_button;
    }

    static Button build_square_button(string name, RectTransform parent, string icon_path)
    {
        var hit = PlayUi.image(name, parent, PlayUi.pill(), true);
        hit.type = Image.Type.Sliced;
        hit.color = PlayTheme.fill_soft;

        var button = hit.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;

        var glow = PlayUi.image("glow", hit.rectTransform, PlayUi.radial_glow());
        glow.color = PlayTheme.clear;
        LobbyUi.place_fraction(glow.rectTransform, 0.5f - GLOW_RATIO * 0.5f, 0.5f - GLOW_RATIO * 0.5f, 0.5f + GLOW_RATIO * 0.5f, 0.5f + GLOW_RATIO * 0.5f);

        var icon = PlayUi.image("icon", hit.rectTransform, PlayUi.sprite(icon_path));
        icon.enabled = icon.sprite != null;
        icon.color = PlayTheme.gold;
        icon.preserveAspect = true;
        LobbyUi.place_fraction(icon.rectTransform, 0.5f - ICON_GLYPH_RATIO * 0.5f, 0.5f - ICON_GLYPH_RATIO * 0.5f, 0.5f + ICON_GLYPH_RATIO * 0.5f, 0.5f + ICON_GLYPH_RATIO * 0.5f);
        PlayUi.add_shadow(icon, 2f);

        var hover = hit.gameObject.AddComponent<LobbyCodeButtonHover>();
        hover.setup(icon, glow);

        return button;
    }

    static Text fitted_text(string name, Transform parent, string value, int min_size, int max_size, Color color, TextAnchor anchor)
    {
        var label = PlayUi.text(name, parent, value, max_size, color, anchor);
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = min_size;
        label.resizeTextMaxSize = max_size;
        return label;
    }
}

public class LobbyCodeRow : MonoBehaviour
{
    const float GAP_RATIO = 0.14f;

    RectTransform row_rect;
    RectTransform body;
    List<RectTransform> squares;

    public void setup(RectTransform body_rect, List<RectTransform> square_list)
    {
        row_rect = (RectTransform)transform;
        body = body_rect;
        squares = square_list;
        apply();
    }

    void OnEnable()
    {
        apply();
    }

    void OnRectTransformDimensionsChange()
    {
        apply();
    }

    void apply()
    {
        if (row_rect == null || body == null || squares == null) return;

        var rect = row_rect.rect;
        if (rect.width <= 0.01f || rect.height <= 0.01f) return;

        var side = Mathf.Min(1f, rect.height / rect.width);
        var gap = side * GAP_RATIO;
        var edge = 1f;

        for (var i = 0; i < squares.Count; i++)
        {
            var square = squares[i];
            if (square == null) continue;

            LobbyUi.place_fraction(square, edge - side, 0f, edge, 1f);
            edge -= side + gap;
        }

        LobbyUi.place_fraction(body, 0f, 0f, Mathf.Max(0f, edge), 1f);
    }
}

public class LobbyCodeButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    const float FADE_IN_SECONDS = 0.15f;
    const float FADE_OUT_SECONDS = 0.1f;
    const float ACTIVE_SCALE = 1.08f;

    Selectable selectable;
    Image icon_image;
    Image glow_image;
    bool pointer_over;
    bool focused;
    float amount;

    public void setup(Image icon, Image glow)
    {
        selectable = GetComponent<Selectable>();
        icon_image = icon;
        glow_image = glow;
        apply(0f);
    }

    void OnDisable()
    {
        pointer_over = false;
        focused = false;
        amount = 0f;
        apply(0f);
    }

    void Update()
    {
        var active = PlayFocus.lit(pointer_over, focused) && selectable != null && selectable.IsInteractable();
        var target = active ? 1f : 0f;
        if (Mathf.Approximately(amount, target)) return;

        var seconds = active ? FADE_IN_SECONDS : FADE_OUT_SECONDS;
        amount = Mathf.MoveTowards(amount, target, Time.unscaledDeltaTime / seconds);
        apply(amount);
    }

    void apply(float t)
    {
        if (icon_image != null) icon_image.rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, ACTIVE_SCALE, t);
        if (glow_image != null) glow_image.color = PlayTheme.with_alpha(PlayTheme.glow, PlayTheme.glow.a * t);
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        pointer_over = true;
    }

    public void OnPointerExit(PointerEventData event_data)
    {
        pointer_over = false;
    }

    public void OnSelect(BaseEventData event_data)
    {
        focused = true;
    }

    public void OnDeselect(BaseEventData event_data)
    {
        focused = false;
    }
}

public class LobbyCodeView : MonoBehaviour
{
    const float COPIED_SECONDS = 1.2f;
    const float COPIED_FADE_SECONDS = 0.3f;

    public Text code_text;
    public Text copied_text;
    public Button copy_button;

    float copied_timer;
    bool bound;

    public void bind()
    {
        if (bound) return;
        bound = true;

        copy_button.onClick.AddListener(copy_code);

        LobbyState.code_changed += refresh_code_text;

        refresh_code_text();
    }

    void OnDestroy()
    {
        LobbyState.code_changed -= refresh_code_text;
    }

    void Update()
    {
        update_copied();
    }

    void refresh_code_text()
    {
        if (code_text == null) return;
        code_text.text = string.IsNullOrEmpty(LobbyState.lobby_code) ? string.Empty : LobbyState.lobby_code;
    }

    void copy_code()
    {
        if (string.IsNullOrEmpty(LobbyState.lobby_code)) return;

        GUIUtility.systemCopyBuffer = LobbyState.lobby_code;
        copied_timer = COPIED_SECONDS + COPIED_FADE_SECONDS;
        PlayFocus.clear_selection();
    }

    void update_copied()
    {
        if (copied_text == null || copied_timer <= 0f) return;

        copied_timer = Mathf.Max(0f, copied_timer - Time.unscaledDeltaTime);
        var alpha = Mathf.Clamp01(copied_timer / COPIED_FADE_SECONDS);
        copied_text.color = PlayTheme.with_alpha(PlayTheme.gold_soft, alpha);
    }
}
