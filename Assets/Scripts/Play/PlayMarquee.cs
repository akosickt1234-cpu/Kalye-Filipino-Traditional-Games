using UnityEngine;
using UnityEngine.UI;

public class PlayMarquee : MonoBehaviour
{
    const string ELLIPSIS = "...";
    const float START_WAIT = 1.2f;
    const float SCROLL_SPEED = 70f;
    const float EASE_TIME = 0.4f;
    const float END_PAUSE = 1f;
    const float RETURN_TIME = 0.6f;
    const float REST_TIME = 2.5f;
    const float END_PADDING = 2f;

    enum Phase
    {
        Wait,
        Scroll,
        End,
        Return,
        Rest
    }

    RectTransform label_rect;
    Text label;
    float view_width;
    string full_text = string.Empty;
    string short_text = string.Empty;
    float full_width;
    float short_width;
    float scroll_distance;
    float scroll_time;
    float ease_time;
    float timer;
    bool scrolls;
    Phase phase;

    public RectTransform rect { get; private set; }

    public static PlayMarquee create(Transform parent, string name, int font_size, Color color, float width, float height)
    {
        var holder = PlayUi.rect(name, parent);
        holder.sizeDelta = new Vector2(width, height);
        holder.gameObject.AddComponent<RectMask2D>();

        var marquee = holder.gameObject.AddComponent<PlayMarquee>();
        marquee.rect = holder;
        marquee.build(font_size, color, width);
        return marquee;
    }

    public void set_text(string value)
    {
        full_text = value ?? string.Empty;
        measure_text();
        restart();
    }

    public void set_color(Color color)
    {
        if (label != null) label.color = color;
    }

    void build(int font_size, Color color, float width)
    {
        view_width = width;

        label = PlayUi.text("text", rect, string.Empty, font_size, color, TextAnchor.MiddleLeft);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label_rect = label.rectTransform;
        label_rect.anchorMin = new Vector2(0f, 0f);
        label_rect.anchorMax = new Vector2(0f, 1f);
        label_rect.pivot = new Vector2(0f, 0.5f);
        label_rect.anchoredPosition = Vector2.zero;
        label_rect.sizeDelta = new Vector2(0f, 0f);

        enabled = false;
    }

    void measure_text()
    {
        var settings = label.GetGenerationSettings(Vector2.zero);
        full_width = measure(full_text, settings);
        scrolls = full_width > view_width;

        if (!scrolls)
        {
            short_text = full_text;
            short_width = full_width;
            scroll_distance = 0f;
            scroll_time = 0f;
            ease_time = 0f;
            return;
        }

        short_text = truncate(full_text, settings);
        short_width = measure(short_text, settings);
        scroll_distance = full_width - view_width + END_PADDING;
        ease_time = Mathf.Min(EASE_TIME, scroll_distance / SCROLL_SPEED);
        scroll_time = scroll_distance / SCROLL_SPEED + ease_time;
    }

    float measure(string value, TextGenerationSettings settings)
    {
        return label.cachedTextGeneratorForLayout.GetPreferredWidth(value, settings) / label.pixelsPerUnit;
    }

    string truncate(string value, TextGenerationSettings settings)
    {
        if (measure(ELLIPSIS, settings) > view_width) return ELLIPSIS;

        var low = 0;
        var high = value.Length - 1;
        while (low < high)
        {
            var mid = (low + high + 1) / 2;
            if (measure(value.Substring(0, mid).TrimEnd() + ELLIPSIS, settings) <= view_width) low = mid;
            else high = mid - 1;
        }

        if (low > 0 && char.IsHighSurrogate(value[low - 1])) low--;
        return value.Substring(0, low).TrimEnd() + ELLIPSIS;
    }

    void restart()
    {
        show(short_text, short_width);
        set_offset(scrolls ? 0f : (view_width - short_width) * 0.5f);
        phase = Phase.Wait;
        timer = 0f;
        enabled = scrolls;
    }

    void show(string value, float width)
    {
        label.text = value;
        label_rect.sizeDelta = new Vector2(width, 0f);
    }

    void set_offset(float x)
    {
        label_rect.anchoredPosition = new Vector2(x, 0f);
    }

    void enter(Phase next)
    {
        phase = next;
        timer = 0f;
    }

    void start_scroll()
    {
        show(full_text, full_width);
        enter(Phase.Scroll);
    }

    float scroll_profile(float t)
    {
        if (t < ease_time) return 0.5f * SCROLL_SPEED * t * t / ease_time;

        var tail = scroll_time - t;
        if (tail < ease_time) return scroll_distance - 0.5f * SCROLL_SPEED * tail * tail / ease_time;

        return 0.5f * SCROLL_SPEED * ease_time + SCROLL_SPEED * (t - ease_time);
    }

    void Update()
    {
        timer += Time.unscaledDeltaTime;

        switch (phase)
        {
            case Phase.Wait:
                if (timer >= START_WAIT) start_scroll();
                break;

            case Phase.Scroll:
                if (timer >= scroll_time)
                {
                    set_offset(-scroll_distance);
                    enter(Phase.End);
                    break;
                }
                set_offset(-scroll_profile(timer));
                break;

            case Phase.End:
                if (timer >= END_PAUSE) enter(Phase.Return);
                break;

            case Phase.Return:
            {
                if (timer >= RETURN_TIME)
                {
                    show(short_text, short_width);
                    set_offset(0f);
                    enter(Phase.Rest);
                    break;
                }
                var t = timer / RETURN_TIME;
                set_offset(-scroll_distance * (1f - t * t * (3f - 2f * t)));
                break;
            }

            case Phase.Rest:
                if (timer >= REST_TIME) start_scroll();
                break;
        }
    }
}
