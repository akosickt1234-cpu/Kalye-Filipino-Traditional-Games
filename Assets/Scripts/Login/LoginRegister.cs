using System.Collections.Generic;
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

        // Stretch to full inner height (matching LoginForm) for consistent spacing with header
        LobbyUi.place_fraction(holder, 0f, 0f, 1f, 1f / (1f - LoginTheme.HEADER_RATIO));

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
        // Stack matches the sign-in page: the username top aligns with the login email
        // field, and every field keeps a hint band below it for validation text.
        const float USERNAME_TOP = 0.690f;
        const float USERNAME_BOTTOM = 0.604f;
        const float EMAIL_TOP = 0.559f;
        const float EMAIL_BOTTOM = 0.473f;
        const float PASSWORD_TOP = 0.428f;
        const float PASSWORD_BOTTOM = 0.342f;
        // Submit plate matches the field height and the sign-in plate proportions.
        const float SUBMIT_BOTTOM = 0.209f;
        const float SUBMIT_TOP = 0.295f;
        // Divider row.
        const float DIVIDER_BOTTOM = 0.162f;
        const float DIVIDER_TOP = 0.195f;
        const float DIVIDER_LINE_LEFT_END = 0.225f;
        const float DIVIDER_LINE_RIGHT_START = 0.775f;
        const float DIVIDER_MARK_LEFT = 0.244f;
        const float DIVIDER_MARK_RIGHT = 0.756f;
        const float DIVIDER_MARK_HALF = 0.014f;
        const float DIVIDER_LABEL_LEFT = 0.29f;
        const float DIVIDER_LABEL_RIGHT = 0.71f;
        const float DIVIDER_RATIO = 0.85f;
        // Guest login row.
        const float GUEST_LEFT = 0.109f;
        const float GUEST_RIGHT = 0.887f;
        const float GUEST_BOTTOM = 0.074f;
        const float GUEST_TOP = 0.148f;
        const float GUEST_BORDER = 2f;
        const float GUEST_ICON_LEFT = 0.23f;
        const float GUEST_ICON_RIGHT = 0.33f;
        const float GUEST_ICON_BOTTOM = 0.2f;
        const float GUEST_ICON_TOP = 0.8f;
        const float GUEST_LABEL_LEFT = 0.36f;
        const float GUEST_LABEL_RIGHT = 0.76f;
        const float GUEST_LABEL_RATIO = 0.55f;
        // "Already have an account?" link row (kept clear of the card's bottom border art).
        const float LINK_BOTTOM = 0.022f;
        const float LINK_TOP = 0.060f;

    const float LINE_THICKNESS = 2f;
    const float LINE_ALPHA = 0.8f;
    const float GLOW_ALPHA = 0.45f;
    const int MIN_FONT_SIZE = 10;

    class FitItem
    {
        public Text label;
        public float ratio;
    }

    const string DIVIDER_TEXT = "OR CONTINUE WITH";
    const string GUEST_TEXT = "GUEST LOGIN";

    LoginFieldView username_field;
    LoginFieldView email_field;
    LoginFieldView password_field;
    LoginSubmitButton submit_button;
    Button link_button;
    Button guest_button;
    CanvasGroup guest_canvas;
    readonly List<FitItem> fit_items = new List<FitItem>();
    bool ready;
    bool was_busy;

    public void setup(RectTransform holder)
    {
        username_field = add_field(holder, "login_register_username", USERNAME_TOP, USERNAME_BOTTOM, "USERNAME", USERNAME_ICON_PATH, false);
        email_field = add_field(holder, "login_register_email", EMAIL_TOP, EMAIL_BOTTOM, "EMAIL", LoginTheme.ICON_EMAIL_PATH, false);
        password_field = add_field(holder, "login_register_password", PASSWORD_TOP, PASSWORD_BOTTOM, "PASSWORD", LoginTheme.ICON_LOCK_PATH, true);

        var submit_slot = PlayUi.rect("login_register_submit_slot", holder);
        LobbyUi.place_fraction(submit_slot, 0f, SUBMIT_BOTTOM, 1f, SUBMIT_TOP);
        submit_button = LoginSubmitButton.create(submit_slot, "login_register_submit", SUBMIT_TEXT, BUSY_TEXT, submit);

        build_divider(holder);
        build_guest(holder);

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

    static LoginFieldView add_field(RectTransform holder, string name, float top, float bottom, string placeholder, string icon_path, bool is_password)
    {
        var slot = PlayUi.rect(name, holder);
        LobbyUi.place_fraction(slot, 0f, bottom, 1f, top);

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
        LoginState.change_mode(LoginMode.SignIn);
    }

    void build_divider(RectTransform holder)
    {
        build_line(holder, "login_register_divider_line_left", 0f, DIVIDER_LINE_LEFT_END, DIVIDER_BOTTOM, DIVIDER_TOP);
        build_line(holder, "login_register_divider_line_right", DIVIDER_LINE_RIGHT_START, 1f, DIVIDER_BOTTOM, DIVIDER_TOP);
        build_mark(holder, "login_register_divider_mark_left", DIVIDER_MARK_LEFT, DIVIDER_BOTTOM, DIVIDER_TOP, DIVIDER_MARK_HALF);
        build_mark(holder, "login_register_divider_mark_right", DIVIDER_MARK_RIGHT, DIVIDER_BOTTOM, DIVIDER_TOP, DIVIDER_MARK_HALF);

        var label = PlayUi.text("login_register_divider_label", holder, DIVIDER_TEXT, LoginTheme.FIELD_ERROR_SIZE, LoginTheme.text_placeholder, TextAnchor.MiddleCenter);
        LobbyUi.place_fraction(label.rectTransform, DIVIDER_LABEL_LEFT, DIVIDER_BOTTOM, DIVIDER_LABEL_RIGHT, DIVIDER_TOP);
        track(label, DIVIDER_RATIO);
    }

    void build_line(RectTransform holder, string name, float left, float right, float bottom, float top)
    {
        var line = PlayUi.image(name, holder, null);
        line.color = PlayTheme.with_alpha(LoginTheme.gold, LINE_ALPHA);
        var center = (bottom + top) * 0.5f;
        PlayUi.place(line.rectTransform, new Vector2(left, center), new Vector2(right, center), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, LINE_THICKNESS));
    }

    void build_mark(RectTransform holder, string name, float center_x, float bottom, float top, float half_width)
    {
        var mark = LoginUi.icon(name, holder, LobbyUi.ICON_RHOMBUS_PATH, LoginTheme.gold);
        var center = (bottom + top) * 0.5f;
        PlayUi.place(mark.rectTransform, new Vector2(center_x - half_width, center), new Vector2(center_x + half_width, center), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        var fitter = mark.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        fitter.aspectRatio = 1f;
    }

    void build_guest(RectTransform holder)
    {
        var ring = LoginUi.rounded_box("login_register_guest", holder, LoginTheme.gold, LoginTheme.close_fill, GUEST_BORDER, true);
        LobbyUi.place_fraction(ring.rectTransform, GUEST_LEFT, GUEST_BOTTOM, GUEST_RIGHT, GUEST_TOP);

        var glow = LobbyUi.rim_glow("login_register_guest_glow", ring.rectTransform, LoginTheme.clear);
        glow.rectTransform.SetAsFirstSibling();

        var icon = LoginUi.icon("login_register_guest_icon", ring.rectTransform, LobbyUi.ICON_AVATAR_DEFAULT_PATH, LoginTheme.text_primary);
        LobbyUi.place_fraction(icon.rectTransform, GUEST_ICON_LEFT, GUEST_ICON_BOTTOM, GUEST_ICON_RIGHT, GUEST_ICON_TOP);

        var label = PlayUi.text("login_register_guest_label", ring.rectTransform, GUEST_TEXT, LoginTheme.FIELD_TEXT_SIZE, LoginTheme.text_primary, TextAnchor.MiddleCenter);
        LobbyUi.place_fraction(label.rectTransform, GUEST_LABEL_LEFT, 0.1f, GUEST_LABEL_RIGHT, 0.9f);
        track(label, GUEST_LABEL_RATIO);

        guest_button = ring.gameObject.AddComponent<Button>();
        guest_button.targetGraphic = ring;
        LobbyUi.no_transition(guest_button);
        guest_button.onClick.AddListener(try_guest);
        LoginFocusGlow.attach(guest_button, ring, LoginTheme.gold, LoginTheme.gold_soft, glow, PlayTheme.with_alpha(LoginTheme.gold, GLOW_ALPHA));
        guest_canvas = ring.gameObject.AddComponent<CanvasGroup>();
    }

    void try_guest()
    {
        if (!ready || LoginState.is_busy) return;

        LoginGuest.open(LoginMode.Register);
    }

    void apply_busy(bool busy)
    {
        if (!ready) return;

        username_field.set_enabled(!busy);
        email_field.set_enabled(!busy);
        password_field.set_enabled(!busy);
        submit_button.set_busy(busy);
        link_button.interactable = !busy;
        if (guest_button != null) guest_button.interactable = !busy;
        if (guest_canvas != null) guest_canvas.alpha = busy ? 0.5f : 1f;
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

    void LateUpdate()
    {
        for (var i = 0; i < fit_items.Count; i++) fit(fit_items[i]);
    }

    void track(Text label, float ratio)
    {
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        fit_items.Add(new FitItem { label = label, ratio = ratio });
    }

    void fit(FitItem item)
    {
        if (item.label == null) return;

        var rect = item.label.rectTransform.rect;
        if (rect.height < 1f || rect.width < 1f) return;

        var size = Mathf.Max(MIN_FONT_SIZE, Mathf.RoundToInt(rect.height * item.ratio));
        if (item.label.fontSize != size) item.label.fontSize = size;

        var wide = item.label.preferredWidth;
        if (wide > rect.width)
        {
            size = Mathf.Max(MIN_FONT_SIZE, Mathf.FloorToInt(size * rect.width / wide));
            if (item.label.fontSize != size) item.label.fontSize = size;
        }
    }
}
