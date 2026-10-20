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
    const string BUTTON_PATH = "Buttons/Play/BtnPlayGold";
    const string START_TEXT = "START MATCH";
    const string WAITING_TEXT = "Waiting for host";
    const string CANCEL_TEXT = "CANCEL";
    const float BUTTON_ASPECT = 768f / 217f;
    const float ORNAMENT_HEIGHT = 0.3f;
    const float ORNAMENT_LEFT_START = 0.02f;
    const float ORNAMENT_LEFT_END = 0.20f;
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
    Text label;
    Text cancel_label;
    CanvasGroup button_group;
    PlayButtonFx button_fx;

    public void setup(RectTransform start_bar)
    {
        build_ornament(start_bar, "ornament_left", ORNAMENT_LEFT_START, ORNAMENT_LEFT_END, false);
        build_ornament(start_bar, "ornament_right", 1f - ORNAMENT_LEFT_END, 1f - ORNAMENT_LEFT_START, true);
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

    static void build_ornament(RectTransform parent, string name, float x_min, float x_max, bool mirrored)
    {
        var ornament = LobbyOrnament.diamond_tail(parent, name, PlayTheme.gold, !mirrored);
        ornament.anchorMin = new Vector2(x_min, 0.5f - ORNAMENT_HEIGHT * 0.5f);
        ornament.anchorMax = new Vector2(x_max, 0.5f + ORNAMENT_HEIGHT * 0.5f);
        ornament.pivot = new Vector2(0.5f, 0.5f);
        ornament.offsetMin = Vector2.zero;
        ornament.offsetMax = Vector2.zero;
    }

    void build_button(RectTransform start_bar)
    {
        var slot = PlayUi.rect("start_slot", start_bar);
        LobbyUi.place_fraction(slot, SLOT_LEFT, 0f, SLOT_RIGHT, 1f);

        var button_rect = PlayUi.rect("start_button", slot);
        var fitter = button_rect.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = BUTTON_ASPECT;

        button_group = button_rect.gameObject.AddComponent<CanvasGroup>();

        var art = PlayUi.sprite(BUTTON_PATH);
        face = button_rect.gameObject.AddComponent<Image>();
        face.sprite = art != null ? art : PlayUi.pill();
        face.type = art != null ? Image.Type.Simple : Image.Type.Sliced;
        face.color = art != null ? PlayTheme.white : PlayTheme.gold;
        face.raycastTarget = true;

        var glow_rect = PlayUi.rect("start_glow", button_rect);
        PlayUi.stretch(glow_rect);
        var glow = glow_rect.gameObject.AddComponent<Image>();
        glow.sprite = face.sprite;
        glow.type = face.type;
        glow.color = PlayTheme.glow;
        glow.raycastTarget = false;
        var glow_group = glow_rect.gameObject.AddComponent<CanvasGroup>();
        glow_group.alpha = 0f;
        glow_group.blocksRaycasts = false;
        glow_group.interactable = false;

        label = PlayUi.text("start_label", button_rect, START_TEXT, LABEL_MAX_SIZE, PlayTheme.label_brown, TextAnchor.MiddleCenter);
        fit_text(label, LABEL_MIN_SIZE, LABEL_MAX_SIZE);

        cancel_label = PlayUi.text("cancel_label", button_rect, CANCEL_TEXT, CANCEL_MAX_SIZE, PlayTheme.label_brown, TextAnchor.MiddleCenter);
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

        if (searching) LobbyUi.place_fraction(label.rectTransform, LABEL_SIDE, SEARCH_LABEL_BOTTOM, 1f - LABEL_SIDE, SEARCH_LABEL_TOP);
        else LobbyUi.place_fraction(label.rectTransform, LABEL_SIDE, 0f, 1f - LABEL_SIDE, 1f);

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
