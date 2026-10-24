using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class LoginTextLink
{
    // The ornaments live inside the link row so the short lines and rhombus marks hug
    // the text, matching the sign-in "New to Kalye?" row. Slot-wide ornaments used to
    // run into the card's corner art and read as stray lines.
    const float ROW_LEFT = 0.08f;
    const float ROW_RIGHT = 0.92f;
    const float SPACING = 8f;
    const float ORNAMENT_WIDTH = 46f;
    const float ORNAMENT_HEIGHT = 16f;
    // Fraction of the ornament holder taken by the line; the rest is the diamond.
    const float ORNAMENT_LINE_END = 0.6957f;
    const float ORNAMENT_LINE_THICKNESS = 2f;
    const float ORNAMENT_LINE_ALPHA = 0.8f;
    const float ORNAMENT_DIAMOND = 14f;
    const float UNDERLINE_THICKNESS = 2f;
    const float UNDERLINE_DROP = 14f;
    const int TEXT_SIZE = 22;

    public static Button create(RectTransform slot, string prefix, string link, UnityAction on_click)
    {
        if (slot == null) return null;

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

        add_ornament(row, "login_link_ornament_left", false);

        if (!string.IsNullOrEmpty(prefix))
        {
            var lead = PlayUi.text("login_link_prefix", row, prefix, TEXT_SIZE, LoginTheme.text_primary, TextAnchor.MiddleCenter);
            lead.horizontalOverflow = HorizontalWrapMode.Overflow;
            lead.verticalOverflow = VerticalWrapMode.Overflow;
        }

        var label = PlayUi.text("login_link_text", row, link, TEXT_SIZE, LoginTheme.gold, TextAnchor.MiddleCenter);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = true;

        var underline = PlayUi.image("login_link_underline", label.rectTransform, null);
        underline.color = LoginTheme.gold;
        PlayUi.place(underline.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, -UNDERLINE_DROP), new Vector2(0f, UNDERLINE_THICKNESS));

        var button = label.gameObject.AddComponent<Button>();
        button.targetGraphic = label;
        LobbyUi.no_transition(button);
        if (on_click != null) button.onClick.AddListener(on_click);

        // Same reasoning as the sign-in links: tint + underline only, no rectangle glow.
        LoginFocusGlow.attach(button, label, LoginTheme.gold, LoginTheme.gold_soft, null, LoginTheme.clear);

        add_ornament(row, "login_link_ornament_right", true);
        return button;
    }

    static void add_ornament(RectTransform row, string name, bool mirrored)
    {
        var holder = PlayUi.rect(name, row);
        var element = holder.gameObject.AddComponent<LayoutElement>();
        element.preferredWidth = ORNAMENT_WIDTH;
        element.preferredHeight = ORNAMENT_HEIGHT;
        element.flexibleWidth = 0f;

        var diamond = LoginUi.icon(name + "_diamond", holder, LobbyUi.ICON_RHOMBUS_PATH, LoginTheme.gold);
        var diamond_rect = diamond.rectTransform;
        var edge = mirrored ? 0f : 1f;
        diamond_rect.anchorMin = new Vector2(edge, 0.5f);
        diamond_rect.anchorMax = new Vector2(edge, 0.5f);
        diamond_rect.pivot = new Vector2(edge, 0.5f);
        diamond_rect.anchoredPosition = Vector2.zero;
        diamond_rect.sizeDelta = new Vector2(ORNAMENT_DIAMOND, ORNAMENT_DIAMOND);

        var line = PlayUi.image(name + "_line", holder, null);
        line.color = PlayTheme.with_alpha(LoginTheme.gold, ORNAMENT_LINE_ALPHA);
        var line_rect = line.rectTransform;
        var start = mirrored ? 1f - ORNAMENT_LINE_END : 0f;
        var end = mirrored ? 1f : ORNAMENT_LINE_END;
        line_rect.anchorMin = new Vector2(start, 0.5f);
        line_rect.anchorMax = new Vector2(end, 0.5f);
        line_rect.pivot = new Vector2(0.5f, 0.5f);
        line_rect.anchoredPosition = Vector2.zero;
        line_rect.sizeDelta = new Vector2(0f, ORNAMENT_LINE_THICKNESS);
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
