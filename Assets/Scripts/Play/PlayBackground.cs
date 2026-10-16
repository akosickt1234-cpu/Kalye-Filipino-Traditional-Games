using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class PlayBackground
{
    const int GRADIENT_STEPS = 256;
    const float LEFT_WIDTH_RATIO = 0.45f;
    const float LEFT_MAX_ALPHA = 0.6f;
    const float BOTTOM_HEIGHT_RATIO = 0.42f;
    const float BOTTOM_MAX_ALPHA = 0.75f;
    const float GRADIENT_FALLOFF = 2f;
    const float SUN_ANCHOR_X = 0.72f;
    const float SUN_ANCHOR_Y = 0.86f;
    const float SUN_WIDTH = 1000f;
    const float SUN_HEIGHT = 760f;

    public static void build(RectTransform root)
    {
        if (root == null) return;

        var holder = PlayUi.rect("play_background", root);
        holder.SetAsFirstSibling();
        PlayUi.stretch(holder);
        holder.gameObject.AddComponent<RectMask2D>();

        var motion = PlayUi.rect("bg_motion", holder);
        PlayUi.stretch(motion);
        motion.pivot = new Vector2(0.5f, 0.5f);

        var bottom = create_layer("bg_layer_bottom", motion);
        var top = create_layer("bg_layer_top", motion);
        top.gameObject.AddComponent<CanvasGroup>().alpha = 0f;

        var sun = create_sun_glow(motion);
        motion.gameObject.AddComponent<PlayBackgroundMotion>().setup(motion, sun);

        var fader = holder.gameObject.AddComponent<PlayBackgroundFader>();
        fader.setup(bottom, top);

        create_motes(holder);

        create_gradient("bg_gradient_left", holder, true, LEFT_MAX_ALPHA, new Vector2(0f, 0f), new Vector2(LEFT_WIDTH_RATIO, 1f));
        create_gradient("bg_gradient_bottom", holder, false, BOTTOM_MAX_ALPHA, new Vector2(0f, 0f), new Vector2(1f, BOTTOM_HEIGHT_RATIO));
    }

    static Image create_layer(string name, RectTransform parent)
    {
        var layer = PlayUi.rect(name, parent);
        layer.anchorMin = new Vector2(0.5f, 0.5f);
        layer.anchorMax = new Vector2(0.5f, 0.5f);
        layer.pivot = new Vector2(0.5f, 0.5f);
        layer.sizeDelta = Vector2.zero;

        var picture = layer.gameObject.AddComponent<Image>();
        picture.raycastTarget = false;
        picture.enabled = false;

        var fitter = layer.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        return picture;
    }

    static Image create_sun_glow(RectTransform parent)
    {
        var glow = PlayUi.image("bg_sun_glow", parent, PlayUi.radial_glow());
        glow.color = PlayTheme.clear;
        var anchor = new Vector2(SUN_ANCHOR_X, SUN_ANCHOR_Y);
        PlayUi.place(glow.rectTransform, anchor, anchor, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(SUN_WIDTH, SUN_HEIGHT));
        return glow;
    }

    static void create_motes(RectTransform parent)
    {
        var area = PlayUi.rect("bg_motes", parent);
        PlayUi.stretch(area);
        area.gameObject.AddComponent<PlayBackgroundMotes>().setup(area);
    }

    static void create_gradient(string name, RectTransform parent, bool horizontal, float max_alpha, Vector2 anchor_min, Vector2 anchor_max)
    {
        var rect = PlayUi.rect(name, parent);
        rect.anchorMin = anchor_min;
        rect.anchorMax = anchor_max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var picture = rect.gameObject.AddComponent<Image>();
        picture.sprite = make_gradient_sprite(horizontal, max_alpha);
        picture.raycastTarget = false;
    }

    static Sprite make_gradient_sprite(bool horizontal, float max_alpha)
    {
        var width = horizontal ? GRADIENT_STEPS : 1;
        var height = horizontal ? 1 : GRADIENT_STEPS;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color[GRADIENT_STEPS];
        for (var i = 0; i < GRADIENT_STEPS; i++)
        {
            var t = i / (float)(GRADIENT_STEPS - 1);
            var alpha = max_alpha * Mathf.Pow(1f - t, GRADIENT_FALLOFF);
            pixels[i] = PlayTheme.with_alpha(PlayTheme.overlay_dark, alpha);
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
    }
}

public class PlayBackgroundFader : MonoBehaviour
{
    const float FADE_SECONDS = 0.35f;

    Image bottom_image;
    Image top_image;
    CanvasGroup top_group;
    string bottom_path;
    string top_path;
    Coroutine fade_routine;

    public void setup(Image bottom, Image top)
    {
        bottom_image = bottom;
        top_image = top;
        top_group = top != null ? top.GetComponent<CanvasGroup>() : null;

        var game = PlayState.current;
        var path = game != null ? game.bg_path : null;
        bottom_path = path;
        top_path = path;
        apply(bottom_image, path);
    }

    void OnEnable()
    {
        PlayState.selection_changed += on_selection_changed;
    }

    void OnDisable()
    {
        PlayState.selection_changed -= on_selection_changed;
        fade_routine = null;
    }

    void on_selection_changed(int index)
    {
        if (bottom_image == null || top_image == null || top_group == null) return;

        var game = PlayState.current;
        var path = game != null ? game.bg_path : null;
        if (path == top_path) return;

        if (fade_routine != null)
        {
            StopCoroutine(fade_routine);
            fade_routine = null;
        }

        if (bottom_path != top_path)
        {
            bottom_path = top_path;
            apply(bottom_image, bottom_path);
        }

        top_path = path;
        apply(top_image, path);

        if (!isActiveAndEnabled)
        {
            finish_fade();
            return;
        }

        fade_routine = StartCoroutine(fade());
    }

    IEnumerator fade()
    {
        top_group.alpha = 0f;
        var elapsed = 0f;
        while (elapsed < FADE_SECONDS)
        {
            elapsed += Time.unscaledDeltaTime;
            top_group.alpha = Mathf.Clamp01(elapsed / FADE_SECONDS);
            yield return null;
        }

        finish_fade();
        fade_routine = null;
    }

    void finish_fade()
    {
        bottom_path = top_path;
        apply(bottom_image, bottom_path);
        top_group.alpha = 0f;
    }

    static void apply(Image target, string path)
    {
        if (target == null) return;

        var sprite = PlayUi.sprite(path);
        target.sprite = sprite;
        target.enabled = sprite != null;
        if (sprite == null) return;

        var fitter = target.GetComponent<AspectRatioFitter>();
        var size = sprite.rect.size;
        if (fitter != null && size.y > 0f) fitter.aspectRatio = size.x / size.y;
    }
}

public class PlayBackgroundMotion : MonoBehaviour
{
    const float SCALE_MIN = 1.08f;
    const float SCALE_MAX = 1.12f;
    const float SCALE_PERIOD = 26f;
    const float PAN_X = 14f;
    const float PAN_Y = 10f;
    const float PAN_X_PERIOD = 19f;
    const float PAN_Y_PERIOD = 23f;
    const float PAN_Y_PHASE = 1.3f;
    const float GLOW_PERIOD = 9f;
    const float GLOW_ALPHA_MIN = 0.08f;
    const float GLOW_ALPHA_MAX = 0.2f;
    const float TAU = Mathf.PI * 2f;

    RectTransform motion_rect;
    Image glow_image;

    public void setup(RectTransform rect, Image glow)
    {
        motion_rect = rect;
        glow_image = glow;
        apply(Time.unscaledTime);
    }

    void OnEnable()
    {
        apply(Time.unscaledTime);
    }

    void Update()
    {
        apply(Time.unscaledTime);
    }

    void apply(float time)
    {
        if (motion_rect != null)
        {
            var breathe = 0.5f + 0.5f * Mathf.Sin(time * TAU / SCALE_PERIOD);
            var scale = Mathf.Lerp(SCALE_MIN, SCALE_MAX, breathe);
            motion_rect.localScale = new Vector3(scale, scale, 1f);
            motion_rect.anchoredPosition = new Vector2(
                Mathf.Sin(time * TAU / PAN_X_PERIOD) * PAN_X,
                Mathf.Sin(time * TAU / PAN_Y_PERIOD + PAN_Y_PHASE) * PAN_Y);
        }

        if (glow_image != null)
        {
            var pulse = 0.5f + 0.5f * Mathf.Sin(time * TAU / GLOW_PERIOD);
            glow_image.color = PlayTheme.with_alpha(PlayTheme.magic_glow, Mathf.Lerp(GLOW_ALPHA_MIN, GLOW_ALPHA_MAX, pulse));
        }
    }
}

public class PlayBackgroundMotes : MonoBehaviour
{
    const int COUNT = 20;
    const float SIZE_MIN = 6f;
    const float SIZE_MAX = 16f;
    const float RISE_MIN = 0.006f;
    const float RISE_MAX = 0.02f;
    const float PULSE_MIN = 0.4f;
    const float PULSE_MAX = 1.2f;
    const float SWAY_MIN = 10f;
    const float SWAY_MAX = 30f;
    const float SWAY_SPEED_MIN = 0.2f;
    const float SWAY_SPEED_MAX = 0.6f;
    const float MIN_HEIGHT = 0.04f;
    const float MAX_HEIGHT = 0.95f;
    const float EDGE_FADE = 0.12f;
    const float MAX_ALPHA = 0.65f;
    const float MAX_STEP = 0.1f;

    class Mote
    {
        public RectTransform rect;
        public Image image;
        public float x;
        public float y;
        public float rise;
        public float phase;
        public float sway;
        public float sway_speed;
        public float pulse;
    }

    RectTransform area;
    Mote[] motes;

    public void setup(RectTransform target_area)
    {
        area = target_area;
        if (area == null) return;

        var sprite = PlayUi.radial_glow();
        motes = new Mote[COUNT];

        for (var i = 0; i < COUNT; i++)
        {
            var picture = PlayUi.image("bg_mote", area, sprite);
            picture.color = PlayTheme.clear;

            var size = Random.Range(SIZE_MIN, SIZE_MAX);
            PlayUi.place(picture.rectTransform, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size, size));

            motes[i] = new Mote
            {
                rect = picture.rectTransform,
                image = picture,
                x = Random.value,
                y = Random.Range(MIN_HEIGHT, MAX_HEIGHT),
                rise = Random.Range(RISE_MIN, RISE_MAX),
                phase = Random.Range(0f, Mathf.PI * 2f),
                sway = Random.Range(SWAY_MIN, SWAY_MAX),
                sway_speed = Random.Range(SWAY_SPEED_MIN, SWAY_SPEED_MAX),
                pulse = Random.Range(PULSE_MIN, PULSE_MAX)
            };
        }
    }

    void Update()
    {
        if (area == null || motes == null) return;

        var time = Time.unscaledTime;
        var step = Mathf.Min(Time.unscaledDeltaTime, MAX_STEP);
        var bounds = area.rect.size;

        foreach (var mote in motes)
        {
            mote.y += mote.rise * step;
            if (mote.y > MAX_HEIGHT)
            {
                mote.y = MIN_HEIGHT;
                mote.x = Random.value;
            }

            var sway = Mathf.Sin(time * mote.sway_speed + mote.phase) * mote.sway;
            mote.rect.anchoredPosition = new Vector2(mote.x * bounds.x + sway, mote.y * bounds.y);

            var pulse = 0.5f + 0.5f * Mathf.Sin(time * mote.pulse + mote.phase);
            var edge = Mathf.Clamp01(Mathf.Min(mote.y - MIN_HEIGHT, MAX_HEIGHT - mote.y) / EDGE_FADE);
            mote.image.color = PlayTheme.with_alpha(PlayTheme.magic_spark, MAX_ALPHA * pulse * pulse * edge);
        }
    }
}
