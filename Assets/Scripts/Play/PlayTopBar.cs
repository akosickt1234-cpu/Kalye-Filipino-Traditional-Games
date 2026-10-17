using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class PlayTopBar
{
    private const float SCALE = 1.12f;
    private const float BAR_WIDTH = 720f * SCALE;
    private const float BAR_HEIGHT = 140f * SCALE;
    private const float AVATAR_X = 46f * SCALE;
    private const float AVATAR_Y = 12f * SCALE;
    private const float AVATAR_SIZE = 100f * SCALE;
    private const float AVATAR_FILL_SIZE = 88f * SCALE;
    private const float BADGE_X = 114f * SCALE;
    private const float BADGE_SIZE = 36f * SCALE;
    private const float BADGE_Y = AVATAR_Y + AVATAR_SIZE - BADGE_SIZE;
    private const int BADGE_TEXT_SIZE = 20;
    private const float CONTAINER_X = AVATAR_X + AVATAR_SIZE * 0.5f;
    private const float CONTAINER_END_X = 430f * SCALE;
    private const float CONTAINER_HEIGHT = AVATAR_SIZE;
    private const float NAME_FIELD_X = AVATAR_X + AVATAR_SIZE;
    private const float NAME_PADDING = 10f * SCALE;
    private const float NAME_X = NAME_FIELD_X + NAME_PADDING;
    private const float NAME_Y = 24f * SCALE;
    private const float NAME_WIDTH = CONTAINER_END_X - NAME_FIELD_X - NAME_PADDING * 2f;
    private const float NAME_HEIGHT = 44f * SCALE;
    private const int NAME_TEXT_SIZE = 36;
    private const float NAME_LINE_CENTER_Y = 82f * SCALE;
    private const float NAME_LINE_WIDTH = NAME_WIDTH;
    private const float ICON_X = 450f * SCALE;
    private const float ICON_Y = 25f * SCALE;
    private const float ICON_SIZE = 64f * SCALE;
    private const float ICON_GLYPH_RATIO = 0.62f;
    private const float ICON_GAP = 24f * SCALE;
    private const float ICON_SHADOW_DISTANCE = 3f;
    private const float GLOW_SIZE = 150f * SCALE;
    private const float LINE_GAP = 8f;
    private const float LINE_HEIGHT = 3f;
    private const string DEFAULT_NAME = "Guest";

    private static Sprite circle_sprite;
    private static Sprite glow_sprite;
    private static Sprite center_line_sprite;

    public static void build(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        RectTransform bar = PlayUi.rect("TopBar", root);
        place_top_left(bar, 0f, 0f, BAR_WIDTH, BAR_HEIGHT);

        build_container(bar);
        build_avatar(bar);
        build_name(bar);
        build_icons(bar);
    }

    private static void build_container(RectTransform bar)
    {
        Image container = PlayUi.image("ProfileContainer", bar, PlayUi.pill());
        container.type = Image.Type.Sliced;
        container.color = PlayTheme.fill_soft;
        container.raycastTarget = false;
        place_top_left(container.rectTransform, CONTAINER_X, AVATAR_Y, CONTAINER_END_X - CONTAINER_X, CONTAINER_HEIGHT);
    }

    private static void build_avatar(RectTransform bar)
    {
        RectTransform avatar = PlayUi.rect("Avatar", bar);
        place_top_left(avatar, AVATAR_X, AVATAR_Y, AVATAR_SIZE, AVATAR_SIZE);

        Image fill = PlayUi.image("AvatarPlaceholderFill", avatar, get_circle_sprite());
        fill.color = PlayTheme.panel_dark;
        place_center(fill.rectTransform, AVATAR_FILL_SIZE);

        Image ring = PlayUi.image("AvatarRing", avatar, PlayUi.sprite("Frames/Play/FrameAvatarRing"));
        ring.enabled = ring.sprite != null;
        PlayUi.stretch(ring.rectTransform);

        RectTransform badge = PlayUi.rect("LevelBadge", bar);
        place_top_left(badge, BADGE_X, BADGE_Y, BADGE_SIZE, BADGE_SIZE);

        Image badge_fill = PlayUi.image("LevelBadgeFill", badge, get_circle_sprite());
        badge_fill.color = PlayTheme.panel_dark;
        PlayUi.stretch(badge_fill.rectTransform);

        Image badge_ring = PlayUi.image("LevelBadgeRing", badge, PlayUi.sprite("Frames/Play/FrameAvatarRing"));
        badge_ring.enabled = badge_ring.sprite != null;
        PlayUi.stretch(badge_ring.rectTransform);

        Text level = PlayUi.text("LevelText", badge, Mathf.Max(0, PlayState.player_level).ToString(), BADGE_TEXT_SIZE, PlayTheme.text, TextAnchor.MiddleCenter);
        level.raycastTarget = false;
        level.horizontalOverflow = HorizontalWrapMode.Overflow;
        level.verticalOverflow = VerticalWrapMode.Overflow;
        PlayUi.stretch(level.rectTransform);
    }

    private static void build_name(RectTransform bar)
    {
        string value = string.IsNullOrWhiteSpace(PlayState.player_name) ? DEFAULT_NAME : PlayState.player_name;

        PlayMarquee name_marquee = PlayMarquee.create(bar, "PlayerName", NAME_TEXT_SIZE, PlayTheme.text, NAME_WIDTH, NAME_HEIGHT);
        place_top_left(name_marquee.rect, NAME_X, NAME_Y, NAME_WIDTH, NAME_HEIGHT);
        name_marquee.set_left_aligned(true);
        name_marquee.set_text(value);

        RectTransform line = PlayOrnament.arrow_line(bar, "PlayerNameLine", NAME_LINE_WIDTH, PlayTheme.gold);
        PlayUi.place(line, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(NAME_X + NAME_WIDTH * 0.5f, -NAME_LINE_CENTER_Y), new Vector2(NAME_LINE_WIDTH, line.sizeDelta.y));
    }

    private static void build_icons(RectTransform bar)
    {
        string[] names = { "ButtonSettings", "ButtonFriends", "ButtonHome" };
        string[] icon_paths = { "Icons/Play/IconTopSettings", "Icons/Play/IconTopProfile", "Icons/Play/IconTopHome" };
        string[] scenes = { "Settings", "Friends", "Menu" };

        Button[] buttons = new Button[names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            float x = ICON_X + i * (ICON_SIZE + ICON_GAP);
            buttons[i] = build_icon_button(bar, names[i], icon_paths[i], scenes[i], x);
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnLeft = i > 0 ? buttons[i - 1] : null;
            navigation.selectOnRight = i < buttons.Length - 1 ? buttons[i + 1] : null;
            buttons[i].navigation = navigation;
        }
    }

    private static Button build_icon_button(RectTransform bar, string object_name, string icon_path, string scene_name, float x)
    {
        Image hit = PlayUi.image(object_name, bar, PlayUi.pill(), true);
        hit.type = Image.Type.Sliced;
        hit.color = PlayTheme.fill_soft;
        place_top_left(hit.rectTransform, x, ICON_Y, ICON_SIZE, ICON_SIZE);

        Button button = hit.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() =>
        {
            PlayUi.go_to_scene(scene_name);
            PlayFocus.clear_selection();
        });

        Image glow = PlayUi.image("Glow", hit.rectTransform, get_glow_sprite());
        glow.color = PlayTheme.clear;
        place_center(glow.rectTransform, GLOW_SIZE);

        Image icon = PlayUi.image("Icon", hit.rectTransform, PlayUi.sprite(icon_path));
        icon.enabled = icon.sprite != null;
        icon.color = PlayTheme.icon;
        icon.preserveAspect = true;
        place_center(icon.rectTransform, ICON_SIZE * ICON_GLYPH_RATIO);
        PlayUi.add_shadow(icon, ICON_SHADOW_DISTANCE);

        Image line = PlayUi.image("Line", hit.rectTransform, get_center_line_sprite());
        line.color = PlayTheme.clear;
        PlayUi.place(line.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, -LINE_GAP), new Vector2(0f, LINE_HEIGHT));

        PlayTopBarIconHover hover = hit.gameObject.AddComponent<PlayTopBarIconHover>();
        hover.setup(icon, glow, line);

        return button;
    }

    private static void place_top_left(RectTransform r, float x, float y, float width, float height)
    {
        PlayUi.place(r, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, -y), new Vector2(width, height));
    }

    private static void place_center(RectTransform r, float size)
    {
        PlayUi.place(r, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));
    }

    private static Sprite get_circle_sprite()
    {
        if (circle_sprite == null)
        {
            circle_sprite = make_sprite(128, 128, (u, v) =>
            {
                float d = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
                return Mathf.Clamp01((1f - d) * 64f);
            });
        }
        return circle_sprite;
    }

    private static Sprite get_glow_sprite()
    {
        if (glow_sprite == null)
        {
            glow_sprite = make_sprite(128, 128, (u, v) =>
            {
                float d = Mathf.Clamp01(Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f);
                float falloff = 1f - d;
                return falloff * falloff;
            });
        }
        return glow_sprite;
    }

    private static Sprite get_center_line_sprite()
    {
        if (center_line_sprite == null)
        {
            center_line_sprite = make_sprite(128, 2, (u, v) => Mathf.Clamp01((1f - Mathf.Abs(u * 2f - 1f)) * 4f));
        }
        return center_line_sprite;
    }

    private static Sprite make_sprite(int width, int height, System.Func<float, float, float> alpha_at)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float u = (x + 0.5f) / width;
                float v = (y + 0.5f) / height;
                pixels[y * width + x] = PlayTheme.with_alpha(PlayTheme.white, alpha_at(u, v));
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }
}

[RequireComponent(typeof(Selectable))]
public class PlayTopBarIconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private const float FADE_IN_SECONDS = 0.15f;
    private const float FADE_OUT_SECONDS = 0.1f;
    private const float ACTIVE_SCALE = 1.08f;
    private const float LINE_WIDTH = 44f;

    private Selectable selectable;
    private Image icon_image;
    private Image glow_image;
    private Image line_image;
    private RectTransform icon_rect;
    private RectTransform line_rect;
    private bool pointer_over;
    private bool focused;
    private float amount;

    public void setup(Image icon, Image glow, Image line)
    {
        selectable = GetComponent<Selectable>();
        icon_image = icon;
        glow_image = glow;
        line_image = line;
        icon_rect = icon != null ? icon.rectTransform : null;
        line_rect = line != null ? line.rectTransform : null;
        apply(0f);
    }

    private void OnDisable()
    {
        pointer_over = false;
        focused = false;
        amount = 0f;
        apply(0f);
    }

    private void Update()
    {
        bool active = PlayFocus.lit(pointer_over, focused) && selectable != null && selectable.IsInteractable();
        float target = active ? 1f : 0f;

        if (Mathf.Approximately(amount, target))
        {
            return;
        }

        float seconds = active ? FADE_IN_SECONDS : FADE_OUT_SECONDS;
        amount = Mathf.MoveTowards(amount, target, Time.unscaledDeltaTime / seconds);
        apply(amount);
    }

    private void apply(float t)
    {
        if (icon_rect != null)
        {
            icon_rect.localScale = Vector3.one * Mathf.Lerp(1f, ACTIVE_SCALE, t);
        }

        if (glow_image != null)
        {
            glow_image.color = PlayTheme.with_alpha(PlayTheme.glow, PlayTheme.glow.a * t);
        }

        if (line_image != null)
        {
            line_image.color = PlayTheme.with_alpha(PlayTheme.gold, PlayTheme.gold.a * t);
        }

        if (line_rect != null)
        {
            line_rect.sizeDelta = new Vector2(LINE_WIDTH * t, line_rect.sizeDelta.y);
        }
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        pointer_over = true;
    }

    public void OnPointerExit(PointerEventData event_data)
    {
        pointer_over = false;
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
