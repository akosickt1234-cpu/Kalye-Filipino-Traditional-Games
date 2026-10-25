using System;
using UnityEngine;
using UnityEngine.UI;

public static class ApiToast
{
    const string CANVAS_NAME = "api_toast_canvas";
    const string HOLDER_NAME = "api_toast_holder";
    const string UI_LAYER = "UI";
    const int SORT_ORDER = 5000;
    const float REFERENCE_WIDTH = 1920f;
    const float REFERENCE_HEIGHT = 1080f;
    const float MATCH_WIDTH_OR_HEIGHT = 0.5f;

    static RectTransform holder;
    static LoginToast current;

    public static void toast(string message, bool is_error)
    {
        show(message, is_error ? LoginTheme.error : LoginTheme.success, null);
    }

    public static void toast_retry(string message, Action retry_action)
    {
        show(message, LoginTheme.error, retry_action);
    }

    static void show(string message, Color accent, Action retry_action)
    {
        var clean = LobbyUi.sanitize(message);
        if (clean.Length == 0) return;

        ensure_holder();
        if (retry_action == null && current != null && current.has_retry && current.message == clean) return;

        if (current != null)
        {
            current.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(current.gameObject);
        }

        current = LoginToast.create(holder, clean, accent, retry_action);
    }

    static void ensure_holder()
    {
        if (holder != null) return;

        var canvas_object = new GameObject(CANVAS_NAME, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas_object.layer = LayerMask.NameToLayer(UI_LAYER);
        UnityEngine.Object.DontDestroyOnLoad(canvas_object);

        var canvas = canvas_object.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SORT_ORDER;

        var scaler = canvas_object.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REFERENCE_WIDTH, REFERENCE_HEIGHT);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = MATCH_WIDTH_OR_HEIGHT;

        holder = PlayUi.rect(HOLDER_NAME, canvas_object.transform);
        PlayUi.stretch(holder);
    }
}
