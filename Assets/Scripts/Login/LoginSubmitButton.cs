using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginSubmitButton : MonoBehaviour
{
    const float ART_ASPECT = 512f / 111f;
    const float DISABLED_ALPHA = 0.6f;
    const float LABEL_LEFT = 0.14f;
    const float LABEL_RIGHT = 0.86f;
    const float DOT_RATE = 3f;
    const int DOT_STEPS = 4;
    const int LABEL_MIN_SIZE = 14;
    const int LABEL_MAX_SIZE = 32;

    Button button;
    Image face;
    Text label;
    CanvasGroup group;
    PlayButtonFx fx;
    string idle_text;
    string busy_text;
    bool busy;
    int dots = -1;

    public Button control
    {
        get { return button; }
    }

    public bool is_busy
    {
        get { return busy; }
    }

    public static LoginSubmitButton create(RectTransform slot, string name, string idle_label, string busy_label, UnityAction on_click)
    {
        if (slot == null) return null;

        var rect = PlayUi.rect(name, slot);
        var fitter = rect.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = ART_ASPECT;

        var view = rect.gameObject.AddComponent<LoginSubmitButton>();
        view.build(rect, idle_label, busy_label, on_click);
        return view;
    }

    public void set_busy(bool value)
    {
        busy = value;
        dots = -1;
        if (button == null) return;

        button.interactable = !value;
        face.raycastTarget = !value;
        fx.enabled = !value;
        group.alpha = value ? DISABLED_ALPHA : 1f;
        label.text = value ? busy_text : idle_text;
    }

    void build(RectTransform rect, string idle_label, string busy_label, UnityAction on_click)
    {
        idle_text = idle_label;
        busy_text = busy_label;
        group = gameObject.AddComponent<CanvasGroup>();

        var art = PlayUi.sprite(LoginTheme.BUTTON_GOLD_PATH);
        face = gameObject.AddComponent<Image>();
        face.sprite = art != null ? art : PlayUi.pill();
        face.type = art != null ? Image.Type.Simple : Image.Type.Sliced;
        face.color = art != null ? PlayTheme.white : LoginTheme.gold;
        face.raycastTarget = true;

        var glow_rect = PlayUi.rect("login_submit_glow", rect);
        PlayUi.stretch(glow_rect);
        var glow = glow_rect.gameObject.AddComponent<Image>();
        glow.sprite = face.sprite;
        glow.type = face.type;
        glow.color = LoginTheme.glow;
        glow.raycastTarget = false;
        var glow_group = glow_rect.gameObject.AddComponent<CanvasGroup>();
        glow_group.alpha = 0f;
        glow_group.blocksRaycasts = false;
        glow_group.interactable = false;

        label = PlayUi.text("login_submit_label", rect, idle_text, LABEL_MAX_SIZE, PlayTheme.label_brown, TextAnchor.MiddleCenter);
        label.fontStyle = FontStyle.Bold;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = LABEL_MIN_SIZE;
        label.resizeTextMaxSize = LABEL_MAX_SIZE;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        LobbyUi.place_fraction(label.rectTransform, LABEL_LEFT, 0f, LABEL_RIGHT, 1f);

        button = gameObject.AddComponent<Button>();
        button.targetGraphic = face;
        LobbyUi.no_transition(button);
        if (on_click != null) button.onClick.AddListener(on_click);

        fx = gameObject.AddComponent<PlayButtonFx>();
        fx.setup(rect, glow_group);
    }

    void Update()
    {
        if (!busy || label == null) return;

        var step = (int)(Time.unscaledTime * DOT_RATE) % DOT_STEPS;
        if (step == dots) return;

        dots = step;
        label.text = busy_text + new string('.', step);
    }
}
