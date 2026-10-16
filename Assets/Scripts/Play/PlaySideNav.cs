using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class PlaySideNav
{
    const string NAV_NAME = "SideNav";
    const string TAB_PATH = "Buttons/Play/TabSelectedGold";
    const float LEFT = 35f;
    const float ROW_WIDTH = 326f;
    const float ROW_HEIGHT = 63f;
    const float FRAME_HEIGHT = 71f;
    const float ICON_SIZE = 44f;
    const float ICON_CENTER_X = 72f;
    const float LABEL_LEFT = 112f;
    const int LABEL_SIZE = 30;

    static readonly string[] LABELS = { "Mode", "Customize", "Inventory", "Shop" };
    static readonly string[] ICON_PATHS =
    {
        "Icons/Play/IconNavMode",
        "Icons/Play/IconNavCustomize",
        "Icons/Play/IconNavInventory",
        "Icons/Play/IconNavShop"
    };
    static readonly string[] SCENES = { null, "Customize", "Inventory", "Shop" };
    static readonly float[] ROW_Y = { 278f, 348f, 419f, 489f };

    public static void build(RectTransform root)
    {
        if (root == null) return;

        var nav = PlayUi.rect(NAV_NAME, root);
        PlayUi.stretch(nav);

        var frame = PlayUi.image("SelectionFrame", nav, PlayUi.sprite(TAB_PATH));
        frame.preserveAspect = false;
        PlayUi.place(frame.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(LEFT, -ROW_Y[0]), new Vector2(ROW_WIDTH, FRAME_HEIGHT));

        var frame_mover = nav.gameObject.AddComponent<PlaySideNavFrame>();
        frame_mover.setup(frame.rectTransform, ROW_Y);

        int count = LABELS.Length;
        var buttons = new Button[count];

        for (int i = 0; i < count; i++)
        {
            buttons[i] = build_row(nav, i, frame_mover);
        }

        for (int i = 0; i < count; i++)
        {
            var navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[(i + count - 1) % count],
                selectOnDown = buttons[(i + 1) % count]
            };
            buttons[i].navigation = navigation;
        }

        PlayFocus.default_target = buttons[0].gameObject;
    }

    static Button build_row(RectTransform parent, int index, PlaySideNavFrame frame_mover)
    {
        string label_value = LABELS[index];

        var row = PlayUi.rect("Nav" + label_value, parent);
        PlayUi.place(row, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(LEFT, -ROW_Y[index]), new Vector2(ROW_WIDTH, ROW_HEIGHT));

        var hit = row.gameObject.AddComponent<Image>();
        hit.color = PlayTheme.clear;
        hit.raycastTarget = true;

        var content = PlayUi.rect("Content", row);
        PlayUi.stretch(content);

        var icon = PlayUi.image("Icon" + label_value, content, PlayUi.sprite(ICON_PATHS[index]));
        PlayUi.place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(ICON_CENTER_X, 0f), new Vector2(ICON_SIZE, ICON_SIZE));
        icon.preserveAspect = true;

        var label = PlayUi.text("Label" + label_value, content, label_value, LABEL_SIZE, PlayTheme.text_muted, TextAnchor.MiddleLeft);
        PlayUi.place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(LABEL_LEFT, 0f), new Vector2(ROW_WIDTH - LABEL_LEFT, ROW_HEIGHT));
        label.horizontalOverflow = HorizontalWrapMode.Overflow;

        var button = row.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;

        var item = row.gameObject.AddComponent<PlaySideNavItem>();
        item.setup(index, SCENES[index], frame_mover, content, icon, label);

        button.onClick.AddListener(item.activate);
        return button;
    }
}

public class PlaySideNavFrame : MonoBehaviour
{
    const float DURATION = 0.2f;

    RectTransform frame;
    float[] rows_y;
    float from_y;
    float to_y;
    float progress = 1f;

    public int selected_index { get; private set; }

    public void setup(RectTransform frame_rect, float[] row_positions)
    {
        frame = frame_rect;
        rows_y = row_positions;
        selected_index = 0;
        progress = 1f;
        if (rows_y != null && rows_y.Length > 0)
        {
            from_y = to_y = -rows_y[0];
        }
    }

    public void select(int index)
    {
        if (rows_y == null || index < 0 || index >= rows_y.Length || index == selected_index) return;

        selected_index = index;
        from_y = current_y();
        to_y = -rows_y[index];
        progress = 0f;
    }

    void Update()
    {
        if (frame == null || progress >= 1f) return;

        progress = Mathf.Min(1f, progress + Time.unscaledDeltaTime / DURATION);
        apply();
    }

    float current_y()
    {
        return frame != null ? frame.anchoredPosition.y : to_y;
    }

    void apply()
    {
        float eased = Mathf.SmoothStep(0f, 1f, progress);
        var position = frame.anchoredPosition;
        position.y = Mathf.Lerp(from_y, to_y, eased);
        frame.anchoredPosition = position;
    }
}

public class PlaySideNavItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    const float SHIFT = 12f;
    const float SPEED = 14f;

    int index;
    string scene_name;
    PlaySideNavFrame frame_mover;
    RectTransform content;
    Image icon;
    Text label;
    bool hovered;
    bool focused;
    float amount;

    public void setup(int item_index, string target_scene, PlaySideNavFrame mover, RectTransform content_rect, Image icon_image, Text label_text)
    {
        index = item_index;
        scene_name = target_scene;
        frame_mover = mover;
        content = content_rect;
        icon = icon_image;
        label = label_text;
        apply(0f);
    }

    void OnDisable()
    {
        hovered = false;
        focused = false;
        amount = 0f;
        apply(0f);
    }

    void Update()
    {
        float target = PlayFocus.lit(hovered, focused) ? 1f : 0f;
        amount = Mathf.Lerp(amount, target, 1f - Mathf.Exp(-SPEED * Time.unscaledDeltaTime));
        apply(amount);
    }

    bool is_current()
    {
        return frame_mover != null && frame_mover.selected_index == index;
    }

    void apply(float t)
    {
        var base_color = is_current() ? PlayTheme.text : PlayTheme.text_muted;
        if (content != null) content.anchoredPosition = Vector2.right * (SHIFT * t);
        if (label != null) label.color = Color.Lerp(base_color, PlayTheme.gold, t);
        if (icon != null) icon.color = Color.Lerp(base_color, PlayTheme.gold, t);
    }

    public void activate()
    {
        if (!string.IsNullOrEmpty(scene_name) && !is_current())
        {
            if (Application.CanStreamedLevelBeLoaded(scene_name) && frame_mover != null)
            {
                frame_mover.select(index);
            }
            PlayUi.go_to_scene(scene_name);
        }

        if (!PlayFocus.show_focus) PlayFocus.clear_selection();
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData event_data)
    {
        hovered = false;
    }

    public void OnSelect(BaseEventData event_data)
    {
        focused = true;
    }

    public void OnDeselect(BaseEventData event_data)
    {
        focused = false;
    }
}
