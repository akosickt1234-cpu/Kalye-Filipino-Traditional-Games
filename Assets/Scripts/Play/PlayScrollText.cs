using UnityEngine;
using UnityEngine.UI;

public class PlayScrollText : MonoBehaviour
{
    const float START_WAIT = 1.2f;
    const float SCROLL_SPEED = 40f;
    const float EASE_TIME = 0.4f;
    const float END_PAUSE = 1f;
    const float RETURN_TIME = 0.6f;
    const float REST_TIME = 2.5f;
    const float END_PADDING = 2f;
    const float FIT_TOLERANCE = 0.5f;

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
    int min_size;
    int max_size;
    float view_width;
    float view_height;
    string full_text = string.Empty;
    float text_height;
    float scroll_distance;
    float scroll_time;
    float ease_time;
    float timer;
    bool scrolls;
    Phase phase;

    public RectTransform rect { get; private set; }

    public static PlayScrollText create(Transform parent, string name, int min_size, int max_size, Color color, float width, float height)
    {
        var holder = PlayUi.rect(name, parent);
        holder.sizeDelta = new Vector2(width, height);
        holder.gameObject.AddComponent<RectMask2D>();

        var scroll_text = holder.gameObject.AddComponent<PlayScrollText>();
        scroll_text.rect = holder;
        scroll_text.build(min_size, max_size, color, width, height);
        return scroll_text;
    }

    public void set_text(string value)
    {
        full_text = value ?? string.Empty;
        fit_text();
        restart();
    }

    public void set_color(Color color)
    {
        if (label != null) label.color = color;
    }

    void build(int min_value, int max_value, Color color, float width, float height)
    {
        min_size = Mathf.Max(1, Mathf.Min(min_value, max_value));
        max_size = Mathf.Max(min_size, Mathf.Max(min_value, max_value));
        view_width = width;
        view_height = height;

        label = PlayUi.text("text", rect, string.Empty, max_size, color, TextAnchor.UpperLeft);
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.resizeTextForBestFit = false;
        label_rect = label.rectTransform;
        label_rect.anchorMin = new Vector2(0f, 1f);
        label_rect.anchorMax = new Vector2(0f, 1f);
        label_rect.pivot = new Vector2(0f, 1f);
        label_rect.anchoredPosition = Vector2.zero;
        label_rect.sizeDelta = new Vector2(width, height);

        enabled = false;
    }

    float measure_height()
    {
        var settings = label.GetGenerationSettings(new Vector2(view_width, 0f));
        return label.cachedTextGeneratorForLayout.GetPreferredHeight(full_text, settings) / label.pixelsPerUnit;
    }

    void fit_text()
    {
        label.text = full_text;

        var size = max_size;
        while (true)
        {
            label.fontSize = size;
            text_height = measure_height();
            if (text_height <= view_height + FIT_TOLERANCE || size <= min_size) break;
            size--;
        }

        label_rect.sizeDelta = new Vector2(view_width, text_height);
        scrolls = text_height > view_height + FIT_TOLERANCE;

        if (!scrolls)
        {
            scroll_distance = 0f;
            scroll_time = 0f;
            ease_time = 0f;
            return;
        }

        scroll_distance = text_height - view_height + END_PADDING;
        ease_time = Mathf.Min(EASE_TIME, scroll_distance / SCROLL_SPEED);
        scroll_time = scroll_distance / SCROLL_SPEED + ease_time;
    }

    void restart()
    {
        set_offset(0f);
        phase = Phase.Wait;
        timer = 0f;
        enabled = scrolls;
    }

    void set_offset(float y)
    {
        label_rect.anchoredPosition = new Vector2(0f, y);
    }

    void enter(Phase next)
    {
        phase = next;
        timer = 0f;
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
                if (timer >= START_WAIT) enter(Phase.Scroll);
                break;

            case Phase.Scroll:
                if (timer >= scroll_time)
                {
                    set_offset(scroll_distance);
                    enter(Phase.End);
                    break;
                }
                set_offset(scroll_profile(timer));
                break;

            case Phase.End:
                if (timer >= END_PAUSE) enter(Phase.Return);
                break;

            case Phase.Return:
            {
                if (timer >= RETURN_TIME)
                {
                    set_offset(0f);
                    enter(Phase.Rest);
                    break;
                }
                var t = timer / RETURN_TIME;
                set_offset(scroll_distance * (1f - t * t * (3f - 2f * t)));
                break;
            }

            case Phase.Rest:
                if (timer >= REST_TIME) enter(Phase.Scroll);
                break;
        }
    }
}
