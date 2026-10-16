using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class PlayTopBar
{
    private const float BAR_WIDTH = 720f;
    private const float BAR_HEIGHT = 140f;
    private const float AVATAR_X = 46f;
    private const float AVATAR_Y = 12f;
    private const float AVATAR_SIZE = 100f;
    private const float AVATAR_FILL_SIZE = 88f;
    private const float BADGE_X = 43f;
    private const float BADGE_Y = 72f;
    private const float BADGE_SIZE = 36f;
    private const int BADGE_TEXT_SIZE = 18;
    private const float NAME_X = 170f;
    private const float NAME_Y = 24f;
    private const float NAME_WIDTH = 260f;
    private const float NAME_HEIGHT = 44f;
    private const int NAME_TEXT_SIZE = 32;
    private const int NAME_TEXT_MIN_SIZE = 18;
    private const float NAME_LINE_Y = 70f;
    private const float NAME_LINE_WIDTH = 240f;
    private const float NAME_LINE_HEIGHT = 2f;
    private const float ICON_X = 450f;
    private const float ICON_Y = 25f;
    private const float ICON_SIZE = 64f;
    private const float ICON_GAP = 24f;
    private const float GLOW_SIZE = 150f;
    private const float LINE_GAP = 8f;
    private const float LINE_HEIGHT = 3f;
    private const string DEFAULT_NAME = "Player";

    private static Sprite circle_sprite;
    private static Sprite glow_sprite;
    private static Sprite fade_line_sprite;
    private static Sprite center_line_sprite;

    public static void build(RectTransform root)
    {
        if (root == null)
        {
            return;
        }

        RectTransform bar = PlayUi.rect("TopBar", root);
        place_top_left(bar, 0f, 0f, BAR_WIDTH, BAR_HEIGHT);

        build_avatar(bar);
        build_name(bar);
        build_icons(bar);
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

        Text name_text = PlayUi.text("PlayerName", bar, value, NAME_TEXT_SIZE, PlayTheme.text, TextAnchor.MiddleLeft);
        name_text.raycastTarget = false;
        name_text.horizontalOverflow = HorizontalWrapMode.Truncate;
        name_text.verticalOverflow = VerticalWrapMode.Truncate;
        name_text.resizeTextForBestFit = true;
        name_text.resizeTextMinSize = NAME_TEXT_MIN_SIZE;
        name_text.resizeTextMaxSize = NAME_TEXT_SIZE;
        place_top_left(name_text.rectTransform, NAME_X, NAME_Y, NAME_WIDTH, NAME_HEIGHT);

        Image line = PlayUi.image("PlayerNameLine", bar, get_fade_line_sprite());
        line.color = PlayTheme.gold;
        place_top_left(line.rectTransform, NAME_X, NAME_LINE_Y, NAME_LINE_WIDTH, NAME_LINE_HEIGHT);
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
        Image hit = PlayUi.image(object_name, bar, null, true);
        hit.color = PlayTheme.clear;
        place_top_left(hit.rectTransform, x, ICON_Y, ICON_SIZE, ICON_SIZE);

        Button button = hit.gameObject.AddComponent<Button>();
        button.targetGraphic = hit;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => PlayUi.go_to_scene(scene_name));

        Image glow = PlayUi.image("Glow", hit.rectTransform, get_glow_sprite());
        glow.color = PlayTheme.clear;
        place_center(glow.rectTransform, GLOW_SIZE);

        Image icon = PlayUi.image("Icon", hit.rectTransform, PlayUi.sprite(icon_path));
        icon.enabled = icon.sprite != null;
        PlayUi.stretch(icon.rectTransform);

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

    private static Sprite get_fade_line_sprite()
    {
        if (fade_line_sprite == null)
        {
            fade_line_sprite = make_sprite(256, 2, (u, v) => 1f - u);
        }
        return fade_line_sprite;
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
    private const float FADE_SECONDS = 0.15f;
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
        bool active = (pointer_over || focused) && selectable != null && selectable.IsInteractable();
        float target = active ? 1f : 0f;

        if (Mathf.Approximately(amount, target))
        {
            return;
        }

        amount = Mathf.MoveTowards(amount, target, Time.unscaledDeltaTime / FADE_SECONDS);
        apply(amount);
    }

    private void apply(float t)
    {
        if (icon_image != null)
        {
            icon_image.color = Color.Lerp(PlayTheme.text_muted, PlayTheme.text, t);
        }

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
