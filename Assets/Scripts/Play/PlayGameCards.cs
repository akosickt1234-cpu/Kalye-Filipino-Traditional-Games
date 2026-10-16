using UnityEngine;
using UnityEngine.EventSystems;
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
    const float HEADING_ARROW_SIZE = 36f;
    const float HEADING_LEFT_ARROW_X = 84f;
    const float HEADING_ARROW_GAP = 14f;
    const float HEADING_LINE_GAP = 12f;
    const float HEADING_LINE_THICKNESS = 3f;
    const float HEADING_FALLBACK_WIDTH = 330f;
    const int NAME_MAX_SIZE = 26;
    const int NAME_MIN_SIZE = 12;
    const float MARKER_SIZE = 16f;
    const float OUTLINE_PAD = 3f;
    const float OUTLINE_THICKNESS = 3f;
    const float AURA_OUTER_PAD_X = 80f;
    const float AURA_OUTER_PAD_Y = 64f;
    const float AURA_INNER_PAD_X = 34f;
    const float AURA_INNER_PAD_Y = 28f;
    const float SHIMMER_SIZE = 64f;
    const float SPARK_MIN_SIZE = 8f;
    const float SPARK_MAX_SIZE = 16f;
    const int SHIMMER_COUNT = 3;
    const int SPARK_COUNT = 5;
    const float HOVER_OVERLAY_ALPHA = 0.18f;

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

        var left_arrow = PlayOrnament.arrow(parent, "heading_arrow_left", HEADING_ARROW_SIZE, true, PlayTheme.gold);
        place_center(left_arrow.rectTransform, HEADING_LEFT_ARROW_X, HEADING_Y, HEADING_ARROW_SIZE, HEADING_ARROW_SIZE);

        var right_arrow_x = HEADING_X + text_width + HEADING_ARROW_GAP + HEADING_ARROW_SIZE * 0.5f;
        var right_arrow = PlayOrnament.arrow(parent, "heading_arrow_right", HEADING_ARROW_SIZE, false, PlayTheme.gold);
        place_center(right_arrow.rectTransform, right_arrow_x, HEADING_Y, HEADING_ARROW_SIZE, HEADING_ARROW_SIZE);

        var line_x = right_arrow_x + HEADING_ARROW_SIZE * 0.5f + HEADING_LINE_GAP;
        var line_width = Mathf.Max(0f, ROW_X + row_width() - line_x);
        var line = PlayOrnament.line(parent, "heading_line", line_width, HEADING_LINE_THICKNESS, PlayTheme.gold, false, true);
        PlayUi.place(line.rectTransform, Vector2.zero, Vector2.zero, new Vector2(0f, 0.5f), new Vector2(line_x, REF_HEIGHT - HEADING_Y), new Vector2(line_width, HEADING_LINE_THICKNESS));
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

        var back_group = PlayUi.rect("selected_back", visual);
        PlayUi.stretch(back_group);
        var aura_outer = PlayUi.image("aura_outer", back_group, PlayUi.radial_glow());
        aura_outer.color = PlayTheme.magic_glow;
        stretch_padded(aura_outer.rectTransform, AURA_OUTER_PAD_X, AURA_OUTER_PAD_Y);
        var aura_inner = PlayUi.image("aura_inner", back_group, PlayUi.radial_glow());
        aura_inner.color = PlayTheme.glow;
        stretch_padded(aura_inner.rectTransform, AURA_INNER_PAD_X, AURA_INNER_PAD_Y);
        build_outline(back_group);

        var thumb_sprite = game != null ? PlayUi.sprite(game.thumb_path) : null;
        var thumb = PlayUi.image("thumbnail", visual, thumb_sprite);
        place_top_stretch(thumb.rectTransform, width * THUMB_HEIGHT_RATIO);

        var frame = PlayUi.image("frame", visual, frame_sprite);
        PlayUi.stretch(frame.rectTransform);

        var overlay = PlayUi.image("hover_overlay", visual, null);
        overlay.color = PlayTheme.with_alpha(PlayTheme.magic_glow, HOVER_OVERLAY_ALPHA);
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

        var front_group = PlayUi.rect("selected_front", visual);
        PlayUi.stretch(front_group);

        var shimmers = new Image[SHIMMER_COUNT];
        for (var i = 0; i < SHIMMER_COUNT; i++)
        {
            shimmers[i] = PlayUi.image("shimmer_" + i, front_group, PlayUi.radial_glow());
            shimmers[i].color = PlayTheme.magic_glow;
            place_bottom_left_center(shimmers[i].rectTransform, SHIMMER_SIZE);
        }

        var sparks = new Image[SPARK_COUNT];
        for (var i = 0; i < SPARK_COUNT; i++)
        {
            sparks[i] = PlayUi.image("spark_" + i, front_group, PlayUi.radial_glow());
            sparks[i].color = PlayTheme.magic_spark;
            place_bottom_left_center(sparks[i].rectTransform, Mathf.Lerp(SPARK_MIN_SIZE, SPARK_MAX_SIZE, spark_seed(index, i, 3)));
        }

        var marker = build_diamond("selected_marker", front_group, 0f, 0f, MARKER_SIZE, PlayTheme.gold);
        marker.anchorMin = new Vector2(0.5f, 1f);
        marker.anchorMax = new Vector2(0.5f, 1f);
        marker.anchoredPosition = Vector2.zero;

        var item = card_rect.gameObject.AddComponent<PlayGameCardItem>();
        item.index = index;
        item.card_width = width;
        item.card_height = height;
        item.visual = visual;
        item.back_group = back_group.gameObject;
        item.front_group = front_group.gameObject;
        item.overlay = overlay;
        item.name_label = name_label;
        item.aura_outer = aura_outer;
        item.aura_inner = aura_inner;
        item.shimmers = shimmers;
        item.sparks = sparks;
        item.targetGraphic = hit;
        item.transition = Selectable.Transition.None;
        item.initialize();
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

    static void stretch_padded(RectTransform r, float pad_x, float pad_y)
    {
        PlayUi.stretch(r);
        r.offsetMin = new Vector2(-pad_x, -pad_y);
        r.offsetMax = new Vector2(pad_x, pad_y);
    }

    static void place_center(RectTransform r, float center_x, float center_y, float width, float height)
    {
        PlayUi.place(r, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(center_x, REF_HEIGHT - center_y), new Vector2(width, height));
    }

    static void place_bottom_left_center(RectTransform r, float size)
    {
        PlayUi.place(r, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
    }

    public static float spark_seed(int card_index, int spark_index, int salt)
    {
        var value = Mathf.Sin((card_index * 7 + spark_index * 13 + salt * 29) * 12.9898f) * 43758.5453f;
        return value - Mathf.Floor(value);
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
    const float HOVER_ALPHA = 0.18f;
    const float BREATHE_SPEED = 1.8f;
    const float AURA_OUTER_MIN = 0.3f;
    const float AURA_OUTER_MAX = 0.7f;
    const float AURA_INNER_MIN = 0.3f;
    const float AURA_INNER_MAX = 0.55f;
    const float AURA_SCALE_GAIN = 0.04f;
    const float SHIMMER_PERIOD = 4.5f;
    const float SHIMMER_TRAIL = 0.025f;
    const float SHIMMER_INSET = 4f;
    const float SHIMMER_TRAIL_FADE = 0.3f;
    const float SPARK_RISE = 150f;
    const float SPARK_START = 0.25f;
    const float SPARK_DRIFT = 9f;
    const float SPARK_MIN_SPEED = 0.18f;
    const float SPARK_MAX_SPEED = 0.34f;

    public int index;
    public float card_width;
    public float card_height;
    public RectTransform visual;
    public GameObject back_group;
    public GameObject front_group;
    public Image overlay;
    public Text name_label;
    public Image aura_outer;
    public Image aura_inner;
    public Image[] shimmers;
    public Image[] sparks;

    bool hovered;
    bool is_selected;
    float lift_current;
    float overlay_current;

    public void initialize()
    {
        refresh_selected();
        apply_visual(true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        PlayState.selection_changed += on_selection_changed;
        hovered = false;
        initialize();
    }

    protected override void OnDisable()
    {
        PlayState.selection_changed -= on_selection_changed;
        hovered = false;
        base.OnDisable();
    }

    protected override void OnDestroy()
    {
        PlayState.selection_changed -= on_selection_changed;
        base.OnDestroy();
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
    }

    public override void OnPointerEnter(PointerEventData event_data)
    {
        base.OnPointerEnter(event_data);
        hovered = true;
    }

    public override void OnPointerExit(PointerEventData event_data)
    {
        base.OnPointerExit(event_data);
        hovered = false;
    }

    public void OnPointerClick(PointerEventData event_data)
    {
        if (event_data != null && event_data.button != PointerEventData.InputButton.Left) return;
        PlayState.select(index);
        PlayFocus.clear_selection();
    }

    public void OnSubmit(BaseEventData event_data)
    {
        PlayState.select(index);
    }

    void Update()
    {
        apply_visual(false);
        if (is_selected) animate_magic();
    }

    void on_selection_changed(int selected_index)
    {
        refresh_selected();
    }

    bool is_focused()
    {
        var event_system = EventSystem.current;
        return event_system != null && event_system.currentSelectedGameObject == gameObject;
    }

    void refresh_selected()
    {
        is_selected = PlayState.selected_index == index;
        if (back_group != null) back_group.SetActive(is_selected);
        if (front_group != null) front_group.SetActive(is_selected);
        if (name_label != null) name_label.color = is_selected ? PlayTheme.gold_soft : PlayTheme.text;
        if (is_selected) PlayFocus.default_target = gameObject;
    }

    void apply_visual(bool instant)
    {
        var lit = PlayFocus.lit(hovered, is_focused());
        var lift_target = lit ? LIFT_PIXELS : 0f;
        var overlay_target = lit ? 1f : 0f;

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
            overlay.color = PlayTheme.with_alpha(PlayTheme.magic_glow, HOVER_ALPHA * overlay_current);
        }
    }

    void animate_magic()
    {
        var time = Time.unscaledTime;
        var breathe = 0.5f + 0.5f * Mathf.Sin(time * BREATHE_SPEED + index);

        if (aura_outer != null)
        {
            aura_outer.color = PlayTheme.with_alpha(PlayTheme.magic_glow, Mathf.Lerp(AURA_OUTER_MIN, AURA_OUTER_MAX, breathe));
            var scale = 1f + AURA_SCALE_GAIN * breathe;
            aura_outer.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        if (aura_inner != null)
        {
            aura_inner.color = PlayTheme.with_alpha(PlayTheme.glow, Mathf.Lerp(AURA_INNER_MIN, AURA_INNER_MAX, 1f - breathe));
        }

        animate_shimmer(time);
        animate_sparks(time);
    }

    void animate_shimmer(float time)
    {
        if (shimmers == null) return;

        var head = time / SHIMMER_PERIOD + index * 0.25f;
        for (var i = 0; i < shimmers.Length; i++)
        {
            if (shimmers[i] == null) continue;
            var progress = head - i * SHIMMER_TRAIL;
            progress -= Mathf.Floor(progress);
            shimmers[i].rectTransform.anchoredPosition = perimeter_point(progress);
            var fade = 1f - i * SHIMMER_TRAIL_FADE;
            shimmers[i].color = PlayTheme.with_alpha(PlayTheme.magic_glow, PlayTheme.magic_glow.a * fade);
        }
    }

    void animate_sparks(float time)
    {
        if (sparks == null) return;

        for (var i = 0; i < sparks.Length; i++)
        {
            if (sparks[i] == null) continue;
            var speed = Mathf.Lerp(SPARK_MIN_SPEED, SPARK_MAX_SPEED, PlayGameCards.spark_seed(index, i, 1));
            var phase = time * speed + PlayGameCards.spark_seed(index, i, 2);
            phase -= Mathf.Floor(phase);

            var base_x = card_width * Mathf.Lerp(0.1f, 0.9f, PlayGameCards.spark_seed(index, i, 4));
            var x = base_x + Mathf.Sin(time * 1.7f + i * 2.1f) * SPARK_DRIFT;
            var y = card_height * SPARK_START + phase * SPARK_RISE;
            sparks[i].rectTransform.anchoredPosition = new Vector2(x, y);
            sparks[i].color = PlayTheme.with_alpha(PlayTheme.magic_spark, Mathf.Sin(phase * Mathf.PI));
        }
    }

    Vector2 perimeter_point(float progress)
    {
        var inner_width = Mathf.Max(1f, card_width - SHIMMER_INSET * 2f);
        var inner_height = Mathf.Max(1f, card_height - SHIMMER_INSET * 2f);
        var distance = progress * 2f * (inner_width + inner_height);

        if (distance < inner_width) return new Vector2(SHIMMER_INSET + distance, card_height - SHIMMER_INSET);
        distance -= inner_width;
        if (distance < inner_height) return new Vector2(card_width - SHIMMER_INSET, card_height - SHIMMER_INSET - distance);
        distance -= inner_height;
        if (distance < inner_width) return new Vector2(card_width - SHIMMER_INSET - distance, SHIMMER_INSET);
        distance -= inner_width;
        return new Vector2(SHIMMER_INSET, SHIMMER_INSET + distance);
    }
}
