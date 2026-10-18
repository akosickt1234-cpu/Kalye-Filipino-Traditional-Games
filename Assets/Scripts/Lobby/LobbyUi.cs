using UnityEngine;
using UnityEngine.UI;

public static class LobbyUi
{
    public const string PANEL_LOBBY_PATH = "Frames/Lobby/PanelLobby";
    public const string FRAME_PLAYER_CARD_PATH = "Frames/Lobby/FramePlayerCard";
    public const string FRAME_AVATAR_OVAL_PATH = "Frames/Lobby/FrameAvatarOval";
    public const string ICON_CROWN_PATH = "Icons/Lobby/IconCrown";
    public const string ICON_MINIMIZE_PATH = "Icons/Lobby/IconMinimize";
    public const string ICON_KEY_PATH = "Icons/Lobby/IconKey";
    public const string ICON_PRIVATE_LOBBY_PATH = "Icons/Lobby/IconPrivateLobby";
    public const string ICON_CHAT_PATH = "Icons/Lobby/IconChat";
    public const string ICON_REFRESH_PATH = "Icons/Lobby/IconRefresh";
    public const string ICON_COPY_PATH = "Icons/Lobby/IconCopy";
    public const string ICON_ARROW_RIGHT_PATH = "Icons/Lobby/IconArrowRight";
    public const string BACKDROP_PATH = "Background/Lobby/LobbyBackdrop";
    public const float PANEL_BORDER_SCALE = 1.8f;
    const float BLOCKER_ALPHA = 0.6f;

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
}
