using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class LobbyCode
{
    const string COPIED_TEXT = "Copied";
    const string ENTER_TEXT = "Enter Lobby Code";
    const int ENTER_MIN_SIZE = 10;
    const int ENTER_MAX_SIZE = 26;
    const int LABEL_MIN_SIZE = 10;
    const int LABEL_MAX_SIZE = 22;
    const int CODE_MIN_SIZE = 12;
    const int CODE_MAX_SIZE = 40;
    const float LABEL_BOTTOM = 0.64f;
    const float ROW_TOP = 0.56f;
    const float ROW_BOTTOM = 0.04f;
    const float CODE_ROW_RIGHT = 0.52f;
    const float ENTER_LEFT = 0.56f;
    const float ENTER_TEXT_LEFT = 0.08f;
    const float ENTER_TEXT_RIGHT = 0.78f;
    const float ENTER_ARROW_LEFT = 0.80f;
    const float ENTER_ARROW_RIGHT = 0.95f;
    const float ENTER_ARROW_PADDING = 0.22f;
    const float ENTER_HOVER_SCALE = 1.1f;
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
        build_enter(code_slot, view);

        view.bind();
    }

    static void build_code(RectTransform slot, LobbyCodeView view)
    {
        var label_area = PlayUi.rect("label_area", slot);
        LobbyUi.place_fraction(label_area, 0f, LABEL_BOTTOM, CODE_ROW_RIGHT, 1f);

        var copied = fitted_text("copied_text", label_area, COPIED_TEXT, LABEL_MIN_SIZE, LABEL_MAX_SIZE, PlayTheme.gold_soft, TextAnchor.LowerRight);
        LobbyUi.place_fraction(copied.rectTransform, 0f, 0f, 1f, 1f);
        copied.color = PlayTheme.clear;

        var row = PlayUi.rect("code_row", slot);
        LobbyUi.place_fraction(row, 0f, ROW_BOTTOM, CODE_ROW_RIGHT, ROW_TOP);

        var pill = LobbyUi.blue_pill("code_pill", row, false);

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

    static void build_enter(RectTransform slot, LobbyCodeView view)
    {
        var row = PlayUi.rect("enter_row", slot);
        LobbyUi.place_fraction(row, ENTER_LEFT, ROW_BOTTOM, 1f, ROW_TOP);

        var pill = LobbyUi.blue_pill("enter_pill", row, true);
        PlayUi.stretch(pill.rectTransform);

        var label = fitted_text("enter_text", pill.rectTransform, ENTER_TEXT, ENTER_MIN_SIZE, ENTER_MAX_SIZE, PlayTheme.text_white, TextAnchor.MiddleLeft);
        LobbyUi.place_fraction(label.rectTransform, ENTER_TEXT_LEFT, 0.08f, ENTER_TEXT_RIGHT, 0.92f);
        PlayUi.add_shadow(label, 2f);

        var arrow = PlayUi.image("arrow_icon", pill.rectTransform, PlayUi.sprite(LobbyUi.ICON_ARROW_RIGHT_PATH));
        arrow.enabled = arrow.sprite != null;
        arrow.color = PlayTheme.gold;
        arrow.preserveAspect = true;
        LobbyUi.place_fraction(arrow.rectTransform, ENTER_ARROW_LEFT, ENTER_ARROW_PADDING, ENTER_ARROW_RIGHT, 1f - ENTER_ARROW_PADDING);
        PlayUi.add_shadow(arrow, 2f);

        var button = pill.gameObject.AddComponent<Button>();
        button.targetGraphic = pill;
        LobbyUi.no_transition(button);

        var hover = pill.gameObject.AddComponent<LobbyChatHover>();
        hover.setup(button, arrow, PlayTheme.gold, PlayTheme.gold_soft, arrow.rectTransform, ENTER_HOVER_SCALE, null);

        view.enter_button = button;
    }

    public static void open_window(GameObject return_target)
    {
        if (LobbyCodeWindow.current != null) return;

        var overlay = LobbyScreen.overlay_root;
        if (overlay == null) return;

        var blocker = LobbyUi.modal_blocker(overlay);
        var window = blocker.gameObject.AddComponent<LobbyCodeWindow>();
        window.build(blocker, return_target);
    }

    static Button build_square_button(string name, RectTransform parent, string icon_path)
    {
        var hit = LobbyUi.blue_pill(name, parent, true);

        var button = hit.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        LobbyUi.no_transition(button);

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

    internal static Text fitted_text(string name, Transform parent, string value, int min_size, int max_size, Color color, TextAnchor anchor)
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

        var seconds = active ? LobbyUi.FADE_IN_SECONDS : LobbyUi.FADE_OUT_SECONDS;
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
    public Button enter_button;

    float copied_timer;
    bool bound;

    public void bind()
    {
        if (bound) return;
        bound = true;

        copy_button.onClick.AddListener(copy_code);
        var enter_target = enter_button.gameObject;
        enter_button.onClick.AddListener(() => LobbyCode.open_window(enter_target));

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

public class LobbyCodeWindow : MonoBehaviour
{
    const string TITLE_TEXT = "Enter Lobby Code";
    const string PLACEHOLDER_TEXT = "Enter friend's code";
    const string JOIN_TEXT = "Join";
    const string JOINING_TEXT = "Joining...";
    const int TITLE_FONT_SIZE = 44;
    const int INPUT_MIN_SIZE = 12;
    const int INPUT_MAX_SIZE = 34;
    const int ERROR_MIN_SIZE = 10;
    const int ERROR_MAX_SIZE = 26;
    const int JOIN_MIN_SIZE = 12;
    const int JOIN_MAX_SIZE = 34;
    const int CODE_LENGTH = 9;
    const float JOIN_DELAY_SECONDS = 0.4f;
    const float SPINNER_DEGREES_PER_SECOND = 360f;
    const float FOCUS_FILL_ALPHA = 0.22f;
    const float FILL_LERP_SPEED = 14f;
    const float CLOSE_HOVER_SCALE = 1.08f;
    const float JOIN_HOVER_SCALE = 1.04f;

    const float WINDOW_LEFT = 0.28f;
    const float WINDOW_RIGHT = 0.72f;
    const float WINDOW_BOTTOM = 0.28f;
    const float WINDOW_TOP = 0.72f;
    const float CONTENT_LEFT = 0.08f;
    const float CONTENT_RIGHT = 0.92f;
    const float TITLE_BOTTOM = 0.78f;
    const float TITLE_TOP = 0.94f;
    const float TITLE_RIGHT = 0.80f;
    const float LINE_BOTTOM = 0.755f;
    const float LINE_TOP = 0.763f;
    const float CLOSE_LEFT = 0.84f;
    const float CLOSE_BOTTOM = 0.78f;
    const float CLOSE_TOP = 0.94f;
    const float CLOSE_BAR_LEFT = 0.26f;
    const float CLOSE_BAR_RIGHT = 0.74f;
    const float CLOSE_BAR_BOTTOM = 0.46f;
    const float CLOSE_BAR_TOP = 0.54f;
    const float INPUT_BOTTOM = 0.44f;
    const float INPUT_TOP = 0.68f;
    const float KEY_LEFT = 0.03f;
    const float KEY_RIGHT = 0.12f;
    const float KEY_PADDING = 0.2f;
    const float TEXT_LEFT = 0.15f;
    const float TEXT_RIGHT = 0.96f;
    const float ERROR_BOTTOM = 0.32f;
    const float ERROR_TOP = 0.42f;
    const float JOIN_LEFT = 0.30f;
    const float JOIN_RIGHT = 0.70f;
    const float JOIN_BOTTOM = 0.08f;
    const float JOIN_TOP = 0.28f;
    const float SPINNER_LEFT = 0.08f;
    const float SPINNER_RIGHT = 0.24f;
    const float SPINNER_PADDING = 0.22f;
    const float JOIN_TEXT_LEFT = 0.12f;
    const float JOIN_TEXT_RIGHT = 0.88f;
    const float SELECTION_ALPHA = 0.3f;

    public static LobbyCodeWindow current { get; private set; }

    readonly List<Selectable> focus_ring = new List<Selectable>();

    RectTransform blocker;
    GameObject return_target;
    Image input_pill;
    InputField input;
    Button join_button;
    Button close_button;
    Text join_label;
    Text error_text;
    Image spinner;
    bool busy;
    bool formatting;
    float join_timer;
    int focus_wait;

    public void build(RectTransform blocker_rect, GameObject target)
    {
        current = this;
        blocker = blocker_rect;
        return_target = target;

        var blocker_image = blocker.GetComponent<Image>();
        var dismiss = blocker.gameObject.AddComponent<Button>();
        dismiss.targetGraphic = blocker_image;
        LobbyUi.no_transition(dismiss);
        dismiss.navigation = new Navigation { mode = Navigation.Mode.None };
        dismiss.onClick.AddListener(close);

        var window = LobbyUi.panel("code_window", blocker, LobbyUi.PANEL_LOBBY_PATH, LobbyUi.PANEL_BORDER_SCALE, LobbyFrameLight.Style.GoldTop);
        window.raycastTarget = true;
        LobbyUi.place_fraction(window.rectTransform, WINDOW_LEFT, WINDOW_BOTTOM, WINDOW_RIGHT, WINDOW_TOP);

        build_header(window.rectTransform);
        build_input(window.rectTransform);
        build_error(window.rectTransform);
        build_join(window.rectTransform);
        build_close(window.rectTransform);
        link_focus();

        apply_busy();
        focus_wait = LobbyUi.FOCUS_DELAY_FRAMES;
    }

    void OnDestroy()
    {
        if (current == this) current = null;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            close();
            return;
        }

        tick_join();
        tick_fill();
        tick_spinner();
        tick_tab(keyboard);
    }

    void LateUpdate()
    {
        if (blocker == null) return;

        if (focus_wait > 0)
        {
            focus_wait--;
            if (focus_wait == 0) focus_input();
            return;
        }

        keep_focus_inside();
    }

    public void close()
    {
        if (blocker == null) return;

        var event_system = EventSystem.current;
        if (return_target != null && return_target.activeInHierarchy && event_system != null)
        {
            event_system.SetSelectedGameObject(return_target);
        }
        else
        {
            PlayFocus.clear_selection();
        }

        var target = blocker.gameObject;
        blocker = null;
        busy = false;
        if (current == this) current = null;
        Destroy(target);
    }

    void build_header(RectTransform window)
    {
        var title = PlayUi.text("title", window, TITLE_TEXT, TITLE_FONT_SIZE, PlayTheme.gold, TextAnchor.MiddleLeft);
        title.horizontalOverflow = HorizontalWrapMode.Overflow;
        LobbyUi.place_fraction(title.rectTransform, CONTENT_LEFT, TITLE_BOTTOM, TITLE_RIGHT, TITLE_TOP);
        PlayUi.add_shadow(title);

        var line = PlayOrnament.line(window, "title_line", 0f, 0f, PlayTheme.gold, true, true);
        LobbyUi.place_fraction(line.rectTransform, CONTENT_LEFT, LINE_BOTTOM, CONTENT_RIGHT, LINE_TOP);
    }

    void build_input(RectTransform window)
    {
        input_pill = PlayUi.image("code_input", window, PlayUi.pill(), true);
        input_pill.type = Image.Type.Sliced;
        input_pill.color = PlayTheme.fill_soft;
        LobbyUi.place_fraction(input_pill.rectTransform, CONTENT_LEFT, INPUT_BOTTOM, CONTENT_RIGHT, INPUT_TOP);

        var key = PlayUi.image("key_icon", input_pill.rectTransform, PlayUi.sprite(LobbyUi.ICON_KEY_PATH));
        key.enabled = key.sprite != null;
        key.color = PlayTheme.gold;
        key.preserveAspect = true;
        LobbyUi.place_fraction(key.rectTransform, KEY_LEFT, KEY_PADDING, KEY_RIGHT, 1f - KEY_PADDING);

        var area = PlayUi.rect("text_area", input_pill.rectTransform);
        LobbyUi.place_fraction(area, TEXT_LEFT, 0.1f, TEXT_RIGHT, 0.9f);
        area.gameObject.AddComponent<RectMask2D>();

        var typed = LobbyCode.fitted_text("text", area, string.Empty, INPUT_MIN_SIZE, INPUT_MAX_SIZE, PlayTheme.text, TextAnchor.MiddleLeft);
        PlayUi.stretch(typed.rectTransform);
        typed.supportRichText = false;

        var placeholder = LobbyCode.fitted_text("placeholder", area, PLACEHOLDER_TEXT, INPUT_MIN_SIZE, INPUT_MAX_SIZE, PlayTheme.text_muted, TextAnchor.MiddleLeft);
        PlayUi.stretch(placeholder.rectTransform);
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.supportRichText = false;

        input = input_pill.gameObject.AddComponent<InputField>();
        input.targetGraphic = input_pill;
        LobbyUi.no_transition(input);
        input.textComponent = typed;
        input.placeholder = placeholder;
        input.lineType = InputField.LineType.SingleLine;
        input.contentType = InputField.ContentType.Standard;
        input.characterLimit = CODE_LENGTH;
        input.customCaretColor = true;
        input.caretColor = PlayTheme.gold;
        input.caretWidth = LobbyUi.CARET_WIDTH;
        input.selectionColor = PlayTheme.with_alpha(PlayTheme.gold, SELECTION_ALPHA);
        input.onValueChanged.AddListener(on_input_changed);
        input.onSubmit.AddListener(submit);
    }

    void build_error(RectTransform window)
    {
        var area = PlayUi.rect("error_area", window);
        LobbyUi.place_fraction(area, CONTENT_LEFT, ERROR_BOTTOM, CONTENT_RIGHT, ERROR_TOP);

        error_text = LobbyCode.fitted_text("error_text", area, string.Empty, ERROR_MIN_SIZE, ERROR_MAX_SIZE, PlayTheme.error, TextAnchor.MiddleLeft);
        PlayUi.stretch(error_text.rectTransform);
    }

    void build_join(RectTransform window)
    {
        var background = PlayUi.image("join_button", window, PlayUi.pill(), true);
        background.type = Image.Type.Sliced;
        background.color = PlayTheme.gold;
        LobbyUi.place_fraction(background.rectTransform, JOIN_LEFT, JOIN_BOTTOM, JOIN_RIGHT, JOIN_TOP);
        PlayUi.add_shadow(background);

        join_label = LobbyCode.fitted_text("label", background.rectTransform, JOIN_TEXT, JOIN_MIN_SIZE, JOIN_MAX_SIZE, PlayTheme.label_brown, TextAnchor.MiddleCenter);
        LobbyUi.place_fraction(join_label.rectTransform, JOIN_TEXT_LEFT, 0.1f, JOIN_TEXT_RIGHT, 0.9f);

        spinner = PlayUi.image("spinner", background.rectTransform, PlayUi.sprite(LobbyUi.ICON_REFRESH_PATH));
        spinner.color = PlayTheme.label_brown;
        spinner.preserveAspect = true;
        spinner.raycastTarget = false;
        LobbyUi.place_fraction(spinner.rectTransform, SPINNER_LEFT, SPINNER_PADDING, SPINNER_RIGHT, 1f - SPINNER_PADDING);

        join_button = background.gameObject.AddComponent<Button>();
        join_button.targetGraphic = background;
        LobbyUi.no_transition(join_button);
        join_button.onClick.AddListener(start_join);

        var hover = background.gameObject.AddComponent<LobbyChatHover>();
        hover.setup(join_button, background, PlayTheme.gold, PlayTheme.gold_soft, background.rectTransform, JOIN_HOVER_SCALE, null);
    }

    void build_close(RectTransform window)
    {
        var region = PlayUi.rect("close_area", window);
        LobbyUi.place_fraction(region, CLOSE_LEFT, CLOSE_BOTTOM, CONTENT_RIGHT, CLOSE_TOP);

        var hit = PlayUi.circle("close_button", region, PlayTheme.fill_soft, true);
        PlayUi.stretch(hit.rectTransform);
        var fitter = hit.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 1f;

        build_close_bar(hit.rectTransform, 45f);
        build_close_bar(hit.rectTransform, -45f);

        close_button = hit.gameObject.AddComponent<Button>();
        close_button.targetGraphic = hit;
        LobbyUi.no_transition(close_button);
        close_button.onClick.AddListener(close);

        var hover = hit.gameObject.AddComponent<LobbyChatHover>();
        hover.setup(close_button, hit, PlayTheme.fill_soft, PlayTheme.with_alpha(PlayTheme.gold, FOCUS_FILL_ALPHA), hit.rectTransform, CLOSE_HOVER_SCALE, null);
    }

    void build_close_bar(RectTransform parent, float degrees)
    {
        var bar = PlayUi.image("close_bar", parent, PlayUi.pill());
        bar.type = Image.Type.Sliced;
        bar.color = PlayTheme.gold;
        bar.raycastTarget = false;
        LobbyUi.place_fraction(bar.rectTransform, CLOSE_BAR_LEFT, CLOSE_BAR_BOTTOM, CLOSE_BAR_RIGHT, CLOSE_BAR_TOP);
        bar.rectTransform.localRotation = Quaternion.Euler(0f, 0f, degrees);
    }

    void link_focus()
    {
        focus_ring.Add(input);
        focus_ring.Add(join_button);
        focus_ring.Add(close_button);

        input.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = join_button, selectOnUp = close_button };
        join_button.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = input, selectOnDown = close_button };
        close_button.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = join_button, selectOnDown = input };
    }

    void on_input_changed(string value)
    {
        if (formatting) return;

        formatting = true;
        var formatted = LobbyState.format_code_input(value);
        if (formatted != value)
        {
            input.SetTextWithoutNotify(formatted);
            input.caretPosition = formatted.Length;
            input.selectionAnchorPosition = formatted.Length;
            input.selectionFocusPosition = formatted.Length;
        }
        formatting = false;

        error_text.text = string.Empty;
    }

    void submit(string value)
    {
        start_join();
    }

    void start_join()
    {
        if (busy || blocker == null) return;

        busy = true;
        join_timer = JOIN_DELAY_SECONDS;
        error_text.text = string.Empty;
        apply_busy();
    }

    void tick_join()
    {
        if (!busy) return;

        join_timer -= Time.unscaledDeltaTime;
        if (join_timer > 0f) return;

        busy = false;
        apply_busy();

        var result = LobbyState.join_code(input.text);
        if (result == LobbyState.JOIN_OK)
        {
            close();
            return;
        }

        error_text.text = result;
        focus_input();
    }

    void apply_busy()
    {
        join_button.interactable = !busy;
        input.readOnly = busy;
        join_label.text = busy ? JOINING_TEXT : JOIN_TEXT;
        spinner.enabled = busy && spinner.sprite != null;
        if (!busy) spinner.rectTransform.localRotation = Quaternion.identity;
    }

    void tick_spinner()
    {
        if (!busy || spinner == null) return;

        spinner.rectTransform.Rotate(0f, 0f, -SPINNER_DEGREES_PER_SECOND * Time.unscaledDeltaTime);
    }

    void tick_fill()
    {
        if (input_pill == null || input == null) return;

        var target = input.isFocused ? PlayTheme.with_alpha(PlayTheme.gold, FOCUS_FILL_ALPHA) : PlayTheme.fill_soft;
        input_pill.color = Color.Lerp(input_pill.color, target, Mathf.Clamp01(Time.unscaledDeltaTime * FILL_LERP_SPEED));
    }

    void tick_tab(Keyboard keyboard)
    {
        if (keyboard == null || !keyboard.tabKey.wasPressedThisFrame) return;

        var step = keyboard.shiftKey.isPressed ? -1 : 1;
        var event_system = EventSystem.current;
        var index = event_system != null ? ring_index(event_system.currentSelectedGameObject) : -1;
        if (index < 0) index = step > 0 ? -1 : 0;

        for (var i = 1; i <= focus_ring.Count; i++)
        {
            var next = focus_ring[((index + step * i) % focus_ring.Count + focus_ring.Count) % focus_ring.Count];
            if (next == null || !next.IsInteractable()) continue;

            focus(next);
            return;
        }
    }

    void keep_focus_inside()
    {
        var event_system = EventSystem.current;
        if (event_system == null) return;

        var selected = event_system.currentSelectedGameObject;
        if (selected == null || ring_index(selected) >= 0) return;

        focus_input();
    }

    int ring_index(GameObject selected)
    {
        if (selected == null) return -1;

        for (var i = 0; i < focus_ring.Count; i++)
        {
            if (focus_ring[i] != null && focus_ring[i].gameObject == selected) return i;
        }

        return -1;
    }

    void focus(Selectable target)
    {
        var event_system = EventSystem.current;
        if (event_system == null) return;

        event_system.SetSelectedGameObject(target.gameObject);
        if (target == input) input.ActivateInputField();
    }

    void focus_input()
    {
        if (input == null || !input.gameObject.activeInHierarchy) return;

        focus(input);
    }
}
