using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class LobbyScreen : MonoBehaviour
{
    const string CANVAS_NAME = "Canvas";
    const string EVENT_SYSTEM_NAME = "EventSystem";
    const string UI_LAYER = "UI";
    const float REFERENCE_WIDTH = 1920f;
    const float REFERENCE_HEIGHT = 1080f;
    const float MATCH_WIDTH_OR_HEIGHT = 0.5f;
    const float TOP_BAR_HEIGHT_RATIO = 0.12f;
    const float PANEL_TOP = 0.17f;
    const float PANEL_BOTTOM = 0.79f;
    const float MAIN_LEFT = 0.05f;
    const float MAIN_RIGHT = 0.66f;
    const float FRIENDS_LEFT = 0.70f;
    const float FRIENDS_RIGHT = 0.95f;
    const float HEADING_LEFT = 0.03f;
    const float HEADING_RIGHT = 0.58f;
    const float HEADING_TOP = 0.05f;
    const float HEADING_BOTTOM = 0.16f;
    const float CODE_LEFT = 0.60f;
    const float CODE_RIGHT = 0.97f;
    const float CODE_TOP = 0.05f;
    const float CODE_BOTTOM = 0.16f;
    const float SLOTS_LEFT = 0.03f;
    const float SLOTS_RIGHT = 0.97f;
    const float SLOTS_TOP = 0.172f;
    const float SLOTS_BOTTOM = 0.76f;
    const float INFO_TOP = 0.80f;
    const float INFO_BOTTOM = 0.97f;
    const float START_LEFT = 0.32f;
    const float START_RIGHT = 0.68f;
    const float BOTTOM_TOP = 0.82f;
    const float BOTTOM_BOTTOM = 0.94f;
    const float COLUMN_COUNT = 3f;

    public static RectTransform overlay_root { get; private set; }

    void Awake()
    {
        PlayUi.font = PlayUi.load_font();
        PlayFocus.reset();
        LobbyState.enter();

        var root = create_canvas();
        ensure_event_system();

        LobbyBackground.build(root);

        var top_bar = create_region("top_bar", root, 0f, 1f - TOP_BAR_HEIGHT_RATIO, 1f, 1f);

        var main_panel = LobbyUi.panel("main_panel", root, LobbyUi.PANEL_LOBBY_PATH, LobbyUi.PANEL_BORDER_SCALE, LobbyFrameLight.Style.GoldTop);
        place_from_top(main_panel.rectTransform, MAIN_LEFT, PANEL_TOP, MAIN_RIGHT, PANEL_BOTTOM);

        var heading_slot = create_region("heading_slot", main_panel.rectTransform, HEADING_LEFT, 1f - HEADING_BOTTOM, HEADING_RIGHT, 1f - HEADING_TOP);
        var code_slot = create_region("code_slot", main_panel.rectTransform, CODE_LEFT, 1f - CODE_BOTTOM, CODE_RIGHT, 1f - CODE_TOP);
        var slots_row = create_region("slots_row", main_panel.rectTransform, SLOTS_LEFT, 1f - SLOTS_BOTTOM, SLOTS_RIGHT, 1f - SLOTS_TOP);
        var info_row = create_region("info_row", main_panel.rectTransform, SLOTS_LEFT, 1f - INFO_BOTTOM, SLOTS_RIGHT, 1f - INFO_TOP);
        var chat_col = create_column("chat_col", info_row, 0);
        var count_col = create_column("count_col", info_row, 1);
        var mode_col = create_column("mode_col", info_row, 2);

        var friends_panel = LobbyUi.panel("friends_panel", root, LobbyUi.PANEL_LOBBY_PATH, LobbyUi.PANEL_BORDER_SCALE, LobbyFrameLight.Style.GoldTop);
        place_from_top(friends_panel.rectTransform, FRIENDS_LEFT, PANEL_TOP, FRIENDS_RIGHT, PANEL_BOTTOM);

        var start_bar = create_region("start_bar", root, START_LEFT, 1f - BOTTOM_BOTTOM, START_RIGHT, 1f - BOTTOM_TOP);

        LobbyTopBar.build(top_bar);
        LobbyPlayerSlots.build(heading_slot, slots_row);
        LobbyChat.build(chat_col);
        LobbyInfoBar.build(count_col, mode_col);
        LobbyCode.build(code_slot);
        LobbyFriendsPanel.build(friends_panel.rectTransform);
        LobbyStartBar.build(start_bar);

        overlay_root = create_region("overlay_root", root, 0f, 0f, 1f, 1f);
        overlay_root.SetAsLastSibling();
    }

    void Update()
    {
        PlayFocus.tick();
    }

    void OnDestroy()
    {
        overlay_root = null;
        LobbyState.clear_listeners();
    }

    RectTransform create_canvas()
    {
        var canvas_object = new GameObject(CANVAS_NAME, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas_object.layer = LayerMask.NameToLayer(UI_LAYER);
        canvas_object.transform.SetParent(transform, false);

        var canvas = canvas_object.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvas_object.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REFERENCE_WIDTH, REFERENCE_HEIGHT);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = MATCH_WIDTH_OR_HEIGHT;

        return canvas_object.GetComponent<RectTransform>();
    }

    void ensure_event_system()
    {
        if (FindAnyObjectByType<EventSystem>() != null) return;

        var event_system_object = new GameObject(EVENT_SYSTEM_NAME, typeof(EventSystem));
        var input_module = event_system_object.AddComponent<InputSystemUIInputModule>();
        if (input_module.actionsAsset == null) input_module.AssignDefaultActions();
    }

    static RectTransform create_region(string name, Transform parent, float x_min, float y_min, float x_max, float y_max)
    {
        var region = PlayUi.rect(name, parent);
        LobbyUi.place_fraction(region, x_min, y_min, x_max, y_max);
        return region;
    }

    static RectTransform create_column(string name, Transform parent, int index)
    {
        return create_region(name, parent, index / COLUMN_COUNT, 0f, (index + 1) / COLUMN_COUNT, 1f);
    }

    static void place_from_top(RectTransform r, float x_min, float top, float x_max, float bottom)
    {
        LobbyUi.place_fraction(r, x_min, 1f - bottom, x_max, 1f - top);
    }
}
