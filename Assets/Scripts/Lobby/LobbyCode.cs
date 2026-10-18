using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class LobbyCode
{
    const string LABEL_TEXT = "Lobby Code";
    const string COPIED_TEXT = "Copied";
    const string PLACEHOLDER_TEXT = "Enter friend's code";
    const int LABEL_MIN_SIZE = 10;
    const int LABEL_MAX_SIZE = 22;
    const int CODE_MIN_SIZE = 12;
    const int CODE_MAX_SIZE = 40;
    const int INPUT_MIN_SIZE = 10;
    const int INPUT_MAX_SIZE = 28;
    const int ERROR_MIN_SIZE = 9;
    const int ERROR_MAX_SIZE = 20;
    const int CODE_GROUP_LENGTH = 4;
    const int CODE_LENGTH = CODE_GROUP_LENGTH * 2 + 1;
    const float LABEL_BOTTOM = 0.64f;
    const float ROW_TOP = 0.56f;
    const float ROW_BOTTOM = 0.04f;
    const float JOIN_ROW_TOP = 0.76f;
    const float JOIN_ROW_BOTTOM = 0.30f;
    const float ERROR_TOP = 0.26f;
    const float ERROR_BOTTOM = 0.0f;
    const float KEY_LEFT = 0.04f;
    const float KEY_RIGHT = 0.16f;
    const float KEY_VERTICAL_PADDING = 0.2f;
    const float CODE_TEXT_LEFT = 0.19f;
    const float CODE_TEXT_RIGHT = 0.96f;
    const float INPUT_TEXT_LEFT = 0.18f;
    const float INPUT_TEXT_RIGHT = 0.95f;
    const float ICON_GLYPH_RATIO = 0.5f;
    const float GLOW_RATIO = 1.5f;
    const float SELECTION_ALPHA = 0.3f;

    public static void build(RectTransform code_slot, RectTransform join_slot)
    {
        if (code_slot == null || join_slot == null) return;

        var view = code_slot.gameObject.AddComponent<LobbyCodeView>();

        build_code(code_slot, view);
        build_join(join_slot, view);

        view.bind();
    }

    static void build_code(RectTransform slot, LobbyCodeView view)
    {
        var label_area = PlayUi.rect("label_area", slot);
        LobbyUi.place_fraction(label_area, 0f, LABEL_BOTTOM, 1f, 1f);

        var label = fitted_text("label", label_area, LABEL_TEXT, LABEL_MIN_SIZE, LABEL_MAX_SIZE, PlayTheme.text_muted, TextAnchor.LowerLeft);
        LobbyUi.place_fraction(label.rectTransform, 0.02f, 0f, 0.6f, 1f);

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

        var code = fitted_text("code_text", pill.rectTransform, LobbyState.lobby_code, CODE_MIN_SIZE, CODE_MAX_SIZE, PlayTheme.gold_soft, TextAnchor.MiddleCenter);
        LobbyUi.place_fraction(code.rectTransform, CODE_TEXT_LEFT, 0.08f, CODE_TEXT_RIGHT, 0.92f);
        PlayUi.add_shadow(code, 2f);

        var copy_button = build_square_button("copy_button", row, LobbyUi.ICON_COPY_PATH);
        var refresh_button = build_square_button("refresh_button", row, LobbyUi.ICON_REFRESH_PATH);

        var layout = row.gameObject.AddComponent<LobbyCodeRow>();
        layout.setup(pill.rectTransform, new List<RectTransform> { refresh_button.rectTransform, copy_button.rectTransform });

        view.code_text = code;
        view.copied_text = copied;
        view.copy_button = copy_button;
        view.refresh_button = refresh_button;
        view.refresh_icon = refresh_button.transform.Find("icon").GetComponent<Image>();
    }

    static void build_join(RectTransform slot, LobbyCodeView view)
    {
        var row = PlayUi.rect("join_row", slot);
        LobbyUi.place_fraction(row, 0f, JOIN_ROW_BOTTOM, 1f, JOIN_ROW_TOP);

        var pill = PlayUi.image("join_pill", row, PlayUi.pill(), true);
        pill.type = Image.Type.Sliced;
        pill.color = PlayTheme.fill_soft;

        var key = PlayUi.image("key_icon", pill.rectTransform, PlayUi.sprite(LobbyUi.ICON_KEY_PATH));
        key.enabled = key.sprite != null;
        key.color = PlayTheme.gold;
        key.preserveAspect = true;
        LobbyUi.place_fraction(key.rectTransform, KEY_LEFT, KEY_VERTICAL_PADDING, KEY_RIGHT + 0.02f, 1f - KEY_VERTICAL_PADDING);

        var area = PlayUi.rect("input_area", pill.rectTransform);
        LobbyUi.place_fraction(area, INPUT_TEXT_LEFT, 0.1f, INPUT_TEXT_RIGHT, 0.9f);
        area.gameObject.AddComponent<RectMask2D>();

        var input_text = fitted_text("text", area, string.Empty, INPUT_MIN_SIZE, INPUT_MAX_SIZE, PlayTheme.text, TextAnchor.MiddleLeft);
        PlayUi.stretch(input_text.rectTransform);
        input_text.supportRichText = false;

        var placeholder = fitted_text("placeholder", area, PLACEHOLDER_TEXT, INPUT_MIN_SIZE, INPUT_MAX_SIZE, PlayTheme.text_muted, TextAnchor.MiddleLeft);
        PlayUi.stretch(placeholder.rectTransform);
        placeholder.fontStyle = FontStyle.Italic;

        var input = pill.gameObject.AddComponent<InputField>();
        input.targetGraphic = pill;
        input.transition = Selectable.Transition.None;
        input.textComponent = input_text;
        input.placeholder = placeholder;
        input.lineType = InputField.LineType.SingleLine;
        input.contentType = InputField.ContentType.Standard;
        input.characterLimit = CODE_LENGTH;
        input.caretColor = PlayTheme.gold;
        input.customCaretColor = true;
        input.selectionColor = PlayTheme.with_alpha(PlayTheme.gold, SELECTION_ALPHA);

        var submit_button = build_square_button("join_button", row, LobbyUi.ICON_ARROW_RIGHT_PATH);

        var layout = row.gameObject.AddComponent<LobbyCodeRow>();
        layout.setup(pill.rectTransform, new List<RectTransform> { submit_button.rectTransform });

        var error_area = PlayUi.rect("error_area", slot);
        LobbyUi.place_fraction(error_area, 0.04f, ERROR_BOTTOM, 1f, ERROR_TOP);

        var error = fitted_text("error_text", error_area, string.Empty, ERROR_MIN_SIZE, ERROR_MAX_SIZE, PlayTheme.error, TextAnchor.UpperLeft);
        PlayUi.stretch(error.rectTransform);

        view.join_pill = pill;
        view.join_input = input;
        view.join_button = submit_button;
        view.error_text = error;
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
        icon.color = PlayTheme.icon;
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

    public static string format_input(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var chars = new List<char>(CODE_LENGTH);
        for (var i = 0; i < value.Length && chars.Count < CODE_GROUP_LENGTH * 2; i++)
        {
            var c = char.ToUpperInvariant(value[i]);
            if (char.IsLetterOrDigit(c) && c < 128) chars.Add(c);
        }

        var text = new System.Text.StringBuilder(CODE_LENGTH);
        for (var i = 0; i < chars.Count; i++)
        {
            if (i == CODE_GROUP_LENGTH) text.Append('-');
            text.Append(chars[i]);
        }

        if (chars.Count == CODE_GROUP_LENGTH && value.EndsWith("-")) text.Append('-');
        return text.ToString();
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
    const float DIMMED_ALPHA = 0.35f;
    const float FOCUS_FILL_ALPHA = 0.22f;
    const float COPIED_SECONDS = 1.2f;
    const float COPIED_FADE_SECONDS = 0.3f;

    public Text code_text;
    public Text copied_text;
    public Button copy_button;
    public Button refresh_button;
    public Image refresh_icon;
    public Image join_pill;
    public InputField join_input;
    public Button join_button;
    public Text error_text;

    float copied_timer;
    bool formatting;
    bool bound;

    public void bind()
    {
        if (bound) return;
        bound = true;

        copy_button.onClick.AddListener(copy_code);
        refresh_button.onClick.AddListener(refresh_code);
        join_button.onClick.AddListener(submit);
        join_input.onValueChanged.AddListener(on_input_changed);

        LobbyState.code_changed += refresh_code_text;
        LobbyState.players_changed += refresh_host_state;

        refresh_code_text();
        refresh_host_state();
        PlayFocus.default_target = copy_button.gameObject;
    }

    void OnDestroy()
    {
        LobbyState.code_changed -= refresh_code_text;
        LobbyState.players_changed -= refresh_host_state;
    }

    void Update()
    {
        update_copied();
        update_focus_fill();
        check_enter();
    }

    void refresh_code_text()
    {
        if (code_text == null) return;
        code_text.text = string.IsNullOrEmpty(LobbyState.lobby_code) ? string.Empty : LobbyState.lobby_code;
    }

    void refresh_host_state()
    {
        if (refresh_button == null) return;

        var host = LobbyState.is_host;
        refresh_button.interactable = host;
        if (refresh_icon != null) refresh_icon.color = PlayTheme.with_alpha(PlayTheme.icon, host ? 1f : DIMMED_ALPHA);
    }

    void copy_code()
    {
        if (string.IsNullOrEmpty(LobbyState.lobby_code)) return;

        GUIUtility.systemCopyBuffer = LobbyState.lobby_code;
        copied_timer = COPIED_SECONDS + COPIED_FADE_SECONDS;
        PlayFocus.clear_selection();
    }

    void refresh_code()
    {
        if (!LobbyState.is_host) return;

        LobbyState.regenerate_code();
        PlayFocus.clear_selection();
    }

    void update_copied()
    {
        if (copied_text == null || copied_timer <= 0f) return;

        copied_timer = Mathf.Max(0f, copied_timer - Time.unscaledDeltaTime);
        var alpha = Mathf.Clamp01(copied_timer / COPIED_FADE_SECONDS);
        copied_text.color = PlayTheme.with_alpha(PlayTheme.gold_soft, alpha);
    }

    void update_focus_fill()
    {
        if (join_pill == null || join_input == null) return;

        var target = join_input.isFocused ? PlayTheme.with_alpha(PlayTheme.gold, FOCUS_FILL_ALPHA) : PlayTheme.fill_soft;
        join_pill.color = Color.Lerp(join_pill.color, target, Mathf.Clamp01(Time.unscaledDeltaTime * 14f));
    }

    void check_enter()
    {
        if (join_input == null || !join_input.isFocused) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) submit();
    }

    void on_input_changed(string value)
    {
        if (formatting) return;

        formatting = true;
        var formatted = LobbyCode.format_input(value);
        if (formatted != value)
        {
            join_input.SetTextWithoutNotify(formatted);
            join_input.caretPosition = formatted.Length;
            join_input.selectionAnchorPosition = formatted.Length;
            join_input.selectionFocusPosition = formatted.Length;
        }
        formatting = false;

        error_text.text = string.Empty;
    }

    void submit()
    {
        var result = LobbyState.join_code(join_input.text);
        if (result == LobbyState.JOIN_OK)
        {
            join_input.SetTextWithoutNotify(string.Empty);
            error_text.text = string.Empty;
            PlayFocus.clear_selection();
            return;
        }

        error_text.text = result;
        join_input.ActivateInputField();
    }
}
