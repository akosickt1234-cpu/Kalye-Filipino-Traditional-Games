using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class LobbyInfoBar
{
    const string PLAYERS_ICON_PATH = "Icons/Play/IconPlayers";
    const string COUNT_SUFFIX = "PLAYERS";
    const string WAITING_SUBTEXT = "Waiting for more...";
    const string READY_SUBTEXT = "Ready to start";
    const string MODE_SUBTEXT = "Classic Mode";
    const string MODE_ICON_PATH = "Icons/Play/IconNavMode";
    const int COUNT_FONT_SIZE = 30;
    const int COUNT_MIN_FONT_SIZE = 14;
    const int NAME_FONT_SIZE = 32;
    const int MODE_MIN_FONT_SIZE = 12;
    const int SUB_FONT_SIZE = 22;
    const int SUB_MIN_FONT_SIZE = 11;
    const float MODE_HOVER_ALPHA = 0.12f;
    const float MODE_PRESS_ALPHA = 0.26f;
    const float DIVIDER_V_MIN = 0.12f;
    const float DIVIDER_V_MAX = 0.88f;
    const float DIVIDER_THICKNESS = 2f;
    const float DIVIDER_ALPHA = 0.6f;
    const float TOP_LINE_ANCHOR = 0.93f;
    const float TOP_LINE_THICKNESS = 2f;
    const float COUNT_ICON_LEFT = 0.12f;
    const float COUNT_ICON_RIGHT = 0.30f;
    const float COUNT_ICON_V_MIN = 0.22f;
    const float COUNT_ICON_V_MAX = 0.78f;
    const float COUNT_TEXT_LEFT = 0.34f;
    const float COUNT_TEXT_RIGHT = 0.98f;
    const float MODE_HIT_LEFT = 0.04f;
    const float MODE_HIT_RIGHT = 0.98f;
    const float MODE_HIT_V_MIN = 0.08f;
    const float MODE_HIT_V_MAX = 0.92f;
    const float MODE_ICON_LEFT = 0.06f;
    const float MODE_ICON_RIGHT = 0.20f;
    const float MODE_ICON_V_MIN = 0.22f;
    const float MODE_ICON_V_MAX = 0.78f;
    const float MODE_TEXT_LEFT = 0.24f;
    const float MODE_TEXT_RIGHT = 0.78f;
    const float MAIN_V_MIN = 0.46f;
    const float MAIN_V_MAX = 0.90f;
    const float SUB_V_MIN = 0.10f;
    const float SUB_V_MAX = 0.46f;
    const float CHEVRON_LEFT = 0.80f;
    const float CHEVRON_RIGHT = 0.96f;
    const float CHEVRON_SIZE = 26f;
    const float CHEVRON_ROTATION = 90f;

    static string suffix_hex;

    public static void build(RectTransform count_col, RectTransform mode_col)
    {
        var row = count_col != null ? count_col.parent as RectTransform : null;
        if (row != null) build_top_line(row);

        if (count_col != null)
        {
            build_divider(count_col);
            build_count(count_col);
        }

        if (mode_col != null)
        {
            build_divider(mode_col);
            build_mode(mode_col);
        }
    }

    public static string count_text()
    {
        if (suffix_hex == null) suffix_hex = ColorUtility.ToHtmlStringRGB(PlayTheme.gold);
        return $"{LobbyState.player_count}/{LobbyState.team_size} <color=#{suffix_hex}>{COUNT_SUFFIX}</color>";
    }

    public static string count_subtext()
    {
        return LobbyState.player_count >= LobbyState.team_size ? READY_SUBTEXT : WAITING_SUBTEXT;
    }

    static void build_divider(RectTransform column)
    {
        var holder = PlayUi.rect("info_divider", column);
        PlayUi.place(holder, new Vector2(0f, DIVIDER_V_MIN), new Vector2(0f, DIVIDER_V_MAX), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(DIVIDER_THICKNESS, 0f));

        var color = PlayTheme.with_alpha(PlayTheme.gold, DIVIDER_ALPHA);
        var line = PlayOrnament.line(holder, "info_divider_line", 0f, DIVIDER_THICKNESS, color, true, true);
        line.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        var divider = holder.gameObject.AddComponent<LobbyInfoBarDivider>();
        divider.setup(holder, line.rectTransform, DIVIDER_THICKNESS);
    }

    static void build_top_line(RectTransform row)
    {
        var color = PlayTheme.with_alpha(PlayTheme.gold, DIVIDER_ALPHA);
        var line = PlayOrnament.line(row, "info_top_line", 0f, TOP_LINE_THICKNESS, color, true, true);
        var rect = line.rectTransform;
        rect.anchorMin = new Vector2(0f, TOP_LINE_ANCHOR);
        rect.anchorMax = new Vector2(1f, TOP_LINE_ANCHOR);
        rect.offsetMin = new Vector2(0f, -TOP_LINE_THICKNESS * 0.5f);
        rect.offsetMax = new Vector2(0f, TOP_LINE_THICKNESS * 0.5f);
    }

    static void build_count(RectTransform column)
    {
        var view = PlayUi.rect("info_count", column);
        PlayUi.stretch(view);

        var icon_sprite = PlayUi.sprite(PLAYERS_ICON_PATH);
        var icon = PlayUi.image("count_icon", view, icon_sprite, false);
        icon.preserveAspect = true;
        if (icon_sprite != null) icon.color = PlayTheme.gold;
        LobbyUi.place_fraction(icon.rectTransform, COUNT_ICON_LEFT, COUNT_ICON_V_MIN, COUNT_ICON_RIGHT, COUNT_ICON_V_MAX);

        var label = PlayUi.text("count_label", view, string.Empty, COUNT_FONT_SIZE, PlayTheme.text, TextAnchor.LowerLeft);
        fit_text(label, COUNT_MIN_FONT_SIZE, COUNT_FONT_SIZE);
        LobbyUi.place_fraction(label.rectTransform, COUNT_TEXT_LEFT, MAIN_V_MIN, COUNT_TEXT_RIGHT, MAIN_V_MAX);

        var sub = PlayUi.text("count_subtext", view, string.Empty, SUB_FONT_SIZE, PlayTheme.text_muted, TextAnchor.UpperLeft);
        fit_text(sub, SUB_MIN_FONT_SIZE, SUB_FONT_SIZE);
        LobbyUi.place_fraction(sub.rectTransform, COUNT_TEXT_LEFT, SUB_V_MIN, COUNT_TEXT_RIGHT, SUB_V_MAX);

        var count = view.gameObject.AddComponent<LobbyInfoBarCount>();
        count.setup(label, sub);
    }

    static void build_mode(RectTransform column)
    {
        var hit = PlayUi.image("info_mode", column, PlayUi.pill(), true);
        hit.type = Image.Type.Sliced;
        hit.color = PlayTheme.with_alpha(PlayTheme.gold, 0f);
        LobbyUi.place_fraction(hit.rectTransform, MODE_HIT_LEFT, MODE_HIT_V_MIN, MODE_HIT_RIGHT, MODE_HIT_V_MAX);

        var button = hit.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;

        var group = hit.gameObject.AddComponent<CanvasGroup>();

        var mode_icon_sprite = PlayUi.sprite(MODE_ICON_PATH);
        var mode_icon = PlayUi.image("mode_icon", hit.rectTransform, mode_icon_sprite, false);
        mode_icon.preserveAspect = true;
        if (mode_icon_sprite != null) mode_icon.color = PlayTheme.gold;
        LobbyUi.place_fraction(mode_icon.rectTransform, MODE_ICON_LEFT, MODE_ICON_V_MIN, MODE_ICON_RIGHT, MODE_ICON_V_MAX);

        var mode_label = PlayUi.text("mode_name", hit.rectTransform, string.Empty, NAME_FONT_SIZE, PlayTheme.gold_soft, TextAnchor.LowerLeft);
        fit_text(mode_label, MODE_MIN_FONT_SIZE, NAME_FONT_SIZE);
        LobbyUi.place_fraction(mode_label.rectTransform, MODE_TEXT_LEFT, MAIN_V_MIN, MODE_TEXT_RIGHT, MAIN_V_MAX);

        var mode_sub = PlayUi.text("mode_subtext", hit.rectTransform, MODE_SUBTEXT, SUB_FONT_SIZE, PlayTheme.text_muted, TextAnchor.UpperLeft);
        fit_text(mode_sub, SUB_MIN_FONT_SIZE, SUB_FONT_SIZE);
        LobbyUi.place_fraction(mode_sub.rectTransform, MODE_TEXT_LEFT, SUB_V_MIN, MODE_TEXT_RIGHT, SUB_V_MAX);

        var chevron_holder = PlayUi.rect("mode_chevron_holder", hit.rectTransform);
        LobbyUi.place_fraction(chevron_holder, CHEVRON_LEFT, 0f, CHEVRON_RIGHT, 1f);
        var chevron = PlayOrnament.arrow(chevron_holder, "mode_chevron", CHEVRON_SIZE, false, PlayTheme.gold);
        chevron.rectTransform.localRotation = Quaternion.Euler(0f, 0f, CHEVRON_ROTATION);

        var hover = hit.gameObject.AddComponent<LobbyInfoBarHover>();
        hover.setup(button, hit, mode_label, PlayTheme.with_alpha(PlayTheme.gold, 0f), PlayTheme.with_alpha(PlayTheme.gold, MODE_HOVER_ALPHA), PlayTheme.gold_soft, PlayTheme.text);
        hover.set_press(PlayTheme.with_alpha(PlayTheme.gold, MODE_PRESS_ALPHA));
        hover.set_accent(chevron, PlayTheme.gold, PlayTheme.text_white);

        var mode = hit.gameObject.AddComponent<LobbyInfoBarMode>();
        mode.setup(button, group, mode_label, column);
    }

    static void fit_text(Text label, int min_size, int max_size)
    {
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = min_size;
        label.resizeTextMaxSize = max_size;
    }
}

public class LobbyInfoBarDivider : MonoBehaviour
{
    RectTransform holder_rect;
    RectTransform line_rect;
    float thickness;

    public void setup(RectTransform holder, RectTransform line, float line_thickness)
    {
        holder_rect = holder;
        line_rect = line;
        thickness = line_thickness;
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
        if (holder_rect == null || line_rect == null) return;
        line_rect.sizeDelta = new Vector2(Mathf.Max(0f, holder_rect.rect.height), thickness);
    }
}

public class LobbyInfoBarCount : MonoBehaviour
{
    Text label;
    Text sub;

    public void setup(Text target, Text sub_target)
    {
        label = target;
        sub = sub_target;
        LobbyState.players_changed += refresh;
        refresh();
    }

    void OnDestroy()
    {
        LobbyState.players_changed -= refresh;
    }

    void refresh()
    {
        if (label == null) return;
        label.text = LobbyInfoBar.count_text();
        if (sub != null) sub.text = LobbyInfoBar.count_subtext();
    }
}

public class LobbyInfoBarHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    const float FADE_IN_SECONDS = 0.15f;
    const float FADE_OUT_SECONDS = 0.1f;
    const float PRESS_IN_SECONDS = 0.05f;
    const float PRESS_OUT_SECONDS = 0.12f;

    Selectable selectable;
    Graphic fill;
    Text label;
    Color rest_fill;
    Color lit_fill;
    Color rest_text;
    Color lit_text;
    Color press_fill;
    bool has_press;
    Graphic accent;
    Color rest_accent;
    Color lit_accent;
    bool pointer_over;
    bool focused;
    bool pressed;
    float amount;
    float press_amount;

    public void setup(Selectable target, Graphic fill_graphic, Text text_label, Color fill_rest, Color fill_lit, Color text_rest, Color text_lit)
    {
        selectable = target;
        fill = fill_graphic;
        label = text_label;
        rest_fill = fill_rest;
        lit_fill = fill_lit;
        rest_text = text_rest;
        lit_text = text_lit;
        apply(0f, 0f);
    }

    public void set_press(Color fill_pressed)
    {
        press_fill = fill_pressed;
        has_press = true;
    }

    public void set_accent(Graphic graphic, Color rest, Color lit)
    {
        accent = graphic;
        rest_accent = rest;
        lit_accent = lit;
        apply(amount, press_amount);
    }

    void OnDisable()
    {
        pointer_over = false;
        focused = false;
        pressed = false;
        amount = 0f;
        press_amount = 0f;
        apply(0f, 0f);
    }

    void Update()
    {
        var usable = selectable != null && selectable.IsInteractable();
        var active = PlayFocus.lit(pointer_over, focused) && usable;
        var target = active ? 1f : 0f;
        var press_target = has_press && pressed && usable ? 1f : 0f;
        if (Mathf.Approximately(amount, target) && Mathf.Approximately(press_amount, press_target)) return;

        var seconds = active ? FADE_IN_SECONDS : FADE_OUT_SECONDS;
        amount = Mathf.MoveTowards(amount, target, Time.unscaledDeltaTime / seconds);
        var press_seconds = press_target > press_amount ? PRESS_IN_SECONDS : PRESS_OUT_SECONDS;
        press_amount = Mathf.MoveTowards(press_amount, press_target, Time.unscaledDeltaTime / press_seconds);
        apply(amount, press_amount);
    }

    void apply(float t, float p)
    {
        if (fill != null)
        {
            var color = Color.Lerp(rest_fill, lit_fill, t);
            fill.color = has_press ? Color.Lerp(color, press_fill, p) : color;
        }
        if (label != null) label.color = Color.Lerp(rest_text, lit_text, t);
        if (accent != null) accent.color = Color.Lerp(rest_accent, lit_accent, Mathf.Max(t, p));
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        pointer_over = true;
    }

    public void OnPointerExit(PointerEventData event_data)
    {
        pointer_over = false;
    }

    public void OnPointerDown(PointerEventData event_data)
    {
        pressed = true;
    }

    public void OnPointerUp(PointerEventData event_data)
    {
        pressed = false;
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

public class LobbyInfoBarMode : MonoBehaviour
{
    const float DIM_ALPHA = 0.4f;

    Button button;
    CanvasGroup group;
    Text mode_label;
    RectTransform column;

    public void setup(Button target, CanvasGroup canvas_group, Text label, RectTransform host_column)
    {
        button = target;
        group = canvas_group;
        mode_label = label;
        column = host_column;

        button.onClick.AddListener(on_click);
        LobbyState.mode_changed += refresh;
        LobbyState.players_changed += refresh;
        refresh();
    }

    void OnDestroy()
    {
        LobbyState.mode_changed -= refresh;
        LobbyState.players_changed -= refresh;
    }

    void refresh()
    {
        if (mode_label == null || button == null) return;

        var host = LobbyState.is_host;
        mode_label.text = LobbyState.mode_name;
        group.alpha = host ? 1f : DIM_ALPHA;
        button.interactable = host;
    }

    void on_click()
    {
        if (!LobbyState.is_host) return;
        LobbyInfoBarOverlay.open(column, gameObject);
    }
}

public class LobbyInfoBarOverlay : MonoBehaviour
{
    const float PANEL_PADDING = 22f;
    const float ROW_HEIGHT = 64f;
    const float PANEL_GAP = 10f;
    const float ROW_INSET = 0.06f;
    const float ROW_GAP_RATIO = 0.04f;
    const int ROW_FONT_SIZE = 30;
    const int ROW_MIN_FONT_SIZE = 14;
    const float CURRENT_FILL_ALPHA = 0.28f;
    const float CURRENT_LIT_ALPHA = 0.42f;
    const float LIT_ALPHA = 0.16f;

    static LobbyInfoBarOverlay active;

    GameObject opener;
    GameObject previous_default;
    Button[] rows;

    public static void open(RectTransform anchor_column, GameObject opener_object)
    {
        if (active != null) return;

        var host_root = LobbyScreen.overlay_root;
        if (host_root == null || anchor_column == null || !LobbyState.is_host) return;

        var games = PlayState.games;
        if (games == null || games.Length == 0) return;

        var blocker = LobbyUi.modal_blocker(host_root);
        var overlay = blocker.gameObject.AddComponent<LobbyInfoBarOverlay>();
        active = overlay;
        overlay.build(host_root, blocker, anchor_column, games, opener_object);
    }

    void build(RectTransform host_root, RectTransform blocker, RectTransform anchor_column, PlayGameData[] games, GameObject opener_object)
    {
        opener = opener_object;
        previous_default = PlayFocus.default_target;

        var blocker_button = blocker.gameObject.AddComponent<Button>();
        blocker_button.targetGraphic = blocker.GetComponent<Image>();
        blocker_button.transition = Selectable.Transition.None;
        blocker_button.navigation = new Navigation { mode = Navigation.Mode.None };
        blocker_button.onClick.AddListener(close);

        var count = games.Length;
        var panel = LobbyUi.panel("mode_list_panel", blocker, LobbyUi.PANEL_LOBBY_PATH, LobbyUi.PANEL_BORDER_SCALE);
        panel.raycastTarget = true;
        place_above(panel.rectTransform, host_root, anchor_column, count * ROW_HEIGHT + PANEL_PADDING * 2f);

        var inner = PlayUi.rect("mode_list", panel.rectTransform);
        PlayUi.stretch(inner);
        inner.offsetMin = new Vector2(PANEL_PADDING, PANEL_PADDING);
        inner.offsetMax = new Vector2(-PANEL_PADDING, -PANEL_PADDING);

        rows = new Button[count];
        var current = Mathf.Clamp(LobbyState.mode_index, 0, count - 1);
        for (var i = 0; i < count; i++) rows[i] = build_row(inner, games[i].display_name, i, count, i == current);

        for (var i = 0; i < count; i++)
        {
            var navigation = new Navigation();
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = rows[(i + count - 1) % count];
            navigation.selectOnDown = rows[(i + 1) % count];
            rows[i].navigation = navigation;
        }

        PlayFocus.default_target = rows[current].gameObject;
        if (PlayFocus.show_focus && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(rows[current].gameObject);

        LobbyState.players_changed += on_players_changed;
    }

    Button build_row(RectTransform parent, string display_name, int index, int count, bool is_current)
    {
        var row = PlayUi.image("mode_row_" + index, parent, PlayUi.pill(), true);
        row.type = Image.Type.Sliced;
        var rest_fill = is_current ? PlayTheme.with_alpha(PlayTheme.gold, CURRENT_FILL_ALPHA) : PlayTheme.clear;
        var lit_fill = PlayTheme.with_alpha(PlayTheme.gold, is_current ? CURRENT_LIT_ALPHA : LIT_ALPHA);
        row.color = rest_fill;

        var top = 1f - index / (float)count;
        var bottom = 1f - (index + 1) / (float)count;
        var gap = ROW_GAP_RATIO / count;
        LobbyUi.place_fraction(row.rectTransform, 0f, bottom + gap, 1f, top - gap);

        var button = row.gameObject.AddComponent<Button>();
        button.targetGraphic = row;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => pick(index));

        var label = PlayUi.text("mode_row_label", row.rectTransform, display_name, ROW_FONT_SIZE, is_current ? PlayTheme.gold_soft : PlayTheme.text, TextAnchor.MiddleLeft);
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = ROW_MIN_FONT_SIZE;
        label.resizeTextMaxSize = ROW_FONT_SIZE;
        LobbyUi.place_fraction(label.rectTransform, ROW_INSET, 0f, 1f - ROW_INSET, 1f);

        var hover = row.gameObject.AddComponent<LobbyInfoBarHover>();
        var rest_text = is_current ? PlayTheme.gold_soft : PlayTheme.text;
        hover.setup(button, row, label, rest_fill, lit_fill, rest_text, PlayTheme.gold_soft);

        return button;
    }

    static void place_above(RectTransform panel, RectTransform host_root, RectTransform anchor_column, float height)
    {
        var corners = new Vector3[4];
        anchor_column.GetWorldCorners(corners);
        var low = host_root.InverseTransformPoint(corners[0]);
        var high = host_root.InverseTransformPoint(corners[2]);
        var bounds = host_root.rect;

        var x_min = Mathf.Clamp01(Mathf.InverseLerp(bounds.xMin, bounds.xMax, low.x));
        var x_max = Mathf.Clamp01(Mathf.InverseLerp(bounds.xMin, bounds.xMax, high.x));
        var y_top = Mathf.Clamp01(Mathf.InverseLerp(bounds.yMin, bounds.yMax, high.y));

        panel.anchorMin = new Vector2(x_min, y_top);
        panel.anchorMax = new Vector2(x_max, y_top);
        panel.pivot = new Vector2(0.5f, 0f);
        panel.anchoredPosition = new Vector2(0f, PANEL_GAP);
        panel.sizeDelta = new Vector2(0f, height);
    }

    void pick(int index)
    {
        if (LobbyState.is_host) LobbyState.set_mode(index);
        close();
    }

    void on_players_changed()
    {
        if (!LobbyState.is_host) close();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            close();
            return;
        }

        var gamepad = Gamepad.current;
        if (gamepad != null && gamepad.buttonEast.wasPressedThisFrame) close();
    }

    void close()
    {
        if (active != this) return;

        active = null;
        LobbyState.players_changed -= on_players_changed;
        PlayFocus.default_target = previous_default;

        var restore = PlayFocus.show_focus && opener != null ? opener : null;
        Destroy(gameObject);

        if (EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(restore);
    }

    void OnDestroy()
    {
        LobbyState.players_changed -= on_players_changed;
        if (active == this) active = null;
    }
}
