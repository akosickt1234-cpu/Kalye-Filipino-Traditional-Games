using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class MenuItemHover : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private RectTransform diamond;
    [SerializeField] private RectTransform label;
    [SerializeField] private Graphic diamond_graphic;
    [SerializeField] private Graphic label_graphic;
    [SerializeField] private Graphic paint_graphic;
    [SerializeField] private Graphic smear_graphic;
    [SerializeField] private Shadow[] text_shadows;
    [SerializeField] private Color normal_color = Color.white;
    [SerializeField] private Color active_color = Color.black;
    [SerializeField] private float shadow_alpha = 0.8f;
    [SerializeField] private float label_shift = 16f;
    [SerializeField] private float diamond_scale = 1.2f;
    [SerializeField] private float diamond_turn = 45f;
    [SerializeField] private float paint_start_scale = 0.7f;
    [SerializeField] private float speed = 14f;

    private Selectable selectable;
    private Vector2 label_base;
    private bool selected;
    private float amount;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        label_base = label.anchoredPosition;
        apply(0f);
    }

    private void OnDisable()
    {
        selected = false;
        amount = 0f;
        apply(0f);
    }

    private void Update()
    {
        float target = selected && selectable.IsInteractable() ? 1f : 0f;
        amount = Mathf.Lerp(amount, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
        apply(amount);
    }

    private void apply(float t)
    {
        label.anchoredPosition = label_base + Vector2.right * (label_shift * t);
        diamond.localScale = Vector3.one * Mathf.Lerp(1f, diamond_scale, t);
        diamond.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, diamond_turn, t));

        Color tint = Color.Lerp(normal_color, active_color, t);
        label_graphic.color = tint;
        diamond_graphic.color = tint;

        Color paint = paint_graphic.color;
        paint.a = t;
        paint_graphic.color = paint;
        paint_graphic.rectTransform.localScale = new Vector3(Mathf.Lerp(paint_start_scale, 1f, t), 1f, 1f);

        Color smear = smear_graphic.color;
        smear.a = 1f - t;
        smear_graphic.color = smear;

        foreach (Shadow text_shadow in text_shadows)
        {
            Color shadow_color = text_shadow.effectColor;
            shadow_color.a = shadow_alpha * (1f - t);
            text_shadow.effectColor = shadow_color;
        }
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        if (EventSystem.current != null && selectable.IsInteractable())
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void OnSelect(BaseEventData event_data)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData event_data)
    {
        selected = false;
    }
}
