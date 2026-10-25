using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class LoginActions
{
    public static void build(RectTransform root)
    {
        var holder = LoginUi.view("login_actions", root, LoginMode.SignIn);
        if (holder == null) return;

        holder.gameObject.AddComponent<LoginActionsView>().setup(holder);
    }
}

public class LoginActionsView : MonoBehaviour
{
    class FitItem
    {
        public Text label;
        public float ratio;
        public RectTransform underline;
        public float align;
    }

    const string BUTTON_PATH = "Buttons/Play/BtnPlayGold";
    const string FORM_NAME = "login_form";
    const string LOGIN_TEXT = "LOG IN";
    const string GUEST_TEXT = "GUEST LOGIN";
    const string FORGOT_TEXT = "Forgot password?";
    const string DIVIDER_TEXT = "OR CONTINUE WITH";
    const string NEW_TEXT = "New to Kalye?";
    const string CREATE_TEXT = "Create Account";
    const string EMPTY_IDENTIFIER = "Enter your email or username.";
    const string EMPTY_PASSWORD = "Enter your password.";
    const string FORM_NOT_READY = "The login form is not ready.";
    const string GENERIC_FAIL = "Login failed. Please try again.";

    const float SUBMIT_LEFT = 0.2575f;
    const float SUBMIT_RIGHT = 0.7425f;
    const float SUBMIT_BOTTOM = 0.286f;
    const float SUBMIT_TOP = 0.381f;
    const float SUBMIT_STAR_LEFT = 0.215f;
    const float SUBMIT_STAR_RIGHT = 0.285f;
    const float SUBMIT_STAR_BOTTOM = 0.26f;
    const float SUBMIT_STAR_TOP = 0.74f;
    const float SUBMIT_LABEL_LEFT = 0.3f;
    const float SUBMIT_LABEL_RIGHT = 0.7f;
    const float SUBMIT_LABEL_RATIO = 0.5f;
    const float SUBMIT_NORMAL_TINT = 0.94f;
    const float SPIN_DEGREES = 240f;

    const float FORGOT_LEFT = 0.3f;
    const float FORGOT_RIGHT = 0.7f;
    const float FORGOT_BOTTOM = 0.231f;
    const float FORGOT_TOP = 0.269f;
    const float FORGOT_RATIO = 0.8f;

    const float DIVIDER_BOTTOM = 0.182f;
    const float DIVIDER_TOP = 0.215f;
    const float DIVIDER_LINE_LEFT_START = 0f;
    const float DIVIDER_LINE_LEFT_END = 0.225f;
    const float DIVIDER_LINE_RIGHT_START = 0.775f;
    const float DIVIDER_LINE_RIGHT_END = 1f;
    const float DIVIDER_MARK_LEFT = 0.244f;
    const float DIVIDER_MARK_RIGHT = 0.756f;
    const float DIVIDER_MARK_HALF = 0.014f;
    const float DIVIDER_LABEL_LEFT = 0.29f;
    const float DIVIDER_LABEL_RIGHT = 0.71f;
    const float DIVIDER_RATIO = 0.85f;

    const float GUEST_LEFT = 0.109f;
    const float GUEST_RIGHT = 0.887f;
    const float GUEST_BOTTOM = 0.091f;
    const float GUEST_TOP = 0.165f;
    const float GUEST_BORDER = 2f;
    const float GUEST_ICON_LEFT = 0.23f;
    const float GUEST_ICON_RIGHT = 0.33f;
    const float GUEST_ICON_BOTTOM = 0.2f;
    const float GUEST_ICON_TOP = 0.8f;
    const float GUEST_LABEL_LEFT = 0.36f;
    const float GUEST_LABEL_RIGHT = 0.76f;
    const float GUEST_LABEL_RATIO = 0.55f;

    // Lifted off the card's bottom border so the row has its own margin.
    const float NEW_BOTTOM = 0.016f;
    const float NEW_TOP = 0.072f;
    const float NEW_LINE_LEFT_START = 0.12f;
    const float NEW_LINE_LEFT_END = 0.172f;
    const float NEW_LINE_RIGHT_START = 0.826f;
    const float NEW_LINE_RIGHT_END = 0.88f;
    const float NEW_MARK_LEFT = 0.193f;
    const float NEW_MARK_RIGHT = 0.805f;
    const float NEW_MARK_HALF = 0.012f;
    const float NEW_LABEL_LEFT = 0.22f;
    const float NEW_LABEL_SPLIT = 0.47f;
    const float NEW_LABEL_GAP = 0.0075f;
    const float NEW_LABEL_RIGHT = 0.78f;
    const float NEW_RATIO = 0.62f;

    const float LINE_THICKNESS = 2f;
    const float LINE_ALPHA = 0.8f;
    const float UNDERLINE_THICKNESS = 2f;
    const float UNDERLINE_DROP = 0.55f;
    const float GLOW_ALPHA = 0.45f;
    const float DISABLED_ALPHA = 0.5f;
    const int MIN_FONT_SIZE = 10;

    static readonly Color divider_text_color = LoginTheme.text_placeholder;
    static readonly Color link_color = PlayTheme.white_blue;

    readonly List<FitItem> fit_items = new List<FitItem>();
    readonly List<CanvasGroup> locked_groups = new List<CanvasGroup>();
    readonly List<RectTransform> spinners = new List<RectTransform>();

    RectTransform host;
    Button submit_button;
    Text submit_label;
    LoginFieldView identifier_field;
    LoginFieldView password_field;
    int request_ticket;
    bool owns_busy;

    public void setup(RectTransform holder)
    {
        host = holder;

        build_submit();
        build_forgot();
        build_divider();
        build_guest();
        build_create();

        LoginState.on_busy_changed += refresh;
        LoginState.on_submit += try_login;
        refresh(LoginState.is_busy);
    }

    void OnDestroy()
    {
        LoginState.on_busy_changed -= refresh;
        LoginState.on_submit -= try_login;
        request_ticket++;
        if (!owns_busy) return;

        owns_busy = false;
        LoginState.set_busy(false);
    }

    void Update()
    {
        if (!LoginState.is_busy) return;

        for (var i = 0; i < spinners.Count; i++)
        {
            if (spinners[i] != null) spinners[i].Rotate(0f, 0f, -SPIN_DEGREES * Time.unscaledDeltaTime);
        }
    }

    void LateUpdate()
    {
        for (var i = 0; i < fit_items.Count; i++) fit(fit_items[i]);
    }

    static float slot(float inner_y)
    {
        return inner_y / (1f - LoginTheme.HEADER_RATIO);
    }

    static void place(RectTransform rect, float left, float bottom, float right, float top)
    {
        LobbyUi.place_fraction(rect, left, slot(bottom), right, slot(top));
    }

    void build_submit()
    {
        var face = PlayUi.image("login_submit", host, PlayUi.sprite(BUTTON_PATH), true);
        face.type = Image.Type.Simple;
        face.preserveAspect = false;
        place(face.rectTransform, SUBMIT_LEFT, SUBMIT_BOTTOM, SUBMIT_RIGHT, SUBMIT_TOP);

        var glow = LobbyUi.rim_glow("login_submit_glow", face.rectTransform, LoginTheme.clear, 0f);
        glow.rectTransform.SetAsFirstSibling();

        spinners.Add(build_star("login_submit_star_left", face.rectTransform, SUBMIT_STAR_LEFT, SUBMIT_STAR_RIGHT));
        spinners.Add(build_star("login_submit_star_right", face.rectTransform, 1f - SUBMIT_STAR_RIGHT, 1f - SUBMIT_STAR_LEFT));

        submit_label = PlayUi.text("login_submit_label", face.rectTransform, LOGIN_TEXT, LoginTheme.LOADING_TEXT_SIZE, PlayTheme.label_brown, TextAnchor.MiddleCenter);
        submit_label.fontStyle = FontStyle.Bold;
        LobbyUi.place_fraction(submit_label.rectTransform, SUBMIT_LABEL_LEFT, 0.1f, SUBMIT_LABEL_RIGHT, 0.9f);
        track(submit_label, SUBMIT_LABEL_RATIO, 0f);

        submit_button = face.gameObject.AddComponent<Button>();
        submit_button.targetGraphic = face;
        LobbyUi.no_transition(submit_button);
        submit_button.onClick.AddListener(try_login);

        var normal = new Color(SUBMIT_NORMAL_TINT, SUBMIT_NORMAL_TINT, SUBMIT_NORMAL_TINT, 1f);
        LoginFocusGlow.attach(submit_button, face, normal, PlayTheme.white, glow, PlayTheme.with_alpha(LoginTheme.gold, GLOW_ALPHA));
    }

    RectTransform build_star(string name, RectTransform parent, float left, float right)
    {
        var star = LoginUi.icon(name, parent, LobbyUi.ICON_RHOMBUS_PATH, PlayTheme.label_brown);
        LobbyUi.place_fraction(star.rectTransform, left, SUBMIT_STAR_BOTTOM, right, SUBMIT_STAR_TOP);
        return star.rectTransform;
    }

    void build_forgot()
    {
        var label = build_link("login_forgot_link", FORGOT_TEXT, link_color, FORGOT_LEFT, FORGOT_BOTTOM, FORGOT_RIGHT, FORGOT_TOP, FORGOT_RATIO, TextAnchor.MiddleCenter, 0.5f, () => LoginState.change_mode(LoginMode.Forgot));
        locked_groups.Add(label.gameObject.AddComponent<CanvasGroup>());
    }

    void build_divider()
    {
        build_line("login_divider_line_left", DIVIDER_LINE_LEFT_START, DIVIDER_LINE_LEFT_END, DIVIDER_BOTTOM, DIVIDER_TOP);
        build_line("login_divider_line_right", DIVIDER_LINE_RIGHT_START, DIVIDER_LINE_RIGHT_END, DIVIDER_BOTTOM, DIVIDER_TOP);
        build_mark("login_divider_mark_left", DIVIDER_MARK_LEFT, DIVIDER_BOTTOM, DIVIDER_TOP, DIVIDER_MARK_HALF);
        build_mark("login_divider_mark_right", DIVIDER_MARK_RIGHT, DIVIDER_BOTTOM, DIVIDER_TOP, DIVIDER_MARK_HALF);

        var label = PlayUi.text("login_divider_label", host, DIVIDER_TEXT, LoginTheme.FIELD_ERROR_SIZE, divider_text_color, TextAnchor.MiddleCenter);
        place(label.rectTransform, DIVIDER_LABEL_LEFT, DIVIDER_BOTTOM, DIVIDER_LABEL_RIGHT, DIVIDER_TOP);
        track(label, DIVIDER_RATIO, 0f);
    }

    void build_guest()
    {
        var ring = LoginUi.rounded_box("login_guest", host, LoginTheme.gold, LoginTheme.close_fill, GUEST_BORDER, true);
        place(ring.rectTransform, GUEST_LEFT, GUEST_BOTTOM, GUEST_RIGHT, GUEST_TOP);

        var glow = LobbyUi.rim_glow("login_guest_glow", ring.rectTransform, LoginTheme.clear);
        glow.rectTransform.SetAsFirstSibling();

        var icon = LoginUi.icon("login_guest_icon", ring.rectTransform, LobbyUi.ICON_AVATAR_DEFAULT_PATH, LoginTheme.text_primary);
        LobbyUi.place_fraction(icon.rectTransform, GUEST_ICON_LEFT, GUEST_ICON_BOTTOM, GUEST_ICON_RIGHT, GUEST_ICON_TOP);

        var label = PlayUi.text("login_guest_label", ring.rectTransform, GUEST_TEXT, LoginTheme.FIELD_TEXT_SIZE, LoginTheme.text_primary, TextAnchor.MiddleCenter);
        LobbyUi.place_fraction(label.rectTransform, GUEST_LABEL_LEFT, 0.1f, GUEST_LABEL_RIGHT, 0.9f);
        track(label, GUEST_LABEL_RATIO, 0f);

        var button = ring.gameObject.AddComponent<Button>();
        button.targetGraphic = ring;
        LobbyUi.no_transition(button);
        button.onClick.AddListener(try_guest);
        LoginFocusGlow.attach(button, ring, LoginTheme.gold, LoginTheme.gold_soft, glow, PlayTheme.with_alpha(LoginTheme.gold, GLOW_ALPHA));
        locked_groups.Add(ring.gameObject.AddComponent<CanvasGroup>());
    }

    void build_create()
    {
        build_line("login_new_line_left", NEW_LINE_LEFT_START, NEW_LINE_LEFT_END, NEW_BOTTOM, NEW_TOP);
        build_line("login_new_line_right", NEW_LINE_RIGHT_START, NEW_LINE_RIGHT_END, NEW_BOTTOM, NEW_TOP);
        build_mark("login_new_mark_left", NEW_MARK_LEFT, NEW_BOTTOM, NEW_TOP, NEW_MARK_HALF);
        build_mark("login_new_mark_right", NEW_MARK_RIGHT, NEW_BOTTOM, NEW_TOP, NEW_MARK_HALF);

        var prompt = PlayUi.text("login_new_label", host, NEW_TEXT, LoginTheme.FIELD_ERROR_SIZE, LoginTheme.text_primary, TextAnchor.MiddleRight);
        place(prompt.rectTransform, NEW_LABEL_LEFT, NEW_BOTTOM, NEW_LABEL_SPLIT - NEW_LABEL_GAP, NEW_TOP);
        track(prompt, NEW_RATIO, 0f);

        var link = build_link("login_create_link", CREATE_TEXT, LoginTheme.gold, NEW_LABEL_SPLIT + NEW_LABEL_GAP, NEW_BOTTOM, NEW_LABEL_RIGHT, NEW_TOP, NEW_RATIO, TextAnchor.MiddleLeft, 0f, () => LoginState.change_mode(LoginMode.Register));
        locked_groups.Add(link.gameObject.AddComponent<CanvasGroup>());
    }

    Text build_link(string name, string value, Color color, float left, float bottom, float right, float top, float ratio, TextAnchor anchor, float align, UnityEngine.Events.UnityAction on_click)
    {
        var label = PlayUi.text(name, host, value, LoginTheme.FIELD_ERROR_SIZE, color, anchor);
        label.raycastTarget = true;
        place(label.rectTransform, left, bottom, right, top);

        var underline = PlayUi.image(name + "_underline", label.rectTransform, null);
        underline.color = color;

        var button = label.gameObject.AddComponent<Button>();
        button.targetGraphic = label;
        LobbyUi.no_transition(button);
        button.onClick.AddListener(on_click);
        // Hover feedback is the tint + underline; no rectangle glow (it would wash
        // over the text's padding and the card background).
        LoginFocusGlow.attach(button, label, color, LoginTheme.gold_soft, null, LoginTheme.clear);

        track(label, ratio, align);
        fit_items[fit_items.Count - 1].underline = underline.rectTransform;
        return label;
    }

    void build_line(string name, float left, float right, float bottom, float top)
    {
        var line = PlayUi.image(name, host, null);
        line.color = PlayTheme.with_alpha(LoginTheme.gold, LINE_ALPHA);
        var center = slot((bottom + top) * 0.5f);
        PlayUi.place(line.rectTransform, new Vector2(left, center), new Vector2(right, center), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, LINE_THICKNESS));
    }

    void build_mark(string name, float center_x, float bottom, float top, float half_width)
    {
        var mark = LoginUi.icon(name, host, LobbyUi.ICON_RHOMBUS_PATH, LoginTheme.gold);
        var center = slot((bottom + top) * 0.5f);
        PlayUi.place(mark.rectTransform, new Vector2(center_x - half_width, center), new Vector2(center_x + half_width, center), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        var fitter = mark.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
        fitter.aspectRatio = 1f;
    }

    void track(Text label, float ratio, float align)
    {
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        fit_items.Add(new FitItem { label = label, ratio = ratio, align = align });
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
            wide = item.label.preferredWidth;
        }

        if (item.underline == null) return;

        item.underline.anchorMin = new Vector2(item.align, 0f);
        item.underline.anchorMax = new Vector2(item.align, 0f);
        item.underline.pivot = new Vector2(item.align, 0.5f);
        item.underline.sizeDelta = new Vector2(Mathf.Min(wide, rect.width), UNDERLINE_THICKNESS);
        item.underline.anchoredPosition = new Vector2(0f, rect.height * 0.5f - size * UNDERLINE_DROP);
        var picture = item.underline.GetComponent<Image>();
        if (picture != null) picture.color = item.label.color;
    }

    bool resolve_fields()
    {
        if (identifier_field != null && password_field != null) return true;

        var form = host != null && host.parent != null ? host.parent.Find(FORM_NAME) : null;
        if (form == null) return false;

        var found = form.GetComponentsInChildren<LoginFieldView>(true);
        if (found.Length < 2) return false;

        identifier_field = found[0];
        password_field = found[1];
        return true;
    }

    void refresh(bool busy)
    {
        if (submit_button != null) submit_button.interactable = !busy;
        if (submit_label != null) submit_label.text = busy ? LoginTheme.LOADING_TEXT : LOGIN_TEXT;

        for (var i = 0; i < locked_groups.Count; i++)
        {
            if (locked_groups[i] == null) continue;
            locked_groups[i].interactable = !busy;
            locked_groups[i].alpha = busy ? DISABLED_ALPHA : 1f;
        }

        if (resolve_fields())
        {
            identifier_field.set_enabled(!busy);
            password_field.set_enabled(!busy);
        }

        if (busy) return;

        for (var i = 0; i < spinners.Count; i++)
        {
            if (spinners[i] != null) spinners[i].localRotation = Quaternion.identity;
        }
    }

    bool can_act()
    {
        return this != null && isActiveAndEnabled && !LoginState.is_busy && LoginState.mode == LoginMode.SignIn;
    }

    bool is_current(int ticket)
    {
        return this != null && ticket == request_ticket;
    }

    void begin()
    {
        owns_busy = true;
        LoginState.set_busy(true);
    }

    void end()
    {
        if (!owns_busy) return;

        owns_busy = false;
        LoginState.set_busy(false);
    }

    void try_login()
    {
        if (!can_act()) return;

        if (!resolve_fields())
        {
            LoginFeedback.toast(FORM_NOT_READY, true);
            return;
        }

        var identifier = identifier_field.get_value().Trim();
        var password = password_field.get_value();
        identifier_field.set_error(identifier.Length == 0 ? EMPTY_IDENTIFIER : null);
        password_field.set_error(password.Length == 0 ? EMPTY_PASSWORD : null);
        if (identifier.Length == 0 || password.Length == 0) return;

        var ticket = ++request_ticket;
        begin();
        LoginService.login(identifier, password, () => finish_ok(ticket, identifier), reason => finish_fail(ticket, reason));
    }

    void try_guest()
    {
        if (!can_act()) return;

        LoginGuest.open(LoginMode.SignIn);
    }

    void finish_ok(int ticket, string name)
    {
        if (!is_current(ticket)) return;

        end();
        if (!LoginSession.is_logged_in) LoginSession.set(name, string.Empty, string.Empty, false);
        LoginOverlay.succeed();
    }

    void finish_fail(int ticket, string reason)
    {
        if (!is_current(ticket)) return;

        end();
        LoginFeedback.toast(string.IsNullOrEmpty(reason) ? GENERIC_FAIL : reason, true);
    }
}
