using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class MenuItemHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private RectTransform diamond;
    [SerializeField] private RectTransform label;
    [SerializeField] private Graphic diamond_graphic;
    [SerializeField] private Graphic label_graphic;
    [SerializeField] private Color normal_color = Color.white;
    [SerializeField] private Color active_color = new Color(1f, 0.83f, 0.4f, 1f);
    [SerializeField] private float label_shift = 16f;
    [SerializeField] private float diamond_scale = 1.25f;
    [SerializeField] private float diamond_turn = 45f;
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
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        if (EventSystem.current != null && selectable.IsInteractable())
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void OnPointerExit(PointerEventData event_data)
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
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
