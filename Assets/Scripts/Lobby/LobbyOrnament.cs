using UnityEngine;
using UnityEngine.UI;

public static class LobbyOrnament
{
    const float ARROW_MIN = 0.12f;
    const float ARROW_MAX = 0.88f;
    const float LINE_THICKNESS = 2f;
    const float LINE_OVERLAP = 0.5f;

    public static RectTransform arrow_tail(Transform parent, string name, Color color, bool arrow_on_right)
    {
        var holder = PlayUi.rect(name, parent);

        var arrow_slot = PlayUi.rect("arrow_slot", holder);
        var edge = arrow_on_right ? 1f : 0f;
        arrow_slot.anchorMin = new Vector2(edge, ARROW_MIN);
        arrow_slot.anchorMax = new Vector2(edge, ARROW_MAX);
        arrow_slot.pivot = new Vector2(edge, 0.5f);
        arrow_slot.offsetMin = Vector2.zero;
        arrow_slot.offsetMax = Vector2.zero;
        var fit = arrow_slot.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        fit.aspectRatio = 1f;

        var arrow = PlayOrnament.arrow(arrow_slot, "arrow", 1f, arrow_on_right, color);
        PlayUi.stretch(arrow.rectTransform);

        var line = PlayOrnament.line(holder, "line", 10f, LINE_THICKNESS, color, arrow_on_right, !arrow_on_right);
        var half = LINE_THICKNESS * 0.5f;
        var line_rect = line.rectTransform;
        line_rect.pivot = new Vector2(0.5f, 0.5f);
        line_rect.anchorMin = new Vector2(0f, 0.5f);
        line_rect.anchorMax = new Vector2(1f, 0.5f);
        line_rect.offsetMin = new Vector2(0f, -half);
        line_rect.offsetMax = new Vector2(0f, half);
        line_rect.SetAsFirstSibling();

        var overlap = holder.gameObject.AddComponent<LobbyOrnamentLine>();
        overlap.setup(line_rect, arrow_slot, arrow_on_right, LINE_OVERLAP, half);
        return holder;
    }
}

public class LobbyOrnamentLine : MonoBehaviour
{
    RectTransform line;
    RectTransform arrow_slot;
    bool arrow_on_right;
    float overlap;
    float half;

    public void setup(RectTransform line_rect, RectTransform slot, bool on_right, float arrow_overlap, float half_thickness)
    {
        line = line_rect;
        arrow_slot = slot;
        arrow_on_right = on_right;
        overlap = arrow_overlap;
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
        if (line == null || arrow_slot == null) return;

        var reach = arrow_slot.rect.width * overlap;
        if (arrow_on_right)
        {
            line.offsetMin = new Vector2(0f, -half);
            line.offsetMax = new Vector2(-reach, half);
            return;
        }

        line.offsetMin = new Vector2(reach, -half);
        line.offsetMax = new Vector2(0f, half);
    }
}
