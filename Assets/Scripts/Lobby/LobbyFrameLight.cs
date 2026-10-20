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
    const float LIGHT_POWER = 2f;
    const float BOTTOM_STRENGTH = 0.8f;
    const float LIGHT_MIX = 0.6f;
    const float SHADE_MIN = 0.9f;
    const float BLUE_HUE = 0.585f;
    const float BLUE_SATURATION = 1.25f;
    const float RING_ALPHA_GAIN = 2f;
    const int GLOW_BLUR_RADIUS = 6;
    const int GLOW_BLUR_PASSES = 3;
    const float GLOW_PEAK = 0.9f;
    const float GLOW_FALLOFF = 1.35f;
    const float GLOW_SIDE_LEVEL = 0.5f;

    public const int GLOW_PAD = 28;

    static readonly Color gold_light = new Color(1f, 0.96f, 0.80f, 1f);
    static readonly Color blue_light = new Color(0.80f, 0.92f, 1f, 1f);
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite sprite(string path, Style style, bool ring_only = false)
    {
        var source = PlayUi.sprite(path);
        if (source == null || style == Style.None) return source;

        var key = path + "|" + style + (ring_only ? "|ring" : string.Empty);
        if (cache.TryGetValue(key, out var cached) && cached != null) return cached;

        var baked = bake(source, style, ring_only);
        if (baked == null) return source;

        cache[key] = baked;
        return baked;
    }

    public static Sprite glow_sprite(string path)
    {
        var key = path + "|glow";
        if (cache.TryGetValue(key, out var cached) && cached != null) return cached;

        var source = PlayUi.sprite(path);
        if (source == null) return null;

        var rect = source.textureRect;
        var width = Mathf.RoundToInt(rect.width);
        var height = Mathf.RoundToInt(rect.height);
        if (width <= 0 || height <= 0) return null;

        var pixels = read_pixels(source.texture, Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), width, height);
        if (pixels == null || pixels.Length != width * height) return null;

        var padded_width = width + GLOW_PAD * 2;
        var padded_height = height + GLOW_PAD * 2;
        var alpha = new float[padded_width * padded_height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var c = pixels[y * width + x];
                var warm = Mathf.Clamp01((c.r - c.b) / 255f / WARM_RANGE);
                alpha[(y + GLOW_PAD) * padded_width + x + GLOW_PAD] = c.a / 255f * Mathf.Clamp01(warm * RING_ALPHA_GAIN);
            }
        }

        for (var pass = 0; pass < GLOW_BLUR_PASSES; pass++) box_blur(alpha, padded_width, padded_height, GLOW_BLUR_RADIUS);

        var peak = 0f;
        for (var i = 0; i < alpha.Length; i++) peak = Mathf.Max(peak, alpha[i]);
        var scale = peak > 0f ? GLOW_PEAK / peak : 0f;

        var output = new Color32[alpha.Length];
        for (var y = 0; y < padded_height; y++)
        {
            var t = padded_height > 1 ? y / (float)(padded_height - 1) : 1f;
            var level = Mathf.Lerp(GLOW_SIDE_LEVEL, 1f, light_at(t, true));
            for (var x = 0; x < padded_width; x++)
            {
                var index = y * padded_width + x;
                var soft = Mathf.Pow(Mathf.Clamp01(alpha[index] * scale), GLOW_FALLOFF) * level;
                output[index] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(soft) * 255f));
            }
        }

        var texture = new Texture2D(padded_width, padded_height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.SetPixels32(output);
        texture.Apply(false, true);

        var border = source.border + new Vector4(GLOW_PAD, GLOW_PAD, GLOW_PAD, GLOW_PAD);
        var baked = Sprite.Create(texture, new Rect(0f, 0f, padded_width, padded_height), new Vector2(0.5f, 0.5f), source.pixelsPerUnit, 0, SpriteMeshType.FullRect, border);
        cache[key] = baked;
        return baked;
    }

    static void box_blur(float[] data, int width, int height, int radius)
    {
        var temp = new float[data.Length];
        var span = radius * 2 + 1;

        for (var y = 0; y < height; y++)
        {
            var row = y * width;
            var sum = 0f;
            for (var x = -radius; x <= radius; x++) sum += data[row + Mathf.Clamp(x, 0, width - 1)];
            for (var x = 0; x < width; x++)
            {
                temp[row + x] = sum / span;
                sum += data[row + Mathf.Min(x + radius + 1, width - 1)] - data[row + Mathf.Max(x - radius, 0)];
            }
        }

        for (var x = 0; x < width; x++)
        {
            var sum = 0f;
            for (var y = -radius; y <= radius; y++) sum += temp[Mathf.Clamp(y, 0, height - 1) * width + x];
            for (var y = 0; y < height; y++)
            {
                data[y * width + x] = sum / span;
                sum += temp[Mathf.Min(y + radius + 1, height - 1) * width + x] - temp[Mathf.Max(y - radius, 0) * width + x];
            }
        }
    }

    static Sprite bake(Sprite source, Style style, bool ring_only)
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
                if (ring_only) c.a *= Mathf.Clamp01(warm * RING_ALPHA_GAIN);
                if (warm <= 0f)
                {
                    if (ring_only) pixels[index] = c;
                    continue;
                }

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
