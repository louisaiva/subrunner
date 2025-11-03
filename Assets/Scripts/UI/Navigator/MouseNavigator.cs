using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// MouseNavigator is a navigation plugin for the UI_Navigator
/// that is responsible for handling navigation with mouse & keyboard
/// </summary>
public class MouseNavigator : MonoBehaviour, Navigator
{

    [Header("Mouse navigation parameters")]
    public LayerMask ui_slot_layer;
    public Vector2 BasePosition => new Vector2(Screen.width / 2f, Screen.height / 2f);

    [Header("Components")]
    public UI_Navigator manager;

    [Header("Logs")]
    [SerializeField] private bool log = false;
    [SerializeField] private bool log_hover = false;

    // HANDLE SLOTTABLE ACTIVATION
    public void ActivateSlottable(Slottable slottable)
    {
        // we update the slots
        manager.UpdateSlots();

        // if we have some slots but they are out of screen we automatically go to the closest' slot panel
        if (manager.IsAllSlotsOutOfScreen())
        {
            if (log) { Debug.LogWarning("(UI_MouseNavigator) all slots are out of screen"); }
        }

        // si on a des slots on navigue tout simplement
        Navigate();
    }
    public async void NavigateToClosest(Vector2 position)
    {
        // wait a frame for ui to update it self
        await System.Threading.Tasks.Task.Yield();

        // if (log) { Debug.Log($"(UI_MouseNavigator) navigating to closest !! we navigate after 1 frame"); }
        // bool old_log_hover = log_hover;
        // log_hover = true;
        Navigate(position);
        // log_hover = old_log_hover;
    }
    public void NavigateToClosest() => NavigateToClosest(Vector2.zero);



    // START
    private void Start()
    {
        manager = GetComponent<UI_Navigator>();
    }


    // NAVIGATION
    public void Navigate() { Navigate(Vector2.zero); }
    public void Navigate(Vector2 position)
    {
        if (log_hover) { Debug.Log($"(UI_MouseNavigator) navigating with mouse position : {Input.mousePosition}"); }

        // prepare UI_Slot result list
        List<UI_Slot> hovered_slots = new List<UI_Slot>();
        UI_Slot slot;

        // get the mouse position
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        string log_results = "(UI_MouseNavigator) raycast results : " + results.Count;

        // we filter the results to keep only slots on "UI_Slot" layers & that have a UI_Slot on them
        for (int i = 0; i < results.Count; i++)
        {
            log_results += $"\n\t- {results[i].gameObject.name} (layer {results[i].gameObject.layer})";

            slot = results[i].gameObject.GetComponent<UI_Slot>();
            if (slot == null) { continue; }
            if ((ui_slot_layer & (1 << results[i].gameObject.layer)) == 0) { continue; }
            hovered_slots.Add(slot);
        }

        if (log_hover) { Debug.Log(log_results); }

        // if we have no slots hovered, we unhover & return
        if (hovered_slots.Count == 0) {
            if (log_hover) { Debug.Log($"(UI_MouseNavigator) no ui_slots - unhovering slot"); }
            manager.UnhoverSlot(); return; }

        // on récupère le premier résultat
        slot = hovered_slots[0];
        if (log_hover) { Debug.Log($"(UI_MouseNavigator) hovered {hovered_slots.Count} ui_slots ! first is {slot.name}"); }

        // on vérifie si le slot est disabled
        if (slot.Disabled)
        {
            if (log_hover) { Debug.Log($"(UI_MouseNavigator) current slot is disabled, unhovering slot."); }
            manager.UnhoverSlot();
            return;
        }

        // on hover le slot
        manager.HoverSlot(slot);
    }
}