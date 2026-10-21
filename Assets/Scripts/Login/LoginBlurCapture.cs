using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoginBlurCapture : MonoBehaviour
{
    const int HALVINGS = 3;
    const int MIN_SIZE = 16;
    const int BLUR_RADIUS = 3;
    const int BLUR_ROUNDS = 2;

    CanvasGroup group;
    RawImage blur_image;
    Graphic dim_graphic;
    Texture2D blurred;

    public void begin(RawImage image, Graphic dim)
    {
        blur_image = image;
        dim_graphic = dim;

        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        StartCoroutine(capture());
    }

    IEnumerator capture()
    {
        yield return new WaitForEndOfFrame();

        var texture = create_blur();
        group.alpha = 1f;
        if (texture == null) yield break;

        blurred = texture;
        blur_image.texture = texture;
        blur_image.enabled = true;
        dim_graphic.enabled = true;
    }

    void OnDestroy()
    {
        if (blurred != null) Destroy(blurred);
    }

    static Texture2D create_blur()
    {
        Texture2D shot = null;
        RenderTexture current = null;
        var previous = RenderTexture.active;

        try
        {
            shot = ScreenCapture.CaptureScreenshotAsTexture();
            if (shot == null || shot.width < 2 || shot.height < 2) return null;

            Texture source = shot;
            var width = shot.width;
            var height = shot.height;
            for (var i = 0; i < HALVINGS; i++)
            {
                width = Mathf.Max(MIN_SIZE, width / 2);
                height = Mathf.Max(MIN_SIZE, height / 2);
                var next = RenderTexture.GetTemporary(width, height, 0);
                next.filterMode = FilterMode.Bilinear;
                Graphics.Blit(source, next);
                if (current != null) RenderTexture.ReleaseTemporary(current);
                current = next;
                source = next;
            }

            RenderTexture.active = current;
            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0f, 0f, width, height), 0, 0, false);

            var first = result.GetPixels32();
            var second = new Color32[first.Length];
            for (var round = 0; round < BLUR_ROUNDS; round++)
            {
                blur_pass(first, second, width, height, true);
                blur_pass(second, first, width, height, false);
            }

            result.SetPixels32(first);
            result.wrapMode = TextureWrapMode.Clamp;
            result.filterMode = FilterMode.Bilinear;
            result.Apply(false, true);
            return result;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"login background blur failed: {exception.Message}");
            return null;
        }
        finally
        {
            RenderTexture.active = previous;
            if (current != null) RenderTexture.ReleaseTemporary(current);
            if (shot != null) Destroy(shot);
        }
    }

    static void blur_pass(Color32[] source, Color32[] target, int width, int height, bool horizontal)
    {
        var span = BLUR_RADIUS * 2 + 1;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var r = 0;
                var g = 0;
                var b = 0;
                for (var k = -BLUR_RADIUS; k <= BLUR_RADIUS; k++)
                {
                    var sx = horizontal ? Mathf.Clamp(x + k, 0, width - 1) : x;
                    var sy = horizontal ? y : Mathf.Clamp(y + k, 0, height - 1);
                    var c = source[sy * width + sx];
                    r += c.r;
                    g += c.g;
                    b += c.b;
                }

                target[y * width + x] = new Color32((byte)(r / span), (byte)(g / span), (byte)(b / span), 255);
            }
        }
    }
}
