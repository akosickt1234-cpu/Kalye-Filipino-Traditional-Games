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
    const float GLOW_PAD_X = 28f;
    const float GLOW_PAD_Y = 20f;

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
        nav.gameObject.AddComponent<PlaySideNavLayout>().setup(root);

        var tab_backgrounds = new Image[LABELS.Length];
        for (int i = 0; i < tab_backgrounds.Length; i++)
        {
            tab_backgrounds[i] = build_tab_background(nav, i);
        }

        var frame = PlayUi.image("SelectionFrame", nav, PlayUi.sprite(TAB_PATH));
        frame.preserveAspect = false;
        PlayUi.place(frame.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(LEFT, -ROW_Y[0]), new Vector2(ROW_WIDTH, FRAME_HEIGHT));
        PlayUi.add_shadow(frame);

        var glow = PlayUi.image("FrameGlow", frame.rectTransform, PlayUi.radial_glow());
        PlayUi.stretch(glow.rectTransform);
        glow.rectTransform.offsetMin = new Vector2(-GLOW_PAD_X, -GLOW_PAD_Y);
        glow.rectTransform.offsetMax = new Vector2(GLOW_PAD_X, GLOW_PAD_Y);
        glow.preserveAspect = false;
        glow.color = PlayTheme.with_alpha(PlayTheme.glow, 0f);

        var frame_mover = nav.gameObject.AddComponent<PlaySideNavFrame>();
        frame_mover.setup(frame.rectTransform, glow, ROW_Y);

        int count = LABELS.Length;
        var buttons = new Button[count];

        for (int i = 0; i < count; i++)
        {
            buttons[i] = build_row(nav, i, frame_mover, tab_backgrounds[i]);
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

    static Image build_tab_background(RectTransform parent, int index)
    {
        var background = PlayUi.image("TabBackground" + LABELS[index], parent, PlayUi.sprite(TAB_PATH));
        background.preserveAspect = false;
        background.raycastTarget = false;
        background.color = PlayTheme.fill_soft;
        PlayUi.place(background.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(LEFT, -ROW_Y[index]), new Vector2(ROW_WIDTH, FRAME_HEIGHT));
        return background;
    }

    static Button build_row(RectTransform parent, int index, PlaySideNavFrame frame_mover, Image tab_background)
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
        icon.color = PlayTheme.icon;
        PlayUi.place(icon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(ICON_CENTER_X, 0f), new Vector2(ICON_SIZE, ICON_SIZE));
        icon.preserveAspect = true;
        PlayUi.add_shadow(icon);

        var label = PlayUi.text("Label" + label_value, content, label_value, LABEL_SIZE, PlayTheme.text_muted, TextAnchor.MiddleLeft);
        PlayUi.place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(LABEL_LEFT, 0f), new Vector2(ROW_WIDTH - LABEL_LEFT, ROW_HEIGHT));
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        PlayUi.add_shadow(label);

        var button = row.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;

        var item = row.gameObject.AddComponent<PlaySideNavItem>();
        item.setup(index, SCENES[index], frame_mover, content, icon, label, tab_background);

        button.onClick.AddListener(item.activate);
        return button;
    }
}

public class PlaySideNavLayout : MonoBehaviour
{
    const float REF_HEIGHT = 1080f;
    const float REF_ASPECT = 16f / 9f;

    RectTransform root;
    RectTransform nav;
    float applied_shift;

    public void setup(RectTransform root_rect)
    {
        root = root_rect;
        nav = transform as RectTransform;
        apply();
    }

    void OnRectTransformDimensionsChange()
    {
        apply();
    }

    void apply()
    {
        if (root == null || nav == null) return;

        var canvas_rect = root.rect;
        float extra_width = Mathf.Max(0f, canvas_rect.width - canvas_rect.height * REF_ASPECT);
        float shift = extra_width > 0f ? Mathf.Max(0f, REF_HEIGHT - canvas_rect.height) : 0f;
        float delta = shift - applied_shift;
        if (Mathf.Approximately(delta, 0f)) return;

        applied_shift = shift;
        nav.offsetMin += Vector2.up * delta;
        nav.offsetMax += Vector2.up * delta;
    }
}

public class PlaySideNavFrame : MonoBehaviour
{
    const float DURATION = 0.38f;
    const float OVERSHOOT = 1.15f;
    const float STRETCH = 0.12f;
    const float PULSE_DURATION = 0.55f;
    const float PULSE_STRENGTH = 1.8f;

    RectTransform frame;
    Image glow;
    float[] rows_y;
    float from_y;
    float to_y;
    float progress = 1f;
    float pulse = 1f;

    public int selected_index { get; private set; }

    public void setup(RectTransform frame_rect, Image glow_image, float[] row_positions)
    {
        frame = frame_rect;
        glow = glow_image;
        rows_y = row_positions;
        selected_index = 0;
        progress = 1f;
        pulse = 1f;
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
        pulse = 1f;
    }

    void Update()
    {
        if (frame == null) return;

        if (progress < 1f)
        {
            progress = Mathf.Min(1f, progress + Time.unscaledDeltaTime / DURATION);
            apply();
            if (progress >= 1f) pulse = 0f;
        }
        else if (pulse < 1f)
        {
            pulse = Mathf.Min(1f, pulse + Time.unscaledDeltaTime / PULSE_DURATION);
        }

        apply_glow();
    }

    float current_y()
    {
        return frame != null ? frame.anchoredPosition.y : to_y;
    }

    static float ease_out_back(float t)
    {
        float c3 = OVERSHOOT + 1f;
        float u = t - 1f;
        return 1f + c3 * u * u * u + OVERSHOOT * u * u;
    }

    void apply()
    {
        float eased = ease_out_back(progress);
        var position = frame.anchoredPosition;
        position.y = Mathf.LerpUnclamped(from_y, to_y, eased);
        frame.anchoredPosition = position;

        float reach = Mathf.Clamp01(Mathf.Abs(to_y - from_y) / 140f);
        float stretch = Mathf.Sin(Mathf.PI * progress) * STRETCH * Mathf.Max(0.35f, reach);
        var scale = frame.localScale;
        scale.y = 1f + stretch;
        scale.x = 1f - stretch * 0.15f;
        frame.localScale = scale;
    }

    void apply_glow()
    {
        if (glow == null) return;

        float strength = pulse < 1f ? Mathf.Sin(Mathf.PI * pulse) : 0f;
        float travel = progress < 1f ? Mathf.Sin(Mathf.PI * progress) * 0.35f : 0f;
        glow.color = PlayTheme.with_alpha(PlayTheme.glow, PlayTheme.glow.a * (strength * PULSE_STRENGTH + travel));
    }
}

public class PlaySideNavItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    const float SHIFT = 12f;
    const float SPEED = 14f;
    const float CURRENT_SPEED = 9f;
    const float CURRENT_SHADOW = 0.3f;

    int index;
    string scene_name;
    PlaySideNavFrame frame_mover;
    RectTransform content;
    Image icon;
    Text label;
    Shadow icon_shadow;
    Shadow label_shadow;
    bool hovered;
    bool focused;
    float amount;
    float current_amount;
    Image tab_background;

    public void setup(int item_index, string target_scene, PlaySideNavFrame mover, RectTransform content_rect, Image icon_image, Text label_text, Image background_image)
    {
        index = item_index;
        scene_name = target_scene;
        frame_mover = mover;
        content = content_rect;
        icon = icon_image;
        label = label_text;
        tab_background = background_image;
        icon_shadow = icon != null ? icon.GetComponent<Shadow>() : null;
        label_shadow = label != null ? label.GetComponent<Shadow>() : null;
        current_amount = is_current() ? 1f : 0f;
        apply(0f);
    }

    void OnDisable()
    {
        hovered = false;
        focused = false;
        amount = 0f;
        current_amount = is_current() ? 1f : 0f;
        apply(0f);
    }

    void Update()
    {
        float target = PlayFocus.lit(hovered, focused) ? 1f : 0f;
        amount = Mathf.Lerp(amount, target, 1f - Mathf.Exp(-SPEED * Time.unscaledDeltaTime));
        float current_target = is_current() ? 1f : 0f;
        current_amount = Mathf.Lerp(current_amount, current_target, 1f - Mathf.Exp(-CURRENT_SPEED * Time.unscaledDeltaTime));
        apply(amount);
    }

    bool is_current()
    {
        return frame_mover != null && frame_mover.selected_index == index;
    }

    void apply(float t)
    {
        var hover_color = Color.Lerp(PlayTheme.gold, PlayTheme.label_brown, current_amount);
        if (content != null) content.anchoredPosition = Vector2.right * (SHIFT * t);

        if (label != null)
        {
            var idle = Color.Lerp(PlayTheme.text_muted, PlayTheme.label_brown, current_amount);
            label.color = Color.Lerp(idle, hover_color, t);
        }

        if (icon != null)
        {
            var idle = Color.Lerp(PlayTheme.icon, PlayTheme.label_brown, current_amount);
            icon.color = Color.Lerp(idle, hover_color, t);
        }

        if (tab_background != null) tab_background.color = PlayTheme.with_alpha(PlayTheme.fill_soft, PlayTheme.fill_soft.a * (1f - current_amount));

        var shadow_color = PlayTheme.with_alpha(PlayTheme.shadow, PlayTheme.shadow.a * Mathf.Lerp(1f, CURRENT_SHADOW, current_amount));
        if (icon_shadow != null) icon_shadow.effectColor = shadow_color;
        if (label_shadow != null) label_shadow.effectColor = shadow_color;
    }

    public void activate()
    {
        bool was_current = is_current();

        if (frame_mover != null) frame_mover.select(index);

        if (!was_current && !string.IsNullOrEmpty(scene_name))
        {
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
