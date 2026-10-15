using UnityEngine;

public static class PixelSprite
{
    private static Sprite dot_sprite;
    private static Sprite star_sprite;

    public static Sprite Dot
    {
        get
        {
            if (dot_sprite == null)
            {
                dot_sprite = Build(new[] { "##", "##" });
            }
            return dot_sprite;
        }
    }

    public static Sprite Star
    {
        get
        {
            if (star_sprite == null)
            {
                star_sprite = Build(new[]
                {
                    "..#..",
                    "..#..",
                    "#####",
                    "..#..",
                    "..#.."
                });
            }
            return star_sprite;
        }
    }

    private static Sprite Build(string[] rows)
    {
        int height = rows.Length;
        int width = rows[0].Length;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool filled = rows[y][x] == '#';
                pixels[(height - 1 - y) * width + x] = filled ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 1f);
    }
}
