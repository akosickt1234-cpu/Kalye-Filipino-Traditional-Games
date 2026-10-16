using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class PlayGameCards
{
    const float REF_HEIGHT = 1080f;
    const string FRAME_PATH = "Frames/Play/FrameGameCard";
    const float FRAME_WIDTH = 512f;
    const float FRAME_HEIGHT = 341f;
    const float THUMB_HEIGHT_RATIO = 288f / 512f;
    const float PLATE_TOP_RATIO = 251f / 341f;
    const float PLATE_BOTTOM_RATIO = 317f / 341f;
    const float CARD_WIDTH = 280f;
    const float CARD_GAP = 20f;
    const float ROW_X = 70f;
    const float ROW_Y = 770f;
    const float HEADING_X = 110f;
    const float HEADING_Y = 715f;
    const float HEADING_HEIGHT = 64f;
    const int HEADING_SIZE = 42;
    const float HEADING_ORNAMENT_SIZE = 16f;
    const float HEADING_LEFT_ORNAMENT_X = 80f;
    const float HEADING_ORNAMENT_GAP = 28f;
    const float HEADING_LINE_GAP = 22f;
    const float HEADING_LINE_THICKNESS = 2f;
    const float HEADING_LINE_ALPHA = 0.7f;
    const float HEADING_FALLBACK_WIDTH = 330f;
    const int NAME_MAX_SIZE = 26;
    const int NAME_MIN_SIZE = 12;
    const float MARKER_SIZE = 16f;
    const float OUTLINE_PAD = 3f;
    const float OUTLINE_THICKNESS = 3f;
    static readonly float[] GLOW_PADS = { 6f, 11f, 16f };
    static readonly float[] GLOW_ALPHAS = { 0.6f, 0.4f, 0.22f };
    const float HOVER_OVERLAY_ALPHA = 0.5f;

    public static void build(RectTransform root)
    {
        if (root == null) return;

        var holder = PlayUi.rect("game_cards", root);
        PlayUi.stretch(holder);

        build_heading(holder);
        build_row(holder);
    }

    static void build_heading(RectTransform parent)
    {
        var label = PlayUi.text("heading_text", parent, "CHOOSE A GAME", HEADING_SIZE, PlayTheme.text, TextAnchor.MiddleLeft);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        var text_width = label.preferredWidth;
        if (text_width < 1f) text_width = HEADING_FALLBACK_WIDTH;
        place_top_left(label.rectTransform, HEADING_X, HEADING_Y - HEADING_HEIGHT * 0.5f, text_width, HEADING_HEIGHT);

        var right_x = HEADING_X + text_width + HEADING_ORNAMENT_GAP;
        build_diamond("heading_diamond_left", parent, HEADING_LEFT_ORNAMENT_X, HEADING_Y, HEADING_ORNAMENT_SIZE, PlayTheme.gold);
        build_diamond("heading_diamond_right", parent, right_x, HEADING_Y, HEADING_ORNAMENT_SIZE, PlayTheme.gold);

        var line_x = right_x + HEADING_LINE_GAP;
        var line_end = ROW_X + row_width();
        var line = PlayUi.image("heading_line", parent, null);
        line.color = PlayTheme.with_alpha(PlayTheme.gold, HEADING_LINE_ALPHA);
        place_top_left(line.rectTransform, line_x, HEADING_Y - HEADING_LINE_THICKNESS * 0.5f, Mathf.Max(0f, line_end - line_x), HEADING_LINE_THICKNESS);
    }

    static void build_row(RectTransform parent)
    {
        var games = PlayState.games;
        if (games == null || games.Length == 0) return;

        var frame_sprite = PlayUi.sprite(FRAME_PATH);
        var card_height = CARD_WIDTH * FRAME_HEIGHT / FRAME_WIDTH;
        var cards = new PlayGameCardItem[games.Length];

        for (var i = 0; i < games.Length; i++)
        {
            var x = ROW_X + i * (CARD_WIDTH + CARD_GAP);
            cards[i] = build_card(parent, i, games[i], frame_sprite, x, ROW_Y, CARD_WIDTH, card_height);
        }

        for (var i = 0; i < cards.Length; i++)
        {
            var navigation = new Navigation { mode = Navigation.Mode.Explicit };
            navigation.selectOnLeft = i > 0 ? cards[i - 1] : null;
            navigation.selectOnRight = i < cards.Length - 1 ? cards[i + 1] : null;
            cards[i].navigation = navigation;
        }

        var row = parent.gameObject.AddComponent<PlayGameCardsFocus>();
        row.cards = cards;
    }

    static PlayGameCardItem build_card(RectTransform parent, int index, PlayGameData game, Sprite frame_sprite, float x, float y, float width, float height)
    {
        var id = game != null && !string.IsNullOrEmpty(game.id) ? game.id : index.ToString();
        var card_rect = PlayUi.rect("card_" + id, parent);
        place_top_left(card_rect, x, y, width, height);

        var hit = PlayUi.image("hit_area", card_rect, null, true);
        PlayUi.stretch(hit.rectTransform);

        var visual = PlayUi.rect("visual", card_rect);
        PlayUi.stretch(visual);

        var selected_group = PlayUi.rect("selected_look", visual);
        PlayUi.stretch(selected_group);
        for (var i = 0; i < GLOW_PADS.Length; i++)
        {
            var layer = PlayUi.image("glow_" + i, selected_group, null);
            layer.color = PlayTheme.with_alpha(PlayTheme.glow, GLOW_ALPHAS[i]);
            stretch_padded(layer.rectTransform, GLOW_PADS[i]);
        }

        build_outline(selected_group);

        var thumb_sprite = game != null ? PlayUi.sprite(game.thumb_path) : null;
        var thumb = PlayUi.image("thumbnail", visual, thumb_sprite);
        place_top_stretch(thumb.rectTransform, width * THUMB_HEIGHT_RATIO);

        var frame = PlayUi.image("frame", visual, frame_sprite);
        PlayUi.stretch(frame.rectTransform);

        var overlay = PlayUi.image("hover_overlay", visual, null);
        overlay.color = PlayTheme.with_alpha(PlayTheme.glow, HOVER_OVERLAY_ALPHA);
        PlayUi.stretch(overlay.rectTransform);

        var name_value = game != null ? game.display_name : string.Empty;
        var name_label = PlayUi.text("name", visual, name_value, NAME_MAX_SIZE, PlayTheme.text, TextAnchor.MiddleCenter);
        name_label.resizeTextForBestFit = true;
        name_label.resizeTextMinSize = NAME_MIN_SIZE;
        name_label.resizeTextMaxSize = NAME_MAX_SIZE;
        name_label.horizontalOverflow = HorizontalWrapMode.Wrap;
        var name_rect = name_label.rectTransform;
        name_rect.anchorMin = new Vector2(0f, 1f - PLATE_BOTTOM_RATIO);
        name_rect.anchorMax = new Vector2(1f, 1f - PLATE_TOP_RATIO);
        name_rect.offsetMin = Vector2.zero;
        name_rect.offsetMax = Vector2.zero;

        var marker = build_diamond("selected_marker", visual, 0f, 0f, MARKER_SIZE, PlayTheme.gold);
        marker.anchorMin = new Vector2(0.5f, 1f);
        marker.anchorMax = new Vector2(0.5f, 1f);
        marker.anchoredPosition = Vector2.zero;

        var item = card_rect.gameObject.AddComponent<PlayGameCardItem>();
        item.index = index;
        item.visual = visual;
        item.selected_group = selected_group.gameObject;
        item.marker = marker.gameObject;
        item.overlay = overlay;
        item.name_label = name_label;
        item.targetGraphic = hit;
        item.transition = Selectable.Transition.None;
        return item;
    }

    static void build_outline(RectTransform parent)
    {
        var holder = PlayUi.rect("outline", parent);
        PlayUi.stretch(holder);

        build_edge("outline_top", holder, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, OUTLINE_PAD), new Vector2(OUTLINE_PAD * 2f, OUTLINE_THICKNESS));
        build_edge("outline_bottom", holder, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, -OUTLINE_PAD), new Vector2(OUTLINE_PAD * 2f, OUTLINE_THICKNESS));
        build_edge("outline_left", holder, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(-OUTLINE_PAD, 0f), new Vector2(OUTLINE_THICKNESS, OUTLINE_PAD * 2f));
        build_edge("outline_right", holder, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(OUTLINE_PAD, 0f), new Vector2(OUTLINE_THICKNESS, OUTLINE_PAD * 2f));
    }

    static void build_edge(string name, RectTransform parent, Vector2 anchor_min, Vector2 anchor_max, Vector2 pivot, Vector2 offset, Vector2 size)
    {
        var edge = PlayUi.image(name, parent, null);
        edge.color = PlayTheme.gold;
        var r = edge.rectTransform;
        r.anchorMin = anchor_min;
        r.anchorMax = anchor_max;
        r.pivot = pivot;
        r.anchoredPosition = offset;
        r.sizeDelta = size;
    }

    static RectTransform build_diamond(string name, RectTransform parent, float center_x, float center_y, float size, Color color)
    {
        var diamond = PlayUi.image(name, parent, null);
        diamond.color = color;
        var r = diamond.rectTransform;
        PlayUi.place(r, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(center_x, REF_HEIGHT - center_y), new Vector2(size, size));
        r.localRotation = Quaternion.Euler(0f, 0f, 45f);
        return r;
    }

    static void place_top_left(RectTransform r, float x, float y_top, float width, float height)
    {
        PlayUi.place(r, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(x, REF_HEIGHT - y_top - height), new Vector2(width, height));
    }

    static void place_top_stretch(RectTransform r, float height)
    {
        r.anchorMin = new Vector2(0f, 1f);
        r.anchorMax = new Vector2(1f, 1f);
        r.pivot = new Vector2(0.5f, 1f);
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta = new Vector2(0f, height);
    }

    static void stretch_padded(RectTransform r, float pad)
    {
        PlayUi.stretch(r);
        r.offsetMin = new Vector2(-pad, -pad);
        r.offsetMax = new Vector2(pad, pad);
    }

    static float row_width()
    {
        var count = PlayState.games != null ? PlayState.games.Length : 0;
        if (count <= 0) return CARD_WIDTH;
        return count * CARD_WIDTH + (count - 1) * CARD_GAP;
    }
}

public class PlayGameCardItem : Selectable, IPointerClickHandler, ISubmitHandler
{
    const float LIFT_PIXELS = 6f;
    const float LERP_SPEED = 18f;
    const float SNAP_EPSILON = 0.01f;

    public int index;
    public RectTransform visual;
    public GameObject selected_group;
    public GameObject marker;
    public Image overlay;
    public Text name_label;

    bool active_state;
    float lift_current;
    float overlay_current;

    protected override void OnEnable()
    {
        base.OnEnable();
        PlayState.selection_changed += on_selection_changed;
        refresh_selected();
        apply_visual(true);
    }

    protected override void OnDisable()
    {
        PlayState.selection_changed -= on_selection_changed;
        base.OnDisable();
    }

    protected override void OnDestroy()
    {
        PlayState.selection_changed -= on_selection_changed;
        base.OnDestroy();
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        active_state = state == SelectionState.Highlighted || state == SelectionState.Selected || state == SelectionState.Pressed;
        if (instant) apply_visual(true);
    }

    public void OnPointerClick(PointerEventData event_data)
    {
        if (event_data != null && event_data.button != PointerEventData.InputButton.Left) return;
        PlayState.select(index);
    }

    public void OnSubmit(BaseEventData event_data)
    {
        PlayState.select(index);
    }

    void Update()
    {
        apply_visual(false);
    }

    void on_selection_changed(int selected_index)
    {
        refresh_selected();
    }

    void refresh_selected()
    {
        var is_selected = PlayState.selected_index == index;
        if (selected_group != null) selected_group.SetActive(is_selected);
        if (marker != null) marker.SetActive(is_selected);
        if (name_label != null) name_label.color = is_selected ? PlayTheme.gold_soft : PlayTheme.text;
    }

    void apply_visual(bool instant)
    {
        var lift_target = active_state ? LIFT_PIXELS : 0f;
        var overlay_target = active_state ? 1f : 0f;

        if (instant)
        {
            lift_current = lift_target;
            overlay_current = overlay_target;
        }
        else
        {
            if (Mathf.Abs(lift_current - lift_target) < SNAP_EPSILON && Mathf.Abs(overlay_current - overlay_target) < SNAP_EPSILON) return;
            var step = 1f - Mathf.Exp(-LERP_SPEED * Time.unscaledDeltaTime);
            lift_current = Mathf.Lerp(lift_current, lift_target, step);
            overlay_current = Mathf.Lerp(overlay_current, overlay_target, step);
        }

        if (visual != null) visual.anchoredPosition = new Vector2(0f, lift_current);
        if (overlay != null)
        {
            overlay.enabled = overlay_current > SNAP_EPSILON;
            overlay.canvasRenderer.SetAlpha(overlay_current);
        }
    }
}

public class PlayGameCardsFocus : MonoBehaviour
{
    public PlayGameCardItem[] cards;

    void Update()
    {
        if (cards == null || cards.Length == 0) return;
        var event_system = EventSystem.current;
        if (event_system == null || event_system.currentSelectedGameObject != null) return;
        if (!direction_pressed()) return;

        var target = cards[Mathf.Clamp(PlayState.selected_index, 0, cards.Length - 1)];
        if (target != null) event_system.SetSelectedGameObject(target.gameObject);
    }

    static bool direction_pressed()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)) return true;

        var gamepad = Gamepad.current;
        return gamepad != null && (gamepad.dpad.left.wasPressedThisFrame || gamepad.dpad.right.wasPressedThisFrame);
    }
}
