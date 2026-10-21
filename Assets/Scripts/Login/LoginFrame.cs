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

        var fitter = card_rect.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = ratio;
        card = card_rect;

        var inner = PlayUi.rect("login_inner", card_rect);
        LobbyUi.place_fraction(inner, LoginTheme.INNER_LEFT, LoginTheme.INNER_BOTTOM, LoginTheme.INNER_RIGHT, LoginTheme.INNER_TOP);
        return inner;
    }
}
