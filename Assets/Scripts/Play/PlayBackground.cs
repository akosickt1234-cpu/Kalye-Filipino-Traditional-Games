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

    public static void build(RectTransform root)
    {
        if (root == null) return;

        var holder = PlayUi.rect("play_background", root);
        holder.SetAsFirstSibling();
        PlayUi.stretch(holder);
        holder.gameObject.AddComponent<RectMask2D>();

        var bottom = create_layer("bg_layer_bottom", holder);
        var top = create_layer("bg_layer_top", holder);
        top.gameObject.AddComponent<CanvasGroup>().alpha = 0f;

        var fader = holder.gameObject.AddComponent<PlayBackgroundFader>();
        fader.setup(bottom, top);

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
