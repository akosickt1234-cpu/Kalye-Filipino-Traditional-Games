using UnityEngine;
using UnityEngine.UI;

public static class LoginFrame
{
    public static RectTransform card { get; private set; }

    public static RectTransform build(RectTransform root)
    {
        if (root == null) return null;

        var area = PlayUi.rect("login_card_area", root);
        LobbyUi.place_fraction(area, LoginTheme.CARD_AREA_LEFT, LoginTheme.CARD_AREA_BOTTOM, LoginTheme.CARD_AREA_RIGHT, LoginTheme.CARD_AREA_TOP);

        var card_rect = PlayUi.rect("login_card", area);
        var picture = card_rect.gameObject.AddComponent<Image>();
        picture.raycastTarget = false;

        var art = PlayUi.sprite(LoginTheme.FRAME_PATH);
        var ratio = LoginTheme.CARD_ASPECT;
        if (art != null)
        {
            picture.sprite = art;
            picture.type = Image.Type.Simple;
            ratio = art.rect.width / art.rect.height;
        }
        else
        {
            picture.sprite = PlayUi.pill();
            picture.type = Image.Type.Sliced;
            picture.pixelsPerUnitMultiplier = LoginTheme.CORNER_SCALE;
            picture.color = LoginTheme.card_fill;
        }

        // Sweep the card with the same spinning frame light the lobby player cards use.
        LobbyFrameSpin.attach(picture, new Color(1f, 0.74f, 0.2f, 1f), 0.9f, 0f, 0.6f, false);

        area.gameObject.AddComponent<LoginFrameFit>().setup(card_rect, ratio);
        card = card_rect;

        var inner = PlayUi.rect("login_inner", card_rect);
        LobbyUi.place_fraction(inner, LoginTheme.INNER_LEFT, LoginTheme.INNER_BOTTOM, LoginTheme.INNER_RIGHT, LoginTheme.INNER_TOP);
        return inner;
    }
}

public class LoginFrameFit : MonoBehaviour
{
    const float WIDTH_FRACTION = 0.415f;
    const float MIN_WIDTH = 340f;
    const float MAX_WIDTH = 720f;

    RectTransform bounds;
    RectTransform target;
    float aspect = 1f;

    public void setup(RectTransform card_rect, float ratio)
    {
        bounds = transform as RectTransform;
        target = card_rect;
        aspect = ratio > 0f ? ratio : 1f;

        target.anchorMin = new Vector2(0.5f, 0.5f);
        target.anchorMax = new Vector2(0.5f, 0.5f);
        target.pivot = new Vector2(0.5f, 0.5f);
        target.anchoredPosition = Vector2.zero;
        apply();
    }

    void Start()
    {
        apply();
    }

    void OnRectTransformDimensionsChange()
    {
        apply();
    }

    void apply()
    {
        if (bounds == null || target == null) return;

        var area_width = bounds.rect.width;
        var area_height = bounds.rect.height;
        if (area_width <= 0f || area_height <= 0f) return;

        var width = Mathf.Clamp(area_width * WIDTH_FRACTION, MIN_WIDTH, MAX_WIDTH);
        width = Mathf.Min(width, area_width, area_height * aspect);
        target.sizeDelta = new Vector2(width, width / aspect);
    }
}
