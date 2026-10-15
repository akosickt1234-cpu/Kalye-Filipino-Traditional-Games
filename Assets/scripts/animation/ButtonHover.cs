using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private float hover_scale = 1.08f;
    [SerializeField] private float speed = 12f;

    private Selectable selectable;
    private Vector3 base_scale;
    private Vector3 target_scale;
    private bool pointer_inside;
    private bool selected;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        base_scale = transform.localScale;
        target_scale = base_scale;
    }

    private void OnDisable()
    {
        pointer_inside = false;
        selected = false;
        target_scale = base_scale;
        transform.localScale = base_scale;
    }

    private void Update()
    {
        bool active = selectable.IsInteractable() && (pointer_inside || selected);
        target_scale = active ? base_scale * hover_scale : base_scale;
        transform.localScale = Vector3.Lerp(transform.localScale, target_scale, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        pointer_inside = true;
    }

    public void OnPointerExit(PointerEventData event_data)
    {
        pointer_inside = false;
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
