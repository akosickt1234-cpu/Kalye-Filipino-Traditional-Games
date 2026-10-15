using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ButtonSparkle : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private int burst_count = 6;
    [SerializeField] private float size = 10f;
    [SerializeField] private float distance = 36f;
    [SerializeField] private float lifetime = 0.5f;
    [SerializeField] private Color color = new Color(1f, 0.92f, 0.6f, 1f);

    private class Particle
    {
        public RectTransform rect;
        public Image image;
        public Vector2 origin;
        public Vector2 direction;
        public float age;
    }

    private readonly List<Particle> active = new List<Particle>();
    private readonly Stack<Particle> pool = new Stack<Particle>();
    private Selectable selectable;
    private RectTransform button_rect;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        button_rect = (RectTransform)transform;
    }

    private void OnDisable()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Release(active[i]);
        }
        active.Clear();
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        if (!selectable.IsInteractable())
        {
            return;
        }

        var bounds = button_rect.rect;
        for (int i = 0; i < burst_count; i++)
        {
            var particle = Acquire();
            particle.age = 0f;
            particle.origin = new Vector2(
                Random.Range(bounds.xMin, bounds.xMax),
                Random.Range(bounds.yMin, bounds.yMax));
            particle.direction = Random.insideUnitCircle.normalized;
            particle.rect.anchoredPosition = particle.origin;
            particle.image.color = color;
            particle.image.gameObject.SetActive(true);
            active.Add(particle);
        }
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            var particle = active[i];
            particle.age += dt;
            float t = particle.age / lifetime;

            if (t >= 1f)
            {
                Release(particle);
                active.RemoveAt(i);
                continue;
            }

            float eased = 1f - (1f - t) * (1f - t);
            particle.rect.anchoredPosition = particle.origin + particle.direction * (distance * eased);
            particle.rect.localScale = Vector3.one * Mathf.Lerp(1f, 0.4f, t);

            var tint = color;
            tint.a = color.a * (1f - t);
            particle.image.color = tint;
        }
    }

    private Particle Acquire()
    {
        if (pool.Count > 0)
        {
            return pool.Pop();
        }

        var go = new GameObject("sparkle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = gameObject.layer;
        go.transform.SetParent(transform, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(size, size);

        var image = go.GetComponent<Image>();
        image.sprite = PixelSprite.Star;
        image.raycastTarget = false;

        return new Particle { rect = rect, image = image };
    }

    private void Release(Particle particle)
    {
        particle.image.gameObject.SetActive(false);
        pool.Push(particle);
    }
}
