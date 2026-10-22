using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class LoginField
{
    public static LoginFieldView create(RectTransform parent, string placeholder, string icon_path, bool is_password)
    {
        if (parent == null) return null;

        var ring = LoginUi.rounded_box("login_field", parent, LoginTheme.field_border, LoginTheme.field_fill, LoginTheme.FIELD_BORDER, true);
        PlayUi.stretch(ring.rectTransform);

        var symbol = LoginUi.icon("login_field_icon", ring.rectTransform, icon_path, LoginTheme.icon);
        LobbyUi.place_fraction(symbol.rectTransform, LoginTheme.FIELD_ICON_LEFT, LoginTheme.FIELD_ICON_BOTTOM, LoginTheme.FIELD_ICON_RIGHT, LoginTheme.FIELD_ICON_TOP);

        var text_right = is_password ? LoginTheme.FIELD_TEXT_RIGHT_PASSWORD : LoginTheme.FIELD_TEXT_RIGHT;
        var area = PlayUi.rect("login_field_area", ring.rectTransform);
        LobbyUi.place_fraction(area, LoginTheme.FIELD_TEXT_LEFT, LoginTheme.FIELD_TEXT_BOTTOM, text_right, LoginTheme.FIELD_TEXT_TOP);
        area.gameObject.AddComponent<RectMask2D>();

        var hint = PlayUi.text("login_field_placeholder", area, placeholder, LoginTheme.FIELD_TEXT_SIZE, LoginTheme.text_placeholder, TextAnchor.MiddleLeft);
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

        var error = PlayUi.text("login_field_error", ring.rectTransform, string.Empty, LoginTheme.FIELD_ERROR_SIZE, LoginTheme.error, TextAnchor.UpperLeft);
        PlayUi.place(error.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -LoginTheme.FIELD_ERROR_GAP), new Vector2(0f, LoginTheme.FIELD_ERROR_HEIGHT));

        Button eye = null;
        Image eye_icon = null;
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
        }

        var view = ring.gameObject.AddComponent<LoginFieldView>();
        view.setup(ring.rectTransform, ring, input, error, eye, eye_icon, is_password);
        return view;
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
