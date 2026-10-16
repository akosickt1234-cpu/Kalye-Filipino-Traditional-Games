using UnityEngine;
using UnityEngine.UI;

public class LogoShine : MonoBehaviour
{
    private const string MASK_NAME = "logo_shine_mask";
    private const string BAR_NAME = "logo_shine_bar";
    private const float BAR_HEIGHT_RATIO = 1.6f;
    private const int GRADIENT_WIDTH = 64;

    [SerializeField] private float sweep_duration = 1.6f;
    [SerializeField] private float idle_gap = 2.5f;
    [SerializeField] private float slant_angle = 20f;
    [SerializeField] private float bar_width_ratio = 0.18f;
    [SerializeField] private float bar_alpha = 0.7f;

    private static readonly float[] TIME_SHARES = { 0.15f, 0.35f, 0.35f, 0.15f };

    private RectTransform mask_rect;
    private RectTransform bar_rect;
    private Image bar_image;
    private Texture2D gradient_texture;
    private Sprite gradient_sprite;
    private float clock;

    private void Start()
    {
        var logo_image = GetComponent<Image>();
        if (logo_image == null || logo_image.sprite == null)
        {
            enabled = false;
            return;
        }

        var mask_object = new GameObject(MASK_NAME, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Mask));
        mask_object.layer = gameObject.layer;
        mask_rect = mask_object.GetComponent<RectTransform>();
        mask_rect.SetParent(transform, false);
        mask_rect.anchorMin = Vector2.zero;
        mask_rect.anchorMax = Vector2.one;
        mask_rect.offsetMin = Vector2.zero;
        mask_rect.offsetMax = Vector2.zero;

        var mask_image = mask_object.GetComponent<Image>();
        mask_image.sprite = logo_image.sprite;
        mask_image.preserveAspect = logo_image.preserveAspect;
        mask_image.raycastTarget = false;
        mask_object.GetComponent<Mask>().showMaskGraphic = false;

        var bar_object = new GameObject(BAR_NAME, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bar_object.layer = gameObject.layer;
        bar_rect = bar_object.GetComponent<RectTransform>();
        bar_rect.SetParent(mask_rect, false);
        bar_rect.anchorMin = new Vector2(0f, 0.5f);
        bar_rect.anchorMax = new Vector2(0f, 0.5f);
        bar_rect.pivot = new Vector2(0.5f, 0.5f);
        bar_rect.localRotation = Quaternion.Euler(0f, 0f, -slant_angle);

        gradient_sprite = build_gradient_sprite();
        bar_image = bar_object.GetComponent<Image>();
        bar_image.sprite = gradient_sprite;
        bar_image.color = new Color(1f, 1f, 1f, bar_alpha);
        bar_image.raycastTarget = false;
        bar_image.enabled = false;

        clock = Mathf.Max(0.01f, sweep_duration);
    }

    private void OnDestroy()
    {
        if (gradient_sprite != null)
        {
            Destroy(gradient_sprite);
        }
        if (gradient_texture != null)
        {
            Destroy(gradient_texture);
        }
    }

    private void Update()
    {
        if (bar_rect == null)
        {
            return;
        }

        float sweep = Mathf.Max(0.01f, sweep_duration);
        float cycle = sweep + Mathf.Max(0f, idle_gap);
        clock = (clock + Time.unscaledDeltaTime) % cycle;

        if (clock >= sweep)
        {
            bar_image.enabled = false;
            return;
        }

        var area = mask_rect.rect;
        if (area.width <= 0f || area.height <= 0f)
        {
            bar_image.enabled = false;
            return;
        }

        float bar_height = area.height * BAR_HEIGHT_RATIO;
        float bar_width = area.width * bar_width_ratio;
        float radians = slant_angle * Mathf.Deg2Rad;
        float half_extent = bar_width * 0.5f * Mathf.Cos(radians) + bar_height * 0.5f * Mathf.Sin(radians);

        bar_rect.sizeDelta = new Vector2(bar_width, bar_height);

        float start_x = -half_extent;
        float end_x = area.width + half_extent;
        float progress = path_progress(clock / sweep);
        bar_rect.anchoredPosition = new Vector2(Mathf.Lerp(start_x, end_x, progress), 0f);
        bar_image.enabled = true;
    }

    private static float path_progress(float t)
    {
        float time_start = 0f;
        float quarter = 1f / TIME_SHARES.Length;

        for (int i = 0; i < TIME_SHARES.Length; i++)
        {
            float time_end = time_start + TIME_SHARES[i];
            if (t <= time_end || i == TIME_SHARES.Length - 1)
            {
                float local = Mathf.Clamp01((t - time_start) / TIME_SHARES[i]);
                return (i + local) * quarter;
            }
            time_start = time_end;
        }

        return 1f;
    }

    private Sprite build_gradient_sprite()
    {
        gradient_texture = new Texture2D(GRADIENT_WIDTH, 1, TextureFormat.RGBA32, false);
        gradient_texture.wrapMode = TextureWrapMode.Clamp;
        gradient_texture.filterMode = FilterMode.Bilinear;

        var pixels = new Color32[GRADIENT_WIDTH];
        for (int x = 0; x < GRADIENT_WIDTH; x++)
        {
            float u = (x + 0.5f) / GRADIENT_WIDTH;
            float edge = 1f - Mathf.Abs(u * 2f - 1f);
            byte alpha = (byte)Mathf.RoundToInt(Mathf.SmoothStep(0f, 1f, edge) * 255f);
            pixels[x] = new Color32(255, 255, 255, alpha);
        }

        gradient_texture.SetPixels32(pixels);
        gradient_texture.Apply();
        return Sprite.Create(gradient_texture, new Rect(0f, 0f, GRADIENT_WIDTH, 1f), new Vector2(0.5f, 0.5f), 100f);
    }
}
