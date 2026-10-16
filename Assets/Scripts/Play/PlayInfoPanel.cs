using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class PlayInfoPanel
{
    const string PANEL_PATH = "Panels/Play/PanelInfoRight";
    const string BUTTON_PATH = "Buttons/Play/BtnPlayGold";
    const string PLAYERS_ICON_PATH = "Icons/Play/IconPlayers";
    const string PLAY_LABEL = "Play";
    const float REFERENCE_WIDTH = 1920f;
    const float REFERENCE_HEIGHT = 1080f;
    const float TEXT_CENTER_X = 1686f;
    const float ARCH_WIDTH = 340f;
    const float TITLE_WIDTH = 290f;
    const float TITLE_HEIGHT = 70f;
    const float TITLE_TOP_Y = 330f;
    const float PLAYERS_Y = 442f;
    const float CREST_Y = 480f;
    const float CREST_WIDTH = 300f;
    const float PLAYERS_ROW_HEIGHT = 44f;
    const float DESCRIPTION_X = 1509f;
    const float DESCRIPTION_TOP_Y = 524f;
    const float DESCRIPTION_WIDTH = 345f;
    const float DESCRIPTION_HEIGHT = 170f;
    const float BOTTOM_GRADIENT_HEIGHT = 560f;
    const float BOTTOM_GRADIENT_UPPER_HEIGHT = 320f;
    const float BUTTON_Y = 786f;
    const float BUTTON_WIDTH = 380f;
    const float BUTTON_ASPECT = 768f / 217f;
    const int TITLE_FONT_SIZE = 56;
    const int PLAYERS_FONT_SIZE = 30;
    const int DESCRIPTION_MAX_SIZE = 25;
    const int DESCRIPTION_MIN_SIZE = 16;
    const int BUTTON_LABEL_SIZE = 46;
    const float DESCRIPTION_LINE_SPACING = 1.15f;

    public static void build(RectTransform root)
    {
        if (root == null) return;

        var holder = PlayUi.rect("play_info_panel", root);
        PlayUi.stretch(holder);

        var panel = PlayUi.image("info_panel_art", holder, PlayUi.sprite(PANEL_PATH), false);
        PlayUi.stretch(panel.rectTransform);

        build_bottom_gradient(holder);

        var text_group = PlayUi.rect("info_text_group", holder);
        PlayUi.stretch(text_group);
        var group = text_group.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        var title = PlayMarquee.create(text_group, "info_title", TITLE_FONT_SIZE, PlayTheme.text, TITLE_WIDTH, TITLE_HEIGHT);
        anchor_at(title.rect, TEXT_CENTER_X, TITLE_TOP_Y, new Vector2(TITLE_WIDTH, TITLE_HEIGHT), new Vector2(0.5f, 1f));

        var players_row = PlayUi.rect("info_players_row", text_group);
        anchor_at(players_row, TEXT_CENTER_X, PLAYERS_Y, new Vector2(ARCH_WIDTH, PLAYERS_ROW_HEIGHT), new Vector2(0.5f, 0.5f));

        var players_icon_sprite = PlayUi.sprite(PLAYERS_ICON_PATH);
        var players_icon = PlayUi.image("info_players_icon", players_row, players_icon_sprite, false);
        players_icon.preserveAspect = true;
        if (players_icon_sprite != null) players_icon.color = PlayTheme.icon;

        var players = PlayUi.text("info_players", players_row, string.Empty, PLAYERS_FONT_SIZE, PlayTheme.text, TextAnchor.MiddleCenter);
        players.horizontalOverflow = HorizontalWrapMode.Overflow;

        var crest = PlayOrnament.crest_line(text_group, "info_crest_line", CREST_WIDTH, PlayTheme.icon);
        anchor_at(crest, TEXT_CENTER_X, CREST_Y, crest.sizeDelta, new Vector2(0.5f, 0.5f));

        var description = PlayUi.text("info_description", text_group, string.Empty, DESCRIPTION_MAX_SIZE, PlayTheme.text_white, TextAnchor.UpperLeft);
        anchor_at(description.rectTransform, DESCRIPTION_X, DESCRIPTION_TOP_Y, new Vector2(DESCRIPTION_WIDTH, DESCRIPTION_HEIGHT), new Vector2(0f, 1f));
        description.lineSpacing = DESCRIPTION_LINE_SPACING;
        fit_text(description, DESCRIPTION_MIN_SIZE, DESCRIPTION_MAX_SIZE);

        var view = holder.gameObject.AddComponent<PlayInfoPanelView>();
        view.setup(group, title, players_row, players_icon, players, description);

        build_play_button(holder);
    }

    static void build_bottom_gradient(RectTransform parent)
    {
        add_bottom_gradient(parent, "info_bottom_gradient", BOTTOM_GRADIENT_HEIGHT);
        add_bottom_gradient(parent, "info_bottom_gradient_solid", BOTTOM_GRADIENT_UPPER_HEIGHT);
    }

    static void add_bottom_gradient(RectTransform parent, string name, float height)
    {
        var gradient = PlayUi.image(name, parent, PlayUi.vertical_fade(), false);
        gradient.color = PlayTheme.panel_blue;

        var rect = gradient.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = new Vector2(1f, height / REFERENCE_HEIGHT);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void build_play_button(RectTransform parent)
    {
        var button_rect = PlayUi.rect("play_button", parent);
        anchor_at(button_rect, TEXT_CENTER_X, BUTTON_Y, new Vector2(BUTTON_WIDTH, BUTTON_WIDTH / BUTTON_ASPECT), new Vector2(0.5f, 0.5f));

        var sprite = PlayUi.sprite(BUTTON_PATH);
        var face = button_rect.gameObject.AddComponent<Image>();
        face.sprite = sprite;
        face.raycastTarget = true;
        face.enabled = sprite != null;

        var button = button_rect.gameObject.AddComponent<Button>();
        button.targetGraphic = face;
        button.transition = Selectable.Transition.None;

        var glow_rect = PlayUi.rect("play_button_glow", button_rect);
        PlayUi.stretch(glow_rect);
        var glow_image = glow_rect.gameObject.AddComponent<Image>();
        glow_image.sprite = sprite;
        glow_image.color = PlayTheme.glow;
        glow_image.raycastTarget = false;
        glow_image.enabled = sprite != null;
        var glow_group = glow_rect.gameObject.AddComponent<CanvasGroup>();
        glow_group.alpha = 0f;
        glow_group.blocksRaycasts = false;
        glow_group.interactable = false;

        var label = PlayUi.text("play_button_label", button_rect, PLAY_LABEL, BUTTON_LABEL_SIZE, PlayTheme.label_brown, TextAnchor.MiddleCenter);
        PlayUi.stretch(label.rectTransform);

        var fx = button_rect.gameObject.AddComponent<PlayButtonFx>();
        fx.setup(button_rect, glow_group);
        button.onClick.AddListener(fx.open_lobby);
    }

    static void fit_text(Text label, int min_size, int max_size)
    {
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = min_size;
        label.resizeTextMaxSize = max_size;
        label.verticalOverflow = VerticalWrapMode.Truncate;
    }

    internal static void anchor_at(RectTransform rect, float x, float y, Vector2 size, Vector2 pivot)
    {
        var anchor = new Vector2(x / REFERENCE_WIDTH, 1f - y / REFERENCE_HEIGHT);
        PlayUi.place(rect, anchor, anchor, pivot, Vector2.zero, size);
    }
}

public class PlayInfoPanelView : MonoBehaviour
{
    const float HALF_FADE_SECONDS = 0.1f;
    const float TEXT_CENTER_X = 1686f;
    const float ARCH_WIDTH = 340f;
    const float PLAYERS_Y = 442f;
    const float PLAYERS_ROW_HEIGHT = 44f;
    const float PLAYERS_ICON_SIZE = 34f;
    const float PLAYERS_ICON_GAP = 10f;

    static readonly Vector2 CENTER = new Vector2(0.5f, 0.5f);

    CanvasGroup group;
    PlayMarquee title_marquee;
    RectTransform players_row;
    Image players_icon;
    Text players_label;
    Text description_label;
    string shown_id;
    Coroutine fade_routine;

    public void setup(CanvasGroup text_group, PlayMarquee title, RectTransform row, Image icon, Text players, Text description)
    {
        group = text_group;
        title_marquee = title;
        players_row = row;
        players_icon = icon;
        players_label = players;
        description_label = description;
        refresh();
    }

    void OnEnable()
    {
        PlayState.selection_changed += on_selection_changed;
    }

    void OnDisable()
    {
        PlayState.selection_changed -= on_selection_changed;
        fade_routine = null;
        if (group != null) group.alpha = 1f;
    }

    void on_selection_changed(int index)
    {
        var game = PlayState.current;
        var id = game != null ? game.id : null;
        if (id == shown_id) return;

        if (fade_routine != null)
        {
            StopCoroutine(fade_routine);
            fade_routine = null;
        }

        if (!isActiveAndEnabled || group == null)
        {
            refresh();
            return;
        }

        fade_routine = StartCoroutine(swap());
    }

    IEnumerator swap()
    {
        yield return fade_to(0f);
        refresh();
        yield return fade_to(1f);
        fade_routine = null;
    }

    IEnumerator fade_to(float target)
    {
        var start = group.alpha;
        var elapsed = 0f;
        while (elapsed < HALF_FADE_SECONDS)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / HALF_FADE_SECONDS));
            yield return null;
        }

        group.alpha = target;
    }

    void refresh()
    {
        var game = PlayState.current;
        shown_id = game != null ? game.id : null;

        if (title_marquee != null) title_marquee.set_text(game != null && game.display_name != null ? game.display_name : string.Empty);
        if (players_label != null) players_label.text = format_players(game);
        if (description_label != null) description_label.text = game != null && game.description != null ? game.description : string.Empty;

        layout_players(PLAYERS_Y);
    }

    void layout_players(float players_y)
    {
        if (players_row == null) return;

        PlayInfoPanel.anchor_at(players_row, TEXT_CENTER_X, players_y, new Vector2(ARCH_WIDTH, PLAYERS_ROW_HEIGHT), CENTER);

        var has_text = !string.IsNullOrEmpty(players_label.text);
        var has_icon = has_text && players_icon != null && players_icon.sprite != null;
        if (players_icon != null) players_icon.enabled = has_icon;

        var text_width = has_text ? players_label.preferredWidth : 0f;
        var icon_block = has_icon ? PLAYERS_ICON_SIZE + PLAYERS_ICON_GAP : 0f;
        var left_edge = -(icon_block + text_width) * 0.5f;

        if (has_icon)
        {
            var icon_x = left_edge + PLAYERS_ICON_SIZE * 0.5f;
            PlayUi.place(players_icon.rectTransform, CENTER, CENTER, CENTER, new Vector2(icon_x, 0f), new Vector2(PLAYERS_ICON_SIZE, PLAYERS_ICON_SIZE));
        }

        var text_x = left_edge + icon_block + text_width * 0.5f;
        PlayUi.place(players_label.rectTransform, CENTER, CENTER, CENTER, new Vector2(text_x, 0f), new Vector2(text_width, PLAYERS_ROW_HEIGHT));
    }

    static string format_players(PlayGameData game)
    {
        if (game == null || game.max_players <= 0) return string.Empty;
        if (game.min_players <= 0 || game.min_players == game.max_players)
        {
            return game.max_players == 1 ? "1 Player" : $"{game.max_players} Players";
        }

        return $"{game.min_players} - {game.max_players} Players";
    }
}

public class PlayButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    const string LOBBY_SCENE = "Lobby";
    const float HOVER_SCALE = 1.03f;
    const float PRESS_SCALE = 0.97f;
    const float SMOOTH_SPEED = 18f;

    RectTransform target;
    CanvasGroup glow_group;
    bool hovered;
    bool focused;
    bool pressed;
    bool opening;

    public void setup(RectTransform button_rect, CanvasGroup glow)
    {
        target = button_rect;
        glow_group = glow;
    }

    public void open_lobby()
    {
        if (opening) return;
        opening = Application.CanStreamedLevelBeLoaded(LOBBY_SCENE);
        PlayUi.go_to_scene(LOBBY_SCENE);
    }

    public void OnPointerEnter(PointerEventData event_data) { hovered = true; }
    public void OnPointerExit(PointerEventData event_data) { hovered = false; pressed = false; }
    public void OnPointerDown(PointerEventData event_data) { pressed = true; }
    public void OnPointerUp(PointerEventData event_data) { pressed = false; }
    public void OnPointerClick(PointerEventData event_data) { PlayFocus.clear_selection(); }
    public void OnSelect(BaseEventData event_data) { focused = true; }
    public void OnDeselect(BaseEventData event_data) { focused = false; pressed = false; }

    void OnDisable()
    {
        hovered = false;
        focused = false;
        pressed = false;
        if (target != null) target.localScale = Vector3.one;
        if (glow_group != null) glow_group.alpha = 0f;
    }

    void Update()
    {
        if (target == null) return;

        var active = PlayFocus.lit(hovered, focused);
        var scale_goal = pressed ? PRESS_SCALE : active ? HOVER_SCALE : 1f;
        var glow_goal = active || pressed ? 1f : 0f;
        var blend = 1f - Mathf.Exp(-SMOOTH_SPEED * Time.unscaledDeltaTime);

        var scale = Mathf.Lerp(target.localScale.x, scale_goal, blend);
        target.localScale = new Vector3(scale, scale, 1f);
        if (glow_group != null) glow_group.alpha = Mathf.Lerp(glow_group.alpha, glow_goal, blend);
    }
}
