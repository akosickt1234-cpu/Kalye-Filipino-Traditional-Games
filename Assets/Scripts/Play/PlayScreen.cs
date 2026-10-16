using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class PlayScreen : MonoBehaviour
{
    const string MENU_SCENE = "Menu";
    const string CANVAS_NAME = "Canvas";
    const string EVENT_SYSTEM_NAME = "EventSystem";
    const string UI_LAYER = "UI";
    const float REFERENCE_WIDTH = 1920f;
    const float REFERENCE_HEIGHT = 1080f;
    const float MATCH_WIDTH_OR_HEIGHT = 0.5f;

    void Awake()
    {
        PlayUi.font = PlayUi.load_font();
        PlayState.reset();

        var root = create_canvas();
        ensure_event_system();

        PlayBackground.build(root);
        PlayInfoPanel.build(root);
        PlaySideNav.build(root);
        PlayTopBar.build(root);
        PlayGameCards.build(root);

        PlayState.select(0);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) PlayUi.go_to_scene(MENU_SCENE);
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
}
