using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoginBackground : MonoBehaviour
{
    const float FADE_SECONDS = 0.25f;
    const float FALLBACK_DIM_ALPHA = 0.8f;
    const int MIN_SIZE = 8;

    static readonly int[] DOWN_DIVISORS = { 4, 8, 16 };
    static readonly int[] UP_DIVISORS = { 8, 4 };

    CanvasGroup group;
    RawImage blur_image;
    Image dim_image;
    RenderTexture blurred;

    public static void build(RectTransform root)
    {
        if (root == null) return;

        var holder = PlayUi.rect("login_background", root);
        PlayUi.stretch(holder);

        var blur_holder = PlayUi.rect("login_blur", holder);
        PlayUi.stretch(blur_holder);
        var blur = blur_holder.gameObject.AddComponent<RawImage>();
        blur.raycastTarget = false;
        blur.enabled = false;

        var dim = PlayUi.image("login_dim", holder, null);
        PlayUi.stretch(dim.rectTransform);
        dim.color = PlayTheme.with_alpha(PlayTheme.overlay_dark, LoginTheme.BLUR_DIM_ALPHA);
        dim.enabled = false;

        var background = holder.gameObject.AddComponent<LoginBackground>();
        background.begin(root, blur, dim);
    }

    void begin(RectTransform root, RawImage image, Image dim)
    {
        blur_image = image;
        dim_image = dim;

        group = root.GetComponent<CanvasGroup>();
        if (group == null) group = root.gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        StartCoroutine(run());
    }

    IEnumerator run()
    {
        yield return new WaitForEndOfFrame();

        var texture = create_blur();
        var alpha = LoginTheme.BLUR_DIM_ALPHA;
        if (texture != null)
        {
            blurred = texture;
            blur_image.texture = texture;
            blur_image.enabled = true;
        }
        else
        {
            alpha = FALLBACK_DIM_ALPHA;
        }

        dim_image.color = PlayTheme.with_alpha(PlayTheme.overlay_dark, alpha);
        dim_image.enabled = true;

        var elapsed = 0f;
        while (elapsed < FADE_SECONDS)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Clamp01(elapsed / FADE_SECONDS);
            yield return null;
        }

        group.alpha = 1f;
    }

    void OnDestroy()
    {
        if (blur_image != null) blur_image.texture = null;
        if (blurred != null) RenderTexture.ReleaseTemporary(blurred);
        blurred = null;
    }

    static RenderTexture create_blur()
    {
        if (!SystemInfo.supportsRenderTextures) return null;

        Texture2D shot = null;
        RenderTexture current = null;
        var previous_active = RenderTexture.active;

        try
        {
            shot = ScreenCapture.CaptureScreenshotAsTexture();
            if (shot == null || shot.width < 2 || shot.height < 2) return null;

            shot.filterMode = FilterMode.Bilinear;
            shot.wrapMode = TextureWrapMode.Clamp;

            Texture source = shot;
            for (var i = 0; i < DOWN_DIVISORS.Length; i++)
            {
                source = blit_to(source, shot.width, shot.height, DOWN_DIVISORS[i], ref current);
            }

            for (var i = 0; i < UP_DIVISORS.Length; i++)
            {
                source = blit_to(source, shot.width, shot.height, UP_DIVISORS[i], ref current);
            }

            var result = current;
            current = null;
            return result;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"login background blur failed: {exception.Message}");
            return null;
        }
        finally
        {
            RenderTexture.active = previous_active;
            if (current != null) RenderTexture.ReleaseTemporary(current);
            if (shot != null) Destroy(shot);
        }
    }

    static RenderTexture blit_to(Texture source, int full_width, int full_height, int divisor, ref RenderTexture current)
    {
        var width = Mathf.Max(MIN_SIZE, full_width / divisor);
        var height = Mathf.Max(MIN_SIZE, full_height / divisor);
        var next = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        next.filterMode = FilterMode.Bilinear;
        next.wrapMode = TextureWrapMode.Clamp;
        Graphics.Blit(source, next);
        if (current != null) RenderTexture.ReleaseTemporary(current);
        current = next;
        return next;
    }
}
