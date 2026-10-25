using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PlayUi
{
    const string FONT_PATH = "Fonts/MatchaSalt";
    const string BUILTIN_FONT = "LegacyRuntime.ttf";
    const string FALLBACK_OS_FONT = "Arial";
    const int FALLBACK_FONT_SIZE = 16;
    const int DEFAULT_LAYER = 5;
    const int FADE_STEPS = 64;
    const int GLOW_SIZE = 128;
    const float GLOW_FALLOFF = 2f;
    const int PILL_SIZE = 64;
    const float PILL_RADIUS = 32f;
    const float PILL_BORDER = 31f;
    const int CIRCLE_SIZE = 128;
    const float CIRCLE_EDGE_SHARPNESS = 64f;

    static readonly Dictionary<string, Sprite> sprite_cache = new Dictionary<string, Sprite>();
    static readonly HashSet<string> missing_paths = new HashSet<string>();
    static readonly List<Shadow> shadow_buffer = new List<Shadow>();
    static Font font_value;
    static Sprite vertical_fade_sprite;
    static Sprite radial_glow_sprite;
    static Sprite pill_sprite;
    static Sprite circle_sprite;

    public static Font font
    {
        get
        {
            if (font_value == null) font_value = load_font();
            return font_value;
        }
        set { font_value = value; }
    }

    public static Font load_font()
    {
        var loaded = Resources.Load<Font>(FONT_PATH);
        if (loaded != null) return loaded;

        Debug.LogWarning($"font '{FONT_PATH}' was not found in Resources, using the built-in font");
        loaded = Resources.GetBuiltinResource<Font>(BUILTIN_FONT);
        if (loaded != null) return loaded;

        return Font.CreateDynamicFontFromOSFont(FALLBACK_OS_FONT, FALLBACK_FONT_SIZE);
    }

    public static Sprite sprite(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (sprite_cache.TryGetValue(path, out var cached) && cached != null) return cached;
        if (missing_paths.Contains(path)) return null;

        var loaded = Resources.Load<Sprite>(path);
        if (loaded == null)
        {
            missing_paths.Add(path);
            Debug.LogWarning($"sprite '{path}' was not found in Resources");
            return null;
        }

        sprite_cache[path] = loaded;
        return loaded;
    }

    public static RectTransform rect(string name, Transform parent)
    {
        var holder = new GameObject(string.IsNullOrEmpty(name) ? "rect" : name, typeof(RectTransform));
        holder.layer = parent != null ? parent.gameObject.layer : DEFAULT_LAYER;

        var holder_rect = holder.GetComponent<RectTransform>();
        holder_rect.SetParent(parent, false);
        return holder_rect;
    }

    public static Image image(string name, Transform parent, Sprite sprite, bool raycast = false)
    {
        var holder = rect(name, parent);
        var picture = holder.gameObject.AddComponent<Image>();
        picture.sprite = sprite;
        picture.raycastTarget = raycast;
        if (sprite == null) picture.color = PlayTheme.clear;
        return picture;
    }

    public static Text text(string name, Transform parent, string value, int size, Color color, TextAnchor anchor)
    {
        var holder = rect(name, parent);
        var label = holder.gameObject.AddComponent<Text>();
        label.font = font;
        label.text = value ?? string.Empty;
        label.fontSize = size;
        label.color = color;
        label.alignment = anchor;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        return label;
    }

    public static void stretch(RectTransform r)
    {
        if (r == null) return;

        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
    }

    public static void place(RectTransform r, Vector2 anchor_min, Vector2 anchor_max, Vector2 pivot, Vector2 anchored_pos, Vector2 size)
    {
        if (r == null) return;

        r.anchorMin = anchor_min;
        r.anchorMax = anchor_max;
        r.pivot = pivot;
        r.anchoredPosition = anchored_pos;
        r.sizeDelta = size;
    }

    public static Sprite vertical_fade()
    {
        if (vertical_fade_sprite != null) return vertical_fade_sprite;

        var texture = new Texture2D(1, FADE_STEPS, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color[FADE_STEPS];
        for (var i = 0; i < FADE_STEPS; i++)
        {
            var alpha = 1f - i / (float)(FADE_STEPS - 1);
            pixels[i] = PlayTheme.with_alpha(PlayTheme.white, alpha);
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        vertical_fade_sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, FADE_STEPS), new Vector2(0.5f, 0.5f), 100f);
        return vertical_fade_sprite;
    }

    public static Sprite radial_glow()
    {
        if (radial_glow_sprite != null) return radial_glow_sprite;

        var texture = new Texture2D(GLOW_SIZE, GLOW_SIZE, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color[GLOW_SIZE * GLOW_SIZE];
        var center = (GLOW_SIZE - 1) * 0.5f;
        for (var y = 0; y < GLOW_SIZE; y++)
        {
            for (var x = 0; x < GLOW_SIZE; x++)
            {
                var distance = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center)) / center;
                var alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), GLOW_FALLOFF);
                pixels[y * GLOW_SIZE + x] = PlayTheme.with_alpha(PlayTheme.white, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        radial_glow_sprite = Sprite.Create(texture, new Rect(0f, 0f, GLOW_SIZE, GLOW_SIZE), new Vector2(0.5f, 0.5f), 100f);
        return radial_glow_sprite;
    }

    public static Sprite pill()
    {
        if (pill_sprite != null) return pill_sprite;

        var texture = new Texture2D(PILL_SIZE, PILL_SIZE, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color[PILL_SIZE * PILL_SIZE];
        var center = (PILL_SIZE - 1) * 0.5f;
        var inner = center - PILL_RADIUS + 0.5f;
        for (var y = 0; y < PILL_SIZE; y++)
        {
            for (var x = 0; x < PILL_SIZE; x++)
            {
                var dx = Mathf.Max(Mathf.Abs(x - center) - inner, 0f);
                var dy = Mathf.Max(Mathf.Abs(y - center) - inner, 0f);
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                var alpha = Mathf.Clamp01(PILL_RADIUS - distance + 0.5f);
                pixels[y * PILL_SIZE + x] = PlayTheme.with_alpha(PlayTheme.white, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        var border = new Vector4(PILL_BORDER, PILL_BORDER, PILL_BORDER, PILL_BORDER);
        pill_sprite = Sprite.Create(texture, new Rect(0f, 0f, PILL_SIZE, PILL_SIZE), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        return pill_sprite;
    }

    public static void add_shadow(Graphic g, float distance = 4f)
    {
        if (g == null) return;

        g.GetComponents(shadow_buffer);
        Shadow target = null;
        for (var i = 0; i < shadow_buffer.Count; i++)
        {
            if (shadow_buffer[i].GetType() != typeof(Shadow)) continue;
            target = shadow_buffer[i];
            break;
        }
        shadow_buffer.Clear();

        if (target == null) target = g.gameObject.AddComponent<Shadow>();
        target.effectColor = PlayTheme.shadow;
        target.effectDistance = new Vector2(distance, -distance);
        target.useGraphicAlpha = true;
    }

    public static void go_to_scene(string scene_name)
    {
        if (string.IsNullOrEmpty(scene_name) || !Application.CanStreamedLevelBeLoaded(scene_name))
        {
            Debug.LogWarning($"scene '{scene_name}' is not in the build settings yet");
            return;
        }

        SceneManager.LoadScene(scene_name);
    }

    public static Image circle(string name, Transform parent, Color color, bool raycast = false)
    {
        var picture = image(name, parent, circle_art(), raycast);
        picture.type = Image.Type.Simple;
        picture.preserveAspect = true;
        picture.color = color;
        return picture;
    }

    static Sprite circle_art()
    {
        if (circle_sprite != null) return circle_sprite;

        var texture = new Texture2D(CIRCLE_SIZE, CIRCLE_SIZE, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color[CIRCLE_SIZE * CIRCLE_SIZE];
        for (var y = 0; y < CIRCLE_SIZE; y++)
        {
            for (var x = 0; x < CIRCLE_SIZE; x++)
            {
                var u = (x + 0.5f) / CIRCLE_SIZE;
                var v = (y + 0.5f) / CIRCLE_SIZE;
                var distance = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f)) * 2f;
                pixels[y * CIRCLE_SIZE + x] = PlayTheme.with_alpha(PlayTheme.white, (1f - distance) * CIRCLE_EDGE_SHARPNESS);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        circle_sprite = Sprite.Create(texture, new Rect(0f, 0f, CIRCLE_SIZE, CIRCLE_SIZE), new Vector2(0.5f, 0.5f), 100f);
        return circle_sprite;
    }
}
