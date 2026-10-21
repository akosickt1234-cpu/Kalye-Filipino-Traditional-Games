using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class LoginTextLink
{
    const float ORNAMENT_START = 0f;
    const float ORNAMENT_END = 0.14f;
    const float ORNAMENT_HEIGHT = 0.3f;
    const float ROW_LEFT = 0.16f;
    const float ROW_RIGHT = 0.84f;
    const float SPACING = 8f;
    const float UNDERLINE_THICKNESS = 2f;
    const float UNDERLINE_DROP = 14f;
    const float GLOW_ALPHA = 0.45f;
    const int TEXT_SIZE = 22;

    public static Button create(RectTransform slot, string prefix, string link, UnityAction on_click)
    {
        if (slot == null) return null;

        add_ornament(slot, "login_link_ornament_left", ORNAMENT_START, ORNAMENT_END, true);
        add_ornament(slot, "login_link_ornament_right", 1f - ORNAMENT_END, 1f - ORNAMENT_START, false);

        var row = PlayUi.rect("login_link_row", slot);
        LobbyUi.place_fraction(row, ROW_LEFT, 0f, ROW_RIGHT, 1f);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = SPACING;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;
        row.gameObject.AddComponent<LoginLinkFit>();

        if (!string.IsNullOrEmpty(prefix))
        {
            var lead = PlayUi.text("login_link_prefix", row, prefix, TEXT_SIZE, LoginTheme.text_primary, TextAnchor.MiddleCenter);
            lead.horizontalOverflow = HorizontalWrapMode.Overflow;
            lead.verticalOverflow = VerticalWrapMode.Truncate;
        }

        var label = PlayUi.text("login_link_text", row, link, TEXT_SIZE, LoginTheme.gold, TextAnchor.MiddleCenter);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = true;

        var glow = LobbyUi.rim_glow("login_link_glow", label.rectTransform, LoginTheme.clear);
        glow.rectTransform.SetAsFirstSibling();

        var underline = PlayUi.image("login_link_underline", label.rectTransform, null);
        underline.color = LoginTheme.gold;
        PlayUi.place(underline.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, -UNDERLINE_DROP), new Vector2(0f, UNDERLINE_THICKNESS));

        var button = label.gameObject.AddComponent<Button>();
        button.targetGraphic = label;
        LobbyUi.no_transition(button);
        if (on_click != null) button.onClick.AddListener(on_click);

        LoginFocusGlow.attach(button, label, LoginTheme.gold, LoginTheme.gold_soft, glow, PlayTheme.with_alpha(LoginTheme.gold, GLOW_ALPHA));
        return button;
    }

    static void add_ornament(RectTransform slot, string name, float x_min, float x_max, bool diamond_on_right)
    {
        var ornament = LobbyOrnament.diamond_tail(slot, name, LoginTheme.gold, diamond_on_right);
        ornament.anchorMin = new Vector2(x_min, 0.5f - ORNAMENT_HEIGHT * 0.5f);
        ornament.anchorMax = new Vector2(x_max, 0.5f + ORNAMENT_HEIGHT * 0.5f);
        ornament.pivot = new Vector2(0.5f, 0.5f);
        ornament.offsetMin = Vector2.zero;
        ornament.offsetMax = Vector2.zero;
    }
}

public class LoginLinkFit : MonoBehaviour
{
    RectTransform rect;

    void Awake()
    {
        rect = (RectTransform)transform;
    }

    void LateUpdate()
    {
        var available = rect.rect.width;
        var needed = LayoutUtility.GetPreferredWidth(rect);
        var scale = needed > available && needed > 0f ? available / needed : 1f;
        if (Mathf.Approximately(rect.localScale.x, scale)) return;

        rect.localScale = new Vector3(scale, scale, 1f);
    }
}
