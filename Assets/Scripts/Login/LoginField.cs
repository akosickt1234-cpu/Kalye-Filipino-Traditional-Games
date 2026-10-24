using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class LoginField
{
    const string LABEL_SHOW = "SHOW PASSWORD";
    const string LABEL_HIDE = "HIDE PASSWORD";
    const float TOOLTIP_SHADOW = 2f;

    public static LoginFieldView create(RectTransform parent, string placeholder, string icon_path, bool is_password)
    {
        if (parent == null) return null;

        var ring = LoginUi.input_box("login_field", parent, true);
        PlayUi.stretch(ring.rectTransform);

        var symbol = LoginUi.icon("login_field_icon", ring.rectTransform, icon_path, LoginTheme.icon);
        LobbyUi.place_fraction(symbol.rectTransform, LoginTheme.FIELD_ICON_LEFT, LoginTheme.FIELD_ICON_BOTTOM, LoginTheme.FIELD_ICON_RIGHT, LoginTheme.FIELD_ICON_TOP);

        var text_right = is_password ? LoginTheme.FIELD_TEXT_RIGHT_PASSWORD : LoginTheme.FIELD_TEXT_RIGHT;
        var area = PlayUi.rect("login_field_area", ring.rectTransform);
        LobbyUi.place_fraction(area, LoginTheme.FIELD_TEXT_LEFT, LoginTheme.FIELD_TEXT_BOTTOM, text_right, LoginTheme.FIELD_TEXT_TOP);
        area.gameObject.AddComponent<RectMask2D>();

        var hint = PlayUi.text("login_field_placeholder", area, (placeholder ?? string.Empty).ToUpperInvariant(), LoginTheme.FIELD_TEXT_SIZE, LoginTheme.text_placeholder, TextAnchor.MiddleLeft);
        hint.horizontalOverflow = HorizontalWrapMode.Overflow;
        hint.verticalOverflow = VerticalWrapMode.Truncate;
        PlayUi.stretch(hint.rectTransform);

        var value = PlayUi.text("login_field_text", area, string.Empty, LoginTheme.FIELD_TEXT_SIZE, LoginTheme.text_primary, TextAnchor.MiddleLeft);
        value.supportRichText = false;
        value.horizontalOverflow = HorizontalWrapMode.Overflow;
        value.verticalOverflow = VerticalWrapMode.Truncate;
        PlayUi.stretch(value.rectTransform);

        var input = ring.gameObject.AddComponent<InputField>();
        input.targetGraphic = ring;
        LobbyUi.no_transition(input);
        input.textComponent = value;
        input.placeholder = hint;
        input.lineType = InputField.LineType.SingleLine;
        input.contentType = is_password ? InputField.ContentType.Password : InputField.ContentType.Standard;
        input.characterLimit = LoginTheme.FIELD_MAX_CHARS;
        input.caretWidth = LobbyUi.CARET_WIDTH;
        input.customCaretColor = true;
        input.caretColor = LoginTheme.caret;
        input.selectionColor = LoginTheme.selection;
        input.onValidateInput = reject_control;
        input.onEndEdit.AddListener(on_end_edit);

        var error = PlayUi.text("login_field_error", ring.rectTransform, string.Empty, LoginTheme.FIELD_HINT_SIZE, LoginTheme.error, TextAnchor.UpperLeft);
        error.horizontalOverflow = HorizontalWrapMode.Overflow;
        error.verticalOverflow = VerticalWrapMode.Overflow;
        PlayUi.place(error.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -LoginTheme.FIELD_HINT_GAP), new Vector2(0f, LoginTheme.FIELD_HINT_HEIGHT));

        Button eye = null;
        Image eye_icon = null;
        Text eye_label = null;
        if (is_password)
        {
            var hit = PlayUi.image("login_field_eye", ring.rectTransform, null, true);
            hit.color = LoginTheme.clear;
            LobbyUi.place_fraction(hit.rectTransform, LoginTheme.FIELD_EYE_LEFT, LoginTheme.FIELD_EYE_BOTTOM, LoginTheme.FIELD_EYE_RIGHT, LoginTheme.FIELD_EYE_TOP);

            eye_icon = LoginUi.icon("login_field_eye_icon", hit.rectTransform, LoginTheme.ICON_EYE_PATH, LoginTheme.icon);
            PlayUi.stretch(eye_icon.rectTransform);

            eye = hit.gameObject.AddComponent<Button>();
            eye.targetGraphic = hit;
            LobbyUi.no_transition(eye);
            LoginFocusGlow.attach(eye, eye_icon, LoginTheme.icon, LoginTheme.gold, null, LoginTheme.clear);

            eye_label = build_eye_label(ring.rectTransform, eye, input);
        }

        var view = ring.gameObject.AddComponent<LoginFieldView>();
        view.setup(ring.rectTransform, input, error, hint, eye, eye_icon, is_password);

        if (eye != null && eye_label != null)
        {
            var label = eye_label;
            eye.onClick.AddListener(() => label.text = reveal_label(input));
        }

        return view;
    }

    static Text build_eye_label(RectTransform ring, Button eye, InputField input)
    {
        var label = PlayUi.text("login_field_eye_label", ring, reveal_label(input), LoginTheme.FIELD_ERROR_SIZE, LoginTheme.gold_soft, TextAnchor.UpperRight);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        PlayUi.place(label.rectTransform, new Vector2(LoginTheme.FIELD_EYE_RIGHT, 0f), new Vector2(LoginTheme.FIELD_EYE_RIGHT, 0f), new Vector2(1f, 1f), new Vector2(0f, -LoginTheme.FIELD_ERROR_GAP), Vector2.zero);

        var fitter = label.gameObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var shadow = label.gameObject.AddComponent<Shadow>();
        shadow.effectColor = LoginTheme.field_fill;
        shadow.effectDistance = new Vector2(TOOLTIP_SHADOW, -TOOLTIP_SHADOW);

        label.gameObject.SetActive(false);

        var state = new bool[2];
        var trigger = eye.gameObject.AddComponent<EventTrigger>();
        add_trigger(trigger, EventTriggerType.PointerEnter, () => { state[0] = true; refresh_label(label, state, input); });
        add_trigger(trigger, EventTriggerType.PointerExit, () => { state[0] = false; refresh_label(label, state, input); });
        add_trigger(trigger, EventTriggerType.Select, () => { state[1] = true; refresh_label(label, state, input); });
        add_trigger(trigger, EventTriggerType.Deselect, () => { state[1] = false; refresh_label(label, state, input); });
        return label;
    }

    static void add_trigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    static void refresh_label(Text label, bool[] state, InputField input)
    {
        if (label == null) return;

        var show = state[0] || (state[1] && PlayFocus.show_focus);
        if (show) label.text = reveal_label(input);
        label.gameObject.SetActive(show);
    }

    static string reveal_label(InputField input)
    {
        var hidden = input == null || input.contentType == InputField.ContentType.Password;
        return hidden ? LABEL_SHOW : LABEL_HIDE;
    }

    static char reject_control(string text, int index, char added)
    {
        return char.IsControl(added) ? '\0' : added;
    }

    static void on_end_edit(string value)
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) LoginState.submit();
    }
}
