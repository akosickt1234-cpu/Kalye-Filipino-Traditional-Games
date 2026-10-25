using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoginFocusGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    Selectable selectable;
    Graphic tint_target;
    Graphic glow;
    Color normal_color;
    Color lit_color;
    Color glow_color;
    bool hovered;
    bool focused;

    public static LoginFocusGlow attach(Selectable target, Graphic tint_target, Color normal_color, Color lit_color, Graphic glow, Color glow_color)
    {
        var driver = target.gameObject.AddComponent<LoginFocusGlow>();
        driver.selectable = target;
        driver.tint_target = tint_target;
        driver.normal_color = normal_color;
        driver.lit_color = lit_color;
        driver.glow = glow;
        driver.glow_color = glow_color;
        driver.apply(false);
        return driver;
    }

    void Update()
    {
        var interactive = selectable != null && selectable.IsInteractable();
        apply(interactive && PlayFocus.lit(hovered, focused));
    }

    void OnDisable()
    {
        hovered = false;
        focused = false;
        apply(false);
    }

    void apply(bool lit)
    {
        if (tint_target != null) tint_target.color = lit ? lit_color : normal_color;
        if (glow != null) glow.color = lit ? glow_color : LoginTheme.clear;
    }

    public void OnPointerEnter(PointerEventData event_data)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData event_data)
    {
        hovered = false;
    }

    public void OnSelect(BaseEventData event_data)
    {
        focused = true;
    }

    public void OnDeselect(BaseEventData event_data)
    {
        focused = false;
    }
}
