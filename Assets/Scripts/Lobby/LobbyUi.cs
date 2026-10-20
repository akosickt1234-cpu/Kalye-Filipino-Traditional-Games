using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class LobbyUi
{
    public const string PANEL_LOBBY_PATH = "Frames/Lobby/PanelLobby";
    public const string FRAME_PLAYER_CARD_PATH = "Frames/Lobby/FramePlayerCard";
    public const string BACK_FRAME_PATH = "Frames/Lobby/BackFrame";
    public const string ICON_CROWN_PATH = "Icons/Lobby/IconCrown";
    public const string ICON_MINIMIZE_PATH = "Icons/Lobby/IconMinimize";
    public const string ICON_KEY_PATH = "Icons/Lobby/IconKey";
    public const string ICON_PRIVATE_LOBBY_PATH = "Icons/Lobby/IconPrivateLobby";
    public const string ICON_CHAT_PATH = "Icons/Lobby/IconChat";
    public const string ICON_REFRESH_PATH = "Icons/Lobby/IconRefresh";
    public const string ICON_COPY_PATH = "Icons/Lobby/IconCopy";
    public const string ICON_ARROW_RIGHT_PATH = "Icons/Lobby/IconArrowRight";
    public const string ICON_SUN_PATH = "Icons/Lobby/IconSun";
    public const string ICON_RHOMBUS_PATH = "Icons/Lobby/IconRhombus";
    public const string ICON_SWORDS_PATH = "Icons/Lobby/IconSwords";
    public const string ICON_HOURGLASS_PATH = "Icons/Lobby/IconHourglass";
    public const string ICON_USER_PLUS_PATH = "Icons/Lobby/IconUserPlus";
    public const string ICON_AVATAR_DEFAULT_PATH = "Icons/Lobby/IconAvatarDefault";
    public const string PANEL_LOBBY_BLUE_PATH = "Frames/Lobby/PanelLobbyBlue";
    public const string FRAME_PLAYER_CARD_BLUE_PATH = "Frames/Lobby/FramePlayerCardBlue";
    public const string FRAME_PLAYER_CARD_HOST_PATH = "Frames/Lobby/FramePlayerCardHost";
    public const string FRAME_PILL_PATH = "Frames/Lobby/FramePill";
    public const string CORNER_BRACKET_PATH = "Frames/Lobby/CornerBracket";
    public const string DIVIDER_FADE_PATH = "Frames/Lobby/DividerFade";
    public const string FRAME_AVATAR_CIRCLE_PATH = "Frames/Play/FrameAvatarRing";
    public const string BACKDROP_PATH = "Background/Lobby/LobbyBackdrop";
    public const float PANEL_BORDER_SCALE = 1.8f;
    public const float RIM_GLOW_FEATHER = 24f;
    public const float SCROLL_SENSITIVITY = 40f;
    public const float FADE_IN_SECONDS = 0.15f;
    public const float FADE_OUT_SECONDS = 0.1f;
    public const int FOCUS_DELAY_FRAMES = 2;
    public const int CARET_WIDTH = 2;
    public const string PLAY_SCENE = "Play";
    const float BLOCKER_ALPHA = 0.6f;
    const int RIM_GLOW_SIZE = 64;
    const float RIM_GLOW_RADIUS = 8f;
    const int GRADIENT_STEPS = 128;
    const int EDGE_GLOW_SIZE = 64;
    const float EDGE_GLOW_FEATHER = 24f;
    const float REFERENCE_PPU = 100f;
    const float PANEL_GRADIENT_ALPHA = 0.92f;
    const float PANEL_SHADOW_ALPHA = 0.6f;
    const float PANEL_SHADOW_BLUR = 16f;
    const float BRACKET_SIZE = 72f;
    const float BRACKET_INSET = -6f;
    static readonly Dictionary<Color[], Sprite> vertical_cache = new Dictionary<Color[], Sprite>();
    static readonly Dictionary<Color[], Sprite> horizontal_cache = new Dictionary<Color[], Sprite>();
    static Sprite rim_glow_sprite;
    static Sprite edge_glow_sprite;

    public static RectTransform modal_blocker(Transform parent)
    {
        var blocker = PlayUi.image("modal_blocker", parent, null, true);
        blocker.color = PlayTheme.with_alpha(PlayTheme.overlay_dark, BLOCKER_ALPHA);
        PlayUi.stretch(blocker.rectTransform);
        return blocker.rectTransform;
    }

    public static Image panel(string name, Transform parent, string sprite_path, float border_scale = 1f)
    {
        var art = PlayUi.sprite(sprite_path);
        var picture = PlayUi.image(name, parent, art != null ? art : PlayUi.pill(), false);
        picture.type = Image.Type.Sliced;
        picture.color = art != null ? PlayTheme.white : PlayTheme.fill_soft;
        picture.pixelsPerUnitMultiplier = Mathf.Max(0.01f, border_scale);
        PlayUi.stretch(picture.rectTransform);
        return picture;
    }

    public static void place_fraction(RectTransform r, float x_min, float y_min, float x_max, float y_max)
    {
        if (r == null) return;

        r.anchorMin = new Vector2(x_min, y_min);
        r.anchorMax = new Vector2(x_max, y_max);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    public static void go_to_scene(string scene_name)
    {
        PlayUi.go_to_scene(scene_name);
    }

    public static void no_transition(Selectable selectable)
    {
        selectable.transition = Selectable.Transition.None;
    }

    public static Image rim_glow(string name, Transform parent, Color color)
    {
        var picture = PlayUi.image(name, parent, rim_glow_art());
        picture.type = Image.Type.Sliced;
        picture.fillCenter = false;
        picture.color = color;
        PlayUi.stretch(picture.rectTransform);
        picture.rectTransform.offsetMin = new Vector2(-RIM_GLOW_FEATHER, -RIM_GLOW_FEATHER);
        picture.rectTransform.offsetMax = new Vector2(RIM_GLOW_FEATHER, RIM_GLOW_FEATHER);
        return picture;
    }

    static Sprite rim_glow_art()
    {
        if (rim_glow_sprite != null) return rim_glow_sprite;

        var texture = new Texture2D(RIM_GLOW_SIZE, RIM_GLOW_SIZE, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color[RIM_GLOW_SIZE * RIM_GLOW_SIZE];
        var center = (RIM_GLOW_SIZE - 1) * 0.5f;
        var inner = center - RIM_GLOW_FEATHER - RIM_GLOW_RADIUS;
        for (var y = 0; y < RIM_GLOW_SIZE; y++)
        {
            for (var x = 0; x < RIM_GLOW_SIZE; x++)
            {
                var dx = Mathf.Max(Mathf.Abs(x - center) - inner, 0f);
                var dy = Mathf.Max(Mathf.Abs(y - center) - inner, 0f);
                var distance = Mathf.Max(Mathf.Sqrt(dx * dx + dy * dy) - RIM_GLOW_RADIUS, 0f);
                var alpha = Mathf.Pow(Mathf.Clamp01(1f - distance / RIM_GLOW_FEATHER), 2f);
                pixels[y * RIM_GLOW_SIZE + x] = PlayTheme.with_alpha(PlayTheme.white, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        var border = new Vector4(RIM_GLOW_FEATHER, RIM_GLOW_FEATHER, RIM_GLOW_FEATHER, RIM_GLOW_FEATHER);
        rim_glow_sprite = Sprite.Create(texture, new Rect(0f, 0f, RIM_GLOW_SIZE, RIM_GLOW_SIZE), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        return rim_glow_sprite;
    }

    public static Color sample_stops(Color[] stops, float t)
    {
        if (stops == null || stops.Length == 0) return PlayTheme.white;
        if (stops.Length == 1) return stops[0];

        var position = Mathf.Clamp01(t) * (stops.Length - 1);
        var index = Mathf.Min(Mathf.FloorToInt(position), stops.Length - 2);
        return Color.Lerp(stops[index], stops[index + 1], position - index);
    }

    public static Sprite gradient_sprite(Color[] stops, bool vertical)
    {
        if (stops == null || stops.Length == 0) return null;

        var cache = vertical ? vertical_cache : horizontal_cache;
        Sprite cached;
        if (cache.TryGetValue(stops, out cached) && cached != null) return cached;

        var width = vertical ? 1 : GRADIENT_STEPS;
        var height = vertical ? GRADIENT_STEPS : 1;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color[GRADIENT_STEPS];
        for (var i = 0; i < GRADIENT_STEPS; i++)
        {
            var t = i / (float)(GRADIENT_STEPS - 1);
            pixels[i] = sample_stops(stops, vertical ? 1f - t : t);
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), REFERENCE_PPU);
        cache[stops] = sprite;
        return sprite;
    }

    public static Image gradient_image(string name, Transform parent, Color[] stops, bool vertical)
    {
        var picture = PlayUi.image(name, parent, gradient_sprite(stops, vertical));
        picture.color = PlayTheme.white;
        PlayUi.stretch(picture.rectTransform);
        return picture;
    }

    public static Image glow(string name, Transform parent, Color color, float alpha, float blur)
    {
        var picture = rim_glow(name, parent, PlayTheme.with_alpha(color, alpha));
        var feather = Mathf.Max(1f, blur);
        picture.pixelsPerUnitMultiplier = RIM_GLOW_FEATHER / feather;
        picture.rectTransform.offsetMin = new Vector2(-feather, -feather);
        picture.rectTransform.offsetMax = new Vector2(feather, feather);
        return picture;
    }

    public static Image inner_glow(string name, Transform parent, Color color, float inset)
    {
        var picture = PlayUi.image(name, parent, edge_glow_art());
        picture.type = Image.Type.Sliced;
        picture.fillCenter = false;
        picture.color = color;
        inset_rect(picture.rectTransform, inset);
        return picture;
    }

    public static RectTransform corner_brackets(Transform parent, Color[] trim_stops)
    {
        var holder = PlayUi.rect("corner_brackets", parent);
        PlayUi.stretch(holder);

        var art = PlayUi.sprite(CORNER_BRACKET_PATH);
        var tint = sample_stops(trim_stops, 0.5f);
        corner_bracket(holder, "bracket_top_left", art, tint, 0f, 1f, 1f, 1f);
        corner_bracket(holder, "bracket_top_right", art, tint, 1f, 1f, -1f, 1f);
        corner_bracket(holder, "bracket_bottom_left", art, tint, 0f, 0f, 1f, -1f);
        corner_bracket(holder, "bracket_bottom_right", art, tint, 1f, 0f, -1f, -1f);
        return holder;
    }

    public static void decorate_panel(Image panel, Color[] stops, Color inner_glow_color)
    {
        if (panel == null || panel.sprite == null) return;

        var inset = panel_inset(panel);
        var shadow = glow("panel_edge_shadow", panel.transform, PlayTheme.lobby_edge_shadow, PANEL_SHADOW_ALPHA, PANEL_SHADOW_BLUR);
        shadow.raycastTarget = false;

        var fill = gradient_image("panel_gradient", panel.transform, stops, true);
        fill.color = PlayTheme.with_alpha(PlayTheme.white, PANEL_GRADIENT_ALPHA);
        inset_rect(fill.rectTransform, inset);

        inner_glow("panel_inner_glow", panel.transform, inner_glow_color, inset);
        corner_brackets(panel.transform, PlayTheme.trim_primary_stops);
    }

    static float panel_inset(Image panel)
    {
        var sprite = panel.sprite;
        var scale = Mathf.Max(0.01f, sprite.pixelsPerUnit / REFERENCE_PPU * panel.pixelsPerUnitMultiplier);
        return sprite.border.x / scale;
    }

    static void inset_rect(RectTransform r, float inset)
    {
        PlayUi.stretch(r);
        r.offsetMin = new Vector2(inset, inset);
        r.offsetMax = new Vector2(-inset, -inset);
    }

    static void corner_bracket(RectTransform parent, string name, Sprite art, Color tint, float anchor_x, float anchor_y, float flip_x, float flip_y)
    {
        var bracket = PlayUi.image(name, parent, art);
        bracket.enabled = art != null;
        bracket.preserveAspect = true;
        bracket.color = tint;
        var r = bracket.rectTransform;
        r.anchorMin = new Vector2(anchor_x, anchor_y);
        r.anchorMax = new Vector2(anchor_x, anchor_y);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(BRACKET_SIZE, BRACKET_SIZE);
        var reach = BRACKET_SIZE * 0.5f + BRACKET_INSET;
        r.anchoredPosition = new Vector2(flip_x * reach, -flip_y * reach);
        r.localScale = new Vector3(flip_x, flip_y, 1f);
    }

    static Sprite edge_glow_art()
    {
        if (edge_glow_sprite != null) return edge_glow_sprite;

        var texture = new Texture2D(EDGE_GLOW_SIZE, EDGE_GLOW_SIZE, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color[EDGE_GLOW_SIZE * EDGE_GLOW_SIZE];
        for (var y = 0; y < EDGE_GLOW_SIZE; y++)
        {
            for (var x = 0; x < EDGE_GLOW_SIZE; x++)
            {
                var edge = Mathf.Min(Mathf.Min(x, EDGE_GLOW_SIZE - 1 - x), Mathf.Min(y, EDGE_GLOW_SIZE - 1 - y));
                var alpha = Mathf.Pow(Mathf.Clamp01(1f - edge / EDGE_GLOW_FEATHER), 2f);
                pixels[y * EDGE_GLOW_SIZE + x] = PlayTheme.with_alpha(PlayTheme.white, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        var border = new Vector4(EDGE_GLOW_FEATHER, EDGE_GLOW_FEATHER, EDGE_GLOW_FEATHER, EDGE_GLOW_FEATHER);
        edge_glow_sprite = Sprite.Create(texture, new Rect(0f, 0f, EDGE_GLOW_SIZE, EDGE_GLOW_SIZE), new Vector2(0.5f, 0.5f), REFERENCE_PPU, 0, SpriteMeshType.FullRect, border);
        return edge_glow_sprite;
    }

    public static string sanitize(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var clean = value.Replace("<", string.Empty).Replace(">", string.Empty);
        var start = 0;
        var end = clean.Length - 1;
        while (start <= end && (char.IsControl(clean[start]) || char.IsWhiteSpace(clean[start]))) start++;
        while (end >= start && (char.IsControl(clean[end]) || char.IsWhiteSpace(clean[end]))) end--;
        return clean.Substring(start, end - start + 1);
    }
}
