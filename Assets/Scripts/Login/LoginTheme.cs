using UnityEngine;

public static class LoginTheme
{
    public const string FRAME_PATH = "Frames/Login/LoginFrame";
    public const string BUTTON_GOLD_PATH = "Buttons/Play/TabSelectedGold";
    public const string ICON_EMAIL_PATH = "Icons/Login/IconEmail";
    public const string ICON_LOCK_PATH = "Icons/Login/IconLock";
    public const string ICON_EYE_PATH = "Icons/Login/IconEye";
    public const string ICON_EYE_OFF_PATH = "Icons/Login/IconEyeOff";
    public const string ICON_CLOSE_PATH = "Icons/Login/IconClose";

    public const float CARD_ASPECT = 980f / 1230f;
    public const float CARD_AREA_LEFT = 0.04f;
    public const float CARD_AREA_RIGHT = 0.96f;
    public const float CARD_AREA_BOTTOM = 0.06f;
    public const float CARD_AREA_TOP = 0.94f;
    public const float INNER_LEFT = 0.087f;
    public const float INNER_RIGHT = 0.917f;
    public const float INNER_BOTTOM = 0.065f;
    public const float INNER_TOP = 0.865f;
    public const float HEADER_RATIO = 0.28f;

    public const float CLOSE_LEFT = 0.855f;
    public const float CLOSE_RIGHT = 0.935f;
    public const float CLOSE_CENTER_Y = 0.897f;
    public const float CLOSE_BORDER = 2f;
    public const float CLOSE_ICON_INSET = 0.28f;
    public const float CLOSE_GLOW_ALPHA = 0.55f;

    public const float CORNER_SCALE = 4f;
    public const float FIELD_BORDER = 2f;
    public const float FIELD_ICON_LEFT = 0.03f;
    public const float FIELD_ICON_RIGHT = 0.115f;
    public const float FIELD_ICON_BOTTOM = 0.22f;
    public const float FIELD_ICON_TOP = 0.78f;
    public const float FIELD_TEXT_LEFT = 0.14f;
    public const float FIELD_TEXT_RIGHT = 0.96f;
    public const float FIELD_TEXT_RIGHT_PASSWORD = 0.86f;
    public const float FIELD_TEXT_BOTTOM = 0.1f;
    public const float FIELD_TEXT_TOP = 0.9f;
    public const float FIELD_EYE_LEFT = 0.88f;
    public const float FIELD_EYE_RIGHT = 0.97f;
    public const float FIELD_EYE_BOTTOM = 0.2f;
    public const float FIELD_EYE_TOP = 0.8f;
    public const float FIELD_ERROR_HEIGHT = 30f;
    public const float FIELD_ERROR_GAP = 4f;
    public const float FIELD_DISABLED_ALPHA = 0.5f;
    public const int FIELD_MAX_CHARS = 128;
    public const int FIELD_TEXT_SIZE = 26;
    public const int FIELD_ERROR_SIZE = 20;

    public const float LOADING_WIDTH = 0.26f;
    public const float LOADING_HEIGHT = 0.075f;
    public const int LOADING_TEXT_MIN_SIZE = 10;
    public const int LOADING_TEXT_SIZE = 28;
    public const string LOADING_TEXT = "SIGNING IN";

    public const float BLUR_DIM_ALPHA = 0.25f;

    public static readonly Color gold = PlayTheme.gold;
    public static readonly Color gold_soft = PlayTheme.gold_soft;
    public static readonly Color card_fill = new Color(0.008f, 0.106f, 0.204f, 1f);
    public static readonly Color field_fill = new Color(0.004f, 0.075f, 0.160f, 0.92f);
    public static readonly Color field_border = new Color(0.239f, 0.447f, 0.780f, 1f);
    public static readonly Color close_fill = new Color(0.020f, 0.100f, 0.220f, 1f);
    public static readonly Color text_primary = new Color(0.96f, 0.97f, 1f, 1f);
    public static readonly Color text_placeholder = new Color(0.62f, 0.70f, 0.84f, 1f);
    public static readonly Color icon = new Color(0.80f, 0.87f, 0.98f, 1f);
    public static readonly Color error = PlayTheme.error;
    public static readonly Color success = PlayTheme.success;
    public static readonly Color caret = PlayTheme.gold_soft;
    public static readonly Color selection = new Color(0.380f, 0.616f, 0.906f, 0.4f);
    public static readonly Color glow = PlayTheme.glow;
    public static readonly Color clear = PlayTheme.clear;
}
