using UnityEngine;
using UnityEngine.UI;

public static class PlayOrnament
{
    const string ARROW_PATH = "Icons/Play/IconPlayArrow";
    const int LINE_TEXTURE_WIDTH = 64;
    const float LINE_FADE_RATIO = 0.35f;
    const float ARROW_LINE_HEIGHT = 26f;
    const float ARROW_LINE_THICKNESS = 2f;
    const float CREST_ARROW_SIZE = 22f;
    const float CREST_GAP = 8f;
    const float CREST_THICKNESS = 2f;
    const float UP_ROTATION = -90f;

    static readonly Vector2 CENTER = new Vector2(0.5f, 0.5f);
    static readonly Vector2 LEFT_MIDDLE = new Vector2(0f, 0.5f);
    static readonly Vector2 RIGHT_MIDDLE = new Vector2(1f, 0.5f);
    static readonly Sprite[] line_sprites = new Sprite[4];

    public static Image arrow(Transform parent, string name, float size, bool point_right, Color color)
    {
        var picture = PlayUi.image(name, parent, PlayUi.sprite(ARROW_PATH));
        picture.preserveAspect = true;
        if (picture.sprite != null) picture.color = color;

        var picture_rect = picture.rectTransform;
        PlayUi.place(picture_rect, CENTER, CENTER, CENTER, Vector2.zero, new Vector2(size, size));
        picture_rect.localScale = new Vector3(point_right ? -1f : 1f, 1f, 1f);
        return picture;
    }

    public static Image arrow_up(Transform parent, string name, float size, Color color)
    {
        var picture = arrow(parent, name, size, false, color);
        picture.rectTransform.localRotation = Quaternion.Euler(0f, 0f, UP_ROTATION);
        return picture;
    }

    public static Image line(Transform parent, string name, float width, float thickness, Color color, bool fade_start, bool fade_end)
    {
        var picture = PlayUi.image(name, parent, line_sprite(fade_start, fade_end));
        picture.color = color;

        PlayUi.place(picture.rectTransform, CENTER, CENTER, CENTER, Vector2.zero, new Vector2(Mathf.Max(0f, width), Mathf.Max(0f, thickness)));
        return picture;
    }

    public static RectTransform arrow_line(Transform parent, string name, float width, Color color)
    {
        var safe_width = Mathf.Max(width, ARROW_LINE_HEIGHT * 2f);
        var holder = PlayUi.rect(name, parent);
        PlayUi.place(holder, CENTER, CENTER, CENTER, Vector2.zero, new Vector2(safe_width, ARROW_LINE_HEIGHT));

        var left_arrow = arrow(holder, "arrow_left", ARROW_LINE_HEIGHT, false, color);
        PlayUi.place(left_arrow.rectTransform, LEFT_MIDDLE, LEFT_MIDDLE, CENTER, new Vector2(ARROW_LINE_HEIGHT * 0.5f, 0f), new Vector2(ARROW_LINE_HEIGHT, ARROW_LINE_HEIGHT));

        var right_arrow = arrow(holder, "arrow_right", ARROW_LINE_HEIGHT, true, color);
        PlayUi.place(right_arrow.rectTransform, RIGHT_MIDDLE, RIGHT_MIDDLE, CENTER, new Vector2(-ARROW_LINE_HEIGHT * 0.5f, 0f), new Vector2(ARROW_LINE_HEIGHT, ARROW_LINE_HEIGHT));

        var segment_width = (safe_width - ARROW_LINE_HEIGHT * 2f) * 0.5f;

        var left_line = line(holder, "line_left", segment_width, ARROW_LINE_THICKNESS, color, false, true);
        PlayUi.place(left_line.rectTransform, LEFT_MIDDLE, LEFT_MIDDLE, LEFT_MIDDLE, new Vector2(ARROW_LINE_HEIGHT, 0f), new Vector2(segment_width, ARROW_LINE_THICKNESS));

        var right_line = line(holder, "line_right", segment_width, ARROW_LINE_THICKNESS, color, true, false);
        PlayUi.place(right_line.rectTransform, RIGHT_MIDDLE, RIGHT_MIDDLE, RIGHT_MIDDLE, new Vector2(-ARROW_LINE_HEIGHT, 0f), new Vector2(segment_width, ARROW_LINE_THICKNESS));

        return holder;
    }

    public static RectTransform crest_line(Transform parent, string name, float width, Color color)
    {
        var safe_width = Mathf.Max(width, CREST_ARROW_SIZE + CREST_GAP * 2f);
        var holder = PlayUi.rect(name, parent);
        PlayUi.place(holder, CENTER, CENTER, CENTER, Vector2.zero, new Vector2(safe_width, CREST_ARROW_SIZE));

        var crest = arrow_up(holder, "crest_arrow", CREST_ARROW_SIZE, color);
        PlayUi.place(crest.rectTransform, CENTER, CENTER, CENTER, Vector2.zero, new Vector2(CREST_ARROW_SIZE, CREST_ARROW_SIZE));

        var segment_width = (safe_width - CREST_ARROW_SIZE - CREST_GAP * 2f) * 0.5f;

        var left_line = line(holder, "line_left", segment_width, CREST_THICKNESS, color, true, false);
        PlayUi.place(left_line.rectTransform, LEFT_MIDDLE, LEFT_MIDDLE, LEFT_MIDDLE, Vector2.zero, new Vector2(segment_width, CREST_THICKNESS));

        var right_line = line(holder, "line_right", segment_width, CREST_THICKNESS, color, false, true);
        PlayUi.place(right_line.rectTransform, RIGHT_MIDDLE, RIGHT_MIDDLE, RIGHT_MIDDLE, Vector2.zero, new Vector2(segment_width, CREST_THICKNESS));

        return holder;
    }

    static Sprite line_sprite(bool fade_start, bool fade_end)
    {
        var index = (fade_start ? 1 : 0) | (fade_end ? 2 : 0);
        if (line_sprites[index] != null) return line_sprites[index];

        var texture = new Texture2D(LINE_TEXTURE_WIDTH, 1, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color[LINE_TEXTURE_WIDTH];
        for (var i = 0; i < LINE_TEXTURE_WIDTH; i++)
        {
            var t = i / (float)(LINE_TEXTURE_WIDTH - 1);
            var start_alpha = fade_start ? Mathf.Clamp01(t / LINE_FADE_RATIO) : 1f;
            var end_alpha = fade_end ? Mathf.Clamp01((1f - t) / LINE_FADE_RATIO) : 1f;
            pixels[i] = PlayTheme.with_alpha(PlayTheme.white, Mathf.Min(start_alpha, end_alpha));
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        line_sprites[index] = Sprite.Create(texture, new Rect(0f, 0f, LINE_TEXTURE_WIDTH, 1f), new Vector2(0.5f, 0.5f), 100f);
        return line_sprites[index];
    }
}
