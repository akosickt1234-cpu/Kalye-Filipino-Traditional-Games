using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LoginOverlayHost : MonoBehaviour
{
    readonly List<Selectable> buffer = new List<Selectable>();

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            LoginOverlay.close();
            return;
        }

        var event_system = EventSystem.current;
        if (event_system == null) return;

        var before = event_system.currentSelectedGameObject;
        PlayFocus.tick();

        if (keep_inside(event_system)) return;
        if (keyboard == null || !keyboard.tabKey.wasPressedThisFrame) return;

        move_focus(event_system, before, keyboard.shiftKey.isPressed);
    }

    void OnDestroy()
    {
        LoginOverlay.forget(gameObject);
    }

    bool keep_inside(EventSystem event_system)
    {
        var selected = event_system.currentSelectedGameObject;
        if (selected == null || selected.transform.IsChildOf(transform)) return false;

        collect();
        event_system.SetSelectedGameObject(buffer.Count > 0 ? buffer[0].gameObject : null);
        return true;
    }

    void move_focus(EventSystem event_system, GameObject before, bool backwards)
    {
        collect();
        if (buffer.Count == 0) return;

        var current = event_system.currentSelectedGameObject;
        if (before == null && current != null) return;

        var next = backwards ? buffer.Count - 1 : 0;
        var index = index_of(current);
        if (index >= 0)
        {
            var step = backwards ? -1 : 1;
            next = (index + step + buffer.Count) % buffer.Count;
        }

        event_system.SetSelectedGameObject(buffer[next].gameObject);
    }

    void collect()
    {
        GetComponentsInChildren(false, buffer);
        for (var i = buffer.Count - 1; i >= 0; i--)
        {
            if (!buffer[i].IsInteractable()) buffer.RemoveAt(i);
        }
    }

    int index_of(GameObject target)
    {
        if (target == null) return -1;

        for (var i = 0; i < buffer.Count; i++)
        {
            if (buffer[i].gameObject == target) return i;
        }

        return -1;
    }
}
