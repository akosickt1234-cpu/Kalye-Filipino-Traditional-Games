using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class LobbyPlayerSlots
{
    const string TITLE = "GATHER YOUR TEAM";
    const string YOU_LABEL = "YOU";
    const string INVITE_LABEL = "Invite Friend";
    const string READY_LABEL = "Ready";
    const string WAITING_LABEL = "Waiting...";
    const float CARD_ASPECT = 485f / 768f;
    const float CARD_BORDER_SCALE = 1.8f;
    const float CELL_PAD_X = 0.04f;
    const float CELL_TOP = 0.90f;
    const float CELL_BOTTOM = 0.05f;
    const float EMBLEM_RIGHT = 0.09f;
    const float TITLE_LEFT = 0.10f;
    const float TITLE_RIGHT = 0.72f;
    const float CREST_LEFT = 0.73f;
    const float CREST_RIGHT = 0.92f;
    const float CREST_BOTTOM = 0.32f;
    const float CREST_TOP = 0.68f;
    const float RING_ALPHA = 0.55f;
    const float RING_INSET = 0.05f;
    const float PLUS_MIN = 0.30f;
    const float PLUS_MAX = 0.70f;
    const float PLUS_HALF_THICKNESS = 0.025f;
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
    const float AVATAR_LEFT = 0.12f;
    const float AVATAR_RIGHT = 0.88f;
    const float AVATAR_BOTTOM = 0.38f;
    const float AVATAR_TOP = 0.76f;
    const float AVATAR_FILL_INSET = 0.07f;
    const float RHOMBUS_LEFT = 0.38f;
    const float RHOMBUS_RIGHT = 0.62f;
    const float RHOMBUS_BOTTOM = 0.795f;
    const float RHOMBUS_TOP = 0.885f;
    const float RHOMBUS_ALPHA_EMPTY = 0.6f;
    const float FRAME_ALPHA_FAINT = 0.4f;
    const float FRAME_ALPHA_HOST = 0.8f;
    const float RIM_GLOW_YOU_ALPHA = 0.5f;
    const float RIM_GLOW_HOST_ALPHA = 0.25f;
    const float AVATAR_GLOW_MIN = -0.35f;
    const float AVATAR_GLOW_MAX = 1.35f;
    const float AVATAR_GLOW_ALPHA = 0.6f;
    const float CROWN_BOTTOM = 0.91f;
    const float CROWN_TOP = 1.09f;
    const float CROWN_ICON_INSET = 0.12f;
    const float NAME_BOTTOM = 0.24f;
    const float NAME_TOP = 0.36f;
    const float STATUS_BOTTOM = 0.10f;
    const float STATUS_TOP = 0.22f;

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
        var ordered = ordered_players(count);
        for (var i = 0; i < count; i++)
        {
            var player = ordered[i];
            var cell = PlayUi.rect("slot_" + i, row_root);
            LobbyUi.place_fraction(cell, i / (float)count, 0f, (i + 1) / (float)count, 1f);
            build_card(cell, player != null && !player.is_empty ? player : null);
        }
    }

    static List<LobbyPlayer> ordered_players(int count)
    {
        var players = LobbyState.players;
        var result = new List<LobbyPlayer>(count);

        for (var i = 0; i < players.Count && result.Count < count; i++)
        {
            if (players[i] != null && !players[i].is_empty && players[i].is_host) result.Add(players[i]);
        }

        for (var i = 0; i < players.Count && result.Count < count; i++)
        {
            if (players[i] != null && !players[i].is_empty && !players[i].is_host) result.Add(players[i]);
        }

        while (result.Count < count) result.Add(null);
        return result;
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
        LobbyUi.place_fraction(crest_cell, CREST_LEFT, 0f, CREST_RIGHT, 1f);
        build_crest(crest_cell);
    }

    static void build_emblem(RectTransform cell)
    {
        var emblem = PlayUi.rect("emblem", cell);
        PlayUi.stretch(emblem);
        fit_aspect(emblem, 1f, AspectRatioFitter.AspectMode.FitInParent);

        var sun = PlayUi.image("sun", emblem, PlayUi.sprite(LobbyUi.ICON_SUN_PATH));
        sun.enabled = sun.sprite != null;
        sun.preserveAspect = true;
        sun.color = PlayTheme.gold;
        PlayUi.stretch(sun.rectTransform);
        PlayUi.add_shadow(sun, 2f);
    }

    static void build_crest(RectTransform cell)
    {
        var tail = LobbyOrnament.diamond_tail(cell, "crest_tail", PlayTheme.gold, false);
        LobbyUi.place_fraction(tail, 0f, CREST_BOTTOM, 1f, CREST_TOP);
    }

    static void build_card(RectTransform cell, LobbyPlayer player)
    {
        var empty = player == null;
        var is_you = !empty && player.is_you;
        var is_host = !empty && player.is_host;

        var frame = PlayUi.rect("card", cell);
        LobbyUi.place_fraction(frame, CELL_PAD_X, CELL_BOTTOM, 1f - CELL_PAD_X, CELL_TOP);
        fit_aspect(frame, CARD_ASPECT, AspectRatioFitter.AspectMode.FitInParent);

        if (is_you) LobbyUi.rim_glow("you_glow", frame, PlayTheme.with_alpha(PlayTheme.gold, RIM_GLOW_YOU_ALPHA));
        else if (is_host) LobbyUi.rim_glow("host_glow", frame, PlayTheme.with_alpha(PlayTheme.gold, RIM_GLOW_HOST_ALPHA));

        var card = LobbyUi.panel("card_frame", frame, LobbyUi.FRAME_PLAYER_CARD_PATH, CARD_BORDER_SCALE);
        card.raycastTarget = true;
        card.color = PlayTheme.with_alpha(PlayTheme.white, is_you ? 1f : is_host ? FRAME_ALPHA_HOST : FRAME_ALPHA_FAINT);

        if (empty) build_empty_avatar(frame);
        else build_avatar(frame, player);

        build_rhombus(frame, empty ? PlayTheme.with_alpha(PlayTheme.gold_soft, RHOMBUS_ALPHA_EMPTY) : PlayTheme.gold_soft);
        if (is_host) build_host_tag(frame);
        if (is_you) build_you_tag(frame, is_host);

        var name_value = empty ? INVITE_LABEL : LobbyUi.sanitize(player.name);
        var name_color = empty ? PlayTheme.text_muted : PlayTheme.text;
        var name_cell = PlayUi.rect("name", frame);
        LobbyUi.place_fraction(name_cell, 0.08f, NAME_BOTTOM, 0.92f, NAME_TOP);
        LobbyPlayerNameFit.attach(name_cell, name_value, name_color, NAME_MIN_SIZE, NAME_MAX_SIZE, NAME_HEIGHT_RATIO);

        var ready = !empty && player.ready;
        build_status(frame, ready);
    }

    static RectTransform build_avatar_holder(RectTransform frame, string name)
    {
        var area = PlayUi.rect(name + "_area", frame);
        LobbyUi.place_fraction(area, AVATAR_LEFT, AVATAR_BOTTOM, AVATAR_RIGHT, AVATAR_TOP);

        var holder = PlayUi.rect(name, area);
        fit_aspect(holder, 1f, AspectRatioFitter.AspectMode.FitInParent);
        return holder;
    }

    static Image build_circle_frame(RectTransform holder, Color color)
    {
        var ring = PlayUi.image("avatar_ring", holder, PlayUi.sprite(LobbyUi.FRAME_AVATAR_CIRCLE_PATH));
        ring.enabled = ring.sprite != null;
        ring.preserveAspect = true;
        ring.color = color;
        PlayUi.stretch(ring.rectTransform);
        return ring;
    }

    static void build_rhombus(RectTransform frame, Color color)
    {
        var rhombus = PlayUi.image("rhombus", frame, PlayUi.sprite(LobbyUi.ICON_RHOMBUS_PATH));
        rhombus.enabled = rhombus.sprite != null;
        rhombus.preserveAspect = true;
        rhombus.color = color;
        LobbyUi.place_fraction(rhombus.rectTransform, RHOMBUS_LEFT, RHOMBUS_BOTTOM, RHOMBUS_RIGHT, RHOMBUS_TOP);
        PlayUi.add_shadow(rhombus, 2f);
    }

    static void build_avatar(RectTransform frame, LobbyPlayer player)
    {
        var holder = build_avatar_holder(frame, "avatar");
        var emphasized = player.is_you || player.is_host;
        if (emphasized)
        {
            var glow = PlayUi.image("avatar_glow", holder, PlayUi.radial_glow());
            glow.color = PlayTheme.with_alpha(PlayTheme.gold, AVATAR_GLOW_ALPHA);
            LobbyUi.place_fraction(glow.rectTransform, AVATAR_GLOW_MIN, AVATAR_GLOW_MIN, AVATAR_GLOW_MAX, AVATAR_GLOW_MAX);
        }

        var portrait = string.IsNullOrEmpty(player.avatar_path) ? null : Resources.Load<Sprite>(player.avatar_path);

        var fill = PlayUi.circle("avatar_fill", holder, portrait != null ? PlayTheme.panel_dark : PlayTheme.panel_blue);
        fill.preserveAspect = false;
        LobbyUi.place_fraction(fill.rectTransform, AVATAR_FILL_INSET, AVATAR_FILL_INSET, 1f - AVATAR_FILL_INSET, 1f - AVATAR_FILL_INSET);
        fill.gameObject.AddComponent<Mask>().showMaskGraphic = true;

        if (portrait != null)
        {
            var picture = PlayUi.image("portrait", fill.rectTransform, portrait);
            picture.color = PlayTheme.white;
            PlayUi.stretch(picture.rectTransform);
            var rect = portrait.rect;
            fit_aspect(picture.rectTransform, rect.width / Mathf.Max(1f, rect.height), AspectRatioFitter.AspectMode.EnvelopeParent);
        }

        build_circle_frame(holder, emphasized ? PlayTheme.white : PlayTheme.with_alpha(PlayTheme.white, RING_ALPHA));
    }

    static void build_empty_avatar(RectTransform frame)
    {
        var holder = build_avatar_holder(frame, "avatar_empty");

        var fill = PlayUi.circle("plus_fill", holder, PlayTheme.panel_blue);
        fill.preserveAspect = false;
        LobbyUi.place_fraction(fill.rectTransform, AVATAR_FILL_INSET, AVATAR_FILL_INSET, 1f - AVATAR_FILL_INSET, 1f - AVATAR_FILL_INSET);

        build_circle_frame(holder, PlayTheme.with_alpha(PlayTheme.white, RING_ALPHA));

        var horizontal = PlayUi.image("plus_horizontal", holder, null);
        horizontal.color = PlayTheme.text_muted;
        LobbyUi.place_fraction(horizontal.rectTransform, PLUS_MIN, 0.5f - PLUS_HALF_THICKNESS, PLUS_MAX, 0.5f + PLUS_HALF_THICKNESS);

        var vertical = PlayUi.image("plus_vertical", holder, null);
        vertical.color = PlayTheme.text_muted;
        LobbyUi.place_fraction(vertical.rectTransform, 0.5f - PLUS_HALF_THICKNESS, PLUS_MIN, 0.5f + PLUS_HALF_THICKNESS, PLUS_MAX);

    }

    static void build_host_tag(RectTransform frame)
    {
        var badge = PlayUi.rect("host_crown", frame);
        badge.anchorMin = new Vector2(0.5f, CROWN_BOTTOM);
        badge.anchorMax = new Vector2(0.5f, CROWN_TOP);
        badge.pivot = new Vector2(0.5f, 0.5f);
        badge.offsetMin = Vector2.zero;
        badge.offsetMax = Vector2.zero;
        var badge_fit = badge.gameObject.AddComponent<AspectRatioFitter>();
        badge_fit.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        badge_fit.aspectRatio = 1f;

        var fill = PlayUi.circle("crown_fill", badge, PlayTheme.panel_blue);
        PlayUi.stretch(fill.rectTransform);

        var crown = PlayUi.image("crown_icon", badge, PlayUi.sprite(LobbyUi.ICON_CROWN_PATH));
        crown.enabled = crown.sprite != null;
        crown.preserveAspect = true;
        crown.color = PlayTheme.gold;
        LobbyUi.place_fraction(crown.rectTransform, CROWN_ICON_INSET, CROWN_ICON_INSET, 1f - CROWN_ICON_INSET, 1f - CROWN_ICON_INSET);
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
        LobbyUi.place_fraction(row, 0.05f, STATUS_BOTTOM, 0.95f, STATUS_TOP);

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
