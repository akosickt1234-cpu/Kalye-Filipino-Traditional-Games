using UnityEngine;
using UnityEngine.UI;

public class FireflyField : MonoBehaviour
{
    [SerializeField] private int count = 24;
    [SerializeField] private float size = 6f;
    [SerializeField] private Color color = new Color(1f, 0.86f, 0.45f, 1f);
    [SerializeField] private Vector2 rise_speed = new Vector2(0.01f, 0.03f);
    [SerializeField] private Vector2 pulse_speed = new Vector2(0.6f, 1.6f);
    [SerializeField] private float sway = 24f;
    [SerializeField] private float min_height = 0.05f;
    [SerializeField] private float max_height = 0.8f;
    [SerializeField] private float edge_fade = 0.1f;

    private class Fly
    {
        public RectTransform rect;
        public Image image;
        public float x;
        public float y;
        public float rise;
        public float phase;
        public float sway_speed;
        public float pulse;
    }

    private Fly[] flies;

    private void Start()
    {
        var sprite = PixelSprite.Dot;
        flies = new Fly[count];

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("firefly", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(size, size);

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;

            flies[i] = new Fly
            {
                rect = rect,
                image = image,
                x = Random.value,
                y = Random.Range(min_height, max_height),
                rise = Random.Range(rise_speed.x, rise_speed.y),
                phase = Random.Range(0f, Mathf.PI * 2f),
                sway_speed = Random.Range(0.3f, 0.9f),
                pulse = Random.Range(pulse_speed.x, pulse_speed.y)
            };
        }
    }

    private void Update()
    {
        if (flies == null)
        {
            return;
        }

        float time = Time.unscaledTime;
        float dt = Time.unscaledDeltaTime;

        foreach (var fly in flies)
        {
            fly.y += fly.rise * dt;
            if (fly.y > max_height)
            {
                fly.y = min_height;
                fly.x = Random.value;
            }

            var anchor = new Vector2(fly.x, fly.y);
            fly.rect.anchorMin = anchor;
            fly.rect.anchorMax = anchor;
            fly.rect.anchoredPosition = new Vector2(Mathf.Sin(time * fly.sway_speed + fly.phase) * sway, 0f);

            float pulse = 0.5f + 0.5f * Mathf.Sin(time * fly.pulse + fly.phase);
            float edge = Mathf.Clamp01(Mathf.Min(fly.y - min_height, max_height - fly.y) / edge_fade);
            var tint = color;
            tint.a = color.a * pulse * pulse * edge;
            fly.image.color = tint;
        }
    }
}
