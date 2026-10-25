using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginToast : MonoBehaviour
{
    const float WIDTH_IN_HEIGHTS = 1.1f;
    const float WIDTH_MIN = 0.4f;
    const float WIDTH_MAX = 0.92f;
    const float BOTTOM = 0.04f;
    const float TOP = 0.13f;
    const float BORDER = 2f;
    const float SECONDS = 4f;
    const float RETRY_SECONDS = 10f;
    const float FADE_IN_SECONDS = 0.15f;
    const float FADE_OUT_SECONDS = 0.4f;
    const float TEXT_LEFT = 0.04f;
    const float TEXT_RIGHT = 0.96f;
    const float TEXT_RIGHT_RETRY = 0.66f;
    const float RETRY_LEFT = 0.7f;
    const float RETRY_RIGHT = 0.97f;
    const float RETRY_BOTTOM = 0.14f;
    const float RETRY_TOP = 0.86f;
    const float RETRY_GLOW_ALPHA = 0.55f;
    const int TEXT_SIZE = 26;
    const int TEXT_MIN_SIZE = 12;
    const int RETRY_TEXT_SIZE = 24;
    const string RETRY_TEXT = "RETRY";

    RectTransform strip;
    CanvasGroup group;
    Action retry_action;
    float lifetime;
    float elapsed;
    float last_aspect;

    public string message { get; private set; }
    public bool has_retry { get; private set; }

    public static LoginToast create(RectTransform parent, string text, Color accent, Action retry)
    {
        var strip_rect = PlayUi.rect("login_toast", parent);
        strip_rect.SetAsLastSibling();
        var toast = strip_rect.gameObject.AddComponent<LoginToast>();
        toast.strip = strip_rect;
        toast.message = text;
        toast.retry_action = retry;
        toast.has_retry = retry != null;
        toast.lifetime = retry != null ? RETRY_SECONDS : SECONDS;
        toast.group = strip_rect.gameObject.AddComponent<CanvasGroup>();
        toast.group.alpha = 0f;
        toast.group.blocksRaycasts = retry != null;
        toast.group.interactable = retry != null;
        toast.apply_layout();

        var pill = LoginUi.rounded_box("login_toast_pill", strip_rect, accent, LoginTheme.card_fill, BORDER, false);
        PlayUi.stretch(pill.rectTransform);

        var label = PlayUi.text("login_toast_text", pill.rectTransform, text, TEXT_SIZE, LoginTheme.text_primary, TextAnchor.MiddleCenter);
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = TEXT_MIN_SIZE;
        label.resizeTextMaxSize = TEXT_SIZE;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        LobbyUi.place_fraction(label.rectTransform, TEXT_LEFT, 0f, retry != null ? TEXT_RIGHT_RETRY : TEXT_RIGHT, 1f);

        if (retry != null) toast.build_retry(pill.rectTransform);
        return toast;
    }

    void build_retry(RectTransform pill)
    {
        var ring = LoginUi.rounded_box("login_toast_retry", pill, LoginTheme.gold, LoginTheme.close_fill, BORDER, true);
        LobbyUi.place_fraction(ring.rectTransform, RETRY_LEFT, RETRY_BOTTOM, RETRY_RIGHT, RETRY_TOP);

        var glow = LobbyUi.rim_glow("login_toast_retry_glow", ring.rectTransform, LoginTheme.clear);
        glow.rectTransform.SetAsFirstSibling();

        var label = PlayUi.text("login_toast_retry_text", ring.rectTransform, RETRY_TEXT, RETRY_TEXT_SIZE, PlayTheme.text, TextAnchor.MiddleCenter);
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = TEXT_MIN_SIZE;
        label.resizeTextMaxSize = RETRY_TEXT_SIZE;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        PlayUi.stretch(label.rectTransform);

        var button = ring.gameObject.AddComponent<Button>();
        button.targetGraphic = ring;
        LobbyUi.no_transition(button);
        button.onClick.AddListener(on_retry);

        LoginFocusGlow.attach(button, ring, LoginTheme.gold, LoginTheme.gold_soft, glow, PlayTheme.with_alpha(LoginTheme.gold, RETRY_GLOW_ALPHA));
    }

    void on_retry()
    {
        var action = retry_action;
        retry_action = null;
        gameObject.SetActive(false);
        Destroy(gameObject);
        if (action != null) action();
    }

    void Update()
    {
        var aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        if (!Mathf.Approximately(aspect, last_aspect)) apply_layout();

        if (!focused_inside()) elapsed += Time.unscaledDeltaTime;
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        var fade_in = Mathf.Clamp01(elapsed / FADE_IN_SECONDS);
        var fade_out = Mathf.Clamp01((lifetime - elapsed) / FADE_OUT_SECONDS);
        group.alpha = Mathf.Min(fade_in, fade_out);
    }

    void OnDestroy()
    {
        retry_action = null;
    }

    bool focused_inside()
    {
        var event_system = EventSystem.current;
        var selected = event_system != null ? event_system.currentSelectedGameObject : null;
        return selected != null && selected.transform.IsChildOf(transform);
    }

    void apply_layout()
    {
        last_aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        var width = Mathf.Clamp(WIDTH_IN_HEIGHTS / last_aspect, WIDTH_MIN, WIDTH_MAX);
        var left = 0.5f - width * 0.5f;
        LobbyUi.place_fraction(strip, left, BOTTOM, left + width, TOP);
    }
}
