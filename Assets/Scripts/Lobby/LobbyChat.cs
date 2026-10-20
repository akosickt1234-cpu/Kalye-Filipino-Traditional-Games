using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class LobbyChat
{
    const string BUTTON_NAME = "chat_button";
    const string EMPTY_TEXT = "No messages yet";
    const string EMPTY_SUBTEXT = "Be the first to say hi!";
    const string ELLIPSIS = "...";
    const string TITLE_TEXT = "Lobby Chat";
    const string INPUT_HINT = "Type a message";
    const string SEND_TEXT = "Send";
    const string FALLBACK_NAME = "You";
    const int COLUMN_FONT_SIZE = 28;
    const int COLUMN_SUB_FONT_SIZE = 24;
    const float COLUMN_SPLIT = 0.5f;
    const int TITLE_FONT_SIZE = 44;
    const int ROW_FONT_SIZE = 30;
    const int INPUT_FONT_SIZE = 30;
    const int SEND_FONT_SIZE = 30;
    const int MAX_LENGTH = 200;
    const int ROW_PADDING = 12;
    const float ROW_SPACING = 10f;
    const float ICON_HOVER_SCALE = 1.1f;
    const float SEND_HOVER_SCALE = 1.04f;

    const float ICON_LEFT = 0.03f;
    const float ICON_RIGHT = 0.19f;
    const float ICON_BOTTOM = 0.15f;
    const float ICON_TOP = 0.85f;
    const float AREA_LEFT = 0.22f;
    const float AREA_RIGHT = 0.97f;
    const float AREA_BOTTOM = 0.10f;
    const float AREA_TOP = 0.90f;

    const float WINDOW_LEFT = 0.22f;
    const float WINDOW_RIGHT = 0.78f;
    const float WINDOW_BOTTOM = 0.10f;
    const float WINDOW_TOP = 0.90f;
    const float CONTENT_LEFT = 0.07f;
    const float CONTENT_RIGHT = 0.93f;
    const float TITLE_BOTTOM = 0.86f;
    const float TITLE_TOP = 0.95f;
    const float LINE_BOTTOM = 0.835f;
    const float LINE_TOP = 0.843f;
    const float LIST_BOTTOM = 0.20f;
    const float LIST_TOP = 0.82f;
    const float INPUT_BOTTOM = 0.06f;
    const float INPUT_TOP = 0.17f;
    const float INPUT_RIGHT = 0.77f;
    const float SEND_LEFT = 0.79f;
    const float VIEWPORT_INSET = 0.02f;
    const float TEXT_INSET_X = 0.03f;
    const float TEXT_INSET_Y = 0.10f;

    public static void build(RectTransform chat_col)
    {
        if (chat_col == null) return;

        var button_rect = PlayUi.rect(BUTTON_NAME, chat_col);
        PlayUi.stretch(button_rect);

        var hit = button_rect.gameObject.AddComponent<Image>();
        hit.color = PlayTheme.clear;
        hit.raycastTarget = true;

        var glow = PlayUi.image("icon_glow", button_rect, PlayUi.radial_glow());
        LobbyUi.place_fraction(glow.rectTransform, 0f, 0f, ICON_RIGHT + ICON_LEFT, 1f);
        glow.preserveAspect = true;
        glow.color = PlayTheme.with_alpha(PlayTheme.glow, 0f);

        var icon = PlayUi.image("icon_chat", button_rect, PlayUi.sprite(LobbyUi.ICON_CHAT_PATH));
        LobbyUi.place_fraction(icon.rectTransform, ICON_LEFT, ICON_BOTTOM, ICON_RIGHT, ICON_TOP);
        icon.preserveAspect = true;
        icon.color = PlayTheme.gold;
        PlayUi.add_shadow(icon);

        var area = PlayUi.rect("message_area", button_rect);
        LobbyUi.place_fraction(area, AREA_LEFT, AREA_BOTTOM, AREA_RIGHT, AREA_TOP);

        var button = button_rect.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        LobbyUi.no_transition(button);

        var hover = button_rect.gameObject.AddComponent<LobbyChatHover>();
        hover.setup(button, icon, PlayTheme.gold, PlayTheme.gold_soft, icon.rectTransform, ICON_HOVER_SCALE, glow);

        var column = button_rect.gameObject.AddComponent<LobbyChatColumn>();
        column.setup(area, COLUMN_FONT_SIZE, COLUMN_SUB_FONT_SIZE, COLUMN_SPLIT);

        var target = button.gameObject;
        button.onClick.AddListener(() => open_window(target));
    }

    public static void open_window(GameObject return_target)
    {
        if (LobbyChatWindow.current != null) return;

        var overlay = LobbyScreen.overlay_root;
        if (overlay == null) return;

        var blocker = LobbyUi.modal_blocker(overlay);
        var window = blocker.gameObject.AddComponent<LobbyChatWindow>();
        window.build(blocker, return_target);
    }

    public static string format_message(LobbyMessage message)
    {
        var name_color = message.name == LobbyState.SYSTEM_NAME ? PlayTheme.text_muted : PlayTheme.gold;
        return "<color=#" + ColorUtility.ToHtmlStringRGB(name_color) + ">" + clean(message.name) + "</color>: " + clean(message.text);
    }

    public static string empty_text
    {
        get { return EMPTY_TEXT; }
    }

    public static string empty_subtext
    {
        get { return EMPTY_SUBTEXT; }
    }

    internal static string fit_text(Text label, string value, float width)
    {
        if (label == null || string.IsNullOrEmpty(value) || width <= 0f) return string.Empty;

        var settings = label.GetGenerationSettings(Vector2.zero);
        var generator = label.cachedTextGeneratorForLayout;
        var scale = label.pixelsPerUnit;
        if (generator.GetPreferredWidth(value, settings) / scale <= width) return value;

        var low = 0;
        var high = value.Length;
        while (low < high)
        {
            var mid = (low + high + 1) / 2;
            var candidate = value.Substring(0, mid).TrimEnd() + ELLIPSIS;
            if (generator.GetPreferredWidth(candidate, settings) / scale <= width) low = mid;
            else high = mid - 1;
        }

        return low == 0 ? ELLIPSIS : value.Substring(0, low).TrimEnd() + ELLIPSIS;
    }

    static string clean(string value)
    {
        return LobbyUi.sanitize(value).Replace("\r", string.Empty).Replace("\n", " ");
    }

    internal static string sender_name()
    {
        var self = LobbyState.you;
        return self != null && !self.is_empty ? self.name : FALLBACK_NAME;
    }

    internal static void build_window(LobbyChatWindow owner, RectTransform blocker, out ScrollRect scroll, out RectTransform content, out InputField input, out Button send)
    {
        var window = LobbyUi.panel("chat_window", blocker, LobbyUi.PANEL_LOBBY_PATH, LobbyUi.PANEL_BORDER_SCALE);
        window.raycastTarget = true;
        LobbyUi.place_fraction(window.rectTransform, WINDOW_LEFT, WINDOW_BOTTOM, WINDOW_RIGHT, WINDOW_TOP);

        var title = PlayUi.text("title", window.rectTransform, TITLE_TEXT, TITLE_FONT_SIZE, PlayTheme.gold, TextAnchor.MiddleLeft);
        title.horizontalOverflow = HorizontalWrapMode.Overflow;
        LobbyUi.place_fraction(title.rectTransform, CONTENT_LEFT, TITLE_BOTTOM, CONTENT_RIGHT, TITLE_TOP);
        PlayUi.add_shadow(title);

        var line = PlayOrnament.line(window.rectTransform, "title_line", 0f, 0f, PlayTheme.gold, true, true);
        LobbyUi.place_fraction(line.rectTransform, CONTENT_LEFT, LINE_BOTTOM, CONTENT_RIGHT, LINE_TOP);

        build_list(window.rectTransform, out scroll, out content);
        build_input(window.rectTransform, owner, out input);
        build_send(window.rectTransform, owner, out send);
        link_navigation(input, send);
    }

    static void build_list(RectTransform window, out ScrollRect scroll, out RectTransform content)
    {
        var list = PlayUi.image("message_list", window, PlayUi.pill(), true);
        list.type = Image.Type.Sliced;
        list.color = PlayTheme.fill_soft;
        LobbyUi.place_fraction(list.rectTransform, CONTENT_LEFT, LIST_BOTTOM, CONTENT_RIGHT, LIST_TOP);

        var viewport = PlayUi.rect("viewport", list.rectTransform);
        LobbyUi.place_fraction(viewport, VIEWPORT_INSET, VIEWPORT_INSET, 1f - VIEWPORT_INSET, 1f - VIEWPORT_INSET);
        viewport.gameObject.AddComponent<RectMask2D>();

        content = PlayUi.rect("content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;

        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(ROW_PADDING, ROW_PADDING, ROW_PADDING, ROW_PADDING);
        layout.spacing = ROW_SPACING;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll = list.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = LobbyUi.SCROLL_SENSITIVITY;
        scroll.viewport = viewport;
        scroll.content = content;
    }

    static void build_input(RectTransform window, LobbyChatWindow owner, out InputField input)
    {
        var background = PlayUi.image("message_input", window, PlayUi.pill(), true);
        background.type = Image.Type.Sliced;
        background.color = PlayTheme.fill_soft;
        LobbyUi.place_fraction(background.rectTransform, CONTENT_LEFT, INPUT_BOTTOM, INPUT_RIGHT, INPUT_TOP);

        var text_area = PlayUi.rect("text_area", background.rectTransform);
        LobbyUi.place_fraction(text_area, TEXT_INSET_X, TEXT_INSET_Y, 1f - TEXT_INSET_X, 1f - TEXT_INSET_Y);
        text_area.gameObject.AddComponent<RectMask2D>();

        var placeholder = PlayUi.text("placeholder", text_area, INPUT_HINT, INPUT_FONT_SIZE, PlayTheme.text_muted, TextAnchor.MiddleLeft);
        placeholder.horizontalOverflow = HorizontalWrapMode.Overflow;
        placeholder.supportRichText = false;
        PlayUi.stretch(placeholder.rectTransform);

        var typed = PlayUi.text("text", text_area, string.Empty, INPUT_FONT_SIZE, PlayTheme.text, TextAnchor.MiddleLeft);
        typed.horizontalOverflow = HorizontalWrapMode.Overflow;
        typed.verticalOverflow = VerticalWrapMode.Truncate;
        typed.supportRichText = false;
        PlayUi.stretch(typed.rectTransform);

        input = background.gameObject.AddComponent<InputField>();
        input.targetGraphic = background;
        LobbyUi.no_transition(input);
        input.textComponent = typed;
        input.placeholder = placeholder;
        input.lineType = InputField.LineType.SingleLine;
        input.characterLimit = MAX_LENGTH;
        input.customCaretColor = true;
        input.caretColor = PlayTheme.gold;
        input.caretWidth = LobbyUi.CARET_WIDTH;
        input.selectionColor = PlayTheme.glow;
        input.onSubmit.AddListener(owner.submit);
        input.onValueChanged.AddListener(owner.sync_send);
    }

    static void build_send(RectTransform window, LobbyChatWindow owner, out Button send)
    {
        var background = PlayUi.image("send_button", window, PlayUi.pill(), true);
        background.type = Image.Type.Sliced;
        background.color = PlayTheme.gold;
        LobbyUi.place_fraction(background.rectTransform, SEND_LEFT, INPUT_BOTTOM, CONTENT_RIGHT, INPUT_TOP);
        PlayUi.add_shadow(background);

        var label = PlayUi.text("label", background.rectTransform, SEND_TEXT, SEND_FONT_SIZE, PlayTheme.label_brown, TextAnchor.MiddleCenter);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        PlayUi.stretch(label.rectTransform);

        send = background.gameObject.AddComponent<Button>();
        send.targetGraphic = background;
        LobbyUi.no_transition(send);
        send.onClick.AddListener(owner.send);

        var hover = background.gameObject.AddComponent<LobbyChatHover>();
        hover.setup(send, background, PlayTheme.gold, PlayTheme.gold_soft, background.rectTransform, SEND_HOVER_SCALE, null);
    }

    static void link_navigation(InputField input, Button send)
    {
        input.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnRight = send };
        send.navigation = new Navigation { mode = Navigation.Mode.Explicit, selectOnLeft = input };
    }

    internal static Text build_row(RectTransform content, string value, Color color)
    {
        var row = PlayUi.text("message", content, value, ROW_FONT_SIZE, color, TextAnchor.UpperLeft);
        row.horizontalOverflow = HorizontalWrapMode.Wrap;
        row.verticalOverflow = VerticalWrapMode.Overflow;
        return row;
    }
}

public class LobbyChatColumn : MonoBehaviour
{
    const float WIDTH_EPSILON = 0.5f;

    RectTransform area;
    Text title_label;
    Text sub_label;
    int title_size;
    int sub_size;
    float split;
    float built_width;
    string title_value = string.Empty;
    string sub_value = string.Empty;
    Color title_color = PlayTheme.text;
    Color sub_color = PlayTheme.text_muted;

    public void setup(RectTransform message_area, int title_font_size, int sub_font_size, float split_fraction)
    {
        area = message_area;
        title_size = title_font_size;
        sub_size = sub_font_size;
        split = split_fraction;
        LobbyState.chat_changed += refresh;
        refresh();
    }

    void OnDestroy()
    {
        LobbyState.chat_changed -= refresh;
    }

    void LateUpdate()
    {
        if (area == null) return;

        var width = area.rect.width;
        if (width <= 0f) return;
        if (title_label != null && Mathf.Abs(width - built_width) < WIDTH_EPSILON) return;

        rebuild(width);
    }

    void refresh()
    {
        var list = LobbyState.messages;
        if (list.Count == 0)
        {
            title_value = LobbyChat.empty_text;
            sub_value = LobbyChat.empty_subtext;
            title_color = PlayTheme.text;
            sub_color = PlayTheme.text_muted;
        }
        else
        {
            var latest = list[list.Count - 1];
            title_value = LobbyUi.sanitize(latest.name) + ":";
            sub_value = LobbyUi.sanitize(latest.text).Replace("\r", string.Empty).Replace("\n", " ");
            title_color = latest.name == LobbyState.SYSTEM_NAME ? PlayTheme.text_muted : PlayTheme.gold;
            sub_color = PlayTheme.text;
        }

        apply();
    }

    void rebuild(float width)
    {
        if (title_label != null) Destroy(title_label.gameObject);
        if (sub_label != null) Destroy(sub_label.gameObject);

        built_width = width;
        title_label = build_line("message_title", title_size, TextAnchor.LowerLeft, 0f, split, 1f);
        sub_label = build_line("message_sub", sub_size, TextAnchor.UpperLeft, 0f, 0f, split);
        apply();
    }

    Text build_line(string line_name, int size, TextAnchor anchor, float x_min, float y_min, float y_max)
    {
        var label = PlayUi.text(line_name, area, string.Empty, size, PlayTheme.text, anchor);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.supportRichText = false;
        LobbyUi.place_fraction(label.rectTransform, x_min, y_min, 1f, y_max);
        return label;
    }

    void apply()
    {
        if (title_label == null || sub_label == null) return;

        var width = area.rect.width;
        title_label.color = title_color;
        title_label.text = LobbyChat.fit_text(title_label, title_value, width);
        sub_label.color = sub_color;
        sub_label.text = LobbyChat.fit_text(sub_label, sub_value, width);
    }
}

public class LobbyChatWindow : MonoBehaviour
{
    public static LobbyChatWindow current { get; private set; }

    RectTransform blocker;
    RectTransform content;
    ScrollRect scroll;
    InputField input;
    Button send_button;
    GameObject return_target;
    bool scroll_pending;
    int focus_wait;

    public void build(RectTransform blocker_rect, GameObject target)
    {
        current = this;
        blocker = blocker_rect;
        return_target = target;

        var blocker_image = blocker.GetComponent<Image>();
        var close_button = blocker.gameObject.AddComponent<Button>();
        close_button.targetGraphic = blocker_image;
        LobbyUi.no_transition(close_button);
        close_button.navigation = new Navigation { mode = Navigation.Mode.None };
        close_button.onClick.AddListener(close);

        LobbyChat.build_window(this, blocker, out scroll, out content, out input, out send_button);

        LobbyState.chat_changed += refresh;
        sync_send(input.text);
        refresh();
        focus_wait = LobbyUi.FOCUS_DELAY_FRAMES;
    }

    void OnDestroy()
    {
        LobbyState.chat_changed -= refresh;
        if (current == this) current = null;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) close();
    }

    void LateUpdate()
    {
        if (scroll_pending && content != null && scroll != null)
        {
            scroll_pending = false;
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            scroll.verticalNormalizedPosition = 0f;
        }

        if (focus_wait > 0)
        {
            focus_wait--;
            if (focus_wait == 0) focus_input();
        }
    }

    public void close()
    {
        if (blocker == null) return;

        var event_system = EventSystem.current;
        if (PlayFocus.show_focus && return_target != null && return_target.activeInHierarchy && event_system != null)
        {
            event_system.SetSelectedGameObject(return_target);
        }
        else
        {
            PlayFocus.clear_selection();
        }

        var target = blocker.gameObject;
        blocker = null;
        LobbyState.chat_changed -= refresh;
        if (current == this) current = null;
        Destroy(target);
    }

    public void submit(string value)
    {
        send();
    }

    public void send()
    {
        if (input == null) return;

        var value = input.text;
        if (!string.IsNullOrWhiteSpace(value))
        {
            LobbyState.add_message(LobbyChat.sender_name(), value);
            input.text = string.Empty;
        }

        focus_wait = LobbyUi.FOCUS_DELAY_FRAMES;
    }

    public void sync_send(string value)
    {
        if (send_button != null) send_button.interactable = !string.IsNullOrWhiteSpace(value);
    }

    void focus_input()
    {
        if (input == null || !input.gameObject.activeInHierarchy) return;

        var event_system = EventSystem.current;
        if (event_system != null) event_system.SetSelectedGameObject(input.gameObject);
        input.ActivateInputField();
    }

    void refresh()
    {
        if (content == null) return;

        for (var i = content.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }

        var list = LobbyState.messages;
        if (list.Count == 0)
        {
            LobbyChat.build_row(content, LobbyChat.empty_text, PlayTheme.text_muted);
        }
        else
        {
            for (var i = 0; i < list.Count; i++) LobbyChat.build_row(content, LobbyChat.format_message(list[i]), PlayTheme.text);
        }

        scroll_pending = true;
    }
}

public class LobbyChatHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    const float DISABLED_ALPHA = 0.45f;

    Selectable selectable;
    Graphic tint;
    Image glow;
    RectTransform scaled;
    Color rest_color;
    Color lit_color;
    float scale_up = 1f;
    bool pointer_over;
    bool focused;
    float amount;

    public void setup(Selectable target, Graphic tint_graphic, Color rest, Color lit, RectTransform scale_target, float scale, Image glow_image)
    {
        selectable = target;
        tint = tint_graphic;
        rest_color = rest;
        lit_color = lit;
        scaled = scale_target;
        scale_up = scale;
        glow = glow_image;
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
        var interactable = selectable != null && selectable.IsInteractable();
        var active = interactable && PlayFocus.lit(pointer_over, focused);
        var target = active ? 1f : 0f;

        if (!Mathf.Approximately(amount, target))
        {
            var seconds = active ? LobbyUi.FADE_IN_SECONDS : LobbyUi.FADE_OUT_SECONDS;
            amount = Mathf.MoveTowards(amount, target, Time.unscaledDeltaTime / seconds);
        }

        apply(amount);
    }

    void apply(float t)
    {
        var interactable = selectable == null || selectable.IsInteractable();

        if (scaled != null) scaled.localScale = Vector3.one * Mathf.Lerp(1f, scale_up, t);

        if (tint != null)
        {
            var color = Color.Lerp(rest_color, lit_color, t);
            tint.color = interactable ? color : PlayTheme.with_alpha(color, color.a * DISABLED_ALPHA);
        }

        if (glow != null) glow.color = PlayTheme.with_alpha(PlayTheme.glow, PlayTheme.glow.a * t);
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
