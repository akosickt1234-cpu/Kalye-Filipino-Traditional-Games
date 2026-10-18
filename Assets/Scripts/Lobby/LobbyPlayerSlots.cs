using UnityEngine;
using UnityEngine.UI;

public static class LobbyPlayerSlots
{
    const string TITLE = "GATHER YOUR TEAM";
    const string HOST_LABEL = "HOST";
    const string YOU_LABEL = "YOU";
    const string INVITE_LABEL = "Invite Friend";
    const string READY_LABEL = "Ready";
    const string WAITING_LABEL = "Waiting...";
    const float CARD_ASPECT = 485f / 768f;
    const float AVATAR_ASPECT = 512f / 415f;
    const float CARD_BORDER_SCALE = 1.8f;
    const float CELL_PAD_X = 0.04f;
    const float CELL_TOP = 0.90f;
    const float CELL_BOTTOM = 0.05f;
    const float EMBLEM_RIGHT = 0.09f;
    const float TITLE_LEFT = 0.10f;
    const float TITLE_RIGHT = 0.72f;
    const float CREST_LEFT = 0.74f;
    const float EMBLEM_OUTER_MIN = 0.12f;
    const float EMBLEM_OUTER_MAX = 0.88f;
    const float EMBLEM_INNER_MIN = 0.32f;
    const float EMBLEM_INNER_MAX = 0.68f;
    const float EMBLEM_CORE_MIN = 0.42f;
    const float EMBLEM_CORE_MAX = 0.58f;
    const float DIAMOND_ROTATION = 45f;
    const float CREST_ARROW_SIZE = 22f;
    const float CREST_GAP = 8f;
    const float CREST_THICKNESS = 2f;
    const float GLOW_MIN = -0.25f;
    const float GLOW_MAX = 1.25f;
    const float RING_ALPHA = 0.55f;
    const float RING_INSET = 0.05f;
    const float PLUS_MIN = 0.30f;
    const float PLUS_MAX = 0.70f;
    const float PLUS_HALF_THICKNESS = 0.025f;
    const float MASK_MIN = 0.10f;
    const float MASK_MAX = 0.90f;
    const float DOT_SIZE = 14f;
    const float STATUS_SPACING = 8f;
    const int TITLE_MAX_SIZE = 56;
    const int TITLE_MIN_SIZE = 18;
    const int LABEL_MAX_SIZE = 22;
    const int LABEL_MIN_SIZE = 10;
    const int TAG_MAX_SIZE = 20;
    const int STATUS_SIZE = 22;
    const int NAME_MAX_SIZE = 30;
    const int NAME_MIN_SIZE = 12;
    const float NAME_HEIGHT_RATIO = 0.7f;

    static RectTransform row_root;

    public static void build(RectTransform heading_slot, RectTransform slots_row)
    {
        if (heading_slot != null) build_heading(heading_slot);
        if (slots_row == null) return;

        row_root = slots_row;
        LobbyState.players_changed -= rebuild;
        LobbyState.mode_changed -= rebuild;
        LobbyState.players_changed += rebuild;
        LobbyState.mode_changed += rebuild;
        rebuild();
    }

    static void rebuild()
    {
        if (row_root == null) return;

        for (var i = row_root.childCount - 1; i >= 0; i--)
        {
            var child = row_root.GetChild(i).gameObject;
            child.SetActive(false);
            Object.Destroy(child);
        }

        var count = LobbyState.team_size;
        var players = LobbyState.players;
        for (var i = 0; i < count; i++)
        {
            var player = i < players.Count ? players[i] : null;
            var cell = PlayUi.rect("slot_" + i, row_root);
            LobbyUi.place_fraction(cell, i / (float)count, 0f, (i + 1) / (float)count, 1f);
            build_card(cell, player != null && !player.is_empty ? player : null);
        }
    }

    static void build_heading(RectTransform heading_slot)
    {
        var emblem_cell = PlayUi.rect("emblem_cell", heading_slot);
        LobbyUi.place_fraction(emblem_cell, 0f, 0f, EMBLEM_RIGHT, 1f);
        build_emblem(emblem_cell);

        var title_cell = PlayUi.rect("title_cell", heading_slot);
        LobbyUi.place_fraction(title_cell, TITLE_LEFT, 0f, TITLE_RIGHT, 1f);
        var title = PlayUi.text("title", title_cell, TITLE, TITLE_MAX_SIZE, PlayTheme.gold, TextAnchor.MiddleLeft);
        fit_text(title, TITLE_MIN_SIZE, TITLE_MAX_SIZE);
        PlayUi.stretch(title.rectTransform);
        PlayUi.add_shadow(title);

        var crest_cell = PlayUi.rect("crest_cell", heading_slot);
        LobbyUi.place_fraction(crest_cell, CREST_LEFT, 0f, 1f, 1f);
        build_crest(crest_cell);
    }

    static void build_emblem(RectTransform cell)
    {
        var emblem = PlayUi.rect("emblem", cell);
        PlayUi.stretch(emblem);
        fit_aspect(emblem, 1f, AspectRatioFitter.AspectMode.FitInParent);

        build_diamond("diamond_outer", emblem, EMBLEM_OUTER_MIN, EMBLEM_OUTER_MAX, PlayTheme.gold);
        build_diamond("diamond_inner", emblem, EMBLEM_INNER_MIN, EMBLEM_INNER_MAX, PlayTheme.panel_blue);
        build_diamond("diamond_core", emblem, EMBLEM_CORE_MIN, EMBLEM_CORE_MAX, PlayTheme.gold_soft);
    }

    static void build_diamond(string name, RectTransform parent, float min, float max, Color color)
    {
        var diamond = PlayUi.image(name, parent, null);
        diamond.color = color;
        LobbyUi.place_fraction(diamond.rectTransform, min, min, max, max);
        diamond.rectTransform.localRotation = Quaternion.Euler(0f, 0f, DIAMOND_ROTATION);
    }

    static void build_crest(RectTransform cell)
    {
        var arrow = PlayOrnament.arrow_up(cell, "crest_arrow", CREST_ARROW_SIZE, PlayTheme.gold);
        arrow.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        arrow.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

        var reach = CREST_GAP + CREST_ARROW_SIZE * 0.5f;
        var half = CREST_THICKNESS * 0.5f;

        var left = PlayOrnament.line(cell, "line_left", 10f, CREST_THICKNESS, PlayTheme.gold, true, false);
        left.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        left.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        left.rectTransform.offsetMin = new Vector2(0f, -half);
        left.rectTransform.offsetMax = new Vector2(-reach, half);

        var right = PlayOrnament.line(cell, "line_right", 10f, CREST_THICKNESS, PlayTheme.gold, false, true);
        right.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        right.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        right.rectTransform.offsetMin = new Vector2(reach, -half);
        right.rectTransform.offsetMax = new Vector2(0f, half);
    }

    static void build_card(RectTransform cell, LobbyPlayer player)
    {
        var empty = player == null;
        var is_you = !empty && player.is_you;
        var is_host = !empty && player.is_host;

        var frame = PlayUi.rect("card", cell);
        LobbyUi.place_fraction(frame, CELL_PAD_X, CELL_BOTTOM, 1f - CELL_PAD_X, CELL_TOP);
        fit_aspect(frame, CARD_ASPECT, AspectRatioFitter.AspectMode.FitInParent);

        if (is_you)
        {
            var glow = PlayUi.image("you_glow", frame, PlayUi.radial_glow());
            glow.color = PlayTheme.glow;
            LobbyUi.place_fraction(glow.rectTransform, GLOW_MIN, GLOW_MIN, GLOW_MAX, GLOW_MAX);
        }

        var card = LobbyUi.panel("card_frame", frame, LobbyUi.FRAME_PLAYER_CARD_PATH, CARD_BORDER_SCALE);
        card.raycastTarget = true;

        if (empty) build_empty_avatar(frame);
        else build_avatar(frame, player);

        if (is_host) build_host_tag(frame);
        if (is_you) build_you_tag(frame, is_host);

        var name_value = empty ? INVITE_LABEL : LobbyUi.sanitize(player.name);
        var name_color = empty ? PlayTheme.text_muted : PlayTheme.text;
        var name_cell = PlayUi.rect("name", frame);
        LobbyUi.place_fraction(name_cell, 0.08f, 0.26f, 0.92f, 0.38f);
        LobbyPlayerNameFit.attach(name_cell, name_value, name_color, NAME_MIN_SIZE, NAME_MAX_SIZE, NAME_HEIGHT_RATIO);

        var ready = !empty && player.ready;
        build_status(frame, ready);
    }

    static void build_avatar(RectTransform frame, LobbyPlayer player)
    {
        var holder = PlayUi.rect("avatar", frame);
        LobbyUi.place_fraction(holder, 0.12f, 0.42f, 0.88f, 0.80f);
        fit_aspect(holder, AVATAR_ASPECT, AspectRatioFitter.AspectMode.FitInParent);

        var mask_area = PlayUi.image("avatar_mask", holder, PlayUi.pill());
        mask_area.type = Image.Type.Sliced;
        mask_area.color = PlayTheme.panel_dark;
        LobbyUi.place_fraction(mask_area.rectTransform, MASK_MIN, MASK_MIN, MASK_MAX, MASK_MAX);
        var mask = mask_area.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        var portrait = PlayUi.sprite(player.avatar_path);
        if (portrait != null)
        {
            var picture = PlayUi.image("portrait", mask_area.rectTransform, portrait);
            picture.color = PlayTheme.white;
            PlayUi.stretch(picture.rectTransform);
            var rect = portrait.rect;
            fit_aspect(picture.rectTransform, rect.width / Mathf.Max(1f, rect.height), AspectRatioFitter.AspectMode.EnvelopeParent);
        }
        else
        {
            var circle = PlayUi.image("placeholder", mask_area.rectTransform, PlayUi.pill());
            circle.type = Image.Type.Sliced;
            circle.color = PlayTheme.panel_blue;
            PlayUi.stretch(circle.rectTransform);
            fit_aspect(circle.rectTransform, 1f, AspectRatioFitter.AspectMode.FitInParent);
        }

        var oval = PlayUi.image("avatar_oval", holder, PlayUi.sprite(LobbyUi.FRAME_AVATAR_OVAL_PATH));
        oval.enabled = oval.sprite != null;
        oval.color = PlayTheme.white;
        PlayUi.stretch(oval.rectTransform);
    }

    static void build_empty_avatar(RectTransform frame)
    {
        var holder = PlayUi.rect("avatar_empty", frame);
        LobbyUi.place_fraction(holder, 0.20f, 0.42f, 0.80f, 0.80f);
        fit_aspect(holder, 1f, AspectRatioFitter.AspectMode.FitInParent);

        var ring = PlayUi.image("plus_ring", holder, PlayUi.pill());
        ring.type = Image.Type.Sliced;
        ring.color = PlayTheme.with_alpha(PlayTheme.text_muted, RING_ALPHA);
        PlayUi.stretch(ring.rectTransform);

        var inner = PlayUi.image("plus_fill", holder, PlayUi.pill());
        inner.type = Image.Type.Sliced;
        inner.color = PlayTheme.panel_blue;
        LobbyUi.place_fraction(inner.rectTransform, RING_INSET, RING_INSET, 1f - RING_INSET, 1f - RING_INSET);

        var horizontal = PlayUi.image("plus_horizontal", holder, null);
        horizontal.color = PlayTheme.text_muted;
        LobbyUi.place_fraction(horizontal.rectTransform, PLUS_MIN, 0.5f - PLUS_HALF_THICKNESS, PLUS_MAX, 0.5f + PLUS_HALF_THICKNESS);

        var vertical = PlayUi.image("plus_vertical", holder, null);
        vertical.color = PlayTheme.text_muted;
        LobbyUi.place_fraction(vertical.rectTransform, 0.5f - PLUS_HALF_THICKNESS, PLUS_MIN, 0.5f + PLUS_HALF_THICKNESS, PLUS_MAX);
    }

    static void build_host_tag(RectTransform frame)
    {
        var crown = PlayUi.image("host_crown", frame, PlayUi.sprite(LobbyUi.ICON_CROWN_PATH));
        crown.preserveAspect = true;
        crown.color = crown.sprite != null ? PlayTheme.gold : PlayTheme.clear;
        LobbyUi.place_fraction(crown.rectTransform, 0.34f, 0.91f, 0.66f, 1.05f);

        var label = PlayUi.text("host_label", frame, HOST_LABEL, LABEL_MAX_SIZE, PlayTheme.gold, TextAnchor.MiddleCenter);
        fit_text(label, LABEL_MIN_SIZE, LABEL_MAX_SIZE);
        LobbyUi.place_fraction(label.rectTransform, 0.2f, 0.81f, 0.8f, 0.90f);
    }

    static void build_you_tag(RectTransform frame, bool bottom)
    {
        var tag = PlayUi.image("you_tag", frame, PlayUi.pill());
        tag.type = Image.Type.Sliced;
        tag.color = PlayTheme.gold;
        var y_min = bottom ? -0.045f : 0.955f;
        LobbyUi.place_fraction(tag.rectTransform, 0.30f, y_min, 0.70f, y_min + 0.09f);

        var label = PlayUi.text("you_label", tag.rectTransform, YOU_LABEL, TAG_MAX_SIZE, PlayTheme.label_brown, TextAnchor.MiddleCenter);
        fit_text(label, LABEL_MIN_SIZE, TAG_MAX_SIZE);
        LobbyUi.place_fraction(label.rectTransform, 0.1f, 0.1f, 0.9f, 0.9f);
    }

    static void build_status(RectTransform frame, bool ready)
    {
        var row = PlayUi.rect("status", frame);
        LobbyUi.place_fraction(row, 0.05f, 0.12f, 0.95f, 0.24f);

        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = STATUS_SPACING;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        var dot = PlayUi.image("status_dot", row, PlayUi.pill());
        dot.type = Image.Type.Sliced;
        dot.color = ready ? PlayTheme.success : PlayTheme.text_muted;
        var dot_layout = dot.gameObject.AddComponent<LayoutElement>();
        dot_layout.preferredWidth = DOT_SIZE;
        dot_layout.preferredHeight = DOT_SIZE;

        var label = PlayUi.text("status_text", row, ready ? READY_LABEL : WAITING_LABEL, STATUS_SIZE, ready ? PlayTheme.success : PlayTheme.text_muted, TextAnchor.MiddleLeft);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
    }

    static void fit_text(Text label, int min_size, int max_size)
    {
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = min_size;
        label.resizeTextMaxSize = max_size;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
    }

    static void fit_aspect(RectTransform target, float ratio, AspectRatioFitter.AspectMode mode)
    {
        var fitter = target.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = mode;
        fitter.aspectRatio = ratio;
    }
}

public class LobbyPlayerNameFit : MonoBehaviour
{
    const float WIDTH_TOLERANCE = 0.5f;

    string value;
    Color color;
    int min_size;
    int max_size;
    float height_ratio;
    float built_width;
    PlayMarquee marquee;

    public static void attach(RectTransform slot, string value, Color color, int min_size, int max_size, float height_ratio)
    {
        var fit = slot.gameObject.AddComponent<LobbyPlayerNameFit>();
        fit.value = value;
        fit.color = color;
        fit.min_size = min_size;
        fit.max_size = max_size;
        fit.height_ratio = height_ratio;
    }

    void Start()
    {
        refresh();
    }

    void OnRectTransformDimensionsChange()
    {
        refresh();
    }

    void refresh()
    {
        if (!isActiveAndEnabled) return;

        var slot = (RectTransform)transform;
        var width = slot.rect.width;
        var height = slot.rect.height;
        if (width <= 1f || height <= 1f) return;
        if (marquee != null && Mathf.Abs(width - built_width) < WIDTH_TOLERANCE) return;

        if (marquee != null) Destroy(marquee.gameObject);

        built_width = width;
        var size = Mathf.Clamp(Mathf.RoundToInt(height * height_ratio), min_size, max_size);
        marquee = PlayMarquee.create(slot, "name_text", size, color, width, height);
        PlayUi.stretch(marquee.rect);
        marquee.set_text(value);
    }
}
