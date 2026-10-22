using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class LoginForgot
{
    public static void build(RectTransform root)
    {
        var holder = LoginUi.view("login_forgot", root, LoginMode.Forgot);
        if (holder == null) return;

        holder.gameObject.AddComponent<LoginForgotView>().setup(holder);
    }
}

public class LoginForgotView : MonoBehaviour
{
    const string SUBMIT_TEXT = "SEND RESET LINK";
    const string BUSY_TEXT = "SENDING";
    const string LINK_TEXT = "Back to login";
    const string SENT_TEXT = "If that email has an account, a reset link is on its way.";
    const string FAIL_TEXT = "Could not send the reset link. Try again.";
    const float FIELD_HEIGHT = 0.15f;
    const float FIELD_TOP = 0.80f;
    const float SUBMIT_BOTTOM = 0.34f;
    const float SUBMIT_TOP = 0.56f;
    const float LINK_BOTTOM = 0.2f;
    const float LINK_TOP = 0.30f;

    LoginFieldView email_field;
    LoginSubmitButton submit_button;
    Button link_button;
    bool ready;
    bool was_busy;

    public void setup(RectTransform holder)
    {
        var field_slot = PlayUi.rect("login_forgot_email", holder);
        LobbyUi.place_fraction(field_slot, 0f, FIELD_TOP - FIELD_HEIGHT, 1f, FIELD_TOP);
        email_field = LoginField.create(field_slot, "EMAIL", LoginTheme.ICON_EMAIL_PATH, false);
        var input = email_field.root.GetComponent<InputField>();
        if (input != null) input.onValueChanged.AddListener(value => email_field.set_error(null));

        var submit_slot = PlayUi.rect("login_forgot_submit_slot", holder);
        LobbyUi.place_fraction(submit_slot, 0f, SUBMIT_BOTTOM, 1f, SUBMIT_TOP);
        submit_button = LoginSubmitButton.create(submit_slot, "login_forgot_submit", SUBMIT_TEXT, BUSY_TEXT, submit);

        var link_slot = PlayUi.rect("login_forgot_link_slot", holder);
        LobbyUi.place_fraction(link_slot, 0f, LINK_BOTTOM, 1f, LINK_TOP);
        link_button = LoginTextLink.create(link_slot, null, LINK_TEXT, back_to_sign_in);

        ready = true;
        apply_busy(LoginState.is_busy);
    }

    void OnEnable()
    {
        LoginState.on_submit += on_submit_requested;
        LoginState.on_busy_changed += apply_busy;
        if (!ready) return;

        apply_busy(LoginState.is_busy);
        select_email();
    }

    void OnDisable()
    {
        LoginState.on_submit -= on_submit_requested;
        LoginState.on_busy_changed -= apply_busy;
    }

    void select_email()
    {
        var event_system = EventSystem.current;
        if (event_system == null) return;

        event_system.SetSelectedGameObject(email_field.root.gameObject);
    }

    void on_submit_requested()
    {
        if (LoginState.mode != LoginMode.Forgot) return;
        submit();
    }

    void back_to_sign_in()
    {
        LoginState.set_mode(LoginMode.SignIn);
    }

    void apply_busy(bool busy)
    {
        if (!ready) return;

        email_field.set_enabled(!busy);
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

        var email = LobbyUi.sanitize(email_field.get_value());
        var problem = LoginRegister.email_error(email);
        email_field.set_error(problem);
        if (problem != null)
        {
            select_email();
            return;
        }

        LoginState.set_busy(true);
        LoginService.forgot(email, on_sent, on_failed);
    }

    void on_sent()
    {
        if (this == null) return;

        LoginState.set_busy(false);
        LoginFeedback.toast(SENT_TEXT, false);
    }

    void on_failed(string reason)
    {
        if (this == null) return;

        LoginState.set_busy(false);
        LoginFeedback.toast(string.IsNullOrEmpty(reason) ? FAIL_TEXT : reason, true);
    }
}
