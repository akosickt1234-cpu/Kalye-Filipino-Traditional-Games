using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LoginOverlay
{
    const string ROOT_NAME = "login_overlay";
    const string LOADING_NAME = "login_loading";
    const string CANVAS_NAME = "Canvas";
    const string EVENT_SYSTEM_NAME = "EventSystem";
    const string HEADER_SLOT_NAME = "login_header_slot";
    const string BODY_SLOT_NAME = "login_body_slot";

    static RectTransform root_value;
    static Action pending_success;
    static GameObject saved_selection;
    static bool selection_saved;
    static int restore_id;

    public static RectTransform root
    {
        get { return root_value; }
    }

    public static bool is_open
    {
        get { return root_value != null; }
    }

    public static void require_login(Action on_success)
    {
        if (LoginSession.is_logged_in)
        {
            if (on_success != null) on_success();
            return;
        }

        if (string.IsNullOrEmpty(LoginStore.load()))
        {
            open(on_success);
            return;
        }

        var canvas = find_canvas();
        if (canvas == null)
        {
            Debug.LogWarning("login overlay could not find a canvas in the active scene");
            return;
        }

        restore_id++;
        var id = restore_id;
        show_loading(canvas);

        LoginService.restore(
            () =>
            {
                if (id != restore_id) return;
                close();
                if (on_success != null) on_success();
            },
            reason =>
            {
                if (id != restore_id) return;
                LoginStore.clear();
                open(on_success);
            });
    }

    public static void open(Action on_success)
    {
        var canvas = find_canvas();
        if (canvas == null)
        {
            Debug.LogWarning("login overlay could not find a canvas in the active scene");
            return;
        }

        restore_id++;
        destroy_root();
        remember_selection();
        prepare();
        pending_success = on_success;
        LoginState.reset();

        var overlay = create_root(ROOT_NAME, canvas);
        LobbyUi.modal_blocker(overlay);
        LoginBackground.build(overlay);

        var inner = LoginFrame.build(overlay);
        var header_slot = create_slot(HEADER_SLOT_NAME, inner, 1f - LoginTheme.HEADER_RATIO, 1f);
        var body_slot = create_slot(BODY_SLOT_NAME, inner, 0f, 1f - LoginTheme.HEADER_RATIO);

        LoginHeader.build(header_slot);
        LoginForm.build(body_slot);
        LoginActions.build(body_slot);
        LoginRegister.build(body_slot);
        LoginForgot.build(body_slot);
        LoginGuest.build(body_slot);
        LoginUi.close_button(LoginFrame.card, close);
        LoginFeedback.build(overlay);

        PlayFocus.default_target = first_selectable(overlay);
    }

    public static void close()
    {
        restore_id++;
        pending_success = null;
        destroy_root();
        restore_selection();
    }

    public static void succeed()
    {
        var callback = pending_success;
        close();
        if (callback != null) callback();
    }

    public static void forget(GameObject destroyed)
    {
        if (root_value != null) return;

        pending_success = null;
        saved_selection = null;
        selection_saved = false;
    }

    static void show_loading(Canvas canvas)
    {
        destroy_root();
        remember_selection();
        prepare();

        var holder = create_root(LOADING_NAME, canvas);
        LobbyUi.modal_blocker(holder);

        var pill = LobbyUi.blue_pill("login_loading_pill", holder, false);
        var half_width = LoginTheme.LOADING_WIDTH * 0.5f;
        var half_height = LoginTheme.LOADING_HEIGHT * 0.5f;
        LobbyUi.place_fraction(pill.rectTransform, 0.5f - half_width, 0.5f - half_height, 0.5f + half_width, 0.5f + half_height);

        var label = PlayUi.text("login_loading_text", pill.rectTransform, LoginTheme.LOADING_TEXT, LoginTheme.LOADING_TEXT_SIZE, PlayTheme.text, TextAnchor.MiddleCenter);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = LoginTheme.LOADING_TEXT_MIN_SIZE;
        label.resizeTextMaxSize = LoginTheme.LOADING_TEXT_SIZE;
        PlayUi.stretch(label.rectTransform);
    }

    static RectTransform create_root(string name, Canvas canvas)
    {
        var holder = PlayUi.rect(name, canvas.transform);
        PlayUi.stretch(holder);
        holder.SetAsLastSibling();
        holder.gameObject.AddComponent<LoginOverlayHost>();
        root_value = holder;
        return holder;
    }

    static RectTransform create_slot(string name, RectTransform parent, float y_min, float y_max)
    {
        var slot = PlayUi.rect(name, parent);
        LobbyUi.place_fraction(slot, 0f, y_min, 1f, y_max);
        return slot;
    }

    static void destroy_root()
    {
        if (root_value != null)
        {
            root_value.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(root_value.gameObject);
        }

        root_value = null;
    }

    static void prepare()
    {
        ensure_event_system();
        PlayUi.font = PlayUi.load_font();
        PlayFocus.reset();
        PlayFocus.clear_selection();
    }

    static GameObject first_selectable(RectTransform overlay)
    {
        var found = overlay.GetComponentsInChildren<Selectable>(false);
        for (var i = 0; i < found.Length; i++)
        {
            if (found[i].IsInteractable()) return found[i].gameObject;
        }

        return null;
    }

    static void remember_selection()
    {
        if (selection_saved) return;

        var event_system = EventSystem.current;
        saved_selection = event_system != null ? event_system.currentSelectedGameObject : null;
        selection_saved = true;
    }

    static void restore_selection()
    {
        if (!selection_saved) return;

        var target = saved_selection;
        saved_selection = null;
        selection_saved = false;

        var event_system = EventSystem.current;
        if (event_system == null) return;

        event_system.SetSelectedGameObject(target != null && target.activeInHierarchy ? target : null);
    }

    static Canvas find_canvas()
    {
        Canvas fallback = null;
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        for (var i = 0; i < roots.Length; i++)
        {
            var canvases = roots[i].GetComponentsInChildren<Canvas>(false);
            for (var j = 0; j < canvases.Length; j++)
            {
                var candidate = canvases[j];
                if (!candidate.isRootCanvas) continue;
                if (candidate.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    if (fallback == null) fallback = candidate;
                    continue;
                }

                if (candidate.name == CANVAS_NAME) return candidate;
                if (fallback == null || fallback.renderMode != RenderMode.ScreenSpaceOverlay) fallback = candidate;
            }
        }

        return fallback;
    }

    static void ensure_event_system()
    {
        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;

        var holder = new GameObject(EVENT_SYSTEM_NAME, typeof(EventSystem));
        var input_module = holder.AddComponent<InputSystemUIInputModule>();
        if (input_module.actionsAsset == null) input_module.AssignDefaultActions();
    }
}
