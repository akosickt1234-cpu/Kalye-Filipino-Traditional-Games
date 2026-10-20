using System.Collections.Generic;
using UnityEngine;

public static class LobbyFrameLight
{
    public enum Style
    {
        None,
        GoldTop,
        GoldTopBottom,
        BlueTopBottom
    }

    const float WARM_RANGE = 0.3f;
    const float TOP_START = 0.45f;
    const float BOTTOM_END = 0.4f;
    const float LIGHT_POWER = 1.5f;
    const float BOTTOM_STRENGTH = 0.85f;
    const float LIGHT_MIX = 0.8f;
    const float SHADE_MIN = 0.86f;
    const float BLUE_HUE = 0.585f;
    const float BLUE_SATURATION = 1.25f;

    static readonly Color gold_light = new Color(1f, 0.96f, 0.80f, 1f);
    static readonly Color blue_light = new Color(0.80f, 0.92f, 1f, 1f);
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite sprite(string path, Style style)
    {
        var source = PlayUi.sprite(path);
        if (source == null || style == Style.None) return source;

        var key = path + "|" + style;
        if (cache.TryGetValue(key, out var cached) && cached != null) return cached;

        var baked = bake(source, style);
        if (baked == null) return source;

        cache[key] = baked;
        return baked;
    }

    static Sprite bake(Sprite source, Style style)
    {
        var rect = source.textureRect;
        var width = Mathf.RoundToInt(rect.width);
        var height = Mathf.RoundToInt(rect.height);
        if (width <= 0 || height <= 0) return null;

        var pixels = read_pixels(source.texture, Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), width, height);
        if (pixels == null || pixels.Length != width * height) return null;

        var blue = style == Style.BlueTopBottom;
        var bottom = style == Style.GoldTopBottom || style == Style.BlueTopBottom;
        var tint = blue ? blue_light : gold_light;

        for (var y = 0; y < height; y++)
        {
            var t = height > 1 ? y / (float)(height - 1) : 1f;
            var light = light_at(t, bottom);
            var shade = Mathf.Lerp(SHADE_MIN, 1f, light);

            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (pixels[index].a == 0) continue;

                Color c = pixels[index];
                var warm = Mathf.Clamp01((c.r - c.b) / WARM_RANGE);
                if (warm <= 0f) continue;

                if (blue)
                {
                    Color.RGBToHSV(c, out _, out var saturation, out var value);
                    var shifted = Color.HSVToRGB(BLUE_HUE, Mathf.Clamp01(saturation * BLUE_SATURATION), value);
                    c = Color.Lerp(c, shifted, warm);
                }

                var alpha = c.a;
                var shaded = new Color(c.r * shade, c.g * shade, c.b * shade, alpha);
                var lit = Color.Lerp(shaded, tint, Mathf.Clamp01(light * LIGHT_MIX * warm));
                lit.a = alpha;
                pixels[index] = lit;
            }
        }

        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.SetPixels32(pixels);
        texture.Apply(false, true);

        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), source.pixelsPerUnit, 0, SpriteMeshType.FullRect, source.border);
    }

    static float light_at(float t, bool with_bottom)
    {
        var top = Mathf.Pow(Mathf.Clamp01((t - TOP_START) / (1f - TOP_START)), LIGHT_POWER);
        if (!with_bottom) return top;

        var low = Mathf.Pow(Mathf.Clamp01((BOTTOM_END - t) / BOTTOM_END), LIGHT_POWER) * BOTTOM_STRENGTH;
        return Mathf.Max(top, low);
    }

    static Color32[] read_pixels(Texture2D texture, int x, int y, int width, int height)
    {
        if (texture == null) return null;

        var target = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        var previous = RenderTexture.active;
        var readable = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Graphics.Blit(texture, target);
        RenderTexture.active = target;
        readable.ReadPixels(new Rect(x, y, width, height), 0, 0);
        readable.Apply(false);
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(target);

        var pixels = readable.GetPixels32();
        Object.Destroy(readable);
        return pixels;
    }
}
