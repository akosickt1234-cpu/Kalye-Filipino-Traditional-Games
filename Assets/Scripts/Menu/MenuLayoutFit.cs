using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MenuLayoutFit : MonoBehaviour
{
    [SerializeField] private RectTransform logo;
    [SerializeField] private RectTransform menu_list;
    [SerializeField, Range(0f, 0.2f)] private float top_margin_ratio = 0.05f;
    [SerializeField, Range(0f, 0.2f)] private float bottom_margin_ratio = 0.05f;
    [SerializeField, Range(0f, 0.2f)] private float gap_ratio = 0.04f;
    [SerializeField] private float min_gap = 24f;
    [SerializeField, Range(0.1f, 1f)] private float min_logo_scale = 0.55f;

    private const float REFERENCE_HEIGHT = 1080f;

    private RectTransform canvas_rect;
    private Vector2 logo_size;
    private float logo_left;
    private float menu_base_y;
    private Vector2 last_size;

    private void Awake()
    {
        canvas_rect = (RectTransform)transform;
        logo_size = logo.sizeDelta;
        logo_left = logo.anchoredPosition.x;
        menu_base_y = menu_list.anchoredPosition.y;
    }

    private void Start()
    {
        fit();
    }

    private void LateUpdate()
    {
        if (canvas_rect.rect.size != last_size)
        {
            fit();
        }
    }

    private void fit()
    {
        var area = canvas_rect.rect;
        if (area.width <= 0f || area.height <= 0f || logo_size.x <= 0f || logo_size.y <= 0f)
        {
            return;
        }

        last_size = area.size;

        float extra_width = Mathf.Max(0f, area.width - area.height * 16f / 9f);
        float layout_height = extra_width > 0f ? Mathf.Max(area.height, REFERENCE_HEIGHT) : area.height;

        float top_margin = layout_height * top_margin_ratio;
        float bottom_margin = layout_height * bottom_margin_ratio;
        float gap = Mathf.Max(min_gap, layout_height * gap_ratio);
        float menu_height = menu_list.rect.height;

        float menu_top = layout_height * 0.5f - menu_base_y - menu_height * 0.5f;
        menu_top = Mathf.Min(menu_top, area.height - bottom_margin - menu_height);
        float available = menu_top - gap - top_margin;

        float shift = 0f;
        float needed = logo_size.y * min_logo_scale - available;
        if (needed > 0f)
        {
            float room = Mathf.Max(0f, area.height - bottom_margin - (menu_top + menu_height));
            shift = Mathf.Min(needed, room);
            available += shift;
        }

        float menu_y = area.height * 0.5f - (menu_top + shift) - menu_height * 0.5f;
        menu_list.anchoredPosition = new Vector2(menu_list.anchoredPosition.x, menu_y);

        float available_width = Mathf.Max(1f, area.width - logo_left * 2f);
        float scale = Mathf.Min(1f, Mathf.Max(1f, available) / logo_size.y, available_width / logo_size.x);
        var size = logo_size * scale;
        float top = top_margin + Mathf.Max(0f, available - size.y) * 0.5f;

        logo.sizeDelta = size;
        logo.anchoredPosition = new Vector2(logo_left, -top);
    }
}
