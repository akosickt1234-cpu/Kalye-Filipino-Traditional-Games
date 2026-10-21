using UnityEngine;
using UnityEngine.UI;

public static class LoginUi
{
    public static RectTransform view(string name, RectTransform parent, params LoginMode[] modes)
    {
        if (parent == null) return null;

        var holder = PlayUi.rect(name, parent);
        PlayUi.stretch(holder);
        LoginModeToggle.attach(holder.gameObject, modes);
        return holder;
    }

    public static Image rounded_box(string name, Transform parent, Color border_color, Color fill_color, float border_width, bool raycast)
    {
        var ring = PlayUi.image(name, parent, PlayUi.pill(), raycast);
        ring.type = Image.Type.Sliced;
        ring.pixelsPerUnitMultiplier = LoginTheme.CORNER_SCALE;
        ring.color = border_color;

        var fill = PlayUi.image(name + "_fill", ring.rectTransform, PlayUi.pill());
        fill.type = Image.Type.Sliced;
        fill.pixelsPerUnitMultiplier = LoginTheme.CORNER_SCALE;
        fill.color = fill_color;
        PlayUi.stretch(fill.rectTransform);
        fill.rectTransform.offsetMin = new Vector2(border_width, border_width);
        fill.rectTransform.offsetMax = new Vector2(-border_width, -border_width);
        return ring;
    }

    public static Image icon(string name, Transform parent, string path, Color color)
    {
        var sprite = PlayUi.sprite(path);
        var picture = PlayUi.image(name, parent, sprite);
        picture.enabled = sprite != null;
        picture.preserveAspect = true;
        picture.color = color;
        return picture;
    }
}
