using UnityEngine;
using UnityEngine.UI;

public static class LobbyOrnament
{
    const float DEFAULT_DIAMOND_HEIGHT = 0.76f;
    const float LINE_THICKNESS = 2f;
    const float LINE_ALPHA = 0.7f;
    const float LINE_OVERLAP = 0.5f;

    public static RectTransform diamond_tail(Transform parent, string name, Color color, bool diamond_on_right, float diamond_height = DEFAULT_DIAMOND_HEIGHT)
    {
        var diamond_min = 0.5f - diamond_height * 0.5f;
        var diamond_max = 0.5f + diamond_height * 0.5f;
        var holder = PlayUi.rect(name, parent);

        var diamond_slot = PlayUi.rect("diamond_slot", holder);
        var edge = diamond_on_right ? 1f : 0f;
        diamond_slot.anchorMin = new Vector2(edge, diamond_min);
        diamond_slot.anchorMax = new Vector2(edge, diamond_max);
        diamond_slot.pivot = new Vector2(edge, 0.5f);
        diamond_slot.offsetMin = Vector2.zero;
        diamond_slot.offsetMax = Vector2.zero;
        var fit = diamond_slot.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        fit.aspectRatio = 1f;

        var diamond = PlayUi.image("diamond", diamond_slot, PlayUi.sprite(LobbyUi.ICON_RHOMBUS_PATH));
        diamond.enabled = diamond.sprite != null;
        diamond.preserveAspect = true;
        diamond.color = color;
        PlayUi.stretch(diamond.rectTransform);

        var line = PlayOrnament.line(holder, "line", 10f, LINE_THICKNESS, PlayTheme.with_alpha(color, LINE_ALPHA), diamond_on_right, !diamond_on_right);
        var half = LINE_THICKNESS * 0.5f;
        var line_rect = line.rectTransform;
        line_rect.pivot = new Vector2(0.5f, 0.5f);
        line_rect.anchorMin = new Vector2(0f, 0.5f);
        line_rect.anchorMax = new Vector2(1f, 0.5f);
        line_rect.offsetMin = new Vector2(0f, -half);
        line_rect.offsetMax = new Vector2(0f, half);
        line_rect.SetAsFirstSibling();

        var overlap = holder.gameObject.AddComponent<LobbyOrnamentLine>();
        overlap.setup(line_rect, diamond_slot, diamond_on_right, LINE_OVERLAP, half);
        return holder;
    }
}

public class LobbyOrnamentLine : MonoBehaviour
{
    RectTransform line;
    RectTransform diamond_slot;
    bool diamond_on_right;
    float overlap;
    float half;

    public void setup(RectTransform line_rect, RectTransform slot, bool on_right, float diamond_overlap, float half_thickness)
    {
        line = line_rect;
        diamond_slot = slot;
        diamond_on_right = on_right;
        overlap = diamond_overlap;
        half = half_thickness;
        apply();
    }

    void OnEnable()
    {
        apply();
    }

    void OnRectTransformDimensionsChange()
    {
        apply();
    }

    void apply()
    {
        if (line == null || diamond_slot == null) return;

        var reach = diamond_slot.rect.width * overlap;
        if (diamond_on_right)
        {
            line.offsetMin = new Vector2(0f, -half);
            line.offsetMax = new Vector2(-reach, half);
            return;
        }

        line.offsetMin = new Vector2(reach, -half);
        line.offsetMax = new Vector2(0f, half);
    }
}
