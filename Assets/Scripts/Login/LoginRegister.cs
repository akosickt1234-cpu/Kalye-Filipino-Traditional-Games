using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class LoginRegister
{
    const int USERNAME_MIN = 3;
    const int USERNAME_MAX = 20;
    const int PASSWORD_MIN = 8;
    const int EMAIL_MAX = 254;

    public static void build(RectTransform root)
    {
        var holder = LoginUi.view("login_register", root, LoginMode.Register);
        if (holder == null) return;

        holder.gameObject.AddComponent<LoginRegisterView>().setup(holder);
    }

    public static string username_error(string username)
    {
        if (string.IsNullOrEmpty(username)) return "Enter a username.";
        if (username.Length < USERNAME_MIN || username.Length > USERNAME_MAX) return "Use 3 to 20 characters.";

        for (var i = 0; i < username.Length; i++)
        {
            var letter = username[i];
            var allowed = letter < 128 && (char.IsLetterOrDigit(letter) || letter == '_');
            if (!allowed) return "Use letters, numbers or _ only.";
        }

        return null;
    }

    public static string email_error(string email)
    {
        if (string.IsNullOrEmpty(email)) return "Enter your email.";

        var at = email.IndexOf('@');
        var valid = at > 0 && at == email.LastIndexOf('@') && email.IndexOf(' ') < 0 && email.Length <= EMAIL_MAX;
        if (valid)
        {
            var dot = email.LastIndexOf('.');
            valid = dot > at + 1 && dot < email.Length - 1;
        }

        return valid ? null : "Enter a valid email.";
    }

    public static string password_error(string password)
    {
        if (string.IsNullOrEmpty(password)) return "Enter a password.";
        if (password.Length < PASSWORD_MIN) return "Use at least 8 characters.";
        return null;
    }
}

public class LoginRegisterView : MonoBehaviour
{
    const string USERNAME_ICON_PATH = "Icons/Play/IconTopProfile";
    const string SUBMIT_TEXT = "CREATE ACCOUNT";
    const string BUSY_TEXT = "CREATING";
    const string LINK_PREFIX = "Already have an account?";
    const string LINK_TEXT = "Log in";
    const string FAIL_TEXT = "Could not create the account. Try again.";
    const float FIELD_HEIGHT = 0.15f;
    const float USERNAME_TOP = 0.99f;
    const float EMAIL_TOP = 0.77f;
    const float PASSWORD_TOP = 0.55f;
    const float SUBMIT_BOTTOM = 0.12f;
    const float SUBMIT_TOP = 0.34f;
    const float LINK_BOTTOM = 0f;
    const float LINK_TOP = 0.10f;

    LoginFieldView username_field;
    LoginFieldView email_field;
    LoginFieldView password_field;
    LoginSubmitButton submit_button;
    Button link_button;
    bool ready;
    bool was_busy;

    public void setup(RectTransform holder)
    {
        username_field = add_field(holder, "login_register_username", USERNAME_TOP, "USERNAME", USERNAME_ICON_PATH, false);
        email_field = add_field(holder, "login_register_email", EMAIL_TOP, "EMAIL", LoginTheme.ICON_EMAIL_PATH, false);
        password_field = add_field(holder, "login_register_password", PASSWORD_TOP, "PASSWORD", LoginTheme.ICON_LOCK_PATH, true);

        var submit_slot = PlayUi.rect("login_register_submit_slot", holder);
        LobbyUi.place_fraction(submit_slot, 0f, SUBMIT_BOTTOM, 1f, SUBMIT_TOP);
        submit_button = LoginSubmitButton.create(submit_slot, "login_register_submit", SUBMIT_TEXT, BUSY_TEXT, submit);

        var link_slot = PlayUi.rect("login_register_link_slot", holder);
        LobbyUi.place_fraction(link_slot, 0f, LINK_BOTTOM, 1f, LINK_TOP);
        link_button = LoginTextLink.create(link_slot, LINK_PREFIX, LINK_TEXT, back_to_sign_in);

        ready = true;
        apply_busy(LoginState.is_busy);
    }

    void OnEnable()
    {
        LoginState.on_submit += on_submit_requested;
        LoginState.on_busy_changed += apply_busy;
        if (!ready) return;

        apply_busy(LoginState.is_busy);
        select(username_field);
    }

    void OnDisable()
    {
        LoginState.on_submit -= on_submit_requested;
        LoginState.on_busy_changed -= apply_busy;
    }

    static LoginFieldView add_field(RectTransform holder, string name, float top, string placeholder, string icon_path, bool is_password)
    {
        var slot = PlayUi.rect(name, holder);
        LobbyUi.place_fraction(slot, 0f, top - FIELD_HEIGHT, 1f, top);

        var field = LoginField.create(slot, placeholder, icon_path, is_password);
        if (field == null) return null;

        var input = field.root.GetComponent<InputField>();
        if (input != null) input.onValueChanged.AddListener(value => field.set_error(null));
        return field;
    }

    static void select(LoginFieldView field)
    {
        var event_system = EventSystem.current;
        if (event_system == null || field == null) return;

        event_system.SetSelectedGameObject(field.root.gameObject);
    }

    void on_submit_requested()
    {
        if (LoginState.mode != LoginMode.Register) return;
        submit();
    }

    void back_to_sign_in()
    {
        LoginState.set_mode(LoginMode.SignIn);
    }

    void apply_busy(bool busy)
    {
        if (!ready) return;

        username_field.set_enabled(!busy);
        email_field.set_enabled(!busy);
        password_field.set_enabled(!busy);
        submit_button.set_busy(busy);
        link_button.interactable = !busy;
        var finished = was_busy && !busy;
        was_busy = busy;
        if (!finished) return;

        var event_system = EventSystem.current;
        if (event_system != null && event_system.currentSelectedGameObject == null) event_system.SetSelectedGameObject(submit_button.control.gameObject);
    }

    void submit()
    {
        if (!ready || LoginState.is_busy) return;

        var username = LobbyUi.sanitize(username_field.get_value());
        var email = LobbyUi.sanitize(email_field.get_value());
        var password = password_field.get_value();

        var username_problem = LoginRegister.username_error(username);
        var email_problem = LoginRegister.email_error(email);
        var password_problem = LoginRegister.password_error(password);
        username_field.set_error(username_problem);
        email_field.set_error(email_problem);
        password_field.set_error(password_problem);

        if (username_problem != null) select(username_field);
        else if (email_problem != null) select(email_field);
        else if (password_problem != null) select(password_field);
        if (username_problem != null || email_problem != null || password_problem != null) return;

        LoginState.set_busy(true);
        LoginService.register(username, email, password, () => on_registered(username), on_failed);
    }

    void on_registered(string username)
    {
        if (this == null) return;

        LoginState.set_busy(false);
        if (!LoginSession.is_logged_in) LoginSession.set(username, string.Empty, string.Empty, false);
        LoginOverlay.succeed();
    }

    void on_failed(string reason)
    {
        if (this == null) return;

        LoginState.set_busy(false);
        LoginFeedback.toast(string.IsNullOrEmpty(reason) ? FAIL_TEXT : reason, true);
    }
}
