using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginForm : MonoBehaviour
{
    const float EMAIL_TOP = 0.690f;
    const float EMAIL_BOTTOM = 0.587f;
    // Password sits below the email field's reserved hint band.
    const float PASSWORD_TOP = 0.535f;
    const float PASSWORD_BOTTOM = 0.432f;
    const string EMAIL_PLACEHOLDER = "Email or username";
    const string PASSWORD_PLACEHOLDER = "Password";
    const string EMAIL_MISSING = "Enter your email or username.";
    const string PASSWORD_MISSING = "Enter your password.";

    static LoginForm active;

    LoginFieldView identifier_field;
    LoginFieldView password_field;
    InputField identifier_input;
    InputField password_input;

    public static void build(RectTransform root)
    {
        var holder = LoginUi.view("login_form", root, LoginMode.SignIn);
        if (holder == null) return;

        LobbyUi.place_fraction(holder, 0f, 0f, 1f, 1f / (1f - LoginTheme.HEADER_RATIO));

        var identifier_row = PlayUi.rect("login_form_identifier_row", holder);
        LobbyUi.place_fraction(identifier_row, 0f, EMAIL_BOTTOM, 1f, EMAIL_TOP);

        var password_row = PlayUi.rect("login_form_password_row", holder);
        LobbyUi.place_fraction(password_row, 0f, PASSWORD_BOTTOM, 1f, PASSWORD_TOP);

        var form = holder.gameObject.AddComponent<LoginForm>();
        form.identifier_field = LoginField.create(identifier_row, EMAIL_PLACEHOLDER, LoginTheme.ICON_EMAIL_PATH, false);
        form.password_field = LoginField.create(password_row, PASSWORD_PLACEHOLDER, LoginTheme.ICON_LOCK_PATH, true);
        form.setup();
    }

    public static string identifier()
    {
        return active != null && active.identifier_field != null ? active.identifier_field.get_value().Trim() : string.Empty;
    }

    public static string password()
    {
        return active != null && active.password_field != null ? active.password_field.get_value() : string.Empty;
    }

    public static bool validate()
    {
        if (active == null) return false;

        var missing_identifier = string.IsNullOrEmpty(identifier());
        var missing_password = string.IsNullOrEmpty(password());
        active.identifier_field.set_error(missing_identifier ? EMAIL_MISSING : null);
        active.password_field.set_error(missing_password ? PASSWORD_MISSING : null);

        if (missing_identifier) active.select(active.identifier_input);
        else if (missing_password) active.select(active.password_input);
        return !missing_identifier && !missing_password;
    }

    public static void show_error(string message)
    {
        if (active == null) return;

        active.password_field.set_error(LobbyUi.sanitize(message));
    }

    public static void clear_errors()
    {
        if (active == null) return;

        active.identifier_field.set_error(null);
        active.password_field.set_error(null);
    }

    public static void focus_first()
    {
        if (active != null) active.queue_focus(active.identifier_input);
    }

    void setup()
    {
        if (identifier_field == null || password_field == null) return;

        identifier_input = identifier_field.root.GetComponent<InputField>();
        password_input = password_field.root.GetComponent<InputField>();
        if (identifier_input != null) identifier_input.onValueChanged.AddListener(_ => identifier_field.set_error(null));
        if (password_input != null) password_input.onValueChanged.AddListener(_ => password_field.set_error(null));

        active = this;
        LoginState.on_busy_changed += on_busy_changed;
        LoginState.on_mode_changed += on_mode_changed;
        apply_busy(LoginState.is_busy);
        if (isActiveAndEnabled) queue_focus(identifier_input);
    }

    void OnEnable()
    {
        if (identifier_input != null) queue_focus(identifier_input);
    }

    void OnDestroy()
    {
        LoginState.on_busy_changed -= on_busy_changed;
        LoginState.on_mode_changed -= on_mode_changed;
        if (active == this) active = null;
    }

    void on_mode_changed(LoginMode value)
    {
        if (value == LoginMode.SignIn) queue_focus(identifier_input);
    }

    void on_busy_changed(bool busy)
    {
        apply_busy(busy);
        if (busy) return;

        var target = identifier_field.get_value().Length > 0 ? password_input : identifier_input;
        queue_focus(target);
    }

    void apply_busy(bool busy)
    {
        identifier_field.set_enabled(!busy);
        password_field.set_enabled(!busy);
    }

    void queue_focus(InputField target)
    {
        if (target == null || !isActiveAndEnabled) return;

        StopAllCoroutines();
        StartCoroutine(focus_next_frame(target));
    }

    IEnumerator focus_next_frame(InputField target)
    {
        yield return null;
        if (target != null && LoginState.mode == LoginMode.SignIn && !LoginState.is_busy) select(target);
    }

    void select(InputField target)
    {
        var event_system = EventSystem.current;
        if (target == null || event_system == null || !target.IsInteractable() || !target.gameObject.activeInHierarchy) return;

        event_system.SetSelectedGameObject(target.gameObject);
        target.ActivateInputField();
    }
}
