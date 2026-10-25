using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// The guest form is its own page in the shared login card, so it reuses the overlay's
// card, header, field, plate and link modules instead of scaling a second card.
public static class LoginGuest
{
    const string EMPTY_ERROR = "Enter a name.";
    const string LENGTH_ERROR = "Use 3 to 20 characters.";
    const string SPACE_ERROR = "Use one space between words.";
    const string CHAR_ERROR = "Use letters, numbers, spaces, _ or -.";
    const int NAME_MIN = 3;
    const int NAME_MAX = 20;

    static LoginMode return_mode = LoginMode.SignIn;

    public static void open(LoginMode from)
    {
        if (LoginState.is_busy) return;

        return_mode = from;
        LoginState.change_mode(LoginMode.Guest);
    }

    public static void back()
    {
        LoginState.change_mode(return_mode);
    }

    public static void build(RectTransform root)
    {
        var holder = LoginUi.view("login_guest_page", root, LoginMode.Guest);
        if (holder == null) return;

        // Stretch to full inner height like the other pages so the rows share the grid.
        LobbyUi.place_fraction(holder, 0f, 0f, 1f, 1f / (1f - LoginTheme.HEADER_RATIO));

        holder.gameObject.AddComponent<LoginGuestView>().setup(holder);
    }

    public static string name_error(string name)
    {
        if (string.IsNullOrEmpty(name)) return EMPTY_ERROR;
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

public class LoginGuestView : MonoBehaviour
{
    const string ICON_PATH = "Icons/Lobby/IconAvatarDefault";
    const string HINT_TEXT = "Pick a name to play as a guest.";
    const string PLACEHOLDER_TEXT = "GUEST NAME";
    const string SUBMIT_TEXT = "CONTINUE";
    const string BUSY_TEXT = "CONNECTING";
    const string LINK_TEXT = "Cancel";
    const string FAIL_TEXT = "Could not sign in as a guest. Try again.";

    // One field, one plate, one link, on the same grid as the forgot page.
    const float HINT_LEFT = 0.02f;
    const float HINT_RIGHT = 0.98f;
    const float HINT_BOTTOM = 0.700f;
    const float HINT_TOP = 0.750f;
    const float FIELD_BOTTOM = 0.587f;
    const float FIELD_TOP = 0.690f;
    const float SUBMIT_BOTTOM = 0.440f;
    const float SUBMIT_TOP = 0.540f;
    const float LINK_BOTTOM = 0.300f;
    const float LINK_TOP = 0.350f;
    const int HINT_SIZE = 22;
    const int TEXT_MIN_SIZE = 12;

    LoginFieldView field;
    LoginSubmitButton submit_button;
    Button link_button;
    bool ready;
    bool was_busy;

    public void setup(RectTransform holder)
    {
        var hint = PlayUi.text("login_guest_hint", holder, HINT_TEXT, HINT_SIZE, LoginTheme.text_placeholder, TextAnchor.MiddleCenter);
        hint.resizeTextForBestFit = true;
        hint.resizeTextMinSize = TEXT_MIN_SIZE;
        hint.resizeTextMaxSize = HINT_SIZE;
        hint.verticalOverflow = VerticalWrapMode.Truncate;
        LobbyUi.place_fraction(hint.rectTransform, HINT_LEFT, HINT_BOTTOM, HINT_RIGHT, HINT_TOP);

        var field_slot = PlayUi.rect("login_guest_field_slot", holder);
        LobbyUi.place_fraction(field_slot, 0f, FIELD_BOTTOM, 1f, FIELD_TOP);
        field = LoginField.create(field_slot, PLACEHOLDER_TEXT, ICON_PATH, false);
        var input = field.root.GetComponent<InputField>();
        if (input != null) input.onValueChanged.AddListener(value => field.set_error(null));

        var submit_slot = PlayUi.rect("login_guest_submit_slot", holder);
        LobbyUi.place_fraction(submit_slot, 0f, SUBMIT_BOTTOM, 1f, SUBMIT_TOP);
        submit_button = LoginSubmitButton.create(submit_slot, "login_guest_submit", SUBMIT_TEXT, BUSY_TEXT, submit);

        var link_slot = PlayUi.rect("login_guest_link_slot", holder);
        LobbyUi.place_fraction(link_slot, 0f, LINK_BOTTOM, 1f, LINK_TOP);
        link_button = LoginTextLink.create(link_slot, null, LINK_TEXT, LoginGuest.back);

        ready = true;
        apply_busy(LoginState.is_busy);
    }

    void OnEnable()
    {
        LoginState.on_submit += on_submit_requested;
        LoginState.on_busy_changed += apply_busy;
        if (!ready) return;

        apply_busy(LoginState.is_busy);
        select_field();
    }

    void OnDisable()
    {
        LoginState.on_submit -= on_submit_requested;
        LoginState.on_busy_changed -= apply_busy;
    }

    void on_submit_requested()
    {
        if (LoginState.mode != LoginMode.Guest) return;
        submit();
    }

    void select_field()
    {
        var event_system = EventSystem.current;
        if (event_system == null || field == null) return;

        event_system.SetSelectedGameObject(field.root.gameObject);
    }

    void apply_busy(bool busy)
    {
        if (!ready) return;

        field.set_enabled(!busy);
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

        var name = LobbyUi.sanitize(field.get_value());
        var problem = LoginGuest.name_error(name);
        field.set_error(problem);
        if (problem != null)
        {
            select_field();
            return;
        }

        LoginState.set_busy(true);
        LoginService.guest(name, () => on_entered(name), on_failed);
    }

    void on_entered(string name)
    {
        if (this == null) return;

        LoginState.set_busy(false);
        if (!LoginSession.is_logged_in) LoginSession.set(name, string.Empty, string.Empty, true);
        LoginOverlay.succeed();
    }

    void on_failed(string reason)
    {
        if (this == null) return;

        LoginState.set_busy(false);
        LoginFeedback.toast(string.IsNullOrEmpty(reason) ? FAIL_TEXT : reason, true);
    }
}
