using UnityEngine;
using UnityEngine.UI;

public static class LobbyStartBar
{
    public static void build(RectTransform start_bar)
    {
        if (start_bar == null) return;

        var view = start_bar.gameObject.AddComponent<LobbyStartBarView>();
        view.setup(start_bar);
    }
}

public class LobbyStartBarView : MonoBehaviour
{
    const string START_TEXT = "START MATCH";
    const string WAITING_TEXT = "Waiting for host";
    const string CANCEL_TEXT = "CANCEL";
    const float BUTTON_ASPECT = 768f / 217f;
    const float BUTTON_SCALE = 1.6f;
    const float BORDER_WIDTH = 3f;
    const float GLOW_BLUR = 12f;
    const float ICON_LEFT = 0.10f;
    const float ICON_RIGHT = 0.26f;
    const float ICON_BOTTOM = 0.22f;
    const float ICON_TOP = 0.78f;
    const float LABEL_LEFT_WITH_ICON = 0.28f;
    const float SLOT_LEFT = 0.22f;
    const float SLOT_RIGHT = 0.78f;
    const float LABEL_SIDE = 0.12f;
    const float SEARCH_LABEL_BOTTOM = 0.36f;
    const float SEARCH_LABEL_TOP = 0.92f;
    const float CANCEL_BOTTOM = 0.10f;
    const float CANCEL_TOP = 0.40f;
    const float DISABLED_ALPHA = 0.55f;
    const int LABEL_MIN_SIZE = 16;
    const int LABEL_MAX_SIZE = 46;
    const int CANCEL_MIN_SIZE = 10;
    const int CANCEL_MAX_SIZE = 24;

    Button button;
    Image face;
    Image swords;
    Text label;
    Text cancel_label;
    CanvasGroup button_group;
    PlayButtonFx button_fx;

    public void setup(RectTransform start_bar)
    {
        build_button(start_bar);
        refresh();
    }

    void OnEnable()
    {
        LobbyState.search_changed += refresh;
        LobbyState.players_changed += refresh;
    }

    void OnDisable()
    {
        LobbyState.search_changed -= refresh;
        LobbyState.players_changed -= refresh;
    }

    void build_button(RectTransform start_bar)
    {
        var slot = PlayUi.rect("start_slot", start_bar);
        LobbyUi.place_fraction(slot, SLOT_LEFT, 0f, SLOT_RIGHT, 1f);

        var frame = PlayUi.rect("start_frame", slot);
        frame.localScale = new Vector3(BUTTON_SCALE, BUTTON_SCALE, 1f);
        var fitter = frame.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = BUTTON_ASPECT;

        button_group = frame.gameObject.AddComponent<CanvasGroup>();

        var static_glow = LobbyUi.glow("start_glow_static", frame, PlayTheme.start_glow, PlayTheme.start_glow.a, GLOW_BLUR);
        static_glow.raycastTarget = false;

        var button_rect = PlayUi.rect("start_button", frame);
        PlayUi.stretch(button_rect);

        face = button_rect.gameObject.AddComponent<Image>();
        face.sprite = PlayUi.pill();
        face.type = Image.Type.Sliced;
        face.color = PlayTheme.start_border;
        face.raycastTarget = true;

        var fill_rect = PlayUi.rect("start_fill", button_rect);
        PlayUi.stretch(fill_rect);
        fill_rect.offsetMin = new Vector2(BORDER_WIDTH, BORDER_WIDTH);
        fill_rect.offsetMax = new Vector2(-BORDER_WIDTH, -BORDER_WIDTH);
        var fill_mask = fill_rect.gameObject.AddComponent<Image>();
        fill_mask.sprite = PlayUi.pill();
        fill_mask.type = Image.Type.Sliced;
        fill_mask.raycastTarget = false;
        var mask = fill_rect.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        var gradient = LobbyUi.gradient_image("start_gradient", fill_rect, PlayTheme.start_stops, true);
        gradient.raycastTarget = false;

        var glow_rect = PlayUi.rect("start_glow", button_rect);
        PlayUi.stretch(glow_rect);
        var glow = glow_rect.gameObject.AddComponent<Image>();
        glow.sprite = face.sprite;
        glow.type = face.type;
        glow.color = PlayTheme.start_glow;
        glow.raycastTarget = false;
        var glow_group = glow_rect.gameObject.AddComponent<CanvasGroup>();
        glow_group.alpha = 0f;
        glow_group.blocksRaycasts = false;
        glow_group.interactable = false;

        var swords_sprite = PlayUi.sprite(LobbyUi.ICON_SWORDS_PATH);
        swords = PlayUi.image("start_swords", button_rect, swords_sprite, false);
        swords.preserveAspect = true;
        swords.enabled = swords_sprite != null;
        swords.color = PlayTheme.start_text;
        LobbyUi.place_fraction(swords.rectTransform, ICON_LEFT, ICON_BOTTOM, ICON_RIGHT, ICON_TOP);

        label = PlayUi.text("start_label", button_rect, START_TEXT, LABEL_MAX_SIZE, PlayTheme.start_text, TextAnchor.MiddleCenter);
        fit_text(label, LABEL_MIN_SIZE, LABEL_MAX_SIZE);

        cancel_label = PlayUi.text("cancel_label", button_rect, CANCEL_TEXT, CANCEL_MAX_SIZE, PlayTheme.start_text, TextAnchor.MiddleCenter);
        LobbyUi.place_fraction(cancel_label.rectTransform, LABEL_SIDE, CANCEL_BOTTOM, 1f - LABEL_SIDE, CANCEL_TOP);
        fit_text(cancel_label, CANCEL_MIN_SIZE, CANCEL_MAX_SIZE);

        button = button_rect.gameObject.AddComponent<Button>();
        button.targetGraphic = face;
        LobbyUi.no_transition(button);
        button.onClick.AddListener(on_click);

        button_fx = button_rect.gameObject.AddComponent<PlayButtonFx>();
        button_fx.setup(button_rect, glow_group);
    }

    void on_click()
    {
        if (!LobbyState.in_lobby || !LobbyState.is_host) return;

        if (LobbyState.searching) LobbyState.cancel_search();
        else LobbyState.start_search();
    }

    void refresh()
    {
        if (button == null) return;

        var host = LobbyState.in_lobby && LobbyState.is_host;
        var searching = host && LobbyState.searching;

        if (!host) label.text = WAITING_TEXT;
        else if (searching) label.text = "SEARCHING " + LobbyState.opponents_found + "/" + LobbyState.SEARCH_TARGET;
        else label.text = START_TEXT;

        var show_swords = host && !searching && swords.sprite != null;
        swords.gameObject.SetActive(show_swords);

        if (searching) LobbyUi.place_fraction(label.rectTransform, LABEL_SIDE, SEARCH_LABEL_BOTTOM, 1f - LABEL_SIDE, SEARCH_LABEL_TOP);
        else LobbyUi.place_fraction(label.rectTransform, show_swords ? LABEL_LEFT_WITH_ICON : LABEL_SIDE, 0f, 1f - LABEL_SIDE, 1f);

        cancel_label.gameObject.SetActive(searching);

        button.interactable = host;
        face.raycastTarget = host;
        button_fx.enabled = host;
        button_group.alpha = host ? 1f : DISABLED_ALPHA;

        if (host) PlayFocus.default_target = button.gameObject;
        else if (PlayFocus.default_target == button.gameObject) PlayFocus.default_target = null;
    }

    static void fit_text(Text target, int min_size, int max_size)
    {
        target.resizeTextForBestFit = true;
        target.resizeTextMinSize = min_size;
        target.resizeTextMaxSize = max_size;
        target.verticalOverflow = VerticalWrapMode.Truncate;
    }
}
