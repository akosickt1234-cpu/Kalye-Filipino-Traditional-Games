using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class LobbyPlayerSlots
{
    const string TITLE = "GATHER YOUR TEAM";
    const bool SHOW_SUBTEXT = true;
    const string SUBTEXT = "Play together, Make new memories";
    const string INVITE_LABEL = "Invite Friend";
    const string READY_LABEL = "Ready";
    const string WAITING_LABEL = "Waiting...";
    const float CARD_ASPECT = 485f / 768f;
    const float CARD_BORDER_SCALE = 1.8f;
    const float CELL_PAD_X = 0.04f;
    const float CELL_TOP = 0.93f;
    const float CELL_BOTTOM = 0.07f;
    const float EMBLEM_RIGHT = 0.09f;
    const float TITLE_LEFT = 0.10f;
    const float TITLE_RIGHT = 0.72f;
    const float TITLE_SPLIT = 0.38f;
    const float SUBTEXT_RIGHT = 0.57f;
    const float CREST_LEFT = 0.58f;
    const float CREST_RIGHT = 0.78f;
    const float SUBTEXT_CREST_BOTTOM = 0.25f;
    const float SUBTEXT_CREST_TOP = 0.75f;
    const float DIVIDER_THICKNESS = 2f;
    const float DIVIDER_ALPHA = 0.6f;
    const float CREST_BOTTOM = 0.32f;
    const float CREST_TOP = 0.68f;
    const float RING_ALPHA = 0.9f;
    const float YOU_GLOW_ALPHA = 0.5f;
    const float PLUS_MIN = 0.30f;
    const float PLUS_MAX = 0.70f;
    const float PLUS_HALF_THICKNESS = 0.025f;
    const float DOT_SIZE = 14f;
    const float STATUS_SPACING = 8f;
    const int TITLE_MAX_SIZE = 56;
    const int TITLE_MIN_SIZE = 18;
    const int LABEL_MAX_SIZE = 22;
    const int SUBTEXT_MAX_SIZE = 26;
    const int SUBTEXT_MIN_SIZE = 10;
    const int STATUS_SIZE = 22;
    const int NAME_MAX_SIZE = 30;
    const int NAME_MIN_SIZE = 12;
    const float NAME_HEIGHT_RATIO = 0.7f;
    const float AVATAR_LEFT = 0.12f;
    const float AVATAR_RIGHT = 0.88f;
    const float AVATAR_BOTTOM = 0.38f;
    const float AVATAR_TOP = 0.76f;
    const float AVATAR_FILL_INSET = 0.07f;
    const float CROWN_LEFT = 0.34f;
    const float CROWN_RIGHT = 0.66f;
    const float CROWN_BOTTOM = 0.78f;
    const float CROWN_TOP = 0.90f;
    const float RHOMBUS_LEFT = 0.28f;
    const float RHOMBUS_RIGHT = 0.72f;
    const float RHOMBUS_BOTTOM = 0.77f;
    const float RHOMBUS_TOP = 0.935f;
    const float CARD_GLOW_ALPHA = 0.38f;
    const float EMPTY_CARD_ALPHA = 0.55f;
    const float SPIN_SPEED = 0.9f;
    const float SPIN_PHASE_STEP = 1.1f;
    const float SPIN_FRAME_INTENSITY = 0.6f;
    const float SPIN_GLOW_INTENSITY = 0.3f;
    static readonly Color GLOW_GOLD_COLOR = new Color(1f, 0.72f, 0.18f, 1f);
    static readonly Color SPIN_LIGHT_GOLD = new Color(1f, 0.74f, 0.2f, 1f);
    const float FRAME_ALPHA_FAINT = 0.9f;
    const float AVATAR_ICON_INSET = 0.22f;
    const float HOURGLASS_SIZE = 20f;
    const float NAME_BOTTOM = 0.24f;
    const float NAME_TOP = 0.36f;
    const float STATUS_BOTTOM = 0.10f;
    const float STATUS_TOP = 0.22f;

    static RectTransform row_root;

    public static void build(RectTransform heading_slot, RectTransform slots_row)
    {
        if (heading_slot != null) build_heading(heading_slot);
        if (slots_row == null) return;

        build_top_divider(slots_row);

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
            build_card(cell, player != null && !player.is_empty ? player : null, i);
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

    static void build_top_divider(RectTransform row)
    {
        var parent = row.parent as RectTransform;
        if (parent == null) return;

        var color = PlayTheme.with_alpha(PlayTheme.border_blue, DIVIDER_ALPHA);
        var line = PlayOrnament.line(parent, "cards_top_line", 100f, DIVIDER_THICKNESS, color, true, true);
        PlayUi.place(line.rectTransform, new Vector2(row.anchorMin.x, row.anchorMax.y), new Vector2(row.anchorMax.x, row.anchorMax.y), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, DIVIDER_THICKNESS));
    }

    static void build_heading(RectTransform heading_slot)
    {
        var emblem_cell = PlayUi.rect("emblem_cell", heading_slot);
        LobbyUi.place_fraction(emblem_cell, 0f, 0f, EMBLEM_RIGHT, 1f);
        build_emblem(emblem_cell);

        var title_cell = PlayUi.rect("title_cell", heading_slot);
        LobbyUi.place_fraction(title_cell, TITLE_LEFT, SHOW_SUBTEXT ? TITLE_SPLIT : 0f, TITLE_RIGHT, 1f);
        var title = PlayUi.text("title", title_cell, TITLE, TITLE_MAX_SIZE, PlayTheme.text_white, TextAnchor.MiddleLeft);
        fit_text(title, TITLE_MIN_SIZE, TITLE_MAX_SIZE);
        PlayUi.stretch(title.rectTransform);
        PlayUi.add_shadow(title);

        if (SHOW_SUBTEXT)
        {
            var subtext_cell = PlayUi.rect("subtext_cell", heading_slot);
            LobbyUi.place_fraction(subtext_cell, TITLE_LEFT, 0f, SUBTEXT_RIGHT, TITLE_SPLIT);
            var subtext = PlayUi.text("subtext", subtext_cell, SUBTEXT, SUBTEXT_MAX_SIZE, PlayTheme.light_blue, TextAnchor.MiddleLeft);
            fit_text(subtext, SUBTEXT_MIN_SIZE, SUBTEXT_MAX_SIZE);
            PlayUi.stretch(subtext.rectTransform);
        }

        var crest_cell = PlayUi.rect("crest_cell", heading_slot);
        LobbyUi.place_fraction(crest_cell, CREST_LEFT, 0f, CREST_RIGHT, SHOW_SUBTEXT ? TITLE_SPLIT : 1f);
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
        LobbyUi.place_fraction(tail, 0f, SHOW_SUBTEXT ? SUBTEXT_CREST_BOTTOM : CREST_BOTTOM, 1f, SHOW_SUBTEXT ? SUBTEXT_CREST_TOP : CREST_TOP);
    }

    static void build_card(RectTransform cell, LobbyPlayer player, int card_index)
    {
        var empty = player == null;
        var is_you = !empty && player.is_you;
        var is_host = !empty && player.is_host;

        var frame = PlayUi.rect("card", cell);
        LobbyUi.place_fraction(frame, CELL_PAD_X, CELL_BOTTOM, 1f - CELL_PAD_X, CELL_TOP);
        fit_aspect(frame, CARD_ASPECT, AspectRatioFitter.AspectMode.FitInParent);

        var light = empty ? LobbyFrameLight.Style.BlueCard : LobbyFrameLight.Style.GoldCard;
        var card_glow_alpha = is_you ? YOU_GLOW_ALPHA : CARD_GLOW_ALPHA;
        Image glow = null;
        if (!empty) glow = LobbyUi.glow_panel("card_glow", frame, LobbyUi.FRAME_PLAYER_CARD_PATH, CARD_BORDER_SCALE, GLOW_GOLD_COLOR * new Color(1f, 1f, 1f, card_glow_alpha));
        var card = LobbyUi.panel("card_frame", frame, LobbyUi.FRAME_PLAYER_CARD_PATH, CARD_BORDER_SCALE, light, true);
        card.raycastTarget = true;
        card.color = PlayTheme.with_alpha(PlayTheme.white, empty ? FRAME_ALPHA_FAINT : 1f);

        if (empty) frame.gameObject.AddComponent<CanvasGroup>().alpha = EMPTY_CARD_ALPHA;
        else
        {
            var beam_phase = card_index * SPIN_PHASE_STEP;
            LobbyFrameSpin.attach(glow, SPIN_LIGHT_GOLD, SPIN_SPEED, beam_phase, SPIN_GLOW_INTENSITY, true);
            LobbyFrameSpin.attach(card, SPIN_LIGHT_GOLD, SPIN_SPEED, beam_phase, SPIN_FRAME_INTENSITY, false);
        }

        if (empty) build_empty_avatar(frame);
        else build_avatar(frame, player);

        if (is_host) build_top_icon(frame, "crown", LobbyUi.ICON_CROWN_PATH, PlayTheme.gold, CROWN_LEFT, CROWN_BOTTOM, CROWN_RIGHT, CROWN_TOP);
        else build_top_icon(frame, "rhombus", LobbyUi.ICON_RHOMBUS_PATH, empty ? PlayTheme.light_blue : PlayTheme.gold_soft, RHOMBUS_LEFT, RHOMBUS_BOTTOM, RHOMBUS_RIGHT, RHOMBUS_TOP);

        var you = LobbyState.you;
        var name_value = empty ? INVITE_LABEL : LobbyUi.sanitize(is_you && you != null ? you.name : player.name);
        var name_color = empty ? PlayTheme.text_white : is_you ? PlayTheme.gold : PlayTheme.text_white;
        var name_cell = PlayUi.rect("name", frame);
        LobbyUi.place_fraction(name_cell, 0.08f, NAME_BOTTOM, 0.92f, NAME_TOP);
        LobbyPlayerNameFit.attach(name_cell, name_value, name_color, NAME_MIN_SIZE, NAME_MAX_SIZE, NAME_HEIGHT_RATIO);

        var ready = !empty && player.ready;
        build_status(frame, ready, empty);
    }

    static RectTransform build_avatar_holder(RectTransform frame, string name)
    {
        var area = PlayUi.rect(name + "_area", frame);
        LobbyUi.place_fraction(area, AVATAR_LEFT, AVATAR_BOTTOM, AVATAR_RIGHT, AVATAR_TOP);

        var holder = PlayUi.rect(name, area);
        fit_aspect(holder, 1f, AspectRatioFitter.AspectMode.FitInParent);
        return holder;
    }

    static Image build_circle_frame(RectTransform holder, Color color, LobbyFrameLight.Style light)
    {
        var ring = PlayUi.image("avatar_ring", holder, LobbyFrameLight.sprite(LobbyUi.FRAME_AVATAR_CIRCLE_PATH, light));
        ring.enabled = ring.sprite != null;
        ring.preserveAspect = true;
        ring.color = color;
        PlayUi.stretch(ring.rectTransform);
        return ring;
    }

    static void build_top_icon(RectTransform frame, string name, string path, Color color, float left, float bottom, float right, float top)
    {
        var icon = PlayUi.image(name, frame, PlayUi.sprite(path));
        icon.enabled = icon.sprite != null;
        icon.preserveAspect = true;
        icon.color = color;
        LobbyUi.place_fraction(icon.rectTransform, left, bottom, right, top);
        PlayUi.add_shadow(icon, 2f);
    }

    static void build_avatar(RectTransform frame, LobbyPlayer player)
    {
        var holder = build_avatar_holder(frame, "avatar");
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
        else
        {
            var fallback = PlayUi.image("avatar_default", fill.rectTransform, PlayUi.sprite(LobbyUi.ICON_AVATAR_DEFAULT_PATH));
            fallback.enabled = fallback.sprite != null;
            fallback.preserveAspect = true;
            fallback.color = PlayTheme.icon;
            LobbyUi.place_fraction(fallback.rectTransform, AVATAR_ICON_INSET, AVATAR_ICON_INSET, 1f - AVATAR_ICON_INSET, 1f - AVATAR_ICON_INSET);
        }

        build_circle_frame(holder, PlayTheme.white, LobbyFrameLight.Style.GoldTopBottom);
    }

    static void build_empty_avatar(RectTransform frame)
    {
        var holder = build_avatar_holder(frame, "avatar_empty");

        var fill = PlayUi.circle("plus_fill", holder, PlayTheme.panel_blue);
        fill.preserveAspect = false;
        LobbyUi.place_fraction(fill.rectTransform, AVATAR_FILL_INSET, AVATAR_FILL_INSET, 1f - AVATAR_FILL_INSET, 1f - AVATAR_FILL_INSET);

        build_circle_frame(holder, PlayTheme.with_alpha(PlayTheme.white, RING_ALPHA), LobbyFrameLight.Style.BlueTopBottom);

        var horizontal = PlayUi.image("plus_horizontal", holder, null);
        horizontal.color = PlayTheme.light_blue;
        LobbyUi.place_fraction(horizontal.rectTransform, PLUS_MIN, 0.5f - PLUS_HALF_THICKNESS, PLUS_MAX, 0.5f + PLUS_HALF_THICKNESS);

        var vertical = PlayUi.image("plus_vertical", holder, null);
        vertical.color = PlayTheme.light_blue;
        LobbyUi.place_fraction(vertical.rectTransform, 0.5f - PLUS_HALF_THICKNESS, PLUS_MIN, 0.5f + PLUS_HALF_THICKNESS, PLUS_MAX);

    }

    static void build_status(RectTransform frame, bool ready, bool empty)
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

        if (ready)
        {
            var dot = PlayUi.image("status_dot", row, PlayUi.pill());
            dot.type = Image.Type.Sliced;
            dot.color = PlayTheme.success;
            var dot_layout = dot.gameObject.AddComponent<LayoutElement>();
            dot_layout.preferredWidth = DOT_SIZE;
            dot_layout.preferredHeight = DOT_SIZE;
        }
        else
        {
            var hourglass = PlayUi.image("status_hourglass", row, PlayUi.sprite(LobbyUi.ICON_HOURGLASS_PATH));
            hourglass.enabled = hourglass.sprite != null;
            hourglass.preserveAspect = true;
            hourglass.color = empty ? PlayTheme.light_blue : PlayTheme.text_muted;
            var hourglass_layout = hourglass.gameObject.AddComponent<LayoutElement>();
            hourglass_layout.preferredWidth = HOURGLASS_SIZE;
            hourglass_layout.preferredHeight = HOURGLASS_SIZE;
        }

        var label = PlayUi.text("status_text", row, ready ? READY_LABEL : WAITING_LABEL, STATUS_SIZE, ready ? PlayTheme.success : empty ? PlayTheme.light_blue : PlayTheme.text_muted, TextAnchor.MiddleLeft);
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
