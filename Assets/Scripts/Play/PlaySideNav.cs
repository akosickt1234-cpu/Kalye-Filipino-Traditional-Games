using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class PlaySideNav
{
    const string NAV_NAME = "SideNav";
    const string TAB_PATH = "Buttons/Play/TabSelectedGold";
    const float LEFT = 35f;
    const float ROW_WIDTH = 290f;
    const float ROW_HEIGHT = 63f;
    const float ICON_SIZE = 44f;
    const float ICON_CENTER_X = 36f;
    const float LABEL_LEFT = 76f;
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

        int count = LABELS.Length;
        var buttons = new Button[count];

        for (int i = 0; i < count; i++)
        {
            buttons[i] = build_row(nav, i);
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
    }

    static Button build_row(RectTransform parent, int index)
    {
        bool active = index == 0;
        string label_value = LABELS[index];

        var row = PlayUi.rect("Nav" + label_value, parent);
        PlayUi.place(row, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(LEFT, -ROW_Y[index]), new Vector2(ROW_WIDTH, ROW_HEIGHT));

        var hit = row.gameObject.AddComponent<Image>();
        hit.color = PlayTheme.clear;
        hit.raycastTarget = true;

        if (active)
        {
            var tab = PlayUi.image("TabBackground", row, PlayUi.sprite(TAB_PATH));
            PlayUi.stretch(tab.rectTransform);
            tab.preserveAspect = false;
        }

        var content = PlayUi.rect("Content", row);
        PlayUi.stretch(content);

        var icon = PlayUi.image("Icon" + label_value, content, PlayUi.sprite(ICON_PATHS[index]));
        PlayUi.place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(ICON_CENTER_X, 0f), new Vector2(ICON_SIZE, ICON_SIZE));
        icon.preserveAspect = true;

        var label_color = active ? PlayTheme.text : PlayTheme.text_muted;
        var label = PlayUi.text("Label" + label_value, content, label_value, LABEL_SIZE, label_color, TextAnchor.MiddleLeft);
        PlayUi.place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(LABEL_LEFT, 0f), new Vector2(ROW_WIDTH - LABEL_LEFT, ROW_HEIGHT));
        label.horizontalOverflow = HorizontalWrapMode.Overflow;

        var button = row.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;

        if (active) return button;

        var item = row.gameObject.AddComponent<PlaySideNavItem>();
        item.setup(content, icon, label);

        string scene_name = SCENES[index];
        button.onClick.AddListener(() => PlayUi.go_to_scene(scene_name));
        return button;
    }
}

public class PlaySideNavItem : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
{
    const float SHIFT = 12f;
    const float SPEED = 14f;

    RectTransform content;
    Image icon;
    Text label;
    Selectable selectable;
    bool selected;
    float amount;

    public void setup(RectTransform content_rect, Image icon_image, Text label_text)
    {
        content = content_rect;
        icon = icon_image;
        label = label_text;
        selectable = GetComponent<Selectable>();
        apply(0f);
    }

    void OnDisable()
    {
        selected = false;
        amount = 0f;
        apply(0f);
    }

    void Update()
    {
        float target = selected && selectable != null && selectable.IsInteractable() ? 1f : 0f;
        amount = Mathf.Lerp(amount, target, 1f - Mathf.Exp(-SPEED * Time.unscaledDeltaTime));
        apply(amount);
    }

    void apply(float t)
    {
        if (content != null) content.anchoredPosition = Vector2.right * (SHIFT * t);
        if (label != null) label.color = Color.Lerp(PlayTheme.text_muted, PlayTheme.gold, t);
        if (icon != null) icon.color = Color.Lerp(PlayTheme.white, PlayTheme.gold, t);
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        if (EventSystem.current != null && selectable != null && selectable.IsInteractable())
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void OnSelect(BaseEventData event_data)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData event_data)
    {
        selected = false;
    }
}
