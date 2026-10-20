using UnityEngine;
using UnityEngine.UI;

public static class LobbyTopBar
{
    private const string SETTINGS_SCENE = "Settings";
    private const string PRIVATE_LOBBY_SCENE = "PrivateLobby";
    private const string SETTINGS_ICON_PATH = "Icons/Play/IconTopSettings";
    private const float ICON_GLYPH_RATIO = 0.62f;
    private const float ICON_SHADOW_DISTANCE = 3f;
    private const float GLOW_OVERSHOOT = 0.55f;
    private const float LINE_GAP = 8f;
    private const float LINE_HEIGHT = 3f;
    private const float ORNAMENT_WIDTH = 160f;
    private const float ORNAMENT_DIAMOND_HEIGHT = 0.5f;
    private const float BACK_FILL_INSET = 0.1f;

    public static void build(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        RectTransform bar = PlayUi.rect("LobbyTopBar", root);
        PlayUi.stretch(bar);

        Button back = build_circle(bar, "ButtonBack", LobbyUi.ICON_ARROW_RIGHT_PATH, 180f, true, on_back);

        RectTransform crumb = PlayUi.rect("Breadcrumb", bar);
        HorizontalLayoutGroup group = crumb.gameObject.AddComponent<HorizontalLayoutGroup>();
        group.childAlignment = TextAnchor.MiddleLeft;
        group.childControlWidth = true;
        group.childControlHeight = true;
        group.childForceExpandWidth = false;
        group.childForceExpandHeight = true;

        Image star = PlayUi.image("TitleStar", crumb, PlayUi.sprite(LobbyUi.ICON_SUN_PATH));
        star.enabled = star.sprite != null;
        star.color = PlayTheme.gold;
        star.preserveAspect = true;
        star.raycastTarget = false;
        LayoutElement star_element = star.gameObject.AddComponent<LayoutElement>();
        star_element.flexibleWidth = 0f;

        Text label = PlayUi.text("BreadcrumbText", crumb, string.Empty, 24, PlayTheme.text, TextAnchor.MiddleLeft);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        PlayUi.add_shadow(label);

        build_ornament(crumb, "TitleOrnament");

        Button private_lobby = build_circle(bar, "ButtonPrivateLobby", LobbyUi.ICON_PRIVATE_LOBBY_PATH, 0f, false, on_private_lobby);
        Button settings = build_circle(bar, "ButtonSettings", SETTINGS_ICON_PATH, 0f, false, on_settings);
        Button minimize = build_circle(bar, "ButtonMinimize", LobbyUi.ICON_MINIMIZE_PATH, 0f, false, on_minimize);

        LobbyTopBarLayout layout = bar.gameObject.AddComponent<LobbyTopBarLayout>();
        layout.setup(bar, back.GetComponent<RectTransform>(), crumb, group, label, star_element, new[]
        {
            private_lobby.GetComponent<RectTransform>(),
            settings.GetComponent<RectTransform>(),
            minimize.GetComponent<RectTransform>()
        });
    }

    private static void on_back()
    {
        LobbyState.leave();
        PlayUi.go_to_scene(LobbyUi.PLAY_SCENE);
        PlayFocus.clear_selection();
    }

    private static void on_private_lobby()
    {
        if (Application.CanStreamedLevelBeLoaded(PRIVATE_LOBBY_SCENE))
        {
            PlayUi.go_to_scene(PRIVATE_LOBBY_SCENE);
        }
        PlayFocus.clear_selection();
    }

    private static void on_settings()
    {
        if (Application.CanStreamedLevelBeLoaded(SETTINGS_SCENE))
        {
            PlayUi.go_to_scene(SETTINGS_SCENE);
        }
        PlayFocus.clear_selection();
    }

    private static void on_minimize()
    {
        LobbyState.minimize();
        PlayFocus.clear_selection();
    }

    private static void build_ornament(RectTransform parent, string object_name)
    {
        RectTransform ornament = LobbyOrnament.diamond_tail(parent, object_name, PlayTheme.gold, false, ORNAMENT_DIAMOND_HEIGHT);
        LayoutElement element = ornament.gameObject.AddComponent<LayoutElement>();
        element.minWidth = ORNAMENT_WIDTH;
        element.preferredWidth = ORNAMENT_WIDTH;
        element.flexibleWidth = 0f;
    }

    private static Button build_circle(RectTransform parent, string object_name, string icon_path, float icon_rotation, bool with_ring, UnityEngine.Events.UnityAction on_click)
    {
        Image hit;
        if (with_ring)
        {
            hit = PlayUi.image(object_name, parent, null, true);
        }
        else
        {
            hit = PlayUi.image(object_name, parent, PlayUi.pill(), true);
            hit.type = Image.Type.Sliced;
            hit.color = PlayTheme.fill_soft;
        }

        Button button = hit.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        LobbyUi.no_transition(button);
        button.onClick.AddListener(on_click);

        if (with_ring)
        {
            Image fill = PlayUi.circle("Fill", hit.rectTransform, PlayTheme.panel_blue);
            fill.preserveAspect = false;
            anchor_fraction(fill.rectTransform, BACK_FILL_INSET, BACK_FILL_INSET, 1f - BACK_FILL_INSET, 1f - BACK_FILL_INSET);
        }

        Image glow = PlayUi.image("Glow", hit.rectTransform, PlayUi.radial_glow());
        glow.color = PlayTheme.clear;
        anchor_fraction(glow.rectTransform, -GLOW_OVERSHOOT, -GLOW_OVERSHOOT, 1f + GLOW_OVERSHOOT, 1f + GLOW_OVERSHOOT);

        if (with_ring)
        {
            Image frame = PlayUi.image("Frame", hit.rectTransform, PlayUi.sprite(LobbyUi.BACK_FRAME_PATH));
            frame.enabled = frame.sprite != null;
            PlayUi.stretch(frame.rectTransform);
        }

        Image icon = PlayUi.image("Icon", hit.rectTransform, PlayUi.sprite(icon_path));
        icon.enabled = icon.sprite != null;
        icon.color = PlayTheme.icon;
        icon.preserveAspect = true;
        float margin = (1f - ICON_GLYPH_RATIO) * 0.5f;
        anchor_fraction(icon.rectTransform, margin, margin, 1f - margin, 1f - margin);
        icon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, icon_rotation);
        PlayUi.add_shadow(icon, ICON_SHADOW_DISTANCE);

        Image line = PlayOrnament.line(hit.rectTransform, "Line", 0f, LINE_HEIGHT, PlayTheme.clear, true, true);
        PlayUi.place(line.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -LINE_GAP), new Vector2(0f, LINE_HEIGHT));

        PlayTopBarIconHover hover = hit.gameObject.AddComponent<PlayTopBarIconHover>();
        hover.setup(icon, glow, line);

        return button;
    }

    private static void anchor_fraction(RectTransform r, float x_min, float y_min, float x_max, float y_max)
    {
        r.anchorMin = new Vector2(x_min, y_min);
        r.anchorMax = new Vector2(x_max, y_max);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }
}

public class LobbyTopBarLayout : MonoBehaviour
{
    private const float BUTTON_RATIO = 0.56f;
    private const float BACK_ASPECT = 512f / 415f;
    private const float MARGIN_RATIO = 0.35f;
    private const float GAP_RATIO = 0.3f;
    private const float CRUMB_GAP_RATIO = 0.15f;
    private const float CRUMB_SPACING_RATIO = 0.1f;
    private const float TEXT_RATIO = 0.26f;
    private const float STAR_RATIO = 0.32f;
    private const float CRUMB_RIGHT_ANCHOR = 0.7f;
    private const int MIN_TEXT_SIZE = 12;
    private const string SEPARATOR = " LOBBY";
    private const string LOBBY_ONLY = "LOBBY";
    private const char HAIR_SPACE = ' ';

    private RectTransform bar;
    private RectTransform back;
    private RectTransform crumb;
    private HorizontalLayoutGroup crumb_group;
    private Text crumb_text;
    private LayoutElement star_element;
    private RectTransform[] right_buttons;
    private bool ready;
    private int applied_text_size;

    public void setup(RectTransform bar_rect, RectTransform back_rect, RectTransform crumb_rect, HorizontalLayoutGroup group, Text label, LayoutElement star, RectTransform[] buttons)
    {
        bar = bar_rect;
        back = back_rect;
        crumb = crumb_rect;
        crumb_group = group;
        crumb_text = label;
        star_element = star;
        right_buttons = buttons;
        ready = true;

        LobbyState.mode_changed += refresh_label;
        refresh_label();
        apply();
    }

    private void OnDestroy()
    {
        LobbyState.mode_changed -= refresh_label;
    }

    private void OnRectTransformDimensionsChange()
    {
        apply();
    }

    private void refresh_label()
    {
        if (!ready || crumb_text == null)
        {
            return;
        }

        string mode = LobbyState.mode_name;
        string value = string.IsNullOrEmpty(mode) ? LOBBY_ONLY : mode.ToUpperInvariant() + SEPARATOR;
        crumb_text.text = space_letters(value);
        LayoutRebuilder.MarkLayoutForRebuild(crumb);
    }

    private string space_letters(string value)
    {
        if (crumb_text.font == null || !crumb_text.font.HasCharacter(HAIR_SPACE))
        {
            return value;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(value.Length * 2);
        for (int i = 0; i < value.Length; i++)
        {
            builder.Append(value[i]);
            if (i < value.Length - 1 && value[i] != ' ' && value[i + 1] != ' ')
            {
                builder.Append(HAIR_SPACE);
            }
        }
        return builder.ToString();
    }

    private void apply()
    {
        if (!ready || bar == null)
        {
            return;
        }

        float height = bar.rect.height;
        if (height <= 0f)
        {
            return;
        }

        float size = height * BUTTON_RATIO;
        float margin = height * MARGIN_RATIO;
        float gap = height * GAP_RATIO;

        float back_width = size * BACK_ASPECT;
        place_left(back, margin, back_width, size);

        float crumb_left = margin + back_width + height * CRUMB_GAP_RATIO;
        crumb.anchorMin = new Vector2(0f, 0.5f);
        crumb.anchorMax = new Vector2(CRUMB_RIGHT_ANCHOR, 0.5f);
        crumb.pivot = new Vector2(0f, 0.5f);
        crumb.offsetMin = new Vector2(crumb_left, -size * 0.5f);
        crumb.offsetMax = new Vector2(0f, size * 0.5f);
        crumb_group.spacing = height * CRUMB_SPACING_RATIO;

        float star_size = height * STAR_RATIO;
        star_element.minWidth = star_size;
        star_element.preferredWidth = star_size;

        int text_size = Mathf.Max(MIN_TEXT_SIZE, Mathf.RoundToInt(height * TEXT_RATIO));
        if (text_size != applied_text_size)
        {
            applied_text_size = text_size;
            crumb_text.fontSize = text_size;
            LayoutRebuilder.MarkLayoutForRebuild(crumb);
        }

        int count = right_buttons.Length;
        for (int i = 0; i < count; i++)
        {
            int from_right = count - 1 - i;
            place_right(right_buttons[i], margin + from_right * (size + gap), size);
        }
    }

    private static void place_left(RectTransform r, float x, float width, float height)
    {
        PlayUi.place(r, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, 0f), new Vector2(width, height));
    }

    private static void place_right(RectTransform r, float x, float size)
    {
        PlayUi.place(r, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-x, 0f), new Vector2(size, size));
    }
}
