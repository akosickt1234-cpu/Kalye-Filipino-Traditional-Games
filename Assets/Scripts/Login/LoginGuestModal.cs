using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LoginGuestModal : MonoBehaviour
{
    const string ICON_PATH = "Icons/Lobby/IconAvatarDefault";
    const string TITLE_TEXT = "GUEST NAME";
    const string HINT_TEXT = "Pick a name to play as a guest.";
    const string PLACEHOLDER_TEXT = "GUEST NAME";
    const string CANCEL_TEXT = "CANCEL";
    const string CONFIRM_TEXT = "CONTINUE";
    const string LENGTH_ERROR = "Use 3 to 20 characters.";
    const string SPACE_ERROR = "Use one space between words.";
    const string CHAR_ERROR = "Use letters, numbers, spaces, _ or -.";
    const int NAME_MIN = 3;
    const int NAME_MAX = 20;

    const float AREA_LEFT = 0.04f;
    const float AREA_RIGHT = 0.96f;
    const float AREA_BOTTOM = 0.12f;
    const float AREA_TOP = 0.88f;
    const float CARD_ASPECT = 1.6f;
    const float CARD_BORDER = 2f;
    const float CONTENT_LEFT = 0.06f;
    const float CONTENT_RIGHT = 0.94f;
    const float TITLE_BOTTOM = 0.72f;
    const float TITLE_TOP = 0.92f;
    const float HINT_BOTTOM = 0.62f;
    const float HINT_TOP = 0.72f;
    const float FIELD_BOTTOM = 0.37f;
    const float FIELD_TOP = 0.57f;
    const float BUTTON_BOTTOM = 0.06f;
    const float BUTTON_TOP = 0.24f;
    const float CANCEL_RIGHT = 0.47f;
    const float CONFIRM_LEFT = 0.53f;
    const float BUTTON_GLOW_ALPHA = 0.55f;
    const int TITLE_SIZE = 44;
    const int HINT_SIZE = 24;
    const int BUTTON_TEXT_SIZE = 28;
    const int TEXT_MIN_SIZE = 12;

    readonly List<Selectable> buffer = new List<Selectable>();
    LoginFieldView field;
    InputField input;
    Action<string> on_name;
    LoginOverlayHost host;
    GameObject previous_selection;
    GameObject previous_default;
    bool closing;

    public static LoginGuestModal open(RectTransform parent, Action<string> callback)
    {
        var shell = PlayUi.rect("login_guest_modal", parent);
        PlayUi.stretch(shell);
        shell.SetAsLastSibling();
        var modal = shell.gameObject.AddComponent<LoginGuestModal>();
        modal.on_name = callback;
        modal.hold_overlay();

        LobbyUi.modal_blocker(shell);
        var area = PlayUi.rect("login_guest_area", shell);
        LobbyUi.place_fraction(area, AREA_LEFT, AREA_BOTTOM, AREA_RIGHT, AREA_TOP);

        var card = LoginUi.rounded_box("login_guest_card", area, LoginTheme.gold, LoginTheme.card_fill, CARD_BORDER, true);
        var fitter = card.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = CARD_ASPECT;

        var content = PlayUi.rect("login_guest_content", card.rectTransform);
        PlayUi.stretch(content);
        modal.build_content(content);
        modal.focus_input();
        return modal;
    }

    void hold_overlay()
    {
        var event_system = EventSystem.current;
        previous_selection = event_system != null ? event_system.currentSelectedGameObject : null;
        previous_default = PlayFocus.default_target;

        var overlay = LoginOverlay.root;
        host = overlay != null ? overlay.GetComponent<LoginOverlayHost>() : null;
        if (host != null) host.enabled = false;
    }

    void build_content(RectTransform content)
    {
        var title = PlayUi.text("login_guest_title", content, TITLE_TEXT, TITLE_SIZE, PlayTheme.text, TextAnchor.MiddleCenter);
        fit(title, TITLE_SIZE);
        LobbyUi.place_fraction(title.rectTransform, CONTENT_LEFT, TITLE_BOTTOM, CONTENT_RIGHT, TITLE_TOP);

        var hint = PlayUi.text("login_guest_hint", content, HINT_TEXT, HINT_SIZE, LoginTheme.text_placeholder, TextAnchor.MiddleCenter);
        fit(hint, HINT_SIZE);
        LobbyUi.place_fraction(hint.rectTransform, CONTENT_LEFT, HINT_BOTTOM, CONTENT_RIGHT, HINT_TOP);

        var slot = PlayUi.rect("login_guest_field_slot", content);
        LobbyUi.place_fraction(slot, CONTENT_LEFT, FIELD_BOTTOM, CONTENT_RIGHT, FIELD_TOP);
        field = LoginField.create(slot, PLACEHOLDER_TEXT, ICON_PATH, false);
        input = field.GetComponent<InputField>();
        input.onEndEdit.RemoveAllListeners();
        input.onEndEdit.AddListener(on_end_edit);
        input.onValueChanged.AddListener(on_value_changed);

        build_button(content, "login_guest_cancel", CANCEL_TEXT, CONTENT_LEFT, CANCEL_RIGHT, false, cancel);
        build_button(content, "login_guest_confirm", CONFIRM_TEXT, CONFIRM_LEFT, CONTENT_RIGHT, true, submit);
    }

    static void build_button(RectTransform content, string name, string text, float left, float right, bool filled, UnityEngine.Events.UnityAction action)
    {
        var fill_color = filled ? LoginTheme.gold : LoginTheme.close_fill;
        var text_color = filled ? PlayTheme.label_brown : PlayTheme.text;
        var ring = LoginUi.rounded_box(name, content, LoginTheme.gold, fill_color, CARD_BORDER, true);
        LobbyUi.place_fraction(ring.rectTransform, left, BUTTON_BOTTOM, right, BUTTON_TOP);

        var glow = LobbyUi.rim_glow(name + "_glow", ring.rectTransform, LoginTheme.clear);
        glow.rectTransform.SetAsFirstSibling();

        var label = PlayUi.text(name + "_text", ring.rectTransform, text, BUTTON_TEXT_SIZE, text_color, TextAnchor.MiddleCenter);
        fit(label, BUTTON_TEXT_SIZE);
        PlayUi.stretch(label.rectTransform);

        var button = ring.gameObject.AddComponent<Button>();
        button.targetGraphic = ring;
        LobbyUi.no_transition(button);
        button.onClick.AddListener(action);

        LoginFocusGlow.attach(button, ring, LoginTheme.gold, LoginTheme.gold_soft, glow, PlayTheme.with_alpha(LoginTheme.gold, BUTTON_GLOW_ALPHA));
    }

    static void fit(Text label, int max_size)
    {
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = TEXT_MIN_SIZE;
        label.resizeTextMaxSize = max_size;
        label.verticalOverflow = VerticalWrapMode.Truncate;
    }

    void focus_input()
    {
        PlayFocus.default_target = input.gameObject;
        var event_system = EventSystem.current;
        if (event_system != null) event_system.SetSelectedGameObject(input.gameObject);
        input.ActivateInputField();
    }

    void on_end_edit(string value)
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) submit();
    }

    void on_value_changed(string value)
    {
        field.set_error(null);
    }

    void submit()
    {
        if (closing) return;

        string name;
        var error = validate(field.get_value(), out name);
        if (error != null)
        {
            field.set_error(error);
            focus_input();
            return;
        }

        var callback = on_name;
        close();
        if (callback != null) callback(name);
    }

    void cancel()
    {
        close();
    }

    void close()
    {
        if (closing) return;

        closing = true;
        on_name = null;
        PlayFocus.default_target = previous_default;
        var event_system = EventSystem.current;
        if (event_system != null)
        {
            var target = previous_selection != null && previous_selection.activeInHierarchy ? previous_selection : null;
            event_system.SetSelectedGameObject(target);
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (host != null) host.enabled = true;
    }

    void Update()
    {
        if (closing) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            cancel();
            return;
        }

        var event_system = EventSystem.current;
        if (event_system == null) return;

        var before = event_system.currentSelectedGameObject;
        PlayFocus.tick();

        if (keep_inside(event_system)) return;
        if (keyboard == null || !keyboard.tabKey.wasPressedThisFrame) return;

        move_focus(event_system, before, keyboard.shiftKey.isPressed);
    }

    bool keep_inside(EventSystem event_system)
    {
        var selected = event_system.currentSelectedGameObject;
        if (selected == null || selected.transform.IsChildOf(transform)) return false;

        collect();
        event_system.SetSelectedGameObject(buffer.Count > 0 ? buffer[0].gameObject : null);
        return true;
    }

    void move_focus(EventSystem event_system, GameObject before, bool backwards)
    {
        collect();
        if (buffer.Count == 0) return;

        var current = event_system.currentSelectedGameObject;
        if (before == null && current != null) return;

        var next = backwards ? buffer.Count - 1 : 0;
        var index = index_of(current);
        if (index >= 0)
        {
            var step = backwards ? -1 : 1;
            next = (index + step + buffer.Count) % buffer.Count;
        }

        event_system.SetSelectedGameObject(buffer[next].gameObject);
    }

    void collect()
    {
        GetComponentsInChildren(false, buffer);
        for (var i = buffer.Count - 1; i >= 0; i--)
        {
            if (!buffer[i].IsInteractable()) buffer.RemoveAt(i);
        }
    }

    int index_of(GameObject target)
    {
        if (target == null) return -1;

        for (var i = 0; i < buffer.Count; i++)
        {
            if (buffer[i].gameObject == target) return i;
        }

        return -1;
    }

    static string validate(string raw, out string name)
    {
        name = (raw ?? string.Empty).Trim();
        if (name.Length < NAME_MIN || name.Length > NAME_MAX) return LENGTH_ERROR;
        if (name.Contains("  ")) return SPACE_ERROR;

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (!(char.IsLetterOrDigit(c) || c == ' ' || c == '_' || c == '-')) return CHAR_ERROR;
        }

        return null;
    }
}
