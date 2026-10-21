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
    public const string FRAME_AVATAR_CIRCLE_PATH = "Frames/Play/FrameAvatarRing";
    public const string BACKDROP_PATH = "Background/Lobby/LobbyBackdrop";
    public const float PANEL_BORDER_SCALE = 1.8f;
    public const float PILL_BORDER = 1f;
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
    const float PILL_BORDER_DARKEN = 0.4f;
    const float PANEL_LIGHT_INSET = 8f;
    const float PANEL_LIGHT_CORNER_SCALE = 2.2f;
    const float PANEL_LIGHT_ALPHA = 0.07f;
    const float PANEL_LIGHT_SPREAD = 1.1f;
    const int PANEL_LIGHT_WIDTH = 128;
    const int PANEL_LIGHT_ROWS = 64;
    const float PANEL_LIGHT_HEIGHT = 0.55f;
    const float PANEL_SHADE_ALPHA = 0.3f;
    const float PANEL_SHADE_HEIGHT = 0.45f;
    const float CANVAS_REFERENCE_PPU = 100f;
    static Sprite rim_glow_sprite;
    static Sprite panel_light_sprite;
    static readonly Color pill_border_color = Color.Lerp(PlayTheme.border_blue, PlayTheme.pill_blue_dark, PILL_BORDER_DARKEN);

    public static RectTransform modal_blocker(Transform parent)
    {
        var blocker = PlayUi.image("modal_blocker", parent, null, true);
        blocker.color = PlayTheme.with_alpha(PlayTheme.overlay_dark, BLOCKER_ALPHA);
        PlayUi.stretch(blocker.rectTransform);
        return blocker.rectTransform;
    }

    public static Image panel(string name, Transform parent, string sprite_path, float border_scale = 1f, LobbyFrameLight.Style light = LobbyFrameLight.Style.None, bool ring_only = false)
    {
        var art = LobbyFrameLight.sprite(sprite_path, light, ring_only);
        var picture = PlayUi.image(name, parent, art != null ? art : PlayUi.pill(), false);
        picture.type = Image.Type.Sliced;
        picture.color = art != null ? PlayTheme.white : PlayTheme.fill_soft;
        picture.pixelsPerUnitMultiplier = Mathf.Max(0.01f, border_scale);
        PlayUi.stretch(picture.rectTransform);
        return picture;
    }

    public static RectTransform panel_light(Image panel_image)
    {
        var holder = PlayUi.image("panel_light", panel_image.transform, PlayUi.pill());
        holder.type = Image.Type.Sliced;
        holder.pixelsPerUnitMultiplier = PANEL_LIGHT_CORNER_SCALE;
        holder.color = PlayTheme.white;
        holder.gameObject.AddComponent<Mask>().showMaskGraphic = false;
        PlayUi.stretch(holder.rectTransform);
        holder.rectTransform.offsetMin = new Vector2(PANEL_LIGHT_INSET, PANEL_LIGHT_INSET);
        holder.rectTransform.offsetMax = new Vector2(-PANEL_LIGHT_INSET, -PANEL_LIGHT_INSET);
        holder.rectTransform.SetAsFirstSibling();

        var gradient = PlayUi.image("panel_light_gradient", holder.rectTransform, panel_light_art());
        gradient.color = PlayTheme.with_alpha(PlayTheme.light_blue, PANEL_LIGHT_ALPHA);
        place_fraction(gradient.rectTransform, 0f, 1f - PANEL_LIGHT_HEIGHT, 1f, 1f);

        var shade = PlayUi.image("panel_shade_gradient", holder.rectTransform, PlayUi.vertical_fade());
        shade.color = PlayTheme.with_alpha(PlayTheme.overlay_dark, PANEL_SHADE_ALPHA);
        place_fraction(shade.rectTransform, 0f, 0f, 1f, PANEL_SHADE_HEIGHT);
        return holder.rectTransform;
    }

    static Sprite panel_light_art()
    {
        if (panel_light_sprite != null) return panel_light_sprite;

        var texture = new Texture2D(PANEL_LIGHT_WIDTH, PANEL_LIGHT_ROWS, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color[PANEL_LIGHT_WIDTH * PANEL_LIGHT_ROWS];
        for (var y = 0; y < PANEL_LIGHT_ROWS; y++)
        {
            var down = 1f - y / (float)(PANEL_LIGHT_ROWS - 1);
            for (var x = 0; x < PANEL_LIGHT_WIDTH; x++)
            {
                var across = (x / (float)(PANEL_LIGHT_WIDTH - 1) - 0.5f) * 2f / PANEL_LIGHT_SPREAD;
                var distance = Mathf.Sqrt(across * across + down * down);
                var alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(1f - distance));
                pixels[y * PANEL_LIGHT_WIDTH + x] = PlayTheme.with_alpha(PlayTheme.white, alpha * alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        panel_light_sprite = Sprite.Create(texture, new Rect(0f, 0f, PANEL_LIGHT_WIDTH, PANEL_LIGHT_ROWS), new Vector2(0.5f, 0.5f), 100f);
        return panel_light_sprite;
    }

    public static Image blue_pill(string name, Transform parent, bool raycast)
    {
        var ring = PlayUi.image(name, parent, PlayUi.pill(), raycast);
        ring.type = Image.Type.Sliced;
        ring.color = pill_border_color;

        var fill = PlayUi.image(name + "_fill", ring.rectTransform, PlayUi.pill());
        fill.type = Image.Type.Sliced;
        fill.color = PlayTheme.pill_blue_dark;
        PlayUi.stretch(fill.rectTransform);
        fill.rectTransform.offsetMin = new Vector2(PILL_BORDER, PILL_BORDER);
        fill.rectTransform.offsetMax = new Vector2(-PILL_BORDER, -PILL_BORDER);
        return ring;
    }

    public static Image glow_panel(string name, Transform parent, string sprite_path, float border_scale, Color color)
    {
        var art = LobbyFrameLight.glow_sprite(sprite_path);
        var picture = PlayUi.image(name, parent, art);
        picture.enabled = art != null;
        picture.type = Image.Type.Sliced;
        picture.fillCenter = false;
        picture.color = color;
        picture.pixelsPerUnitMultiplier = Mathf.Max(0.01f, border_scale);
        PlayUi.stretch(picture.rectTransform);
        if (art == null) return picture;

        var reach = LobbyFrameLight.GLOW_PAD * CANVAS_REFERENCE_PPU / (art.pixelsPerUnit * Mathf.Max(0.01f, border_scale));
        picture.rectTransform.offsetMin = new Vector2(-reach, -reach);
        picture.rectTransform.offsetMax = new Vector2(reach, reach);
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
