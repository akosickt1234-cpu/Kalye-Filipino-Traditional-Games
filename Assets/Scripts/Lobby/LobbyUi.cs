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
    static Sprite rim_glow_sprite;

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
