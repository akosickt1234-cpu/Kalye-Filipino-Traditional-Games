using UnityEngine;
using UnityEngine.UI;

public class LoginFieldView : MonoBehaviour
{
    InputField input;
    Text error_text;
    Text placeholder;
    Button eye_button;
    Image eye_icon;
    CanvasGroup group;
    bool is_password;
    bool is_revealed;

    public RectTransform root { get; private set; }

    public void setup(RectTransform field_root, InputField field_input, Text field_error, Text field_placeholder, Button eye, Image eye_image, bool password)
    {
        root = field_root;
        input = field_input;
        error_text = field_error;
        placeholder = field_placeholder;
        eye_button = eye;
        eye_icon = eye_image;
        is_password = password;
        group = gameObject.GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();

        if (eye_button != null) eye_button.onClick.AddListener(toggle_reveal);
        set_error(null);
        apply_reveal();
    }

    public string get_value()
    {
        return input != null ? input.text : string.Empty;
    }

    public void set_error(string message)
    {
        var has_error = !string.IsNullOrEmpty(message);
        if (error_text == null) return;

        error_text.text = has_error ? message : string.Empty;
        error_text.gameObject.SetActive(has_error);
    }

    public void set_enabled(bool value)
    {
        if (input != null) input.interactable = value;
        if (eye_button != null) eye_button.interactable = value;
        if (group == null) return;

        group.interactable = value;
        group.alpha = value ? 1f : LoginTheme.FIELD_DISABLED_ALPHA;
    }

    public void clear()
    {
        if (input != null) input.text = string.Empty;
        is_revealed = false;
        apply_reveal();
        set_error(null);
    }

    void Update()
    {
        var focused = input != null && input.isFocused;
        // The field frame is baked art, so focus feedback lives in the placeholder;
        // it follows the gold accent while the field is clicked.
        var placeholder_color = focused ? LoginTheme.gold_soft : LoginTheme.text_placeholder;
        if (placeholder != null && placeholder.color != placeholder_color) placeholder.color = placeholder_color;
    }

    void toggle_reveal()
    {
        is_revealed = !is_revealed;
        apply_reveal();
    }

    void apply_reveal()
    {
        if (input == null) return;

        if (is_password)
        {
            input.contentType = is_revealed ? InputField.ContentType.Standard : InputField.ContentType.Password;
            input.ForceLabelUpdate();
        }

        if (eye_icon == null) return;

        var sprite = PlayUi.sprite(is_revealed ? LoginTheme.ICON_EYE_OFF_PATH : LoginTheme.ICON_EYE_PATH);
        eye_icon.sprite = sprite;
        eye_icon.enabled = sprite != null;
    }
}
