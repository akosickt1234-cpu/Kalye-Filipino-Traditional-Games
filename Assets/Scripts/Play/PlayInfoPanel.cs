using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class PlayInfoPanel
{
    const string PANEL_PATH = "Panels/Play/PanelInfoRight";
    const string BUTTON_PATH = "Buttons/Play/BtnPlayGold";
    const string PLAY_LABEL = "Play";
    const float REFERENCE_WIDTH = 1920f;
    const float REFERENCE_HEIGHT = 1080f;
    const float TEXT_CENTER_X = 1686f;
    const float TITLE_Y = 373f;
    const float PLAYERS_Y = 458f;
    const float DESCRIPTION_X = 1509f;
    const float DESCRIPTION_TOP_Y = 540f;
    const float DESCRIPTION_HEIGHT = 150f;
    const float DESCRIPTION_WIDTH = 345f;
    const float BUTTON_Y = 786f;
    const float BUTTON_WIDTH = 380f;
    const float BUTTON_ASPECT = 768f / 217f;
    const int TITLE_MAX_SIZE = 72;
    const int TITLE_MIN_SIZE = 36;
    const int PLAYERS_MAX_SIZE = 30;
    const int PLAYERS_MIN_SIZE = 20;
    const int DESCRIPTION_MAX_SIZE = 25;
    const int DESCRIPTION_MIN_SIZE = 16;
    const int BUTTON_LABEL_SIZE = 46;
    const float DESCRIPTION_LINE_SPACING = 1.15f;
    const float LABEL_BROWN_RED = 0.3f;
    const float LABEL_BROWN_GREEN = 0.2f;
    const float LABEL_BROWN_BLUE = 0.1f;

    public static void build(RectTransform root)
    {
        if (root == null) return;

        var holder = PlayUi.rect("play_info_panel", root);
        PlayUi.stretch(holder);

        var panel = PlayUi.image("info_panel_art", holder, PlayUi.sprite(PANEL_PATH), false);
        PlayUi.stretch(panel.rectTransform);

        var text_group = PlayUi.rect("info_text_group", holder);
        PlayUi.stretch(text_group);
        var group = text_group.gameObject.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        var title = PlayUi.text("info_title", text_group, string.Empty, TITLE_MAX_SIZE, PlayTheme.text, TextAnchor.MiddleCenter);
        anchor_at(title.rectTransform, TEXT_CENTER_X, TITLE_Y, new Vector2(520f, 100f), new Vector2(0.5f, 0.5f));
        fit_text(title, TITLE_MIN_SIZE, TITLE_MAX_SIZE);

        var players = PlayUi.text("info_players", text_group, string.Empty, PLAYERS_MAX_SIZE, PlayTheme.text, TextAnchor.MiddleCenter);
        anchor_at(players.rectTransform, TEXT_CENTER_X, PLAYERS_Y, new Vector2(340f, 44f), new Vector2(0.5f, 0.5f));
        fit_text(players, PLAYERS_MIN_SIZE, PLAYERS_MAX_SIZE);

        var description = PlayUi.text("info_description", text_group, string.Empty, DESCRIPTION_MAX_SIZE, PlayTheme.text_muted, TextAnchor.UpperLeft);
        anchor_at(description.rectTransform, DESCRIPTION_X, DESCRIPTION_TOP_Y, new Vector2(DESCRIPTION_WIDTH, DESCRIPTION_HEIGHT), new Vector2(0f, 1f));
        description.lineSpacing = DESCRIPTION_LINE_SPACING;
        fit_text(description, DESCRIPTION_MIN_SIZE, DESCRIPTION_MAX_SIZE);

        var view = holder.gameObject.AddComponent<PlayInfoPanelView>();
        view.setup(group, title, players, description);

        build_play_button(holder);
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

        var label = PlayUi.text("play_button_label", button_rect, PLAY_LABEL, BUTTON_LABEL_SIZE, label_color(), TextAnchor.MiddleCenter);
        PlayUi.stretch(label.rectTransform);

        var fx = button_rect.gameObject.AddComponent<PlayButtonFx>();
        fx.setup(button_rect, glow_group);
        button.onClick.AddListener(fx.open_lobby);
    }

    static Color label_color()
    {
        var gold = PlayTheme.gold;
        return new Color(gold.r * LABEL_BROWN_RED, gold.g * LABEL_BROWN_GREEN, gold.b * LABEL_BROWN_BLUE, 1f);
    }

    static void fit_text(Text label, int min_size, int max_size)
    {
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = min_size;
        label.resizeTextMaxSize = max_size;
        label.verticalOverflow = VerticalWrapMode.Truncate;
    }

    static void anchor_at(RectTransform rect, float x, float y, Vector2 size, Vector2 pivot)
    {
        var anchor = new Vector2(x / REFERENCE_WIDTH, 1f - y / REFERENCE_HEIGHT);
        PlayUi.place(rect, anchor, anchor, pivot, Vector2.zero, size);
    }
}

public class PlayInfoPanelView : MonoBehaviour
{
    const float HALF_FADE_SECONDS = 0.1f;

    CanvasGroup group;
    Text title_label;
    Text players_label;
    Text description_label;
    string shown_id;
    Coroutine fade_routine;

    public void setup(CanvasGroup text_group, Text title, Text players, Text description)
    {
        group = text_group;
        title_label = title;
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

        if (title_label != null) title_label.text = game != null && game.display_name != null ? game.display_name : string.Empty;
        if (players_label != null) players_label.text = format_players(game);
        if (description_label != null) description_label.text = game != null && game.description != null ? game.description : string.Empty;
    }

    static string format_players(PlayGameData game)
    {
        if (game == null || game.max_players <= 0) return string.Empty;
        if (game.min_players <= 0 || game.min_players == game.max_players) return $"{game.max_players} Players";
        return $"{game.min_players} - {game.max_players} Players";
    }
}

public class PlayButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
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

        var active = hovered || focused;
        var scale_goal = pressed ? PRESS_SCALE : active ? HOVER_SCALE : 1f;
        var glow_goal = active || pressed ? 1f : 0f;
        var blend = 1f - Mathf.Exp(-SMOOTH_SPEED * Time.unscaledDeltaTime);

        var scale = Mathf.Lerp(target.localScale.x, scale_goal, blend);
        target.localScale = new Vector3(scale, scale, 1f);
        if (glow_group != null) glow_group.alpha = Mathf.Lerp(glow_group.alpha, glow_goal, blend);
    }
}
